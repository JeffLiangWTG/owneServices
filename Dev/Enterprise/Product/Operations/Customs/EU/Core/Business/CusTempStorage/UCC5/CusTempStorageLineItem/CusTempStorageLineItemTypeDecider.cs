using System;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusTempStorageLineItemTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.ICusTempStorageLineItem>)
			};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusTempStorageLineItem);

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.ICusTempStorageLineItem>();
	}
}
