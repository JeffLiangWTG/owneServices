using CargoWise.Types;
using Enterprise.Client.JAS.Business.JXC.Testing;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class HEADRecordTest : JXCRecordTestCase
	{
		public void TestProperties()
		{
			HEADRecord record = (HEADRecord)RecordFactory.NewRecord("HEAD3100;AUADL;ITUDN;AUCOR;ITCES;AUADL");
			AssertEquals("AUCOR", record.DestinationNettingCode);
			AssertEquals("AUADL", record.DestinationOfficeCode);
			AssertEquals("ITCES", record.SendingNettingCode);
			AssertEquals("ITUDN", record.SendingOfficeCode);
			AssertEquals("AUADL", record.FreightDestination);
		}

		public void TestUpdateJXCHeaderBusinessObject()
		{
			HEADRecord record = (HEADRecord)RecordFactory.NewRecord("HEAD3100;AUADL;ITUDN;AUCOR;ITCES;AUADL");
			JXCHeaderForTest headerForTest = new JXCHeaderForTest();
			AssertNull("Pre-condition", headerForTest.SendingForwarder);
			AssertNull("Pre-condition", headerForTest.ReceivingForwarder);
			AssertEquals("Pre-condition", "", headerForTest.FreightDest);
			DataImportFlagChanger.LastBizO = null;
			record.UpdateJXCHeaderBusinessObject(headerForTest, null);
			AssertEquals("AUCOR", headerForTest.ReceivingForwarder.NettingCode);
			AssertEquals("AUADL", headerForTest.ReceivingForwarder.OfficeCode);
			AssertEquals("ITCES", headerForTest.SendingForwarder.NettingCode);
			AssertEquals("ITUDN", headerForTest.SendingForwarder.OfficeCode);
			AssertEquals("AUADL", headerForTest.FreightDest);
			AssertEquals("Should use DataImportFlagChanger", headerForTest, DataImportFlagChanger.LastBizO);
		}

		protected override JXCRecord GetNewRecord(ZString lineType, ZString lineContent)
		{
			return new HEADRecord(lineType, lineContent);
		}
	}
}
