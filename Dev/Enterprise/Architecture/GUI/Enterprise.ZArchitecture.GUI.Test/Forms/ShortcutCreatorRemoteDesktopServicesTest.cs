#if !WINZOR
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ShortcutCreatorRemoteDesktopServicesTest : RemoteDesktopServicesTest
	{
		public void TestCreateDesktopShortcut()
		{
			var shortcutCreator = new ShortcutCreator();
			var shortcutFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory), SampleCaptionAsFilename + " (2)" + ".url");
			if (File.Exists(shortcutFile))
			{
				File.Delete(shortcutFile);
			}

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			shortcutCreator.CreateDesktopShortcut(SampleCaption, SampleUrl);
			AssertEquals("A shortcut shouldn't be created unless the user clicks OK", false, File.Exists(shortcutFile));

			var existingShortcutFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory), SampleCaptionAsFilename + ".url");
			File.WriteAllText(existingShortcutFile, "");

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			try
			{
				shortcutCreator.CreateDesktopShortcut(SampleCaption, SampleUrl);
				System.Threading.Thread.Sleep(1000);
				AssertEquals("A shortcut file should be created", true, File.Exists(shortcutFile));
				AssertEquals("Shortcut file content",
	@"[InternetShortcut]
URL=" + SampleUrl + @"
IconIndex=0
IconFile=" + Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ApplicationIcon.ico") + "\r\n",
File.ReadAllText(shortcutFile));
			}
			finally
			{
				File.Delete(existingShortcutFile);
				File.Delete(shortcutFile);
			}
		}

		protected override void SetUp()
		{
			ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest());
			base.SetUp();
		}

		const string SampleCaption = @"pink/fluffy<>\unicorns";
		const string SampleCaptionAsFilename = @"pink_fluffy___unicorns";
		const string SampleUrl = "https://www.youtube.com/watch?v=eWM2joNb9NE";
	}
}
#endif
