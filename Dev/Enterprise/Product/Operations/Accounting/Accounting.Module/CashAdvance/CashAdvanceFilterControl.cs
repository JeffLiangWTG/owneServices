using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class CashAdvanceFilterControl : ZFilterStripControl
	{
		protected override ZFilterStrip NewZFilterStrip()
		{
			return new ZFilterStrip();
		}

		public CashAdvanceFilterControl(IBusinessObjectCollection gridCollection, CashAdvanceFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning && !(filterBusinessObject is ARCashAdvanceFilterBusinessObject))
			{
				grid.RemoveFromAvailableColumns(nameof(AccCashAdvanceRequestHeader.CAH_Printed));
			}
		}

		protected override void BindCore()
		{
			base.BindCore();
			UpdateCashAdvanceRequestLineGrid();
		}

		CashAdvanceFilterBusinessObject CashAdvanceBusinessObject
		{
			get { return FilterBusinessObject as CashAdvanceFilterBusinessObject; }
		}

		public AccCashAdvanceRequestHeaderCollection CashAdvanceRequest
		{
			get { return GridCollection as AccCashAdvanceRequestHeaderCollection; }
		}

		void FilteredGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			ResetLinesForSelectedCashAdvanceRequest();
		}

		void FilteredGrid_AfterBind(object sender, EventArgs e)
		{
			((AccCashAdvanceRequestHeaderCollection)FilteredGrid.DataSource).CountChanged += CashAdvanceFilterControl_CountChanged;
		}

		void CashAdvanceFilterControl_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (CashAdvanceRequest.Count > 0)
			{
				ResetLinesForSelectedCashAdvanceRequest();
			}
			else if (!IsDisposed)
			{
				// clear the bottom grid if top grid has nothing
				UpdateCashAdvanceRequestLineGrid();
			}
		}

		void ResetLinesForSelectedCashAdvanceRequest()
		{
			if (IsCurrentSelectionValid && CashAdvanceRequest.Count > 0 && FilteredGrid.CurrentRowIndex < FilteredGrid.List.Count)
			{
				var selectedCashAdvanceRequest = CashAdvanceRequest[FilteredGrid.CurrentRowIndex];
				CashAdvanceRequestLineDisplayGrid.SetDataBinding(selectedCashAdvanceRequest.Lines, "");
				CashAdvanceRequestLineDisplayGrid.Refresh();
			}
		}

		ZBool IsCurrentSelectionValid => FilteredGrid.CurrentRowIndex >= 0;

		public override void Find(bool isManualSearch = true)
		{
			//Clear bottom grid when Find button is clicked
			UpdateCashAdvanceRequestLineGrid();

			base.Find(isManualSearch);

			//If there are any requests, then select first request and reset bottom grid
			if (CashAdvanceRequest.Count > 0)
			{
				FilteredGrid.CurrentRowIndex = 0;
				ResetLinesForSelectedCashAdvanceRequest();
			}
		}

		void UpdateCashAdvanceRequestLineGrid()
		{
			CashAdvanceRequestLineDisplayGrid.SetDataBinding(CashAdvanceBusinessObject.CashAdvanceLines, "");
		}

		protected override void HandleGridSizing()
		{
			if (FilteredGrid != null)
			{
				FilteredGrid.Location = ControlDpiScalingHelper.NewScaledPoint(0, FilteredGrid.Top, false);

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

		internal void GridSplitter_SplitterMoved(object sender, System.Windows.Forms.SplitterEventArgs e)
		{
			if (FilteredGrid != null)
			{
				int maxSplitterPosition = Math.Max(0, ClientSize.Height - FilteredGrid.Top - ControlDpiScalingHelper.ScaleToCurrentDpiY(100));

				var splitter = (KSplitter)sender;

				if (splitter.SplitPosition > maxSplitterPosition)
				{
					splitter.SplitPosition = maxSplitterPosition;
				}
			}
		}
	}
}
