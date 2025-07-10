using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DummyDisplayInstructions : IDisplayInstructions
	{
		public bool ShowEvents { get; set; }
		public bool ShowLastEventDetails { get; set; }
		public IEnumerable<IMenuItemDescriptor> MenuItems { get; set; }
	}
}