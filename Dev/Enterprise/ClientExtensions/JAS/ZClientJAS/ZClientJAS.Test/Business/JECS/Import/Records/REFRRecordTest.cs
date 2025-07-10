using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class REFRRecordTest : JXCRecordTestCase
	{
		public void TestUpdateShipment_ReferenceFromShipper()
		{
			REFRRecord record = (REFRRecord)RecordFactory.NewRecord("REFR3100;REF 12348;S");
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			AssertEquals("Pre-condition", "", shipment.JS_BookingReference);
			record.UpdateShipment(shipment);
			AssertEquals("REF 12348", shipment.JS_BookingReference);
		}

		public void TestUpdateShipment_ReferenceShouldBeTrimmedIfExceedingMaxLength()
		{
			REFRRecord record = (REFRRecord)RecordFactory.NewRecord("REFR3100;THIS IS WAY TOOOOOOOO LONG TO BE  A REFERENCE NUMBER;S");
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			AssertEquals("Pre-condition", "", shipment.JS_BookingReference);
			record.UpdateShipment(shipment);
			string expected = new ZString("THIS IS WAY TOOOOOOOO LONG TO BE  A REFERENCE NUMBER").Left(shipment.JS_BookingReferenceInfo.MaxLength);
			AssertEquals(expected, shipment.JS_BookingReference);
		}

		public void TestUpdateShipment_ReferenceFromConsignee()
		{
			REFRRecord record = (REFRRecord)RecordFactory.NewRecord("REFR3100;REF 12348;C");
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			AssertEquals("Pre-condition", "", shipment.JS_BookingReference);
			record.UpdateShipment(shipment);
			AssertEquals("Should not be populated to here. Only consignee reference is ignored at the moment", "", shipment.JS_BookingReference);
		}

		protected override JXCRecord GetNewRecord(ZString lineType, ZString lineContent)
		{
			return new REFRRecord(lineType, lineContent);
		}
	}
}
