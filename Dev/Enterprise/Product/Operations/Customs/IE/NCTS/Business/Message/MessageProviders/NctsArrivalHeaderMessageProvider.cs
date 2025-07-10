using CargoWise.Common;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public abstract class NctsArrivalHeaderMessageProvider : NctsHeaderMessageProvider
	{
		protected NctsArrivalHeaderMessageProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
			ArrivalMovementHeader = Argument.NotNull(nctsHeader.ArrivalMovementHeader, nameof(ArrivalMovementHeader));
		}
		public readonly NctsArrivalMovementHeader ArrivalMovementHeader;
	}
}
