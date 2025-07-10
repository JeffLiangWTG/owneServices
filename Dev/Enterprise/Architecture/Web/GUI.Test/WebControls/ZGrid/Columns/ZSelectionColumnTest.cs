using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZSelectionColumnTest : ZTemplateColumnTest
	{
		public void TestConstructor()
		{
			AssertNotNull("Should be assigned by constructor", TestColumn.Grid);
			AssertEquals("BIndTo should be empty", "", TestColumn.BindTo);
			AssertEquals("HeaderText should be empty", "", TestColumn.HeaderText);
			AssertEquals("SortExpression should be empty", "", TestColumn.SortExpression);
		}

		public void TestHeaderTemplate()
		{
			AssertNotNull("Should be assigned by Initialize", TestColumn.HeaderTemplate);
			AssertNotNull("Should be SelectedColumnHeaderTemplate", TestColumn.HeaderTemplate as ZSelectionColumnHeaderTemplate);
		}

		#region Overrides

		public override void TestHeader()
		{
			AssertEquals("HeaderText should be empty", "", TestColumn.HeaderText);
		}

		public override void TestBindTo()
		{
			AssertEquals("BIndTo should be empty", "", TestColumn.BindTo);
		}

		public override void TestSortExpression()
		{
			AssertEquals("SortExpression should be empty", "", TestColumn.SortExpression);
		}

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZSelectionColumn); }
		}

		new ZSelectionColumn TestColumn
		{
			get { return base.TestColumn as ZSelectionColumn; }
		}

		protected override ZTemplateColumn GetNewColumn(string header, string bindTo)
		{
			ZDataGrid grid = new ZDataGrid();
			return new ZSelectionColumn(grid);
		}

		#endregion
	}
}
