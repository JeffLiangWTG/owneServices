using CargoWise.Data;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.Wow
{
	public class WowNumberFountainsTest : TestCaseWithFactory
	{
		public void TestWowEdiTrackMessageID()
		{
			Db.Connection.BeginTransaction();
			try
			{
				WowNumberFountains wowNF = WowNumberFountains.Instance;
				AssertEquals("First WOW ediTrackMessageId for 'ABC':", "1", wowNF.GetWowEdiTrackMessageID("ABC").GetNextFormatted(Db.Connection));
				AssertEquals("Secong WOW ediTrackMessageId for 'XYZ':", "1", wowNF.GetWowEdiTrackMessageID("XYZ").GetNextFormatted(Db.Connection));
				AssertEquals("Secong WOW ediTrackMessageId for 'XYZ':", "2", wowNF.GetWowEdiTrackMessageID("XYZ").GetNextFormatted(Db.Connection));
				AssertEquals("Secong WOW ediTrackMessageId for 'ABC':", "2", wowNF.GetWowEdiTrackMessageID("ABC").GetNextFormatted(Db.Connection));
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}
	}
}
