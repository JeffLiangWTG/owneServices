using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.RulesEngine.Facts.Testing
{
	public class LandTransportJobWarehouseFactTest : TestCase
	{
		public void TestPlugInNull_ThrowsException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new LandTransportJobWarehouseFact(null, null));
		}

		public void TestWarehouseCode()
		{
			var warehouseCode = "XXX";

			var warehouseJobFact = CreateWarehouseJobFactWithValidTestData(warehouseCode);

			var warehouseFact = warehouseJobFact.Warehouse;
			AssertNotNull(warehouseFact);
			AssertType<FactLeftJoin<IWarehouseFact>>(warehouseFact);
			AssertNotNull(warehouseFact.Fact);
			AssertEquals(warehouseCode, warehouseFact.Fact.Code);
		}

		public void TestTransportClientCode()
		{
			var transportClientCode = "TST";
			var warehouseJobFact = CreateWarehouseJobFactWithValidTestData(transportClientCode: transportClientCode);

			AssertEquals(transportClientCode, warehouseJobFact.TransportClientCode);
		}

		public void TestPK()
		{
			var warehouseJobFact = CreateWarehouseJobFactWithValidTestData();

			AssertNotNull(warehouseJobFact.PK);
		}

		public void TestTransportClientCode_AndWareHouseCode_ShouldBeNull_WhenWarehouseOrderIsNotIWhsOrder()
		{
			var warehouseOrder = Factory.NewWithValidTestData<WhsReceive>();

			var consignment = Factory.NewWithValidTestData<DtbConsignment>();

			var warehouseJobFact = new LandTransportJobWarehouseFact(consignment, warehouseOrder);

			AssertNull(warehouseJobFact.TransportClientCode);
			AssertNotNull(warehouseJobFact.Warehouse);
			AssertNull(warehouseJobFact.Warehouse.Fact);
		}

		LandTransportJobWarehouseFact CreateWarehouseJobFactWithValidTestData(string warehouseCode = "", string transportClientCode = "")
		{
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = warehouseCode;

			var warehouseOrder = Factory.NewWithValidTestData<WhsOrder>();
			warehouseOrder.WD_WW_Whs = warehouse.PK;

			var consignment = Factory.NewWithValidTestData<DtbConsignment>();

			var transportCompany = Factory.NewWithValidTestData<OrgHeader>();
			transportCompany.OH_Code = transportClientCode;
			var transportOrgAddress = transportCompany.Addresses.AddNewMainAddress();
			JobDocAddress.GetOrCreateNonPersistantDocAddress(warehouseOrder, MasterFiles.Integration.DocAddressType.TransportCompanyDocumentaryAddress, transportOrgAddress.PK);

			return new LandTransportJobWarehouseFact(consignment, warehouseOrder);
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;
	}
}
