using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.NCTS
{
	internal class NctsHeaderPhase4ValueSetStrategy : IValueSetStrategy
	{
		public NctsHeaderPhase4ValueSetStrategy(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}

		readonly NctsHeader nctsHeader;

		public void ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			switch (valueThatHasChanged.Name)
			{
				case NctsHeader.Schema.BH_RL_NKImportLoadPort:
					DefaultTHI();
					break;
			}
		}

		public void DefaultTHI()
		{
			if (nctsHeader.IsDepartureMovement)
			{
				var movementHeader = nctsHeader.MovementHeader;
				movementHeader.ChargePaymentOrDestinationID = UniversalReferenceDataHelper.GetChargePaymentOrDestinationID(nctsHeader.Factory, movementHeader);
			}
		}
	}
}
