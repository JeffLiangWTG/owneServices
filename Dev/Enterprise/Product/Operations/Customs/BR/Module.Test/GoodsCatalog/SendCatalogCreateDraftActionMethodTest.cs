using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(SendCatalogCreateDraftActionMethod))]
	class SendCatalogCreateDraftActionMethodTest : OperationalActionMethodTest<SendCatalogCreateDraftActionMethod>
	{
		protected override SendCatalogCreateDraftActionMethod NewMethod() => new SendCatalogCreateDraftActionMethod();

		public void TestNameAndDescription()
		{
			AssertEquals("Create Draft", Method.Name);
			AssertEquals("Send Catalog Message - Create Draft", Method.Description);
		}

		public new void TestNewApplicator()
		{
			var actionMethod = Method.NewApplicator(Factory, null);
			AssertType<SendCatalogCreateDraftApplicator>(actionMethod);
		}
	}
}
