using System.Collections.Generic;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class LodgementStatusResponse
	{
		public string status { get; set; }

		public string receivedDate { get; set; }

		public string type { get; set; }

		public string resultMessage { get; set; }

		public string result { get; set; }

		public List<ValidationMessage> messages { get; set; }
	}
}
