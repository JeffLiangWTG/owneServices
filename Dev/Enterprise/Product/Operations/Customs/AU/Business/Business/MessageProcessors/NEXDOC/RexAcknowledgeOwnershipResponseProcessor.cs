using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.AU.Declaration.Business.RexOwnershipSoap;
using Enterprise.Edifact;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RexAcknowledgeOwnershipResponseProcessor : NEXDOCMessageProcessor<RexAcknowledgeOwnershipResponse>
	{
		public RexAcknowledgeOwnershipResponseProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override void ProcessMessageInternal(EDIMessage message, RexAcknowledgeOwnershipResponse acknowledgeOwnershipResponse)
		{
			var rexNumber = GetRexNumberFromInterchangeHeader(message);
			var notification = FindNotificationByRexNumber(message.Factory, rexNumber, acknowledgeOwnershipResponse.outcome);
			if (notification != null)
			{
				message.EM_LinkedObject = notification;

				if (acknowledgeOwnershipResponse.outcome == RexOwnershipOutcomeType.CLOSED_ACCEPTED)
				{
					notification.QN_AcknowledgeStatus = NEXDOCAcknowledgeStatus.Codes.Accepted;
					notification.QN_MessageStatus = NEXDOCMessageStatus.Codes.ClosedAccepted;
				}
				else if (acknowledgeOwnershipResponse.outcome == RexOwnershipOutcomeType.CLOSED_REJECTED)
				{
					notification.QN_AcknowledgeStatus = NEXDOCAcknowledgeStatus.Codes.Rejected;
					notification.QN_MessageStatus = NEXDOCMessageStatus.Codes.ClosedRejected;
				}

				var detailsBuilder = new ZStringBuilder();
				detailsBuilder.Append("REX Number: " + notification.QN_RexNumber);
				detailsBuilder.Append("Exporter Reference: " + notification.QN_ExporterReference);
				detailsBuilder.AppendLine("Message Status: " + acknowledgeOwnershipResponse.outcome);
				var details = detailsBuilder.ToStringWithDelimiterBetweenAppends("<br />");

				var email = CreateEmail("NEXDOC Notification Advice", "A NEXDOC Notification has been received.", details);
				SendAcknowledgementReport(null, email);
			}
			else
			{
				throw new InvalidFormatException("Unexpected or Invalid Rex Number '" + rexNumber + "'. Cannot Process.");
			}
		}

		QuarantineNexDocNotification FindNotificationByRexNumber(BusinessObjectFactory factory, ZString rexNumber, RexOwnershipOutcomeType outcome)
		{
			QuarantineNexDocNotification result = null;

			if (!rexNumber.IsEmpty)
			{
				var query = new ZQuery(QuarantineNexDocNotificationSchema.QN_RexNumber, rexNumber);

				var prerequisiteStatus = GetPrerequisiteAcknowledgeStatus(outcome);
				if (!prerequisiteStatus.IsEmpty)
				{
					query.AddToFilter(QuarantineNexDocNotificationSchema.QN_AcknowledgeStatus, prerequisiteStatus);
				}

				result = factory.LoadTop1<QuarantineNexDocNotification>(query);
			}

			return result;
		}

		ZString GetPrerequisiteAcknowledgeStatus(RexOwnershipOutcomeType outcome)
		{
			var result = ZString.Empty;

			if (outcome == RexOwnershipOutcomeType.CLOSED_ACCEPTED)
			{
				result = NEXDOCAcknowledgeStatus.Codes.PendingAccept;
			}
			else if (outcome == RexOwnershipOutcomeType.CLOSED_REJECTED)
			{
				result = NEXDOCAcknowledgeStatus.Codes.PendingReject;
			}

			return result;
		}
	}
}
