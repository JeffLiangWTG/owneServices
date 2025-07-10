using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZHyperLinkColumnTest : ZTemplateColumnTest
	{
		public void TestConstructor()
		{
			AssertEquals("TestHeader", TestColumn.HeaderText);
			AssertEquals("TestBindTo", TestColumn.BindTo);
			AssertEquals("TestBindTo", TestColumn.SortExpression);

			AssertEquals("Text", "", TestColumn.Text);
			AssertNull("ClientClickHandler", TestColumn.ClientClickHandler);

			AssertNotNull("DataTextFields", TestColumn.DataTextFields);
			AssertEquals("DataTextFields", 1, TestColumn.DataTextFields.Length);
			AssertEquals("DataTextFields", "TestBindTo", TestColumn.DataTextFields[0]);
			AssertEquals("DataTextFormatString", "{0}", TestColumn.DataTextFormatString);

			AssertEquals("ImageUrl", "", TestColumn.ImageUrl);
			AssertNotNull("DataImageUrlFields", TestColumn.DataImageUrlFields);
			AssertEquals("DataImageUrlFields", 0, TestColumn.DataImageUrlFields.Length);
			AssertEquals("DataImageUrlFormatString", "", TestColumn.DataImageUrlFormatString);

			AssertEquals("NavigateUrl", "", TestColumn.NavigateUrl);
			AssertEquals("DataNavigateUrlFormatString", "", TestColumn.DataNavigateUrlFormatString);
			AssertNotNull("DataNavigateUrlFields", TestColumn.DataNavigateUrlFields);
			AssertEquals("DataNavigateUrlFields", 1, TestColumn.DataNavigateUrlFields.Length);
			AssertEquals("DataNavigateUrlFields", "TestBindTo", TestColumn.DataNavigateUrlFields[0]);
		}

		public void TestClientClickHandler()
		{
			AssertNull("ClientClickHandler is null", TestColumn.ClientClickHandler);

			string testHandler = "javascript: alert('Test');";
			TestColumn.ClientClickHandler = testHandler;
			AssertEquals("ClientClickHandler should be as assigned", testHandler, TestColumn.ClientClickHandler);
		}

		public void TestGetEditItemTemplate()
		{
			AssertNotNull("EditItemTemplate", TestColumn.GetEditItemTemplate());
			AssertNotNull("Is ZHyperLinkColumnItemTemplate", TestColumn.GetEditItemTemplate() as ZHyperLinkColumnItemTemplate);
		}

		public void TestGetItemTemplate()
		{
			AssertNotNull("ItemTemplate", TestColumn.GetItemTemplate());
			AssertNotNull("Is ZHyperLinkColumnItemTemplate", TestColumn.GetItemTemplate() as ZHyperLinkColumnItemTemplate);
		}

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZHyperLinkColumn); }
		}

		new ZHyperLinkColumn TestColumn
		{
			get { return base.TestColumn as ZHyperLinkColumn; }
		}
	}
}
