using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.ZArchitecture.GUI.ZFilterStripBaseControl;

namespace Enterprise.BufferManagement.GUI
{
	public class ModuleSelectorControl : ZUserControl, IBoardSectionControl
	{
		ZGroupBox groupBox;
		ZDropEdit dropEdit;
		ZLabel label;
		ZButton openModuleInANewWindowButton;
		ZButton hideShowFilterButton;
		FlowLayoutPanel flowPanel;
		Control embeddedControl;
		KSplitContainer splitContainer;

		ModuleGridSectionPanelConfiguration selectedPanelConfiguration;
		ModuleIdentifier selectedModuleId;
		readonly Lazy<BusinessObjectFactory> factory;

		public ModuleSelectorControl(ModuleGridSectionConfiguration configuration, BoardSectionViewModel sectionViewModel)
		{
			SectionViewModel = sectionViewModel;
			SectionConfiguration = configuration;

			factory = new Lazy<BusinessObjectFactory>(() => new BusinessObjectFactory { NameForDebugging = "ModuleSelectorControl.Initialize" });

			Initialize();
		}

		#region Initialize

		bool initializing;
		bool dummyEmbeddedControlIsReplaced;

		void Initialize()
		{
			initializing = true;

			groupBox = new ZGroupBox();
			groupBox.Dock = DockStyle.Fill;
			Controls.Add(groupBox);

			splitContainer = new KSplitContainer();
			splitContainer.Dock = DockStyle.Fill;
			splitContainer.FixedPanel = FixedPanel.Panel1;
			splitContainer.IsSplitterFixed = true;
			splitContainer.Orientation = Orientation.Horizontal;
			splitContainer.SplitterWidth = 1;
			groupBox.Controls.Add(splitContainer);

			flowPanel = new FlowLayoutPanel() { Name = "FlowPanel" };
			flowPanel.AutoSize = true;
			flowPanel.FlowDirection = FlowDirection.LeftToRight;
			splitContainer.Panel1.Controls.Add(flowPanel);

			dropEdit = new ZDropEdit() { Name = "ModuleSelectDropEdit" };
			dropEdit.SetDataBinding(SectionConfiguration, "PanelID");
			dropEdit.Visible = SectionConfiguration.PanelConfigurations.Count > 1;
			dropEdit.LastSelectedItemChanged += DropEdit_SelectedIndexChanged;
			flowPanel.Controls.Add(dropEdit);

			hideShowFilterButton = new ZButton() { Name = "HideShowFilterButton" };
			hideShowFilterButton.AutoSize = true;
			hideShowFilterButton.Margin = ControlDpiScalingHelper.NewScaledPadding(1, 1, 1, 1);
			hideShowFilterButton.Text = Res.GetString("6b19d20b-5e9c-4f01-af83-121d04858481", "Filters");
			hideShowFilterButton.Visible = false;
			hideShowFilterButton.Click += ToggleFilterVisibility_Click;
			flowPanel.Controls.Add(hideShowFilterButton);

			openModuleInANewWindowButton = new ZButton() { Name = "OpenInANewWindowButton" };
			openModuleInANewWindowButton.AutoSize = true;
			openModuleInANewWindowButton.Margin = ControlDpiScalingHelper.NewScaledPadding(1, 1, 1, 1);
			openModuleInANewWindowButton.Text = Res.GetString("A11EBEB8-0C7B-4D7C-B7BF-48C63F31C49C", "New Window");
			openModuleInANewWindowButton.Visible = false;
			openModuleInANewWindowButton.Click += Button_Click;
			flowPanel.Controls.Add(openModuleInANewWindowButton);

			label = new ZLabel();
			label.Dock = DockStyle.Fill;
			label.TextAlign = ContentAlignment.MiddleCenter;
			splitContainer.Panel2.Controls.Add(label);

			embeddedControl = new Control();
			DisposableLeakListener.Instance.RegisterDisposable(embeddedControl);
			embeddedControl.Dock = DockStyle.Fill;
			splitContainer.Panel2.Controls.Add(embeddedControl);

			selectedPanelConfiguration = SectionConfiguration.PanelConfigurations.MinBySafe(config => (config as ModuleGridSectionPanelConfiguration).Sequence) as ModuleGridSectionPanelConfiguration;
			if (selectedPanelConfiguration != null)
			{
				groupBox.Text = DisplayName;
				SectionConfiguration.PanelID = selectedPanelConfiguration.Sequence.ToString();
				selectedModuleId = ModuleIDs.AllIncludingClientModules.FirstOrDefault(moduleID => string.Equals(moduleID.Name, selectedPanelConfiguration.ModuleName, StringComparison.OrdinalIgnoreCase));
				LoadModule();
			}

			initializing = false;
		}

		protected override void Dispose(bool disposing)
		{
			label.Dispose();
			DisposeEmbeddedControlIfNeeded();
			base.Dispose(disposing);
		}

		void DisposeEmbeddedControlIfNeeded()
		{
			if (!dummyEmbeddedControlIsReplaced) // "real" embedded controls are disposed by subscription, the dummy one needs to be disposed manually
			{
				embeddedControl.Dispose();
				DisposableLeakListener.Instance.UnRegisterDisposable(embeddedControl);
			}
		}

		string DisplayName
		{
			get
			{
				if (string.IsNullOrEmpty(SectionViewModel.SectionName))
				{
					return selectedPanelConfiguration.PanelName;
				}
				else
				{
					return SectionViewModel.SectionName + ": " + selectedPanelConfiguration.PanelName;
				}
			}
		}

		void LoadModule()
		{
			if (splitContainer.Panel2.Controls.Contains(label))
			{
				splitContainer.Panel2.Controls.Remove(label);
			}

			if (splitContainer.Panel2.Controls.Contains(embeddedControl))
			{
				DisposeEmbeddedControlIfNeeded();
				splitContainer.Panel2.Controls.Remove(embeddedControl);
			}

			var moduleLoaded = false;

			if (selectedModuleId != null)
			{
				try
				{
					var module = ZModuleFactory.Instance.Create(selectedModuleId);

					if (module is ZFilterGridModule gridModule)
					{
						gridModule.DoNotShowRecentItems = true;
						gridModule.ShowFormsFromMainThread = true;
						gridModule.AsyncStrategy = AsyncStrategy.Default;
						gridModule.ShouldPerformSearchAsync = BMSRegistry.Instance.AllowAsynchronousModuleGridSearch.Value;
					}

					if (module != null)
					{
						Disposed += (s, e) => module.Dispose();

						if (module is ZEmbeddedModule embeddedModule)
						{
							SetupAsEmbeddedModule(embeddedModule);
							moduleLoaded = true;
						}
					}
				}
				catch (ModuleNotAssignedIDException)
				{
					// Om nom nom nom. There might be a really good reason for this.
				}
				catch (ModuleGuiNotSupportedException)
				{
					// Om nom nom nom. Good old flow control via exception.
				}
				catch (Exception e)
				{
					if (e.IsCriticalException())
					{
						throw;
					}
					else
					{
						ErrorReporter.ReportOnce("e7b6b210-92f5-41a7-880c-8c9447b9b6b2", string.Format(CultureInfo.InvariantCulture, "The module [{0}] threw an exception when", selectedPanelConfiguration.ModuleName), e);
					}
				}
			}

			if (!moduleLoaded)
			{
				HandleModuleNotLoaded();
			}
		}

		void SetupAsEmbeddedModule(ZEmbeddedModule embeddedModule)
		{
			embeddedModule.AllowToggleFilterVisibilityMenuItem = selectedPanelConfiguration.AllowFilterToggle;
			embeddedControl = embeddedModule.EmbeddedControl;
			dummyEmbeddedControlIsReplaced = true;
			var filterStripControl = embeddedControl as ZFilterStripBaseControl;

			if (embeddedModule is ZFilterModule filterModule)
			{
				filterModule.DoNotCheckOrSaveChanges = true;
				filterModule.ShowFormsFromMainThread = true;
			}

			if (filterStripControl != null)
			{
				SetupEmbeddedModuleFilterStripControl(filterStripControl);
			}
		}

		public event EventHandler OnEmbeddedModuleFilterStripControlPerformSearch;

		void SetupEmbeddedModuleFilterStripControl(ZFilterStripBaseControl filterStripControl)
		{
			filterStripControl.FindButtonTextChanging += FilterStrip_FilterEdited;
			filterStripControl.PerformSearch += (s, e) => OnEmbeddedModuleFilterStripControlPerformSearch?.Invoke(s, e);
			filterStripControl.IsFilterVisible = selectedPanelConfiguration.ShowFilters;
			filterStripControl.IsFilterReadonly = !selectedPanelConfiguration.AllowFilterEdit;

			if (selectedPanelConfiguration.FilterLayout.IsValid)
			{
				LoadLayout(filterStripControl, selectedPanelConfiguration.FilterLayout, factory.Value);
			}

			SetupEmbeddedControl();
			if (!initializing)
			{
				EventHandler onAfterPerformSearch = null;
				onAfterPerformSearch = (s, e) =>
				{
					filterStripControl.OnAfterPerformSearch -= onAfterPerformSearch;
					filterStripControl.OnLoad_Exposed();
				};
				filterStripControl.OnAfterPerformSearch += onAfterPerformSearch;
				filterStripControl.FirePerformSearch(false);
			}
		}

		void FilterStrip_FilterEdited(object sender, ModuleFilterToApplyEventArgs e)
		{
			var filtersModifiedText = Res.GetString("0eadf929-548d-4eb0-bafb-157c6df629a4", "- filters modified");

			var sb = new ZStringBuilder(groupBox.Text);
			if (e.ModuleFilterToApply == null)
			{
				if (!groupBox.Text.EndsWith(filtersModifiedText))
				{
					sb.Append(" ");
					sb.Append(filtersModifiedText);
					groupBox.Text = sb.ToString();
				}
			}
		}

		void ToggleFilterVisibility_Click(object sender, EventArgs args)
		{
			var filterControl = embeddedControl as ZFilterStripCommonControl;
			if (filterControl != null)
			{
				filterControl.IsFilterVisible = !filterControl.IsFilterVisible;
			}
		}

		void SetupEmbeddedControl()
		{
			hideShowFilterButton.Visible = selectedPanelConfiguration.AllowFilterToggle;
			openModuleInANewWindowButton.Visible = selectedPanelConfiguration.AllowOpenModule;

			if (SectionConfiguration.PanelConfigurations.Count == 1 && !openModuleInANewWindowButton.Visible && !hideShowFilterButton.Visible)
			{
				splitContainer.SplitterDistance = ControlDpiScalingHelper.ScaleToCurrentDpiX(10);
				if (SectionConfiguration.PanelConfigurations.Count == 1)
				{
					splitContainer.Panel1Collapsed = true;
				}
			}
			else
			{
				splitContainer.SplitterDistance = ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			}

			splitContainer.Panel2.Controls.Add(embeddedControl); // insert Dock = before and remove 3 lines above
		}

		void Button_Click(object sender, EventArgs e)
		{
			if (selectedModuleId == null)
			{
				return;
			}

			bool canOpenModuleInNewWindow = ZModuleFactory.Instance.IsZPopupModuleNonSingleton(selectedModuleId)
				|| ZModuleFactory.Instance.IsModuleOfType<ZEmbeddedModule>(selectedModuleId);

			if (!canOpenModuleInNewWindow)
			{
				return;
			}

			var module = ZModuleFactory.Instance.Create(selectedModuleId);
			if (module == null || !(module is ZFilterModule filterGridModule))
			{
				return;
			}

			openModuleInANewWindowButton.Enabled = false;

			filterGridModule.AllowToggleFilterVisibilityMenuItem = selectedPanelConfiguration.AllowFilterToggle;
			filterGridModule.DoNotCheckOrSaveChanges = true;
			filterGridModule.ShowFormsFromMainThread = true;

			if (filterGridModule.EmbeddedControl is ZFilterStripControl filterStripControl)
			{
				filterStripControl.IsFilterVisible = selectedPanelConfiguration.ShowFilters;
				filterStripControl.IsFilterReadonly = !selectedPanelConfiguration.AllowFilterEdit;
				filterStripControl.RunSearchOnEnteringAModuleOverride = true;
			}

			StmModuleFilter layout = null;
			if (selectedPanelConfiguration.FilterLayout.IsValid)
			{
				layout = factory.Value.Load<StmModuleFilter>(selectedPanelConfiguration.FilterLayout);
			}

			var popup = new EmbeddedModulePopup(filterGridModule, layout);
			if (popup != null)
			{
				popup.Disposed += (obj, ea) =>
				{
					openModuleInANewWindowButton.Enabled = true;
					module.Dispose();
				};

				popup.Show();
			}
		}

		void DropEdit_SelectedIndexChanged(object sender, EventArgs e)
		{
			var dropEdit = sender as ZDropEdit;
			var last = dropEdit.LastSelectedItem;
			if (last == null)
			{
				return;
			}

			selectedPanelConfiguration = SectionConfiguration.PanelConfigurations.FirstOrDefault(p => (p as ModuleGridSectionPanelConfiguration).Sequence.ToString() == last.Code) as ModuleGridSectionPanelConfiguration;
			if (selectedPanelConfiguration != null)
			{
				groupBox.Text = DisplayName;
				selectedModuleId = ModuleIDs.AllIncludingClientModules.FirstOrDefault(moduleID => string.Equals(moduleID.Name, selectedPanelConfiguration.ModuleName, StringComparison.OrdinalIgnoreCase));
				LoadModule();
			}
		}

		void HandleModuleNotLoaded()
		{
			label.Text = Res.GetString("2e8503e0-cc69-4878-bd75-62de411ba738", "Unable to load a grid for the module [{0}]", selectedPanelConfiguration.ModuleName);
			splitContainer.Panel2.Controls.Add(label);
		}

		static void LoadLayout(ZFilterStripBaseControl filterStripControl, ZGuid layoutPk, BusinessObjectFactory factory)
		{
			var layout = factory.Load<StmModuleFilter>(layoutPk);
			filterStripControl.LayoutToLoad = layout;
		}

		#endregion

		#region IBoardSectionControl Members

		string IBoardSectionControl.SectionType => BMConstants.ModuleGridSectionType;

		void IBoardSectionControl.Refresh(BoardRefreshEventArgs args)
		{
			foreach (var stripControl in this.FindAll<ZFilterStripCommonControl>())
			{
				stripControl.FirePerformSearch(false);
			}

			RefreshCompleted?.Invoke(this, args);
		}

		bool IBoardSectionControl.AcceptDraggedControl(object control)
		{
			return false;
		}

		public event EventHandler<BoardRefreshEventArgs> RefreshCompleted;

		bool IBoardSectionControl.SuppressBoardRefresh(BoardRefreshEventArgs args)
		{
			return false;
		}

		public BoardSectionViewModel SectionViewModel { get; }

		public ModuleGridSectionConfiguration SectionConfiguration { get; }

		#endregion
	}
}
