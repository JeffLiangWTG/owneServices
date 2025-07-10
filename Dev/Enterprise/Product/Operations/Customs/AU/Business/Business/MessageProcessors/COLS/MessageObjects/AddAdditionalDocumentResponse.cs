using System.Collections.Generic;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AddAdditionalDocumentResponse
	{
		public string result { get; set; }

		public List<ValidationMessage> validationMessages { get; set; }
	}
}
