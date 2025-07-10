using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using static CargoWise.EventReference.Constants;

namespace Enterprise.Customs.CH.Business;

public class EComplaintMessageManager : BaseMessageManager<EComplaintMessageSendingObject>
{
	public EComplaintMessageManager(EComplaintMessageSendingObject messageSender) : base(messageSender)
	{
	}

	public override string MessageFriendlyName => MessageTypeCodeList.Descriptions.ECM;

	ZString originaLastEComplaintStatus;

	IEnumerable<KeyValuePair<string, string>> GetEventReference()
	{
		yield return new KeyValuePair<string, string>(EventReferenceParameters.Codes.New, EComplaintStatusList.Codes.Sent);
	}

	protected override void AfterGenerateMessage(EComplaintMessageSendingObject sendingObject, EDIMessage message)
	{
		var entryHeader = sendingObject.EntryHeader;
		originaLastEComplaintStatus = entryHeader.CH_LastEComplaintStatus;

		entryHeader.Messages.Add(message);
		entryHeader.CH_LastEComplaintStatus = EComplaintStatusList.Codes.Sent;
		entryHeader.Logs.AddNew(Events.EComStatusChange, GetEventReference().ToArray());
	}

	public override void RollbackOnSaveFailed()
	{
		SendingObject.EntryHeader.CH_LastEComplaintStatus = originaLastEComplaintStatus;
		SendingObject.EntryHeader.Logs.LogsNotInDB.ForEach(l => l.Delete());
	}
}
