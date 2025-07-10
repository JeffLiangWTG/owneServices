using System;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSDeclarationInfoResponseMessageProcessor : CDSMessageProcessor<CDSDeclarationInfoResponseEDIMessage>
	{
		public CDSDeclarationInfoResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString ProcessMessageCore(CDSDeclarationInfoResponseEDIMessage cdsEDIMessage, BusinessObjectFactory factory)
		{
			if (cdsEDIMessage?.MessageDataObject is DeclarationStatusResponse)
			{
				ProcessMessageDetails(cdsEDIMessage, factory);
				SendEmailNotification(cdsEDIMessage);
			}

			return Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK;
		}

		void ProcessMessageDetails(CDSDeclarationInfoResponseEDIMessage cdsEDIMessage, BusinessObjectFactory factory)
		{
			var messageDataObject = cdsEDIMessage?.MessageDataObject as DeclarationStatusResponse;
			foreach (var detail in messageDataObject?.DeclarationStatusDetails)
			{
				UpdateDeclarationStatusDetails(detail, factory);
			}
		}

		void UpdateDeclarationStatusDetails(DeclarationStatusResponseDeclarationStatusDetails detail, BusinessObjectFactory factory)
		{
			var cusEntryHeader = detail.GetMessageAttacheeFromMRN(factory) as Business.Declaration.CusEntryHeader;
			if (cusEntryHeader != null)
			{
				var dec = detail.Declaration;
				using (DisposableEnvironment.ForBranch(cusEntryHeader.Branch.PK.ToGuid()))
				{
					cusEntryHeader.CH_RouteOfEntry = dec?.ROE;
					cusEntryHeader.CH_ImportClearanceStatusICS = dec?.ICS;
					cusEntryHeader.CH_IrcInventoryReturnCode = dec?.IRC;
					cusEntryHeader.CH_EntryReleaseDate = dec.AcceptanceDateTime.Item.ToZDateTime();
				}
			}
		}

		void SendEmailNotification(CDSDeclarationInfoResponseEDIMessage cdsEDIMessage)
		{
			var emailContent = cdsEDIMessage.EM_MessageInterpretation;

			if (!emailContent.IsEmpty)
			{
				var email = new HtmlNotificationEmailSender().CreateEmail(EmailSubject, emailContent);
				var emailSender = new EmailSender(new LoggingInformation());

				emailSender.SendEmail_SaveNow(email,
							GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationCDS, "", GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty),
							GBCustomsDataRegistry.Instance.GetNotificationItem("", "", GBCustomsDataRegistry.Instance.NotificationCDS));
			}
		}
		public const string EmailSubject = "CDS DIS Query Response";

		protected override string MessageFriendlyNameCore => "CDS Declaration Query Response Message";

		protected override ZString MessageType => CDSEDIMessageTypeList.Codes.QueryResponse;
	}
}

