using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZFolderBrowserDialog : Component, IZFolderBrowserDialog
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1090:Don't use System.Windows.Forms dialogs", Justification = "This is the implementation of ZFolderBrowserDialog")]
		readonly FolderBrowserDialog folderBrowseDialog = new FolderBrowserDialog(); // This is the implementation of ZFolderBrowserDialog

		public string Description
		{
			get { return folderBrowseDialog.Description; }
			set { folderBrowseDialog.Description = value; }
		}

		public System.Environment.SpecialFolder RootFolder
		{
			get { return folderBrowseDialog.RootFolder; }
			set { folderBrowseDialog.RootFolder = value; }
		}

		public string SelectedPath
		{
			set { folderBrowseDialog.SelectedPath = value; }
		}

		public string UnmappedSelectedPath
		{
			get { return folderBrowseDialog.SelectedPath; }
		}

		public string MappedSelectedPath
		{
			get { return ZOpenFileDialog.GetMappedFileName(UnmappedSelectedPath); }
		}

		public bool ShowNewFolderButton
		{
			get { return folderBrowseDialog.ShowNewFolderButton; }
			set { folderBrowseDialog.ShowNewFolderButton = value; }
		}

		public bool CreateDirectory
		{
			get;
			set;
		}

		public bool RequireMappablePath
		{
			get;
			set;
		}

		public bool IsNeedingToUseEnterpriseChannel { get; private set; }

		public DialogResult ShowDialog()
		{
			return ShowDialog(null);
		}

		public DialogResult ShowDialog(IWin32Window owner, bool canceledIfPathCannotAccess = true)
		{
			using (PerformanceStatisticsCollector.Exclude())
			{
				if (ZSaveFileDialog.IsRemote)
				{
					return ShowRemoteDialog(canceledIfPathCannotAccess);
				}
				else if (DataRegistry.Instance.RemoteAppAllowEDocAccessWithoutConnectorMode == RemoteConnectingModes.ConnectorOnly)
				{
					Globals.Message.ShowError(ZTerminalService.ClientPluginApplicationNotInstalledError);
					return DialogResult.Cancel;
				}
				else
				{
					var result = new DialogResult();
					try
					{
						result = folderBrowseDialog.ShowDialog(owner);
					}
					catch (InvalidOperationException)
					{
						Globals.Message.ShowError(Res.GetString("189e70e0-dd7c-4a14-bee4-0d0f432d763d", "Unable to Show Dialog. \r\n The system has detected that you are not running Remote Desktop Connector and your administrator has disabled server file system access. \r\n We suggest you install the Remote Desktop Connector. \r\n This will allow this action you have just attempted to succeed"));
						result = DialogResult.Cancel;
					}

					if (result == DialogResult.OK && CreateDirectory && !Directory.Exists(UnmappedSelectedPath))
					{
						Directory.CreateDirectory(UnmappedSelectedPath);
					}

					return result;
				}
			}
		}

		DialogResult ShowRemoteDialog(bool canceledIfPathCannotAccess)
		{
			var result = RemoteFileDialog.ShowFolderBrowserDialog(this.folderBrowseDialog, CreateDirectory);
			if (result.dialogResult == DialogResult.OK && RequireMappablePath && ZOpenFileDialog.GetMappedFileName(result.selectedPath) == null)
			{
				if (canceledIfPathCannotAccess)
				{
					Globals.Message.ShowError(Res.GetString("e82368b0-1050-4fdc-b49f-f75e1d1796d9", "The selected path cannot be used, please select a new path."));
					result.dialogResult = DialogResult.Cancel;
				}
				else
				{
					IsNeedingToUseEnterpriseChannel = true;
					this.SelectedPath = result.selectedPath;
				}
			}
			else
			{
				this.SelectedPath = result.selectedPath;
			}
			return result.dialogResult;
		}

		public static bool IsRemoteFolderAccessible(string unmappedPath)
		{
			return Directory.Exists(new MappedClientPath().GetMappedPath(unmappedPath));
		}

		protected override void Dispose(bool isDisposing)
		{
			base.Dispose(isDisposing);
			folderBrowseDialog.Dispose();
		}

#if DEBUG
		internal FolderBrowserDialog Dialog
		{
			get { return folderBrowseDialog; }
		}
#endif
	}
}
