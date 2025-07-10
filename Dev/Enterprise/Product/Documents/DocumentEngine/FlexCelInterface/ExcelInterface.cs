using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.DigitalSignature;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.RemotePrinting.Engine;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using FlexCel.Core;
using FlexCel.Draw;
using FlexCel.Pdf;
using FlexCel.Render;
using FlexCel.XlsAdapter;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DocumentEngine.FlexCelInterface
{
	public class ExcelInterface : IExcelInterface, IDisposable
	{
		public ExcelInterface()
			: this(true)
		{
		}

		public ExcelInterface(XlsFile xlsFile)
			: this(xlsFile, true)
		{
		}

		internal ExcelInterface(bool shouldRegisterWithDisposableLeakListener) : this(new XlsFile(),
			shouldRegisterWithDisposableLeakListener)
		{
		}

		internal ExcelInterface(XlsFile xlsFile, bool shouldRegisterWithDisposableLeakListener)
		{
			if (shouldRegisterWithDisposableLeakListener)
			{
				DisposableLeakListener.Instance.RegisterDisposable(this);
			}

			excelFile = xlsFile;
			excelFile.ErrorActions = TExcelFileErrorActions.ErrorOnFormulaConstantTooLong;
		}

		internal ExcelInterface(byte[] buffer, bool shouldRegisterWithDisposableLeakListener = true)
			: this(shouldRegisterWithDisposableLeakListener)
		{
			LoadExcelFile(buffer);
		}

		readonly XlsFile excelFile;

		public XlsFile Xls
		{
			get { return excelFile; }
		}

		public bool IsCached { get; set; }

		public bool ContainsCustomisedSections { get; set; }

		public bool HasBeenStyled { get; set; }

		public int GetRowHeight(int sheet, int row)
		{
			return excelFile.GetRowHeight(sheet, row + 1, true);
		}

		public int GetColumnWidth(int sheet, int column)
		{
			return excelFile.GetColWidth(sheet, column + 1, true);
		}

		public int GetColumnWidthInPixels(int sheet, int column)
		{
			return (int)Utilities.Round((decimal)(GetColumnWidth(sheet, column) / GetColumnWidthMultiplier(GraphicsUnit.Pixel)), 0);
		}

		public int GetMergedColumnWidth(int sheet, int row, int column)
		{
			excelFile.ActiveSheet = sheet + 1;

			var result = 0;
			var mergedBounds = excelFile.CellMergedBounds(row + 1, column + 1);

			for (var columnInMergedBounds = mergedBounds.Left; columnInMergedBounds <= mergedBounds.Right; columnInMergedBounds++)
			{
				result += excelFile.GetColWidth(sheet + 1, columnInMergedBounds, true);
			}

			return result;
		}

		public float GetRowHeightMultiplier(GraphicsUnit unit)
		{
			switch (unit)
			{
				case GraphicsUnit.Display:
					return (float)ExcelMetrics.RowMultDisplay(excelFile);

				default:
					throw new ArgumentException(@"Unit should be GraphicsUnit.Display", nameof(unit));
			}
		}

		public float GetColumnWidthMultiplier(GraphicsUnit unit)
		{
			switch (unit)
			{
				case GraphicsUnit.Display:
					return (float)ExcelMetrics.ColMultDisplay(excelFile);

				case GraphicsUnit.Pixel:
					return (float)ExcelMetrics.ColMult(excelFile);

				default:
					throw new ArgumentException(@"Unit should be GraphicsUnit.Display or GraphicsUnit.Pixel", nameof(unit));
			}
		}

		public int MaxRowCountSupportedByCurrentExcelFile
		{
			get
			{
				if (excelFile.FileFormatWhenOpened == TFileFormats.Xlsx)
				{
					return ZArchitecture.Environment.Excel.MaxRowCountSupported2007;
				}
				else
				{
					return ZArchitecture.Environment.Excel.MaxRowCountSupported97_2003;
				}
			}
		}

		public int GetMaxFormulaTextSizeSupportedByCurrentExcelFile(TFileFormats fileFormat)
		{
			return fileFormat == TFileFormats.Xlsx ? FlxConsts.Max_FormulaLen2007 : FlxConsts.Max_FormulaLen97_2003;
		}

		public int GetMaxFormulaArgumentsByCurrentExcelFile(TFileFormats fileFormat)
		{
			return fileFormat == TFileFormats.Xlsx ? FlxConsts.Max_FormulaArguments2007 : FlxConsts.Max_FormulaArguments2003;
		}

		public int MaxColCountSupportedByCurrentExcelFile
		{
			get
			{
				if (excelFile.FileFormatWhenOpened == TFileFormats.Xlsx)
				{
					return ZArchitecture.Environment.Excel.MaxColCountSupported2007;
				}
				else
				{
					return ZArchitecture.Environment.Excel.MaxColCountSupported97_2003;
				}
			}
		}

		internal void MergeCells(int sheet, int firstRow, int firstColumn, int lastRow, int lastColumn)
		{
			excelFile.ActiveSheet = sheet + 1;
			excelFile.MergeCells(firstRow + 1, firstColumn + 1, lastRow + 1, lastColumn + 1);
		}

		internal IDisposable SetCellValueTemporarily(int row, int column, object value)
		{
			var originalValue = excelFile.GetCellValue(row + 1, column + 1);
			SetCellValue(row, column, value);

			return new DisposableAction(() => SetCellValue(row, column, originalValue));
		}

		internal void SetCellValue(int row, int column, object value)
		{
			excelFile.SetCellValue(row + 1, column + 1, value);
		}

		internal void SetCellValue(int sheet, int row, int column, object value)
		{
			excelFile.ActiveSheet = sheet + 1;
			excelFile.SetCellValue(row + 1, column + 1, value);
		}

		public string GetCellReference(int rowIndex, int columnIndex)
		{
			if (rowIndex >= ZArchitecture.Environment.Excel.MaxRowCountSupported2007)
			{
				throw new ArgumentException("Out of range", nameof(rowIndex));
			}

			if (columnIndex >= ZArchitecture.Environment.Excel.MaxColCountSupported2007)
			{
				throw new ArgumentException("Out of range", nameof(columnIndex));
			}

			return CellReference.GetCellRef(rowIndex, columnIndex);
		}

		public TXlsNamedRange GetPrintTitlesRange(int sheetIndex)
		{
			return Xls.GetNamedRange(TXlsNamedRange.GetInternalName(InternalNameRange.Print_Titles), 0, sheetIndex + 1);
		}

		public void SetPrintTitlesRange(TXlsNamedRange range)
		{
			if (range.Name != TXlsNamedRange.GetInternalName(InternalNameRange.Print_Titles))
			{
				throw new ArgumentException("The given range must be a PrintTitles range", nameof(range));
			}

			Xls.SetNamedRange(range);
		}

		internal ExcelWorkSheet GetEmptyWorkSheetForUnitTests()
		{
			return new ExcelWorkSheet(this, "");
		}

		public ExcelWorkSheet CopySheet(ExcelWorkSheet sheetToCopy, string name = "")
		{
			var sheetIndex = WorkSheets.IndexOf(sheetToCopy) + 1;

			CopySheetCore(sheetIndex, sheetIndex + 1, 1);
			return AddSheet(sheetIndex + 1, name);
		}

		public void CopySheet(ExcelWorkSheet sheetToCopy, int count)
		{
			var sheetIndex = WorkSheets.IndexOf(sheetToCopy) + 1;

			CopySheetCore(sheetIndex, sheetIndex + 1, count);

			for (var i = 0; i < count; i++)
			{
				AddSheet(sheetIndex + 1 + i);
			}
		}

		void CopySheetCore(int copyFrom, int copyTo, int count)
		{
			try
			{
				Xls.InsertAndCopySheets(copyFrom, copyTo, count);
			}
			catch (FlexCelXlsAdapterException ex) when (ex.ErrorCode == XlsErr.ErrCantCopyPictFmla)
			{
				throw new ExcelInterfaceException(ExcelInterfaceExceptionType.CopyOLEObjectInTemplateException, ex.Message);
			}
		}

		ExcelWorkSheet AddSheet(int index, string name = "")
		{
			Xls.ActiveSheet = index;
			if (!string.IsNullOrEmpty(name))
			{
				Xls.SheetName = name;
			}

			var sheet = new ExcelWorkSheet(this, Xls.SheetName);
			WorkSheets.Insert(index - 1, sheet);
			return sheet;
		}

		public void LoadExcelFile(string fileName)
		{
			try
			{
				Xls.Open(fileName);
				UpdateWorkSheets();
			}
			catch (Exception exception)
			{
				if (exception.IsCriticalException())
				{
					throw;
				}

				if (!HandleFlexCelXlsAdapterException(exception as FlexCelXlsAdapterException, fileName: fileName))
				{
					throw new ExcelInterfaceException(ExcelInterfaceExceptionType.CouldNotOpenFile, "Could not open file " + fileName, exception);
				}
			}
		}

		public void LoadExcelFile(Stream xlsStream)
		{
			try
			{
				xlsStream.Seek(0, SeekOrigin.Begin);
				Xls.Open(xlsStream);
				UpdateWorkSheets();
			}
			catch (Exception exception)
			{
				if (exception.IsCriticalException())
				{
					throw;
				}

				if (!HandleFlexCelXlsAdapterException(exception as FlexCelXlsAdapterException, xlsStream))
				{
					throw new ExcelInterfaceException(ExcelInterfaceExceptionType.CouldNotOpenStream, "Could not open Xls stream", exception);
				}
			}
		}

		public void LoadExcelFile(byte[] buffer)
		{
			using (var stream = new MemoryStream(buffer))
			{
				LoadExcelFile(stream);
			}
		}

		internal void UpdateWorkSheets()
		{
			WorkSheets.Clear();

			var previousActiveSheet = Xls.ActiveSheet;

			for (var index = 1; index <= Xls.SheetCount; index++)
			{
				Xls.ActiveSheet = index;
				var workSheet = new ExcelWorkSheet(this, Xls.SheetName);
				WorkSheets.Add(workSheet);
			}

			Xls.ActiveSheet = previousActiveSheet;
		}

		public static ExcelInterface GetLoadedExcelInterface(Stream stream)
		{
			ExcelInterface cache = null;
			var key = string.Empty;

			Func<Stream, ExcelInterface> createLoadedExcelFromStream = s =>
			{
				var excelInterface = new ExcelInterface();
				try
				{
					excelInterface.LoadExcelFile(s);
				}
				catch
				{
					excelInterface.Dispose();
					throw;
				}
				return excelInterface;
			};

			if (shouldCacheLoadedExcelInterfaceByBinary)
			{
				using (var md5 = MD5.Create())
				{
					stream.Seek(0, SeekOrigin.Begin);
					key = Convert.ToBase64String(md5.ComputeHash(stream));
				}

				if (!string.IsNullOrEmpty(key))
				{
					cache = loadedExcelInterfacesFromBinary.GetOrAdd(key, k =>
					{
						var loadedExcelInterface = createLoadedExcelFromStream(stream);
						loadedExcelInterface.loadedExcelKeyFromBinary = k;
						return loadedExcelInterface;
					});
				}
			}

			if (cache == null)
			{
				cache = createLoadedExcelFromStream(stream);
			}
			return cache;
		}

		public static ExcelInterface GetLoadedExcelInterface(byte[] buffer)
		{
			using (var stream = new MemoryStream(buffer))
			{
				return GetLoadedExcelInterface(stream);
			}
		}

		public static IDisposable EnableCacheLoadedExcelInterfaceByBinary()
		{
			if (shouldCacheLoadedExcelInterfaceByBinary)
			{
				return null;
			}

			shouldCacheLoadedExcelInterfaceByBinary = true;

			Action disposeAction = () =>
			{
				shouldCacheLoadedExcelInterfaceByBinary = false;
				foreach (var cache in loadedExcelInterfacesFromBinary.Values)
				{
					cache.Dispose();
				}
				loadedExcelInterfacesFromBinary.Clear();
			};

			return new DisposableAction(disposeAction);
		}

		string loadedExcelKeyFromBinary = string.Empty;

		[ThreadSafe]
		static bool shouldCacheLoadedExcelInterfaceByBinary = false;

		[ThreadSafe]
		static readonly ConcurrentDictionary<string, ExcelInterface> loadedExcelInterfacesFromBinary = new ConcurrentDictionary<string, ExcelInterface>();

		#region SuppressResourceStringsCheckRegion

		bool HandleFlexCelXlsAdapterException(FlexCelXlsAdapterException exception, Stream xlsStream = null, string fileName = null)
		{
			if (exception != null)
			{
				switch (exception.ErrorCode)
				{
					case XlsErr.ErrFileIsNotSupported:
						var errorMessage = new System.Text.StringBuilder();
						errorMessage.AppendLine()
								.AppendLine(ExcelInterfaceExceptionBase.ErrorMessageFileFormatNotSupported)
								.Append("Sheetname: ").AppendLine(Xls?.SheetName)
								.Append("Filename: ").AppendLine(Xls?.ActiveFileName)
								.Append("Excel file format: ").AppendLine(Xls?.ExcelFileFormat.ToString());

						if (xlsStream != null)
						{
							var headerData = new byte[8];
							xlsStream.Position = 0;
							xlsStream.Read(headerData, 0, (int)Math.Min(headerData.Length, xlsStream.Length));
							errorMessage.Append("Excel file header: ").AppendLine(string.Join("-", headerData));
						}

						if (fileName != null)
						{
							errorMessage.Append("Excel file name: ").AppendLine(fileName);
						}
						throw new ExcelInterfaceException(ExcelInterfaceExceptionType.FileFormatNotSupported, ExcelInterfaceExceptionBase.ErrorMessageFileFormatNotSupported, new InvalidDataException(errorMessage.ToString(), exception));
				}
			}
			return false;
		}

		#endregion

		public static TFileFormats GetTFileFormat(string fileFormat)
		{
			TFileFormats flexcelFormat = TFileFormats.Automatic;

			switch (fileFormat.ToUpper())
			{
				case ExcelFileFormatOptionList.Codes.XLS:
					flexcelFormat = TFileFormats.Xls;
					break;

				case ExcelFileFormatOptionList.Codes.XLSX:
					flexcelFormat = TFileFormats.Xlsx;
					break;
			}

			return flexcelFormat;
		}

		public static string GetExtensionForExcel(TFileFormats format)
		{
			var extension = string.Empty;
			switch (format)
			{
				case TFileFormats.Xls:
					extension = AttachmentTypeList.Codes.Xls;
					break;

				case TFileFormats.Xlsx:
					extension = AttachmentTypeList.Codes.Xlsx;
					break;
			}

			return extension;
		}

		public string GetExtensionForExcelFromFile()
		{
			var extension = string.Empty;
			var format = excelFile.FileFormatWhenOpened;
			switch (format)
			{
				case TFileFormats.Xls:
					extension = AttachmentTypeList.Codes.Xls;
					break;

				case TFileFormats.Xlsx:
					extension = AttachmentTypeList.Codes.Xlsx;
					break;
			}

			return extension;
		}

		public void SaveToStream(Stream saveStream)
		{
			this.SaveToStream(saveStream, string.Empty);
		}

		public void SaveToStream(Stream saveStream, string fileFormat)
		{
			DoBeforeSaveCleanup();
			try
			{
				lock (excelFile)
				{
					excelFile.Save(saveStream, GetTFileFormat(fileFormat));
				}
			}
			catch (FlexCelException exception)
			{
				if (!HandleSaveException(exception))
				{
					throw;
				}
			}
			catch (ArgumentException argumentException)
			{
				if (!HandleIllegalCharactersInPathError(argumentException))
				{
					throw;
				}
			}
		}

		static bool HandleIllegalCharactersInPathError(ArgumentException argumentException)
		{
			if (argumentException.Message.Equals((NoResString)"Illegal characters in path."))
			{
				throw new ExcelInterfaceException(ExcelInterfaceExceptionType.IllegalCharactersInPath, $"{argumentException.Message}, possibly because there are some illegal characters in the hyper link(s).", argumentException);
			}

			return false;
		}

		static bool HandleSaveException(FlexCelException exception)
		{
			if (exception is FlexCelXlsAdapterException adapterException)
			{
				switch (adapterException.ErrorCode)
				{
					case XlsErr.ErrTooManyXFDefs:
						throw new ExcelInterfaceException(ExcelInterfaceExceptionType.TooManyCellStyles, "There are too many cell formats in this file.", exception);

					case XlsErr.ErrTooManyRows:
					case XlsErr.ErrTooManyEntries:
						throw new DocumentEngineTooManyRowsForThisFileFormatException(exception);

					case XlsErr.ErrTooManyColumns:
						throw new DocumentEngineTooManyColumnsForThisFileFormatException(exception);
				}
			}
			else if (exception is FlexCelCoreException coreException)
			{
				if (coreException.ErrorCode == FlxErr.ErrFormulaTooLong)
				{
					throw new DocumentEngineTooLongFormulaForThisFileFormatException(exception);
				}
			}
			return false;
		}

		public void ProtectSheets(string password)
		{
			for (int i = 1; i <= Xls.SheetCount; i++)
			{
				Xls.ActiveSheet = i;
				Xls.Protection.SetSheetProtection(password, new TSheetProtectionOptions(true));
			}
		}

		void DoBeforeSaveCleanup()
		{
			for (int i = 1; i <= Xls.SheetCount; i++)
			{
				Xls.ActiveSheet = i;
				if (Xls.SheetVisible == TXlsSheetVisible.Visible)
				{
					var frozenPane = Xls.GetFrozenPanes();
					Xls.ScrollWindow(TPanePosition.UpperLeft, 1, 1);
					if (frozenPane.CellRef == "A1")
					{
						Xls.ScrollWindow(TPanePosition.UpperRight, 1, 1);
						Xls.ScrollWindow(TPanePosition.LowerLeft, 1, 1);
						Xls.ScrollWindow(TPanePosition.LowerRight, 1, 1);
					}
					else
					{
						if (frozenPane.Row == 1 && frozenPane.Col > 1)
						{
							Xls.ScrollWindow(TPanePosition.UpperRight, 1, frozenPane.Col);
						}
						else if (frozenPane.Row > 1 && frozenPane.Col == 1)
						{
							Xls.ScrollWindow(TPanePosition.LowerLeft, frozenPane.Row, 1);
						}
						else if (frozenPane.Row > 1 && frozenPane.Col > 1)
						{
							Xls.ScrollWindow(TPanePosition.LowerRight, frozenPane.Row, frozenPane.Col);
						}
					}
					Xls.SelectCell(1, 1, true);
				}
			}
			SelectFirstVisibleSheet();
		}

		void SelectFirstVisibleSheet()
		{
			for (int i = 1; i <= Xls.SheetCount; i++)
			{
				Xls.ActiveSheet = i;
				if (Xls.SheetVisible == TXlsSheetVisible.Visible)
				{
					break;
				}
			}
			if (Xls.SheetVisible != TXlsSheetVisible.Visible)
			{
				throw new ExcelInterfaceException(ExcelInterfaceExceptionType.NoVisibleSheetsToSelect, "Xls file has no visible sheets in it!");
			}
		}

		public void SaveToFile(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
			{
				throw new ApplicationException("Filename must not be null or empty");
			}
			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}
			DoBeforeSaveCleanup();
			try
			{
				if (Xls.DefaultFileFormat == TFileFormats.Automatic)
				{
					Xls.Save(fileName);
				}
				else
				{
					Xls.Save(fileName, Xls.DefaultFileFormat);
				}
			}
			catch (FlexCelException exception)
			{
				if (!HandleSaveException(exception))
				{
					throw;
				}
			}
			catch (ArgumentException argumentException)
			{
				if (!HandleIllegalCharactersInPathError(argumentException))
				{
					throw;
				}
			}
		}

		public void NewExcelFile(int sheetCount)
		{
			NewExcelFile(sheetCount, TExcelFileFormat.v2003);
		}

		public void NewExcelFile(int sheetCount, TExcelFileFormat fileFormat)
		{
			Xls.NewFile(sheetCount, fileFormat);
			for (int i = 1; i <= Xls.SheetCount; i++)
			{
				Xls.ActiveSheet = i;
				ExcelWorkSheet sheet = new ExcelWorkSheet(this, Xls.SheetName);
				WorkSheets.Add(sheet);
			}
		}

		public int SheetCount
		{
			get { return Xls.SheetCount; }
		}

		public bool IsA1Selected()
		{
			bool result = false;
			TXlsCellRange[] selectionRange = Xls.GetSelectedCells();
			if (selectionRange.Length == 1)
			{
				result = (selectionRange[0].Bottom == 1) && (selectionRange[0].Top == 1) && (selectionRange[0].Left == 1) && (selectionRange[0].Right == 1);
			}
			return result;
		}

		public int GetSheetIndex(string sheetName)
		{
			Xls.ActiveSheetByName = sheetName;
			return Xls.ActiveSheet;
		}

		public string GetSheetName(int workSheetNo)
		{
			Xls.ActiveSheet = workSheetNo + 1;
			return Xls.SheetName;
		}

		public int ActiveWorksheet
		{
			get { return Xls.ActiveSheet - 1; }
			set { Xls.ActiveSheet = value + 1; }
		}

		public void PreviewInXl()
		{
			PreviewInXl(TempFileWithDelayedDelete.NewWithExtension("xls"));
		}

		public void PreviewInXl(string fileName)
		{
			PreviewInXl(TempFileWithDelayedDelete.NewWithFilename(fileName));
		}

		public void PreviewInXl(TempFile tempFile)
		{
			try
			{
				SaveToFile(tempFile.Filename);
			}
			catch (IOException ex)
			{
				Globals.Message.ShowError(ex.Message);
				try
				{
					tempFile.Dispose();
				}
				catch (IOException)
				{
					// Let's try disposing the file, but the same IO exception may prevent it
				}
				return;
			}
			try
			{
				object handle = OpenFile(tempFile.Filename);
				Process process = handle as Process;
				if (process != null)
				{
					process.EnableRaisingEvents = true;
					process.Exited += new EventHandler(delegate
						{
							tempFile.Dispose();
						});
					if (process.HasExited)
					{
						tempFile.Dispose();
					}
				}
				else
				{
					tempFile.Dispose();
				}
			}
			catch (Win32Exception ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		public void PreviewInXlWithoutDeletingFile(string fileName)
		{
			SaveToFile(fileName);
			try
			{
				OpenFile(fileName);
			}
			catch (Win32Exception ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		protected virtual object OpenFile(string fileName)
		{
#if DEBUG
			if (Globals.IsTest)
			{
				return null;
			}
#endif
			return ObjectFactory.Get<IUserFileAccess>().OpenFile(fileName);
		}

		public void SetPrintMargins(TXlsMargins margin)
		{
			Xls.SetPrintMargins(margin);
		}

		public void SetPaperSize(int widthInTenthsOfMillimeter, int heightInTenthsOfMillimeter)
		{
			Xls.PrintPaperSize = TPaperSize.Undefined;

			TPrinterDriverSettings settings = Xls.GetPrinterDriverSettings();

			const long DM_PAPERSIZE = 0x00000002L;
			const long DM_PAPERLENGTH = 0x00000004L;
			const long DM_PAPERWIDTH = 0x00000008L;

			if (settings != null)
			{
				byte[] pdata = settings.GetData();
				if (pdata != null && pdata.Length > 66 + 8 + 12 && pdata[0] == 0 && pdata[1] == 0)
				{
					long flags = GetCardinal(pdata, 66 + 8);

					if ((flags & DM_PAPERSIZE) != 0)
					{
						pdata[66 + 8 + 6] = 0;
						pdata[66 + 8 + 7] = 0;
					}

					if ((flags & DM_PAPERLENGTH) != 0)
					{
						unchecked
						{
							pdata[66 + 8 + 8] = (byte)heightInTenthsOfMillimeter;
							pdata[66 + 8 + 9] = (byte)(heightInTenthsOfMillimeter >> 8);
						}
					}

					if ((flags & DM_PAPERWIDTH) != 0)
					{
						unchecked
						{
							pdata[66 + 8 + 10] = (byte)widthInTenthsOfMillimeter;
							pdata[66 + 8 + 11] = (byte)(widthInTenthsOfMillimeter >> 8);
						}
					}

					Xls.SetPrinterDriverSettings(new TPrinterDriverSettings(pdata));
				}
			}
		}

		public void SetPageLayout(int widthInTenthsOfMillimeter, int heightInTenthsOfMillimeter, TXlsMargins margin = null)
		{
			SetPaperSize(widthInTenthsOfMillimeter, heightInTenthsOfMillimeter);
			SetPrintMargins(margin ?? new TXlsMargins(0, 0, 0, 0, 0, 0));
		}

		static long GetCardinal(byte[] data, int tPos)
		{
			unchecked
			{
				return (uint)(data[tPos] + (data[tPos + 1] << 8) + (data[tPos + 2] << 16) + (data[tPos + 3] << 24));
			}
		}

		public double GetBottomMargin()
		{
			return Xls.GetPrintMargins().Bottom;
		}

		public double GetTopMargin()
		{
			return Xls.GetPrintMargins().Top;
		}

		public double GetLeftMargin()
		{
			return Xls.GetPrintMargins().Left;
		}

		public double GetRightMargin()
		{
			return Xls.GetPrintMargins().Right;
		}

		public ExcelWorkSheetList WorkSheets
		{
			get { return workSheets ??= new ExcelWorkSheetList(); }
		}
		ExcelWorkSheetList workSheets;

		public byte[] GetPrinterDriverSettings()
		{
			return new PrinterDriverAdapter(Xls).GetPrinterDriverSettings();
		}

		public void SetPrinterDriverSettings(byte[] data)
		{
			new PrinterDriverAdapter(Xls).SetPrinterDriverSettings(data);
		}

		public int PaperSize
		{
			get { return (int)Xls.PrintPaperSize; }
		}

		public TPrintOptions PrintOptions
		{
			get { return Xls.PrintOptions; }
		}

		#region Print and Export

		public static void ScaleDocument(ExcelFile xls)
		{
			float xScale = 1;
			float yScale = 1;

			int scaleSheetIndex = xls.GetSheetIndex(Report.FlexCelScaleSheetName, false);

			if (scaleSheetIndex > 0)
			{
				xls.ActiveSheet = scaleSheetIndex;
				object xs = xls.GetCellValue(1, 2);
				if (!(xs is double))
				{
					throw new ExcelInterfaceException(ExcelInterfaceExceptionType.FlexCelScaleSheetError, "Error on #FlexCelScale sheet. X Scale (Cell B1) is not a number.");
				}
				xScale = (float)(double)xs;

				if (xScale <= 0.5)
				{
					throw new ExcelInterfaceException(ExcelInterfaceExceptionType.FlexCelScaleSheetError, string.Format("Error on #FlexCelScale sheet. Invalid X Scale: \"{0}\" .", xScale));
				}

				object ys = xls.GetCellValue(2, 2);
				if (!(ys is double))
				{
					throw new ExcelInterfaceException(ExcelInterfaceExceptionType.FlexCelScaleSheetError, "Error on #FlexCelScale sheet. Y Scale (Cell B2) is not a number.");
				}
				yScale = (float)(double)ys;

				if (yScale <= 0.5)
				{
					throw new ExcelInterfaceException(ExcelInterfaceExceptionType.FlexCelScaleSheetError, string.Format("Error on #FlexCelScale sheet. Invalid Y Scale: \"{0}\" .", yScale));
				}
			}

			xls.WidthCorrection = 1 / xScale;
			xls.HeightCorrection = 1 / yScale;
		}

		internal static string GetRangeFormulaWithNewSheetName(string rangeFormula, string fromName, string toName)
		{
			/* SheetName might be quoted, so we try with quotes first, then without quotes.
			 * examples for print titles range formulae:
			 *		='Sheet 3'!$B:$C,'Sheet 3'!$5:$6
			 *		=Sheet3!$B:$C,Sheet3!$5:$6
			 *		=$B:$C,$5:$6
			 *		=B:C,5:6
			 *		=$B:$C
			 *		=B:C
			 *		=5:6
			 * */
			if (rangeFormula.Contains(FormattableString.Invariant($"'{fromName}'")))
			{
				return rangeFormula.Replace(fromName, toName);
			}

			return rangeFormula.Replace(fromName, FormattableString.Invariant($"'{toName}'"));
		}

		internal static Tuple<int, int> GetFirstAndLastRowsToRepeat(string rangeFormula)
		{
			/* examples for print titles range formulae:
			 *		='Sheet 3'!$B:$C,'Sheet 3'!$5:$6
			 *		=Sheet3!$B:$C,Sheet3!$5:$6
			 *		=$B:$C,$5:$6
			 *		=B:C,5:6
			 *		=$B:$C
			 *		=B:C
			 *		=5:6
			 *	This method will extract the 5 and 6 from the examples above.
			 * */
			int first = 0;
			int last = 0;

			var match = Regex.Match(rangeFormula, @"\$?(\d+):\$?(\d+)");
			if (match.Success)
			{
				first = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
				last = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
			}

			if (0 < first && first <= last)
			{
				return new Tuple<int, int>(first, last);
			}
			else
			{
				return new Tuple<int, int>(0, 0);
			}
		}

		public void ExportToPdfAndScale(Stream stream, int zoomFactor, bool signDocument = false)
		{
			ExportToPdfAndScale(stream, zoomFactor, null, FlexCelPdfExportSafe.DefaultPdfVersion, TPdfType.Standard, signDocument);
		}

		public void ExportToPdfAndScale(string fileName, int zoomFactor, RemotePrinting.Engine.Watermark watermarkData)
		{
			using (var pdfStream = new FileStream(fileName, FileMode.Create))
			{
				ExportToPdfAndScale(pdfStream, zoomFactor, watermarkData);
			}
		}

		public void ExportToPdfAndScale(Stream stream, int zoomFactor, RemotePrinting.Engine.Watermark watermarkData)
		{
			ExportToPdfAndScale(stream, zoomFactor, watermarkData, FlexCelPdfExportSafe.DefaultPdfVersion, TPdfType.Standard);
		}

		public void ExportToPdfAndScale(Stream stream, int zoomFactor, RemotePrinting.Engine.Watermark watermarkData, int[] sheetsToExport)
		{
			ExportToPdfAndScale(stream, zoomFactor, watermarkData, FlexCelPdfExportSafe.DefaultPdfVersion, TPdfType.Standard, false, sheetsToExport);
		}

		public void ExportToPdfAndScale(Stream stream, int zoomFactor, RemotePrinting.Engine.Watermark watermarkData, TPdfType pdfType, bool signDocument = false, string signingOption = DocumentsSignBy.PFX, string signerName = "", ZGuid? branch = null, IEnumerable<FlexCelPdfA3FileInfo> files = null)
		{
			ExportToPdfAndScale(stream, zoomFactor, watermarkData, FlexCelPdfExportSafe.DefaultPdfVersion, pdfType, signDocument, signingOption: signingOption, signerName: signerName, branch: branch, files: files);
		}

		public void ExportToPdfAndScale(Stream stream, int zoomFactor, RemotePrinting.Engine.Watermark watermarkData, TPdfVersion pdfVersion)
		{
			ExportToPdfAndScale(stream, zoomFactor, watermarkData, pdfVersion, TPdfType.Standard);
		}

		public void ExportToPdfAndScale(Stream stream, int zoomFactor, RemotePrinting.Engine.Watermark watermarkData, TPdfVersion pdfVersion, TPdfType pdfType, bool signDocument = false, int[] sheetsToExport = null, string signingOption = DocumentsSignBy.PFX, string signerName = "", ZGuid? branch = null, IEnumerable<FlexCelPdfA3FileInfo> files = null)
		{
			ScaleDocument(Xls);
			Xls.PrintScale = Xls.PrintScale * zoomFactor / 100;

			using (var pdfExport = new FlexCelPdfExportSafe(Xls, true, pdfType, pdfVersion))
			{
				fWatermarkData = watermarkData;
				if (watermarkData != null)
				{
					pdfExport.AfterGeneratePage += new FlexCel.Render.PageEventHandler(AfterGeneratePage);
				}

				try
				{
					try
					{
						if (signDocument)
						{
							var signature = PdfSignatureFactory.NewPdfSignature(signingOption, signerName, pdfExport.Workbook.PrintPaperDimensions, branch);
							if (signature != null)
							{
								pdfExport.Sign(signature);
							}
						}
						pdfExport.BeginExport(stream);
						try
						{
							if (sheetsToExport == null || sheetsToExport.Length == 0)
							{
								sheetsToExport = Enumerable.Range(1, Xls.SheetCount).ToArray();
							}
							foreach (var i in sheetsToExport)
							{
								Xls.ActiveSheet = i;
								if (Xls.SheetVisible == TXlsSheetVisible.Visible)
								{
#if DEBUG
									ExportFallbackFonts_Exposed = pdfExport.FallbackFonts;
#endif
									RunDuringPDFExportForTesting();
									pdfExport.ExportSheet();
								}
							}
							files?.ForEach(f => pdfExport.AttachFile(f.FileName, f.MimeType, f.Description, f.ModificationTimeLocalTimeZone, f.AttachmentKind, f.DataProvider));
						}
						catch (FlexCelException e)
						{
							ExcelInterfaceExceptionType? excelInterfaceExceptionType = null;

							if (e is FlexCelCoreException flexCelCoreException)
							{
								if (flexCelCoreException.ErrorCode == FlxErr.ErrFontNotFound)
								{
									excelInterfaceExceptionType = ExcelInterfaceExceptionType.ErrorFontNotFound;
								}
								else if (flexCelCoreException.ErrorCode == FlxErr.ErrFontNotSupported)
								{
									excelInterfaceExceptionType = ExcelInterfaceExceptionType.ErrorFontNotSupported;
								}
								else if (flexCelCoreException.ErrorCode == FlxErr.ErrInvalidColumn)
								{
									excelInterfaceExceptionType = ExcelInterfaceExceptionType.ErrorInvalidColumn;
								}
							}
							else if (e is FlexCelPdfException flexCelPdfException)
							{
								if (flexCelPdfException.ErrorCode == PdfErr.ErrFontNotFound)
								{
									excelInterfaceExceptionType = ExcelInterfaceExceptionType.ErrorFontNotFound;
								}
							}

							if (excelInterfaceExceptionType.HasValue)
							{
								throw new ExcelInterfaceException(excelInterfaceExceptionType.Value, e.Message);
							}
							else
							{
								throw new DocumentEngineException("Error exporting " + Xls.ActiveSheetByName + " (" + Xls.ActiveFileName + ") to PDF.", e);
							}
						}
						catch (ArgumentException e)
						{
							//Font 'Arial Unicode MS' does not support style 'Regular'
							var regex = new System.Text.RegularExpressions.Regex(@"Font '.+' does not support style '.+'");
							if (regex.IsMatch(e.Message))
							{
								throw new ExcelInterfaceException(ExcelInterfaceExceptionType.ErrorFontNotSupported, e.Message);
							}
						}
						finally
						{
							pdfExport.EndExport();
						}
					}
					catch (CryptographicException)
					{
						var certificate = DocumentsDataRegistry.Instance.DigitalSignature.Value;
						var errorMessage = string.Empty;
						try
						{
							errorMessage = CertificateValidationHelper.GetInvalidCertificateChainErrors(certificate.DigitalSignature, certificate.CertificatePassword);
						}
						catch (CryptographicException e)
						{
							throw new ExcelInterfaceException(ExcelInterfaceExceptionType.ErrorGettingCertificateChain, @"There was a problem with getting certificate chain, please check your certificate installation.

Error: " + e.Message);
						}

						if (!errorMessage.IsNullOrEmpty())
						{
							throw new ExcelInterfaceException(ExcelInterfaceExceptionType.DigitalSignatureNotValid, errorMessage);
						}
						else
						{
							throw;
						}
					}
				}
				finally
				{
					if (watermarkData != null)
					{
						pdfExport.AfterGeneratePage -= new FlexCel.Render.PageEventHandler(AfterGeneratePage);
					}
				}
			}
		}

		[Conditional("DEBUG")]
		protected virtual void RunDuringPDFExportForTesting()
		{
		}

		void AfterGeneratePage(object sender, FlexCel.Render.PageEventArgs e)
		{
			if (fWatermarkData != null)
			{
				fWatermarkData.Draw(e.File, new SizeF((float)e.File.PageSize.Width * 72f / 100f * (float)e.File.Scale, (float)e.File.PageSize.Height * 72f / 100f * (float)e.File.Scale));
			}
		}

		#region HTML

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		public const string HTMLFilesDirectory = "_files";

		public class DocDataMultiplex
		{
			public DocDataMultiplex(byte[] data, bool isHTML)
			{
				this.Data = data;
				this.isHTML = isHTML;
				this.AdditionalAttachments = new List<AttachmentData>();
			}

			public DocDataMultiplex(byte[] data, bool isHTML, bool hasModifyPassword)
				: this(data, isHTML)
			{
				this.HasModifyPassword = hasModifyPassword;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
			public byte[] Data { get; set; }
			public string Path { get; set; }
			public bool isHTML { get; set; }
			public bool HasModifyPassword { get; }
			public List<AttachmentData> AdditionalAttachments { get; }
		}

		public class AttachmentData
		{
			public AttachmentData(string filename, byte[] data)
			{
				this.Filename = filename;
				this.Data = data;
			}

			public string Filename { get; set; }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
			public byte[] Data { get; set; }
		}

		public DocDataMultiplex ExportFirstSheetToHTMLAndRestToPDF(string fileName, int zoomFactor, RemotePrinting.Engine.Watermark watermarkData, bool deleteAfterwards = false)
		{
			string imagesDirectoryPath = null;
			TempFile tempFile = null;

			try
			{
				if (string.IsNullOrEmpty(fileName))
				{
					tempFile = TempFile.NewWithExtension("html");
					fileName = tempFile.Filename;
					deleteAfterwards = true;
				}

				var result = ExportToHTMLAndScale(fileName, zoomFactor, false, true);

				var imagesDirectoryName = DocumentConverter.GetSafeImagesDirectoryName(fileName);
				imagesDirectoryPath = Path.Combine(Path.GetDirectoryName(fileName), imagesDirectoryName);

				if (Xls.SheetCountVisible > 1)
				{
					SelectFirstVisibleSheet();
					Xls.SheetVisible = TXlsSheetVisible.Hidden;
					if (!Directory.Exists(imagesDirectoryPath))
					{
						Directory.CreateDirectory(imagesDirectoryPath);
					}
					var pdfFileName = Path.Combine(imagesDirectoryPath, Path.GetFileNameWithoutExtension(fileName) + ".pdf");
					var safePdfFileName = PathValidation.GetFilePathWithValidLength(pdfFileName);
					ExportToPdfAndScale(safePdfFileName, zoomFactor, watermarkData);
					if (File.Exists(safePdfFileName))
					{
						result.AdditionalAttachments.Add(new AttachmentData(Path.GetFileName(safePdfFileName), File.ReadAllBytes(safePdfFileName)));
					}
				}

				return result;
			}
			finally
			{
				if (deleteAfterwards)
				{
					if (tempFile != null)
					{
						tempFile.Dispose();
					}

					if (File.Exists(fileName))
					{
						TempFile.TryDeleteHandleAllExceptions(fileName);
					}

					if (Directory.Exists(imagesDirectoryPath))
					{
						TempDirectory.DeleteDirectory(imagesDirectoryPath, false);
					}
				}
			}
		}

		public DocDataMultiplex ExportToHTMLAndScale(string fileName, int zoomFactor, bool deleteAfterwards = false, bool firstPageOnly = false)
		{
			DocDataMultiplex result = new DocDataMultiplex(null, true);
			string imagesDirectoryPath = null;
			TempFile tempFile = null;

			ScaleDocument(Xls);
			Xls.PrintScale = Xls.PrintScale * zoomFactor / 100;

			using (var htmlExport = new FlexCelHtmlExport(Xls, true))
			{
				try
				{
					htmlExport.ImageNaming = TImageNaming.Guid; //to prevent problems with characters like () *
					htmlExport.UseContentId = true;
					htmlExport.FixOutlook2007CssSupport = true;
					htmlExport.SaveImage += SkipHiddenImages;

					if (string.IsNullOrEmpty(fileName))
					{
						tempFile = TempFile.NewWithExtension("html");
						fileName = tempFile.Filename;
						deleteAfterwards = true;
					}

					var imagesDirectoryName = DocumentConverter.GetSafeImagesDirectoryName(fileName);
					if (firstPageOnly && Xls.SheetCountVisible > 0)
					{
						SelectFirstVisibleSheet();
						htmlExport.Export(fileName, imagesDirectoryName);
					}
					else
					{
						htmlExport.ExportAllVisibleSheetsAsOneHtmlFile(fileName, imagesDirectoryName, null, (NoResString)@"<hr />");
					}
					imagesDirectoryPath = Path.Combine(Path.GetDirectoryName(fileName), imagesDirectoryName);

					DocumentConverter.FormatHtmlContent(fileName);

					result.Data = File.ReadAllBytes(fileName);
					result.Path = fileName;

					if (Directory.Exists(imagesDirectoryPath))
					{
						foreach (var imageFileName in Directory.EnumerateFiles(imagesDirectoryPath))
						{
							result.AdditionalAttachments.Add(new AttachmentData(Path.GetFileName(imageFileName), File.ReadAllBytes(imageFileName)));
						}
					}

					return result;
				}
				catch (FlexCelException e)
				{
					ExcelInterfaceExceptionType? excelInterfaceExceptionType = null;

					var flexCelCoreException = e as FlexCelCoreException;

					if (flexCelCoreException != null)
					{
						if (flexCelCoreException.ErrorCode == FlxErr.ErrFontNotFound)
						{
							excelInterfaceExceptionType = ExcelInterfaceExceptionType.ErrorFontNotFound;
						}
						else if (flexCelCoreException.ErrorCode == FlxErr.ErrFontNotSupported)
						{
							excelInterfaceExceptionType = ExcelInterfaceExceptionType.ErrorFontNotSupported;
						}
					}

					if (excelInterfaceExceptionType.HasValue)
					{
						throw new ExcelInterfaceException(excelInterfaceExceptionType.Value, e.Message);
					}
					else
					{
						throw new DocumentEngineException("Error exporting " + Xls.ActiveSheetByName + " (" + Xls.ActiveFileName + ") to HTML.", e);
					}
				}
				catch (ArgumentException e)
				{
					//Font 'Arial Unicode MS' does not support style 'Regular'
					var regex = new System.Text.RegularExpressions.Regex(@"Font '.+' does not support style '.+'");
					if (regex.IsMatch(e.Message))
					{
						throw new ExcelInterfaceException(ExcelInterfaceExceptionType.ErrorFontNotSupported, e.Message);
					}
				}
				finally
				{
					if (deleteAfterwards)
					{
						if (tempFile != null)
						{
							tempFile.Dispose();
						}

						if (File.Exists(fileName))
						{
							TempFile.TryDeleteHandleAllExceptions(fileName);
						}

						if (Directory.Exists(imagesDirectoryPath))
						{
							TempDirectory.DeleteDirectory(imagesDirectoryPath, false);
						}
					}
				}
			}

			throw new DocumentEngineException("Error exporting " + Xls.ActiveSheetByName + " (" + Xls.ActiveFileName + ") to HTML.");
		}

		void SkipHiddenImages(object sender, SaveImageEventArgs e)
		{
			// If the image is in a hidden row, we don't need to generate it.
			if (e.Workbook != null && e.ShapeProps?.Anchor?.Row1 > 0)
			{
				var totalRowHeight = GetImageTotalRowHeight(e.Workbook, e.ShapeProps.Anchor.Row1, e.ShapeProps.Anchor.Row2);
				if (totalRowHeight <= 0)
				{
					e.Processed = true;
				}
			}
		}

		int GetImageTotalRowHeight(ExcelFile workbook, int startRow, int endRow)
		{
			var rowHeight = 0;
			for (var i = startRow; i <= endRow; i++)
			{
				rowHeight += workbook.GetRowHeight(i, HiddenIsZero: true);
			}
			return rowHeight;
		}

		#endregion

		#region Multipage TIFF
		static ImageCodecInfo GetTiffEncoder()
		{
			ImageCodecInfo[] imageEncoders = ImageCodecInfo.GetImageEncoders();
			foreach (ImageCodecInfo imageEncoder in imageEncoders)
			{
				if (imageEncoder.MimeType == "image/tiff")
				{
					return imageEncoder;
				}
			}
			throw new Exception("Tiff encoder not found");
		}

#if DEBUG
		public
#endif
 Bitmap CreateBitmap(float resolution, TPaperDimensions pd, PixelFormat pixFmt)
		{
			Bitmap result = null;
			int width = (int)Math.Ceiling(pd.Width / 96F * resolution);
			int height = (int)Math.Ceiling(pd.Height / 96F * resolution);

			try
			{
				result = GetNewBitmap(pixFmt, width, height);
				result.SetResolution(resolution, resolution);
			}
			catch (Exception exception)
			{
				if (exception.IsCriticalException())
				{
					throw;
				}

				throw new BitmapCreationException(exception, width, height, resolution, pixFmt);
			}
			return result;
		}

#if DEBUG
		protected virtual
#endif
 Bitmap GetNewBitmap(PixelFormat pixFmt, int width, int height)
		{
			return new Bitmap(width, height, pixFmt);
		}

		void CreateMultiPageTiff(FlexCelImgExport imgExport, Stream outStream, int lastPage, bool toFaxFormat, PixelFormat pixFmt, RemotePrinting.Engine.Watermark watermarkData)
		{
			if (pixFmt != PixelFormat.Format1bppIndexed && pixFmt != PixelFormat.Format8bppIndexed && pixFmt != PixelFormat.Format24bppRgb)
			{
				throw new ArgumentException("Only Pixel formats supported are 1bpp, 8bpp and 24bpp");
			}
			Xls.ActiveSheet = 1;
			while (Xls.ActiveSheet < Xls.SheetCount && Xls.SheetVisible != TXlsSheetVisible.Visible)
			{
				Xls.ActiveSheet++;
			}

			ImageCodecInfo info = GetTiffEncoder();

			int paramCount = toFaxFormat ? 2 : 1;
			EncoderParameters ep = new EncoderParameters(paramCount);
			ep.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.MultiFrame);
			if (paramCount > 1)
			{
				ep.Param[1] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionCCITT3);
			}

			TImgExportInfo exportInfo = null;
			TPaperDimensions pd = imgExport.GetRealPageSize();

			//The temporary format in which images must be rendered (with current algorithm) to be converted to 1 byte is 32bpp. We don't need 32bpp on standard tiffs.
			//Also, we can not draw on an indexed color canvas.
			PixelFormat pixFmtTmp = pixFmt != PixelFormat.Format24bppRgb ? PixelFormat.Format32bppPArgb : pixFmt;

			using (Bitmap outImg = CreateBitmap((float)imgExport.Resolution, pd, pixFmt))
			{
				//First image is handled differently.
				if (pixFmt != pixFmtTmp)
				{
					using (Bitmap tmpImg = CreateBitmap((float)imgExport.Resolution, pd, pixFmtTmp))
					{
						using (Graphics gr = Graphics.FromImage(tmpImg))
						{
							gr.FillRectangle(Brushes.White, 0, 0, outImg.Width, outImg.Height); //Clear the background
							imgExport.ExportNext(gr, ref exportInfo);
							if (watermarkData != null)
							{
								gr.PageUnit = GraphicsUnit.Point;
								watermarkData.Draw(gr, new SizeF((float)pd.Width / 100f * 72f, (float)pd.Height / 100f * 72f));
							}
						}

						if (pixFmt == PixelFormat.Format1bppIndexed)
						{
							FloydSteinbergDither.ConvertToBlackAndWhite(tmpImg, outImg);
						}
						else
						{
							OctreeQuantizer.ConvertTo256Colors(tmpImg, outImg);
						}
					}
				}
				else
				{
					using (Graphics gr = Graphics.FromImage(outImg))
					{
						gr.FillRectangle(Brushes.White, 0, 0, outImg.Width, outImg.Height); //Clear the background
						imgExport.ExportNext(gr, ref exportInfo);
						if (watermarkData != null)
						{
							gr.PageUnit = GraphicsUnit.Point;
							watermarkData.Draw(gr, new SizeF((float)pd.Width / 100f * 72f, (float)pd.Height / 100f * 72f));
						}
					}
				}

				outImg.Save(outStream, info, ep);
				ep.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);

				//Now the rest of images.
				int start = Xls.ActiveSheet;
				for (int i = start; i <= lastPage; i++)
				{
					Xls.ActiveSheet = i;
					if (i > start)
					{
						pd = imgExport.GetRealPageSize();
						exportInfo = imgExport.GetFirstPageExportInfo();  //Clear the old imageinfo.
					}

					if (Xls.SheetVisible == TXlsSheetVisible.Visible)
					{
						for (int p = exportInfo.CurrentPage; p < exportInfo.TotalPages; p++)
						{
							using (Bitmap tmpImg = CreateBitmap((float)imgExport.Resolution, pd, pixFmtTmp))
							{
								using (Graphics gr = Graphics.FromImage(tmpImg))
								{
									gr.FillRectangle(Brushes.White, 0, 0, tmpImg.Width, tmpImg.Height); //Clear the background
									imgExport.ExportNext(gr, ref exportInfo);
									if (watermarkData != null)
									{
										gr.PageUnit = GraphicsUnit.Point;
										watermarkData.Draw(gr, new SizeF((float)pd.Width / 100f * 72f, (float)pd.Height / 100f * 72f));
									}

									if (pixFmt == PixelFormat.Format1bppIndexed)
									{
										using (Bitmap faxImg = FloydSteinbergDither.ConvertToBlackAndWhite(tmpImg))
										{
											outImg.SaveAdd(faxImg, ep);
										}
									}
									else if (pixFmt != pixFmtTmp)
									{
										using (Bitmap quantImg = OctreeQuantizer.ConvertTo256Colors(tmpImg))
										{
											outImg.SaveAdd(quantImg, ep);
										}
									}
									else
									{
										outImg.SaveAdd(tmpImg, ep);
									}
								}
							}
						}
					}
				}

				ep.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.Flush);
				outImg.SaveAdd(ep);
			}
		}

		public void ExportToMultiPageTiffAndScale(string fileName, bool printFirstWorkSheetOnly, int zoomFactor, bool toFaxFormat, int resolution, PixelFormat pixFormat, RemotePrinting.Engine.Watermark watermarkData)
		{
			using (FileStream tiffStream = new FileStream(fileName, FileMode.Create))
			{
				ExportToMultiPageTiffAndScale(tiffStream, printFirstWorkSheetOnly, zoomFactor, toFaxFormat, resolution, pixFormat, watermarkData);
			}
		}

		public void ExportToMultiPageTiffAndScale(Stream tiffStream, bool printFirstWorkSheetOnly, int zoomFactor, bool toFaxFormat, int resolution, PixelFormat pixFormat, RemotePrinting.Engine.Watermark watermarkData)
		{
			ScaleDocument(Xls);
			Xls.PrintScale = Xls.PrintScale * zoomFactor / 100;

			using (FlexCelImgExport imgExport = new FlexCelImgExport(Xls, true))
			{
				imgExport.Resolution = resolution;

				int count = printFirstWorkSheetOnly ? 1 : Xls.SheetCount;
				try
				{
					CreateMultiPageTiff(imgExport, tiffStream, count, toFaxFormat, pixFormat, watermarkData);
				}
				catch (ExternalException e) when (e.IsGdiPlusException())
				{
					e.ReportWithInvalidCharacters(this);
					throw;
				}
			}
		}

		#endregion

		#endregion

		RemotePrinting.Engine.Watermark fWatermarkData;

		#region IDisposable Members

		public void Dispose()
		{
			if (!shouldCacheLoadedExcelInterfaceByBinary || !loadedExcelInterfacesFromBinary.ContainsKey(loadedExcelKeyFromBinary))
			{
				DisposableLeakListener.Instance.UnRegisterDisposable(this);

				if (workSheets != null)
				{
					for (int index = workSheets.Count - 1; index >= 0; index--)
					{
						ExcelWorkSheet excelWorkSheet = workSheets[index];
						WorkSheets.Remove(excelWorkSheet);
						excelWorkSheet.Dispose();
					}

					workSheets = null;
				}
			}
		}

		#endregion

		IExcelWorkSheetList IExcelInterface.WorkSheets
		{
			get { return WorkSheets; }
		}

		public void SetOrientation(Orientation orientation)
		{
			switch (orientation)
			{
				case Orientation.Landscape:
					Xls.PrintOptions &= ~TPrintOptions.NoPls; // Reset Configuration Bit to 0
					Xls.PrintOptions &= ~TPrintOptions.Orientation; // Set Orientation Bit to 0 (Landscape)
					break;

				case Orientation.Portrait:
					Xls.PrintOptions &= ~TPrintOptions.NoPls; // Reset Configuration Bit to 0
					Xls.PrintOptions |= TPrintOptions.Orientation; // Set Orientation Bit to 1 (Portrait)
					break;
			}
		}

#if DEBUG
		internal string ExportFallbackFonts_Exposed;
#endif
	}

	public enum Orientation { Landscape = 0, Portrait = 1 }
}
