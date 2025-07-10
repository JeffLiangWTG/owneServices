using System;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	public abstract class OverrideReceiptPaymentDetailsFormTest : ZFormBasherTest
	{
		public void TestFormParametersAndButtonVisibility()
		{
			using (var testForm = (OverrideReceiptPaymentDetailsForm)GetFormToBashCore())
			{
				testForm.Show();

				AssertEquals("FormVerb", "", testForm.FormVerb);

				AssertFormDisplayMode(testForm);

				Type formType = (typeof(OverrideReceiptPaymentDetailsForm));
				FieldInfo closeButtonField = formType.GetField("CloseButton", BindingFlags.NonPublic | BindingFlags.Instance);
				FieldInfo continueButtonField = formType.GetField("ContinueButton", BindingFlags.NonPublic | BindingFlags.Instance);

				FieldInfo postingButtonsUserControlField = formType.GetField("PostingButtonsUserControl", BindingFlags.NonPublic | BindingFlags.Instance);

				ZButton closeButton = (ZButton)closeButtonField.GetValue(testForm);
				ZButton continueButton = (ZButton)continueButtonField.GetValue(testForm);

				ZPostingButtonsUserControl postingButtonsUserControl = (ZPostingButtonsUserControl)postingButtonsUserControlField.GetValue(testForm);

				AssertButtonVisibility(closeButton, continueButton, postingButtonsUserControl);
			}
		}

		protected virtual void AssertFormDisplayMode(OverrideReceiptPaymentDetailsForm testForm)
		{
			AssertEquals("DisplayMode", ODisplayMode.NewSaved, testForm.DisplayMode);
			ContinueWithSave saveResult = testForm.FireSaveButton();
			AssertEquals("Precondition: sorm should be correctly", ContinueWithSave.Yes, saveResult);
			AssertEquals("DisableNewAction", ODisplayMode.NewSaved, testForm.DisplayMode);
		}

		protected virtual void AssertButtonVisibility(ZButton closeButton, ZButton continueButton, ZPostingButtonsUserControl postingUserControl)
		{
			Assert(!closeButton.Visible);
			Assert(!continueButton.Visible);

			Assert(postingUserControl.Visible);
		}
	}
}
