using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business.Interfaces;
using Enterprise.Customs.GB.CDS.Helpers;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using static System.FormattableString;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSSynchronousResponseMessageProcessor : CDSMessageProcessor<CDSSynchronousResponseEDIMessage>
	{
		public CDSSynchronousResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString ProcessMessageCore(CDSSynchronousResponseEDIMessage cdsEDIMessage, BusinessObjectFactory factory)
		{
			var messageStatus = EDIMessageStatusList.Codes.Discarded;
			var originalOutgoingMessage = cdsEDIMessage.OriginalOutgoingMessage;
			if (originalOutgoingMessage != null)
			{
				var messageAttachee = originalOutgoingMessage.LinkedMessageAttachee;
				if (messageAttachee != null)
				{
					using (DisposableEnvironment.ForBranch(messageAttachee.Branch.PK.ToGuid()))
					{
						AttachMessageToMessageAttachee(messageAttachee, cdsEDIMessage);
						MakeInterpretationPrettier(cdsEDIMessage, originalOutgoingMessage);
						UpdateOriginalMessage(originalOutgoingMessage, cdsEDIMessage);
					}
					messageStatus = EDIMessageStatusList.Codes.ProcessedOK;
				}
			}

			return messageStatus;
		}

		static void UpdateOriginalMessage(CDSEDIMessage originalOutgoingMessage, CDSSynchronousResponseEDIMessage cdsEDIMessage)
		{
			originalOutgoingMessage.EM_Status = cdsEDIMessage.MessageDataObject.IsAccepted
				? EDIMessage.Status.Acknowledged
				: EDIMessage.Status.Rejected;
			originalOutgoingMessage.EM_ApplicationReference = cdsEDIMessage.EM_ApplicationReference;
		}

		void MakeInterpretationPrettier(CDSSynchronousResponseEDIMessage cdsEDIMessage, CDSEDIMessage originalOutgoingMessage)
		{
			if (cdsEDIMessage.MessageDataObject.IsAccepted)
			{
				var ccsukProcessingInstructionHelper = new CCSUKProcessingInstructionHelper(cdsEDIMessage?.Interchange?.EI_FooterText ?? ZString.Empty);
				ZGuid.TryParse(ccsukProcessingInstructionHelper.ExtCorrelationId, out var trackingID);
				cdsEDIMessage.EM_MessageInterpretation = CDSEDIMessagePrettier.GetCIDMessage(null, trackingID, originalOutgoingMessage, cdsEDIMessage, null, ccsukRecipient, new ZDateTimeOffset(cdsEDIMessage.EM_SystemCreateTimeUtc, System.DateTimeKind.Utc));
			}
			else
			{
				cdsEDIMessage.EM_MessageInterpretation = GetErrorInterpretation(cdsEDIMessage, originalOutgoingMessage);
			}
		}

		static ZString GetErrorInterpretation(CDSSynchronousResponseEDIMessage cdsEDIMessage, CDSEDIMessage originalOutgoingMessage)
		{
			var originalMessageNum = originalOutgoingMessage.EM_MessageNum;
			var messageObject = cdsEDIMessage.MessageDataObject;
			var status = messageObject.Status;
			var code = messageObject.Code;
			var description = messageObject.Description;
			var conversationId = cdsEDIMessage.EM_ApplicationReference;

			var result = Invariant($"Message {originalMessageNum} was '{status}' - '{code}'");
			if (!description.IsEmpty)
			{
				result += Invariant($" - '{description}'");
			}
			if (!((string)conversationId).All(x => x == '0'))
			{
				result += Invariant($" and received Conversation ID '{conversationId}'");
			}

			try
			{
				var helper = new ErrorsHelper(cdsEDIMessage.EM_MessageText, "//SynchronousResponse");
				if (helper.ErrorResponses.Count > 0)
				{
					result += "<h3>The following errors were returned</h3>";
					foreach (var error in helper.ErrorResponses)
					{
						result += Invariant($"<li>{error.DisplayError}</li>");
					}
				}
				else if (description.IsEmpty)
				{
					result += "<br /><small>This is likely caused by a rejection by CDS due to an XML schema failure, i.e. bad data. CCS-UK do not send back details of such failures, they only report that there "
						+ "was a failure, therefore details of the failures cannot be shown here. Retrying the same request via another CSP or directly to CDS (for imports, don't forget to temporarily remove the "
						+ "inventory consignment reference first) is likely to reveal much more detail about the nature of the failure, allowing you to resolve it (and restore the original CSP and inventory "
						+ "reference). Common data-entry errors include: missing unit code for supporting document quantities, missing currency code, missing measurement code for taxes, and new line or other "
						+ "non-standard characters in text fields. Examine the original outgoing XML text to find such cases and remedy it.</small>";
				}
			}
			catch (System.Exception ex)
			{
				result += "<br/>An error was encountered when processing the error details, please see the message text for more details.";
				ErrorReporter.ReportDeveloperExceptionOnce("ErrorsHelper threw whilst trying to process the message: " + cdsEDIMessage.EM_MessageText, ex);
			}

			return result;
		}

		static void AttachMessageToMessageAttachee(IMessageAttachee messageAttachee, CDSSynchronousResponseEDIMessage cdsEDIMessage)
		{
			cdsEDIMessage.EM_LinkedObject = (BusinessObject)messageAttachee;
		}

		protected override string MessageFriendlyNameCore => "CDS Synchronous Response Message";

		protected override ZString MessageType => CDSEDIMessageTypeList.Codes.SynchronousResponse;

		const string ccsukRecipient = "CCSUK";
	}
}
