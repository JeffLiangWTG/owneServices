using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business.Test
{
	class PropertyNameListTest : BMSTestCaseWithFactory
	{
		public void TestList()
		{
			var list = new PropertyNameList(typeof(ProcessHeader));

			AssertEquals(true, list.ContainsCode(ProcessHeaderSchema.Constants.FH_CompletionStatement));
			AssertEquals(true, list.ContainsCode(ProcessHeaderSchema.Constants.FH_DateAcceptability));

			AssertEquals("Description", list[ProcessHeaderSchema.Constants.FH_CompletionStatement].Description);
			AssertEquals("Date Acceptability", list[ProcessHeaderSchema.Constants.FH_DateAcceptability].Description);
		}
	}
}
