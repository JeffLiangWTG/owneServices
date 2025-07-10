using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using universalAlias = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(CountryHelper))]
	sealed class CountryHelperTest : TestCaseWithFactory
	{
		public void TestSupportedCountriesComesFromDatabase()
		{
			foreach (var country in new string[] { "XX", "YY", "ZZ", "SG" })
			{
				var dc = Factory.New<ZZRefCusCodeListCombined>();
				dc.ZZD_CodeType = universalAlias.RefCusCodeListTypes.Codes.ManifestCountry;
				dc.ZZD_Code = country;
				dc.ZZD_Description = "Blah";
				dc.ZZD_EndDate = ZDateTime.MaxSmallDateTimeValue;
				dc.ZZD_StartDate = ZDateTime.MinSmallDateTimeValue;
				dc.ZZD_CountryOrGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping;
			}
			Factory.Save();

			using (ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var supportedCountries = CountryHelper.SupportedCountries(Factory);
				AssertCollectionContains("XX", supportedCountries);
				AssertCollectionContains("YY", supportedCountries);
				AssertCollectionContains("ZZ", supportedCountries);
				AssertCollectionNotContains("DC", supportedCountries);
				AssertCollectionNotContains("SG", supportedCountries);
			}

			var factory2 = new BusinessObjectFactory();
			var supportedCountries2 = CountryHelper.SupportedCountries(factory2);
			AssertCollectionContains("XX", supportedCountries2);
			AssertCollectionContains("YY", supportedCountries2);
			AssertCollectionContains("ZZ", supportedCountries2);
			AssertCollectionContains("SG", supportedCountries2);
			AssertCollectionNotContains("DC", supportedCountries2);
		}

		public void TestAddManifestCompanyFilterInSpecifiedCountry()
		{
			var companyFilter = $"AMA_RN_NKCountry <> 'IN' or AMA_GB IN (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_GC = CONVERT('{GlbCompany.CurrentCompany.PK}', 'System.Guid'))";
			var query = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			CountryHelper.AddManifestCompanyFilterInSpecifiedCountry(query);
			AssertNotContains("Should not include company filter for country not match", companyFilter, query.LiteralTextADO);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				query = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
				CountryHelper.AddManifestCompanyFilterInSpecifiedCountry(query);
				AssertNotContains("Should not include company filter for support user", companyFilter, query.LiteralTextADO);

				GlbStaff.CurrentUser.GS_LoginName = "Enterprise User";
				query = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
				CountryHelper.AddManifestCompanyFilterInSpecifiedCountry(query);
				AssertContains("Should include company filter for country matched", companyFilter, query.LiteralTextADO);
			}
		}
	}
}
