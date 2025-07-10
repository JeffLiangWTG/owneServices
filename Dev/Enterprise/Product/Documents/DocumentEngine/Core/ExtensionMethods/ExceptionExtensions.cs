using System;
using System.Runtime.InteropServices;
using System.Text;
using CargoWise.Common;
using Enterprise.DocumentEngine.FlexCelInterface;
using FlexCel.XlsAdapter;

namespace Enterprise.DocumentEngine
{
	public static class ExceptionExtensions
	{
		public static bool IsBadImageFormatException(this Exception exception)
		{
			return (exception.Source == "System.Drawing" && exception is ArgumentException);
		}

		public static bool IsGdiPlusException(this Exception ex)
		{
			var gdiPlusException = ex.Find<ExternalException>();
			return gdiPlusException is { ErrorCode: -2147467259 };
		}

		public static void ReportWithInvalidCharacters(this ExternalException ex, XlsFile xlsFile)
		{
			if (xlsFile != null)
			{
				using var excelInterface = new ExcelInterface(xlsFile, false);
				excelInterface.UpdateWorkSheets();
				ex.ReportWithInvalidCharacters(excelInterface);
			}
		}

		public static void ReportWithInvalidCharacters(this ExternalException ex, ExcelInterface excelInterface)
		{
			var activeSheetIndex = excelInterface.ActiveWorksheet;
			if (excelInterface.WorkSheets.Count > activeSheetIndex)
			{
				var sheetContent = excelInterface.WorkSheets[activeSheetIndex].ToString();
				ex.ReportWithInvalidCharacters(sheetContent);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "This is only for debugging, This should always be in English")]
		public static void ReportWithInvalidCharacters(this ExternalException ex, string text)
		{
			var errorMessage = new StringBuilder();
			errorMessage.AppendLine(ex.Message);
			errorMessage.AppendLine(@"
This is usually due to specific combination of characters in the string that GDI+ cannot handle.
The current solution is to remove illegal strings from the Excel content.
Please use Enterprise.DocumentEngine.StringExtensions.GetShortestInvalidCharacters() to process the string and retrieve the shortest invalid character combination with hex type.
Then add it to Enterprise.DocumentEngine.StringExtensions.InvalidHexStrings.

The string content in Excel is as follows:
");
			errorMessage.AppendLine(text);

			ErrorReporter.ReportOnce("MeasureStringErrorInGDI+", errorMessage.ToString(), ex);
		}
	}
}
