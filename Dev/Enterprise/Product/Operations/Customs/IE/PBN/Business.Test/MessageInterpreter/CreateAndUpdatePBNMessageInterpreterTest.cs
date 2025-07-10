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
	[TestedType(typeof(CreateAndUpdatePBNMessageInterpreter))]
	sealed class CreateAndUpdatePBNMessageInterpreterTest : InboundMessageInterpreterAbstractTest<PBNInboundEDIMessage, CreateAndUpdatePBNMessageInterpreter, CreateAndUpdatePBNProvider>
	{
		string currentMessageType = PBNMessageTypes.Codes.CreatePBN;
		protected override ZString MessageType => currentMessageType;

		protected override ZString GetExpectedInterpretation(PBNInboundEDIMessage message) => @$"A {message.MessageTypeWithDescription} message which contains 'Manifest Job Number' has been received for Request Report XYZ123.<br />
            <br />
            <table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
                <tr><td>PBN ID</td><td>XYZ123</td></tr>
                <tr><td>Status</td><td>VALID</td></tr>
                <tr><td>Issue</td><td>No issues</td></tr>
            </table><br />
            <br />
            Validation Errors<br />
            <br />
            <table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
                <tr><td>Validation Error Code</td><td>E001</td></tr>
                <tr><td>Validation Error Path</td><td>/path/to/error1</td></tr>
                <tr><td>Validation Error Description</td><td>Error description 1</td></tr>
            </table><br />
            <table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
                <tr><td>Validation Error Code</td><td>E002</td></tr>
                <tr><td>Validation Error Path</td><td>/path/to/error2</td></tr>
                <tr><td>Validation Error Description</td><td>Error description 2</td></tr>
            </table>";

		protected override PBNInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<AsycudaManifestHeader>();

			var message = Factory.New<PBNInboundEDIMessage>();
			message.EM_LinkedObject = declaration;
			message.EM_MessageType = PBNMessageTypes.Codes.CreatePBN;
			var lpcText = JsonSerializer.Serialize(CPBandUPBandUPDMessageForTest());
			message.EM_MessageText = lpcText;

			return message;
		}

		protected override CreateAndUpdatePBNProvider GetProvider(TextReader reader)
		{
			var createAndupdatePBNDefinition = JsonSerializer.Deserialize<CreateAndUpdatePBNMessageDefinition>(reader.ReadToEnd());
			return new CreateAndUpdatePBNProvider(createAndupdatePBNDefinition);
		}

		static CreateAndUpdatePBNMessageDefinition CPBandUPBandUPDMessageForTest()
		{
			return new CreateAndUpdatePBNMessageDefinition
			{
				PbnID = "XYZ123",
				Status = "VALID",
				Issue = "No issues",
				ValidationErrors = new List<PBNValidationErrors>
				{
					new PBNValidationErrors { ErrorCode = "E001", Path = "/path/to/error1", ErrorDescription = "Error description 1" },
					new PBNValidationErrors { ErrorCode = "E002", Path = "/path/to/error2", ErrorDescription = "Error description 2" }
				}
			};
		}

		public void TestUpdatePBNMessageInterpretation()
		{
			currentMessageType = PBNMessageTypes.Codes.UpdatePBN;
			TestGetMessageInterpretation();
		}

		public void TestUpdatePBNDeclarationsMessageInterpretation()
		{
			currentMessageType = PBNMessageTypes.Codes.UpdatePBNDeclarations;
			TestGetMessageInterpretation();
		}
	}
}
