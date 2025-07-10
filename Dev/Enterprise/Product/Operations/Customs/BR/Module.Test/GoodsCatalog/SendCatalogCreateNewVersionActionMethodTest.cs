using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(SendCatalogCreateNewVersionActionMethod))]
	class SendCatalogCreateNewVersionActionMethodTest : OperationalActionMethodTest<SendCatalogCreateNewVersionActionMethod>
	{
		protected override SendCatalogCreateNewVersionActionMethod NewMethod() => new SendCatalogCreateNewVersionActionMethod();

		public void TestNameAndDescription()
		{
			AssertEquals("Create New Version", Method.Name);
			AssertEquals("Send Catalog Message - Create New Version", Method.Description);
		}

		public new void TestNewApplicator()
		{
			var actionMethod = Method.NewApplicator(Factory, null);
			AssertType<SendCatalogCreateNewVersionApplicator>(actionMethod);
		}
	}
}
