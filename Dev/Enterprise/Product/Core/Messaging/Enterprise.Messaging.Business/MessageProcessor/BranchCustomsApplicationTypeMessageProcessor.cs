using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Messaging.MessageProcessors
{
	public abstract class BranchCustomsApplicationTypeMessageProcessor : ApplicationTypeMessageProcessor
	{
		protected BranchCustomsApplicationTypeMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override bool RequiresPreProcessingCore => true;

		protected sealed override ZQuery MessageFilterCore => new ZQuery();

		[SuppressMessage("Microsoft.Design", "CA1021: AvoidOutParameters")]
		protected bool GenerateHtmlEmail(string subject, string header, string description, string bodyDetails, string footerDetails, out EmailDef email, IGlbBranch branchForEmailLogo)
		{
			return new HtmlResponseEmailGenerator().TryGenerateEmail(subject, header, description, bodyDetails, footerDetails, out email, branchForEmailLogo);
		}

		protected void GenerateHtmlEmailAndSendToOriginalOrGroup(BusinessObjectFactory factory, IRelatedJob relatedJob, string messageTypeInSubject, string body, bool isFailure, IGlbBranch branchForEmailLogo, BusinessObject sourceBusinessObject, Func<string> getEmailAddressToSendTo)
		{
			string uri = "";
			string jobNumber = "";
			if (relatedJob != null)
			{
				uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(relatedJob);
				jobNumber = relatedJob.JobNumber;
			}
			GenerateHtmlEmailAndSendToOriginalOrGroup(factory, uri, jobNumber, messageTypeInSubject, body, isFailure, branchForEmailLogo, sourceBusinessObject, getEmailAddressToSendTo);
		}

		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "We are using URL as string on the rest of the implementation.")]
		protected void GenerateHtmlEmailAndSendToOriginalOrGroup(BusinessObjectFactory factory, string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure, IGlbBranch branchForEmailLogo, BusinessObject sourceBusinessObject, Func<string> getEmailAddressToSendTo)
		{
			SetupEmailAndSendToOriginalOrGroup(factory, GenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, branchForEmailLogo), branchForEmailLogo, sourceBusinessObject, getEmailAddressToSendTo);
		}

		protected virtual EmailDef GenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure, IGlbBranch branchForEmailLogo)
		{
			new HtmlResponseEmailGenerator().TryGenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, out var email, branchForEmailLogo);
			return email;
		}

		protected void SetupEmailAndSendToOriginalOrGroup(BusinessObjectFactory factory, EmailDef email, IGlbBranch branchForEmailLogo, BusinessObject sourceBusinessObject, Func<string> getEmailAddressToSendTo)
		{
			email.SetupBusinessEntityInfo(sourceBusinessObject);
			SendEmailToOriginalSenderOrGroupIfSenderInvalid(factory, email, branchForEmailLogo, getEmailAddressToSendTo());
		}

		protected void SendEmailToOriginalSenderOrGroupIfSenderInvalid(BusinessObjectFactory factory, EmailDef email, IGlbBranch branch, ZString emailAddressToSendTo)
		{
			if (email != null)
			{
				if (branch == null)
				{
					branch = GlbBranch.CurrentBranch;
				}

				var emailGroupRegistryItem = GetEmailGroupRegistryItem();
				var alternativeEmailGroupRegistryItem = GetAlternativeEmailGroupRegistryItemWhenNoRecipientFound();
				var emailRepCal = new EmailRecipientCalculator(
					GetEmailSendMode(branch), GetEmailGroupPK(emailGroupRegistryItem, branch),
					emailAddressToSendTo, GetEmailGroupPK(alternativeEmailGroupRegistryItem, branch));
				emailRepCal.SendNotifications(factory, email, !emailRepCal.EmailRedirected ? emailGroupRegistryItem : alternativeEmailGroupRegistryItem);
			}
		}

		protected virtual ZGuid GetEmailGroupPK(IRegistryItem registryItem, IGlbBranch branch)
		{
			var result = ZGuid.Empty;
			if (registryItem != null)
			{
				var emailGroup = registryItem.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty);
				if (emailGroup is GroupNotification groupNotification)
				{
					result = groupNotification.SendGroupPK;
				}
				else if (emailGroup is Guid)
				{
					result = (Guid)emailGroup;
				}
			}
			return result;
		}

		protected virtual IRegistryItem GetEmailGroupRegistryItem() => null;

		protected virtual ZString GetEmailSendMode(IGlbBranch branch)
		{
			var result = GroupNotification.StaffMemberOrNominatedGroup;
			if (branch != null)
			{
				var groupNotificationRegistry = GetEmailGroupRegistryItem()?.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty) as GroupNotification;
				if (groupNotificationRegistry != null)
				{
					result = groupNotificationRegistry.SendMode;
				}
			}
			return result;
		}

		protected virtual IRegistryItem GetAlternativeEmailGroupRegistryItemWhenNoRecipientFound() => null;

		protected ZString GetEmailAddressToSendToFromQueuedUser(EDIMessage originalMessage)
		{
			ZString result = ZString.Empty;
			var originalSender = originalMessage != null ? originalMessage.UserWhoQueuedThisRecord : null;
			if (originalSender != null)
			{
				result = originalSender.GS_EmailAddress;
			}
			return result;
		}
	}
}
