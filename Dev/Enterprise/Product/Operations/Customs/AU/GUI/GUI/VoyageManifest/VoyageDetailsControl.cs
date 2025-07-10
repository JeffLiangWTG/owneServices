using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class VoyageDetailsControl : ZUserControl
	{
		public VoyageDetailsControl()
		{
			InitializeComponent();
			OceanBillMessagesUserControl.SetBindPrepend("OceanBillsView.");

			ArrivalMessagesUserControl.MessagesGrid.BindTo = "ActualArrivalMessages";
			ArrivalMessagesUserControl.MessageTextTextBox.BindTo = "ActualArrivalMessages.EM_FormattedMessageText";
			ArrivalMessagesUserControl.SetBindPrepend("Arrivals.");

			CargoListMessageUserControl.MessagesGrid.BindTo = "CargoListAndLineMessagesCombined";
			CargoListMessageUserControl.MessageTextTextBox.BindTo = "CargoListAndLineMessagesCombined.EM_FormattedMessageText";
			CargoListMessageUserControl.SetBindPrepend("Arrivals.");
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetupPlugins();
		}

		protected void SetupPlugins()
		{
			TabControl.PlugIns.Add(ZArchitecture.Modules.ControllerIDs.Customs.AU.CusSeaManOBLDetailCusUnderbondPluginController);
		}

		void OceanBillGroupBox_Resize(object sender, EventArgs e)
		{
			// Hack to fire the event on resize.
			var parentForm = ParentForm as VoyageManifestForm;
			if ((parentForm == null || !parentForm.HasStartedDisposing) && !OceanBillsViewSplitter.Disposing && !OceanBillsViewSplitter.IsDisposed)
			{
				OceanBillsViewSplitter.SplitPosition = OceanBillsViewSplitter.SplitPosition;
			}
		}

		void ArrivalPortsGroupBox_Resize(object sender, EventArgs e)
		{
			if (!ArrivalInformationSplitter.Disposing && !ArrivalInformationSplitter.IsDisposed)
			{
				ArrivalInformationSplitter.SplitPosition = ArrivalInformationSplitter.SplitPosition;
			}
		}

		#region UserFriendlyStatuses

		internal void BillStatusDetailsButton_Click(object sender, EventArgs e)
		{
			var oceanBill = OceanBillGrid.ListManager?.GetCurrent() as Business.CusSeaManOBLHeader;
			ShowCustomsInfo(oceanBill?.ShipmentCalculator);
		}

		internal void CargoLineStatusDetailsButton_Click(object sender, EventArgs e)
		{
			var cargoLine = CargoLinesGrid.ListManager?.GetCurrent() as Business.CusSeaManOBLHeaderCargoLine;
			ShowCustomsInfo(cargoLine?.ShipmentCalculator);
		}

		void ShowCustomsInfo(Business.ICalculatedCusStatusCalculator statusCalculator)
		{
			var customsInfo = statusCalculator?.UserFriendlyStatusText ?? ZString.Empty;
			if (customsInfo.IsEmpty)
			{
				customsInfo = UserFriendlyStatusMessages.StatusNotAvailable;
			}

			Globals.Message.ShowInformation(customsInfo, UserFriendlyStatusMessages.StatusMessageHeader);
		}

		#endregion
	}
}
