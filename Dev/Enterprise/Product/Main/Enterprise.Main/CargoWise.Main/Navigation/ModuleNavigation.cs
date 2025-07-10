using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Main.Data;
#if !WINZOR
using CargoWise.Interop;
#endif
using CargoWise.Main.Navigation.ViewModels;
using CargoWise.Main.Service;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

#if WINZOR
using Enterprise.Registry.Business;
#endif
using Enterprise.Startup;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using MethodInvoker = System.Windows.Forms.MethodInvoker;
namespace CargoWise.Main.Navigation;

public class ModuleNavigation : KUserControl, IExtendedControl
{
	public ModuleNavigation()
	{
		navigationViewModel = new NavigationViewModel
		{
			MyTasksViewModel = new MyTasksViewModel(),
			RecentMessagesViewModel = new RecentMessagesViewModel(new RecentMessagesRepository()),
			PublicHolidaysViewModel = new PublicHolidaysViewModel(new PublicHolidaysRepository(), ZDateTime.Today.ToDateTime()),
			SnapshotsViewModel = CreateSnapshotsViewModel(),
		};
	}
	internal bool IsInSearchMode => navigationViewModel.IsInSearchMode;

	internal protected void SetSearchDelegate(Func<string, GlobalSearchResult> searchDelegate)
	{
		navigationViewModel.InitializeSearch((searchValue) =>
		{
			var result = searchDelegate(searchValue);
			return result.ToSearchSections();
		});
	}

	internal protected void SetOpenTaskModuleFormAction()
	{
		if (navigationViewModel.MyTasksViewModel is MyTasksViewModel myTasksViewModel)
		{
			myTasksViewModel.OpenTaskModuleForm = () =>
			{
				OpenModuleInNewWindow(new MainFormModule(ModuleIDs.ProcessTasks), null);
			};
		}
	}

	internal protected void SetOpenHolidayModuleFormAction()
	{
		if (navigationViewModel.PublicHolidaysViewModel is PublicHolidaysViewModel publicHolidaysViewModel)
		{
			publicHolidaysViewModel.OpenHolidayModuleForm = () =>
			{
				OpenModuleInNewWindow(new MainFormModule(ModuleIDs.CountryStatesGlbHoliday), null);
			};
		}
	}

	void OpenSnapshotModule(Snapshot snapshot)
	{
		var moduleFilterId = snapshot.ModuleFilter.ModuleFilterId;
		var moduleFilter = SnapshotQueryService.Instance.GetModuleFilterById(moduleFilterId);
		var moduleIdentifier = snapshot.ModuleFilter.ModuleId;
		OpenModuleInNewWindow(new MainFormModule(moduleIdentifier), moduleFilter);
	}

	SnapshotsViewModel CreateSnapshotsViewModel()
	{
		return new SnapshotsViewModel(new SnapshotsRepository(), GlbStaff.CurrentUser.PK.ToGuid())
		{
			RunSnapshotCommand = new RelayCommand(SnapshotCommand.FetchValue),
			RunOpenModuleFilterCommand = new RelayCommand((@object) =>
			{
				if (@object is Snapshot snapshot)
				{
					OpenSnapshotModule(snapshot);
				}
			}),
			OpenModuleCommand = new RelayCommand((moduleIdObj) =>
			{
				ModuleIdentifier moduleIdentifier = moduleIdObj switch
				{
					string strModuleId when !string.IsNullOrEmpty(strModuleId) =>
						SnapshotQueryService.Instance.ModuleList.GetRegisteredIdentifierByTableName(strModuleId),

					ModuleIdentifier moduleId => moduleId,

					SnapshotModule module => module.ModuleId,

					_ => null
				};

				if (moduleIdentifier is not null)
				{
					OpenModuleInNewWindow(new MainFormModule(moduleIdentifier), null);
				}
			}),
		};
	}

	#region Open Module

	public IModuleOpener ModuleOpener { get; set; }

	public virtual void OpenModule(MainFormModule module, bool isANewModuleClick = true) => OpenModuleImpl(module, isANewModuleClick);

	void OpenModuleImpl(MainFormModule module, bool isANewModuleClick = true)
	{
		var formHandle = FindForm().Handle;
		try
		{
			((WinFormsEnvironment)Env.Instance).CurrentModule = module.ID;

#if !WINZOR
			if (SafeNativeMethods.CanLockWindow)
			{
				SafeNativeMethods.LockWindowUpdate(formHandle);
			}
#endif

			ModuleOpener?.OpenModule(module, isANewModuleClick);
		}
		finally
		{
#if !WINZOR
			if (SafeNativeMethods.CurrentlyLockedWindow == formHandle)
			{
				SafeNativeMethods.UnlockWindowUpdate(formHandle);
			}
#endif
		}
	}

	public void UpdateRecentModules(MainFormModule module)
	{
		var shortcut = new LinkWrapper(module.ModuleID.Name, module.RecordKey, module.RecordUrl, module.RecordDescription);

		if (!RecentItemManager.Instance.IsInFavoriteModules(shortcut))
		{
			this.AddOrUpdateModule(
					ModuleTree.Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name],
					ModuleTree.Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name].Sections[ModuleTreeLoaderConstant.Section.RecentModules.Name],
					module);
			RecentItemManager.Instance.AddOrUpdateRecentModules(shortcut);
		}
	}

	internal static bool ModuleAlwaysOpensInNewWindow(MainFormModule module)
		=> !ZModuleFactory.Instance.IsModuleOfType<ZEmbeddedModule>(module.ModuleID);

#if DEBUG
	internal Form newModuleFormForTest;
#if WINZOR
	internal NextAppBarUserControl appBarForTest;
#else
	internal System.Windows.Window newModuleWindowForTest;
#endif
#endif

	internal void OpenModuleInNewWindow(MainFormModule module, StmModuleFilter filterLayout = null)
	{
		if (module != null && !string.IsNullOrEmpty(module.RecordUrl))
		{
			if (FindForm() is MainForm form && form.CheckLicenceAndSecurityPermissions(module) && !form.HasUnReadItemsMandatoryToRead())
			{
				WindowPersister.OpenFormsFromUrls(module.RecordUrl);
			}

			return;
		}

		if (ModuleSupportsOpeningInNewWindow(module))
		{
			var mainForm = (MainForm)FindForm();

			if (mainForm != null && mainForm.HasLicenceAndSecurityPermissions(module))
			{
#if !WINZOR
				if (CWNextFeatureHelper.IsCWNextEnabled())
				{
					Cursor.Current = Cursors.WaitCursor;
				}
#endif
				var newModuleForm = OpenModuleInNewWindowCore(module, filterLayout);

#if DEBUG
				newModuleFormForTest = newModuleForm;
#endif
				OnModuleFormInitialized(newModuleForm);
				newModuleForm?.Focus();

				if (newModuleForm is IMainForm newMainForm && newMainForm.CurrentModule is ZFilterGridModule filterGridModule && filterGridModule.EmbeddedControl is IFilterControl filterControl)
				{
					filterControl.HookFormEvents();
				}
			}
		}
		else
		{
			OpenModuleImpl(module);
		}
	}

	protected virtual void OnModuleFormInitialized(Form form)
	{
	}

	internal static bool ModuleSupportsOpeningInNewWindow(MainFormModule module)
	{
		if (module != null)
		{
			return ZModuleFactory.Instance.IsZPopupModuleNonSingleton(module.ModuleID)
				|| ZModuleFactory.Instance.IsModuleOfType<ZEmbeddedModule>(module.ModuleID);
		}
		return false;
	}

	public Form OpenModuleInNewWindowCore(MainFormModule module, StmModuleFilter filterLayout)
	{
		if (!(FindForm() is MainForm mainForm && mainForm.HasUnReadItemsMandatoryToRead()))
		{
			if (filterLayout != null)
			{
				FilterStripBizO.SetModuleId(module.ModuleID);
				FilterStripBizO.SaveLastUsedLayout(filterLayout.PK);
			}

			var newZModule = module.CreateZModule();
			if (newZModule != null)
			{
				if (newZModule is ZFilterGridModule filterGridModule)
				{
					filterGridModule.AllowLoadTemplateRecords = true;
				}

				if (newZModule is ZModule zmodule)
				{
					UpdateRecentModules(module);

					return OpenModuleInNewWindowCoreInternal(zmodule);
				}
			}
		}

		return null;
	}

	protected internal virtual Form OpenModuleInNewWindowCoreInternal(ZModule zmodule)
	{
		var popup = zmodule.ShowPopup() as Form;
		if (popup != null)
		{
			popup.Disposed += (o, e) => zmodule.Dispose();
		}

		#region Test stuff
#if DEBUG
		if (Globals.IsTest && zmodule is Enterprise.ZArchitecture.Modules.Testing.DummyModule)
		{
			CargoWise.Common.Testing.DisposableLeakListenerWrapper.UnRegisterDisposable(zmodule);
		}
#endif
		#endregion

		return popup;
	}

	#region GetRemoveLinkMenuItem

	ZMenuItem GetRemoveLinkMenuItem(MainFormModule module)
	{
		return new ZMenuItem(
				ResString.GetMultilingualString("BFBE57AC-FCF4-40f6-BC49-BDD037496ADE", "Remove Link"),
				(s, e) =>
				{
					using (((ZMenuItem)s).Parent)
					{
						var message = ResString.GetMultilingualString("70E8B640-4F39-43a2-8575-7E0D4DFCE8FF", "Are you sure you want to remove this link?");
						var caption = ResString.GetMultilingualString("D90C817B-A1B7-42c5-A679-F98879EE59B1", "Remove Link");
						var context = new DialogDefaultContext(new ZGuid("ADA31B33-EB6A-49FE-92BB-9546A9229403"), caption, ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question, showCheckboxOnly: true);
						var result = Globals.Message.ShowOrDefault(context, message);

						if (result == ZDialogResult.Yes)
						{
							var form = StartupOpenMainFormTask.MainFormInstance;
							form?.RemoveSingleLink(module);
						}
					}
				});
	}

	#endregion

	#region GetNewOpenInNewModuleMenuItem

	ModuleMenuItem GetNewOpenInNewModuleMenuItem(MainFormModule module)
		=> new ModuleMenuItem(module, (sender, args) => OpenModuleInNewWindow(((ModuleMenuItem)sender).Module));

	#endregion

	#region GetNewOpenModuleWithFilterLayoutMenuItems

	internal ModuleMenuItem[] GetNewOpenModuleWithFilterLayoutMenuItems(MainFormModule module, StmModuleFilterCollection layouts)
	{
		var result = new List<ModuleMenuItem>();

		if (layouts.Count > 0)
		{
			var favoritesHandler = new FavoriteLayoutsHandler(new BusinessObjectFactory(), module.ID);
			var favoriteLayouts = layouts.Where(filter => ((StmLinkCollection)favoritesHandler.FavoriteFilters).Select(link => link.STL_ItemPK).Contains(filter.PK)).OrderBy(x => x.S9_FilterName).ToList();

			var privateLayouts = layouts.Where(x => !x.S9_IsPublished && !x.IsUserDefinedFilter && !favoriteLayouts.Contains(x)).ToList();
			var sharedLayouts = layouts.Where(x => x.S9_IsPublished && !x.IsUserDefinedFilter && !favoriteLayouts.Contains(x)).ToList();

			if (favoriteLayouts.Count > 0)
			{
				result.Add(new ModuleMenuItem(" ", FontStyle.Regular, Color.Gray, 5f));
				result.Add(new ModuleMenuItem(FilterLayoutsHelper.FavoriteFilterLayoutsText, FontStyle.Regular, Color.Gray, SystemFonts.MenuFont.Size));
				result.AddRange(GetNewOpenModuleWithFilterLayoutMenuItems(module, favoriteLayouts, true));
			}

			if (privateLayouts.Count > 0)
			{
				result.Add(new ModuleMenuItem(" ", FontStyle.Regular, Color.Gray, 5f));
				result.Add(new ModuleMenuItem(FilterLayoutsHelper.MyFilterLayoutsText, FontStyle.Regular, Color.Gray, SystemFonts.MenuFont.Size));
				result.AddRange(GetNewOpenModuleWithFilterLayoutMenuItems(module, privateLayouts, false));
			}

			if (sharedLayouts.Count > 0)
			{
				result.Add(new ModuleMenuItem(" ", FontStyle.Regular, Color.Gray, 5f));
				result.Add(new ModuleMenuItem(FilterLayoutsHelper.SharedFilterLayoutsText, FontStyle.Regular, Color.Gray, SystemFonts.MenuFont.Size));
				result.AddRange(GetNewOpenModuleWithFilterLayoutMenuItems(module, sharedLayouts, false));
			}
		}

		return result.ToArray();
	}

	internal ICollection<ModuleMenuItem> GetNewOpenModuleWithFilterLayoutMenuItems(MainFormModule module, IEnumerable<StmModuleFilter> layouts, bool isFavorite)
	{
		return InsertInTree(module, FilterLayoutsHelper.CalculateTree(layouts, isFavorite));
	}

	List<ModuleMenuItem> InsertInTree(MainFormModule module, List<StripControl.ZFilterToolStripMenuItem> tree)
	{
		var result = new List<ModuleMenuItem>();
		foreach (var item in tree)
		{
			var openModuleOnClick = new EventHandler((sender, args) =>
			{
				OpenModuleInNewWindow(module, (StmModuleFilter)item.Tag);
			});

			var menuItem = new ModuleMenuItem(module, openModuleOnClick) { Text = item.Text };
			if (item.HasDropDownItems)
			{
				var subTree = item.DropDownItems.Cast<StripControl.ZFilterToolStripMenuItem>().ToList();
				var dropDownItems = InsertInTree(module, subTree);

				menuItem.MenuItems.AddRange(dropDownItems.ToArray());
			}
			result.Add(menuItem);
		}
		return result;
	}

	NavBarFilterStripBusinessObject FilterStripBizO => filterStripBizO ?? (filterStripBizO = new NavBarFilterStripBusinessObject());

	NavBarFilterStripBusinessObject filterStripBizO;

	#endregion

	#endregion

	#region Load Tree

	public void LoadModuleTree(ModuleTree tree)
	{
		foreach (var category in tree.Categories.Values.OfType<ModuleCategory>())
		{
			var shouldSortItems = category.Name != ModuleTreeLoaderConstant.Category.Jump.Name;
			AddCategory(category);

			var sections = category.Sections.Values.OfType<ModuleSection>();
			if (shouldSortItems)
			{
				var orderedTileNames = sections.Select(x => x.Subcategory != null ? x.Subcategory.DisplayText.GetUnresolvedString() : string.Empty).Distinct().ToList();
				sections = sections.OrderBy(x => orderedTileNames.IndexOf(x.Subcategory.DisplayText.GetUnresolvedString())).ThenBy(x => x.DisplayText.GetUnresolvedString());
			}

			foreach (var section in sections)
			{
				AddSection(category, section);

				var modules = section.Modules.Values.OfType<MainFormModule>();
				if (shouldSortItems)
				{
					modules = modules.OrderBy(x => x.Description.GetUnresolvedString());
				}

				foreach (var module in modules)
				{
					AddModule(category, section, module);
				}
			}
		}
	}

	void AddCategory(ModuleCategory category)
	{
		var isJump = category.Name == ModuleTreeLoaderConstant.Category.Jump.Name;
		var viewModel = new NavigationMenuViewModel(category.DisplayText, category.Name, isJump ? 0 : 3, isJump);
		navigationViewModel.AddCategory(viewModel);
		categories[category] = viewModel;
	}

	void AddSection(ModuleCategory category, ModuleSection section)
	{
		var viewModel = categories[category];
		if (viewModel != null)
		{
			var sectionType = GetSectionType(section.Name);
			viewModel.AddSection(
				section.Name,
				section.DisplayTextWithoutAmpersand,
				section.DisplayText,
				sectionType,
				MaxNumberOfItems(sectionType),
				section.Subcategory?.Name,
				section.Subcategory?.DisplayText,
				section.Subcategory?.Letter,
				section.Subcategory?.DisplayText);
		}
	}

	SectionType GetSectionType(string sectionName)
	{
		if (sectionName == ModuleTreeLoaderConstant.Section.Favorites.Name)
		{
			return SectionType.Favorite;
		}

		if (sectionName == ModuleTreeLoaderConstant.Section.RecentModules.Name)
		{
			return SectionType.RecentModule;
		}

		return sectionName == ModuleTreeLoaderConstant.Section.RecentItems.Name ? SectionType.RecentItem : SectionType.Module;
	}

	int MaxNumberOfItems(SectionType type)
	{
		switch (type)
		{
			case SectionType.RecentModule:
				return RecentItemManager.Instance.MaximumNumberOfRecentModules;
			case SectionType.RecentItem:
				return RecentItemManager.Instance.MaximumNumberOfRecentItems;
			case SectionType.Favorite:
				return RecentItemManager.Instance.MaximumNumberOfFavorites;
			default:
				return -1;
		}
	}

	public class MainFormModuleInfo
	{
		public MainFormModule module;
		public ModuleCategory category;
		public ModuleSection section;

		public MainFormModuleInfo(MainFormModule module, ModuleCategory category, ModuleSection section)
		{
			this.module = module;
			this.category = category;
			this.section = section;
		}
	}
	internal Dictionary<string, MainFormModuleInfo> ModuleLookup = new Dictionary<string, MainFormModuleInfo>();

	public void AddModule(ModuleCategory category, ModuleSection section, MainFormModule module, bool insertAtTop = false)
	{
		if (module == null)
		{
			return;
		}

		if (categories.TryGetValue(category, out var viewModel) && viewModel != null && section != null)
		{
			var item = CreateMenuItem(category, section, module);
			viewModel.AddItem(section.Name, section.Subcategory?.Name, item, insertAtTop);
		}

		if (!ModuleLookup.ContainsKey(module.ID))
		{
			ModuleLookup.Add(module.ID, new MainFormModuleInfo(module, category, section));
		}
	}

	MenuItem CreateMenuItem(ModuleCategory category, ModuleSection section, MainFormModule module)
	{
		var menuItem = new MenuItem
			(
			module.ID,
			category.Name == ModuleTreeLoaderConstant.Category.Jump.Name ? module.ExtendedDescription : module.Description,
			module.ModuleID.ExtendedDescription,
			() => OnTileLink_Click(this, module),
			() => OnTileLink_RightClick(this, module),
			null,
			section.Name == ModuleTreeLoaderConstant.Section.Favorites.Name ? (moveToIndex => MoveFavorite(category, section, module, moveToIndex)) : null
			);
		menuItem.IsInFavorites = RecentItemManager.Instance.IsInFavoriteModules(new LinkWrapper(module.ModuleID.Name, module.RecordKey, module.RecordUrl, module.RecordDescription));
		menuItem.SetFavoriteAction(() => OnFavorite_Click(menuItem, module));
		return menuItem;
	}

	public MainFormModuleInfo GetModuleInfoForOpener(ModuleOpenerInfo openerInfo)
	{
		if (openerInfo?.ModuleId == null || string.IsNullOrEmpty(openerInfo?.BizoPk))
		{
			return null;
		}
		var moduleIdString = openerInfo.ModuleId.Name + openerInfo.BizoPk; // this should evaluate to the same as module.ID, to match adding the key above in AddModule
		if (!ModuleLookup.ContainsKey(moduleIdString))
		{
			string recordURL;
			var pk = new Guid(openerInfo.BizoPk);

			if (openerInfo.ControllerId == null)
			{
				var command = "ShowEditForm";
				var controllerID = openerInfo.ModuleId.Name;
				var businessPK = openerInfo.BizoPk;
				recordURL = FormattableString.Invariant($"edient:Command={command}&ControllerID={controllerID}&BusinessEntityPK={businessPK}");
			}
			else
			{
				recordURL = ShowEditFormUrlHandler.Instance.Create(openerInfo.ControllerId, pk);
			}
			var tempModule = new MainFormModule(openerInfo.ModuleId, pk, recordURL, "recordDescription");

			ModuleLookup.Add(moduleIdString, new MainFormModuleInfo(tempModule, null, null));
		}

		return ModuleLookup[moduleIdString];
	}

	public void RemoveModule(ModuleCategory category, ModuleSection section, MainFormModule module)
	{
		if (categories.TryGetValue(category, out var viewModel) && section != null && module != null)
		{
			viewModel?.RemoveItem(section.Name, section.Subcategory?.Name, module.ID);
		}
	}

	public void RemoveModulesWithSameDescription(ModuleCategory category, ModuleSection section, MainFormModule module)
	{
		if (categories.TryGetValue(category, out var viewModel) && section != null && module != null)
		{
			viewModel?.RemoveItemsWithSameDescription(section.Name, section.Subcategory?.Name, module.RecordDescription, module.ID);
		}
	}

	public void AddOrUpdateModule(ModuleCategory category, ModuleSection section, MainFormModule module, bool updateOnly = false)
	{
		if (InvokeRequired)
		{
			BeginInvoke(new MethodInvoker(() => { AddOrUpdateModule(category, section, module, updateOnly); }));
		}
		else
		{
			if (updateOnly && categories.TryGetValue(category, out var viewModel) && viewModel != null && section != null && module != null)
			{
				var item = CreateMenuItem(category, section, module);
				viewModel.UpdateItemWithSameKey(section.Name, section.Subcategory?.Name, item);
			}
			else
			{
				RemoveModulesWithSameDescription(category, section, module);
				AddModule(category, section, module, true);
			}
		}
	}

	readonly Dictionary<ModuleCategory, NavigationMenuViewModel> categories = new Dictionary<ModuleCategory, NavigationMenuViewModel>();

	#endregion

	#region Selected Section/Category

	public ModuleSection SelectedSection
	{
		get => null;
		set
		{
			foreach (var category in categories)
			{
				if (category.Key == value.ParentCategory)
				{
					navigationViewModel.SelectedCategory = category.Value;
					break;
				}
			}
		}
	}

	public ModuleCategory SelectedCategory
	{
		get
		{
			var currentCategoryModel = navigationViewModel?.SelectedCategory;
			var currentCategory = GetCurrentCategory(currentCategoryModel);

			if (currentCategory?.Sections.Count == 1)
			{
				var button = currentCategoryModel?.Buttons.FirstOrDefault();
				if (button != null)
				{
					button.IsSelected = true;
					currentCategoryModel.SelectedItem = button;
				}
			}

			return currentCategory;
		}
		set => navigationViewModel.SelectedCategory = categories[value];
	}

	protected virtual ModuleCategory GetCurrentCategory(NavigationMenuViewModel currentCategoryModel)
	{
		return categories?.FirstOrDefault(c => c.Value == currentCategoryModel).Key;
	}

	public void SwitchCategory(ModuleCategory category)
	{
		SelectedCategory = category;
	}

	protected void HookCategoryChangedEvent()
	{
		navigationViewModel.PropertyChanged += (sender, e) =>
		{
			if (e.PropertyName == "SelectedCategory")
			{
				OnCategoryChanged();
			}
		};
	}
	public event EventHandler CategoryChanged;
	void OnCategoryChanged() => CategoryChanged?.Invoke(this, EventArgs.Empty);

	public bool SelectModuleFromFirstLetter(char letter)
	{
		var button = navigationViewModel.SelectedCategory.Buttons.OfType<MenuSection>().FirstOrDefault(x => x.Letter != null && x.Letter.StartsWith(letter.ToString()));

		if (button != null)
		{
			navigationViewModel.SelectedCategory.SelectedItem = button;
			return true;
		}
		return false;
	}

	public virtual bool SelectFindbox(string initialText = "")
	{
		return false;
	}

	#endregion

	#region Show/Hide

	static readonly int InitialPinnedWidth = 289;
	static readonly int UnpinnedWidth;

	public bool IsShown
	{
		get => Width > UnpinnedWidth;
		set => ControlDpiScalingHelper.SetWidth(this, value ? InitialPinnedWidth : UnpinnedWidth, true);
	}

	#endregion

	#region Clicking on a NavLink

#if DEBUG
	internal MainFormModule lastTileClickedModule;

	internal
#endif
	void OnTileLink_Click(object sender, MainFormModule module)
	{
#if DEBUG
		lastTileClickedModule = module;
#endif
		if (module.RecordKey != Guid.Empty && !String.IsNullOrEmpty(module.RecordUrl))
		{
			OpenModuleInNewWindow(module);
			return;
		}

		var form = FindForm();
		if (form != null && !form.IsDisposed)
		{
			if ((ModifierKeys == Keys.Control || RawDataRegistry.Instance.OpenModuleInANewWindow.Value) && string.IsNullOrEmpty(module.RecordUrl))
			{
				OpenModuleInNewWindow(module);
			}
			else
			{
				OpenModule(module);
			}
		}
	}

#if DEBUG
	internal
#endif
	void OnTileLink_RightClick(object sender, MainFormModule module)
	{
		ContextMenu menu = null;

		menu = new ContextMenu();
		menu.MenuItems.Add(GetNewOpenInNewModuleMenuItem(module));

		FilterStripBizO.SetModuleId(module.ModuleID);
		var layouts = FilterStripBizO.Layouts;

		if (string.IsNullOrEmpty(module.RecordUrl) && layouts.Count > 0)
		{
			menu.MenuItems.Add("-"); // Add a seperator into the context menu
			menu.MenuItems.Add(new ModuleMenuItem(ResString.GetMultilingualString("65EDE02C-D901-47F1-A1C3-5E91E27E1DA0", "Open {0} with below filter", module.Description), FontStyle.Italic, Color.Gray, SystemFonts.MenuFont.Size));
			menu.MenuItems.AddRange(GetNewOpenModuleWithFilterLayoutMenuItems(module, layouts));
		}

		var shortcut = new LinkWrapper(module.ModuleID.Name, module.RecordKey, module.RecordUrl, module.RecordDescription);

		if (RecentItemManager.Instance.IsInFavoriteModules(shortcut) || RecentItemManager.Instance.IsInRecentItems(string.Empty, shortcut) || RecentItemManager.Instance.IsInRecentModules(shortcut))
		{
			if (menu == null)
			{
				menu = new ContextMenu();
				menu.MenuItems.Add(GetRemoveLinkMenuItem(module));
			}
			else
			{
				menu.MenuItems.Add(1, GetRemoveLinkMenuItem(module));
			}

			if (components == null)
			{
				components = new Container();
			}

			components.Add(menu);
		}

#if WINZOR
		var position = MousePosition;
#else
		var position = HostControl.PointToClient(MousePosition);
#endif
		menu?.Show(HostControl, position);
	}

#if DEBUG
	internal
#endif
	void OnFavorite_Click(object sender, MainFormModule module)
	{
		var form = StartupOpenMainFormTask.MainFormInstance;
		var menuItem = sender as MenuItem;
		if (menuItem != null)
		{
			menuItem.IsInFavorites = form?.AddOrRemoveFromFavorites(module) ?? menuItem.IsInFavorites;
		}
	}

	bool MoveFavorite(ModuleCategory category, ModuleSection section, MainFormModule module, byte moveToIndex)
	{
		if (categories.TryGetValue(category, out var viewModel) && viewModel != null)
		{
			return RecentItemManager.Instance.MoveFavorite(new LinkWrapper(module.ModuleID.Name, module.RecordKey, module.RecordUrl, module.RecordDescription), ++moveToIndex);
		}

		return false;
	}

	#endregion

	#region Get Text

	internal static string GetText(MultilingualString description)
	{
		return KMenuItem.StripAcceleratorKeys(description);
	}

	#endregion

	#region IExtendedControl Implementation

	Control IExtendedControl.Host => this;

	IControlExtensionCollection IExtendedControl.Extensions => extensions ?? (extensions = new ControlExtensionCollection(this));
	IControlExtensionCollection extensions;

	#endregion

	internal NavigationViewModel navigationViewModel;
	internal KElementHost HostControl;
	protected Container components;
}
