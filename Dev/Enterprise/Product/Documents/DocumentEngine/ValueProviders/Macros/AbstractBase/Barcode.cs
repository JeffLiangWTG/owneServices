using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ReportErrorManagement;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal abstract class Barcode : ValueProvider
	{
		public override bool CanBeEvaluatedWithoutCurrentWorkSheet => false;

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter(FormattableString.Invariant($"<{CodeType}(\"{{InputText}}\",{{WidthInColumns}},{{HeightInRows}})>"),
				ResString.GetMultilingualString("DEC5F707-A578-4F8C-B8AB-4CD8A5BACDD6",
					@"Returns a {0} code image that contains the text with the following supplied specifications:
- {1}: Number of columns to span in Excel;
- {2}: Number of rows to span in Excel.",
					CodeType, "WidthInColumns", "HeightInRows"),
				new List<(string example, object expectedResult)> { ($"<{CodeType}(\"<BarCodeText>\", 2, 4)>", ExampleContentForDocumentation) }
			);
		}

		protected abstract string CodeType { get; }
		internal virtual string ExampleContentForDocumentation => "S00001234";

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "No specific exception can be specified from 3rd party libraries.")]
		protected override object GetReplacementCore(string macro, Report report)
		{
			object result;
			try
			{
				var matchedGroups = Regex.Match(macro).Groups;
				var inputText = matchedGroups["InputText"].Value;
				var widthInColumns = Convert.ToInt32(matchedGroups["WidthInColumns"].Value, CultureInfo.InvariantCulture);
				var heightInColumns = Convert.ToInt32(matchedGroups["HeightInRows"].Value, CultureInfo.InvariantCulture);
				var sizeInPixels = GetImageSize(report, widthInColumns, heightInColumns);

				var codeBitmap = BarcodeProcessor.CreateCode(inputText, new BarCodeCreationOptions
				{
					Width = sizeInPixels.Width,
					Height = sizeInPixels.Height
				});
				result = new ExcelImage(codeBitmap, sizeInPixels.Height, sizeInPixels.Width, CodeType, true, true);
			}
			catch (Exception e)
			{
				ReportError(e, CodeType, macro, report);
				result = null;
			}

			return result;
		}

		#region BarcodeProcessor

		/// <summary>
		/// Gets the processor for barcodes.
		/// </summary>
		protected internal abstract IBarcodeProcessor BarcodeProcessor { get; }

		#endregion

		#region Excel Dimensions

		/// <summary>
		/// Gets the size of image in pixels based on a given report and the row and column spans.
		/// </summary>
		/// <param name="report">The report.</param>
		/// <param name="widthInColumns">The width in column spans.</param>
		/// <param name="heightInRows">The height in row spans.</param>
		/// <returns>The size of image in pixels.</returns>
		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "These pixels are not for rendering on screen")]
		protected Size GetImageSize(Report report, int widthInColumns, int heightInRows)
		{
			// Get the width in excel.
			var aggregatedWidth = report.Renderer.OriginalColumnWidths.Skip(report.Renderer.CurrentColumn).Take(widthInColumns).Aggregate(new { TotalWidth = 0, Count = 0 }, (seed, columnWidth) => new { TotalWidth = seed.TotalWidth + columnWidth, Count = seed.Count + 1 });
			if (aggregatedWidth.Count < widthInColumns && !report.ContainsAnyCustomisation)
			{
				ErrorReporter.ReportOnce("BarcodeMacroSpecifiesWidthBeyondAvailableColumns",
					string.Format(CultureInfo.InvariantCulture,
@"Barcode macro specifies image width beyond available columns.
Report: {0}
Template: {1}
Available columns: {2}
Current column: {3}
Image width (in columns): {4}
Used width (in column): {5}",
					report.Name,
					report.Template != null ? report.Template.TemplateName : ZString.Empty,
					report.Renderer.OriginalColumnWidths.Count(),
					report.Renderer.CurrentColumn,
					widthInColumns,
					aggregatedWidth.Count));
			}
			int widthInExcel = aggregatedWidth.TotalWidth;
			// Get the height in excel.
			var currentWorksheet = report.WorkSheetCurrentlyBeingProcessed;
			int heightInExcel = 0;
			for (int i = report.Renderer.CurrentRow; i <= report.Renderer.CurrentRow + heightInRows - 1; i++)
			{
				heightInExcel += currentWorksheet.GetRowHeight(i);
			}
			// Convert the width and height in excel to pixels.
			var widthInPixels = (int)ZArchitecture.Core.Utilities.Round((decimal)(widthInExcel / ExcelMetrics.ColMult(currentWorksheet.ParentExcelInterface.Xls)), 0);
			var heightInPixels = (int)ZArchitecture.Core.Utilities.Round((decimal)(heightInExcel / FlxConsts.RowMult), 0);
			return new Size(widthInPixels, heightInPixels);
		}

		#endregion

		protected void ReportError(Exception ex, string barcode, string macro, Report report)
		{
#if DEBUG
			if (suppressErrorTemporarily)
			{
				return;
			}
#endif

			var message = ex.Message;
			message = Res.GetString("C39BA009-9D04-444E-A3A7-3AB39F6F1C71", "Error generating {0} code by macro [{1}] - [{2}]", barcode, macro, message.ShrinkToMaxLength(200));
			report.ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.Warning, ex));
		}

#if DEBUG
		public static IDisposable SuppressErrorTemporarily()
		{
			suppressErrorTemporarily = true;
			return new DisposableAction(delegate
			{
				suppressErrorTemporarily = false;
			});
		}

		[SuppressMessage("CargoWiseOne", "CW1021", Justification = "For debug and test only.")]
		static bool suppressErrorTemporarily;
#endif
	}
}
