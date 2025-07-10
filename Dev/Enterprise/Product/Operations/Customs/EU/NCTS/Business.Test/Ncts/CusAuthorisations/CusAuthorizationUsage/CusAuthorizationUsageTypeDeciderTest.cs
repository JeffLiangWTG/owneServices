using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class CusAuthorizationUsageTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override Type BaseTypeDecidedType => typeof(CusAuthorizationUsage);

		public void TestTypeDecider()
		{
			CombineAssertions(() =>
			{
				var authorizationUsage = GetNewBusinessObjectForLoadTest();
				AssertType<CusAuthorizationUsage>("New NCTS.EU.CusAuthorizationUsage", authorizationUsage);
				Factory.Save();

				authorizationUsage = new BusinessObjectFactory().Load<CusAuthorizationUsage>(authorizationUsage.PK);
				AssertType<CusAuthorizationUsage>("Load as NCTS.EU.CusAuthorizationUsage", authorizationUsage);
			});

			AssertCusAuthorizationUsageTypeForCountry<Integration.Customs.PL.INctsCusAuthorizationUsage>(Core.Constants.CountryCodes.Poland);
		}

		public void TestTypeDecider_WhenParentIsMovementHeader()
		{
			CombineAssertions(() =>
			{
				var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				var authorizationUsage = nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();
				authorizationUsage.FillWithValidTestData();

				AssertType<CusAuthorizationUsage>("New NCTS.EU.CusAuthorizationUsage", authorizationUsage);
				Factory.Save();

				authorizationUsage = new BusinessObjectFactory().Load<CusAuthorizationUsage>(authorizationUsage.PK);
				AssertType<CusAuthorizationUsage>("Load as NCTS.EU.CusAuthorizationUsage", authorizationUsage);
			});

			AssertCusAuthorizationUsageTypeForCountry<Integration.Customs.PL.INctsCusAuthorizationUsage>(Core.Constants.CountryCodes.Poland);
		}

		void AssertCusAuthorizationUsageTypeForCountry<T>(ZString countryCode)
			where T : class, Integration.Customs.EU.ICusAuthorizationUsage
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				CombineAssertions(() =>
				{
					var authorizationUsage = GetNewBusinessObjectForLoadTest();
					AssertNotNull($"New {countryCode}.CusAuthorizationUsage", authorizationUsage as T);
					Factory.Save();

					authorizationUsage = new BusinessObjectFactory().Load<CusAuthorizationUsage>(authorizationUsage.PK);
					AssertNotNull($"Load as {countryCode}.CusAuthorizationUsage", authorizationUsage as T);

					authorizationUsage = new BusinessObjectFactory().Load<CusAuthorizationUsage>(authorizationUsage.PK);
					AssertNotNull($"Load as {countryCode}.CusAuthorizationUsage", authorizationUsage as T);

					authorizationUsage = new BusinessObjectFactory().Load<T>(authorizationUsage.PK) as CusAuthorizationUsage;
					AssertNotNull($"Load as {countryCode}.CusAuthorizationUsage", authorizationUsage as T);
				});
			}
		}

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Poland, ObjectFactory.GetType<Integration.Customs.PL.INctsCusAuthorizationUsage>() },
				{ Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EU.NCTS.ICusAuthorizationUsage>() }
			};
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			var authorizationUsage = nctsHeader.CusAuthorizationUsages.AddNew();
			authorizationUsage.FillWithValidTestData();
			return authorizationUsage;
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		protected virtual Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.INctsCusAuthorizationUsage>() },
				{ Core.Constants.CountryCodes.Latvia, ObjectFactory.GetType<Integration.Customs.EU.NCTS.ICusAuthorizationUsage>() }
			};
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			(bizO as CusAuthorizationUsage).Header.Company.GC_RN_NKCountryCode = countryCode;
		}
	}
}
