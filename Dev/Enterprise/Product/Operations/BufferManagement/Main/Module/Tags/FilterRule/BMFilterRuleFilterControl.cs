using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.BufferManagement.Module
{
	public partial class BMFilterRuleFilterControl : ZFilterStripControl
	{
		private ZArchitecture.ZLabel moduleDescription;

		public BMFilterRuleFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			CaptionRenderingEnabledChanged += delegate
			{
				if (CaptionRenderingEnabled.HasValue && !CaptionRenderingEnabled.Value)
				{
					moduleDescription.Hide();
					moduleDescription.Height = ControlDpiScalingHelper.ScaleToCurrentDpiX(0);
					PerformLayout();
				}
			};
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new ProcessHeaderFilterStrip();
		}

		protected override void HandleGridSizing()
		{
			base.HandleGridSizing();

			if (moduleDescription != null)
			{
				ControlDpiScalingHelper.SetHeight(Grid, ClientSize.Height - (Grid.Top + moduleDescription.Height), false);
			}
		}
	}
}
