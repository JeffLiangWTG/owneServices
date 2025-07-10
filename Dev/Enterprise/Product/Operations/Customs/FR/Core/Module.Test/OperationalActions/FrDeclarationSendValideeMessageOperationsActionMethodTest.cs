using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.OperationalActions.Testing
{
	[TestedType(typeof(FrDeclarationSendValideeMessageOperationalActionMethod))]
	class FrDeclarationSendValideeMessageOperationsActionMethodTest : Services.OperationalActions.Support.Testing.OperationalActionMethodTest<FrDeclarationSendValideeMessageOperationalActionMethod>
	{
		protected override FrDeclarationSendValideeMessageOperationalActionMethod NewMethod()
		{
			return new FrDeclarationSendValideeMessageOperationalActionMethod();
		}

		public void TestNameIsSendValidated()
		{
			var frDeclarationSendValideeMessageOperationalActionMethod = new FrDeclarationSendValideeMessageOperationalActionMethod();
			AssertEquals("Name should be Send validated", "Send validated", frDeclarationSendValideeMessageOperationalActionMethod.Name);
		}

		public void TestDescriptionIsSendValidated()
		{
			var frDeclarationSendValideeMessageOperationalActionMethod = new FrDeclarationSendValideeMessageOperationalActionMethod();
			AssertEquals("Description should be Send validated", "Send validated", frDeclarationSendValideeMessageOperationalActionMethod.Description);
		}
	}
}
