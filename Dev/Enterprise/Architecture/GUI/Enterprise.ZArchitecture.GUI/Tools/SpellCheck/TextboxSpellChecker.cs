using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business.SpellCheck;
using WTG.SpellCheck;

namespace Enterprise.ZArchitecture.GUI.Tools
{
	class TextboxSpellChecker : SpellChecker
	{
		/// <summary>
		/// Please use SpellChecker.InitialiseSpellcheck to create a spellcheck instance
		/// </summary>
		internal TextboxSpellChecker(ISpellChecker spellchecker, ZTextBox control, string name, StringsToExclude stringsToExclude = null)
			: base(spellchecker, control, GetMenuStrip(control), name, stringsToExclude)
		{
		}

		static ContextMenuStrip GetMenuStrip(ZTextBox control)
		{
			if (control.ContextMenuStrip == null)
			{
				control.contextMenuManager.InitializeContextMenu();
			}

			return control.ContextMenuStrip;
		}

		[return: DpiState(DpiState.ScaleY)]
		protected override int GetOffsetForBaseline(int charIndex) => GetOffsetForBaselineFromFont(textbox.Font);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used for calculating line height.")]
		protected override (int, int) CalculateVisibleIndexRange()
		{
			var (start, end) = base.CalculateVisibleIndexRange();
			var bottomOfLine = textbox.GetPositionFromCharIndex(end).Y + TextRenderer.MeasureText("Hy", textbox.Font).Height;

			if (bottomOfLine > textbox.ClientSize.Height)
			{
				end = textbox.GetFirstCharIndexFromLine(textbox.GetLineFromCharIndex(end)) - 1;
			}

			return (start, end);
		}
	}
}
