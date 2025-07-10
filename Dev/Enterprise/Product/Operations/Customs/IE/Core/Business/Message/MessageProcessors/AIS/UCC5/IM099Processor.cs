using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using static Enterprise.Customs.IE.Business.Constants;
using IM099Provider = Enterprise.Customs.IE.Messaging.UCC5.IM099Provider;
using IM404Provider = Enterprise.Customs.IE.Messaging.UCC5.IM404Provider;
using IM429Provider = Enterprise.Customs.IE.Messaging.UCC5.IM429Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM099Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, IM099Provider>
	{
		public IM099Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => AISInterchangeTypeList.Descriptions.IM099;

		public override string GetEntryStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM099Provider provider)
		{
			if (provider != null)
			{
				if (provider.Remarks.EqualsIgnoringCase(RemarksTypeList.RefundApplicationRequired))
				{
					return AISEntryStatusList.Codes.RefundApplicationRequested;
				}
				else if (provider.Remarks.EqualsIgnoringCase(RemarksTypeList.InsufficientFund))
				{
					return AISEntryStatusList.Codes.InsufficientFund;
				}
			}
			return null;
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AISUCC5InboundEDIMessage message, IM099Provider provider)
		{
			OneTimeProcessingMessage = message;
			base.ProcessMessageCore(factory, message, provider);
		}

		readonly object processingMessageLock = new object();
		AISUCC5InboundEDIMessage processingMessageCache;
		AISUCC5InboundEDIMessage OneTimeProcessingMessage
		{
			get
			{
				lock (processingMessageLock)
				{
					var result = processingMessageCache;
					processingMessageCache = null;
					return result;
				}
			}
			set
			{
				lock (processingMessageLock)
				{
					processingMessageCache = value;
				}
			}
		}

		protected override string GetEmailNotification(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee) => CommonResStrings.GetIM099MessageInterpreterSummary(messageAttachee.RelatedJob.JobNumber);

		protected override string GetEmailSubject(AISUCC5InboundEDIMessage message) => AISInterchangeTypeList.Descriptions.IM099;

		protected override Type MessageInterpreterType => typeof(IM099MessageInterpreter);

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, IM099Provider provider)
		{
			AttachRefundsIfRefundApplicationRequired(messageAttachee, provider);
		}

		void AttachRefundsIfRefundApplicationRequired(IMessageAttachee messageAttachee, IM099Provider provider)
		{
			var processingMessage = OneTimeProcessingMessage;
			if (!provider.Remarks.EqualsIgnoringCase(RemarksTypeList.RefundApplicationRequired))
			{
				return;
			}
			var historyQuery = MessageAttacheeMessageHistoryHandler.GetMessageHistoryCreateTimeDescendingQuery(messageAttachee, processingMessage);
			var historyMessages = messageAttachee.Factory.Load<AISUCC5InboundEDIMessage>(historyQuery);

			if (historyMessages.FirstOrDefault(message => message.EM_MessageType == AISInterchangeTypeList.Codes.IM404)?.GetDataProvider() is IM404Provider iM404Provider
				&& historyMessages.FirstOrDefault(message => message.EM_MessageType == AISInterchangeTypeList.Codes.IM429)?.GetDataProvider() is IM429Provider iM429Provider
			)
			{
				var im429Dictionary = iM429Provider.GoodsItems.ToDictionary(
					x => x.DeclarationGoodsItemNumber,
					x => x.TaxTypes.ToDictionary(
						x => x.TaxType,
						x => new { x.Amount, x.TaxAmount }
				));

				var entryHeader = messageAttachee as CusEntryHeader;
				var entryLineDictionary = entryHeader.MergedLines.Cast<CusEntryLine>().ToDictionary(x => x.CL_LineNumber.ToString(), x => x);

				foreach (var im404Item in iM404Provider.GoodsShipment.Items)
				{
					if (entryLineDictionary.TryGetValue(im404Item.DeclarationGoodsItemNumber, out var entryLine))
					{
						var refundDuties = entryLine.RefundDuties;
						refundDuties.RemoveAndDeleteAll();

						foreach (var refund in im404Item.TaxTypes)
						{
							var confirmedRelease = im429Dictionary[im404Item.DeclarationGoodsItemNumber][refund.TaxType].TaxAmount;
							var confirmedAmendment = refund.TaxAmount;
							var differenceForRefunds = confirmedRelease - confirmedAmendment;

							if (differenceForRefunds > 0)
							{
								var newRefundDuty = refundDuties.AddNew(refund.TaxType);
								newRefundDuty.TaxAmountConfirmedRelease = confirmedRelease;
								newRefundDuty.TaxAmountConfirmedAmendment = confirmedAmendment;
								newRefundDuty.TaxAmountDifferenceForRefunds = differenceForRefunds;
							}
						}
					}
				}
			}
		}
	}
}
