using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.DataTransfer.Test.Universal
{
	[TestedType(typeof(CusMAWBDataObjectWriter))]
	sealed class CusMAWBDataObjectWriterTest : DataObjectWriterBaseTest
	{
		public void Test_PopulateCountrySpecificDetails()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.Code = "GBXX";
			unloco.Description = "GB Airport";
			ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow", portName: "Heathrow");
			Factory.Save();

			var storageDate = ZDateTime.Today.AddHours(1);
			var status1Date = ZDateTime.Today.AddHours(2);
			var customsActionDate = ZDateTimeOffset.Today.AddHours(3);
			var mawb = Factory.New<CusMAWB>();
			mawb.NumberOfPiecesReceived = 10000;
			mawb.Profile = "FOLIO";
			mawb.AirportOfOrigin = "GBXX";
			mawb.AirportOfArrival = "LHR";
			mawb.CargoTerminalOperatorAirportAndShed = "LHRBAC";
			mawb.CM_CustomsStatus = "XO";
			mawb.PresenceOnNetworkStatus = "COM";
			mawb.LatestCustomsActionText = "CUSTOMS ACTION";
			mawb.TemporaryStorageEndDate = storageDate;
			mawb.Status1Date = status1Date;
			mawb.Status2Granted = true;
			mawb.ShipmentDescriptionCode = "T";
			mawb.ConsignmentOrEntryType = "23";
			mawb.MasterLevelHouseHelper.CS_CustomsStatus = "XO";
			CusHAWB.LogCustomsActionCodeEvent("XO", customsActionDate, mawb.MasterLevelHouseHelper.Logs, mawb.MasterLevelHouseHelper.PK);

			var manager = (IShipmentDataContextManager)mawb.GetUniversalDataContextManager();
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			var shipment = (Shipment)writer.GetDataObject(mawb);

			AssertEquals("Number of Pieces", 10000, shipment.TotalNoOfPiecesLanded);
			AssertEquals("Folio", "FOLIO", shipment.Folio);
			AssertValueTypePair("Profile", shipment.CustomsProfileIdentifier, "PIMA", "FOLIO");
			AssertCodeDescriptionPair("Shipment Code", shipment.ShipmentSubType, "T", "Total Consignment Manifested");
			AssertCodeDescriptionPair("Shipment Type", shipment.ShipmentType, "23", "Import");
			AssertUNLOCO("Origin", shipment.PortOfLoading, "GBXX", "GB Airport");
			AssertUNLOCO("Arrival", shipment.PortOfFirstArrival, "LHR", "Heathrow");
			AssertEquals("Airport & Shed", "LHRBAC", shipment.WarehouseLocation);
			AssertCodeDescriptionPair("Customs Action Code", shipment.EntryStatus, "XO", "MASTER OPEN");
			AssertCodeDescriptionPair("Presence on Network Status", shipment.MessageStatus, "COM", "Completed on CCS-UK");
			AssertEquals("Customs Action Text", "CUSTOMS ACTION", shipment.AddInfoCollection.GetZStringValue("CustomsActionText"));
			AssertEquals("Customs Action Date", customsActionDate, shipment.AddInfoCollection.GetZDateTimeOffsetValue("CustomsActionDate"));
			AssertEquals("Storage Date", storageDate, shipment.AddInfoCollection.GetZDateTimeValue("StorageDate"));
			AssertEquals("Status 1", status1Date, shipment.AddInfoCollection.GetZDateTimeValue("Status1Date"));
			AssertEquals("Status 2", true, shipment.AddInfoCollection.GetZBoolValue("Status2"));
		}

		public void TestPopulateSplits()
		{
			var storageDate = ZDateTime.Today.AddHours(1);
			var status1Date = ZDateTime.Today.AddHours(2);
			var customsActionDate = ZDateTime.Today.AddHours(3);

			var mawb = Factory.New<CusMAWB>();
			var split = mawb.Splits.AddNew();
			split.SplitReference = "01";
			split.NumberOfPiecesExpected = 10;
			split.AgentBadge = "ABC";
			split.Weight = 1000;
			split.WeightCode = "KG";
			split.NumberOfPiecesReceived = 10;
			split.Status1Date = status1Date;
			split.LatestCustomsActionText = "Customs Action Text";
			split.SetCustomsActionCode(CustomsStatusCodes.Codes.ClearedByCustoms, customsActionDate);
			split.HandlingInformation = "Handling Info";
			split.TemporaryStorageEndDate = storageDate;

			var manager = (IShipmentDataContextManager)mawb.GetUniversalDataContextManager();
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			var mawbData = (Shipment)writer.GetDataObject(mawb);

			AssertEquals("Additional Bills Count", 1, mawbData.AdditionalBillCollection.Count);
			var additionalBill = mawbData.AdditionalBillCollection[0];
			AssertEquals("BillNumber", "01", additionalBill.BillNumber);
			AssertCodeDescriptionPair("MessageStatus", additionalBill.MessageStatus, PresenceOnNetworkList.Codes.CompletedOnCcsUk, PresenceOnNetworkList.Descriptions.CompletedOnCcsUk);
			AssertEquals("NoOfPacks", 10m, additionalBill.NoOfPacks);
			AssertEquals("Agent", "ABC", additionalBill.AddInfoCollection.GetZStringValue("Agent"));
			AssertEquals("Weight", 1000m, additionalBill.AddInfoCollection.GetZDecimalValue("Weight"));
			AssertEquals("WeightUnit", "KG", additionalBill.AddInfoCollection.GetZStringValue("WeightUnit"));
			AssertEquals("NumberOfPiecesReceived", 10m, additionalBill.AddInfoCollection.GetZDecimalValue("NumberOfPiecesReceived"));
			AssertEquals("Status1Date", status1Date, additionalBill.AddInfoCollection.GetZDateTimeValue("Status1Date"));
			AssertEquals("CustomsActionText", "Customs Action Text", additionalBill.AddInfoCollection.GetZStringValue("CustomsActionText"));
			AssertEquals("CustomsActionDate", customsActionDate, additionalBill.AddInfoCollection.GetZDateTimeValue("CustomsActionDate"));
			AssertEquals("CustomsActionCode", CustomsStatusCodes.Codes.ClearedByCustoms, additionalBill.AddInfoCollection.GetZStringValue("CustomsActionCode"));
			AssertEquals("CustomsActionCodeDescription", CustomsStatusCodes.Descriptions.ClearedByCustoms, additionalBill.AddInfoCollection.GetZStringValue("CustomsActionCodeDescription"));
			AssertEquals("HandlingInformation", "Handling Info", additionalBill.AddInfoCollection.GetZStringValue("HandlingInformation"));
			AssertEquals("StorageDate", storageDate, additionalBill.AddInfoCollection.GetZDateTimeValue("StorageDate"));
		}
	}
}
