using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Module
{
	public partial class SumARegisterFilterStripControl : ZFilterStripControl
	{
		public SumARegisterFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			SetLinesGridDataBinding();

			ToolStripFindDropButton.Click += OnToolStripFindDropButtonOnClick;

			HeaderAndLinesGridSplitContainer.AllowOverlap(ToolStrip);
			HeaderAndLinesGridSplitContainer.AllowOverlap(ToolStripHelp);
		}

		protected override void OnCreateControl()
		{
			base.OnCreateControl();

			grid.ListManager.CurrentChanged += OnListManagerOnCurrentChanged;
		}

		protected override void OnSearchPerformed(bool showError, bool didSearch, bool isManualSearch, Form form)
		{
			base.OnSearchPerformed(showError, didSearch, isManualSearch, form);

			ClearCurrentLinesQuery();
			OnListManagerOnCurrentChanged(grid.ListManager, EventArgs.Empty);
		}

		protected override void HandleGridSizing()
		{
			if (HeaderAndLinesGridSplitContainer != null && (HeaderAndLinesGridSplitContainer.Left != 0 || HeaderAndLinesGridSplitContainer.Right != ClientRectangle.Right || HeaderAndLinesGridSplitContainer.Bottom != ClientRectangle.Bottom))
			{
				HeaderAndLinesGridSplitContainer.Location = ControlDpiScalingHelper.NewScaledPoint(0, HeaderAndLinesGridSplitContainer.Top, false);
				ControlDpiScalingHelper.SetHeight(HeaderAndLinesGridSplitContainer, ClientSize.Height - HeaderAndLinesGridSplitContainer.Top, false);
				ControlDpiScalingHelper.SetWidth(HeaderAndLinesGridSplitContainer, ClientSize.Width, false);
			}
		}

		protected override void UpdateFilteredGridRefreshWarning()
		{
			base.UpdateFilteredGridRefreshWarning();

			UpdateSplitContainerOnFilterStripsChanged();
		}

		void ClearCurrentLinesQuery()
		{
			var currentItem = grid.ListManager.GetCurrent() as CusTempStorageRegHeader;
			if (currentItem != null)
			{
				currentItem.CusTempStorageRegLines.AdditionalFilter = new ZDBOnlyQuery(typeof(CusTempStorageRegLine));
			}
		}

		void OnListManagerOnCurrentChanged(object sender, EventArgs args)
		{
			var filter = FilterBusinessObject as SumARegisterFilterBusinessObject;

			var listManager = sender as CurrencyManager;
			var selectedHeader = listManager.GetCurrent() as CusTempStorageRegHeader;

			if (selectedHeader != null && filter != null)
			{
				selectedHeader.CusTempStorageRegLines.AdditionalFilter = filter.LineOnlyQuery;
			}
		}

		void OnToolStripFindDropButtonOnClick(object sender, EventArgs args)
		{
			var filterBizO = (SumARegisterFilterBusinessObject)FilterBusinessObject;
			filterBizO.ResetLineOnlyQuery();
		}

		void SetLinesGridDataBinding()
		{
			LinesGrid.SetDataBinding(GridCollection, nameof(CusTempStorageRegHeader.CusTempStorageRegLines));
		}

		void UpdateSplitContainerOnFilterStripsChanged()
		{
			if (HeaderAndLinesGridSplitContainer != null)
			{
				ControlDpiScalingHelper.SetTop(HeaderAndLinesGridSplitContainer, AddStripButton.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(4), false);

				if (AutoRefreshWarningLabel.Visible)
				{
					if (IsFilterVisible)
					{
						ControlDpiScalingHelper.SetTop(HeaderAndLinesGridSplitContainer, ControlForLayout.Top + AutoRefreshWarningLabel.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(2), false);
					}
					else
					{
						ControlDpiScalingHelper.SetTop(HeaderAndLinesGridSplitContainer, 0, false);
						ControlDpiScalingHelper.SetTop(HeaderAndLinesGridSplitContainer, AutoRefreshWarningLabel.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(2), false);
					}
				}
			}
		}
	}
}

