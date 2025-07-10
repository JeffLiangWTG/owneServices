using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class ImportLicenseSupplierHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestColumnLayoutContext()
		{
			using (var control = new ImportLicenseSupplierHeaderUserControl())
			{
				AssertEquals("Column layout context should have been set", BRJobMessageTypeList.Codes.ImportLicense, control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
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
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				using (var control = brokerageControl.SupplierHeaderUserControl)
				{
					var fobAmountBoundCurrencyControl = control.Controls.Find("JZ_FOBAmountBoundCurrencyControl", true).FirstOrDefault();
					AssertEquals("FOB Value", ((ConvertToLocalCurrencyControl)fobAmountBoundCurrencyControl)?.CaptionResourceString?.Caption);

					var cifAmountBoundCurrencyControl = control.Controls.Find("JZ_CIFAmountBoundCurrencyControl", true).FirstOrDefault();
					AssertEquals("Customs Value", ((ConvertToLocalCurrencyControl)cifAmountBoundCurrencyControl)?.CaptionResourceString?.Caption);
				}
			}
		}

		public void TestColumnsGridVisibilityAndAvailable()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				using (var control = brokerageControl.SupplierHeaderUserControl)
				{
					var actualColumns = control.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns.GetVisibleColumnMappingNames();
					var columns = new string[]
					{
						JobComInvoiceHeader.Schema.JZ_InvoiceNumber,
						JobComInvoiceHeader.Schema.JZ_InvoiceAmount,
						JobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency,
						JobComInvoiceHeader.Schema.JZ_Calc_BalanceString,
						JobComInvoiceHeader.Schema.JZ_PaymentDate,
						JobComInvoiceHeader.Schema.JZ_Calc_GroupInvoice,
						JobComInvoiceHeader.Schema.SupplierDocOrgPK,
						JobComInvoiceHeader.Schema.SupplierDocAddressPK,
						JobComInvoiceHeader.Schema.JZ_Remarks
					};
					AssertContainsExactElementsInAnyOrder("Columns visible should be", columns, actualColumns);

					var allcolumns = control.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns;
					CombineAssertions(() =>
					{
						AssertNull("Should NOT have JZ_PaymentAmount column in JobComInvoiceHeadersBoundGrid", allcolumns[JobComInvoiceHeader.Schema.JZ_PaymentAmount]);
						AssertNull("Should NOT have JZ_PaymentExRate column in JobComInvoiceHeadersBoundGrid", allcolumns[JobComInvoiceHeader.Schema.JZ_PaymentExRate]);
						AssertEquals("ExchangeHedgeType should be visible", false, allcolumns[JobComInvoiceHeader.Schema.ExchangeHedgeType].IsVisible);
						AssertEquals("ExchangeHedgePaymentMethod should be visible", false, allcolumns[JobComInvoiceHeader.Schema.ExchangeHedgePaymentMethod].IsVisible);
						AssertEquals("ExchangeHedgePaymentDeadline should be visible", false, allcolumns[JobComInvoiceHeader.Schema.ExchangeHedgePaymentDeadline].IsVisible);
						AssertEquals("ExchangeHedgeReason should be visible", false, allcolumns[JobComInvoiceHeader.Schema.ExchangeHedgeReason].IsVisible);
						AssertEquals("ExchangeHedgeFinancialInstitution should be visible", false, allcolumns[JobComInvoiceHeader.Schema.ExchangeHedgeFinancialInstitution].IsVisible);
					});
				}
			}
		}

		public void TestInvDetailLeftPanelRemovedProperties()
		{
			using (var control = new ImportLicenseSupplierHeaderUserControl())
			{
				AssertEquals(false, control.Controls.Find("JZ_InvoiceCurrExRateCalcEdit", true)[0].Visible);
				AssertEquals(false, control.Controls.Find("JZ_IncoTermPlaceTextBox", true)[0].Visible);
				AssertEquals(false, control.Controls.Find("JZ_InvoiceCurrLandedCostExRateCalcEdit", true)[0].Visible);
				AssertEquals(false, control.Controls.Find("NoOfPacksCalcDropEdit", true)[0].Visible);
			}
		}
	}
}
