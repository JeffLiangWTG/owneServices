using System.IO;
using System.Text.Json;
using CargoWise.Customs.IE.MessageDefinitions.PBN;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.PBN.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	[TestedType(typeof(LPCPBNMessageInterpreter))]
	sealed class LPCPBNMessageInterpreterTest : InboundMessageInterpreterAbstractTest<PBNInboundEDIMessage, LPCPBNMessageInterpreter, LPCPBNProvider>
	{
		protected override ZString MessageType => PBNMessageTypes.Codes.LookupPBNChannel;

		protected override ZString GetExpectedInterpretation(PBNInboundEDIMessage message) => @"Lookup PBN Channel<br />
            <br />
            <table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
                <tr><td>PBN ID</td><td>CD456HIJ</td></tr>
                <tr><td>Channel</td><td>Online</td></tr>
                <tr><td>Action</td><td>Update</td></tr>
                <tr><td>Customs Office</td><td>DUB</td></tr>
                <tr><td>Ship ID</td><td>SHIP123</td></tr>
                <tr><td>Scheduled Time of Arrival</td><td>2025-01-23T09:41:00</td></tr>
                <tr><td>Registration Number</td><td>REG456</td></tr>
            </table>";

		protected override PBNInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<AsycudaManifestHeader>();

			var message = Factory.New<PBNInboundEDIMessage>();
			message.EM_LinkedObject = declaration;
			var lpcText = JsonSerializer.Serialize(LPCMessageForTest());
			message.EM_MessageText = lpcText;

			return message;
		}

		protected override LPCPBNProvider GetProvider(TextReader reader)
		{
			var lpcDefinition = JsonSerializer.Deserialize<LPCDefinition>(reader.ReadToEnd());
			return new LPCPBNProvider(lpcDefinition);
		}

		static LPCDefinition LPCMessageForTest()
		{
			return new LPCDefinition
			{
				PbnID = "CD456HIJ",
				Channel = "Online",
				Action = "Update",
				PairedTransport = new PBNPairedTransport
				{
					CustomsOffice = "DUB",
					ShipId = "SHIP123",
					ScheduledTimeofArrival = "2025-01-23T09:41:00",
					RegistrationNumber = "REG456"
				}
			};
		}
	}
}
