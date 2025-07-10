using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Client.Wow
{
	public class MIHeaderCsvRecordTest : TestCaseWithFactory
	{
		public void TestUpdateTimeStampForEdiTrack()
		{
			string line = "\"0\",\"MANAGING IMPORTS\",2004-1-13,";
			MIHeaderCsvRecord mIHeader = new MIHeaderCsvRecord(line);
			AssertEquals("FieldValues[3]", ZString.Empty, mIHeader.FieldValues[3]);
			Db.Connection.BeginTransaction();
			try
			{
				mIHeader.UpdateSequenceNoForEdiTrack("AGENT");
				AssertEquals("FieldValues[3]", "0001", mIHeader.FieldValues[3]);
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}
	}
}
