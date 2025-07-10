using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.RulesEngine.Facts
{
	public class CountryFact : ICountryFact
	{
		public CountryFact(RefCountry country)
		{
			Argument.NotNull(country, nameof(country));

			Code = country.RN_Code;
			EconomicGrouping = country.RN_EconomicGrouping;
		}

		public string Code { get; }

		public string EconomicGrouping { get; }
	}
}
