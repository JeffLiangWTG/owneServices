using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class EUInvoiceLineUserControlBaseOnlyTest : EUInvoiceLineUserControlTest<EUInvoiceLineUserControl>
	{
		[RequiresSTA]
		public void TestGDMLinkBehaviour()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new EUInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				var gdmLink = control.FindSingle<ZLinkLabel>("GDMLink");
				Assert("GDMLink should be visible by default", gdmLink.Visible);
				gdmLink.PerformClick_ForTest();

				var gDMForm = ZFormModaliser.LastFormShownDialogForTest;
				CombineAssertions("GDM Form should be created and shown", () =>
				{
					AssertNotNull("GDM Form created", gDMForm);
					AssertType<GuidedDecisionMakingForm>(gDMForm);
				});
			}
		}

		[RequiresSTA]
		public void TestGDMLinkBehaviourInMultiLineMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var line1 = header.InvoiceLines.AddNew();
			var line2 = header.InvoiceLines.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new EUInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();
				control.CustomsInvoiceLinesBoundGrid.Select(0);
				control.CustomsInvoiceLinesBoundGrid.Select(1);
				var gdmLink = control.FindSingle<ZLinkLabel>("GDMLink");
				Assert("GDMLink should be visible by default", gdmLink.Visible);

				line1.JI_Tariff = "1234";
				line2.JI_Tariff = "1234";
				gdmLink.PerformClick_ForTest();
				var gDMForm = ZFormModaliser.LastFormShownDialogForTest;
				CombineAssertions("GDM Form should be created and shown", () =>
				{
					AssertNotNull("GDM Form created", gDMForm);
					AssertType<GuidedDecisionMakingForm>(gDMForm);
				});

				line2.JI_Tariff = "5678";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
				gdmLink.PerformClick_ForTest();
				AssertEquals("ExpectedMessage", "The selected Invoice Lines have different Commodity Codes. Only the current Invoice Line will be updated.", UnitTestUserNotification.Instance.LastMessage.Text);
				gDMForm = ZFormModaliser.LastFormShownDialogForTest;
				CombineAssertions("GDM Form should be created and shown", () =>
				{
					AssertNotNull("GDM Form created", gDMForm);
					AssertType<GuidedDecisionMakingForm>(gDMForm);
				});
			}
		}

		[RequiresSTA]
		public void TestFourthQtyCalcDropEdit_Invisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new EUInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				var lineDetailsTabPage = control.FindSingle<ZTabPage>("LineDetailsTabPage");
				control.LineDetailTabControl.SelectTab(lineDetailsTabPage);
				AssertEquals(false, lineDetailsTabPage.FindSingle<ZCalcDropEdit>("FourthQtyCalcDropEdit").Visible);
			}
		}

		[RequiresSTA]
		public void TestJI_TaxOrFeeDetail()
		{
			using (var form = new ZForm())
			using (var control = new EUImportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var vatDetailGuidDropEdit = (ZDropEdit)control.Controls.Find("VatDetailGuidDropEdit", true).First();
				Assert("Detailed VAT control should not show.", !vatDetailGuidDropEdit.Visible);
			}
		}

		[RequiresSTA]
		public void TestJI_ZZF_NKTaxTypeVisibility()
		{
			using (var form = new ZForm())
			using (var control = new EUImportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var vatTypeDropEdit = (ZDropEdit)control.Controls.Find("VatTypeDropEdit", true).First();
				AssertEquals("Basic VAT control should show.", true, vatTypeDropEdit.Visible);
			}
		}

		public void TestSetSupplyChainActorTabPageCaptionResourceString()
		{
			using (var form = new ZForm())
			using (var control = new EUInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals("SupplyChainActorTabPage", "Add. Supply Chain Actors", control.FindSingle<ZTabPage>("SupplyChainActorTabPage").CaptionResourceString.Caption);
			}
		}
		public void TestSetFiscalReferencesTabPageCaptionResourceString()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			using (var form = new ZForm(declaration))
			using (var control = new EUInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();
				AssertEquals("FiscalReferencesTabPage", "Fiscal References", control.FindSingle<ZTabPage>("FiscalReferencesTabPage").CaptionResourceString.Caption);
			}
		}

		[RequiresSTA]
		public void TestUniversalTariffFindBox_GetDataGroupingIsSet()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new EUInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var tariffFindBox = (Universal.GUI.TariffFindBox)control.tariffFindBox;
				AssertEquals("GetDataGroupingForUniversalTariff", tariffFindBox.GetDataGrouping.Method.Name);
			}
		}

		[RequiresSTA]
		public void TestFiscalReferencesUserControlType()
		{
			var invoiceLineConfigurationMock = new Mock<InvoiceLineConfiguration>();
			invoiceLineConfigurationMock.Protected().Setup<ZBool>("FiscalReferencesSupportCore", ItExpr.IsAny<BusinessObject>()).Returns(true);

			using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(Factory, "GetNewInvoiceLineConfiguration", invoiceLineConfigurationMock.Object, null))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.Invoices.AddNew().InvoiceLines.AddNew();
				using (var form = new ZForm(declaration))
				using (var control = new EUInvoiceLineUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					var fiscalReferencesUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("FiscalReferencesUserControl");
					AssertEquals(typeof(InvoiceLineFiscalReferencesUserControl), fiscalReferencesUserControl.UserControlType);
				}
			}
		}

		[RequiresSTA]
		public void TestFiscalReferencesTabPage_Invisible()
		{
			var mock = new Mock<InvoiceLineConfiguration>();
			mock.Protected().Setup<ZBool>("FiscalReferencesSupportCore", ItExpr.IsAny<BusinessObject>())
				.Returns(false);
			AssertTabPageVisibility(false, "FiscalReferencesTabPage", mock.Object);
		}

		[RequiresSTA]
		public void TestFiscalReferencesTabPage_Visible()
		{
			var mock = new Mock<InvoiceLineConfiguration>();
			mock.Protected().Setup<ZBool>("FiscalReferencesSupportCore", ItExpr.IsAny<BusinessObject>())
				.Returns(true);
			AssertTabPageVisibility(true, "FiscalReferencesTabPage", mock.Object);
		}

		public void TestEntryInstructions()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;

					var invoiceLineControl = (EUInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
					AssertEquals("JI_CEIGuidDropEdit", true, invoiceLineControl.InvoiceDetailsGroupBox.Controls.Find("JI_CEIGuidDropEdit", true)?.FirstOrDefault()?.Visible);
					AssertEquals(true, invoiceLineControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CEI).IsVisible);
				}
			}
		}

		[RequiresSTA]
		public void TestDisplayOfCalculationBoxesJiggling()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			dec.Invoices.AddNew().InvoiceLines.AddNew();

			using (var form = new ZForm())
			using (var control = new EUInvoiceLineUserControlForTest())
			{
				control.JobDeclaration = dec;
				form.Controls.Add(control);
				form.Show();
				var customsValueLocalCurrencyControl = control.Controls.Find("CustomsValueLocalCurrencyControl", true)[0];
				var valueForGstVatLocalCurrencyControl = control.Controls.Find("ValueForGstVatLocalCurrencyControl", true)[0];
				var statisticalValueLocalCurrencyControl = control.Controls.Find("StatisticalValueLocalCurrencyControl", true)[0];
				var jI_Calc_CIFConvertToLocalCurrencyControl = control.Controls.Find("JI_Calc_CIFConvertToLocalCurrencyControl", true)[0];
				var jI_Calc_FOBConvertToLocalCurrencyControl = control.Controls.Find("JI_Calc_FOBConvertToLocalCurrencyControl", true)[0];

				Assert(!jI_Calc_FOBConvertToLocalCurrencyControl.Visible);
				Assert(customsValueLocalCurrencyControl.Visible);
				Assert(valueForGstVatLocalCurrencyControl.Visible);
				Assert(statisticalValueLocalCurrencyControl.Visible);
				Assert(jI_Calc_CIFConvertToLocalCurrencyControl.Visible);

				AssertEquals(customsValueLocalCurrencyControl.Location.X, valueForGstVatLocalCurrencyControl.Location.X);
				AssertEquals(customsValueLocalCurrencyControl.Location.X, statisticalValueLocalCurrencyControl.Location.X);
				AssertEquals(customsValueLocalCurrencyControl.Location.X, jI_Calc_CIFConvertToLocalCurrencyControl.Location.X);

				Assert(customsValueLocalCurrencyControl.Location.Y < valueForGstVatLocalCurrencyControl.Location.Y);
				Assert(valueForGstVatLocalCurrencyControl.Location.Y < statisticalValueLocalCurrencyControl.Location.Y);
				Assert(statisticalValueLocalCurrencyControl.Location.Y < jI_Calc_CIFConvertToLocalCurrencyControl.Location.Y);
			}
		}

		[RequiresSTA]
		public void TestSupportingDocumentsTabPage_Invisible()
		{
			var mock = new Mock<InvoiceLineConfiguration>();
			mock.Protected().Setup<ZBool>("SupportingDocumentsSupportCore", ItExpr.IsAny<BusinessObject>())
				.Returns(false);
			AssertTabPageVisibility(false, "SupportingDocumentsTabPage", mock.Object);
		}

		[RequiresSTA]
		public void TestSupportingDocumentsTabPage_Visible()
		{
			var mock = new Mock<InvoiceLineConfiguration>();
			mock.Protected().Setup<ZBool>("SupportingDocumentsSupportCore", ItExpr.IsAny<BusinessObject>())
				.Returns(true);
			AssertTabPageVisibility(true, "SupportingDocumentsTabPage", mock.Object);
		}

		[RequiresSTA]
		public void TestAdditionalInfosTabPage_Invisible()
		{
			var mock = new Mock<InvoiceLineConfiguration>();
			mock.Protected().Setup<ZBool>("AdditionalInfosSupportCore", ItExpr.IsAny<BusinessObject>())
				.Returns(false);
			AssertTabPageVisibility(false, "AdditionalInfosTabPage", mock.Object);
		}

		[RequiresSTA]
		public void TestAdditionalInfosTabPage_Visible()
		{
			var mock = new Mock<InvoiceLineConfiguration>();
			mock.Protected().Setup<ZBool>("AdditionalInfosSupportCore", ItExpr.IsAny<BusinessObject>())
				.Returns(true);
			AssertTabPageVisibility(true, "AdditionalInfosTabPage", mock.Object);
		}

		[RequiresSTA]
		public void TestPreviousDocumentsTabPage_Invisible()
		{
			var mock = new Mock<InvoiceLineConfiguration>();
			mock.Protected().Setup<ZBool>("PreviousDocumentsSupportCore", ItExpr.IsAny<JobDeclaration>())
				.Returns(false);
			AssertTabPageVisibility(false, "PreviousDocumentsTabPage", mock.Object);
		}

		[RequiresSTA]
		public void TestPreviousDocumentsTabPage_Visible()
		{
			var mock = new Mock<InvoiceLineConfiguration>();
			mock.Protected().Setup<ZBool>("PreviousDocumentsSupportCore", ItExpr.IsAny<JobDeclaration>())
				.Returns(true);
			AssertTabPageVisibility(true, "PreviousDocumentsTabPage", mock.Object);
		}

		[RequiresSTA]
		public void TestTaxTabPage_Invisible()
		{
			var mock = new Mock<InvoiceLineConfiguration>();
			mock.Protected().Setup<ZBool>("TaxSupportCore", ItExpr.IsAny<BusinessObject>())
				.Returns(false);
			AssertTabPageVisibility(false, "TaxTabPage", mock.Object);
		}

		[RequiresSTA]
		public void TestTaxTabPage_Visible()
		{
			var mock = new Mock<InvoiceLineConfiguration>();
			mock.Protected().Setup<ZBool>("TaxSupportCore", ItExpr.IsAny<BusinessObject>())
				.Returns(true);
			AssertTabPageVisibility(true, "TaxTabPage", mock.Object);
		}

		[RequiresSTA]
		public void TestBrandTabPage_Invisible()
		{
			var mock = new Mock<InvoiceLineConfiguration>();
			mock.Protected().Setup<ZBool>("VehicleSupportCore", ItExpr.IsAny<BusinessObject>())
				.Returns(false);
			AssertTabPageVisibility(false, "VehicleTabPage", mock.Object);
		}

		[RequiresSTA]
		public void TestBrandTabPage_Visible()
		{
			var mock = new Mock<InvoiceLineConfiguration>();
			mock.Protected().Setup<ZBool>("VehicleSupportCore", ItExpr.IsAny<BusinessObject>())
				.Returns(true);
			AssertTabPageVisibility(true, "VehicleTabPage", mock.Object);
		}

		[RequiresSTA]
		public void TestValueIndicatorsTabPage_Invisible()
		{
			var mock = new Mock<InvoiceLineConfiguration>();
			mock.Protected().Setup<ZBool>("ValueIndicatorsSupportCore", ItExpr.IsAny<BusinessObject>())
				.Returns(false);
			AssertTabPageVisibility(false, "ValueIndicatorsTabPage", mock.Object);
		}

		[RequiresSTA]
		public void TestValueIndicatorsTabPage_Visible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			using (var form = new ZForm(declaration))
			using (var control = new EUInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals("Visible", true, control.FindSingle<ZTabPage>("ValueIndicatorsTabPage").TabVisible);
			}
		}

		[RequiresSTA]
		public void TestValueIndicatorsTabPage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			using (var form = new ZForm(declaration))
			using (var control = new EUInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();
				AssertEquals("ValueIndicatorsTabPage", "Valuation Indicators", control.FindSingle<ZTabPage>("ValueIndicatorsTabPage").CaptionResourceString.Caption);
			}
		}

		[RequiresSTA]
		public void TestValuationIndicatorsUserControlType()
		{
			using (var control = new EUInvoiceLineUserControlForTest())
			{
				AssertEquals(typeof(InvoiceLineValuationIndicatorCheckboxesUserControl), control.GetValuationIndicatorUserControlTypeExposed());
			}
		}

		public void TestAdditionalProcedureCodeApplicable()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				using (var form = new JobDeclarationForm(declaration))
				{
					declaration.JE_ApplicationCode = "CHF";
					form.Show();
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;

					var invoiceLineControl = (EUInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
					AssertEquals(false, ((ZButton)(invoiceLineControl.Controls.Find("zButtonMoreAdditionalProcedureCode", true)[0])).Visible);
					AssertEquals(false, ((ZTextBox)(invoiceLineControl.Controls.Find("zTextBoxAddtionalProcedureCodeAsString", true)[0])).Visible);
				}
				using (var form = new JobDeclarationForm(declaration))
				{
					declaration.JE_ApplicationCode = "CDS";
					form.Show();
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;

					var invoiceLineControl = (EUInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
					AssertEquals(true, ((ZButton)(invoiceLineControl.Controls.Find("zButtonMoreAdditionalProcedureCode", true)[0])).Visible);
					AssertEquals(true, ((ZTextBox)(invoiceLineControl.Controls.Find("zTextBoxAddtionalProcedureCodeAsString", true)[0])).Visible);
				}
			}
		}

		public void TestSupplementaryCodes()
		{
			using (var control = new EUOrgSupplierPartFormCustomsControl())
			{
				AssertNotNull("CI_AdditionalSupplementsTextBox", control.FindSingle<ZTextBox>(x => x.Name == "CI_AdditionalSupplementsTextBox"));
				AssertNotNull("AdditionalSupplementaryCodesEditButton", control.FindSingle<ZButton>(x => x.Name == "AdditionalSupplementaryCodesEditButton"));
			}
		}

		[RequiresSTA]
		public void TestColumnTypes()
		{
			using (var control = new EUInvoiceLineUserControlForTest())
			{
				var map = new Dictionary<string, Type>();
				map.Add(JobComInvoiceLine.Schema.MergedLineNumber, typeof(ZTextBoxColumnStyleInfo));
				map.Add(JobComInvoiceLine.Schema.JI_FormattedTariff, typeof(Universal.GUI.TariffColumnStyleInfo));
				map.Add(JobComInvoiceLine.Schema.JI_FormattedProcedure, typeof(ZCodeFindBoxColumnStyleInfo));
				map.Add(JobComInvoiceLine.Schema.JI_SupplementaryCode1, typeof(ZMultiControlColumnStyleInfo));
				map.Add(JobComInvoiceLine.Schema.JI_SupplementaryCode2, typeof(ZMultiControlColumnStyleInfo));
				map.Add(JobComInvoiceLine.Schema.JI_CustomsSecondQuantity, typeof(ZCalcEditColumnStyleInfo));
				map.Add(JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty, typeof(ZDropEditColumnStyleInfo));
				map.Add(JobComInvoiceLine.Schema.JI_CountryOfOrigin, typeof(ZDropEditColumnStyleInfo));
				map.Add(JobComInvoiceLine.Schema.ZG_CommercialReference, typeof(ZTextBoxColumnStyleInfo));
				var columnStyles = control.CustomsInvoiceLinesBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
				CombineAssertions(() =>
				{
					foreach (var columnName in map.Keys)
					{
						var columnStyle = columnStyles.Where(x => x.ColumnName == columnName).SingleOrDefault();
						var type = map[columnName];
						AssertType($"Column of name {columnName} should have a style of type {type}", type, columnStyle);
					}
				});
			}
		}

		public void TestDefaultSupplementaryCodeControlType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var control = (EUInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;

				var supplementaryCode1DropEdit = control.Controls.Find("SupplementaryCode1DropEdit", true)[0] as ZDropEdit;
				var supplementaryCode2DropEdit = control.Controls.Find("SupplementaryCode2DropEdit", true)[0] as ZDropEdit;
				AssertType<ZDropEdit>(supplementaryCode1DropEdit);
				Assert(supplementaryCode1DropEdit.Visible);
				Assert(supplementaryCode1DropEdit.ShowDescriptionInDropDown);
				AssertType<ZDropEdit>(supplementaryCode2DropEdit);
				Assert(supplementaryCode2DropEdit.Visible);
				Assert(supplementaryCode2DropEdit.ShowDescriptionInDropDown);

				var supplementaryCode1TextBox = control.Controls.Find("SupplementaryCode1TextBox", true)[0];
				var supplementaryCode2TextBox = control.Controls.Find("SupplementaryCode2TextBox", true)[0];
				AssertType<ZTextBox>(supplementaryCode1TextBox);
				Assert(!supplementaryCode1TextBox.Visible);
				AssertType<ZTextBox>(supplementaryCode2TextBox);
				Assert(!supplementaryCode2TextBox.Visible);
			}
		}

		public void TestMergeLineSortOrder()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceNumber = "INV1234";

			var invoiceLine1 = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine1.JI_PartNo = "DOGS";
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_FormattedTariff = "1";
			AssertEquals("First InvoiceLine LineNo = 1", 1, invoiceLine1.JI_LineNo.ToZInt());

			var invoiceLine2 = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = "CATS";
			invoiceLine2.JI_Tariff = "2";
			invoiceLine2.JI_FormattedTariff = "2";
			AssertEquals("Second InvoiceLine LineNo = 2", 2, invoiceLine2.JI_LineNo.ToZInt());

			var invoiceLine3 = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine3.JI_PartNo = "DOGS";
			invoiceLine3.JI_Tariff = "1";
			invoiceLine3.JI_FormattedTariff = "1";
			AssertEquals("Third InvoiceLine LineNo = 3", 3, invoiceLine3.JI_LineNo.ToZInt());

			var invoiceLine4 = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine4.JI_PartNo = "CATS";
			invoiceLine4.JI_Tariff = "2";
			invoiceLine4.JI_FormattedTariff = "2";
			AssertEquals("Forth InvoiceLine LineNo = 4", 4, invoiceLine4.JI_LineNo.ToZInt());

			var invoiceLine5 = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine5.JI_PartNo = "DOGS";
			invoiceLine5.JI_Tariff = "1";
			invoiceLine5.JI_FormattedTariff = "1";
			AssertEquals("Fifth InvoiceLine LineNo = 5", 5, invoiceLine5.JI_LineNo.ToZInt());

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(true);
			Factory.Save();
			declaration.DoMerge();

			declaration.CustomsEntryHeaders[0].EntryNumber = "191-600103X";
			ZString entryMergeLine1 = "1/191-600103X";
			ZString entryMergeLine2 = "2/191-600103X";

			var inv2 = declaration.Invoices.AddNew();
			inv2.JZ_ValuationDateOverride = ZDateTime.Today.AddDays(-1);
			var inv2Line1 = inv2.JobComInvoiceLines.AddNew();
			inv2Line1.JI_Tariff = "2203";
			inv2Line1.JI_PartNo = "WOODY";
			inv2Line1.JI_FormattedTariff = "5";
			var inv2Line2 = inv2.JobComInvoiceLines.AddNew();
			inv2Line2.JI_Tariff = "3033";
			inv2Line2.JI_PartNo = "BUZZ";
			inv2Line2.JI_FormattedTariff = "4";
			Factory.Save();
			declaration.DoMerge();
			AssertEquals("Entry Header Count", 2, declaration.CustomsEntryHeaders.Count);

			var invLineView = declaration.FilteredInvoiceLines;

			invLineView.Sort(BaseJobComInvoiceLine.Schema.MergedLineNumber, ListSortDirection.Ascending);
			AssertEquals("1st InvoiceGridLine Merge LineNo = 1", "1", invLineView[0].MergedLineNumber);
			AssertEquals("2nd InvoiceGridLine Merge LineNo = 2", "2", invLineView[1].MergedLineNumber);
			AssertEquals("3rd InvoiceGridLine Merge LineNo = 1 191-600103X", entryMergeLine1, invLineView[2].MergedLineNumber);
			AssertEquals("4th InvoiceGridLine Merge LineNo = 1 191-600103X", entryMergeLine1, invLineView[3].MergedLineNumber);
			AssertEquals("5th InvoiceGridLine Merge LineNo = 1 191-600103X", entryMergeLine1, invLineView[4].MergedLineNumber);
			AssertEquals("6th InvoiceGridLine Merge LineNo = 2 191-600103X", entryMergeLine2, invLineView[5].MergedLineNumber);
			AssertEquals("7th InvoiceGridLine Merge LineNo = 2 191-600103X", entryMergeLine2, invLineView[6].MergedLineNumber);

			invLineView.Sort(BaseJobComInvoiceLine.Schema.MergedLineNumber, ListSortDirection.Descending);
			AssertEquals("1st InvoiceGridLine Merge LineNo = 2", "2", invLineView[0].MergedLineNumber);
			AssertEquals("2nd InvoiceGridLine Merge LineNo = 1", "1", invLineView[1].MergedLineNumber);
			AssertEquals("3rd InvoiceGridLine Merge LineNo = 2 191-600103X", entryMergeLine2, invLineView[2].MergedLineNumber);
			AssertEquals("4th InvoiceGridLine Merge LineNo = 2 191-600103X", entryMergeLine2, invLineView[3].MergedLineNumber);
			AssertEquals("5th InvoiceGridLine Merge LineNo = 1 191-600103X", entryMergeLine1, invLineView[4].MergedLineNumber);
			AssertEquals("6th InvoiceGridLine Merge LineNo = 1 191-600103X", entryMergeLine1, invLineView[5].MergedLineNumber);
			AssertEquals("7th InvoiceGridLine Merge LineNo = 1 191-600103X", entryMergeLine1, invLineView[6].MergedLineNumber);

			declaration.CustomsEntryHeaders[1].EntryNumber = "222-99999Y";
			invLineView.Sort(BaseJobComInvoiceLine.Schema.MergedLineNumber, ListSortDirection.Ascending);
			AssertEquals("1st InvoiceGridLine Merge LineNo = 1 191-600103X", entryMergeLine1, invLineView[0].MergedLineNumber);
			AssertEquals("2nd InvoiceGridLine Merge LineNo = 1 191-600103X", entryMergeLine1, invLineView[1].MergedLineNumber);
			AssertEquals("3rd InvoiceGridLine Merge LineNo = 1 191-600103X", entryMergeLine1, invLineView[2].MergedLineNumber);
			AssertEquals("4th InvoiceGridLine Merge LineNo = 2 191-600103X", entryMergeLine2, invLineView[3].MergedLineNumber);
			AssertEquals("5th InvoiceGridLine Merge LineNo = 2 191-600103X", entryMergeLine2, invLineView[4].MergedLineNumber);
			AssertEquals("6th InvoiceGridLine Merge LineNo = 1 222-99999Y", "1/222-99999Y", invLineView[5].MergedLineNumber);
			AssertEquals("7th InvoiceGridLine Merge LineNo = 2 222-99999Y", "2/222-99999Y", invLineView[6].MergedLineNumber);
		}

		[RequiresSTA]
		public void TestDefaultColumns()
		{
			using (var control = new EUInvoiceLineUserControl())
			{
				var declaration = Factory.New<JobDeclaration>();
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();

				var lineGrid = control.CustomsInvoiceLinesBoundGrid;
				var styles = lineGrid.ColumnStyles;

				AssertEquals(0, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_LineNo)));
				AssertEquals(1, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Calc_Invoice)));
				AssertEquals(2, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CEI)));
				AssertEquals(4, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_PartNo)));
				AssertEquals(5, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedTariff)));
				AssertEquals(6, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Description)));
				AssertEquals(7, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedProcedure)));
				AssertEquals(8, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_InvoiceQuantity)));
				AssertEquals(9, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CountryOfOrigin)));
				AssertEquals(10, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsQuantity)));
				AssertEquals(11, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_LinePrice)));
				AssertEquals(12, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_SupplementaryCode1)));
				AssertEquals(13, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_SupplementaryCode2)));
				AssertEquals(14, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsSecondQuantity)));
				AssertEquals(15, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty)));
				AssertEquals(16, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.EntryReferenceNumber)));
				AssertEquals(17, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.MergedLineNumber)));
				AssertEquals(52, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.ZG_CommercialReference)));

				AssertEquals("JI_LineNo is visible.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_LineNo).IsVisible);
				AssertEquals("JI_Calc_Invoice is visible.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Calc_Invoice).IsVisible);
				AssertEquals("JI_CEI is visible.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CEI).IsVisible);
				AssertEquals("EntryInstructionDescription is visible.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.EntryInstructionDescription).IsVisible);
				AssertEquals("JI_PartNo is visible.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_PartNo).IsVisible);
				AssertEquals("JI_FormattedTariff is visible.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedTariff).IsVisible);
				AssertEquals("JI_Description is visible.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Description).IsVisible);
				AssertEquals("JI_FormattedProcedure is visible.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedProcedure).IsVisible);
				AssertEquals("JI_InvoiceQuantity is visible.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_InvoiceQuantity).IsVisible);
				AssertEquals("JI_CountryOfOrigin is visible.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CountryOfOrigin).IsVisible);
				AssertEquals("JI_CustomsQuantity is visible.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsQuantity).IsVisible);
				AssertEquals("JI_LinePrice is visible.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_LinePrice).IsVisible);
				AssertEquals("JI_SupplementaryCode1 is visible.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_SupplementaryCode1).IsVisible);
				AssertEquals("JI_SupplementaryCode2 is visible.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_SupplementaryCode2).IsVisible);
				AssertEquals("JI_CustomsSecondQuantity is visible.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsSecondQuantity).IsVisible);
				AssertEquals("JI_CustomsSecondUnitQty is visible.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty).IsVisible);
				AssertEquals("EntryReferenceNunber is visible.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.EntryReferenceNumber).IsVisible);
				AssertEquals("MergedLineNumber is visible.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.MergedLineNumber).IsVisible);
				AssertEquals("Commercial Reference should not be visible by default", false, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.ZG_CommercialReference).IsVisible);
				AssertEquals("Commercial Reference should not available by default", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.ZG_CommercialReference).IsUnavailable);
			}
		}

		[RequiresSTA]
		public void TestTariffFindBoxAndColumn()
		{
			using (var control = new EUInvoiceLineUserControl())
			{
				var declaration = Factory.New<JobDeclaration>();
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();

				AssertNull(control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(BaseJobComInvoiceLine.Schema.JI_Tariff));
				AssertType<Universal.GUI.TariffColumnStyleInfo>("TariffColumnStyleInfo", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedTariff));
				AssertType<Universal.GUI.TariffFindBox>(control.Controls.Find("TariffFindBox", true)[0]);
			}
		}

		[RequiresSTA]
		public void TestHookInvoiceLineEventsIsCalledAfterBind()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.Invoices.AddNew().InvoiceLines.AddNew();

			using (var form = new ZForm())
			using (var control = new EUInvoiceLineUserControlForTest())
			{
				control.JobDeclaration = dec;
				form.Controls.Add(control);
				form.Show();
				control.HookInvoiceLineEventsCalled = false;
				control.SetDataBinding(dec, "");
				AssertEquals("Control is bound, HookInvoiceLineEventsCalled is called", true, control.HookInvoiceLineEventsCalled);
			}
		}

		[RequiresSTA]
		public void TestLineGridColumnWidths()
		{
			using (var control = new EUInvoiceLineUserControl())
			{
				var declaration = Factory.New<JobDeclaration>();
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				var lineGrid = control.CustomsInvoiceLinesBoundGrid;

				CombineAssertions(() =>
				{
					AssertEquals("JI_CustomsQuantity", ControlDpiScalingHelper.ScaleToCurrentDpiX(140), lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsQuantity).Width);
					AssertEquals("JI_FormattedProcedure", ControlDpiScalingHelper.ScaleToCurrentDpiX(51), lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedProcedure).Width);
					AssertEquals("JI_CEI", ControlDpiScalingHelper.ScaleToCurrentDpiX(104), lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CEI).Width);
					AssertEquals("JI_CountryOfOrigin", ControlDpiScalingHelper.ScaleToCurrentDpiX(76), lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CountryOfOrigin).Width);
					AssertEquals("ZG_CommercialReference", ControlDpiScalingHelper.ScaleToCurrentDpiX(80), lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.ZG_CommercialReference).Width);
				});
			}
		}

		[RequiresSTA]
		public void TestColumnJI_CustomsSecondQuantity()
		{
			using (var form = new ZForm())
			using (var control = new EUInvoiceLineUserControlForTest())
			{
				var declaration = Factory.New<JobDeclaration>();
				control.JobDeclaration = declaration;

				control.InitializeGridLayout();
				form.Show();

				var groupNameDetails = "Supplementary Qty/UQ";
				AssertEquals("Column Group Name", groupNameDetails, control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(EU.Business.Declaration.JobComInvoiceLine.Schema.JI_CustomsSecondQuantity).GroupName.Caption);
			}
		}

		[RequiresSTA]
		public void TestColumnJI_CustomsSecondUnitQty()
		{
			using (var form = new ZForm())
			using (var control = new EUInvoiceLineUserControlForTest())
			{
				var declaration = Factory.New<JobDeclaration>();
				control.JobDeclaration = declaration;

				control.InitializeGridLayout();
				form.Show();

				var groupNameDetails = "Supplementary Qty/UQ";
				AssertEquals("Column Group Name", groupNameDetails, control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(EU.Business.Declaration.JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty).GroupName.Caption);
			}
		}

		[RequiresSTA]
		public void TestColumnJI_CustomsQuantity()
		{
			using (var form = new ZForm())
			using (var control = new EUInvoiceLineUserControlForTest())
			{
				var declaration = Factory.New<JobDeclaration>();
				control.JobDeclaration = declaration;

				control.InitializeGridLayout();
				form.Show();

				AssertEquals("Column Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140), control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(EU.Business.Declaration.JobComInvoiceLine.Schema.JI_CustomsQuantity).Width);
			}
		}

		[RequiresSTA]
		public void TestInvoiceLineVehicleUserControlType()
		{
			using (var form = new ZForm())
			using (var control = new EUInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var invoiceLineVehicleUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("VehicleUserControl");
				AssertEquals("VehicleUserControl.UserControlType", typeof(InvoiceLineVehicleUserControl), invoiceLineVehicleUserControl.UserControlType);
			}
		}

		[RequiresSTA]
		public void TestAuthorisationsForInvoiceLineUserControlType()
		{
			var invoiceLineConfigurationMock = new Mock<InvoiceLineConfiguration>();
			invoiceLineConfigurationMock.Protected()
				.Setup<ZBool>("AuthorisationsSupportForInvoiceLineCore", ItExpr.IsAny<JobDeclaration>())
				.Returns(true);

			using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(Factory, "GetNewInvoiceLineConfiguration", invoiceLineConfigurationMock.Object, null))
			using (var form = new ZForm())
			using (var control = new EUInvoiceLineUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var invoiceLineAuthorisationsUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("AuthorisationsUserControl");
				AssertEquals("AuthorisationsUserControl.UserControlType", typeof(InvoiceLineAuthorisationsUserControl), invoiceLineAuthorisationsUserControl.UserControlType);
			}
		}

		[RequiresSTA]
		public void TestAuthorisationsTabPage_Visible()
		{
			var mock = new Mock<InvoiceLineConfiguration>();
			mock.Protected().Setup<ZBool>("AuthorisationsSupportForInvoiceLineCore", ItExpr.IsAny<JobDeclaration>())
				.Returns(true);
			AssertTabPageVisibility(true, "AuthorisationsTabPage", mock.Object);
		}

		[RequiresSTA]
		public void TestAuthorisationsTabPage_Invisible()
		{
			var mock = new Mock<InvoiceLineConfiguration>();
			mock.Protected().Setup<ZBool>("AuthorisationsSupportForInvoiceLineCore", ItExpr.IsAny<JobDeclaration>())
				.Returns(false);
			AssertTabPageVisibility(false, "AuthorisationsTabPage", mock.Object);
		}

		void AssertTabPageVisibility(ZBool visible, ZString tabPageName, InvoiceLineConfiguration configuration)
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new EUInvoiceLineUserControl())
			{
				ConfigurationTestHelper.ClearDeclarationConfiguration(declaration);
				ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(declaration.Factory, "GetNewInvoiceLineConfiguration", configuration, declaration.GetDefaultDataGroupingCode());

				form.Controls.Add(control);
				form.Show();

				if (visible)
				{
					AssertEquals("Visible", true, control.FindSingle<ZTabPage>(tabPageName).TabVisible);
				}
				else
				{
					AssertNull("Invisible", control.FindSingleOrDefault<ZTabPage>(tabPageName));
				}
			}
		}

		[RequiresSTA]
		public void TestIs_ZGCommercialReferenceVisibleDefaultValue()
		{
			using (var control = new EUInvoiceLineUserControlForTest())
			{
				AssertEquals("Commercial Reference should not be visible by default", false, control.GetIsZG_CommercialReferenceVisible());
			}
		}

		[RequiresSTA]
		public void TestCommercialReferenceVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new EUInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var commercialReferenceBox = control.Controls.Find("CommercialReferenceBox", true)[0];
				AssertNotNull("For Import Declaration, Commercial Reference must be available on the screen", commercialReferenceBox);
				Assert("Commercial Reference should not be visible by default", !commercialReferenceBox.Visible);
			}
		}

		[RequiresSTA]
		public void TestIsZG_TransactionNatureVisibleDefaultValue()
		{
			using (var control = new EUInvoiceLineUserControlForTest())
			{
				AssertEquals("Transaction Nature should not be visible by default", false, control.GetIsZG_TransactionNatureVisible());
			}
		}

		[RequiresSTA]
		public void TestTransactionNatureVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new EUInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				Assert("Transaction Nature should not be visible by default", !control.TransactionNatureDropEdit.Visible);
			}
		}

		[RequiresSTA]
		public void TestOrganizationsUserControlType()
		{
			using (var control = new EUInvoiceLineUserControlForTest())
			{
				AssertEquals(typeof(InvoiceLineOrganizationsUserControl), control.GetOrganizationsUserControlTypeExposed());
			}
		}

		[RequiresSTA]
		public void TestInvoiceLinePaymentTabPageVisibilityAndUserControlType()
		{
			var declaration = Factory.New<JobDeclaration>();

			ConfigurationTestHelper.ClearDeclarationConfiguration(declaration);

			var invoiceLineConfigurationMock = new Mock<InvoiceLineConfiguration>();
			invoiceLineConfigurationMock.Protected()
				.Setup<ZBool>("InvoiceLinePaymentSupportCore", ItExpr.IsAny<BusinessObject>())
				.Returns(true);

			using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(declaration.Factory, "GetNewInvoiceLineConfiguration", invoiceLineConfigurationMock.Object, declaration.GetDefaultDataGroupingCode()))
			using (var form = new ZForm(declaration))
			using (var control = new EUInvoiceLineUserControl())
			{
				declaration.Invoices.AddNew().InvoiceLines.AddNew();
				form.Controls.Add(control);
				form.Show();
				var invoiceLinePaymentTabPage = control.FindSingle<ZTabPage>("InvoiceLinePaymentTabPage");
				AssertEquals("InvoiceLinePaymentTabPage is visible due to the configuration.", true, invoiceLinePaymentTabPage.TabVisible);

				invoiceLinePaymentTabPage.NotifyBindingOrShowing();
				Application.DoEvents();
				var invoiceLinePaymentUserControl1 = control.FindSingle<ZDynamicControlCreationUserControl>("invoiceLinePaymentUserControl1");
				AssertEquals(typeof(InvoiceLinePaymentUserControl), invoiceLinePaymentUserControl1.UserControlType);
			}
		}

		[RequiresSTA]
		public void TestInitTabsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new EUInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var invoiceLinePaymentTabPage = control.FindSingleOrDefault<ZTabPage>("InvoiceLinePaymentTabPage");
				AssertNull("invoiceLinePaymentTabPage is not visible when init.", invoiceLinePaymentTabPage);
			}
		}

		[RequiresSTA]
		public void TestSupplyChainActorReferenceUserControl()
		{
			using (var control = new EUImportInvoiceLineUserControl())
			{
				var userControl = control.FindSingle<ZUserControl>("SupplyChainActorReferencesUserControl");
				CombineAssertions(() =>
				{
					AssertEquals("Type", typeof(SupplyChainActorReferencesUserControl), userControl.GetType());
					AssertEquals("DockStyle", DockStyle.Fill, userControl.Dock);
				});
			}
		}

		[RequiresSTA]
		public void TestSupplyChainActorTabPage()
		{
			using (var control = new EUImportInvoiceLineUserControl())
			{
				AssertEquals("Add. Supply Chain Actors", control.FindSingle<ZTabPage>("SupplyChainActorTabPage").CaptionResourceString.Caption);
			}
		}

		[RequiresSTA]
		public void TestSupplyChainActorTabPage_Invisible()
		{
			var mock = new Mock<InvoiceLineConfiguration>();
			mock.Protected().Setup<ZBool>("AdditionalSupplyChainActorSupportCore", ItExpr.IsAny<JobDeclaration>())
				.Returns(false);
			AssertTabPageVisibility(false, "SupplyChainActorTabPage", mock.Object);
		}

		[RequiresSTA]
		public void TestSupplyChainActorTabPage_Visible()
		{
			var mock = new Mock<InvoiceLineConfiguration>();
			mock.Protected().Setup<ZBool>("AdditionalSupplyChainActorSupportCore", ItExpr.IsAny<JobDeclaration>())
				.Returns(true);
			AssertTabPageVisibility(true, "SupplyChainActorTabPage", mock.Object);
		}

		class EUInvoiceLineUserControlForTest : EUInvoiceLineUserControl
		{
			public EUInvoiceLineUserControlForTest()
			{
				InitializeGridLayoutCore();
			}

			public EUInvoiceLineUserControlForTest(bool callBaseToo)
				: base()
			{
			}

			public bool HookInvoiceLineEventsCalled;

			protected override void HookInvoiceLineEvents(BaseJobComInvoiceLine invoiceLine)
			{
				HookInvoiceLineEventsCalled = true;
			}

			public bool GetIsZG_CommercialReferenceVisible()
			{
				return IsZG_CommercialReferenceVisible;
			}

			public bool GetIsZG_TransactionNatureVisible()
			{
				return IsZG_TransactionNatureVisible;
			}

			public Type GetOrganizationsUserControlTypeExposed() => base.GetOrganizationsUserControlType();

			public Type GetValuationIndicatorUserControlTypeExposed() => base.GetValuationIndicatorsUserControlType();

			protected override bool IsCifCalculationVisible => true;
			protected override bool IsCustomsValueCalulationVisible => true;
			protected override bool IsValueForVatGstCalculationVisible => true;
			protected override bool IsStatisticalValueCalculationVisible => true;
		}
	}
}
