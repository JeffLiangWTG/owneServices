using System;
using System.Collections;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	interface IDocumentCustomisationMenusMaker
	{
#if DEBUG
		void RaiseMenuItemsChangedForTesting();
#endif
		event EventHandler MenuItemsChanged;
		void Make(IList menuItems);
	}
}
