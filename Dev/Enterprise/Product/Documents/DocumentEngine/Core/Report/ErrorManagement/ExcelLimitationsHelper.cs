using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.ReportErrorManagement
{
	public static class ExcelLimitationsHelper
	{
		public enum LimitationType
		{
			Row,
			Column,
			Formula
		}

		public static class Messages
		{
			#region Show Messages

			public static void ShowTooMany(ExcelLimitationBaseException exception)
			{
				Globals.Message.Show(GetTooMany(exception.LimitationType));
			}

			public static void ShowTooManyWithReportName(ExcelLimitationBaseException exception, string reportName)
			{
				Globals.Message.Show(GetTooManyWithReportName(exception.LimitationType, reportName));
			}

			public static void ShowTooManyForExcel2003WithReportName(ExcelLimitationBaseException exception, string reportName)
			{
				Globals.Message.Show(GetTooManyForExcel2003WithReportName(exception.LimitationType, reportName));
			}

			public static ZDialogResult ShowTooManyForExcel2003WithFormatSwitchQuestion(ExcelLimitationBaseException exception)
			{
				return Globals.Message.Show(GetTooManyForExcel2003WithFormatSwitchQuestion(exception.LimitationType), string.Empty, ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question);
			}

			public static ZDialogResult ShowTooManyForExcel2003WithFormatSwitchQuestion(LimitationType limitationType)
			{
				return Globals.Message.Show(GetTooManyForExcel2003WithFormatSwitchQuestion(limitationType), string.Empty, ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question);
			}

			#endregion

			#region Get Messages

			public static MultilingualString GetTooMany(ExcelLimitationBaseException exception)
			{
				return GetTooMany(exception.LimitationType);
			}

			public static MultilingualString GetTooManyWithReportName(ExcelLimitationBaseException exception, string reportName)
			{
				return GetTooManyWithReportName(exception.LimitationType, reportName);
			}

			public static MultilingualString GetTooManyForExcel2003WithReportName(ExcelLimitationBaseException exception, string reportName)
			{
				return GetTooManyForExcel2003WithReportName(exception.LimitationType, reportName);
			}

			public static MultilingualString GetTooManyForExcel2003WithFormatSwitchQuestion(ExcelLimitationBaseException exception)
			{
				return GetTooManyForExcel2003WithFormatSwitchQuestion(exception.LimitationType);
			}

			static MultilingualString GetTooMany(LimitationType limitationType)
			{
				if (limitationType == LimitationType.Row)
				{
					return RowMessages.TooMany;
				}
				else if (limitationType == LimitationType.Formula)
				{
					return FormulaMessages.TooLong;
				}
				else
				{
					return ColumnMessages.TooMany;
				}
			}

			static MultilingualString GetTooManyWithReportName(LimitationType limitationType, string reportName)
			{
				if (limitationType == LimitationType.Row)
				{
					return RowMessages.GetTooManyWithReportName(reportName);
				}
				else if (limitationType == LimitationType.Formula)
				{
					return FormulaMessages.GetTooLongWithReportName(reportName);
				}
				else
				{
					return ColumnMessages.GetTooManyWithReportName(reportName);
				}
			}

			static MultilingualString GetTooManyForExcel2003WithReportName(LimitationType limitationType, string reportName)
			{
				if (limitationType == LimitationType.Row)
				{
					return RowMessages.GetTooManyForExcel2003WithReportName(reportName);
				}
				else if (limitationType == LimitationType.Formula)
				{
					return FormulaMessages.GetTooLongForExcel2003WithReportName(reportName);
				}
				else
				{
					return ColumnMessages.GetTooManyForExcel2003WithReportName(reportName);
				}
			}

			static MultilingualString GetTooManyForExcel2003WithFormatSwitchQuestion(LimitationType limitationType)
			{
				if (limitationType == LimitationType.Row)
				{
					return RowMessages.TooManyForExcel2003WithFormatSwitchQuestion;
				}
				else if (limitationType == LimitationType.Formula)
				{
					return FormulaMessages.TooLongForExcel2003WithFormatSwitchQuestion;
				}
				else
				{
					return ColumnMessages.TooManyForExcel2003WithFormatSwitchQuestion;
				}
			}

			#endregion
		}

		static class RowMessages
		{
			public static MultilingualString TooMany => ResString.GetMultilingualString("0e5ef9f5-535e-493d-9e62-46164d6eed9d",
@"Because of the large number of lines (or ""rows"") this report is returning from the database, you have exceeded the Microsoft imposed limit (1,048,575 rows) on this Excel 2007 based report.
Try filtering or narrowing the size of the report so that it is below the Excel 2007 limit of 1,048,575 rows.");

			public static MultilingualString TooManyForExcel2003WithFormatSwitchQuestion => ResString.GetMultilingualString("d2d842ec-b550-48fb-adb6-80c947dac1f0",
@"Because of the large number of lines (or ""rows"") this report is returning from the database, you have exceeded the Microsoft imposed limit (65,535 rows) on this Excel 97-2003 based report.
You can generate it using Excel 2007 XLSX format, which will raise the rows limit to 1,048,575.
Otherwise, try filtering or narrowing the size of the report so that it is below the Excel 97-2003 limit of 65,535 rows.

Do you want to generate this report with Excel 2007 XLSX format?");

			public static MultilingualString GetTooManyWithReportName(string reportName)
			{
				return ResString.GetMultilingualString("b210fd77-a3ae-4e46-88de-2df1661d1b0f",
@"Because of the large number of lines (or ""rows"") Report '{0}' is returning from the database, it has exceeded the Microsoft imposed limit (65,535 rows) on this Excel 97-2003 based report.
If you wish, you can deliver this report using Excel 2007 XLSX format which will raise the rows limit to 1,048,575.", reportName);
			}

			public static MultilingualString GetTooManyForExcel2003WithReportName(string reportName)
			{
				return ResString.GetMultilingualString("28009026-A405-4943-8BF4-C41F5797A38B",
@"Because of the large number of lines (or ""rows"") Report '{0}' is returning from the database, it has exceeded the Microsoft imposed limit (65,535 rows) on this Excel 97-2003 based report.
Try filtering or narrowing the size of the report so that it is below the Excel 97-2003 limit of 65,535 rows.", reportName);
			}
		}

		static class ColumnMessages
		{
			public static MultilingualString TooMany => ResString.GetMultilingualString("dd0d8b88-807a-4dd0-8bf7-3f5381650913",
@"Because of the large number of columns this report is returning, you have exceeded the Microsoft imposed limit (16,384 columns) on this Excel 2007 based report.");

			public static MultilingualString TooManyForExcel2003WithFormatSwitchQuestion => ResString.GetMultilingualString("416475a5-4511-404e-af49-8de06f1a2e9f",
@"Because of the large number of columns this report is returning, you have exceeded the Microsoft imposed limit (256 columns) on this Excel 97-2003 based report.
You can generate it using Excel 2007 XLSX format, which will raise the columns limit to 16,384.

Do you want to generate this report with Excel 2007 XLSX format?");

			public static MultilingualString GetTooManyWithReportName(string reportName)
			{
				return ResString.GetMultilingualString("ae589370-c638-4693-8b66-fc3b7b7bff1d",
@"Because of the large number of columns Report '{0}' is returning, it has exceeded the Microsoft imposed limit (256 columns) on this Excel 97-2003 based report.
If you wish, you can deliver this report using Excel 2007 XLSX format which will raise the columns limit to 16,384.", reportName);
			}

			public static MultilingualString GetTooManyForExcel2003WithReportName(string reportName)
			{
				return ResString.GetMultilingualString("D68D89D3-5212-4731-8EEE-8168A3E52AFD",
@"Because of the large number of columns Report '{0}' is returning, it has exceeded the Microsoft imposed limit (256 columns) on this Excel 97-2003 based report.", reportName);
			}
		}

		static class FormulaMessages
		{
			public static MultilingualString TooLong => ResString.GetMultilingualString("5BCFAABD-A289-4F67-9364-494A5EC247EF",
@"Because of the large formula in a cell this report is returning from the database, you have exceeded the Microsoft imposed limit on this Excel 97-2003 based report.
If you wish, you can deliver this report using Excel 2007 XLSX format which will raise the limit.");

			public static MultilingualString TooLongForExcel2003WithFormatSwitchQuestion => ResString.GetMultilingualString("E75F123B-407C-4716-8E0B-0E06534728BE",
@"Because of the large formula in a cell this report is returning from the database, you have exceeded the Microsoft imposed limit on this Excel 97-2003 based report.
You can generate it using Excel 2007 XLSX format, which will raise the limit.

Do you want to generate this report with Excel 2007 XLSX format?");

			public static MultilingualString GetTooLongWithReportName(string reportName)
			{
				return ResString.GetMultilingualString("47BBF237-D32B-48CE-A368-8DBFBC725EFF",
@"Because of the large formula in a cell Report '{0}' is returning from the database, it has exceeded the Microsoft imposed limit on this Excel 97-2003 based report.
If you wish, you can deliver this report using Excel 2007 XLSX format which will raise the limit.", reportName);
			}

			public static MultilingualString GetTooLongForExcel2003WithReportName(string reportName)
			{
				return ResString.GetMultilingualString("E25BDF6F-DCF9-4D7C-B033-708A8F347243",
@"Because of the large formula in a cell Report '{0}' is returning from the database, it has exceeded the Microsoft imposed limit on this Excel 97-2003 based report.
If you wish, you can deliver this report using Excel 2007 XLSX format which will raise the limit.", reportName);
			}
		}
	}
}
