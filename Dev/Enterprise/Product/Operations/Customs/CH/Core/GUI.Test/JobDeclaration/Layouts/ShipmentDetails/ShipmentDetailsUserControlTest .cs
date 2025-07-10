using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.GUI.Testing;

sealed class ShipmentDetailsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		using (var control = new ShipmentDetailsUserControl())
		{
			AssertEquals("ShipmentDetailsUserControl test data source", typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}
	}

	public void TestPaymentControlBindings()
	{
		using (var control = new ShipmentDetailsUserControl())
		{
			AssertEquals("Duty payment method binding", nameof(JobDeclaration.JE_PaymentMethod), control.PaymentMethodUserControl.PaymentMethodDropEdit.BindTo);
			AssertEquals("Duty account no binding", nameof(JobDeclaration.DutyPaidByAccountNo), control.PaymentMethodUserControl.AccountNoTextBox.BindTo);
			AssertEquals("VAT payment method binding", nameof(JobDeclaration.JE_VATPaidBy), control.VatPaidByUserControl.PaymentMethodDropEdit.BindTo);
			AssertEquals("VAT account no binding", nameof(JobDeclaration.VATPaidByAccountNo), control.VatPaidByUserControl.AccountNoTextBox.BindTo);
		}
	}

	public void TestControls()
	{
		using (var control = new ShipmentDetailsUserControl())
		{
			CombineAssertions(() =>
			{
				AssertEquals("PaymentMethodUserControl.Visible", true, control.PaymentMethodUserControl.Visible);
				AssertEquals("VATPaidByUserControl.Visible", true, control.VatPaidByUserControl.Visible);
				AssertEquals("ClearanceLocationDropEdit.Visible", true, control.ClearanceLocationDropEdit.Visible);
				AssertEquals("AdditionalDecisionInfoCheckBox.Visible", true, control.AdditionalDecisionInfoCheckBox.Visible);
			});
		}
	}
}
