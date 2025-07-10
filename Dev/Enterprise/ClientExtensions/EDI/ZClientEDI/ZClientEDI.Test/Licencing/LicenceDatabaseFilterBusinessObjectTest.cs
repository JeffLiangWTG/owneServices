using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.Testing;

[TestedType(typeof(LicenceDatabaseFilterBusinessObject))]
public class LicenceDatabaseFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
{
	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
	{
		return new LicenceDatabaseFilterBusinessObject();
	}

	#region Product Test

	public void TestProductTypeFilter()
	{
		var list = new SystemProductCollection();

		list.AddNew("ZEU", "Zeus", true);
		list.AddNew("APL", "Apollo", true);
		list.AddNew("JUP", "Jupiter", true);

		EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

		LicenceDatabase ld1 = Factory.NewWithValidTestData<LicenceDatabase>();
		LicenceDatabase ld2 = Factory.NewWithValidTestData<LicenceDatabase>();
		LicenceDatabase ld3 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld1.LD_Product = "ZEU";
		ld2.LD_Product = "APL";
		ld3.LD_Product = "JUP";

		Factory.Save();

		var licenceDatabaseFilter = new LicenceDatabaseFilterBusinessObject();
		var productFilter = (ModuleTextFilter)licenceDatabaseFilter["Product"];
		LicenceDatabaseNonDependentCollection licenceDatabaseCollection = new LicenceDatabaseNonDependentCollection(Factory);

		productFilter.Property = "ZEU";
		productFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
		productFilter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should only contain Licence Databases with 'ZEU' Product", licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should only contain Licence Databases with 'ZEU' Product", !licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should only contain Licence Databases with 'ZEU' Product", !licenceDatabaseCollection.Contains(ld3.PK));

		productFilter.Property = "ZEU";
		productFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
		productFilter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should not contain Licence Databases with 'ZEU' Product", !licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should not contain Licence Databases with 'ZEU' Product", licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should not contain Licence Databases with 'ZEU' Product", licenceDatabaseCollection.Contains(ld3.PK));

		productFilter.Property = "JUP";
		productFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
		productFilter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should only contain Licence Databases with 'JUP' Product", !licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should only contain Licence Databases with 'JUP' Product", !licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should only contain Licence Databases with 'JUP' Product", licenceDatabaseCollection.Contains(ld3.PK));
	}

	#endregion

	#region Organisation

	public void TestOrganisationFilter()
	{
		OrgHeader oh1 = Factory.NewWithValidTestData<OrgHeader>();
		oh1.OH_Code = "FLNTSTONE";
		OrgHeader oh2 = Factory.NewWithValidTestData<OrgHeader>();
		oh2.OH_Code = "JETSONS12";
		OrgHeader oh3 = Factory.NewWithValidTestData<OrgHeader>();
		oh3.OH_Code = "TOMNJERRY";

		LicenceEnterprise le1 = Factory.NewWithValidTestData<LicenceEnterprise>();
		le1.LE_OH = oh1.PK;
		le1.LE_EnterpriseCode = "AU1";
		LicenceEnterprise le2 = Factory.NewWithValidTestData<LicenceEnterprise>();
		le2.LE_OH = oh2.PK;
		le2.LE_EnterpriseCode = "AU2";
		LicenceEnterprise le3 = Factory.NewWithValidTestData<LicenceEnterprise>();
		le3.LE_OH = oh3.PK;
		le3.LE_EnterpriseCode = "AU3";

		LicenceDatabase ld1 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld1.LD_LE = le1.PK;
		LicenceDatabase ld2 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld2.LD_LE = le2.PK;
		LicenceDatabase ld3 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld3.LD_LE = le3.PK;

		Factory.Save();

		var licenceDatabaseFilter = new LicenceDatabaseFilterBusinessObject();
		var organisationFilter = (ModuleGuidFilter)licenceDatabaseFilter["Organisation"];
		LicenceDatabaseNonDependentCollection licenceDatabaseCollection = new LicenceDatabaseNonDependentCollection(Factory);

		organisationFilter.Property = oh1.PK;
		organisationFilter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should only contain Flintstones", licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should only contain Flintstones", !licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should only contain Flintstones", !licenceDatabaseCollection.Contains(ld3.PK));

		organisationFilter.Property = oh2.PK;
		organisationFilter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should only contain The Jetsons", !licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should only contain The Jetsons", licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should only contain The Jetsons", !licenceDatabaseCollection.Contains(ld3.PK));

		organisationFilter.Property = oh3.PK;
		organisationFilter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should only contain Tom & Jerry", !licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should only contain Tom & Jerry", !licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should only contain Tom & Jerry", licenceDatabaseCollection.Contains(ld3.PK));
	}
	#endregion

	#region Organisation Name

	public void TestOrganisationNameFilter()
	{
		OrgHeader oh1 = Factory.NewWithValidTestData<OrgHeader>();
		oh1.OH_Code = "FLNTSTONE";
		oh1.OH_FullName = "The Flintstones";
		OrgHeader oh2 = Factory.NewWithValidTestData<OrgHeader>();
		oh2.OH_Code = "JETSONS12";
		oh2.OH_FullName = "The Jetsons";
		OrgHeader oh3 = Factory.NewWithValidTestData<OrgHeader>();
		oh3.OH_Code = "TOMNJERRY";
		oh3.OH_FullName = "Tom & Jerry";

		LicenceEnterprise le1 = Factory.NewWithValidTestData<LicenceEnterprise>();
		le1.LE_OH = oh1.PK;
		le1.LE_EnterpriseCode = "AU1";
		LicenceEnterprise le2 = Factory.NewWithValidTestData<LicenceEnterprise>();
		le2.LE_OH = oh2.PK;
		LicenceEnterprise le3 = Factory.NewWithValidTestData<LicenceEnterprise>();
		le3.LE_OH = oh3.PK;
		le3.LE_EnterpriseCode = "AU3";

		LicenceDatabase ld1 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld1.LD_LE = le1.PK;
		LicenceDatabase ld2 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld2.LD_LE = le2.PK;
		LicenceDatabase ld3 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld3.LD_LE = le3.PK;

		Factory.Save();

		var licenceDatabaseFilter = new LicenceDatabaseFilterBusinessObject();
		var organisationFilter = (ModuleTextFilter)licenceDatabaseFilter["Organisation Name"];
		LicenceDatabaseNonDependentCollection licenceDatabaseCollection = new LicenceDatabaseNonDependentCollection(Factory);

		organisationFilter.Property = oh1.OH_FullName;
		organisationFilter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should only contain Flintstones", licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should only contain Flintstones", !licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should only contain Flintstones", !licenceDatabaseCollection.Contains(ld3.PK));

		organisationFilter.Property = oh2.OH_FullName;
		organisationFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
		organisationFilter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should only contain The Jetsons", licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should only contain The Jetsons", !licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should only contain The Jetsons", licenceDatabaseCollection.Contains(ld3.PK));

		organisationFilter.Property = "The";
		organisationFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
		organisationFilter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should not contain Tom & Jerry", licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should not contain Tom & Jerry", licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should not contain Tom & Jerry", !licenceDatabaseCollection.Contains(ld3.PK));

		organisationFilter.Property = "&";
		organisationFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
		organisationFilter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should only contain Tom & Jerry", !licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should only contain Tom & Jerry", !licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should only contain Tom & Jerry", licenceDatabaseCollection.Contains(ld3.PK));
	}

	#endregion

	#region Enterprise Code

	public void TestEnterpriseCodeFilter()
	{
		LicenceEnterprise le1 = Factory.NewWithValidTestData<LicenceEnterprise>();
		LicenceEnterprise le2 = Factory.NewWithValidTestData<LicenceEnterprise>();
		LicenceEnterprise le3 = Factory.NewWithValidTestData<LicenceEnterprise>();
		le3.LE_EnterpriseCode = ZString.Empty;

		LicenceDatabase ld1 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld1.LD_LE = le1.PK;
		LicenceDatabase ld2 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld2.LD_LE = le2.PK;
		LicenceDatabase ld3 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld3.LD_LE = le3.PK;

		Factory.Save();

		var licenceDatabaseFilter = new LicenceDatabaseFilterBusinessObject();
		ModuleGuidFilter filter = (ModuleGuidFilter)licenceDatabaseFilter["Enterprise Code"];
		LicenceDatabaseNonDependentCollection licenceDatabaseCollection = new LicenceDatabaseNonDependentCollection(Factory);
		AssertEquals(true, filter.SupportsBlankComparisonOperators);

		filter.Property = le1.PK;
		filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
		filter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should only contain ld1", licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should only contain ld1", !licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should only contain ld1", !licenceDatabaseCollection.Contains(ld3.PK));

		filter.Property = le1.PK;
		filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
		filter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should not contain ld1", !licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should not contain ld1", licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should not contain ld1", licenceDatabaseCollection.Contains(ld3.PK));

		filter.Property = le3.PK;
		filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
		filter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should Only contain ld3", !licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should Only contain ld3", !licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should Only contain ld3", licenceDatabaseCollection.Contains(ld3.PK));

		filter.Property = ZGuid.Empty;
		filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
		filter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);
		Assert(licenceDatabaseCollection.Contains(ld1.PK));
		Assert(licenceDatabaseCollection.Contains(ld2.PK));
		Assert(!licenceDatabaseCollection.Contains(ld3.PK));

		filter.Property = ZGuid.Empty;
		filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
		filter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);
		Assert(!licenceDatabaseCollection.Contains(ld1.PK));
		Assert(!licenceDatabaseCollection.Contains(ld2.PK));
		Assert(licenceDatabaseCollection.Contains(ld3.PK));
	}

	public void TestEnterpriseIDFilter()
	{
		LicenceEnterprise le1 = Factory.NewWithValidTestData<LicenceEnterprise>();
		LicenceEnterprise le2 = Factory.NewWithValidTestData<LicenceEnterprise>();
		LicenceEnterprise le3 = Factory.NewWithValidTestData<LicenceEnterprise>();

		LicenceDatabase ld1 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld1.LD_LE = le1.PK;
		LicenceDatabase ld2 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld2.LD_LE = le2.PK;
		LicenceDatabase ld3 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld3.LD_LE = le3.PK;

		Factory.Save();

		var licenceDatabaseFilter = new LicenceDatabaseFilterBusinessObject();
		ModuleGuidFilter filter = (ModuleGuidFilter)licenceDatabaseFilter["Enterprise ID"];
		LicenceDatabaseNonDependentCollection licenceDatabaseCollection = new LicenceDatabaseNonDependentCollection(Factory);
		AssertEquals(false, filter.SupportsBlankComparisonOperators);

		filter.Property = le1.PK;
		filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
		filter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should only contain ld1", licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should only contain ld1", !licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should only contain ld1", !licenceDatabaseCollection.Contains(ld3.PK));

		filter.Property = le1.PK;
		filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
		filter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should not contain ld1", !licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should not contain ld1", licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should not contain ld1", licenceDatabaseCollection.Contains(ld3.PK));

		filter.Property = le3.PK;
		filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
		filter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should Only contain ld3", !licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should Only contain ld3", !licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should Only contain ld3", licenceDatabaseCollection.Contains(ld3.PK));
	}

	#endregion

	#region Database Server Name

	public void TestLicenceDatabaseServerName()
	{
		OrgHeader oh1 = Factory.NewWithValidTestData<OrgHeader>();
		oh1.OH_Code = "FLNTSTONE";
		oh1.OH_FullName = "The Flintstones";
		OrgHeader oh2 = Factory.NewWithValidTestData<OrgHeader>();
		oh2.OH_Code = "JETSONS12";
		oh2.OH_FullName = "The Jetsons";
		OrgHeader oh3 = Factory.NewWithValidTestData<OrgHeader>();
		oh3.OH_Code = "TOMNJERRY";
		oh3.OH_FullName = "Tom & Jerry";

		LicenceEnterprise le1 = Factory.NewWithValidTestData<LicenceEnterprise>();
		le1.LE_OH = oh1.PK;
		le1.LE_EnterpriseCode = "AU1";
		LicenceEnterprise le2 = Factory.NewWithValidTestData<LicenceEnterprise>();
		le2.LE_OH = oh2.PK;
		LicenceEnterprise le3 = Factory.NewWithValidTestData<LicenceEnterprise>();
		le3.LE_OH = oh3.PK;
		le3.LE_EnterpriseCode = "AU3";

		LicenceDatabase ld1 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld1.LD_LE = le1.PK;
		ld1.LD_ReportedHostServerName = "ZZZ";
		LicenceDatabase ld2 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld2.LD_LE = le2.PK;
		ld2.LD_ReportedHostServerName = "ZZX";
		LicenceDatabase ld3 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld3.LD_LE = le3.PK;
		ld3.LD_ReportedHostServerName = "XZY";

		Factory.Save();

		var licenceDatabaseFilter = new LicenceDatabaseFilterBusinessObject();
		var organisationFilter = (ModuleTextFilter)licenceDatabaseFilter["Database Server Name"];
		LicenceDatabaseNonDependentCollection licenceDatabaseCollection = new LicenceDatabaseNonDependentCollection(Factory);

		organisationFilter.Property = "ZZZ";
		organisationFilter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should only contain Flintstones", licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should only contain Flintstones", !licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should only contain Flintstones", !licenceDatabaseCollection.Contains(ld3.PK));

		organisationFilter.Property = "ZZ";
		organisationFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
		organisationFilter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should not contain Tom & Jerry", licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should not contain Tom & Jerry", licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should not contain Tom & Jerry", !licenceDatabaseCollection.Contains(ld3.PK));

		organisationFilter.Property = "X";
		organisationFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
		organisationFilter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should not contain Flintstones", !licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should not contain Flintstones", licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should not contain Flintstones", licenceDatabaseCollection.Contains(ld3.PK));
	}

	#endregion

	#region Database Name

	public void TestLicenceDatabaseName()
	{
		OrgHeader oh1 = Factory.NewWithValidTestData<OrgHeader>();
		oh1.OH_Code = "FLNTSTONE";
		oh1.OH_FullName = "The Flintstones";
		OrgHeader oh2 = Factory.NewWithValidTestData<OrgHeader>();
		oh2.OH_Code = "JETSONS12";
		oh2.OH_FullName = "The Jetsons";
		OrgHeader oh3 = Factory.NewWithValidTestData<OrgHeader>();
		oh3.OH_Code = "TOMNJERRY";
		oh3.OH_FullName = "Tom & Jerry";

		LicenceEnterprise le1 = Factory.NewWithValidTestData<LicenceEnterprise>();
		le1.LE_OH = oh1.PK;
		le1.LE_EnterpriseCode = "AU1";
		LicenceEnterprise le2 = Factory.NewWithValidTestData<LicenceEnterprise>();
		le2.LE_OH = oh2.PK;
		LicenceEnterprise le3 = Factory.NewWithValidTestData<LicenceEnterprise>();
		le3.LE_OH = oh3.PK;
		le3.LE_EnterpriseCode = "AU3";

		LicenceDatabase ld1 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld1.LD_LE = le1.PK;
		ld1.LD_ReportedHostDBName = "ZZZ";
		LicenceDatabase ld2 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld2.LD_LE = le2.PK;
		ld2.LD_ReportedHostDBName = "ZZX";
		LicenceDatabase ld3 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld3.LD_LE = le3.PK;
		ld3.LD_ReportedHostDBName = "XZY";

		Factory.Save();

		var licenceDatabaseFilter = new LicenceDatabaseFilterBusinessObject();
		var organisationFilter = (ModuleTextFilter)licenceDatabaseFilter["Database Name"];
		LicenceDatabaseNonDependentCollection licenceDatabaseCollection = new LicenceDatabaseNonDependentCollection(Factory);

		organisationFilter.Property = "ZZZ";
		organisationFilter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should only contain Flintstones", licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should only contain Flintstones", !licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should only contain Flintstones", !licenceDatabaseCollection.Contains(ld3.PK));

		organisationFilter.Property = "ZZ";
		organisationFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
		organisationFilter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should not contain Tom & Jerry", licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should not contain Tom & Jerry", licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should not contain Tom & Jerry", !licenceDatabaseCollection.Contains(ld3.PK));

		organisationFilter.Property = "X";
		organisationFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
		organisationFilter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should not contain Flintstones", !licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should not contain Flintstones", licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should not contain Flintstones", licenceDatabaseCollection.Contains(ld3.PK));
	}

	#endregion

	#region Database Connection Server

	public void TestLicenceDatabaseConnectionServer()
	{
		OrgHeader oh1 = Factory.NewWithValidTestData<OrgHeader>();
		oh1.OH_Code = "FLNTSTONE";
		oh1.OH_FullName = "The Flintstones";
		OrgHeader oh2 = Factory.NewWithValidTestData<OrgHeader>();
		oh2.OH_Code = "JETSONS12";
		oh2.OH_FullName = "The Jetsons";
		OrgHeader oh3 = Factory.NewWithValidTestData<OrgHeader>();
		oh3.OH_Code = "TOMNJERRY";
		oh3.OH_FullName = "Tom & Jerry";

		LicenceEnterprise le1 = Factory.NewWithValidTestData<LicenceEnterprise>();
		le1.LE_OH = oh1.PK;
		le1.LE_EnterpriseCode = "AU1";
		LicenceEnterprise le2 = Factory.NewWithValidTestData<LicenceEnterprise>();
		le2.LE_OH = oh2.PK;
		LicenceEnterprise le3 = Factory.NewWithValidTestData<LicenceEnterprise>();
		le3.LE_OH = oh3.PK;
		le3.LE_EnterpriseCode = "AU3";

		LicenceDatabase ld1 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld1.LD_LE = le1.PK;
		ld1.LD_HostConnectionServerName = "ZZZ";
		LicenceDatabase ld2 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld2.LD_LE = le2.PK;
		ld2.LD_HostConnectionServerName = "ZZX";
		LicenceDatabase ld3 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld3.LD_LE = le3.PK;
		ld3.LD_HostConnectionServerName = "XZY";

		Factory.Save();

		var licenceDatabaseFilter = new LicenceDatabaseFilterBusinessObject();
		var organisationFilter = (ModuleTextFilter)licenceDatabaseFilter["Database Connection Server"];
		LicenceDatabaseNonDependentCollection licenceDatabaseCollection = new LicenceDatabaseNonDependentCollection(Factory);

		organisationFilter.Property = "ZZZ";
		organisationFilter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should only contain Flintstones", licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should only contain Flintstones", !licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should only contain Flintstones", !licenceDatabaseCollection.Contains(ld3.PK));

		organisationFilter.Property = "ZZ";
		organisationFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
		organisationFilter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should not contain Tom & Jerry", licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should not contain Tom & Jerry", licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should not contain Tom & Jerry", !licenceDatabaseCollection.Contains(ld3.PK));

		organisationFilter.Property = "X";
		organisationFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
		organisationFilter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should not contain Flintstones", !licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should not contain Flintstones", licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should not contain Flintstones", licenceDatabaseCollection.Contains(ld3.PK));
	}

	#endregion

	#region Database Registration

	public void TestLicenceDatabaseRegistration()
	{
		OrgHeader oh1 = Factory.NewWithValidTestData<OrgHeader>();
		oh1.OH_Code = "FLNTSTONE";
		oh1.OH_FullName = "The Flintstones";
		OrgHeader oh2 = Factory.NewWithValidTestData<OrgHeader>();
		oh2.OH_Code = "JETSONS12";
		oh2.OH_FullName = "The Jetsons";
		OrgHeader oh3 = Factory.NewWithValidTestData<OrgHeader>();
		oh3.OH_Code = "TOMNJERRY";
		oh3.OH_FullName = "Tom & Jerry";

		LicenceEnterprise le1 = Factory.NewWithValidTestData<LicenceEnterprise>();
		le1.LE_OH = oh1.PK;
		le1.LE_EnterpriseCode = "AU1";
		LicenceEnterprise le2 = Factory.NewWithValidTestData<LicenceEnterprise>();
		le2.LE_OH = oh2.PK;
		LicenceEnterprise le3 = Factory.NewWithValidTestData<LicenceEnterprise>();
		le3.LE_OH = oh3.PK;
		le3.LE_EnterpriseCode = "AU3";

		LicenceDatabase ld1 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld1.LD_LE = le1.PK;
		ld1.LD_Status = DatabaseStatusList.Codes.NON;
		LicenceDatabase ld2 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld2.LD_LE = le2.PK;
		ld2.LD_Status = DatabaseStatusList.Codes.REG;
		LicenceDatabase ld3 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld3.LD_LE = le3.PK;
		ld3.LD_Status = DatabaseStatusList.Codes.Preregistered;

		Factory.Save();

		var licenceDatabaseFilter = new LicenceDatabaseFilterBusinessObject();
		var organisationFilter = (ModuleTextFilter)licenceDatabaseFilter["Database Registration"];
		LicenceDatabaseNonDependentCollection licenceDatabaseCollection = new LicenceDatabaseNonDependentCollection(Factory);

		organisationFilter.Property = DatabaseStatusList.Codes.NON;
		organisationFilter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should only contain Flintstones", licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should only contain Flintstones", !licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should only contain Flintstones", !licenceDatabaseCollection.Contains(ld3.PK));

		organisationFilter.Property = DatabaseStatusList.Codes.REG;
		organisationFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
		organisationFilter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should not contain Jetsons", licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should not contain Jetsons", !licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should not contain Jetsons", licenceDatabaseCollection.Contains(ld3.PK));
	}

	#endregion

	#region Database System Info Group

	public void TestLicenceDatabaseBelongToDatabaseSystemInfoGroup()
	{
		var licenceDatabaseFilter = new LicenceDatabaseFilterBusinessObject();
		Assert("Expect category is databaseSystemInfo", licenceDatabaseFilter["Database Server Name"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", licenceDatabaseFilter["Database Name"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", licenceDatabaseFilter["Database Connection Server"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", licenceDatabaseFilter["Database Registration"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", licenceDatabaseFilter["OS Name (Free Text)"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", licenceDatabaseFilter["OS Name"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", licenceDatabaseFilter["OS Version (Free Text)"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", licenceDatabaseFilter["OS Version"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", licenceDatabaseFilter["System Manufacturer (Free Text)"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", licenceDatabaseFilter["System Manufacturer"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", licenceDatabaseFilter["BIOS Release Date"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", licenceDatabaseFilter["Total Physical Memory in MB"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", licenceDatabaseFilter["Number of Processors"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", licenceDatabaseFilter["Processor Type (Free Text)"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", licenceDatabaseFilter["Processor Type"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", licenceDatabaseFilter["Processor Speed in Mhz"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", licenceDatabaseFilter["Is Virtual Machine"].Category.ToString() == "Database System Info");
		Assert("Expect category is databaseSystemInfo", licenceDatabaseFilter["Feature Set"].Category.ToString() == "Database System Info");
	}

	#endregion

	public void TestStaffFilter()
	{
		var ld1 = Factory.NewWithValidTestData<LicenceDatabase>();
		var ld2 = Factory.NewWithValidTestData<LicenceDatabase>();

		ld1.LD_GS_NKOwner = "U01";
		ld2.LD_GS_NKOwner = "U02";

		Factory.Save();

		var filter = new LicenceDatabaseFilterBusinessObject();
		((ModuleNkFilter)filter["Staff Owner"]).Property = "U02";
		((ModuleNkFilter)filter["Staff Owner"]).IsActive = true;

		var lds = new LicenceDatabaseNonDependentCollection(Factory);
		lds.Load(filter.Filter);

		AssertCollectionContains(ld2, lds);
		AssertCollectionNotContains(ld1, lds);
	}

	public void TestStlPriceListFilters()
	{
		var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "AU1", "DB1");
		var lic2 = BillingTestHelper.CreateLicence(Factory, "EN2", "AU2", "DB2");
		var priceList1 = BillingTestHelper.CreatePriceHeader(lic1.Company, "STL", "STL v1", "AUD", new ZDateTime(2016, 1, 1), false);
		var price1 = BillingTestHelper.AddPriceItem(priceList1, "C01", "TRA", "", 100m);
		var link1 = BillingTestHelper.CreatePriceLink(lic1.Database, priceList1, new ZDateTime(2016, 1, 1));
		link1.PHL_ValidTo = new ZDateTime(2017, 1, 1);
		link1.PHL_L6 = priceList1.PK;
		link1.PHL_RX_NKCurrency = "AUD";
		link1.PHL_VolumeCode = "HV";
		link1.PHL_CorePackCode = "INC";
		link1.PHL_SystemCreateUser = "U01";
		link1.PHL_SystemCreateTimeUtc = new ZDateTime(2016, 2, 1);

		var settingDIS = lic1.Database.LicenceSettings.AddNew();
		settingDIS.LS9_ValidFrom = settingDIS.LS9_ValidTo = new ZDateTime(2016, 1, 1);
		settingDIS.LS9_Type = "DIS";
		settingDIS.LS9_Name = "SPECIAL";
		settingDIS.LS9_Percent = 10;
		settingDIS.LS9_Comment = "Comment002";

		Factory.Save();

		var filter = new LicenceDatabaseFilterBusinessObject();
		((ModuleNkFilter)filter["STL Prices Currency"]).Property = "AUD";
		((ModuleNkFilter)filter["STL Prices Currency"]).IsActive = true;

		((ModuleTextFilter)filter["STL Setting Discount Name"]).Property = "SPECIAL";
		((ModuleTextFilter)filter["STL Setting Discount Name"]).IsActive = true;

		var lds = new LicenceDatabaseNonDependentCollection(Factory);
		lds.Load(filter.Filter);

		AssertCollectionContains(lic1.Database, lds);
		AssertCollectionNotContains(lic2.Database, lds);
	}

	public void TestEnterpriseRegistrationFilter()
	{
		var le1 = Factory.NewWithValidTestData<LicenceEnterprise>();
		var le2 = Factory.NewWithValidTestData<LicenceEnterprise>();
		var le3 = Factory.NewWithValidTestData<LicenceEnterprise>();

		LicenceDatabase ld1 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld1.LD_LE = le1.PK;
		LicenceDatabase ld2 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld2.LD_LE = le2.PK;
		LicenceDatabase ld3 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld3.LD_LE = le3.PK;

		Factory.Save();

		EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, le2.LE_EnterpriseID);

		var enterpriseRegistrationFilter = new LicenceDatabaseFilterBusinessObject();
		var filter = (ModuleFlagsFilter)enterpriseRegistrationFilter["Databases Not registered with an Enterprise ID"];
		filter.IsActive = true;
		filter["Yes"] = true;

		var licenceDatabaseCollection = new LicenceDatabaseNonDependentCollection(Factory);
		licenceDatabaseCollection.Load(enterpriseRegistrationFilter.Filter);

		Assert(!licenceDatabaseCollection.Contains(ld1.PK));
		Assert(licenceDatabaseCollection.Contains(ld2.PK));
		Assert(!licenceDatabaseCollection.Contains(ld3.PK));

		filter["Yes"] = false;
		licenceDatabaseCollection.Load(enterpriseRegistrationFilter.Filter);

		Assert(licenceDatabaseCollection.Contains(ld1.PK));
		Assert(licenceDatabaseCollection.Contains(ld2.PK));
		Assert(licenceDatabaseCollection.Contains(ld3.PK));
	}

	public void TestTokenAuthenticationFilter()
	{
		var ld1 = Factory.NewWithValidTestData<LicenceDatabase>();
		var ld2 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld2.LD_TokenAuthenticationEnabled = true;
		var ld3 = Factory.NewWithValidTestData<LicenceDatabase>();

		Factory.Save();

		var licenceDatabaseFilter = new LicenceDatabaseFilterBusinessObject();
		var productFilter = (ModuleTextFilter)licenceDatabaseFilter["Token Authentication"];
		var licenceDatabaseCollection = new LicenceDatabaseNonDependentCollection(Factory);
		productFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
		productFilter.IsActive = true;

		productFilter.Property = TokenAuthenticationStatusList.Codes.Enabled;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should only contain Licence Databases ld2", !licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should only contain Licence Databases ld2", licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should only contain Licence Databases ld2", !licenceDatabaseCollection.Contains(ld3.PK));

		productFilter.Property = TokenAuthenticationStatusList.Codes.NotEnabled;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should not contain Licence Databases with ld2", licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should not contain Licence Databases with ld2", !licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should not contain Licence Databases with ld2", licenceDatabaseCollection.Contains(ld3.PK));

		productFilter.Property = TokenAuthenticationStatusList.Codes.ALL;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should contains all Licence Databases", licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should contains all Licence Databases", licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should contains all Licence Databases", licenceDatabaseCollection.Contains(ld3.PK));
	}

	#region Trusted System

	public void TestTrustedSystemFilter()
	{
		var ts1 = Factory.NewWithValidTestData<EdiTrustedSystem>();
		var ts2 = Factory.NewWithValidTestData<EdiTrustedSystem>();
		var ts3 = Factory.NewWithValidTestData<EdiTrustedSystem>();

		var ld1 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld1.LD_ETS_TrustedSystem = ts1.PK;
		var ld2 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld2.LD_ETS_TrustedSystem = ts2.PK;
		var ld3 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld3.LD_ETS_TrustedSystem = ts3.PK;

		Factory.Save();

		var licenceDatabaseFilter = new LicenceDatabaseFilterBusinessObject();
		var filter = (ModuleGuidFilter)licenceDatabaseFilter["Trusted System"];
		var licenceDatabaseCollection = new LicenceDatabaseNonDependentCollection(Factory);
		AssertEquals(true, filter.SupportsBlankComparisonOperators);

		filter.Property = ts1.PK;
		filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
		filter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should only contain ld1", licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should only contain ld1", !licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should only contain ld1", !licenceDatabaseCollection.Contains(ld3.PK));

		filter.Property = ts1.PK;
		filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
		filter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should not contain ld1", !licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should not contain ld1", licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should not contain ld1", licenceDatabaseCollection.Contains(ld3.PK));

		filter.Property = ts3.PK;
		filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
		filter.IsActive = true;
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should Only contain ld3", !licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should Only contain ld3", !licenceDatabaseCollection.Contains(ld2.PK));
		Assert("Should Only contain ld3", licenceDatabaseCollection.Contains(ld3.PK));
	}

	#endregion

	public void TestFeatureSetFilter()
	{
		var ld1 = Factory.NewWithValidTestData<LicenceDatabase>();
		var ld2 = Factory.NewWithValidTestData<LicenceDatabase>();

		var featureSet = Factory.NewWithValidTestData<FeatureControlSet>();
		featureSet.FCS_ProductName = "CWN";
		ld1.LD_FCS_FeatureSet = featureSet.PK;

		Factory.Save();

		var licenceDatabaseFilter = new LicenceDatabaseFilterBusinessObject();
		var featureSetFilter = (ModuleTextFilter)licenceDatabaseFilter["Feature Set"];
		var licenceDatabaseCollection = new LicenceDatabaseNonDependentCollection(Factory);
		featureSetFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
		featureSetFilter.IsActive = true;

		featureSetFilter.Property = "CWN";
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should only contain Licence Databases ld1", licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should only contain Licence Databases ld1", !licenceDatabaseCollection.Contains(ld2.PK));

		featureSetFilter.Property = "test";
		licenceDatabaseCollection.Load(licenceDatabaseFilter.Filter);

		Assert("Should not contain Licence Databases with ld1", !licenceDatabaseCollection.Contains(ld1.PK));
		Assert("Should not contain Licence Databases with ld2", !licenceDatabaseCollection.Contains(ld2.PK));
	}
}
