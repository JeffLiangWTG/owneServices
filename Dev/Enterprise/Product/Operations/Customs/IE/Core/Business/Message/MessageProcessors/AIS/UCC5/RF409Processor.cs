using System;
using System.Linq;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using RF409Provider = Enterprise.Customs.IE.Messaging.UCC5.RF409Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class RF409Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, RF409Provider>
	{
		public RF409Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		public override string GetEntryStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, RF409Provider provider)
			=> provider.RefundApplicationAccepted ? AISEntryStatusList.Codes.RefundApplicationAccepted : AISEntryStatusList.Codes.RefundApplicationRejected;

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, RF409Provider provider) => LogicalStatusList.Codes.Accepted;

		protected override string MessageFriendlyNameCore => CommonResStrings.RF409MessageFriendlyName;

		protected override Type MessageInterpreterType => typeof(RF409MessageInterpreter);

		protected override bool NeedToSendEmailNotification(EDIMessage message) => true;

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, RF409Provider provider)
		{
			SetRefundDutiesToBeRepaidIfValid(messageAttachee, provider);
		}

		void SetRefundDutiesToBeRepaidIfValid(IMessageAttachee messageAttachee, RF409Provider provider)
		{
			if (provider.RefundApplicationAccepted)
			{
				var amountToRemit = provider.AmountToBeRepaid;
				if (!amountToRemit.IsEmpty && messageAttachee is CusEntryHeader entryHeader)
				{
					var entryLineWithRefunds = entryHeader.MergedLines.Single(entryLine => entryLine.RefundDuties.Count > 0);
					var mostCloseDuty = entryLineWithRefunds.RefundDuties.Cast<RefundDuty>()
						.Where(fee => !fee.TaxAmountDifferenceForRefunds.IsEmpty && fee.TaxAmountOfDutyToBeRepaid.IsEmpty)
						.OrderBy(refund => Math.Abs(refund.TaxAmountDifferenceForRefunds - amountToRemit))
						.FirstOrDefault();
					if (mostCloseDuty != null)
					{
						mostCloseDuty.TaxAmountOfDutyToBeRepaid = amountToRemit;
					}
				}
			}
		}
	}
}
