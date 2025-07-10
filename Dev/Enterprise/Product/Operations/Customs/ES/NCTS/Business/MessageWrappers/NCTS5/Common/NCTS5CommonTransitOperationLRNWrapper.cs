using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonTransitOperationLRNWrapper : INCTSCommonTransitOperationLRN
	{
		public NCTS5CommonTransitOperationLRNWrapper(NctsHeader header)
		{
			nctsHeader = Argument.NotNull(header, nameof(header));
			departureMovement = Argument.NotNull(nctsHeader.MovementHeader, nameof(nctsHeader.MovementHeader));
		}
		protected NctsHeader nctsHeader;
		protected readonly NctsDepartureMovementHeader departureMovement;

		public ZString LRN => departureMovement.BM_PaperlessInbondNum;
	}
}
