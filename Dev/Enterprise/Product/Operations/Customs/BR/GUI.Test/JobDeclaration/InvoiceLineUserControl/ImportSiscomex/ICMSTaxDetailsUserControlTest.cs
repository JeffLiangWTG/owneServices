using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class ICMSTaxDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestDataSourceType()
		{
			using (var control = new ICMSTaxDetailsUserControl())
			{
				AssertEquals("DataSourceType", typeof(JobComInvoiceLine), control.DataSourceType);
			}
		}

		public void TestGroupBoxCaptions()
		{
			using (var control = new ICMSTaxDetailsUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("ICMSGroupBox caption must be", "ICMS", control.ICMSGroupBox.CaptionResourceString.Caption);
				});
			}
		}

		public void TestFieldsInForm()
		{
			using (var control = new ICMSTaxDetailsUserControl())
			{
				AssertType<ZDropEdit>("ICMSLegalBaseDropEdit must be ZDropEdit", control.ICMSLegalBaseDropEdit);
				AssertType<ZDropEdit>("ICMSTaxRegimeDropEdit must be ZDropEdit", control.ICMSTaxRegimeDropEdit);
				AssertType<ZCalcEdit>("ICMSRateCalcEdit must be ZCalcEdit", control.ICMSRateCalcEdit);
				AssertType<ZCalcEdit>("ICMSBaseValueReductionPercentageCalcEdit must be ZCalcEdit", control.ICMSBaseValueReductionPercentageCalcEdit);
				AssertType<ZCalcEdit>("ICMSTotalAmountReductionPercentageCalcEdit must be ZCalcEdit", control.ICMSTotalAmountReductionPercentageCalcEdit);
				AssertType<ZDropEdit>("ICMSFormulaDropEdit must be ZDropEdit", control.ICMSFormulaDropEdit);
				AssertType<ZButton>("ICMSFormulaDropEdit must be ZButton", control.ICMSFormulaExplanationButton);
			}
		}

		public void TestICMSFormulaExplanationButton()
		{
			using (var control = new ICMSTaxDetailsUserControl())
			{
				control.ICMSFormulaExplanationButton.PerformClick();
				AssertEquals("ICMS Formula Explanation", @"Example:
Total amounts that make up the ICMS Base = BRL 12,561.41
ICMS Rate 18% and ICMS Base Reduction (%): 51.1111 %

BC ICMS Formula: BC- Reduction in BC ICMS
The amount above BRL 12,561.41 must be divided by (1-18%), that is, BRL 12,561.41 / 0.82,
which results in BRL 15,318.80, after which the base is reduced by 51.11111%, that is
BRL 15,318.80 - (15,318.80 x 51.11111%) = BRL 15,318.80 - BRL 7,829.61 = 7,489.19

BC ICMS Formula: BCR- Reduction of the rate that makes up BC ICMS and Reduction in BC ICMS
The amount above BRL 12,561.41 must be divided by (1-(0.18-(0.18*51.11111%))), that is, also
we reduced 51.11111% from the 18% Rate, to consider the applicable rate of 8.80 and formula
BRL 12,561.41/ 0.912, which results in BRL 13,773.47, after which the base is reduced by 51.11111%, that is
BRL 13,773.47 - (13,773.47 x 51.11111%) = BRL 13,773.47 - BRL 7,039.77 = 6,733.70", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestJI_ICMSBaseValueReductionPercentageInfo_ValueChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();

			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				using (var control = brokerageControl.InvoiceLinesUserControl as ImportSiscomexInvoiceLineUserControl)
				{
					control.LineDetailTabControl.SelectedTab = control.ICMSTaxDetailsTab;
					using (var icmsControl = control.ICMSTaxDetailsUserControl)
					{
						var invoiceLine = icmsControl.CurrentDataItem as JobComInvoiceLine;
						Assert("ICMSFormulaExplanationButton should be false", !icmsControl.ICMSFormulaExplanationButton.Enabled);

						invoiceLine.JI_ICMSBaseValueReductionPercentage = 10m;
						Assert("ICMSFormulaExplanationButton should be true", icmsControl.ICMSFormulaExplanationButton.Enabled);

						control.CustomsInvoiceLinesBoundGrid.ListManager.Position = 1;
						Assert("ICMSFormulaExplanationButton should be false", !icmsControl.ICMSFormulaExplanationButton.Enabled);
					}
				}
			}
		}
	}
}
