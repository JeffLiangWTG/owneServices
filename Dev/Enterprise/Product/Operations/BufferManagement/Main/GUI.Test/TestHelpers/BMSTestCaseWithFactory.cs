using System;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI.Test
{
	public abstract class BMSGUITestCaseWithFactory : BMSTestCaseWithFactory
	{
		#region Button Strip Messages

		public static void AssertButtonStripNotificationShown<T>(MultiActionButtonDialogWrapper<T> dialog, string dialogMessage, string dialogCaption, params ButtonStripAction<T>[] shownButtons)
			where T : struct, IConvertible
		{
			AssertButtonStripNotificationShown("Dialog should have been shown with the following values", dialog, dialogMessage, dialogCaption, shownButtons);
		}

		public static void AssertButtonStripNotificationShown<T>(string testFailureMessage, MultiActionButtonDialogWrapper<T> dialog, string dialogMessage, string dialogCaption, params ButtonStripAction<T>[] shownButtons)
			where T : struct, IConvertible
		{
			CombineAssertions(testFailureMessage, () =>
			{
				AssertEquals("Message", dialogMessage, dialog.LastMessage);
				AssertEquals("Caption", dialogCaption, dialog.LastCaption);

				AssertEquals("Number of button options shown to the user", shownButtons.Length, dialog.ButtonStripActions?.Length ?? 0);

				for (var i = 0; i < shownButtons.Length; i++)
				{
					var expected = shownButtons[i];
					var actual = dialog.ButtonStripActions[i];

					AssertEquals($"Button index {i} text", expected.Text, actual.Text);
					AssertEquals($"Button index {i} tooltip", expected.ToolTip, actual.ToolTip);
					AssertEquals($"Button index {i} response", expected.Response, actual.Response);
				}
			});
		}

		#endregion
	}
}
