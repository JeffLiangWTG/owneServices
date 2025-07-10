using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WTG.SpellCheck;

namespace CargoWise.Tools.SpellCheck.GUI
{
	/// <summary>
	/// Result of the spell checking GUI
	/// </summary>
	public enum ControlSpellCheckerResult
	{
		/// <summary>
		/// There were no misspelled words
		/// </summary>
		NoErrors = 0,

		/// <summary>
		/// All misspelled words were corrected
		/// </summary>
		AllErrorsCorrected = 1,

		/// <summary>
		/// There were one or more misspelled words that were not corrected
		/// </summary>
		ErrorsIgnored = 2,

		/// <summary>
		/// The spell check process was cancelled by the user
		/// </summary>
		Cancel = 4,

		/// <summary>
		/// The spell check process was re-run due to a manual edit
		/// </summary>
		ManualEditsMade = 5
	}

	/// <summary>
	/// Provides a GUI to spell check TextBoxBase controls on a form.
	/// Give a list of TextBox controls on a form to spellcheck, or a container control to recursivly spellcheck all decendants.
	/// Call SpellCheck() to perform the spellcheck and show the GUI for misspelled words.
	/// </summary>
	public class ControlSpellChecker
	{
		public ControlSpellChecker(ISpellChecker spellChecker, params Control[] controls)
			: this(spellChecker, (IEnumerable<Control>)controls)
		{
		}

		public ControlSpellChecker(ISpellChecker spellChecker, IEnumerable<Control> controls)
			: this(spellChecker, controls, GetDefaultSpellCheckerForm)
		{
		}

		public ControlSpellChecker(ISpellChecker spellChecker, IEnumerable<Control> controls, Func<ISpellCheckerForm> getAlternateSpellcheckForm)
		{
			getNewSpellCheckerForm = getAlternateSpellcheckForm;
			SpellChecker = spellChecker;
			AddControls(controls);
		}

		static ISpellCheckerForm GetDefaultSpellCheckerForm() => new SpellCheckerForm();

		readonly Func<ISpellCheckerForm> getNewSpellCheckerForm;

		readonly List<Control> controls = new List<Control>();
		readonly List<Control> excludedControls = new List<Control>();
		readonly Dictionary<Control, ISpellChecker> controlSpecificSpellCheckers = new Dictionary<Control, ISpellChecker>();

		protected HashSet<string> ignored = new HashSet<string>();
		readonly List<string> ignoreAll = new List<string>();

		protected ISpellCheckerForm spellCheckerForm;

		List<ControlSpellCheckError> spellingErrors;
		int currentSpellingErrorIndex;
		ControlSpellCheckerResult spellCheckResult;

		/// <summary>
		/// Add a control to be spell checked. If the control is a container control, all decendants will spellchecked.
		/// </summary>
		public void AddControl(Control control)
		{
			controls.Add(control);
		}

		/// <summary>
		/// Add a control to be spell checked, with a specific spell checker
		/// </summary>
		public void AddControl(ISpellChecker spellChecker, Control control)
		{
			controls.Add(control);
			controlSpecificSpellCheckers.Add(control, spellChecker);
		}

		public void AddControls(IEnumerable<Control> controls)
		{
			this.controls.AddRange(controls);
		}

		public void AddControls(ISpellChecker spellChecker, IEnumerable<Control> controls)
		{
			this.controls.AddRange(controls);
			foreach (var control in controls)
			{
				controlSpecificSpellCheckers.Add(control, spellChecker);
			}
		}

		public void AddControls(params Control[] controls)
		{
			this.controls.AddRange(controls);
		}

		public void AddControls(ISpellChecker spellChecker, params Control[] controls)
		{
			this.controls.AddRange(controls);
			foreach (var control in controls)
			{
				controlSpecificSpellCheckers.Add(control, spellChecker);
			}
		}

		/// <summary>
		/// Exclude a specific TextBoxBase control from being spellchecked. Exclude a container control to exclude all decendants.
		/// </summary>
		public void ExcludeControl(Control control)
		{
			excludedControls.Add(control);
		}

		/// <see cref="ExcludeControl"/>
		public void ExcludeControls(IEnumerable<Control> controls)
		{
			excludedControls.AddRange(controls);
		}

		/// <see cref="ExcludeControl"/>
		public void ExcludeControls(params Control[] controls)
		{
			excludedControls.AddRange(controls);
		}

		/// <summary>
		/// Get or set the spell chcker instance used to spell check the form
		/// </summary>
		public ISpellChecker SpellChecker { get; set; }

		void PrepareNewSpellcheck()
		{
			spellCheckResult = ControlSpellCheckerResult.NoErrors;
			spellingErrors = new List<ControlSpellCheckError>();

			var textEditableControls = GetTextEditableControls();
			foreach (var kvp in textEditableControls)
			{
				var spellChecker = kvp.Value;
				var control = kvp.Key;
				var checkSpellingErrors = spellChecker.CheckSpellingWithSuggestions(control.Text);
				foreach (var checkSpellingError in checkSpellingErrors)
				{
					var controlSpellingError = new ControlSpellCheckError(control, checkSpellingError);
					spellingErrors.Add(controlSpellingError);
				}
			}

			currentSpellingErrorIndex = -1;
		}

		/// <summary>
		/// Perform the spellcheck, showing the GUI for misspelled words
		/// </summary>
		/// <returns>The result of the spell check process</returns>
		public ControlSpellCheckerResult CheckSpelling()
		{
			PrepareNewSpellcheck();

			if (MoveToNextError(false))
			{
				using (spellCheckerForm = getNewSpellCheckerForm())
				{
					spellCheckerForm.ItemSpellCheckCompleted += SpellCheckerForm_ItemSpellCheckCompleted;
					spellCheckerForm.LoadSpellingError(CurrentError);
					spellCheckerForm.ShowDialog(CurrentError.Control.TopLevelControl);
				}
			}

			return spellCheckResult;
		}

		bool MoveToNextError(bool recheck)
		{
			while (++currentSpellingErrorIndex < spellingErrors.Count)
			{
				if (!ignoreAll.Contains(CurrentError.SpellingError.Word))
				{
					if (!recheck || !ignored.Contains(CurrentError.SpellingError.Word))
					{
						return true;
					}
				}
			}
			return false;
		}

		protected ControlSpellCheckError CurrentError
		{
			get { return spellingErrors[currentSpellingErrorIndex]; }
		}

		ControlSpellCheckError SpellCheckerForm_ItemSpellCheckCompleted(ISpellCheckerFormSpellingError error, SpellCheckerFormResult result, string value, string errorParagraph = "", int errorParagraphIndex = -1)
		{
			switch (result)
			{
				case SpellCheckerFormResult.Cancel:
					spellCheckResult = ControlSpellCheckerResult.Cancel;
					return null;

				case SpellCheckerFormResult.Ignore:
					ignored.Add(error.SpellingError.Word);
					spellCheckResult = ControlSpellCheckerResult.ErrorsIgnored;
					break;

				case SpellCheckerFormResult.IgnoreAll:
					ignored.Add(error.SpellingError.Word);
					ignoreAll.Add(error.SpellingError.Word);
					spellCheckResult = ControlSpellCheckerResult.ErrorsIgnored;
					break;

				case SpellCheckerFormResult.ChangeManual:
					//restart the spellchecker with the passed in line replacing the current error line, but preserve ignored and ignore all (i.e. don't create a new controlSpellChecker)
					if (errorParagraphIndex != -1)
					{
						var errorCast = (ControlSpellCheckError)error;
						SetErrorControlText(errorCast, errorCast.Control.Text.Substring(0, errorParagraphIndex) + value + errorCast.Control.Text.Substring(errorParagraphIndex + errorParagraph.Length));
						PrepareNewSpellcheck();
						spellCheckResult = ControlSpellCheckerResult.ManualEditsMade;
					}
					break;

				case SpellCheckerFormResult.Change:
				case SpellCheckerFormResult.ChangeAll:
					if (result == SpellCheckerFormResult.ChangeAll)
					{
						ChangeAllSpellings((ControlSpellCheckError)error, currentSpellingErrorIndex, value);
					}
					else
					{
						ChangeSpelling((ControlSpellCheckError)error, currentSpellingErrorIndex, value);
					}

					if (spellCheckResult == ControlSpellCheckerResult.NoErrors)
					{
						spellCheckResult = ControlSpellCheckerResult.AllErrorsCorrected;
					}

					break;
			}

			if (MoveToNextError(spellCheckResult == ControlSpellCheckerResult.ManualEditsMade))
			{
				return CurrentError;
			}
			else
			{
				return null;
			}
		}

		void SetErrorControlText(ControlSpellCheckError error, string newText)
		{
			error.Control.Text = newText;
			error.Control.GetType().GetMethod("OnValidating", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(error.Control, new object[] { null }); // validation for data bound controls
			error.Control.GetType().GetMethod("OnValidated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(error.Control, new object[] { null });
		}

		void ChangeSpelling(ControlSpellCheckError error, int errorIndex, string value)
		{
			SetErrorControlText(error, error.Control.Text.Substring(0, error.SpellingError.WordIndex) + value + error.Control.Text.Substring(error.SpellingError.WordIndex + error.SpellingError.Word.Length));

			// adjust word indices of errors occuring in the same control after the replacement!!
			var indexDelta = value.Length - error.SpellingError.Word.Length;
			if (indexDelta != 0)
			{
				for (var i = errorIndex + 1; i < spellingErrors.Count && spellingErrors[i].Control == error.Control; i++)
				{
					// this assumes errors are ordered by word index
					var oldError = spellingErrors[i].SpellingError;
					var newError = new SpellingErrorWithSuggestions(oldError.Word, oldError.WordIndex + indexDelta, oldError.Suggestions);
					spellingErrors[i].SpellingError = newError;
				}
			}
		}

		void ChangeAllSpellings(ControlSpellCheckError error, int errorIndex, string value)
		{
			ChangeSpelling(error, errorIndex, value);
			for (var i = spellingErrors.Count - 1; i > errorIndex; i--)
			{
				if (error.SpellingError.Word == spellingErrors[i].SpellingError.Word)
				{
					ChangeSpelling(spellingErrors[i], i, value);
					spellingErrors.RemoveAt(i);
				}
			}
		}

		List<KeyValuePair<Control, ISpellChecker>> GetTextEditableControls()
		{
			var fctval = new List<KeyValuePair<Control, ISpellChecker>>();
			foreach (var control in controls)
			{
				fctval.AddRange(GetTextEditableControls(control, null));
			}
			return fctval;
		}

		List<KeyValuePair<Control, ISpellChecker>> GetTextEditableControls(Control control, ISpellChecker spellChecker)
		{
			var fctval = new List<KeyValuePair<Control, ISpellChecker>>();
			if (controlSpecificSpellCheckers.ContainsKey(control))
			{
				spellChecker = controlSpecificSpellCheckers[control];
			}
			if (!excludedControls.Contains(control) && control.Enabled)
			{
				if (IsTextEditable(control))
				{
					fctval.Add(new KeyValuePair<Control, ISpellChecker>(control, spellChecker ?? SpellChecker));
				}
				else
				{
					foreach (Control subcontrol in control.Controls)
					{
						fctval.AddRange(GetTextEditableControls(subcontrol, spellChecker));
					}
				}
			}
			return fctval;
		}

		static bool IsTextEditable(Control control)
		{
			return control is TextBoxBase;
		}
	}

	public class ControlSpellCheckError : ISpellCheckerFormSpellingError
	{
		public ControlSpellCheckError(Control control, ISpellingErrorWithSuggestions spellingError)
		{
			Control = control;
			SpellingError = spellingError;
		}

		public ISpellingErrorWithSuggestions SpellingError { get; set; }

		public string Text
		{
			get { return Control.Text; }
		}

		public Control Control { get; }

		bool controlHideSelection;

		public void OnLoadSpellingErrorContent()
		{
			controlHideSelection = ((TextBoxBase)Control).HideSelection;
			if (controlHideSelection)
			{
				var text = Text;
				((TextBoxBase)Control).HideSelection = false;
				Control.Text = text;
			}
			((TextBoxBase)Control).Select(SpellingError.WordIndex, SpellingError.Word.Length);
		}

		public void OnItemSpellCheckCompleted()
		{
			if (controlHideSelection)
			{
				var text = Text;
				((TextBoxBase)Control).HideSelection = controlHideSelection;
				Control.Text = text;
			}
		}
	}
}
