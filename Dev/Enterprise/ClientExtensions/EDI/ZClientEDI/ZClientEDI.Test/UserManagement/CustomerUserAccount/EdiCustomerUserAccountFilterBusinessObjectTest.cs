using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.UserManagement.Testing;

[TestedType(typeof(EdiCustomerUserAccountFilterBusinessObject))]
public class EdiCustomerUserAccountFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
{
	EdiCustomerUserAccountFilterBusinessObject FilterBizO
	{
		get { return (EdiCustomerUserAccountFilterBusinessObject)CachedBusinessObject; }
	}

	public void TestSystemUserIDFilter()
	{
		const string FilterName = "System User ID";

		var device1 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
		device1.EUA_UserID = "12345";

		var device2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
		device2.EUA_UserID = "54321";

		Factory.Save();

		FilterBizO[FilterName].IsActive = true;

		var filter = (ModuleTextFilter)FilterBizO[FilterName];
		filter.IsActive = true;
		filter.Property = "12345";
		filter.SqlComparisonOperator = SQLComparisonOperator.Equal;

		var collection = new EdiCustomerUserAccountCollection(Factory);
		collection.Load(FilterBizO.Filter);
		AssertEquals(1, collection.Count);
		AssertCollectionContains(device1, collection);
		AssertCollectionNotContains(device2, collection);
	}

	public void TestUserFullNameFilter()
	{
		const string FilterName = "User FullName";

		var device1 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
		device1.EUA_FullName = "Fullname 1";

		var device2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
		device2.EUA_FullName = "Name 2";

		Factory.Save();

		FilterBizO[FilterName].IsActive = true;

		var filter = (ModuleTextFilter)FilterBizO[FilterName];
		filter.IsActive = true;
		filter.Property = "Fullname 1";
		filter.SqlComparisonOperator = SQLComparisonOperator.Equal;

		var collection = new EdiCustomerUserAccountCollection(Factory);
		collection.Load(FilterBizO.Filter);
		AssertEquals(1, collection.Count);
		AssertCollectionContains(device1, collection);
		AssertCollectionNotContains(device2, collection);
	}

	public void TestUserEmailFilter()
	{
		const string FilterName = "User Email";

		var device1 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
		device1.EUA_Email = "email@wisetech.com";

		var device2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
		device2.EUA_Email = "anotheremail@wisetech.com";

		Factory.Save();

		FilterBizO[FilterName].IsActive = true;

		var filter = (ModuleTextFilter)FilterBizO[FilterName];
		filter.IsActive = true;
		filter.Property = "email@wisetech.com";
		filter.SqlComparisonOperator = SQLComparisonOperator.Equal;

		var collection = new EdiCustomerUserAccountCollection(Factory);
		collection.Load(FilterBizO.Filter);
		AssertEquals(1, collection.Count);
		AssertCollectionContains(device1, collection);
		AssertCollectionNotContains(device2, collection);
	}

	public void TestContactRelationshipStatusFilter()
	{
		const string FilterName = "Contact Relationship Status";

		var device1 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
		device1.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.AccountReactivated;

		var device2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
		device2.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.EmailChanged;

		Factory.Save();

		FilterBizO[FilterName].IsActive = true;

		var filter = (ModuleTextFilter)FilterBizO[FilterName];
		filter.IsActive = true;
		filter.Property = ContactRelationshipStatusList.Codes.AccountReactivated;
		filter.SqlComparisonOperator = SQLComparisonOperator.Equal;

		var collection = new EdiCustomerUserAccountCollection(Factory);
		collection.Load(FilterBizO.Filter);
		AssertEquals(1, collection.Count);
		AssertCollectionContains(device1, collection);
		AssertCollectionNotContains(device2, collection);
	}

	public void TestProductFilter()
	{
		const string FilterName = "Product";

		var ld1 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld1.LD_Product = ProductTypes.Codes.CargoWiseOne;
		var device1 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
		device1.EUA_LD = ld1.PK;

		var ld2 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld2.LD_Product = ProductTypes.Codes.ProductivityWise;
		var device2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
		device2.EUA_LD = ld2.PK;

		Factory.Save();

		FilterBizO[FilterName].IsActive = true;

		var filter = (ModuleTextFilter)FilterBizO[FilterName];
		filter.IsActive = true;
		filter.Property = ProductTypes.Codes.CargoWiseOne;
		filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
		AssertEquals("Licence Database", filter.Category.ToString());

		var collection = new EdiCustomerUserAccountCollection(Factory);
		collection.Load(FilterBizO.Filter);
		AssertEquals(1, collection.Count);
		AssertCollectionContains(device1, collection);
		AssertCollectionNotContains(device2, collection);
	}

	public void TestSystemIDFilter()
	{
		const string FilterName = "Tenant ID";

		var ld1 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld1.LD_TenantID = "250885";
		var device1 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
		device1.EUA_LD = ld1.PK;

		var ld2 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld2.LD_TenantID = "040987";
		var device2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
		device2.EUA_LD = ld2.PK;

		Factory.Save();

		FilterBizO[FilterName].IsActive = true;

		var filter = (ModuleTextFilter)FilterBizO[FilterName];
		filter.IsActive = true;
		filter.Property = "250885";
		filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
		AssertEquals("Licence Database", filter.Category.ToString());

		var collection = new EdiCustomerUserAccountCollection(Factory);
		collection.Load(FilterBizO.Filter);
		AssertEquals(1, collection.Count);
		AssertCollectionContains(device1, collection);
		AssertCollectionNotContains(device2, collection);
	}

	public void TestServerCodeFilter()
	{
		const string FilterName = "Server Code";

		var ld1 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld1.LD_ServerCode = "CD1";
		var device1 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
		device1.EUA_LD = ld1.PK;

		var ld2 = Factory.NewWithValidTestData<LicenceDatabase>();
		ld2.LD_ServerCode = "SE2";
		var device2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
		device2.EUA_LD = ld2.PK;

		Factory.Save();

		FilterBizO[FilterName].IsActive = true;

		var filter = (ModuleTextFilter)FilterBizO[FilterName];
		filter.IsActive = true;
		filter.Property = "CD1";
		filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
		AssertEquals("Licence Database", filter.Category.ToString());

		var collection = new EdiCustomerUserAccountCollection(Factory);
		collection.Load(FilterBizO.Filter);
		AssertEquals(1, collection.Count);
		AssertCollectionContains(device1, collection);
		AssertCollectionNotContains(device2, collection);
	}

	public void TestUserActiveFilter()
	{
		const string FilterName = "User Active";

		var device1 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
		device1.EUA_IsActive = true;

		var device2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
		device2.EUA_IsActive = false;

		Factory.Save();

		FilterBizO[FilterName].IsActive = true;

		var filter = (ModuleFlagsFilter)FilterBizO[FilterName];
		filter.IsActive = true;
		filter.Property0 = true;
		//filter.SqlComparisonOperator = SQLComparisonOperator.Equal;

		var collection = new EdiCustomerUserAccountCollection(Factory);
		collection.Load(FilterBizO.Filter);
		AssertEquals(1, collection.Count);
		AssertCollectionContains(device1, collection);
		AssertCollectionNotContains(device2, collection);
	}

	#region Implementation

	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EdiCustomerUserAccountFilterBusinessObject();

	#endregion
}
