using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Edifact;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRMessageResponseProcessor : CustomsMessageProcessor, IErrorNotification
	{
		public CMRMessageResponseProcessor(LoggingInformation logger, ZString messageCode, ZString messageName)
			: base(logger, messageCode, messageName)
		{
		}

		public CMRMessageResponseProcessor(LoggingInformation logger, Func<EDIMessage, string> getMessageType, ZString messageName)
			: base(logger, getMessageType, messageName)
		{
		}

		public static readonly Overridable<bool> SendReportToWebUsers = new Overridable<bool>();

		#region Constants

		protected const string RejectedString = "REJECTED";
		protected const string AcceptedString = "ACCEPTED";
		protected const string WithdrawnString = "WITHDRAWN";
		protected const string WebUserReferenceRegEx = @"^([a-zA-Z0-9_\-\.]+)\@((([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}) \([A-Z]{3,9}\)";

		#endregion

		#region Implementation

		protected virtual bool IsUnsolicitedMessage
		{
			get { return false; }
		}

		protected override string DoProcessingReturningStatus(EDIMessage message)
		{
			try
			{
				message.Factory.Saved -= Factory_Saved;
				delayEmailReport = false;
				emailReportThatHasBeenDelayed = null;
				var result = DoProcessingReturningStatusCore(message);

				var depotMessage = message as ICMRDepotMessage;
				if (depotMessage != null && depotMessage.UnmatchedContainers.Count > 0)
				{
					var mailDef = depotMessage.ConstructUnmatchedContainersEmail(depotMessage.UnmatchedContainers, message.Factory, MessageFriendlyName);
					SendErrorReport(message.EM_LinkedObject, mailDef);
				}

				var carstMessage = message as CMRCARSTMessage;
				if (carstMessage != null && carstMessage.IsUnmatchedHouseReportRequired)
				{
					var mailDef = carstMessage.ConstructUnmatchedHouseEmail(MessageFriendlyName);
					SendErrorReport(message.EM_LinkedObject, mailDef);
				}

				var cusresMessage = message as CMRCUSRESMessage;
				if (cusresMessage != null)
				{
					cusresMessage.ResetCUSRESCache();
				}

				return result;
			}
			catch (MessageProcessorException ex)
			{
				MessageProcessorErrorReporter.ProcessException(ex);
			}

			return MessageErrorCode;
		}
		protected bool delayEmailReport;
		EmailDef emailReportThatHasBeenDelayed;

		protected override void SendReport(EmailDef email)
		{
			if (delayEmailReport && emailReportThatHasBeenDelayed == null)
			{
				emailReportThatHasBeenDelayed = email;
			}
			else
			{
				base.SendReport(email);
			}
		}

		protected void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				factory.Saved -= Factory_Saved;
			}

			new BondedWarehouseMessageProcessor(incomingMessage.PK, emailReportThatHasBeenDelayed, SendEmail).ProcessAfterSaved(savedSuccessfully);
		}

		protected bool ShouldUpdateInward(ZString whsStatus)
		{
			return whsStatus.IsEmpty || whsStatus == WarehouseTransactionStatusList.Codes.InwardCanceled || whsStatus == WarehouseTransactionStatusList.Codes.InwardCreatedPending || whsStatus == WarehouseTransactionStatusList.Codes.InwardCreationHeld || whsStatus == WarehouseTransactionStatusList.Codes.InwardUpdatedPending;
		}

		class BondedWarehouseMessageProcessor : BondedWarehouseDeclarationMessageProcessor
		{
			internal BondedWarehouseMessageProcessor(ZGuid messagePK, EmailDef emailReportThatHasBeenDelayed, Action<EmailDef> sendMail)
				: base(messagePK, emailReportThatHasBeenDelayed)
			{
				this.sendMail = Argument.NotNull(sendMail, "sendMail");
			}
			readonly Action<EmailDef> sendMail;

			protected CusEntryHeader entryHeader
			{
				get { return (CusEntryHeader)message.EM_LinkedObject; }
			}

			protected new JobDeclaration declaration
			{
				get { return (JobDeclaration)base.declaration; }
			}

			protected override bool HasBeenWithdrawn
			{
				get { return entryHeader.HasBeenWithdrawn; }
			}

			protected override bool IsAmendmentError
			{
				get { return entryHeader.CH_Status == CustomsEntryStatus.FailAmendment.Code; }
			}

			protected override bool IsAmendmentClear
			{
				get { return entryHeader.CH_Status == CustomsEntryStatus.ClearAmendment.Code; }
			}

			protected override bool IsOriginalError
			{
				get { return entryHeader.CH_Status == CustomsEntryStatus.FailFormalLodge.Code; }
			}

			protected override bool IsWithdrawalError
			{
				get { return entryHeader.CH_Status == CustomsEntryStatus.FailWithdrawal.Code; }
			}

			protected override void SendEmailCore(EmailDef email)
			{
				sendMail(email);
			}

			protected override bool IsValidBondedWarehousingStatus(ZString warehouseTransactionStatus)
			{
				return base.IsValidBondedWarehousingStatus(warehouseTransactionStatus) || warehouseTransactionStatus.IsEmpty;
			}

			protected override void PublishUniversalShipmentToBondedWarehouseInward()
			{
				if (IsFinalisedOrCustomsPaymentPaid())
				{
					base.PublishUniversalShipmentToBondedWarehouseInward();
				}
				else if (entryHeader.CH_WarehouseTransactionStatus != WarehouseTransactionStatusList.Codes.InwardCreationHeld)
				{
					var result = entryHeader.Declaration.PublishShipmentForWHSInward(true);
					if (result != null)
					{
						var warehouseJob = result.FindJobIfExists() as IRelatedJob;
						switch (result.ResultType)
						{
							case UniversalResult.HadErrors:
								SendEmail(GetWHSEmail(Res.GetString("F3602C82-9CE7-4D71-A008-4BE082B90FE8", "Failed to create Stock Levels Hold"), Res.GetString("36417F77-C582-483B-8795-08142E1761D3", "<p>Cannot create Stock Levels Hold due to the following errors.{0}</p>{1}", GetInwardWarehouseJobInfo(warehouseJob), ReplaceNewLineWithHTMLLineBreak(result.ErrorMessage))));
								break;
							case UniversalResult.Internal:
							case UniversalResult.External:
								SendEmail(GetWHSEmail(Res.GetString("1CB8D301-CE6F-4F6E-951E-6C568215203F", "Stock Levels Hold"), Res.GetString("C918F0A1-BE98-44C5-AC1B-B771B0E27D80", "<p>Stock Levels have been held.{0}</p>", GetInwardWarehouseJobInfo(warehouseJob))));
								break;
						}
					}
				}
			}

			bool IsFinalisedOrCustomsPaymentPaid()
			{
				return entryHeader.CH_EntryStatus == CMRImportEntryAdvice.Finalised.Code || entryHeader.EntryPayInfos.AsEnumerable().Any(x => x.C9_TransactionType == PaymentTransactionTypeList.Codes.CustomsChargePayment || x.C9_TransactionType == PaymentTransactionTypeList.Codes.CustomsAQISPayment);
			}

			protected override string GetReferenceDetail()
			{
				return Res.GetString("7124d673-ed93-4734-b8ed-f729a40e2fd1", @"<strong>Reference Number: {0}<br />
Declaration Reference: {1}<br />
Entry Number: {2}<br />", entryHeader.CH_BGMReference, EmailDefBuilder.GetJobLink(declaration, declaration.JE_DeclarationReference), entryHeader.EntryNumber);
			}
		}

		void SendEmail(EmailDef email)
		{
			if (email.Recipients.Count == 0)
			{
				var userToNotify = GetUserToNotify(entryHeader);
				if (userToNotify != null && !userToNotify.GS_EmailAddress.IsEmpty) // Possibly add registries option to select group or user.
				{
					email.AddRecipientForSystemCommunication(userToNotify.GS_EmailAddress);
				}
				else
				{
					email.AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty((Guid)Env.Registry.RawRegistry.NotificationGroup.GetFallBackValueAtAllLevels(entryHeader.RegistryCompanyPK, entryHeader.RegistryBranchPK, Guid.Empty), Env.Registry.RawRegistry.NotificationGroup);
				}
			}

			Env.OutgoingCustomsMailManager.CreateAndSave(email);
			if (email == emailReportThatHasBeenDelayed)
			{
				emailReportThatHasBeenDelayed = null;
			}
		}

		protected virtual void ProcessUnmatchedResponseMessage()
		{
			if (!AUCustomsDataRegistry.Instance.IgnoreUnknownResponses.Value)
			{
				throw new CouldNotFindLinkedObjectException(incomingMessage.BGMMessageType + "/" + incomingMessage.SendersReference, incomingMessage, this);
			}
		}

		protected virtual bool FindShipment()
		{
			return false;
		}

		protected override bool ReportWhenNoParent
		{
			get { return true; }
		}

		protected override string DoPreProcessingReturningStatus(EDIMessage message)
		{
			try
			{
				return DoPreProcessingReturningStatusCore(message);
			}
			catch (MessageProcessorException ex)
			{
				MessageProcessorErrorReporter.ProcessException(ex);
			}

			return MessageErrorCode;
		}

		string DoPreProcessingReturningStatusCore(EDIMessage message)
		{
			var status = Messaging.Integration.EDIMessageStatusList.Codes.PreProcessedOK;

			incomingMessage = (CMRCUSRESMessage)message;
			incomingMessage.SetEM_LinkedObject();
			if (incomingMessage.EM_LinkedObject != null || FindShipment())
			{
				outgoingMessage = incomingMessage.LastSentOrPendingOutgoingCMRMessage;
				if (outgoingMessage != null)
				{
					message.EM_GB = outgoingMessage.EM_GB;
				}
			}
			else
			{
				ProcessUnmatchedResponseMessage();
				Logger.LogWarning(ZString.Format("Unable to find business object for {0}/{1}; message status set to ERROR.", incomingMessage.BGMMessageType, incomingMessage.SendersReference));
				status = MessageErrorCode;
			}

			return status;
		}

		string DoProcessingReturningStatusCore(EDIMessage message)
		{
			incomingMessage = (CMRCUSRESMessage)message;
			var status = Messaging.Integration.EDIMessageStatusList.Codes.PreProcessedOK;

			if (incomingMessage.EM_LinkedObject != null)
			{
				outgoingMessage = incomingMessage.LastSentOrPendingOutgoingCMRMessage;
			}
			else
			{
				status = DoPreProcessingReturningStatus(message);
			}

			if (status == Messaging.Integration.EDIMessageStatusList.Codes.PreProcessedOK)
			{
				cUSRES = (CUSRESMessage)message.GetAutoEdifactMessageUsingNamedFactory(AuEdifactMessageFactory.AUCMessageFactory, new UNOCCMRCharacterSet());

				consolidatedDeclaration = incomingMessage.EM_LinkedObject as ConsolidatedDeclaration;
				entryHeader = consolidatedDeclaration != null ? ((JobDeclaration)consolidatedDeclaration.LeadDeclaration).EntryHeader : incomingMessage.EM_LinkedObject as CusEntryHeader;
				declaration = entryHeader?.Declaration ?? incomingMessage.EM_LinkedObject as JobDeclaration;

				statusType = incomingMessage.GetStatus();
				statusDescription = incomingMessage.GetStatusDescription();
				SetMessageSubType();

				if (DoAdditionalProcessing())
				{
					var report = GetReport();

					if (SendReportToWebUsers.Value)
					{
						var webUsersMailAddresses = GetAssociatedWebUsersAddresses(incomingMessage.EM_LinkedObject);
						if (webUsersMailAddresses.Length > 0)
						{
							var emailGroupUtility = new EmailGroupUtility();
							webUsersMailAddresses = webUsersMailAddresses.Where(x => !emailGroupUtility.IsHostNotificationEmail(x)).ToArray();
							report.AddRecipientForSystemCommunication(webUsersMailAddresses, RecipientDef.RecipientTypes.CC);
						}
					}

					if (ShouldSendAcknowledgementReport)
					{
						SendAcknowledgementReport(incomingMessage.EM_LinkedObject, report);
					}
					else
					{
						SendErrorReport(incomingMessage.EM_LinkedObject, report);
					}

					if (statusType.Contains(RejectedString))
					{
						incomingMessage.CancelPendingMessages(incomingMessage);
					}
					else
					{
						incomingMessage.ReleasePendingMessages();
					}

					status = Messaging.Integration.EDIMessageStatusList.Codes.Received;
				}
				else
				{
					status = MessageErrorCode;
				}
			}

			return status;
		}

		protected virtual bool ShouldSendAcknowledgementReport
		{
			get { return statusType == CMRDocumentStatus.Clear.Code || statusType == CMRDocumentStatus.Withdrawn.Code; }
		}

		protected virtual bool DoAdditionalProcessing()
		{
			return true;
		}

		protected virtual string MessageErrorCode
		{
			get { return Messaging.Integration.EDIMessageStatusList.Codes.Error; }
		}

		protected CUSRESMessage cUSRES;
		protected ZString statusType;
		protected ZString statusDescription;
		protected EDIMessage outgoingMessage;
		protected CMRCUSRESMessage incomingMessage;
		protected CusEntryHeader entryHeader;
		protected JobDeclaration declaration;
		protected ConsolidatedDeclaration consolidatedDeclaration;

		#region Send Report Emails to WebUsers

		protected internal string[] GetAssociatedWebUsersAddresses(BusinessObject linkedBusinessObject)
		{
			var eventFilter = new ZQuery(StmALogSchema.SL_Parent, SQLComparisonOperator.Equal, linkedBusinessObject.PK);
			eventFilter.AddToFilter(JoinCondition.And, StmALogSchema.SL_GS_NKUser, SQLComparisonOperator.Equal, WebUserCode);
			eventFilter.AddToFilter(JoinCondition.And, StmALogSchema.SL_Reference, SQLComparisonOperator.NotEqual, "");

			var events = new StmALogCollection(linkedBusinessObject.Factory);
			events.Load(eventFilter);
			var addressList = new ArrayList();

			var re = new Regex(WebUserReferenceRegEx);

			foreach (var @event in events.Cast<StmALog>())
			{
				string emailAddress = @event.SL_Reference;
				if (re.IsMatch(emailAddress))
				{
					emailAddress = emailAddress.Substring(0, emailAddress.IndexOf(" "));
					if (!addressList.Contains(emailAddress))
					{
						addressList.Add(emailAddress);
					}
				}
			}

			var result = new string[addressList.Count];
			addressList.CopyTo(result);
			return result;
		}

		protected internal const string WebUserCode = "ZZ";

		#endregion

		protected virtual void SetMessageSubType()
		{
			if (statusType.IndexOf(CMRMessage.CMRMessageStatusDescription.REJECTED) != -1)
			{
				incomingMessage.EM_MessageSubType = CMRMessage.ManifestResponseSubTypes.Rejected;
			}
			else if (statusType.IndexOf(CMRMessage.CMRMessageStatusDescription.ERROR) != -1)
			{
				incomingMessage.EM_MessageSubType = CMRMessage.ManifestResponseSubTypes.Error;
			}
			else if (statusType.IndexOf(CMRMessage.CMRMessageStatusDescription.CLEAR) != -1 || statusType.IndexOf(CMRMessage.CMRMessageStatusDescription.ACCEPTED) != -1)
			{
				incomingMessage.EM_MessageSubType = CMRMessage.ManifestResponseSubTypes.Clear;
			}
			else if (statusType.IndexOf(CMRMessage.CMRMessageStatusDescription.WITHDRAWN) != -1)
			{
				incomingMessage.EM_MessageSubType = CMRMessage.ManifestResponseSubTypes.Withdrawn;
			}
		}

		#region Email Report

		protected EmailDef GetReport()
		{
			var respondeeDescription = CMRRespondeeWrapper.GetWrapper(incomingMessage.EM_LinkedObject).ShortDescription;
			var subject = string.Format("{0} Message{1} - {2}", MessageFriendlyName, (!respondeeDescription.IsEmpty ? " for " + respondeeDescription : string.Empty), statusType);
			var emailBuilder = new EmailDefBuilder(subject, EmailDefBuilder.HtmlTemplates.FreeFormResponse);
			if (!incomingMessage.SupportsHTMLResponseEmails)
			{
				emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.HeaderSectionDetails, incomingMessage.Report.Replace("\r\n", "<br>"));
			}
			else
			{
				emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.HeaderSectionDetails, incomingMessage.ReportForHTMLHeaderSection.Replace("\r\n", "<br>"));
				if (incomingMessage.ErrorNotifications.Count > 0)
				{
					var creator = new HtmlTableCreator(new[] { "Location", "Code", "Text" });
					foreach (var notification in incomingMessage.ErrorNotifications)
					{
						creator.WriteRow(notification.Line, notification.Code, notification.Text);
					}
					emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, incomingMessage.ErrorNotificationsCaption.Replace("\r\n", "") + "<br>" + creator.ToHtml());
				}

				if (incomingMessage.LineStatusNotifications.Count > 0)
				{
					var creator = new HtmlTableCreator(new[] { "Line", "Status", "Description" });
					foreach (var notification in incomingMessage.LineStatusNotifications)
					{
						creator.WriteRow(notification.Line, notification.Code, notification.Text);
					}
					emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml2, incomingMessage.LineStatusNotificationsCaption + "<br>" + creator.ToHtml());
				}

				if (incomingMessage.AdviceNotifications.Count > 0)
				{
					var creator = new HtmlTableCreator(new[] { "Location", "Code", "text" });
					foreach (var notification in incomingMessage.AdviceNotifications)
					{
						creator.WriteRow(notification.Line, notification.Code, notification.Text);
					}
					emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml3, "Message Advices<br>" + creator.ToHtml());
				}

				emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.EndSectionDetails, incomingMessage.ReportForHTMLFooterSection.Replace("\r\n", "<br>"));
			}

			emailBuilder.AddArgReplacement(MessageFriendlyName);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.FromMessageSender, MessageSender);
			if (outgoingMessage != null)
			{
				emailBuilder.AddAttachment(OutgoingMessageAttachmentFilename, ConvertToNiceOutput(outgoingMessage.EM_MessageText));
			}

			if (incomingMessage != null)
			{
				emailBuilder.AddAttachment(IncomingMessageAttachmentFilename, ConvertToNiceOutput(incomingMessage.EM_MessageText));
			}

			return emailBuilder.ToEmail();
		}

		protected static string MessageSender
		{
			get { return " from the ACS"; }
		}

		public const string IncomingMessageAttachmentFilename = "IncomingMessage.edi";
		public const string OutgoingMessageAttachmentFilename = "OutgoingMessage.edi";

		#endregion

		#region Data Retrievers

		protected ZString GetCAN(CUSRESMessage message)
		{
			foreach (SegmentGroup6 group6 in message.Group6)
			{
				foreach (RFFSegment rFF in group6.RFF)
				{
					if (rFF.Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.TransactionReferenceNumber.ToString())
					{
						return rFF.Reference.ReferenceIdentifier;
					}
				}
			}
			return ZString.Empty;
		}

		#endregion

		#region Overridden Properties

		protected override ZGuid AcknowledgementEmailGroup
		{
			get
			{
				if (declaration != null)
				{
					return Env.Registry.AUCustoms.ExportDeclarationSendAcknowledgementsToGroupForBranch(declaration.RegistryCompanyPK, declaration.BranchOfEmailGroupRegistry);
				}
				else
				{
					return Env.Registry.AUCustoms.ExportDeclarationSendAcknowledgementsToGroup;
				}
			}
		}

		protected override ZString AcknowledgementEmailMode
		{
			get
			{
				return Env.Registry.AUCustoms.ExportDeclarationSendAcknowledgements;
			}
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get
			{
				if (declaration != null)
				{
					return Env.Registry.AUCustoms.ExportDeclarationSendImpedimentsToGroupForBranch(declaration.RegistryCompanyPK, declaration.BranchOfEmailGroupRegistry);
				}
				else
				{
					return Env.Registry.AUCustoms.ExportDeclarationSendImpedimentsToGroup;
				}
			}
		}

		protected override ZString ImpedimentEmailMode
		{
			get
			{
				return Env.Registry.AUCustoms.ExportDeclarationSendImpediments;
			}
		}

		protected override ZGuid ErrorEmailGroup
		{
			get
			{
				if (declaration != null)
				{
					return Env.Registry.AUCustoms.ExportDeclarationSendErrorsToGroupForBranch(declaration.RegistryCompanyPK, declaration.BranchOfEmailGroupRegistry);
				}
				else
				{
					return Env.Registry.AUCustoms.ExportDeclarationSendErrorsToGroup;
				}
			}
		}

		protected override ZString ErrorEmailMode
		{
			get
			{
				return Env.Registry.AUCustoms.ExportDeclarationSendErrors;
			}
		}

		#endregion

		#endregion

		#region IErrorNotification members

		void IErrorNotification.SendError(EmailDef email)
		{
			SendErrorReport(incomingMessage == null ? null : incomingMessage.EM_LinkedObject, email);
		}

		void IErrorNotification.SendErrorToPostMaster(EmailDef email)
		{
			if (email.Recipients.Count == 0 && email.CCRecipients.Count == 0 && email.BCCRecipients.Count == 0)
			{
				try
				{
					Env.OutgoingCustomsMailManager.CreateAndSave(email, Core.Constants.Groups.PostMastersGroupPK, GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));
				}
				catch (EmailHasNoRecipientsException)
				{ }
			}
		}

		void IErrorNotification.LogError(string errorMessage)
		{
			Logger.ContinueDebugLog(errorMessage);
		}

		string IErrorNotification.MessageProcessorName
		{
			get { return MessageFriendlyName; }
		}

		#endregion

		#region TestCase

#if DEBUG

#endif

		#endregion
	}
}
