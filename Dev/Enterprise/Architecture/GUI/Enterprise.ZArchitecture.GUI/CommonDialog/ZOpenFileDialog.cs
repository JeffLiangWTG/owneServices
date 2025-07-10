using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZOpenFileDialog : ZFileDialog<OpenFileDialog>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1090:Don't use System.Windows.Forms dialogs", Justification = "This is the implementation of ZOpenFileDialog")]
		public ZOpenFileDialog()
			: base(new OpenFileDialog()) // This is the implementation of ZOpenFileDialog
		{
		}

		public bool Multiselect
		{
			get { return FileDialog.Multiselect; }
			set { FileDialog.Multiselect = value; }
		}

		public bool ReadOnlyChecked
		{
			get { return FileDialog.ReadOnlyChecked; }
			set { FileDialog.ReadOnlyChecked = value; }
		}

		public override Stream OpenFile()
		{
			return OpenFile(this.UnmappedFileName);
		}

		public IStreamSource GetStreamSource()
		{
			return new FileInfoStreamSource(new FileInfo(UnmappedFileName));
		}

		public static Stream OpenFile(string unmappedFileName)
		{
			if (IsRemote)
			{
				var mappedFile = new MappedClientPath().GetMappedPath(unmappedFileName);
				if (mappedFile != null)
				{
					return File.OpenRead(mappedFile);
				}
				else
				{
					return new MemoryStream(RemoteFileDialog.OpenFile(unmappedFileName));
				}
			}
			else if (DataRegistry.Instance.RemoteAppAllowEDocAccessWithoutConnectorMode == RemoteConnectingModes.ConnectorOnly)
			{
				Globals.Message.ShowError(ZTerminalService.ClientPluginApplicationNotInstalledError);
				return Stream.Null;
			}
			else
			{
				return File.OpenRead(unmappedFileName);
			}
		}

		public string ForceLocalFile()
		{
			var fileName = UnmappedFileName;
			AddDisposable(ForceLocalFile(ref fileName));
			return fileName;
		}

		public string[] ForceLocalFiles()
		{
			var fileNames = Array.ConvertAll(SelectedFiles, fileInfo => fileInfo.UnmappedFileName);
			for (var i = 0; i < fileNames.Length; i++)
			{
				AddDisposable(ForceLocalFile(ref fileNames[i]));
			}
			return fileNames;
		}

		public static IDisposable ForceLocalFile(ref string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
			{
				throw new ArgumentException("Value cannot be null or empty.", nameof(fileName));
			}

			if (IsRemote)
			{
				var mappedFile = new MappedClientPath().GetMappedPath(fileName);
				if (mappedFile != null)
				{
					fileName = mappedFile;
					return DisposableAction.NoAction;
				}
				else
				{
					var tempFile = Path.Combine(Temp.TempPath, Path.GetFileName(fileName));
					for (var i = 0; File.Exists(tempFile); i++)
					{
						tempFile = Path.Combine(Temp.TempPath, Path.GetFileNameWithoutExtension(fileName) + "[" + i + "]" + Path.GetExtension(fileName));
					}
					File.WriteAllBytes(tempFile, RemoteFileDialog.OpenFile(fileName));
					fileName = tempFile;
					return new DisposableAction(delegate
						{
							try
							{
								if (File.Exists(tempFile))
								{
									File.Delete(tempFile);
								}
							}
							catch (ArgumentException) { }
							catch (IOException) { }
							catch (NotSupportedException) { }
							catch (UnauthorizedAccessException) { }
						});
				}
			}
			else if (DataRegistry.Instance.RemoteAppAllowEDocAccessWithoutConnectorMode == RemoteConnectingModes.ConnectorOnly)
			{
				Globals.Message.ShowError(ZTerminalService.ClientPluginApplicationNotInstalledError);
			}
			return DisposableAction.NoAction;
		}

		public FileInfo[] SelectedFiles
		{
			get
			{
				if (IsRemote)
				{
					return remoteSelectedFiles;
				}
				else if (DataRegistry.Instance.RemoteAppAllowEDocAccessWithoutConnectorMode == RemoteConnectingModes.ConnectorOnly)
				{
					Globals.Message.ShowError(ZTerminalService.ClientPluginApplicationNotInstalledError);
					return Array.Empty<FileInfo>();
				}
				else
				{
					return Array.ConvertAll(FileDialog.FileNames, fileName => new FileInfo(fileName));
				}
			}
		}
		FileInfo[] remoteSelectedFiles;

		protected override DialogResult ShowRemoteDialog()
		{
			var result = RemoteFileDialog.ShowOpenFileDialog(FileDialog);
			this.FilterIndex = result.filterIndex;
			this.FileName = result.fileName;
			remoteSelectedFiles = Array.ConvertAll(result.fileNames, fileName => new FileInfo(fileName));
			return result.dialogResult;
		}

		void AddDisposable(IDisposable disposable)
		{
			if (disposables == null)
			{
				disposables = new List<IDisposable>();
			}
			disposables.Add(disposable);
		}
		List<IDisposable> disposables;

		protected override void Dispose(bool isDisposing)
		{
			if (disposables != null)
			{
				foreach (var disposable in disposables)
				{
					disposable.Dispose();
				}
			}
			base.Dispose(isDisposing);
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
