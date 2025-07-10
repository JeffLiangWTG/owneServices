using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.ResourceStrings.GUI
{
	public class TranslationFeedbackDiffHighlighter
	{
		public TranslationFeedbackDiffHighlighter(ZGrid grid)
		{
			this.grid = grid;
			grid.AfterBind += new EventHandler(grid_AfterBind);
		}

		void grid_AfterBind(object sender, EventArgs e)
		{
			#if !WINZOR
			if (grid.Columns[StmTranslationFeedback.Schema.XT_OriginalTranslation].IsVisible && grid.Columns[StmTranslationFeedback.Schema.XT_SuggestedTranslation].IsVisible)
			{
				((ZTextBoxColumnStyle)grid.Columns[StmTranslationFeedback.Schema.XT_OriginalTranslation].ColumnStyle).PaintHighlights += new ZTextBoxColumnStyle.PaintHighlightsDelegate(OriginalTranslation_PaintHighlights);
				((ZTextBoxColumnStyle)grid.Columns[StmTranslationFeedback.Schema.XT_SuggestedTranslation].ColumnStyle).PaintHighlights += new ZTextBoxColumnStyle.PaintHighlightsDelegate(SuggestedTranslation_PaintHighlights);
			}
			#endif
		}

#if !WINZOR

		void OriginalTranslation_PaintHighlights(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, string cellText, StringFormat format, Font cellFont, Brush backBrush, Brush foreBrush, bool rightToLeft, bool useEllipsis)
		{
			var translationFeedback = source.List[rowNum] as StmTranslationFeedback;
			if (translationFeedback != null)
			{
				PaintHighlights(translationFeedback.TranslationDiff.x, translationFeedback.TranslationDiff.xChanged, g, bounds, cellText, format, cellFont);
			}
		}

		void SuggestedTranslation_PaintHighlights(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, string cellText, StringFormat format, Font cellFont, Brush backBrush, Brush foreBrush, bool rightToLeft, bool useEllipsis)
		{
			var translationFeedback = source.List[rowNum] as StmTranslationFeedback;
			if (translationFeedback != null)
			{
				PaintHighlights(translationFeedback.TranslationDiff.y, translationFeedback.TranslationDiff.yChanged, g, bounds, cellText, format, cellFont);
			}
		}

		void PaintHighlights(string[] parts, bool[] changed, Graphics g, Rectangle bounds, string cellText, StringFormat format, Font cellFont)
		{
			if (parts.Length > 32)
			{
				return;
			}

			CharacterRange[] ranges = new CharacterRange[parts.Length];
			int start = 0;
			for (int i = 0 ; i < parts.Length; i++)
			{
				ranges[i] = new CharacterRange(start, parts[i].Length);
				start += parts[i].Length;
			}

			format.SetMeasurableCharacterRanges(ranges);
			var regions = g.MeasureCharacterRanges(cellText, cellFont, bounds, format);
			for (int i = 0; i < regions.Length; i++)
			{
				if (changed[i])
				{
					g.FillRectangle(new SolidBrush(Color.FromArgb(128, Color.Yellow)), regions[i].GetBounds(g));
				}
			}
		}

#endif

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used for .Net Framework, this will be fixed when #if !WINZOR removed")]
		readonly ZGrid grid;
	}
}
