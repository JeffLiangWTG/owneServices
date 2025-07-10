using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalCopy.Business;
using Enterprise.UniversalCopy.GUI.ImportService;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalCopy.GUI
{
	public abstract class UniversalCopyManager : IUniversalCopyManager, IDisposable
	{
		protected UniversalCopyManager(Type elementType, ZModule module, bool mainThreadOnlyActions = false)
			: this(elementType, module.ID, mainThreadOnlyActions)
		{
			localModule = module;
		}

		protected UniversalCopyManager(Type elementType, ModuleIdentifier moduleId, bool mainThreadOnlyActions = false)
		{
			ElementType = elementType;
			ModuleId = moduleId;
			ExecuteOnlyOnMainThread = mainThreadOnlyActions;

			InterfaceType = GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(elementType, true);
			UseExtendedEntitiesConfiguration = InterfaceType == null && elementType != null && elementType.GetCustomAttributes(typeof(UniversalCopyWithExtendedEntitiesAttribute), true).Any();
		}

		void IUniversalCopyManager.AddMenuItems(Menu rootMenu, bool includeEditMenuItems, bool includeCopySchedulesItem, bool lazyPopulate)
		{
			AddMenuItems(rootMenu, includeEditMenuItems, includeCopySchedulesItem, lazyPopulateMenuItems: lazyPopulate);
		}

		protected void AddMenuItems(Menu rootMenu, bool includeEditMenuItems, bool includeCopySchedulesItem, bool lazyPopulateMenuItems = true)
		{
			if (RootMenu != null)
			{
				throw new InvalidOperationException("Already added menu items to [" + RootMenu + "]");
			}

			RootMenu = rootMenu ?? throw new ArgumentNullException(nameof(rootMenu));

			if (AllowsUniversalCopy)
			{
				AddMenuItemsCore(includeEditMenuItems, includeCopySchedulesItem, lazyPopulateMenuItems);
			}
		}

		protected virtual void AddMenuItemsCore(bool includeEditMenuItems, bool includeCopySchedulesItem, bool lazyPopulateMenuItems)
		{
			AddCopyMenuItems(includeEditMenuItems, includeCopySchedulesItem, lazyPopulateMenuItems);
		}

		protected internal Type ElementType { get; }

		protected internal ModuleIdentifier ModuleId { get; }

		bool ExecuteOnlyOnMainThread { get; }

		protected Type InterfaceType { get; }

		protected bool UseExtendedEntitiesConfiguration { get; }

		UniversalCopyFactory CopyFactory => factory ??= new UniversalCopyFactory(ElementType, ModuleId, CopyTemplateTree_ExceptionThrown);

		UniversalCopyFactory factory;

		Dictionary<string, WeakReference<BusinessObject>> TemporaryBizos => temporaryBizos ??= new Dictionary<string, WeakReference<BusinessObject>>();

		Dictionary<string, WeakReference<BusinessObject>> temporaryBizos;

		#region Security

		public virtual bool AllowsUniversalCopy
		{
			get
			{
				bool result = InterfaceType != null || UseExtendedEntitiesConfiguration;
				return result;
			}
		}

		internal UniversalCopySecurity Security
		{
			get
			{
				return security ?? (security = new UniversalCopySecurity(LocalModule));
			}
		}

		UniversalCopySecurity security;

		/// <summary>
		/// Returns module for the grid or for current ZForm.
		/// </summary>
		internal protected virtual ZModule LocalModule
		{
			get { return localModule ?? (localModule = GetNewLocalModule()); }
		}

		ZModule localModule;

		protected virtual ZModule GetNewLocalModule()
		{
			if (ModuleId != null && ModuleId != ModuleIDs.NotAssigned)
			{
				return ZModuleFactory.Instance.Create(ModuleId);
			}
			return null;
		}

		#endregion

		#region Menus

		#region Populate menu

		Menu RootMenu { get; set; }
		MenuItem SchedulesMenuItem { get; set; }
		public bool IsCopyMenuItemPopulated { get; internal set; }

		readonly object tagSplitterMenuItem = new object();
		readonly object tagCustomizeMenuItem = new object();
		readonly object tagNewMenuItem = new object();
		readonly object tagNewFromMenuItem = new object();
		readonly object tagSchedulesSplitterMenuItem = new object();
		readonly object tagSchedulesMenuItem = new object();
		readonly object tagImportMenuItem = new object();

		void AddCopyMenuItems(bool includeEditMenuItems, bool includeCopySchedulesItem, bool lazyPopulateMenuItems)
		{
			if (includeEditMenuItems)
			{
				var newMenuHandler = Security.CanCreateEditDeletePrivate ? ExecuteOnCorrectThread(NewTemplateMenuClicked) : ExecuteOnCorrectThread(ShowDisableCreateEditDeletePriSecurityMessageOnMenuHandler);
				RootMenu.MenuItems.Add(new ZMenuItem("-") { Tag = tagSplitterMenuItem });
				RootMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("dd510fae-007f-47ad-9472-beea0253ce43", "New Copy Template"), newMenuHandler) { Tag = tagNewMenuItem });
				RootMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("2c055098-7232-4fb9-abd7-724b8717eea5", "Edit Copy Template")) { Tag = tagCustomizeMenuItem });
				RootMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("a51c445d-29e5-496c-83eb-9a8e57b29ec3", "New Copy Template From")) { Tag = tagNewFromMenuItem });

				if (includeCopySchedulesItem)
				{
					RootMenu.MenuItems.Add(new ZMenuItem("-") { Tag = tagSchedulesSplitterMenuItem, Name = "CopySchedulesSplitter" });
					SchedulesMenuItem = new ZMenuItem(ResString.GetMultilingualString("1ae29d94-caec-4e50-b1a8-8b766d93fa7e", "Copy Schedules")) { Tag = tagSchedulesMenuItem, Name = "CopySchedules" };
					RootMenu.MenuItems.Add(SchedulesMenuItem);
					var createScheduleMenuHandler = Security.CanCreateEditDeletePrivate ? ExecuteOnCorrectThread(CreateScheduleMenuClicked) : ExecuteOnCorrectThread(ShowDisableCreateEditDeletePriSecurityMessageOnMenuHandler);
					SchedulesMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("19db4c15-e9df-4f83-b2cd-b2f54ef0da18", "Create Copy Schedule"), createScheduleMenuHandler));
					SchedulesMenuItem.MenuItems.Add(new ZMenuItem("-"));

					if (lazyPopulateMenuItems)
					{
						SchedulesMenuItem.Select += ExecuteOnCorrectThread(SchedulesMenuSelect);
					}
					else
					{
						ExecuteOnCorrectThread(SchedulesMenuSelect).Invoke(null, EventArgs.Empty);
					}
				}
			}

			if (lazyPopulateMenuItems && RootMenu is MenuItem)
			{
				((MenuItem)RootMenu).Select += ExecuteOnCorrectThread(DelayedPopulateCopyMenuItemsHandler);
			}
			else if (lazyPopulateMenuItems && RootMenu is ContextMenu)
			{
				((ContextMenu)RootMenu).Popup += ExecuteOnCorrectThread(DelayedPopulateCopyMenuItemsHandler);
			}
			else
			{
				PopulateCopyMenuItems();
			}
		}

		protected EventHandler ExecuteOnCorrectThread(Action<object, EventArgs> universalCopyAction)
		{
			if (ExecuteOnlyOnMainThread)
			{
				return (object sender, EventArgs e) => MainThreadRunner.RunOnMainThread(() => universalCopyAction(sender, e));
			}
			else
			{
				return (object sender, EventArgs e) => universalCopyAction(sender, e);
			}
		}

		void DelayedPopulateCopyMenuItemsHandler(object sender, EventArgs e)
		{
			if (!IsCopyMenuItemPopulated)
			{
				using (PerformanceStatisticsCollector.StartMonitoring("PopulateUniversalCopyManagerMenuItems"))
				{
					PopulateCopyMenuItems();
				}
			}
		}

		protected void PopulateCopyMenuItems()
		{
			PopulateCopyMenuItemsCore();
			IsCopyMenuItemPopulated = true;
		}

		protected virtual void PopulateCopyMenuItemsCore()
		{
			PopulateCopyMenuItems(RootMenu.MenuItems);
		}

		protected virtual bool ShouldIncludeFilteredCopyMenuItems
		{
			get { return true; }
		}

		protected void PopulateCopyMenuItems(Menu.MenuItemCollection menuItemCollection)
		{
			var copyMenuHandler = Security.CanRun ? ExecuteOnCorrectThread(CopyMenuClicked) : ExecuteOnCorrectThread(ShowDisabledRunSecurityMessageOnMenuHandler);
			var editMenuHandler = Security.CanCreateEditDeletePrivate ? ExecuteOnCorrectThread(CustomiseTemplateMenuClicked) : ExecuteOnCorrectThread(ShowDisableCreateEditDeletePriSecurityMessageOnMenuHandler);
			var newFromMenuHandler = Security.CanCreateEditDeletePrivate ? ExecuteOnCorrectThread(NewTemplateFromMenuClicked) : ExecuteOnCorrectThread(ShowDisableCreateEditDeletePriSecurityMessageOnMenuHandler);

			var specialItemsCount = 0;
			ZMenuItem specialItemsSplitter = null, customizeCopyMenuItem = null, newFromCopyMenuItem = null;

			for (var i = menuItemCollection.Count - 1; i >= 0; i--)
			{
				var item = (ZMenuItem)menuItemCollection[i];

				if (item.Tag == tagSplitterMenuItem)
				{
					specialItemsSplitter = item;
					specialItemsCount++;
				}
				else if (item.Tag == tagCustomizeMenuItem)
				{
					customizeCopyMenuItem = item;
					customizeCopyMenuItem.MenuItems.Clear();
					specialItemsCount++;
				}
				else if (item.Tag == tagNewFromMenuItem)
				{
					newFromCopyMenuItem = item;
					newFromCopyMenuItem.MenuItems.Clear();
					specialItemsCount++;
				}
				else if (item.Tag == tagNewMenuItem)
				{
					specialItemsCount++;
				}
				else if (item.Tag == tagSchedulesSplitterMenuItem)
				{
					specialItemsCount++;
				}
				else if (item.Tag == tagSchedulesMenuItem)
				{
					specialItemsCount++;
				}
				else
				{
					menuItemCollection.RemoveAt(i);
				}
			}

			foreach (var copyTemplate in GetStoredCopyConfiguration())
			{
				var templateMenuItemPathParts = copyTemplate.CopyTemplateTree.ConfigurationName.ToString().Split('\\');
				var templateMenuItemText = copyTemplate.S9_IsPublished | copyTemplate.S9_IsSystem
					? "[" + templateMenuItemPathParts[templateMenuItemPathParts.Length - 1] + "]"
					: templateMenuItemPathParts[templateMenuItemPathParts.Length - 1];

				if (copyTemplate.IsActive &&
					copyTemplate.CopyTemplateTree.ConfigurationSource != ConfigurationSourceCodes.FilteredRecord &&
					copyTemplate.CopyTemplateTree.ConfigurationSource != ConfigurationSourceCodes.NominatedRecord)
				{
					var menuItem = CreateNewTemplateMenuItem(templateMenuItemText, copyMenuHandler, copyTemplate);
					AddCopyConfigurationMenu(menuItem, templateMenuItemPathParts, menuItemCollection, specialItemsCount);
				}

				if (newFromCopyMenuItem != null)
				{
					var menuItem = CreateNewTemplateMenuItem(templateMenuItemText, newFromMenuHandler, copyTemplate);
					AddCopyConfigurationMenu(menuItem, templateMenuItemPathParts, newFromCopyMenuItem.MenuItems, 0);
				}

				if (!copyTemplate.S9_IsSystem && customizeCopyMenuItem != null)
				{
					var menuItem = CreateNewTemplateMenuItem(templateMenuItemText, editMenuHandler, copyTemplate);
					AddCopyConfigurationMenu(menuItem, templateMenuItemPathParts, customizeCopyMenuItem.MenuItems, 0);
				}
			}

			if (specialItemsSplitter != null)
			{
				specialItemsSplitter.Visible = menuItemCollection.Count > specialItemsCount;
			}

			if (customizeCopyMenuItem != null)
			{
				customizeCopyMenuItem.Visible = customizeCopyMenuItem.MenuItems.Count > 0;
			}

			if (newFromCopyMenuItem != null)
			{
				if (newFromCopyMenuItem.MenuItems.Count > 0)
				{
					newFromCopyMenuItem.MenuItems.Add(new ZMenuItem("-"));
				}
				var importMenuHandler = Security.CanCreateEditDeletePrivate ? ExecuteOnCorrectThread(ImportTemplateMenuClicked) : ExecuteOnCorrectThread(ShowDisableCreateEditDeletePriSecurityMessageOnMenuHandler);
				newFromCopyMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("b5bc40a4-3198-4dc5-a200-7d0a988ff602", "Import from File"), importMenuHandler) { Tag = tagImportMenuItem });
			}
		}

		ZMenuItem CreateNewTemplateMenuItem(string menuText, EventHandler menuHandler, UniversalCopyTemplate menuItemTag)
		{
			return new ZMenuItem(menuText, menuHandler) { Tag = menuItemTag };
		}

		void AddCopyConfigurationMenu(ZMenuItem templateMenuItem, string[] templatePathParts, Menu.MenuItemCollection topLevelMenuItemCollection, int indexOffset)
		{
			var bottomLevelMenuItemCollection = topLevelMenuItemCollection;

			if (templatePathParts.Length > 1)
			{
				var topMostMenuItem = GetMenuItemWithoutHotKey(topLevelMenuItemCollection, templatePathParts[0], indexOffset);
				bottomLevelMenuItemCollection = GetBottomLevelMenuItems(topMostMenuItem, templatePathParts);
				indexOffset = 0;
			}

			bottomLevelMenuItemCollection.Add(bottomLevelMenuItemCollection.Count - indexOffset, templateMenuItem);
		}

		Menu.MenuItemCollection GetBottomLevelMenuItems(MenuItem topMostMenuItem, string[] menuPathParts)
		{
			var currentLevelMenuItems = topMostMenuItem.MenuItems;

			for (var i = 1; i < menuPathParts.Length - 1; ++i)
			{
				var menuItemOnCurrentLevel = GetMenuItemWithoutHotKey(currentLevelMenuItems, menuPathParts[i], 0);
				currentLevelMenuItems = menuItemOnCurrentLevel.MenuItems;
			}

			return currentLevelMenuItems;
		}

		ZMenuItem GetMenuItemWithoutHotKey(Menu.MenuItemCollection collection, string text, int indexOffset)
		{
			var menuItem =  collection.Cast<ZMenuItem>().FirstOrDefault(mi => mi.Text.Replace("&", string.Empty).Equals(text.Replace("&", string.Empty), StringComparison.InvariantCultureIgnoreCase) && mi.Tag is null );

			if (menuItem == null)
			{
				menuItem = new ZMenuItem(text);
				collection.Add(collection.Count - indexOffset, menuItem);
			}

			return menuItem;
		}

		#endregion
		#region Menu Handlers
		#region Copy
		#region CopyMenuClicked event handler

		internal void CopyMenuClicked(object sender, EventArgs e)
		{
			IEnumerable<BusinessObject> copyTargets;
			if (!CopyMenuClicked_TryGetCopyTargets(out copyTargets))
			{
				return;
			}

			var copyTargetsWhichDoNotSupportUniversalCopy = copyTargets.OfType<IUniversalCopySelectivelySupportable>().Where(t => !t.SupportsUniversalCopy).ToArray();

			if (copyTargetsWhichDoNotSupportUniversalCopy.Any())
			{
				var caption = Res.GetString("30d4a53c-e1ae-4c80-a4f8-b9d26a7ef25b", "Cannot perform Universal Copy");
				var message = new StringBuilder();

				if (copyTargetsWhichDoNotSupportUniversalCopy.Length > 1)
				{
					message.Append(Res.GetString("eb34e9af-35b3-41c2-9063-af7eff38d8b1", "Universal Copy cannot be performed for the following reasons:"));
				}
				else
				{
					message.Append(Res.GetString("12fe7aa4-0bb6-4841-a41b-8cbe030d96ef", "Universal Copy cannot be performed for the following reason:"));
				}

				foreach (var target in copyTargetsWhichDoNotSupportUniversalCopy)
				{
					message.AppendLine();
					message.Append(target.ReasonForNotSupportingUniversalCopy);
				}

				Globals.Message.ShowError(message.ToString(), caption);

				return;
			}

			MenuItem menuItem = (sender is IConvertedFromMenuItem convertedMenuItem) ? convertedMenuItem.SourceMenuItem : sender as MenuItem;

			if (menuItem.Tag is UniversalCopyTemplate copyTemplate)
			{
				if (copyTemplate.CopyTemplateTree.CopyTemplateNode.HasData())
				{
					// Extend is needed to restore non-serialized reflection data.
					CopyFactory.ExtendTemplate(copyTemplate, InterfaceType);
					CopyMenuClicked(copyTemplate.CopyTemplateTree.CopyTemplateNode, copyTargets);
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("f75c1e1c-de30-4d60-a847-922497d075db", "Selected Copy Template has nothing set to copy and cannot be used."));
				}
			}
			else
			{
				ErrorReporter.ReportOnce("UniversalCopyManager_CopyMenuClicked_NullConfiguration", "CopyTemplate [stored in MenuItem.Tag] is null, menu item: " + (menuItem != null ? menuItem.Text : "<NULL>"));
			}
		}

		protected abstract bool CopyMenuClicked_TryGetCopyTargets(out IEnumerable<BusinessObject> copyTargets);

		#endregion

		#region CopyMenuClicked

		internal void CopyMenuClicked(CopyTemplateTree configurationTree, IEnumerable<BusinessObject> selectedBizos)
		{
			if (!CopyMenuClicked_CheckCanCopy())
			{
				return;
			}

			if (configurationTree != null)
			{
				var copyManager = new BusinessObjectCopyManager(GetDefaultFactoryForCopyManager());
				copyManager.Notifications = new Notifications();
				copyManager.GetFilterBusinessObjectMethod = GetFilter;

				foreach (var selectedElement in selectedBizos)
				{
					try
					{
						if (selectedElement is IUniversalCopyValidationStrategy validationStrategy)
						{
							var validationErrorMessages = validationStrategy.ValidateUniversalCopyPreconditions(configurationTree);

							if (!string.IsNullOrEmpty(validationErrorMessages))
							{
								Globals.Message.ShowWarning(validationErrorMessages);

								return;
							}
						}

						var sourceElement = CopyMenuClicked_GetSourceElement(selectedElement);

						var copyResult = copyManager.Copy(sourceElement, configurationTree);
						if (copyResult.Object == null)
						{
							ShowElementWasNotCopiedInformation();

							return;
						}

						var newElement = (BusinessObject)copyResult.Object;
						if (sourceElement.Factory != selectedElement.Factory)
						{
							sourceElement.CancelChanges();

							var notifications = copyManager.Notifications.ToString();
							if (!string.IsNullOrEmpty(notifications))
							{
								Globals.Message.ShowWarning(
									Res.GetString("90aa32f5-20ec-46ea-9d89-eab81ddfcb66",
										"There were errors during copy process, you need to review and fix them if necessary:") +
									"\r\n\r\n" + notifications,
									Res.GetString("20c1e7a4-549c-462f-9c82-e8306f6825f6", "Universal Copy"));
							}
						}

						newElement.IsFromUniversalCopy = true;

						CopyMenuClicked_OnNewElement(newElement);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						var ucException = ex as UniversalCopyAbortException ?? ex.InnerException as UniversalCopyAbortException;
						if (ucException != null)
						{
							Globals.Message.ShowError(ucException.Message, Res.GetString("9f762b67-6f29-4955-a231-c838d03103bd", "Universal Copy error"));
							return;
						}
						else
						{
							Globals.Message.ShowError(ex.Message, Res.GetString("9f762b67-6f29-4955-a231-c838d03103bd", "Universal Copy error"));
							ErrorReporter.ReportOnce("Universal Copy error: " + ex.Message, ex);
							return;
						}
					}
				}
			}
		}

		protected virtual BusinessObjectFactory GetDefaultFactoryForCopyManager()
		{
			if (LocalModule is ZFilterModule filterModule)
			{
				return filterModule.GetNewController(null)?.Factory;
			}
			return null;
		}

		protected virtual bool CopyMenuClicked_CheckCanCopy()
		{
			return true;
		}

		protected virtual BusinessObject CopyMenuClicked_GetSourceElement(BusinessObject selectedElement)
		{
			return selectedElement;
		}

		protected abstract void CopyMenuClicked_OnNewElement(BusinessObject newElement);

		public event EventHandler<FormShowingForElementArgs> FormShowingForNewElement;
		protected void OnFormShowingForNewElement(FormShowingForElementArgs args)
		{
			FormShowingForNewElement?.Invoke(this, args);
		}

		protected void ShowElementWasNotCopiedInformation()
		{
			Globals.Message.ShowInformation(Res.GetString("fd6573be-c75a-4a04-9790-5339346623f2", "Element was not copied. Check that the copy template is correctly configured."));
		}

		protected BusinessObject ImportIntoAnotherFactory(BusinessObject selectedElement, Type elementType, BusinessObjectFactory otherFactory)
		{
			if (selectedElement is ITemplateRecordProvider templateRecordProvider && templateRecordProvider.IsTemplateRecord)
			{
				return GetInstantiateFromTemplateRecord(templateRecordProvider, elementType, otherFactory);
			}
			else
			{
				return otherFactory.ImportFromAnotherFactory(selectedElement, elementType);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1052:DoNotCaseFactoryLoadMethod", Justification = "Baseline")]
		protected BusinessObject GetInstantiateFromTemplateRecord(ITemplateRecordProvider templateRecordProvider, Type elementType, BusinessObjectFactory otherFactory)
		{
			var otherTemplateRecord = (StmTemplateRecord)otherFactory.ImportFromAnotherFactory((StmTemplateRecord)templateRecordProvider.TemplateRecord, typeof(StmTemplateRecord));

			return templateRecordProvider.InstantiateFromTemplateRecord(otherFactory, elementType, otherTemplateRecord);
		}

		#endregion

		#endregion

		#region Schedule

		void SchedulesMenuSelect(object sender, EventArgs e)
		{
			PopulateSchedulesMenuItem();
		}

		void PopulateSchedulesMenuItem()
		{
			var editSchduleMenuHandler = Security.CanCreateEditDeletePrivate ? ExecuteOnCorrectThread(EditScheduleMenuClicked) : ExecuteOnCorrectThread(ShowDisableCreateEditDeletePriSecurityMessageOnMenuHandler);
			for (int i = SchedulesMenuItem.MenuItems.Count - 1; i > 1; i--)
			{
				SchedulesMenuItem.MenuItems.RemoveAt(i);
			}
			SchedulesMenuItem.MenuItems[1].Visible = false;

			BusinessObject scheduleTarget;
			if (SchedulesMenuSelect_TryGetScheduleTarget(out scheduleTarget))
			{
				foreach (var schedule in CopyFactory.LoadCopySchedules(scheduleTarget))
				{
					SchedulesMenuItem.MenuItems[1].Visible = true;
					SchedulesMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("54a6644e-4b82-4e8f-ad87-2e822093302c", "Edit: {0}", schedule.S5_ScheduleDescription), editSchduleMenuHandler) { Tag = schedule.PK });
				}
			}
		}

		protected abstract bool SchedulesMenuSelect_TryGetScheduleTarget(out BusinessObject scheduleTarget);

		internal void CreateScheduleMenuClicked(object sender, EventArgs e)
		{
			BusinessObject copyObject;
			if (!CreateScheduleMenuClicked_TryGetScheduleTarget(out copyObject))
			{
				return;
			}

			var selectivelySupportable = copyObject as IUniversalCopySelectivelySupportable;
			var supportsUniversalCopy = selectivelySupportable?.SupportsUniversalCopy ?? true;

			if (!supportsUniversalCopy)
			{
				var caption = Res.GetString("bf007c43-d4df-4b18-bcf4-509e92316a85", "Cannot create a copy schedule for this object");
				var message = selectivelySupportable.ReasonForNotSupportingUniversalCopy;

				Globals.Message.ShowError(message, caption);

				return;
			}

			var copyTask = new BusinessObjectFactory().New<StmUniversalCopyScheduleTask>();
			copyTask.Parent.GridContext = CopyFactory.GetContextKey();
			copyTask.Parent.CopyObject = copyObject;
			copyTask.Parent.RemoveIgnoreElement += CopyFactory.RemoveIgnoreElementIfNeeded;
			if (copyTask.Parent.HasErrors)
			{
				Globals.Message.ShowError(Res.GetString("aa2c542f-1968-48bb-8e48-16d2ce2c04cc", "Copy schedule cannot be created:") + " " + string.Join("\r\n", copyTask.Parent.NotificationsIncludingChildren.Where(item => item.Type == CargoWise.ComponentModel.NotificationType.Error).Select(item => item.Message).ToArray()));
			}
			else
			{
				ZFormModaliser.ShowDialogAndDispose(new UniversalCopyScheduleForm(copyTask));
			}
		}

		protected abstract bool CreateScheduleMenuClicked_TryGetScheduleTarget(out BusinessObject scheduleTarget);

		internal void EditScheduleMenuClicked(object sender, EventArgs e)
		{
			ZGuid tag;
			var menuItem = sender as ZMenuItem;
			if (menuItem != null)
			{
				tag = (ZGuid)menuItem.Tag;
			}
			else
			{
				var toolStripMenuItem = sender as ZToolStripMenuItem;
				if (toolStripMenuItem != null)
				{
					tag = (ZGuid)toolStripMenuItem.Tag;
				}
				else
				{
					throw new InvalidOperationException("Only ZMenuItem and ZToolStripMenuItem are supported controls as senders for this method. Actual control type: " + sender.GetType().FullName);
				}
			}
			var copyTask = new BusinessObjectFactory().Load<StmUniversalCopyScheduleTask>(tag);
			ZFormModaliser.ShowDialogAndDispose(new UniversalCopyScheduleForm(copyTask));
		}

		#endregion

		#region Customize

		void CustomiseTemplateMenuClicked(object sender, EventArgs e)
		{
			MenuItem menuItem = (sender is IConvertedFromMenuItem) ? ((IConvertedFromMenuItem)sender).SourceMenuItem : sender as MenuItem;
			UniversalCopyTemplate copyTemplate = menuItem != null ? menuItem.Tag as UniversalCopyTemplate : null;
			if (copyTemplate != null)
			{
				new UniversalCopyTemplateForm(CopyFactory.GetCopyTemplateInLocalFactoryAndExtend(copyTemplate, InterfaceType), this).Show();
			}
			else
			{
				ErrorReporter.ReportOnce("UniversalCopyManager_CustomizeMenuClicked_NullConfiguration", "CopyTemplate [stored in MenuItem.Tag] is null, menu item: " + (menuItem != null ? menuItem.Text : "<NULL>"));
			}
		}

		#endregion

		#region New

		void NewTemplateMenuClicked(object sender, EventArgs e)
		{
			if (InterfaceType != null || UseExtendedEntitiesConfiguration)
			{
				new UniversalCopyTemplateForm(CopyFactory.GetNewCopyTemplate(InterfaceType), this).Show();
			}
		}

		void CopyTemplateTree_ExceptionThrown(object sender, CopyTemplateTree.ExceptionThrownEventArgs e)
		{
			if (DialogResult.Yes == Globals.Message.Show(Res.GetString("d25a9307-5e13-4c88-ac47-341bb2ffd216", "{0}\r\nContinue and skip this element?", e.Message), Res.GetString("72343db3-8d52-45ed-bbe3-f4d0c0fab602", "Question"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes))
			{
				e.Continue = true;
			}
		}

		#endregion

		#region NewTemplateFrom

		void NewTemplateFromMenuClicked(object sender, EventArgs e)
		{
			MenuItem menuItem = (sender is IConvertedFromMenuItem) ? ((IConvertedFromMenuItem)sender).SourceMenuItem : sender as MenuItem;
			UniversalCopyTemplate copyTemplate = menuItem != null ? menuItem.Tag as UniversalCopyTemplate : null;
			if (copyTemplate != null)
			{
				new UniversalCopyTemplateForm(CopyFactory.GetNewCopyTemplateFrom(copyTemplate, InterfaceType), this).Show();
			}
			else
			{
				ErrorReporter.ReportOnce("UniversalCopyManager_CustomizeMenuClicked_NullConfiguration", "CopyTemplate [stored in MenuItem.Tag] is null, menu item: " + (menuItem != null ? menuItem.Text : "<NULL>"));
			}
		}

		#endregion

		#region Import

		void ImportTemplateMenuClicked(object sender, EventArgs e)
		{
			var copyTemplateTree = GetCopyTemplateTreeFromImport();
			if (copyTemplateTree != null && AllowsUniversalCopy)
			{
				var copyTemplate = GetNewCopyTemplateFromImport(copyTemplateTree);
				copyTemplate.CopyTemplateTree.ConfigurationName = string.Empty;
				new UniversalCopyTemplateForm(copyTemplate, this).Show();
			}
		}

		protected virtual CopyTemplateTree GetCopyTemplateTreeFromImport()
		{
			var service = new UniversalCopyXmlImportService();
			var copyTemplateTree = service.Import(false);
			return copyTemplateTree;
		}

		public UniversalCopyTemplate GetNewCopyTemplateFromImport(CopyTemplateTree copyTemplateTree)
		{
			return CopyFactory.GetNewCopyTemplateFromImport(copyTemplateTree, InterfaceType);
		}

		#endregion

		#region Security warnings

		protected void ShowDisabledRunSecurityMessageOnMenuHandler(object sender, EventArgs e)
		{
			if (Security.UniversalCopyRunCheckpoint != null)
			{
				Security.UniversalCopyRunCheckpoint.ShowError();
			}
		}

		void ShowDisableCreateEditDeletePriSecurityMessageOnMenuHandler(object sender, EventArgs e)
		{
			if (Security.UniversalCopyCEDPrivateCheckpoint != null)
			{
				Security.UniversalCopyCEDPrivateCheckpoint.ShowError();
			}
		}

		#endregion

		#endregion

		#endregion

		#region Filters

		[SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToLower")]
		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant, internal error message")]
		public FilterStripBusinessObject GetFilter(CopyTemplateNode copyTemplateNode, IEnumerable<string> path)
		{
			FilterStripBusinessObject filter = null;

			var collectionType = GetComponentTypeFromPath(path, false);
			if (collectionType != null)
			{
				filter = GetFilter(collectionType);
			}
			else if (copyTemplateNode is CopyTemplateTree)
			{
				if (ModuleId != null)
				{
					filter = GetFilter(ModuleId);
				}
			}

			if (filter == null)
			{
				var tableName = GetTableName(copyTemplateNode);
				if (tableName != null)
				{
					filter = GetFilter(tableName);

					var schemaFiterStripBizo = filter as SchemaFilterStripBusinessObject;
					if (schemaFiterStripBizo != null)
					{
						var currentNode = copyTemplateNode;
						var wrappedNode = currentNode as WrappedCopyTemplateNode;
						while (wrappedNode != null)
						{
							currentNode = wrappedNode.InnerNode;
							wrappedNode = currentNode as WrappedCopyTemplateNode;
						}

						if (currentNode is EntityCopyTemplateNode entityNode)
						{
							var bizObjType = GetComponentTypeFromPath(path, true) ?? BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(tableName), false);
							var bizObjTypeProps = GetBusinessObjectProperties(bizObjType);
							schemaFiterStripBizo.ColumnNamesToInclude = new Dictionary<string, System.Collections.IList>();
							foreach (var childNode in entityNode.Nodes)
							{
								try
								{
									if (childNode is PropertyCopyTemplateNode propertyCopyTemplateNode)
									{
										System.Collections.IList bizoList = null;
										if (propertyCopyTemplateNode.PropertyType?.ToLower() == "string")
										{
											bizoList = GetBusinessObjectList(bizObjTypeProps, childNode.Name, bizObjType);
										}
										schemaFiterStripBizo.ColumnNamesToInclude.Add(childNode.Name, bizoList);
									}
								}
								catch (ArgumentException ex) when (!ex.IsCriticalException())
								{
									// If the duplicate key issue has been removed, this block of code can be removed.
									// Also, please remove the code added in [WI00863933]
									var errorMessage = new StringBuilder();
									errorMessage.AppendLine(ex.Message);
									errorMessage.AppendLine((NoResString)"Entity: " + entityNode.Name);
									errorMessage.AppendLine((NoResString)"Property: " + childNode.Name);
									errorMessage.AppendLine(string.Join(" | ", entityNode.Nodes.Select(x =>
									{
										if (x is RelatedEntityCopyTemplateNode parent)
										{
											return $"(Name: {parent.Name}, RelatedEntityTableName: {parent.RelatedEntityTableName})";
										}

										return x.Name;
									})));
									ErrorReporter.ReportOnce("UniversalCopyManager_GetFilter_DictionaryArgumentException", errorMessage.ToString());
								}
							}
						}
					}
				}
			}

			if (filter != null)
			{
				filter.LoadLayout(null);

				EntityFilter entityfilter = null;

				CopyTemplateTree templateTree;
				CollectionCopyTemplateNode collectionNode;

				if ((templateTree = copyTemplateNode as CopyTemplateTree) != null)
				{
					entityfilter = templateTree.Filter;
				}
				else if ((collectionNode = copyTemplateNode as CollectionCopyTemplateNode) != null)
				{
					entityfilter = collectionNode.Filter;
				}

				if (entityfilter != null)
				{
					LoadFilterLayout(filter, entityfilter);
				}
			}

			return filter;
		}

		public string LoadFilterLayout(FilterStripBusinessObject filterBusinessObject, EntityFilter entityFilter)
		{
			if (filterBusinessObject != null && entityFilter != null && entityFilter.FilterTypeId == ZArchitectureFilterId)
			{
				var filterDataParts = entityFilter.FilterData.Split(new[] { LayoutAndValuesSeparator }, StringSplitOptions.None);
				if (filterDataParts.Length == 2)
				{
					using (filterBusinessObject.SuspendSettingHasChangesIncludingChildren())
					using (var layoutString = new StringReader(filterDataParts[0]))
					using (var dataString = new StringReader(filterDataParts[1]))
					using (var layoutReader = new XmlTextReader(layoutString) { WhitespaceHandling = WhitespaceHandling.None })
					using (var dataReader = new XmlTextReader(dataString) { WhitespaceHandling = WhitespaceHandling.None })
					{
						filterBusinessObject.FilterStrips.RemoveAndDeleteAll();
						filterBusinessObject.FilterStrips.LoadFromXml(layoutReader, dataReader);
					}
				}
			}

			return entityFilter != null ? entityFilter.OrderBy : string.Empty;
		}

		public EntityFilter SaveFilterLayout(FilterStripBusinessObject filterBusinessObject, string orderBy)
		{
			EntityFilter entityFilter = new EntityFilter();
			entityFilter.FilterTypeId = ZArchitectureFilterId;
			entityFilter.OrderBy = orderBy;

			StringBuilder filterData = new StringBuilder();

			using (MemoryStream ms = new MemoryStream())
			{
				filterBusinessObject.FilterStrips.WriteLayoutToXml(ms);
				ms.Position = 0;
				using (StreamReader sr = new StreamReader(ms))
				{
					filterData.Append(sr.ReadToEnd());
				}
			}

			filterData.Append(LayoutAndValuesSeparator);

			using (MemoryStream ms = new MemoryStream())
			{
				filterBusinessObject.FilterStrips.WriteLayoutValuesToXml(ms);
				ms.Position = 0;
				using (StreamReader sr = new StreamReader(ms))
				{
					filterData.Append(sr.ReadToEnd());
				}
			}

			entityFilter.FilterData = filterData.ToString();

			return entityFilter;
		}

		const string ZArchitectureFilterId = "ZArchitectureFilter";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant separator literal")]
		const string LayoutAndValuesSeparator = "~~~LayoutAndValuesSeparator~~~";

		public Type GetComponentTypeFromPath(IEnumerable<string> path, bool returnElementType)
		{
			return GetComponentTypeFromPath(ElementType, path, returnElementType);
		}

		public Type GetComponentTypeFromPath(Type rootElementType, IEnumerable<string> path, bool returnElementType)
		{
			if (rootElementType != null)
			{
				Type currentCollectionType = null;
				Type currentElementType = rootElementType;

				foreach (string propertyName in path)
				{
					var actualDataPropertyType = GetActualDataPropertyType(currentElementType, propertyName);
					if (actualDataPropertyType != null)
					{
						currentElementType = actualDataPropertyType;
					}
					else
					{
						currentElementType = GetPropertyType(currentElementType, propertyName);
					}

					if (currentElementType != null && typeof(IBusinessObjectCollection).IsAssignableFrom(currentElementType) && typeof(BusinessObjectCollection) != currentElementType)
					{
						currentCollectionType = currentElementType;
						currentElementType = BusinessObjectCollection.GetElementTypeFromCollectionType(currentCollectionType);
					}
					else
					{
						currentCollectionType = null;
					}

					if (currentElementType == null)
					{
						break;
					}
				}

				Type componentType = returnElementType ? currentElementType : currentCollectionType;
				if (componentType != null)
				{
					return componentType;
				}
			}

			return null;
		}

		Type GetActualDataPropertyType(Type currentElementType, string propertyName)
		{
			var propertyInfo = CopyTemplateTree.GetProperty(currentElementType, propertyName);
			if (propertyInfo != null)
			{
				var actualDataPropertyTypeAttribute = (ActualDataPropertyTypeAttribute)System.Attribute.GetCustomAttributes(propertyInfo, typeof(ActualDataPropertyTypeAttribute), true).FirstOrDefault();
				if (actualDataPropertyTypeAttribute != null)
				{
					return actualDataPropertyTypeAttribute.DataPropertyType;
				}
			}

			return null;
		}

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public ModuleIdentifier GetComponentPropertyListModuleIdFromPath(IEnumerable<string> path, string propertyName, out Type collectionType)
		{
			collectionType = GetComponentPropertyListTypeFromPath(path, propertyName);
			if (collectionType != null)
			{
				return GetModuleIdentifier(collectionType);
			}

			return ModuleIDs.NotAssigned;
		}

		public Type GetComponentPropertyListTypeFromPath(IEnumerable<string> path, string propertyName)
		{
			var componentType = GetComponentTypeFromPath(path, true);
			if (componentType != null)
			{
				var propertyInfo = GetPropertyInfo(componentType, propertyName);
				if (propertyInfo != null)
				{
					var listOverrideAttribute = (UniversalCopyListOverrideAttribute)System.Attribute.GetCustomAttributes(propertyInfo, typeof(UniversalCopyListOverrideAttribute), true).FirstOrDefault();
					if (!string.IsNullOrEmpty(listOverrideAttribute?.DataPath))
					{
						return GetComponentTypeFromPath(componentType, listOverrideAttribute.DataPath.Split('.', '+'), false);
					}

					var listAttribute = (ListAttribute)System.Attribute.GetCustomAttributes(propertyInfo, typeof(ListAttribute), true).FirstOrDefault();
					var listMember = MetadataAccessor.GetListMember(listAttribute, propertyName);
					if (!string.IsNullOrEmpty(listMember))
					{
						return GetComponentTypeFromPath(componentType, listMember.Split('.', '+'), false);
					}
				}
			}
			return null;
		}

		ModuleIdentifier GetModuleIdentifier(Type collectionType)
		{
			var attributes = (ModuleIDAttribute[])collectionType.GetCustomAttributes(typeof(ModuleIDAttribute), true);
			return attributes.Length > 0 ? attributes[0].ModuleIdentifier : ModuleIDs.NotAssigned;
		}

#if DEBUG
		public
#endif
		Type GetPropertyType(Type componentType, string propertyName)
		{
			return GetPropertyInfo(componentType, propertyName)?.PropertyType;
		}

		PropertyInfo GetPropertyInfo(Type componentType, string propertyName)
		{
			var propertyInfo = CopyTemplateTree.GetProperty(componentType, propertyName);
			if (propertyInfo == null)
			{
				foreach (UniversalCopyAssociateElementAttribute associationAttribute in componentType.GetCustomAttributes(typeof(UniversalCopyAssociateElementAttribute), true))
				{
					if (string.Equals(associationAttribute.DefinitionElement, propertyName, StringComparison.Ordinal))
					{
						propertyInfo = CopyTemplateTree.GetProperty(componentType, associationAttribute.ComponentElement);
						break;
					}
				}
			}
			return propertyInfo;
		}

		string GetTableName(CopyTemplateNode copyNodeBizo)
		{
			CollectionCopyTemplateNode collectionNode;

			if (copyNodeBizo is CopyTemplateTree)
			{
				if (ElementType != null)
				{
					return BusinessObjectFactory.GetTableNameFromType(ElementType);
				}
			}
			else if ((collectionNode = copyNodeBizo as CollectionCopyTemplateNode) != null)
			{
				return collectionNode.ItemsTableName;
			}

			return null;
		}

		#region GetFilter

		FilterStripBusinessObject GetFilter(string tableName)
		{
			FilterStripBusinessObject filterBizo = null;

			string tablePrefix = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(tableName);
			Type bizoType = !string.IsNullOrEmpty(tablePrefix) ? BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(tablePrefix, false) : null;
			if (bizoType != null)
			{
				Type collectionType = bizoType.Assembly.GetType(bizoType.Namespace + (NoResString)"." + bizoType.Name + (NoResString)"Collection");
				if (collectionType != null)
				{
					Type collectionElementType = BusinessObjectCollection.GetElementTypeFromCollectionType(collectionType);
					if (collectionElementType.IsAssignableFrom(bizoType) || bizoType.IsAssignableFrom(collectionElementType))
					{
						filterBizo = GetFilter(collectionType);
					}
				}
			}

			if (filterBizo == null && !string.IsNullOrEmpty(tableName))
			{
				var tableSchema = EnterpriseSchema.GetTableSchema(tableName);
				if (tableSchema != null)
				{
					filterBizo = GetFilter(tableSchema);
				}
			}

			return filterBizo;
		}

		FilterStripBusinessObject GetFilter(Type collectionType)
		{
			var moduleID = GetModuleIdentifier(collectionType);
			return GetFilter(moduleID);
		}

		FilterStripBusinessObject GetFilter(ModuleIdentifier moduleID)
		{
			if (moduleID != ModuleIDs.NotAssigned)
			{
				using (ZFilterModule filterModule = ZModuleFactory.Instance.CreateNew(moduleID) as ZFilterModule)
				{
					if (filterModule != null)
					{
						filterModule.DoNotCheckOrSaveChanges = true;
						return filterModule.FilterBusinessObject;
					}
				}
			}

			return null;
		}

		UCSchemaFilterStripBusinessObject GetFilter(ITableSchema tableSchema)
		{
			return new UCSchemaFilterStripBusinessObject(tableSchema);
		}

		#endregion

		#endregion

		#region Stored configurations

		protected IEnumerable<UniversalCopyTemplate> GetStoredCopyConfiguration()
		{
			var loadedTemplates = CopyFactory.LoadCopyTemplates().Where(x => x.IsApplicable).ToList();
			loadedTemplates.ForEach(t => t.CopyTemplateTree = null);
			var matchEvaluator = new MatchEvaluator(m =>
			{
				return m.Value switch
				{
					"&" => string.Empty,
					"\\" => (NoResString)"\0",
					_ => m.Value
				};
			});

			return loadedTemplates.OrderBy(t => Regex.Replace(t.CopyTemplateTree.ConfigurationName, "[&\\\\]", matchEvaluator), StringComparer.OrdinalIgnoreCase);
		}

		#endregion

		#region Disposing

		public virtual void Dispose()
		{
			if (localModule != null)
			{
				localModule.Dispose();
				localModule = null;
			}
		}

		#endregion

		#region Notifications

		class Notifications : INotifications
		{
			public void Add(INotification notification)
			{
				text.AppendLine(notification.Message);
			}

			readonly StringBuilder text = new StringBuilder();

			public override string ToString()
			{
				return text.ToString();
			}
		}

		#endregion

		#region BusinessObject List

		List<PropertyDescriptor> GetBusinessObjectProperties(Type bizObjType)
		{
			List<PropertyDescriptor> bizObjTypeProps = null;
			if (bizObjType != null)
			{
				bizObjTypeProps = TypeDescriptor.GetProperties(bizObjType).Cast<PropertyDescriptor>().ToList();
			}
			return bizObjTypeProps;
		}

		System.Collections.IList GetBusinessObjectList(List<PropertyDescriptor> bizObjTypeProps, string childNodeName, Type bizObjType)
		{
			var prop = bizObjTypeProps?.FirstOrDefault(p => p.Name == childNodeName);
			if (prop != null)
			{
				var listMember = MetadataAccessor.GetListMember(null, prop, prop.Name);
				if (!string.IsNullOrEmpty(listMember))
				{
					var bizo = NewBusinessObjectInTable(bizObjType);
					return GetLastObjectFromPath(bizo, listMember);
				}
			}
			return null;
		}

		BusinessObject NewBusinessObjectInTable(Type bizObjType)
		{
			if (!TemporaryBizos.TryGetValue(bizObjType.Name, out WeakReference<BusinessObject> weakBizo) || !weakBizo.TryGetTarget(out BusinessObject bizo))
			{
				try
				{
					bizo = new BusinessObjectFactory { RefreshEnabled = false }.GetNull(bizObjType);
					weakBizo = new WeakReference<BusinessObject>(bizo);
					TemporaryBizos[bizObjType.Name] = weakBizo;
				}
				catch (NoConcreteTypeException)
				{
					bizo = null;
				}
			}
			return bizo;
		}

		System.Collections.IList GetLastObjectFromPath(object root, string bindToList)
		{
			try
			{
				object currentObject = null;

				if (!string.IsNullOrEmpty(bindToList) && root != null)
				{
					var properties = bindToList.Split('.', '+');
					currentObject = root;
					for (var i = 0; i < properties.Length && currentObject != null; i++)
					{
						var info = GetPropertyFromType(currentObject.GetType(), properties[i]);
						currentObject = info?.GetValue(currentObject, BindingFlags.GetProperty, null, null, CultureInfo.CurrentCulture);
					}
				}

				return currentObject as System.Collections.IList;
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				return null; //Some property threw an exception on our path to retrieve the list. We'll swallow and return null.
			}
		}

		PropertyInfo GetPropertyFromType(Type type, string propertyName)
		{
			PropertyInfo info = null;

			while (info == null && type != null)
			{
				info = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
				type = type.BaseType;
			}

			return info;
		}

		#endregion
	}
}


