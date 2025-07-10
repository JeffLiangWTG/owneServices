using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal abstract class DummyHouseRecordTestCase : JXCRecordTestCase
	{
		public void TestLoadOrCreateShipment_NullParams()
		{
			DummyHouseRecord record = (DummyHouseRecord)RecordFactory.NewRecord(ExpectedLineIdentifier + ";N;TRAFFICNO#123;AUCOR;HB12345");
			AssertNull("Should return null when Consol is null", record.LoadOrCreateShipment(null, null));
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			AssertNotNull("Should return a Shipment when Consol is not null", record.LoadOrCreateShipment(consol, null));
		}

		public void TestLoadOrCreateShipment_CannotMatchExistingShipments()
		{
			AddOrgHeaderForTest();
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			DummyHouseRecord record = (DummyHouseRecord)RecordFactory.NewRecord(ExpectedLineIdentifier + ";N;TRAFFICNO#123;AUCOR;HB12345");
			AssertEquals("Pre-condition", 0, consol.Shipments.Count);
			JASForwardingShipment shipment = record.LoadOrCreateShipment(consol, NotificationBuffer);
			AssertEquals(1, consol.Shipments.Count);
			AssertEquals(shipment.PK, consol.Shipments[0].PK);
			AssertContainAttachingNewShipmentNotification(NotificationBuffer, consol);
		}

		public void TestLoadOrCreateShipment_MatchExistingShipments()
		{
			AddOrgHeaderForTest();
			JASForwardingShipment shipment1 = Factory.New<JASForwardingShipment>();
			shipment1.JS_TransportMode = TransportMode;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_HouseBill = "HB12344";
			JASForwardingShipment shipment2 = Factory.New<JASForwardingShipment>();
			shipment2.JS_TransportMode = TransportMode;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_HouseBill = "HB12345";
			JASForwardingShipment shipment3 = Factory.New<JASForwardingShipment>();
			shipment3.JS_TransportMode = TransportMode;
			shipment3.JS_RL_NKOrigin = "USATL";
			shipment3.JS_HouseBill = "HB12345";
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			DummyHouseRecord record = (DummyHouseRecord)RecordFactory.NewRecord(ExpectedLineIdentifier + ";N;TRAFFICNO#123;AUCOR;HB12345");
			Assert("Pre-condition", !consol.Shipments.Contains(shipment2));
			JASForwardingShipment newShipment = record.LoadOrCreateShipment(consol, NotificationBuffer);
			AssertEquals(shipment2.PK, newShipment.PK);
			Assert("Should be attached to the consol now", consol.Shipments.Contains(shipment2));
			AssertContainAttachingExistingShipmentNotification(NotificationBuffer, consol, shipment2);
		}

		public void TestUpdateShipment()
		{
			AddOrgHeaderForTest();
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			shipment.JS_RL_NKOrigin = "IDJKT";
			DummyHouseRecord record = (DummyHouseRecord)RecordFactory.NewRecord(ExpectedLineIdentifier + ";N;TRAFFICNO#123;AUCOR;HB12345");
			DataImportFlagChanger.LastBizO = null;
			record.UpdateShipment(shipment, null);
			AssertEquals("Should use DataImportFlagChanger", shipment, DataImportFlagChanger.LastBizO);
			AssertEquals("HB12345", shipment.JS_HouseBill);
			AssertEquals("Should be unchanged", "IDJKT", shipment.JS_RL_NKOrigin);
			AssertEquals("TRAFFICNO#123", shipment.JS_BookingReference);
			shipment.JS_RL_NKOrigin = "";
			record.UpdateShipment(shipment, null);
			AssertEquals("HB12345", shipment.JS_HouseBill);
			AssertEquals("AUSYD", shipment.JS_RL_NKOrigin);
			AssertEquals("TRAFFICNO#123", shipment.JS_BookingReference);
			record = (DummyHouseRecord)RecordFactory.NewRecord(ExpectedLineIdentifier + ";N;TRAFFICNO#456;AUCOR;" + new ZString('*', 200));
			record.UpdateShipment(shipment, null);
			AssertEquals("Should be trimmed", new ZString('*', JobShipmentSchema.JS_HouseBill.MaxLength), shipment.JS_HouseBill);
			AssertEquals("TRAFFICNO#456", shipment.JS_BookingReference);
		}

		[ExpectNoExceptions]
		public void TestUpdateShipment_ExcessivelyLongStringIsTrimmed()
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			string excessivelyLongString = new string('X', 1000);
			DummyHouseRecord record = (DummyHouseRecord)RecordFactory.NewRecord(string.Format("{0};{1};{1};{1};{1}", ExpectedLineIdentifier, excessivelyLongString));
			record.UpdateShipment(shipment, null);
		}

		protected override JXCRecord GetNewRecord(ZString lineType, ZString lineContent)
		{
			return new DummyHouseRecord(lineType, lineContent);
		}

		NotificationBuffer NotificationBuffer
		{
			get
			{
				if (fNotificationBuffer == null)
				{
					fNotificationBuffer = new NotificationBuffer();
				}

				return fNotificationBuffer;
			}
		}

		void AddOrgHeaderForTest()
		{
			JASOrgHeader orgHeader = Factory.NewWithValidTestData<JASOrgHeader>();
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OfficeCode = "AUCOR";
			Factory.Save();
		}

		ZString TransportMode
		{
			get
			{
				return ExpectedLineType == JXCConstants.LineTypes.DHAB ? Core.Constants.TransportModes.Air : Core.Constants.TransportModes.Sea;
			}
		}

		ZString ExpectedLineIdentifier
		{
			get
			{
				return ExpectedLineType + JXCConstants.Version;
			}
		}

		protected abstract ZString ExpectedLineType { get; }

		NotificationBuffer fNotificationBuffer;
	}
}
