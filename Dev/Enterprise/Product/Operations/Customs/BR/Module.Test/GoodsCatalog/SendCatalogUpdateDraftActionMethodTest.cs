using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(SendCatalogUpdateDraftActionMethod))]
	class SendCatalogUpdateDraftActionMethodTest : OperationalActionMethodTest<SendCatalogUpdateDraftActionMethod>
	{
		protected override SendCatalogUpdateDraftActionMethod NewMethod() => new SendCatalogUpdateDraftActionMethod();

		public void TestNameAndDescription()
		{
			AssertEquals("Update Draft", Method.Name);
			AssertEquals("Send Catalog Message - Update Draft", Method.Description);
		}

		public new void TestNewApplicator()
		{
			var actionMethod = Method.NewApplicator(Factory, null);
			AssertType<SendCatalogUpdateDraftApplicator>(actionMethod);
		}
	}
}
