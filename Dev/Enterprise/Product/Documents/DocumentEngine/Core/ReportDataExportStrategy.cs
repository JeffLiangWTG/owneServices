using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine
{
	public abstract class ReportDataExportStrategy : ICreateDeliveryInfoStrategy
	{
		abstract protected void Initialize(Report report, DocDeliveryContact deliveryContact, DeliveryInstructions deliveryInstructions);
		abstract protected void ExportColumnHeadings(string[] columns);
		abstract protected void ExportDataRow(string[] values);
		abstract protected string AttachmentType { get; }
		abstract protected Stream GenerateAttachment();
		enum ReportStatus { HasDataToExport, HasNoDataToExport, HasBeenCancelled }

		public DeliveryInfo CreateDeliveryInfo(IDeliverable deliverable, DocDeliveryContact deliveryContact, DeliveryInstructions deliveryInstructions)
		{
			var report = deliverable as Report;
			if (report != null && report.Style == Report.Styles.Report)
			{
				Initialize(report, deliveryContact, deliveryInstructions);
				ReportStatus status = ReadReportAndExportData(report, deliveryContact, deliveryInstructions);
				if (status == ReportStatus.HasBeenCancelled)
				{
					return null;
				}
				else if (report.ContainsDataRows && status == ReportStatus.HasNoDataToExport)
				{
					var message = Res.GetString("740f95ce-a5d9-42d0-abe9-9c63054ff622", @"Report '{0}' does not contain any data that can be exported to '{1}' format. This report contains data that would be suitable for exporting to XLS, PDF or TIFF.
This is because '{1}' requires a consistent format throughout and not all report templates have a suitable structure.", report.MenuItem != null ? report.MenuItem.SU_MenuName : report.Name, this.AttachmentType);

					if (Globals.CanShowDialogs)
					{
						Globals.Message.ShowInformation(message, Res.GetString("015a70c6-1a5b-4ee5-8589-f1e30f6a5ca7", "No data can be exported"));
					}
					else if (report.IsScheduledReport && report.ScheduleTaskNotifications != null)
					{
						report.ScheduleTaskNotifications.Notify(new InfoNotification(message));
					}
				}

				var deliveryInfo = report.GetDeliveryInfo(deliveryInstructions.IsDraft);
				deliveryInfo.DeliveryGroupID = deliveryInstructions.DeliveryGroups[0].PK;
				deliveryInfo.Instructions = deliveryInstructions;
				deliveryInfo.HasDataForCurrentFileFormat = (status == ReportStatus.HasDataToExport);
				deliveryInfo.SetFileContents(GenerateAttachment(), AttachmentType);
				return deliveryInfo;
			}

			return null;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		ReportStatus ReadReportAndExportData(Report report, DocDeliveryContact deliveryContact, DeliveryInstructions deliveryInstructions)
		{
			ReportStatus status = ReportStatus.HasNoDataToExport;

			using (var excelStream = new MemoryStream())
			{
				try
				{
					report.Renderer = new SingleSectionBodyDataAreaReportRenderer(report);
					report.Save(deliveryContact, deliveryInstructions.OfficialRecipient, excelStream);
				}
				finally
				{
					report.Renderer = null;
				}

				if (!report.IsGenerated)
				{
					return ReportStatus.HasBeenCancelled;
				}

				int startingColumn;

				var visibleColumnHeadings = ReadColumnHeadings(report, out startingColumn).ToArray();
				if (visibleColumnHeadings != null && visibleColumnHeadings.Any(c => !string.IsNullOrWhiteSpace(c)))
				{
					ExportColumnHeadings(visibleColumnHeadings);
				}

				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(excelStream);

					foreach (var workSheet in excelInterface.WorkSheets)
					{
						if (!workSheet.IsHidden)
						{
							for (var row = 0; row < workSheet.RowCount; row++)
							{
								var cells = new List<string>();
								var allColumnHeadingsCount = report.ColumnHeadingManager.CurrentConfiguration.Worksheets.OfType<Worksheet>()
							 .FirstOrDefault(w => Report.GetLocalizedSheetName(!w.NameOverride.IsEmpty ? w.NameOverride : w.Name) == report.SheetNames.FirstOrDefault(s => s.StrictName == workSheet.SheetName)?.EntireName)?.ColumnHeadings?.OfType<ColumnHeading>()
							 .Where(c => !c.IsSafeToRemove)?.Count() ?? 0;

								for (var column = startingColumn; (column - startingColumn < allColumnHeadingsCount) || (!visibleColumnHeadings.Any() && column < workSheet.ColumnCount); column++)
								{
									if (!workSheet.IsColumnHidden(column))
									{
										cells.Add(workSheet.GetCell(row, column).FormattedValue.Trim());
									}
								}

								if (cells.Any(c => !string.IsNullOrWhiteSpace(c)))
								{
									ExportDataRow(cells.ToArray());
									status = ReportStatus.HasDataToExport;
								}
							}
						}
					}
				}
			}

			return status;
		}

		public static IEnumerable<string> ReadColumnHeadings(Report report, out int startingColumn)
		{
			var configuration = report.ColumnHeadingManager.CurrentConfiguration;

			foreach (Worksheet workSheet in configuration.Worksheets)
			{
				if (!report.IsDisabledOptionalTemplateSheet(workSheet.Name))
				{
					var columns = ReadColumnHeadings(report, workSheet, out startingColumn);
					if (columns.Any())
					{
						return columns;
					}
				}
			}

			startingColumn = 0;
			return System.Array.Empty<string>();
		}

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "This out parameter is by design. However consider removing such if possible in future.")]
		public static IEnumerable<string> ReadColumnHeadings(Report parentReport, Worksheet worksheet, out int startingColumn)
		{
			var conditionalColumnsShouldBeHidden = new List<int>();
			foreach (int columnIndex in parentReport.Analyser.Config.HideColumnExpressions.Keys)
			{
				if (parentReport.Analyser.ShouldHideConditionalColumn(columnIndex))
				{
					conditionalColumnsShouldBeHidden.Add(columnIndex);
				}
			}

			var columnHeadings = worksheet.ColumnHeadings;
			var columns = columnHeadings.Cast<ColumnHeading>()
				.Where(c => !c.Hidden && !conditionalColumnsShouldBeHidden.Contains(c.OriginalColumnNumber)
					&& (!c.HideIfDescriptionEmpty || !string.IsNullOrEmpty(c.Description.ToString())))
				.OrderBy(c => c.CurrentPosition)
				.Select(c => c.TagName.IsEmpty ? (string)c.HeadingText : (string)c.TagName)
				.ToArray();

			startingColumn = columns.Any() ? (int)columnHeadings[0].OriginalColumnNumber : 0;
			return columns;
		}
	}
}
