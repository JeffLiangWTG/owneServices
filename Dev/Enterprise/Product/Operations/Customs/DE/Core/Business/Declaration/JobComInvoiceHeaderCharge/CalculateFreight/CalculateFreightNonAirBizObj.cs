using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class CalculateFreightNonAirBizObj : EU.Business.Declaration.CalculateFreightNonAirBizObj
	{
		protected CalculateFreightNonAirBizObj(JobComInvChargeCollection<InvoiceCharge> charges, EUIncoTermAndCustomsChargeFactory euIncoTermAndChargeFactory)
			: base(charges, euIncoTermAndChargeFactory)
		{
		}

		protected CalculateFreightNonAirBizObj(JobComInvChargeCollection<GroupInvoiceCharge> charges, EUIncoTermAndCustomsChargeFactory euIncoTermAndChargeFactory)
			: base(charges, euIncoTermAndChargeFactory)
		{
		}

		protected override bool GetCalculateResult()
		{
			var result = false;
			if (!AmountToEUBorder.IsEmpty)
			{
				euIncoTermAndChargeFactory.SetupToEUBorderCharge(charges.AddNew(), AmountToEUBorder, Currency);
				result = true;
			}
			if (!AmountToDestinationCountry.IsEmpty)
			{
				euIncoTermAndChargeFactory.SetupAfterEUBorderCharge(charges.AddNew(), AmountToDestinationCountry, Currency);
				result = true;
			}
			return result;
		}

		protected override ImmutableHashSet<ZString> GetChargeTypesToCalculate() => ImmutableHashSet.Create<ZString>(
			euIncoTermAndChargeFactory.FreightToEUBorderCode,
			euIncoTermAndChargeFactory.FreightAfterEUBorderCode);
	}
}
