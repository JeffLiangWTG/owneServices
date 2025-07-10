using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.BR.Business.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	public sealed class ImportInvoiceLineUserControlTest : BaseInvoiceLineUserControlAbstractTest
	{
		public void TestColumnLayoutContextForInvoiceLinesGrid()
		{
			using var control = new ImportInvoiceLineUserControl();
			AssertEquals("Column layout context should be import", nameof(DeclarationType.Import), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
		}

		public void TestRequestTTCEReferenceFileContextMenu_Visibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				var grid = FindInvoiceLinesGrid(form);
				AssertNotNull("Request TTCE should be visible", grid.ContextMenu.MenuItems.FindByText("Request TTCE Reference File"));

				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

				grid = FindInvoiceLinesGrid(form);
				AssertNull("Request TTCE should be invisible", grid.ContextMenu.MenuItems.FindByText("Request TTCE Reference File"));
			}

			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = BRJobMessageTypeList.Codes.Import;
			using (var form = new CommercialInvoiceForm(invoice))
			{
				var grid = FindInvoiceLinesGrid(form);
				AssertNull("Request TTCE should be invisible", grid.ContextMenu.MenuItems.FindByText("Request TTCE Reference File"));
			}
		}

		public void TestRequestTTCEReferenceFileContextMenu_Action()
		{
			var codes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("BR", "105")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Country, "Country Codes Mapping", codes);

			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "BR1";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			using var form = new JobDeclarationForm(declaration);
			var grid = FindInvoiceLinesGrid(form);
			var requestTTCEMenu = grid.ContextMenu.MenuItems.FindByText("Request TTCE Reference File");

			var exchangeDate = new ZDateTime(2024, 12, 11);
			invoiceHeader.ExchangeRateDate = exchangeDate;
			invoiceLine.JI_Tariff = "01010101";

			Factory.Save();
			requestTTCEMenu.PerformClick();
			AssertEquals("No rows selected.", UnitTestUserNotification.Instance.LastMessage.Text);

			grid.Select(0);
			requestTTCEMenu.PerformClick();
			AssertEquals("Digital Certificate not found, expired, or invalid.", UnitTestUserNotification.Instance.LastMessage.Text);

			var password = BRGlbStaffWrapper.Get(broker).CCTPassword;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(-1);

			requestTTCEMenu.PerformClick();
			AssertEquals("Digital Certificate not found, expired, or invalid.", UnitTestUserNotification.Instance.LastMessage.Text);

			password.GP_ExpiryDate = ZDateTime.Today.AddDays(1);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Invalid;
			requestTTCEMenu.PerformClick();
			AssertEquals("Digital Certificate not found, expired, or invalid.", UnitTestUserNotification.Instance.LastMessage.Text);

			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;
			requestTTCEMenu.PerformClick();
			AssertEquals("None of the invoice lines selected attend the criteria: Tariff and Country of Origin informed.", UnitTestUserNotification.Instance.LastMessage.Text);

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Brazil;
			requestTTCEMenu.PerformClick();

			AssertEquals("1 TTCE message(s) have been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		ZGrid FindInvoiceLinesGrid(JobDeclarationForm form)
		{
			form.Show();
			Application.DoEvents();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			var invoiceLineUserControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
			return invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
		}

		ZGrid FindInvoiceLinesGrid(CommercialInvoiceForm form)
		{
			form.Show();
			Application.DoEvents();
			form.MainTabControl.SelectedTab = form.LinesTabPage;
			Application.DoEvents();
			var invoiceLineUserControl = form.InvoiceLineUserControl;
			return invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
		}

		public void TestDefaultColumnsForPersistentJob()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();

			using var form = new JobDeclarationForm(declaration);
			var grid = FindInvoiceLinesGrid(form);
			var visibleColumnCount = grid.Columns.Count(x => x.IsVisible);
			CombineAssertions(() =>
			{
				AssertEquals(39, visibleColumnCount);
				Assert("Should NOT have NaladiNcca column in CustomsInvoiceLinesBoundGrid", !grid.Columns.Contains(JobComInvoiceLine.Schema.NaladiNcca));
				Assert("Should have JI_Calc_MergedLineNumber column in CustomsInvoiceLinesBoundGrid", grid.Columns.Contains(JobComInvoiceLine.Schema.JI_Calc_MergedLineNumber));
				Assert("Should have JI_ManufacturerIndicator column in CustomsInvoiceLinesBoundGrid", grid.Columns.Contains(JobComInvoiceLine.Schema.JI_ManufacturerIndicator));
				Assert("Should have ManufacturerOrgPK column in CustomsInvoiceLinesBoundGrid", grid.Columns.Contains(JobComInvoiceLine.Schema.ManufacturerOrgPK));
				Assert("Should have JI_ManufacturerAuthorityIdentifier column in CustomsInvoiceLinesBoundGrid", grid.Columns.Contains(JobComInvoiceLine.Schema.JI_ManufacturerAuthorityIdentifier));
				AssertEquals("JI_ManufacturerAuthorityIdentifier Header Text", "Manufacturer Authority", grid.Columns[JobComInvoiceLine.Schema.JI_ManufacturerAuthorityIdentifier].ColumnStyle.HeaderText);
				Assert("Should have JI_ManufacturerAuthorityVersion column in CustomsInvoiceLinesBoundGrid", grid.Columns.Contains(JobComInvoiceLine.Schema.JI_ManufacturerAuthorityVersion));
				AssertEquals("JI_ManufacturerAuthorityVersion Header Text", "Manufacturer Version", grid.Columns[JobComInvoiceLine.Schema.JI_ManufacturerAuthorityVersion].ColumnStyle.HeaderText);
				Assert("Should have JI_OA_ManufacturerAddress column in CustomsInvoiceLinesBoundGrid", grid.Columns.Contains(JobComInvoiceLine.Schema.JI_OA_ManufacturerAddress));
				AssertNull(grid.Columns[JobComInvoiceLine.Schema.JI_RequiresImportLicense]);
				AssertNull("JI_RequiresImportLicense should be null", grid.Columns[JobComInvoiceLine.Schema.JI_RequiresImportLicense]);
				AssertNull("JI_CC should be null", grid.Columns[JobComInvoiceLine.Schema.JI_CC]);
				AssertNull("Should NOT have NaladiHs column in CustomsInvoiceLinesBoundGrid", grid.Columns[JobComInvoiceLine.Schema.NaladiHs]);
				AssertNull("Should NOT have NaladiNcca column in CustomsInvoiceLinesBoundGrid", grid.Columns[JobComInvoiceLine.Schema.NaladiNcca]);
				AssertNull("Should NOT have RequiresImportLicense column in CustomsInvoiceLinesBoundGrid", grid.Columns[JobComInvoiceLine.Schema.JI_RequiresImportLicense]);
				AssertNull("Should NOT have ImportLicenseNumber column in CustomsInvoiceLinesBoundGrid", grid.Columns[JobComInvoiceLine.Schema.ImportLicenseNumber]);
				AssertNull("Should NOT have ImportLicenseReference column in CustomsInvoiceLinesBoundGrid", grid.Columns["AttachedImportLicenseLine+Declaration+JE_DeclarationReference"]);
				AssertNull("Should NOT have LineNoReference column in CustomsInvoiceLinesBoundGrid", grid.Columns["AttachedImportLicenseLine+JI_LineNo"]);
				Assert("FMMBenefit column must be default", grid.Columns[JobComInvoiceLine.Schema.FMMBenefit].IsVisible);
				AssertEquals("FMMBenefit caption", "FMM Benefit", grid.Columns[JobComInvoiceLine.Schema.FMMBenefit].ColumnStyle.HeaderText);
				AssertEquals("FMMBenefit GroupName", "FMM Benefit", grid.Columns[JobComInvoiceLine.Schema.FMMBenefit].GroupName.Caption);
				Assert("FMMBenefitDescription column must be default", grid.Columns[JobComInvoiceLine.Schema.FMMBenefitDescription].IsVisible);
				AssertEquals("FMMBenefitDescription caption", "FMM Benefit Description", grid.Columns[JobComInvoiceLine.Schema.FMMBenefitDescription].ColumnStyle.HeaderText);
				AssertEquals("FMMBenefitDescription GroupName", "FMM Benefit", grid.Columns[JobComInvoiceLine.Schema.FMMBenefitDescription].GroupName.Caption);
				Assert("ICMSTaxRegime column must be default", grid.Columns[JobComInvoiceLine.Schema.ICMSTaxRegime].IsVisible);
				Assert("ICMSLegalBase column must be default", grid.Columns[JobComInvoiceLine.Schema.ICMSLegalBase].IsVisible);
				Assert("JI_ICMSRate column must be default", grid.Columns[JobComInvoiceLine.Schema.JI_ICMSRate].IsVisible);
				Assert("JI_ICMSBaseValueReductionPercentage column must be default", grid.Columns[JobComInvoiceLine.Schema.JI_ICMSBaseValueReductionPercentage].IsVisible);
				Assert("JI_ICMSFormula column must be default", grid.Columns[JobComInvoiceLine.Schema.JI_ICMSFormula].IsVisible);
				Assert("ICMSFCPRateValue column must be default", grid.Columns[JobComInvoiceLine.Schema.ICMSFCPRateValue].IsVisible);
				Assert("JI_ICMSTotalAmountReductionPercentage column must be default", grid.Columns[JobComInvoiceLine.Schema.JI_ICMSTotalAmountReductionPercentage].IsVisible);
				Assert("ManufacturerName column must be default", grid.Columns[JobComInvoiceLine.Schema.ManufacturerName].IsVisible);
			});
		}

		public void TestDefaultColumnsForNonPersistentJob()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = BRJobMessageTypeList.Codes.Import;

			using var form = new CommercialInvoiceForm(invoice);
			var grid = FindInvoiceLinesGrid(form);
			var visibleColumnCount = grid.Columns.Count(x => x.IsVisible);
			CombineAssertions(() =>
			{
				AssertEquals(37, visibleColumnCount);
				Assert("Should NOT have NaladiNcca column in CustomsInvoiceLinesBoundGrid", !grid.Columns.Contains(JobComInvoiceLine.Schema.NaladiNcca));
				Assert("Should have JI_Calc_MergedLineNumber column in CustomsInvoiceLinesBoundGrid", grid.Columns.Contains(JobComInvoiceLine.Schema.JI_Calc_MergedLineNumber));
				AssertEquals("JI_Calc_MergedLineNumber", "Merged Ln. #", grid.Columns[JobComInvoiceLine.Schema.JI_Calc_MergedLineNumber].ColumnStyle.HeaderText);
				Assert("Should have JI_ManufacturerIndicator column in CustomsInvoiceLinesBoundGrid", grid.Columns.Contains(JobComInvoiceLine.Schema.JI_ManufacturerIndicator));
				AssertEquals("JI_ManufacturerIndicator", "Manufacturer Ind.", grid.Columns[JobComInvoiceLine.Schema.JI_ManufacturerIndicator].ColumnStyle.HeaderText);
				Assert("Should have ManufacturerOrgPK column in CustomsInvoiceLinesBoundGrid", grid.Columns.Contains(JobComInvoiceLine.Schema.ManufacturerOrgPK));
				AssertEquals("ManufacturerOrgPK", "Manufacturer", grid.Columns[JobComInvoiceLine.Schema.ManufacturerOrgPK].ColumnStyle.HeaderText);
				Assert("Should have JI_ManufacturerAuthorityIdentifier column in CustomsInvoiceLinesBoundGrid", grid.Columns.Contains(JobComInvoiceLine.Schema.JI_ManufacturerAuthorityIdentifier));
				AssertEquals("JI_ManufacturerAuthorityIdentifier Header Text", "Manufacturer Authority", grid.Columns[JobComInvoiceLine.Schema.JI_ManufacturerAuthorityIdentifier].ColumnStyle.HeaderText);
				Assert("Should have JI_ManufacturerAuthorityVersion column in CustomsInvoiceLinesBoundGrid", grid.Columns.Contains(JobComInvoiceLine.Schema.JI_ManufacturerAuthorityVersion));
				AssertEquals("JI_ManufacturerAuthorityVersion Header Text", "Manufacturer Version", grid.Columns[JobComInvoiceLine.Schema.JI_ManufacturerAuthorityVersion].ColumnStyle.HeaderText);
				Assert("Should have JI_OA_ManufacturerAddress column in CustomsInvoiceLinesBoundGrid", grid.Columns.Contains(JobComInvoiceLine.Schema.JI_OA_ManufacturerAddress));
				AssertEquals("JI_OA_ManufacturerAddress", "Manufacturer Address", grid.Columns[JobComInvoiceLine.Schema.JI_OA_ManufacturerAddress].ColumnStyle.HeaderText);
				AssertNotNull("JI_RequiresImportLicense should NOT be null", grid.Columns[JobComInvoiceLine.Schema.JI_RequiresImportLicense]);
				AssertNull("Should NOT have NaladiHs column in CustomsInvoiceLinesBoundGrid", grid.Columns[JobComInvoiceLine.Schema.NaladiHs]);
				AssertNull("Should NOT have NaladiNcca column in CustomsInvoiceLinesBoundGrid", grid.Columns[JobComInvoiceLine.Schema.NaladiNcca]);
				AssertNotNull(grid.Columns[JobComInvoiceLine.Schema.JI_RequiresImportLicense]);
				Assert("JI_RequiresImportLicense column must NOT be default", !grid.Columns[JobComInvoiceLine.Schema.JI_RequiresImportLicense].IsVisible);
				AssertNull("JI_CC should be null", grid.Columns[JobComInvoiceLine.Schema.JI_CC]);
				AssertNull("ImportLicenseNumber should be null", grid.Columns[JobComInvoiceLine.Schema.ImportLicenseNumber]);
				AssertNull("ImportLicenseReference should be null", grid.Columns["AttachedImportLicenseLine+Declaration+JE_DeclarationReference"]);
				AssertNull("LineNoReference should be null", grid.Columns["AttachedImportLicenseLine+JI_LineNo"]);
				Assert("FMMBenefit column must be default", grid.Columns[JobComInvoiceLine.Schema.FMMBenefit].IsVisible);
				AssertEquals("FMMBenefit caption", "FMM Benefit", grid.Columns[JobComInvoiceLine.Schema.FMMBenefit].ColumnStyle.HeaderText);
				AssertEquals("FMMBenefit GroupName", "FMM Benefit", grid.Columns[JobComInvoiceLine.Schema.FMMBenefit].GroupName.Caption);
				Assert("FMMBenefitDescription column must be default", grid.Columns[JobComInvoiceLine.Schema.FMMBenefitDescription].IsVisible);
				AssertEquals("FMMBenefitDescription caption", "FMM Benefit Description", grid.Columns[JobComInvoiceLine.Schema.FMMBenefitDescription].ColumnStyle.HeaderText);
				AssertEquals("FMMBenefitDescription GroupName", "FMM Benefit", grid.Columns[JobComInvoiceLine.Schema.FMMBenefitDescription].GroupName.Caption);
				Assert("ICMSTaxRegime column must be default", grid.Columns[JobComInvoiceLine.Schema.ICMSTaxRegime].IsVisible);
				Assert("ICMSLegalBase column must be default", grid.Columns[JobComInvoiceLine.Schema.ICMSLegalBase].IsVisible);
				Assert("JI_ICMSRate column must be default", grid.Columns[JobComInvoiceLine.Schema.JI_ICMSRate].IsVisible);
				Assert("JI_ICMSBaseValueReductionPercentage column must be default", grid.Columns[JobComInvoiceLine.Schema.JI_ICMSBaseValueReductionPercentage].IsVisible);
				Assert("JI_ICMSFormula column must be default", grid.Columns[JobComInvoiceLine.Schema.JI_ICMSFormula].IsVisible);
				Assert("ICMSFCPRateValue column must be default", grid.Columns[JobComInvoiceLine.Schema.ICMSFCPRateValue].IsVisible);
				Assert("JI_ICMSTotalAmountReductionPercentage column must be default", grid.Columns[JobComInvoiceLine.Schema.JI_ICMSTotalAmountReductionPercentage].IsVisible);
				Assert("ManufacturerName column must be default", grid.Columns[JobComInvoiceLine.Schema.ManufacturerName].IsVisible);
			});
		}

		public void TestImportLabels()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				using (var control = brokerageControl.InvoiceLinesUserControl)
				{
					var cifConvertToLocalCurrencyControl = control.Controls.Find("JI_Calc_CIFConvertToLocalCurrencyControl", true).FirstOrDefault();
					AssertEquals("Customs Value", ((ConvertToLocalCurrencyControl)cifConvertToLocalCurrencyControl)?.CaptionResourceString?.Caption);
				}
			}
		}

		public void TestTabPages()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.LinkedDocumentTab;
				Assert(invoiceLineUserControl.LinkedDocumentUserControl.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.MercosulForeignDeclarationTabPage;
				Assert(invoiceLineUserControl.MercosulForeignDeclarationUserControl.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.ICMSTaxDetailsTab;
				Assert(invoiceLineUserControl.ICMSTaxDetailsUserControl.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.PermitTab;
				Assert(invoiceLineUserControl.PermitTab.TabVisible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.SpecialCasesTabPage;
				Assert(invoiceLineUserControl.SpecialCasesUserControl.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.TaxTreatmentTab;
				Assert(invoiceLineUserControl.TaxTreatmentTab.TabVisible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.NcmAttributesTab;
				Assert(invoiceLineUserControl.NcmAttributesTab.TabVisible);
			}
		}

		public void TestGoodsCatalogAvailable()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();

			using (Registry.BRCustomsDataRegistry.Instance.EnableCatalogModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using var form = new JobDeclarationForm(declaration);
				var grid = FindInvoiceLinesGrid(form);
				AssertNull(grid.GetColumnStyle("JI_CGC_Catalog"));
				AssertNull(grid.GetColumnStyle("JI_CatalogAuthorityIdentifier"));
				AssertNull(grid.GetColumnStyle("JI_CatalogAuthorityVersion"));
			}

			using (Registry.BRCustomsDataRegistry.Instance.EnableCatalogModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using var form = new JobDeclarationForm(declaration);
				var grid = FindInvoiceLinesGrid(form);
				AssertType<ZGuidFindBoxColumnStyleInfo>(grid.GetColumnStyle("JI_CGC_Catalog"));
				AssertType<ZTextBoxColumnStyleInfo>(grid.GetColumnStyle("JI_CatalogAuthorityIdentifier"));
				AssertType<ZTextBoxColumnStyleInfo>(grid.GetColumnStyle("JI_CatalogAuthorityVersion"));
			}
		}

		public void TestCustomsInvoiceLinesBoundGridColumnsOrder_EnableCatalogModule()
		{
			Assert("Precondition: Enable Catalog Module turns off", !Registry.BRCustomsDataRegistry.Instance.EnableCatalogModule.Value);

			using (Registry.BRCustomsDataRegistry.Instance.EnableCatalogModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestCustomsInvoiceLinesBoundGridColumnsOrder();
			}
		}

		protected override string JobMessageType => BRJobMessageTypeList.Codes.Import;

		protected override List<ZString> ExpectedColumnNamesListInOrder
		{
			get
			{
				var expectedColumnNamesListOnThisOrder = new List<ZString>
				{
					JobComInvoiceLine.Schema.JI_LineNo,
					JobComInvoiceLine.Schema.JI_Calc_Invoice,
					JobComInvoiceLine.Schema.JI_PartNo,
				};

				if (Registry.BRCustomsDataRegistry.Instance.EnableCatalogModule.Value)
				{
					expectedColumnNamesListOnThisOrder.Add(JobComInvoiceLine.Schema.JI_CGC_Catalog);
					expectedColumnNamesListOnThisOrder.Add(JobComInvoiceLine.Schema.JI_CatalogAuthorityIdentifier);
					expectedColumnNamesListOnThisOrder.Add(JobComInvoiceLine.Schema.JI_CatalogAuthorityVersion);
				}

				expectedColumnNamesListOnThisOrder.AddRange(new List<ZString>
				{
					JobComInvoiceLine.Schema.JI_Tariff,
					JobComInvoiceLine.Schema.JI_InvoiceQuantity,
					JobComInvoiceLine.Schema.JI_InvoiceUQ,
					JobComInvoiceLine.Schema.JI_CustomsQuantity,
					JobComInvoiceLine.Schema.JI_CustomsUnitQty,
					JobComInvoiceLine.Schema.JI_LinePrice,
					JobComInvoiceLine.Schema.FullGoodsDescription,
					JobComInvoiceLine.Schema.JI_RH_NKCommodity_Code,
					JobComInvoiceLine.Schema.JI_Weight,
					JobComInvoiceLine.Schema.JI_WeightUQ,
					JobComInvoiceLine.Schema.JI_NetWeight,
					JobComInvoiceLine.Schema.JI_NetWeightUQ,
					JobComInvoiceLine.Schema.JI_Volume,
					JobComInvoiceLine.Schema.JI_VolumeUQ,
					JobComInvoiceLine.Schema.JI_OrderNumber,
					JobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine,
					JobComInvoiceLine.Schema.UnitPrice,
					JobComInvoiceLine.Schema.JI_SerialNumber,
					JobComInvoiceLine.Schema.JI_ManufacturerIndicator,
					JobComInvoiceLine.Schema.ManufacturerOrgPK,
					JobComInvoiceLine.Schema.JI_OA_ManufacturerAddress,
					JobComInvoiceLine.Schema.JI_CountryOfOrigin,
					JobComInvoiceLine.Schema.JI_ManufacturerAuthorityVersion,
					JobComInvoiceLine.Schema.JI_ManufacturerAuthorityIdentifier,
					JobComInvoiceLine.Schema.ManufacturerName,
					JobComInvoiceLine.Schema.FMMBenefit,
					JobComInvoiceLine.Schema.FMMBenefitDescription,
					JobComInvoiceLine.Schema.JI_CEI,
					JobComInvoiceLine.Schema.JI_Calc_MergedLineNumber,
					JobComInvoiceLine.Schema.ICMSTaxRegime,
					JobComInvoiceLine.Schema.ICMSLegalBase,
					JobComInvoiceLine.Schema.JI_ICMSRate,
					JobComInvoiceLine.Schema.JI_ICMSBaseValueReductionPercentage,
					JobComInvoiceLine.Schema.JI_ICMSFormula,
					JobComInvoiceLine.Schema.ICMSFCPRateValue,
					JobComInvoiceLine.Schema.JI_ICMSTotalAmountReductionPercentage
				});

				return expectedColumnNamesListOnThisOrder;
			}
		}
	}
}
