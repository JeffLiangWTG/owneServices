using System.Linq;
using System.Threading;
using CargoWise.Customs.CH.MessageContracts.Chartera.Outgoing;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.Business.CusPollingTransaction;
using static Enterprise.Customs.CH.Business.CompanyPollingTransaction;

namespace Enterprise.Customs.CH.Business;

class CharteraOutputDocumentDeliveryRequestSender : BasePassarCompanyMessageSender
{
	public CharteraOutputDocumentDeliveryRequestSender(LoggingInformation logger) : base(logger)
	{
	}

	protected override string FriendlyName => (NoResString)"Chartera Output Document Delivery Request";

	protected override ZString ApplicationCode => ApplicationCodes.CHCustomsCharteraOutput;

	protected override ZString MessageType => MessageTypeCodeList.Codes.REQ;

	protected override ZString MessageSubType => MessageSubTypeCodeList.Codes.CharteraOutputDocumentDeliveryRequest;

	protected override int SendCore(CancellationToken cancellationToken, GlbCompany company, GlbCompanyTokenCredentials tokenCredentials)
	{
		var factory = company.Factory;
		var result = 0;

		var transactions = company.LoadDocumentDeliveryTransactionsToSend(factory, CHCustomsDataRegistry.Instance.MaxNumberOfSimultaneousDownloads.Value);
		if (transactions.Length > 0)
		{
			foreach (var transaction in transactions)
			{
				UpdateTransaction(transaction);
			}

			var dataProvider = new CharteraOutputDocumentDeliveryRequestDataProvider(transactions);
			var messageBuilder = (IXmlMessageBuilder)new DocumentDeliveryRequestV1MessageBuilder(dataProvider);
			var message = CreateEDIMessage(factory, company, messageBuilder.GenerateXmlMessage().GetSerializedString(), dataProvider.ProcessId, tokenCredentials?.PK);

			var interchange = CreateEDIInterchanges(message).SingleOrDefault();
			if (interchange != null)
			{
				ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);
				result = 1;
			}
		}
		return result;
	}

	void UpdateTransaction(CusPollingTransaction transaction)
	{
		if (transaction.CPT_NumberOfAttempts < CHCustomsDataRegistry.Instance.MaxNumberOfDownloadAttempts.Value)
		{
			transaction.CPT_Status = StatusCodes.AwaitingResponse;
			transaction.CPT_NumberOfAttempts += 1;
			transaction.CPT_EarliestTimeOfNextAttemptUtc = ZDateTime.UtcNow.AddMinutes(GetDelay());
		}
		else
		{
			transaction.CPT_Status = StatusCodes.Skip;
		}
		transaction.CPT_StatusTimeUtc = ZDateTime.UtcNow;

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
}
