using System.IO;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Integration.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZSaveFileDialog : ZFileDialog<SaveFileDialog>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1090:Don't use System.Windows.Forms dialogs", Justification = "This is the implementation of ZSaveFileDialog")]
		public ZSaveFileDialog()
			: base(new SaveFileDialog()) // This is the implementation of ZSaveFileDialog
		{
		}

		public bool CreatePrompt
		{
			get { return FileDialog.CreatePrompt; }
			set { FileDialog.CreatePrompt = value; }
		}

		public bool OverwritePrompt
		{
			get { return FileDialog.OverwritePrompt; }
			set { FileDialog.OverwritePrompt = value; }
		}

		public override Stream OpenFile()
		{
			return OpenFile(this.UnmappedFileName);
		}

		public static Stream OpenFile(string unmappedFileName)
		{
			try
			{
				if (IsRemote)
				{
					return new RemoteFileSaveStream(unmappedFileName, ObjectFactory.Get<IMappedClientPath>());
				}
				else if (DataRegistry.Instance.RemoteAppAllowEDocAccessWithoutConnectorMode == RemoteConnectingModes.ConnectorOnly && Globals.IsUserInteractive)
				{
					Globals.Message.ShowError(ZTerminalService.ClientPluginApplicationNotInstalledError);
					return Stream.Null;
				}
				else
				{
					return File.Open(unmappedFileName, FileMode.Create, FileAccess.ReadWrite);
				}
			}
			catch (System.UnauthorizedAccessException)
			{
				Globals.Message.ShowError(Res.GetString("C917BE46-F8DF-44FE-8A79-3BEB08A3EB13", "Cannot write the file to disk. Please check with your system administrator."));
				return Stream.Null;
			}
			catch (System.NotSupportedException nsEx)
			{
				if (nsEx.Message.Contains("com1:") || nsEx.Message.Contains("lpt1:")) // Exception message data.
				{
					Globals.Message.ShowError(Res.GetString("0C53583B-77B7-4ECF-9466-3C0427FE7166", "Cannot write the file. The specified path is not supported."));
				}
				return Stream.Null;
			}
		}

		protected override DialogResult ShowRemoteDialog()
		{
			var result = RemoteFileDialog.ShowSaveFileDialog(this.FileDialog);
			this.FilterIndex = result.filterIndex;
			this.FileName = result.fileName;
			return result.dialogResult;
		}
	}
}
