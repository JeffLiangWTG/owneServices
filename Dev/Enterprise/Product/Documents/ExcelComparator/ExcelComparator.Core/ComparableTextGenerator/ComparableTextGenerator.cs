using System;
using System.Collections.Generic;
using System.IO;
using Enterprise.DocumentEngine.FlexCelInterface;
using FlexCel.Core;
using FlexCel.XlsAdapter;

namespace Enterprise.ExcelComparator
{
	public class ComparableTextGenerator : IDisposable
	{
		public ComparableTextGenerator(string filePath, int columnNo)
		{
			this.filePath = filePath ?? "";
			this.columnNo = columnNo;
		}

		readonly string filePath;
		readonly int columnNo;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public bool SaveAsComparableTextTempFile()
		{
			ErrorText = string.Format("An Unknown Error occurred reading [{0}].", filePath);
			var pages = new List<string>();
			using (var excelInterface = new ExcelInterface())
			{
				try
				{
					excelInterface.LoadExcelFileWithoutCallingResDotGetString(filePath);
				}
				catch (Exception exception)
				{
					ErrorText = string.Format("An Error occurred reading [{0}]\r\n{1}", filePath, FormatExceptionMessage(exception));
					return false;
				}

				try
				{
					var excelFile = excelInterface.Xls;

					string author = (excelFile.DocumentProperties.GetStandardProperty(TPropertyId.Author) ?? "").ToString();
					string pageHeader = excelFile.PageHeader;
					string pageFooter = excelFile.PageFooter;

					pages.Add(string.Format(@"
Author: [{0}]
PageHeader: [{1}]
PageFooter: [{2}]
"
						, author
						, pageHeader
						, pageFooter
						).Trim());

					foreach (ExcelWorkSheet workSheet in excelInterface.WorkSheets)
					{
						string workSheetTitle = string.Format(@"{0}
WorkSheet No: [{1}]   (Is {3})
Sheet Name:   [{2}]
{0}
"
							, "=".PadRight(80, '-')
							, workSheet.WorkSheetNumber
							, workSheet.SheetName
							, workSheet.IsHidden ? "Hidden" : "Visible"
							);
						pages.Add((workSheetTitle + GetWorkSheetAsComparableText(workSheet)).Trim());

						if (OnlyCompareFirstWorkSheet())
						{
							break;
						}
					}
				}
				catch (Exception exception)
				{
					ErrorText = string.Format("An Error occurred parsing Worksheets in [{0}]\r\n{1}", filePath, FormatExceptionMessage(exception));
					return false;
				}
			}

			this.Content = string.Join("\r\n\r\n\r\n", pages.ToArray());

			try
			{
				string tempFilePath = GetTempFilePath();
				TempFileName = Path.Combine(tempFilePath, Path.GetFileNameWithoutExtension(filePath) + ".txt" + columnNo.ToString());
				File.WriteAllText(TempFileName, this.Content);
			}
			catch (Exception exception)
			{
				ErrorText = string.Format("An Error occurred writing comparable file [{0}]\r\n{1}", filePath, FormatExceptionMessage(exception));
				return false;
			}

			return true;
		}

		readonly List<IComparableTextGeneratorOption> options = new List<IComparableTextGeneratorOption>();
		public void AddOption(IComparableTextGeneratorOption option)
		{
			options.Add(option);
		}

		bool OnlyCompareFirstWorkSheet()
		{
			return options.Exists(x => x is CompareSectionsOption);
		}

		string GetWorkSheetAsComparableText(ExcelWorkSheet workSheet)
		{
			var text = workSheet.ToString(new CellFormatterExposingFormulae(), true);
			foreach (var option in options)
			{
				text = option.RunOptionOnWorkSheetContents(text, workSheet.WorkSheetNumber);
			}
			return text;
		}

		public string Content
		{
			get;
			private set;
		}

		static string FormatExceptionMessage(Exception exception)
		{
			if (exception is Enterprise.DocumentEngine.Exceptions.ExcelInterfaceException
				|| exception is FlexCel.Core.FlexCelException)
			{
				return "\"" + exception.Message + "\"";
			}
			else
			{
				return exception.ToString();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI011:TempPathRule", Justification = "Outside of Enterprise")]
		public static string GetTempFilePath()
		{
			string tempFolder = Path.Combine(Path.GetTempPath(), "ExcelComparator");
			if (!Directory.Exists(tempFolder))
			{
				Directory.CreateDirectory(tempFolder);
			}
			return tempFolder;
		}

		public string ErrorText
		{
			get;
			private set;
		}

		public string TempFileName
		{
			get;
			private set;
		}

		void IDisposable.Dispose()
		{
			if (!string.IsNullOrEmpty(TempFileName) && File.Exists(TempFileName))
			{
				try
				{
					File.Delete(TempFileName);
				}
				catch { }
			}
		}
	}

	static class ExcelInterfaceExtensions
	{
		public static void LoadExcelFileWithoutCallingResDotGetString(this ExcelInterface excelInterface, string filePath)
		{
			var xls = excelInterface.Xls;

			try
			{
				xls.Open(filePath);
			}
			catch (FlexCelXlsAdapterException exception)
			{
				if (exception.ErrorCode == XlsErr.ErrFileIsNotSupported)
				{
					throw new FlexCelXlsAdapterException("You can only load templates saved as 'Excel 97-2003 Workbook' (.xls) or 'Excel 2007 Workbook' (.xlsx) format.", XlsErr.ErrFileIsNotSupported);
				}
				throw;
			}
			catch (System.IO.FileNotFoundException exception)
			{
				throw new FlexCelXlsAdapterException(exception.Message, XlsErr.ErrFileIsNotSupported);
			}
			catch (ArgumentException exception)
			{
				throw new FlexCelXlsAdapterException(exception.Message, XlsErr.ErrFileIsNotSupported);
			}

			for (int i = 1; i <= xls.SheetCount; i++)
			{
				xls.ActiveSheet = i;
				ExcelWorkSheet sheet = new ExcelWorkSheet(excelInterface, xls.SheetName);
				excelInterface.WorkSheets.Add(sheet);
			}
		}
	}
}
