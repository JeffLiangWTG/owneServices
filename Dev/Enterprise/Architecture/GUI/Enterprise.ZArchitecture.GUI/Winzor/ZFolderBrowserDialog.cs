using System;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZFolderBrowserDialog : IZFolderBrowserDialog
	{
		readonly FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

		public bool RequireMappablePath { get; set; }

		public string MappedSelectedPath
		{
			get => folderBrowserDialog.SelectedPath;
		}

		public string UnmappedSelectedPath
		{
			get => folderBrowserDialog.SelectedPath;
		}

		public string SelectedPath
		{
			set => folderBrowserDialog.SelectedPath = value;
		}

		public System.Environment.SpecialFolder RootFolder { get; set; }

		public bool CreateDirectory { get; set; }

		public string Description { get; set; }

		public bool ShowNewFolderButton { get; set; }

		public bool IsNeedingToUseEnterpriseChannel { get; }

		public DialogResult ShowDialog() => ShowDialog(null);

		public DialogResult ShowDialog(IWin32Window owner, bool canceledIfPathCannotAccess = true)
		{
			if (ZSaveFileDialog.IsRemote)
			{
				return folderBrowserDialog.ShowDialog(owner);
			}
			return DialogResult.Cancel;
		}

		public void Dispose()
		{
		}

		public static bool IsRemoteFolderAccessible(string unmappedPath)
		{
			throw new NotImplementedException();
		}

#if DEBUG
		internal FolderBrowserDialog Dialog
		{
			get { return folderBrowserDialog; }
		}
#endif
	}
}
