using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Tools.SpellCheck.GUI;

namespace Enterprise.ZArchitecture.GUI.Tools.SpellCheck
{
	/// <summary>
	/// The GUI form that displays and corrects misspelled words, MS Word inspired.
	/// </summary>
	public partial class SpellCheckerForm : ZChildForm, ISpellCheckerForm
	{
		public SpellCheckerForm()
		{
			InitializeComponent();
			buttonIgnoreUndo.Text = CaptionUndoEdit;

			listBoxSuggestions.DoubleClick += new EventHandler(buttonChange_Click);
			richTextBoxContext.UndoEvent += new EventHandler(richTextBoxContext_UndoEvent);
			richTextBoxContext.KeyPress += RichTextBoxContext_KeyPress;

			Shown += new EventHandler(delegate
			{ isShown = true; OnHandleSpellingError(); });
		}

		static readonly Color colorSpellingError = Color.Red;

		public event ItemSpellCheckDelegate ItemSpellCheckCompleted;

		public ISpellCheckerFormSpellingError CurrentError { get; private set; }

		String CurrentControlText
		{
			get => currentControlText;
			set
			{
				if (currentControlText != value && shouldTrackingCurrentControlText)
				{
					currentControlTextChangeLogs.Add($"Changed: '{currentControlText}' => '{value}'" );
				}
				currentControlText = value;
			}
		}

		string currentControlText;

		readonly List<string> currentControlTextChangeLogs = new();
		int currentControlTextStartIndex;
		bool manualEdit;

		public void LoadSpellingError(ISpellCheckerFormSpellingError error)
		{
			LoadSpellingErrorSuggestions(error);
			LoadSpellingErrorContent(error);

			if (isShown)
			{
				OnHandleSpellingError();
			}
		}

		void LoadSpellingErrorContent(ISpellCheckerFormSpellingError error)
		{
			error.OnLoadSpellingErrorContent();

			CurrentError = error;
			ExtractContext();
			richTextBoxContext.StartChangingContent();
			SetManualEditMode(false);
			richTextBoxContext.Text = CurrentControlText;
			ResetSpellingErrorTextBox();
			HighlightSpellingErrorTextBoxContent();
			richTextBoxContext.Select(WordIndex + Word.Length, 0);
			richTextBoxContext.EndChangingContent();
		}

		void ExtractContext()
		{
			using (EnableTrackingCurrentControlText())
			{
				var i = 0;
				CurrentControlText = CurrentError.Text;
				wordIndex = CurrentError.SpellingError.WordIndex;
				try
				{
					i = LineBreakIndexAfter(CurrentControlText, wordIndex);
				}
				catch (ArgumentOutOfRangeException e)
				{
					ErrorReporter.Instance.Report("SpellCheckerForm_" + Word, FormattableString.Invariant($"The current error: '{CurrentControlText}' with spelling error: '{CurrentError.SpellingError.Word}' has an out of range word index: {wordIndex}."), e); // Silent Developer error only
				}

				if (i >= 0)
				{
					CurrentControlText = CurrentControlText.Substring(0, i);
				}
				var firstCalculatedIndex = i;

				i = LineBreakIndexBefore(CurrentControlText);
				if (i >= 0)
				{
					if (wordIndex < i)
					{
						var message = FormattableString.Invariant(
							@$"The current error: '{CurrentControlText}' with spelling error: '{CurrentError.SpellingError.Word}' has an out of range word index: {wordIndex} and original input Text: '{CurrentError.Text}'. 
firstCalculatedIndex:{firstCalculatedIndex}.
concurrentCurrentControlTextModification: {concurrentCurrentControlTextModification}.
CurrentControlTextChangeLogs: {string.Join("\r\n", currentControlTextChangeLogs)}");
						ErrorReporter.ReportOnce($"SpellCheckerForm_{Word}_{wordIndex}", message); // Silent Developer error only
					}

					CurrentControlText = CurrentControlText.Substring(i);
					wordIndex -= i;
				}
				currentControlTextStartIndex = i == -1 ? 0 : i;
			}
		}

		bool shouldTrackingCurrentControlText;
		bool concurrentCurrentControlTextModification;

		DisposableAction EnableTrackingCurrentControlText()
		{
			if (shouldTrackingCurrentControlText)
			{
				concurrentCurrentControlTextModification = true;
			}

			shouldTrackingCurrentControlText = true;
			return new DisposableAction(() => shouldTrackingCurrentControlText = false);
		}

		int LineBreakIndexAfter(String str, int afterPosition)
		{
			var nPosition = str.IndexOf('\n', afterPosition);
			var rPosition = str.IndexOf('\r', afterPosition);
			return rPosition >= 0 && rPosition < nPosition ? rPosition : nPosition;
		}

		int LineBreakIndexBefore(String str)
		{
			var nPosition = str.LastIndexOf('\n');
			var rPosition = str.LastIndexOf('\r');
			var position = nPosition > rPosition ? nPosition : rPosition;
			if (position >= 0)
			{
				position++;
			}

			return position;
		}

		void HighlightSpellingErrorTextBoxContent()
		{
			richTextBoxContext.StartChangingContent();
			richTextBoxContext.Select(WordIndex, Word.Length);
			richTextBoxContext.SelectionFont = new Font(richTextBoxContext.Font, FontStyle.Bold);
			richTextBoxContext.SelectionColor = colorSpellingError;
			richTextBoxContext.EndChangingContent();
			richTextBoxContext.ScrollToCaret();
		}

		void ResetSpellingErrorTextBox()
		{
			ResetSpellingErrorTextBox(true);
		}

		void ResetSpellingErrorTextBox(bool font)
		{
			richTextBoxContext.StartChangingContent();
			var selectionStart = richTextBoxContext.SelectionStart;
			var selectionLength = richTextBoxContext.SelectionLength;
			richTextBoxContext.SelectAll();
			if (font)
			{
				richTextBoxContext.SelectionFont = richTextBoxContext.Font;
			}
			richTextBoxContext.SelectionColor = richTextBoxContext.ForeColor;
			richTextBoxContext.Select(selectionStart, selectionLength);
			richTextBoxContext.EndChangingContent();
		}

		void LoadSpellingErrorSuggestions(ISpellCheckerFormSpellingError error)
		{
			listBoxSuggestions.Items.Clear();
			foreach (var suggestion in error.SpellingError.Suggestions)
			{
				listBoxSuggestions.Items.Add(suggestion);
			}

			if (listBoxSuggestions.Items.Count > 0)
			{
				listBoxSuggestions.SelectedIndex = 0;
			}
		}

		String Word
		{
			get { return CurrentError.SpellingError.Word; }
		}

		int WordIndex
		{
			get { return wordIndex; }
		}
		int wordIndex;

		string CaptionUndoEdit => Res.GetString("{516ED69E-EAD0-4030-99CD-D1AA945E6B3A}", "&Undo Edit");
		string CaptionIgnoreOnce => Res.GetString("19d4be0e-ee44-4986-b0de-95101da7f75b", "&Ignore Once");

		void SetManualEditMode(bool manual)
		{
			manualEdit = manual;
			buttonChange.Enabled = manual || listBoxSuggestions.Items.Count > 0;
			buttonChangeAll.Enabled = !manual && listBoxSuggestions.Items.Count > 0;
			listBoxSuggestions.Enabled = !manual;
			buttonIgnoreAll.Enabled = !manual;
			buttonIgnoreUndo.Text = manual ? CaptionUndoEdit : CaptionIgnoreOnce;
			if (manual)
			{
				ResetSpellingErrorTextBox(false);
			}
		}

		void OnItemSpellCheckCompleted(SpellCheckerFormResult result)
		{
			OnItemSpellCheckCompleted(result, null);
		}

		void OnItemSpellCheckCompleted(SpellCheckerFormResult result, string value, string errorParagraph = "", int errorParagraphIndex = -1)
		{
			CurrentError.OnItemSpellCheckCompleted();

			if (ItemSpellCheckCompleted != null)
			{
				var error = ItemSpellCheckCompleted(CurrentError, result, value, errorParagraph, errorParagraphIndex);
				if (error == null)
				{
					CurrentError = null;
					Close();
				}
				else
				{
					LoadSpellingError(error);
				}
			}
		}

		void buttonCancel_Click(object sender, EventArgs e)
		{
			OnItemSpellCheckCompleted(SpellCheckerFormResult.Cancel);
		}

		void buttonIgnoreUndo_Click(object sender, EventArgs e)
		{
			if (manualEdit)
			{
				LoadSpellingError(CurrentError);
			}
			else
			{
				OnItemSpellCheckCompleted(SpellCheckerFormResult.Ignore);
			}
		}

		void richTextBoxContext_UndoEvent(object sender, EventArgs e)
		{
			if (manualEdit)
			{
				LoadSpellingError(CurrentError);
			}
		}

		void buttonIgnoreAll_Click(object sender, EventArgs e)
		{
			OnItemSpellCheckCompleted(SpellCheckerFormResult.IgnoreAll);
		}

		void buttonAddToDictionary_Click(object sender, EventArgs e)
		{
			WordDictionaryManager.Instance.AddWord(CurrentError.SpellingError.Word);
			OnItemSpellCheckCompleted(SpellCheckerFormResult.IgnoreAll);
		}

		void buttonChange_Click(object sender, EventArgs e)
		{
			//if there was a manual edit, re-start the spellchecker and find the first non-ignored error word; else replace with the selected suggestion
			OnItemSpellCheckCompleted(manualEdit ? SpellCheckerFormResult.ChangeManual : SpellCheckerFormResult.Change, manualEdit ? richTextBoxContext.Text : listBoxSuggestions.SelectedItem.ToString(), CurrentControlText, currentControlTextStartIndex);
		}

		void buttonChangeAll_Click(object sender, EventArgs e)
		{
			OnItemSpellCheckCompleted(SpellCheckerFormResult.ChangeAll, listBoxSuggestions.SelectedItem.ToString());
		}

		void richTextBoxContext_TextChanged(object sender, EventArgs e)
		{
			if (!manualEdit && richTextBoxContext.Text != CurrentControlText)
			{
				SetManualEditMode(true);
			}
		}

		void RichTextBoxContext_KeyPress(object sender, KeyPressEventArgs e)
		{
#if !WINZOR
			var isInsertModeOn = IsKeyLocked(Keys.Insert);
			var isAtEndOfWord = richTextBoxContext.SelectionStart < richTextBoxContext.Text.Length && !char.IsLetterOrDigit(richTextBoxContext.Text[richTextBoxContext.SelectionStart]);

			e.Handled = isInsertModeOn && isAtEndOfWord;
#endif
		}

		bool isShown;

		void OnHandleSpellingError()
		{
			CargoWise.Tools.SpellCheck.GUI.SpellCheckerForm.OnHandleSpellingError(this);
		}
	}
}
