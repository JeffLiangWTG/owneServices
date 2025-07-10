using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Main.Navigation.DragDrop;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Main.Navigation;

#nullable disable
public class MenuItem : INotifyPropertyChanged, IDragable
{
	static readonly HashSet<string> ModuleExemptionSet = new HashSet<string>
	{
		#pragma warning disable CW1161
		"Network Diagrams",
		"Visual Boards"
		#pragma warning restore CW1161
	};

	public MenuItem(string key, MultilingualString multilingualText, MultilingualString module, Action action, Action rightClickAction, Action favoriteAction, Func<byte, bool> moveAction)
	{
		MultilingualText = multilingualText;
		Module = module;
		IsTranslatable = multilingualText != null && !typeof(NoResString).IsAssignableFrom(multilingualText.GetType());
		Name = multilingualText;
		Key = key;
		Status = (NoResString)"Working";

		if (action != null)
		{
			LinkAction = new ClickCommand(action);
		}

		if (rightClickAction != null)
		{
			LinkRightClickAction = new ClickCommand(rightClickAction);
		}

		SetFavoriteAction(favoriteAction);

		if (moveAction != null)
		{
			MoveAction = moveAction;
		}
	}

	public MenuItem(string key, MultilingualString multilingualString, Action action, Action rightClickAction, Action favoriteAction)
		: this(key, multilingualString, null, action, rightClickAction, favoriteAction, null)
	{
	}

	public MenuItem(string key, MultilingualString multilingualString, Action action)
		: this(key, multilingualString, null, action, null, null, null)
	{
	}

	public MenuItem(string key, MultilingualString multilingualString)
		: this(key, multilingualString, null, null, null, null, null)
	{
	}

	public MenuItem(MultilingualString multilingualString)
		: this(multilingualString, multilingualString, null, null, null)
	{
	}

	public void SetIDWithName()
	{
		if(!ModuleExemptionSet.Contains(Module?.GetUnresolvedString()))
		{ 
			var nameParts = MultilingualText.ToString().Split('-').ToList();
			if (nameParts.Count > 1)
			{
				ID = nameParts[0].Trim();
				nameParts.RemoveAt(0);
				Name = string.Join("-", nameParts).Trim();
			}
		}
	}

	public void ClearCommands()
	{
		if (LinkAction != null)
		{
			LinkAction.Clear();
		}

		if (LinkRightClickAction != null)
		{
			LinkRightClickAction.Clear();
		}

		if (FavoriteAction != null)
		{
			FavoriteAction.Clear();
		}

		MoveAction = null;
	}

	public void SetFavoriteAction(Action favoriteAction)
	{
		if (FavoriteAction != null)
		{
			FavoriteAction.Clear();
		}

		if (favoriteAction != null)
		{
			FavoriteAction = new ClickCommand(favoriteAction);
		}
	}

	public List<MenuItem> SubMenuItems { get; } = [];
	public ClickCommand LinkAction { get; private set; }
	public ClickCommand LinkRightClickAction { get; private set; }
	public ClickCommand FavoriteAction { get; private set; }
	public Func<byte, bool> MoveAction { get; private set; }
	public string Name { get; private set; }
	public MultilingualString MultilingualText { get; set; }
	public bool IsTranslatable { get; private set; }
	public string Key { get; private set; }
	public string ID { get; private set; }
	public MultilingualString Module { get; private set; }

	public string TaskDescription { get; }
	public string Status { get; private set; }

	public MultilingualString AddToFavoriteTooltip => ResString.GetMultilingualString("FAB0DCB5-4CD8-4E70-BD4C-E1DF3B2A4BB4", "Add to Favorites");
	public MultilingualString RemoveFromFavoritesTooltip => ResString.GetMultilingualString("8ADEBFDD-A1C4-4A8A-97A1-1D409C8E54DC", "Remove from Favorites");
	public MultilingualString DragToChangeOrderTooltip => ResString.GetMultilingualString("445C2C19-0538-45F4-A073-0783968296CC", "Drag to change order");
	public MultilingualString ShortcutCtrl => ResString.GetMultilingualString("357AEE07-0DA3-430C-A33B-EA1D3BBEB95E", "CTRL");
	public MultilingualString OpenHint => ResString.GetMultilingualString("3CBA09D7-BAF3-4064-9802-5221DA8D91F7", "Open");

	bool isSelected;

	public bool IsSelected
	{
		get { return isSelected; }
		set
		{
			if (isSelected != value)
			{
				isSelected = value;
				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsSelected)));
				}
			}
		}
	}

	public bool IsInFavorites
	{
		get { return isInFavorites; }
		set
		{
			if (isInFavorites != value)
			{
				isInFavorites = value;
				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsInFavorites)));
				}
			}
		}
	}
	bool isInFavorites;

	public int Index
	{
		get
		{
			return index;
		}
		set
		{
			if (index != value)
			{
				index = value;
				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(Index)));
				}
			}
		}
	}
	int index;

	public event PropertyChangedEventHandler PropertyChanged;

	public Type DataType
	{
		get { return typeof(MenuItem); }
	}

	public void Remove(object i)
	{
	}

	public bool AreBothIDandModuleSet => !ID.IsNullOrEmpty() && !Module.IsEmpty;
}
