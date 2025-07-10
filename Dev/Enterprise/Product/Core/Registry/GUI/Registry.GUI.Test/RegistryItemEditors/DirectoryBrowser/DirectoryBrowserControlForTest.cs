using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class DirectoryBrowserControlForTest : DirectoryBrowserControl
	{
		public ZTextBox GetDirectoryTextBox()
		{
			return DirectoryTextBox;
		}

		public ZButton GetDirectoryBrowserButton()
		{
			return DirectoryBrowserButton;
		}

		public string FolderBrowserTextForTest = "ThisIsADirectoryPath";
		protected override string GetFolderBrowserText()
		{
			return FolderBrowserTextForTest;
		}
	}
}
