using System;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IZFolderBrowserDialog : IDisposable
	{
		string Description { get; set; }
		System.Environment.SpecialFolder RootFolder { get; set; }
		string SelectedPath { set; }
		string UnmappedSelectedPath { get; }
		string MappedSelectedPath { get; }
		bool ShowNewFolderButton { get; set; }
		bool CreateDirectory { get; set; }
		bool RequireMappablePath { get; set; }
		bool IsNeedingToUseEnterpriseChannel { get; }
		DialogResult ShowDialog();
		DialogResult ShowDialog(IWin32Window owner, bool canceledIfPathCannotAccess = true);
	}
}
