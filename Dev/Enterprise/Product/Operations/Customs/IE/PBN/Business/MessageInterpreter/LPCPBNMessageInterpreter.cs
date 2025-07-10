using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.PBN.Messaging;

namespace Enterprise.Customs.IE.PBN.Business
{
	public class LPCPBNMessageInterpreter : InboundMessageInterpreter<LPCPBNProvider>
	{
		public LPCPBNMessageInterpreter(PBNInboundEDIMessage message, LPCPBNProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("12345678-1234-1234-1234-123456789012", "Lookup PBN Channel");

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (Res.GetString("A1B2C3D4-E5F6-7890-1234-56789ABCDEF1", "PBN ID"), provider.PbnID.ToString());
			yield return (Res.GetString("A1B2C3D4-E5F6-7890-1234-56789ABCDEF2", "Channel"), provider.Channel.ToString());
			yield return (Res.GetString("A1B2C3D4-E5F6-7890-1234-56789ABCDEF3", "Action"), provider.Action.ToString());

			if (provider.PairedTransport != null)
			{
				yield return (Res.GetString("A1B2C3D4-E5F6-7890-1234-56789ABCDEF4", "Customs Office"), provider.PairedTransport.CustomsOffice.ToString());
				yield return (Res.GetString("A1B2C3D4-E5F6-7890-1234-56789ABCDEF5", "Ship ID"), provider.PairedTransport.ShipId.ToString());
				yield return (Res.GetString("A1B2C3D4-E5F6-7890-1234-56789ABCDEF6", "Scheduled Time of Arrival"), provider.PairedTransport.ScheduledTimeofArrival.ToString());
				yield return (Res.GetString("A1B2C3D4-E5F6-7890-1234-56789ABCDEF7", "Registration Number"), provider.PairedTransport.RegistrationNumber.ToString());
			}
		}
	}
}
