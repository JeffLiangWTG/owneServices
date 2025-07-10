using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.Modules.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZCheckboxSelectionColumnTest : ZTemplateColumnTest
	{
		public void TestConstructor()
		{
			AssertEquals("BindTo should be empty", "", TestColumn.BindTo);
			AssertEquals("Some Header", TestColumn.HeaderText);
			AssertEquals("SortExpression should be empty", "", TestColumn.SortExpression);
			AssertEquals("ZCheckBoxForSelectColumn-SomeKey", TestColumn.CheckBoxID);
			AssertEquals("SomeKey", TestColumn.Key);
		}

		public void TestHeaderTemplate()
		{
			AssertNotNull("Should be assigned by Initialize", TestColumn.HeaderTemplate);
			AssertNotNull("Should be SelectedColumnHeaderTemplate", TestColumn.HeaderTemplate as ZCheckBoxForSelectColumnHeaderTemplate);
		}

		#region Overrides

		public override void TestHeader()
		{
			AssertEquals("Some Header", TestColumn.HeaderText);
		}

		public override void TestBindTo()
		{
			AssertEquals("BindTo should be empty", "", TestColumn.BindTo);
		}

		public override void TestSortExpression()
		{
			AssertEquals("SortExpression should be empty", "", TestColumn.SortExpression);
		}

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZCheckBoxForSelectColumn); }
		}

		protected new ZCheckBoxForSelectColumn TestColumn
		{
			get { return base.TestColumn as ZCheckBoxForSelectColumn; }
		}

		protected override ZTemplateColumn GetNewColumn(string header, string bindTo)
		{
			return new ZCheckBoxForSelectColumn("Some Header", "SomeKey", new ZDataGrid(), module);
		}

		DummyZFilterStripGridModuleWithISupportEDocsBulkDownload module;

		protected override void SetUp()
		{
			base.SetUp();
			module = new DummyZFilterStripGridModuleWithISupportEDocsBulkDownload(new BusinessObjectFactory(), new DummyPage());
		}

		protected override void TearDown()
		{
			if (module != null && !module.IsDisposed)
			{
				module.Dispose();
				module = null;
			}
			base.TearDown();
		}

		#endregion
	}
}
