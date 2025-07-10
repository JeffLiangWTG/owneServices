using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.ExitControl.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	class EXTSTAMessageProcessor : ExportMessageProcessor<AesInboundEDIMessage<IEXTSTA>, IEXTSTA>
	{
		public EXTSTAMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("3f4b37d3-bcd6-4a02-a4eb-41afc461053c", "Export EXTSTA Message Processor");

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AesInboundEDIMessage<IEXTSTA> message)
		{
			var status = EDIMessageStatusList.Codes.ProcessedOK;

			if (message.EM_LinkedObject is CusExitReport cusExitReport)
			{
				var exitStatus = message.DataProvider.ExitStatus;
				cusExitReport.CER_Status = exitStatus;
				cusExitReport.Logs.AddNew(Events.CustomsEntryStatus, exitStatus, ZDateTime.Now.ToOffset());

				var consignment = cusExitReport.Consignment;
				if (consignment != null)
				{
					consignment.CXC_Status = exitStatus;
					CreateEvent(consignment, exitStatus);
				}

				attachedDocumentsCached = message.AttachedDocuments;

				var body = CreateEmailBody(message.DataProvider, cusExitReport);
				var subject = Res.GetString("5f2bea97-dabf-4bf3-a53e-a8e37d57c484", "AES EXT Status Message");

				GenerateHtmlEmailAndSendToOriginalOrGroup(
					factory: factory,
					relatedJob: cusExitReport.Header,
					messageTypeInSubject: subject,
					body,
					isFailure: false,
					branchForEmailLogo: message.Branch,
					sourceBusinessObject: cusExitReport,
					() => GetOutboundMessages().FirstOrDefault()
				);
			}
			else
			{
				status = EDIMessageStatusList.Codes.Error;
			}

			message.SetLogbookRegistrationNumber(message.DataProvider.MovementReferenceNumber);
			message.EM_Status = status;

			EDIMessage[] GetOutboundMessages()
			{
				EDIMessage[] result = null;
				var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCode)
					.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit)
					.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Sent)
					.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, cusExitReport.PK)
					.AddToFilter(EDIMessageSchema.EM_SystemCreateUser, SQLComparisonOperator.NotEqual, User.ServiceUserCode)
					.AddToFilter(EDIMessageSchema.EM_GB, GlbCompany.CurrentCompany.Branches.GetPKs());
				result = factory.Load<EDIMessage>(query).OrderByDescending(x => x.EM_SystemCreateTimeUtc).ToArray();
				return result ?? Array.Empty<EDIMessage>();
			}
		}

		protected override EmailDef GenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure,
			IGlbBranch branchForEmailLogo)
		{
			var email = base.GenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, branchForEmailLogo);
			AttachDocumentsToEmail(email, attachedDocumentsCached);
			return email;
		}

		protected override BusinessObject GetLinkedObject(AesInboundEDIMessage<IEXTSTA> message)
		{
			return GetLinkedObjectFromOriginalMessage(message.Factory, message.DataProvider?.ReferencedMessageIdentifier);
		}

		void CreateEvent(EU.ExitControl.Business.CusExitConsignment consignment, string status)
		{
			consignment.GetLogs().AddNew(AutoEvents.CustomsEntryStatus, status);
		}

		string CreateEmailBody(IEXTSTA provider, CusExitReport report)
		{
			var emailBody = new StringBuilder();

			emailBody.Append(Res.GetString("bebbaab3-1fa1-49cb-aed1-a36751b32280",
				"Your Exit Control Message for {0} has a Status Message. For details, please follow the Link to the Job.",
				report.Header.CXH_JobReference));

			emailBody.Append((NoResString)@"<br/><br/>");

			var emailTable = new HtmlTableCreator();

			var exitStatus = provider.ExitStatus;
			emailTable.WriteRow(Res.GetString("2f4e8ec9-7c93-4c4d-bb9b-f13f303a9f05", "MRN:"), provider.MovementReferenceNumber);
			emailTable.WriteRow(Res.GetString("dd142014-4667-4864-9047-87c91f06217c", "Status:"), exitStatus);
			emailTable.WriteRow(Res.GetString("9a0e2585-6c80-4a9f-a144-5afca44fa47b", "Status Text:"), statusCodeList.GetDescriptionFromCode(exitStatus));

			emailBody.Append(emailTable.ToHtml());

			return emailBody.ToString();
		}

		readonly A0116ATLASStatusCodeList statusCodeList = new A0116ATLASStatusCodeList();

		protected sealed override IRegistryItem GetEmailGroupRegistryItem() => ExitControlCustomsDataRegistry.Instance.SendExitControlAcknowledgements;

		protected sealed override ZGuid GetEmailGroupPK(IRegistryItem registryItem, IGlbBranch branch)
		{
			var result = ZGuid.Empty;
			if (registryItem != null)
			{
				result = ((ExitControlGroupNotification)registryItem.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty)).SendGroupPK;
			}
			return result;
		}

		protected sealed override ZString GetEmailSendMode(IGlbBranch branch) => ExitControlCustomsDataRegistry.Instance.SendExitControlAcknowledgements.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty).SendMode;
		IReadOnlyCollection<AttachedDocument> attachedDocumentsCached;
	}
}
