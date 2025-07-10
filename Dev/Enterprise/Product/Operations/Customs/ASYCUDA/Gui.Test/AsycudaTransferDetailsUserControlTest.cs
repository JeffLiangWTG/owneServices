using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class AsycudaTransferDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestTransferDetails()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			transferHeader.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			var transferBill = transferHeader.TransferBills.AddNew();
			transferBill.ATB_BillOfLadingType = "STD";

			using (var form = new ZForm())
			using (var control = new AsycudaTransferDetailsUserControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(header, "ArrivalHeaders.TransferHeaders");
				form.Show();

				var transferDetailsGroupbox = control.FindSingle<ZGroupBox>("transferDetailsGroupbox");
				AssertEquals("Transfer Details", transferDetailsGroupbox.Text);

				var transferBillsGrid = transferDetailsGroupbox.FindSingle<ZGrid>("transferBillsGrid");
				var billNumberColumn = transferBillsGrid.GetColumnStyle(AsycudaTransferBill.Schema.ATB_BillNumber);
				AssertType<ZDropEditColumnStyleInfo>(billNumberColumn);
				Assert(billNumberColumn.IsVisible);
				var messageStatusColumn = transferBillsGrid.GetColumnStyle(AsycudaTransferBill.Schema.ATB_MessageStatus);
				AssertType<ZTextBoxColumnStyleInfo>(messageStatusColumn);
				Assert(messageStatusColumn.IsVisible);

				var dynamicDetailsPanel = control.FindSingle<DynamicLayoutPanel>("dynamicDetailsPanel");
				AssertEquals(true, dynamicDetailsPanel.Visible);

				CombineAssertions(() =>
				{
					Assert("DestinationPortCodeFindBox", dynamicDetailsPanel.FindSingle<ZCodeFindBox>("DestinationPortCodeFindBox").Visible);
					Assert("TransferTypeDropEdit", dynamicDetailsPanel.FindSingle<ZDropEdit>("TransferTypeDropEdit").Visible);
					Assert("CarrierAddressControl", dynamicDetailsPanel.FindSingle<ZAddressControl>("CarrierAddressControl").Visible);
					Assert("CarrierIDTextBox", dynamicDetailsPanel.FindSingle<ZTextBox>("CarrierIDTextBox").Visible);
					Assert("OnwardCarrierCodeFindBox", dynamicDetailsPanel.FindSingle<ZCodeFindBox>("OnwardCarrierCodeFindBox").Visible);
					Assert("DestinationWarehouseAddressControl", dynamicDetailsPanel.FindSingle<ZAddressControl>("DestinationWarehouseAddressControl").Visible);
					Assert("DestinationWarehouseIDTextBox", dynamicDetailsPanel.FindSingle<ZTextBox>("DestinationWarehouseIDTextBox").Visible);
				});

				AssertEquals(false, control.FindSingle<ZPanel>("CountrySpecificPanel").Visible);
			}
		}
	}
}
