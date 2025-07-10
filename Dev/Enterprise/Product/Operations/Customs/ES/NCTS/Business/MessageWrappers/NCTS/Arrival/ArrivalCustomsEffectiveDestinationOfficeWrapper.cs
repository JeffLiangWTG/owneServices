using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class ArrivalCustomsEffectiveDestinationOfficeWrapper : IArrivalCustomsEffectiveDestinationOffice
	{
		public ArrivalCustomsEffectiveDestinationOfficeWrapper(NctsHeader header)
		{
			nctsHeader = Argument.NotNull(header, nameof(header));
			arrivalMovement = Argument.NotNull(nctsHeader.ArrivalMovementHeader, nameof(nctsHeader.ArrivalMovementHeader));
		}
		readonly NctsHeader nctsHeader;
		readonly NctsArrivalMovementHeader arrivalMovement;

		public ZString CustomsDestinationOfficeCode => nctsHeader.DestinationCustomsOffice?.OfficeCode.Right(4) ?? ZString.Empty;

		public ZString CustomsDestinationLocationCode => arrivalMovement.BM_LocationOfGoodsCode.Right(6);
	}
}
