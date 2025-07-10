using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Edifact;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class REFREJMessageProcessor : BaseImportDeclarationMessageProcessor
	{
		public REFREJMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.REFREJ, "Refund Rejected Response (REFREJ)")
		{
		}

		protected override bool ShouldSendAcknowledgementReport
		{
			get { return AcknowledgementEmailMode != Core.Constants.EmailTo.NoEmails; }
		}

		#region E-mail Registry Settings

		protected override ZGuid AcknowledgementEmailGroup
		{
			get { return AUCustomsDataRegistry.Instance.ThirdPartyRefundRejectionSendAcknowledgementsToGroup.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty); }
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { return AUCustomsDataRegistry.Instance.ThirdPartyRefundRejectionSendAcknowledgements.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty); }
		}

		#endregion

		protected override void ProcessUnmatchedResponseMessage()
		{
			var message = incomingMessage;
			if (ShouldSendAcknowledgementReport && AcknowledgementEmailGroup.IsValid)
			{
				cUSRES = (CUSRESMessage)message.GetAutoEdifactMessageUsingNamedFactory(AuEdifactMessageFactory.AUCMessageFactory, new UNOCCMRCharacterSet());
				statusType = incomingMessage.GetStatus();
				SetMessageSubType();

				var responseEmail = ConstructUnmatchedResponseEmail();
				responseEmail.ContentType = EmailContentTypes.HTML;
				responseEmail.AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(AcknowledgementEmailGroup.ToGuid(), Env.Registry.RawRegistry.NotificationGroup);
				try
				{
					SendReport(responseEmail);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					ErrorReporter.ReportOnce("5F0BF99D-52D6-4D1B-8270-E438FB306760", "Trying to send email for unmatched REFREJ message from REFREJMessageProcessor", e);
				}
			}

			incomingMessage.EM_Status = Messaging.Business.EDIMessage.Status.Received;
		}

		#region Third Party Refund Rejection Notification

		protected EmailDef ConstructUnmatchedResponseEmail()
		{
			string subject = "Refund Rejected for Unknown Entry (" + ReferenceNumber(SendersReference) + ")";
			var emailBuilder = new EmailDefBuilder(subject, EmailDefBuilder.HtmlTemplates.FreeFormResponse);
			string report = string.Format(@"A response message has been received from Customs which does not match an operational customs declaration.
Shown below is a summary of relevant information received in the message.

Reference Number:		{0}
Declaration Reference:	{1}
Client Reference:		{2}
Document Reference:		{3}
Document Version:		{4}
Rejection Date:			{5}
Status:					REFUND REJECTED

Errors:
", ReferenceNumber(SendersReference), ReferenceNumber(BrokerReference), ClientReference, ReferenceNumber(CustomsDeclarationNumber), MessageVersion, RejectionDate);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, report.Replace("\r\n", "<br>"));

			var creator = new HtmlTableCreator(new[] { "" });
			foreach (FTXSegment ftx in cUSRES.FTX)
			{
				if (ftx.TextSubjectCodeQualifier == "ACD")
				{
					creator.WriteRow(string.IsNullOrEmpty(ftx.TextLiteral.FreeTextValue2) ? ftx.TextLiteral.FreeTextValue1 : ftx.TextLiteral.FreeTextValue1 + " " + ftx.TextLiteral.FreeTextValue2);
				}
			}

			emailBuilder.AddArgReplacement("Refund Rejection");
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml2, creator.ToHtml());
			return emailBuilder.ToEmail();
		}

		string ReferenceNumber(string refQualifier)
		{
			string result = "";
			foreach (SegmentGroup3 group3 in cUSRES.Group3)
			{
				if (group3.RFF[0].Reference.ReferenceFunctionCodeQualifier == refQualifier)
				{
					result = group3.RFF[0].Reference.ReferenceIdentifier;
					break;
				}
			}

			return result;
		}

		string ClientReference
		{
			get
			{
				string result = "";
				foreach (FTXSegment ftx in cUSRES.FTX)
				{
					if (ftx.TextSubjectCodeQualifier == "ACB")
					{
						result = ftx.TextLiteral.FreeTextValue1;
						break;
					}
				}

				return result;
			}
		}

		string MessageVersion
		{
			get
			{
				return cUSRES.BGM[0].DocumentMessageIdentification.Version;
			}
		}

		ZDateTime RejectionDate
		{
			get
			{
				ZDateTime result = ZDateTime.Today;
				DTMSegment dtm = cUSRES.DTM[0];
				if (dtm != null)
				{
					ZDateTime.TryParseExact(dtm.DateTimePeriod.DateTimePeriodValue, out result, "yyyyMMdd");
				}

				return result;
			}
		}

		const string CustomsDeclarationNumber = "ABT";
		const string SendersReference = "ABO";
		const string BrokerReference = "ADU";

		#endregion
	}
}
