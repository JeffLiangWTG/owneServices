using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class AsycudaTransfersUserControlTest : TestCaseWithFactory
	{
		public void TestTransferHeaders()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			transferHeader.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			var transferBill = transferHeader.TransferBills.AddNew();
			transferBill.ATB_BillOfLadingType = "STD";

			using (var form = new ZForm(arrivalHeader))
			using (var control = new AsycudaTransfersUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var transferHeadersGrid = control.FindSingle<ZGrid>("transferHeadersGrid");
				var destinationPortColumn = transferHeadersGrid.GetColumnStyle(AsycudaTransferHeader.Schema.ATF_RL_NKDestinationPortCode);
				AssertType<ZCodeFindBoxColumnStyleInfo>(destinationPortColumn);
				Assert(destinationPortColumn.IsVisible);
				var transferTypeColumn = transferHeadersGrid.GetColumnStyle(AsycudaTransferHeader.Schema.ATF_TransferType);
				AssertType<ZDropEditColumnStyleInfo>(transferTypeColumn);
				Assert(transferTypeColumn.IsVisible);
				var carrierAddressColumn = transferHeadersGrid.GetColumnStyle(AsycudaTransferHeader.Schema.ATF_OA_Carrier);
				AssertType<ZGuidDropEditColumnStyleInfo>(carrierAddressColumn);
				Assert(carrierAddressColumn.IsVisible);
				var carrierIDColumn = transferHeadersGrid.GetColumnStyle(AsycudaTransferHeader.Schema.ATF_CarrierID);
				AssertType<ZTextBoxColumnStyleInfo>(carrierIDColumn);
				Assert(carrierIDColumn.IsVisible);
				AssertEquals(System.Windows.Forms.CharacterCasing.Upper, carrierIDColumn.CharacterCasing);
				var onwardCarrierColumn = transferHeadersGrid.GetColumnStyle(AsycudaTransferHeader.Schema.ATF_OnwardCarrier);
				AssertType<ZCodeFindBoxColumnStyleInfo>(onwardCarrierColumn);
				Assert(onwardCarrierColumn.IsVisible);
				var destinationWarehouseColumn = transferHeadersGrid.GetColumnStyle(AsycudaTransferHeader.Schema.ATF_OA_DestinationWarehouse);
				AssertType<ZGuidDropEditColumnStyleInfo>(destinationWarehouseColumn);
				Assert(destinationWarehouseColumn.IsVisible);
				var destinationWarehouseIDColumn = transferHeadersGrid.GetColumnStyle(AsycudaTransferHeader.Schema.ATF_DestinationWarehouseID);
				AssertType<ZTextBoxColumnStyleInfo>(destinationWarehouseIDColumn);
				Assert(destinationWarehouseIDColumn.IsVisible);
				AssertEquals(System.Windows.Forms.CharacterCasing.Upper, destinationWarehouseIDColumn.CharacterCasing);

				Assert(control.FindSingle<AsycudaTransferDetailsUserControl>("transferDetailsUserControl").Visible);
				AssertEquals(true, control.FindSingle<ZPanel>("transferDetailsSpecificPanel").Visible);
			}
		}
	}
}
