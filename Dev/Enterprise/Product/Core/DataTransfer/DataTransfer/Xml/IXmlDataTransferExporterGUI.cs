using System;
using System.IO;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DataTransfer.Business
{
	public interface IXmlDataTransferExporterGUI
	{
		ZDialogResult ShowSaveFileDialog(string defaultFileName, string initialDirectory);
		IDisposable ShowProgressForm(IProgressSupporter progressSupporter, int totalCount);
		Stream OpenFile();
		string UnmappedFile { get; }
		bool IsLocalFile { get; }
	}
}
