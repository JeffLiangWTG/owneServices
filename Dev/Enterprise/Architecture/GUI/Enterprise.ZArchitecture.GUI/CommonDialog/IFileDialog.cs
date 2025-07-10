using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IFileDialog : IDisposable
	{
		bool CheckFileExists { get; set; }
		DialogResult ShowDialog(IWin32Window owner);
		string UnmappedFileName { get; }
		Stream OpenFile();
		string Filter { get; set; }

		[SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly", Justification = "Legacy code")]
		string FileName { set; }

#if DEBUG
		FileDialog Dialog { get; }
#endif
	}
}
