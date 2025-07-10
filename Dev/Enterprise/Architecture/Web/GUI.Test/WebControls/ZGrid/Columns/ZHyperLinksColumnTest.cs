using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZHyperLinksColumnTest : ZTemplateColumnTest
	{
		public void TestConstructor()
		{
			AssertEquals("TestHeader", TestColumn.HeaderText);
			AssertEquals("TestBindTo", TestColumn.BindTo);
			AssertEquals("TestBindToField", TestColumn.BindToField);
		}

		public void TestGetItemTemplate()
		{
			AssertNotNull("ItemTemplate", TestColumn.GetItemTemplate());
			AssertNotNull("ItemTemplate is ZHyperLinksColumnItemTemplate", TestColumn.GetItemTemplate() as ZHyperLinksColumnItemTemplate);

			AssertNotNull("EditItemTemplate", TestColumn.GetEditItemTemplate());
			AssertNotNull("EditItemTemplate is ZHyperLinksColumnItemTemplate", TestColumn.GetEditItemTemplate() as ZHyperLinksColumnItemTemplate);
		}

		#region Overriden Methods

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZHyperLinksColumn); }
		}

		new ZHyperLinksColumn TestColumn
		{
			get { return base.TestColumn as ZHyperLinksColumn; }
		}

		protected override ZTemplateColumn GetNewColumn(string header, string bindTo)
		{
			ZTemplateColumn result = (ZTemplateColumn)Activator.CreateInstance(ExpectedColumnType, new object[] { header, bindTo, "TestBindToField" });
			return result;
		}

		#endregion
	}
}
