using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business;

public sealed class CC551CMessageInterpreter : BaseMessageInterpreter<ICC551CDataProvider>
{
	public override string Interpret(ICC551CDataProvider dataProvider, EDIMessage ediMessage)
	{
		var note = new ZStringBuilder();
		note.Append((NoResString)"The goods are not Released by customs.");
		note.Append($"Following was reported: {dataProvider.OtherThingsToReport}");
		note.Append($"Control was done on {dataProvider.ControlDate:dd-MMM-yy H:mm:ss}");
		note.Append($"Customs reported: {dataProvider.ControlText}");
		note.Append((NoResString)"Status is set to DNR (Declaration No Release)");

		return note.ToStringWithDelimiterBetweenAppends(Constants.HtmlContent.Break);
	}
}
