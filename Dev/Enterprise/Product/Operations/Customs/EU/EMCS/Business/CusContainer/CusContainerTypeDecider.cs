using System;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class CusContainerTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DEEMCS.IEMCSCusContainer>),
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(EMCSCusContainer);
	}
}
