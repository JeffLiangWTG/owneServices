using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.Business;

public class CC529CMessageInterpreter : BaseMessageInterpreter<ICC529CDataProvider>
{
	public override string Interpret(ICC529CDataProvider dataProvider, EDIMessage ediMessage)
	{
		var entryHeader = (CusEntryHeader)ediMessage.EM_LinkedObject;
		var note = new ZStringBuilder();
		note.Append($"Declaration is released on {dataProvider.ReleaseDate:dd-MMM-yy}");
		note.Append($"Status is changed to {entryHeader.CH_EntryStatus}");

		return note.ToStringWithDelimiterBetweenAppends(Constants.HtmlContent.Break);
	}
}
