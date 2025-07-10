using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.Business;

public class CC504CMessageInterpreter : BaseMessageInterpreter<ICC504CDataProvider>
{
	public override string Interpret(ICC504CDataProvider dataProvider, EDIMessage ediMessage)
	{
		var noteHTMLPresentation = new ZStringBuilder();
		var entryHeader = (CusEntryHeader)ediMessage.EM_LinkedObject;
		AddHTMLNoteTextLineInterpretation(noteHTMLPresentation, $"Declaration is amended on {dataProvider.AmendmentAcceptanceDateAndTime}");
		AddHTMLNoteTextLineInterpretation(noteHTMLPresentation, $"Status is set to {entryHeader.CH_EntryStatus}");
		return noteHTMLPresentation.ToString();
	}
}
