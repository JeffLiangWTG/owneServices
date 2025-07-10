using System;
using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.RulesEngine.Facts
{
	public class AddressFact : IAddressFact
	{
		public AddressFact(OrgAddress address, ICountryFact countryFact)
		{
			Argument.NotNull(address, nameof(address));
			Argument.NotNull(address.Country, nameof(address.Country));
			Argument.NotNull(countryFact, nameof(countryFact));

			PK = address.PK.ToGuid();
			Country = new FactJoin<ICountryFact>(countryFact);
		}

		public Guid PK { get; }

		public FactJoin<ICountryFact> Country { get; }
	}
}
