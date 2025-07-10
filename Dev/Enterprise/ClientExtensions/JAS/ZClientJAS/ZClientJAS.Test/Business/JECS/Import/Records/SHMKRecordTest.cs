using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class SHMKRecordTest : JXCRecordTestCase
	{
		public void TestUpdateShipment()
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			SHMKRecord record = (SHMKRecord)RecordFactory.NewRecord("SHMK3100;TESTING!@#;BLAHBLAH;forty");
			Assert("Pre-condition", shipment.JS_MarksAndNumbers.IsEmpty);
			record.UpdateShipment(shipment);
			AssertEquals("TESTING!@# BLAHBLAH", shipment.JS_MarksAndNumbers);
		}

		public void TestUpdateShipment_TextShouldBeTrimmedIfExceedingMaxLength()
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			SHMKRecord record = (SHMKRecord)RecordFactory.NewRecord("SHMK3100;" + new string('X', shipment.JS_MarksAndNumbersInfo.MaxLength) + ";BLAHBLAH;forty");
			Assert("Pre-condition", shipment.JS_MarksAndNumbers.IsEmpty);
			record.UpdateShipment(shipment);
			AssertEquals(new string('X', shipment.JS_MarksAndNumbersInfo.MaxLength), shipment.JS_MarksAndNumbers);
		}

		[ExpectNoExceptions]
		public void TestUpdateShipment_NullParam()
		{
			SHMKRecord record = (SHMKRecord)RecordFactory.NewRecord("SHMK3100;TESTING!@#;BLAHBLAH;forty");
			record.UpdateShipment(null);
		}

		protected override JXCRecord GetNewRecord(ZString lineType, ZString lineContent)
		{
			return new SHMKRecord(lineType, lineContent);
		}
	}
}
