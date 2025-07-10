using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase5
{
	public class NctsArrivalMovementHeaderDataObjectWriter : NctsMovementHeaderDataObjectWriter
	{
		public NctsArrivalMovementHeaderDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override void PopulateDataObjectMain(NctsHeader headerBO, Shipment headerData)
		{
		}
	}
}
