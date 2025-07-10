using System.Collections.Generic;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IDisplayInstructions
	{
		bool ShowEvents { get; }
		bool ShowLastEventDetails { get; }
		IEnumerable<IMenuItemDescriptor> MenuItems { get; }
	}
}
