using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.Base.Unmatching;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class MatchingFilterControl : ZFilterStripControl
	{
		protected enum FilteredGridColumnLayoutContext
		{
			AP, AR
		}

		public MatchingFilterControl()
		{
			InitialiseForm();
		}

		public MatchingFilterControl(IBusinessObjectCollection matchLinks, FilterStripBusinessObject filterBizO)
			: base(matchLinks, filterBizO)
		{
			InitialiseForm();
		}

		protected void SetContext(FilteredGridColumnLayoutContext context)
		{
			FilteredGrid.ColumnLayoutContext = context.ToString();
			MatchTransactionsDisplayGrid.GridId = "efce3ae4-f317-4f14-b3f8-aecd261c723a|" + context.ToString(); // To have different selected color filter in AP and AR modules
		}

		protected void InitialiseForm()
		{
			SuspendLayout();
			try
			{
				InitializeComponent();
				BindingSource.DataSource = FilterBusinessObject;
			}
			finally
			{
				ResumeLayout();
			}
		}

		internal ZBool IsCurrentSelectionValid
		{
			get
			{
				return FilteredGrid.CurrentRowIndex >= 0 && FilteredGrid.CurrentRowIndex < FilteredGrid.List.Count;
			}
		}

		void FilteredGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			ResetTransactionHeadersForSelectedMatchGroup();
		}

		internal UnmatchingRow fPreviousMatchGroup;
		internal void ResetTransactionHeadersForSelectedMatchGroup()
		{
			FilteredGrid.ListManager.EndCurrentEdit();
			if (IsCurrentSelectionValid &&
				(fPreviousMatchGroup == null || fPreviousMatchGroup != ((UnmatchingRowCollection)FilteredGrid.DataSource)[FilteredGrid.CurrentRowIndex]))
			{
				fPreviousMatchGroup = ((UnmatchingRowCollection)FilteredGrid.DataSource)[FilteredGrid.CurrentRowIndex];
				((MatchingBaseFilterBusinessObject)FilterBusinessObject).SetCurrentTransactionsForMatchGroup(fPreviousMatchGroup);
			}
		}

		void FilteredGrid_AfterBind(object sender, EventArgs e)
		{
			((UnmatchingRowCollection)FilteredGrid.DataSource).AfterLoaded += new EventHandler(MatchingFilterControl_AfterLoaded);
		}

		void MatchingFilterControl_AfterLoaded(object sender, EventArgs e)
		{
			fPreviousMatchGroup = null;
			ResetTransactionHeadersForSelectedMatchGroup();
		}

		protected override void HandleGridSizing()
		{
			if (FilteredGrid != null)
			{
				FilteredGrid.Location = ControlDpiScalingHelper.NewScaledPoint(0, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(FilteredGrid.Top));

				if (GridSplitter != null)
				{
					ControlDpiScalingHelper.SetHeight(FilteredGrid, ClientSize.Height - FilteredGrid.Top - GridSplitter.SplitPosition - ControlDpiScalingHelper.ScaleToCurrentDpiY(10), false);
				}
				else
				{
					ControlDpiScalingHelper.SetHeight(FilteredGrid, ClientSize.Height - FilteredGrid.Top, false);
				}

				ControlDpiScalingHelper.SetWidth(FilteredGrid, ClientSize.Width, false);
			}
		}

		void GridSplitter_SplitterMoved(object sender, System.Windows.Forms.SplitterEventArgs e)
		{
			if (FilteredGrid != null)
			{
				int maxSplitterPosition = Math.Max(((KSplitter)sender).MinSize, ClientSize.Height - FilteredGrid.Top - ControlDpiScalingHelper.ScaleToCurrentDpiY(100));

				if (((KSplitter)sender).SplitPosition > maxSplitterPosition)
				{
					((KSplitter)sender).SplitPosition = maxSplitterPosition;
				}
			}
		}

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (FilteredGrid.DataSource != null)
			{
				((UnmatchingRowCollection)FilteredGrid.DataSource).AfterLoaded -= MatchingFilterControl_AfterLoaded;
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}

