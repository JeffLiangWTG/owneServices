using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common.Testing;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public abstract class ZFileDialog<DialogType> : Component, IFileDialog where DialogType : FileDialog
	{
		protected ZFileDialog(DialogType fileDialog)
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
			this.FileDialog = fileDialog;
		}

		protected DialogType FileDialog
		{
			get;
			private set;
		}

		public bool AddExtension
		{
			get { return FileDialog.AddExtension; }
			set { FileDialog.AddExtension = value; }
		}

		public virtual bool CheckFileExists
		{
			get { return FileDialog.CheckFileExists; }
			set { FileDialog.CheckFileExists = value; }
		}

		public bool CheckPathExists
		{
			get { return FileDialog.CheckPathExists; }
			set { FileDialog.CheckPathExists = value; }
		}

		public string DefaultExt
		{
			get { return FileDialog.DefaultExt; }
			set { FileDialog.DefaultExt = value; }
		}

		public bool DereferenceLinks
		{
			get { return FileDialog.DereferenceLinks; }
			set { FileDialog.DereferenceLinks = value; }
		}

		public string Filter
		{
			get { return FileDialog.Filter; }
			set { FileDialog.Filter = value; }
		}

		public int FilterIndex
		{
			get { return FileDialog.FilterIndex; }
			set { FileDialog.FilterIndex = value; }
		}

		public string InitialDirectory
		{
			get { return FileDialog.InitialDirectory; }
			set { FileDialog.InitialDirectory = value; }
		}

		public bool RestoreDirectory
		{
			get { return FileDialog.RestoreDirectory; }
			set { FileDialog.RestoreDirectory = value; }
		}

		public bool ShowHelp
		{
			get { return FileDialog.ShowHelp; }
			set { FileDialog.ShowHelp = value; }
		}

		public bool SupportMultiDottedExtensions
		{
			get { return FileDialog.SupportMultiDottedExtensions; }
			set { FileDialog.SupportMultiDottedExtensions = value; }
		}

		public string Title
		{
			get { return FileDialog.Title; }
			set { FileDialog.Title = value; }
		}

		public bool ValidateNames
		{
			get { return FileDialog.ValidateNames; }
			set { FileDialog.ValidateNames = value; }
		}

		public event CancelEventHandler FileOk;

		protected void OnFileOk(CancelEventArgs eventArgs)
		{
			if (FileOk != null)
			{
				FileOk(this, eventArgs);
			}
		}

		public string FileName
		{
			set { FileDialog.FileName = value; }
		}

		public string UnmappedFileName
		{
			get { return FileDialog.FileName; }
		}

		internal static string GetMappedFileName(string unmappedFileName)
		{
			var mappedFile = string.Empty;
			if (IsRemote)
			{
				mappedFile = new MappedClientPath().GetMappedPath(unmappedFileName);
			}
			else if (DataRegistry.Instance.RemoteAppAllowEDocAccessWithoutConnectorMode == RemoteConnectingModes.ConnectorOnly)
			{
				Globals.Message.ShowError(ZTerminalService.ClientPluginApplicationNotInstalledError);
			}
			else
			{
				mappedFile = unmappedFileName;
			}
			return mappedFile;
		}

		public DialogResult ShowDialog()
		{
			return ShowDialog(null);
		}

		public DialogResult ShowDialog(IWin32Window owner)
		{
			var result = DialogResult.Cancel;
			if (IsRemote)
			{
				result = ShowRemoteDialog();
			}
			else if (DataRegistry.Instance.RemoteAppAllowEDocAccessWithoutConnectorMode == RemoteConnectingModes.ConnectorOnly)
			{
				Globals.Message.ShowError(ZTerminalService.ClientPluginApplicationNotInstalledError);
			}
			else
			{
				try
				{
					result = FileDialog.ShowDialog(owner);
				}
				catch (InvalidOperationException ex)
				{
					result = DialogResult.Cancel;
					Globals.Message.Show(ex.Message);
				}
				catch (ArgumentException ex)
				{
					result = DialogResult.Cancel;
					Globals.Message.Show(ex.Message);
				}
			}
			if (result == DialogResult.OK)
			{
				OnFileOk(new CancelEventArgs(false));
			}
			return result;
		}

		public static bool IsRemote
		{
			get { return ObjectFactory.Get<TerminalService>().IsWTSSession && ObjectFactory.Get<TerminalService>().IsRemoteAppSession && RemoteFileDialog.IsSupported; }
		}

		protected abstract DialogResult ShowRemoteDialog();
		public abstract Stream OpenFile();

		protected override void Dispose(bool isDisposing)
		{
			base.Dispose(isDisposing);
			FileDialog.Dispose();
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}

#if DEBUG
		internal DialogType Dialog
		{
			get { return FileDialog; }
		}

		FileDialog IFileDialog.Dialog
		{
			get { return Dialog; }
		}
#endif
	}
}
