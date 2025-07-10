using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Pipes;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI
{
	public partial class BMComponentControl : ComponentUserControl, IBoardSectionControl, IHotkeyHandler, IBoardSectionRefreshable
	{
		[SuppressMessage("Microsoft.Design", "CA1031: Do not catch general exception types", Justification = "Valid for handling disposables in constructors")]
		public BMComponentControl(BMComponentSectionConfiguration sectionConfiguration, BMBoardSectionViewModel viewModel)
			: base(viewModel)
		{
			try
			{
				this.sectionConfiguration = sectionConfiguration;
				sharedTaskMenuStrip = TaskCardControl.CreateMenuStrip();

				DrawComponent();

				var filterQueryFactory = viewModel.FactoryProvider.GetBoardGUIThreadFactory();

				boardWorkflowFilter = GetFilterInCorrectFactory(sectionConfiguration.WorkflowFilter, filterQueryFactory);
				boardTaskFilter = GetFilterInCorrectFactory(sectionConfiguration.TaskFilter, filterQueryFactory);

				isSectionConfigurationTaskFilterSet = sectionConfiguration.TaskFilter.IsInDatabase;

				if (sectionConfiguration.IsBuffer)
				{
					hasZones = sectionConfiguration.ShowZones && ViewModel.ComponentGrid.Cells.Count(x => x.ContentType == CellContentType.ZoneHeading) >= BMConstants.NumberOfZonesIncludingZoneZero;
				}

				expansionStrategy = new TableCellExpansionStrategy(Table, ViewModel);

				viewModel.ComponentGrid.LoadFailed += ComponentGrid_LoadFailed;
				viewModel.ComponentGrid.HeadingsRefreshed += ComponentGrid_HeadingsRefreshed;
				viewModel.ComponentGrid.CellRefreshStarted += ComponentGrid_CellRefreshStarted;
				viewModel.ComponentGrid.CellRefreshFinished += ComponentGrid_CellRefreshFinished;
				viewModel.LoadingIndicatorMessageUpdated += ViewModel_LoadingIndicatorMessageUpdated;
			}
			catch
			{
				try
				{
					Dispose();
				}
				catch { }
				throw;
			}
		}

		readonly BMComponentSectionConfiguration sectionConfiguration;
		readonly KContextMenuStrip sharedTaskMenuStrip;
		readonly bool isSectionConfigurationTaskFilterSet;

		internal BMComponentSectionConfiguration SectionConfiguration => sectionConfiguration;

		#region Draw

		void DrawComponent()
		{
			ContextMenuStrip = GetContextMenu(SectionConfiguration.Section);

			using (new DisposableAction(SuspendLayout, ResumeLayout))
			{
				var headerTable = GetHeaderPanel();
				Controls.Add(headerTable);

				ContentTable = GetContentTable(headerTable);

				if (ContentTable != null)
				{
					Controls.Add(ContentTable);
				}
			}
		}

#if DEBUG
		public
#else
	internal
#endif
			AcceptabilityBandTileContainerControl AcceptabilityBandTiles
		{ get; private set; }
		internal KTableLayoutPanel ContentTable { get; private set; }
		KTableLayoutPanel sectionPanel;
		bool showAcceptabilityBandTiles;
		const int HeaderDefaultSize = 28;
		const int HeaderPadding = 4;
		readonly TableCellExpansionStrategy expansionStrategy;

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Not assigning pixels")]
		FlowLayoutPanel GetHeaderPanel()
		{
			showAcceptabilityBandTiles = !ViewModel.SuppressAcceptabilityBandVisualisation && SectionConfiguration.TileAcceptabilityBands.Any(b => b.AcceptabilityBand?.BAB_IsActive ?? false);
			var visualBoardForm = ParentForm as VisualBoardForm;

			var table = new FlowLayoutPanel
			{
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				Margin = Padding.Empty,
				Padding = Padding.Empty,
				WrapContents = false,
			};
			ControlDpiScalingHelper.SetWidth(table, this.Width, false);
			ControlDpiScalingHelper.SetHeight(table, visualBoardForm != null ? Math.Max(visualBoardForm.RefreshButton.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(HeaderPadding), ControlDpiScalingHelper.ScaleToCurrentDpiY(HeaderDefaultSize)) : ControlDpiScalingHelper.ScaleToCurrentDpiY(HeaderDefaultSize), false);

			if (showAcceptabilityBandTiles)
			{
				AcceptabilityBandTiles = new AcceptabilityBandTileContainerControl(ViewModel)
				{
					AutoSize = true,
					Anchor = AnchorStyles.Left | AnchorStyles.Right,
					Margin = Padding.Empty,
					Padding = Padding.Empty,
				};
			}

			sectionPanel = new KTableLayoutPanel
			{
				AutoSize = true,
				Dock = DockStyle.Top,
				Margin = Padding.Empty,
				Padding = Padding.Empty,
				Name = "SectionHeadingPanel",
			};

			SectionLabel = GetSectionLabel();
			sectionPanel.Padding = ControlDpiScalingHelper.NewScaledPadding(GetSectionLabelHorizontalPadding(), 0, 0, 0, false);
			ControlDpiScalingHelper.SetWidth(sectionPanel, SectionLabel.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(1), false);

			this.Resize += OnControlResize;
			sectionPanel.LocationChanged += SectionPanel_LocationChanged;

			sectionPanel.Controls.Add(SectionLabel);
			UpdateSubHeadingContent();
			ViewModel.PropertyChanged += ViewModel_PropertyChanged;

			AddHeadingPanelsInCorrectOrder(table);

			ControlDpiScalingHelper.SetHeight(table, Math.Max(table.Height, sectionPanel.Height), false);

			if (IsZeroCellAcceptabilityBandTileSection)
			{
				table.Height = ClientSize.Height;
				table.Dock = DockStyle.Fill;
				AcceptabilityBandTiles.Width = ClientSize.Width - SectionLabel.Width;
				AcceptabilityBandTiles.Height = table.Height;
				AcceptabilityBandTiles.WrapContents = true;
			}

			return table;
		}

		void AddHeadingPanelsInCorrectOrder(FlowLayoutPanel table)
		{
			if (IsZeroCellAcceptabilityBandTileSection)
			{
				table.Controls.Add(sectionPanel);
				table.Controls.Add(AcceptabilityBandTiles);
			}
			else
			{
				if (AcceptabilityBandTiles != null)
				{
					table.Controls.Add(AcceptabilityBandTiles);
				}

				table.Controls.Add(sectionPanel);
			}
		}

#if DEBUG
		public
#endif
 void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "SubHeadingAppearance")
			{
				try
				{
					this.BeginInvokeSafe(new Action(UpdateSubHeadingContent));
				}
				catch (InvalidAsynchronousStateException)
				{
					// Form is closing, UpdateSubHeadingContent not essential
				}
			}
		}

		void SectionPanel_LocationChanged(object sender, EventArgs e)
		{
			RecalculateSectionPanelPadding();
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "We're setting property values from other properties. Those would already be scaled.")]
		void OnControlResize(object sender, EventArgs e)
		{
			try
			{
				if (IsZeroCellAcceptabilityBandTileSection)
				{
					AcceptabilityBandTiles.MaximumSize = new Size(ClientSize.Width - SectionLabel.Width, ClientSize.Height);
					AcceptabilityBandTiles.MinimumSize = AcceptabilityBandTiles.MaximumSize;
				}

				RecalculateSectionPanelPadding();
				EnsureTasksLaidOutAppropriately();
			}
			catch (Exception ex) when (ex is DatabaseUpgradeException || ex.InnerException is DatabaseUpgradeException)
			{
				// Well, if database is upgrading, we can't do much about it
			}
		}

		void RecalculateSectionPanelPadding()
		{
			sectionPanel.Padding = ControlDpiScalingHelper.NewScaledPadding(GetSectionLabelHorizontalPadding(), 0, 0, 0, false);
		}

		int GetSectionLabelHorizontalPadding()
		{
			var controlHalfway = (this.Width / 2);
			var sectionLabelHalfway = SectionLabel.Width / 2;

			if (showAcceptabilityBandTiles)
			{
				return (controlHalfway - AcceptabilityBandTiles.Width - sectionLabelHalfway > 0) ? controlHalfway - AcceptabilityBandTiles.Width - sectionLabelHalfway : 0;
			}
			else
			{
				return (controlHalfway - sectionLabelHalfway > 0) ? controlHalfway - sectionLabelHalfway : 0;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031: Do not catch general exception types", Justification = "valid for handling disposables in factory methods")]
		KTableLayoutPanel GetContentTable(FlowLayoutPanel headerTable)
		{
			var componentGrid = ViewModel.ComponentGrid;

			if (componentGrid.TotalColumns == 0)
			{
				return null;
			}

			var table = new KTableLayoutPanel();
			try
			{
				using (new DisposableAction(table.SuspendLayout, table.ResumeLayout))
				{
					table.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
					table.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
					table.Name = ContentTableName;

					ControlDpiScalingHelper.SetTop(table, headerTable.Height, false);
					ControlDpiScalingHelper.SetHeight(table, this.Height - headerTable.Height, false);
					ControlDpiScalingHelper.SetWidth(table, this.Width, false);
					table.SetDoubleBuffered(true);

					for (int row = 0; row < componentGrid.TotalRows; row++)
					{
						table.RowStyles.Add(new RowStyle { SizeType = SizeType.Percent });
						table.RowCount++;
						for (int col = 0; col < componentGrid.TotalColumns; col++)
						{
							var cell = componentGrid[row, col];
							if (cell.Column == table.ColumnStyles.Count)
							{
								table.ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.Percent });
								table.ColumnCount++;
							}
							var control = GetControl(cell);
							table.Controls.Add(control, cell.Column, cell.Row);
						}
					}

					SetRowAndColumnStyles(table, componentGrid);
					CollapseHeadings(table, componentGrid);
				}

				return table;
			}
			catch
			{
				try
				{
					table.Dispose();
				}
				catch { }
				throw;
			}
		}

		bool IsVertical
		{
			get { return ViewModel.Orientation == BMBoardSectionOrientation.Vertical; }
		}

		bool IsHorizontal
		{
			get { return ViewModel.Orientation == BMBoardSectionOrientation.Horizontal; }
		}

		bool IsZeroCellAcceptabilityBandTileSection
		{
			get { return ViewModel.CellsPerSubsection == 0 && AcceptabilityBandTiles != null; }
		}

		const string ContentTableName = "ContentTable";

		#region Section Label

		public ZLabel SectionLabel { get; private set; }

		ZLabel GetSectionLabel()
		{
			return new ZLabel
			{
				AutoSize = true,
				Dock = DockStyle.Fill,
				Text = ViewModel.SectionName,
				TextAlign = ContentAlignment.MiddleCenter,
				Font = ViewModel.IsPreview ? new Font("Arial", 10F, FontStyle.Bold) : new Font("Arial", 12F, FontStyle.Bold),
				Name = "SectionLabel",
			};
		}

		#endregion

		#region Context Menu

		ContextMenuStrip GetContextMenu(BMBoardSection section)
		{
			var component = section.Component;
			var contextMenuStrip = new LazyContextMenuStrip((strip, control) => PopulateContextMenu(strip, section, component), clearOnClose: false);

#if DEBUG
			if (AutoGenerateComponentMenuItems.Value)
			{
				contextMenuStrip.AddItems_ForTest(this);
			}
#endif

			return contextMenuStrip;
		}

#if DEBUG
		public static readonly LazyOverridable<bool> AutoGenerateComponentMenuItems = new LazyOverridable<bool>(() => Globals.IsTest);
#endif

		public void PopulateContextMenu(KContextMenuStrip contextMenuStrip, BMBoardSection section, BMComponent component, EventHandler editScheduleCellEventHandler = null, EventHandler openJobWorkflowsModuleCellEventHandler = null, TaskPanel currentTaskPanel = null)
		{
			contextMenuStrip.Items.Add(new CurrentTaskFilterMenuItem(ViewModel));

			if (section.SectionConfiguration.IsBuffer)
			{
				contextMenuStrip.Items.Add(new RiskFilterMenuItem(ViewModel));
			}

			if (component != null)
			{
				contextMenuStrip.Items.Add(new ComponentViewFilterMenuItem(ViewModel, this, section.SectionConfiguration));
			}

			if (!section.SectionConfiguration.ShowWorkflowOrJobWorkflowCards)
			{
				var capabilityMenuItem = new CapabilityTaskFilterMenuItem(ViewModel);
				capabilityMenuItem.RemovedFromFilterManager += (s, e) => RefreshHeadingsAsync(section.Factory.NameForDebugging, ViewModel.ComponentGrid.CardAllocationMap);
				capabilityMenuItem.DropDownItemClicked += (s, e) => RefreshHeadingsAsync(section.Factory.NameForDebugging, ViewModel.ComponentGrid.CardAllocationMap);

				contextMenuStrip.Items.Add(capabilityMenuItem);
			}

			var filterStripMenuItem = new ZToolStripMenuItem(Res.GetString("580B7805-3AD0-4FCC-B4A9-271B07B20DC9", "Filter Strips"), FilterStripMenuItem_Click);
			contextMenuStrip.Items.Add(filterStripMenuItem);
			filterStripMenuItem.ToolTipText = Res.GetString("f2e2b25f-964f-4be0-bdc8-4b28542f06d0", "View or edit the Filter Strips which restrict the workflows shown on this board section.");
			contextMenuStrip.Items.Add("-");

			if (ViewModel.IsBuffer && !ViewModel.IsPreview)
			{
				var menuItem = new ConstrainedModeMenuItem(ViewModel);
				menuItem.ModeChanged += ConstrainedModeMenuItem_ModeChanged;
				contextMenuStrip.Items.Add(menuItem);
			}

			contextMenuStrip.Items.Add(new ZToolStripMenuItem(Res.GetString("df58b38a-2053-48b4-b8bb-0c91db498e28", "Legend"), LegendMenuItem_Click));

			contextMenuStrip.Items.Add("-");
			var refreshMenuItemText = Res.GetString("ed32e943-d5ba-4656-9ceb-d4583fa73c7d", "Refresh section [{0}]", ViewModel.SectionName);
			contextMenuStrip.Items.Add(new ZToolStripMenuItem(refreshMenuItemText, (s, e) => SetupCards(new BoardRefreshEventArgs { TriggeredByUserRefreshingBoardOrSection = true })));

			var thisSectionText = Res.GetString("d62f2ae2-e240-4e1b-a772-f65440f1a71a", "This section");
			var thisCellText = Res.GetString("6432a1a7-2c4b-49b0-8861-9142c6682332", "This cell");

			if (editScheduleCellEventHandler != null)
			{
				var editSchedulesMenuItem = new ZToolStripMenuItem(Res.GetString("29efe502-3d95-42a2-840e-a96fe5c26e4f", "Edit Schedules"));
				contextMenuStrip.Items.Add(editSchedulesMenuItem);
				editSchedulesMenuItem.DropDownItems.Add(new ZToolStripMenuItem(thisSectionText, EditSchedulesThisSection_Click));
				editSchedulesMenuItem.DropDownItems.Add(new ZToolStripMenuItem(thisCellText, editScheduleCellEventHandler));
			}
			else
			{
				contextMenuStrip.Items.Add(new ZToolStripMenuItem(Res.GetString("6711f8bc-398e-46c9-b21d-d2852370121c", "Edit Schedules (this section)"), EditSchedulesThisSection_Click));
			}

			if (openJobWorkflowsModuleCellEventHandler != null)
			{
				var editSchedulesMenuItem = new ZToolStripMenuItem(Res.GetString("0b84dc51-ede6-4d1e-b32c-5937ef32866d", "Open Job Workflows Module"));
				contextMenuStrip.Items.Add(editSchedulesMenuItem);
				editSchedulesMenuItem.DropDownItems.Add(new ZToolStripMenuItem(thisSectionText, OpenJobWorkflowsThisSection_Click));
				editSchedulesMenuItem.DropDownItems.Add(new ZToolStripMenuItem(thisCellText, openJobWorkflowsModuleCellEventHandler));
			}
			else
			{
				contextMenuStrip.Items.Add(new ZToolStripMenuItem(Res.GetString("16f3f90a-ec73-4acc-907f-04cbe78071e9", "Open Job Workflows Module (this section)"), OpenJobWorkflowsThisSection_Click));
			}

			contextMenuStrip.ImageScalingSize = ControlDpiScalingHelper.NewScaledSize(16, 16, true);
			ContextMenuStrip = contextMenuStrip;
			foreach (var panel in visitedTaskPanels)
			{
				panel.ContextMenuStrip = contextMenuStrip;
			}
			if (currentTaskPanel != null)
			{
				visitedTaskPanels.Add(currentTaskPanel);
			}
		}

		readonly List<TaskPanel> visitedTaskPanels = new List<TaskPanel>();

		void ConstrainedModeMenuItem_ModeChanged(object sender, EventArgs e)
		{
			var visualBoardForm = ParentForm as VisualBoardForm;
			if (visualBoardForm != null)
			{
				visualBoardForm.ReloadBoard();
			}
		}

		void ClearMenuItems(ZToolStripMenuItem menuItem)
		{
			foreach (var item in menuItem.DropDownItems.Cast<IDisposable>().ToArray())
			{
				item.Dispose();
			}

			menuItem.DropDownItems.Clear();
		}

		void LegendMenuItem_Click(object sender, EventArgs e)
		{
			var legendForm = new LegendForm();
			ZFormModaliser.Show(legendForm, FindForm());
		}

		void FilterStripMenuItem_Click(object sender, EventArgs e)
		{
			ShowFilterDialog();
		}

		void EditSchedulesThisSection_Click(object sender, EventArgs e)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = GetType().Name + ": EditSchedulesThisSection_Click" };
			var errorMessage = Res.GetString("3c9ab5cb-9110-4797-ab21-4846347eecf8", "This section has no workflows to edit schedules for.");
			EditSchedules(ViewModel.SectionWorkflowPKsWithTaskAndSectionFiltersApplied, factory, errorMessage);
		}

		void OpenJobWorkflowsThisSection_Click(object sender, EventArgs e)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = GetType().Name + ": OpenJobWorkflowsThisSection_Click" };
			var errorMessage = Res.GetString("c301e6b0-7184-4408-a2c9-d02eaf454dd8", "This section has no workflows to open.");
			OpenJobWorkflows(ViewModel.SectionWorkflowPKsWithTaskAndSectionFiltersApplied, factory, errorMessage);
		}

		internal static void OpenJobWorkflows(IEnumerable<ZGuid> workflowPKs, BusinessObjectFactory factoryForFilterLayout, string errorMessage)
		{
			if (workflowPKs.IsNullOrEmpty())
			{
				Globals.Message.ShowError(errorMessage);
			}
			else
			{
				var emptyLayout = factoryForFilterLayout.New<StmModuleFilter>();
				var query = new ZQuery(ProcessHeaderSchema.PK, workflowPKs);
				var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.ProcessHeader);

				module.AddAdditionalDisplayFilter = additionalQuery => additionalQuery.AddToFilter(query);

				var filterControl = (ZFilterStripCommonControl)module.EmbeddedControl;

				filterControl.RunSearchOnEnteringAModuleOverride = true;
				filterControl.CaptionRenderingEnabled = false;

				var jobWorkflowEmbeddedModulePopup = new EmbeddedModulePopup(module, emptyLayout, true) { IsSelectionMandatory = false };
				jobWorkflowEmbeddedModulePopup.Show();
			}
		}

		internal static void EditSchedules(IEnumerable<ZGuid> workflowPKs, BusinessObjectFactory factory, string errorMessage)
		{
			if (workflowPKs.IsNullOrEmpty())
			{
				Globals.Message.ShowError(errorMessage);
			}
			else
			{
				var workflows = factory.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.PK, workflowPKs));
				MultiJobHeaderEditorForm.ShowFormIfAllowed(workflows, factory);
			}
		}

		#endregion

		#region Filter Strips

		public void ShowFilterDialog()
		{
			var potentialWorkflowFilter = (StmModuleFilter)boardWorkflowFilter.Clone();

			var potentialTaskFilter = (StmModuleFilter)boardTaskFilter.Clone();

			var filterStripViewModel = new StmModuleFilterViewModel(potentialWorkflowFilter, potentialTaskFilter, ViewModel.SectionName);

			var filterStripsForm = new FilterStripsForm(filterStripViewModel);
			if (ZFormModaliser.ShowDialogAndDispose(filterStripsForm) == DialogResult.OK)
			{
				CopyWorkflowAndTaskFiltersAndRefresh(potentialWorkflowFilter, potentialTaskFilter);
			}
		}

		void CopyWorkflowAndTaskFiltersAndRefresh(StmModuleFilter workflowFilter, StmModuleFilter taskFilter)
		{
			var layoutHelper = new EmptyLayoutsHelper();
			var workflowFilterHasChanged = workflowFilter != null && StmModuleFilter.AreFiltersDifferent(boardWorkflowFilter, workflowFilter, layoutHelper);
			var taskFilterHasChanged = taskFilter != null && StmModuleFilter.AreFiltersDifferent(boardTaskFilter, taskFilter, layoutHelper);

			if (workflowFilterHasChanged)
			{
				boardWorkflowFilter = (StmModuleFilter)workflowFilter.Clone();
				ViewModel.OverriddenWorkflowSectionFilter = RelatedModuleFiltersHelper.GetFilterQuerySafe(boardWorkflowFilter);
			}

			if (taskFilterHasChanged)
			{
				boardTaskFilter = (StmModuleFilter)taskFilter.Clone();
				var taskBusinessObject = RelatedModuleFiltersHelper.GetNewFilterBusinessObject(ModuleIDs.ProcessTasks);
				var needToSetFilter = isSectionConfigurationTaskFilterSet || taskBusinessObject.GetFilterStripsCount(boardTaskFilter) > 0;
				ViewModel.OverriddenTaskSectionFilter = needToSetFilter ? taskBusinessObject.GetFilter(boardTaskFilter, true) : null;
			}

			if (workflowFilterHasChanged || taskFilterHasChanged)
			{
				((IBoardSectionControl)this).Refresh(new BoardRefreshEventArgs { TriggeredByUserRefreshingBoardOrSection = true });
			}
		}

		static StmModuleFilter GetFilterInCorrectFactory(StmModuleFilter filter, BusinessObjectFactory filterFactory)
		{
			if (filter.Factory == filterFactory)
			{
				return filter;
			}

			var query = new ZQuery(StmModuleFilterSchema.PK, filter.PK) { FetchOnlyFromLocalCache = true };
			var cachedFilter = filterFactory.LoadTop1<StmModuleFilter>(query);

			return cachedFilter ?? (StmModuleFilter)filterFactory.ImportFromAnotherFactory(filter);
		}

		StmModuleFilter boardWorkflowFilter;
		StmModuleFilter boardTaskFilter;

#if DEBUG
		public void SetCustomWorkflowFilterForTestingAndRefresh(StmModuleFilter filter)
		{
			CopyWorkflowAndTaskFiltersAndRefresh(workflowFilter: filter, taskFilter: null);
		}

		public void SetCustomTaskFilterForTestingAndRefresh(StmModuleFilter filter)
		{
			CopyWorkflowAndTaskFiltersAndRefresh(workflowFilter: null, taskFilter: filter);
		}
#endif

		#endregion

		#region Section Sub-heading Label

		ZLabel subHeadingLabel;

		ZLabel GetSectionSubHeadingLabel()
		{
			return new ZLabel
			{
				AutoSize = true,
				Dock = DockStyle.Fill,
				Text = ViewModel.SubHeadingAppearance.SectionSubHeading,
				TextAlign = ContentAlignment.MiddleCenter,
				Font = new Font("Arial", 10F),
				Name = "SubHeadingLabel",
			};
		}

		protected virtual void UpdateSubHeadingContent()
		{
			if (!IsDisposed)
			{
				var viewModel = ViewModel.SubHeadingAppearance;

				if (subHeadingLabel == null && !string.IsNullOrEmpty(viewModel.SectionSubHeading))
				{
					subHeadingLabel = GetSectionSubHeadingLabel();

					sectionPanel.Controls.Add(subHeadingLabel);
				}

				if (subHeadingLabel != null && !subHeadingLabel.IsDisposed)
				{
					subHeadingLabel.Text = viewModel.SectionSubHeading;

					if (!string.IsNullOrEmpty(viewModel.SectionSubHeadingDetailText))
					{
						AddCalculationDetailsMenuItem(subHeadingLabel);

						if (!SectionLabel.IsDisposed)
						{
							AddCalculationDetailsMenuItem(SectionLabel);
						}
					}
				}

				if (!viewModel.SectionHeadingBackgroundColor.IsEmpty && !SectionLabel.IsDisposed)
				{
					SectionLabel.BackColor = viewModel.SectionHeadingBackgroundColor;
					SectionLabel.ForeColor = viewModel.SectionHeadingForegroundColor;

					if (subHeadingLabel != null && !subHeadingLabel.IsDisposed)
					{
						subHeadingLabel.BackColor = viewModel.SectionHeadingBackgroundColor;
						subHeadingLabel.ForeColor = viewModel.SectionHeadingForegroundColor;
					}
				}
			}
		}

		void AddCalculationDetailsMenuItem(Control controlToAddMenu)
		{
			controlToAddMenu.ContextMenuStrip = new LazyContextMenuStrip((strip, control) =>
			{
				strip.Items.Add(new ZToolStripMenuItem(Res.GetString("4e60e129-d5b6-4d8b-9e80-5632b59b9c05", "Show Calculation Details"), OnSubHeadingShowCalculationDetailsClick));
			}, clearOnClose: false);
		}

		void OnSubHeadingShowCalculationDetailsClick(object sender, EventArgs eventArgs)
		{
			var appearance = ViewModel.SubHeadingAppearance;
			Globals.Message.Show(appearance.SectionSubHeadingDetailText, appearance.SectionSubHeading, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		#endregion

		#endregion

		#region Cell controls

		Control GetControl(CellContent cell)
		{
			var control = GetControlCore(cell);
			control.Dock = DockStyle.Fill;
			control.Margin = Padding.Empty;
			control.BackColor = GetBackgroundColor(cell);

			if (cell.ForeColor != null)
			{
				control.ForeColor = cell.ForeColor.Value;
			}

			return control;
		}

		Control GetControlCore(CellContent cell)
		{
			switch (cell.ContentType)
			{
				case CellContentType.Label:
					return GetLabel(cell);

				case CellContentType.ChannelHeading:
					return GetHeaderControlCore(cell, this, ViewModel, () =>
						{
							var control = new ChannelHeaderControl(cell, ViewModel);
							if (!ViewModel.IsPreview)
							{
								HeadingsRefreshed += control.RefreshHeading;
							}
							return control;
						});

				case CellContentType.Cards:
					if (ViewModel.IsPreview)
					{
						return GetLabel(cell);
					}
					else if (SectionConfiguration.IsReleaseScheduler)
					{
						return new ReleaseSchedulerTaskPanel(cell, ViewModel, sharedTaskMenuStrip, this, SectionConfiguration.Section);
					}
					else
					{
						if (cachedTaskCardRenderer == null)
						{
							cachedTaskCardRenderer = TaskCardRendererFactory.GetRenderer(ViewModel, false, r => new CachingTaskCardRenderer(r));
						}

						return new TaskPanel(cell, ViewModel, sharedTaskMenuStrip, cachedTaskCardRenderer, this, SectionConfiguration.Section);
					}
				case CellContentType.AgeHeading:
				case CellContentType.ZoneHeading:
				case CellContentType.CCRHeading:
				case CellContentType.SubComponentZoneHeading:
					return GetHeaderControlCore(cell, this, ViewModel, () => GetLabel(cell));
				default:
					throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "CellContentType of {0} is not supported", cell.ContentType));
			}
		}

		protected virtual Control GetHeaderControlCore(CellContent cell, BMComponentControl componentControl, BMBoardSectionViewModel viewModel, Func<Control> controlToWrapGetter)
		{
			return HeaderControlProvider.GetHeaderControl(cell, componentControl, viewModel, controlToWrapGetter);
		}

		Label GetLabel(CellContent cell)
		{
			var label = new DirectionalLabel(cell, ViewModel.GradientAngle,
				isVertical: (cell.ContentType.IsZoneHeading() || (new[] { CellContentType.AgeHeading, CellContentType.CCRHeading }).Contains(cell.ContentType)) && IsVertical);

			label.TextAlign = ContentAlignment.MiddleCenter;
			if (cell.ContentType.In(CellContentType.ZoneHeading, CellContentType.SubComponentZoneHeading))
			{
				label.Font = new Font(label.Font.FontFamily, 8.25f, FontStyle.Bold);
			}
			label.Text = GetLabelText(cell);
			if (!ViewModel.IsPreview)
			{
				label.ContextMenuStrip = GetLabelContextMenuStrip(cell);
				if (label.ContextMenuStrip != null)
				{
					label.Disposed += (s, e) =>
					{
						var senderAsLabel = (Label)s;
						senderAsLabel.ContextMenuStrip.Dispose();
						senderAsLabel.ContextMenuStrip = null;
					};
				}
			}

			return label;
		}

		static ContextMenuStrip GetLabelContextMenuStrip(CellContent cell)
		{
			return cell.SubComponentZones.Any()
				? new LazyContextMenuStrip((strip, _) => strip.Items.Add(new OpenSubComponentBoardMenuItem(cell.SubComponentHeadingPK)), false)
				: new LazyContextMenuStrip(null, false);
		}

		static string GetLabelText(CellContent cell)
		{
			switch (cell.ContentType)
			{
				case CellContentType.Cards:
					return string.Empty;
				case CellContentType.ZoneHeading:
				case CellContentType.SubComponentZoneHeading:
				case CellContentType.Label:
					return cell.Label ?? string.Empty;
				case CellContentType.ChannelHeading:
					return Res.GetString("3f5cdd80-8517-4169-808b-0c3fb8848d90", "Channel");
				case CellContentType.CCRHeading:
					return cell.Zone == 2 ? Res.GetString("73D2F9A2-DF97-4DF4-B882-F222C65CF857", "CCR target") : string.Empty;

				default:
					return cell.Label ?? cell.ContentType.ToString();
			}
		}

		static Color GetBackgroundColor(CellContent cell)
		{
			switch (cell.ContentType)
			{
				case CellContentType.ChannelHeading:
					return Color.LightYellow;
				case CellContentType.Cards:
				case CellContentType.ZoneHeading:
				case CellContentType.SubComponentZoneHeading:
				case CellContentType.AgeHeading:
				case CellContentType.CCRHeading:
					return cell.BackColor != null ? cell.BackColor.Value : Color.White;

				default:
					return Color.White;
			}
		}

		#endregion

		#region Table layout

		IEnumerable<TaskPanel> TaskPanels => ViewModel.ComponentGrid.CardCells.Select(c => Table.GetControlFromPosition(c.Column, c.Row)).OfType<TaskPanel>().ToArray();

		IEnumerable<TaskCardControl> TaskCards => TaskPanels.SelectMany(p => p.TaskCards).ToArray();

#if !WINZOR

		HashSet<CardBitmaps> CardBitmaps => TaskCards.Select(c => c.BackgroundBitmaps).Where(b => b != null).ToHashSet();

#endif

#if DEBUG
		public
#endif
 TableLayoutPanel Table
		{
			get { return ContentTable; }
		}

		internal void EnsureAppropriateZoneHeadingsVisible(float size, CellContent cell, BMBoardSectionOrientation orientation, BMComponent component, bool hideZoneHeading)
		{
			var cellSize = (component != null &&
				cell.ContentType == CellContentType.SubComponentZoneHeading &&
				cell.SubComponentHeadingPK == component.PK)
				|| cell.ContentType == CellContentType.ZoneHeading && !hideZoneHeading ? size : 0f;

			if (orientation == BMBoardSectionOrientation.Vertical)
			{
				Table.ColumnStyles[cell.Column].Width = cellSize;
			}
			else if (orientation == BMBoardSectionOrientation.Horizontal)
			{
				Table.RowStyles[cell.Row].Height = cellSize;
			}
		}

		internal void RefreshTaskPanels()
		{
			foreach (var panel in TaskPanels)
			{
				panel.Border = null;
			}
			Refresh();
		}

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		void SetRowAndColumnStyles(TableLayoutPanel table, ComponentGrid componentGrid)
		{
			SetRowStyles(table, componentGrid, SectionConfiguration.Subsections, SectionConfiguration.CellsPerSubsection);
			SetColumnStyles(table, componentGrid, SectionConfiguration.Subsections, SectionConfiguration.CellsPerSubsection);
		}

		void SetRowStyles(TableLayoutPanel table, ComponentGrid componentGrid, ZInt subsections, ZInt cellsPerSubsection)
		{
			for (var i = 0; i < table.RowCount; i++)
			{
				var row = table.RowStyles[i];
				if (AreAllRowCellsOfTypeOrEmpty(i, componentGrid, CellContentType.AgeHeading, CellContentType.CCRHeading))
				{
					row.SizeType = SizeType.Absolute;
					ControlDpiScalingHelper.SetHeight(row, ZoneHeaderSize, true);
				}
				else if (AreAllRowCellsOfTypeOrEmpty(i, componentGrid, CellContentType.ZoneHeading))
				{
					var shouldShowZoneHeading = componentGrid.CurrentlySelectedComponent == null ||
						!componentGrid.CurrentlySelectedComponent.IsChildBuffer;
					row.SizeType = SizeType.Absolute;
					ControlDpiScalingHelper.SetHeight(row, shouldShowZoneHeading ? ZoneHeaderSize : 0, true);
					ViewModel.SetOriginalRowHeight(i, ControlDpiScalingHelper.ScaleToCurrentDpiX(ZoneHeaderSize));
				}
				else if (AreAllRowCellsOfTypeOrEmpty(i, componentGrid, CellContentType.SubComponentZoneHeading))
				{
					var shouldShowSubComponentZoneHeading = ShouldShowSubComponentZoneHeading(componentGrid, i, CellContentType.SubComponentZoneHeading, SectionConfiguration, isRow: true);
					row.SizeType = SizeType.Absolute;
					ControlDpiScalingHelper.SetHeight(row, shouldShowSubComponentZoneHeading ? ZoneHeaderSize : 0, true);
				}
				else if (AreAllRowCellsOfTypeOrEmpty(i, componentGrid, CellContentType.ChannelHeading, CellContentType.AgeHeading))
				{
					row.SizeType = SizeType.Absolute;
					ControlDpiScalingHelper.SetHeight(row, ChannelHeaderSize, true);
				}
				else
				{
					ControlDpiScalingHelper.SetHeight(row, (int)Math.Ceiling(ViewModel.Orientation == BMBoardSectionOrientation.Horizontal
						? 100D / Math.Max(subsections, 1)
						: 100D / Math.Max(cellsPerSubsection, 1)), true);
				}

				if (!AreAllRowCellsOfTypeOrEmpty(i, componentGrid, CellContentType.ZoneHeading))
				{
					ViewModel.SetOriginalRowHeight(i, row.Height);
				}
			}
		}

		void SetColumnStyles(TableLayoutPanel table, ComponentGrid componentGrid, ZInt subsections, ZInt cellsPerSubsection)
		{
			for (var i = 0; i < table.ColumnCount; i++)
			{
				var col = table.ColumnStyles[i];
				if (AreAllColumnCellsOfTypeOrEmpty(i, componentGrid, CellContentType.AgeHeading, CellContentType.CCRHeading))
				{
					col.SizeType = SizeType.Absolute;
					ControlDpiScalingHelper.SetWidth(col, ZoneHeaderSize, true);
				}
				else if (AreAllColumnCellsOfTypeOrEmpty(i, componentGrid, CellContentType.ZoneHeading))
				{
					var shouldShowZoneHeading = componentGrid.CurrentlySelectedComponent == null ||
						!componentGrid.CurrentlySelectedComponent.IsChildBuffer;
					col.SizeType = SizeType.Absolute;
					ControlDpiScalingHelper.SetWidth(col, shouldShowZoneHeading ? ZoneHeaderSize : 0, true);
					ViewModel.SetOriginalColumnWidth(i, ControlDpiScalingHelper.ScaleToCurrentDpiY(ZoneHeaderSize));
				}
				else if (AreAllColumnCellsOfTypeOrEmpty(i, componentGrid, CellContentType.SubComponentZoneHeading))
				{
					var shouldShowSubComponentZoneHeading = ShouldShowSubComponentZoneHeading(componentGrid, i, CellContentType.SubComponentZoneHeading, SectionConfiguration, isRow: false);
					col.SizeType = SizeType.Absolute;
					ControlDpiScalingHelper.SetWidth(col, shouldShowSubComponentZoneHeading ? ZoneHeaderSize : 0, true);
				}
				else if (AreAllColumnCellsOfTypeOrEmpty(i, componentGrid, CellContentType.ChannelHeading, CellContentType.AgeHeading))
				{
					col.SizeType = SizeType.Absolute;
					ControlDpiScalingHelper.SetWidth(col, ChannelHeaderSize, true);
				}
				else
				{
					ControlDpiScalingHelper.SetWidth(col, ViewModel.Orientation == BMBoardSectionOrientation.Horizontal
						? 100 / Math.Max(cellsPerSubsection, 1)
						: 100 / Math.Max(subsections, 1), true);
				}

				if (!AreAllColumnCellsOfTypeOrEmpty(i, componentGrid, CellContentType.ZoneHeading))
				{
					ViewModel.SetOriginalColumnWidth(i, col.Width);
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		static bool ShouldShowSubComponentZoneHeading(ComponentGrid componentGrid, int i, CellContentType contentType, BMComponentSectionConfiguration sectionConfiguration, bool isRow)
		{
			var cell = componentGrid.Cells.First(c => (isRow ? c.Row : c.Column) == i && c.ContentType == contentType);
			if (sectionConfiguration.ShowChildComponentZones && cell.ContentType == CellContentType.SubComponentZoneHeading)
			{
				return true;
			}

			return componentGrid.CurrentlySelectedComponent != null && cell.SubComponentHeadingPK == componentGrid.CurrentlySelectedComponent.PK;
		}

		static bool AreAllRowCellsOfTypeOrEmpty(int row, ComponentGrid componentGrid, params CellContentType[] cellTypes)
		{
			return componentGrid.Cells.Where(c => c.Row == row).All(c => cellTypes.Any(t => c.ContentType == t) || c.ContentType == CellContentType.Label);
		}

		static bool AreAllColumnCellsOfTypeOrEmpty(int col, ComponentGrid componentGrid, params CellContentType[] cellTypes)
		{
			return componentGrid.Cells.Where(c => c.Column == col).All(c => cellTypes.Any(t => c.ContentType == t) || c.ContentType == CellContentType.Label);
		}

		void CollapseHeadings(TableLayoutPanel table, ComponentGrid componentGrid)
		{
			CollapseRows(table, componentGrid);
			CollapseColumns(table, componentGrid);
		}

		CellContentType[] GetContentTypesToCollapse(bool shouldCollapse)
		{
			return shouldCollapse
				? new[] { CellContentType.ZoneHeading, CellContentType.SubComponentZoneHeading, CellContentType.Label, CellContentType.CCRHeading }
				: new[] { CellContentType.ZoneHeading, CellContentType.Label, CellContentType.CCRHeading };
		}

		void CollapseRows(TableLayoutPanel table, ComponentGrid componentGrid)
		{
			CollapseRows(table, componentGrid, GetContentTypesToCollapse(IsHorizontal));
		}

		static void CollapseRows(TableLayoutPanel table, ComponentGrid componentGrid, params CellContentType[] contentTypesToCollapse)
		{
			var collapsibleCells =
				from c in componentGrid.Cells
				where contentTypesToCollapse.Any(type => type == c.ContentType)
				orderby c.Column
				group c by new { c.Row, c.Zone, c.Label, c.ContentType } into grp
				where grp.Take(2).Count() > 1
				select grp;

			Collapse(collapsibleCells, table, (control, length) => table.SetColumnSpan(control, length));
		}

		void CollapseColumns(TableLayoutPanel table, ComponentGrid componentGrid)
		{
			CollapseColumns(table, componentGrid, GetContentTypesToCollapse(IsVertical));
		}

		static void CollapseColumns(TableLayoutPanel table, ComponentGrid componentGrid, params CellContentType[] contentTypesToCollapse)
		{
			var collapsibleCells =
				from c in componentGrid.Cells
				where contentTypesToCollapse.Any(type => type == c.ContentType)
				orderby c.Row
				group c by new { c.Column, c.Zone, c.Label, c.ContentType } into grp
				where grp.Take(2).Count() > 1
				select grp;

			Collapse(collapsibleCells, table, (control, length) => table.SetRowSpan(control, length));
		}

		static void Collapse(IEnumerable<IEnumerable<CellContent>> collapsibleCellGroups, TableLayoutPanel table, Action<Control, int> tableCollapseAction)
		{
			foreach (var group in collapsibleCellGroups)
			{
				var groupCells = group.ToList();
				while (groupCells.Count > 0)
				{
					var contiguousCells = new List<CellContent>();
					int? lastPosition = null;

					foreach (var cell in groupCells.OrderBy(c => c.Column).ThenBy(c => c.Row).ToArray())
					{
						var cellPosition = cell.Column + cell.Row;
						if (lastPosition != null)
						{
							if (Math.Abs(cellPosition - lastPosition.Value) > 1)
							{
								lastPosition = null;

								break;
							}
						}

						lastPosition = cellPosition;
						contiguousCells.Add(cell);
						groupCells.Remove(cell);
					}

					CollapseCore(contiguousCells.ToArray(), table, tableCollapseAction);
					contiguousCells.Clear();
				}
			}
		}

		static void CollapseCore(CellContent[] groupCells, TableLayoutPanel table, Action<Control, int> tableCollapseAction)
		{
			var cells = groupCells.Where(c => c != null).Select(c => c).ToArray();

			if (cells.Length > 1)
			{
				var collapsedToCell = cells[0];
				foreach (var cell in cells.Skip(1))
				{
					var control = table.GetControlFromPosition(cell.Column, cell.Row);
					cell.CollapsedToCell = collapsedToCell;
					table.Controls.Remove(control);
					control.Dispose();
				}

				var collapsedToControl = table.GetControlFromPosition(collapsedToCell.Column, collapsedToCell.Row);
				tableCollapseAction(collapsedToControl, groupCells.Length);
			}
		}

		const int ZoneHeaderSize = 20;
		const int ChannelHeaderSize = 40;

		#endregion

		#region RefreshHeadings

		public event EventHandler<HeadingsRefreshedEventArgs> HeadingsRefreshed;

		void RefreshHeadings(ChannelHeadingViewModelSet channelViewModels)
		{
			HeadingsRefreshed?.Invoke(this, new HeadingsRefreshedEventArgs(channelViewModels));
		}

		#endregion

		#region SetupTasks

		DisposableList setupTasksDisposableActions;

		public void SetupCards(BoardRefreshEventArgs args)
		{
			ResetRefreshedData();

			setupTasksDisposableActions = new DisposableList(1);

			if (!isRefreshingSection)
			{
				StartSectionLoadingTimer();

				isRefreshingSection = true;

				if (refreshResultSet != null)
				{
					refreshResultSet.Dispose();
				}

				var loadSetup = new LoadCardContentSetup(ViewModel);
				var engine = BoardSectionRefreshPipeEngine.Create(args, loadSetup, this);

				var asyncStrategy = PipeDispatcherStrategy.GetAsyncStrategy();

				refreshResultSet = engine.ExecuteAll(asyncStrategy, PipeDispatcherStrategy.GetSyncDispatcher(this));
			}
		}

		HashSet<ChannelHeaderControl> RefreshedChannelHeaderControls;

		void ChannelHeader_RefreshCompleted(object sender, EventArgs args)
		{
			if (sender != null)
			{
				var control = (ChannelHeaderControl)sender;
				control.RefreshCompleted -= ChannelHeader_RefreshCompleted;
				RefreshedChannelHeaderControls.Add(control);

				NotifySectionFinishRefreshed();
			}
		}

		List<BoardRefreshEventArgs> refreshedBoardRefreshEventArgs;

		internal void NotifyComponentFinishRefreshed(BoardRefreshEventArgs args)
		{
			refreshedBoardRefreshEventArgs.Add(args);
			NotifySectionFinishRefreshed();
		}

		void NotifySectionFinishRefreshed()
		{
			if (HaveRefreshedBoardEventArgsBeenSet && RefreshedChannelHeaderControls.SetEquals(GetChannelHeaderControls()))
			{
				NotifySectionFinishedLoading(refreshedBoardRefreshEventArgs.First());
			}
		}

		bool HaveRefreshedBoardEventArgsBeenSet
		{
			get
			{
				return refreshedBoardRefreshEventArgs != null
					&& (
						(AcceptabilityBandTiles == null && refreshedBoardRefreshEventArgs.Count == 1)
						|| refreshedBoardRefreshEventArgs.Count == 2
					);
			}
		}

		IEnumerable<ChannelHeaderControl> GetChannelHeaderControls()
		{
			return this.FindAll<ChannelHeaderControl>(maxLevelsDeep: 2);
		}

		internal void NotifySectionFinishedLoading(BoardRefreshEventArgs args)
		{
			setupTasksDisposableActions?.Dispose();
			setupTasksDisposableActions = null;

			OnRefreshCompleted(args);
		}

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		void StartSectionLoadingTimer()
		{
			var stopwatch = Stopwatch.StartNew();

			setupTasksDisposableActions.Add(new DisposableAction(() =>
			{
				stopwatch.Stop();
				SectionLabel.Text = ViewModel.SectionName;

				ToolTipService.SetToolTip(SectionLabel, Res.GetString("CEC7F2C2-99EC-4F93-9556-AB13A21474BA", "Loaded section in {0} seconds", Utilities.Round(Convert.ToDecimal(stopwatch.Elapsed.TotalSeconds), 1)));
			}));
		}

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		void ComponentGrid_LoadFailed(object sender, ComponentGrid.LoadFailureEventArgs e)
		{
			IsShowingLoadError = true;

			Controls.RemoveAndDisposeAll();

			var label = new ZLabel
			{
				Text = e.Failure.FailureText,
				Name = "LoadFailedLabel",
				Dock = DockStyle.Fill,
				TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
				Padding = ControlDpiScalingHelper.NewScaledPadding(20),
				Font = new Font(Font.FontFamily, 12f)
			};

			if (e.Failure.FailureException != null)
			{
				ToolTipService.SetToolTip(label, Res.GetString("74d36113-4cb2-4330-bb7e-2dd001a0af08", "Click to see exception details."));

				label.Cursor = Cursors.Hand;
				label.Click += (s, e2) => Globals.Message.ShowError(e.Failure.FailureException.ToString());
			}

			Controls.Add(label);

			ContextMenuStrip = null;
		}

		internal bool IsShowingLoadError { get; private set; }

		void RefreshHeadingsAsync(string nameForDebugging, CardAllocationMap allocationMap, bool clearCache = true)
		{
			AsyncStrategy.Default.DoAsync(() =>
			{
				using (PerformanceStatisticsCollector.StartMonitoring(GetType().Name + ".RefreshHeadingsAsync", ViewModel.SectionName))
				{
					var channels = ViewModel.PrimaryChannels;
					if (clearCache)
					{
						channels.ForEach(c => c.ClearChannelCache());
					}

					var headingFactory = ViewModel.FactoryProvider.GetNewBackgroundThreadLoaderFactory(nameForDebugging + " RefreshHeadings");
					var channelViewModels = channels.MakeViewModelSet(headingFactory, ViewModel, allocationMap);
					RefreshHeadings(channelViewModels);
				}
			});
		}

		internal IDisposable DrawAsBitmapWrapper()
		{
			try
			{
				return this.TemporarilyDrawAsBitmap();
			}
			catch (ArgumentException)
			{
				return null; // Tasks try to render in Error Reports. Error reports shouldn't throw exceptions.
			}
		}

		volatile bool isRefreshingSection;

		void ComponentGrid_CellRefreshFinished(object sender, EventArgs e)
		{
			if (cachedTaskCardRenderer != null)
			{
				cachedTaskCardRenderer.OnRenderingCardsCompleted();
			}

#if !WINZOR

			ViewModel.RemoveUnusedBitmapsFromCache(usedBitmaps: CardBitmaps);

#endif
		}

		void ComponentGrid_CellRefreshStarted(object sender, EventArgs e)
		{
			var form = FindForm();
			if (form != null)
			{
				form.ActiveControl = null;
			}
		}

		ITaskCardRenderer cachedTaskCardRenderer;

		void TryDisposeTaskRenderer()
		{
			var disposable = cachedTaskCardRenderer as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}

		void EnsureTasksLaidOutAppropriately()
		{
			if (Table != null)
			{
				for (var col = 0; col < Table.ColumnCount; col++)
				{
					for (var row = 0; row < Table.RowCount; row++)
					{
						var control = Table.GetControlFromPosition(col, row) as ITasksControl;
						if (control != null)
						{
							control.SetupTasks(disposeExistingTickets: false);
						}
					}
				}
			}
		}

		#endregion

		#region Loading Indicator

		void ViewModel_LoadingIndicatorMessageUpdated(object sender, LoadingIndicatorEventArgs e)
		{
			if (string.IsNullOrEmpty(e.Message))
			{
				RemoveAndDisposeLoadingIndicator();

				if (e.RefreshType == BoardRefreshType.Partial && isRefreshingSection)
				{
					isRefreshingSection = false;
				}
			}
			else
			{
				if (e.RefreshType == BoardRefreshType.Partial && !isRefreshingSection)
				{
					isRefreshingSection = true;
				}

				ShowLoadingSectionOverlayControl(e);
			}
		}

		internal void ShowLoadingSectionOverlayControl(LoadingIndicatorEventArgs args)
		{
			if (loadingSectionControl == null)
			{
				loadingSectionControl = new LoadingSectionControl();

				AddAndRefineLoadingOverlayControl();

				if (args.IsForInitialLayoutRenderOrUserForcedRefresh)
				{
					// If statistics for this section exist, show section load progress here, otherwise show 'Loading...' or 'Refreshing...'.
					var estimatedLoadTimeForSectionInSeconds = ViewModel.BoardViewModel.Cache.GetCachedValue<decimal>(ViewModel.SectionPK, BoardViewModel.EstimatedLoadingTimeCacheKey);

					if (estimatedLoadTimeForSectionInSeconds > 0)
					{
						setSectionLoadingIndicatorActions = new DisposableList(1);
						loadingTicks = 1;
						SetSectionLoadingIndicator(estimatedLoadTimeForSectionInSeconds, args.IsForInitialLayoutRender);
					}
				}
			}

			loadingSectionControl.LoadingText = args.Message;
			loadingSectionControl.Refresh();
		}

		int loadingTicks;

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		void SetSectionLoadingIndicator(decimal estimatedLoadTimeForSectionInSeconds, bool isLoad)
		{
			var loadingIndicatorTimer = ObjectFactory.Get<IWindowsTimer>();
			loadingIndicatorTimer.Interval = LoadingIndicatorTimerInterval;
			loadingIndicatorTimer.Tick += SectionLoadingIndicatorTimer_Tick(estimatedLoadTimeForSectionInSeconds, isLoad);
			loadingIndicatorTimer.Start();

			setSectionLoadingIndicatorActions.Add(new DisposableAction(() =>
			{
				loadingIndicatorTimer.Stop();
				loadingIndicatorTimer.Tick -= SectionLoadingIndicatorTimer_Tick(estimatedLoadTimeForSectionInSeconds, isLoad);
			}));
		}

		public const int LoadingIndicatorTimerInterval = 50;

		DisposableList setSectionLoadingIndicatorActions;

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		EventHandler SectionLoadingIndicatorTimer_Tick(decimal estimatedLoadTimeForSectionInSeconds, bool isLoad)
		{
			return (s, e) =>
			{
				loadingTicks++;

				var percentComplete = new ZDecimal(Math.Min(99m, (loadingTicks / estimatedLoadTimeForSectionInSeconds) * 100)).Round(0);
				var message = isLoad ? Res.GetString("AA3AF6AF-9441-457E-9363-A05004E510F5", "Loading {0}%", percentComplete)
							: Res.GetString("DD9680B4-08DE-4DDA-8EC1-4BBF561CEEA3", "Refreshing {0}%", percentComplete);

				ViewModel.NotifyLoadingIndicatorNeedsUpdating(LoadingIndicatorEventArgs.ForSpecificMessage(message, BoardRefreshType.Full));
			};
		}

		#region Loading Overlay Control

		void AddAndRefineLoadingOverlayControl()
		{
			var widthMargin = 25;
			var heightMargin = 50;

			var suggestedX = Width / 2 - ControlDpiScalingHelper.MarkAsScaled(loadingSectionControl.Width / 2);
			var suggestedY = Height / 2 - ControlDpiScalingHelper.MarkAsScaled(loadingSectionControl.Height / 2);
			var relativeStartingLocation = ControlDpiScalingHelper.NewScaledPoint(suggestedX, suggestedY, false);
			if (relativeStartingLocation.X + loadingSectionControl.Width >= Width)
			{
				relativeStartingLocation = ControlDpiScalingHelper.NewScaledPoint(widthMargin, relativeStartingLocation.Y, false);
				loadingSectionControl.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(loadingSectionControl.Width > Width ? Width : loadingSectionControl.Width);
			}

			if (relativeStartingLocation.Y + loadingSectionControl.Height >= Height)
			{
				relativeStartingLocation = ControlDpiScalingHelper.NewScaledPoint(relativeStartingLocation.X, heightMargin, false);
				loadingSectionControl.Height = ControlDpiScalingHelper.ScaleToCurrentDpiX(loadingSectionControl.Height > Height ? Height - ControlDpiScalingHelper.MarkAsScaled(heightMargin) : loadingSectionControl.Height);
			}

			loadingSectionControl.Location = relativeStartingLocation;
			if (!loadingSectionControl.IsDisposed)
			{
				Controls.Add(loadingSectionControl);
				loadingSectionControl.BringToFront();
				loadingSectionControl.Disposed += LoadingOverlayControl_Disposed;
			}
		}

		void LoadingOverlayControl_Disposed(object sender, EventArgs e)
		{
			var loadingIndicatorControl = sender as LoadingSectionControl;
			if (loadingIndicatorControl != null)
			{
				loadingIndicatorControl.Disposed -= LoadingOverlayControl_Disposed;
				Controls.Remove(loadingIndicatorControl);
				PerformLayout();
			}
		}

		public LoadingSectionControl LoadingOverlaySectionControl
		{
			get { return loadingSectionControl; }
		}
		LoadingSectionControl loadingSectionControl;

		#endregion

		#endregion

		#region IHotkeyHandler Members

		bool IHotkeyHandler.ShouldHandle(Keys pressedKeys)
		{
			if (ShouldHandleBoardMeetingShortcuts && ShouldHandleKey(pressedKeys))
			{
				if (IsThisSectionHandlingChannelNavigation() && SectionConfiguration.Section.Board.Sections.Count > 1)
				{
					MaybeMarkOtherSectionAsHandingChannelNavigation(pressedKeys);
				}

				return true;
			}
			else
			{
				return false;
			}
		}

		static bool ShouldHandleKey(Keys pressedKeys)
		{
			return pressedKeys.In(BoardMeetingModeShortcuts.NextChannelKeyboardShortcut, BoardMeetingModeShortcuts.PreviousChannelKeyboardShortcut);
		}

		void IHotkeyHandler.HandleHotkeys(Keys pressedKeys)
		{
			if (ShouldHandleBoardMeetingShortcuts && IsThisSectionHandlingChannelNavigation())
			{
				switch (pressedKeys)
				{
					case BoardMeetingModeShortcuts.NextChannelKeyboardShortcut:
						CloseAllOpenOverlayControls();
						ExpandNextHeader();
						break;

					case BoardMeetingModeShortcuts.PreviousChannelKeyboardShortcut:
						CloseAllOpenOverlayControls();
						ExpandPreviousHeader();
						break;
				}
			}
		}

		bool ShouldHandleBoardMeetingShortcuts => SectionConfiguration.IsBuffer && IsInBoardMeeting;

		#endregion

		#region Filterable Implementation

		bool IsThisSectionHandlingChannelNavigation()
		{
			var value = ViewModel.BoardViewModel.Cache.GetCachedValue<ZGuid?>(ZGuid.Empty, MeetingModeTokenCacheKey, () => ViewModel.SectionPK);

			return value.HasValue && value.Value == ViewModel.SectionPK;
		}

		void MaybeMarkOtherSectionAsHandingChannelNavigation(Keys pressedKeys)
		{
			var bufferSections = SectionConfiguration.Section.Board.Sections.Where(s => s.MS_SectionType == BMConstants.ComponentSectionType && s.SectionConfiguration.IsBuffer).OrderBy(s => s.DisplaySequence).ToArray();

			if (bufferSections.Length > 0)
			{
				var expandedHeader = ExpandedHeaders.FirstOrDefault();

				if (expandedHeader != null && IsExpandedChannelOnSectionBoundary(expandedHeader, pressedKeys))
				{
					switch (pressedKeys)
					{
						case BoardMeetingModeShortcuts.NextChannelKeyboardShortcut:
							var followingSection = bufferSections.FirstOrDefault(s => s.DisplaySequence > SectionConfiguration.Section.DisplaySequence)
								?? (SectionConfiguration.Section == bufferSections.Last() ? bufferSections.First() : null); // Rollover back to the first section

							if (followingSection != null)
							{
								MakeOtherChannelHandleChannelNavigation(followingSection);
							}
							break;

						case BoardMeetingModeShortcuts.PreviousChannelKeyboardShortcut:
							var preceedingSection = bufferSections.LastOrDefault(s => s.DisplaySequence < SectionConfiguration.Section.DisplaySequence)
								?? (SectionConfiguration.Section == bufferSections.First() ? bufferSections.Last() : null); // Rollover back to the last section

							if (preceedingSection != null)
							{
								MakeOtherChannelHandleChannelNavigation(preceedingSection);
							}
							break;
					}
				}
			}
		}

		bool IsExpandedChannelOnSectionBoundary(CellContent expandedHeader, Keys pressedKeys)
		{
			var expandedChannelPK = expandedHeader.Channel.EntityPK;
			var orderedChannels = Lazy.Create(() => SectionConfiguration.OrderedPrimaryAxisChannels);

			switch (pressedKeys)
			{
				case BoardMeetingModeShortcuts.NextChannelKeyboardShortcut:
					return expandedChannelPK == orderedChannels.Value.LastOrDefault()?.MSC_ParentID;

				case BoardMeetingModeShortcuts.PreviousChannelKeyboardShortcut:
					return expandedChannelPK == orderedChannels.Value.FirstOrDefault()?.MSC_ParentID;

				default:
					return false;
			}
		}

		void CloseAllOpenOverlayControls()
		{
			BeginInvokeOnForm(form => form.CloseAllOpenOverlayControls());
		}

		const string MeetingModeTokenCacheKey = nameof(IsThisSectionHandlingChannelNavigation);

		protected override void OnFiltersChanged(FiltersChangedEventArgs e)
		{
			base.OnFiltersChanged(e);

			if (e.AddedFilters.Any(f => f is BoardMeetingModeFilter))
			{
				PrepareComponentForBoardMeetingMode();
			}
			else if (e.RemovedFilters.Any(f => f is BoardMeetingModeFilter && !(f is RiskFilter)))
			{
				CleanupComponentAfterBoardMeetingMode();
			}
		}

		#endregion

		#region Header Expand/Collapse

		bool IsInBoardMeeting => ViewModel.BoardViewModel.SlideShowViewModel.IsInBoardMeeting;

		readonly bool hasZones;

		void PrepareComponentForBoardMeetingMode()
		{
			if (ShouldHandleBoardMeetingShortcuts)
			{
				if (hasZones)
				{
					ExpandOrCollapseZonesForBoardMeetingMode(doExpand: true);
				}

				if (IsThisSectionHandlingChannelNavigation())
				{
					ExpandNextHeader();
				}
			}
		}

		void CleanupComponentAfterBoardMeetingMode()
		{
			ViewModel.BoardViewModel.Cache.Remove(ZGuid.Empty, MeetingModeTokenCacheKey);

			if (hasZones)
			{
				ExpandOrCollapseZonesForBoardMeetingMode(doExpand: false);
			}

			CollapseAllChannelHeaders();
		}

		void MakeOtherChannelHandleChannelNavigation(BMBoardSection otherSection)
		{
			ViewModel.BoardViewModel.Cache.OverwriteCachedValue(ZGuid.Empty, MeetingModeTokenCacheKey, otherSection.PK);
			CollapseAllChannelHeaders();
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Using factors, assigning on current DPI already")]
#if DEBUG
		public
#else
		internal
#endif
		void HeaderClicked(CellContent cell)
		{
			ExpandOrCollapseCell(cell, TableCellExpansionStrategy.ExpansionMode.Toggle);
		}

		void ExpandCell(CellContent cell)
		{
			ExpandOrCollapseCell(cell, TableCellExpansionStrategy.ExpansionMode.ExpandIfNotExpanded);
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Using factors, assigning on current DPI already")]
#if DEBUG
		public
#else
		internal
#endif
		void ExpandCurrentHeaderAndCollapseOthers(CellContent cell)
		{
			CollapseAllChannelHeaders();
			ExpandCell(cell);
		}

		void CollapseAllChannelHeaders()
		{
			expansionStrategy.ExpandOrCollapseCells(HeaderCells, TableCellExpansionStrategy.ExpansionMode.CollapseIfNotCollapsed);
		}

		void CollapseAllAgeHeaders()
		{
			expansionStrategy.ExpandOrCollapseCells(AgeHeaderCells, TableCellExpansionStrategy.ExpansionMode.CollapseIfNotCollapsed);
		}

		void ExpandOrCollapseCells(IEnumerable<CellContent> cells, TableCellExpansionStrategy.ExpansionMode mode)
		{
			expansionStrategy.ExpandOrCollapseCells(cells, mode);

			if (AreAllChannelHeadersExpanded())
			{
				CollapseAllChannelHeaders();
			}

			if (AreAllAgeHeadersExpanded())
			{
				CollapseAllAgeHeaders();
			}

			BeginInvokeOnForm(form => form.RedrawAllOverlayControls());
		}

		void BeginInvokeOnForm(Action<VisualBoardForm> action)
		{
			var form = FindForm() as VisualBoardForm;
			form?.BeginInvoke(new Action(() => action(form)));
		}

		void ExpandOrCollapseCell(CellContent cell, TableCellExpansionStrategy.ExpansionMode mode)
		{
			ExpandOrCollapseCells(new[] { cell }, mode);
		}

#if DEBUG
		public
#endif
		void ExpandNextHeader()
		{
			if (HeaderCells.Any())
			{
				CellContent cell;
				var lastExpandedHeader = ExpandedHeaders.LastOrDefault();

				if (IsVertical)
				{
					if (lastExpandedHeader == null || lastExpandedHeader == HeaderCells.Last(x => x.Row == lastExpandedHeader.Row))
					{
						cell = CollapsedHeaders.First();
					}
					else
					{
						cell = CollapsedHeaders.First(x => x.Column > lastExpandedHeader.Column);
					}
				}
				else
				{
					if (lastExpandedHeader == null || lastExpandedHeader == HeaderCells.Last(x => x.Column == lastExpandedHeader.Column))
					{
						cell = CollapsedHeaders.First();
					}
					else
					{
						cell = CollapsedHeaders.First(x => x.Row > lastExpandedHeader.Row);
					}
				}

				ExpandCurrentHeaderAndCollapseOthers(cell);
			}
		}

#if DEBUG
		public
#endif
		void ExpandPreviousHeader()
		{
			if (HeaderCells.Any())
			{
				CellContent cell;
				var firstExpandedHeader = ExpandedHeaders.FirstOrDefault();

				if (IsVertical)
				{
					if (firstExpandedHeader == null || firstExpandedHeader == HeaderCells.First(x => x.Row == firstExpandedHeader.Row))
					{
						cell = CollapsedHeaders.Last();
					}
					else
					{
						cell = CollapsedHeaders.Last(x => x.Column < firstExpandedHeader.Column);
					}
				}
				else
				{
					if (firstExpandedHeader == null || firstExpandedHeader == HeaderCells.First(x => x.Column == firstExpandedHeader.Column))
					{
						cell = CollapsedHeaders.Last();
					}
					else
					{
						cell = CollapsedHeaders.Last(x => x.Row < firstExpandedHeader.Row);
					}
				}

				ExpandCurrentHeaderAndCollapseOthers(cell);
			}
		}

		bool IsHeaderColumnExpanded(CellContent cell)
		{
			return Table.ColumnStyles[cell.Column].Width > ViewModel.GetOriginalColumnWidth(cell.Column);
		}

		bool IsHeaderRowExpanded(CellContent cell)
		{
			return Table.RowStyles[cell.Row].Height > ViewModel.GetOriginalRowHeight(cell.Row);
		}

		IEnumerable<CellContent> ExpandedHeaders
		{
			get { return IsVertical ? HeaderCells.Where(IsHeaderColumnExpanded) : HeaderCells.Where(IsHeaderRowExpanded); }
		}

		IEnumerable<CellContent> ExpandedAgeHeaders
		{
			get { return IsVertical ? AgeHeaderCells.Where(IsHeaderRowExpanded) : AgeHeaderCells.Where(IsHeaderColumnExpanded); }
		}

		IEnumerable<CellContent> CollapsedHeaders
		{
			get { return IsVertical ? HeaderCells.Where(x => !IsHeaderColumnExpanded(x)) : HeaderCells.Where(x => !IsHeaderRowExpanded(x)); }
		}

		CellContent[] HeaderCells
		{
			get { return headerCells ?? (headerCells = ViewModel.ComponentGrid.Cells.Where(x => x.ContentType == CellContentType.ChannelHeading).ToArray()); }
		}
		CellContent[] headerCells;

		CellContent[] AgeHeaderCells
		{
			get { return ageHeaderCells ?? (ageHeaderCells = ViewModel.ComponentGrid.Cells.Where(x => x.ContentType == CellContentType.AgeHeading).ToArray()); }
		}
		CellContent[] ageHeaderCells;

		bool AreAllChannelHeadersExpanded()
		{
			return ExpandedHeaders.Count() == HeaderCells.Length;
		}

		bool AreAllAgeHeadersExpanded()
		{
			return ExpandedAgeHeaders.Count() == AgeHeaderCells.Length;
		}

		#endregion

		#region Zones Expand/Collapse

		void ExpandOrCollapseZonesForBoardMeetingMode(bool doExpand)
		{
			if (doExpand)
			{
				CollapseAllChannelHeaders();
			}

			var allZoneHeaderCells = ViewModel.ComponentGrid.Cells.Where(x => x.ContentType == CellContentType.ZoneHeading).ToArray();
			if (allZoneHeaderCells.Any())
			{
				var mode = doExpand ? TableCellExpansionStrategy.ExpansionMode.ExpandIfNotExpanded : TableCellExpansionStrategy.ExpansionMode.CollapseIfNotCollapsed;

				ExpandOrCollapseZone0PrimaryComponent(allZoneHeaderCells, mode);
				ExpandOrCollapseZone1PrimaryComponent(allZoneHeaderCells, mode);

				if (ViewModel.IsInConstrainedMode)
				{
					ExpandOrCollapseSubComponentZone1(mode);
				}
				else
				{
					ExpandOrCollapseTopCellOfZone2PrimaryComponent(mode);
				}
			}
		}

		void ExpandOrCollapseZone0PrimaryComponent(IEnumerable<CellContent> allZoneHeaderCells, TableCellExpansionStrategy.ExpansionMode mode)
		{
			var zoneZeroCell = allZoneHeaderCells.First(x => x.Zone == 0);
			ExpandOrCollapseCell(zoneZeroCell, mode);
		}

		void ExpandOrCollapseZone1PrimaryComponent(IEnumerable<CellContent> allZoneHeaderCells, TableCellExpansionStrategy.ExpansionMode mode)
		{
			var zoneOneCell = allZoneHeaderCells.First(x => x.Zone == 1);
			ExpandOrCollapseCell(zoneOneCell, mode);
		}

		void ExpandOrCollapseSubComponentZone1(TableCellExpansionStrategy.ExpansionMode mode)
		{
			var subComponentCells = ViewModel.ComponentGrid.Cells.Where(c =>
				c.ContentType == CellContentType.Cards && c.IsInPreConstraint && c.SubComponentZones.Any(zone => zone.Value == 1));

			foreach (var subComponentCell in subComponentCells)
			{
				ExpandOrCollapseCell(subComponentCell, mode);
			}
		}

		void ExpandOrCollapseTopCellOfZone2PrimaryComponent(TableCellExpansionStrategy.ExpansionMode mode)
		{
			var lastCellInZone2 = ViewModel.ComponentGrid.Cells.Where(x => x.ContentType == CellContentType.AgeHeading && x.Zone == 2 && x.IsLastAgeIndexForZone).ToList();
			ExpandOrCollapseCells(lastCellInZone2, mode);
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			try
			{
				ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
				ViewModel.ComponentGrid.LoadFailed -= ComponentGrid_LoadFailed;
				ViewModel.ComponentGrid.HeadingsRefreshed -= ComponentGrid_HeadingsRefreshed;
				ViewModel.ComponentGrid.CellRefreshFinished -= ComponentGrid_CellRefreshFinished;
				ViewModel.ComponentGrid.CellRefreshStarted -= ComponentGrid_CellRefreshStarted;
				ViewModel.LoadingIndicatorMessageUpdated -= ViewModel_LoadingIndicatorMessageUpdated;
			}
			finally
			{
				base.Dispose(disposing);
			}

			if (disposing)
			{
				if (setupTasksDisposableActions != null)
				{
					setupTasksDisposableActions.Dispose();
				}

				if (setSectionLoadingIndicatorActions != null)
				{
					setSectionLoadingIndicatorActions.Dispose();
				}

				if (refreshResultSet != null)
				{
					refreshResultSet.Dispose();
				}

				if (!sharedTaskMenuStrip.IsDisposed)
				{
					sharedTaskMenuStrip.Dispose();
				}

				if (ContextMenuStrip != null)
				{
					ContextMenuStrip.Dispose();
				}

				TryDisposeTaskRenderer();
				ViewModel.DisposeAllBitmaps();
			}
		}

		EngineExecutionResultSet refreshResultSet;

		internal void NotifyRefreshFinished()
		{
			setSectionLoadingIndicatorActions?.Dispose();
			setSectionLoadingIndicatorActions = null;

			RemoveAndDisposeLoadingIndicator();
		}

		void RemoveAndDisposeLoadingIndicator()
		{
			loadingSectionControl?.Dispose();
			loadingSectionControl = null;
		}

		#endregion

		#region TryReallocateTask

#if DEBUG
		public
#endif
 readonly MultiActionButtonDialogWrapper<CrossChannelTaskAssignments> dialogWrapper = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();

		protected override IEnumerable<ProcessHeader> TryReallocateTaskCore(BusinessObjectFactory factory, TaskCardControl taskCard, Point currentMousePosition)
		{
			var result = Enumerable.Empty<ProcessHeader>();

			if (Table != null)
			{
				var locationWithinTable = ControlDpiScalingHelper.NewScaledPoint(currentMousePosition.X - Table.Left - Left, currentMousePosition.Y - Table.Top - Top, false);

				var row = Table.GetRow(locationWithinTable.Y);
				var column = Table.GetColumn(locationWithinTable.X);

				if (row >= 0 && column >= 0)
				{
					var firstCell = GetAppropriateCellToDragCardInto(taskCard, row, column);

					if (IsHorizontal)
					{
						column = firstCell != null ? firstCell.Column : taskCard.Cell.Column;
					}
					else
					{
						row = firstCell != null ? firstCell.Row : taskCard.Cell.Row;
					}

					var destinationCell = ViewModel.ComponentGrid[row, column];
					var handler = new TicketDragDropHandler(factory, ViewModel, taskCard.Cell, destinationCell, taskCard.CardContent, dialogWrapper, taskCard.ViewModel);

					if (handler.TryMoveToDestinationCell())
					{
						result = handler.WorkflowsAffected;
						taskCard.Dispose();
					}
				}
			}

			return result;
		}

		CellContent GetAppropriateCellToDragCardInto(TaskCardControl taskCard, int row, int col)
		{
			var hasMovedComponent = !ViewModel.AllShownComponentPKs.Contains(taskCard.CardContent.CapacityDto.ComponentPK);

			if (hasMovedComponent)
			{
				if (ViewModel.TimeProgressionMode == TimeProgressionModeList.Codes.Age)
				{
					return ViewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.Cards && c.TimeIndex == 0);
				}
				else
				{
					return ViewModel.ComponentGrid.Cells
						.Where(c => c.ContentType == CellContentType.Cards)
						.OrderByDescending(c => c.TimeIndex)
						.FirstOrDefault();
				}
			}
			else
			{
				return ViewModel.ComponentGrid[row, col];
			}
		}

		#endregion

		#region IBoardSectionControl Members

		string IBoardSectionControl.SectionType
		{
			get { return BMConstants.ComponentSectionType; }
		}

		void IBoardSectionControl.Refresh(BoardRefreshEventArgs args)
		{
			ApplyCurrentItemsFilterIfRequired(args);

			RefreshFilters();
			RefreshCore(args);
		}

#if DEBUG
		public
#endif
		void ResetRefreshedData()
		{
			RefreshedChannelHeaderControls = new HashSet<ChannelHeaderControl>();
			foreach (var control in GetChannelHeaderControls())
			{
				control.RefreshCompleted += ChannelHeader_RefreshCompleted;
			}

			refreshedBoardRefreshEventArgs = new List<BoardRefreshEventArgs>();
		}

		void ApplyCurrentItemsFilterIfRequired(BoardRefreshEventArgs args)
		{
			if (!IsShowingLoadError
				&& ViewModel.EnableShowCurrentItemsFilterByDefault
				&& (args.IsInitialLoad || args.ShouldReload))
			{
				ViewModel.FilterManager.ApplyFilter(new CurrentTaskFilter());
			}
		}

		internal int RefreshDelayCounter { get; private set; } = BMSRegistry.Instance.NumberOfRefreshesBeforeFullRefresh.Value;

		void RefreshCore(BoardRefreshEventArgs args)
		{
			if (args.IsInitialLoad || !args.TriggeredByRefreshTimer || RefreshDelayCounter-- < 1)
			{
				RefreshDelayCounter = BMSRegistry.Instance.NumberOfRefreshesBeforeFullRefresh.Value;
				args.IsDifferentialRefresh = false;
			}
			else
			{
				args.IsDifferentialRefresh = true;
			}

			SetupCards(args);
		}

		void RefreshFilters()
		{
			ViewModel.FilterManager.AppliedAndInheritedFilters.OfType<IStatefulBoardFilter>()
				.ForEach(f => f.Refresh());
		}

		bool IBoardSectionControl.AcceptDraggedControl(object control)
		{
			var taskCard = control as TaskCardControl;
			if (taskCard != null)
			{
				return TryReallocateTask(taskCard);
			}
			else
			{
				return false;
			}
		}

		void OnRefreshCompleted(BoardRefreshEventArgs args)
		{
			RefreshCompleted?.Invoke(this, args);
			isRefreshingSection = false;
		}

		public event EventHandler<BoardRefreshEventArgs> RefreshCompleted;

		bool IBoardSectionControl.SuppressBoardRefresh(BoardRefreshEventArgs args)
		{
			return isRefreshingSection && !args.IsForcedReload;
		}

		BoardSectionViewModel IBoardSectionControl.SectionViewModel => ViewModel;

		#endregion

		#region Refresh Execution Strategy

		void ComponentGrid_HeadingsRefreshed(object sender, ComponentGrid.RefreshHeadingsEventArgs e)
		{
			RefreshHeadings(e.ChannelViewModels);
		}

		#endregion

		#region IBoardSectionRefreshable Members

		void IBoardSectionRefreshable.NotifyComponentFinishRefreshed(BoardRefreshEventArgs refreshArgs)
		{
			NotifyComponentFinishRefreshed(refreshArgs);
		}

		void IBoardSectionRefreshable.NotifyRefreshFinished()
		{
			NotifyRefreshFinished();
		}

		void IBoardSectionRefreshable.ShowLoadingSectionOverlayControl(LoadingIndicatorEventArgs args)
		{
			ShowLoadingSectionOverlayControl(args);
		}

		void IBoardSectionRefreshable.ShowLoadingIndicator(bool isLoading, BoardRefreshEventArgs eventArgs)
		{
			var args = isLoading
				? LoadingIndicatorEventArgs.ForInitialLayoutRender()
				: LoadingIndicatorEventArgs.ForExistingLayoutRefresh(BoardRefreshType.Full, !eventArgs.TriggeredByRefreshTimer);
			ShowLoadingSectionOverlayControl(args);
		}

		bool IBoardSectionRefreshable.WrapDrawCardsWithLoadingCursor(Func<bool> onRefreshComponentCore, bool isLoading, BoardRefreshEventArgs eventArgs)
		{
			try
			{
				Cursor = Cursors.WaitCursor;

				try
				{
					if (eventArgs.TriggeredByRefreshTimer && !eventArgs.IsReloading || eventArgs.TriggeredBySlideshowTimer && !eventArgs.IsInitialLoad)
					{
						((IBoardSectionRefreshable)this).ShowLoadingIndicator(isLoading, eventArgs);
					}

					this.SuspendDrawing();
					return onRefreshComponentCore();
				}
				finally
				{
					this.ResumeDrawing();
					NotifyComponentFinishRefreshed(eventArgs);
					NotifyRefreshFinished();
				}
			}
			catch (ObjectDisposedException)
			{
				// The form is probably already closing - nothing to do.
				return false;
			}
			finally
			{
				if (!IsDisposed)
				{
					Cursor = Cursors.Arrow;
				}
			}
		}

		void IBoardSectionRefreshable.DisplayAcceptabilityBandResults(IEnumerable<BoardSectionAcceptabilityBandResult> results)
		{
			if (AcceptabilityBandTiles != null)
			{
				var tileResults = (results).Where(x => x.SectionBand.ShouldShowAsTile).ToArray();
				AcceptabilityBandTiles.PopulateTiles(tileResults);
			}
			var headingResults = results.Where(x => x.SectionBand.ShouldShowInHeading).ToArray();
			ViewModel.RefreshAcceptabilityBandSubHeading(headingResults);
		}

		#endregion

		#region For Test

		public bool IsHeaderExpanded_ForTest(CellContent cell)
		{
			return ExpandedHeaders.Contains(cell);
		}

		#endregion
	}
}
