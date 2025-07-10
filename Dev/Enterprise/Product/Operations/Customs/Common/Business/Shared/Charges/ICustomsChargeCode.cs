using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common
{
	public interface ICustomsChargeCode
	{
		string Code { get; }
		MultilingualString Description { get; }
		bool IsDutiable { get; }
		bool IsDutiableDeemedForThisCharge { get; }
		bool IsVATible { get; }
		bool IsVATibleDeemedForThisCharge { get; }
		bool IsIncludedInITOTDeemedForThisCharge { get; }

		bool? IsIncludedInITOTIfDeemed { get; }
		bool IsPercentageApplicable { get; }
		bool IsStatisticalValueApplicable { get; }

		bool IsStatisticalValueApplicableDeemed { get; }
		bool IsIncoTermNeutral { get; }

		string DistributeBy { get; }
		bool DistributeByDeemedForThisCharge { get; }

		bool ConsiderIncotermWhenGroupChargeIsAppoorting { get; }
		bool ConsiderIncotermWhenAppoorting { get; }

		ChargeCodeChargeKey ChargeCodeChargeKey { get; }

		ChargeParentTypes ParentTypes { get; }
	}
}
