using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	public sealed class ImportSiscomexInvoiceLineUserControlTest : BaseInvoiceLineUserControlAbstractTest
	{
		public void TestColumnLayoutContextForInvoiceLinesGrid()
		{
			using (var control = new ImportSiscomexInvoiceLineUserControl())
			{
				AssertEquals("Column layout context should be ISW", "ISW", control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestDefaultColumns()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var header = jobDeclaration.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclaration))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (ImportSiscomexInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					var visibleColumnCount = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Count(x => x.IsVisible);
					CombineAssertions(() =>
					{
						AssertEquals(57, visibleColumnCount);

						Assert("NaladiNcca column in CustomsInvoiceLinesBoundGrid", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.NaladiNcca));
						Assert("NaladiNcca column must be default", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.NaladiNcca].IsVisible);
						AssertEquals("NaladiNcca", "NALADI/NCCA", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.NaladiNcca].ColumnStyle.HeaderText);
						Assert("NaladiHs column in CustomsInvoiceLinesBoundGrid", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.NaladiHs));
						Assert("NaladiHs column must be default", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.NaladiHs].IsVisible);
						AssertEquals("NaladiHs", "NALADI/HS", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.NaladiHs].ColumnStyle.HeaderText);
						Assert("JI_Calc_MergedLineNumber column in CustomsInvoiceLinesBoundGrid", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_Calc_MergedLineNumber));
						Assert("JI_Calc_MergedLineNumber column must be default", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_Calc_MergedLineNumber].IsVisible);
						Assert("JI_ManufacturerIndicator column in CustomsInvoiceLinesBoundGrid", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_ManufacturerIndicator));
						AssertEquals("JI_ManufacturerIndicator", "Manufacturer Ind.", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_ManufacturerIndicator].ColumnStyle.HeaderText);
						Assert("ManufacturerOrgPK column in CustomsInvoiceLinesBoundGrid", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.ManufacturerOrgPK));
						AssertEquals("ManufacturerOrgPK", "Manufacturer", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.ManufacturerOrgPK].ColumnStyle.HeaderText);
						Assert("JI_OA_ManufacturerAddress column in CustomsInvoiceLinesBoundGrid", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_OA_ManufacturerAddress));
						AssertEquals("JI_OA_ManufacturerAddress", "Manufacturer Address", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_OA_ManufacturerAddress].ColumnStyle.HeaderText);
						Assert("ImportLicenseNumber column must be default", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.ImportLicenseNumber].IsVisible);
						AssertEquals("ImportLicenseNumber", "Import License No", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.ImportLicenseNumber].ColumnStyle.HeaderText);
						Assert("JI_GoodsApplication column must be default", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_GoodsApplication].IsVisible);
						AssertEquals("JI_GoodsApplication", "Goods Application", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_GoodsApplication].ColumnStyle.HeaderText);
						Assert("ImportLicenseReference column must be default", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns["AttachedImportLicenseLine+Declaration+JE_DeclarationReference"].IsVisible);
						AssertEquals("ImportLicenseReference", "Import License Ref.", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns["AttachedImportLicenseLine+Declaration+JE_DeclarationReference"].ColumnStyle.HeaderText);
						Assert("LineNoReference column must be default", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns["AttachedImportLicenseLine+JI_LineNo"].IsVisible);
						Assert("JI_GoodsCondition column must be default", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_GoodsCondition].IsVisible);
						AssertEquals("JI_GoodsCondition", "Goods Condition", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_GoodsCondition].ColumnStyle.HeaderText);

						Assert("DutyTaxRegime column must be default", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DutyTaxRegime].IsVisible);
						AssertEquals("DutyTaxRegime", "Duty Tax Regime", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DutyTaxRegime].ColumnStyle.HeaderText);
						Assert("DutyLegalBase column must be default", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DutyLegalBase].IsVisible);
						AssertEquals("DutyLegalBase", "Duty Legal Base", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DutyLegalBase].ColumnStyle.HeaderText);
						Assert("IPITaxRegime column must be default", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.IPITaxRegime].IsVisible);
						AssertEquals("IPITaxRegime", "IPI Tax Regime", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.IPITaxRegime].ColumnStyle.HeaderText);
						Assert("JI_ComplementaryNote column must be default", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_ComplementaryNote].IsVisible);
						AssertEquals("JI_ComplementaryNote", "TIPI Complementary Note", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_ComplementaryNote].ColumnStyle.HeaderText);
						Assert("PisCofinsTaxRegime column must be default", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.PisCofinsTaxRegime].IsVisible);
						AssertEquals("PisCofinsTaxRegime", "PIS/COFINS Tax Regime", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.PisCofinsTaxRegime].ColumnStyle.HeaderText);
						Assert("PisCofinsLegalBase column must be default", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.PisCofinsLegalBase].IsVisible);
						AssertEquals("PisCofinsLegalBase", "PIS/COFINS Legal Base", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.PisCofinsLegalBase].ColumnStyle.HeaderText);
						Assert("ICMSTaxRegime column must be default", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.ICMSTaxRegime].IsVisible);
						AssertEquals("ICMSTaxRegime", "ICMS Tax Regime", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.ICMSTaxRegime].ColumnStyle.HeaderText);
						Assert("ICMSLegalBase column must be default", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.ICMSLegalBase].IsVisible);
						AssertEquals("ICMSLegalBase", "ICMS Legal Base", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.ICMSLegalBase].ColumnStyle.HeaderText);
						Assert("JI_ICMSRate column must be default", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_ICMSRate].IsVisible);
						AssertEquals("JI_ICMSRate", "ICMS Rate (%)", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_ICMSRate].ColumnStyle.HeaderText);

						Assert("FMMBenefit column must be default", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.FMMBenefit].IsVisible);
						AssertEquals("FMMBenefit caption", "FMM Benefit", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.FMMBenefit].ColumnStyle.HeaderText);
						AssertEquals("FMMBenefit GroupName", "FMM Benefit", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.FMMBenefit].GroupName.Caption);
						Assert("FMMBenefitDescription column must be default", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.FMMBenefitDescription].IsVisible);
						AssertEquals("FMMBenefitDescription caption", "FMM Benefit Description", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.FMMBenefitDescription].ColumnStyle.HeaderText);
						AssertEquals("FMMBenefitDescription GroupName", "FMM Benefit", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.FMMBenefitDescription].GroupName.Caption);

						Assert("MercosulForeignDeclarationType column must NOT be default", !invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.MercosulForeignDeclarationType].IsVisible);
						Assert("JI_ICMSBaseValueReductionPercentage column must NOT be default", !invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_ICMSBaseValueReductionPercentage].IsVisible);
						Assert("JI_RequiresImportLicense column must NOT be default", !invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_RequiresImportLicense].IsVisible);
						Assert("JI_ICMSTotalAmountReductionPercentage column must NOT be default", !invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_ICMSTotalAmountReductionPercentage].IsVisible);
						Assert("JI_ICMSFormula column must NOT be default", !invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_ICMSFormula].IsVisible);

						Assert("IPITaxBenefitLegalActType column must NOT be default", !invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.IPITaxBenefitLegalActType].IsVisible);
						Assert("IPITaxBenefitLegalActIssuingBody column must NOT be default", !invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.IPITaxBenefitLegalActIssuingBody].IsVisible);
						Assert("IPITaxBenefitLegalActNumber column must NOT be default", !invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.IPITaxBenefitLegalActNumber].IsVisible);
						Assert("IPITaxBenefitLegalActYear column must NOT be default", !invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.IPITaxBenefitLegalActYear].IsVisible);

						Assert("ImportLicenseType column must NOT be default", !invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.ImportLicenseType].IsVisible);
						Assert("ImportLicenseAuthorizationDate column must NOT be default", !invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.ImportLicenseAuthorizationDate].IsVisible);
						Assert("ImportLicenseFeeType column must NOT be default", !invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.ImportLicenseFeeType].IsVisible);

						Assert("ICMSFCPRateValue column must NOT be default", !invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.ICMSFCPRateValue].IsVisible);
						Assert("Should have ManufacturerName column", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.ManufacturerName));
					});
				}
			}
		}

		public void TestImportLabels()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

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
			jobDeclartion.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = ((ImportSiscomexInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.BRTariffDetailsTab;
				Assert(invoiceLineUserControl.ImportTariffDetailsUserControl.Visible);
				Assert(invoiceLineUserControl.ImportTariffDetailsUserControl.NveGridLayout.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.LinkedDocumentTab;
				Assert(invoiceLineUserControl.LinkedDocumentUserControl.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.MercosulForeignDeclarationTabPage;
				Assert(invoiceLineUserControl.MercosulForeignDeclarationUserControl.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.SpecialCasesTabPage;
				Assert(invoiceLineUserControl.SpecialCasesUserControl.Visible);
			}
		}

		protected override string JobMessageType => BRJobMessageTypeList.Codes.ImportSiscomex;

		protected override List<ZString> ExpectedColumnNamesListInOrder
		{
			get
			{
				if (expectedColumnNamesListOnThisOrder == null)
				{
					expectedColumnNamesListOnThisOrder = new List<ZString>
					{
						JobComInvoiceLine.Schema.JI_LineNo,
						JobComInvoiceLine.Schema.JI_Calc_Invoice,
						JobComInvoiceLine.Schema.JI_PartNo,
						JobComInvoiceLine.Schema.JI_CC,
						JobComInvoiceLine.Schema.JI_Tariff,
						JobComInvoiceLine.Schema.FMMBenefit,
						JobComInvoiceLine.Schema.FMMBenefitDescription,
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
						JobComInvoiceLine.Schema.JI_CustomAttrib1,
						JobComInvoiceLine.Schema.JI_CustomAttrib2,
						JobComInvoiceLine.Schema.JI_CustomAttrib3,
						JobComInvoiceLine.Schema.JI_CustomAttrib4,
						JobComInvoiceLine.Schema.JI_CustomAttrib5,
						JobComInvoiceLine.Schema.JI_CustomAttrib6,
						JobComInvoiceLine.Schema.JI_CustomTextBlob1,
						JobComInvoiceLine.Schema.JI_PartAttrib1,
						JobComInvoiceLine.Schema.JI_PartAttrib2,
						JobComInvoiceLine.Schema.JI_PartAttrib3,
						JobComInvoiceLine.Schema.JI_SerialNumber,
						JobComInvoiceLine.Schema.JI_CEI,
						JobComInvoiceLine.Schema.JI_Calc_MergedLineNumber,
						JobComInvoiceLine.Schema.JI_ManufacturerIndicator,
						JobComInvoiceLine.Schema.ManufacturerOrgPK,
						JobComInvoiceLine.Schema.JI_OA_ManufacturerAddress,
						JobComInvoiceLine.Schema.ManufacturerName,
						JobComInvoiceLine.Schema.JI_CountryOfOrigin,
						ImportSiscomexInvoiceLineUserControl.Schema.ImportLicenseReference,
						ImportSiscomexInvoiceLineUserControl.Schema.ImportLicenseLineNumber,
						JobComInvoiceLine.Schema.ImportLicenseNumber,
						JobComInvoiceLine.Schema.NaladiHs,
						JobComInvoiceLine.Schema.NaladiNcca,
						JobComInvoiceLine.Schema.JI_GoodsApplication,
						JobComInvoiceLine.Schema.JI_GoodsCondition,
						JobComInvoiceLine.Schema.DutyTaxRegime,
						JobComInvoiceLine.Schema.DutyLegalBase,
						JobComInvoiceLine.Schema.IPITaxRegime,
						JobComInvoiceLine.Schema.JI_ComplementaryNote,
						JobComInvoiceLine.Schema.PisCofinsTaxRegime,
						JobComInvoiceLine.Schema.PisCofinsLegalBase,
						JobComInvoiceLine.Schema.ICMSTaxRegime,
						JobComInvoiceLine.Schema.ICMSLegalBase,
						JobComInvoiceLine.Schema.JI_ICMSRate
					};
				}
				return expectedColumnNamesListOnThisOrder;
			}
		}
		List<ZString> expectedColumnNamesListOnThisOrder;
	}
}
