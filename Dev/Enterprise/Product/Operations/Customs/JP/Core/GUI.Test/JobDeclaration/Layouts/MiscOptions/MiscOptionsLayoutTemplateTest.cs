using Enterprise.ZArchitecture.GUI.UserControls.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	sealed class MiscOptionsLayoutTemplateTest : TestCase
	{
		public void TestNACCSCredentialGuidDropEdit()
		{
			using var control = new MiscOptionsLayoutTemplate();
			TestHelper.AssertControlExists(control, "NACCSCredentialGuidDropEdit", "JE_NACCSCredential");
		}

		public void TestPaymentOptionsControls()
		{
			using var control = new MiscOptionsLayoutTemplate();
			TestHelper.AssertControlExists(control, "PaymentPartyDropEdit", "JE_PaymentMethod");
			TestHelper.AssertControlExists(control, "PaymentDeadlineExtensionDropEdit", "JE_PaymentDeadlineExtension");
			TestHelper.AssertControlExists(control, "PaymentOptionsSeparatorUserControl", string.Empty);
		}
	}
}
