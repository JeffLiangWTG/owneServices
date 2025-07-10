using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using static CargoWise.EventReference.Constants;
using static Enterprise.Customs.CH.Business.CompanyPollingTransaction;

namespace Enterprise.Customs.CH.Business;

public class CharteraOutputDocumentSearchRequestMessageManager : BaseMessageManager<CharteraOutputDocumentSearchSendingObject>
{
	public CharteraOutputDocumentSearchRequestMessageManager(CharteraOutputDocumentSearchSendingObject messageSender) : base(messageSender)
	{
	}

	public override string MessageFriendlyName => MessageSubTypeCodeList.Descriptions.CharteraOutputDocumentSearchRequest;

	protected override void AfterGenerateMessage(CharteraOutputDocumentSearchSendingObject sendingObject, EDIMessage message)
	{
		var company = sendingObject.Company;
		var maxNumberOfAttempts = (byte)CHCustomsDataRegistry.Instance.MaxNumberOfSearchAttempts.Value;
		transaction = company.CreateDocumentSearchTransaction(sendingObject.ApplicationCode, sendingObject.ProcessId, status: StatusCodes.Skip, numberOfAttempts: maxNumberOfAttempts, reference: ReferenceCodes.Manual);
		message.EM_LinkedObject = transaction;

		company.Logs.AddNew(Events.ManualDocumentSearch,
			new KeyValuePair<string, string>(EventReferenceParameters.Codes.From, FormatDateTime(sendingObject.CreationTimeFrom)),
			new KeyValuePair<string, string>(EventReferenceParameters.Codes.To, FormatDateTime(sendingObject.CreationTimeTo)),
			new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, MessageSubTypeCodeList.Descriptions.Request));

		string FormatDateTime(ZDateTime dateTime) => dateTime.ToDateTime().ToString("dd.MM.yy HH:mm");
	}

	public override void RollbackOnSaveFailed()
	{
		transaction?.Delete();
		SendingObject.Company.Logs.LogsNotInDB.ForEach(l => l.Delete());
	}

	CusPollingTransaction transaction;
}
