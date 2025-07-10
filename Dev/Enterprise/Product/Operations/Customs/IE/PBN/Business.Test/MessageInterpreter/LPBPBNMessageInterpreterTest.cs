using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CargoWise.Customs.IE.MessageDefinitions.PBN;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.PBN.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	[TestedType(typeof(LPBPBNMessageInterpreter))]
	sealed class LPBPBNMessageInterpreterTest : InboundMessageInterpreterAbstractTest<PBNInboundEDIMessage, LPBPBNMessageInterpreter, LPBPBNProvider>
	{
		protected override ZString MessageType => PBNMessageTypes.Codes.LookupPBN;

		protected override ZString GetExpectedInterpretation(PBNInboundEDIMessage message) => @"Lookup PBN<br />
            <br />
            <table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
                <tr><td>PBN ID</td><td>AB123FGH</td></tr>
                <tr><td>Status</td><td>INCOMPLETE</td></tr>
                <tr><td>Issue</td><td>Some fields are missing</td></tr>
                <tr><td>Direction</td><td>IN_IRELAND</td></tr>
                <tr><td>Empty Vehicle</td><td>False</td></tr>
                <tr><td>Email</td><td>TestEmail@id.org</td></tr>
                <tr><td>Mobile Number 1</td><td>+12345678910</td></tr>
                <tr><td>Mobile Number 2</td><td>+910000000</td></tr>
            </table><br />
            <br />
            Declarations<br />
            <br />
            <table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
                <tr><td>Declaration ID</td><td>9511</td></tr>
                <tr><td>Declaration Type</td><td>New Declaration</td></tr>
            </table><br />
            <table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
                <tr><td>Declaration ID</td><td>9510</td></tr>
                <tr><td>Declaration Type</td><td>import</td></tr>
            </table>";

		protected override PBNInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<AsycudaManifestHeader>();

			var message = Factory.New<PBNInboundEDIMessage>();
			message.EM_LinkedObject = declaration;
			var lpbText = JsonSerializer.Serialize(LPBMessageForTest());
			message.EM_MessageText = lpbText;

			return message;
		}

		protected override LPBPBNProvider GetProvider(TextReader reader)
		{
			var lpbDefinition = JsonSerializer.Deserialize<LPBDefinition>(reader.ReadToEnd());
			return new LPBPBNProvider(lpbDefinition);
		}

		static LPBDefinition LPBMessageForTest()
		{
			return new LPBDefinition
			{
				PbnID = "AB123FGH",
				Status = "INCOMPLETE",
				Issue = "Some fields are missing",
				Direction = "IN_IRELAND",
				EmptyVehicle = false,
				Declarations = new List<PBNDeclaration>
				{
					new PBNDeclaration { DeclarationId = "9511", DeclarationType = "New Declaration" },
					new PBNDeclaration { DeclarationId = "9510", DeclarationType = "import" }
				},
				ContactDetails = new PBNContactDetails
				{
					Email = "TestEmail@id.org",
					MobileNum1 = "+12345678910",
					MobileNum2 = "+910000000"
				}
			};
		}
	}
}
