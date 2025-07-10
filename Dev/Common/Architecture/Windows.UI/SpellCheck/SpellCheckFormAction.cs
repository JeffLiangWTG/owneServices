#if DEBUG

namespace CargoWise.Tools.SpellCheck.TestFramework
{
	public class SpellCheckFormAction
	{
		public SpellCheckFormAction(GUI.SpellCheckerFormResult buttonAction, SpellCheckFormTestHandler nextHandler)
		{
			NextHandler = nextHandler;
			ButtonAction = buttonAction;
		}

		public SpellCheckFormAction(GUI.SpellCheckerFormResult buttonAction, int selectSuggestion, SpellCheckFormTestHandler nextHandler)
			: this(buttonAction, nextHandler)
		{
			SelectSuggestion = selectSuggestion;
		}

		public SpellCheckFormAction(GUI.SpellCheckerFormResult buttonAction, string manuallyEditedText, SpellCheckFormTestHandler nextHandler)
			: this(buttonAction, nextHandler)
		{
			ManuallyEditedText = manuallyEditedText;
		}

		public GUI.SpellCheckerFormResult ButtonAction { get; private set; }

		public int SelectSuggestion { get; private set; }

		public string ManuallyEditedText { get; private set; }

		public SpellCheckFormTestHandler NextHandler { get; private set; }
	}
}

#endif
