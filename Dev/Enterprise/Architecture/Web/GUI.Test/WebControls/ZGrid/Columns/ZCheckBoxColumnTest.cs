using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZCheckBoxColumnTest : ZTemplateColumnTest
	{
		protected override Type ExpectedColumnType
		{
			get { return typeof(ZCheckBoxColumn); }
		}

		new ZCheckBoxColumn TestColumn
		{
			get { return base.TestColumn as ZCheckBoxColumn; }
		}

		public void TestAutoPostBack()
		{
			TestColumn.AutoPostBack = true;
			AssertEquals("AutoPostBack", true, TestColumn.AutoPostBack);
			TestColumn.AutoPostBack = false;
			AssertEquals("AutoPostBack", false, TestColumn.AutoPostBack);
		}
	}
}
