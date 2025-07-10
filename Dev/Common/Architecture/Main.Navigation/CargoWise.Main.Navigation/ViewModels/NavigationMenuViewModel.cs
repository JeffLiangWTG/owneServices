using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Main.Navigation.ViewModels;
#nullable disable
public class NavigationMenuViewModel : INotifyPropertyChanged
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
	public NavigationMenuViewModel(MultilingualString multilingualText, string name, int columns, bool showSearch)
	{
		DisplayName = multilingualText;
		MultilingualText = multilingualText;
		IsTranslatable = multilingualText != null && !typeof(NoResString).IsAssignableFrom(multilingualText.GetType());
		Name = name;
		Buttons = new ObservableCollection<IMenuSection>();
		Columns = columns;
		ShowSearch = showSearch;
		DetailRows = 0;
	}

	public string DisplayName { get; }
	public MultilingualString MultilingualText { get; set; }
	public bool IsTranslatable { get; private set; }
	public string Name { get; set; }
	public int Columns { get; set; }
	public int ShortcutIndex { get; set; }
	public bool ShowSearch { get; set; }

	public int DetailRows
	{
		get { return detailRows; }
		set
		{
			detailRows = value;

			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(nameof(DetailRows)));
			}
		}
	}
	int detailRows;

	public void AddSection(string name, string displayName, MultilingualString multilingualSectionText, SectionType type, int maxNumberOfRecords, string subcategoryName, string subcategoryDisplayText, string subcategoryLetter, MultilingualString multilingualSubCategoryText)
	{
		var button = new MenuSection(displayName, name, multilingualSectionText, type, maxNumberOfRecords);

		if (!string.IsNullOrEmpty(subcategoryName))
		{
			var existingButton = Buttons.FirstOrDefault(b => b.Name == subcategoryName);
			if (existingButton == null)
			{
				var subcategoryButton = new MenuSection(subcategoryDisplayText, subcategoryName, multilingualSubCategoryText, SectionType.Subcategory, -1, subcategoryLetter);
				subcategoryButton.Subsections.Add(button);
				if ((Buttons.Count - DetailRows) % Columns == 0)
				{
					Buttons.Add(subcategoryButton);
					Buttons.Add(new MenuDetails(DetailRows++));
				}
				else
				{
					Buttons.Insert(Buttons.Count - 1, subcategoryButton);
				}
			}
			else
			{
				existingButton.Subsections.Add(button);
			}
		}
		else
		{
			Buttons.Add(button);
		}
	}

	IMenuSection GetButtonByName(string name, string subcategoryName)
	{
		if (string.IsNullOrEmpty(subcategoryName))
		{
			return Buttons.FirstOrDefault(b => b.Name == name);
		}
		else
		{
			var button = Buttons.FirstOrDefault(b => b.Name == subcategoryName);
			if (button != null)
			{
				return button.Subsections.FirstOrDefault(b => b.Name == name);
			}
		}

		return null;
	}

	public void AddItem(string buttonName, string subcategoryName, MenuItem item, bool insertAtTop = false)
	{
		var button = GetButtonByName(buttonName, subcategoryName) as MenuSection;
		if (button != null && (insertAtTop || item != null))
		{
			button.AddItem(item, insertAtTop);
		}
	}

	public void RemoveItem(string buttonName, string subcategoryName, string itemKey)
	{
		var button = GetButtonByName(buttonName, subcategoryName) as MenuSection;
		if (button != null)
		{
			var item = button.Items.FirstOrDefault(t => t.Key == itemKey);
			if (item != null)
			{
				button.Items.Remove(item);
				for (var i = 0; i < button.Items.Count; i++)
				{
					var buttonItem = button.Items[i];
					buttonItem.Index = i + 1;
				}
			}
		}
	}

	public void RemoveItemsWithSameDescription(string buttonName, string subcategoryName, string recordDescription, string recordKey)
	{
		var button = GetButtonByName(buttonName, subcategoryName) as MenuSection;
		if (button != null)
		{
			foreach (var item in button.Items.Where(t => t.Key == recordKey || t.Name == recordDescription).ToArray())
			{
				if (item != null)
				{
					button.Items.Remove(item);
					for (var i = 0; i < button.Items.Count; i++)
					{
						var buttonItem = button.Items[i];
						buttonItem.Index = i + 1;
					}
				}
			}
		}
	}

	public void UpdateItemWithSameKey(string buttonName, string subcategoryName, MenuItem itemUpdated)
	{
		if (GetButtonByName(buttonName, subcategoryName) is MenuSection button
			&& button.Items.FirstOrDefault(t => t.Key == itemUpdated.Key) is MenuItem itemToBeRemoved)
		{
			var index = button.Items.IndexOf(itemToBeRemoved);
			itemUpdated.Index = index + 1;
			button.Items[index] = itemUpdated;
		}
	}

	public ObservableCollection<IMenuSection> Buttons { get; set; }

	public ObservableCollection<IMenuSection> MenuSections => new ObservableCollection<IMenuSection>(Buttons.Where(b => b is MenuSection));

	public ObservableCollection<IMenuSection> RecentModules => new ObservableCollection<IMenuSection>(Buttons.Where(b => b is MenuSection s && s.Type == SectionType.RecentModule));
	public ObservableCollection<IMenuSection> Favorites => GetFavoriteMenuItems();
	public ObservableCollection<IMenuSection> MenuDetails => new ObservableCollection<IMenuSection>(Buttons.Where(b => b is MenuDetails));
	public ObservableCollection<IMenuSection> GetFavoriteMenuItems()
	{
		var favorites = new ObservableCollection<IMenuSection>(Buttons.Where(b => b is MenuSection s && s.Type == SectionType.Favorite));
		if (favorites.Count == 0)
		{
			return favorites;
		}
		var favoritesSection = (MenuSection)favorites.FirstOrDefault();
		var items = new ObservableCollection<MenuItem>(favoritesSection!.Items);
		favoritesSection.Items.Clear();
		foreach (var item in items)
		{
			item.SetIDWithName();
			favoritesSection.AddItem(item);
		}
		favoritesSection.Items.CollectionChanged -= FavoriteItems_CollectionChanged;
		favoritesSection.Items.CollectionChanged += FavoriteItems_CollectionChanged;

		return [favoritesSection];
	}
	public static void FavoriteItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
	{
		if (e.NewItems == null)
		{
			return;
		}
		foreach (MenuItem item in e.NewItems)
		{
			item.SetIDWithName();
		}
	}
	public ObservableCollection<IMenuSection> RecentItems => new ObservableCollection<IMenuSection>(Buttons.Where(b => b is MenuSection s && s.Type == SectionType.RecentItem));

	public ObservableCollection<MenuItem> RecentMenuItems => GetRecentMenuItems();

	ObservableCollection<MenuItem> GetRecentMenuItems()
	{
		var items = new ObservableCollection<MenuItem>();
		var recentItemSection = (MenuSection)RecentItems.FirstOrDefault();
		if (recentItemSection != null)
		{
			items = recentItemSection.Items;
			foreach (MenuItem item in items)
			{
				item.SetIDWithName();
			}
			items.CollectionChanged += RecentItems_CollectionChanged;
		}
		return items;
	}

	void RecentItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
	{
		if (e.NewItems != null)
		{
			foreach (MenuItem item in e.NewItems)
			{
				item.SetIDWithName();
			}
		}
	}

	public string NoRecentModulesText => ResString.GetMultilingualString("BBC0443E-3F39-49A4-8064-E4B00ACC64FF", "No recent modules.");
	public string NoFavoritesText => ResString.GetMultilingualString("CC117CF7-F5A5-4F28-A94B-12E1E101FAEA", "No favorites.");
	public string DragToReorderText => ResString.GetMultilingualString("dab39f6a-48d5-4e6f-9c63-1dad962afbe5", "Drag to reorder");
	public string NoRecentItemsText => ResString.GetMultilingualString("e69ba075-c191-42a2-86a4-14e71338de6c", "No recent items.");
	public string RecentItemsIDText => ResString.GetMultilingualString("EC423BA6-5D92-4CE0-95F5-11363B20DEE3", "ID #");
	public string RecentItemsNameText => ResString.GetMultilingualString("48DD3376-CA7E-4FC2-9167-C557AF61E2AD", "Name");
	public string RecentItemsModuleText => ResString.GetMultilingualString("1EACB17C-B67C-45AC-87B0-FBC2FA767F58", "Module");
	public string NoTasksText => ResString.GetMultilingualString("e7202d61-f276-4c47-9c0b-c42c44b9b8a7", "No tasks.");
	public string TasksDescriptionText => ResString.GetMultilingualString("0a32ffa8-179b-4512-8987-e3d2c0a8d254", "Task Description");
	public string TasksStatusText => ResString.GetMultilingualString("8db60f9e-2767-4016-8da5-216494adf47a", "Status");

	public IMenuSection SelectedItem
	{
		get
		{
			if (selectedItem == null)
			{
				selectedItem = MenuSections.FirstOrDefault() as MenuSection;
			}
			return selectedItem;
		}
		set
		{
			if (value is MenuSection)
			{
				if (selectedItem != null)
				{
					selectedItem.IsSelected = false;
				}

				selectedItem = value;
				selectedItem.IsSelected = true;

				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedItem)));
				}
			}
		}
	}
	IMenuSection selectedItem;

	public MenuSection HoveredItem
	{
		get { return hoveredItem; }
		set
		{
			if (hoveredItem != value)
			{
				hoveredItem = value;
				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(HoveredItem)));
				}
			}
		}
	}
	MenuSection hoveredItem;

	public event PropertyChangedEventHandler PropertyChanged;
}
