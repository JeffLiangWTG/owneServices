using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	[DataContract]
	public class MTDLiabilities
	{
		[DataMember]
		public IEnumerable<MTDLiability> liabilities { get; set; }
	}

	[DataContract]
	public class MTDLiability
	{
		[DataMember(IsRequired = false)]
		public MTDTaxPeriod taxPeriod { get; set; }

		[DataMember]
		public string type { get; set; }

		[DataMember]
		public decimal originalAmount { get; set; }

		[DataMember(IsRequired = false)]
		public decimal outstandingAmount { get; set; }

		[DataMember(IsRequired = false)]
		public string due { get; set; }
	}

	[DataContract]
	public class MTDTaxPeriod
	{
		[DataMember]
		public string from { get; set; }

		[DataMember]
		public string to { get; set; }
	}
}
