using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Business
{
	public class CusExitItemPackageTypeDecider : CountrySpecificTypeDecider
	{
		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.ICusExitItemPackage>();

		protected override Type DefaultTypeForUnsupportedCountry => ObjectFactory.GetType<Integration.Customs.EU.ICusExitItemPackage>();

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => Array.Empty<CountrySpecificType>();

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory) => GetTypeForCountryCode(GetCusExitItemCountryCode(row, factory));

		ZString GetCusExitItemCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			var cusExitItemPK = (row != null) ? new ZGuid(row[CusExitItemPackage.Schema.B5_ParentID]) : ZGuid.Invalid;
			var cusExitItem = cusExitItemPK != ZGuid.Invalid ? factory.Load<CusExitItem>(cusExitItemPK) : null;
			return cusExitItem?.Header?.CountryCode ?? CurrentCountryCode;
		}
	}
}
