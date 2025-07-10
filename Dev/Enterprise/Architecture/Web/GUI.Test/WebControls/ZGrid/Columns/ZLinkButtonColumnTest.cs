using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZLinkButtonColumnTest : ZTemplateColumnTest
	{
		public void TestClientClickHandler()
		{
			AssertNull("ClientClickHandler is null", TestColumn.ClientClickHandler);

			string testHandler = "javascript: alert('Test');";
			TestColumn.ClientClickHandler = testHandler;
			AssertEquals("ClientClickHandler should be as assigned", testHandler, TestColumn.ClientClickHandler);
		}

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZLinkButtonColumn); }
		}

		new ZLinkButtonColumn TestColumn
		{
			get { return base.TestColumn as ZLinkButtonColumn; }
		}
	}
}
