using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public abstract partial class AsycudaUniversalEventMessageProcessor
	{
		protected AsycudaUniversalEventMessageProcessor(IXmlSessionTracker logger, UniversalEvent universalEvent, AsycudaEDIMessage message, AsycudaManifestHeader manifestHeader)
		{
			this.logger = Argument.NotNull(logger, "logger");
			this.universalEvent = Argument.NotNull(universalEvent, "universalEvent");
			this.message = Argument.NotNull(message, "message");
			this.manifestHeader = Argument.NotNull(manifestHeader, nameof(manifestHeader));
			this.factory = message.Factory;
		}

		protected readonly IXmlSessionTracker logger;
		protected readonly UniversalEvent universalEvent;
		protected readonly EDIMessage message;
		protected readonly AsycudaManifestHeader manifestHeader;
		protected readonly BusinessObjectFactory factory;

		protected EmailGroupUtility EmailGroupUtility => factory.GetValue(ref emailGroupUtility, () => new EmailGroupUtility());

		CachedProperty<EmailGroupUtility> emailGroupUtility;

		public bool Process()
		{
			var needToSendEmail = SendToGroup();
			if (needToSendEmail)
			{
				var email = CreateEmail();

				AddStaffRecipients(email);
				SendEmail(email);
			}

			ProcessCore();

			return true;
		}

		protected virtual void ProcessCore()
		{
		}

		EmailDef CreateEmail()
		{
			var emailBuilder = new EmailDefBuilder(GetSubject(), EmailDefBuilder.HtmlTemplates.AsycudaManifestUniversalEventResponse);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, message.EM_MessageInterpretation);
			var email = emailBuilder.ToEmail();

			return email;
		}

		void AddStaffRecipients(EmailDef email)
		{
			var emailType = RecipientDef.RecipientTypes.TO;
			if (SendToStaff())
			{
				var staff = GetStaffToSend();
				if (staff != null
					&& !staff.GS_EmailAddress.IsEmpty
					&& !EmailGroupUtility.IsHostNotificationEmail(staff.GS_EmailAddress))
				{
					email.AddRecipientForUserCommunication(staff.GS_EmailAddress, emailType);
				}
			}
		}

		void SendEmail(EmailDef email)
		{
			try
			{
				var group = GetGroupToSend();
				if (group != null)
				{
					Env.OutgoingMailManager.Create(factory, email, group.PK.ToGuid(), GroupSourceLocator.GetFromGroup(group));
				}
				else
				{
					Env.OutgoingMailManager.CreateAndSaveToPostmasterGroup(email, factory);
				}
			}
			catch (EmailSendFailedException e)
			{
				logger.LogBoth(Integration.LogType.Error, "Couldn't send email: " + e.Message + ".  Here are the contents of the email that couldn't be sent:\r\n\r\n" +
					"SUBJECT: " + email.Subject + "\r\n" +
					"BODY: " + email.Body + "\r\n"); // Exception Message
			}
		}

		GlbStaff GetStaffToSend()
		{
			return manifestHeader.Factory.LoadTop1<GlbStaff>(
				new ZQuery(GlbStaffSchema.GS_Code, message.EM_SystemCreateUser)
			);
		}

		internal GlbGroup GetGroupToSend()
		{
			GlbGroup result = null;
			var groupGuid = GetGroupToSendCore();
			if (groupGuid != Guid.Empty)
			{
				result = factory.Load<GlbGroup>(groupGuid);
			}
			return result;
		}

		protected abstract ZGuid GetGroupToSendCore();

		protected abstract ZString GetSubject();

		protected abstract bool SendToGroup();

		protected abstract bool SendToStaff();
	}
}
