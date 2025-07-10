using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class ExpandToFit : ValueProvider, INonVisualisableValueProviderThatModifyDocumentLayout
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<ExpandToFit[({RowToExpand})]>", ResString.GetMultilingualString("6b9c3c3b-83b6-492f-bd56-6f63d7343961", @"Used as a prefix to a subsequent macro. 
If the value inserted by the subsequent macro won't fit in the cell in the template, the row height will be progressively expanded until it will fit. 
The {0} parameter is used to determine if the first or last row of a range of merged cells should be expanded. 
If {0} parameter is not specified, the first row in a merged cell will expand. 
This macro can not be used with the {1} or {2} macros. 
{3}",
"{RowToExpand}", "<ShrinkToFit>", "<AutoHeight>", DocumentationForFormatting),
				new List<(string example, object expectedResult)> { ("<ExpandToFit><ShipmentNumber>", null), ((NoResString)"<ExpandToFit(First)><ShipmentNumber>", null), ((NoResString)"<ExpandToFit(Last)><ShipmentNumber>", null) });
		}

		protected override object GetReplacementCore(string macro, Report report) => string.Empty;

		public override Regex Regex => regex;

		static readonly Regex regex = new Regex(string.Format("^{0}$", RegexPattern), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only. Regular Expression.")]
		const string RegexPattern = @"<ExpandToFit(\(\s*(?<RowToExpand>First|Last)\s*\))?>";
		internal static readonly Regex RegexToFindMacroAnyWhereInString = new Regex(RegexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		internal static RowToExpand GetRowToExpand(string macro)
		{
			var result = RowToExpand.First;
			var match = RegexToFindMacroAnyWhereInString.Match(macro);

			if (match.Success)
			{
				var rowToExpandGroup = match.Groups["RowToExpand"];
				if (rowToExpandGroup.Success)
				{
					if (rowToExpandGroup.Value.Equals((NoResString)"Last", System.StringComparison.InvariantCultureIgnoreCase))
					{
						result = RowToExpand.Last;
					}
				}
			}

			return result;
		}

		internal static void Expand(ExcelInterface excelInterface, int sheet, int row, int column, RowToExpand rowToExpand, string text)
		{
			var workSheet = excelInterface.WorkSheets[sheet];
			var cell = workSheet.GetCell(row, column);
			if (ShouldExpandCell(cell))
			{
				DoExpand(workSheet, cell, excelInterface, sheet, row, column, rowToExpand, text);
			}
		}

		// should only expand-to-fit already visible cells, otherwise they have probably been hidden by HideRowIf for a good reason
		static bool ShouldExpandCell(ExcelCell cell) => cell.Height > 0;

		static void DoExpand(ExcelWorkSheet workSheet, ExcelCell cell, ExcelInterface excelInterface, int sheet, int row, int column, RowToExpand rowToExpand, string text)
		{
			var format = workSheet.GetCellFormat(row, column);
			using (var font = format.GetFont())
			{
				var calculator = new TextSizeCalculator(font);
				var cellWidthInXl = excelInterface.GetMergedColumnWidth(sheet, row, column);
				var cellWidthUnscaled = (int)Math.Ceiling(cellWidthInXl / excelInterface.GetColumnWidthMultiplier(GraphicsUnit.Display));
				var cellWidthScaled = ControlDpiScalingHelper.ScaleToCurrentDpiX(cellWidthUnscaled);
				// Excel adds left / right padding to cells which reduces the width available for the text
				var availableWidthForText = cellWidthScaled - ControlDpiScalingHelper.MarkAsScaled(2 * ExcelCell.ExcelCellTextPaddingInDeviceIndependentPixels);
				var sizeScaled = calculator.GetTextSize(text, calculator.Font, GraphicsUnit.Display, availableWidthForText);

				var rowHeightMultiplier = excelInterface.GetRowHeightMultiplier(GraphicsUnit.Display);
				var requiredHeight = (int)Math.Ceiling(sizeScaled.Height * rowHeightMultiplier);
				if (rowToExpand == RowToExpand.First)
				{
					ExpandFirstRow(cell, requiredHeight);
				}
				else
				{
					ExpandLastRow(cell, requiredHeight);
				}
			}
		}

		static void ExpandFirstRow(ExcelCell cell, int height)
		{
			var range = cell.MergedRange;
			var workSheet = cell.WorkSheet;

			for (var row = range.Bottom; row > range.Top; row--)
			{
				height -= workSheet.GetRowHeight(row);
			}

			SetRowHeight(workSheet, range.Top, height);
		}

		static void ExpandLastRow(ExcelCell cell, int height)
		{
			var range = cell.MergedRange;
			var workSheet = cell.WorkSheet;

			for (var row = range.Top; row < range.Bottom; row++)
			{
				height -= workSheet.GetRowHeight(row);
			}

			SetRowHeight(workSheet, range.Bottom, height);
		}

		static void SetRowHeight(ExcelWorkSheet workSheet, int row, int proposedHeight)
		{
			var height = Math.Min(proposedHeight, MaximumRowHeight);
			if (height > 0 && workSheet.GetRowHeight(row) < height)
			{
				workSheet.SetRowHeight(row, height);
			}
		}

		internal static int MaximumRowHeight => ExcelWorkSheet.MaxRowHeight;

		#region INonVisualisableValueProviderThatModifyDocumentLayout

		public ZString DocumentationForFormatting => AdditionalDocumentation;
		public Regex RegexToReplaceMacro => RegexToFindMacroAnyWhereInString;

		#endregion
	}

	enum RowToExpand
	{
		First,
		Last
	}
}
