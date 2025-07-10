using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	public partial class DeclarationDetailsTabUserControl : EU.NCTS.GUI.DeclarationDetailsTabUserControl
	{
		public DeclarationDetailsTabUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			var leftDataPanelHeight = LeftDataPanel.Height;
			var mainDataPanelHeight = MainDataPanel.Height;
			var traderDetailsGroupBoxHeight = TraderDetailsGroupBox.Height;
			var maxHeight = (leftDataPanelHeight > mainDataPanelHeight && leftDataPanelHeight > traderDetailsGroupBoxHeight) ? leftDataPanelHeight
				: (mainDataPanelHeight > traderDetailsGroupBoxHeight ? mainDataPanelHeight : traderDetailsGroupBoxHeight);	
			AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(maxHeight), true);
		}

		protected override Type GetContainersAndSealsUserControlType() => typeof(FRContainersAndSealsUserControl);

		protected override Type GetGuaranteesUserControlType() => typeof(FRGuaranteesUserControl);

		protected override void MoveControlsBelowGoodsLocation()
		{
			var header = Header;
			if (header != null && header.IsDepartureMovement && header.MovementHeader.IsSimplifiedNctsProcedure)
			{
				FRSpecificGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 566, true);
			}
			else
			{
				FRSpecificGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 620, true);
			}
		}

		NctsHeader Header => CurrentDataItem as NctsHeader;
	}
}
