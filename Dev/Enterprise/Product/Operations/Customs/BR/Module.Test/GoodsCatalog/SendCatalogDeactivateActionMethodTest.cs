using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(SendCatalogDeactivateActionMethod))]
	class SendCatalogDeactivateActionMethodTest : OperationalActionMethodTest<SendCatalogDeactivateActionMethod>
	{
		protected override SendCatalogDeactivateActionMethod NewMethod() => new SendCatalogDeactivateActionMethod();

		public void TestNameAndDescription()
		{
			AssertEquals("Deactivate", Method.Name);
			AssertEquals("Send Catalog Message - Deactivate", Method.Description);
		}

		public new void TestNewApplicator()
		{
			var actionMethod = Method.NewApplicator(Factory, null);
			AssertType<SendCatalogDeactivateApplicator>(actionMethod);
		}
	}
}
