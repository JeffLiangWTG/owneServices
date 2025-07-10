using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Enterprise.Customs.BR.Business;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Excel;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public class NFeDataExcelExporter
	{
		public NFeDataExcelExporter(NFeExportObject dataToExport, ZGrid grid, IExcelExporterNotifications notifications)
			: this(dataToExport, GetExcelExportColumnsForLines(grid), notifications)
		{
		}

		public NFeDataExcelExporter(NFeExportObject dataToExport, IEnumerable<ExcelExportColumn<NFeInvoiceLineExportObject>> lineExportColumns, IExcelExporterNotifications notifications)
		{
			this.dataToExport = dataToExport;
			this.notifications = notifications;

			GetExcelExportColumnsForLines(lineExportColumns);
		}

		readonly NFeExportObject dataToExport;
		readonly IExcelExporterNotifications notifications;

		List<ExcelExportColumnBase> excelExportColumns;
		Dictionary<string, int> indexOfProperties;

		readonly string[] totalPropertyNames = new[]
		{
			NFeInvoiceLineExportObject.Schema.FOBValue,
			NFeInvoiceLineExportObject.Schema.FreightValue,
			NFeInvoiceLineExportObject.Schema.InsuranceValue,
			NFeInvoiceLineExportObject.Schema.CIFValue,
			NFeInvoiceLineExportObject.Schema.GrossWeight,
			NFeInvoiceLineExportObject.Schema.NetWeight
		};

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Const")]
		internal const string TotalPropertyPrefix = "Total";

		void GetExcelExportColumnsForLines(IEnumerable<ExcelExportColumn<NFeInvoiceLineExportObject>> lineExportColumns)
		{
			excelExportColumns = lineExportColumns.Cast<ExcelExportColumnBase>().ToList();
			indexOfProperties = new Dictionary<string, int>();

			for (int i = 0; i < lineExportColumns.Count(); i++)
			{
				var column = lineExportColumns.ElementAt(i);
				indexOfProperties.Add(column.PropertyName, i);
			}
		}

		static IEnumerable<ExcelExportColumn<NFeInvoiceLineExportObject>> GetExcelExportColumnsForLines(ZGrid grid)
		{
			return grid.Columns.Where(x => x.IsVisible).Select(x => ExcelExportColumn<NFeInvoiceLineExportObject>.CreateFromColumn(x));
		}

		public void ExportDataToExcel(Stream outputStream)
		{
			if (dataToExport.Entries.Count > 0)
			{
				using (var excelInterface = ExcelInterfaceFactory.New())
				{
					excelInterface.NewExcelFile(dataToExport.Entries.Count);

					for (var i = 0; i < dataToExport.Entries.Count; i++)
					{
						var workSheet = excelInterface.WorkSheets[i];

						var entryToExport = dataToExport.Entries[i];
						if (entryToExport.Lines.Count > 0)
						{
							new ExcelExporter(entryToExport.Lines, excelExportColumns, notifications).ExportIntoExcel(excelInterface, workSheet);
						}

						workSheet.InsertRows(0, 7);
						SetCell(workSheet, 0, 0, true, Res.GetString("0c7b1b9b-6a33-4ee5-ae3d-469bac565d5c", "Importer:"));
						SetCell(workSheet, 0, 1, false, dataToExport.Declaration.Importer?.OH_FullName);
						SetCell(workSheet, 1, 0, true, Res.GetString("b54d33f8-24a8-4f78-8163-bbce3138828b", "Registration Number:"));
						SetCell(workSheet, 1, 1, false, dataToExport.Declaration.Importer?.GetCNPJOrCPF());
						SetCell(workSheet, 2, 0, true, Res.GetString("d217427a-9ee9-4dc8-a3b8-393495332e4f", "Job:"));
						SetCell(workSheet, 2, 1, false, dataToExport.Declaration.JobNumber);
						SetCell(workSheet, 3, 0, true, Res.GetString("200a6314-1fc0-4b03-8783-325fbd666d47", "Entry No.:"));
						SetCell(workSheet, 3, 1, false, entryToExport.EntryHeader.MovementReferenceNumber);
						SetCell(workSheet, 4, 0, true, Res.GetString("c34b081c-0cef-456f-8149-809d9a88a387", "Issue Date:"));
						SetCell(workSheet, 4, 1, false, entryToExport.EntryHeader.MovementReferenceNumberIssueDate.ToString("dd-MM-yyyy"));
						SetCell(workSheet, 5, 0, true, Res.GetString("E17A4E0B-D83D-482B-A0DD-F83106DBE4C0", "Release Date:"));
						SetCell(workSheet, 5, 1, false, entryToExport.EntryHeader.CH_EntryReleaseDate.ToString("dd-MM-yyyy"));

						var totalValueRowIndex = entryToExport.Lines.Count + 8;

						foreach (var propertyName in totalPropertyNames)
						{
							if (indexOfProperties.ContainsKey(propertyName))
							{
								SetCell(workSheet, totalValueRowIndex, indexOfProperties[propertyName], false, ExcelExportColumn<NFeEntryExportObject>.GetValueForExport(entryToExport, TotalPropertyPrefix + propertyName));
							}
						}
					}

					excelInterface.SaveToStream(outputStream);
				}
			}
		}

		void SetCell(IExcelWorkSheet workSheet, int row, int col, bool isHeadering, object content)
		{
			workSheet[row, col] = content;
			workSheet.SetCellFormat(row, col, new CellFormat() { FontStyle = isHeadering ? FontStyle.Bold : FontStyle.Regular });
		}
	}
}
