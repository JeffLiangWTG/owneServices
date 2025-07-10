using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class ShrinkToFit : ValueProvider, INonVisualisableValueProvider
	{
		public const int XlsWidthConversionFactor = 136;
		public const int XlsHeightConversionFactor = 66;
		public const float DefaultMinimumFontSize = 8f;
		public const float MinimumPossibleFontSize = 1f;

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<ShrinkToFit[({MinimumFontSize})]>",
				ResString.GetMultilingualString("5b96615d-4c15-45b2-978c-21ec8e83fc0a",
				@"Used as a prefix to a subsequent macro. If the value inserted by the subsequent macro does not fit in the cell in the template, the font size will be progressively reduced until it fits, to the minimum of {0}. If no minimum font size is specified, the text will be reduced to a minimum of 8 point.
Note: {1} Will be ignored if {2} is also specified in the same cell or the cell Wrap Text property is not checked.",
				"{MinimumFontSize}", "<ShrinkToFit>", "<AutoHeight>"),
				new List<(string example, object expectedResult)> { ("<ShrinkToFit><ShipmentNumber>", null), ("<ShrinkToFit(4.5)><ShipmentNumber>", null) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return string.Empty;
		}

		internal static void Shrink(string cellContent, float minimumFontSize, Report report, float textHeightMultiplier = 1)
		{
			var workSheet = report.WorkSheetCurrentlyBeingProcessed;
			if (workSheet != null)
			{
				var renderer = report.Renderer;
				var excelInterface = workSheet.ParentExcelInterface;
				var row = renderer.CurrentRow;
				var column = renderer.CurrentColumn;

				var format = workSheet.GetCellFormat(row, column);
				using (var font = format.GetFont())
				{
					var columnWidth = workSheet.GetCellWidth(row, column);
					var rowHeight = workSheet.GetCellHeight(row, column);

					if (format.WrapText)
					{
						var size = ControlDpiScalingHelper.NewScaledSize(columnWidth, rowHeight, false);

						using (var shrinkedFont = Shrink(font, size, minimumFontSize, cellContent, excelInterface, report, textHeightMultiplier))
						{
							format.FontName = shrinkedFont.Name;
							format.FontSize = shrinkedFont.Size;
							format.FontStyle = shrinkedFont.Style;
						}
					}
					else
					{
						using (var shrinkedFont = Shrink(font, columnWidth, minimumFontSize, cellContent, excelInterface, report, textHeightMultiplier))
						{
							format.FontName = shrinkedFont.Name;
							format.FontSize = shrinkedFont.Size;
							format.FontStyle = shrinkedFont.Style;
						}
					}

					workSheet.SetCellFormat(row, column, format);
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Not working in pixels")]
		static Font Shrink(Font startingFont, int widthInExcelInternalUnits, float minimumFontSize, string text, ExcelInterface excelInterface, Report report, float textHeightMultiplier)
		{
			return Shrink(startingFont, new Size(widthInExcelInternalUnits, 0), minimumFontSize, text, excelInterface, report, textHeightMultiplier);
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Not working in pixels")]
		static Font Shrink(Font startingFont, Size proposedSize, float minimumFontSize, string text, ExcelInterface excelInterface, Report report, float textHeightMultiplier)
		{
			var calculator = new TextSizeCalculator(startingFont);
			var columnWidthMultiplier = excelInterface.GetColumnWidthMultiplier(GraphicsUnit.Display);
			var rowWidthMultiplier = excelInterface.GetRowHeightMultiplier(GraphicsUnit.Display);

			var width = Convert.ToInt32(Math.Floor(proposedSize.Width / columnWidthMultiplier));
			var height = Convert.ToInt32(Math.Floor(proposedSize.Height / rowWidthMultiplier));
			// Excel adds left / right padding to cells which reduces the width available for the text
			var availableWidthForText = width - 2 * ExcelCell.ExcelCellTextPaddingInDeviceIndependentPixels;
			var sizeInDisplayUnits = new Size(availableWidthForText, height);

			var result = (Font)startingFont.Clone();

			while (result.SizeInPoints >= minimumFontSize)
			{
				Size textSize;

				if (sizeInDisplayUnits.Height > 0)
				{
					textSize = calculator.GetTextSize(text, result, GraphicsUnit.Display, sizeInDisplayUnits.Width);
				}
				else
				{
					textSize = calculator.GetTextSize(text, result, GraphicsUnit.Display);
				}

				if ((sizeInDisplayUnits.Width > 0 && textSize.Width > sizeInDisplayUnits.Width) ||
					(sizeInDisplayUnits.Height > 0 && textSize.Height > sizeInDisplayUnits.Height * textHeightMultiplier))
				{
					using (var previousFont = result)
					{
						var proposedNewSize = previousFont.SizeInPoints - 0.5f;
						if (proposedNewSize < minimumFontSize)
						{
							proposedNewSize = minimumFontSize;
						}
						if (proposedNewSize == previousFont.SizeInPoints)
						{
							break;
						}
						result = ReSize(previousFont, proposedNewSize);
					}
				}
				else
				{
					break;
				}
			}

			return result;
		}

		internal static float GetMinimumFontSize(string macro)
		{
			var result = DefaultMinimumFontSize;
			var minimumFontSizeParameter = RegexToFindMacroAnyWhereInString.Match(macro).Groups["MinimumFontSize"].Value;
			/*safe;
			 * If groupname is not the name of a capturing group in the collection,
			 * or if groupname is the name of a capturing group that has not been matched in the input string,
			 * the method returns a Group object whose Group.Success property is false and whose Group.Value property is String.Empty.
			*/

			float.TryParse(minimumFontSizeParameter, out result);

			return result > 0 ? result : MinimumPossibleFontSize;
		}

		static Font ReSize(Font inFont, float newEmSizeInPoints)
		{
			return new Font(inFont.Name, newEmSizeInPoints, inFont.Style, GraphicsUnit.Point, inFont.GdiCharSet);
		}

		public override Passes PassToStartReplacingOn
		{
			get { return Passes.SecondPass; }
		}

		public override Regex Regex => regex;
		static readonly Regex regex = new Regex("^" + RegexPattern + "$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		/// <summary>
		/// This is the regex really used to interpret this line in the class Enterprise.DocumentEngine.Areas.Area 
		/// and by the visualiser to remove the macro text before visualising.
		/// </summary>
		internal static readonly Regex RegexToFindMacroAnyWhereInString = new Regex(RegexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Regular Expression")]
		public const string RegexPattern = @"<[\s]*Shrink[\s]*To[\s]*Fit[\s]*(|\([\s]*(?<MinimumFontSize>[0-9.]+)[\s]*\))[\s]*>";

		#region INonVisualisableValueProvider

		public Regex RegexToReplaceMacro => RegexToFindMacroAnyWhereInString;

		#endregion
	}
}
