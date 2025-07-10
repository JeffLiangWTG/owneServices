using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Business
{
	public class CusExitDetailTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.ICusExitDetail>),
			new CountrySpecificType(Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusExitDetail>),
			new CountrySpecificType(Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusExitDetail>),
		};

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.ICusExitDetail>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusExitDetail);

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory) => GetTypeForCountryCode(GetCusExitDetailCountryCode(row, factory));

		ZString GetCusExitDetailCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			var headerPK = (row != null) ? new ZGuid(row[CusExitDetail.Schema.CED_CEH]) : ZGuid.Invalid;
			var header = factory.Load<CusExitControlHeader>(headerPK);

			return header?.CountryCode ?? CurrentCountryCode;
		}
	}
}
