using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.Forms
{
	/// <summary>
	/// Displays a prompt in a dialog box, waits for the user to input text or click a button, and then returns a string containing the contents of the text box.
	/// </summary>
	public static class InputBox
	{
		/// <summary>
		/// Displays a prompt in a dialog box, waits for the user to input text or click a button, and then returns a string containing the contents of the text box.
		/// </summary>
		/// <param name="prompt">String expression displayed as the message in the dialog box.</param>
		/// <param name="title">String expression displayed in the title bar of the dialog box.</param>
		/// <returns>The value in the textbox is returned if the user clicks OK or presses the ENTER key. If the user clicks Cancel, a zero-length string is returned.</returns>
		public static string Show(string prompt, string title)
		{
			return Show(prompt, title, "", -1, -1, false);
		}

		public static string Show(string prompt, string title, bool allowEmptyResult)
		{
			return Show(prompt, title, "", -1, -1, allowEmptyResult);
		}

		/// <summary>
		/// Displays a prompt in a dialog box, waits for the user to input text or click a button, and then returns a string containing the contents of the text box.
		/// </summary>
		/// <param name="prompt">String expression displayed as the message in the dialog box.</param>
		/// <param name="title">String expression displayed in the title bar of the dialog box.</param>
		/// <param name="defaultResponse">String expression displayed in the text box as the default response if no other input is provided. If you omit DefaultResponse, the displayed text box is empty.</param>
		/// <returns>The value in the textbox is returned if the user clicks OK or presses the ENTER key. If the user clicks Cancel, a zero-length string is returned.</returns>
		public static string Show(string prompt, string title, string defaultResponse, bool allowEmptyResult)
		{
			return Show(prompt, title, defaultResponse, -1, -1, allowEmptyResult);
		}

		/// <summary>
		/// Displays a prompt in a dialog box, waits for the user to input text or click a button, and then returns a string containing the contents of the text box.
		/// </summary>
		/// <param name="prompt">String expression displayed as the message in the dialog box.</param>
		/// <param name="title">String expression displayed in the title bar of the dialog box.</param>
		/// <param name="defaultResponse">String expression displayed in the text box as the default response if no other input is provided. If you omit DefaultResponse, the displayed text box is empty.</param>
		/// <param name="xPos">Integer expression that specifies, in pixels, the distance of the left edge of the dialog box from the left edge of the screen.</param>
		/// <param name="yPos">Integer expression that specifies, in pixels, the distance of the upper edge of the dialog box from the top of the screen.</param>
		/// <returns>The value in the textbox is returned if the user clicks OK or presses the ENTER key. If the user clicks Cancel, a zero-length string is returned.</returns>
		public static string Show(string prompt, string title, string defaultResponse, int xPos, int yPos, bool allowEmptyResult)
		{
			// Create a new input box dialog
			using (var inputBoxForm = new InputBoxForm())
			{
				inputBoxForm.AllowBlankString = allowEmptyResult;
				inputBoxForm.Title = title;
				inputBoxForm.Prompt = prompt;
				inputBoxForm.DefaultResponse = defaultResponse;
				if (xPos >= 0 && yPos >= 0)
				{
					inputBoxForm.StartLocation = ControlDpiScalingHelper.NewScaledPoint(xPos, yPos);
				}
				else
				{
					inputBoxForm.StartPosition = FormStartPosition.CenterParent;
				}
				ZFormModaliser.ShowDialogWithoutDispose(inputBoxForm);
				return inputBoxForm.ReturnValue;
			}
		}
	}
}
