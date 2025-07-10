using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class FlightStatsUsage : UniversalPriceSystemUsage
	{
		public FlightStatsUsage(
			BusinessObjectFactory factory,
			string systemCode,
			string priceHeaderCode,
			IUsingParty user,
			ZDateTime periodStart)
			: base(factory, systemCode, priceHeaderCode, user, periodStart, isTransactional: true)
		{
		}

		protected override bool PriceItemIsCorrectFeeType(ClientLicencePriceItem priceItem)
		{
			return priceItem != null && (priceItem.IsTransactional || priceItem.IsTransactionalOneVolumeBreak);
		}

		public override ZInt IncludedUnitCount => (PriceItem != null && !PriceItem.IsTransactionalOneVolumeBreak) ? PriceItem.L7_UnitBreak : ZInt.Zero;
	}
}


