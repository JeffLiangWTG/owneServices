using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(SendCatalogActivateActionMethod))]
	class SendCatalogActivateActionMethodTest : OperationalActionMethodTest<SendCatalogActivateActionMethod>
	{
		protected override SendCatalogActivateActionMethod NewMethod() => new SendCatalogActivateActionMethod();

		public void TestNameAndDescription()
		{
			AssertEquals("Activate", Method.Name);
			AssertEquals("Send Catalog Message - Activate", Method.Description);
		}

		public new void TestNewApplicator()
		{
			var actionMethod = Method.NewApplicator(Factory, null);
			AssertType<SendCatalogActivateApplicator>(actionMethod);
		}
	}
}
