using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using ZClientEDI.GUI.UserManagement;

namespace Enterprise.Client.EDI.UserManagement.GUI.Testing
{
	[TestedType(typeof(CustomerUserAccountWizardFilterBusinessObject))]
	public class CustomerUserAccountWizardFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		CustomerUserAccountWizardFilterBusinessObject FilterBizO
		{
			get { return (CustomerUserAccountWizardFilterBusinessObject)CachedBusinessObject; }
		}

		public void TestLayoutContext()
		{
			var bizo = GetNewFilterStripBusinessObject() as CustomerUserAccountWizardFilterBusinessObject;
			AssertEquals("EdiCustomerUserAccountWizard", ((IFilterStripBusinessObjectInternals)bizo).LayoutContext);
		}

		public void TestEnterpriseCodeFilter()
		{
			const string FilterName = "Enterprise Code";

			var licenceEnterprise1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			licenceEnterprise1.LE_EnterpriseCode = "PKG";
			var licenceDatabase1 = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase1.LD_LE = licenceEnterprise1.PK;
			var userAccount1 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = licenceDatabase1.PK;

			var licenceEnterprise2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			licenceEnterprise2.LE_EnterpriseCode = "FMT";
			var licenceDatabase2 = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase2.LD_LE = licenceEnterprise2.PK;
			var userAccount2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = licenceDatabase2.PK;

			Factory.Save();

			FilterBizO[FilterName].IsActive = true;

			var filter = (ModuleTextFilter)FilterBizO[FilterName];
			filter.IsActive = true;
			filter.Property = "PKG";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			var collection = new EdiCustomerUserAccountCollection(Factory);
			collection.Load(FilterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(userAccount1, collection);
			AssertCollectionNotContains(userAccount2, collection);
		}

		public void TestEnterpriseIdFilter()
		{
			const string FilterName = "Enterprise ID";

			var licenceEnterprise1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			licenceEnterprise1.LE_EnterpriseID = "EN123";
			var licenceDatabase1 = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase1.LD_LE = licenceEnterprise1.PK;
			var userAccount1 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = licenceDatabase1.PK;

			var licenceEnterprise2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			licenceEnterprise2.LE_EnterpriseID = "FM456";
			var licenceDatabase2 = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase2.LD_LE = licenceEnterprise2.PK;
			var userAccount2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = licenceDatabase2.PK;

			Factory.Save();

			FilterBizO[FilterName].IsActive = true;

			var filter = (ModuleTextFilter)FilterBizO[FilterName];
			filter.IsActive = true;
			filter.Property = "EN123";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			var collection = new EdiCustomerUserAccountCollection(Factory);
			collection.Load(FilterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(userAccount1, collection);
			AssertCollectionNotContains(userAccount2, collection);
		}

		public void TestLicenceDatabaseNumberFilters()
		{
			const string FilterName = "Database Number";

			var licenceDatabase1 = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase1.LD_DatabaseNumber = 123;
			var userAccount1 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = licenceDatabase1.PK;

			var licenceDatabase2 = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase2.LD_DatabaseNumber = 456;
			var userAccount2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = licenceDatabase2.PK;

			Factory.Save();

			FilterBizO[FilterName].IsActive = true;

			var filter = (ModuleNumberRangeFilter)FilterBizO[FilterName];
			filter.IsActive = true;
			filter.Property1 = 123;

			var collection = new EdiCustomerUserAccountCollection(Factory);
			collection.Load(FilterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(userAccount1, collection);
			AssertCollectionNotContains(userAccount2, collection);
		}

		public void TestLicenceTypeFilters()
		{
			const string FilterName = "Licence Type";

			var licenceDatabase1 = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase1.LD_LicenceType = DatabaseTypes.Codes.Demo;
			var userAccount1 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = licenceDatabase1.PK;

			var licenceDatabase2 = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase2.LD_LicenceType = DatabaseTypes.Codes.Production;
			var userAccount2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = licenceDatabase2.PK;

			Factory.Save();

			FilterBizO[FilterName].IsActive = true;

			var filter = (ModuleTextFilter)FilterBizO[FilterName];
			filter.IsActive = true;
			filter.Property = DatabaseTypes.Codes.Demo;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			var collection = new EdiCustomerUserAccountCollection(Factory);
			collection.Load(FilterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(userAccount1, collection);
			AssertCollectionNotContains(userAccount2, collection);
		}

		#region Organisation

		public void TestOrganisationFilter()
		{
			const string FilterName = "Organisation";

			var oh1 = Factory.NewWithValidTestData<OrgHeader>();
			oh1.OH_Code = "FLNTSTONE";
			var oh2 = Factory.NewWithValidTestData<OrgHeader>();
			oh2.OH_Code = "JETSONS12";
			var oh3 = Factory.NewWithValidTestData<OrgHeader>();
			oh3.OH_Code = "TOMNJERRY";

			var le1 = Factory.NewWithValidTestData<OrgContact>();
			le1.OC_OH = oh1.PK;
			var le2 = Factory.NewWithValidTestData<OrgContact>();
			le2.OC_OH = oh2.PK;
			var le3 = Factory.NewWithValidTestData<OrgContact>();
			le3.OC_OH = oh3.PK;

			var ld1 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			ld1.EUA_OC_WebAccessContact = le1.PK;
			var ld2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			ld2.EUA_OC_WebAccessContact = le2.PK;
			var ld3 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			ld3.EUA_OC_WebAccessContact = le3.PK;

			Factory.Save();

			FilterBizO[FilterName].IsActive = true;

			var filter = (ModuleGuidFilter)FilterBizO[FilterName];
			filter.IsActive = true;
			filter.Property = oh1.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			var collection = new EdiCustomerUserAccountCollection(Factory);
			collection.Load(FilterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(ld1, collection);
			AssertCollectionNotContains(ld2, collection);
			AssertCollectionNotContains(ld3, collection);
		}

		#endregion

		#region Organisation Name

		public void TestOrganisationNameFilter()
		{
			const string FilterName = "Organisation Name";

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "FLNTSTONE";
			org1.OH_FullName = "The Flintstones";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "JETSONS12";
			org2.OH_FullName = "The Jetsons";
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "TOMNJERRY";
			org3.OH_FullName = "Tom & Jerry";
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			org4.OH_Code = "JETSONS98";
			org4.OH_FullName = "The Jetsons 2";
			var address4 = org4.Addresses.AddNew();
			address4.FillWithValidTestData();
			address4.OA_CompanyNameOverride = string.Empty;

			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_OH = org1.PK;
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_OH = org2.PK;
			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_OH = org3.PK;
			var contact4 = Factory.NewWithValidTestData<OrgContact>();
			contact4.OC_OH = org4.PK;
			contact4.OC_OA_OrgAddress = address4.PK;

			var user1 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			user1.EUA_OC_WebAccessContact = contact1.PK;
			var user2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			user2.EUA_OC_WebAccessContact = contact2.PK;
			var user3 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			user3.EUA_OC_WebAccessContact = contact3.PK;
			var user4 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			user4.EUA_OC_WebAccessContact = contact4.PK;

			Factory.Save();

			FilterBizO[FilterName].IsActive = true;

			var filter = (ModuleTextFilter)FilterBizO[FilterName];
			filter.IsActive = true;
			filter.Property = "The Jetsons";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			var collection = new EdiCustomerUserAccountCollection(Factory);
			collection.Load(FilterBizO.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionNotContains(user1, collection);
			AssertCollectionContains(user2, collection);
			AssertCollectionNotContains(user3, collection);
			AssertCollectionContains(user4, collection);

			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			collection.Load(FilterBizO.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(user1, collection);
			AssertCollectionNotContains(user2, collection);
			AssertCollectionContains(user3, collection);
			AssertCollectionNotContains(user4, collection);
		}

		#endregion

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CustomerUserAccountWizardFilterBusinessObject();
	}
}
