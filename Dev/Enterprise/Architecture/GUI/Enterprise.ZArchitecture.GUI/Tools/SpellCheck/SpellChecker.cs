using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Tools.SpellCheck;
using CargoWise.Tools.SpellCheck.GUI;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business.SpellCheck;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Tools.SpellCheck;
#if WINZOR
using Microsoft.JSInterop;
#endif
using WTG.SpellCheck;
using static CargoWise.Windows.UI.ControlDpiScalingHelper;

namespace Enterprise.ZArchitecture.GUI.Tools
{
	public abstract partial class SpellChecker
	{
		[return: DpiState(DpiState.ScaleY)]
		protected abstract int GetOffsetForBaseline(int charIndex);
		protected static int GetOffsetForBaselineFromFont(Font f) => (int)(TextRenderer.MeasureText("X", f).Height * 0.8);

		public static SpellChecker InitialiseSpellcheck(ZTextBox control, string name)
		{
			var stringsToExclude = new StringsToExclude(WordDictionaryManager.Instance.CurrentDictionary);
			return new TextboxSpellChecker(GetSpellChecker(stringsToExclude), control, name, stringsToExclude);
		}

		public static SpellChecker InitialiseSpellcheck(ZRichTextBox control, string name)
		{
			var stringsToExclude = new StringsToExclude(WordDictionaryManager.Instance.CurrentDictionary);
			return new RichTextSpellChecker(GetSpellChecker(stringsToExclude), control, name, stringsToExclude);
		}

		public static SpellChecker InitialiseSpellcheck(KRichTextBox control, string name)
		{
			var stringsToExclude = new StringsToExclude(WordDictionaryManager.Instance.CurrentDictionary);
			return new RichTextSpellChecker(GetSpellChecker(stringsToExclude), control, name, stringsToExclude);
		}

		internal static ISpellChecker GetSpellChecker(StringsToExclude stringsToExclude = null)
			=> EnterpriseResourceStringSpellChecker.GetGeneralSpellChecker(SpellCheckCulture, (ICollection<string>)stringsToExclude ?? WordDictionaryManager.Instance.CurrentDictionary);

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected SpellChecker(ISpellChecker spellChecker, TextBoxBase textbox, ContextMenuStrip menuStrip, string name, StringsToExclude stringsToExclude = null)
		{
			this.textbox = Argument.NotNull(textbox, nameof(textbox));
			this.spellChecker = Argument.NotNull(spellChecker, nameof(spellChecker));
			this.menuStrip = Argument.NotNull(menuStrip, nameof(menuStrip));
			this.stringsToExclude = stringsToExclude;

			KeyForSpellChecker = "SpellCheckerKey|" + Argument.NotNull(name, nameof(name));
			SpellCheckerStatusSingleton.Instance.AddSpellCheckerInstanceIntoDictionary(KeyForSpellChecker, this);

#if WINZOR
			dotNetObjectReference = DotNetObjectReference.Create(this);
#endif

			if (SpellCheckerEnabled)
			{
				HookEvents();
			}

			AddSpellCheckMenuItems();

			textbox.Disposed += (sender, args) => SpellCheckerStatusSingleton.Instance.RemoveSpellCheckerInstanceFromDictionary(KeyForSpellChecker, this);
		}

		public void UpdateWordsToIgnore(IEnumerable<string> knownNames)
		{
			stringsToExclude?.UpdateWordsToIgnore(knownNames);
			spellChecker = GetSpellChecker(stringsToExclude);
		}
		readonly StringsToExclude stringsToExclude;

		void HookEvents()
		{
			textbox.TextChanged += TextOrVisibleChanged_Event;
			textbox.VisibleChanged += TextOrVisibleChanged_Event;
			textbox.MouseDown += RightClick_Event;
			textbox.Paint += Paint_Event;
			textbox.MouseUp += MouseUpOrDown_Event;
			textbox.MouseDown += MouseUpOrDown_Event;
			textbox.Invalidated += Invalidated_Event;
			textbox.ClientSizeChanged += ClientSizeChanged_Event;
#if WINZOR
			if (textbox is TextBox)
			{
				textbox.RegisterAfterRenderAction(async () => await Interop?.EnableSpellCheckAsync(textbox.ElementReference, dotNetObjectReference));
			}
			else if (textbox is RichTextBox richTextBox)
			{
				richTextBox.ExtendedAfterRenderActions.Add(async (firstRender) =>
				{
					if (firstRender)
					{
						await Interop?.EnableSpellCheckAsync(textbox.ElementReference, dotNetObjectReference);
					}
				});
			}
#endif
		}

		void UnhookEvents()
		{
			textbox.TextChanged -= TextOrVisibleChanged_Event;
			textbox.VisibleChanged -= TextOrVisibleChanged_Event;
			textbox.MouseDown -= RightClick_Event;
			textbox.Paint -= Paint_Event;
			textbox.MouseUp -= MouseUpOrDown_Event;
			textbox.MouseDown -= MouseUpOrDown_Event;
			textbox.Invalidated -= Invalidated_Event;
			textbox.ClientSizeChanged -= ClientSizeChanged_Event;
#if WINZOR
			textbox.InvokeRenderDispatcher(async () => await Interop?.DisableSpellCheckAsync(textbox.ElementReference));
#endif
		}

		void MouseUpOrDown_Event(object sender, MouseEventArgs e)
		{
			textbox.Invalidate();
		}

		void Paint_Event(object sender, PaintEventArgs e)
		{
#if !WINZOR
			PaintSquiggles(e.Graphics);
#endif
		}

		void TextOrVisibleChanged_Event(object sender, EventArgs e)
		{
#if !WINZOR
			FullyRecalculate();
#endif
		}

		void Invalidated_Event(object sender, InvalidateEventArgs e)
		{
			if (!alreadyInvalidating)
			{
				alreadyInvalidating = true;
				ForceRender(e.InvalidRect);
				alreadyInvalidating = false;
			}
		}

		void ClientSizeChanged_Event(object sender, EventArgs e)
		{
			textbox.Invalidate();
		}

		void FullyRecalculate()
		{
			spellingErrors = spellChecker.CheckSpelling(textbox.Text);
			squiggles = null;

			textbox.Invalidate();
		}

		protected virtual void RecalculateSquiggles()
		{
			var (start, end) = CalculateVisibleIndexRange();
			squiggles = spellingErrors?
				.Select(error => (Start: error.WordIndex, End: error.WordIndex + error.Word.Length))
				.Where(indicies => start <= indicies.Start && indicies.End <= end)
				.SelectMany(indicies => FindMultilineTextLocation(indicies.Start, indicies.End))
				.ToList();
		}

		internal void ForceRender(Rectangle? bounds = null)
		{
#if !WINZOR
			RecalculateSquiggles();

			UserIdleWorker.QueueWorkItem(textbox, 0, UserIdleWorkItemOptions.AllowDuringEdit, new Action(() =>
			{
				using (var g = textbox.CreateGraphics())
				{
					g.SetClip(bounds ?? textbox.ClientRectangle);

					PaintSquiggles(g);
				}
			}));
#endif
		}

		public static ControlSpellCheckerResult CheckSpelling(Control control, StringsToExclude stringsToExclude = null)
		{
			var spellChecker = new ControlSpellChecker(GetSpellChecker(stringsToExclude), new[] { control }, CreateSpellCheckForm);
			return spellChecker.CheckSpelling();
		}

		public ControlSpellCheckerResult CheckSpelling()
		{
			var spellChecker = new ControlSpellChecker(GetSpellChecker(stringsToExclude), new[] { textbox }, CreateSpellCheckForm);
			return spellChecker.CheckSpelling();
		}

#if !WINZOR
		void PaintSquiggles(Graphics g)
		{
			if (squiggles != null)
			{
				foreach (var squiggle in squiggles)
				{
					DrawSquiggle(g, squiggle.Item1, squiggle.Item2);
				}
			}
		}
#endif

		IEnumerable<Tuple<Point, Point>> FindMultilineTextLocation(int firstIndex, int lastIndex)
		{
			var firstLine = textbox.GetLineFromCharIndex(firstIndex);
			var lastLine = textbox.GetLineFromCharIndex(lastIndex);

			var startIndex = firstIndex;
			for (var lineNum = firstLine; lineNum < lastLine; lineNum++)
			{
				var lastCharIndex = textbox.GetFirstCharIndexFromLine(lineNum + 1) - 1;
				yield return BringToTextBaseline(
					textbox.GetPositionFromCharIndex(startIndex),
					textbox.GetPositionFromCharIndex(lastCharIndex),
					GetOffsetForBaseline(startIndex)
				);

				startIndex = lastCharIndex + 1;
			}

			yield return BringToTextBaseline(
				textbox.GetPositionFromCharIndex(startIndex),
				textbox.GetPositionFromCharIndex(lastIndex),
				GetOffsetForBaseline(lastIndex)
			);
		}

		protected virtual (int, int) CalculateVisibleIndexRange()
		{
			var lastPoint = new Point(textbox.ClientSize.Width - ScaleToCurrentDpiX(2), textbox.ClientSize.Height - ScaleToCurrentDpiY(2));
			var firstPoint = NewScaledPoint(2, 2);

			return (textbox.GetCharIndexFromPosition(firstPoint), textbox.GetCharIndexFromPosition(lastPoint));
		}

		Tuple<Point, Point> BringToTextBaseline(Point startPos, Point endPos, int baselineOffset)
			=> Tuple.Create(
				NewScaledPoint(startPos.X, startPos.Y + baselineOffset, false),
				NewScaledPoint(endPos.X, endPos.Y + baselineOffset, false));

		protected void RightClick_Event(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				RemovePrevSuggestions();

				var charPos = textbox.GetCharIndexFromPosition(e.Location);
#if !WINZOR
				var spellingError = spellingErrors?.FirstOrDefault(s => s.WordIndex < charPos && charPos < (s.WordIndex + s.Word.Length));
#else
				var spellingError = spellingErrors?.FirstOrDefault(s => s.WordIndex <= charPos && charPos < (s.WordIndex + s.Word.Length));
#endif
				if (spellingError != null)
				{
					AddToDictionaryMenuItem.Tag = spellingError.Word;
					AddToDictionaryMenuItem.Enabled = true;

					AddNewSuggestions(spellingError);
				}
				else
				{
					AddToDictionaryMenuItem.Enabled = false;
				}
			}
		}

		protected virtual void AddSpellCheckMenuItems()
		{
			var seperator = new ToolStripSeparator { Name = MenuItemKeys.SpellCheckSeperator };
			var checkSpelling = new ZToolStripMenuItem(
				Res.GetString("SpellCheck|6e892f85-5b7a-426a-bb45-75a204aaf4a3", "Check Spelling"),
				CheckSpelling_Click);
			checkSpelling.Name = MenuItemKeys.CheckSpelling;

			menuStrip.Items.AddRange(new ToolStripItem[] { seperator, checkSpelling });

			if (WordDictionaryManager.Instance.CurrentDictionary != null)
			{
				var showDictionary = new ZToolStripMenuItem(
					Res.GetString("SpellCheck|88a0636b-4a42-42b2-89da-5c2dcb050751", "Show My Dictionary"),
					(o, ev) => ShowUserDictionary());
				showDictionary.Name = MenuItemKeys.ShowDictionary;

				menuStrip.Items.AddRange(SpellCheckerEnabled ? new ToolStripItem[] { AddToDictionaryMenuItem, showDictionary } : new ToolStripItem[] { showDictionary });
			}

			menuStrip.Items.Add(SpellCheckerEnabled ? DisableSpellCheckMenuItem : EnableSpellCheckMenuItem);
		}

		void CheckSpelling_Click(object sender, EventArgs e)
		{
			if (CheckSpelling(textbox, stringsToExclude) == ControlSpellCheckerResult.NoErrors)
			{
				Globals.Message.Show(Res.GetString("ddef0bfb-8c52-4eab-a339-0dfe94c4df60", "No errors found."));
			}

			FullyRecalculate();
		}

		void DisableSpellCheck_Click(object sender, EventArgs e)
		{
			SpellCheckerStatusSingleton.Instance.SyncroniseSpellCheckerStatus(KeyForSpellChecker, false);
		}

		internal void DisableSpellCheck()
		{
			var indexOfDisableDictionary = menuStrip.Items.IndexOfKey(MenuItemKeys.DisableSpellCheck);
			menuStrip.Items.RemoveAt(indexOfDisableDictionary);
			menuStrip.Items.Insert(indexOfDisableDictionary, EnableSpellCheckMenuItem);
			menuStrip.Items.RemoveByKey(MenuItemKeys.AddToDictionary);

			UnhookEvents();
			RemovePrevSuggestions();
			textbox.Invalidate();
		}

		void EnableSpellCheck_Click(object sender, EventArgs e)
		{
			SpellCheckerStatusSingleton.Instance.SyncroniseSpellCheckerStatus(KeyForSpellChecker, true);
		}

		internal void EnableSpellCheck()
		{
			var indexOfEnableDictionary = menuStrip.Items.IndexOfKey(MenuItemKeys.EnableSpellCheck);
			menuStrip.Items.RemoveAt(indexOfEnableDictionary);
			menuStrip.Items.Insert(indexOfEnableDictionary, DisableSpellCheckMenuItem);
			menuStrip.Items.Insert(menuStrip.Items.IndexOfKey(MenuItemKeys.CheckSpelling) + 1, AddToDictionaryMenuItem);

			HookEvents();
			FullyRecalculate();
		}

		void AddWordToDictionary(string word)
		{
			WordDictionaryManager.Instance.AddWord(word);

			FullyRecalculate();
		}

		void ShowUserDictionary()
		{
			var dictionary = WordDictionaryManager.Instance.CurrentDictionary;
			dictionary.HasChanges = false;

			var form = new PersonalDictionaryForm(dictionary);
			form.Saved += (o, e) => FullyRecalculate();
			form.FormClosed += (o, e) =>
			{
				if (dictionary.HasChanges)
				{
					dictionary.RevertWordsChanges();
				}
			};

			ZFormModaliser.Show(form, textbox.FindForm());
		}

		void RemovePrevSuggestions()
		{
			if (suggestions != null)
			{
				menuStrip.Items.RemoveByKey(MenuItemKeys.SuggestionsSeperator);
				menuStrip.Items.RemoveByKey(MenuItemKeys.NoSuggestionsAvailable);

				foreach (var suggestion in suggestions)
				{
					menuStrip.Items.RemoveByKey(suggestion);
				}
			}
		}

		void AddNewSuggestions(ISpellingError spellingError)
		{
			suggestions = spellChecker.GetSuggestions(spellingError).Take(5).ToList();

			menuStrip.Items.Add(new ToolStripSeparator { Name = MenuItemKeys.SuggestionsSeperator });
			if (suggestions.Count > 0)
			{
				textbox.Select(spellingError.WordIndex, spellingError.Word.Length);
				foreach (var suggestion in suggestions)
				{
					menuStrip.Items.Add(new ZToolStripMenuItem(suggestion, (o, e) => textbox.SelectedText = suggestion) { Name = suggestion });
				}
			}
			else
			{
				var noneAvailableMenuItem = new ZToolStripMenuItem(Res.GetString("SpellCheck|ba881bc7-da55-4517-b2e8-530cd7777b00", "No Suggestions Available"));
				noneAvailableMenuItem.Name = MenuItemKeys.NoSuggestionsAvailable;
				noneAvailableMenuItem.Enabled = false;

				menuStrip.Items.Add(noneAvailableMenuItem);
			}
		}

		protected virtual void DrawSquiggle(Graphics graphics, Point startPos, Point endPos)
		{
			var pen = Pens.Red;
			if ((endPos.X - startPos.X) > 4)
			{
				var squigglePoints = new List<Point>((endPos.X - startPos.X) / 2);
				for (var i = startPos.X; i <= (endPos.X - 2); i += 4)
				{
					squigglePoints.Add(NewScaledPoint(i, startPos.Y, false));
					squigglePoints.Add(NewScaledPoint((i + 2), (startPos.Y + 2), false));
				}

				graphics.DrawLines(pen, squigglePoints.ToArray());
			}
		}

		static CultureInfo SpellCheckCulture =>
			RawDataRegistry.Instance.EnglishSpelling.Value == SharedConstants.Languages.EnglishAmerican ?
				new CultureInfo("en-US") :
				Culture.Default;

		static ISpellCheckerForm CreateSpellCheckForm() => new SpellCheck.SpellCheckerForm { TopMost = true };

		ISpellChecker spellChecker;
		IList<string> suggestions;
		IList<ISpellingError> spellingErrors;
		ICollection<Tuple<Point, Point>> squiggles;

		protected readonly TextBoxBase textbox;
		protected readonly ContextMenuStrip menuStrip;

		bool alreadyInvalidating;

		ZToolStripMenuItem disableSpellCheckMenuItem;
		ZToolStripMenuItem DisableSpellCheckMenuItem => disableSpellCheckMenuItem ?? (disableSpellCheckMenuItem = new ZToolStripMenuItem(
			Res.GetString("SpellCheck|30BE773A-0DAC-4AEE-8619-829D6F74AFDA", "Disable Spellcheck"), DisableSpellCheck_Click)
		{ Name = MenuItemKeys.DisableSpellCheck });

		ZToolStripMenuItem enableSpellCheckMenuItem;
		ZToolStripMenuItem EnableSpellCheckMenuItem => enableSpellCheckMenuItem ?? (enableSpellCheckMenuItem = new ZToolStripMenuItem(
			Res.GetString("SpellCheck|EE95189C-6A49-44BD-BA66-8E86E41EDA1D", "Enable Spellcheck"), EnableSpellCheck_Click)
		{ Name = MenuItemKeys.EnableSpellCheck });

		ZToolStripMenuItem addToDictionaryMenuItem;
		ZToolStripMenuItem AddToDictionaryMenuItem => addToDictionaryMenuItem ?? (addToDictionaryMenuItem = new ZToolStripMenuItem(
			Res.GetString("SpellCheck|836f05ae-d384-43af-b96d-dfb29da22475", "Add to My Dictionary"),
			(o, ev) => AddWordToDictionary((string)((ToolStripItem)o).Tag))
		{ Name = MenuItemKeys.AddToDictionary });

		internal string KeyForSpellChecker { get; }

		bool SpellCheckerEnabled => SpellCheckerStatusSingleton.Instance.IsSpellCheckerEnabled(KeyForSpellChecker);

		internal static class MenuItemKeys
		{
			public const string SpellCheckSeperator = "spellcheckSeperator";
			public const string SuggestionsSeperator = "suggestionSeperator";
			public const string NoSuggestionsAvailable = "noSuggestionsAvailable";
			public const string CheckSpelling = "checkSpelling";
			public const string AddToDictionary = "addToDictionary";
			public const string ShowDictionary = "showDictionary";
			public const string EnableSpellCheck = "enableSpellCheck";
			public const string DisableSpellCheck = "disableSpellCheck";
		}
	}
}

#region Test
#if DEBUG

namespace Enterprise.ZArchitecture.GUI.Tools
{
	partial class SpellChecker
	{
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public ICollection<Tuple<Point, Point>> Squiggles_Exposed => squiggles;

		public void SendRightClick_ForTest(int indexOfCharToClickOn)
				=> SendRightClick(textbox, textbox.GetPositionFromCharIndex(indexOfCharToClickOn));

		void SendRightClick(Control tb, Point location)
		{
			var mouseEvent = new MouseEventArgs(MouseButtons.Right, 1, location.X, location.Y, 0);
			RightClick_Event(tb, mouseEvent);
		}

		public bool SpellCheckerEnabled_Exposed => SpellCheckerEnabled;

		public void DisableSpellCheck_ForTest()
		{
			DisableSpellCheck_Click(null, EventArgs.Empty);
		}

		public void EnableSpellCheck_ForTest()
		{
			EnableSpellCheck_Click(null, EventArgs.Empty);
		}

		public (int, int) CalculateVisibleIndexRange_ForTest()
		{
			return CalculateVisibleIndexRange();
		}
	}
}
#endif
#endregion
