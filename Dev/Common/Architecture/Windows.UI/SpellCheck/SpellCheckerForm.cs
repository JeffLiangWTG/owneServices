using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WTG.SpellCheck;
using Res = CargoWise.Windows.UI.Res;

namespace CargoWise.Tools.SpellCheck.GUI
{
	public interface ISpellCheckerFormSpellingError
	{
		ISpellingErrorWithSuggestions SpellingError { get; }
		string Text { get; }
		void OnLoadSpellingErrorContent();
		void OnItemSpellCheckCompleted();
	}

	public enum SpellCheckerFormResult
	{
		Ignore = 1,
		IgnoreAll = 2,
		Change = 3,
		ChangeAll = 4,
		ChangeManual = 5,
		ChangeAllManual = 6,
		Cancel = 7
	}

	/// <summary>
	/// Handles the completion of spell checking an item
	/// </summary>
	/// <returns>The next spelling error to show, or null if the spell check is complete</returns>
	public delegate ISpellCheckerFormSpellingError ItemSpellCheckDelegate(ISpellCheckerFormSpellingError error, SpellCheckerFormResult result, string value, string errorParagraph = "", int errorParagraphIndex = -1);

	public interface ISpellCheckerForm : IDisposable
	{
		event ItemSpellCheckDelegate ItemSpellCheckCompleted;
		void LoadSpellingError(ISpellCheckerFormSpellingError error);
		DialogResult ShowDialog(IWin32Window owner);
		ISpellCheckerFormSpellingError CurrentError { get; }
	}

	/// <summary>
	/// The GUI form that displays and corrects misspelled words, MS Word inspired.
	/// </summary>
	public partial class SpellCheckerForm : Form, ISpellCheckerForm
	{
		public SpellCheckerForm()
		{
			InitializeComponent();
			Localize();
			listBoxSuggestions.DoubleClick += new EventHandler(ButtonChange_Click);
			richTextBoxContext.UndoEvent += new EventHandler(RichTextBoxContext_UndoEvent);

			Shown += new EventHandler(delegate
			{
				isShown = true;
				OnHandleSpellingError(this);
			});
		}

		static readonly Color colorSpellingError = Color.Red;
		int currentControlTextStartIndex;

		internal static class Captions
		{
			internal static string FormTitle => Res.GetString("{F5BAC3B9-8F8A-4a7b-946A-2AFA7740EE19}", "Spelling");
			internal static string NotInDictionary => Res.GetString("{A7FB5AA4-2BB7-47d3-A0C7-90B85A8DFE99}", "Not in Dictionary&:");
			internal static string Suggestions => Res.GetString("{E3668C18-E18E-493d-B837-B7DF7339E63B}", "Suggestio&ns:");
			internal static string IgnoreOnce => Res.GetString("{D3EF2124-1C39-464d-9A80-B5FC888BE19B}", "&Ignore Once");
			internal static string UndoEdit => Res.GetString("{D7F2AD84-68ED-4746-802D-7D17D1A499B9}", "&Undo Edit");
			internal static string IgnoreAll => Res.GetString("{C7D81258-310F-43fb-AC16-8E66B1CA2395}", "I&gnore All");
			internal static string Change => Res.GetString("{FFC36A8C-10DC-446a-A889-2C2D6DCD7AA3}", "&Change");
			internal static string ChangeAll => Res.GetString("{86BC1DBE-5218-4862-8223-528DEA375D0A}", "Change Al&l");
			internal static string Cancel => Res.GetString("{DEA258E7-0919-44a1-AB50-072D5F9B565E}", "Cancel");
		}

		public event ItemSpellCheckDelegate ItemSpellCheckCompleted;

		public ISpellCheckerFormSpellingError CurrentError { get; private set; }
		string currentControlText;
		bool manualEdit;

		void Localize()
		{
			this.Text = Captions.FormTitle;
			labelSuggestions.Text = Captions.Suggestions;
			labelNotInDictionary.Text = Captions.NotInDictionary;
			buttonIgnoreUndo.Text = Captions.IgnoreOnce;
			buttonIgnoreAll.Text = Captions.IgnoreAll;
			buttonChange.Text = Captions.Change;
			buttonChangeAll.Text = Captions.ChangeAll;
			buttonCancel.Text = Captions.Cancel;
		}

		public void LoadSpellingError(ISpellCheckerFormSpellingError error)
		{
			LoadSpellingErrorContent(error);
			LoadSpellingErrorSuggestions(error);

			if (isShown)
			{
				OnHandleSpellingError(this);
			}
		}

		void LoadSpellingErrorContent(ISpellCheckerFormSpellingError error)
		{
			error.OnLoadSpellingErrorContent();

			CurrentError = error;
			ExtractContext();
			richTextBoxContext.StartChangingContent();
			SetManualEditMode(false);
			richTextBoxContext.Text = currentControlText;
			ResetSpellingErrorTextBox();
			HighlightSpellingErrorTextBoxContent();
			richTextBoxContext.Select(WordIndex + Word.Length, 0);
			richTextBoxContext.EndChangingContent();
		}

		void ExtractContext()
		{
			currentControlText = CurrentError.Text;
			wordIndex = CurrentError.SpellingError.WordIndex;
			var i = LineBreakIndexAfter(currentControlText, wordIndex);
			if (i >= 0)
			{
				currentControlText = currentControlText.Substring(0, i);
			}

			i = LineBreakIndexBefore(currentControlText, wordIndex);
			if (i >= 0)
			{
				currentControlText = currentControlText.Substring(i);
				wordIndex -= i;
			}
			currentControlTextStartIndex = i == -1 ? 0 : i;
		}

		int LineBreakIndexAfter(string str, int afterPosition)
		{
			var nPosition = str.IndexOf('\n', afterPosition);
			var rPosition = str.IndexOf('\r', afterPosition);
			return rPosition >= 0 && rPosition < nPosition ? rPosition : nPosition;
		}

		int LineBreakIndexBefore(string str, int beforePosition)
		{
			var headStr = str.Substring(0, beforePosition);
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
			else
			{
				buttonChange.Enabled = false;
				buttonChangeAll.Enabled = false;
			}
		}

		string Word
		{
			get { return CurrentError.SpellingError.Word; }
		}

		int WordIndex
		{
			get { return wordIndex; }
		}
		int wordIndex;

		void SetManualEditMode(bool manual)
		{
			if (manual != manualEdit)
			{
				manualEdit = manual;
				buttonChange.Enabled = manual || listBoxSuggestions.Items.Count > 0;
				buttonChangeAll.Enabled = !manual && listBoxSuggestions.Items.Count > 0;
				listBoxSuggestions.Enabled = !manual;
				buttonIgnoreAll.Enabled = !manual;
				buttonIgnoreUndo.Text = manual ? Captions.UndoEdit : Captions.IgnoreOnce;
				if (manual)
				{
					ResetSpellingErrorTextBox(false);
				}
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

		void ButtonCancel_Click(object sender, EventArgs e)
		{
			OnItemSpellCheckCompleted(SpellCheckerFormResult.Cancel);
		}

		void ButtonIgnoreUndo_Click(object sender, EventArgs e)
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

		void RichTextBoxContext_UndoEvent(object sender, EventArgs e)
		{
			if (manualEdit)
			{
				LoadSpellingError(CurrentError);
			}
		}

		void ButtonIgnoreAll_Click(object sender, EventArgs e)
		{
			OnItemSpellCheckCompleted(SpellCheckerFormResult.IgnoreAll);
		}

		void ButtonChange_Click(object sender, EventArgs e)
		{
			OnItemSpellCheckCompleted(manualEdit ? SpellCheckerFormResult.ChangeManual : SpellCheckerFormResult.Change, manualEdit ? richTextBoxContext.Text : listBoxSuggestions.SelectedItem.ToString(), currentControlText, currentControlTextStartIndex);
		}

		void ButtonChangeAll_Click(object sender, EventArgs e)
		{
			OnItemSpellCheckCompleted(SpellCheckerFormResult.ChangeAll, listBoxSuggestions.SelectedItem.ToString());
		}

		void RichTextBoxContext_TextChanged(object sender, EventArgs e)
		{
			if (!manualEdit && richTextBoxContext.Text != currentControlText)
			{
				SetManualEditMode(true);
			}
		}

		bool isShown;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void OnHandleSpellingError(ISpellCheckerForm sender)
		{
			handleSpellingError?.Invoke(sender, EventArgs.Empty);
		}
		internal static event EventHandler HandleSpellingError
		{
			add { handleSpellingError += value; }
			remove { handleSpellingError -= value; }
		}
		[ThreadStatic]
		static EventHandler handleSpellingError;
	}
}
