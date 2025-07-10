using Enterprise.Registry.Business;
using Enterprise.Registry.Business.BillCustomisationStrategies;

namespace Enterprise.Customs.GB.Registry
{
	public class GBElementStrategy : Enterprise.Registry.Business.BillCustomisationStrategies.CommonElementStrategy
	{
		public GBElementStrategy(string key, string name, int maxGeneratedLength)
			: base(key, name, "", NumberCustomisationElementCategories.Standard, maxGeneratedLength, StrategyRegExFormattingHelper.GetExactlyLengthRegExValue)
		{ }

		public override bool OrderReadOnly => true;

		public override bool IncludeReadOnly => true;
	}
}
