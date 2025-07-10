using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZTextEditColumnTest : ZTemplateColumnTest
	{
		public void TestAutoPostBack()
		{
			var column = GetNewColumn();
			Assert(!column.AutoPostBack);

			column.AutoPostBack = true;
			Assert(column.AutoPostBack);
		}

		public void TestHtmlEncoding()
		{
			var column = GetNewColumn();
			Assert("Html Encoding", column.EnableHtmlEncoding);

			column.EnableHtmlEncoding = false;
			Assert("Property setter", !column.EnableHtmlEncoding);
		}

		public void TestCanBeEnabledByClient()
		{
			var column = GetNewColumn();
			AssertEquals(false, column.CanBeEnabledByClient);

			column.CanBeEnabledByClient = true;
			AssertEquals(true, column.CanBeEnabledByClient);
		}

		ZTextEditColumn GetNewColumn() => new ZTextEditColumn("HeaderText", "BindTo");

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZTextEditColumn); }
		}
	}
}
