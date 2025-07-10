using System.Collections.Generic;
using System.Windows.Forms;
using WTG.SpellCheck;

namespace CargoWise.Tools.SpellCheck.GUI.Testing
{
	sealed class TestControlSpellChecker : ControlSpellChecker
	{
		public TestControlSpellChecker(ISpellChecker spellChecker, params Control[] controls) : base(spellChecker, controls)
		{
		}

		public HashSet<string> IgnoredExposed => ignored;

		public ISpellCheckerForm SpellCheckerFormExposed => spellCheckerForm;

		public ControlSpellCheckError CurrentErrorExposed => CurrentError;
	}
}
