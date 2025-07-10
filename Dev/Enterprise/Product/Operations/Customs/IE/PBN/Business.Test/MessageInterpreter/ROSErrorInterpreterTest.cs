using System.IO;
using System.Text.Json;
using CargoWise.Customs.IE.MessageDefinitions.PBN;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.PBN.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing;

[TestedType(typeof(ROSErrorInterpreter))]
sealed class ROSErrorInterpreterTest : InboundMessageInterpreterAbstractTest<PBNInboundEDIMessage, ROSErrorInterpreter, ROSErrorProvider>
{
	protected override ZString MessageType => PBNMessageTypes.Codes.CreatePBN;

	protected override PBNInboundEDIMessage CreateIncomingMessageToTest()
	{
		var declaration = Factory.New<AsycudaManifestHeader>();

		var message = Factory.New<PBNInboundEDIMessage>();
		message.EM_LinkedObject = declaration;
		var errorText = JsonSerializer.Serialize(NewDataObjectToTest());
		message.EM_MessageText = errorText;

		return message;
	}

	protected override ZString GetExpectedInterpretation(PBNInboundEDIMessage message) => @"ROS Error<br/>
<br/>
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
	<tr>
		<td>
			Code</td>
		<td>
			111007</td>
	</tr>
	<tr>
		<td>
			Description</td>
		<td>
			Message was not digitally signed</td>
	</tr>
</table>".Replace("\t", string.Empty);

	protected override ROSErrorProvider GetProvider(TextReader reader)
	{
		var text = reader.ReadToEnd();
		var dataObject = JsonSerializer.Deserialize<ROSErrorDefinition>(text);
		return new ROSErrorProvider(dataObject);
	}

	public static ROSErrorDefinition NewDataObjectToTest() => new ROSErrorDefinition
	{
		ValidationErrors = [new PBNValidationErrors { ErrorCode = "111007", ErrorDescription = "Message was not digitally signed" }]
	};
}
