using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSCAPivot))]
	sealed class CusSCAPivotTest : EnterpriseBusinessObjectTestCase
	{
		#region Deleted Event

		public void TestDeleted()
		{
			AssertDeletedEvent((pivot) =>
			{
				pivot.Delete();
			});
		}

		public void TestDeletedFromDataRefresh()
		{
			AssertDeletedEvent((pivot) =>
			{
				var newFactory = NewFactory();

				var pivotInNewFactory = newFactory.Load<CusSCAPivot>(pivot.PK);
				pivotInNewFactory.Delete();

				newFactory.Save();
			});
		}

		public void TestParentHouseDeleted()
		{
			var house = Factory.New<CusSCAHouse>();
			var pivot = Factory.New<CusSCAPivot>();
			pivot.CV_CA = house.PK;
			AssertEquals("House bill retrieved", pivot.HouseBill.PK, house.PK);
			house.Delete();
			AssertNull("Deleted house bill not reachable", pivot.HouseBill);
		}

		void AssertDeletedEvent(Action<CusSCAPivot> deleteAction)
		{
			var masterBill = Factory.New<CusSCAOceanBill>();

			var houseBill = masterBill.HouseBills.AddNew();
			houseBill.CA_HouseBill = "HB00000";

			var container = masterBill.Containers.AddNew();
			var pivot = container.Pivots.AddNew();

			var isDeletedCalled = false;

			Factory.Save();

			pivot.Deleted += (s, e) => isDeletedCalled = true;

			Assert("Precondition.", !houseBill.IsDeleted);
			Assert("Precondition.", !isDeletedCalled);

			deleteAction(pivot);

			Assert("Should be deleted from data refresh.", pivot.IsDeleted);
			Assert("Call Deleted EventHandler", isDeletedCalled);
		}

		#endregion

		#region TestDynamicallyAssigningContainerNumber
		public void TestSettingCV_CN_UpdatesOldAndNewContainersPivots() => CombineAssertions(() =>
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var container1 = oceanBill.Containers.AddNew();
			container1.CN_ContainerNumber = "1";
			var container2 = oceanBill.Containers.AddNew();
			container2.CN_ContainerNumber = "1";
			var containerPivot1 = container1.Pivots.AddNew();
			var containerPivot2 = container1.Pivots.AddNew();
			AssertEquals("Initially - CV_CN", container1.PK, containerPivot1.CV_CN);
			AssertEquals("Initially - container1.Pivots contains containerPivot1", true, container1.Pivots.Contains(containerPivot1));
			AssertEquals("Initially - container1.Pivots contains containerPivot2", true, container1.Pivots.Contains(containerPivot2));
			containerPivot1.CV_CN = container2.PK;
			AssertEquals("After - CV_CN", container2.PK, containerPivot1.CV_CN);
			AssertEquals("After - container1.Pivots doesn't contain containerPivot1", false, container1.Pivots.Contains(containerPivot1));
			AssertEquals("After - container1.Pivots still contains containerPivot2", true, container1.Pivots.Contains(containerPivot2));
			AssertEquals("After - container2.Pivots contains containerPivot1", true, container2.Pivots.Contains(containerPivot1));
		});

		public void TestDynamicallyAssigningContainerNumber()
		{
			CusSCAPivot testPivot = (CusSCAPivot)GetNewBusinessObject();

			CusSCAPivot containerPivot = (CusSCAPivot)GetNewBusinessObject();

			CusSCAOceanBill oceanBill = testPivot.OceanBill;

			CusSCAContainer container1 = oceanBill.Containers.AddNew();
			container1.CN_ContainerNumber = containerNumber1;
			CusSCAContainer container2 = oceanBill.Containers.AddNew();
			container2.CN_ContainerNumber = containerNumber2;

			containerPivot.CV_CN = container1.PK;
			testPivot.CV_AssociatedContainer = containerNumber2;
			AssertEquals("Attaching testPivot to container2", container2.PK, testPivot.CV_CN);
			AssertEquals("container2.Pivots contains testPivot", true, container2.Pivots.Contains(testPivot));

			testPivot.CV_AssociatedContainer = containerNumber1;
			AssertEquals("Attaching testPivot to container1", container1.PK, testPivot.CV_CN);
			AssertEquals("container1.Pivots contains testPivot", true, container1.Pivots.Contains(testPivot));
			AssertEquals("container2.Pivots doesn't contain testPivot", false, container2.Pivots.Contains(testPivot));

			testPivot.CV_AssociatedContainer = newContainerNumber;
			AssertEquals("Invalid Container Number should have no Container", ZGuid.Empty, testPivot.CV_CN);

			CusSCAContainer container3 = oceanBill.Containers.AddNew();
			container3.CN_ContainerNumber = newContainerNumber;
			testPivot.CV_AssociatedContainer = newContainerNumber;
			AssertEquals("Attaching testPivot to container3", container3.PK, testPivot.CV_CN);
			AssertEquals("New Container should be associated", newContainerNumber, testPivot.CV_AssociatedContainer);
			AssertEquals("container3.Pivots contains testPivot", true, container3.Pivots.Contains(testPivot));
			AssertEquals("container1.Pivots doesn't contain testPivot", false, container1.Pivots.Contains(testPivot));
			AssertEquals("container2.Pivots doesn't contain testPivot", false, container2.Pivots.Contains(testPivot));
		}
		#endregion

		#region TestCV_AssociatedHouse
		public void TestCV_AssociatedHouse()
		{
			CusSCAOceanBill ocean = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = ocean.HouseBills.AddNew();
			house.CA_HouseBill = "Cuckoo Squeaker";
			CusSCAContainer container = ocean.Containers.AddNew();
			CusSCAPivot pivot = container.Pivots.AddNew();
			AssertEquals("Pre-condition - FKey should be emptyt", ZGuid.Empty, pivot.CV_CA);
			AssertEquals("pivot.CV_AssociatedHouseInfo.MaxLength", CusSCAHouse.Schema.CA_HouseBillMaxLength, pivot.CV_AssociatedHouseInfo.MaxLength);
			pivot.CV_AssociatedHouse = "Cuckoo Squeaker";
			AssertEquals("FKey should have been set", house.PK, pivot.CV_CA);
			AssertEquals("Pivots should be equal", house.Pivot[0], container.Pivots[0]);
		}
		#endregion

		#region FindHouseByNumber

		public void FindHouseByNumber()
		{
			CusSCAOceanBill ocean = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house1 = ocean.HouseBills.AddNew();
			house1.CA_HouseBill = "00001";
			CusSCAHouse house2 = ocean.HouseBills.AddNew();
			house2.CA_HouseBill = "00002";
			CusSCAContainer container = ocean.Containers.AddNew();
			CusSCAPivotHelper pivot = (CusSCAPivotHelper)container.Pivots.AddNew();
			CusSCAHouse testHouse = pivot.FindHouseByNumberTest("00002");
			AssertEquals(house2, testHouse);
		}
		#endregion

		#region TestIsAutoLogged

		public void TestIsAutoLogged()
		{
			CusSCAPivotHelper houseContainerPivot = Factory.New<CusSCAPivotHelper>();
			AssertEquals("CusSCAPivot should be autologged", true, houseContainerPivot.IsAutoLogged);
		}

		#endregion

		#region Helper

		class CusSCAPivotHelper : CusSCAPivot
		{
			public CusSCAPivotHelper(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public CusSCAHouse FindHouseByNumberTest(ZString houseBillNumber)
			{
				return base.FindHouseByNumber(houseBillNumber);
			}

			public new bool IsAutoLogged
			{
				get { return base.IsAutoLogged; }
			}
		}
		#endregion

		#region TestCV_OceanBillHouseBills_List

		public void TestCV_OceanBillHouseBills_List()
		{
			CusSCAOceanBill ocean = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house1 = ocean.HouseBills.AddNew();
			house1.CA_HouseBill = "11111";
			CusSCAContainer container = ocean.Containers.AddNew();
			CusSCAPivot pivot = container.Pivots.AddNew();
			AssertEquals("11111", pivot.CV_OceanBillHouseBills_List.CodesAsString);
		}
		#endregion

		#region TestDeletePivotBeforeContainers

		[ExpectNoExceptions()]
		public void TestDeletePivotBeforeContainers()
		{
			CusSCAPivot testPivot = (CusSCAPivot)GetNewBusinessObject();
			CusSCAOceanBill oceanBill = testPivot.OceanBill;
			CusSCAHouse houseBill = testPivot.HouseBill;

			CusSCAContainer container1 = oceanBill.Containers.AddNew();
			container1.CN_ContainerNumber = containerNumber1;
			CusSCAContainer container2 = oceanBill.Containers.AddNew();
			container2.CN_ContainerNumber = containerNumber2;

			testPivot.CV_AssociatedContainer = containerNumber2;

			Factory.Save();

			houseBill.Pivot.RemoveAndDeleteAll();
			oceanBill.Containers.RemoveAndDeleteAll();

			testPivot = houseBill.Pivot.AddNew();
			container1 = oceanBill.Containers.AddNew();
			container1.CN_ContainerNumber = containerNumber1;
			container2 = oceanBill.Containers.AddNew();
			container2.CN_ContainerNumber = containerNumber2;
			testPivot.CV_AssociatedContainer = containerNumber2;

			Factory.Save();
		}
		#endregion

		#region TestHouseBill

		public void TestHouseBill()
		{
			var testPivot = (CusSCAPivot)GetNewBusinessObject();
			var houseBill1 = testPivot.HouseBill;
			AssertSame("HouseBill is cached", houseBill1, testPivot.HouseBill);

			testPivot.CV_CA = ZGuid.Empty;
			AssertNull("Setting CV_CA to empty will clear the HouseBill value", testPivot.HouseBill);

			var houseBill2 = testPivot.OceanBill.HouseBills.AddNew();
			testPivot.CV_CA = houseBill2.PK;
			AssertSame("Changing CV_CA will load a different house bill", houseBill2, testPivot.HouseBill);
		}

		#endregion

		#region TestOceanBillIsReturned

		public void TestOceanBillIsReturned()
		{
			CusSCAOceanBill ocean1 = Factory.New<CusSCAOceanBill>();
			CusSCAOceanBill ocean2 = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = ocean1.HouseBills.AddNew();
			CusSCAContainer container = ocean2.Containers.AddNew();
			CusSCAPivot pivot1 = house.Pivot.AddNew();
			CusSCAPivot pivot2 = container.Pivots.AddNew();
			AssertEquals(ocean1, pivot1.OceanBill);
			AssertEquals(ocean2, pivot2.OceanBill);
		}
		#endregion

		#region TestBreakBulkAndBulkContainer

		public void TestBreakBulkAndBulkContainer()
		{
			const string ContainerNumber1 = "GFDS1029321";
			CusSCAPivot houseContainerPivot = (CusSCAPivot)GetNewBusinessObject();
			var oceanBill = houseContainerPivot.OceanBill;
			AssertEquals("Initial countainers count", 1, oceanBill.Containers.Count);
			houseContainerPivot.CV_AssociatedContainer = CusSCAPivot.BreakBulk;

			AssertEquals("Container BREAK BULK added via CV_AssociatedContainer", 2, oceanBill.Containers.Count);
			AssertEquals("Container CN_ContainerTypeInfo ReadOnly", true, houseContainerPivot.CN_ContainerTypeInfo.ReadOnly);
			AssertEquals("Container CN_ContainerModeInfo ReadOnly", true, houseContainerPivot.CN_ContainerModeInfo.ReadOnly);
			AssertEquals("Container CN_ContainerNumberInfo ReadOnly", true, houseContainerPivot.CN_ContainerNumberInfo.ReadOnly);
			AssertEquals("Container CN_SealNumberInfo ReadOnly", true, houseContainerPivot.CN_SealNumberInfo.ReadOnly);
			AssertEquals("Container CN_ShipperOwnedContainerInfo ReadOnly", true, houseContainerPivot.CN_ShipperOwnedContainerInfo.ReadOnly);
			AssertEquals("Container CV_AssociatedContainerInfo ReadOnly", true, houseContainerPivot.CV_AssociatedContainerInfo.ReadOnly);
			AssertEquals("Container CV_AssociatedContainer", CusSCAPivot.BreakBulk, houseContainerPivot.CV_AssociatedContainer);

			CusSCAContainer container1 = houseContainerPivot.OceanBill.Containers.AddNew();
			container1.CN_ContainerNumber = ContainerNumber1;
			AssertEquals("Containers count after adding container1", 3, oceanBill.Containers.Count);

			houseContainerPivot.CV_AssociatedContainer = ContainerNumber1;
			AssertEquals("Containers count after setting CN_ContainerNumber=ContainerNumber1", 3, oceanBill.Containers.Count);
			AssertEquals("Container CN_ContainerTypeInfo ReadOnly", false, houseContainerPivot.CN_ContainerTypeInfo.ReadOnly);
			AssertEquals("Container CN_ContainerModeInfo ReadOnly", false, houseContainerPivot.CN_ContainerModeInfo.ReadOnly);
			AssertEquals("Container CN_ContainerNumberInfo ReadOnly", false, houseContainerPivot.CN_ContainerNumberInfo.ReadOnly);
			AssertEquals("Container CN_SealNumberInfo ReadOnly", false, houseContainerPivot.CN_SealNumberInfo.ReadOnly);
			AssertEquals("Container CN_ShipperOwnedContainerInfo ReadOnly", false, houseContainerPivot.CN_ShipperOwnedContainerInfo.ReadOnly);
			AssertEquals("Container CV_AssociatedContainerInfo ReadOnly", false, houseContainerPivot.CV_AssociatedContainerInfo.ReadOnly);
			AssertNotNull("Container reference", houseContainerPivot.Container);
			CusSCAContainer pivotContainer = houseContainerPivot.Container;

			houseContainerPivot.CV_AssociatedContainer = CusSCAPivot.Bulk;
			AssertEquals("Container BULK added via CV_AssociatedContainer", 4, oceanBill.Containers.Count);
			AssertEquals("Container CN_ContainerTypeInfo ReadOnly", true, houseContainerPivot.CN_ContainerTypeInfo.ReadOnly);
			AssertEquals("Container CN_ContainerModeInfo ReadOnly", true, houseContainerPivot.CN_ContainerModeInfo.ReadOnly);
			AssertEquals("Container CN_ContainerNumberInfo ReadOnly", true, houseContainerPivot.CN_ContainerNumberInfo.ReadOnly);
			AssertEquals("Container CN_SealNumberInfo ReadOnly", true, houseContainerPivot.CN_SealNumberInfo.ReadOnly);
			AssertEquals("Container CN_ShipperOwnedContainerInfo ReadOnly", true, houseContainerPivot.CN_ShipperOwnedContainerInfo.ReadOnly);
			AssertEquals("Container CV_AssociatedContainerInfo ReadOnly", true, houseContainerPivot.CV_AssociatedContainerInfo.ReadOnly);
			AssertEquals("Container CV_AssociatedContainer", CusSCAPivot.Bulk, houseContainerPivot.CV_AssociatedContainer);

			houseContainerPivot.CV_AssociatedContainer = CusSCAPivot.Liquid;
			AssertEquals("Container LIQUID added via CV_AssociatedContainer", 5, oceanBill.Containers.Count);
			AssertEquals("Container CN_ContainerTypeInfo ReadOnly", true, houseContainerPivot.CN_ContainerTypeInfo.ReadOnly);
			AssertEquals("Container CN_ContainerModeInfo ReadOnly", true, houseContainerPivot.CN_ContainerModeInfo.ReadOnly);
			AssertEquals("Container CN_ContainerNumberInfo ReadOnly", true, houseContainerPivot.CN_ContainerNumberInfo.ReadOnly);
			AssertEquals("Container CN_SealNumberInfo ReadOnly", true, houseContainerPivot.CN_SealNumberInfo.ReadOnly);
			AssertEquals("Container CN_ShipperOwnedContainerInfo ReadOnly", true, houseContainerPivot.CN_ShipperOwnedContainerInfo.ReadOnly);
			AssertEquals("Container CV_AssociatedContainerInfo ReadOnly", true, houseContainerPivot.CV_AssociatedContainerInfo.ReadOnly);
			AssertEquals("Container CV_AssociatedContainer", CusSCAPivot.Liquid, houseContainerPivot.CV_AssociatedContainer);

			houseContainerPivot.CV_AssociatedContainer = ContainerNumber1;
			AssertEquals("Containers count after setting CN_ContainerNumber=ContainerNumber1", 5, oceanBill.Containers.Count);
			AssertEquals("Container CN_ContainerTypeInfo ReadOnly", false, houseContainerPivot.CN_ContainerTypeInfo.ReadOnly);
			AssertEquals("Container CN_ContainerModeInfo ReadOnly", false, houseContainerPivot.CN_ContainerModeInfo.ReadOnly);
			AssertEquals("Container CN_ContainerNumberInfo ReadOnly", false, houseContainerPivot.CN_ContainerNumberInfo.ReadOnly);
			AssertEquals("Container CN_SealNumberInfo ReadOnly", false, houseContainerPivot.CN_SealNumberInfo.ReadOnly);
			AssertEquals("Container CN_ShipperOwnedContainerInfo ReadOnly", false, houseContainerPivot.CN_ShipperOwnedContainerInfo.ReadOnly);
			AssertEquals("Container CV_AssociatedContainerInfo ReadOnly", false, houseContainerPivot.CV_AssociatedContainerInfo.ReadOnly);
			AssertEquals("Container CV_AssociatedContainer", ContainerNumber1, houseContainerPivot.CV_AssociatedContainer);

			houseContainerPivot.CV_AssociatedContainer = CusSCAPivot.Bulk;
			AssertEquals("Containers count after setting CN_ContainerNumber=BULK", 5, oceanBill.Containers.Count);
			AssertEquals("Container CV_AssociatedContainerInfo ReadOnly", true, houseContainerPivot.CV_AssociatedContainerInfo.ReadOnly);
			AssertEquals("Container CV_AssociatedContainer", CusSCAPivot.Bulk, houseContainerPivot.CV_AssociatedContainer);
		}
		#endregion

		#region TestContainerTypeProperty
		[ExpectNoExceptions]
		public void TestContainerTypeProperty()
		{
			const string ContainerNumber1 = "GFDS1029321";
			CusSCAPivot houseContainerPivot = (CusSCAPivot)GetNewBusinessObject();
			houseContainerPivot.OceanBill.Containers[0].CN_ContainerNumber = ContainerNumber1;
			houseContainerPivot.CV_AssociatedContainer = ContainerNumber1;
			houseContainerPivot.CN_ContainerType = "";
			houseContainerPivot.CN_ContainerType = "1234";
			houseContainerPivot.CN_ContainerType = "1234324";
			houseContainerPivot.CN_ContainerType = "40GP";
		}
		#endregion

		#region ValidationPromotion

		public void TestValidationPromotion()
		{
			bool validation = EnvProxy.Instance.Registry.LightValidationEnabled;
			EnvProxy.Instance.Registry.LightValidationEnabled = true;
			try
			{
				var oceanBill = Factory.New<CusSCAOceanBill>();
				var container = oceanBill.Containers.AddNew();
				var container1 = oceanBill.Containers.AddNew();
				var house = oceanBill.HouseBills.AddNew();
				var pivot = house.Pivot.AddNew();
				var pivot1 = house.Pivot.AddNew();
				pivot.CV_CN = container.PK;
				pivot.CV_CN = container1.PK;
				pivot.MarkLightValidationAsValidForTesting();
				pivot1.MarkLightValidationAsValidForTesting();
				container.CN_ContainerNumber = "1";
				pivot.CV_AssociatedContainer = container.CN_ContainerNumber;
				Assert(!pivot.LightValidationIsValid);
				Assert(!pivot1.LightValidationIsValid);
			}
			finally
			{
				EnvProxy.Instance.Registry.LightValidationEnabled = validation;
			}
		}

		#endregion

		#region TestShouldDefaultContainerNoAndHouseBillToMarksAndNumbers

		public void TestShouldDefaultContainerNoAndHouseBillToMarksAndNumbers()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_HouseBill = "IANTESTSHOUSE";

			var container = oceanBill.Containers.AddNew();
			container.CN_ContainerNumber = "IANTESTCONT";
			container.CN_ContainerMode = Core.Constants.ContainerModes.LCL;

			var pivot = container.Pivots.AddNew();
			pivot.CV_CA = houseBill.PK;
			AssertEquals("No shipment related to this ocean bill", false, pivot.ShouldDefaultContainerNoAndHouseBillToMarksAndNumbers);

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			houseBill.CA_JS = shipment.PK;
			AssertEquals("Shipment type is not HLS or HVL", false, pivot.ShouldDefaultContainerNoAndHouseBillToMarksAndNumbers);

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			AssertEquals(true, pivot.ShouldDefaultContainerNoAndHouseBillToMarksAndNumbers);

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			AssertEquals(true, pivot.ShouldDefaultContainerNoAndHouseBillToMarksAndNumbers);

			container.CN_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Container mode should be LCL", false, pivot.ShouldDefaultContainerNoAndHouseBillToMarksAndNumbers);

			pivot.CV_AssociatedContainer = CusSCAPivot.BreakBulk;
			AssertEquals(true, pivot.ShouldDefaultContainerNoAndHouseBillToMarksAndNumbers);
		}

		#endregion

		public void TestSynchronisedFieldsAreReadOnlyWhenOverrideFreightDefaultsIsEnabled()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = oceanBill.HouseBills.AddNew();
			var pivot = houseBill.Pivot.AddNew();
			AssertEquals(true, oceanBill.OverrideFreightDefaults);

			AssertEquals(false, pivot.CV_AssociatedContainerInfo.ReadOnly);
			AssertEquals(false, pivot.CV_AssociatedHouseInfo.ReadOnly);
			AssertEquals(false, pivot.CV_GoodsDescriptionInfo.ReadOnly);
			AssertEquals(false, pivot.CV_MarksAndNumbersInfo.ReadOnly);
			AssertEquals(false, pivot.CV_NetWeightInfo.ReadOnly);
			AssertEquals(false, pivot.CV_PackageCountInfo.ReadOnly);
			AssertEquals(false, pivot.CV_VolumeInfo.ReadOnly);
			AssertEquals(false, pivot.CV_WeightInfo.ReadOnly);
			AssertEquals(false, pivot.CV_WeightUQInfo.ReadOnly);

			var consol = Factory.New<ForwardingConsol>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			AssertEquals(false, oceanBill.OverrideFreightDefaults);

			AssertEquals(true, pivot.CV_AssociatedContainerInfo.ReadOnly);
			AssertEquals(true, pivot.CV_AssociatedHouseInfo.ReadOnly);
			AssertEquals(true, pivot.CV_GoodsDescriptionInfo.ReadOnly);
			AssertEquals(true, pivot.CV_MarksAndNumbersInfo.ReadOnly);
			AssertEquals(true, pivot.CV_NetWeightInfo.ReadOnly);
			AssertEquals(true, pivot.CV_PackageCountInfo.ReadOnly);
			AssertEquals(true, pivot.CV_VolumeInfo.ReadOnly);
			AssertEquals(true, pivot.CV_WeightInfo.ReadOnly);
			AssertEquals(true, pivot.CV_WeightUQInfo.ReadOnly);

			oceanBill.OverrideFreightDefaults = true;
			AssertEquals(false, pivot.CV_AssociatedContainerInfo.ReadOnly);
			AssertEquals(false, pivot.CV_AssociatedHouseInfo.ReadOnly);
			AssertEquals(false, pivot.CV_GoodsDescriptionInfo.ReadOnly);
			AssertEquals(false, pivot.CV_MarksAndNumbersInfo.ReadOnly);
			AssertEquals(false, pivot.CV_NetWeightInfo.ReadOnly);
			AssertEquals(false, pivot.CV_PackageCountInfo.ReadOnly);
			AssertEquals(false, pivot.CV_VolumeInfo.ReadOnly);
			AssertEquals(false, pivot.CV_WeightInfo.ReadOnly);
			AssertEquals(false, pivot.CV_WeightUQInfo.ReadOnly);
		}

		[StressTest]
		public void TestPerformanceWhenDeleteCusPivots()
		{
			var count = 2_000;
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var container = oceanBill.Containers.AddNew();
			Enumerable.Range(0, count).ForEach(id =>
			{
				var houseBill = oceanBill.HouseBills.AddNew();
				houseBill.CA_HouseBill = $"HBL{id}";
				var pivot = houseBill.Pivot.AddNew();
				pivot.CV_CN = container.PK;
			});

			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_ParentTableCode = container.TablePrefix;
			underbond.C4_ParentID = container.PK;

			Factory.Save();

			AssertEquals("We have only one underbond.", 1, oceanBill.AllUnderbonds.Count);
			AssertEquals("The underbond is linked to container.", container.PK, oceanBill.AllUnderbonds[0].C4_ParentID);

			var newFactory = new BusinessObjectFactory();
			var oceanBillInNewFactory = newFactory.Load<CusSCAOceanBill>(oceanBill.PK);
			_ = oceanBillInNewFactory.AllUnderbonds;
			var dbHitCount = newFactory.GetTableHitCount(CusUnderbondSchema.Constants.TableName);
			oceanBillInNewFactory.HouseBills.Cast<CusSCAHouse>().ForEach(houseBill =>
			{
				houseBill.Pivot.DeleteAll();
				var pivot = houseBill.Pivot.AddNew();
				pivot.CV_CN = container.PK;
			});
			var dbHitCountAgain = newFactory.GetTableHitCount(CusUnderbondSchema.Constants.TableName);
			AssertEquals("Shoud not access the table CusUnderbond cuz no pivot have underbond linked.", dbHitCount, dbHitCountAgain);
		}

		public void TestStatusNeedsRecalculation()
		{
			var newFactory = new BusinessObjectFactory();
			var oceanBill = newFactory.New<CusSCAOceanBill>();
			var container = oceanBill.Containers.AddNew();
			var house = oceanBill.HouseBills.AddNew();
			var pivot = house.Pivot.AddNew();
			pivot.CV_CN = container.PK;
			newFactory.Save();

			var cMROceanBill = Factory.Load<CusSCAOceanBill>(oceanBill.PK);
			house = cMROceanBill.HouseBills[0];
			pivot = house.Pivot[0];
			Factory.Save();
			AssertEquals("StatusNeedsRecalculation", false, ((IStatusNeedsRecalculationProvider)pivot).StatusNeedsRecalculation);
			AssertEquals("Do not hit the DB to check StatusNeedsRecalculation if we haven't touched the Messages collection.", 0, Factory.GetTableHitCount(EDIMessageSchema.Constants.TableName));
			pivot.Messages.AddNew();
			pivot.Messages[0].HasChanges = true;
			AssertEquals("StatusNeedsRecalculation", true, ((IStatusNeedsRecalculationProvider)pivot).StatusNeedsRecalculation);
		}

		public void TestMoveCARSTMessagesToHouse()
		{
			var ocean = Factory.New<CusSCAOceanBill>();
			var house = ocean.HouseBills.AddNew();
			var pivot = house.Pivot.AddNew();
			pivot.Messages.AddNew();
			pivot.Delete();
			AssertEquals(1, house.Messages.Count);
		}

		public void TestLogIfCV_CargoStatusChanged()
		{
			var ocean = Factory.NewWithValidTestData<CusSCAOceanBill>();
			var house = Factory.New<CusSCAHouse>();
			ocean.HouseBills.Add(house);
			var container = ocean.Containers.AddNew();
			var pivot = house.Pivot.AddNew();
			container.Pivots.Add(pivot);
			Factory.Save();
			pivot.CV_CargoStatus = "CLR";
			Factory.Save();
			AssertEquals(1, new LogsForNominatedEvent(pivot.Logs, Events.CustomsEntryStatus).Count);
		}

		public void TestAttachAnyUnattachedHouseCARSTS()
		{
			var message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3JFD 9FB6 D7B5:1+8'
DTM+9:20050726162613590090:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES'
FTX+AHN+++LCL UNDERBOND SATISFIED:YES'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'
FTX+AHN+++IAR AQIS CLEARED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'
FTX+AHN+++ACS EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+436S++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+MB:OB0987123'
RFF+BH:HB4000'
RFF+AAQ:C001'
DOC+1'
PAC+++LCL:67:95'
UNT+34+000001'".Replace("\r\n", "");

			message.SetEM_LinkedObject();

			AssertEquals(CusSCAOceanBill.Schema.TableName, message.EM_LinkTable);
			AssertEquals("OB0987123", message.EM_ApplicationReference);
			AssertEquals("HB4000", message.EM_MessageOwner);

			var oceanBill = Factory.NewWithValidTestData<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OB0987123";
			var container = oceanBill.Containers.AddNew();
			container.CN_ContainerNumber = "C001";
			var house = oceanBill.HouseBills.AddNew();
			house.CA_HouseBill = "HB4000";
			var pivot = house.Pivot.AddNew();
			container.Pivots.Add(pivot);
			pivot.IsGettingUnattachedCARSTSFromLocalCacheForTest = true;

			pivot.StatusCalculator.DeriveStatusNow();
			AssertEquals("Pre-condition", 0, pivot.Messages.Count);
			AssertEquals("Pre-condition", CMRBaseStatuses.Codes.NotSent, pivot.CV_CargoStatus);

			Factory.Save();

			AssertEquals(1, pivot.Messages.Count);
			AssertEquals("HLD", pivot.CV_CargoStatus);
			AssertEquals("HLD", house.CA_ShipmentStatus);
		}

		public void TestHumanReadableName()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house = oceanBill.HouseBills.AddNew();
			house.CA_HouseBill = "HB1";
			var container = oceanBill.Containers.AddNew();
			container.CN_ContainerNumber = "CN1";
			var packing = house.Pivot.AddNew();
			packing.CV_CN = container.PK;
			AssertEquals("Packing(CN:CN1 HBL:HB1)", packing.HumanReadableName);
		}

		public void TestContainerList()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house = oceanBill.HouseBills.AddNew();
			var pivot = house.Pivot.AddNew();
			AssertEquals(0, pivot.CV_OceanBillContainers_List.Count);

			var container = oceanBill.Containers.AddNew();
			AssertEquals(0, pivot.CV_OceanBillContainers_List.Count);

			container.CN_ContainerNumber = "234987";
			AssertEquals(1, pivot.CV_OceanBillContainers_List.Count);
		}

		public void TestUsesTranshipmentPortOnUnderbond()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house = oceanBill.HouseBills.AddNew();
			var pivot = house.Pivot.AddNew();

			Assert(((ICusUnderbondDependentCollectionParent)pivot).UsesTranshipmentPortOnUnderbond);
			house.CA_RL_NK_PortOfDestination = ZString.Empty;
			AssertEquals(ZString.Empty, ((ICusUnderbondDependentCollectionParent)pivot).DefaultTranshipmentPort);
			house.CA_RL_NK_PortOfDestination = "NZAKL";
			AssertEquals("NZAKL", ((ICusUnderbondDependentCollectionParent)pivot).DefaultTranshipmentPort);

			var underbond = pivot.Underbonds.AddNew();
			house.CA_RL_NK_PortOfDestination = "AUSYD";
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.Transshipment;
			AssertEquals(ZString.Empty, underbond.C4_RL_NKTranshipDestPort);
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.OtherMovement;
			AssertEquals(ZString.Empty, underbond.C4_RL_NKTranshipDestPort);
			house.CA_RL_NK_PortOfDestination = "NZAKL";
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.Transshipment;
			AssertEquals("NZAKL", underbond.C4_RL_NKTranshipDestPort);
			underbond.C4_RL_NKTranshipDestPort = ZString.Empty;
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.OtherMovement;
			AssertEquals(ZString.Empty, underbond.C4_RL_NKTranshipDestPort);
		}

		public void TestValidation()
		{
			CusSCAOceanBill oB = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = oB.HouseBills.AddNew();
			CusSCAPivot pivot = house.Pivot.AddNew();

			AssertEquals("ValidationType", typeof(CusSCAPivotValidation), pivot.Validation.GetType());
		}

		public void TestCanSendWithoutDelay()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			var pivot = house.Pivot.AddNew();
			Assert("Should be delayed because No CARSTS were found", !((ICusUnderbondDependentCollectionParent)pivot).CanSendWithoutDelay);
			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageType = CMRMessage.CMRMessageTypes.CARST;
			house.Messages.Add(message);
			Assert("Should not be delayed due to carst.", ((ICusUnderbondDependentCollectionParent)pivot).CanSendWithoutDelay);
		}

		public void TestCargoStatus()
		{
			CusSCAOceanBill cMROceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = cMROceanBill.Containers.AddNew();
			CusSCAHouse house = cMROceanBill.HouseBills.AddNew();
			CusSCAPivot pivot = house.Pivot.AddNew();

			AssertEquals("NOT", ((IOutturnableLine)pivot).CargoStatus);
			pivot.CV_CargoStatus = "HBW";
			AssertEquals("HBW", ((IOutturnableLine)pivot).CargoStatus);
		}

		public void TestLookups()
		{
			CusSCAOceanBill cMROceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = cMROceanBill.Containers.AddNew();
			CusSCAHouse house = cMROceanBill.HouseBills.AddNew();
			CusSCAPivot pivot = house.Pivot.AddNew();
			AssertEquals("LookupsType", typeof(CusSCAPivotLookups), pivot.Lookups.GetType());
		}

		public void TestCanDelete()
		{
			var cMROceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = cMROceanBill.Containers.AddNew();
			CusSCAHouse house = cMROceanBill.HouseBills.AddNew();
			var pivot = house.Pivot.AddNew();
			AssertEquals("New Pivots should be able to be deleted", true, pivot.CanDelete);

			CusUnderbond underbond = pivot.Underbonds.AddNew();
			underbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			AssertEquals("Underbond count", 1, pivot.Underbonds.Count);
			AssertEquals("Underbond should not be deletable", false, underbond.CanDelete);
			AssertEquals("Pivot should not be able to be deleted as it has an attached underbond", false, pivot.CanDelete);

			underbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.NotSent;
			AssertEquals("Underbond should now be deletable", true, underbond.CanDelete);
			AssertEquals("Pivot should be able to be deleted now that underbond has been deleted", true, pivot.CanDelete);
		}

		public void TestCanBeDeletedHouseAcknowledged()
		{
			CusSCAOceanBill cMROceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = cMROceanBill.HouseBills.AddNew();
			var pivot = house.Pivot.AddNew();
			CusSCAContainer container1 = cMROceanBill.Containers.AddNew();
			container1.CN_ContainerNumber = "CONTAINER1";
			pivot.CV_CN = container1.PK;
			house.CA_MessageStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			Factory.Save();
			AssertEquals("Pivot can not be deleted because it is associated with an acknoledged house", false, pivot.CanDelete);
			var pivot2 = house.Pivot.AddNew();
			AssertEquals("Pivot not in database can be deleted", true, pivot2.CanDelete);
		}

		public void TestDeleteCanBeDeletedException()
		{
			var cMROceanBill = Factory.New<CusSCAOceanBill>();
			var container = cMROceanBill.Containers.AddNew();
			var house = cMROceanBill.HouseBills.AddNew();
			var pivot = house.Pivot.AddNew();
			pivot.CV_CN = container.PK;
			AssertEquals("New Pivots should be able to be deleted", true, pivot.CanDelete);

			var underbond = pivot.Underbonds.AddNew();
			underbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			Factory.Save();
			AssertEquals("Underbond count", 1, pivot.Underbonds.Count);
			AssertEquals("Underbond should not be deletable", false, underbond.CanDelete);
			AssertEquals("Pivot should not be able to be deleted as it has an attached underbond", false, pivot.CanDelete);
			Assert(!pivot.CanDelete);

			underbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.NotSent;
			house.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			AssertEquals("A packing line cannot be deleted as the house bill has an active cargo report", false, pivot.CanDelete);
			Assert(!pivot.CanDelete);

			house.CA_MessageStatus = CMRBaseStatuses.Codes.WithdrawalAccepted;
			pivot.Delete();
			AssertEquals("Pivot should have been deleted", true, pivot.IsDeleted);
			AssertEquals("Underbond should have been deleted", true, underbond.IsDeleted);
		}

		public void TestIOutturnableLinePackagesManifested()
		{
			CusSCAOceanBill cMROceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = cMROceanBill.Containers.AddNew();
			CusSCAHouse house = cMROceanBill.HouseBills.AddNew();
			var pivot_House = house.Pivot.AddNew();
			var pivot_Container = container.Pivots.AddNew();
			AssertEquals(((IOutturnableLine)pivot_Container.Container).PackagesManifested, ((IOutturnableLine)pivot_Container).PackagesManifested);
			AssertEquals(0, ((IOutturnableLine)pivot_House).PackagesManifested);
		}

		public void TestIsHeldAtOutturn()
		{
			var pivot = Factory.New<CusSCAPivot>();
			pivot.CV_IsHeldAtOutturn = true;
			AssertEquals(true, ((ICargoDepotEventParent)pivot).IsHeldAtOutturn);

			((ICargoDepotEventParent)pivot).IsHeldAtOutturn = false;
			AssertEquals(false, pivot.CV_IsHeldAtOutturn);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = oceanBill.Containers.AddNew();
			container.CN_ContainerNumber = "1";
			CusSCAHouse houseBill = oceanBill.HouseBills.AddNew();
			CusSCAPivot houseContainerPivot = houseBill.Pivot.AddNew();
			houseContainerPivot.CV_CN = container.PK;
			AssertNotNull(houseContainerPivot);
			return houseContainerPivot;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		readonly ZString containerNumber1 = "M123345";
		readonly ZString containerNumber2 = "K2029301";
		readonly ZString newContainerNumber = "R2023034";

		#endregion
	}
}
