using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IssueManager.Module
{
	public partial class IssueManagerFilterControl : ZFilterStripControl, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public IssueManagerFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		#region Splitter

		public int SplitterValue
		{
			get { return Splitter.SplitPosition; }
			set
			{
				if (Splitter.Enabled)
				{
					Splitter.SplitPosition = value;
				}
			}
		}

		protected override void HandleGridSizing()
		{
			if (FilteredGrid != null)
			{
				FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, CargoWise.Windows.UI.ControlDpiScalingHelper.UnscaleFromCurrentDpiY(FilteredGrid.Top));
				if (Splitter != null)
				{
					ControlDpiScalingHelper.SetHeight(FilteredGrid, ClientSize.Height - FilteredGrid.Top - Splitter.SplitPosition - ControlDpiScalingHelper.ScaleToCurrentDpiY(10), false);
				}
				else
				{
					ControlDpiScalingHelper.SetHeight(FilteredGrid, ClientSize.Height - FilteredGrid.Top, false);
				}
				ControlDpiScalingHelper.SetWidth(FilteredGrid, ClientSize.Width, false);
			}
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new ErrorLogSatusFilterStrip();
		}

		void Splitter_SplitterMoved(object sender, SplitterEventArgs e)
		{
			var splitter = (KSplitter)sender;
			if (splitter.SplitPosition > GetMaximumSplitterPosition())
			{
				splitter.SplitPosition = GetMaximumSplitterPosition();
			}
		}

		[return: DpiState(DpiState.ScaleY)]
		int GetMaximumSplitterPosition()
		{
			return Math.Max(ClientSize.Height - FilteredGrid.Top - ControlDpiScalingHelper.ScaleToCurrentDpiY(100), 0);
		}

		#endregion

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return control == FilteredGrid && previousControl == PreviewTextBox;
		}
	}
}
