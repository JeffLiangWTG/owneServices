using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using static CargoWise.EventReference.Constants;

namespace Enterprise.Customs.CH.Business;

public class EbdMessageManager : BaseMessageManager<SupportingDocSendingObject>
{
	public EbdMessageManager(SupportingDocSendingObject messageSender) : base(messageSender)
	{
	}

	public override string MessageFriendlyName => MessageTypeCodeList.Descriptions.EBD;

	protected override EDIMessage CreateEDIMessageCore(SupportingDocSendingObject sendingObject)
	{
		var message = base.CreateEDIMessageCore(sendingObject);

		var attachment = message.MessageAttachments.AddNew();
		attachment.EG_StorageDocsGuid = sendingObject.EDoc;
		attachment.EG_FileName = sendingObject.Document?.FileName ?? ZString.Empty;
		return message;
	}

	IEnumerable<KeyValuePair<string, string>> GetEventReference()
	{
		yield return new KeyValuePair<string, string>(EventReferenceParameters.Codes.File, SendingObject.Document?.FileName ?? string.Empty);
		yield return new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, SendingObject.DocumentType);
		yield return new KeyValuePair<string, string>(EventReferenceParameters.Codes.DeclarationID, SendingObject.CaseNumber);
	}

	protected override void AfterGenerateMessage(SupportingDocSendingObject sendingObject, EDIMessage message)
	{
		sendingObject.Header.Messages.Add(message);
		sendingObject.Header.Logs.AddNew(Events.DocumentSent, GetEventReference().ToArray());
	}

	public override void RollbackOnSaveFailed()
	{
		SendingObject.Header.Logs.LogsNotInDB.ForEach(l => l.Delete());
	}
}
