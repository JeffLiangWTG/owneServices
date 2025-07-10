using System.Collections.Generic;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class PaymentStatusResponse
	{
		public string referenceNumberType { get; set; }
		public string invoiceNumber { get; set; }
		public string accountNumber { get; set; }
		public string paymentStatus { get; set; }
		public string result { get; set; }
		public List<ValidationMessage> messages { get; set; }
	}
}
