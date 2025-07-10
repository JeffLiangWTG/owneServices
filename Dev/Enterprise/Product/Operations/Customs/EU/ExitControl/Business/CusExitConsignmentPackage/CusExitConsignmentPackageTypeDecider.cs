using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitConsignmentPackageTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IEExitControl.ICusExitConsignmentPackage>),
			new CountrySpecificType(Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ESExitControl.ICusExitConsignmentPackage>),
			new CountrySpecificType(Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PLExitControl.ICusExitConsignmentPackage>),
		};

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EUExitControl.ICusExitConsignmentPackage>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusExitConsignmentPackage);

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory) => GetTypeForCountryCode(GetCusExitConsignmentCountryCode(row, factory));

		ZString GetCusExitConsignmentCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			var headerPK = row != null ? new ZGuid(row[CusExitConsignmentPackage.Schema.CXP_CXH_Header]) : ZGuid.Invalid;
			var header = factory.Load<CusExitHeader>(headerPK);
			return header?.CountryCode ?? CurrentCountryCode;
		}
	}
}
