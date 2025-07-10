using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Licensing;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	public class SystemUpdatePacketSenderForTest : ISystemUpdatePacketSender
	{
		public int SendCalls;
		public LicenceDatabase Db;
		public SystemUpdatePacket Packet;

		public void Send(LicenceDatabase db, SystemUpdatePacket packet)
		{
			++SendCalls;
			Db = db;
			Packet = packet;
		}
	}
}