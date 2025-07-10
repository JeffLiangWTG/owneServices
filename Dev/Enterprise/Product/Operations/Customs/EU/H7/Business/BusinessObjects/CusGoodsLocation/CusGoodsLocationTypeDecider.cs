using System;
using System.Collections.Generic;
using CargoWise.Application;

namespace Enterprise.Customs.EU.H7.Business
{
	public class CusGoodsLocationTypeDecider : EU.Business.CusGoodsLocationTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IEH7.ICusGoodsLocation>),
			new CountrySpecificType(Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.ITH7.ICusGoodsLocation>),
			new CountrySpecificType(Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ESH7.ICusGoodsLocation>),
			new CountrySpecificType(Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.GBH7.ICusGoodsLocation>),
			new CountrySpecificType(Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FRH7.ICusGoodsLocation>),
		};

		protected override Type DefaultTypeForUnsupportedCountry => ObjectFactory.GetType<Integration.Customs.EUH7.ICusGoodsLocation>();
	}
}
