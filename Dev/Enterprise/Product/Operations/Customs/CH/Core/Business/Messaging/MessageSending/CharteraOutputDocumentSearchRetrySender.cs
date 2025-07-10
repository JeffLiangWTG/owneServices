using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.CH.Business.CompanyPollingTransaction;

namespace Enterprise.Customs.CH.Business;

class CharteraOutputDocumentSearchRetrySender : BasePassarCompanyMessageSender
{
	const string MessageSenderTaskCode = "CHS";

	public CharteraOutputDocumentSearchRetrySender(LoggingInformation logger) : base(logger)
	{
	}

	protected override string FriendlyName => (NoResString)"Chartera Output Document Search Retry Request";

	protected override ZString ApplicationCode => CusPollingTransaction.ApplicationCodes.CHCustomsCharteraOutput;

	protected override ZString MessageType => MessageTypeCodeList.Codes.REQ;

	protected override ZString MessageSubType => MessageSubTypeCodeList.Codes.CharteraOutputDocumentSearchRequest;

	protected override int SendCore(CancellationToken cancellationToken, GlbCompany company, GlbCompanyTokenCredentials tokenCredentials)
	{
		var processedCount = 0;

		var timePeriodNotBefore = ZDateTime.UtcNow.AddHours(-CHCustomsDataRegistry.Instance.PassarSearchRequestConfig.Value.TimeLimit);
		var transactions = company.LoadDocumentSearchRequests(timePeriodNotBefore, StatusList);

		if (transactions.Length > 0)
		{
			var maxNumberOfSearchAttempts = CHCustomsDataRegistry.Instance.MaxNumberOfSearchAttempts.Value;

			foreach (var transaction in transactions)
			{
				CreateRequest(transaction, maxNumberOfSearchAttempts);
			}

			if ((processedCount = transactions.Count(x => x.HasChanges)) > 0)
			{
				try
				{
					company.Factory.Save();
				}
				catch (ZSaveConcurrencyException)
				{
					processedCount = 0;
					logger.LogError((NoResString)"Concurrency Exception: Some customs messages may have been processed in the meantime.");
				}
				catch (ZSaveException ex)
				{
					processedCount = 0;
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
		}

		return processedCount;
	}

	void CreateRequest(CusPollingTransaction transaction, int maxNumberOfSearchAttempts)
	{
		if (transaction.CPT_NumberOfAttempts < maxNumberOfSearchAttempts)
		{
			var lastMessage = GetLastDocumentSearchRequest(transaction);
			if (lastMessage == null)
			{
				logger.LogError($"No Document Search Request sent for transaction PK={transaction.PK}");
			}
			else if (ShouldSendNewSearchRequest(transaction, lastMessage))
			{
				var heldUntilTime = ZDateTime.UtcNow.AddMinutes(GetDelay());
				CreateEDIMessage(transaction.Factory, transaction, lastMessage.EM_MessageText, lastMessage.EM_ApplicationReference, retryCount: transaction.CPT_NumberOfAttempts, heldUntilDate: ZDateTime.UtcNow.AddMinutes(GetDelay()), credentialsPK: lastMessage.EM_GP);
				ServiceTaskHelper.NudgeServiceTaskTime(ServiceTasksLogger, MessageSenderTaskCode, heldUntilTime);
			}
			else
			{
				return;
			}

			transaction.CPT_NumberOfAttempts++;
			transaction.CPT_Status = StatusCodes.AwaitingResponse;
			transaction.CPT_StatusTimeUtc = ZDateTime.UtcNow;
		}
		else
		{
			transaction.CPT_Status = StatusCodes.Skip;
			transaction.CPT_StatusTimeUtc = ZDateTime.UtcNow;
		}

		int GetDelay()
		{
			switch (transaction.CPT_NumberOfAttempts)
			{
				case 1:
					return 5;
				case 2:
					return 10;
				case 3:
					return 30;
				default:
					return 60;
			}
		}
	}

	EDIMessage GetLastDocumentSearchRequest(CusPollingTransaction transaction)
	{
		var query = new ZQuery();
		query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCode);
		query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit);
		query.AddToFilter(EDIMessageSchema.EM_MessageType, MessageType);
		query.AddToFilter(EDIMessageSchema.EM_MessageSubType, MessageSubType);
		query.AddToFilter(EDIMessageSchema.EM_LinkTable, CusPollingTransactionSchema.Constants.TableName);
		query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, transaction.PK);
		query.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + OrderByClause.Descending;
		return transaction.Factory.LoadTop1<EDIMessage>(query);
	}

	bool ShouldSendNewSearchRequest(CusPollingTransaction transaction, EDIMessage message)
	{
		if (transaction.CPT_Status == StatusCodes.AwaitingResponse)
		{
			var lastCommittedTimeStamp = message.EM_HeldUntilDate.IsValid ? message.EM_HeldUntilDate : message.EM_SystemCreateTimeUtc;
			return lastCommittedTimeStamp.AddMinutes(5) < ZDateTime.UtcNow;
		}
		else
		{
			return true;
		}
	}

	static string[] StatusList => new[] { StatusCodes.Error, StatusCodes.AwaitingResponse };

	ILogger ServiceTasksLogger => serviceTasksLogger ??= new ServiceTasksLoggerWrapper(logger);
	ServiceTasksLoggerWrapper serviceTasksLogger;
}
