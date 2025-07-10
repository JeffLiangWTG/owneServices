using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public abstract class ZFileDialog<DialogType> : IFileDialog, IDisposable where DialogType : FileDialog
	{
		protected ZFileDialog(DialogType fileDialog)
		{
			FileDialog = fileDialog;
		}

		protected DialogType FileDialog
		{
			get;
			private set;
		}

		#if DEBUG

		FileDialog IFileDialog.Dialog => FileDialog;

		#endif

		public bool AddExtension
		{
			get => FileDialog.AddExtension;
			set => FileDialog.AddExtension = value;
		}

		public string DefaultExt
		{
			get => FileDialog.DefaultExt;
			set => FileDialog.DefaultExt = value;
		}

		public DialogResult ShowDialog(IWin32Window owner)
		{
			var result = FileDialog.ShowDialog(owner);
			if (result == DialogResult.OK)
			{
				OnFileOk(new CancelEventArgs(false));
			}
			return result;
		}

		public DialogResult ShowDialog() => ShowDialog(null);

		public virtual string UnmappedFileName => FileDialog.FileName;

		public string FileName
		{
			set => FileDialog.FileName = value;
		}

		public bool ValidateNames
		{
			get => FileDialog.ValidateNames;
			set => FileDialog.ValidateNames = value;
		}

		public virtual bool CheckFileExists
		{
			get => FileDialog.CheckFileExists;
			set => FileDialog.CheckFileExists = value;
		}

		public bool CheckPathExists
		{
			get => FileDialog.CheckPathExists;
			set => FileDialog.CheckPathExists = value;
		}

		public bool SupportMultiDottedExtensions
		{
			get => FileDialog.SupportMultiDottedExtensions;
			set => FileDialog.SupportMultiDottedExtensions = value;
		}

		public bool ShowHelp
		{
			get => FileDialog.ShowHelp;
			set => FileDialog.ShowHelp = value;
		}

		public string InitialDirectory
		{
			get => FileDialog.InitialDirectory;
			set => FileDialog.InitialDirectory = value;
		}

		public string Filter
		{
			get => FileDialog.Filter;
			set => FileDialog.Filter = value;
		}

		public int FilterIndex
		{
			get => FileDialog.FilterIndex;
			set => FileDialog.FilterIndex = value;
		}

		public bool DereferenceLinks
		{
			get => FileDialog.DereferenceLinks;
			set => FileDialog.DereferenceLinks = value;
		}

		public bool RestoreDirectory
		{
			get => FileDialog.RestoreDirectory;
			set => FileDialog.RestoreDirectory = value;
		}

		public string Title
		{
			get => FileDialog.Title;
			set => FileDialog.Title = value;
		}

		protected void OnFileOk(CancelEventArgs eventArgs)
		{
			FileOk?.Invoke(this, eventArgs);
		}
		public event CancelEventHandler FileOk;

		public abstract Stream OpenFile();

		public void Dispose()
		{
			FileDialog.Dispose();
		}

		public static bool IsRemote => true; // Winzor files will always be remote; because the communication is always done through the WebSocket Secure protocol.
	}
}
