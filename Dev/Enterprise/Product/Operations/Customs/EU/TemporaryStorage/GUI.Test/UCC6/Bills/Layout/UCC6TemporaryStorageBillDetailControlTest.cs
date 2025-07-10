using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	sealed class UCC6TemporaryStorageBillDetailControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new UCC6TemporaryStorageBillDetailControl())
			{
				AssertNotNull(control.FindSingle<ZTextBox>("billNumberTextBox"));
				AssertNotNull(control.FindSingle<ZTextBox>("uCRNumberTextBox"));
				AssertNotNull(control.FindSingle<ZAddressControl>("consignorAddressControl"));
				AssertNotNull(control.FindSingle<ZAddressControl>("consigneeAddressControl"));
				AssertNotNull(control.FindSingle<ZAddressControl>("notifyPartyAddressControl"));
				AssertNotNull(control.FindSingle<UCC6TemporaryStorageGrossWeightWithUnitUserControl>("uCC6TemporaryStorageGrossWeightWithUnitUserControl"));
			}
		}
	}
}
