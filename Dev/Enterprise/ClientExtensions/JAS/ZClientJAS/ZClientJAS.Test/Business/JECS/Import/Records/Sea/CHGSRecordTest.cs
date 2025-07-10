using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class CHGSRecordTest : JXCRecordTestCase
	{
		public void TestIJobChargeData()
		{
			CHGSRecord record = (CHGSRecord)RecordFactory.NewRecord("CHGS3100;690;SAFe(Security Admin Fee);1.00;P;USD");
			IJobChargeData chargeData = record;
			AssertEquals("690", chargeData.ChargeCode);
			AssertEquals("SAFe(Security Admin Fee)", chargeData.ChargeDescription);
			AssertEquals("USD", chargeData.Currency);
			AssertEquals(1m, chargeData.ChargeAmount);
			Assert(!chargeData.IsCollect);
			record = (CHGSRecord)RecordFactory.NewRecord("CHGS3100;690;SAFe(Security Admin Fee);1.00;c;USD");
			chargeData = record;
			Assert(chargeData.IsCollect);
		}

		protected override JXCRecord GetNewRecord(ZString lineType, ZString lineContent)
		{
			return new CHGSRecord(lineType, lineContent);
		}
	}
}
