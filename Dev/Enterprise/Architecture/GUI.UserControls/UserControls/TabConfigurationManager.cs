using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using ResString = Enterprise.ZArchitecture.GUI.UserControls.ResString;

namespace Enterprise.ZArchitecture.GUI
{
	public sealed class TabConfigurationManager
	{
		public TabConfigurationManager(MainMenu menu, ZTabControl mainTabControl)
		{
			propertyInfosWithValueChangedEventHooked = new List<ZPropertyInfo>();

			if (menu != null)
			{
				this.ViewMenuItem = new ZMenuItem(ResString.GetMultilingualString("MenuItem.View", "View"));
				((IList)menu.MenuItems).Insert(2, ViewMenuItem);
				ViewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("MenuItem.View.Placeholder", "Placeholder")));
				ViewMenuItem.Popup += new EventHandler(ViewMenuItem_Popup);
			}

			this.MainTabControl = mainTabControl;
			mainTabControl.Disposed += new EventHandler(MainTabControl_Disposed);
			mainTabControl.CanTabPageBeVisibleOnSetRelevantDelegate += CanTabPageBeVisibleOnSetRelevant;

			HookWorkflowPropertyEvents();
		}

		readonly ZTabControl MainTabControl;
		readonly MenuItem ViewMenuItem;

		#region Customisation Settings

		readonly List<ZPropertyInfo> propertyInfosWithValueChangedEventHooked;
		BusinessObject topLevelBusinessObject;

		void HookWorkflowPropertyEvents()
		{
			topLevelBusinessObject = (BusinessObject)MainForm.BusinessEntity;
			foreach (string propertyName in PropertiesThatAffectWorkflow)
			{
				var info = topLevelBusinessObject.ZPropertyInfoHash[propertyName];
				info.ValueChanged += ConfigurationMode_ConfigurationModeChanged;
				propertyInfosWithValueChangedEventHooked.Add(info);
			}
		}

		internal IFormCustomisationSettings FormCustomisationSettings
		{
			get
			{
				if (formCustomisationSettings == null)
				{
					IProcessTaskTemplate template = GetProcessTaskTemplate();
					formCustomisationSettings = template != null ? template.FormCustomisationSettings : null;
				}
				return formCustomisationSettings;
			}
			set { formCustomisationSettings = value; }
		}
		IFormCustomisationSettings formCustomisationSettings;

		IProcessTaskTemplate GetProcessTaskTemplate()
		{
			IProcessTaskTemplateLoader loader = (IProcessTaskTemplateLoader)Activator.CreateInstance(ObjectFactory.GetType<IProcessTaskTemplateLoader>(), topLevelBusinessObject.Factory);
			return loader.FindTemplateForScreenLayout((IWorkflowProviderCore)topLevelBusinessObject);
		}

		string[] PropertiesThatAffectWorkflow
		{
			get
			{
				string[] result = Array.Empty<string>();
				if (topLevelBusinessObject != null)
				{
					IPropertiesThatAffectWorkflowProvider provider = (IPropertiesThatAffectWorkflowProvider)Activator.CreateInstance(ObjectFactory.GetType<IPropertiesThatAffectWorkflowProvider>());
					result = provider.GetPropertiesThatAffectWorkflow(((IWorkflowProviderCore)topLevelBusinessObject).WorkflowType);
				}
				return result;
			}
		}

		#endregion

		#region Enabled

		public bool Enabled
		{
			get { return fEnabled; }
			set
			{
				fEnabled = value;
				if (Enabled)
				{
					RefreshTabVisibility(false);
				}
			}
		}

		bool fEnabled;

		#endregion

		#region View Menu Item Creation

		void ViewMenuItem_Popup(object sender, EventArgs e)
		{
			LoadMenuItems();
		}

		void LoadMenuItems()
		{
#if WINZOR
			if (!hasTabPagesChanged() && !firstRender)
			{
				for (var i = 0; i < VisibilityChangeableTabPages.Count(); i++)
				{
					ViewMenuItem.MenuItems[i].Checked = MainTabControl.TabPages.Contains(VisibilityChangeableTabPages.ElementAt(i));
				}
				return;
			}
			firstRender = false;
#endif
			ViewMenuItem.MenuItems.Clear();
			foreach (TabPage page in VisibilityChangeableTabPages)
			{
				MenuItem addedItem = new ZMenuItem(page.Text, ViewMenuItem_Click);
				ViewMenuItem.MenuItems.Add(addedItem);
				addedItem.Checked = MainTabControl.TabPages.Contains(page);
			}
		}

		IEnumerable<ZTabPage> VisibilityChangeableTabPages
		{
			get { return MainTabControl.AllTabPages.Cast<ZTabPage>().Where(t => t.TabRelevant); }
		}

#if WINZOR
		bool hasTabPagesChanged()
		{
			if (ViewMenuItem.MenuItems.Count != VisibilityChangeableTabPages.Count())
			{
				return true;
			}

			var length = ViewMenuItem.MenuItems.Count < VisibilityChangeableTabPages.Count() ? ViewMenuItem.MenuItems.Count : VisibilityChangeableTabPages.Count();

			for (var i = 0; i < length; i++)
			{
				if (ViewMenuItem.MenuItems[i].Text != VisibilityChangeableTabPages.ElementAt(i).Text)
				{
					return true;
				}
			}

			return false;
		}

		bool firstRender = true;
#endif

		#endregion

		#region Persistence

		ITabVisibilityDeciderPersistence TabSettingsWithPersistence
		{
			get { return MainForm as ITabVisibilityDeciderPersistence; }
		}

		ZForm MainForm
		{
			get
			{
				if (MainTabControl.IsDisposed)
				{
					throw new InvalidOperationException("Cannot call FindForm on a disposed object");
				}
				return (ZForm)MainTabControl.FindForm();
			}
		}

		#endregion

		#region Refresh Tabs

		void ConfigurationMode_ConfigurationModeChanged(object sender, EventArgs e)
		{
			IProcessTaskTemplate currentTemplate = GetProcessTaskTemplate();
			if (lastMode != currentTemplate)
			{
				lastMode = currentTemplate;
				formCustomisationSettings = null;
				RefreshTabVisibility(false);
			}
		}

		IProcessTaskTemplate lastMode;

		void RefreshTabVisibility(bool updatePersistence)
		{
			if (MainTabControl != null &&
				(FormCustomisationSettings != null || (TabSettingsWithPersistence != null && TabSettingsWithPersistence.HasTabVisiblePersisted)))
			{
				foreach (ZTabPage page in VisibilityChangeableTabPages)
				{
					if (page != null)
					{
						SetTabPageVisible(page, ShouldTabBeVisible(page), updatePersistence);
					}
				}
			}
		}

		bool? ShouldTabBeVisible(ZTabPage page)
		{
			bool? shouldTabBeVisible = null;

			if (TabSettingsWithPersistence != null && TabSettingsWithPersistence.HasTabVisiblePersisted)
			{
				shouldTabBeVisible = TabSettingsWithPersistence.RetrieveTabPageVisible(page);
			}

			if ((!shouldTabBeVisible.HasValue || shouldTabBeVisible.Value) && FormCustomisationSettings != null)
			{
				shouldTabBeVisible = FormCustomisationSettings.IsElementVisible(page.Name, ElementType.Tab);
			}

			if (shouldTabBeVisible.HasValue && !shouldTabBeVisible.Value)
			{
				var form = MainForm;
				if (form != null && form.PlugInIDToSelectOnLoaded != null)
				{
					var pluginTab = page as ZTabPagePlugIn;
					if (pluginTab != null && pluginTab.PlugIn != null && pluginTab.PlugIn.ControllerID == form.PlugInIDToSelectOnLoaded)
					{
						shouldTabBeVisible = true;
					}
				}
			}

			return shouldTabBeVisible;
		}

		public void RefreshTabVisibility()
		{
			RefreshTabVisibility(true);
		}

		void ViewMenuItem_Click(object sender, EventArgs e)
		{
			var item = (MenuItem)sender;
			var tabPage = VisibilityChangeableTabPages.Cast<ZTabPage>().FirstOrDefault(x => x.Text == item.Text);

			if (tabPage != null)
			{
				bool set = SetTabPageVisible(tabPage, !item.Checked, true);

				if (set)
				{
					item.Checked = tabPage.TabVisible;
				}
			}
		}

		bool SetTabPageVisible(ZTabPage page, bool? visible, bool updatePersistence)
		{
			if (!page.HasErrors && !page.HasMessageErrors)
			{
				var tabVisibilityOverride = page as ITabVisibilityOverride;

				bool? visibilityOverride = tabVisibilityOverride != null
					? tabVisibilityOverride.IsTabVisible
					: null;

				if (visibilityOverride.HasValue)
				{
					updatePersistence = updatePersistence
						&& visible.HasValue
						&& visible.Value == visibilityOverride.Value;

					visible = visibilityOverride.Value;
				}

				if (visible.HasValue)
				{
					page.TabVisible = visible.Value;

					var plugInPage = page as ZTabPagePlugIn;

					if (plugInPage != null && plugInPage.PlugIn.ShouldHideTopLevelMenuWithTab &&
						plugInPage.PlugIn.TopLevelMenu != null && plugInPage.PlugIn.TopLevelMenu.Handle != IntPtr.Zero)
					{
						plugInPage.PlugIn.TopLevelMenu.Visible = visible.Value;
					}

					if (updatePersistence && TabSettingsWithPersistence != null)
					{
						TabSettingsWithPersistence.StoreTabVisible(page);
					}
				}

				return true;
			}

			return false;
		}

		bool CanTabPageBeVisibleOnSetRelevant(ZTabPage page)
		{
			bool result = true;
			if (Enabled)
			{
				bool? shouldTabBeVisible = ShouldTabBeVisible(page);
				result = !shouldTabBeVisible.HasValue || shouldTabBeVisible.Value;
			}
			return result;
		}

		#endregion

		#region Dispose

		void MainTabControl_Disposed(object sender, EventArgs e)
		{
			foreach (var info in propertyInfosWithValueChangedEventHooked)
			{
				info.ValueChanged -= ConfigurationMode_ConfigurationModeChanged;
			}

			topLevelBusinessObject = null;
		}

		#endregion
	}
}
