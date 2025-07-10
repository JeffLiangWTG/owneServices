using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(ProfessionalServicesQuoteFilterBusinessObject))]
	public class ProfessionalServicesQuoteFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		LicenceEnterprise licenceEnt1;
		LicenceEnterprise licenceEnt2;
		ProfessionalServicesQuote quote1;
		ProfessionalServicesQuote quote2;
		ProfessionalServicesQuote quote3;
		ProfessionalServicesQuote quote4;
		protected override void SetUp()
		{
			base.SetUp();
			CreateTestData();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ProfessionalServicesQuoteFilterBusinessObject();
		}

		ProfessionalServicesQuoteFilterBusinessObject FilterBizO
		{
			get
			{
				return (ProfessionalServicesQuoteFilterBusinessObject)CachedBusinessObject;
			}
		}

		public void TestEnterpriseCodeFilterContents()
		{
			ModuleGuidFilter licenceFilter = (ModuleGuidFilter)FilterBizO["Enterprise Code"];
			AssertNull(licenceFilter.FilterColumn);
			AssertEquals("Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterpriseCollectionForEntCodeFilter", licenceFilter.List.GetType().FullName);
			AssertEquals(ClientModuleRegistration.LicenceEnterprise, licenceFilter.ModuleId);
			AssertEquals(FilterCategories.Other, licenceFilter.Category);
		}

		public void TestCountryFilterContents()
		{
			var countryFilter = (ModuleGuidFilter)FilterBizO["Country"];
			AssertNull(countryFilter.FilterColumn);
			AssertEquals(typeof(RefCountryCollection), countryFilter.List.GetType());
			AssertEquals(ModuleIDs.RefCountry, countryFilter.ModuleId);
			AssertEquals(FilterCategories.Other, countryFilter.Category);
		}

		public void TestGetLicenceEnterpriseQuery()
		{
			FilterBizO["Enterprise Code"].IsActive = true;
			((ModuleGuidFilter)FilterBizO["Enterprise Code"]).Property = licenceEnt1.PK;
			var collection = new ProfessionalServicesQuoteCollection(Factory);
			collection.Load(FilterBizO.Filter);
			AssertEquals(3, collection.Count);
			AssertCollectionContains(quote1, collection);
			AssertCollectionContains(quote2, collection);
			AssertCollectionContains(quote3, collection);
			((ModuleGuidFilter)FilterBizO["Enterprise Code"]).Property = licenceEnt2.PK;
			collection.Load(FilterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertEquals(quote4, collection[0]);
		}

		public void TestGetLicenceEnterpriseIDQuery()
		{
			FilterBizO["Enterprise ID"].IsActive = true;
			((ModuleGuidFilter)FilterBizO["Enterprise ID"]).Property = licenceEnt1.PK;
			var collection = new ProfessionalServicesQuoteCollection(Factory);
			collection.Load(FilterBizO.Filter);
			AssertEquals(3, collection.Count);
			AssertCollectionContains(quote1, collection);
			AssertCollectionContains(quote2, collection);
			AssertCollectionContains(quote3, collection);
			((ModuleGuidFilter)FilterBizO["Enterprise ID"]).Property = licenceEnt2.PK;
			collection.Load(FilterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertEquals(quote4, collection[0]);
		}

		public void TestGetLicenceCompanyCountryQuery()
		{
			FilterBizO["Country"].IsActive = true;
			((ModuleGuidFilter)FilterBizO["Country"]).Property = Core.Constants.CountryGuids.Australia;
			var collection = new ProfessionalServicesQuoteCollection(Factory);
			collection.Load(FilterBizO.Filter);
			AssertEquals(3, collection.Count);
			AssertCollectionContains(quote1, collection);
			AssertCollectionContains(quote2, collection);
			AssertCollectionContains(quote3, collection);
			((ModuleGuidFilter)FilterBizO["Country"]).Property = Core.Constants.CountryGuids.UnitedKingdom;
			collection.Load(FilterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertEquals(quote4, collection[0]);
		}

		void CreateTestData()
		{
			OrgHeader clientEnt = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client3 = Factory.NewWithValidTestData<OrgHeader>();
			licenceEnt1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			licenceEnt1.LE_OH = clientEnt.PK;
			LicenceCompany licenceClientEnt1 = Factory.NewWithValidTestData<LicenceCompany>();
			licenceClientEnt1.LC_OH = clientEnt.PK;
			licenceClientEnt1.LC_LE = licenceEnt1.PK;
			licenceClientEnt1.LC_CompanyCountry = Core.Constants.CountryCodes.Australia;
			LicenceCompany licenceClient1 = Factory.NewWithValidTestData<LicenceCompany>();
			licenceClient1.LC_OH = client1.PK;
			licenceClient1.LC_LE = licenceEnt1.PK;
			licenceClient1.LC_CompanyCountry = Core.Constants.CountryCodes.Australia;
			LicenceCompany licenceClient2 = Factory.NewWithValidTestData<LicenceCompany>();
			licenceClient2.LC_OH = client2.PK;
			licenceClient2.LC_LE = licenceEnt1.PK;
			licenceClient2.LC_CompanyCountry = Core.Constants.CountryCodes.Australia;
			licenceEnt2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			licenceEnt2.LE_OH = client3.PK;
			LicenceCompany licenceClient3 = Factory.NewWithValidTestData<LicenceCompany>();
			licenceClient3.LC_OH = client3.PK;
			licenceClient3.LC_LE = licenceEnt2.PK;
			licenceClient3.LC_CompanyCountry = Core.Constants.CountryCodes.UnitedKingdom;
			quote1 = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			quote1.IM_OH_Client = clientEnt.PK;
			quote2 = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			quote2.IM_OH_Client = client1.PK;
			quote3 = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			quote3.IM_OH_Client = client2.PK;
			quote4 = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			quote4.IM_OH_Client = client3.PK;
			Factory.Save();
		}
	}
}
