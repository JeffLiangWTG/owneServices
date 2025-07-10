using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	class CusAuthorizationUsageTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		[ExpectNoExceptions]
		public void TestTypeDecider()
		{
			var authorizationUsage = Factory.NewWithValidTestData<CusAuthorizationUsage>();
			NUnit.Framework.Assert.That(authorizationUsage, NUnit.Framework.Is.TypeOf<CusAuthorizationUsage>(), "New EU.CusAuthorizationUsage");
			Factory.Save();

			authorizationUsage = new BusinessObjectFactory().Load<CusAuthorizationUsage>(authorizationUsage.PK);
			NUnit.Framework.Assert.That(authorizationUsage, NUnit.Framework.Is.TypeOf<CusAuthorizationUsage>(), "Load as EU.CusAuthorizationUsage");

			AssertCusAuthorizationUsageTypeForCountry<Integration.Customs.IE.ICusAuthorizationUsage>(Core.Constants.CountryCodes.Ireland);
			AssertCusAuthorizationUsageTypeForCountry<Integration.Customs.FR.ICusAuthorizationUsage>(Core.Constants.CountryCodes.France);
		}

		[ExpectNoExceptions]
		public void TestTypeForLoadFromTemporaryStorage()
		{
			var asycudaManifestHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			asycudaManifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.Ireland;
			Factory.Save();

			var authorizationUsage = asycudaManifestHeader.AuthorizationUsageOrNew;
			authorizationUsage.FillWithValidTestData();
			Factory.Save();

			AssertCusAuthorizationUsageTypeForCountry<Integration.Customs.IE.ICusAuthorizationUsage>(Core.Constants.CountryCodes.Ireland);
		}

		[ExpectNoExceptions]
		public void TestTypeForLoadFromInvoiceLine()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var invoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();
				NUnit.Framework.Assert.That(invoiceHeader.CountryCode, NUnit.Framework.Is.EqualTo(Core.Constants.CountryCodes.Ireland).Using(CustomComparers.TypeComparison), "Prerequisite : invoiceHeader is Irish");
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				Factory.Save();

				var authorizationUsage = invoiceLine.CusAuthorizationUsages.AddNew();
				authorizationUsage.FillWithValidTestData();
				Factory.Save();

				AssertCusAuthorizationUsageTypeForCountry<Integration.Customs.IE.ICusAuthorizationUsage>(Core.Constants.CountryCodes.Ireland);
			}
		}

		[ExpectNoExceptions]
		void AssertCusAuthorizationUsageTypeForCountry<T>(ZString countryCode)
			where T : class, Integration.Customs.EU.ICusAuthorizationUsage
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var authorizationUsage = Factory.NewWithValidTestData<CusAuthorizationUsage>();
				NUnit.Framework.Assert.That(authorizationUsage as T, NUnit.Framework.Is.Not.EqualTo(default(T)), "$New {countryCode}.CusAuthorizationUsage - should not be [null]");
				Factory.Save();

				authorizationUsage = new BusinessObjectFactory().Load<CusAuthorizationUsage>(authorizationUsage.PK);
				NUnit.Framework.Assert.That(authorizationUsage as T, NUnit.Framework.Is.Not.EqualTo(default(T)), "Load as {countryCode}.CusAuthorizationUsage - should not be [null]");

				authorizationUsage = new BusinessObjectFactory().Load<CusAuthorizationUsage>(authorizationUsage.PK);
				NUnit.Framework.Assert.That(authorizationUsage as T, NUnit.Framework.Is.Not.EqualTo(default(T)), "$Load as {countryCode}.CusAuthorizationUsage - should not be [null]");

				authorizationUsage = new BusinessObjectFactory().Load<T>(authorizationUsage.PK) as CusAuthorizationUsage;
				NUnit.Framework.Assert.That(authorizationUsage as T, NUnit.Framework.Is.Not.EqualTo(default(T)), "$Load as {countryCode}.CusAuthorizationUsage - should not be [null]");
			}
		}

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IE.ICusAuthorizationUsage>() },
				{ Core.Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FR.ICusAuthorizationUsage>() },
				{ Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EU.ICusAuthorizationUsage>() }
			};
		}

		protected virtual Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IE.ICusAuthorizationUsage>() },
				{ Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.ICusAuthorizationUsage>() },
				{ Core.Constants.CountryCodes.Latvia, ObjectFactory.GetType<Integration.Customs.EU.ICusAuthorizationUsage>() }
			};
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			(bizO as CusAuthorizationUsage).Instruction.JobDeclaration.Company.GC_RN_NKCountryCode = countryCode;
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
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

		protected override Type BaseTypeDecidedType => typeof(CusAuthorizationUsage);
	}
}
