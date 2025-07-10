using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public class FilterModuleMenuItemDescriptorCollection : List<FilterModuleMenuItemDescriptorCollection.FilterModuleMenuItemDescriptor>
	{
		public void Add(MultilingualString text, EventHandler onClick)
		{
			Add(new FilterModuleMenuItemDescriptor(text, onClick));
		}

#if DEBUG
		// Looking up an item by text is for unit tests only
		public EventHandler this[string text]
		{
			get
			{
				var match = this.Find(item => item.Text.GetUnresolvedString() == text);
				return match != null ? match.OnClick : null;
			}
		}
#endif

		public class FilterModuleMenuItemDescriptor : Tuple<MultilingualString, EventHandler>
		{
			public FilterModuleMenuItemDescriptor(MultilingualString text, EventHandler onClick)
				: base(text, onClick)
			{ }

			public MultilingualString Text { get { return Item1; } }
			public EventHandler OnClick { get { return Item2; } }
		}
	}
}
