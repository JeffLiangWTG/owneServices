using System;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Common
{
	public abstract class NACCSMessageProcessor
	{
		public NACCSMessageProcessor(LoggingInformation logger)
		{
			Logger = Argument.NotNull(logger, nameof(logger));
		}

		protected LoggingInformation Logger { get; }

		public void ProcessMessage(Messaging.Business.EDIMessage message)
		{
			if (message is EDIMessage naccsMessage)
			{
				ProcessMessageCore(naccsMessage);
			}
			else
			{
				throw new DeveloperNotificationException($"Wrong message type: {message.GetType().FullName} Message: {message.EM_MessageNum}");
			}
		}

		protected abstract void ProcessMessageCore(EDIMessage message);

		protected EDIMessage GetRequestMessage(BusinessObjectFactory factory, IJPInboundMessageHeader header)
		{
			if (!string.IsNullOrEmpty(header.MessageTag))
			{
				var query = new ZDBOnlyQuery(typeof(EDIMessage));
				query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.JPCustoms);
				query.AddToFilter(EDIMessageSchema.EM_MessageNum, header.MessageTag);
				query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);

				var subQueryOnCompany = new ZDBOnlySubQuery(typeof(GlbBranch), EDIMessageSchema.EM_GB);
				subQueryOnCompany.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
				query.AddSubQuery(subQueryOnCompany, JoinCondition.And);

				return factory.LoadTop1<EDIMessage>(query);
			}
			else
			{
				return null;
			}
		}

		protected void SendEmail(EDIMessage message, IJPInboundMessageHeader naccsHeader, ZString emailTo)
		{
			var subject = FormattableString.Invariant($"NACCS Response Messages - {message.EM_MessageNum}");
			var emailHeader = FormattableString.Invariant($@"Job Number: {naccsHeader.InputReference}");

			var emailBuilder = new EmailDefBuilder(subject, EmailDefBuilder.HtmlTemplates.FreeFormResponse);
			emailBuilder.AddArgReplacementRange(subject);

			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.HeaderSectionDetails, emailHeader);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.FromMessageSender, " from Japan Customs");

			var propertiesTable = new HtmlTableCreator(new string[] { "Property", "Value" });

			propertiesTable.WriteRow(nameof(IJPInboundMessageHeader.ProcedureCode), naccsHeader.ProcedureCode);
			propertiesTable.WriteRow(nameof(IJPInboundMessageHeader.OutputInformationCode), naccsHeader.OutputInformationCode);
			propertiesTable.WriteRow(nameof(IJPInboundMessageHeader.ReceivedDateTime), new ZDateTime(naccsHeader.ReceivedDateTime));
			propertiesTable.WriteRow(nameof(IJPInboundMessageHeader.Subject), naccsHeader.Subject);
			propertiesTable.WriteRow(nameof(IJPInboundMessageHeader.InputReference), naccsHeader.InputReference);

			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, propertiesTable.ToHtml());

			var emailDef = emailBuilder.ToEmail();
			emailDef.AddRecipientForUserCommunication(emailTo, RecipientDef.RecipientTypes.TO);

			try
			{
				Env.OutgoingMailManager.Create(message.Factory, emailDef);
			}
			catch (EmailSendFailedException e)
			{
				Logger.LogError(FormattableString.Invariant($@"Couldn't send email: {e.Message}. Here are the contents of the email that couldn't be sent:
SUBJECT: {emailDef.Subject}
BODY: {emailDef.Body}"));
			}
		}

		protected ZString GetEmailAddressToSendToFromQueuedUser(EDIMessage originalMessage)
		{
			return originalMessage?.UserWhoQueuedThisRecord?.GS_EmailAddress ?? string.Empty;
		}

		public ZString AdditionalWarning => additionalWarning;
		protected ZString additionalWarning;
	}
}
