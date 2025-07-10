using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.IdentityTenant.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IdentityApplication.Module.Testing
{
	[TestedType(typeof(EdiIdentityApplicationFilterBusinessObject))]
	public class EdiIdentityApplicationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EdiIdentityApplicationFilterBusinessObject();
		}

		public void TestModuleFilter()
		{
			var app1 = AddApplication(ZGuid.BrettsGuid.ToString(), "App1", true, "NON");
			app1.IDA_ApplicationModule = "test01";
			var app2 = AddApplication(string.Empty, "App2", true, "NON");
			var app3 = AddApplication(ZGuid.NewZGuid().ToString(), "App3", true, "NON");
			app3.IDA_ApplicationModule = "test03";
			Factory.Save();

			var filter = new EdiIdentityApplicationFilterBusinessObject();
			filter.AddActiveStatusFilters(typeof(EdiIdentityApplication));
			var clientIdFilter = (ModuleTextFilter)filter["Application Module"];
			clientIdFilter.IsActive = true;

			clientIdFilter.Property = "test01";
			clientIdFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			var collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("Equal", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { app1 }, collection);

			clientIdFilter.Property = "test01";
			clientIdFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("NotEqual", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { app2, app3 }, collection);

			clientIdFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("IsBlank", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { app2 }, collection);

			clientIdFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("IsNotBlank", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { app1, app3 }, collection);
		}

		public void TestClientIdFilter()
		{
			var app1 = AddApplication(ZGuid.BrettsGuid.ToString(), "App1", true, "NON");
			var app2 = AddApplication(string.Empty, "App2", true, "NON");
			var app3 = AddApplication(ZGuid.NewZGuid().ToString(), "App3", true, "NON");
			Factory.Save();

			var filter = new EdiIdentityApplicationFilterBusinessObject();
			filter.AddActiveStatusFilters(typeof(EdiIdentityApplication));
			var clientIdFilter = (ModuleTextFilter)filter["Client Id"];
			clientIdFilter.IsActive = true;

			clientIdFilter.Property = ZGuid.BrettsGuid.ToString();
			clientIdFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			var collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("Equal", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { app1 }, collection);

			clientIdFilter.Property = ZGuid.BrettsGuid.ToString();
			clientIdFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("NotEqual", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { app2, app3 }, collection);

			clientIdFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("IsBlank", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { app2 }, collection);

			clientIdFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("IsNotBlank", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { app1, app3 }, collection);
		}

		public void TestApplicationNameFilter()
		{
			var app1 = AddApplication(ZGuid.BrettsGuid.ToString(), "App1", true, "NON");
			var app2 = AddApplication(string.Empty, "App3", true, "NON");
			var app3 = AddApplication(string.Empty, string.Empty, true, "NON");

			Factory.Save();

			var filter = new EdiIdentityApplicationFilterBusinessObject();
			filter.AddActiveStatusFilters(typeof(EdiIdentityApplication));
			var clientIdFilter = (ModuleTextFilter)filter["Application Name"];
			clientIdFilter.IsActive = true;

			clientIdFilter.Property = "App1";
			clientIdFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			var collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("Equal", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { app1 }, collection);

			clientIdFilter.Property = "A";
			clientIdFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("StartsWith", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { app1, app2 }, collection);

			clientIdFilter.Property = "A";
			clientIdFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("Contains", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { app1, app2 }, collection);

			clientIdFilter.Property = "App1";
			clientIdFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("NotEqual", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { app2, app3 }, collection);

			clientIdFilter.Property = "A";
			clientIdFilter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("DoesNotStartWith", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { app3 }, collection);

			clientIdFilter.Property = "A";
			clientIdFilter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("NotContains", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { app3 }, collection);

			clientIdFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("IsBlank", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { app3 }, collection);

			clientIdFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("IsNotBlank", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { app1, app2 }, collection);
		}

		public void TestIsRollback()
		{
			var app1 = AddApplication(ZGuid.BrettsGuid.ToString(), "App1", false, "NON");
			var app2 = AddApplication(string.Empty, "App3", true, "NON");
			var app3 = AddApplication(string.Empty, string.Empty, true, "NON");
			Factory.Save();
			var filter = new EdiIdentityApplicationFilterBusinessObject();
			var rolledBackFilter = (ModuleTextFilter)filter["Roll-back Status"];
			rolledBackFilter.Property = EdiIdentityApplicationFilterBusinessObject.StatusRolledBack;
			rolledBackFilter.IsActive = true;

			AssertEquals("Roll-back Status", rolledBackFilter.Description);

			var collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals(2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { app2, app3 }, collection);

			rolledBackFilter.Property = EdiIdentityApplicationFilterBusinessObject.StatusNonRolledBack;

			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals(1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { app1 }, collection);

			rolledBackFilter.Property = FilterStripBusinessObject.StatusAll;

			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals(3, collection.Count);
		}

		public void TestRedirectUrlStatus()
		{
			var app1 = AddApplication(ZGuid.BrettsGuid.ToString(), "App1", true, EdiIdentityApplicationRedirectUrlStatus.Codes.None);
			var app2 = AddApplication(string.Empty, "App2", true, EdiIdentityApplicationRedirectUrlStatus.Codes.Error);
			var app3 = AddApplication(string.Empty, "App3", true, EdiIdentityApplicationRedirectUrlStatus.Codes.None);
			Factory.Save();
			var filter = new EdiIdentityApplicationFilterBusinessObject();
			var processingFilter = (ModuleTextFilter)filter["Redirect URL Status"];
			processingFilter.Property = EdiIdentityApplicationRedirectUrlStatus.Codes.None;
			processingFilter.IsActive = true;

			var collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals(2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { app1, app3 }, collection);

			processingFilter.Property = EdiIdentityApplicationRedirectUrlStatus.Codes.Error;

			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals(1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { app2 }, collection);

			processingFilter.Property = FilterStripBusinessObject.StatusAll;

			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals(3, collection.Count);
		}

		public void TestLicenceDatabase()
		{
			var app1 = AddApplication(ZGuid.BrettsGuid.ToString(), "App1", true, "NON");
			var app2 = AddApplication(string.Empty, "App2", true, "ERR");

			var licenseEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			licenseEnterprise.LE_EnterpriseCode = "EDI";
			licenseEnterprise.LE_EnterpriseID = "E000001";

			var licenseEnterprise2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			licenseEnterprise2.LE_EnterpriseCode = "CW1";
			licenseEnterprise2.LE_EnterpriseID = "E000002";

			var licenseDatabase1 = licenseEnterprise.Databases.AddNew();
			licenseDatabase1.LD_DatabaseNumber = 101;
			licenseDatabase1.LD_ServerCode = "PRD";
			licenseDatabase1.LD_Product = "CW1";

			var licenseDatabase2 = licenseEnterprise2.Databases.AddNew();
			licenseDatabase2.LD_DatabaseNumber = 102;
			licenseDatabase2.LD_ServerCode = "PRD";
			licenseDatabase2.LD_Product = "CW1";

			app1.IDA_LD = licenseDatabase1.PK;
			app2.IDA_LD = licenseDatabase2.PK;
			Factory.Save();

			var filter = new EdiIdentityApplicationFilterBusinessObject();
			filter.AddActiveStatusFilters(typeof(EdiIdentityApplication));
			var licenceFilter = (ModuleGuidFilter)filter["License Database"];
			licenceFilter.IsActive = true;

			licenceFilter.Property = licenseDatabase1.PK;
			licenceFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			var collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("Equal", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { app1 }, collection);

			licenceFilter.Property = ZGuid.BrettsGuid;
			licenceFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("NotEqual", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { app1, app2 }, collection);

			licenceFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("IsBlank", 0, collection.Count);

			licenceFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("IsNotBlank", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { app1, app2 }, collection);
		}

		public void TestTenantIdFilter()
		{
			var tenantId = ZGuid.NewZGuid().ToString();
			var tenant1 = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant1.IDT_TenantId = ZGuid.BrettsGuid.ToString();
			var tenant2 = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant2.IDT_TenantId = tenantId;
			var application1 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application1.IDA_IDT = tenant1.PK;
			var application2 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application2.IDA_IDT = tenant1.PK;
			var application3 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application3.IDA_IDT = tenant2.PK;
			Factory.Save();

			var filter = new EdiIdentityApplicationFilterBusinessObject();
			filter.AddActiveStatusFilters(typeof(EdiIdentityApplication));
			var tenantIdFilter = (ModuleTextFilter)filter["Tenant ID"];
			tenantIdFilter.IsActive = true;

			tenantIdFilter.Property = ZGuid.BrettsGuid.ToString();
			tenantIdFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			var collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("Equal", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { application1, application2 }, collection);

			tenantIdFilter.Property = ZGuid.BrettsGuid.ToString();
			tenantIdFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("StartsWith", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { application1, application2 }, collection);

			tenantIdFilter.Property = ZGuid.BrettsGuid.ToString();
			tenantIdFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("Contains", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { application1, application2 }, collection);

			tenantIdFilter.Property = ZGuid.BrettsGuid.ToString();
			tenantIdFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("NotEqual", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { application3 }, collection);

			tenantIdFilter.Property = ZGuid.BrettsGuid.ToString();
			tenantIdFilter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("DoesNotStartWith", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { application3 }, collection);

			tenantIdFilter.Property = ZGuid.BrettsGuid.ToString();
			tenantIdFilter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("NotContains", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { application3 }, collection);

			tenantIdFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("IsBlank", 0, collection.Count);

			tenantIdFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("IsNotBlank", 3, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { application1, application2, application3 }, collection);
		}

		public void TestProductFilter()
		{
			var application1 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application1.IDA_Product = ProductTypes.Codes.BorderWise;
			var application2 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application2.IDA_Product = ProductTypes.Codes.BorderWise;
			var application3 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application3.IDA_Product = ProductTypes.Codes.EHub;
			var licenceDatabase1 = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase1.LD_Product = ProductTypes.Codes.CargoWiseOne;
			var licenceDatabase2 = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase2.LD_Product = ProductTypes.Codes.CargoWiseNext;
			var application4 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application4.IDA_LD = licenceDatabase1.PK;
			var application5 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application5.IDA_LD = licenceDatabase1.PK;
			var application6 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application6.IDA_LD = licenceDatabase2.PK;
			Factory.Save();

			var filter = new EdiIdentityApplicationFilterBusinessObject();
			filter.AddActiveStatusFilters(typeof(EdiIdentityApplication));
			var productFilter = (ModuleTextFilter)filter["Product"];
			productFilter.IsActive = true;

			productFilter.Property = ProductTypes.Codes.BorderWise;
			productFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			var collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("Equal BorderWise", new[] { application1, application2 }, collection);

			productFilter.Property = ProductTypes.Codes.CargoWiseOne;
			productFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("Equal CargoWiseOne", new[] { application4, application5 }, collection);

			productFilter.Property = ProductTypes.Codes.BorderWise;
			productFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("StartsWith BorderWise", new[] { application1, application2 }, collection);

			productFilter.Property = ProductTypes.Codes.CargoWiseOne;
			productFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("StartsWith CargoWiseOne", new[] { application4, application5 }, collection);

			productFilter.Property = ProductTypes.Codes.BorderWise;
			productFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("Contains BorderWise", new[] { application1, application2 }, collection);

			productFilter.Property = ProductTypes.Codes.CargoWiseOne;
			productFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("Contains CargoWiseOne", new[] { application4, application5 }, collection);

			productFilter.Property = ProductTypes.Codes.BorderWise;
			productFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("NotEqual BorderWise", new[] { application3, application4, application5, application6 }, collection);

			productFilter.Property = ProductTypes.Codes.CargoWiseOne;
			productFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("NotEqual CargoWiseOne", new[] { application1, application2, application3, application6 }, collection);

			productFilter.Property = ProductTypes.Codes.BorderWise;
			productFilter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("DoesNotStartWith BorderWise", new[] { application3, application4, application5, application6 }, collection);

			productFilter.Property = ProductTypes.Codes.CargoWiseOne;
			productFilter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("DoesNotStartWith CargoWiseOne", new[] { application1, application2, application3, application6 }, collection);

			productFilter.Property = ProductTypes.Codes.BorderWise;
			productFilter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("NotContains BorderWise", new[] { application3, application4, application5, application6 }, collection);

			productFilter.Property = ProductTypes.Codes.CargoWiseOne;
			productFilter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("NotContains CargoWiseOne", new[] { application1, application2, application3, application6 }, collection);

			productFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("IsBlank", 0, collection.Count);

			productFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("IsNotBlank", new[] { application1, application2, application3, application4, application5, application6 }, collection);
		}

		public void TestApplicationTypeFilter()
		{
			var application1 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application1.IDA_ApplicationType = DatabaseTypes.Codes.Demo;
			var application2 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application2.IDA_ApplicationType = DatabaseTypes.Codes.Demo;
			var application3 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application3.IDA_ApplicationType = DatabaseTypes.Codes.Test;
			var licenceDatabase1 = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase1.LD_LicenceType = DatabaseTypes.Codes.Production;
			var licenceDatabase2 = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase2.LD_LicenceType = DatabaseTypes.Codes.Training;
			var application4 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application4.IDA_LD = licenceDatabase1.PK;
			var application5 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application5.IDA_LD = licenceDatabase1.PK;
			var application6 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application6.IDA_LD = licenceDatabase2.PK;
			Factory.Save();

			var filter = new EdiIdentityApplicationFilterBusinessObject();
			filter.AddActiveStatusFilters(typeof(EdiIdentityApplication));
			var tenantIdFilter = (ModuleTextFilter)filter["Application Type"];
			tenantIdFilter.IsActive = true;

			tenantIdFilter.Property = DatabaseTypes.Codes.Demo;
			tenantIdFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			var collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("Equal Demo", new[] { application1, application2 }, collection);

			tenantIdFilter.Property = DatabaseTypes.Codes.Production;
			tenantIdFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("Equal Production", new[] { application4, application5 }, collection);

			tenantIdFilter.Property = DatabaseTypes.Codes.Demo;
			tenantIdFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("StartsWith Demo", new[] { application1, application2 }, collection);

			tenantIdFilter.Property = DatabaseTypes.Codes.Production;
			tenantIdFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("StartsWith Production", new[] { application4, application5 }, collection);

			tenantIdFilter.Property = DatabaseTypes.Codes.Demo;
			tenantIdFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("Contains Demo", new[] { application1, application2 }, collection);

			tenantIdFilter.Property = DatabaseTypes.Codes.Production;
			tenantIdFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("Contains Production", new[] { application4, application5 }, collection);

			tenantIdFilter.Property = DatabaseTypes.Codes.Demo;
			tenantIdFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("NotEqual Demo", new[] { application3, application4, application5, application6 }, collection);

			tenantIdFilter.Property = DatabaseTypes.Codes.Production;
			tenantIdFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("NotEqual Production", new[] { application1, application2, application3, application6 }, collection);

			tenantIdFilter.Property = DatabaseTypes.Codes.Demo;
			tenantIdFilter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("DoesNotStartWith Demo", new[] { application3, application4, application5, application6 }, collection);

			tenantIdFilter.Property = DatabaseTypes.Codes.Production;
			tenantIdFilter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("DoesNotStartWith Production", new[] { application1, application2, application3, application6 }, collection);

			tenantIdFilter.Property = DatabaseTypes.Codes.Demo;
			tenantIdFilter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("NotContains Demo", new[] { application3, application4, application5, application6 }, collection);

			tenantIdFilter.Property = DatabaseTypes.Codes.Production;
			tenantIdFilter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("NotContains Production", new[] { application1, application2, application3, application6 }, collection);

			tenantIdFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertEquals("IsBlank", 0, collection.Count);

			tenantIdFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			collection = new EdiIdentityApplicationCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder("IsNotBlank", new[] { application1, application2, application3, application4, application5, application6 }, collection);
		}

		EdiIdentityApplication AddApplication(string clientId, string appName, bool isRollback, string status)
		{
			var application = Factory.New<EdiIdentityApplication>();
			application.IDA_ClientID = clientId;
			application.IDA_ApplicationName = appName;
			application.IDA_IsRollback = isRollback;
			application.IDA_RedirectUrlStatus = status;
			application.IDA_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			application.IDA_SystemLastEditTimeUtc = ZDateTime.BrettsBirthday;
			return application;
		}
	}
}
