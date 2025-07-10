using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.Business;

public sealed class CC599CMessageInterpreter : BaseMessageInterpreter<ICC599CDataProvider>
{
	public override string Interpret(ICC599CDataProvider dataProvider, EDIMessage ediMessage)
	{
		var entryHeader = (CusEntryHeader)ediMessage.EM_LinkedObject;

		var note = new ZStringBuilder();
		if (entryHeader.CH_EntryStatus == StatusCodes.GoodsExitedEU)
		{
			note.Append($"Declaration has exited the EU on {dataProvider.ExitDate:dd-MMM-yy} according customs office {dataProvider.CustomsOfficeOfExit}");
		}
		else
		{
			note.Append($"Declaration has not exited the EU. The exit has stopped on {dataProvider.ExitStoppedDate:dd-MMM-yy} according customs office {dataProvider.CustomsOfficeOfExit}");
		}
		var controlResult = new ExitControlResultList().GetWithDescription(dataProvider.ControlResultCode);

		note.Append($"Control result code is {controlResult}");
		note.Append($"State of seals is {(dataProvider.StateOfSeals == "1" ? "OK" : "NOK")}");
		note.Append($"Status is set to {entryHeader.CH_EntryStatus}");

		return note.ToStringWithDelimiterBetweenAppends(Constants.HtmlContent.Break);
	}
}
