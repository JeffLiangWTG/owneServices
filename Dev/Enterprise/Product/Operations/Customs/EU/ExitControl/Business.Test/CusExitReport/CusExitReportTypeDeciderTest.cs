using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	class CusExitReportTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DEExitControl.ICusExitReport>() },
				{ Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IEExitControl.ICusExitReport>() },
				{ Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EUExitControl.ICusExitReport>() },
				{ Core.Constants.CountryGuids.Poland, ObjectFactory.GetType<Integration.Customs.PLExitControl.ICusExitReport>() }
			};
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesAndExpectedTypes();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesAndExpectedTypes();

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			(bizO as CusExitReport).Header.Company.GC_RN_NKCountryCode = countryCode;
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest() => CusExitReportTest.GetNewBusinessObject(Factory).report;

		protected override Type BaseTypeDecidedType => typeof(CusExitReport);

		Dictionary<string, Type> GetTestCountryCodesAndExpectedTypes()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DEExitControl.ICusExitReport>() },
				{ Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IEExitControl.ICusExitReport>() },
				{ Core.Constants.CountryCodes.Latvia, ObjectFactory.GetType<Integration.Customs.EUExitControl.ICusExitReport>() },
				{ Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PLExitControl.ICusExitReport>() },
			};
		}
	}
}
