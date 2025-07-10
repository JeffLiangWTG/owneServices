using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	class CusExitControlHeaderTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		[ExpectNoExceptions]
		public void TestTypeDecider()
		{
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			NUnit.Framework.Assert.That(exitHeader, NUnit.Framework.Is.TypeOf<CusExitControlHeader>(), "New EU.CusExitControlHeader");
			Factory.Save();

			exitHeader = new BusinessObjectFactory().Load<CusExitControlHeader>(exitHeader.PK);
			NUnit.Framework.Assert.That(exitHeader, NUnit.Framework.Is.TypeOf<CusExitControlHeader>(), "Load as EU.CusExitControlHeader");

			AssertCusExitControlHeaderTypeForCountry<Integration.Customs.FR.ICusExitControlHeader>(Core.Constants.CountryCodes.France);
			AssertCusExitControlHeaderTypeForCountry<Integration.Customs.ES.ICusExitControlHeader>(Core.Constants.CountryCodes.Spain);
			AssertCusExitControlHeaderTypeForCountry<Integration.Customs.DE.ICusExitControlHeader>(Core.Constants.CountryCodes.Germany);
		}

		[ExpectNoExceptions]
		void AssertCusExitControlHeaderTypeForCountry<T>(ZString countryCode)
			where T : class, Integration.Customs.EU.ICusExitControlHeader
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
				NUnit.Framework.Assert.That(exitHeader as T, NUnit.Framework.Is.Not.EqualTo(default(T)), "$New {countryCode}.CusExitControlHeader - should not be [null]");
				Factory.Save();

				exitHeader = new BusinessObjectFactory().Load<CusExitControlHeader>(exitHeader.PK);
				NUnit.Framework.Assert.That(exitHeader as T, NUnit.Framework.Is.Not.EqualTo(default(T)), "Load as {countryCode}.CusExitControlHeader - should not be [null]");

				exitHeader = new BusinessObjectFactory().Load<CusExitControlHeader>(exitHeader.PK);
				NUnit.Framework.Assert.That(exitHeader as T, NUnit.Framework.Is.Not.EqualTo(default(T)), "$Load as {countryCode}.CusExitControlHeader - should not be [null]");

				exitHeader = new BusinessObjectFactory().Load<T>(exitHeader.PK) as CusExitControlHeader;
				NUnit.Framework.Assert.That(exitHeader as T, NUnit.Framework.Is.Not.EqualTo(default(T)), "$Load as {countryCode}.CusExitControlHeader - should not be [null]");
			}
		}

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FR.ICusExitControlHeader>() },
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusExitControlHeader>() },
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusExitControlHeader>() },
				{ Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EU.ICusExitControlHeader>() }
			};
		}

		protected virtual Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.ICusExitControlHeader>() },
				{ Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusExitControlHeader>() },
				{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusExitControlHeader>() },
				{ Core.Constants.CountryCodes.Latvia, ObjectFactory.GetType<Integration.Customs.EU.ICusExitControlHeader>() }
			};
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			(bizO as CusExitControlHeader).Declaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader.CEH_ParentID = declaration.PK;
			exitHeader.CEH_ParentTableCode = declaration.TablePrefix;
			return exitHeader;
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		protected override Type BaseTypeDecidedType => typeof(CusExitControlHeader);
	}
}
