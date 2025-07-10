using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class DialogResultCaptionsTest : TestCase
	{
		public void TestGetCaptionForDialogResult()
		{
			AssertEquals("&Yes", DialogResultCaptions.GetCaptionForDialogResult(DialogResult.Yes));
			AssertEquals("&No", DialogResultCaptions.GetCaptionForDialogResult(DialogResult.No));
			AssertEquals(string.Empty, DialogResultCaptions.GetCaptionForDialogResult(DialogResult.None));
		}

		public void TestGetTextForDialogResult()
		{
			AssertEquals("Yes", DialogResultCaptions.GetTextForDialogResult(DialogResult.Yes));
			AssertEquals("No", DialogResultCaptions.GetTextForDialogResult(DialogResult.No));
			AssertEquals(string.Empty, DialogResultCaptions.GetTextForDialogResult(DialogResult.None));
		}
	}
}
