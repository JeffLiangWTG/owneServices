using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module.Testing
{
	public class AREnquiryFilterControlTest : EnquiryFilterControlTest
	{
		protected override EnquiryFilterControl GetTestFilterControl()
		{
			return new AREnquiryFilterControl(new ARTransactionHeaderCollection(Factory), new AREnquiryFilterBusinessObject());
		}

		public void TestBindingOfARUnpostedRevenueFields()
		{
			using (var form = new ZForm())
			using (var control = (AREnquiryFilterControl)GetTestFilterControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals("UnrecognisedWIPCalcEdit bound", true, control.UnrecognizedWIPCalcEdit.DataBindings.Count > 0);
				AssertEquals("RecognisedWIPCalcEdit bound", true, control.RecognizedWIPCalcEdit.DataBindings.Count > 0);
				AssertEquals("PostedRevenueCalcEdit bound", true, control.PostedRevenueCalcEdit.DataBindings.Count > 0);
				AssertEquals("Total WIP + Revenue bound", true, control.SumOfTotalWIPAndRevenueCalcEdit.DataBindings.Count > 0);
			}
		}

		public void TestARUnpostedRevenueFieldsVisibility()
		{
			using (var form = new ZForm())
			using (var control = (AREnquiryFilterControl)GetTestFilterControl())
			{
				form.Controls.Add(control);
				form.Show();
				Assert("UnrecognisedWIP should be visible", control.UnrecognizedWIPCalcEdit.Visible);
				Assert("RecognisedWIP should be visible", control.RecognizedWIPCalcEdit.Visible);
				Assert("PostedRevenue should be visible", control.PostedRevenueCalcEdit.Visible);
				Assert("Total WIP + Revenue should be visible", control.SumOfTotalWIPAndRevenueCalcEdit.Visible);
			}
		}

		public void TestGlobalAccountCreditStatusGroupFieldsVisibility()
		{
			using (var form = new ZForm())
			using (var control = (AREnquiryFilterControl)GetTestFilterControl())
			{
				form.Controls.Add(control);
				form.Show();
				Assert("GlobalCreditGroupTextBox should be visible", control.GlobalCreditGroupTextBox.Visible);
				Assert("GlobalCreditLimitCalcEdit should be visible", control.GlobalCreditLimitCalcEdit.Visible);
				Assert("GlobalCreditAvailableCalcEdit should be visible", control.GlobalCreditAvailableCalcEdit.Visible);
				Assert("GlobalCreditCurrencyTextBox should be visible", control.GlobalCreditCurrencyTextBox.Visible);
			}
		}

		public void TestSettlementGroupFieldsVisibility()
		{
			using (var form = new ZForm())
			using (var control = (AREnquiryFilterControl)GetTestFilterControl())
			{
				form.Controls.Add(control);
				form.Show();
				Assert("SettlementGroupTextBox should be visible", control.SettlementGroupTextBox.Visible);
				Assert("SettlementGroupTextBox should be readonly", control.SettlementGroupTextBox.ReadOnly);
				Assert("UseSettlementGroupCreditLimitCheckBox should be visible", control.UseSettlementGroupCreditLimitCheckBox.Visible);
				Assert("UseSettlementGroupCreditLimitCheckBox should be readonly", control.UseSettlementGroupCreditLimitCheckBox.ReadOnly);
				Assert("SettlementGroupInfoLabel should be visible", control.SettlementGroupInfoLabel.Visible);
			}
		}
	}
}
