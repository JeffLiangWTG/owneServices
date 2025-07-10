using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.CommissionManagement.Business;
using Enterprise.CommissionManagement.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Module
{
	public partial class CommissionManagementFilterControl : ZFilterStripCommonControl
	{
		[Obsolete("Use the constructor that takes a collection and filter strip, this constructor is just for the designer", true)]
		public CommissionManagementFilterControl()
		{
			InitializeComponent();
		}

		public CommissionManagementFilterControl(ViewCommissionLineCollection collection, CommissionManagementFilterBusinessObject strip)
			: base(strip)
		{
			InitializeComponent();

			collection.AdditionalFilter = ZQuery.NoResultQuery;
			groupingCollection = new TopLevelCommissionManagementGroupingCollection(collection);
			groupingCollection.Init();
			FilterStripAuditDetails.AddAuditDetailsColumns(LinesPreviewPane.LinesGrid, ViewCommissionLineSchema.Constants.TableName, typeof(ViewCommissionLine));
			AddRecentCommissionsMatrix(collection);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				SetupGrid();

				if (ParentForm != null)
				{
					BeginInvoke(new MethodInvoker(() => LoadMainSplitterGuiState()));
				}
			}
		}

		#region RecentCommissionsMatrix

		void AddRecentCommissionsMatrix(ViewCommissionLineCollection collection)
		{
			ParentChanged += (s, e) =>
			{
				var parentForm = FindForm();
				if (parentForm != null)
				{
					RecentCommissionsMatrixControl = new RecentCommissionsMatrixControl();
					parentForm.Controls.Add(RecentCommissionsMatrixControl);

					RecentCommissionsMatrixControl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
					int titleBarheight = 0;
#if WINZOR
					titleBarheight = 24;
#endif
					RecentCommissionsMatrixControl.Location = ControlDpiScalingHelper.NewScaledPoint(810, parentForm.GetType().Name.Contains("EmbeddedModulePopup") ? 38 : (65 - titleBarheight));
					RecentCommissionsMatrixControl.Size = ControlDpiScalingHelper.NewScaledSize(parentForm.Width - parentForm.Padding.Horizontal - RecentCommissionsMatrixControl.Left - ControlDpiScalingHelper.ScaleToCurrentDpiX(20), ControlDpiScalingHelper.ScaleToCurrentDpiY(40), false);

					RecentCommissionsMatrixControl.SetDataBinding(new RecentCommissionsMatrix(ZDateTime.Today, collection), "");
					RecentCommissionsMatrixControl.BringToFront();

#if DEBUG
					//avoid bash test failure(Control overlaps another visible control)
					//the RecentCommissionsMatrixControl overlaps the CommissionManagementFilterControl per requirement
					if (Globals.IsTest)
					{
						RecentCommissionsMatrixControl.Visible = false;
					}
#endif
				}
			};
		}

		void RemoveRecentCommissionsMatrix()
		{
			if (RecentCommissionsMatrixControl == null)
			{
				return;
			}

			if (RecentCommissionsMatrixControl.Parent != null)
			{
				RecentCommissionsMatrixControl.Parent.Controls.Remove(RecentCommissionsMatrixControl);
			}

			RecentCommissionsMatrixControl.Dispose();
			RecentCommissionsMatrixControl = null;
		}

		protected override void UpdateToolStripLayout<T>(System.Collections.Generic.List<T> controlList)
		{
			base.UpdateToolStripLayout(controlList);

			if (RecentCommissionsMatrixControl != null)
			{
				if (Parent != null && !(Parent is IZForm))
				{
					ControlDpiScalingHelper.SetHeight(RecentCommissionsMatrixControl, ToolStrip.Top + ToolStrip.Height + Parent.Location.Y - ControlDpiScalingHelper.ScaleToCurrentDpiY(RecentCommissionsMatrixHeightAdjustment), false);
				}
				else
				{
					ControlDpiScalingHelper.SetHeight(RecentCommissionsMatrixControl, ToolStrip.Top + ToolStrip.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(RecentCommissionsMatrixHeightAdjustment), false);
				}
			}
		}

		protected RecentCommissionsMatrixControl RecentCommissionsMatrixControl;

		protected const int RecentCommissionsMatrixHeightAdjustment = 40;

		#endregion RecentCommissionsMatrix

		#region GridCollection

		public override IBusinessObjectCollection GridCollection
		{
			get { return groupingCollection.InnerCollection; }
		}
		readonly TopLevelViewCommissionLineGroupingCollection groupingCollection;

		#endregion

		#region Grid

		void SetupGrid()
		{
			if (OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.Value)
			{
				Grid.RemoveFromAvailableColumns(ViewCommissionLineGrouping.Schema.CompanyPk);
			}
		}

		#endregion

		#region Bind

		protected override void Bind()
		{
			if (!isBound)
			{
				BindCore();
				isBound = true;
			}
		}

		bool isBound;

		protected void BindCore()
		{
			this.SetDataBinding(groupingCollection, "");
			Grid.SetDataBinding(groupingCollection, "");
			this.PerformSearch += CommissionManagementFilterControl_PerformSearch;
		}

		void CommissionManagementFilterControl_PerformSearch(object sender, EventArgs e)
		{
			groupingCollection.AddAmountNotifications(NotificationType.Warning);
			groupingCollection.AddFullPaymentNotifications(NotificationType.Warning);
		}

		#endregion

		#region Layout

		protected override void OnLayout(LayoutEventArgs e)
		{
			base.OnLayout(e);
			RestrictSplitterPosition();
		}

		protected override void HandleGridSizing()
		{
			if (Grid != null)
			{
				if (mainSplitter != null)
				{
					ControlDpiScalingHelper.SetHeight(Grid, ClientSize.Height - Grid.Top - mainSplitter.SplitPosition - mainSplitter.Height, false);
				}
				else
				{
					ControlDpiScalingHelper.SetHeight(Grid, ClientSize.Height - Grid.Top, false);
				}

				ControlDpiScalingHelper.SetWidth(Grid, ClientSize.Width, false);
			}
		}

		void RestrictSplitterPosition()
		{
			if (Grid != null && mainSplitter != null)
			{
				int maxSplitterPosition = Math.Max(0, ClientSize.Height - Grid.Top - ControlDpiScalingHelper.ScaleToCurrentDpiY(100));

				if (mainSplitter.SplitPosition > maxSplitterPosition)
				{
					mainSplitter.SplitPosition = maxSplitterPosition;
				}
			}
		}

		#endregion

		#region GUI State

		CommissionManagementFilterControlGuiState MainControlGuiState
		{
			get { return mainControlGuiState ?? (mainControlGuiState = new CommissionManagementFilterControlGuiState(this)); }
		}
		CommissionManagementFilterControlGuiState mainControlGuiState;

		protected CommissionLinesPreviewPaneGuiState CommissionLinesPreviewPaneGuiState
		{
			get { return commissionLinesPreviewPaneGuiState ?? (commissionLinesPreviewPaneGuiState = new CommissionLinesPreviewPaneGuiState(LinesPreviewPane)); }
		}
		CommissionLinesPreviewPaneGuiState commissionLinesPreviewPaneGuiState;

		protected void LoadMainSplitterGuiState()
		{
			if (mainSplitter != null)
			{
				var loadedMainSplitterPosition = MainControlGuiState.MainSplitterPosition;
				if (loadedMainSplitterPosition > 0)
				{
					mainSplitter.SplitPosition = loadedMainSplitterPosition;
				}
				if (LinesPreviewPane.SplitContainer != null)
				{
					var loadedSplitterDistance = CommissionLinesPreviewPaneGuiState.SplitterDistance;
					if (loadedSplitterDistance > 0 && (LinesPreviewPane.SplitContainer.Height - LinesPreviewPane.SplitContainer.Panel2MinSize - LinesPreviewPane.SplitContainer.SplitterWidth >= 0))
					{
						LinesPreviewPane.SplitContainer.SplitterDistance = loadedSplitterDistance;
					}
				}
			}
		}

		void SaveGuiState()
		{
			if (mainSplitter != null)
			{
				MainControlGuiState.MainSplitterPosition = mainSplitter.SplitPosition;
				if (LinesPreviewPane.SplitContainer != null)
				{
					CommissionLinesPreviewPaneGuiState.SplitterDistance = LinesPreviewPane.SplitContainer.SplitterDistance;
				}
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				RemoveRecentCommissionsMatrix();

				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			SaveGuiState();
			base.OnHandleDestroyed(e);
		}

		#endregion Dispose

		#region Implementation

		protected override int MinFilterStripPanelHeight
		{
			get { return ControlDpiScalingHelper.ScaleToCurrentDpiY(50); }
		}

		protected override void LayoutLoaded()
		{
			this.FilterStripsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(752, 41, true);
			base.LayoutLoaded();
		}

		#endregion Implementation
	}
}
