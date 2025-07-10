using System.Windows.Forms;

namespace Enterprise.ZArchitecture.Excel
{
	public interface IExcelExporterNotifications
	{
		void ShowExcelNotInstalledAndCurrentUserHasNoEmailError(string message);
		void ShowNoRecordsToExportError(string message);
		void ShowTruncatedCellsMessage(string message);
		void ShowMaxEntriesSupportedByExcelExceededError(string message);
		DialogResult ShowMaxEntriesSupportedByCurrentExcelExceededErrorAndAskForFormatChange(string message);
		void NotifyExportingLotsOfRecords(ExcelExporter exporter);
	}
}