using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business.SpellCheck;
using WTG.SpellCheck;

namespace Enterprise.ZArchitecture.GUI.Tools
{
	class RichTextSpellChecker : SpellChecker
	{
		/// <summary>
		/// Please use SpellChecker.InitialiseSpellcheck to create a spellcheck instance
		/// </summary>
		internal RichTextSpellChecker(ISpellChecker spellChecker, ZRichTextBox richText, string name, StringsToExclude stringsToExclude = null)
			: this(spellChecker, GetRichTextBox(richText), GetMenuStrip(richText, GetRichTextBox(richText)), name, stringsToExclude)
		{
			richText.BeforePopup += (o, e) => InitialiseSpellcheck(e.Form.Controls.OfType<ZRichTextBox>().Single(), name + "_Popup");
		}

		internal RichTextSpellChecker(ISpellChecker spellChecker, KRichTextBox richText, string name, StringsToExclude stringsToExclude = null)
			: this(spellChecker, richText, GetMenuStrip(richText), name, stringsToExclude)
		{
		}

		RichTextSpellChecker(ISpellChecker spellChecker, RichTextBox textbox, ContextMenuStrip menuStrip, string name, StringsToExclude stringsToExclude = null)
			: base(spellChecker, textbox, menuStrip, name, stringsToExclude)
		{
#if !WINZOR
			textbox.VScroll += RenderOnScroll;
			textbox.HScroll += RenderOnScroll;

			void RenderOnScroll(object sender, EventArgs e)
			{
				if (!calculatingSquiggles)
				{
					ForceRender();
				}
			}
#endif
		}

		static ContextMenuStrip GetMenuStrip(ZRichTextBox control, TextBoxBase textbox)
		{
			if (textbox.ContextMenuStrip == null)
			{
				control.contextMenuManager.InitializeContextMenu();
			}

			return textbox.ContextMenuStrip;
		}

		static ContextMenuStrip GetMenuStrip(KRichTextBox textbox)
			=> textbox.ContextMenuStrip ?? (textbox.ContextMenuStrip = new ContextMenuStrip());

		[return: DpiState(DpiState.ScaleY)]
		protected override int GetOffsetForBaseline(int charIndex)
		{
			var line = Textbox.GetLineFromCharIndex(charIndex);
			if (!lineHeightCache.TryGetValue(line, out var result))
			{
				lineHeightCache[line] = result = GetOffsetForBaselineCore(line);
			}

			return result;
		}

		[return: DpiState(DpiState.ScaleY)]
		int GetOffsetForBaselineCore(int line)
		{
			var lineTop = GetLineY(line);
			var nextLine = GetLineY(line + 1);

			if (nextLine > 0)
			{
				var delta = nextLine - lineTop;
				return (int)Math.Round(delta * 0.8); // Accuracy is irrelevant here, I'm all for speed
			}

			// We're on the last line / there is only one line. Gunna have to guess
			return GetOffsetForBaselineFromFont(textbox.Font);
		}

		[return: DpiState(DpiState.ScaleY)]
		int GetLineY(int lineNum)
			=> Textbox.GetPositionFromCharIndex(Textbox.GetFirstCharIndexFromLine(lineNum)).Y;

		protected override void RecalculateSquiggles()
		{
			lineHeightCache.Clear();

			var originalState = (Textbox.SelectionStart, Textbox.SelectionLength);
			Textbox.SuspendDrawing();

#if !WINZOR
			calculatingSquiggles = true;
#endif
			try
			{
				base.RecalculateSquiggles();
			}
			finally
			{
				if (Textbox.SelectionStart != originalState.SelectionStart)
				{
					Textbox.SelectionStart = originalState.SelectionStart;
				}

				if (Textbox.SelectionLength != originalState.SelectionLength)
				{
					Textbox.SelectionLength = originalState.SelectionLength;
				}

				Textbox.ResumeDrawing();
#if !WINZOR
				calculatingSquiggles = false;
#endif
			}
		}

		static RichTextBox GetRichTextBox(ZRichTextBox wrapper)
			=> wrapper.Controls.OfType<RichTextBox>().Single();

		RichTextBox Textbox => (RichTextBox)textbox;

#if !WINZOR
		bool calculatingSquiggles;
#endif
		readonly Dictionary<int, int> lineHeightCache = new Dictionary<int, int>();
	}
}
