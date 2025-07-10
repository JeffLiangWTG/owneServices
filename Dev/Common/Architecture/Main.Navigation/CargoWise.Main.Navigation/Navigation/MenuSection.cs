using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.Main.Navigation.DragDrop;
using CargoWise.Main.Navigation.ViewModels;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;

namespace CargoWise.Main.Navigation;

#nullable disable
public class MenuSection : IMenuSection, INotifyPropertyChanged, IDropable
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
	public MenuSection(string displayName, string name, MultilingualString multilingualText, SectionType type, int maxNumberOfItems = -1, string letter = "")
	{
		Name = name;
		DisplayName = displayName;
		MultilingualText = multilingualText;
		IsTranslatable = multilingualText != null && !typeof(NoResString).IsAssignableFrom(multilingualText.GetType());
		Items = new ObservableCollection<MenuItem>();
		Subsections = new ObservableCollection<IMenuSection>();
		Index = -1;
		MaxNumberOfItems = maxNumberOfItems;
		Type = type;
		Letter = letter;

		if (type == SectionType.Favorite || type == SectionType.RecentItem || type == SectionType.RecentModule)
		{
			RemoveAllLinksAction = new ClickCommand(RemoveAllLinksFromSection);
			RemoveAllLinksText = ResString.GetMultilingualString("173CF420-6977-450d-8BAE-DDD05E942A35", "Remove All Links");
		}

		if (type == SectionType.Favorite)
		{
			SwitchViewText = ResString.GetMultilingualString("CB1FA267-6F4F-4B51-9FA1-25C1937A7BAA", "Switch view");
			SwitchViewAction = new ClickCommand(SwitchView);
		}
	}

	public MenuSection(string displayName, string name, MultilingualString multilingualText)
		: this(displayName, name, multilingualText, SectionType.Module)
	{
	}

	public void Dispose()
	{
		if (Type == SectionType.Favorite)
		{
			Items.CollectionChanged -= NavigationMenuViewModel.FavoriteItems_CollectionChanged;
		}
		foreach (var menuItem in Items)
		{
			if (menuItem != null)
			{
				menuItem.ClearCommands();
			}
		}

		foreach (var subsection in Subsections)
		{
			var menuSubsection = subsection as MenuSection;
			if (menuSubsection != null)
			{
				foreach (var menuItem in menuSubsection.Items)
				{
					if (menuItem != null)
					{
						menuItem.ClearCommands();
					}
				}
			}
		}
	}

	public string Name { get; set; }
	public string DisplayName { get; set; }
	public MultilingualString MultilingualText { get; set; }
	public bool IsTranslatable { get; private set; }
	public ObservableCollection<MenuItem> Items { get; private set; }
	public ObservableCollection<IMenuSection> Subsections { get; private set; }
	public int Index { get; set; }
	public int MaxNumberOfItems { get; private set; }
	public SectionType Type { get; private set; }
	public string Letter { get; private set; }
	public MultilingualString RemoveAllLinksText { get; }
	public ClickCommand RemoveAllLinksAction { get; }
	public MultilingualString SwitchViewText { get; }
	public ClickCommand SwitchViewAction { get; }

	public bool IsRecentOrFavorite
	{
		get
		{
			return Type != SectionType.Module;
		}
	}

	public void AddItem(MenuItem item, bool insertAtTop = false)
	{
		if (insertAtTop)
		{
			Items.Insert(0, item);
			for (var i = 0; i < Items.Count; i++)
			{
				var menuItem = Items[i];
				menuItem.Index = i + 1;
			}
		}
		else
		{
			Items.Add(item);
			item.Index = Items.Count;
		}

		if (MaxNumberOfItems > -1 && Items.Count > MaxNumberOfItems)
		{
			Items.RemoveAt(Items.Count - 1);
		}
	}

	void RemoveAllLinksFromSection()
	{
		Argument.NotNull(RecentItemManager.Instance, nameof(RecentItemManager.Instance));
		Argument.NotNull(Globals.Message, nameof(Globals.Message));

		var message = ResString.GetMultilingualString("17729E66-311C-45cc-9654-47B36A49D713", "Are you sure you want to remove all links?");
		var caption = ResString.GetMultilingualString("AB588248-FF57-48c4-A445-DC539E3CF7BB", "Remove All Links");
		var result = Globals.Message.Show(message, caption, ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question, ZDialogResult.Yes);

		if (result == ZDialogResult.Yes)
		{
			Items.Clear();
			Index = -1;

			switch (Type)
			{
				case SectionType.Favorite:
					RecentItemManager.Instance.RemoveAllFavoriteModules();
					break;
				case SectionType.RecentItem:
					RecentItemManager.Instance.RemoveAllRecentItems(string.Empty);
					break;
				case SectionType.RecentModule:
					RecentItemManager.Instance.RemoveAllRecentModules();
					break;
			}
		}
	}

	bool isCompressedView;
	public bool IsCompressedView
	{
		get { return isCompressedView;  }
		set
		{
			if (isCompressedView != value)
			{
				isCompressedView = value;

				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsCompressedView)));
				}
			}
		}
	}

	void SwitchView()
	{
		IsCompressedView = !IsCompressedView;
	}

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
	bool isSelected;

	public event PropertyChangedEventHandler PropertyChanged;

	public Type DataType
	{
		get { return typeof(MenuItem); }
	}

	public void Drop(object data, int index = -1, bool insertAbove = false)
	{
		if (data != null && index > -1 && index < Items.Count)
		{
			var menuItem = data as MenuItem;
			if (menuItem != null)
			{
				var oldIndex = menuItem.Index - 1;

				if (!insertAbove)
				{
					index++;
				}

				if (oldIndex < index)
				{
					index--;
				}

				if (oldIndex != index)
				{
					if (menuItem.MoveAction != null && menuItem.MoveAction((byte)index))
					{
						Items.Move(oldIndex, index);

						for (var i = Math.Min(oldIndex, index); i <= Math.Max(oldIndex, index); i++)
						{
							var item = Items[i];
							item.Index = i + 1;
						}
					}
				}
			}
		}
	}
}

public enum SectionType
{
	Subcategory,
	Module,
	Favorite,
	RecentModule,
	RecentItem,
	GlobalSearch
}
