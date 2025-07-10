using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZDocAddressColumnTest : ZTemplateColumnTest
	{
		public override void TestHeader()
		{
			AssertEquals("Header incorrect", "Address", TestColumn.HeaderText);
		}

		public override void TestBindTo()
		{
			AssertEquals("BindTo incorrect", "", TestColumn.BindTo);
		}

		public override void TestSortExpression()
		{
			AssertEquals("SortExpression incorrect", "", TestColumn.SortExpression);
		}

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZDocAddressColumn); }
		}

		protected override ZTemplateColumn GetNewColumn(string header, string bindTo)
		{
			ZTemplateColumn result = (ZTemplateColumn)Activator.CreateInstance(ExpectedColumnType, Array.Empty<object>());
			return result;
		}
	}
}
