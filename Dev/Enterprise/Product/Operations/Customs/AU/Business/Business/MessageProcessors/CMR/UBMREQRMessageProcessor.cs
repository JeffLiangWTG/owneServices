using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class UBMREQRMessageProcessor : BaseUnderbondMessageProcessor
	{
		public UBMREQRMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.UBMREQR, "Underbond Movement Request Response - (UBMREQR)")
		{
		}

		protected override bool ShouldSendAcknowledgementReport
		{
			get { return true; }
		}

		protected override string MessageErrorCode
		{
			get
			{
				CMRUBMREQRMessage message = incomingMessage as CMRUBMREQRMessage;
				return message != null && message.IsExpectedArrival ? Enterprise.Messaging.Business.EDIMessage.Status.Discarded : base.MessageErrorCode;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		protected override void ProcessUnmatchedResponseMessage()
		{
			CMRUBMREQRMessage message = incomingMessage as CMRUBMREQRMessage;
			if (message == null)
			{
				throw new ArgumentException("Message must be a CMRUBMREQRMessage");
			}
			else if (message.IsExpectedArrival)
			{
				throw new MessageProcessorException(string.Format(@"An inbound expected arrival, or expected arrival rescind, notice has been detected, but could not be processed and so
has been discarded. These notices are processed by the CargoWise One CFS Module. You either are not using this Module or the
destination premise code ({0}) is unknown to CargoWise One.  For more information about the CargoWise One CFS Module please
contact the CargoWise support desk.", message.DestinationID), message, this, "Expected arrival notice discarded.", true);
			}
			else
			{
				base.ProcessUnmatchedResponseMessage();
			}
		}
	}
}
