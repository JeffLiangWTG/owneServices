using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Engine;
using Enterprise.Customs.Common;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors.HAR
{
	[UseSnapshotProtection]
	public class HARArchiveSystemIntegrationTest : TestCaseWithFactory
	{
		static readonly int OnOrBeforeMinimumValue = RegistryHelper.GetOnOrBeforeMinimumValue(ArchiveManagerConstants.Codes.HAR);

		public void TestRunArchiveSystem_CorrectlyArchivesData()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));
			shipment.JS_UniqueConsignRef = "SHP12345";

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_HCH_Header = consignmentHeader.PK;

			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_JS_LoadedOnShipment = shipment.PK;
			item.HVI_HVC_Consignment = consignment.PK;

			var itemLine = Factory.NewWithValidTestData<HVLVItemLine>();
			itemLine.HVS_HVI_HVLVItem = item.PK;
			itemLine.HVS_Quantity = 1;
			Factory.Save();

			var objectsNotToBeArchived = new List<Tuple<BusinessObject, Type>>();
			objectsNotToBeArchived.AddRange(new List<Tuple<BusinessObject, Type>>
				{
					new(item, typeof(HVLVItem)),
					new(itemLine, typeof(HVLVItemLine))
				});

			CombineAssertions("Precondition", () => AssertNotArchived(shipment, consignmentHeader, consignment, objectsNotToBeArchived));

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			var newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<ForwardingShipment>(shipment.PK);
			consignmentHeader = newFactory.Load<HVLVConsignmentHeader>(consignmentHeader.PK);
			consignment = newFactory.Load<HVLVConsignment>(consignment.PK);
			item = newFactory.Load<HVLVItem>(item.PK);
			itemLine = newFactory.Load<HVLVItemLine>(itemLine.PK);

			var objectsToBeArchived = new List<BusinessObject>();
			objectsToBeArchived.AddRange(new List<BusinessObject> { item, itemLine });

			CombineAssertions(() =>
			{
				AssertArchived(shipment, consignmentHeader, consignment, objectsToBeArchived);
				AssertLog(logger);
			});
		}

		public void TestRunArchiveSystem_WhenArchivingHVLVConsignmentWithHVLVReturnPivot()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));
			shipment.JS_UniqueConsignRef = "SHP12345";

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_HCH_Header = consignmentHeader.PK;

			var returnPivotWithFormerFK = Factory.NewWithValidTestData<HVLVReturnPivot>();
			returnPivotWithFormerFK.HVP_HVC_Former = consignment.PK;
			var returnPivotWithReturnFK = Factory.NewWithValidTestData<HVLVReturnPivot>();
			returnPivotWithReturnFK.HVP_HVC_Return = consignment.PK;
			Factory.Save();

			var objectsNotToBeArchived = new List<Tuple<BusinessObject, Type>>();
			objectsNotToBeArchived.AddRange(new List<Tuple<BusinessObject, Type>>
			{
				new(returnPivotWithFormerFK, typeof(HVLVReturnPivot)),
				new(returnPivotWithReturnFK, typeof(HVLVReturnPivot))
			});

			CombineAssertions("Precondition", () => AssertNotArchived(shipment, consignmentHeader, consignment, objectsNotToBeArchived));

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			var newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<ForwardingShipment>(shipment.PK);
			consignmentHeader = newFactory.Load<HVLVConsignmentHeader>(consignmentHeader.PK);
			consignment = newFactory.Load<HVLVConsignment>(consignment.PK);
			returnPivotWithFormerFK = newFactory.Load<HVLVReturnPivot>(returnPivotWithFormerFK.PK);
			returnPivotWithReturnFK = newFactory.Load<HVLVReturnPivot>(returnPivotWithReturnFK.PK);

			var objectsToBeArchived = new List<BusinessObject>();
			objectsToBeArchived.AddRange(new List<BusinessObject> { returnPivotWithFormerFK, returnPivotWithReturnFK });

			CombineAssertions(() =>
			{
				AssertArchived(shipment, consignmentHeader, consignment, objectsToBeArchived);
				AssertLog(logger);
			});
		}

		void AssertLog(TestArchiveLogger logger)
		{
			Assert("Logs shouldn't contain any errors.", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
			AssertEquals("Count of messages in the logs.", 20, logger.ListOfMessages.Count);

			var index = 0;

			AssertEquals($"Information|{ArchiveManagerConstants.Codes.HAR}|Registry Settings:", logger.ListOfMessages[index++]);
			AssertEquals($"Information|{ArchiveManagerConstants.Codes.HAR}|On or Before Minimum: 1", logger.ListOfMessages[index++]);
			AssertEquals($"Information|{ArchiveManagerConstants.Codes.HAR}|Set Batch Size: 100", logger.ListOfMessages[index++]);
			AssertEquals($"Information|{ArchiveManagerConstants.Codes.HAR}|Archiving File Format: CSV", logger.ListOfMessages[index++]);

			AssertEquals($"Information|{ArchiveManagerConstants.Codes.HAR}|Configuration Parameters:", logger.ListOfMessages[index++]);
			AssertContains($"Information|{ArchiveManagerConstants.Codes.HAR}|Archiving Records on or Before:", logger.ListOfMessages[index++]);
			AssertEquals($"Information|{ArchiveManagerConstants.Codes.HAR}|Max Run Duration: 10 minutes", logger.ListOfMessages[index++]);
			AssertEquals($"Information|{ArchiveManagerConstants.Codes.HAR}|Verbose Logging: No", logger.ListOfMessages[index++]);

			AssertEquals($"Information|{ArchiveManagerConstants.Codes.HAR}|Executing: HVLV Archive", logger.ListOfMessages[index++]);

			AssertContains($"Information|{ArchiveManagerConstants.Codes.HAR}|Loaded next batch of 1 JobShipment", logger.ListOfMessages[index++]);
			AssertContains($"Information|{ArchiveManagerConstants.Codes.HAR}|Loaded JobShipment 'SHP12345' and 4 related records", logger.ListOfMessages[index++]);
			AssertEquals($"Information|{ArchiveManagerConstants.Codes.HAR}|Archiving the consignments, consignment header, and items associated with JobShipment 'SHP12345'", logger.ListOfMessages[index++]);
			AssertContains($"Information|{ArchiveManagerConstants.Codes.HAR}|Deleting 3 related records from JobShipment 'SHP12345'", logger.ListOfMessages[index++]);

			AssertContains($"Information|{ArchiveManagerConstants.Codes.HAR}|Loaded next batch of 0 JobShipment", logger.ListOfMessages[index++]);

			AssertContains($"Information|{ArchiveManagerConstants.Codes.HAR}|Time taken to load 1 Archive Set(s) of JobShipment record(s) in 1 batch(es):", logger.ListOfMessages[index++]);
			AssertContains($"Information|{ArchiveManagerConstants.Codes.HAR}|Time taken to archive 1 JobShipment record(s) and their related record(s)", logger.ListOfMessages[index++]);
			AssertContains($"Information|{ArchiveManagerConstants.Codes.HAR}|JobShipments Per Hour: ", logger.ListOfMessages[index++]);
			AssertContains($"Information|{ArchiveManagerConstants.Codes.HAR}|Archived Data Dated Between Earliest Possible Date and ", logger.ListOfMessages[index++]);
			AssertEquals($"Information|{ArchiveManagerConstants.Codes.HAR}|Generated Archive Report and stored on eDocs tab of the Archive Schedule", logger.ListOfMessages[index++]);
			AssertEquals($"Information|{ArchiveManagerConstants.Codes.HAR}|Completed archiving records", logger.ListOfMessages[index++]);
		}

		public void TestRunArchiveSystem_WhenDataInvalidForArchiving_ThenDoNotArchiveData()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue - 1));

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Expected shipment to have 0 eDocs", 0, shipment.DocManagerInfo.AllEDocs.Count);
				Assert("ConsignmentHeader should not be archived", !consignmentHeader.HCH_IsArchived);
			});

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			var newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<ForwardingShipment>(shipment.PK);
			consignmentHeader = newFactory.Load<HVLVConsignmentHeader>(consignmentHeader.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Expected shipment to have 0 eDocs", 0, shipment.DocManagerInfo.AllEDocs.Count);
				Assert("Expected consignment header to not be archived", !consignmentHeader.HCH_IsArchived);

				Assert("Logs shouldn't contain any errors", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
			});
		}

		public void TestRunArchiveSystem_WhenMultipleJobShipmentsAreValidForArchiving_ThenCorrectlyArchivesData()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment1.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));
			shipment2.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var consignmentHeader1 = shipment1.GetOrCreateHVLVConsignmentHeader();
			var consignmentHeader2 = shipment2.GetOrCreateHVLVConsignmentHeader();
			var consignmentHeader3 = shipment3.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HCH_Header = consignmentHeader1.PK;
			consignment2.HVC_HCH_Header = consignmentHeader2.PK;
			consignment3.HVC_HCH_Header = consignmentHeader3.PK;

			var item1 = consignment1.Items.AddNew();
			var item2 = consignment2.Items.AddNew();
			var item3 = consignment3.Items.AddNew();
			item1.HVI_JS_LoadedOnShipment = shipment1.PK;
			item2.HVI_JS_LoadedOnShipment = shipment2.PK;
			item3.HVI_JS_LoadedOnShipment = shipment3.PK;

			var itemLine1 = item1.Lines.AddNew();
			var itemLine2 = item2.Lines.AddNew();
			var itemLine3 = item3.Lines.AddNew();
			itemLine1.HVS_Quantity = 1;
			itemLine2.HVS_Quantity = 1;
			itemLine3.HVS_Quantity = 1;

			Factory.Save();

			var objectsNotToBeArchived1 = new List<Tuple<BusinessObject, Type>>();
			objectsNotToBeArchived1.AddRange(new List<Tuple<BusinessObject, Type>>
			{
				new(item1, typeof(HVLVItem)),
				new(itemLine1, typeof(HVLVItemLine))
			});

			var objectsNotToBeArchived2 = new List<Tuple<BusinessObject, Type>>();
			objectsNotToBeArchived2.AddRange(new List<Tuple<BusinessObject, Type>>
			{
				new(item2, typeof(HVLVItem)),
				new(itemLine2, typeof(HVLVItemLine))
			});

			var objectsNotToBeArchived3 = new List<Tuple<BusinessObject, Type>>();
			objectsNotToBeArchived3.AddRange(new List<Tuple<BusinessObject, Type>>
			{
				new(item3, typeof(HVLVItem)),
				new(itemLine3, typeof(HVLVItemLine))
			});

			CombineAssertions("Precondition", () =>
			{
				AssertNotArchived(shipment1, consignmentHeader1, consignment1, objectsNotToBeArchived1);
				AssertNotArchived(shipment2, consignmentHeader2, consignment2, objectsNotToBeArchived2);
				AssertNotArchived(shipment3, consignmentHeader3, consignment3, objectsNotToBeArchived3);
			});

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			var newFactory = new BusinessObjectFactory();

			shipment1 = newFactory.Load<ForwardingShipment>(shipment1.PK);
			shipment2 = newFactory.Load<ForwardingShipment>(shipment2.PK);
			shipment3 = newFactory.Load<ForwardingShipment>(shipment3.PK);

			consignmentHeader1 = newFactory.Load<HVLVConsignmentHeader>(consignmentHeader1.PK);
			consignmentHeader2 = newFactory.Load<HVLVConsignmentHeader>(consignmentHeader2.PK);
			consignmentHeader3 = newFactory.Load<HVLVConsignmentHeader>(consignmentHeader3.PK);

			consignment1 = newFactory.Load<HVLVConsignment>(consignment1.PK);
			consignment2 = newFactory.Load<HVLVConsignment>(consignment2.PK);
			consignment3 = newFactory.Load<HVLVConsignment>(consignment3.PK);

			item1 = newFactory.Load<HVLVItem>(item1.PK);
			item2 = newFactory.Load<HVLVItem>(item2.PK);
			item3 = newFactory.Load<HVLVItem>(item3.PK);

			itemLine1 = newFactory.Load<HVLVItemLine>(itemLine1.PK);
			itemLine2 = newFactory.Load<HVLVItemLine>(itemLine2.PK);
			itemLine3 = newFactory.Load<HVLVItemLine>(itemLine3.PK);

			var objectsToBeArchived1 = new List<BusinessObject>();

			objectsToBeArchived1.AddRange(new List<BusinessObject> { item1, itemLine1 });

			var objectsToBeArchived2 = new List<BusinessObject>();

			objectsToBeArchived2.AddRange(new List<BusinessObject> { item2, itemLine2 });

			CombineAssertions(() =>
			{
				AssertArchived(shipment1, consignmentHeader1, consignment1, objectsToBeArchived1);
				AssertArchived(shipment2, consignmentHeader2, consignment2, objectsToBeArchived2);
				AssertNotArchived(shipment3, consignmentHeader3, consignment3, objectsNotToBeArchived3);

				Assert("Logs shouldn't contain any errors", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
			});
		}

		public void TestRunArchiveSystem_WhenDeletingArchivedHVLVItem_NoTrigger()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			consignmentHeader.HCH_JS_Shipment = shipment.PK;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_HCH_Header = consignmentHeader.PK;

			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			var itemLine = item.Lines.AddNew();
			itemLine.HVS_Quantity = 1;

			consignment.HVC_WeightUQ = Core.Constants.Weight.Kilograms;
			item.HVI_ManifestedWeight = 5;

			Factory.Save();

			var disableTriggerSqlText = @"ALTER TABLE dbo.HVLVItem DISABLE TRIGGER TG_HVLVItem_CalculateMeasurements_Update ";
			var enableTriggerSqlText = @"ALTER TABLE dbo.HVLVItem ENABLE TRIGGER TG_HVLVItem_CalculateMeasurements_Update ";
			var sql = @"UPDATE dbo.HVLVItem SET HVI_ManifestedWeight = 10 WHERE HVI_PK = @ItemPK ";

			_ = Db.Connection.ExecuteNonQuery(disableTriggerSqlText);

			try
			{
				using var cmd = Db.Connection.Command(sql);
				_ = cmd.AddParameter("@ItemPK", SqlDbType.UniqueIdentifier, item.PK.ToGuid());
				_ = cmd.ExecuteNonQuery();
			}
			finally
			{
				_ = Db.Connection.ExecuteNonQuery(enableTriggerSqlText);
			}

			consignment.Reload();
			item.Reload();

			CombineAssertions("Precondition: Item weight is greater than consignment", () =>
			{
				AssertEquals("Item weight should be 10", 10m, item.HVI_ManifestedWeight);
				AssertEquals("Consignment weight should be 5", 5m, consignment.HVC_ManifestedWeight);
			});

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			AssertNoExceptionThrown(() => TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule));

			var newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<ForwardingShipment>(shipment.PK);
			consignmentHeader = newFactory.Load<HVLVConsignmentHeader>(consignmentHeader.PK);
			consignment = newFactory.Load<HVLVConsignment>(consignment.PK);
			item = newFactory.Load<HVLVItem>(item.PK);
			itemLine = newFactory.Load<HVLVItemLine>(itemLine.PK);

			var objectsToBeArchived = new List<BusinessObject>();
			objectsToBeArchived.AddRange(new List<BusinessObject> { item, itemLine });

			CombineAssertions(() =>
			{
				AssertArchived(shipment, consignmentHeader, consignment, objectsToBeArchived);
				Assert("Logs shouldn't contain any errors.", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
			});
		}

		public void TestRunArchiveSystem_WhenItemNotHookedToJobShipment_ItemShouldStillBeArchived()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			consignmentHeader.HCH_JS_Shipment = shipment.PK;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_HCH_Header = consignmentHeader.PK;

			var itemOnShipment = consignment.Items.AddNew();
			itemOnShipment.HVI_JS_LoadedOnShipment = shipment.PK;

			var itemNotOnShipment = consignment.Items.AddNew();

			var itemLine1 = itemOnShipment.Lines.AddNew();
			var itemLine2 = itemNotOnShipment.Lines.AddNew();
			itemLine1.HVS_Quantity = 1;
			itemLine2.HVS_Quantity = 1;

			Factory.Save();

			var objectsNotToBeArchived = new List<Tuple<BusinessObject, Type>>();
			objectsNotToBeArchived.AddRange(new List<Tuple<BusinessObject, Type>>
			{
				new(itemOnShipment, typeof(HVLVItem)),
				new(itemLine1, typeof(HVLVItemLine))
			});

			CombineAssertions("Precondition", () =>
			{
				AssertNotArchived(shipment, consignmentHeader, consignment, objectsNotToBeArchived);
				AssertType(typeof(HVLVItem), itemNotOnShipment);
				AssertType(typeof(HVLVItemLine), itemLine2);
			});

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			AssertNoExceptionThrown(() => TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule));

			var newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<ForwardingShipment>(shipment.PK);
			consignmentHeader = newFactory.Load<HVLVConsignmentHeader>(consignmentHeader.PK);
			consignment = newFactory.Load<HVLVConsignment>(consignment.PK);
			itemOnShipment = newFactory.Load<HVLVItem>(itemOnShipment.PK);
			itemNotOnShipment = newFactory.Load<HVLVItem>(itemNotOnShipment.PK);
			itemLine1 = newFactory.Load<HVLVItemLine>(itemLine1.PK);
			itemLine2 = newFactory.Load<HVLVItemLine>(itemLine2.PK);

			var objectsToBeArchived = new List<BusinessObject>();
			objectsToBeArchived.AddRange(new List<BusinessObject> { itemOnShipment, itemLine1 });

			CombineAssertions(() =>
			{
				AssertArchived(shipment, consignmentHeader, consignment, objectsToBeArchived);
				AssertNull("Item not on shipment should be deleted", itemNotOnShipment);
				AssertNull("Line should be deleted", itemLine2);
			});
		}

		public void TestRunArchiveSystem_WhenConsignmentHasACusEntryNumber_CusEntryNumberIsArchived()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_HCH_Header = consignmentHeader.PK;

			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;
			item.HVI_HVC_Consignment = consignment.PK;

			var itemLine = item.Lines.AddNew();
			itemLine.HVS_HVI_HVLVItem = item.PK;
			itemLine.HVS_Quantity = 1;

			var cusEntryNumber = consignment.CustomsReferenceNumbers.AddNew();
			cusEntryNumber.CE_ParentID = consignment.PK;

			Factory.Save();

			var objectsNotToBeArchived = new List<Tuple<BusinessObject, Type>>();
			objectsNotToBeArchived.AddRange(new List<Tuple<BusinessObject, Type>>
			{
				new(item, typeof(HVLVItem)),
				new(itemLine, typeof(HVLVItemLine))
			});

			CombineAssertions("Precondition", () =>
			{
				AssertNotArchived(shipment, consignmentHeader, consignment, objectsNotToBeArchived);
				AssertType(typeof(CusEntryNumber), cusEntryNumber);
			});

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			var newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<ForwardingShipment>(shipment.PK);
			consignmentHeader = newFactory.Load<HVLVConsignmentHeader>(consignmentHeader.PK);
			consignment = newFactory.Load<HVLVConsignment>(consignment.PK);
			item = newFactory.Load<HVLVItem>(item.PK);
			itemLine = newFactory.Load<HVLVItemLine>(itemLine.PK);
			cusEntryNumber = newFactory.Load<CusEntryNumber>(cusEntryNumber.PK);

			var objectsToBeArchived = new List<BusinessObject>();
			objectsToBeArchived.AddRange(new List<BusinessObject> { item, itemLine });

			CombineAssertions(() =>
			{
				AssertArchived(shipment, consignmentHeader, consignment, objectsToBeArchived);
				AssertNull("Expected CusEntryNumber to be deleted", cusEntryNumber);

				Assert("Logs shouldn't contain any errors", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
				AssertCollectionContains("Archiving Items Associated with JobShipment Log", $"Information|HAR|Archiving the consignments, consignment header, and items associated with JobShipment '{shipment.JS_UniqueConsignRef}'", logger.ListOfMessages);
			});
		}

		public void TestRunArchiveSystem_GivenConsignmentsAndItemsNotAttachedToAnyShipment_ThenDoNotArchiveThoseConsignmentsAndItems()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue - 1));

			var consignmentHeader = Factory.NewWithValidTestData<HVLVConsignmentHeader>();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_HCH_Header = consignmentHeader.PK;

			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_HVC_Consignment = consignment.PK;

			var line = Factory.NewWithValidTestData<HVLVItemLine>();
			line.HVS_HVI_HVLVItem = item.PK;
			line.HVS_Quantity = 1;
			Factory.Save();

			var objectsNotToBeArchived = new List<Tuple<BusinessObject, Type>>();
			objectsNotToBeArchived.AddRange(new List<Tuple<BusinessObject, Type>>
			{
				new(item, typeof(HVLVItem)),
				new(line, typeof(HVLVItemLine))
			});

			CombineAssertions("Precondition", () => AssertNotArchived(shipment, consignmentHeader, consignment, objectsNotToBeArchived));

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			shipment.Reload();
			consignmentHeader.Reload();
			consignment.Reload();
			item.Reload();
			line.Reload();

			CombineAssertions(() =>
			{
				AssertNotArchived(shipment, consignmentHeader, consignment, objectsNotToBeArchived);
				Assert("Logs shouldn't contain any errors", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
			});
		}

		public void TestRunArchiveSystem_GivenConsignmentAndItemWithNoItemLine_ThenArchiveConsignmentAndItem()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(OnOrBeforeMinimumValue + 1));

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_HCH_Header = consignmentHeader.PK;

			var item1 = Factory.NewWithValidTestData<HVLVItem>();
			var item2 = Factory.NewWithValidTestData<HVLVItem>();
			var item3 = Factory.NewWithValidTestData<HVLVItem>();
			item1.HVI_HVC_Consignment = consignment.PK;
			item2.HVI_HVC_Consignment = consignment.PK;
			item3.HVI_HVC_Consignment = consignment.PK;
			item1.HVI_JS_LoadedOnShipment = shipment.PK;
			item2.HVI_JS_LoadedOnShipment = shipment.PK;
			item3.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Expected shipment to have 0 eDocs", 0, shipment.DocManagerInfo.AllEDocs.Count);

				Assert("Expected Consignment header to not be archived", !consignmentHeader.HCH_IsArchived);
				AssertType(typeof(HVLVConsignment), consignment);
				AssertType(typeof(HVLVItem), item1);
				AssertType(typeof(HVLVItem), item2);
				AssertType(typeof(HVLVItem), item3);
			});

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, logger, schedule);

			var newFactory = new BusinessObjectFactory();

			shipment = newFactory.Load<ForwardingShipment>(shipment.PK);

			consignmentHeader = newFactory.Load<HVLVConsignmentHeader>(consignmentHeader.PK);
			consignment = newFactory.Load<HVLVConsignment>(consignment.PK);

			item1 = newFactory.Load<HVLVItem>(item1.PK);
			item2 = newFactory.Load<HVLVItem>(item2.PK);
			item3 = newFactory.Load<HVLVItem>(item3.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Expected shipment to have one eDoc", 1, shipment.DocManagerInfo.AllEDocs.Count);

				Assert("Expected Consignment header to be archived", consignmentHeader.HCH_IsArchived);
				AssertNull("Expected consignment to be deleted", consignment);
				AssertNull("Expected Item to be deleted", item1);
				AssertNull("Expected Item to be deleted", item2);
				AssertNull("Expected Item to be deleted", item3);
			});
		}

		void AssertArchived(ForwardingShipment shipment, HVLVConsignmentHeader consignmentHeader, HVLVConsignment consignment, List<BusinessObject> objectsToBeDeleted)
		{
			AssertEquals("Expected shipment to have one eDoc", 1, shipment.DocManagerInfo.AllEDocs.Count);

			Assert("Expected consignment header to be archived.", consignmentHeader.HCH_IsArchived);
			AssertNull("Expected consignment to be deleted.", consignment);

			foreach (var item in objectsToBeDeleted)
			{
				AssertNull("Expected object to be deleted.", item);
			}
		}

		void AssertNotArchived(ForwardingShipment shipment, HVLVConsignmentHeader consignmentHeader, HVLVConsignment consignment, List<Tuple<BusinessObject, Type>> objectsNotToBeDeleted)
		{
			AssertEquals("Expected shipment to have no eDocs", 0, shipment.DocManagerInfo.AllEDocs.Count);

			Assert("Expected consignment header not to be archived.", !consignmentHeader.HCH_IsArchived);
			AssertType("Expected consignment not to be deleted.", typeof(HVLVConsignment), consignment);

			foreach (var item in objectsNotToBeDeleted)
			{
				AssertType("Expected object not to be deleted.", item.Item2, item.Item1);
			}
		}
	}
}
