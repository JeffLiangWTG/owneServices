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
	class CusExitDetailTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		[ExpectNoExceptions]
		public void TestTypeDecider()
		{
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			var exitDetail = exitHeader.CusExitDetails.AddNew();
			exitDetail.CED_Status = "XXX";
			NUnit.Framework.Assert.That(exitDetail, NUnit.Framework.Is.TypeOf<CusExitDetail>(), "New EU.CusExitDetail");
			Factory.Save();

			exitDetail = new BusinessObjectFactory().Load<CusExitDetail>(exitDetail.PK);
			NUnit.Framework.Assert.That(exitDetail, NUnit.Framework.Is.TypeOf<CusExitDetail>(), "Load as EU.CusExitDetail");

			AssertCusExitDetailTypeForCountry<Integration.Customs.FR.ICusExitDetail>(Core.Constants.CountryCodes.France);

			AssertCusExitDetailTypeForCountry<Integration.Customs.ES.ICusExitDetail>(Core.Constants.CountryCodes.Spain);

			AssertCusExitDetailTypeForCountry<Integration.Customs.DE.ICusExitDetail>(Core.Constants.CountryCodes.Germany);
		}

		[ExpectNoExceptions]
		void AssertCusExitDetailTypeForCountry<T>(ZString countryCode)
			where T : class, Integration.Customs.EU.ICusExitDetail
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
				var exitDetail = exitHeader.CusExitDetails.AddNew();
				exitDetail.CED_Status = "XXX";
				NUnit.Framework.Assert.That(exitDetail as T, NUnit.Framework.Is.Not.EqualTo(default(T)), "$New {countryCode}.CusExitDetail - should not be [null]");
				Factory.Save();

				exitDetail = new BusinessObjectFactory().Load<CusExitDetail>(exitDetail.PK);
				NUnit.Framework.Assert.That(exitDetail as T, NUnit.Framework.Is.Not.EqualTo(default(T)), "$Load as {countryCode}.CusExitDetail - should not be [null]");

				exitDetail = new BusinessObjectFactory().Load<CusExitDetail>(exitDetail.PK);
				NUnit.Framework.Assert.That(exitDetail as T, NUnit.Framework.Is.Not.EqualTo(default(T)), "$Load as {countryCode}.CusExitDetail - should not be [null]");

				exitDetail = new BusinessObjectFactory().Load<T>(exitDetail.PK) as CusExitDetail;
				NUnit.Framework.Assert.That(exitDetail as T, NUnit.Framework.Is.Not.EqualTo(default(T)), "$Load as {countryCode}.CusExitDetail - should not be [null]");
			}
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			(bizO as CusExitDetail).Header.Declaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader.CEH_ParentID = declaration.PK;
			exitHeader.CEH_ParentTableCode = declaration.TablePrefix;
			var exitDetail = exitHeader.CusExitDetails.AddNew();
			exitDetail.CED_Status = "XXX";

			return exitDetail;
		}

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FR.ICusExitDetail>() },
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusExitDetail>() },
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusExitDetail>() },
				{ Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EU.ICusExitDetail>() }
			};
		}

		protected virtual Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.ICusExitDetail>() },
				{ Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusExitDetail>() },
				{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusExitDetail>() },
				{ Core.Constants.CountryCodes.Latvia, ObjectFactory.GetType<Integration.Customs.EU.ICusExitDetail>() }
			};
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		protected override Type BaseTypeDecidedType => typeof(CusExitDetail);
	}
}
