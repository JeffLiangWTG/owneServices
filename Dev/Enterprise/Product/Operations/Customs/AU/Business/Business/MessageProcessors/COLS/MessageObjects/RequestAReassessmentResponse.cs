using System.Collections.Generic;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RequestAReassessmentResponse
	{
		public string generatedLrn { get; set; }

		public string result { get; set; }

		public List<ValidationMessage> messages { get; set; }
	}
}
