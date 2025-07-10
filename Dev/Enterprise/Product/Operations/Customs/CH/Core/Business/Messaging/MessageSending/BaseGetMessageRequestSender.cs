using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Business;

namespace Enterprise.Customs.CH.Business;

public abstract class BaseGetMessageRequestSender : BasePassarCompanyMessageSender
{
	public BaseGetMessageRequestSender(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZString MessageType => MessageTypeCodeList.Codes.MSG;

	protected override ZString MessageSubType => ZString.Empty;

	protected override int SendCore(CancellationToken cancellationToken, GlbCompany company, GlbCompanyTokenCredentials tokenCredentials)
	{
		var processedMessagesCount = 0;
		var factory = company.Factory;

		CusPollingTransaction[] transactions = null;

		do
		{
			cancellationToken.ThrowIfCancellationRequested();

			transactions = company.LoadMessageIdTransactionsToSend(factory, ApplicationCode, MaxMessagesPerSave);

			if (transactions.Length > 0)
			{
				var messages = new List<EDIMessage>();

				foreach (var transaction in transactions.Cast<CusPollingTransaction>())
				{
					messages.Add(CreateEDIMessage(factory, company, MessagePlaceHolder, transaction.CPT_TransactionID, tokenCredentials?.PK));
					UpdateTransaction(transaction);
					processedMessagesCount++;
				}

				foreach (var interchange in CreateEDIInterchanges(messages.ToArray()))
				{
					var message = interchange.ContainedMessages.Single() as EDIMessage;

					var headerAttributes = CustomsMessageHelper.CreateHeaderAttributes()
						.AddBpId()
						.AddMessageId(message.EM_ApplicationReference);

					interchange.SetHeaderTextWithAttributeDictionary(headerAttributes);

					interchange.EI_BodyText = ZString.Empty;
					message.EM_MessageText = interchange.EI_HeaderText;
				}

				ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);
			}

			factory = new BusinessObjectFactory();
		}
		while (transactions.Length == MaxMessagesPerSave);

		return processedMessagesCount;
	}

	void UpdateTransaction(CusPollingTransaction transaction)
	{
		transaction.CPT_Status = CompanyPollingTransaction.StatusCodes.AwaitingResponse;
		transaction.CPT_StatusTimeUtc = ZDateTime.UtcNow;
		transaction.CPT_NumberOfAttempts++;
	}

	internal const int MaxMessagesPerSave = 100;
}
