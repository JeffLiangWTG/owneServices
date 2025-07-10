using System;
using System.Web.UI;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public sealed class ZTextBoxBaseNonDescendingTest : WebControlTest
	{
		protected override Control GetNewControl()
		{
			return new ZDummyTextBox();
		}

		public void TestOnPreRenderNoValidationPattern()
		{
			var page1 = new ZTestPage();
			var control1 = GetNewControl() as ZDummyTextBox;
			page1.Controls.Add(control1);

			AssertEquals("PreCondition: ValidationPattern", "", control1.ValidationPatternInternal);
			AssertEquals("PreCondition: ClientScript", false, page1.ZClientScript.IsClientScriptBlockRegistered(control1.GetType(), control1.ValidationScriptKeyInternal));
			AssertEquals("PreCondition: OnBlur Attribute", null, control1.Attributes["OnBlur"]);

			control1.OnPreRenderInternal(EventArgs.Empty);
			AssertEquals("ClientScript should not be registered", false, page1.ZClientScript.IsClientScriptBlockRegistered(control1.GetType(), control1.ValidationScriptKeyInternal));
			AssertEquals("OnBlur Attribute should not have been added", null, control1.Attributes["OnBlur"]);

			control1.SetValidationPattern(@"^\d+$");
		}

		public void TestOnPreRenderValidationPatternSingleControl()
		{
			var page1 = new ZTestPage();
			var control1 = GetNewControl() as ZDummyTextBox;
			page1.Controls.Add(control1);

			control1.SetValidationPattern(@"^\d+$");
			AssertEquals("PreCondition: ValidationPattern", @"^\d+$", control1.ValidationPatternInternal);
			AssertEquals("PreCondition: ClientScript", false, page1.ZClientScript.IsClientScriptBlockRegistered(control1.GetType(), control1.ValidationScriptKeyInternal));
			AssertEquals("PreCondition: OnBlur Attribute", null, control1.Attributes["OnBlur"]);

			control1.OnPreRenderInternal(EventArgs.Empty);
			string expectedOnBlurHandler = String.Format(@"ValidateUserInput(this, '{0}', 'Invalid characters have been entered')", control1.ValidationPatternInternal);
			AssertEquals("ClientScript should now be registered", true, page1.ZClientScript.IsClientScriptBlockRegistered(control1.GetType(), control1.ValidationScriptKeyInternal));
			AssertEquals("OnBlur Attribute should have been added", expectedOnBlurHandler, control1.Attributes["OnBlur"]);
		}

		public void TestOnPreRenderValidationPatternMultipleControls()
		{
			var page1 = new ZTestPage();
			var control1 = GetNewControl() as ZDummyTextBox;
			control1.SetValidationPattern(@"^\d+$");
			page1.Controls.Add(control1);

			var control2 = GetNewControl() as ZDummyTextBox;
			control2.SetValidationPattern(@"^\d*\.\d+$");
			page1.Controls.Add(control2);

			AssertEquals("PreCondition: Control1 ValidationPattern", @"^\d+$", control1.ValidationPatternInternal);
			AssertEquals("PreCondition: Control2 ValidationPattern", @"^\d*\.\d+$", control2.ValidationPatternInternal);
			AssertEquals("PreCondition: ClientScript", false, page1.ZClientScript.IsClientScriptBlockRegistered(control1.GetType(), control1.ValidationScriptKeyInternal));
			AssertEquals("PreCondition: Control1 OnBlur Attribute", null, control1.Attributes["OnBlur"]);
			AssertEquals("PreCondition: Control2 OnBlur Attribute", null, control1.Attributes["OnBlur"]);

			control1.OnPreRenderInternal(EventArgs.Empty);
			control2.OnPreRenderInternal(EventArgs.Empty);

			var expectedOnBlurHandlerControl1 = String.Format(@"ValidateUserInput(this, '{0}', 'Invalid characters have been entered')", control1.ValidationPatternInternal);
			var expectedOnBlurHandlerControl2 = String.Format(@"ValidateUserInput(this, '{0}', 'Invalid characters have been entered')", control2.ValidationPatternInternal);

			AssertEquals("ClientScript should now be registered", true, page1.ZClientScript.IsClientScriptBlockRegistered(control1.GetType(), control1.ValidationScriptKeyInternal));
			AssertEquals("OnBlur Attribute should have been added to Control1", expectedOnBlurHandlerControl1, control1.Attributes["OnBlur"]);
			AssertEquals("OnBlur Attribute should have been added to Control2", expectedOnBlurHandlerControl2, control2.Attributes["OnBlur"]);
		}
	}
}
