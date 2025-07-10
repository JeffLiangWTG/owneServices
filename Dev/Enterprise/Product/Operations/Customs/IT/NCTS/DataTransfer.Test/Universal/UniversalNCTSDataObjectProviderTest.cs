using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using NctsHeader = Enterprise.Customs.IT.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.IT.NCTS.DataTransfer.Testing;

sealed class UniversalNCTSDataObjectProviderTest : OrganizationAddressTestHelper
{
	public void TestGetNewNctsHeaderDataObjectReaderType_Phase4()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = "NC4";

		var manager = (IShipmentDataContextManager)header.GetUniversalDataContextManager();
		var writingManager = new DataWritingManager(new ActionInfo(null, header));
		var writer = manager.GetShipmentDataObjectWriter(writingManager);

		var shipment = (Shipment)writer.GetDataObject(header);
		var reader = ((IShipmentDataContextManagerInternal)manager).GetShipmentDataObjectReader(shipment, new DummyLogger(), Factory);
		AssertType<NctsHeaderDataObjectReader>(reader);
	}

	public void TestGetNewNctsHeaderDataObjectReaderType_Phase5_Departure()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = "NC5";
		header.BH_HeaderType = NctsMovementType.Codes.Departure;

		var manager = (IShipmentDataContextManager)header.GetUniversalDataContextManager();
		var writingManager = new DataWritingManager(new ActionInfo(null, header));
		var writer = manager.GetShipmentDataObjectWriter(writingManager);

		var shipment = (Shipment)writer.GetDataObject(header);
		var reader = ((IShipmentDataContextManagerInternal)manager).GetShipmentDataObjectReader(shipment, new DummyLogger(), Factory);
		AssertType<EU.NCTS.DataTransfer.Phase5.NctsDepartureMovementHeaderDataObjectReader>(reader);
	}

	public void TestGetNewNctsHeaderDataObjectWriterType_Phase4()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = "NC4";

		var manager = (IShipmentDataContextManager)header.GetUniversalDataContextManager();
		var writingManager = new DataWritingManager(new ActionInfo(null, header));
		var writer = manager.GetShipmentDataObjectWriter(writingManager);
		AssertType<NctsHeaderDataObjectWriter>(writer);
	}

	public void TestGetNewNctsHeaderDataObjectWriterType_Phase5_Departure()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = "NC5";
		header.BH_HeaderType = NctsMovementType.Codes.Departure;

		var manager = (IShipmentDataContextManager)header.GetUniversalDataContextManager();
		var writingManager = new DataWritingManager(new ActionInfo(null, header));
		var writer = manager.GetShipmentDataObjectWriter(writingManager);
		AssertType<EU.NCTS.DataTransfer.Phase5.NctsDepartureMovementHeaderDataObjectWriter>(writer);
	}

	public void TestGetNewNctsHeaderDataObjectWriterType_Phase5_Arrival()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = "NC5";
		header.BH_HeaderType = NctsMovementType.Codes.Arrival;

		var manager = (IShipmentDataContextManager)header.GetUniversalDataContextManager();
		var writingManager = new DataWritingManager(new ActionInfo(null, header));
		var writer = manager.GetShipmentDataObjectWriter(writingManager);
		AssertType<EU.NCTS.DataTransfer.Phase5.NctsArrivalMovementHeaderDataObjectWriter>(writer);
	}
}
