using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	class CusExitContainerTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override Type BaseTypeDecidedType => typeof(CusExitContainer);

		protected override BusinessObject GetNewBusinessObjectForLoadTest() => CusExitContainerTest.GetNewBusinessObject(Factory).container;

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesAndExpectedTypes();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesAndExpectedTypes();

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IEExitControl.ICusExitContainer>() },
				{ Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EUExitControl.ICusExitContainer>() },
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ESExitControl.ICusExitContainer>() },
				{ Core.Constants.CountryGuids.Poland, ObjectFactory.GetType<Integration.Customs.PLExitControl.ICusExitContainer>() },
			};
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			((CusExitContainer)bizO).Header.Company.GC_RN_NKCountryCode = countryCode;
		}

		Dictionary<string, Type> GetTestCountryCodesAndExpectedTypes()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IEExitControl.ICusExitContainer>() },
				{ Core.Constants.CountryCodes.Latvia, ObjectFactory.GetType<Integration.Customs.EUExitControl.ICusExitContainer>() },
				{ Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ESExitControl.ICusExitContainer>() },
				{ Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PLExitControl.ICusExitContainer>() },
			};
		}
	}
}
