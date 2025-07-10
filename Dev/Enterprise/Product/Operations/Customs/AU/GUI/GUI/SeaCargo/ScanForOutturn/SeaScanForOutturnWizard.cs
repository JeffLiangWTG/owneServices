using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaScanForOutturnWizard : ScanForOutturnWizard
	{
		public SeaScanForOutturnWizard()
		{
		}

		public SeaScanForOutturnWizard(ScanCusSCAOceanBill hostBO)
			: base(new SeaScanForOutturnManager(hostBO))
		{ }

		protected override void UnderbondSelectionTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			base.UnderbondSelectionTabPage_InitializeTab(sender, e);
			this.UnderbondSelectionGrid.RemoveFromAvailableColumns(AutoUnderbondSelectorLine.Schema.FlightNumber);
			this.UnderbondSelectionGrid.RemoveFromAvailableColumns(AutoUnderbondSelectorLine.Schema.ArivalDate);
			this.UnderbondSelectionGrid.RemoveFromAvailableColumns(AutoUnderbondSelectorLine.Schema.UnderbondStatusText);
		}

		protected override void ShipmentSelectionTapPage_InitializeTab(object sender, System.EventArgs e)
		{
			base.ShipmentSelectionTapPage_InitializeTab(sender, e);
			this.ShipmentSelectionGrid.RemoveFromAvailableColumns(AutoShipmentSelectorLine.Schema.UnderbondStatusText);
			this.ShipmentSelectionGrid.RemoveFromAvailableColumns(AutoShipmentSelectorLine.Schema.Consignee);
		}
	}
}
