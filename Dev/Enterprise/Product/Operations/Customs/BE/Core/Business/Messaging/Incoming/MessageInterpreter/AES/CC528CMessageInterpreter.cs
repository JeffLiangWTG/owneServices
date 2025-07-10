using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.Business;

public sealed class CC528CMessageInterpreter : BaseMessageInterpreter<ICC528CDataProvider>
{
	public override string Interpret(ICC528CDataProvider dataProvider, EDIMessage ediMessage)
	{
		var note = new ZStringBuilder();
		note.Append($"Declaration is accepted on {dataProvider.DeclarationAcceptanceDate:dd-MMM-yy H:mm:ss}");
		note.Append($"Status is changed to MRN");

		return note.ToStringWithDelimiterBetweenAppends(Constants.HtmlContent.Break);
	}
}
