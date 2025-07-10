using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.DocumentEngine.Business;

namespace Enterprise.DocumentEngine.GUI
{
	class Shortcuts : IShortcuts
	{
		public string NoneShortcut => nameof(Shortcut.None);

		public IEnumerable<string> AllShortcuts => Enum.GetNames(typeof(Shortcut));

		public bool IsValidShortcut(string shortcut) => Enum.TryParse<Shortcut>(shortcut, out _);
	}
}
