using System;
using System.Globalization;
using System.Text;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ExportEXPURGMessageProcessor : ExportMessageProcessor<AesInboundEDIMessage<IEXPURG>, IEXPURG>
	{
		public ExportEXPURGMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("6eb5f4f9-53dd-43f7-9f61-150176416408", "Export EXPURG Message Processor");

		protected override BusinessObject GetLinkedObject(AesInboundEDIMessage<IEXPURG> message) => GetLinkedObjectFromOriginalMessageOrMrn(message.Factory, message.DataProvider?.ReferencedMessageIdentifier, message.DataProvider?.MovementReferenceNumber);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AesInboundEDIMessage<IEXPURG> message)
		{
			var entryHeader = (CusEntryHeader)message.EM_LinkedObject;
			message.EM_Status = EDIMessage.Status.ProcessedOK;

			var dataProvider = message.DataProvider;
			message.SetLogbookRegistrationNumber(dataProvider.MovementReferenceNumber);
			message.SetLogbookLocalReferenceNumber(dataProvider.LocalReferenceNumber);

			entryHeader.AddCustomsEntryStatusLog(UniversalReferenceConstants.EntryStatus.URG);

			SendEmailIfNeeded(message, entryHeader);
		}

		void SendEmailIfNeeded(AesInboundEDIMessage<IEXPURG> message, CusEntryHeader entryHeader)
		{
			var declaration = entryHeader.Declaration;
			if (declaration != null)
			{
				var dataProvider = message.DataProvider;
				GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory
					, declaration
					, Res.GetString("D3E10D67-EDC8-415C-B8FE-E3F349724051", "AES EXP")
					, GetEmailBody(declaration, dataProvider.LatestResponseDate)
					, false
					, message.Branch
					, declaration
					, dataProvider.ReferencedMessageIdentifier);
			}
		}

		ZString GetEmailBody(JobDeclaration declaration, DateTime latestResponseDate)
		{
			var htmlBody = new StringBuilder();
			htmlBody.Append(Res.GetString("3F6AFF18-1179-446A-B7A8-3B6CB77C2E92", @"Your Export Declaration Message for Job {0} has a request to send an AES Entire Message (E_EXP_ENT). For details please follow the Link to the Job.", declaration.JE_DeclarationReference));
			htmlBody.Append((NoResString)"<br/>"); // Html
			htmlBody.Append(Res.GetString("58DEFCA3-473A-4545-ADE8-F796313DFDEA", "Date for latest response of AES Entire Message: <b>{0}<b/>", latestResponseDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)));

			return htmlBody.ToString();
		}
	}
}
