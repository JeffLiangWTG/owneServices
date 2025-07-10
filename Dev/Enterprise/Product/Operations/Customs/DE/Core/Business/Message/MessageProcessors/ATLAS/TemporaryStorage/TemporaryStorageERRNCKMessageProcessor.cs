using System.Text;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public sealed class TemporaryStorageERRNCKMessageProcessor : TemporaryStorageMessageProcessor<AtlasInboundEDIMessage<IERRNCK>, IERRNCK>
	{
		public TemporaryStorageERRNCKMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("F05B2CB1-B971-4699-BB39-825019AF54E2", "Temporary Storage E_ERR_NCK Message Processor");

		protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendTemporaryStorageErrors;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<IERRNCK> message)
		{
			var storageDec = (CusTempStorageDec)message.EM_LinkedObject;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			storageDec.STH_MessageStatus = EDIMessageStatusList.Codes.Rejected;
			var dataProvider = message.DataProvider;
			var referencedMessageIdentifier = dataProvider.ReferencedMessageIdentifier;
			SendEmail(message, storageDec);
			message.SetLogbookRegistrationNumber(new ZString[] { referencedMessageIdentifier, dataProvider.ReferenceNumber });
		}

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<IERRNCK> message) => GetLinkedObjectFromOriginalMessage(message.Factory, message.DataProvider?.ReferencedMessageIdentifier);

		void SendEmail(AtlasInboundEDIMessage<IERRNCK> message, CusTempStorageDec declaration)
		{
			var storageHeader = declaration.StorageHeader;
			GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory
				, storageHeader
				, Res.GetString("6878FFE8-377B-4A30-984D-1E7FF24B39A3", "SumA ERRNCK - Error message")
				, GetEmailBody(storageHeader.SJH_JobReference, message.DataProvider)
				, false
				, message.Branch
				, declaration
				, () => declaration.Messages.LastSentOutgoingMessage);
		}

		static string GetEmailBody(string reference, IERRNCK provider)
		{
			var htmlBody = new StringBuilder();
			htmlBody.Append(Res.GetString("DF1944BD-E04C-4911-8A74-E7811CF9CD45", @"Your SumA Declaration for Job {0} received an Error Message. For details please follow the link to the job.", reference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			var localReferenceNumber = provider.LocalReferenceNumber;
			var tableCreator = new HtmlTableCreator();
			tableCreator.WriteRow(Res.GetString("B2953318-D404-4441-97B4-D26B351F1FE1", "Registration Number"), provider.ReferenceNumber);
			if (!localReferenceNumber.IsEmpty())
			{
				tableCreator.WriteRow(Res.GetString("262AD5E4-6E79-4726-A49F-5B907FF7432B", "Local Reference Number"), localReferenceNumber);
			}
			htmlBody.Append(tableCreator.ToHtml());
			return htmlBody.ToString();
		}
	}
}
