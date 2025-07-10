using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class DirectoryBrowserControlTest : TestCase
	{
		public void TestGetValue()
		{
			using (DirectoryBrowserControlForTest control = new DirectoryBrowserControlForTest())
			{
				ZTextBox directoryTextBox = control.GetDirectoryTextBox();
				directoryTextBox.Text = "TEST!";
				AssertEquals("GetValue()", "TEST!", control.GetValue());
			}
		}

		public void TestSetValue()
		{
			using (DirectoryBrowserControlForTest control = new DirectoryBrowserControlForTest())
			{
				ZTextBox directoryTextBox = control.GetDirectoryTextBox();
				control.SetValue("TEST!");
				AssertEquals("DirectoryTextBox.Text", "TEST!", directoryTextBox.Text);
			}
		}

		public void TestDirectoryBrowserButtonClick()
		{
			using (DirectoryBrowserControlForTest control = new DirectoryBrowserControlForTest())
			{
				ZButton directoryBrowserButton = control.GetDirectoryBrowserButton();
				directoryBrowserButton.PerformClick();
				AssertEquals("DirectoryTextBox.Text", "ThisIsADirectoryPath", control.GetDirectoryTextBox().Text);
				control.FolderBrowserTextForTest = "";
				directoryBrowserButton.PerformClick();
				AssertEquals("DirectoryTextBox.Text", "ThisIsADirectoryPath", control.GetDirectoryTextBox().Text);
			}
		}
	}
}
