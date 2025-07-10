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
	public class CusExitReportTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DEExitControl.ICusExitReport>),
			new CountrySpecificType(Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IEExitControl.ICusExitReport>),
			new CountrySpecificType(Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ESExitControl.ICusExitReport>),
			new CountrySpecificType(Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PLExitControl.ICusExitReport>),
		};

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EUExitControl.ICusExitReport>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusExitReport);

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory) => GetTypeForCountryCode(GetCusExitHeaderCountryCode(row, factory));

		ZString GetCusExitHeaderCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			var headerPK = row != null ? new ZGuid(row[CusExitReport.Schema.CER_CXH_Header]) : ZGuid.Invalid;
			var header = factory.Load<CusExitHeader>(headerPK);
			return header?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
