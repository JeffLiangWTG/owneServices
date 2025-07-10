using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business;

public class CC509CMessageInterpreter : BaseMessageInterpreter<ICC509CDataProvider>
{
	public override string Interpret(ICC509CDataProvider dataProvider, EDIMessage ediMessage)
	{
		var noteHTMLPresentation = new ZStringBuilder();

		AddHTMLNoteTextLineInterpretation(noteHTMLPresentation, $"Declaration is invalidated/cancelled on {dataProvider.InvalidationDecisionDateAndTime:dd-MMM-yy H:mm:ss}");
		AddHTMLNoteTextLineInterpretation(noteHTMLPresentation, (NoResString)"Status is set to CAN");

		return noteHTMLPresentation.ToString();
	}
}
