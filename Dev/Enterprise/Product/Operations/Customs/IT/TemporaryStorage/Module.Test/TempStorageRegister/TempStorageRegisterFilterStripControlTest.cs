using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IT.TemporaryStorage.Module.Testing;

sealed class TempStorageRegisterFilterStripControlTest : TestCaseWithFactory
{
	public void TestGridTransportIDColumn()
	{
		using var filterStripControl = new TempStorageRegisterFilterStripControl(new CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>(Factory), new TempStorageRegisterFilterBusinessObject());
		var grid = filterStripControl.Grid;
		var transportIDColumn = grid.GetColumnStyle(CusTempStorageRegHeader.Schema.SRH_TransportID) as ZTextBoxColumnStyleInfo;
		AssertNotNull(transportIDColumn);
		AssertEquals(transportIDColumn.Width, 100);
	}
}
