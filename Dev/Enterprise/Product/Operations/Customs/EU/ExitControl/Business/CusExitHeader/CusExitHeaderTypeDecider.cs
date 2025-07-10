using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitHeaderTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DEExitControl.ICusExitHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IEExitControl.ICusExitHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ESExitControl.ICusExitHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PLExitControl.ICusExitHeader>),
		};

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EUExitControl.ICusExitHeader>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusExitHeader);

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory) => GetTypeForCountryCode(GetCusExitConsignmentCountryCode(row, factory));

		ZString GetCusExitConsignmentCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			var companyPK = row != null ? new ZGuid(row[CusExitHeader.Schema.CXH_GC_Company]) : ZGuid.Invalid;
			var company = factory.Load<GlbCompany>(companyPK);
			return company?.GC_RN_NKCountryCode ?? CurrentCountryCode;
		}
	}
}
