using System;
using System.Collections.Generic;
using CargoWise.Application;

namespace Enterprise.Customs.EU.Business
{
	public class CusPermitHeaderTypeDecider : Customs.Business.CusPermitHeaderTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusPermitHeader>),
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusPermitHeader);
	}
}
