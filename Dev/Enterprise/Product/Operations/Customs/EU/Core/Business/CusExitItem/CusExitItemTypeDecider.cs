using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Business
{
	public class CusExitItemTypeDecider : CountrySpecificTypeDecider
	{
		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.ICusExitItem>();

		protected override Type DefaultTypeForUnsupportedCountry => ObjectFactory.GetType<Integration.Customs.EU.ICusExitItem>();

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusExitItem>),
		};

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory) => GetTypeForCountryCode(GetCusExitItemCountryCode(row, factory));

		ZString GetCusExitItemCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			var detailPK = (row != null) ? new ZGuid(row[CusExitItem.Schema.CXI_CED]) : ZGuid.Invalid;
			var detail = detailPK != ZGuid.Invalid ? factory.Load<CusExitDetail>(detailPK) : null;

			return detail?.Header.CountryCode ?? CurrentCountryCode;
		}
	}
}
