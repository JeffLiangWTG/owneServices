using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.OperationalActions.Testing
{
	[TestedType(typeof(FrDeclarationSendPrelodgeAmendmentOperationalActionMethod))]
	class FrDeclarationSendPrelodgeAmendmentOperationalActionMethodTest : Services.OperationalActions.Support.Testing.OperationalActionMethodTest<FrDeclarationSendPrelodgeAmendmentOperationalActionMethod>
	{
		protected override FrDeclarationSendPrelodgeAmendmentOperationalActionMethod NewMethod()
		{
			return new FrDeclarationSendPrelodgeAmendmentOperationalActionMethod();
		}

		public void TestNameIsSendPrelodgeAmendment()
		{
			var actionMethod = new FrDeclarationSendPrelodgeAmendmentOperationalActionMethod();
			AssertEquals("Name should be Send pre-lodge amendment.", "Send pre-lodge amendment", actionMethod.Name);
		}

		public void TestDescriptionIsSendPrelodgeAmendment()
		{
			var actionMethod = new FrDeclarationSendPrelodgeAmendmentOperationalActionMethod();
			AssertEquals("Description should be Send pre-lodge amendment.", "Send pre-lodge amendment", actionMethod.Description);
		}
	}
}
