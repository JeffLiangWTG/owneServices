using System.Linq;
using System.Threading;
using CargoWise.Customs.CH.MessageContracts.Chartera.Outgoing;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.Business.CusPollingTransaction;

namespace Enterprise.Customs.CH.Business;

class CharteraOutputDocumentSearchRequestSender : BasePassarCompanyMessageSender
{
	public CharteraOutputDocumentSearchRequestSender(LoggingInformation logger) : base(logger)
	{
	}

	protected override string FriendlyName => (NoResString)"Chartera Output Document Search Request";

	protected override ZString ApplicationCode => ApplicationCodes.CHCustomsCharteraOutput;

	protected override ZString MessageType => MessageTypeCodeList.Codes.REQ;

	protected override ZString MessageSubType => MessageSubTypeCodeList.Codes.CharteraOutputDocumentSearchRequest;

	protected override int SendCore(CancellationToken cancellationToken, GlbCompany company, GlbCompanyTokenCredentials tokenCredentials)
	{
		var requestCreated = false;
		var transaction = company.LoadDocumentSearchRequestForNextAttempt(ApplicationCode);

		if (transaction == null)
		{
			requestCreated = CreateRequest(company, tokenCredentials, ZDateTime.UtcNow.AddHours(-1), ZDateTime.UtcNow);
		}
		else if (transaction.CPT_EarliestTimeOfNextAttemptUtc < ZDateTime.UtcNow.AddHours(-1))
		{
			var earliestTime = ZDateTime.UtcNow.AddHours(-CHCustomsDataRegistry.Instance.PassarSearchRequestConfig.Value.TimeLimit);
			var timePeriodFrom = transaction.CPT_EarliestTimeOfNextAttemptUtc;
			if (timePeriodFrom < earliestTime)
			{
				timePeriodFrom = earliestTime;
			}
			var timePeriodTo = timePeriodFrom.AddHours(1);
			if (timePeriodTo > ZDateTime.UtcNow)
			{
				timePeriodTo = ZDateTime.UtcNow;
			}
			requestCreated = CreateRequest(company, tokenCredentials, timePeriodFrom, timePeriodTo);
		}
		else if (transaction.CPT_EarliestTimeOfNextAttemptUtc < ZDateTime.UtcNow.AddMinutes(-5))
		{
			requestCreated = CreateRequest(company, tokenCredentials, transaction.CPT_EarliestTimeOfNextAttemptUtc, ZDateTime.UtcNow);
		}

		if (requestCreated)
		{
			ZExceptionReporting.ProcessWithSaveExceptionHandling(company.Factory.Save, null);
			return 1;
		}

		return 0;
	}

	bool CreateRequest(GlbCompany company, GlbCompanyTokenCredentials tokenCredentials, ZDateTime timeFrom, ZDateTime timeTo)
	{
		var sendingObject = new CharteraOutputDocumentSearchSendingObject(company.Factory)
		{
			CreationTimeFrom = new ZDateTime(timeFrom, System.DateTimeKind.Utc),
			CreationTimeTo = new ZDateTime(timeTo, System.DateTimeKind.Utc),
		};
		var messageBuilder = (IXmlMessageBuilder)new DocumentSearchRequestV1MessageBuilder(sendingObject);
		var messageText = messageBuilder.GenerateXmlMessage().GetSerializedString();
		var transaction = company.CreateDocumentSearchTransaction(ApplicationCode, sendingObject.ProcessId, timeOfNextAttempt: timeTo);
		var message = CreateEDIMessage(company.Factory, transaction, messageText, sendingObject.ProcessId, credentialsPK: tokenCredentials?.PK);
		return CreateEDIInterchanges(message).SingleOrDefault() != null;
	}
}
