using System;
using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.RulesEngine.Facts
{
	public class UNLOCOFact : IUNLOCOFact
	{
		public UNLOCOFact(RefUNLOCO unloco, ICountryFact countryFact)
		{
			Argument.NotNull(unloco, nameof(unloco));
			Argument.NotNull(unloco.Country, nameof(unloco.Country));
			Argument.NotNull(countryFact, nameof(countryFact));

			PK = unloco.PK.ToGuid();
			UNLOCO = unloco.RL_Code.ToString();
			Country = new FactJoin<ICountryFact>(countryFact);
		}

		public Guid PK { get; }

		public string UNLOCO { get; }

		public FactJoin<ICountryFact> Country { get; }
	}
}
