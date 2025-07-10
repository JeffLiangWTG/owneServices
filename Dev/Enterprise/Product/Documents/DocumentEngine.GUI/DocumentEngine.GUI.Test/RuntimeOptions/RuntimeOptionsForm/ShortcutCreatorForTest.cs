using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	internal class ShortcutCreatorForTest : ShortcutCreator
	{
		public string LastCreatedHyperlinkCaption;
		public string LastCreatedHyperlinkURL;
		public override void CopyHyperlinkToClipboard(string caption, string url)
		{
			LastCreatedHyperlinkCaption = caption;
			LastCreatedHyperlinkURL = url;
		}

		public string LastCreatedDesktopShortcutCaption;
		public string LastCreatedDesktopShortcutURL;
		public override void CreateDesktopShortcut(string caption, string url)
		{
			LastCreatedDesktopShortcutCaption = caption;
			LastCreatedDesktopShortcutURL = url;
		}
	}
}
