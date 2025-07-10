using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.MessageProcessors;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	public class MonthlyClosingCUSTRAMessageProcessor : MonthlyClosingMessageProcessor<AtlasInboundEDIMessage<ICUSTRA>, ICUSTRA>
	{
		public MonthlyClosingCUSTRAMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("F2A4DC64-9B26-4926-B3A9-BE98705B184D", "Monthly Closing CUSTRA Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<ICUSTRA> message) => GetCusReconDeclarationFromMRN(message.Factory, message.Branch.GB_GC, message.DataProvider?.MRN, message.DataProvider?.ReferenceNumber);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<ICUSTRA> message)
		{
			var reconDeclaration = (CusReconDeclaration)message.EM_LinkedObject;
			reconDeclaration.CRD_CustomsStatus = EntryStatus.TRA;

			message.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK;
			var referenceNumber = message.DataProvider.ReferenceNumber;
			var mrn = message.DataProvider.MRN;
			message.SetLogbookRegistrationNumber(new ZString[] { referenceNumber, mrn });
			SendEmail();

			void SendEmail()
			{
				var sendMRN = !mrn.IsNullOrEmpty() && CusEntryNumber.Load(reconDeclaration).Select(x => x.CE_EntryNum == mrn).Any();

				GenerateHtmlEmailAndSendToOriginalOrGroup(factory
					, reconDeclaration
					, Res.GetString("237B8ABA-92E5-40A3-A910-067A096738A5", "Monthly Closing CUSTRA – Customs Transmission Message")
					, GetEmailBody(reconDeclaration, message.DataProvider, sendMRN)
					, isFailure: false
					, message.Branch
					, reconDeclaration
					, () => reconDeclaration.Messages.LastSentOutgoingMessage);
			}

			UpdateReconEntryLines(reconDeclaration);
		}

		ZString GetEmailBody(CusReconDeclaration reconDeclaration, ICUSTRA provider, ZBool sendMRN)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("E7B9F1B8-F51C-48D0-B170-D53D9C58D504", "Your Monthly Closing Declaration for Job {0} received a Customs Transmission Message. For details please follow the link to the job.", reconDeclaration.CRD_JobReferenceNumber));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var transmissionDate = provider.ForwardedDate;
			var reason = provider.Reason;

			var tableCreator = new HtmlTableCreator();
			if (!provider.ReferenceNumber.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("E9D47285-5D85-464F-BC28-492C9182B764", "Registration Number"), provider.ReferenceNumber);
			}
			if (!provider.MRN.IsNullOrEmpty())
			{
				tableCreator.WriteRow(Res.GetString("EDF597A4-CA10-43F6-A9FC-9E9DB5D9B60D", "MRN"), provider.MRN);
			}
			if (!transmissionDate.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("4E90854C-233B-41DE-AE8E-4408FC91C0A6", "Transmission Date"), transmissionDate.ToString("dd.MM.yyyy"));
			}
			if (!reason.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("F2D341E3-0B26-47BC-B147-966B09F707F0", "Transmission Reason"), reason);
			}
			htmlBody.Append(tableCreator.ToHtml());

			return htmlBody.ToString();
		}

		void UpdateReconEntryLines(CusReconDeclaration reconDeclaration)
		{
			var allCusReconEntryLines = reconDeclaration.CusReconEntries.SelectMany(x => x.CusReconEntryLines).ToArray();
			foreach (var reconEntryLine in allCusReconEntryLines)
			{
				if (reconEntryLine.CRL_CustomsStatus != EntryStatus.REJ)
				{
					reconEntryLine.CRL_CustomsStatus = EntryStatus.TRA;
				}
			}
		}
	}
}
