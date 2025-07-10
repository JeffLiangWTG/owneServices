using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Accounting.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class AlternateChartofAccountsFilterControl : ZFilterStripControl
	{
		public AlternateChartofAccountsFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitialiseForm();
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new AlternateGLAccountFilterStrip();
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

		public AlternateChartofAccountsFilterControl()
		{
			InitialiseForm();
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

		void Splitter_SplitterMoved(object sender, SplitterEventArgs e)
		{
			if (FilteredGrid != null)
			{
				var innerSender = ((KSplitter)sender);
				int maxSplitterPosition = Math.Max(innerSender.MinSize, ClientSize.Height - FilteredGrid.Top - ControlDpiScalingHelper.ScaleToCurrentDpiY(100));

				if (innerSender.SplitPosition > maxSplitterPosition)
				{
					innerSender.SplitPosition = maxSplitterPosition;
				}
			}
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (this.FilteredGrid.ListManager != null)
				{
					this.FilteredGrid.ListManager.CurrentChanged -= ListManager_CurrentChanged;
					this.FilteredGrid.ListManager.PositionChanged -= ListManager_PositionChanged;
				}

				this.FilteredGrid.AfterBind -= FilteredGrid_AfterBind;
			}
			base.Dispose(disposing);
		}

		#endregion

		void FilteredGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			ResetAlternateChartFormatForSelectedChart();
		}

		void FilteredGrid_AfterBind(object sender, EventArgs e)
		{
			this.FilteredGrid.ListManager.CurrentChanged += new EventHandler(this.ListManager_CurrentChanged);
			this.FilteredGrid.ListManager.PositionChanged += new EventHandler(this.ListManager_PositionChanged);
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			ResetAlternateChartFormatForSelectedChart();
		}

		void ListManager_PositionChanged(object sender, EventArgs e)
		{
			ResetAlternateChartFormatForSelectedChart();
		}

		void ResetAlternateChartFormatForSelectedChart()
		{
			if (FilteredGrid.CurrentRowIndex >= 0)
			{
				fAlternateChart = ((AccAlternateChartCollection)FilteredGrid.DataSource)[FilteredGrid.CurrentRowIndex];
				((AlternateChartofAccountsFilterBusinessObject)FilterBusinessObject).SetCurrentAlternateChartFormat(fAlternateChart);
			}
			else
			{
				((AlternateChartofAccountsFilterBusinessObject)FilterBusinessObject).SetCurrentAlternateChartFormat(null);
			}
		}

		AccAlternateChart fAlternateChart;
		public AlternateChartFormatsControl AlternateChartFormatsControl;
	}
}

