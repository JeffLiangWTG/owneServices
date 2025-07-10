using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsDepartureMovementHeaderTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var headerPK = new ZGuid(row[NctsDepartureMovementHeader.Schema.BM_BH]);
			var header = factory.Load<NctsHeader>(headerPK);
			return GetTypeForCountryCode(header?.CountryCode ?? ZString.Empty);
		}

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.IDepartureMovementHeader>),
				new CountrySpecificType(Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.IDepartureMovementHeader>),
				new CountrySpecificType(Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.IDepartureMovementHeader>),
				new CountrySpecificType(Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.IDepartureMovementHeader>),
				new CountrySpecificType(Core.Constants.CountryCodes.Turkey, ObjectFactory.GetType<Integration.Customs.TR.IDepartureMovementHeader>),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.IDepartureMovementHeader>),
				new CountrySpecificType(Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IENCTS.IDepartureMovementHeader>),
				new CountrySpecificType(Core.Constants.CountryCodes.Belgium, ObjectFactory.GetType<Integration.Customs.BE.IDepartureMovementHeader>),
				new CountrySpecificType(Core.Constants.CountryCodes.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.IDepartureMovementHeader>),
				new CountrySpecificType(Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.IDepartureMovementHeader>),
				new CountrySpecificType(Core.Constants.CountryCodes.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.IDepartureMovementHeader>),
			};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(NctsDepartureMovementHeader);
	}
}
