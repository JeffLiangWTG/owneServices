using System;
using System.IO;

namespace Enterprise.DocumentEngineIntegration
{
	public interface IExcelInterface : IDisposable
	{
		void NewExcelFile(int sheetCount);
		void SaveToFile(string fileName);
		void SaveToStream(Stream stream);
		IExcelWorkSheetList WorkSheets { get; }
		void LoadExcelFile(Stream xlsStream);
		void LoadExcelFile(string fileName);
		void PreviewInXl();
		void PreviewInXl(string fileNameToSaveAs);
		void PreviewInXlWithoutDeletingFile(string fileNameToSaveAs);
		string GetCellReference(int rowIndex, int columnIndex);
		string GetExtensionForExcelFromFile();
	}
}