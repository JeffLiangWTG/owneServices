using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	public class LicenceFiltersTest : TestCaseWithFactory
	{
		public void TestDatabaseStaffReceivedFilter()
		{
			string code1 = "JN1";
			string code2 = "JN2";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_Email = code1 + "@test.com";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_Email = code2 + "@test.com";

			var enterprise1 = Factory.New<LicenceEnterprise>();
			enterprise1.LE_EnterpriseCode = code1;
			enterprise1.LE_OH = org1.PK;

			var enterprise2 = Factory.New<LicenceEnterprise>();
			enterprise2.LE_EnterpriseCode = code2;
			enterprise2.LE_OH = org2.PK;

			var company1 = enterprise1.Companies.AddNew();
			company1.LC_CompanyCode = code1;
			company1.LC_OH = org1.PK;
			company1.LC_LE = enterprise1.PK;

			var company2 = enterprise1.Companies.AddNew();
			company2.LC_CompanyCode = code2;
			company2.LC_OH = org2.PK;
			company2.LC_LE = enterprise2.PK;

			var database1 = enterprise1.Databases.AddNew();
			database1.LD_ServerCode = code1;
			database1.LD_HostDBName = "ZZZ";
			database1.LD_LicenceType = DatabaseTypes.Codes.Production;
			database1.LD_StaffFirstReportUtc = ZDateTime.Now;

			var database2 = enterprise2.Databases.AddNew();
			database2.LD_ServerCode = code2;
			database2.LD_HostDBName = "ZZ1";
			database2.LD_LicenceType = DatabaseTypes.Codes.Production;

			var licence1 = Factory.New<LicenceHeader>();
			licence1.LA_LD = database1.PK;
			licence1.LA_LC = company1.PK;
			licence1.LA_SiteLiveDate = new ZDateTime(2010, 1, 1);

			var licence2 = Factory.New<LicenceHeader>();
			licence2.LA_LD = database2.PK;
			licence2.LA_LC = company2.PK;
			licence2.LA_SiteLiveDate = new ZDateTime(2010, 1, 1);

			Factory.Save();

			var filters = new LicenceFilters(new LicenceKeyFilterBusinessObject(), FilterStripBusinessObject.StatusActive, FilterStripBusinessObject.StatusInactive, FilterStripBusinessObject.StatusAll);
			var filter = filters.FilterStripBusinessObject["Database Staff List Received"] as ModuleFlagsFilter;
			filter.IsActive = true;
			filter.Property0 = ZBool.True;

			var headers = Factory.Load<LicenceHeader>(filters.FilterStripBusinessObject.Filter);
			AssertEquals(1, headers.Length);
			AssertEquals(licence1.PK, headers[0].PK);

			filter.Property0 = ZBool.False;
			headers = Factory.Load<LicenceHeader>(filters.FilterStripBusinessObject.Filter);
			AssertEquals(1, headers.Length);
			AssertEquals(licence2.PK, headers[0].PK);
		}

		public void TestDatabaseBillableFilter()
		{
			string code1 = "JN1";
			string code2 = "JN2";

			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_Email = code1 + "@test.com";

			var enterprise1 = Factory.New<LicenceEnterprise>();
			enterprise1.LE_EnterpriseCode = code1;
			enterprise1.LE_OH = org1.PK;

			var company1 = enterprise1.Companies.AddNew();
			company1.LC_CompanyCode = code1;
			company1.LC_OH = org1.PK;
			company1.LC_LE = enterprise1.PK;

			var database1 = enterprise1.Databases.AddNew();
			database1.LD_ServerCode = code1;
			database1.LD_HostDBName = "ZZZ";
			database1.LD_LicenceType = DatabaseTypes.Codes.Production;
			database1.LD_Billable = "N";

			var database2 = enterprise1.Databases.AddNew();
			database2.LD_ServerCode = code2;
			database2.LD_HostDBName = "ZZ1";
			database2.LD_LicenceType = DatabaseTypes.Codes.Production;
			database2.LD_Billable = "Y";

			var licence1 = Factory.New<LicenceHeader>();
			licence1.LA_LD = database1.PK;
			licence1.LA_LC = company1.PK;
			licence1.LA_SiteLiveDate = new ZDateTime(2010, 1, 1);

			var licence2 = Factory.New<LicenceHeader>();
			licence2.LA_LD = database2.PK;
			licence2.LA_LC = company1.PK;
			licence2.LA_SiteLiveDate = new ZDateTime(2010, 1, 1);

			Factory.Save();

			var filters = new LicenceFilters(new LicenceKeyFilterBusinessObject(), FilterStripBusinessObject.StatusActive, FilterStripBusinessObject.StatusInactive, FilterStripBusinessObject.StatusAll);
			var filter = filters.FilterStripBusinessObject["Billable"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "Y";

			var licenceDatabases = Factory.Load<LicenceHeader>(filters.FilterStripBusinessObject.Filter);
			AssertEquals(1, licenceDatabases.Length);
			AssertEquals(licence2.PK, licenceDatabases[0].PK);
		}

		public void TestCurrentDateOffsetFilter()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AAASYD";
			org.OH_FullName = "AAA Sydney";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Contact Current Date Offset Test";

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "E99";
			enterprise.LE_OH = org.PK;

			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SY2";
			database.LD_LE = enterprise.PK;

			Factory.Save();

			var company = Factory.New<LicenceCompany>();
			company.LC_CompanyCode = "ED9";
			company.LC_OH = org.PK;
			company.LC_LE = enterprise.PK;
			company.LC_CompanyCountry = "AU";

			var licence = Factory.New<LicenceHeader>();
			licence.LA_LC = company.PK;
			licence.LA_LD = database.PK;
			licence.LA_SiteLiveDate = ZDateTime.UtcToday.AddDays(1);

			Factory.Save();

			var expectedResult = new BusinessObject[] { contact };

			var filter = new ModuleCurrentDateOffsetFilter("moo", LicenceHeaderSchema.LA_SiteLiveDate);
			filter.Offset = 2;

			var actualResult = Factory.Load<CampaignContact>(ContactQuery(filter.Query));
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);

			licence.LA_SiteLiveDate = ZDateTime.UtcToday.AddDays(-1);
			Factory.Save();

			actualResult = Factory.Load<CampaignContact>(ContactQuery(filter.Query));
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);

			licence.LA_SiteLiveDate = ZDateTime.UtcToday.AddDays(3);
			Factory.Save();

			expectedResult = System.Array.Empty<BusinessObject>();
			actualResult = Factory.Load<CampaignContact>(ContactQuery(filter.Query));
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, expectedResult, actualResult);
		}

		protected ZQuery ContactQuery(ZQuery innerQuery)
		{
			var query = new ZDBOnlyQuery(typeof(CampaignContact));
			ZDBOnlySubQuery subOrgHeaderQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			ZDBOnlySubQuery subLicenceCompanyQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceCompanySchema.LC_OH);
			ZDBOnlySubQuery subLicenceHeaderSubQuery = new ZDBOnlySubQuery(typeof(LicenceHeader), LicenceHeaderSchema.LA_LC);

			subLicenceHeaderSubQuery.AddToFilter(innerQuery);
			subLicenceCompanyQuery.AddSubQuery(subLicenceHeaderSubQuery, JoinCondition.And);
			subOrgHeaderQuery.AddSubQuery(subLicenceCompanyQuery, JoinCondition.And);
			query.AddSubQuery(ViewCampaignContactSchema.VCC_OH, subOrgHeaderQuery, JoinCondition.And);
			query.AddToFilter(ViewCampaignContactSchema.VCC_ContactName, "Contact Current Date Offset Test");
			return query;
		}

		public void TestTenantIDFilter()
		{
			string code1 = "JN1";
			string code2 = "JN2";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_Email = code1 + "@test.com";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_Email = code2 + "@test.com";

			var enterprise1 = Factory.New<LicenceEnterprise>();
			enterprise1.LE_EnterpriseCode = code1;
			enterprise1.LE_OH = org1.PK;

			var enterprise2 = Factory.New<LicenceEnterprise>();
			enterprise2.LE_EnterpriseCode = code2;
			enterprise2.LE_OH = org2.PK;

			var company1 = enterprise1.Companies.AddNew();
			company1.LC_CompanyCode = code1;
			company1.LC_OH = org1.PK;
			company1.LC_LE = enterprise1.PK;

			var company2 = enterprise1.Companies.AddNew();
			company2.LC_CompanyCode = code2;
			company2.LC_OH = org2.PK;
			company2.LC_LE = enterprise2.PK;

			var database1 = enterprise1.Databases.AddNew();
			database1.LD_ServerCode = code1;
			database1.LD_HostDBName = "ZZZ";
			database1.LD_LicenceType = DatabaseTypes.Codes.Production;
			database1.LD_StaffFirstReportUtc = ZDateTime.Now;
			database1.LD_TenantID = "Ref001";

			var database2 = enterprise2.Databases.AddNew();
			database2.LD_ServerCode = code2;
			database2.LD_HostDBName = "ZZ1";
			database2.LD_LicenceType = DatabaseTypes.Codes.Production;
			database2.LD_TenantID = "Ref002";

			var licence1 = Factory.New<LicenceHeader>();
			licence1.LA_LD = database1.PK;
			licence1.LA_LC = company1.PK;
			licence1.LA_SiteLiveDate = new ZDateTime(2010, 1, 1);

			var licence2 = Factory.New<LicenceHeader>();
			licence2.LA_LD = database2.PK;
			licence2.LA_LC = company2.PK;
			licence2.LA_SiteLiveDate = new ZDateTime(2010, 1, 1);

			Factory.Save();

			var filters = new LicenceFilters(new LicenceKeyFilterBusinessObject(), FilterStripBusinessObject.StatusActive, FilterStripBusinessObject.StatusInactive, FilterStripBusinessObject.StatusAll);
			var filter = filters.FilterStripBusinessObject["Tenant ID"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "ref002";

			var licenceDatabases = Factory.Load<LicenceHeader>(filters.FilterStripBusinessObject.Filter);
			AssertEquals(1, licenceDatabases.Length);
			AssertEquals(licence2.PK, licenceDatabases[0].PK);
		}

		public void TestSystemIDFilter()
		{
			string code1 = "JN1";
			string code2 = "JN2";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_Email = code1 + "@test.com";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_Email = code2 + "@test.com";

			var enterprise1 = Factory.New<LicenceEnterprise>();
			enterprise1.LE_EnterpriseCode = code1;
			enterprise1.LE_OH = org1.PK;

			var enterprise2 = Factory.New<LicenceEnterprise>();
			enterprise2.LE_EnterpriseCode = code2;
			enterprise2.LE_OH = org2.PK;

			var company1 = enterprise1.Companies.AddNew();
			company1.LC_CompanyCode = code1;
			company1.LC_OH = org1.PK;
			company1.LC_LE = enterprise1.PK;

			var company2 = enterprise1.Companies.AddNew();
			company2.LC_CompanyCode = code2;
			company2.LC_OH = org2.PK;
			company2.LC_LE = enterprise2.PK;

			var database1 = enterprise1.Databases.AddNew();
			database1.LD_ServerCode = code1;
			database1.LD_HostDBName = "ZZZ";
			database1.LD_LicenceType = DatabaseTypes.Codes.Production;
			database1.LD_StaffFirstReportUtc = ZDateTime.Now;
			database1.GetOrCreateTrustedSystem().ETS_SystemID = "Ref001";

			var database2 = enterprise2.Databases.AddNew();
			database2.LD_ServerCode = code2;
			database2.LD_HostDBName = "ZZ1";
			database2.LD_LicenceType = DatabaseTypes.Codes.Production;
			database2.GetOrCreateTrustedSystem().ETS_SystemID = "Ref002";

			var licence1 = Factory.New<LicenceHeader>();
			licence1.LA_LD = database1.PK;
			licence1.LA_LC = company1.PK;
			licence1.LA_SiteLiveDate = new ZDateTime(2010, 1, 1);

			var licence2 = Factory.New<LicenceHeader>();
			licence2.LA_LD = database2.PK;
			licence2.LA_LC = company2.PK;
			licence2.LA_SiteLiveDate = new ZDateTime(2010, 1, 1);

			Factory.Save();

			var filters = new LicenceFilters(new LicenceKeyFilterBusinessObject(), FilterStripBusinessObject.StatusActive, FilterStripBusinessObject.StatusInactive, FilterStripBusinessObject.StatusAll);
			var filter = filters.FilterStripBusinessObject["System ID"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "ref002";

			var licenceDatabases = Factory.Load<LicenceHeader>(filters.FilterStripBusinessObject.Filter);
			AssertEquals(1, licenceDatabases.Length);
			AssertEquals(licence2.PK, licenceDatabases[0].PK);
		}

		public void TestDatabaseActiveQuery()
		{
			var filters = new LicenceFilters(new LicenceKeyFilterBusinessObject(), FilterStripBusinessObject.StatusActive, FilterStripBusinessObject.StatusInactive, FilterStripBusinessObject.StatusAll);
			var allLanguages = LanguageHelper.GetDefaultLanguageForOLookUpEditType().Keys.ToList();
			allLanguages.ForEach(lan =>
			{
				using (Res.TemporarilySwitchLanguage(lan))
				{
					var query = filters.GetDatabaseActiveQuery(FilterStripBusinessObject.StatusActive);
					AssertEquals("LD_IsActive = 1", query.LiteralTextADO);

					query = filters.GetDatabaseActiveQuery(FilterStripBusinessObject.StatusInactive);
					AssertEquals("LD_IsActive = 0", query.LiteralTextADO);

					query = filters.GetDatabaseActiveQuery(FilterStripBusinessObject.StatusAll);
					AssertEquals("LD_IsActive = 1 or LD_IsActive = 0", query.LiteralTextADO);
				}
			});
		}

		public void TestLicenceActiveQuery()
		{
			var filters = new LicenceFilters(new LicenceKeyFilterBusinessObject(), FilterStripBusinessObject.StatusActive, FilterStripBusinessObject.StatusInactive, FilterStripBusinessObject.StatusAll);
			var allLanguages = LanguageHelper.GetDefaultLanguageForOLookUpEditType().Keys.ToList();
			allLanguages.ForEach(lan =>
			{
				using (Res.TemporarilySwitchLanguage(lan))
				{
					var query = filters.GetLicenceActiveQuery(FilterStripBusinessObject.StatusActive);
					AssertEquals("LA_IsActive = 1", query.LiteralTextADO);

					query = filters.GetLicenceActiveQuery(FilterStripBusinessObject.StatusInactive);
					AssertEquals("LA_IsActive = 0", query.LiteralTextADO);

					query = filters.GetLicenceActiveQuery(FilterStripBusinessObject.StatusAll);
					AssertEquals("LA_IsActive = 1 or LA_IsActive = 0", query.LiteralTextADO);
				}
			});
		}
	}
}
