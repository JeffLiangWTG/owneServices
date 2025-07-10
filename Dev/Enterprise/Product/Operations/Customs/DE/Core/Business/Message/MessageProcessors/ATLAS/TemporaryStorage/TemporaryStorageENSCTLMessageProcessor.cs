using System.Collections.Generic;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class TemporaryStorageENSCTLMessageProcessor : TemporaryStorageMessageProcessor<AtlasInboundEDIMessage<IENSCTL>, IENSCTL>
	{
		public TemporaryStorageENSCTLMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("14221209-a43c-4bc2-a407-bfa8871f0ce0", "Temporary Storage ENSCTL Message Processor");

		protected override bool MustHaveLinkedObject => false;

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<IENSCTL> message) => null;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<IENSCTL> message)
		{
			var provider = message.DataProvider;
			message.EM_Status = AtlasEDIMessage.Status.Discarded;
			if (provider != null)
			{
				var orgHeader = factory.GetOrgHeaderFromEoriCode(provider.MessageRecipientIdentificationNumber);
				var premisesAddress = orgHeader?.GetOrgCusCode(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, provider.MessageRecipientSubsidiaryNumber)?.PremisesAddress;
				var emailAddress = premisesAddress?.OA_Email ?? ZString.Empty;
				if (!emailAddress.IsEmpty)
				{
					SendEmail();
					message.EM_Status = AtlasEDIMessage.Status.ProcessedOK;
				}
				else
				{
					message.CreateOrUpdateNote(Res.GetString("5b9923c7-d600-43c3-9477-661ced70bc07", "Could not send Notification Email"), Res.GetString("2e4bde76-7642-4c1f-89dd-cb1ce9c57886", "Could not send Notification Email due to non matching Data to obtain Organization, Address, Email-Address."));
				}
				message.SetLogbookRegistrationNumber(provider.MRN);

				void SendEmail()
				{
					attachedDocumentsCached = message.AttachedDocuments;
					GenerateHtmlEmailAndSendToOriginalOrGroup(factory
						, null
						, Res.GetString("f7af9fcd-2eee-4dfb-8acf-ecd6329b17fe", "EKS Declaration Message Status")
						, GetEmailBody(provider)
						, isFailure: false
						, message.Branch
						, null
						, () => emailAddress);
				}
			}
		}

		protected override EmailDef GenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure, IGlbBranch branchForEmailLogo)
		{
			var email = base.GenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, branchForEmailLogo);
			AttachDocumentsToEmail(email, attachedDocumentsCached);
			return email;
		}

		static ZString GetEmailBody(IENSCTL provider)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("48c4ee73-c56f-4ada-90db-458a1202aefc", "For details please see attached PDF-Report."));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			var tableCreator = new HtmlTableCreator();
			tableCreator.WriteRow(Res.GetString("6B05BCBC-71E2-42FB-AF7C-BBD3ECDF8686", "MRN"), provider.MRN);
			var transportDocumentNumber = provider.TransportDocumentNumber;
			if (!string.IsNullOrWhiteSpace(transportDocumentNumber))
			{
				tableCreator.WriteRow(Res.GetString("2eca7b34-eb58-4ebd-a6a8-da968c3a5a0d", "Reference"), transportDocumentNumber);
			}
			htmlBody.Append(tableCreator.ToHtml());
			return htmlBody.ToString();
		}

		IReadOnlyCollection<AttachedDocument> attachedDocumentsCached;
	}
}
