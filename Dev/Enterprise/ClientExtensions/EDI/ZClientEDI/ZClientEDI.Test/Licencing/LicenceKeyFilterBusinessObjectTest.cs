using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.Testing;

[TestedType(typeof(LicenceKeyFilterBusinessObject))]
public class LicenceKeyFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
{
	public void TestEnterpriseCodeFilter()
	{
		string code1 = "JN1";
		string code2 = "JN2";

		EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
		var contact1 = org1.Contacts.AddNew();
		contact1.OC_Email = code1 + "@test.com";

		LicenceEnterprise enterprise1 = Factory.New<LicenceEnterprise>();
		enterprise1.LE_EnterpriseCode = code1;
		enterprise1.LE_OH = org1.PK;

		LicenceCompany company1 = enterprise1.Companies.AddNew();
		company1.LC_CompanyCode = code1;
		company1.LC_OH = org1.PK;
		company1.LC_LE = enterprise1.PK;

		LicenceDatabase database1 = enterprise1.Databases.AddNew();
		database1.LD_ServerCode = code1;
		database1.LD_LicenceType = DatabaseTypes.Codes.Production;

		LicenceHeader licence1 = Factory.New<LicenceHeader>();
		licence1.LA_LD = database1.PK;
		licence1.LA_LC = company1.PK;
		licence1.LA_SiteLiveDate = new ZDateTime(2010, 1, 1);

		Factory.Save();

		EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
		var contact2 = org2.Contacts.AddNew();
		contact2.OC_Email = code2 + "@test.com";

		LicenceEnterprise enterprise2 = Factory.New<LicenceEnterprise>();
		enterprise2.LE_EnterpriseCode = code2;
		enterprise2.LE_OH = org2.PK;

		LicenceCompany company2 = enterprise2.Companies.AddNew();
		company2.LC_CompanyCode = code2;
		company2.LC_OH = org2.PK;
		company2.LC_LE = enterprise2.PK;

		LicenceDatabase database2 = enterprise2.Databases.AddNew();
		database2.LD_ServerCode = code2;
		database2.LD_LicenceType = DatabaseTypes.Codes.Production;

		LicenceHeader licence2 = Factory.New<LicenceHeader>();
		licence2.LA_LD = database2.PK;
		licence2.LA_LC = company2.PK;
		licence2.LA_SiteLiveDate = new ZDateTime(2010, 1, 1);

		Factory.Save();

		ModuleGuidFilter filter = (ModuleGuidFilter)FilterBizO["Enterprise Code"];
		filter.Property = enterprise1.PK;
		filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
		filter.IsActive = true;

		LicenceHeaderCollection headers = new LicenceHeaderCollection(Factory);
		headers.Load(FilterBizO.Filter);
		Assert("Collection should contain JN1", headers.Contains(licence1));
		Assert("Collection should not contain JN2", !headers.Contains(licence2));

		filter.Property = ZGuid.Empty;

		filter.Property = enterprise2.PK;
		filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
		filter.IsActive = true;
		headers.Load(FilterBizO.Filter);
		Assert("Collection should not contain JN1", !headers.Contains(licence2));
		Assert("Collection should contain JN2", headers.Contains(licence1));
	}

	public void TestEnterpriseIDilter()
	{
		string code1 = "JN1";
		string code2 = "JN2";

		EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
		var contact1 = org1.Contacts.AddNew();
		contact1.OC_Email = code1 + "@test.com";

		LicenceEnterprise enterprise1 = Factory.New<LicenceEnterprise>();
		enterprise1.LE_EnterpriseCode = code1;
		enterprise1.LE_OH = org1.PK;

		LicenceCompany company1 = enterprise1.Companies.AddNew();
		company1.LC_CompanyCode = code1;
		company1.LC_OH = org1.PK;
		company1.LC_LE = enterprise1.PK;

		LicenceDatabase database1 = enterprise1.Databases.AddNew();
		database1.LD_ServerCode = code1;
		database1.LD_LicenceType = DatabaseTypes.Codes.Production;

		LicenceHeader licence1 = Factory.New<LicenceHeader>();
		licence1.LA_LD = database1.PK;
		licence1.LA_LC = company1.PK;
		licence1.LA_SiteLiveDate = new ZDateTime(2010, 1, 1);

		Factory.Save();

		EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
		var contact2 = org2.Contacts.AddNew();
		contact2.OC_Email = code2 + "@test.com";

		LicenceEnterprise enterprise2 = Factory.New<LicenceEnterprise>();
		enterprise2.LE_EnterpriseCode = code2;
		enterprise2.LE_OH = org2.PK;

		LicenceCompany company2 = enterprise2.Companies.AddNew();
		company2.LC_CompanyCode = code2;
		company2.LC_OH = org2.PK;
		company2.LC_LE = enterprise2.PK;

		LicenceDatabase database2 = enterprise2.Databases.AddNew();
		database2.LD_ServerCode = code2;
		database2.LD_LicenceType = DatabaseTypes.Codes.Production;

		LicenceHeader licence2 = Factory.New<LicenceHeader>();
		licence2.LA_LD = database2.PK;
		licence2.LA_LC = company2.PK;
		licence2.LA_SiteLiveDate = new ZDateTime(2010, 1, 1);

		Factory.Save();

		ModuleGuidFilter filter = (ModuleGuidFilter)FilterBizO["Enterprise ID"];
		filter.Property = enterprise1.PK;
		filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
		filter.IsActive = true;

		LicenceHeaderCollection headers = new LicenceHeaderCollection(Factory);
		headers.Load(FilterBizO.Filter);
		Assert("Collection should contain JN1", headers.Contains(licence1));
		Assert("Collection should not contain JN2", !headers.Contains(licence2));

		filter.Property = ZGuid.Empty;

		filter.Property = enterprise2.PK;
		filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
		filter.IsActive = true;
		headers.Load(FilterBizO.Filter);
		Assert("Collection should not contain JN1", !headers.Contains(licence2));
		Assert("Collection should contain JN2", headers.Contains(licence1));
	}

	#region Database Server Name

	public void TestLicenceKeyDatabaseServerNameFilter()
	{
		string code1 = "JN1";
		string code2 = "JN2";

		EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
		var contact1 = org1.Contacts.AddNew();
		contact1.OC_Email = code1 + "@test.com";

		LicenceEnterprise enterprise1 = Factory.New<LicenceEnterprise>();
		enterprise1.LE_EnterpriseCode = code1;
		enterprise1.LE_OH = org1.PK;

		LicenceCompany company1 = enterprise1.Companies.AddNew();
		company1.LC_CompanyCode = code1;
		company1.LC_OH = org1.PK;
		company1.LC_LE = enterprise1.PK;

		LicenceDatabase database1 = enterprise1.Databases.AddNew();
		database1.LD_ServerCode = code1;
		database1.LD_ReportedHostServerName = "ZZZ";
		database1.LD_LicenceType = DatabaseTypes.Codes.Production;

		LicenceHeader licence1 = Factory.New<LicenceHeader>();
		licence1.LA_LD = database1.PK;
		licence1.LA_LC = company1.PK;
		licence1.LA_SiteLiveDate = new ZDateTime(2010, 1, 1);

		Factory.Save();

		EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
		var contact2 = org2.Contacts.AddNew();
		contact2.OC_Email = code2 + "@test.com";

		LicenceEnterprise enterprise2 = Factory.New<LicenceEnterprise>();
		enterprise2.LE_EnterpriseCode = code2;
		enterprise2.LE_OH = org2.PK;

		LicenceCompany company2 = enterprise2.Companies.AddNew();
		company2.LC_CompanyCode = code2;
		company2.LC_OH = org2.PK;
		company2.LC_LE = enterprise2.PK;

		LicenceDatabase database2 = enterprise2.Databases.AddNew();
		database2.LD_ServerCode = code2;
		database2.LD_ReportedHostServerName = "XZY";
		database2.LD_LicenceType = DatabaseTypes.Codes.Production;

		LicenceHeader licence2 = Factory.New<LicenceHeader>();
		licence2.LA_LD = database2.PK;
		licence2.LA_LC = company2.PK;
		licence2.LA_SiteLiveDate = new ZDateTime(2010, 1, 1);

		Factory.Save();

		ModuleTextFilter filter = (ModuleTextFilter)FilterBizO["Database Server Name"];

		filter.Property = "Z";
		filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
		filter.IsActive = true;

		LicenceHeaderCollection headers = new LicenceHeaderCollection(Factory);
		headers.Load(FilterBizO.Filter);
		Assert("Collection should contain JN1", headers.Contains(licence1));
		Assert("Collection should not contain JN2", !headers.Contains(licence2));

		filter.Property = "X";
		filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
		filter.IsActive = true;
		headers.Load(FilterBizO.Filter);
		Assert("Collection should not contain JN1", !headers.Contains(licence1));
		Assert("Collection should contain JN2", headers.Contains(licence2));
	}

	#endregion

	#region Database Name

	public void TestLicenceKeyDatabaseNameFilter()
	{
		string code1 = "JN1";
		string code2 = "JN2";

		EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
		var contact1 = org1.Contacts.AddNew();
		contact1.OC_Email = code1 + "@test.com";

		LicenceEnterprise enterprise1 = Factory.New<LicenceEnterprise>();
		enterprise1.LE_EnterpriseCode = code1;
		enterprise1.LE_OH = org1.PK;

		LicenceCompany company1 = enterprise1.Companies.AddNew();
		company1.LC_CompanyCode = code1;
		company1.LC_OH = org1.PK;
		company1.LC_LE = enterprise1.PK;

		LicenceDatabase database1 = enterprise1.Databases.AddNew();
		database1.LD_ServerCode = code1;
		database1.LD_ReportedHostDBName = "ZZZ";
		database1.LD_LicenceType = DatabaseTypes.Codes.Production;

		LicenceHeader licence1 = Factory.New<LicenceHeader>();
		licence1.LA_LD = database1.PK;
		licence1.LA_LC = company1.PK;
		licence1.LA_SiteLiveDate = new ZDateTime(2010, 1, 1);

		Factory.Save();

		EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
		var contact2 = org2.Contacts.AddNew();
		contact2.OC_Email = code2 + "@test.com";

		LicenceEnterprise enterprise2 = Factory.New<LicenceEnterprise>();
		enterprise2.LE_EnterpriseCode = code2;
		enterprise2.LE_OH = org2.PK;

		LicenceCompany company2 = enterprise2.Companies.AddNew();
		company2.LC_CompanyCode = code2;
		company2.LC_OH = org2.PK;
		company2.LC_LE = enterprise2.PK;

		LicenceDatabase database2 = enterprise2.Databases.AddNew();
		database2.LD_ServerCode = code2;
		database2.LD_ReportedHostDBName = "XZY";
		database2.LD_LicenceType = DatabaseTypes.Codes.Production;

		LicenceHeader licence2 = Factory.New<LicenceHeader>();
		licence2.LA_LD = database2.PK;
		licence2.LA_LC = company2.PK;
		licence2.LA_SiteLiveDate = new ZDateTime(2010, 1, 1);

		Factory.Save();

		ModuleTextFilter filter = (ModuleTextFilter)FilterBizO["Database Name"];
		filter.Property = "Z";
		filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
		filter.IsActive = true;

		LicenceHeaderCollection headers = new LicenceHeaderCollection(Factory);
		headers.Load(FilterBizO.Filter);
		Assert("Collection should contain JN1", headers.Contains(licence1));
		Assert("Collection should not contain JN2", !headers.Contains(licence2));

		filter.Property = "X";
		filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
		filter.IsActive = true;
		headers.Load(FilterBizO.Filter);
		Assert("Collection should not contain JN1", !headers.Contains(licence1));
		Assert("Collection should contain JN2", headers.Contains(licence2));
	}

	#endregion

	#region Database Connection Server

	public void TestLicenceKeyDatabaseConnectionServerFilter()
	{
		string code1 = "JN1";
		string code2 = "JN2";

		EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
		var contact1 = org1.Contacts.AddNew();
		contact1.OC_Email = code1 + "@test.com";

		LicenceEnterprise enterprise1 = Factory.New<LicenceEnterprise>();
		enterprise1.LE_EnterpriseCode = code1;
		enterprise1.LE_OH = org1.PK;

		LicenceCompany company1 = enterprise1.Companies.AddNew();
		company1.LC_CompanyCode = code1;
		company1.LC_OH = org1.PK;
		company1.LC_LE = enterprise1.PK;

		LicenceDatabase database1 = enterprise1.Databases.AddNew();
		database1.LD_ServerCode = code1;
		database1.LD_HostConnectionServerName = "ZZZ";
		database1.LD_LicenceType = DatabaseTypes.Codes.Production;

		LicenceHeader licence1 = Factory.New<LicenceHeader>();
		licence1.LA_LD = database1.PK;
		licence1.LA_LC = company1.PK;
		licence1.LA_SiteLiveDate = new ZDateTime(2010, 1, 1);

		Factory.Save();

		EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
		var contact2 = org2.Contacts.AddNew();
		contact2.OC_Email = code2 + "@test.com";

		LicenceEnterprise enterprise2 = Factory.New<LicenceEnterprise>();
		enterprise2.LE_EnterpriseCode = code2;
		enterprise2.LE_OH = org2.PK;

		LicenceCompany company2 = enterprise2.Companies.AddNew();
		company2.LC_CompanyCode = code2;
		company2.LC_OH = org2.PK;
		company2.LC_LE = enterprise2.PK;

		LicenceDatabase database2 = enterprise2.Databases.AddNew();
		database2.LD_ServerCode = code2;
		database2.LD_HostConnectionServerName = "XZY";
		database2.LD_LicenceType = DatabaseTypes.Codes.Production;

		LicenceHeader licence2 = Factory.New<LicenceHeader>();
		licence2.LA_LD = database2.PK;
		licence2.LA_LC = company2.PK;
		licence2.LA_SiteLiveDate = new ZDateTime(2010, 1, 1);

		Factory.Save();

		ModuleTextFilter filter = (ModuleTextFilter)FilterBizO["Database Connection Server"];
		filter.Property = "Z";
		filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
		filter.IsActive = true;

		LicenceHeaderCollection headers = new LicenceHeaderCollection(Factory);
		headers.Load(FilterBizO.Filter);
		Assert("Collection should contain JN1", headers.Contains(licence1));
		Assert("Collection should not contain JN2", !headers.Contains(licence2));

		//filter.Property = ZGuid.Empty;

		filter.Property = "X";
		filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
		filter.IsActive = true;
		headers.Load(FilterBizO.Filter);
		Assert("Collection should not contain JN1", !headers.Contains(licence1));
		Assert("Collection should contain JN2", headers.Contains(licence2));
	}

	#endregion

	#region Database Registration

	public void TestLicenceKeyDatabaseRegistrationFilter()
	{
		string code1 = "JN1";
		string code2 = "JN2";
		string code3 = "JN3";

		// No. 1
		EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
		var contact1 = org1.Contacts.AddNew();
		contact1.OC_Email = code1 + "@test.com";

		LicenceEnterprise enterprise1 = Factory.New<LicenceEnterprise>();
		enterprise1.LE_EnterpriseCode = code1;
		enterprise1.LE_OH = org1.PK;

		LicenceCompany company1 = enterprise1.Companies.AddNew();
		company1.LC_CompanyCode = code1;
		company1.LC_OH = org1.PK;
		company1.LC_LE = enterprise1.PK;

		LicenceDatabase database1 = enterprise1.Databases.AddNew();
		database1.LD_ServerCode = code1;
		database1.LD_Status = DatabaseStatusList.Codes.NON;
		database1.LD_LicenceType = DatabaseTypes.Codes.Production;

		LicenceHeader licence1 = Factory.New<LicenceHeader>();
		licence1.LA_LD = database1.PK;
		licence1.LA_LC = company1.PK;
		licence1.LA_SiteLiveDate = new ZDateTime(2010, 1, 1);

		Factory.Save();

		// No. 2
		EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
		var contact2 = org2.Contacts.AddNew();
		contact2.OC_Email = code2 + "@test.com";

		LicenceEnterprise enterprise2 = Factory.New<LicenceEnterprise>();
		enterprise2.LE_EnterpriseCode = code2;
		enterprise2.LE_OH = org2.PK;

		LicenceCompany company2 = enterprise2.Companies.AddNew();
		company2.LC_CompanyCode = code2;
		company2.LC_OH = org2.PK;
		company2.LC_LE = enterprise2.PK;

		LicenceDatabase database2 = enterprise2.Databases.AddNew();
		database2.LD_ServerCode = code2;
		database2.LD_Status = DatabaseStatusList.Codes.REG;
		database2.LD_LicenceType = DatabaseTypes.Codes.Production;

		LicenceHeader licence2 = Factory.New<LicenceHeader>();
		licence2.LA_LD = database2.PK;
		licence2.LA_LC = company2.PK;
		licence2.LA_SiteLiveDate = new ZDateTime(2010, 1, 1);

		Factory.Save();

		// No. 3
		EDIOrgHeader org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
		var contact3 = org3.Contacts.AddNew();
		contact3.OC_Email = code3 + "@test.com";

		LicenceEnterprise enterprise3 = Factory.New<LicenceEnterprise>();
		enterprise3.LE_EnterpriseCode = code3;
		enterprise3.LE_OH = org3.PK;

		LicenceCompany company3 = enterprise3.Companies.AddNew();
		company3.LC_CompanyCode = code3;
		company3.LC_OH = org3.PK;
		company3.LC_LE = enterprise3.PK;

		LicenceDatabase database3 = enterprise3.Databases.AddNew();
		database3.LD_ServerCode = code3;
		database3.LD_Status = DatabaseStatusList.Codes.Preregistered;
		database3.LD_LicenceType = DatabaseTypes.Codes.Production;

		LicenceHeader licence3 = Factory.New<LicenceHeader>();
		licence3.LA_LD = database3.PK;
		licence3.LA_LC = company3.PK;
		licence3.LA_SiteLiveDate = new ZDateTime(2010, 1, 1);

		Factory.Save();

		ModuleTextFilter filter = (ModuleTextFilter)FilterBizO["Database Registration"];
		filter.Property = DatabaseStatusList.Codes.NON;
		filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
		filter.IsActive = true;

		LicenceHeaderCollection headers = new LicenceHeaderCollection(Factory);
		headers.Load(FilterBizO.Filter);
		Assert("Collection should contain JN1", headers.Contains(licence1));
		Assert("Collection should not contain JN2", !headers.Contains(licence2));
		Assert("Collection should not contain JN3", !headers.Contains(licence3));

		filter.Property = DatabaseStatusList.Codes.REG;
		filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
		filter.IsActive = true;
		headers.Load(FilterBizO.Filter);
		Assert("Collection should contain JN1", headers.Contains(licence1));
		Assert("Collection should not contain JN2", !headers.Contains(licence2));
		Assert("Collection should contain JN2", headers.Contains(licence3));
	}

	#endregion

	#region Database System Info Group

	public void TestLicenceKeyBelongToDatabaseSystemInfoGroup()
	{
		Assert("Expect category is databaseSystemInfo", FilterBizO["Database Server Name"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", FilterBizO["Database Name"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", FilterBizO["Database Connection Server"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", FilterBizO["Database Registration"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", FilterBizO["OS Name (Free Text)"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", FilterBizO["OS Name"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", FilterBizO["OS Version (Free Text)"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", FilterBizO["OS Version"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", FilterBizO["System Manufacturer (Free Text)"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", FilterBizO["System Manufacturer"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", FilterBizO["BIOS Release Date"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", FilterBizO["Total Physical Memory in MB"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", FilterBizO["Number of Processors"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", FilterBizO["Processor Type (Free Text)"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", FilterBizO["Processor Type"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", FilterBizO["Processor Speed in Mhz"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", FilterBizO["Is Virtual Machine"].Category.ToString() == "Database System Info");
	}

	#endregion

	FilterStripBusinessObject FilterBizO
	{
		get
		{
			if (fFilterBizO == null)
			{
				fFilterBizO = GetNewFilterStripBusinessObject();
			}
			return fFilterBizO;
		}
	}
	FilterStripBusinessObject fFilterBizO;
	public void TestFilters()
	{
		OrgHeader org = Factory.New<OrgHeader>();
		org.OH_Code = "ZUBIN";
		org.OH_FullName = "Zubs Organisation";
		OrgHeader org2 = Factory.New<OrgHeader>();
		org2.OH_Code = "ZUBI2";
		org2.OH_FullName = "Zub2 Organisation";
		Factory.Save();

		LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
		enterprise.LE_EnterpriseCode = "ENT";
		enterprise.LE_OH = org.PK;
		LicenceEnterprise enterprise2 = Factory.New<LicenceEnterprise>();
		enterprise2.LE_EnterpriseCode = "EN2";
		enterprise2.LE_OH = org2.PK;
		Factory.Save();

		// No. 1
		LicenceCompany company = Factory.New<LicenceCompany>();
		company.LC_CompanyCode = "COM";
		company.LC_LE = enterprise.PK;
		company.LC_OH = org.PK;

		LicenceDatabase database = Factory.New<LicenceDatabase>();
		database.LD_ServerCode = "SRV";
		database.LD_LE = enterprise.PK;

		LicenceHeader header = Factory.New<LicenceHeader>();
		header.LA_LC = company.PK;
		header.LA_LD = database.PK;

		// No. 2
		LicenceCompany company2 = Factory.New<LicenceCompany>();
		company2.LC_CompanyCode = "CO2";
		company2.LC_LE = enterprise2.PK;
		company2.LC_OH = org2.PK;

		LicenceDatabase database2 = Factory.New<LicenceDatabase>();
		database2.LD_ServerCode = "SR2";
		database2.LD_LE = enterprise2.PK;

		LicenceHeader header2 = Factory.New<LicenceHeader>();
		header2.LA_LC = company2.PK;
		header2.LA_LD = database2.PK;

		// No. 3
		LicenceCompany company3 = Factory.New<LicenceCompany>();
		company3.LC_CompanyCode = "CO3";

		OrgHeader org3 = Factory.New<OrgHeader>();
		org3.OH_Code = "ZUBI3";
		org3.OH_FullName = "Zub3 Organisation";

		company3.LC_LE = enterprise2.PK;
		company3.LC_OH = org3.PK;

		LicenceDatabase database3 = Factory.New<LicenceDatabase>();
		database3.LD_ServerCode = "SR3";
		database3.LD_LE = enterprise2.PK;

		LicenceHeader header3 = Factory.New<LicenceHeader>();
		header3.LA_LC = company3.PK;
		header3.LA_LD = database3.PK;

		Factory.Save();

		LicenceHeaderFilterBusinessObject filterBizo = new LicenceHeaderFilterBusinessObject();

		((ModuleTextFilter)filterBizo["Organisation Name"]).Property = "Zub";
		((ModuleTextFilter)filterBizo["Organisation Name"]).IsActive = true;
		LicenceHeaderCollection headers = new LicenceHeaderCollection(Factory);
		headers.Load(filterBizo.Filter);
		AssertEquals("All items found", 3, headers.Count);

		((ModuleTextFilter)filterBizo["Organisation Name"]).IsActive = false;
		((ModuleGuidFilter)filterBizo["Organisation"]).Property = org2.PK;
		((ModuleGuidFilter)filterBizo["Organisation"]).IsActive = true;
		headers.Load(filterBizo.Filter);
		AssertEquals("2nd found", 1, headers.Count);
		AssertEquals("2nd found", header2.PK, headers[0].PK);

		((ModuleGuidFilter)filterBizo["Organisation"]).IsActive = false;
		((ModuleGuidFilter)filterBizo["Enterprise Code"]).Property = enterprise2.PK;
		((ModuleGuidFilter)filterBizo["Enterprise Code"]).IsActive = true;
		headers.Load(filterBizo.Filter);
		AssertEquals("2nd and 3rd found", 2, headers.Count);
		AssertCollectionContains(header2, headers);
		AssertCollectionContains(header3, headers);

		((ModuleGuidFilter)filterBizo["Enterprise Code"]).Property = enterprise.PK;
		headers.Load(filterBizo.Filter);
		AssertEquals("1st found", 1, headers.Count);
		AssertCollectionContains(header, headers);
	}

	public void TestCoreFilters()
	{
		OrgHeader org = Factory.New<OrgHeader>();
		org.OH_Code = "ZUBIN";
		org.OH_FullName = "Zubs Organisation";
		OrgHeader org2 = Factory.New<OrgHeader>();
		org2.OH_Code = "ZUBI2";
		org2.OH_FullName = "Zub2 Organisation";
		OrgHeader org3 = Factory.New<OrgHeader>();
		org3.OH_Code = "ZUBI3";
		org3.OH_FullName = "Zub3 Organisation";
		Factory.Save();

		LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
		enterprise.LE_EnterpriseCode = "ENT";
		enterprise.LE_OH = org.PK;
		LicenceEnterprise enterprise2 = Factory.New<LicenceEnterprise>();
		enterprise2.LE_EnterpriseCode = "EN2";
		enterprise2.LE_OH = org2.PK;
		LicenceEnterprise enterprise3 = Factory.New<LicenceEnterprise>();
		enterprise3.LE_EnterpriseCode = "EN3";
		enterprise3.LE_OH = org3.PK;
		Factory.Save();

		// No. 1
		LicenceCompany company = Factory.New<LicenceCompany>();
		company.LC_CompanyCode = "COM";
		company.LC_LE = enterprise.PK;
		company.LC_OH = org.PK;
		LicenceDatabase database = Factory.New<LicenceDatabase>();
		database.LD_ServerCode = "SRV";
		database.LD_LE = enterprise.PK;

		// No. 2
		LicenceCompany company2 = Factory.New<LicenceCompany>();
		company2.LC_CompanyCode = "CO2";
		company2.LC_LE = enterprise2.PK;
		company2.LC_OH = org2.PK;
		LicenceDatabase database2 = Factory.New<LicenceDatabase>();
		database2.LD_ServerCode = "SR2";
		database2.LD_LE = enterprise2.PK;

		// No. 2
		LicenceCompany company3 = Factory.New<LicenceCompany>();
		company3.LC_CompanyCode = "CO3";
		company3.LC_LE = enterprise3.PK;
		company3.LC_OH = org3.PK;
		LicenceDatabase database3 = Factory.New<LicenceDatabase>();
		database3.LD_ServerCode = "SR3";
		database3.LD_LE = enterprise3.PK;

		LicenceHeader header1 = Factory.New<LicenceHeader>();
		header1.LA_LC = company.PK;
		header1.LA_LD = database.PK;
		LicenceModules licModules1 = Factory.New<LicenceModules>();
		licModules1.LM_LA = header1.PK;
		licModules1.LM_GroupModuleCode = "COR";
		licModules1.LM_LicenceType = "XXX";
		licModules1.LM_UserCount = 15;

		LicenceHeader header2 = Factory.New<LicenceHeader>();
		header2.LA_LC = company2.PK;
		header2.LA_LD = database2.PK;
		LicenceModules licModules2 = Factory.New<LicenceModules>();
		licModules2.LM_LA = header2.PK;
		licModules2.LM_GroupModuleCode = "COR";
		licModules2.LM_LicenceType = "ABC";
		licModules2.LM_UserCount = 10;

		LicenceHeader header3 = Factory.New<LicenceHeader>();
		header3.LA_LC = company3.PK;
		header3.LA_LD = database3.PK;
		LicenceModules licModules3 = Factory.New<LicenceModules>();
		licModules3.LM_LA = header3.PK;
		licModules3.LM_GroupModuleCode = "FRE"; // is ignored in Core-Fee-Basis and Core-Set-Count filter
		licModules3.LM_LicenceType = "XXX";
		licModules3.LM_UserCount = 10;

		Factory.Save();

		LicenceKeyFilterBusinessObject filterBizo = new LicenceKeyFilterBusinessObject();

		((ModuleTextFilter)filterBizo["Organisation Name"]).Property = "Zub";

		((ModuleTextFilter)filterBizo["Core Fee Basis"]).Property = "Zub";
		((ModuleTextFilter)filterBizo["Core Fee Basis"]).IsActive = true;
		LicenceHeaderCollection headers = new LicenceHeaderCollection(Factory);
		headers.Load(filterBizo.Filter);
		AssertEquals("NO items found", 0, headers.Count);

		((ModuleTextFilter)filterBizo["Core Fee Basis"]).Property = "XXX";
		((ModuleTextFilter)filterBizo["Core Fee Basis"]).IsActive = true;
		headers.Load(filterBizo.Filter);
		AssertEquals("1 items found", 1, headers.Count);

		((ModuleTextFilter)filterBizo["Core Fee Basis"]).IsActive = false;
		ModuleNumberRangeFilter coreSeatCountFilter = (ModuleNumberRangeFilter)filterBizo["Core Seat Count"];
		coreSeatCountFilter.IsActive = true;
		coreSeatCountFilter.Property1 = 9;
		coreSeatCountFilter.Property2 = 11;
		headers.Load(filterBizo.Filter);
		AssertEquals("1 items found", 1, headers.Count);
	}

	public void TestProductFilter()
	{
		var list = new SystemProductCollection();

		list.AddNew("ERD", "EntityRD", true);
		list.AddNew("EMP", "ElectricMP", true);
		list.AddNew("RPG", "RapidGP", true);
		EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

		LicenceHeader header1 = Factory.NewWithValidTestData<LicenceHeader>();
		header1.Database.LD_Product = "ERD";

		LicenceHeader header2 = Factory.NewWithValidTestData<LicenceHeader>();
		header2.Database.LD_Product = "EMP";

		LicenceHeader header3 = Factory.NewWithValidTestData<LicenceHeader>();
		header3.Database.LD_Product = "RPG";

		Factory.Save();

		var licenceKeyFilter = new LicenceKeyFilterBusinessObject();
		var productFilter = (ModuleTextFilter)licenceKeyFilter["Product"];
		var productCollection = new LicenceHeaderCollection(Factory);

		productFilter.Property = "EMP";
		productFilter.IsActive = true;
		productCollection.Load(licenceKeyFilter.Filter);

		Assert("Should only contain 'EMP' Products", !productCollection.Contains(header1.PK));
		Assert("Should only contain 'EMP' Products", productCollection.Contains(header2.PK));
		Assert("Should only contain 'EMP' Products", !productCollection.Contains(header3.PK));

		productFilter.Property = "ERD";
		productCollection.Load(licenceKeyFilter.Filter);

		Assert("Should only contain 'ERD' Products", productCollection.Contains(header1.PK));
		Assert("Should only contain 'ERD' Products", !productCollection.Contains(header2.PK));
		Assert("Should only contain 'ERD' Products", !productCollection.Contains(header3.PK));

		productFilter.Property = "RPG";
		productFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
		productCollection.Load(licenceKeyFilter.Filter);

		Assert("Should only contain 'RPG' Products", productCollection.Contains(header1.PK));
		Assert("Should only contain 'RPG' Products", productCollection.Contains(header2.PK));
		Assert("Should only contain 'RPG' Products", !productCollection.Contains(header3.PK));
	}

	public void TestTagNoteFilter()
	{
		var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
		var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB");
		var lic3 = BillingTestHelper.CreateLicence(Factory, "CCC");
		var licWithoutBilling = BillingTestHelper.CreateLicence(Factory, "ZZZ");
		lic1.Billing.L0_TagNote = "";
		lic2.Billing.L0_TagNote = "Note1";
		lic3.Billing.L0_TagNote = "Note2";

		Factory.Save();

		LicenceKeyFilterBusinessObject filterBizo = new LicenceKeyFilterBusinessObject();
		var tagFilter = ((ModuleTextFilter)filterBizo["Tag/Note"]);
		tagFilter.IsActive = true;
		tagFilter.Property = "Note1";
		tagFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
		LicenceHeaderCollection headers = new LicenceHeaderCollection(Factory);
		headers.Load(filterBizo.Filter);
		AssertEquals("items found", 1, headers.Count);
		AssertEquals("lic2", lic2.PK, headers[0].PK);

		tagFilter.Property = "Note";
		tagFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
		headers = new LicenceHeaderCollection(Factory);
		headers.Load(filterBizo.Filter);
		AssertEquals("items found", 2, headers.Count);
		Assert("lic2", headers.Contains(lic2));
		Assert("lic3", headers.Contains(lic3));

		tagFilter.Property = "";
		tagFilter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
		headers.Load(filterBizo.Filter);
		AssertEquals("items found", 1, headers.Count);
		Assert("lic1", headers.Contains(lic1));
	}

	public void TestFilterLicenceCountry()
	{
		LicenceHeader company1 = Factory.NewWithValidTestData<LicenceHeader>();
		company1.Company.LC_CompanyCountry = "AU";

		LicenceHeader company2 = Factory.NewWithValidTestData<LicenceHeader>();
		company2.Company.LC_CompanyCountry = "NZ";

		Factory.Save();

		var licenceKeyFilter = new LicenceKeyFilterBusinessObject();
		var filter = (ModuleNkFilter)licenceKeyFilter["Licence Country"];
		var collection = new LicenceHeaderCollection(Factory);

		filter.Property = "AU";
		filter.IsActive = true;
		collection.Load(licenceKeyFilter.Filter);

		Assert("Should return company 1", collection.Contains(company1.PK));
		Assert("Should not return company 2", !collection.Contains(company2.PK));

		filter.Property = "NZ";
		collection.Load(licenceKeyFilter.Filter);

		Assert("Should not return company 1", !collection.Contains(company1.PK));
		Assert("Should return company 2", collection.Contains(company2.PK));

		filter.Property = "SB";
		collection.Load(licenceKeyFilter.Filter);

		Assert("Should return company 1", !collection.Contains(company1.PK));
		Assert("Should return company 2", !collection.Contains(company2.PK));
	}

	public void TestFilterReciprocalRates()
	{
		LicenceHeader company1 = Factory.NewWithValidTestData<LicenceHeader>();
		company1.Company.LC_IsReciprocal = true;

		LicenceHeader company2 = Factory.NewWithValidTestData<LicenceHeader>();
		company2.Company.LC_IsReciprocal = false;

		Factory.Save();

		var licenceKeyFilter = new LicenceKeyFilterBusinessObject();
		var filter = (ModuleFlagsFilter)licenceKeyFilter["Reciprocal Rates"];
		var collection = new LicenceHeaderCollection(Factory);

		filter.Property0 = true;
		filter.IsActive = true;
		collection.Load(licenceKeyFilter.Filter);

		Assert("Should return company 1", collection.Contains(company1.PK));
		Assert("Should not return company 2", !collection.Contains(company2.PK));

		filter.Property0 = false;
		collection.Load(licenceKeyFilter.Filter);

		Assert("Should not return company 1", !collection.Contains(company1.PK));
		Assert("Should return company 2", collection.Contains(company2.PK));
	}

	public void TestFilterGSTRegistered()
	{
		LicenceHeader company1 = Factory.NewWithValidTestData<LicenceHeader>();
		company1.Company.LC_IsGSTRegistered = true;

		LicenceHeader company2 = Factory.NewWithValidTestData<LicenceHeader>();
		company2.Company.LC_IsGSTRegistered = false;

		Factory.Save();

		var licenceKeyFilter = new LicenceKeyFilterBusinessObject();
		var filter = (ModuleFlagsFilter)licenceKeyFilter["GST Registered"];
		var collection = new LicenceHeaderCollection(Factory);

		filter.Property0 = true;
		filter.IsActive = true;
		collection.Load(licenceKeyFilter.Filter);

		Assert("Should return company 1", collection.Contains(company1.PK));
		Assert("Should not return company 2", !collection.Contains(company2.PK));

		filter.Property0 = false;
		collection.Load(licenceKeyFilter.Filter);

		Assert("Should not return company 1", !collection.Contains(company1.PK));
		Assert("Should return company 2", collection.Contains(company2.PK));
	}

	public void TestFilterGSTCashEnabled()
	{
		LicenceHeader company1 = Factory.NewWithValidTestData<LicenceHeader>();
		company1.Company.LC_IsGSTCashBasis = true;

		LicenceHeader company2 = Factory.NewWithValidTestData<LicenceHeader>();
		company2.Company.LC_IsGSTCashBasis = false;

		Factory.Save();

		var licenceKeyFilter = new LicenceKeyFilterBusinessObject();
		var filter = (ModuleFlagsFilter)licenceKeyFilter["GST Cash Enabled"];
		var collection = new LicenceHeaderCollection(Factory);

		filter.Property0 = true;
		filter.IsActive = true;
		collection.Load(licenceKeyFilter.Filter);

		Assert("Should return company 1", collection.Contains(company1.PK));
		Assert("Should not return company 2", !collection.Contains(company2.PK));

		filter.Property0 = false;
		collection.Load(licenceKeyFilter.Filter);

		Assert("Should not return company 1", !collection.Contains(company1.PK));
		Assert("Should return company 2", collection.Contains(company2.PK));
	}

	public void TestFilterWithholdingRegistered()
	{
		LicenceHeader company1 = Factory.NewWithValidTestData<LicenceHeader>();
		company1.Company.LC_IsWHTRegistered = true;

		LicenceHeader company2 = Factory.NewWithValidTestData<LicenceHeader>();
		company2.Company.LC_IsWHTRegistered = false;

		Factory.Save();

		var licenceKeyFilter = new LicenceKeyFilterBusinessObject();
		var filter = (ModuleFlagsFilter)licenceKeyFilter["Withholding Registered"];
		var collection = new LicenceHeaderCollection(Factory);

		filter.Property0 = true;
		filter.IsActive = true;
		collection.Load(licenceKeyFilter.Filter);

		Assert("Should return company 1", collection.Contains(company1.PK));
		Assert("Should not return company 2", !collection.Contains(company2.PK));

		filter.Property0 = false;
		collection.Load(licenceKeyFilter.Filter);

		Assert("Should not return company 1", !collection.Contains(company1.PK));
		Assert("Should return company 2", collection.Contains(company2.PK));
	}

	public void TestFilterWithholdingCashBasis()
	{
		LicenceHeader company1 = Factory.NewWithValidTestData<LicenceHeader>();
		company1.Company.LC_IsWHTCashBasis = true;

		LicenceHeader company2 = Factory.NewWithValidTestData<LicenceHeader>();
		company2.Company.LC_IsWHTCashBasis = false;

		Factory.Save();

		var licenceKeyFilter = new LicenceKeyFilterBusinessObject();
		var filter = (ModuleFlagsFilter)licenceKeyFilter["Withholding Cash Basis"];
		var collection = new LicenceHeaderCollection(Factory);

		filter.Property0 = true;
		filter.IsActive = true;
		collection.Load(licenceKeyFilter.Filter);

		Assert("Should return company 1", collection.Contains(company1.PK));
		Assert("Should not return company 2", !collection.Contains(company2.PK));

		filter.Property0 = false;
		collection.Load(licenceKeyFilter.Filter);

		Assert("Should not return company 1", !collection.Contains(company1.PK));
		Assert("Should return company 2", collection.Contains(company2.PK));
	}

	public void TestFilterCurrency()
	{
		LicenceHeader company1 = Factory.NewWithValidTestData<LicenceHeader>();
		company1.Company.LC_RX_NKCurrency = "AUD";

		LicenceHeader company2 = Factory.NewWithValidTestData<LicenceHeader>();
		company2.Company.LC_RX_NKCurrency = "XYZ";

		Factory.Save();

		var licenceKeyFilter = new LicenceKeyFilterBusinessObject();
		var filter = (ModuleNkFilter)licenceKeyFilter["Currency"];
		var collection = new LicenceHeaderCollection(Factory);

		filter.Property = "AUD";
		filter.IsActive = true;
		collection.Load(licenceKeyFilter.Filter);

		Assert("Should return company 1", collection.Contains(company1.PK));
		Assert("Should not return company 2", !collection.Contains(company2.PK));

		filter.Property = "XYZ";
		collection.Load(licenceKeyFilter.Filter);

		Assert("Should not return company 1", !collection.Contains(company1.PK));
		Assert("Should return company 2", collection.Contains(company2.PK));

		filter.Property = "NUP";
		collection.Load(licenceKeyFilter.Filter);

		Assert("Should return company 1", !collection.Contains(company1.PK));
		Assert("Should return company 2", !collection.Contains(company2.PK));
	}

	public void TestOrganisationActiveStatusFilter()
	{
		var licenceKeyFilter = new LicenceKeyFilterBusinessObject();
		var filter = (ModuleTextFilter)licenceKeyFilter["Organisation Active"];
		filter.IsActive = true;
		AllLanguages.ForEach(lan =>
		{
			using (Res.TemporarilySwitchLanguage(lan))
			{
				filter.Property = FilterStripBusinessObject.StatusActive;
				AssertContains("OH_IsActive = 1", licenceKeyFilter.Filter.LiteralTextADO);

				filter.Property = FilterStripBusinessObject.StatusInactive;
				AssertContains("OH_IsActive = 0", licenceKeyFilter.Filter.LiteralTextADO);

				filter.Property = FilterStripBusinessObject.StatusAll;
				AssertContains("OH_IsActive = 1 or OH_IsActive = 0", licenceKeyFilter.Filter.LiteralTextADO);
			}
		});
	}

	#region Implementation

	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
	{
		return new LicenceKeyFilterBusinessObject();
	}

	#endregion
}
