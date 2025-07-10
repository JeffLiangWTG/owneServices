using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.AU.Declaration.Business.RexOwnershipSoap;
using Enterprise.Edifact;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class TransferForwardBaseProcessor<T> : NEXDOCMessageProcessor<T>
		where T : class
	{
		protected TransferForwardBaseProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected abstract ZString EmailTitle { get; }

		protected abstract RexOwnershipOutcomeType GetOutcomeTypeFromResponse(T ownershipResponse);

		protected override void ProcessMessageInternal(EDIMessage message, T ownershipResponse)
		{
			var rexNumber = GetRexNumberFromInterchangeHeader(message);
			var invoice = FindInvoiceByRexNumber(message.Factory, rexNumber);
			if (invoice != null)
			{
				message.EM_LinkedObject = invoice.QuarantineExDocHeader;

				var outcome = GetOutcomeTypeFromResponse(ownershipResponse);
				invoice.QuarantineExDocHeader.RequestForPermitStatus = GetNEXDOCMessageStatusCode(outcome);

				var detailsBuilder = new ZStringBuilder();
				detailsBuilder.Append("REX Number: " + rexNumber);
				detailsBuilder.AppendLine("Exporter Reference: " + invoice.JZ_ExporterReference);
				detailsBuilder.AppendLine("Outcome: " + outcome);
				var details = detailsBuilder.ToStringWithDelimiterBetweenAppends("<br />");

				var email = CreateEmail("NEXDOC Notification Advice", "A " + EmailTitle + " for REX " + rexNumber + " has been received", details);
				SendAcknowledgementReport(null, email);
			}
			else
			{
				throw new InvalidFormatException($"Unexpected or Invalid Rex Number '" + rexNumber + "'. Cannot Process.");
			}
		}

		protected ZString GetNEXDOCMessageStatusCode(RexOwnershipOutcomeType outcome)
		{
			switch (outcome)
			{
				case RexOwnershipOutcomeType.CLOSED_ACCEPTED:
					return NEXDOCMessageStatus.Codes.ClosedAccepted;
				case RexOwnershipOutcomeType.CLOSED_REJECTED:
					return NEXDOCMessageStatus.Codes.ClosedRejected;
				case RexOwnershipOutcomeType.CLOSED_WITHDRAWN:
					return NEXDOCMessageStatus.Codes.ClosedWithdrawn;
				case RexOwnershipOutcomeType.FORWARD_ON_HOLD:
					return NEXDOCMessageStatus.Codes.ForwardOnHold;
				case RexOwnershipOutcomeType.OPEN_PENDING:
					return NEXDOCMessageStatus.Codes.OpenPending;
			}

			return ZString.Empty;
		}
	}
}
