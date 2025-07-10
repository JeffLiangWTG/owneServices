using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	class CusTempStorageDecTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override Type BaseTypeDecidedType => typeof(CusTempStorageDec);

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var storageHeader = Factory.NewWithValidTestData<CusTempStorageJobHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var storageDec = storageHeader.CusTempStorageDecs.AddNew();
			return storageDec;
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Germany, Type.GetType("Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageDec, Enterprise.Customs.DE.Business") },
				{ Core.Constants.CountryGuids.France, Type.GetType("Enterprise.Customs.FR.Business.CusTempStorage.ISTCusTempStorageDec, Enterprise.Customs.FR.Business") },
				{ Core.Constants.CountryGuids.Poland, ObjectFactory.GetType<Integration.Customs.PL.ICusTempStorageDec>() },
				{ Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EU.ICusTempStorageDec>() }
			};
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO1, ZString countryCode)
		{
			var bizO = bizO1 as CusTempStorageDec;
			if (bizO != null)
			{
				bizO.StorageHeader.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
			bizO.STH_DeclarationType = "AAAAAA";
			if (countryCode == Core.Constants.CountryCodes.Germany)
			{
				bizO.STH_DeclarationType = "CUSPRL";
			}
			else if (countryCode == Core.Constants.CountryCodes.France)
			{
				bizO.STH_DeclarationType = "IST";
			}
		}

		static Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Latvia, ObjectFactory.GetType<Integration.Customs.EU.ICusTempStorageDec>() }
			};
		}
	}
}
