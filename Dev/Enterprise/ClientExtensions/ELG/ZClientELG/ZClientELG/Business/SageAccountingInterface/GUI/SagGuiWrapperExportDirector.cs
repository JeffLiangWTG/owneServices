using System.IO;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.ELG
{
	class SagGuiWrapperExportDirector : SagAccountsExportDirector
	{
		public SagGuiWrapperExportDirector(BusinessObjectFactory factory, INotifications notificationSubscriber)
			: base(factory, notificationSubscriber)
		{
		}

		protected override void NotifySuccess()
		{
			base.NotifySuccess();
			Globals.Message.ShowInformation(Exporter.GetMessageToDisplayWhenExportIsFinished(notifications.AsString), "Financial Transaction Export");
		}

		protected override void NotifyFailure()
		{
			base.NotifyFailure();
			ZString message = Exporter.GetMessageToDisplayWhenExportIsFinished(notifications.AsString);
			message = message.Replace("\r\nBATCH ERROR: Please do not use the files exported and contact support.", ZString.Empty);
			Globals.Message.ShowInformation(message, "Financial Transaction Export");
		}

		protected override Stream OpenFile(string fileName)
			=> ZSaveFileDialog.OpenFile(fileName);

		protected override ZString GetFinalDirectory()
		{
			if (InitialFolderExists)
			{
				return ExportPathName;
			}

			if (Globals.IsUserInteractive)
			{
				return GetFolderLocationFromBrowseDialog();
			}

			return string.Empty;
		}

		bool InitialFolderExists
		{
			get
			{
				return ZOpenFileDialog.IsRemote ? ZFolderBrowserDialog.IsRemoteFolderAccessible(ExportPathName) : Directory.Exists(ExportPathName);
			}
		}

#if DEBUG
		protected virtual
#endif
		string GetFolderLocationFromBrowseDialog()
		{
			using (var folderDialog = new ZFolderBrowserDialog())
			{
				folderDialog.ShowNewFolderButton = false;
				var result = folderDialog.ShowDialog();

				if (result == DialogResult.OK)
				{
					return folderDialog.UnmappedSelectedPath;
				}
			}

			return string.Empty;
		}

		protected override bool EmailNotificationEnabled
		{
			get { return false; }
		}
	}
}
