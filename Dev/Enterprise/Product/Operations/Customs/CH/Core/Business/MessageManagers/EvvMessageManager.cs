using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using static CargoWise.EventReference.Constants;

namespace Enterprise.Customs.CH.Business;

public class EvvMessageManager : IMessageManager
{
	public EvvMessageManager(EvvRequestSendingObject sendingObject)
	{
		SendingObject = sendingObject;
	}
	EvvRequestSendingObject SendingObject { get; }
	IEDIMessageCollectionOwner MessageOwner => SendingObject.MessageOwner;
	IStmALogParent LogParent => MessageOwner as IStmALogParent;
	BusinessObjectFactory Factory => SendingObject.Factory;

	public EDIMessage[] GenerateMessages()
	{
		return new[] { CreateMessage() };
	}

	EDIMessage CreateMessage()
	{
		var message = Factory.New<CHEDIMessage>();
		message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CHCustomsEdec;
		message.EM_MessageType = MessageTypeCodeList.Codes.EVV;
		message.EM_MessageSubType = SendingObject.MessageSubTypeForEDIMessage;
		message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		message.EM_Status = EDIMessageStatusList.Codes.Queued;
		message.EM_GP = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany)?.GlbExternalPassword?.PK ?? ZGuid.Empty;
		message.EM_MessageText = SendingObject.ToMessageString();
		message.EM_ApplicationReference = $"{SendingObject.Mrn}.{SendingObject.MrnVersion}";

		MessageOwner.Messages.Add(message);
		LogParent?.Logs.AddNew(Events.ElectronicAssessmentDecisionStatus, GetEventReference(SendingObject.DocumentType).ToArray());

		return message;
	}

	IEnumerable<KeyValuePair<string, string>> GetEventReference(string documentType)
	{
		yield return new KeyValuePair<string, string>(EventReferenceParameters.Codes.CustomsStatus, CustomsStatusReference);
		yield return new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, documentType);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string CustomsStatusReference = "Requested";

	public void RollbackOnSaveFailed()
	{
		LogParent?.Logs.LogsNotInDB.ForEach(l => l.Delete());
	}
}
