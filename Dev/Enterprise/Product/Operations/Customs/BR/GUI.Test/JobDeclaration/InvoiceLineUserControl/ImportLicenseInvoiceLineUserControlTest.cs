using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	public sealed class ImportLicenseInvoiceLineUserControlTest : BaseInvoiceLineUserControlAbstractTest
	{
		public void TestColumnLayoutContextForInvoiceLinesGrid()
		{
			using (var control = new ImportLicenseInvoiceLineUserControl())
			{
				AssertEquals("Column layout context should be import", BRJobMessageTypeList.Codes.ImportLicense, control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestDefaultColumns()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var header = jobDeclaration.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclaration))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (ImportLicenseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					var visibleColumnCount = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Count(x => x.IsVisible);
					CombineAssertions(() =>
					{
						AssertEquals(40, visibleColumnCount);
						Assert("Should have NaladiHs column in CustomsInvoiceLinesBoundGrid", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.NaladiHs));
						Assert("Should have NaladiHs column must be default", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.NaladiHs].IsVisible);
						AssertEquals("NaladiHs caption", "NALADI/HS", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.NaladiHs].ColumnStyle.HeaderText);
						Assert("Should have JI_Calc_MergedLineNumber column in CustomsInvoiceLinesBoundGrid", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_Calc_MergedLineNumber));
						Assert("Should have JI_Calc_MergedLineNumber column must be default", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.NaladiHs].IsVisible);
						AssertEquals("JI_Calc_MergedLineNumber caption", "Merged Ln. #", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_Calc_MergedLineNumber].ColumnStyle.HeaderText);
						Assert("Should have JI_ManufacturerIndicator column in CustomsInvoiceLinesBoundGrid", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_ManufacturerIndicator));
						AssertEquals("JI_ManufacturerIndicator caption", "Manufacturer Ind.", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_ManufacturerIndicator].ColumnStyle.HeaderText);
						Assert("Should have ManufacturerDocOrgPK column in CustomsInvoiceLinesBoundGrid", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.ManufacturerDocOrgPK));
						AssertEquals("ManufacturerDocOrgPK caption", "Manufacturer", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.ManufacturerDocOrgPK].ColumnStyle.HeaderText);
						Assert("Should have ManufacturerDocAddressPK column in CustomsInvoiceLinesBoundGrid", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.ManufacturerDocAddressPK));
						AssertEquals("ManufacturerDocAddressPK caption", "Manufacturer Address", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.ManufacturerDocAddressPK].ColumnStyle.HeaderText);
						Assert("Should have DutyTaxRegime column in CustomsInvoiceLinesBoundGrid", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.DutyTaxRegime));
						AssertEquals("DutyTaxRegime caption", "Duty Tax Regime", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DutyTaxRegime].ColumnStyle.HeaderText);
						Assert("Should have DrawbackCANumber column in CustomsInvoiceLinesBoundGrid", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.DrawbackCANumber));
						Assert("Should have DrawbackCANumber column must NOT be default", !invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DrawbackCANumber].IsVisible);
						Assert("Should have DrawbackItemNumber column in CustomsInvoiceLinesBoundGrid", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.DrawbackItemNumber));
						Assert("Should have DrawbackItemNumber column must NOT be default", !invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DrawbackCANumber].IsVisible);
						Assert("Should have DutyLegalBase column in CustomsInvoiceLinesBoundGrid", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.DutyLegalBase));
						AssertEquals("DutyLegalBase caption", "Duty Legal Base", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DutyLegalBase].ColumnStyle.HeaderText);
						Assert("Should have DrawbackModality column in CustomsInvoiceLinesBoundGrid", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(nameof(JobComInvoiceLine.DrawbackModality)));
						AssertEquals("DrawbackModality caption", "Modality", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[nameof(JobComInvoiceLine.DrawbackModality)].ColumnStyle.HeaderText);
						Assert("Should have JI_UsedMaterialRegime column in CustomsInvoiceLinesBoundGrid", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_UsedMaterialRegime));
						AssertEquals("JI_UsedMaterialRegime caption", "Used Mat. Reg.", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[nameof(JobComInvoiceLine.JI_UsedMaterialRegime)].ColumnStyle.HeaderText);
						Assert("Should have JI_UsedMaterialOperationType column in CustomsInvoiceLinesBoundGrid", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_UsedMaterialOperationType));
						AssertEquals("JI_UsedMaterialOperationType caption", "Operation Type", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_UsedMaterialOperationType].ColumnStyle.HeaderText);
						Assert("Should have JI_BrandName column in CustomsInvoiceLinesBoundGrid", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_BrandName));
						AssertEquals("JI_BrandName caption", "Brand", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_BrandName].ColumnStyle.HeaderText);
						Assert("Should have JI_Model column in CustomsInvoiceLinesBoundGrid", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_Model));
						AssertEquals("JI_Model caption", "Model", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_Model].ColumnStyle.HeaderText);
						Assert("Should have JI_UsedMaterialManufactureYear column in CustomsInvoiceLinesBoundGrid", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_UsedMaterialManufactureYear));
						AssertEquals("JI_UsedMaterialManufactureYear caption", "Year", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_UsedMaterialManufactureYear].ColumnStyle.HeaderText);
						Assert("Should have Agreement column must NOT be default", !invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_SecondaryPreference].IsVisible);
						Assert("Should have JI_Volume column must NOT be default", !invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_Volume));
						Assert("Should have JI_VolumeUQ column must NOT be default", !invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_VolumeUQ));
						Assert("Should have JI_Calc_OrderLineNumberAndSubLine column must NOT be default", !invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine));
						Assert("Should have ManufacturerName column", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.ManufacturerName));
					});
				}
			}
		}

		public void TestImportLabels()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

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
			jobDeclartion.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = ((ImportLicenseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.BRTariffDetailsTab;
				Assert(invoiceLineUserControl.ImportTariffDetailsUserControl.Visible);
				Assert(invoiceLineUserControl.ImportTariffDetailsUserControl.NveGridLayout.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.AdditionalDetailsTab;
				Assert(invoiceLineUserControl.AdditionalDetailsUserControl.Visible);

				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.ManufacturerDetailsTab;
				Assert(invoiceLineUserControl.ManufacturerDetailsUserControl.Visible);
			}
		}

		public void TestLineCalculation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var lineUserControl = ((ImportLicenseInvoiceLineUserControl)testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					AssertEquals("FOB Value visible", true, lineUserControl.Controls.Find("JI_Calc_FOBConvertToLocalCurrencyControl", true).FirstOrDefault().Visible);
					AssertEquals("Freight visible", true, lineUserControl.Controls.Find("JI_Calc_FreightConvertToLocalCurrencyControl", true).FirstOrDefault().Visible);
					AssertEquals("Insurance visible", true, lineUserControl.Controls.Find("JI_Calc_InsuranceConvertToLocalCurrencyControl", true).FirstOrDefault().Visible);
					AssertEquals("Customs Value visible", true, lineUserControl.Controls.Find("JI_Calc_CIFConvertToLocalCurrencyControl", true).FirstOrDefault().Visible);
					AssertEquals("Invoice Amount visible", true, lineUserControl.Controls.Find("JI_Calc_InvAmountControl", true).FirstOrDefault().Visible);
					AssertEquals("Duty Not visible", false, lineUserControl.DutyConvertToLocalCurrencyControl.Visible);
					AssertEquals("GST visible", true, lineUserControl.Controls.Find("JI_Calc_GSTConvertToLocalCurrencyControl", true).FirstOrDefault().Visible);
				}
			}
		}

		protected override string JobMessageType => BRJobMessageTypeList.Codes.ImportLicense;

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
						JobComInvoiceLine.Schema.JI_OrderNumber,
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
						JobComInvoiceLine.Schema.ManufacturerDocOrgPK,
						JobComInvoiceLine.Schema.ManufacturerDocAddressPK,
						JobComInvoiceLine.Schema.ManufacturerName,
						JobComInvoiceLine.Schema.JI_CountryOfOrigin,
						JobComInvoiceLine.Schema.NaladiHs,
						JobComInvoiceLine.Schema.DutyTaxRegime,
						JobComInvoiceLine.Schema.DutyLegalBase,
						JobComInvoiceLine.Schema.DrawbackModality,
					};
				}
				return expectedColumnNamesListOnThisOrder;
			}
		}
		List<ZString> expectedColumnNamesListOnThisOrder;
	}
}
