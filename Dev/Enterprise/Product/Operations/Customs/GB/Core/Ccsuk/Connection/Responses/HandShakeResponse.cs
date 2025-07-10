using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public class HandShakeResponse : HandShake
	{
		public HandShakeResponse(string textPayload)
		{
			this.textPayload = textPayload;
			Parse();
		}

		public string HandShakeResponseCodeMeaning
		{
			get
			{
				return new HandShakeResponseCodes().GetDescriptionFromCode(HandShakeResponseCodeNumbers);
			}
		}

		public bool IsSuccessfulHandShake { get; private set; }

		void Parse()
		{
			if (HandShakeIdentifierOfReceivedMessage == HandShake.HandShakeIdentifier)
			{
				if (MessageType == ExpectedMessageType)
				{
					switch (HandShakeResponseCodeNumbers)
					{
						case HandShakeResponseCodes.Codes.InvalidHostMnemonic:
						case HandShakeResponseCodes.Codes.IpMismatch:
						case HandShakeResponseCodes.Codes.IpUnknown:
							IsSuccessfulHandShake = false;
							break;
						case HandShakeResponseCodes.Codes.ValidParticipant:
							IsSuccessfulHandShake = true;
							break;
						default:
							throw new Exceptions.Handshake.UnexpectedResponseCode(HandShakeResponseCodeNumbers);
					}
				}
				else
				{
					throw new Exceptions.Handshake.BadHandshakeMessageType(MessageType);
				}
			}
			else
			{
				throw new Exceptions.Handshake.BadHandshakeIdentifier(HandShakeIdentifier);
			}
		}

		public override ZString PayloadAsString { get { return textPayload; } }

		public string HandShakeIdentifierOfReceivedMessage { get { return textPayload.Substring(0, 16); } }

		public override string MessageType { get { return textPayload.Substring(16, 4); } }

		public override string Host { get { return textPayload.Substring(20, 10); } }

		public string HandShakeResponseCodeNumbers { get { return textPayload.Substring(30, 4); } }

		readonly string textPayload;

		public static string ExpectedMessageType = "0002";
	}
}
