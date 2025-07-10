using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZHyperLinkColumnItemTemplateTest : ZItemTemplateTest
	{
		public void TestGetControl()
		{
			TestColumn.Text = "Text";
			TestColumn.ImageUrl = "Image.gif";
			TestColumn.NavigateUrl = "NavigateUrl";
			TestColumn.DataTextFields = new string[] { "DataField" };
			TestColumn.DataTextFormatString = "DataTextFormat";
			TestColumn.DataNavigateUrlFields = new string[] { "DataNavigateField" };
			TestColumn.DataImageUrlFields = new string[] { "DataImageField" };
			TestColumn.DataNavigateUrlFormatString = "DataNavigateFormat";
			TestColumn.DataImageUrlFormatString = "DataImageFormat";
			TestColumn.Target = "Target";
			TestColumn.WindowStyle = "Style";

			ISelfBindingWebControl control = TestItemTemplate.GetControl();
			ZHyperlink link = control as ZHyperlink;
			AssertNotNull("Control has expected type", link);

			AssertEquals("BindTo", TestColumn.BindTo, link.BindTo);
			AssertEquals("Text", TestColumn.Text, link.Text);
			AssertEquals("ImageUrl", TestColumn.ImageUrl, link.ImageUrl);
			AssertEquals("NavigateUrl", TestColumn.NavigateUrl, link.NavigateUrl);
			AssertEquals("DataTextFields", TestColumn.DataTextFields, link.DataTextFields);
			AssertEquals("DataTextFormatString", TestColumn.DataTextFormatString, link.DataTextFormatString);
			AssertEquals("DataNavigateUrlFields", TestColumn.DataNavigateUrlFields, link.DataNavigateUrlFields);
			AssertEquals("DataNavigateUrlFormatString", TestColumn.DataNavigateUrlFormatString, link.DataNavigateUrlFormatString);
			AssertEquals("DataImageUrlFields", TestColumn.DataImageUrlFields, link.DataImageUrlFields);
			AssertEquals("DataImageUrlFormatString", TestColumn.DataImageUrlFormatString, link.DataImageUrlFormatString);
			AssertEquals("Target", TestColumn.Target, link.Target);
			AssertEquals("WindowStyle", TestColumn.WindowStyle, link.WindowStyle);

			AssertNull("onclick Attribute is not set", link.Attributes["onclick"]);

			TestColumn.ClientClickHandler = "javascript: alert('Test');";

			ISelfBindingWebControl control1 = TestItemTemplate.GetControl();
			ZHyperlink link1 = control1 as ZHyperlink;
			AssertNotNull("onclick Attribute exists", link1.Attributes["onclick"]);
			AssertEquals("onclick Attribute", TestColumn.ClientClickHandler, link1.Attributes["onclick"]);
		}

		public void TestIsExternalHyperLink()
		{
			AssertEquals("Column.IsExternalHyperlink (Default Value)", false, TestColumn.IsExternalHyperlink);

			ZHyperlink link = TestItemTemplate.GetControl() as ZHyperlink;
			AssertNotNull("Control has expected type", link);
			AssertEquals("link.IsExternalHyperLink", false, link.IsExternalHyperLink);

			TestColumn.IsExternalHyperlink = true;
			link = TestItemTemplate.GetControl() as ZHyperlink;
			AssertNotNull("Control has expected type", link);
			AssertEquals("link.IsExternalHyperLink", true, link.IsExternalHyperLink);
		}

		#region Implementation

		new ZHyperLinkColumn TestColumn
		{
			get { return base.TestColumn as ZHyperLinkColumn; }
		}

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZHyperLinkColumn); }
		}

		protected override Type ExpectedItemTemplateType
		{
			get { return typeof(ZHyperLinkColumnItemTemplate); }
		}

		#endregion
	}
}
