using System.Runtime.Serialization;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	[DataContract]
	public class MTDTokensData
	{
		[DataMember]
		public string access_token { get; set; }
		[DataMember]
		public string token_type { get; set; }
		[DataMember]
		public int expires_in { get; set; }
		[DataMember]
		public string refresh_token { get; set; }
		[DataMember]
		public string scope { get; set; }
	}
}
