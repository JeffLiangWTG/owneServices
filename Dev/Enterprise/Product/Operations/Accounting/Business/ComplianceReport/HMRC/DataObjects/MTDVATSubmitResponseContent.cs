using System.Runtime.Serialization;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	[DataContract]
	public class MTDVATSubmitResponseContent
	{
		[DataMember]
		public string processingDate { get; set; }

		[DataMember(IsRequired = false)]
		public string paymentIndicator { get; set; }

		[DataMember]
		public string formBundleNumber { get; set; }

		[DataMember(IsRequired = false)]
		public string chargeRefNumber { get; set; }

		public string ReceiptID { get; set; }
	}
}
