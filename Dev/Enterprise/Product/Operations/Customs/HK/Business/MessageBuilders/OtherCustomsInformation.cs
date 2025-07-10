using CargoWise.Types;

namespace Enterprise.Customs.HK.Business.MessageBuilders
{
	public class OtherCustomsInformation
	{
		public string CountryCode { get; set; }
		public string ContactPerson { get; set; }
		public ZString ContactCode { get; set; }
		public ZString ContactNumber { get; set; }
		public ZString ContactEmail { get; set; }
		public string AccountHolder { get; set; }
		public string AccountName { get; set; }
		public string AccountIssuer { get; set; }
		public string AccountNumber { get; set; }
		public string TraderNoType { get; set; }
		public string TraderNo { get; set; }
		public string Identifier { get; set; }
	}
}
