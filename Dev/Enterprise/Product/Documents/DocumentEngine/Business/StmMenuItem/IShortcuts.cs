using System.Collections.Generic;

namespace Enterprise.DocumentEngine.Business
{
	public interface IShortcuts
	{
		string NoneShortcut { get; }
		IEnumerable<string> AllShortcuts { get; }
		bool IsValidShortcut(string shortcut);
	}
}
