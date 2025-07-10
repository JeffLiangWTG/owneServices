using System.Collections.Generic;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class MakeAnEnquiryResponse
	{
		public string lrn { get; set; }

		public string result { get; set; }

		public List<ValidationMessage> messages { get; set; }
	}
}
