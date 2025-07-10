using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.BR.Business
{
	public abstract class BRCResponseMessageProcessor : ApplicationTypeMessageProcessor
	{
		public BRCResponseMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string ApplicationCodeCore
		{
			get { return EDIMessage.ApplicationCodes.BRCustoms; }
		}

		protected sealed override bool RequiresPreProcessingCore => true;

		protected sealed override void PreProcessMessageCore(EDIMessage message)
		{
			if (GetLinkedObject(message) is BusinessObject linkedObject)
			{
				message.EM_LinkedObject = linkedObject;

				if (linkedObject is IMessageAttachee attachee)
				{
					message.EM_GB = attachee.BranchPK;
				}
			}
			else if (FailIfLinkedObjectNotFound)
			{
				message.EM_Status = EDIMessage.Status.Failed;
				Logger.LogError($"Unable to locate the related Business Object for {message.EM_MessageType} message #{message.EM_MessageNum}");
			}

			if (message.EM_Status == EDIMessage.Status.Queued)
			{
				base.PreProcessMessageCore(message);
			}

			AddProcessingLog(message);
		}

		protected virtual bool FailIfLinkedObjectNotFound => true;

		protected abstract BusinessObject GetLinkedObject(EDIMessage message);

		protected BusinessObject GetLinkedObjectFromOutgoingMessage(EDIMessage incomingMessage, int? seq = null)
		{
			var outgoingMessage = BRMessageHelper.GetOutgoingMessage(incomingMessage, seq);
			if (outgoingMessage == null)
			{
				Logger.LogError($"Unable to locate the related outgoing message for {incomingMessage.EM_MessageType} message #{incomingMessage.EM_MessageNum}");
			}
			return outgoingMessage?.EM_LinkedObject;
		}

		protected sealed override void ProcessMessageCore(EDIMessage message)
		{
			ProcessResponseMessage(message);

			if (message.EM_Status != EDIMessage.Status.Failed && message.EM_Status != EDIMessage.Status.Discarded)
			{
				message.EM_Status = EDIMessage.Status.Received;
			}

			AddProcessingLog(message);
		}

		void AddProcessingLog(EDIMessage message)
		{
			if (Logger.UserLogStrings.Count > 0)
			{
				message.Notes.AddNew(true, ProcessingLog, string.Join(System.Environment.NewLine, Logger.UserLogStrings.Cast<string>()));
			}
		}

		static readonly string ProcessingLog = (NoResString)"Processing Log";

		protected abstract void ProcessResponseMessage(EDIMessage message);

		#region SuppressResourceStringsCheckRegion Create Email

		protected void SendErrorNotification(EDIMessage incomingMessage, EDIMessage outgoingMessage, string subject, string heading)
		{
			if (!incomingMessage.EM_MessageInterpretation.IsEmpty)
			{
				var (emailModeRegistry, emailGroupRegistry) = GetSendErrorsToRegistry(incomingMessage);
				if (emailModeRegistry != null && emailGroupRegistry != null!)
				{
					SendEmailToNotificationGroup(incomingMessage, outgoingMessage, emailModeRegistry, emailGroupRegistry, subject, heading);
				}
			}
		}

		static (CodePairRegistryItem, GuidRegistryItem) GetSendErrorsToRegistry(EDIMessage incomingMessage)
		{
			switch (incomingMessage.EM_LinkedObject)
			{
				case CusEntryHeader entryHeader when entryHeader.Declaration.IsLPCO:
					return (BRCustomsDataRegistry.Instance.SendLPCOErrorsTo, BRCustomsDataRegistry.Instance.SendLPCOErrorsToGroup);
				case CusEntryHeader entryHeader when entryHeader.IsExport:
					return (BRCustomsDataRegistry.Instance.SendExportDeclarationErrorsTo, BRCustomsDataRegistry.Instance.SendExportDeclarationErrorsToGroup);
				case CusEntryHeader entryHeader when entryHeader.IsImport:
					return (BRCustomsDataRegistry.Instance.SendImportDeclarationErrorsTo, BRCustomsDataRegistry.Instance.SendImportDeclarationErrorsToGroup);
				case GlbExternalPassword_BRS:
					return (BRCustomsDataRegistry.Instance.SendSubscriptionErrorsTo, BRCustomsDataRegistry.Instance.SendSubscriptionErrorsToGroup);
				case CusGoodsCatalog:
					return (BRCustomsDataRegistry.Instance.SendProductCatalogErrorsTo, BRCustomsDataRegistry.Instance.SendProductCatalogErrorsToGroup);
				default:
					return (null, null);
			}
		}

		static string GetJobLink(EDIMessage incomingMessage)
		{
			switch (incomingMessage.EM_LinkedObject)
			{
				case CusEntryHeader entryHeader:
					return $"the job: {EmailDefBuilder.GetJobLink(entryHeader.Declaration, entryHeader.Declaration.JE_DeclarationReference)}";
				case GlbExternalPassword_BRS externalPassword:
					return $"the staff: {EmailDefBuilder.GetJobLink(ControllerIDs.GlbStaff, externalPassword.Staff.PK.ToGuid(), externalPassword.Staff.GS_FullName)}";
				default:
					return null;
			}
		}

		void SendEmailToNotificationGroup(EDIMessage incomingMessage, EDIMessage outgoingMessage,
			CodePairRegistryItem emailModeRegistry, GuidRegistryItem emailGroupRegistry,
			string subject, string heading)
		{
			var companyPK = incomingMessage.EM_GC.ToGuid();
			var branchPK = incomingMessage.EM_GB.ToGuid();

			var emailMode = emailModeRegistry.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
			if (emailMode != Core.Constants.EmailTo.NoEmails)
			{
				var emailGroupPK = emailGroupRegistry.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);

				var emailBuilder = GetEmailBuilder(incomingMessage, subject, heading);
				var originalSender = outgoingMessage?.UserWhoQueuedThisRecord;

				new Customs.Business.EmailSender(Logger).SendNotification(emailBuilder.ToEmail(), originalSender, emailMode, emailGroupPK, emailGroupRegistry, incomingMessage.Factory);
			}
		}

		static EmailDefBuilder GetEmailBuilder(EDIMessage message, string subject, string heading)
		{
			var emailBuilder = new EmailDefBuilder(subject, EmailDefBuilder.HtmlTemplates.EmptyWithDynamicHtml5);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtmlHeading, heading);

			var jobLink = GetJobLink(message);
			if (!string.IsNullOrEmpty(jobLink))
			{
				emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, $"Click here to open {jobLink}");
			}

			emailBuilder.AddDynamicHtmlReplacement(message.EM_MessageInterpretation);
			return emailBuilder;
		}

		#endregion
	}
}
