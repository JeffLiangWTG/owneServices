using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	sealed class CusHAWBLoaderWithMutexManagementTest : TestCaseWithFactory
	{
		public void TestLoad()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var shipment2 = Factory.New<ForwardingShipment>();
			var cusMAWB = Factory.New<CusMAWB>();
			cusMAWB.CM_JK = consol.PK;
			var cusHAWB = cusMAWB.ChildBills.AddNew();
			cusHAWB.CS_JS = shipment.PK;
			var cusHAWB2 = Factory.New<CusHAWB>();
			cusHAWB2.CS_JS = shipment2.PK;
			cusHAWB2.CS_ApplicationCode = "CMR";
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var shipment1Loaded = factory2.Load<ForwardingShipment>(shipment.PK);
			var shipment2Loaded = factory2.Load<ForwardingShipment>(shipment2.PK);
			var cusHAWB1Loaded = factory2.Load<Business.CusHAWB>(cusHAWB.PK); //happens when shipment is shown on a form
			var cusHAWB2Loaded = factory2.Load<Business.CusHAWB>(cusHAWB2.PK); //happens when shipment is shown on a form
			AssertEquals(cusHAWB.PK, new CusHAWBLoaderWithMutexManagement(shipment1Loaded).Load(cusMAWB).PK);
			shipment1Loaded.JS_HouseBill = "34890234";
			AssertNotNull("PreCondition", shipment1Loaded.AirCargoSynchroniser);
			AssertEquals("House bill synchronised", "34890234", cusHAWB1Loaded.CS_HAWB);
			Assert(cusHAWB1Loaded.MAWB.ChildBills.Contains(cusHAWB1Loaded));
			AssertEquals(cusHAWB2Loaded, new CusHAWBLoaderWithMutexManagement(shipment2Loaded).Load(null));
			shipment2Loaded.JS_HouseBill = "34890235";
			AssertNotNull("PreCondition", shipment2Loaded.AirCargoSynchroniser);
			AssertEquals("House bill synchronised", "34890235", cusHAWB2Loaded.CS_HAWB);
		}

		public void TestCreateWithMutext()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var cusMAWB = Factory.New<CusMAWB>();
			cusMAWB.CM_JK = consol.PK;
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var shipmentLoaded = factory2.Load<ForwardingShipment>(shipment.PK);
			var loader = new CusHAWBLoaderWithMutexManagement(shipmentLoaded);
			var loader2 = new CusHAWBLoaderWithMutexManagement(shipment);
			var loader3 = new CusHAWBLoaderWithMutexManagement(shipment);
			try
			{
				var cusHAWBCreated = loader.CreateWithMutexLock(cusMAWB);
				AssertNotNull(cusHAWBCreated);
				Assert("Not saved yet", !cusHAWBCreated.IsInDatabase);
				AssertEquals(1, cusMAWB.FilteredChildBills.Count);
				var cusHAWBCreated2 = loader2.CreateWithMutexLock(cusMAWB);
				AssertNull("Should not create another cusHAWB while mutex is locked", cusHAWBCreated2);
				shipmentLoaded.Factory.Save();
				var cusHAWBCreated3 = loader3.LoadOrCreate(cusMAWB);
				AssertEquals("Should Load cusHAWB that is just saved after mutex is released", cusHAWBCreated.PK, cusHAWBCreated3.PK);
			}
			finally
			{
				loader.UnlockMutexIfNecessary();
				loader2.UnlockMutexIfNecessary();
				loader3.UnlockMutexIfNecessary();
			}
		}
	}
}
