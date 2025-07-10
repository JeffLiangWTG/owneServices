using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class CommonTransferDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestControlTypes()
		{
			using (var control = new CommonTransferDetailsUserControl())
			{
				AssertNotNull(control.FindSingleOrDefault<ZCodeFindBox>("DestinationPortCodeFindBox"));
				AssertNotNull(control.FindSingleOrDefault<ZDropEdit>("TransferTypeDropEdit"));
				AssertNotNull(control.FindSingleOrDefault<ZAddressControl>("CarrierAddressControl"));
				AssertNotNull(control.FindSingleOrDefault<ZTextBox>("CarrierIDTextBox"));
				AssertNotNull(control.FindSingleOrDefault<ZCodeFindBox>("OnwardCarrierCodeFindBox"));
				AssertNotNull(control.FindSingleOrDefault<ZAddressControl>("DestinationWarehouseAddressControl"));
				AssertNotNull(control.FindSingleOrDefault<ZTextBox>("DestinationWarehouseIDTextBox"));
			}
		}
	}
}
