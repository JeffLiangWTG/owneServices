using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.eHubMessaging.Business;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Licencing.Business
{
	public class SystemUpdatePacketEHubSender : ISystemUpdatePacketSender
	{
		public void Send(LicenceDatabase db, SystemUpdatePacket packet)
		{
			var factory = new BusinessObjectFactory();
			IOutgoingSystemMessage sender = GetSender();
			sender.Create(factory, packet.ToXMLString(), db.LicenceCodeForSystemMessage);
			factory.Save();
		}

		public virtual IOutgoingSystemMessage GetSender()
		{
			return new SystemMessage();
		}
	}
}

