using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.Business;

public sealed class CC928CMessageInterpreter : BaseMessageInterpreter<ICC928CDataProvider>
{
	public override string Interpret(ICC928CDataProvider dataProvider, EDIMessage ediMessage)
	{
		var entryHeader = (CusEntryHeader)ediMessage.EM_LinkedObject;
		var note = new ZStringBuilder();
		note.Append($"Correlation Id: {dataProvider.CorrelationId}");
		if (entryHeader.CH_EntryStatus == StatusCodes.DeclarationAcknowledged)
		{
			note.Append($"Status is set to {entryHeader.CH_EntryStatus}");
		}
		else
		{
			note.Append($"Status remains {entryHeader.CH_EntryStatus}");
		}

		return note.ToStringWithDelimiterBetweenAppends(Constants.HtmlContent.Break);
	}
}
