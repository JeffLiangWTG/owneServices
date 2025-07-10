using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.Business;

public sealed class CC560CMessageInterpreter : BaseMessageInterpreter<ICC560CDataProvider>
{
	public override string Interpret(ICC560CDataProvider dataProvider, EDIMessage ediMessage)
	{
		var entryHeader = (CusEntryHeader)ediMessage.EM_LinkedObject;
		var interpretationTextBuilder = new ZStringBuilder();
		AddHTMLNoteTextLineInterpretation(interpretationTextBuilder, $"Status is set to {entryHeader.CH_EntryStatus}");
		AddHTMLNoteTextLineInterpretation(interpretationTextBuilder, $"Customs decision to control ({dataProvider.NotificationType}{(dataProvider.NotificationType == null ? "" : " - " + new NotificationTypesList().GetDescriptionFromCode(dataProvider.NotificationType))}) was taken on {dataProvider.ControlNotificationDateTime.ToString("dd/MM/yyyy HH:mm:ss")}");
		if (dataProvider.AnticipatedControlDate == null)
		{
			AddHTMLNoteTextLineInterpretation(interpretationTextBuilder, $"The anticipated date of control is not given");
		}
		else
		{
			AddHTMLNoteTextLineInterpretation(interpretationTextBuilder, $"The anticipated date of control will be on {dataProvider.AnticipatedControlDate.Value.ToString("dd/MM/yyyy")}");
		}
		AddHTMLNoteTextLineInterpretation(interpretationTextBuilder, dataProvider.Text);
		AddHTMLNoteTextLineInterpretation(interpretationTextBuilder, "");

		foreach (var typeOfControl in dataProvider.TypeOfControls)
		{
			AddHTMLNoteTextLineInterpretation(interpretationTextBuilder, $"Type of control: {typeOfControl.Type} - {new TypeOfControlsList().GetDescriptionFromCode(typeOfControl.Type)}");
			AddHTMLNoteTextLineInterpretation(interpretationTextBuilder, $"Additional info: {typeOfControl.Text}");
			AddHTMLNoteTextLineInterpretation(interpretationTextBuilder, "");
		}

		foreach (var requestedDocument in dataProvider.RequestedDocuments)
		{
			AddHTMLNoteTextLineInterpretation(interpretationTextBuilder, $"Document: {requestedDocument.DocumentType}");
			AddHTMLNoteTextLineInterpretation(interpretationTextBuilder, $"Description: {requestedDocument.Description}");
			AddHTMLNoteTextLineInterpretation(interpretationTextBuilder, "");
		}
		return interpretationTextBuilder.ToString();
	}
}
