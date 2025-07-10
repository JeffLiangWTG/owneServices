using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using Enterprise.DocumentEngine;
using FlexCel.Core;
using FlexCel.XlsAdapter;

namespace Enterprise.DocBuilderTemplateMerge
{
	public class SystemDocumentElementsMerger
	{
		public byte[] systemDocumentAsByteArray;

		public SystemDocumentElementsMerger(byte[] systemDocumentAsByteArray)
		{
			Argument.NotNull(systemDocumentAsByteArray, nameof(systemDocumentAsByteArray));
			this.systemDocumentAsByteArray = systemDocumentAsByteArray;
		}

		XlsFile systemDocument;
		XlsFile customizedDocument;

		public void Merge(byte[] customizedDocumentAsByteArray)
		{
			Argument.NotNull(customizedDocumentAsByteArray, nameof(customizedDocumentAsByteArray));

			using (var systemDocumentStream = new MemoryStream(systemDocumentAsByteArray))
			using (var customizedDocumentStream = new MemoryStream(customizedDocumentAsByteArray))
			{
				systemDocument = new XlsFile();
				customizedDocument = new XlsFile();

				systemDocument.Open(systemDocumentStream);
				systemDocument.DocumentProperties.PreserveCreationDate = true;
				systemDocument.DocumentProperties.PreserveModifiedDate = true;
				customizedDocument.Open(customizedDocumentStream);

				ReplaceAllSections(systemDocument, customizedDocument);

				using (var mergedSystemDocumentStream = new MemoryStream())
				{
					ReStyleRedundantFormatsForDocBuilder(systemDocument);
					systemDocument.Save(mergedSystemDocumentStream);
					systemDocumentAsByteArray = mergedSystemDocumentStream.ToArray();
				}
			}
		}

		void ReplaceAllSections(ExcelFile systemDocument, ExcelFile source)
		{
			Argument.NotNull(source, nameof(source));
			Argument.NotNull(systemDocument, nameof(systemDocument));
			Argument.NotNull(this.customizedDocument, nameof(this.customizedDocument));

			systemDocument.ActiveSheet = 1;
			customizedDocument.ActiveSheet = 1;

			var rowFrom = 1;
			SectionInfo? sourceSectionInfo = GetNextSectionRowInfo(source, rowFrom);
			while (sourceSectionInfo.HasValue)
			{
				ReplaceSection(systemDocument, source, sourceSectionInfo.Value);
				rowFrom = sourceSectionInfo.Value.StartRow + 1;
				sourceSectionInfo = GetNextSectionRowInfo(source, rowFrom);
			}
		}

		void ReplaceSection(ExcelFile systemDocument, ExcelFile source, SectionInfo sourceSectionInfo)
		{
			Argument.NotNull(systemDocument, nameof(systemDocument));
			Argument.NotNull(source, nameof(source));

			var rowFrom = 1;
			SectionInfo? sectionInfo = GetNextSectionRowInfo(systemDocument, rowFrom);
			while (sectionInfo.HasValue)
			{
				if (sectionInfo.Value.Name == sourceSectionInfo.Name)
				{
					ReplaceContents(systemDocument, sectionInfo.Value.StartRow, sectionInfo.Value.EndRow, source, sourceSectionInfo.StartRow, sourceSectionInfo.EndRow);
					return;
				}

				rowFrom = sectionInfo.Value.EndRow + 1;
				sectionInfo = GetNextSectionRowInfo(systemDocument, rowFrom);
			}

			InsertContents(systemDocument, rowFrom, source, sourceSectionInfo.StartRow, sourceSectionInfo.EndRow);
		}

		SectionInfo? GetNextSectionRowInfo(ExcelFile excelFile, int rowFrom)
		{
			Argument.NotNull(excelFile, nameof(excelFile));
			int sectionStartRow = rowFrom;
			int sectionEndRow = rowFrom;

			string sectionName;

			while (!(sectionName = GetCellTextToUpper(excelFile, sectionStartRow, 1)).StartsWith(Constants.AreaIdentifierTags.ConfigurableSection))
			{
				if (sectionName.StartsWith(Constants.AreaIdentifierTags.EndOfReport) || (sectionStartRow > excelFile.RowCount))
				{
					return null;
				}
				sectionStartRow++;
			}

			sectionEndRow = sectionStartRow;
			string nextCellText;
			while (!(nextCellText = GetCellTextToUpper(excelFile, sectionEndRow + 1, 1)).StartsWith(Constants.AreaIdentifierTags.ConfigurableSection)
				&& !nextCellText.StartsWith(Constants.AreaIdentifierTags.EndOfReport)
				&& (sectionEndRow <= excelFile.RowCount))
			{
				sectionEndRow++;
			}

			return new SectionInfo(sectionName, sectionStartRow, sectionEndRow);
		}

		string GetCellTextToUpper(ExcelFile excelFile, int row, int col)
		{
			Argument.NotNull(excelFile, nameof(excelFile));

			var cellValue = excelFile.GetCellValue(row, col);
			return cellValue != null ? cellValue.ToString().ToUpper() : string.Empty;
		}

		void InsertContents(ExcelFile destination, int destinationStartRow, ExcelFile source, int sourceStartRow, int sourceEndRow)
		{
			Argument.NotNull(source, nameof(source));
			Argument.NotNull(destination, nameof(destination));

			var sourceRange = new TXlsCellRange(sourceStartRow, 1, sourceEndRow, source.ColCount);
			destination.InsertAndCopyRange(sourceRange, destinationStartRow, 1, 1, TFlxInsertMode.ShiftRowDown, TRangeCopyMode.All, source, source.ActiveSheet);
		}

		void ReplaceContents(ExcelFile destination, int destinationStartRow, int destinationEndRow, ExcelFile source, int sourceStartRow, int sourceEndRow)
		{
			Argument.NotNull(destination, nameof(destination));
			Argument.NotNull(source, nameof(source));

			var destinationRange = new TXlsCellRange(destinationStartRow, 1, destinationEndRow, 256);
			destination.DeleteRange(destinationRange, TFlxInsertMode.ShiftRowDown);

			InsertContents(destination, destinationStartRow, source, sourceStartRow, sourceEndRow);
		}

		struct SectionInfo
		{
			public readonly string Name;
			public readonly int StartRow;
			public readonly int EndRow;

			public SectionInfo(string name, int startRow, int endRow)
			{
				Name = name;
				StartRow = startRow;
				EndRow = endRow;
			}
		}

		public const string DocBuilderDocumentsDir = @"Enterprise\Product\Documents\ExcelTemplates\Documents\DocBuilder\";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "static file name")]
		public const string SystemDocumentElementsFileName = "System Document Elements.xls";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "static file name")]
		const string CustomizedDocumentElementsFileNameFormat = @"Customized Document Elements For Shelf [{0}].xls";
		public static readonly Regex CustomizedDocumentElementsFileNameRegex = new Regex(String.Format(CustomizedDocumentElementsFileNameFormat.Replace("[", "\\[").Replace("]", "\\]"), @"[^\[^\]]+"));

		public static string NewCustomizedDocumentElementsFileName()
		{
			return String.Format(CustomizedDocumentElementsFileNameFormat, Guid.NewGuid().ToString());
		}

		#region ReStyleRedundantFormatsForDocBuilder

		const int DocumentHeadingColorIndex = 49;
		const int PageNumberHeadingColorIndex = 46;
		const int PrimaryHeadingColorIndex = 48;
		const int SecondaryHeadingColorIndex = 56;
		const int PrimaryBodyColorIndex = 24;
		const int SecondaryBodyColorIndex = 40;

		public const long DocumentHeadingMagicColor = 0xFF003366;
		public const long PageNumberHeadingMagicColor = 0xFFFF6600;
		public const long PrimaryHeadingMagicColor = 0xFF969696;
		public const long SecondaryHeadingMagicColor = 0xFF333333;
		public const long PrimaryBodyMagicColor = 0xFFCCCCFF;
		public const long SecondaryBodyMagicColor = 0xFFFFCC99;

		static void ReStyleRedundantFormatsForDocBuilder(XlsFile xlsFile)
		{
			Argument.NotNull(xlsFile, nameof(xlsFile));

			var allowedIndexedColors = new[]
			{
				DocumentHeadingColorIndex,
				PageNumberHeadingColorIndex,
				PrimaryHeadingColorIndex,
				SecondaryHeadingColorIndex,
				PrimaryBodyColorIndex,
				SecondaryBodyColorIndex
			};

			var formatCount = xlsFile.FormatCount;

			for (var index = 0; index < formatCount; index++)
			{
				var format = xlsFile.GetFormat(index);

				format.Borders.Bottom.Color = TExcelColor.Automatic;
				format.Borders.Top.Color = TExcelColor.Automatic;
				format.Borders.Right.Color = TExcelColor.Automatic;
				format.Borders.Left.Color = TExcelColor.Automatic;
				format.Borders.Diagonal.Color = TExcelColor.Automatic;

				var backgroundColour = format.FillPattern.FgColor;

				switch (backgroundColour.ColorType)
				{
					case TColorType.RGB:
						switch (backgroundColour.RGB)
						{
							case DocumentHeadingMagicColor:
								format.FillPattern.FgColor = TExcelColor.FromIndex(DocumentHeadingColorIndex);
								break;

							case PageNumberHeadingMagicColor:
								format.FillPattern.FgColor = TExcelColor.FromIndex(PageNumberHeadingColorIndex);
								break;

							case PrimaryHeadingMagicColor:
								format.FillPattern.FgColor = TExcelColor.FromIndex(PrimaryHeadingColorIndex);
								break;

							case SecondaryHeadingMagicColor:
								format.FillPattern.FgColor = TExcelColor.FromIndex(SecondaryHeadingColorIndex);
								break;

							case PrimaryBodyMagicColor:
								format.FillPattern.FgColor = TExcelColor.FromIndex(PrimaryBodyColorIndex);
								break;

							case SecondaryBodyMagicColor:
								format.FillPattern.FgColor = TExcelColor.FromIndex(SecondaryBodyColorIndex);
								break;

							default:
								throw new InvalidOperationException("RGB Colors are not allowed.");
						}

						break;

					case TColorType.Theme:
						throw new InvalidOperationException("Themes are not allowed.");

					case TColorType.Indexed:
						if (!allowedIndexedColors.Contains(backgroundColour.Index))
						{
							throw new InvalidOperationException("Color index not allowed.");
						}

						break;
				}

				if (backgroundColour.ColorType == TColorType.Indexed)
				{
					switch (backgroundColour.Index)
					{
						case DocumentHeadingColorIndex:
							format.Font.Name = "Arial";
							format.Font.Size20 = 20 * 20;
							format.Font.Style = TFlxFontStyles.Bold;
							format.Font.Color = TExcelColor.FromIndex(2);
							break;

						case PageNumberHeadingColorIndex:
							format.Font.Name = "Arial";
							format.Font.Size20 = 10 * 20;
							format.Font.Style = TFlxFontStyles.Bold;
							format.Font.Color = TExcelColor.FromIndex(2);
							break;

						case PrimaryHeadingColorIndex:
							format.Font.Name = "Arial";
							format.Font.Size20 = 8 * 20;
							format.Font.Style = TFlxFontStyles.Bold;
							format.Font.Color = TExcelColor.FromIndex(2);
							break;

						case SecondaryHeadingColorIndex:
							format.Font.Name = "Arial";
							format.Font.Size20 = 8 * 20;
							format.Font.Style = TFlxFontStyles.Bold;
							format.Font.Color = TExcelColor.FromIndex(2);
							break;
					}
				}

				xlsFile.SetFormat(index, format);
			}
		}

		#endregion
	}
}
