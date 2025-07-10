using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsArrivalMovementHeaderTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var headerPK = new ZGuid(row[NctsArrivalMovementHeader.Schema.BM_BH]);
			var header = factory.Load<NctsHeader>(headerPK);
			return GetTypeForCountryCode(header?.CountryCode ?? ZString.Empty);
		}

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.IArrivalMovementHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Belgium, ObjectFactory.GetType<Integration.Customs.BE.IArrivalMovementHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.IArrivalMovementHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.IArrivalMovementHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IENCTS.IArrivalMovementHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.IArrivalMovementHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.IArrivalMovementHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.IArrivalMovementHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Norway, ObjectFactory.GetType<Integration.Customs.NO.IArrivalMovementHeader>),
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(NctsArrivalMovementHeader);
	}
}
