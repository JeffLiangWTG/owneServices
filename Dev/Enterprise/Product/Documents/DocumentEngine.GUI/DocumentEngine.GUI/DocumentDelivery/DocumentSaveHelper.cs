using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using FlexCel.Pdf;

namespace Enterprise.DocumentEngine.GUI.DocumentDelivery
{
	class DocumentSaveHelper
	{
		public SaveAsFileInfo GetSaveAsFileInfo(PrintTask printTask, string fileName)
		{
			using (var dialog = new ZSaveFileDialog())
			{
				if (printTask != null && printTask.ParentMenuCommand != null)
				{
					dialog.FileName = printTask.ParentMenuCommand.SU_MenuNameMultilingual;
				}
				else if (!string.IsNullOrEmpty(fileName))
				{
					dialog.FileName = fileName;
				}

				var disabledFileExtensionIndexes = SetDialogFiltersWithDisabled(dialog, printTask);
				var result = dialog.ShowDialog();

				if (result == DialogResult.OK)
				{
					if (IsFileExtensionMatchingSelectedFilter(dialog, disabledFileExtensionIndexes))
					{
						var type = SaveAsFileType.Xls;

						switch (GetFileDialogFilterIndexPlusDisabled(dialog, disabledFileExtensionIndexes))
						{
							case DialogFilterIndex.PDF:
								type = SaveAsFileType.Pdf;
								break;

							case DialogFilterIndex.PDFA:
								type = SaveAsFileType.Pdfa;
								break;

							case DialogFilterIndex.XLS:
								type = SaveAsFileType.Xls;
								break;

							case DialogFilterIndex.XLSX:
								type = SaveAsFileType.Xlsx;
								break;

							case DialogFilterIndex.TIF:
								type = SaveAsFileType.Tif;
								break;

							case DialogFilterIndex.CSV:
								type = SaveAsFileType.Csv;
								break;

							case DialogFilterIndex.CSVWithHeader:
								type = SaveAsFileType.CsvWithColumnHeadings;
								break;

							case DialogFilterIndex.XML:
								type = SaveAsFileType.Xml;
								break;
						}

						return new SaveAsFileInfo(dialog.OpenFile(), dialog.UnmappedFileName, type);
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("c26aa2a5-d3db-4506-81c1-de9312ad195b", @"The filename extension you have entered does not match the selected file type.
Please select the proper file type."), Res.GetString("1435cf98-ac4e-42ce-93f1-1f48efaaeb77", "Invalid file extension"));

						return null;
					}
				}

				return null;
			}
		}

#if DEBUG
		internal
#endif
		List<int> SetDialogFiltersWithDisabled(ZSaveFileDialog dialog, PrintTask printTask)
		{
			var disabledFileExtensionIndexes = new List<int>();
			var reportCommand = printTask != null ? printTask.ParentMenuCommand as ReportCommand : null;

			var disablCSVExport = reportCommand == null || reportCommand.DisableCSVExport;
			var disableXLSXExport = reportCommand != null && reportCommand.DisableXLSXExport;
			var hasColumnHeader = reportCommand != null && reportCommand.ReportHasColumnHeaders;

			var dialogFilerList = new List<string>()
			{
				Res.GetString("5767382b-8b8e-49c0-bb13-b14dfee71fe0", "Portable Document Format {0}", "(*.pdf)|*.pdf"),
				Res.GetString("f8f3a656-293a-4265-bc59-3aa1f186e859", "Portable Document Format Archive {0}", "(*.pdf)|*.pdf"),
				Res.GetString("3f7fd5d4-392b-437f-8cad-387b80831f90", "Microsoft Excel 97-2003 Spreadsheet {0}", "(*.xls)|*.xls")
			};

			if (disableXLSXExport)
			{
				disabledFileExtensionIndexes.Add(DialogFilterIndex.XLSX);
			}
			else
			{
				dialogFilerList.Add(Res.GetString("c5338e32-ab2a-443c-b764-ede2a5812157", "Microsoft Excel 2007 Spreadsheet {0}", "(*.xlsx)|*.xlsx"));
			}

			dialogFilerList.Add(Res.GetString("6d4fdc7c-e947-455b-901e-2bd7337177d3", "Tagged Image File {0}", "(*.tif)|*.tif"));

			if (disablCSVExport)
			{
				disabledFileExtensionIndexes.Add(DialogFilterIndex.CSV);
				disabledFileExtensionIndexes.Add(DialogFilterIndex.CSVWithHeader);
				disabledFileExtensionIndexes.Add(DialogFilterIndex.XML);
			}
			else
			{
				dialogFilerList.Add(Res.GetString("8a8c83bc-f0d9-4a28-806f-a910a3f13f24", "Comma Separated Values {0}", "(*.csv)|*.csv"));

				if (hasColumnHeader)
				{
					dialogFilerList.Add(Res.GetString("d398270c-cc30-47bf-9d0f-c75c6f0413e2", "Comma Separated Values (With Column Headings) {0}", "(*.csv)|*.csv"));
					dialogFilerList.Add(Res.GetString("59bf14f0-d23f-4e5a-abff-3615c7baf983", "XML Files {0}", "(*.xml)|*.xml"));
				}
				else
				{
					disabledFileExtensionIndexes.Add(DialogFilterIndex.CSVWithHeader);
					disabledFileExtensionIndexes.Add(DialogFilterIndex.XML);
				}
			}

			dialog.Filter = string.Join("|", dialogFilerList);

			return disabledFileExtensionIndexes;
		}

		bool IsFileExtensionMatchingSelectedFilter(ZSaveFileDialog dialog, List<int> disableFileExtensionIndexes)
		{
			var isMatching = false;
			var extension = Path.GetExtension(dialog.UnmappedFileName).ToUpper(CultureInfo.InvariantCulture);
			switch (GetFileDialogFilterIndexPlusDisabled(dialog, disableFileExtensionIndexes))
			{
				case DialogFilterIndex.PDF:
				case DialogFilterIndex.PDFA:
					isMatching = extension == ".PDF";
					break;

				case DialogFilterIndex.XLS:
					isMatching = extension == ".XLS";
					break;

				case DialogFilterIndex.XLSX:
					isMatching = extension == ".XLSX";
					break;

				case DialogFilterIndex.TIF:
					isMatching = extension == ".TIF";
					break;

				case DialogFilterIndex.CSV:
				case DialogFilterIndex.CSVWithHeader:
					isMatching = extension == ".CSV";
					break;

				case DialogFilterIndex.XML:
					isMatching = extension == ".XML";
					break;
			}
			return isMatching;
		}

#if DEBUG
		internal
#endif
		int GetFileDialogFilterIndexPlusDisabled(ZSaveFileDialog dialog, List<int> disabledFileExtensionIndexes)
		{
			var filterIndex = dialog.FilterIndex;
			if (disabledFileExtensionIndexes.Count > 0)
			{
				disabledFileExtensionIndexes.Sort();
				for (int i = 0; i < disabledFileExtensionIndexes.Count; i++)
				{
					filterIndex = filterIndex >= disabledFileExtensionIndexes[i] ? filterIndex + 1 : filterIndex;
				}
			}

			return filterIndex;
		}

		public static void SaveStream(ExcelInterface excelInterface, Stream fileContents, SaveAsFileType type, Stream stream, DeliveryInfo deliveryInfo)
		{
			excelInterface.LoadExcelFile(fileContents);

			switch (type)
			{
				case SaveAsFileType.Pdf:
					excelInterface.ExportToPdfAndScale(stream, 100, deliveryInfo.Watermark, TPdfType.Standard, deliveryInfo.ShouldSignByPFX);
					break;

				case SaveAsFileType.Pdfa:
					excelInterface.ExportToPdfAndScale(stream, 100, deliveryInfo.Watermark, TPdfType.PDFA2, deliveryInfo.ShouldSignByPFX);
					break;

				case SaveAsFileType.Tif:
					excelInterface.ExportToMultiPageTiffAndScale(stream, false, 100, false, Env.Registry.PDFTIFResolution, PixelFormat.Format24bppRgb, deliveryInfo.Watermark);
					break;

				default:
					excelInterface.SaveToStream(stream, type.ToString());
					break;
			}
		}
	}

	class DialogFilterIndex
	{
		internal const int PDF = 1;
		internal const int PDFA = 2;
		internal const int XLS = 3;
		internal const int XLSX = 4;
		internal const int TIF = 5;
		internal const int CSV = 6;
		internal const int CSVWithHeader = 7;
		internal const int XML = 8;
	}
}
