using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	public partial class BoardSectionConfigControl : ZUserControl
	{
		public BoardSectionConfigControl()
		{
			InitializeComponent();

			BoardSectionsGrid.AfterBind += (sender, args) =>
			{
				BoardSectionsGrid.ListManager.ListChanged += ListManager_ListChanged;
				BoardSectionsGrid.ListManager.CurrentChanged += BoardSectionsGrid_CurrentChanged;
			};

			BackColorDropEdit.Resize += BackColorDropEdit_Resize;
		}

		#region UserControl overrides

		protected override void OnLoad(EventArgs e)
		{
			var isDesigner = this.IsDesignMode();

			if (!isDesigner)
			{
				var collection = (BMBoardSectionCollection)BoardSectionsGrid.ListManager.List;
				if (cloneMenuItem == null)
				{
					cloneMenuItem = new CloneMenuItem(BoardSectionsGrid, () => collection);
					BoardSectionsGrid.ContextMenu.MenuItems.Add(cloneMenuItem);
				}
			}

			base.OnLoad(e);

			if (!isDesigner)
			{
				OnSectionConfigChanged();
				SetDropEditWidths();
			}
		}

		CloneMenuItem cloneMenuItem;

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			var proposedWidth = SectionConfigSplitContainer.Width / 2;
			if (proposedWidth >= SectionConfigSplitContainer.Panel1MinSize && proposedWidth <= SectionConfigSplitContainer.Width - SectionConfigSplitContainer.Panel2MinSize)
			{
				SectionConfigSplitContainer.SplitterDistance = proposedWidth;
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				if (BoardSectionsGrid.ListManager != null)
				{
					BoardSectionsGrid.ListManager.ListChanged -= ListManager_ListChanged;
					BoardSectionsGrid.ListManager.CurrentChanged -= BoardSectionsGrid_CurrentChanged;
				}

				if (selectedItem != null)
				{
					selectedItem.MS_SectionTypeInfo.ValueChanged -= SectionType_ValueChanged;
				}
			}
		}

		#endregion

		#region Color drop edits

		void BackColorDropEdit_Resize(object sender, EventArgs e)
		{
			SetDropEditWidths();
		}

		void SetDropEditWidths()
		{
			SetColorDropEditSizes(new[] { BackColorDropEdit, ForeColorDropEdit });
		}

		internal static void SetColorDropEditSizes(IEnumerable<ZDropEdit> dropEdits)
		{
			var colorDropEditWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(90);

			foreach (var dropEdit in dropEdits)
			{
				if (dropEdit.Width != colorDropEditWidth)
				{
					((ZDropEditInternals)dropEdit).SetControlWidth(colorDropEditWidth);
				}
			}
		}

		#endregion

		#region Section Change

		BMBoardSection selectedItem;
		public event EventHandler SectionConfigChanged;
		bool isAddingNewItem;

		void ListManager_ListChanged(object sender, ListChangedEventArgs listChangedEventArgs)
		{
			isAddingNewItem = listChangedEventArgs.ListChangedType == ListChangedType.ItemAdded;
		}

		void BoardSectionsGrid_CurrentChanged(object sender, EventArgs e)
		{
			OnSectionConfigChanged();
		}

		internal void OnSectionConfigChanged()
		{
			var previouslySelectedItem = selectedItem;

			if (previouslySelectedItem != null)
			{
				previouslySelectedItem.MS_SectionTypeInfo.ValueChanged -= SectionType_ValueChanged;
			}

			selectedItem = GetSelectedSection();

			if (selectedItem != null)
			{
				selectedItem.MS_SectionTypeInfo.ValueChanged += SectionType_ValueChanged;
			}

			var hasSectionTypeChanged = selectedItem == null
										|| previouslySelectedItem == null
										|| previouslySelectedItem.IsDeleted
										|| selectedItem.MS_SectionType != previouslySelectedItem.MS_SectionType;

			UpdateSectionControls(shouldClearExistingControls: hasSectionTypeChanged);
			SectionConfigChanged?.Invoke(this, EventArgs.Empty);
			isAddingNewItem = false;
		}

		void SectionType_ValueChanged(object sender, EventArgs e)
		{
			if (!IsDisposed && IsHandleCreated)
			{
				UpdateSectionControls(shouldClearExistingControls: true);
			}
		}

		void LoadConfigurationControlPanel(BMBoardSection section, bool shouldClearExistingControls)
		{
			const string controlName = "configurationControl";

			if (shouldClearExistingControls)
			{
				ConfigurationControlPanel.Controls.RemoveAndDisposeAll();

				ClearAdditionalTabPages();

				if (section == null)
				{
					return;
				}

				AddConfigurationControl(section, controlName);
				AddAdditionalTabPages(section.MS_SectionType);
			}
			else if (section != null)
			{
				var configurationControl = ConfigurationControlPanel.FindSingleOrDefault<ZUserControl>(controlName);
				configurationControl?.SetDataBinding(section.Configuration, string.Empty);
			}
		}

		void ClearAdditionalTabPages()
		{
			var tabs = SectionConfigTabControl.TabPages.OfType<ZTabPage>().Skip(1).ToArray();

			foreach (var tab in tabs)
			{
				tab.Dispose();
				SectionConfigTabControl.TabPages.Remove(tab);
			}
		}

		void AddConfigurationControl(BMBoardSection section, string controlName)
		{
			var configurationControl = BoardSectionControlProvider.GetConfigControl(section.MS_SectionType);

			if (configurationControl != null)
			{
				configurationControl.Name = controlName;
				configurationControl.SetDataBinding(section.Configuration, string.Empty);
				ConfigurationControlPanel.Controls.Add(configurationControl);
			}
		}

		void AddAdditionalTabPages(ZString sectionType)
		{
			try
			{
				SuspendLayout();

				foreach (var tab in BoardSectionControlProvider.GetAdditionalTabPages(sectionType))
				{
					SectionConfigTabControl.TabPages.Add(tab);
				}
			}
			finally
			{
				ResumeLayout();
			}
		}

		void UpdateSectionControls(bool shouldClearExistingControls)
		{
			var selectedSection = GetSelectedSection();
			var currentTabIndex = SectionConfigTabControl.SelectedIndex;

			LoadConfigurationControlPanel(selectedSection, shouldClearExistingControls);
			ResetTabsDataBinding(selectedSection);

			if (isAddingNewItem)
			{
				SectionConfigTabControl.SelectedIndex = 0;
			}
			else
			{
				SectionConfigTabControl.SelectedIndex = currentTabIndex < SectionConfigTabControl.TabCount ? currentTabIndex : SectionConfigTabControl.TabCount - 1;
			}
		}

		void ResetTabsDataBinding(BMBoardSection section)
		{
			if (section == null)
			{
				return;
			}

			var tabsToSetDataBinding = SectionConfigTabControl.TabPages.OfType<ZBindingTabPage>().ToArray();

			foreach (var tab in tabsToSetDataBinding)
			{
				tab.ResetBinding();
				tab.SetDataBinding(section.Configuration, string.Empty);
			}
		}

		#endregion

		public BMBoardSection GetSelectedSection()
		{
			return BoardSectionsGrid.ListManager?.GetCurrent() as BMBoardSection;
		}
	}
}
