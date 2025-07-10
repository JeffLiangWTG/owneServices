using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ShipmentConsolAndMasterBillNumbers.Testing
{
	[TestedType(typeof(ShipmentConsolAndMasterBillNumbers))]
	class ShipmentConsolAndMasterBillNumbersTest : BusinessObjectBaseTestCase
	{
		public void TestFieldValues()
		{
			var consol1 = TestObjectCreator.CreateConsol("AUSYD", "AUBNE", "C001");
			var consol2 = TestObjectCreator.CreateConsol("AUBNE", "NZAKL", "C002");
			var consol3 = TestObjectCreator.CreateConsol("NZAKL", "USLAX", "C003");
			var consol4 = TestObjectCreator.CreateConsol("NZAKL", "USLAX", "C004");
			var consol5 = TestObjectCreator.CreateConsol("NZAKL", "USLAX", "C005");
			var consol6 = TestObjectCreator.CreateConsol("NZAKL", "USLAX", "C006");
			var shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX");
			shipment.JS_HouseBill = "HBNUM123";
			consol1.Shipments.Add(shipment);
			consol2.Shipments.Add(shipment);
			consol3.Shipments.Add(shipment);
			consol4.Shipments.Add(shipment);
			consol5.Shipments.Add(shipment);
			consol6.Shipments.Add(shipment);
			consol1.JK_MasterBillNum = "MBNUM1";
			consol2.JK_MasterBillNum = "MBNUM2";
			consol3.JK_MasterBillNum = "MBNUM3";
			consol4.JK_MasterBillNum = "MBNUM4";
			consol5.JK_MasterBillNum = "MBNUM5";
			consol6.JK_MasterBillNum = "MBNUM6";
			consol4.JK_AgentType = Constants.AgentType.CoLoad;
			consol5.JK_AgentType = Constants.AgentType.CoLoad;
			consol6.JK_AgentType = Constants.AgentType.CoLoad;
			consol3.JK_CoLoadMasterBill = "CLD03";
			consol4.JK_CoLoadMasterBill = "CLD04";
			consol5.JK_CoLoadMasterBill = "";
			consol6.JK_CoLoadMasterBill = "CLD06";
			consol6.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			Factory.Save();
			var shipmentConsolNumbers = Factory.Load<ShipmentConsolAndMasterBillNumbers>(shipment.PK);

			AssertEquals("Consol3 should not have a CLD agent type.",true,
				consol3.JK_AgentType != Constants.AgentType.CoLoad);
			AssertContainsExactElementsInAnyOrder("VV_ConsolNumbers", new string[] { "C001", "C002", "C003", "C004", "C005", "C006" }, shipmentConsolNumbers.VV_ConsolNumbers.ToString().Split(new string[] { ", " }, StringSplitOptions.None));
			AssertContainsExactElementsInAnyOrder("VV_MasterBillNumbers", new string[] { "MBNUM1", "MBNUM2", "MBNUM3", "MBNUM4", "MBNUM5", "MBNUM6" }, shipmentConsolNumbers.VV_MasterBillNumbers.ToString().Split(new string[] { ", " }, StringSplitOptions.None));
			AssertCollectionNotContains("CLD03", shipmentConsolNumbers.VV_CoLoadMasterBillNumbers.ToString().Split(new string[] { ", " }, StringSplitOptions.None));
			AssertCollectionContains("VV_CoLoadMasterBillNumbers",
				shipmentConsolNumbers.VV_CoLoadMasterBillNumbers.ToString(), new string[] { "CLD04, CLD06", "CLD06, CLD04" });
			AssertEquals("VV_HouseBill", "HBNUM123", shipmentConsolNumbers.VV_HouseBill);
			AssertEquals("VV_UniqueConsignRef", "S001", shipmentConsolNumbers.VV_UniqueConsignRef);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			BusinessObject result = Factory.New(GetExpectedBusinessObjectType());
			return result;
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
			}
		}

		TestObjectCreator testObjectCreator;
	}
}
