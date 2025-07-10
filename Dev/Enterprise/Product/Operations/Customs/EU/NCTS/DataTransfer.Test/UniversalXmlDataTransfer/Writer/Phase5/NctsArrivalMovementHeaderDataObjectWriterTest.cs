using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase5.Testing
{
	class NctsArrivalMovementHeaderDataObjectWriterTest : NctsMovementHeaderDataObjectWriter<NctsArrivalMovementHeaderDataObjectWriter>
	{
		protected override ZString MovementType => NctsMovementType.Codes.Arrival;
		protected override NctsArrivalMovementHeaderDataObjectWriter GetNewWriter(IDataWritingManager manager) => new NctsArrivalMovementHeaderDataObjectWriter(manager);

		protected override void AssertAdditionalDataCore(Shipment shipment)
		{
		}

		protected override void SetupAdditionalDataCore(NctsHeader header)
		{
		}
	}
}
