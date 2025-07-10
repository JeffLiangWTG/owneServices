using CargoWise.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public sealed class NctsPackageDeparturePhase5ValidationDecider : INctsPackageDeparturePhase5ValidationDecider
	{
		public NctsPackageDeparturePhase5ValidationDecider(NctsPackage nctsPackage)
		{
			this.nctsPackage = Argument.NotNull(nctsPackage, nameof(nctsPackage));
		}

		public bool IsRuleB1819Active => false;

		public bool IsRuleB1919Active => false;

		public bool IsRuleC0060Active => !nctsPackage.ItemsPackagesAreCustomsCompliant;

		public bool IsRuleC0060_1Active => false;

		public bool IsRuleC0060_2Active => !nctsPackage.ItemsPackagesAreCustomsCompliant;

		public bool IsRuleC0060_3Active => false;

		public bool IsRuleC0670Active => true;

		public bool IsRuleE1111Active => true;

		public bool IsRuleNR0003Active => false;

		public bool IsRuleNR0027Active => false;

		public bool IsRuleR0219Active => false;

		public bool IsRuleR0220Active => false;

		public bool IsRuleR0364_1Active => false;

		public bool IsRuleR0364_2Active => false;

		public bool IsRuleR0364_3Active => false;

		public bool IsRuleTR0066Active => true;

		public bool IsRuleTR0083Active => true;

		readonly NctsPackage nctsPackage;
	}
}
