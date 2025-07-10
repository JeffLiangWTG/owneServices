using System;
using System.Web.UI;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.Modules.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZCheckBoxForSelectColumnItemTemplateTest : ZItemTemplateTest
	{
		public virtual void TestInstantiateIn()
		{
			var container = new Control();
			TestItemTemplate.InstantiateIn(container);
			AssertEquals("Should be one control added", 1, container.Controls.Count);
			var checkBox = container.Controls[0] as ZCheckBoxForSelect;
			AssertNotNull("Should be a ZCheckBoxForSelect", checkBox);
			AssertEquals("ZCheckBoxForSelectColumn-SomeKey", checkBox.ID);
			AssertSelectAllStatus(checkBox);
		}

		protected virtual void AssertSelectAllStatus(ZCheckBoxForSelect checkBox)
		{
			AssertEquals("Should be not SelectAll", "Select/Deselect", checkBox.ToolTip);
		}

		#region Overrides

		public void TestGetControlDoesNotThrowException()
		{
			AssertNull(TestItemTemplate.GetControl());
		}

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZCheckBoxForSelectColumn); }
		}

		protected override Type ExpectedItemTemplateType
		{
			get { return typeof(ZCheckBoxForSelectColumnItemTemplate); }
		}

		public new ZCheckBoxForSelectColumnItemTemplate TestItemTemplate
		{
			get
			{
				var column = GetNewColumn("Some Header", "");
				return column.ItemTemplate as ZCheckBoxForSelectColumnItemTemplate;
			}
		}

		public new ZCheckBoxForSelectColumn TestColumn
		{
			get { return base.TestColumn as ZCheckBoxForSelectColumn; }
		}

		protected override ZTemplateColumn GetNewColumn(string header, string bindTo)
		{
			return new ZCheckBoxForSelectColumn(header, "SomeKey", new ZDataGrid(), module);
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
