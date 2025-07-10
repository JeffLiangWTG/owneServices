using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public class HandShakeRequest : HandShake
	{
		public HandShakeRequest(string participantIpAddress)
		{
			this.participantIpAddress = participantIpAddress;
		}
		readonly string participantIpAddress;

		public override ZString PayloadAsString
		{
			get
			{
				return HandShakeIdentifier + MessageType + Host + LocalIpForCallback + Port;
			}
		}

		public override string Host
		{
			get
			{
				return new Host().HostNameFormatted;
			}
		}

		public string LocalIpForCallback
		{
			get
			{
				return new IpAddress(participantIpAddress).IpAddressFormatted;
			}
		}

#if DEBUG
		public string IpForCallbackDeclaredByParticipantTESTonly { get; set; }
#endif

		string Port
		{
			get
			{
				return this.PortNumber.ToString().PadLeft(5, zero);
			}
		}

		public int PortNumber { set; get; }

		public override string MessageType
		{
			get { return "0001"; }
		}

		readonly char zero = '0';
	}
}
