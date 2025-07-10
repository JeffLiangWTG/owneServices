using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DeviceManagement.Module.Testing
{
	[TestedType(typeof(ClientDeviceHeaderFilterBusinessObject))]
	public class ClientDeviceHeaderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ClientDeviceHeaderFilterBusinessObject();
		public void TestOrganisationFilter()
		{
			const string FilterName = "Organisation";
			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var enterprise1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise1.LE_OH = org1.PK;
			enterprise1.LE_EnterpriseCode = "EN1";
			var enterprise2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise2.LE_OH = org1.PK;
			enterprise2.LE_EnterpriseCode = ZString.Empty;
			var enterprise3 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise3.LE_OH = org2.PK;
			enterprise3.LE_EnterpriseCode = "EN2";
			var database1 = Factory.NewWithValidTestData<LicenceDatabase>();
			database1.LD_LE = enterprise1.PK;
			database1.LD_ServerCode = "DB1";
			var database2 = Factory.NewWithValidTestData<LicenceDatabase>();
			database2.LD_LE = enterprise2.PK;
			database2.LD_ServerCode = "DB2";
			var device1 = Factory.NewWithValidTestData<ClientDeviceHeader>();
			device1.CDH_EnterpriseCode = enterprise1.LE_EnterpriseCode;
			var device2 = Factory.NewWithValidTestData<ClientDeviceHeader>();
			device2.CDH_EnterpriseCode = enterprise3.LE_EnterpriseCode;
			var device3 = Factory.NewWithValidTestData<ClientDeviceHeader>();
			Factory.Save();
			FilterBizO[FilterName].IsActive = true;
			((ModuleGuidFilter)FilterBizO[FilterName]).Property = org1.PK;
			var collection = new ClientDeviceHeaderCollection(Factory);
			collection.AdditionalFilter = FilterBizO.Filter;
			AssertEquals(1, collection.Count);
			AssertCollectionContains(device1, collection);
		}

		public void TestHardwareIdFilter()
		{
			const string FilterName = "Hardware ID";
			var device1 = Factory.NewWithValidTestData<ClientDeviceHeader>();
			device1.CDH_DeviceIdentifier = "12345";
			var device2 = Factory.NewWithValidTestData<ClientDeviceHeader>();
			device2.CDH_DeviceIdentifier = "54321";
			Factory.Save();
			FilterBizO[FilterName].IsActive = true;
			var filter = (ModuleTextFilter)FilterBizO[FilterName];
			filter.IsActive = true;
			filter.Property = "12345";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			var collection = new ClientDeviceHeaderCollection(Factory);
			collection.AdditionalFilter = FilterBizO.Filter;
			AssertEquals(1, collection.Count);
			AssertCollectionContains(device1, collection);
			AssertCollectionNotContains(device2, collection);
		}

		public void TestIdentifierFilter()
		{
			const string FilterName = "Identifier";
			var device1 = Factory.NewWithValidTestData<ClientDeviceHeader>();
			device1.CDH_Identifier = "12345";
			var device2 = Factory.NewWithValidTestData<ClientDeviceHeader>();
			device2.CDH_Identifier = "54321";
			Factory.Save();

			FilterBizO[FilterName].IsActive = true;
			var filter = (ModuleTextFilter)FilterBizO[FilterName];
			filter.IsActive = true;
			filter.Property = "12345";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			var collection = new ClientDeviceHeaderCollection(Factory);
			collection.AdditionalFilter = FilterBizO.Filter;
			AssertEquals(1, collection.Count);
			AssertCollectionContains(device1, collection);
			AssertCollectionNotContains(device2, collection);
		}

		public void TestDeviceKindFilter()
		{
			const string FilterName = "Device Kind";
			var device1 = Factory.NewWithValidTestData<ClientDeviceHeader>();
			device1.CDH_DeviceKind = ClientDeviceHeaderLookups.Kinds.AppleMobility;
			var device2 = Factory.NewWithValidTestData<ClientDeviceHeader>();
			device2.CDH_DeviceKind = ClientDeviceHeaderLookups.Kinds.Android;
			Factory.Save();
			FilterBizO[FilterName].IsActive = true;
			var filter = (ModuleTextFilter)FilterBizO[FilterName];
			filter.IsActive = true;
			filter.Property = ClientDeviceHeaderLookups.Kinds.AppleMobility;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			var collection = new ClientDeviceHeaderCollection(Factory);
			collection.AdditionalFilter = FilterBizO.Filter;
			AssertEquals(1, collection.Count);
			AssertCollectionContains(device1, collection);
			AssertCollectionNotContains(device2, collection);
		}

		ClientDeviceHeaderFilterBusinessObject FilterBizO
		{
			get
			{
				return (ClientDeviceHeaderFilterBusinessObject)CachedBusinessObject;
			}
		}
	}
}
