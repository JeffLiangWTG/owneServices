using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public class CusTempStorageJobHeaderTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override Type BaseTypeDecidedType => typeof(CusTempStorageJobHeader);

		protected override BusinessObject GetNewBusinessObjectForLoadTest() => Factory.NewWithValidTestData<CusTempStorageJobHeader>(TestBusinessObjectKind.MinimumRequiredToSave);

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EU.ICusTempStorageJobHeader>() },
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusTempStorageJobHeader>() },
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusTempStorageJobHeader>() },
				{ Core.Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FR.ICusTempStorageJobHeader>() }
			};
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var baseCusTempStorageJobHeader = bizO as CusTempStorageJobHeader;
			if (baseCusTempStorageJobHeader != null)
			{
				baseCusTempStorageJobHeader.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		static Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Latvia, ObjectFactory.GetType<Integration.Customs.EU.ICusTempStorageJobHeader>() },
				{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusTempStorageJobHeader>() }
			};
		}
	}
}
