using System;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class AlternateGLAccountsFilterControl : ZFilterStripControl
	{
		public AlternateGLAccountsFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
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

		public AlternateGLAccountsFilterControl()
		{
			InitialiseForm();
		}

		protected override void HandleGridSizing()
		{
			if (FilteredGrid != null)
			{
				FilteredGrid.Location = ControlDpiScalingHelper.NewScaledPoint(0, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(FilteredGrid.Top));
				ControlDpiScalingHelper.SetHeight(FilteredGrid, ClientSize.Height - FilteredGrid.Top, false);
				ControlDpiScalingHelper.SetWidth(FilteredGrid, ClientSize.Width, false);
			}
		}

		void GridSplitter_SplitterMoved(object sender, System.Windows.Forms.SplitterEventArgs e)
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
	}
}

