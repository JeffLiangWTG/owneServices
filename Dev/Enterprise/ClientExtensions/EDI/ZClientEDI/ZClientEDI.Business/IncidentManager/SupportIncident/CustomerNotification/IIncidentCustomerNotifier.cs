using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;
using Enterprise.Client.EDI.Registry;
using Enterprise.Customs.Business;
using Enterprise.EConversation.Business;
using Enterprise.Environment;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	/// <summary>
	/// Notifies the customer and customer's CW1 of changes to their incident.
	/// </summary>
	public interface IIncidentCustomerNotifier
	{
		/// <summary>
		/// Send notification of changes. Called from incident OnSaveSuceeded.
		/// Incident is responsible for not calling the notifier recursively while in SendChanges.
		/// </summary>
		void SendChanges();

		void SendEmailNow(SupportIncidentEmail email);

		/// <summary>
		/// Send given email when the incident is next saved, i.e., during SendChanges/SendQueuedEmails.
		/// </summary>
		void SendEmailOnIncidentSave(SupportIncidentEmail email);

		/// <summary>
		/// Must be manually called from incident/work item/etc form after incident is saved.
		/// Some emails generated during SendChanges are to be previewed and edited by the user.
		/// But, we want to avoid showing UI during SendChanges.
		/// Such emails are queued internally and sent by calling this method.
		/// </summary>
		void SendQueuedEmails();

		SupportIncident RelatedIncident { get; }
	}

	/// <summary>
	/// Sends notifications to the customer.
	/// </summary>
	public interface IIncidentCustomerNotificationSender
	{
		void AddHyperlinks(SupportIncidentEmail email);
		void SendChanges(SupportIncident incident, SupportIncidentEmail email);
		void SendEmail(SupportIncidentEmail email);
	}

	public static class IncidentCustomerNotificationSenderExtension
	{
		public static void CreateAndSendConversationMessageToCustomer(this IIncidentCustomerNotificationSender sender, SupportIncident incident, SupportIncidentEmail email)
		{
			if (!incident.HasClientAndContact || incident.Contact.OC_Email.IsEmpty)
			{
				return;
			}

			var messages = incident.EConversation.GetNewLocalPublishedCustomerMessages();
			if ((email == null && messages.Any()) || messages.Any(x => !x.IsSystemMessage))
			{
				var conversationMessageEmail = SupportIncidentEmailQuickBuilder.CreateAwaitingResponseEmail(incident, messages).Item1;

				if (conversationMessageEmail != null)
				{
					conversationMessageEmail.MarkAsNoNeedSaveToEDocs();
					conversationMessageEmail.ShouldSaveBizOFactoryOnSent = false;
					sender.SendEmail(conversationMessageEmail);
				}
			}
		}
	}

	public class NoActionIncidentCustomerNotifier : IIncidentCustomerNotifier
	{
		public NoActionIncidentCustomerNotifier(SupportIncident incident) { RelatedIncident = incident; }
		public SupportIncident RelatedIncident { get; private set; }

		public void SendQueuedEmails() { }
		public void SendChanges() { }
		public void SendEmailNow(SupportIncidentEmail email) { throw new InvalidOperationException(); }
		public void SendEmailOnIncidentSave(SupportIncidentEmail email) { throw new InvalidOperationException(); }
	}

	/// <summary>
	/// Common base class
	/// </summary>
	public abstract class IncidentCustomerNotifier : IIncidentCustomerNotifier
	{
		protected IncidentCustomerNotifier(SupportIncident incident, IIncidentCustomerNotificationSender sender)
		{
			this.Incident = incident;
			this.HasEConversation = CustomerCanAccessEConversation(incident);
			this.Sender = sender;
		}

		public SupportIncident RelatedIncident => Incident;
		readonly protected SupportIncident Incident;
		readonly protected bool HasEConversation;
		readonly IIncidentCustomerNotificationSender Sender;
		List<SupportIncidentEmail> queuedEmails = new List<SupportIncidentEmail>();

		protected abstract SupportIncidentEmail CreateStandardEmailIfApplicable();

		protected SupportIncidentEmail CreateNewEmail(IEDIEmailTemplateBuilder template, Action<EmailContainer> additionalSetting = null)
		{
			if (LastEmailAllowedToBeCreated)
			{
				Argument.NotNull(template, nameof(template));
				var templateCode = template.GetIEDIEmailTemplate()?.TemplateCode ?? string.Empty;
				var container = new EmailContainer(SupportIncidentEmail.New(Incident, template, out LastEmailAllowedToBeCreated));
				if (LastEmailAllowedToBeCreated)
				{
					additionalSetting?.Invoke(container);
				}
				return container.Email;
			}
			return null;
		}

		/*
		 *	This container is only used to provide a way to edit email object when calling CreatingNewEmail
		 *	In short, this is a kind of boxing. And the developer can set the resulting email object however he wants
		 */
		protected class EmailContainer
		{
			public EmailContainer(SupportIncidentEmail email) { this.Email = email; }
			public SupportIncidentEmail Email { get; set; }
		}
		protected bool LastEmailAllowedToBeCreated = true;

		bool inSendChanges;

		public void SendChanges()
		{
			if (inSendChanges)
			{
				throw new InvalidOperationException("SendChanges called recursively");
			}

			inSendChanges = true;

			try
			{
				DoSendChanges();
			}
			finally
			{
				inSendChanges = false;
			}
		}

		void DoSendChanges()
		{
			foreach (var item in queuedEmails.ToArray())
			{
				if (!item.UserCanEdit || !CanPreview)
				{
					item.UserCanEdit = false;
					Sender.SendEmail(item);
					queuedEmails.Remove(item);
				}
			}

			var email = CreateStandardEmailIfApplicable();
			if (email != null)
			{
				if (email.ShouldAppendNewCustomerMessages)
				{
					AppendMessages(email, Incident.EConversation.GetNewLocalPublishedCustomerMessages(!email.BodyContainsResolutionMessage));
					Sender.AddHyperlinks(email);
				}
				if (email.UserCanEdit)
				{
					if (CanPreview)
					{
						queuedEmails.Add(email);
						email = null;
					}
					else
					{
						email.UserCanEdit = false;
					}
				}
			}

			if (LastEmailAllowedToBeCreated)
			{
				Sender.SendChanges(Incident, email);
			}
		}

		protected virtual bool CanPreview => false;

		public void SendQueuedEmails()
		{
			// Note: sending an email will save the incident factory
			// which can call this Notifier recursively.
			// Copy and clear the list first.
			var listCopy = queuedEmails;
			queuedEmails = new List<SupportIncidentEmail>();

			foreach (var email in listCopy)
			{
				if (ConfirmSendEmail(email))
				{
					email.UserCanEdit = false;
					Sender.SendEmail(email);
				}
			}
		}

		public void SendEmailNow(SupportIncidentEmail email)
		{
			Sender.AddHyperlinks(email);
			if (ConfirmSendEmail(email))
			{
				Sender.SendEmail(email);
			}
		}

		public void SendEmailOnIncidentSave(SupportIncidentEmail email)
		{
			Sender.AddHyperlinks(email);
			queuedEmails.Add(email);
		}

		protected virtual bool ConfirmSendEmail(SupportIncidentEmail email)
		{
			return true;
		}

		static void AppendMessages(SupportIncidentEmail email, IEnumerable<JobConversationMessage> messages)
		{
			if (messages.Any())
			{
				var newBody = new ZStringBuilder(email.Body);
				AppendMessages(newBody, messages);
				email.Body = newBody.ToString();
			}
		}

		static void AppendMessages(ZStringBuilder builder, IEnumerable<JobConversationMessage> messages)
		{
			if (messages.Any())
			{
				builder.AppendLine();
				builder.AppendLine();
				builder.AppendLine("Other Comments:");
				foreach (var message in messages)
				{
					builder.AppendLine(" - " + WebUtility.HtmlEncode(message.Body));
				}
			}
		}

		internal static bool CustomerCanAccessEConversation(SupportIncident incident)
		{
			if (incident.IsWebRequest)
			{
				return true;
			}

			if (!incident.IM_ClientIncidentReference.IsEmpty)
			{
				return incident.ClientSystemSupportsBiDirectionUpdate;
			}
			else
			{
				var database = incident.Database;
				bool isReplicateAllowed = ERequestEHubCustomerNotificationSender.IsAllowedToReplicateIncidentOnClient(database);
				if (isReplicateAllowed
					&& database.VersionCanSupportBiDirectionIncidentMessage == TriState.True
					&& incident.ClientCompany != null)
				{
					return true; // assumes it can be published to their system
				}
			}

			return false; // true when GLOW portal is for all
		}
	}

	/// <summary>
	/// Common class for locally initiated changes to the incident (i.e., from staff or service tasks like auto-deployment)
	/// </summary>
	/// 3
	public abstract class LocalChangeIncidentCustomerNotifier : IncidentCustomerNotifier
	{
		protected LocalChangeIncidentCustomerNotifier(SupportIncident incident, IIncidentCustomerNotificationSender sender)
			: base(incident, sender)
		{
		}

		protected override SupportIncidentEmail CreateStandardEmailIfApplicable()
		{
			if (ShouldSendWorkItemCompletedNotification)
			{
				return CreateWorkItemCompletedNotificationOrHandleFailure();
			}

			return null;
		}

		#region Work Item Completed

		public bool ShouldSendWorkItemCompletedNotification
		{
			get
			{
				if (!Incident.HasClientAndContact || Incident.ShouldStayIndependent)
				{
					return false;
				}

				string internallyClosedDisposition = (Incident.IM_Category == SupportIncidentCategoriesList.Codes.Defect ? SupportIncidentLookups.DispositionList.Constants.Closed.ClosedInternal : SupportIncidentLookups.DispositionList.Constants.Closed.Completed);
				bool hasCorrectDisposition = HasDispositionChangedTo(SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade)
											|| (!Incident.NeedUpgrade && (HasDispositionChangedTo(SupportIncidentLookups.DispositionList.Constants.Closed.Completed) || HasDispositionChangedTo(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedInternal)))
											|| (Incident.IsInternal && HasDispositionChangedTo(internallyClosedDisposition));
				var workItems = Incident.RelatedWorkItems;
				bool hasRelatedWorkItemsClosed = (workItems.Any() && workItems.Cast<NewWorkItem>().All(wi => wi.IsClosedOrCancelled) && workItems.Cast<NewWorkItem>().Any(wi => !wi.IsCancelled));

				return hasCorrectDisposition
					&& hasRelatedWorkItemsClosed
					&& !Incident.IsCreatedFromIssue
					&& !HasWorkItemCompletedEmailBeenSent();
			}
		}

		protected bool HasDispositionChangedTo(string disposition)
		{
			var resolutionCodeMatch = Incident.OriginalSnapShot.IM_ResolutionCode != Incident.IM_ResolutionCode && Incident.IM_ResolutionCode == disposition;
			return resolutionCodeMatch || Incident.OriginalSnapShot.IM_ClosureResolution != Incident.IM_ClosureResolution && Incident.IM_ClosureResolution == disposition;
		}

		bool HasWorkItemCompletedEmailBeenSent()
		{
			var messageList = Incident.EConversation.GetTimeOrderedMessages();
			return messageList.Any(msg => msg.Body == GetWorkItemCompletedLogComment());
		}

		string GetWorkItemCompletedLogComment()
		{
			return "Development Work Completed email sent to contact";
		}

		public SupportIncidentEmail CreateWorkItemCompletedNotificationOrHandleFailure()
		{
			var emailContents = new CustomerServiceIncidentWorkItemCompletedEmailContentBuilder(Incident, eRequestV2: HasEConversation, product: Incident.IM_Product, registryTemplateCode: Incident.IM_Category, Incident.NeedUpgrade);
			var email = CreateNewEmail(emailContents, (container) =>
			{
				if (container.Email != null)
				{
					container.Email.XAutoResponseSuppressHeaderValue = SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll;
					container.Email.PublicLogComment = GetWorkItemCompletedLogComment();
					if (!container.Email.CheckIsReadyToSendEmail())
					{
						container.Email = null;
						if (Incident.AssignedToCurrent != null && !Incident.AssignedToCurrent.GS_EmailAddress.IsEmpty)
						{
							var template = new CustomerServiceIncidentWorkItemCompletedDefaultEmailContentBuilder(Incident);
							var defaultEmail = EDIEmailBuilder.GetInstance(Incident).BuildEmailDefByTemplate(template);
							if (defaultEmail != null)
							{
								defaultEmail.AddRecipientForUserCommunication(Incident.AssignedToCurrent.GS_EmailAddress);
								Env.OutgoingMailManager.CreateAndSave(defaultEmail);
							}
						}
					}
				}
				else
				{
					Incident.AddInternalSystemLogMessage("No work item completed notification email sent because no template has been setup");
					Incident.Factory.Save();
				}
			});

			return email;
		}

		#endregion
	}

	/// <summary>
	/// Notifier for customer making changes to the incident, via the web/web service/eHub
	/// </summary>
	public class CustomerChangeIncidentCustomerNotifier : IncidentCustomerNotifier
	{
		public CustomerChangeIncidentCustomerNotifier(SupportIncident incident, IIncidentCustomerNotificationSender sender)
			: base(incident, sender)
		{
			this.isNew = !incident.IsInDatabase;
		}

		readonly bool isNew;

		protected override SupportIncidentEmail CreateStandardEmailIfApplicable()
		{
			SupportIncidentEmail email = null;
			if (ShouldSendIncidentClosedAsFeatureRequestEmail)
			{
				email = CreateIncidentClosedAsFeatureRequestEmail();
			}
			else if (ShouldSendSelfLoggedCustomerNotificationEmail)
			{
				email = CreateSelfLoggedCustomerNotificationEmailIfApplicable();
			}
			else if (ShouldSendIncidentClosedNotification)
			{
				if (Incident.GetClosedReopenRule() == ResolutionAndClosureBehaviour.Constants.Code.NeverAllow)
				{
					(email, LastEmailAllowedToBeCreated) = SupportIncidentEmailQuickBuilder.CreateFinalIncidentClosedEmail(Incident, false);
				}
				else
				{
					(email, LastEmailAllowedToBeCreated) = SupportIncidentEmailQuickBuilder.CreateIncidentClosedEmail(Incident, HasEConversation, Incident.IM_ResolutionCode, false);
				}
			}

			return email;
		}

		SupportIncidentEmail CreateIncidentClosedAsFeatureRequestEmail()
		{
			var (email, isAllowed) = SupportIncidentEmailQuickBuilder.CreateIncidentClosedEmail(Incident, HasEConversation, SupportIncidentLookups.DispositionList.Constants.FeatureAccepted, false);
			if (email != null)
			{
				email.MarkAsNoNeedSaveToEDocs();
			}
			else if (isAllowed)
			{
				Incident.AddInternalSystemLogMessage("No incident closed as feature request notification email sent because no template has been setup");
				try
				{
					Incident.Factory.Save();
				}
				catch (ZSaveConcurrencyException) { }
			}

			return email;
		}

		bool ShouldSendIncidentClosedAsFeatureRequestEmail
		{
			get
			{
				var incident = Incident;

				return incident.Contact != null && !incident.Contact.OC_Email.IsEmpty
					&& incident.IM_Category == SupportIncidentCategoriesList.Codes.FeatureRequest
					&& incident.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.FeatureAccepted
					&& (incident.IsNewIncident || (incident.OriginalSnapShot.IM_Status != incident.IM_Status && incident.IM_Status == SupportIncidentLookups.Status.Closed));
			}
		}

		SupportIncidentEmail CreateSelfLoggedCustomerNotificationEmailIfApplicable()
		{
			var emailContents = new SelfLoggedIncidentNotificationEmailContentBuilder(Incident, HasEConversation, Incident.IM_Product);
			var email = CreateNewEmail(emailContents, (container) =>
			{
				if (container.Email != null)
				{
					container.Email.XAutoResponseSuppressHeaderValue = SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll;
					container.Email.MarkAsNoNeedSaveToEDocs();
				}
				else if (emailContents.IsEmpty)
				{
					Incident.AddInternalSystemLogMessage("No incident created notification email sent because no template has been setup");
					try
					{
						Incident.Factory.Save();
					}
					catch (ZSaveConcurrencyException) { }
				}
			});
			return email;
		}

		bool ShouldSendSelfLoggedCustomerNotificationEmail
		{
			get
			{
				return isNew && Incident.Contact != null && !Incident.Contact.OC_Email.IsEmpty;
			}
		}

		bool ShouldSendIncidentClosedNotification
		{
			get
			{
				var incident = Incident;

				return incident.Contact != null && !incident.Contact.OC_Email.IsEmpty
					&& incident.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed
					&& incident.OriginalSnapShot.IM_ResolutionCode != SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed;
			}
		}
	}

	/// <summary>
	/// Notifier for service tasks making locally initiated changes to the incident.
	/// </summary>
	public class LocalSystemChangeIncidentCustomerNotifier : LocalChangeIncidentCustomerNotifier
	{
		public LocalSystemChangeIncidentCustomerNotifier(SupportIncident incident, IIncidentCustomerNotificationSender sender)
			: base(incident, sender)
		{
		}
	}

	/// <summary>
	/// Notifier for local staff making changes to the incident.
	/// </summary>
	public abstract class StaffChangeIncidentCustomerNotifier : LocalChangeIncidentCustomerNotifier
	{
		protected StaffChangeIncidentCustomerNotifier(SupportIncident incident, IIncidentCustomerNotificationSender sender)
			: base(incident, sender)
		{
		}

		protected override SupportIncidentEmail CreateStandardEmailIfApplicable()
		{
			SupportIncidentEmail email = null;

			var contact = Incident.Contact;

			if (contact != null)
			{
				if (!contact.OC_Email.IsEmpty)
				{
					if (ShouldSendNoClientResponseEmail || ShouldSendIncidentClosedNotification)
					{
						var shouldUseFollowUpTemplate = Incident.GetClosedReopenRule() == ResolutionAndClosureBehaviour.Constants.Code.NeverAllow && ShouldSendIncidentClosedNotification;
						(email, LastEmailAllowedToBeCreated) = SupportIncidentEmailQuickBuilder.CreateIncidentClosedEmail(Incident, HasEConversation, Incident.IM_ResolutionCode, displayNameIsSupportButSignedByCurrentUser: false, shouldUseFollowUpTemplate: shouldUseFollowUpTemplate);
					}

					if (email == null && ShouldSendIncidentCreatedNotification)
					{
						email = CreateIncidentCreatedNotification();
					}

					if (email == null && ShouldSendIncidentCriticalityChangedNotification)
					{
						email = CreateIncidentCriticalityChangedNotification();
					}

					if (email == null && ShouldSendIncidentResolvedNotification)
					{
						(email, LastEmailAllowedToBeCreated) = SupportIncidentEmailQuickBuilder.CreateIncidentResolvedEmail(Incident, HasEConversation, Incident.IM_ResolutionCode, false);
					}
				}

				if (!contact.OC_Email.IsEmpty && ShouldSendWorkItemCreatedNotification)
				{
					email = CreateWorkItemCreatedNotification();
					// Note, user does not get to see and edit this one
				}

				if (email != null)
				{
					email.UserCanEdit = true;
				}
			}

			if (email == null)
			{
				email = base.CreateStandardEmailIfApplicable();
			}

			return email;
		}

		#region Closed

		bool ShouldSendNoClientResponseEmail
		{
			get
			{
				return !Incident.IsCreatedFromIssue
					&& (Incident.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient)
					&& !Incident.EConversation.AnyLocalMessageContains("Support Incident " + Incident.IncidentCloseTypeText + " email sent to contact");
			}
		}

		bool ShouldSendIncidentResolvedNotification
		{
			get
			{
				if (Incident.IsRevertingToClosedOrResolved)
				{
					return false;
				}

				if (Incident.IsCreatedFromIssue)
				{
					return false;
				}

				return Incident.IM_Status == SupportIncidentLookups.Status.Closed && Incident.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.Resolved && Incident.IsUpdatingDisposition;
			}
		}

		bool ShouldSendIncidentClosedNotification
		{
			get
			{
				if (Incident.IsRevertingToClosedOrResolved || Incident.IsCreatedFromIssue)
				{
					return false;
				}

				var resolutionCode = Incident.IM_ResolutionCode;

				if (resolutionCode.EqualsIgnoringCase(SupportIncidentLookups.DispositionList.Constants.Closed.Resolved))
				{
					return false;
				}

				if (Incident.IsClosedDisposition(resolutionCode))
				{
					if (!Incident.IsUpdatingDisposition)
					{
						return false;
					}
					else
					{
						return true;
					}
				}

				if (Incident.ClosingAsIsAwaitingClient
					|| resolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelayed
					|| resolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade)
				{
					return false;
				}

				bool isClosedInContentDevelopmentAndNeedNotification =
					HasClosedAsContentDevelopment
					&& resolutionCode != SupportIncidentLookups.DispositionList.Constants.Closed.NoSupportContract
					&& resolutionCode != SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse;

				bool isClosedInDefectAndNeedNotification =
					HasClosedAsDefect
					&& resolutionCode != SupportIncidentLookups.DispositionList.Constants.Closed.NoSupportContract
					&& resolutionCode != SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse;

				bool isClosedInSupportAndNeedNotification =
					HasClosedAsSupport
					&& resolutionCode != SupportIncidentLookups.DispositionList.Constants.Closed.NoSupportContract
					&& resolutionCode != SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse;

				bool isClosedInFeatureRequestAndNeedEmailNotification =
					HasClosedAsFeatureRequest
					&& resolutionCode != SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided
					&& resolutionCode != SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided
					&& resolutionCode != SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse;

				bool isClosedInCustomerServiceRequestAndNeedNotification =
					HasClosedAsCustomerServiceRequest
					&& resolutionCode != SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse;

				bool isClosedAsComplianceAndNeedNotification =
					HasClosedAsCompliance
					&& resolutionCode != SupportIncidentLookups.DispositionList.Constants.Closed.NoSupportContract
					&& resolutionCode != SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse;

				return isClosedInSupportAndNeedNotification
					|| isClosedInFeatureRequestAndNeedEmailNotification
					|| isClosedInCustomerServiceRequestAndNeedNotification
					|| isClosedAsComplianceAndNeedNotification
					|| isClosedInContentDevelopmentAndNeedNotification
					|| isClosedInDefectAndNeedNotification;
			}
		}

		internal bool HasClosedAsContentDevelopment
		{
			get
			{
				return
					(Incident.IM_Category == SupportIncidentCategoriesList.Codes.ContentDevelopment)
					&& (Incident.OriginalSnapShot.IM_Status != Incident.IM_Status)
					&& (Incident.IM_Status == SupportIncidentLookups.Status.Closed);
			}
		}

		internal bool HasClosedAsDefect
		{
			get
			{
				return
					(Incident.IM_Category == SupportIncidentCategoriesList.Codes.Defect)
					&& (Incident.OriginalSnapShot.IM_Status != Incident.IM_Status)
					&& (Incident.IM_Status == SupportIncidentLookups.Status.Closed);
			}
		}

		internal bool HasClosedAsCompliance
		{
			get
			{
				return
					(Incident.IM_Category == SupportIncidentCategoriesList.Codes.ComplianceRequirement)
					&& (Incident.OriginalSnapShot.IM_Status != Incident.IM_Status)
					&& (Incident.IM_Status == SupportIncidentLookups.Status.Closed);
			}
		}

		internal bool HasClosedAsSupport
		{
			get
			{
				return
					(Incident.IM_Category == SupportIncidentCategoriesList.Codes.Support)
					&& (Incident.OriginalSnapShot.IM_Status != Incident.IM_Status)
					&& (Incident.IM_Status == SupportIncidentLookups.Status.Closed);
			}
		}

		bool HasClosedAsFeatureRequest
		{
			get
			{
				return
					(Incident.IM_Category == SupportIncidentCategoriesList.Codes.FeatureRequest)
					&& (Incident.OriginalSnapShot.IM_Status != Incident.IM_Status)
					&& (Incident.IM_Status == SupportIncidentLookups.Status.Closed);
			}
		}

		bool HasClosedAsCustomerServiceRequest
		{
			get
			{
				return
					(Incident.IM_Category == SupportIncidentCategoriesList.Codes.CustomerServiceRequest)
					&& (Incident.OriginalSnapShot.IM_Status != Incident.IM_Status)
					&& (Incident.IM_Status == SupportIncidentLookups.Status.Closed);
			}
		}

		#endregion

		#region Criticality Changed

		bool ShouldSendIncidentCriticalityChangedNotification
		{
			get
			{
				return !(Incident.PreviouslySavedCriticality).IsEmpty
					&& !Incident.Criticality.IsEmpty
					&& Incident.Criticality != Incident.PreviouslySavedCriticality;
			}
		}

		SupportIncidentEmail CreateIncidentCriticalityChangedNotification()
		{
			var emailContents = new SupportIncidentCriticalityChangedEmailContentBuilder(Incident, HasEConversation, Incident.IM_Product);
			var email = CreateNewEmail(emailContents, (container) =>
			{
				if (container.Email != null)
				{
					container.Email.XAutoResponseSuppressHeaderValue = SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll;
					container.Email.ShouldAppendNewCustomerMessages = false;
					container.Email.PublicLogComment = "Notification email sent to contact";
				}
			});
			return email;
		}

		#endregion

		#region Created

		bool ShouldSendIncidentCreatedNotification
		{
			get
			{
				return !Incident.IsCreatedFromIssue
					&& Incident.IsNewIncident
					&& Incident.IM_ClientIncidentReference.IsEmpty
					&& !Incident.ShouldStayIndependent;
			}
		}

		SupportIncidentEmail CreateIncidentCreatedNotification()
		{
			var emailContents = new CustomerServiceNotificationEmailContentBuilder(Incident, HasEConversation, Incident.IM_Product);
			var email = CreateNewEmail(emailContents, (container) =>
			{
				if (container.Email != null)
				{
					container.Email.XAutoResponseSuppressHeaderValue = SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll;
					container.Email.PublicLogComment = "Notification email sent to contact";
				}
			});

			return email;
		}

		#endregion

		#region Work Item Created

		bool ShouldSendWorkItemCreatedNotification
		{
			get
			{
				return Incident.IM_Category != SupportIncidentCategoriesList.Codes.Support
					&& HasDispositionChangedTo(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated)
					&& !Incident.IsCreatedFromIssue && !Incident.ShouldStayIndependent;
			}
		}

		SupportIncidentEmail CreateWorkItemCreatedNotification()
		{
			var emailContents = new SupportIncidentWorkItemCreatedEmailContentBuilder(Incident, HasEConversation, Incident.IM_Product, Incident.IM_Category);
			var email = CreateNewEmail(emailContents, (container) =>
			{
				if (container.Email != null)
				{
					container.Email.XAutoResponseSuppressHeaderValue = SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll;
					container.Email.PublicLogComment = (Incident.IM_Category == SupportIncidentCategoriesList.Codes.ContentDevelopment)
						? "Content Development Work Scheduled email sent to contact"
						: "Development Work Scheduled email sent to contact";
				}
			});

			return email;
		}

		#endregion
	}
}
