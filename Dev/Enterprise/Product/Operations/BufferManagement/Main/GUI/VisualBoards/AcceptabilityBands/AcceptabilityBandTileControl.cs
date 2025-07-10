using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI
{
	public sealed partial class AcceptabilityBandTileControl : ZPanel
	{
		public AcceptabilityBandTileControl(AcceptabilityBandTileViewModel viewModel)
		{
			InitializeComponent();

			ViewModel = Argument.NotNull(viewModel, "viewModel");

			ContextMenuStrip = new ContextMenuStrip(components);
			ContextMenuStrip.Items.Add(new ZToolStripMenuItem(EditLabel, (_, __) => EditAcceptabilityBand_Click()));
			ContextMenuStrip.Items.Add(new ZToolStripMenuItem(ShowStatusCalculationDetailsLabel, (_, __) => ShowCalculationDetails_Click()));
			ContextMenuStrip.Items.Add(new ZToolStripMenuItem(ShowAllWorkflowsLabel, (_, __) => ShowAll_Click()));

			var showMatchingItemsMenuItem = new ZToolStripMenuItem(ShowMatchingItemsLabel, ShowMatchingItems_Click);
			ContextMenuStrip.Items.Add(showMatchingItemsMenuItem);

			var eventHandler = new MouseEventHandler((s, e) =>
			{
				if (e.Button == MouseButtons.Left)
				{
					ToggleShowMatchingItems(showMatchingItemsMenuItem);
				}
			});

			MouseClick += eventHandler;
			NameAndResultLabel.MouseClick += eventHandler;
			UpdateDetails();
			var filter = GetActiveFilter();

			if (filter != null)
			{
				UpdateAppliedVisualState(showMatchingItemsMenuItem);
				filter.RestoreVisualStateAfterFilterRemovedAction = () => UpdateAppliedVisualState(showMatchingItemsMenuItem);
			}
		}

		internal static string EditLabel => Res.GetString("166e849a-c3ed-449c-b54e-014e4c931ea3", "Edit");
		internal static string ShowStatusCalculationDetailsLabel => Res.GetString("35060bf4-9835-4acd-89a6-bb2a30e8f5f4", "Show AB status Calculation Details");
		internal static string ShowAllWorkflowsLabel => Res.GetString("0639f3ac-656b-4ea9-9c90-ca616db38611", "Show all Workflows matching AB value");
		internal static string ShowMatchingItemsLabel => Res.GetString("d5c217a7-0ac5-43a0-b01f-1c350646e7fb", "Show only cards matching AB value");

		public AcceptabilityBandTileViewModel ViewModel { get; }

		void UpdateDetails()
		{
			var text = new StringBuilder();
			if (!string.IsNullOrEmpty(ViewModel.DisplayName))
			{
				text.Append(ViewModel.DisplayName);
				text.Append(": ");
			}

			if (ViewModel.Status == ComponentAcceptabilityStatus.Timeout)
			{
				text.Append((NoResString)"⏳");
			}
			else
			{
				text.Append(ViewModel.Result);
			}

			NameAndResultLabel.Text = text.ToString();

			NameAndResultLabel.Font = new Font(NameAndResultLabel.Font.FontFamily, 11f);

			BackColor = ViewModel.Status.GetBackgroundColor();
			ForeColor = ViewModel.Status.GetForegroundColor();

			ControlDpiScalingHelper.SetWidth(this, NameAndResultLabel.Width + Margin.Left + Margin.Right, false);
		}

		#region Control Overrides

		protected override void Dispose(bool isNotFinalizing)
		{
			base.Dispose(isNotFinalizing);
			if (isNotFinalizing)
			{
				components.Dispose();
			}
		}

		#endregion

		#region Event Handlers

		void EditAcceptabilityBand_Click()
		{
			MainThreadRunner.RunOnMainThread(() =>
			{
				var factory = new BusinessObjectFactory { NameForDebugging = GetType().Name + ": EditAcceptabilityBand_Click" };
				var band = factory.Load<BMComponentAcceptabilityBand>(ViewModel.AcceptabilityBandPK);

				if (band != null)
				{
					ZControllerFactory.Create(ControllerIDs.AcceptabilityBand).ShowEditForm(band);
				}
			});
		}

		void ShowMatchingItems_Click(object sender, EventArgs e) => ToggleShowMatchingItems((ZToolStripMenuItem)sender);

		void ShowCalculationDetails_Click()
		{
			var text = new StringBuilder(ViewModel.StatusText);

			text.AppendLine();

			if (ViewModel.Status == ComponentAcceptabilityStatus.Timeout)
			{
				text.AppendLine(Res.GetString("25058052-9d42-4013-a794-ed520ceb35f1", "Calculation timeout in {0} seconds", ViewModel.CalculationTimeInSeconds));
				text.AppendLine(Res.GetString("CF29332A-421E-46BF-99C5-9BB3EB41ECFD", "Accurate as of {0}", ViewModel.ResultToDisplayCalculationTimeLocal));
				Globals.Message.Show(text.ToString(), ViewModel.BandName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			text.AppendLine();

			text.AppendLine(Res.GetString("c5d6ecbf-ef1e-4e6d-ba5c-5c1024ebee99", @"Lowest 'Caution' value: {0}", ViewModel.BoundaryValues.CautionMin));
			text.AppendLine(Res.GetString("7486252a-1fa9-4796-8f5f-ac73f348c676", @"Lowest 'Good' value: {0}", ViewModel.BoundaryValues.GoodMin));
			text.AppendLine(Res.GetString("cefe0986-bead-42eb-a563-aacfd03a5897", @"Lowest 'Excellent' value: {0}", ViewModel.BoundaryValues.ExcellentMin));
			text.AppendLine(Res.GetString("a066cda9-8b32-4160-8d2d-c9b7951737dd", @"Highest 'Excellent' value: {0}", ViewModel.BoundaryValues.ExcellentMax));
			text.AppendLine(Res.GetString("ebdee98f-e0fc-40a0-a3b5-087477e8e332", @"Highest 'Good' value: {0}", ViewModel.BoundaryValues.GoodMax));
			text.AppendLine(Res.GetString("ea3823d5-18bb-4483-92de-0aba1bd5d2a3", @"Highest 'Caution' value: {0}", ViewModel.BoundaryValues.CautionMax));

			text.AppendLine();

			if (ViewModel.IsResultPending)
			{
				text.AppendLine(Res.GetString("dd28e90a-ce1d-4ab7-baee-03a5fd142092", "Calculating..."));
			}
			else
			{
				text.AppendLine(Res.GetString("6BD8FF67-D7C8-4ABD-ACA8-6D76E5DE2E9A", "Calculated in {0} seconds", ViewModel.CalculationTimeInSeconds));
				text.AppendLine(Res.GetString("CF29332A-421E-46BF-99C5-9BB3EB41ECFD", "Accurate as of {0}", ViewModel.ResultToDisplayCalculationTimeLocal));
			}

			Globals.Message.Show(text.ToString(), ViewModel.BandName, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		void ShowAll_Click()
		{
			try
			{
				ShowAllWorkflows();
			}
			catch (NullReferenceException)
			{
				Globals.Message.Show(Res.GetString("f266ac53-1a4d-4c49-a227-c07962912754", "Unable to display results since an error has occurred. If the problem persists, talk to your system administrator. Consider simplifying filters defined in Acceptability Band configuration."));
			}
		}

		void ShowAllWorkflows()
		{
			OnShowAllMatches?.Invoke(this, EventArgs.Empty);

			var factory = new ReadOnlyBusinessObjectFactory { NameForDebugging = GetType().Name + ": ShowAllWorkflowsMatchingCriteria" };
			var band = factory.Load<BMComponentAcceptabilityBand>(ViewModel.AcceptabilityBandPK);

			if (band == null)
			{
				return;
			}
			if (!band.UseFilterStripsExclusively)
			{
				var message = Res.GetString("016a869b-56af-41e1-8c2f-97a13029d8c3", "Only Acceptability Bands based on filter strips can be used to display Job Workflows.");
				Globals.Message.Show(message);
				return;
			}
			if (band.FilterRule != null)
			{
				band.FilterRule.S9_IsPublished = true;
			}

			var parameters = new AcceptabilityBandSqlBuilderParameters(band)
			{
				ReleaseGroupPK = ViewModel.ReleaseGroupPK,
				ShouldFilterBySection = band.GetOverriddenShouldFilterBySection(ViewModel.ShouldFilterBySection),
				ShouldFilterByReleaseGroup = band.GetOverriddenShouldFilterByReleaseGroup(ViewModel.ShouldFilterByReleaseGroup),
			};

			if (parameters.ShouldFilterBySection)
			{
				parameters.WorkflowPKs = ViewModel.SectionViewModel.GetCachedWorkflowPKsForABs(factory) ?? BoardAcceptabilityBandCalculator.GetWorkflowPKs(ViewModel.SectionViewModel.SectionPK, factory);
			}

			var bandWorkflowPKs = WorkflowFactory.GetMatchingWorkflows(band, parameters).Result.Keys.ToHashSet();
			var query = new ZQuery(ProcessHeaderSchema.PK, bandWorkflowPKs);

			using var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.ProcessHeader);

			module.AddAdditionalDisplayFilter = (ZQuery additionalQuery) => additionalQuery.AddToFilter(query);
			module.FilterBusinessObject.IsInFilterRuleMode = true;

			var filterControl = (ZFilterStripCommonControl)module.EmbeddedControl;

			filterControl.IsFilterReadonly = true;
			filterControl.IsFilterVisible = false;
			filterControl.RunSearchOnEnteringAModuleOverride = true;
			filterControl.CaptionRenderingEnabled = false;

			var popup = new EmbeddedModulePopup(module, band.FilterRule, true) { IsSelectionMandatory = false };
			ZFormModaliser.ShowDialogAndDispose(popup, null);
		}

		public event EventHandler OnShowAllMatches;

		#endregion

		#region Toggle Show Matching Items

		void ToggleShowMatchingItems(ZToolStripMenuItem menuItem)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = GetType().Name + ": ToggleShowMatchingItems" };
			var band = factory.Load<BMComponentAcceptabilityBand>(ViewModel.AcceptabilityBandPK);

			if (band == null)
			{
				return;
			}

			if (!band.UseFilterStripsExclusively)
			{
				var message = Res.GetString("2e1e38a9-1b99-4f3b-b9f6-1d8d951c4bd5", "Only Acceptability Bands based on filter strips can be used to Show matching cards.");
				Globals.Message.Show(message);
				return;
			}

			var workflowPKs = ViewModel.SectionViewModel.SectionWorkflowPKsWithTaskAndSectionFiltersApplied;
			var filter = new AcceptabilityBandMatchesVisibilityFilter(band, ViewModel.BoardSectionAcceptabilityBandPK, new AcceptabilityBandSqlBuilderParameters(band, ViewModel.ShouldFilterByReleaseGroup, shouldFilterBySection: AcceptabilityBandVisualizationOption.Yes) { ReleaseGroupPK = ViewModel.ReleaseGroupPK, WorkflowPKs = workflowPKs })
			{
				RestoreVisualStateAfterFilterRemovedAction = () => UpdateAppliedVisualState(menuItem),
			};

			ViewModel.SectionViewModel.FilterManager.ToggleFilter(filter);

			if (filter.DidGetCellVisibilityApplicatorCauseErrorOnBackgroundThread)
			{
				ViewModel.SectionViewModel.FilterManager.RemoveFilter(filter);
				UpdateAppliedVisualState(menuItem);
				Globals.Message.Show(Res.GetString("2c0d7b89-811c-4122-a8a1-6a83e5b7a12e", "An error occurred during the application of this filter. It's possible that the query timed out or the database connection was interrupted."));
			}
			else
			{
				UpdateAppliedVisualState(menuItem);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Styling")]
		void UpdateAppliedVisualState(ZToolStripMenuItem menuItem)
		{
#if WINZOR
			if (IsShowingMatchingItems)
			{
				ExtraStyleString = "border-top: 3px #c00 double;border-left: 3px #c00 double;border-bottom:3px black solid;border-right:3px black solid;outline: 1px black solid;outline-offset: -1px;";
				BorderStyle = BorderStyle.None;
				NameAndResultLabel.ExtraStyleString = "top: 1px; left: 1px;";
			}
			else
			{
				ExtraStyleString = string.Empty;
				BorderStyle = BorderStyle.FixedSingle;
				NameAndResultLabel.ExtraStyleString = string.Empty;
			}
#endif
			menuItem.Checked = IsShowingMatchingItems;
			Invalidate();
		}

		AcceptabilityBandMatchesVisibilityFilter GetActiveFilter()
		{
			return ViewModel.SectionViewModel.FilterManager.AppliedFilters.OfType<AcceptabilityBandMatchesVisibilityFilter>().SingleOrDefault(x => x.BoardSectionAcceptabilityBandPK == ViewModel.BoardSectionAcceptabilityBandPK);
		}

		public bool IsShowingMatchingItems => GetActiveFilter() != null;

		#endregion

		#region Paint

#if !WINZOR

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (IsShowingMatchingItems)
			{
				ControlPaint.DrawBorder(e.Graphics, ClientRectangle, Color.Crimson, ButtonBorderStyle.Outset);
			}
		}

#endif

		#endregion
	}
}
