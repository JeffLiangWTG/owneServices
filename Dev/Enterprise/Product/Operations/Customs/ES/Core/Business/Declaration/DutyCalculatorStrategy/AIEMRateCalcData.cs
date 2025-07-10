using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class AIEMRateCalcData : EUUniversalRateCalcData
	{
		public AIEMRateCalcData(CusEntryLine entryLine, RateView rateView) : base(entryLine, rateView)
		{
		}

		protected override decimal CustomsValueCore => EntryLine.Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.G4_Type == EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat)?.G4_BaseAmount ?? ZDecimal.Zero;
	}
}
