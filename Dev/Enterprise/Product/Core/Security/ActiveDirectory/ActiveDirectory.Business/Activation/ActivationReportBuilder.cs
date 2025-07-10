using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security.ActiveDirectory
{
	public class ActivationReportBuilder : IActivationReportBuilder
	{
		public IExcelInterface BuildReport(IEnumerable<EntitySynchronisedEventArgs> syncHistories)
		{
			var excelFile = GetReportExcelFile();
			var worksheet = excelFile.WorkSheets[0];

			var userHistories = syncHistories.Where(h => h.Entity is ADUser).OrderBy(h => h.Entity.EnterpriseIdentity).ToArray();
			var groupHistories = syncHistories.Where(h => h.Entity is ADGroup).OrderBy(h => h.Entity.EnterpriseIdentity).ToArray();

			WriteGeneratedTime(worksheet);

			var row = UserStartingRow;

			if (userHistories.Length > 0)
			{
				row = WriteHistories(worksheet, row, userHistories, UserHeadings);
				if (groupHistories.Length > 0)
				{
					row += 3;
				}
			}
			else
			{
				worksheet.RemoveRows(4, 10);
			}

			if (groupHistories.Length > 0)
			{
				WriteHistories(worksheet, row, groupHistories, GroupHeadings);
			}
			else
			{
				worksheet.RemoveRows(row, row + 6);
			}

			return excelFile;
		}

		static void WriteGeneratedTime(IExcelWorkSheet worksheet)
		{
			worksheet[3, 1] = Res.GetString("2151f69d-21a3-4011-89af-bde727c4132c", "Report generated at {0}", ZDateTime.Now);
		}

		const int UserStartingRow = 6;

		static int WriteHistories(IExcelWorkSheet worksheet, int startingRow, EntitySynchronisedEventArgs[] entityHistories, string[] headings)
		{
			var row = startingRow;
			foreach (var history in entityHistories)
			{
				if (string.IsNullOrEmpty((worksheet[row, 0] ?? string.Empty).ToString()))
				{
					worksheet.CopyAndInsertRows(worksheet, row - 3, 3, row);
					for (int colToClear = 1; colToClear <= headings.Length + 1; colToClear++)
					{
						for (int rowToClear = row; rowToClear <= row + 2; rowToClear++)
						{
							worksheet[rowToClear, colToClear] = string.Empty;
							var cellFormat = worksheet.GetCellFormat(rowToClear, colToClear);
							if (colToClear != headings.Length + 1 && rowToClear == row + 2)
							{
								ApplyDefaultFormatWithLowerBorder(cellFormat);
							}
							else
							{
								ApplyDefaultFormat(cellFormat);
							}
							worksheet.SetCellFormat(rowToClear, colToClear, cellFormat);
						}
					}
				}

				foreach (var syncEvent in history.SyncEvents)
				{
					var col = syncEvent.PropertyName != null ? 1 + headings.IndexOfFirst(s => s == syncEvent.PropertyName) : -1;
					if (col <= 0)
					{
						col = headings.Length + 1;
					}
					worksheet[row, col] = (syncEvent.ADStartingValue ?? string.Empty).ToString();
					worksheet[row + 1, col] = (syncEvent.EnterpriseStartingValue ?? string.Empty).ToString();
					worksheet[row + 2, col] = (syncEvent.SynchronisedValue ?? string.Empty).ToString();

					var acceptedRow = GetAcceptedValueRow(row, syncEvent);
					var overwrittenRows = GetOverwrittenValueRows(row, syncEvent).ToArray();

					if (acceptedRow != null)
					{
						var cellFormat = worksheet.GetCellFormat(acceptedRow.Value, col);
						ApplyFormatBasedOnSyncedStatus(syncEvent, cellFormat, ApplyAcceptedFormat, ApplyDefaultFormat);
						worksheet.SetCellFormat(acceptedRow.Value, col, cellFormat);
					}
					if (overwrittenRows.Length > 0)
					{
						foreach (var overwrittenRow in overwrittenRows)
						{
							var cellFormat = worksheet.GetCellFormat(overwrittenRow, col);
							ApplyFormatBasedOnSyncedStatus(syncEvent, cellFormat, ApplyOverwrittenFormat, ApplyDefaultFormat);
							worksheet.SetCellFormat(overwrittenRow, col, cellFormat);
						}
					}

					for (int i = row; i < row + 3; i++)
					{
						if (i != acceptedRow && !overwrittenRows.Any(r => r == i))
						{
							var cellFormat = worksheet.GetCellFormat(i, col);
							if (i == row + 2)
							{
								ApplyDefaultFormatWithLowerBorder(cellFormat);
							}
							else
							{
								ApplyDefaultFormat(cellFormat);
							}
							worksheet.SetCellFormat(i, col, cellFormat);
						}
					}

					col++;
				}

				row += 3;
			}

			return row;
		}

		static void ApplyFormatBasedOnSyncedStatus(ISyncEvent syncEvent, CellFormat cellFormat, Action<CellFormat> applyNormalFormat, Action<CellFormat> applyForcedFormat)
		{
			if (syncEvent.IsForcedToShowInReport)
			{
				applyForcedFormat(cellFormat);
			}
			else
			{
				applyNormalFormat(cellFormat);
			}
		}

		static int? GetAcceptedValueRow(int blockStartRow, ISyncEvent syncEvent)
		{
			var adValue = (syncEvent.ADStartingValue ?? string.Empty).ToString();
			var enterpriseValue = (syncEvent.EnterpriseStartingValue ?? string.Empty).ToString();
			var synchronisedValue = (syncEvent.SynchronisedValue ?? string.Empty).ToString();
			if (adValue.Equals(enterpriseValue))
			{
				return null;
			}
			else
			{
				return adValue.Equals(synchronisedValue) ? blockStartRow : enterpriseValue.Equals(synchronisedValue) ? new int?(blockStartRow + 1) : null;
			}
		}

		static IEnumerable<int> GetOverwrittenValueRows(int blockStartRow, ISyncEvent syncEvent)
		{
			var adValue = (syncEvent.ADStartingValue ?? string.Empty).ToString();
			var enterpriseValue = (syncEvent.EnterpriseStartingValue ?? string.Empty).ToString();
			var synchronisedValue = (syncEvent.SynchronisedValue ?? string.Empty).ToString();
			if (adValue.Equals(enterpriseValue) && adValue.Equals(synchronisedValue))
			{
				yield break;
			}
			else
			{
				if (!adValue.Equals(synchronisedValue))
				{
					yield return blockStartRow;
				}
				if (!enterpriseValue.Equals(synchronisedValue))
				{
					yield return blockStartRow + 1;
				}
			}
		}

		static IExcelInterface GetReportExcelFile()
		{
			return new ExcelTemplates.ExcelTemplateReadFromExcelTemplatesSolution("ADActivationReport").GetNewExcelInterface();
		}

		public static Color AcceptedFormatBackgroundColor => Color.FromArgb(180, 255, 180);
		public static Color OverwrittenFormatBackgroundColor => Color.FromArgb(255, 180, 180);
		public static Color DefaultFormatBackgroundColor => Color.FromArgb(0, 0, 0, 0);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Font name...")]
		const string FontName = "Tahoma";
		const float FontSize = 8f;

		public static void ApplyAcceptedFormat(CellFormat format)
		{
			format.FontSize = FontSize;
			format.FontName = FontName;
			format.BackgroundColor = AcceptedFormatBackgroundColor;
			format.FillPattern = FillPatternStyle.Solid;
		}

		public static void ApplyOverwrittenFormat(CellFormat format)
		{
			format.FontSize = FontSize;
			format.FontName = FontName;
			format.BackgroundColor = OverwrittenFormatBackgroundColor;
			format.FillPattern = FillPatternStyle.Solid;
			format.FontStyle = FontStyle.Strikeout;
		}

		public static void ApplyDefaultFormat(CellFormat format)
		{
			format.FontSize = FontSize;
			format.FontName = FontName;
			format.BackgroundColor = DefaultFormatBackgroundColor;
			format.FillPattern = FillPatternStyle.None;
		}

		static void ApplyDefaultFormatWithLowerBorder(CellFormat format)
		{
			ApplyDefaultFormat(format);
			format.Borders = new CellBorderFormat
			{
				Bottom = new CellBorder
				{
					BorderStyle = CellBorderStyle.Thin,
					BorderColor = Color.Black
				}
			};
		}

		static string[] UserHeadings
		{
			get
			{
				return new[]
				{
					GlbStaffSchema.Constants.GS_LoginName,
					GlbStaffSchema.Constants.GS_DomainName,
					GlbStaff.Schema.IsADLinked,
					GlbStaffSchema.Constants.GS_IsActive,
					GlbStaffSchema.Constants.GS_Title,
					GlbStaffSchema.Constants.GS_FullName,
					GlbStaffSchema.Constants.GS_EmailAddress,
					GlbStaffSchema.Constants.GS_WorkExtension,
					GlbStaffSchema.Constants.GS_WorkPhone,
					GlbStaffSchema.Constants.GS_MobilePhone,
					GlbStaffSchema.Constants.GS_FaxNum,
					GlbStaffSchema.Constants.GS_HomePhone,
					GlbStaffSchema.Constants.GS_Pager,
					GlbStaffSchema.Constants.GS_UserAddress1,
					GlbStaffSchema.Constants.GS_City,
					GlbStaffSchema.Constants.GS_State,
					GlbStaffSchema.Constants.GS_Postcode,
					GlbStaffSchema.Constants.GS_WorkingLanguage,
				};
			}
		}

		static string[] GroupHeadings
		{
			get
			{
				return new[]
				{
					GlbGroupSchema.Constants.GG_Desc,
					GlbGroupSchema.Constants.GG_DomainName,
					GlbGroup.Schema.IsADLinked,
					GlbGroupSchema.Constants.GG_IsActive
				};
			}
		}
	}
}
