using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Excel;

namespace Enterprise.ZArchitecture.GUI
{
	public class ExcelExporterGuiNotifications : IExcelExporterNotifications
	{
		public ExcelExporterGuiNotifications(Form parentForm)
		{
			this.ParentForm = parentForm;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "untranslatable string")]
		public void ShowExcelNotInstalledAndCurrentUserHasNoEmailError(string message)
		{
			Globals.Message.ShowError(message, "Excel");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "untranslatable string")]
		public void ShowNoRecordsToExportError(string message)
		{
			Globals.Message.ShowError(message, "Excel");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "untranslatable string")]
		public void ShowTruncatedCellsMessage(string message)
		{
			Globals.Message.ShowWarning(message, "Excel");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "untranslatable string")]
		public void ShowMaxEntriesSupportedByExcelExceededError(string message)
		{
			Globals.Message.ShowError(message, "Excel");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "untranslatable string")]
		public DialogResult ShowMaxEntriesSupportedByCurrentExcelExceededErrorAndAskForFormatChange(string message)
		{
			return Globals.Message.Show(message, "Excel", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
		}

		public void NotifyExportingLotsOfRecords(ExcelExporter exporter)
		{
			this.Exporter = exporter;
			exporter.ExportStarting += new ExcelExporter.ExportStartingEventHandler(Exporter_ExportStarting);
			exporter.RecordExported += new ExcelExporter.RecordExportedEventHandler(Exporter_RecordExported);
			exporter.ExportFinished += new ExcelExporter.ExportFinishedEventHandler(Exporter_ExportFinished);
		}

		IProgressForm ProgressForm
		{
			get
			{
				if (progressForm == null)
				{
					progressForm = GetNewProgressForm();
					progressForm.Cancelled += new EventHandler(fProgressForm_Cancelled);
				}
				return progressForm;
			}
		}

		protected virtual IProgressForm GetNewProgressForm()
		{
			return new ProgressForm();
		}

		void Exporter_ExportStarting(int recordsToExport)
		{
			this.recordsToExport = recordsToExport;

			ProgressForm.SetStatusAndPercentComplete(Res.GetString("0a57326f-d5d8-4533-aca7-cb8db08f7cf3", "Initializing Export..."), 0);
			ProgressForm.ShowModalTo(ParentForm);
		}

		void Exporter_RecordExported(int recordNumber)
		{
			var statusText = Res.GetString("96671595-8e13-486e-b2ff-2cd8cf01e450", "Exporting {0} of {1} records.", recordNumber, recordsToExport);
			var percentComplete = recordNumber * 100 / recordsToExport;
			if (percentComplete > 100)
			{
				percentComplete = 100;
			}
			ProgressForm.SetStatusAndPercentComplete(statusText, percentComplete);
		}

		void Exporter_ExportFinished()
		{
			try
			{
				ProgressForm.SetStatusAndPercentComplete(Res.GetString("fc77728c-26d4-461e-a83b-f4e11b4364eb", "Displaying results in Excel..."), 100);
				ProgressForm.Hide();
			}
			finally
			{
				CleanUp();
			}
		}

		void fProgressForm_Cancelled(object sender, EventArgs e)
		{
			try
			{
				Exporter.CancelExport();
			}
			finally
			{
				CleanUp();
			}
		}

		void CleanUp()
		{
			ParentForm = null;

			if (progressForm != null)
			{
				progressForm.Cancelled -= new EventHandler(fProgressForm_Cancelled);
				progressForm.Dispose();
				progressForm = null;
			}

			if (Exporter != null)
			{
				Exporter.ExportStarting -= new ExcelExporter.ExportStartingEventHandler(Exporter_ExportStarting);
				Exporter.RecordExported -= new ExcelExporter.RecordExportedEventHandler(Exporter_RecordExported);
				Exporter.ExportFinished -= new ExcelExporter.ExportFinishedEventHandler(Exporter_ExportFinished);

				Exporter = null;
			}
		}

		protected Form ParentForm;
		protected ExcelExporter Exporter;
		protected IProgressForm progressForm;
		int recordsToExport;
	}
}
