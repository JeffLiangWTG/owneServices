using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI.Testing;

class PaymentMethodUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		using (var control = new ShipmentDetailsUserControl())
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}
	}

	public void TestControls()
	{
		using (var control = new PaymentMethodUserControl())
		{
			AssertType<ZDropEdit>(control.PaymentMethodDropEdit);
			AssertType<ZTextBox>(control.AccountNoTextBox);
		}
	}

	public void TestSetBindingMembers()
	{
		using (var control = new PaymentMethodUserControl())
		{
			AssertEquals("Duty payment method binding", nameof(JobDeclaration.JE_PaymentMethod), control.PaymentMethodDropEdit.BindTo);
			AssertEquals("Duty account no binding", nameof(JobDeclaration.DutyPaidByAccountNo), control.AccountNoTextBox.BindTo);

			control.SetBindingMembers(nameof(JobDeclaration.JE_VATPaidBy), nameof(JobDeclaration.VATPaidByAccountNo));
			AssertEquals("VAT payment method binding", nameof(JobDeclaration.JE_VATPaidBy), control.PaymentMethodDropEdit.BindTo);
			AssertEquals("VAT account no binding", nameof(JobDeclaration.VATPaidByAccountNo), control.AccountNoTextBox.BindTo);
		}
	}
}
