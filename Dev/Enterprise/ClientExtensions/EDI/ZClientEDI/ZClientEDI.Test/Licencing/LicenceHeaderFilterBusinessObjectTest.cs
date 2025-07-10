using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.Testing;

[TestedType(typeof(LicenceHeaderFilterBusinessObject))]
public class LicenceHeaderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
{
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

	public void TestFilters_EntID()
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
		((ModuleGuidFilter)filterBizo["Enterprise ID"]).Property = enterprise2.PK;
		((ModuleGuidFilter)filterBizo["Enterprise ID"]).IsActive = true;
		headers.Load(filterBizo.Filter);
		AssertEquals("2nd and 3rd found", 2, headers.Count);
		AssertCollectionContains(header2, headers);
		AssertCollectionContains(header3, headers);

		((ModuleGuidFilter)filterBizo["Enterprise ID"]).Property = enterprise.PK;
		headers.Load(filterBizo.Filter);
		AssertEquals("1st found", 1, headers.Count);
		AssertCollectionContains(header, headers);
	}

	#region Implementation

	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
	{
		return new LicenceHeaderFilterBusinessObject();
	}

	#endregion
}
