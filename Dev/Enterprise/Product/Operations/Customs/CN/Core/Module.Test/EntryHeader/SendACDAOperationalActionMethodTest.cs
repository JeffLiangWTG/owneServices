using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Module.Testing
{
	[TestedType(typeof(SendACDAOperationalActionMethod))]
	class SendACDAOperationalActionMethodTest : Services.OperationalActions.Support.Testing.OperationalActionMethodTest<SendACDAOperationalActionMethod>
	{
		public void TestProperties()
		{
			var method = new SendACDAOperationalActionMethod();
			AssertEquals("method name should be Send ACDA Message", "Send ACDA Message", method.Name);
			AssertEquals("method description should be Send ACDA Message (CN)", "Send ACDA Message (CN)", method.Description);
			AssertEquals("method should have control", true, method.HasControl);
			AssertEquals("method should not have settings", false, method.HasSettings);
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
			using (SendACDAOperationActionControl control = (SendACDAOperationActionControl)method.NewGuiControl())
			{
				AssertEquals("SendWithErrorsRadioButton should be disable", false, control.SendWithErrorsRadioButton.Enabled);
			}
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
			using (SendACDAOperationActionControl control = (SendACDAOperationActionControl)method.NewGuiControl())
			{
				AssertEquals("SendWithErrorsRadioButton should be enable", true, control.SendWithErrorsRadioButton.Enabled);
			}
		}

		protected override SendACDAOperationalActionMethod NewMethod() => new SendACDAOperationalActionMethod();
	}
}
