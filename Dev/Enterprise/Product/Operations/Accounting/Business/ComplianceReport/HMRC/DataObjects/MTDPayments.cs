using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	[DataContract]
	public class MTDPayments
	{
		[DataMember]
		public IEnumerable<MTDPayment> payments { get; set; }
	}

	[DataContract]
	public class MTDPayment
	{
		[DataMember]
		public decimal amount { get; set; }

		[DataMember(IsRequired = false)]
		public string received { get; set; }
	}
}
