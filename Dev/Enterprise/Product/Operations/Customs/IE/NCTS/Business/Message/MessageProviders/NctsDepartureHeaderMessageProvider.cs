using CargoWise.Common;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public abstract class NctsDepartureHeaderMessageProvider : NctsHeaderMessageProvider
	{
		protected NctsDepartureHeaderMessageProvider(CusGuaranteeHeader cusGuaranteeHeader) : base(cusGuaranteeHeader)
		{
		}

		protected NctsDepartureHeaderMessageProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
			MovementHeader = Argument.NotNull(nctsHeader.MovementHeader, nameof(MovementHeader));
		}

		public NctsDepartureMovementHeader MovementHeader { get; }

		public bool IsInTransitionPeriod => MovementHeader.IsInPhase5TransitionPeriod;
	}
}
