using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Winzor;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZOpenFileDialog : ZFileDialog<OpenFileDialog>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1090:DoNotUseSystemWindowsFormsDialogs", Justification = "Baseline")]
		public ZOpenFileDialog()
			: base(new OpenFileDialog())
		{
		}

		public bool Multiselect
		{
			get => FileDialog.Multiselect;
			set => FileDialog.Multiselect = value;
		}

		public bool ReadOnlyChecked
		{
			get => FileDialog.ReadOnlyChecked;
			set => FileDialog.ReadOnlyChecked = value;
		}

		public string ForceLocalFile() => UnmappedFileName;

		public static IDisposable ForceLocalFile(ref string fileName) => DisposableAction.NoAction;

		public string[] ForceLocalFiles()
		{
			if (FileDialog.BrowserFiles.Length > 0)
			{
				var (validFiles, message) = AcceptableBrowserFileValidator.ValidateFiles(FileDialog.BrowserFiles);
				if (validFiles.Length > 0)
				{
					var maximumLimitSize = SystemDataRegistry.Instance.eDocsMaximumFilesize.Value * FileCommonContent.FromMbToByte;
					FileDialog.SetBrowserFilesAndForceLocalFiles(validFiles, maximumLimitSize);
				}

				if (!string.IsNullOrEmpty(message))
				{
					Globals.Message.ShowWarning(message);
				}
			}
			return FileDialog.FileNames;
		}

		public static Stream OpenFile(string unmappedFileName) => File.OpenRead(unmappedFileName);

		public override Stream OpenFile() => OpenFile(UnmappedFileName);

		public override string UnmappedFileName
		{
			get
			{
				CheckIfLocalFilesExisted();
				return base.UnmappedFileName;
			}
		}

		public FileInfo[] SelectedFiles
		{
			get
			{
				CheckIfLocalFilesExisted();
				return FileDialog.FileNames.Select(fileName => new FileInfo(fileName)).ToArray();
			}
		}

		/// <summary>
		/// In CW1, after User pick files, OpenFileDialog will be possible access OpenFile, UnmappedFileName, SelectedFiles directly.
		/// but In Winzor, need to ForceLocalFiles before manipulate files.
		/// </summary>
		void CheckIfLocalFilesExisted()
		{
			if (string.IsNullOrEmpty(FileDialog.FileName) && FileDialog.BrowserFiles.Length > 0)
			{
				// This method shouldn't limit the file size.e.g.: Upgrades module may upload a file of more than 2GB.
				var maximumLimitSize =  5000 * FileCommonContent.FromMbToByte;
				FileDialog.ForceLocalFiles(maximumLimitSize);
			}
		}

		public class FileInfo
		{
			public FileInfo(string unmappedFileName)
			{
				this.UnmappedFileName = unmappedFileName;
			}

			public string UnmappedFileName
			{
				get;
				private set;
			}

			public Stream OpenFile()
			{
				return ZOpenFileDialog.OpenFile(UnmappedFileName);
			}

			public IStreamSource GetStreamSource()
			{
				return new FileInfoStreamSource(this);
			}
		}

		public class FileInfoStreamSource : IStreamSource
		{
			public FileInfoStreamSource(FileInfo fileInfo)
			{
				this.fileInfo = fileInfo;
			}

			readonly FileInfo fileInfo;

			public Stream GetStream()
			{
				return fileInfo.OpenFile();
			}
		}
	}
}
