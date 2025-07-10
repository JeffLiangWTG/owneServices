using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Environment;
using Enterprise.Licensing;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	public class SystemUpdatePacketMailSenderTest : TestCaseWithFactory
	{
		public void TestSend()
		{
			LicenceDatabase database = Factory.New<LicenceDatabase>();
			database.LD_PublicEmailAddressForUpdate = "test@test.com";
			SystemUpdatePacket packet = new SystemUpdatePacket();
			var sender = new SystemUpdatePacketMailSender();
			sender.Send(database, packet);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
		}
	}
}
