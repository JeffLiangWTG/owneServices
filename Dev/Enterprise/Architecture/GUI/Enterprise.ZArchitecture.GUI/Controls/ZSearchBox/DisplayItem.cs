using System;
using System.Collections.Generic;

namespace Enterprise.ZArchitecture.GUI.SearchBox
{
	public interface IDisplayItem
	{
		IEnumerable<string> Data { get; }
		DisplayItemTheme Theme { get; }
		bool IsSelectable { get; }
		void Select();
	}

	public sealed class DisplayItem : IDisplayItem
	{
		internal DisplayItem(DisplayItemTheme theme, Action selectAction, params string[] data)
		{
			Theme = theme;
			selectedAction = selectAction;
			Data = data;
		}

		public IEnumerable<string> Data { get; }

		public DisplayItemTheme Theme { get; }

		public bool IsSelectable => selectedAction != null;

		public void Select() => selectedAction?.Invoke();
		readonly Action selectedAction;
	}

	public static class DisplayItemFactory
	{
		public static DisplayItem CreateSearchItem(Action executeAction, params string[] data)
			=> new DisplayItem(DisplayItemTheme.DefaultItem, executeAction, data);

		public static DisplayItem CreateHeadingItem(params string[] data)
			=> new DisplayItem(DisplayItemTheme.DefaultHeading, null, data);

		public static DisplayItem CreateErrorItem(params string[] data)
			=> new DisplayItem(DisplayItemTheme.DefaultError, null, data);

#if DEBUG
		public static DisplayItem CreateSearchItemWithEmptyAction(params string[] data)
			=> CreateSearchItem(new Action(() => { }), data);
#endif
	}
}
