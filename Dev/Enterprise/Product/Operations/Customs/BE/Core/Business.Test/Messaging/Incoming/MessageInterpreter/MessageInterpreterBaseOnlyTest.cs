using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(MessageInterpreterForTest))]
class MessageInterpreterBaseOnlyTest : MessageInterpreterTestCase<MessageInterpreterForTest, IInboundProvider>
{
	public void TestAddHtmlTableInterpretation()
	{
		var result = new ZStringBuilder();
		var testData = new List<KeyValuePair<string, string>>
		{
				new KeyValuePair<string, string>("k1", "v1"),
				new KeyValuePair<string, string>("k2", "v2")
		};
		Interpreter.AddHtmlTableInterpretationExposed(result, "50px", testData);
		AssertEquals("<table cellpadding=\"3\" cellspacing=\"0\" width=\"100%\" border=\"1\" style=\"font-size: 14px; border: 1px solid gray; border-collapse: collapse; font-family: Arial, sans-serif;\"><tr><td width=\"50px\">k1</td><td>v1</td></tr><tr><td width=\"50px\">k2</td><td>v2</td></tr></table><br />", result.ToString());
	}

	public void TestAddHTMLNoteTextLineInterpretation()
	{
		var result = new ZStringBuilder();
		Interpreter.AddHTMLNoteTextLineInterpretationExposed(result, "TestNote");
		AssertEquals("TestNote<br />", result.ToString());
	}

	public override void TestInterpret()
	{
		AssertEquals("Interpret", Interpreter.Interpret(new Mock<IInboundProvider>().Object, null));
	}

	public void TestInterpretDiscardedMessage()
	{
		var message = Factory.New<BEMessage>();
		message.EM_Status = EDIMessage.Status.Discarded;
		message.Notes.AddNew(true, Constants.MessageProcessingNotes.ProcessingLog, "This is the interpretation of a discarded message");
		AssertEquals("Interpret discarded message", "This is the interpretation of a discarded message", Interpreter.InterpretDiscardedMessage(new Mock<IInboundProvider>().Object, message));
	}
}

class MessageInterpreterForTest : BaseMessageInterpreter<IInboundProvider>
{
	public override string Interpret(IInboundProvider dataProvider, EDIMessage ediMessage) => "Interpret";

	public void AddHtmlTableInterpretationExposed(ZStringBuilder noteHtmlInterpretation, string keypxWidth, IEnumerable<KeyValuePair<string, string>> items)
	{
		AddHtmlTableInterpretation(noteHtmlInterpretation, keypxWidth, items);
	}

	public void AddHTMLNoteTextLineInterpretationExposed(ZStringBuilder noteHtmlInterpretation, string newlineNote)
	{
		AddHTMLNoteTextLineInterpretation(noteHtmlInterpretation, newlineNote);
	}
}
