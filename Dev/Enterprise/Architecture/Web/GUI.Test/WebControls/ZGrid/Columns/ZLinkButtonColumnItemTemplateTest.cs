using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZLinkButtonColumnItemTemplateTest : ZItemTemplateTest
	{
		public void TestGetControl()
		{
			TestColumn.HeaderText = "Header Text";
			TestColumn.Command = "TestCommand";
			TestColumn.ClientClickHandler = "javascript: alert('Test');";

			ISelfBindingWebControl control = TestItemTemplate.GetControl();
			ZLinkButton linkButton = control as ZLinkButton;
			AssertNotNull("Control has expected type", linkButton);

			AssertEquals("Text", TestColumn.HeaderText, linkButton.Text);
			AssertEquals("CommandName", TestColumn.Command, linkButton.CommandName);
			AssertNotNull("onclick Attribute exists", linkButton.Attributes["onclick"]);
			AssertEquals("onclick Attribute", TestColumn.ClientClickHandler, linkButton.Attributes["onclick"]);
		}

		#region Implementation

		new ZLinkButtonColumn TestColumn
		{
			get { return base.TestColumn as ZLinkButtonColumn; }
		}

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZLinkButtonColumn); }
		}

		protected override Type ExpectedItemTemplateType
		{
			get { return typeof(ZLinkButtonColumnItemTemplate); }
		}

		#endregion
	}
}
