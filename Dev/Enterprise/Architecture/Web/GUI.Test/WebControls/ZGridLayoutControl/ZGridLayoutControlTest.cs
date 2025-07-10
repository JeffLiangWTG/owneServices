using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZGridLayoutControlTest : ZTextIFramePopupTest
	{
		public void TestConstructor()
		{
			AssertNotNull("GridLayoutConttrol should be not null", GridLayoutControl);
			AssertEquals("ButtonText", "Customize Columns", GridLayoutControl.ButtonText);

			AssertEquals("Text should be empty", "", GridLayoutControl.Text);
			AssertEquals("ModuleID", WebModuleIDs.NotAssigned, GridLayoutControl.ModuleID);
		}

		public void TestTestBoxControl()
		{
			AssertNotNull("TextBoxControl should be not null", GridLayoutControl.TextBoxControl);
			Assert("AutoPostBack on TextBoxControl", GridLayoutControl.TextBoxControl.AutoPostBack);
			Assert("TextBoxControl should be hidden", GridLayoutControl.TextBoxControl.Style[HtmlTextWriterStyle.Display] == "none");
		}

		public void TestTextButtonControl()
		{
			AssertNotNull("TextButtonControl should be not null", GridLayoutControl.ButtonControlInternal);
			AssertEquals("ButtonBackgroudStyle should be an empty string", "", GridLayoutControl.ButtonBackgroundStyleInternal);
			AssertEquals("Button Style", GridLayoutControl.ButtonWidthInternal.ToString(), GridLayoutControl.ButtonControlInternal.Style["Width"]);
			AssertEquals("Button Title", "Customize Grid Columns", GridLayoutControl.ButtonControlInternal.Attributes[nameof(HtmlTextWriterAttribute.Title)]);
			AssertEquals("Button Class", "Button", GridLayoutControl.ButtonControlInternal.Attributes[nameof(HtmlTextWriterAttribute.Class)]);
			AssertEquals("Button Disabled status", !GridLayoutControl.Enabled, GridLayoutControl.ButtonControlInternal.Disabled);
		}

		public void TestDisplayStyle()
		{
			string expectedDisplayStyle = string.Format(";HEIGHT:{0};WIDTH:{1}", GridLayoutControl.PopupHeightInternal, GridLayoutControl.PopupWidthInternal);
			Assert("Expecting Height and Width defined in DisplayStyle", GridLayoutControl.DisplayStyleInternal.Contains(expectedDisplayStyle));
		}

		protected override string ExpectedIFrameSourcePage
		{
			get { return "GridLayoutPage.aspx"; }
		}

		protected override string ExpectedPathToIFrameSourcePage
		{
			get { return "/Runtime/Enterprise_ZArchitecture_Web_GUI/" + RuntimeVersion + "/ZTextBoxButton/ZTextPopup/ZTextIFramePopup/ZButtonPopup/ZGridLayoutControl/"; }
		}

		public override void TestPopupDimension()
		{
			AssertEquals("Width", Unit.Pixel(380), GridLayoutControl.PopupWidthInternal);
			AssertEquals("Height", Unit.Pixel(200), GridLayoutControl.PopupHeightInternal);
		}

		public override void TestAssignSelectedValueToInvalidZType()
		{
			ZDateTime dummyDate = new ZDateTime(2004, 12, 2);
			GridLayoutControl.SelectedValue = dummyDate;
			AssertEquals(dummyDate.ToString(), GridLayoutControl.SelectedValue);
		}

		public void TestModuleID()
		{
			GridLayoutControl.ModuleID = WebModuleIDs.Dummy;
			AssertEquals(WebModuleIDs.Dummy, GridLayoutControl.ModuleID);
		}

		public void TestGetTextFromValue()
		{
			AssertNull("Should be null", GridLayoutControl.GetTextFromValueInternal(new ZInt(100)));
			AssertNotNull("should be not null", GridLayoutControl.GetTextFromValueInternal(new ZString("Test")));
			AssertEquals("should be as expected", new ZString("Test"), GridLayoutControl.GetTextFromValueInternal(new ZString("Test")));
		}

		public void TestGetSelectedValue()
		{
			GridLayoutControl.TextBoxControl.Text = "TEST";
			AssertEquals("Expected Selected value", new ZString("TEST"), GridLayoutControl.GetSelectedValueInternal());
		}

		protected override NameValueCollection ExpectedAdditionalParameters
		{
			get
			{
				NameValueCollection result = base.ExpectedAdditionalParameters;
				result.Add(ZGridLayoutControl.GridModuleIDKey, GridLayoutControl.ModuleID.ToString());
				result.Add(ZGridLayoutControl.GridCurrentLayoutKey, GridLayoutControl.Text);
				return result;
			}
		}

		public void TestIsBindableReturnFalse()
		{
			AssertEquals("SHould return false", false, GridLayoutControl.IsBindable(null));

			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			GridLayoutControl.BindTo = DummyBusinessObject.Schema.Z0_Description;
			AssertEquals("SHould return false", false, GridLayoutControl.IsBindable(bizO));
		}

		public void TestHookUnhookTextChangedHandler()
		{
			GridLayoutControl.HookTextChangedEvent(DummyEventHandler);
			AssertEquals("Precondition - Event not fired", false, EventFired);

			typeof(ZTextBox).InvokeMember("OnTextChanged", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, GridLayoutControl.TextBoxControl, new object[] { EventArgs.Empty });
			AssertEquals("Event should be fired", true, EventFired);

			GridLayoutControl.UnhookTextChangedEvent(DummyEventHandler);
			EventFired = false;

			typeof(ZTextBox).InvokeMember("OnTextChanged", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, GridLayoutControl.TextBoxControl, new object[] { EventArgs.Empty });
			AssertEquals("Event should be not fired", false, EventFired);
		}

		protected override Control GetNewControl()
		{
			return new ZGridLayoutControl();
		}

		protected ZGridLayoutControl GridLayoutControl
		{
			get { return (ZGridLayoutControl)Control; }
		}

		protected void DummyEventHandler(object sender, EventArgs e)
		{
			EventFired = true;
		}

		protected bool EventFired;
	}
}
