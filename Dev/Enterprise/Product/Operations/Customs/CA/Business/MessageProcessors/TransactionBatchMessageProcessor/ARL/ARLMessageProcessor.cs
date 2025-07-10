using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	abstract class ARLMessageProcessor : ITransactionBatchMessageProcessor
	{
		protected ARLMessageProcessor(Enterprise.Messaging.Business.EDIMessage message, IXmlSessionTracker logger, ZString messageType)
		{
			this.message = Argument.NotNull(message, "message");
			this.logger = Argument.NotNull(logger, "logger");
			this.factory = message.Factory;
			this.messageType = messageType;

			LinkedObjectManager = new ImportLinkedObjectManager(message);
		}

		protected readonly ZString messageType;
		protected readonly Enterprise.Messaging.Business.EDIMessage message;
		protected readonly IXmlSessionTracker logger;
		protected readonly BusinessObjectFactory factory;
		protected ImportLinkedObjectManager LinkedObjectManager { get; private set; }

		const string ReprocessOwner = "CASOAReprocess";
		protected const int ImporterBusinessNumberMaxLength = 15;

		public bool Process()
		{
			return InvokeWithEnvironmentDeclared(() =>
			{
				var isInReprocessMode = message.EM_MessageOwner == ReprocessOwner;

				var result = ProcessCore();

				if (!isInReprocessMode)
				{
					SendAcknowledgementReport(GetEmailAndSetOnMessage());
				}

				return result;
			});
		}

		protected void SetValue(BusinessObject obj, SchemaStringColumn schemaColumn, ZString value)
		{
			StatementMessageProcessorHelper.SetValue(obj, schemaColumn, value);
		}

		T InvokeWithEnvironmentDeclared<T>(Func<T> func)
		{
			var currentBranch = message.Branch;
			if (currentBranch != null && !currentBranch.GB_IsActive)
			{
				currentBranch = currentBranch.Company.FirstActiveBranch;
			}

			using (DisposableEnvironment.ForBranch(currentBranch.PK.ToGuid()))
			{
				return func();
			}
		}

		public IKeysResult GetKeysForBlockingParallelImport()
		{
			return InvokeWithEnvironmentDeclared(GetKeysCore);
		}

		protected abstract IKeysResult GetKeysCore();

		protected abstract bool ProcessCore();

		EmailDef GetEmailAndSetOnMessage()
		{
			var factoryForEmail = new BusinessObjectFactory();
			var arlMessage = factoryForEmail.Load<ARLMessage>(message.PK);
			var emailBuilder = new EmailDefBuilder(GetSubject(), message.EM_FormattedMessageText, EmailDefBuilder.HtmlTemplates.FreeFormResponse);
			emailBuilder.AddArgReplacement(EmailDefBuilder.GetJobLink(arlMessage, new ARLMessageTypes().GetDescriptionFromCode(messageType)));
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.FromMessageSender, " " + MessageSender.TrimEnd());
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, GetMessageInterpretation(factoryForEmail));
			message.EM_MessageInterpretation = emailBuilder.ToString();
			return emailBuilder.ToEmail();
		}

		protected abstract string GetSubject();

		protected abstract string GetMessageInterpretation(BusinessObjectFactory factoryForEmail);

		protected static string MessageSender
		{
			get { return StatementMessageProcessorHelper.MessageSender; }
		}

		#region SendReport

		protected void SendAcknowledgementReport(EmailDef email)
		{
			SendReport(email, AcknowledgementEmailMode, AcknowledgementEmailGroup);
		}

		protected virtual ZGuid AcknowledgementEmailGroup
		{
			get { return CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgementsToGroup.Value; }
		}

		protected virtual ZString AcknowledgementEmailMode
		{
			get { return CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgements.Value; }
		}

		protected void SendReport(EmailDef email, ZString emailMode, ZGuid emailGroup)
		{
			if (emailMode == Core.Constants.EmailTo.NominatedGroup || emailMode == Core.Constants.EmailTo.StaffMemberAndNominatedGroup)
			{
				CopyGroupToEmails(email, emailGroup);
			}
			SendReport(email);
		}

		protected virtual void SendReport(EmailDef email)
		{
			if (email.Recipients.Count > 0 || email.CCRecipients.Count > 0 || email.BCCRecipients.Count > 0)
			{
				try
				{
					if (factory != null)
					{
						Env.OutgoingMailManager.Create(factory, email);
					}
					else // Just in case there's some error handling trying to report something when the factory has not been set.
					{
						Env.OutgoingMailManager.CreateAndSave(email);
					}
				}
				catch (EmailSendFailedException e)
				{
					logger.LogBoth(Integration.LogType.Error, "Couldn't send email: " + e.Message + ".  Here are the contents of the email that couln't be sent:\r\n\r\n" +
						"SUBJECT: " + email.Subject + "\r\n" +
						"BODY: " + email.Body + "\r\n");
				}
			}
		}

		protected void CopyGroupToEmails(EmailDef email, ZGuid groupToCopy)
		{
			if (!groupToCopy.IsEmpty)
			{
				var group = factory.Load<GlbGroup>(groupToCopy);

				if (group != null)
				{
					var emailGroupUtility = new EmailGroupUtility();

					foreach (GlbStaff staff in group.Staff)
					{
						if (!staff.GS_EmailAddress.IsEmpty && !emailGroupUtility.IsHostNotificationEmail(staff.GS_EmailAddress))
						{
							AddRecipientCore(email, staff.GS_EmailAddress, RecipientDef.RecipientTypes.CC);
						}
					}
				}
			}
		}

		protected virtual void AddRecipientCore(EmailDef emailDef, string email, RecipientDef.RecipientTypes type)
		{
			emailDef.AddRecipientForUserCommunication(email, type);
		}

		#endregion
	}
}

#if DEBUG
namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	using System.Collections.Generic;

	class ARLMessageProcessorForTesting : ARLMessageProcessor
	{
		public ARLMessageProcessorForTesting(Enterprise.Messaging.Business.EDIMessage message, IXmlSessionTracker logger, ZString messageType) : base(message, logger, messageType)
		{
		}

		protected override string GetMessageInterpretation(BusinessObjectFactory factoryForEmail)
		{
			return "For Testing";
		}

		protected override string GetSubject()
		{
			return "For Testing";
		}

		protected override bool ProcessCore()
		{
			logger.LogBoth(Integration.LogType.Information, GlbBranch.CurrentBranch.GB_Code);
			return true;
		}

		protected override IKeysResult GetKeysCore()
		{
			return new EmptyKeysResult();
		}

		class EmptyKeysResult : IKeysResult
		{
			public IEnumerable<(string KeyValue, string KeySource)> KeysInfo => Enumerable.Empty<(string KeyValue, string KeySource)>();
			public IEnumerable<string> Keys => KeysInfo.Select(k => k.KeyValue);

			public bool IsMatch => true;
		}
	}
}

#endif
