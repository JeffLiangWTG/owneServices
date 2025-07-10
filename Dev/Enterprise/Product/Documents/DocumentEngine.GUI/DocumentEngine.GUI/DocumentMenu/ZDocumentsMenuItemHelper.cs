using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.GUI.DocumentDelivery;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

#if WINZOR
using WinzorFramework;
#endif
#if !WINZOR
using Enterprise.RemoteDesktopServices;
#endif

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	public static class ZDocumentsMenuItemConstants
	{
		public readonly static MultilingualString PleaseSaveYourRecord = ResString.GetMultilingualString("MenuItem.Documents.PleaseSaveYourRecord", "Please save your record before running documents.");
	}

	public abstract class ZDocumentsMenuItemHelper<T>
		where T : class
	{
		#region Constructor

		protected ZDocumentsMenuItemHelper(T menuItem)
		{
			this.menuItem = menuItem;
		}

		readonly T menuItem;

		internal static MultilingualString DocMenuName
		{
			get { return ResString.GetMultilingualString("MenuItem.Documents", "&Documents"); }
		}

		#endregion

		#region Setup

		internal void Setup(IDocumentSupportable parentBusinessObject, IDocumentEventsForMenu documentEventsForMenu, UserControlProviderList parentUserFieldList, UserControlProviderList parentSystemDefinedFieldList)
		{
			if (parentBusinessObject == null)
			{
				throw new ArgumentNullException(nameof(parentBusinessObject), "IDocumentSupportable ParentBusinessObject");
			}

			parentBusinessObject.DocumentSupporter.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
			this.parentBusinessObject = parentBusinessObject;
			this.documentEventsForMenu = documentEventsForMenu;
			this.parentUserFieldList = parentUserFieldList;
			this.parentSystemDefinedFieldList = parentSystemDefinedFieldList;
			setupCalled = true;
		}

		IDocumentSupportable parentBusinessObject;
		UserControlProviderList parentUserFieldList;
		UserControlProviderList parentSystemDefinedFieldList;
		bool setupCalled;

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				ResetMenuItemsAreShowing();
			}
		}

		void ResetMenuItemsAreShowing()
		{
			menuItemsAreShowing = null;
		}

		bool? menuItemsAreShowing;

		#endregion

		#region LoadMenus

		internal void LoadMenus(Form parentForm, Func<Form, IDocumentSupportable, UserControlProviderList, IDocumentCustomisationMenusMaker> getNewMenusMaker)
		{
			if (!setupCalled)
			{
				throw new InvalidOperationException("Setup must be called before LoadMenus");
			}

			if (menusMaker == null)
			{
				menusMaker = getNewMenusMaker(parentForm, parentBusinessObject, parentUserFieldList);
				menusMaker.MenuItemsChanged += new EventHandler(MenusMaker_MenuItemsChanged);
			}

			LoadMenusCore();
		}

		IDocumentCustomisationMenusMaker menusMaker;

		void MenusMaker_MenuItemsChanged(object sender, EventArgs e)
		{
			ResetMenuItemsAreShowing();
			LoadMenusCore();
		}

		void LoadMenusCore()
		{
			var menuItemsShouldShow = MenuShouldShowFor(parentBusinessObject.DocumentSupporter);
			if (menuItemsShouldShow != menuItemsAreShowing)
			{
				var menuItems = GetItems(menuItem);
				menuItems.Clear();

				if (menuItemsShouldShow)
				{
					RebuildMenuItems();
				}
				else
				{
					menuItems.Add(GetNewMenuItem("PleaseSaveYourRecord", ZDocumentsMenuItemConstants.PleaseSaveYourRecord, null));
				}

				menuItemsAreShowing = menuItemsShouldShow;
			}
		}

		internal abstract IList GetItems(T menuItem);
		internal abstract T GetNewMenuItem(string name, MultilingualString text, EventHandler handler);

		static bool MenuShouldShowFor(DocumentSupporter documentSupporter)
		{
			return (documentSupporter.IsInDatabase || documentSupporter.IsNonPersistent) && (documentSupporter.IgnoreHasChanges || !documentSupporter.HasChanges);
		}

		#endregion

		#region RebuildMenuItems

		void RebuildMenuItems()
		{
			if (documentEventsForMenu == null && parentBusinessObject is IBusiness)
			{
				documentEventsForMenu = new DocumentEventsForMenu(parentBusinessObject as BusinessObject);
			}

			var documentCommands = new DocumentCommandCollection(parentBusinessObject, true);
			documentCommands.Load();

			foreach (DocumentCommand item in documentCommands.GetApplicableDocumentCommands())
			{
				var newMenu = CreateMenu(item);
#if WINZOR
				((IWinzorMenuItem)newMenu).HandleAsSelectable = true;
#endif
				var parentMenu = GetParentMenu(item);
				GetItems(parentMenu).Add(newMenu);
			}

			var menuItems = GetItems(menuItem);
			if (menuItems.Count == 0)
			{
				menuItems.Add(GetNewMenuItem("NoDocumentsFound", ResString.GetMultilingualString("MenuItem.Documents.NoDocumentsFound", "No Documents Found."), null));
			}

			UnpublishedDocumentMenuItemsManager.MoveUnpublishedDocumentMenuItemsToBottom(menuItems, this);
			OrganizeLegacyDocumentsMenuItem();
			menusMaker.Make(menuItems);

#if DEBUG
			IsRebuildMenuItemsCalled = true;
			IsGetApplicalbeDocumentsCommandsCalled = documentCommands.FilterEvaluatedResult.Keys.Count > 0;
#endif
		}

		IDocumentEventsForMenu documentEventsForMenu;

		#region IsRebuildMenuItemsCalled
#if DEBUG
		internal bool IsRebuildMenuItemsCalled;
		internal bool IsGetApplicalbeDocumentsCommandsCalled;
#endif
		#endregion

		#region CreateMenu

		T CreateMenu(DocumentCommand item)
		{
			var menuShortcut = (NoResString)"None";

			if (!item.SU_MenuShortcut.IsEmpty)
			{
				menuShortcut = (NoResString)item.SU_MenuShortcut;
			}

			var clickHandler = GetEventHandler(item);
			var newItem = new TaggedMenuItem(item.SU_MenuNameMultilingual, clickHandler, menuShortcut, item, item.SU_HintMultilingual, this);
			SetEnabled(newItem, !item.SU_IsSystemDefined || item.SU_IsPublished);
			taggedMenuItemDocumentCommands.Add(newItem, newItem);
			return newItem;
		}

		readonly Dictionary<T, TaggedMenuItem> taggedMenuItemDocumentCommands = new Dictionary<T, TaggedMenuItem>();

		EventHandler GetEventHandler(DocumentCommand command) => command.SU_MenuType == Core.Constants.StmMenuItemTypes.Forms
			? OnFormMenuClick
			: OnMenuClick;

		#region OnMenuClick

		void OnFormMenuClick(object sender, EventArgs e)
		{
			if (parentBusinessObject is BusinessObject bizObj
				&& sender is T t
				&& taggedMenuItemDocumentCommands.TryGetValue(t, out var taggedMenuItem))
			{
				taggedMenuItem.UpdateShowFormsFromMainThread();
				var moduleIDForSecurity = GetModuleIdentifier(sender, out _);

				var visualizerCommandProvider = ObjectFactory.Get<IVisualizableDocumentCommandProvider>();
				var command = visualizerCommandProvider?.GetCommand(bizObj, taggedMenuItem.DocumentCommand, moduleIDForSecurity);
				command?.Execute();
			}
		}

		void OnMenuClick(object sender, EventArgs e)
		{
			if (taggedMenuItemDocumentCommands.TryGetValue(sender as T, out var result))
			{
				result.UpdateShowFormsFromMainThread();
				var documentRunner = GetNewDocumentRunner(sender);
				var hasExtraDialogFormOpened = false;
				var mainMenu = GetMainMenu(menuItem);
				Form mainForm = mainMenu != null ? GetForm(mainMenu) : null;
#if DEBUG
				if (Globals.IsTest)
				{
					LastRunReportInfos.Clear();
					documentRunner.ShouldActuallyRunDocumentSetForTesting = ShouldActuallyRunDocumentSetForTesting;
					documentRunner.ThrowExceptionInGetDocumentPrintSetForTesting = ThrowExceptionInGetDocumentPrintSetForTesting;
				}
#endif
				try
				{
					using (ZFormModaliser.SetTemporaryDelegateToCallBeforeShowingFormsOrDialogs(ProcessForm))
					using (Report.TemporarilySetReportRunSource(Report.ReportRunSource.ManualPrint))
					{
						result.DocumentCommand.ControllerId = TryGetControllerID(sender, mainForm) ?? result.DocumentCommand.ControllerId;
						documentRunner.Run(result.DocumentCommand);
					}

					void ProcessForm(object form)
					{
						if (!(form is DocDeliveryForm))
						{
							hasExtraDialogFormOpened = true;
						}
						else if (hasExtraDialogFormOpened)
						{
							((IDocumentDeliveryView)(form)).HideBackgroundDeliveryCheckBox();
						}
					}
				}
				catch (DocumentEngineException ex)
				{
					var inner = ex.GetInnermostException();
					if (inner is DocumentMenuException)
					{
						Globals.Message.ShowError(Res.GetString("bda0caa0-2952-4065-be9a-1d7d6f926b5f", "An error occurred processing the Document Menu command:\r\n{0}", inner.Message));
					}
					else if (inner is InvalidMenuTemplateFilterException)
					{
						Globals.Message.ShowError(Res.GetString("4BB06890-2FF9-4CCB-B043-6DBA23767579", "The template filter or child document filter below is invalid. Please fix this filter in order to run this document:\r\n{0}", inner.Message));
					}
					else
					{
						var templateGeneratingException = ex.Find<TemplateGeneratingException>();
						if (templateGeneratingException != null)
						{
							Globals.Message.ShowError(templateGeneratingException.GetErrorMessage(result.DocumentCommand));
						}
						else
						{
							throw;
						}
					}
				}
				finally
				{
					if (
#if !WINZOR
						ObjectFactory.Get<TerminalService>().IsRemoteAppSession &&
#endif
						mainForm != null)
					{
						mainForm.Activate();
					}
				}
#if DEBUG
				if (Globals.IsTest)
				{
					LastRunReportInfos.AddRange(documentRunner.LastRunReportInfosForTesting);
					HasExtraFormsForTesting = hasExtraDialogFormOpened;
				}
#endif
			}
		}

#if DEBUG
		internal
#endif
		DocumentRunner GetNewDocumentRunner(object sender)
		{
			var moduleIDForSecurity = GetModuleIdentifier(sender, out var parentForm);

			return new DocumentRunner(parentForm != null ? new DocumentRunnerParentForm(parentForm) : null, moduleIDForSecurity, documentEventsForMenu, parentUserFieldList, parentSystemDefinedFieldList);
		}

		ModuleIdentifier GetModuleIdentifier(object sender, out Form parentForm)
		{
			ModuleIdentifier moduleIDForSecurity = null;

			var mainMenu = GetMainMenu(menuItem);
			if (mainMenu != null)
			{
				parentForm = GetForm(mainMenu);
				moduleIDForSecurity = GetModuleIDFromForm(parentForm);
			}
			else
			{
				parentForm = null;
				if (sender != null)
				{
					moduleIDForSecurity = TryGetModuleIDFromZFilterGrid(sender);
				}
			}

			return moduleIDForSecurity;
		}

		bool GetShowFormsFromMainThread(object sender)
		{
			bool? showFormsFromMainThread = null;

			var mainMenu = GetMainMenu(menuItem);
			if (mainMenu != null)
			{
				var parentForm = GetForm(mainMenu);
				showFormsFromMainThread = GetShowFormsFromMainThreadFromForm(parentForm);
			}
			else if (sender != null)
			{
				showFormsFromMainThread = TryGetShowFormsFromMainThreadFromZFilterGrid(sender);
			}

			return showFormsFromMainThread ?? false;
		}

		public ModuleIdentifier GetModuleIDFromMenu(Menu menu)
		{
			var form = menu != null ? GetForm(menu) : null;
			return GetModuleIDFromForm(form);
		}

		ModuleIdentifier GetModuleIDFromForm(Form form) => GetControllerFromForm(form)?.ModuleIDForDocumentSecurity;

		bool? GetShowFormsFromMainThreadFromForm(Form form) => (GetControllerFromForm(form)?.ParentModule as ZFilterModule)?.ShowFormsFromMainThread;

		ZController GetControllerFromForm(Form form)
		{
			var controllerID = form != null ? ((IZForm)form).ControllerID : null;
			return controllerID != null ? ZControllerFactory.Create(controllerID) : null;
		}

		internal ControllerID TryGetControllerID(object sender, Form form)
		{
			var controllerID = form != null ? ((IZForm)form).ControllerID : null;
			if (controllerID == null)
			{
				var grid = TryGetZFilterGrid(sender);
				controllerID = grid?.ParentModule is ZFilterModule module ? module.GetNewController()?.ID : null;
			}
			return controllerID;
		}

		ModuleIdentifier TryGetModuleIDFromZFilterGrid(object sender) => TryGetZFilterGrid(sender)?.ModuleID;

		bool? TryGetShowFormsFromMainThreadFromZFilterGrid(object sender) => TryGetZFilterGrid(sender)?.ShowFormsFromMainThread;

		ZFilterGrid TryGetZFilterGrid(object sender) => sender is ZMenuItem item ? item.ParentControl as ZFilterGrid : null;

		protected abstract IComponent GetMainMenu(T menuItem);
		protected abstract Form GetForm(IComponent component);

		#endregion

		internal abstract void SetEnabled(T menuItem, bool enabled);

		#endregion

		#region GetParentMenu

		internal T GetParentMenu(DocumentCommand item)
		{
			var folders = item.SU_MenuPathMultilingualParts;
			if (item.SU_IsSystemDefined && !item.SU_IsPublished)
			{
				var modifiedFolders = new MultilingualString[folders.Length + 1];
				modifiedFolders[0] = UnpublishedDocumentMenuItemsManager.UnpublishedDocumentsMenuText;
				Array.Copy(folders, 0, modifiedFolders, 1, folders.Length);
				folders = modifiedFolders;
			}

			T result = this.menuItem;

			foreach (var folder in folders)
			{
				if (!string.IsNullOrEmpty(folder))
				{
					var folderMenu = FindFolderMenu(folder.GetUnresolvedString(), result);
					if (folderMenu == null)
					{
						folderMenu = GetNewMenuItem(folder.GetUnresolvedString(), folder, null);
						GetItems(result).Add(folderMenu);
					}

					result = folderMenu;
				}
			}

			return result;
		}

		T FindFolderMenu(string folderName, T startingMenu)
		{
			T result = default(T);

			if (GetName(startingMenu) == folderName)
			{
				result = startingMenu;
			}
			else
			{
				var foundItem = ZBool.False;

				foreach (T item in GetItems(startingMenu))
				{
					if (GetName(item) == folderName)
					{
						foundItem = ZBool.True;
						result = item;
						break;
					}
				}
			}

			return result;
		}

		internal abstract string GetName(T menuItem);

		#endregion

		#region OrganizeLegacyDocumentsMenuItem

		void OrganizeLegacyDocumentsMenuItem()
		{
			var menuItems = GetItems(menuItem);
			var foundMenuItems = FindMenuItemsByText(menuItems, Core.Constants.DocumentEngine.MenuPaths.LegacyDocuments);

			foreach (var legacyDocumentsMenuItem in foundMenuItems)
			{
				if (menuItems.Count > 1)
				{
					menuItems.Remove(legacyDocumentsMenuItem);

					if (!IsLastMenuItemASeparator(menuItems))
					{
						menuItems.Add(GetSeparator());
					}

					menuItems.Add(legacyDocumentsMenuItem);
				}
			}
		}

		internal abstract T[] FindMenuItemsByText(IList menuItems, string text);
		internal abstract bool IsLastMenuItemASeparator(IList menuItems);
		internal abstract T GetSeparator();

		#endregion

		#endregion

		#region TaggedMenuItem

		class TaggedMenuItem
		{
			internal TaggedMenuItem(string text, EventHandler handler, string cut, DocumentCommand documentCommand, string hint, ZDocumentsMenuItemHelper<T> helper)
			{
				this.DocumentCommand = documentCommand;
				this.helper = helper;
				this.menuItem = helper.SetupTaggedMenuItem(text + documentCommand.MenuNameTag, handler, cut, new EventHandler(TaggedMenuItem_Select));
				this.fHint = Hint;
			}

			readonly T menuItem;
			readonly ZDocumentsMenuItemHelper<T> helper;

			StatusBarPanel fPanelToHint;
			void SetStatusBar()
			{
				fPanelToHint = null;
				var mainMenu = helper.GetMainMenu(menuItem);
				if (mainMenu != null)
				{
					var mainForm = (ZForm)helper.GetForm(mainMenu);
					if (mainForm != null)
					{
						foreach (var ctrl in mainForm.Controls)
						{
							if (ctrl is StatusBar)
							{
								fPanelToHint = (ctrl as StatusBar).Panels[0];
								break;
							}
						}
					}
				}
			}

			string fHint;
			internal string Hint
			{
				get { return fHint; }
				set { fHint = value; }
			}

			internal readonly DocumentCommand DocumentCommand;

			internal void UpdateShowFormsFromMainThread()
			{
				DocumentCommand.ShowFormsFromMainThread = helper.GetShowFormsFromMainThread(menuItem);
			}

			void TaggedMenuItem_Select(object sender, EventArgs e)
			{
				if (fPanelToHint == null)
				{
					SetStatusBar();
				}
				if (fPanelToHint != null)
				{
					fPanelToHint.Text = fHint;
				}
			}

			public static implicit operator T(TaggedMenuItem taggedMenuItem)
			{
				return taggedMenuItem.menuItem;
			}
		}

		protected abstract T SetupTaggedMenuItem(string text, EventHandler clickHandler, string cut, EventHandler selectHandler);

		#endregion

		#region Dispose

		internal void Dispose(bool disposing)
		{
			if (parentBusinessObject != null)
			{
				parentBusinessObject.DocumentSupporter.Factory.Saved -= new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
			}

			if (menusMaker != null)
			{
				menusMaker.MenuItemsChanged -= new EventHandler(MenusMaker_MenuItemsChanged);
			}
		}

		#endregion

		#region CustomisationMenusMaker

		internal abstract IComponent GetParent(T menuItem);
		internal abstract void Make(IList menuItemsCollection, T[] menuItemsToAdd);

		#endregion

		#region Test Access Methods
#if DEBUG

		internal void RaiseMenusMakerMenuItemsChangedForTesting()
		{
			if (menusMaker != null)
			{
				menusMaker.RaiseMenuItemsChangedForTesting();
			}
		}

		internal IDocumentEventsForMenu DocumentEventsForMenuForTesting { get { return documentEventsForMenu; } }
		internal T CreateMenuForTesting(DocumentCommand item) { return CreateMenu(item); }
		internal bool ShouldActuallyRunDocumentSetForTesting = true;
		internal Action ThrowExceptionInGetDocumentPrintSetForTesting;
		public readonly List<DocumentEngine.Testing.ReportRunInfoForTesting> LastRunReportInfos = new List<DocumentEngine.Testing.ReportRunInfoForTesting>();
		internal bool HasExtraFormsForTesting;

		internal DocumentPrintSet GetDocumentPrintSetForTesting(DocumentCommand item)
		{
			var documentRunner = GetNewDocumentRunner(null);
			documentRunner.ShouldActuallyRunDocumentSetForTesting = ShouldActuallyRunDocumentSetForTesting;
			documentRunner.ThrowExceptionInGetDocumentPrintSetForTesting = ThrowExceptionInGetDocumentPrintSetForTesting;
			LastRunReportInfos.Clear();

			return documentRunner.GetDocumentPrintSetForTesting(item);
		}

#endif
		#endregion
	}

	#region SubClasses

	public sealed class ZDocumentsMenuItemMenuHelper : ZDocumentsMenuItemHelper<MenuItem>
	{
		public ZDocumentsMenuItemMenuHelper(MenuItem menuItem = null) : base(menuItem) { }

		#region Internal Methods

		internal override string GetName(MenuItem menuItem)
		{
			return menuItem.Name;
		}

		internal override IList GetItems(MenuItem menuItem)
		{
			return menuItem.MenuItems;
		}

		internal override MenuItem GetNewMenuItem(string name, MultilingualString text, EventHandler handler)
		{
			var menuItem = new ZMenuItem(text, handler);
			menuItem.Name = name;
			return menuItem;
		}

		internal override void SetEnabled(MenuItem menuItem, bool enabled)
		{
			menuItem.Enabled = enabled;
		}

		internal override MenuItem GetSeparator()
		{
			return new ZMenuItem("-");
		}

		internal override bool IsLastMenuItemASeparator(IList menuItems)
		{
			return ((MenuItem)menuItems[menuItems.Count - 1]).Name.Equals("-");
		}

		internal override MenuItem[] FindMenuItemsByText(IList menuItems, string text)
		{
			return ((Menu.MenuItemCollection)menuItems).FindMenuItemsByText(text);
		}

		internal override IComponent GetParent(MenuItem menuItem)
		{
			return menuItem.Parent;
		}

		internal override void Make(IList menuItemsCollection, MenuItem[] menuItemsToAdd)
		{
			((Menu.MenuItemCollection)menuItemsCollection).AddRange(menuItemsToAdd);
		}

		#endregion

		protected override IComponent GetMainMenu(MenuItem menuItem)
		{
			return menuItem.GetMainMenu();
		}

		protected override Form GetForm(IComponent component)
		{
			return ((MainMenu)component).GetForm();
		}

		protected override MenuItem SetupTaggedMenuItem(string text, EventHandler clickHandler, string cut, EventHandler selectHandler)
		{
			var shortCut = (Shortcut)Enum.Parse(typeof(Shortcut), cut);
			var menuItem = new ZMenuItem(text, clickHandler, shortCut);
			menuItem.Select += selectHandler;
			menuItem.OwnerDraw = false;
			return menuItem;
		}
	}

	public sealed class ZDocumentsToolStripMenuHelper : ZDocumentsMenuItemHelper<ToolStripItem>
	{
		public ZDocumentsToolStripMenuHelper(ToolStripItem menuItem = null) : base(menuItem) { }

		#region Internal Methods

		internal override string GetName(ToolStripItem menuItem)
		{
			return menuItem.Name;
		}

		internal override IList GetItems(ToolStripItem menuItem)
		{
			return ((ToolStripMenuItem)menuItem).DropDownItems;
		}

		internal override ToolStripItem GetNewMenuItem(string name, MultilingualString text, EventHandler handler)
		{
			return new ZToolStripMenuItem(text, handler) { Name = name };
		}

		internal override void SetEnabled(ToolStripItem menuItem, bool enabled)
		{
			menuItem.Enabled = enabled;
		}

		internal override ToolStripItem GetSeparator()
		{
			return new ToolStripSeparator();
		}

		internal override bool IsLastMenuItemASeparator(IList menuItems)
		{
			return menuItems[menuItems.Count - 1] is ToolStripSeparator;
		}

		internal override ToolStripItem[] FindMenuItemsByText(IList menuItems, string text)
		{
			return ((ToolStripItemCollection)menuItems).Find(text, true);
		}

		internal override IComponent GetParent(ToolStripItem menuItem)
		{
			return menuItem.Owner;
		}

		internal override void Make(IList menuItemsCollection, ToolStripItem[] menuItemsToAdd)
		{
			((ToolStripItemCollection)menuItemsCollection).AddRange(menuItemsToAdd);
		}

		#endregion

		protected override IComponent GetMainMenu(ToolStripItem menuItem)
		{
			return GetParent(menuItem);
		}

		protected override Form GetForm(IComponent component)
		{
			return ((ToolStrip)component).FindForm();
		}

		protected override ToolStripItem SetupTaggedMenuItem(string text, EventHandler clickHandler, string cut, EventHandler selectHandler)
		{
			if (!Enum.TryParse<Shortcut>(cut, out var shortCut))
			{
				shortCut = Shortcut.None;
			}
			return new ZToolStripMenuItem(text, clickHandler + selectHandler) { ShortcutKeys = (Keys)shortCut, Name = text };
		}
	}

	#endregion
}
