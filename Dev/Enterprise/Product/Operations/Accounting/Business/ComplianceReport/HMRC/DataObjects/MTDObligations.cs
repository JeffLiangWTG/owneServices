using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	[DataContract]
	public class MTDObligations
	{
		[DataMember]
		public IEnumerable<MTDObligation> obligations { get; set; }
	}

	[DataContract]
	public class MTDObligation
	{
		[DataMember]
		public string start { get; set; }

		[DataMember]
		public string end { get; set; }

		[DataMember]
		public string due { get; set; }

		[DataMember]
		public string status { get; set; }

		[DataMember]
		public string periodKey { get; set; }

		[DataMember(IsRequired = false)]
		public string received { get; set; }
	}

	public static class MTDObligationStatus
	{
		public const string Fulfilled = "F";
		public const string Open = "O";
	}
}