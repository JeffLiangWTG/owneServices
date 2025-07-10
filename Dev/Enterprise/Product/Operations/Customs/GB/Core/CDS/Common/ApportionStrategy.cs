using Enterprise.Customs.Common;
using Enterprise.Customs.GB.CDS.Declaration;

namespace Enterprise.Customs.GB.CDS
{
	public class ApportionStrategy : IApportionStrategy
	{
		public decimal Round(decimal amount)
		{
			return System.Math.Truncate(100 * amount) / 100;
		}

		public bool ShouldBackApportion(ApportionChargeKey chargeKey)
		{
			var cdsChargeCode = CDSCustomsChargeTypeList.MapToCDSAdditionDeductionChargeType(chargeKey.ChargeKey.ChargeCode
				, chargeKey.ChargeKey.IsDutiable
				, chargeKey.DistributeBy
				, chargeKey.IsIncludedInITOT == GroupIsIncludedInLinesOptionList.Codes.Yes);

			return !new CDSChargeTypeLevelCalculator(cdsChargeCode).IsItemLevel;
		}

		public decimal UnitOfAmountToBackApportion => 0.01m;
	}
}
