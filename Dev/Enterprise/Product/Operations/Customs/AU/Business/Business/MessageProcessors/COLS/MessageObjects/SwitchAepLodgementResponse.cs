using System.Collections.Generic;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SwitchAepLodgementResponse
	{
		public string result { get; set; }

		public string lrn { get; set; }

		public List<ValidationMessage> validationMessages { get; set; }
	}
}
