using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.AE.Business;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AE.Manifest.Business;

sealed class MessageDetailsProvider : IMessageDetailsProvider
{
	public MessageDetailsProvider(MessageChooserItem messageChooserItem)
	{
		MessageItem = Argument.NotNull(messageChooserItem, nameof(messageChooserItem));
		Bill = Argument.NotNull(messageChooserItem.Bill, nameof(messageChooserItem.Bill));
	}
	AsycudaBill Bill { get; }
	MessageChooserItem MessageItem { get; }

	public string DocumentCode => documentCode ??= GetDocumentCode();
	string documentCode;

	public string DocumentIdentifier => EDIMessage.SendersReferencePlaceHolder;

	public string Version => version ??= GetVersion();
	string version;

	public string MessageFunction
	{
		get
		{
			var entryType = (string)MessageItem.EntryType;
			return entryType switch
			{
				EntryTypes.Codes.Original => MessageFunctionCodeList.Original,
				EntryTypes.Codes.Change => MessageFunctionCodeList.Original,
				EntryTypes.Codes.Cancellation => MessageFunctionCodeList.Cancellation,
				_ => entryType
			};
		}
	}

	string GetDocumentCode() => Bill.ABL_BolType == Core.Constants.ShipmentTypes.StandardHouse
								? DocumentNameCodeList.HouseBillOfLading
								: DocumentNameCodeList.ForwardersBillOfLading;

	const string Version1 = "001";
	string GetVersion()
	{
		var entryType = (string)MessageItem.EntryType;
		return entryType switch
		{
			EntryTypes.Codes.Original => Version1,
			EntryTypes.Codes.Cancellation => null,
			EntryTypes.Codes.Change => GetAmendmentVersion(),
			_ => null
		};
	}

	string GetAmendmentVersion()
	{
		var versionNum = Bill.Messages.Cast<EDIMessage>()
			.Count(x => x.EM_MessageType == AEConstants.Messaging.MessageTypes.CUSRES) + 1;
		return versionNum.ToString("D3");
	}
}
