using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.GSSi
{
	public class GSSiMessageTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		[TestDate(2019, 1, 1, 23, 8, 36)]
		public void TestMessage()
		{
			UPEProcessQueueLog log = Factory.New<UPEProcessQueueLog>();
			log.SetQueueDetails("BLA", "DA", "", "BLA", "GN");
			GSSiMessage msg = new GSSiMessage("1111", "AUSYD", log);
			ZString dateAsStringIgnoringSeconds = ZDateTime.UtcNow.ToString("yyyyMMddHHmm");
			string outputIgnoringSeconds = "01ERRAUSYD7340      N73401111                               04   N  " + dateAsStringIgnoringSeconds;
			string endOutput = "IN" + dateAsStringIgnoringSeconds.Left(8) + "\x0D\x0A";
			AssertEquals("Content of message", true, msg.ToString().StartsWith(outputIgnoringSeconds));
			AssertEquals("Content of message", true, msg.ToString().EndsWith(endOutput));
			log.SetQueueDetails("BLA", "BA", "", "BLA", "GN");
			msg = new GSSiMessage("1111", "AUSYD", log, "X2");
			outputIgnoringSeconds = "01ERRAUSYD7340      N73401111                               03X2 N  " + dateAsStringIgnoringSeconds;
			AssertEquals("Content of message", true, msg.ToString().StartsWith(outputIgnoringSeconds));
			AssertEquals("Content of message", true, msg.ToString().EndsWith(endOutput));
		}
	}
}
