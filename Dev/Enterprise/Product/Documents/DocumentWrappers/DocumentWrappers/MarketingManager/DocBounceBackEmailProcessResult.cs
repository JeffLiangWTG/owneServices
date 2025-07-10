using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MarketingManager.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocBounceBackEmailProcessResult : DocumentWrapper
	{
		DocBounceBackEmailProcessResult(BounceBackEmailProcessResult processResult, BusinessObjectFactory factoryToWrap)
			: base(processResult, factoryToWrap)
		{ }

		public static DocBounceBackEmailProcessResult New(BounceBackEmailProcessResult processResult, BusinessObjectFactory factoryToWrap)
		{
			return (processResult != null) ? new DocBounceBackEmailProcessResult(processResult, factoryToWrap) : null;
		}

		BounceBackEmailProcessResult ProcessResult => (BounceBackEmailProcessResult)WrappedObject;

		[DocumentField("Job Number")]
		public ZString JobNumber => ProcessResult.JobNumber;

		[DocumentField("Document Name")]
		public ZString DocumentName => ProcessResult.DocumentName;

		[DocumentField("Recipient")]
		public ZString Recipient => ProcessResult.BouncedRecipients.ToString();

		[DocumentField("Bounced Reason Code")]
		public ZString BouncedReasonCode => ProcessResult.BounceReasonCode;

		[DocumentField("Origin Email Sent Time")]
		public ZString OriginEmailSentTime => ProcessResult.SentTimeText;

		[DocumentField("Non-delivery Receipt Email PK")]
		public ZString NonDeliveryReceiptEmailPK => ProcessResult.MailItemPK.ToString();

		[DocumentField("Non-delivery Receipt Email Received Time (UTC)")]
		public ZString NonDeliveryReceiptEmailReceivedTime => ProcessResult.MailItemReceivedTimeUtc.ToLongTimeString();

		[DocumentField("Non-delivery Receipt Email Subject")]
		public ZString NonDeliveryReceiptEmailSubject => ProcessResult.MailItemSubject;

		[DocumentField("Sender Email Address")]
		public ZString SenderEmailAddress => ProcessResult.SenderStaff?.GS_EmailAddress ?? ZString.Empty;

		[DocumentField("Sender Staff Code")]
		public ZString SenderStaffCode => ProcessResult.SenderStaff?.GS_Code ?? ZString.Empty;

		[DocumentField("Sender Staff Name")]
		public ZString SenderStaffName => ProcessResult.SenderStaff?.GS_FullName ?? ZString.Empty;
	}
}
