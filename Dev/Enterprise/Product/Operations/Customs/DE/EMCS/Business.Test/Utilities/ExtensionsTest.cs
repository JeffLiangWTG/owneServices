using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	class ExtensionsTest : TestCaseWithFactory
	{
		public void TestIsConsolidatedDocument()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			CombineAssertions(() =>
			{
				AssertEquals("Null parameter test", false, Extensions.IsConsolidatedDocument(null));
				declaration.ZG_DeferredSubmission = EmcsDeferredSubmissionList.Codes.Nein;
				AssertEquals("DeferredSubmission = 0 => No ConsolidatedDocument", false, declaration.IsConsolidatedDocument());
				declaration.ZG_DeferredSubmission = EmcsDeferredSubmissionList.Codes.Ja;
				AssertEquals("DeferredSubmission = 1 => No ConsolidatedDocument", false, declaration.IsConsolidatedDocument());
				declaration.SetConsolidatedDocument();
				AssertEquals("DeferredSubmission = 2 => Is ConsolidatedDocument", true, declaration.IsConsolidatedDocument());
			});
		}

		public void TestEncodeToHTMLFormat()
		{
			ZString originalText = "<Node>Test text</Node>";
			AssertContains("Should be encoded to html format", "&lt;Node&gt;Test text&lt;/Node&gt;", originalText.EncodeToHTMLFormat());
		}
	}
}
