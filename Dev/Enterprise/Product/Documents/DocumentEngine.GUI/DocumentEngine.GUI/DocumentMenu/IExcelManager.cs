using System.Windows.Forms;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	interface IExcelManager
	{
		void Edit(string workingFile, int row, int column);
		event ExcelClosedEventHandler ExcelClosed;
		bool IsSupported { get; }
	}

	public delegate void ExcelClosedEventHandler(DialogResult dialogResult);
}
