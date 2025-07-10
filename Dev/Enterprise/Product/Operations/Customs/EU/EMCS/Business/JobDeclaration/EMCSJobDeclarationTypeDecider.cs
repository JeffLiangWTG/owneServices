using System;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSJobDeclarationTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DEEMCS.IEMCSJobDeclaration>),
			new CountrySpecificType(Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IEEMCS.IEMCSJobDeclaration>),
			new CountrySpecificType(Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GBEMCS.IEMCSJobDeclaration>),
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(EMCSJobDeclaration);
	}
}
