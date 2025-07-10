using System.Collections.Generic;

namespace Enterprise.DocumentVisualizer.GUI
{
	interface IHaveHelpBar
	{
		HelpBarUserControl HelpBar { get; set; }
		IEnumerable<KeyValuePair<string, string>> KeyboardHints { get; }
	}
}
