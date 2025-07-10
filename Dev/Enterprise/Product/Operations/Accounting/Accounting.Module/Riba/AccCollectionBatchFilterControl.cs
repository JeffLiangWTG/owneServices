using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.Riba;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class AccCollectionBatchFilterControl : ZFilterStripControl
	{
		protected override ZFilterStrip NewZFilterStrip()
		{
			return new ZFilterStrip();
		}

		public AccCollectionBatchFilterControl(IBusinessObjectCollection gridCollection, AccCollectionBatchFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override void BindCore()
		{
			base.BindCore();
			CollectionOrdersDisplayGrid.SetDataBinding(CollectionBatchFilterBusinessObject.BatchCollectionOrders, "");
		}

		AccCollectionBatchFilterBusinessObject CollectionBatchFilterBusinessObject
		{
			get { return FilterBusinessObject as AccCollectionBatchFilterBusinessObject; }
		}

		public AccCollectionBatchCollection BatchCollection
		{
			get { return GridCollection as AccCollectionBatchCollection; }
		}

		void FilteredGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			ResetOrdersForSelectedBatch();
		}

		void FilteredGrid_AfterBind(object sender, EventArgs e)
		{
			((AccCollectionBatchCollection)FilteredGrid.DataSource).CountChanged += new EventHandler(FilteredGrid_CountChanged);
		}

		void FilteredGrid_CountChanged(object sender, EventArgs e)
		{
			if (BatchCollection.Count > 0)
			{
				ResetOrdersForSelectedBatch();
			}
			else if (!IsDisposed)
			{
				// clear the bottom grid if top grid has nothing
				{
					CollectionOrdersDisplayGrid.SetDataBinding(CollectionBatchFilterBusinessObject.BatchCollectionOrders, "");
				}
			}
		}

		void ResetOrdersForSelectedBatch()
		{
			if (IsCurrentSelectionValid && BatchCollection.Count > 0)
			{
				var selectedBatch = BatchCollection[FilteredGrid.CurrentRowIndex];
				CollectionOrdersDisplayGrid.SetDataBinding(selectedBatch.CollectionOrders, "");
				CollectionOrdersDisplayGrid.Refresh();
			}
		}

		ZBool IsCurrentSelectionValid
		{
			get
			{
				return FilteredGrid.CurrentRowIndex >= 0;
			}
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
				var maxSplitterPosition = Math.Max(0, ClientSize.Height - FilteredGrid.Top - ControlDpiScalingHelper.ScaleToCurrentDpiY(100));

				var splitter = (KSplitter)sender;

				if (splitter.SplitPosition > maxSplitterPosition)
				{
					splitter.SplitPosition = maxSplitterPosition;
				}
			}
		}
	}
}

