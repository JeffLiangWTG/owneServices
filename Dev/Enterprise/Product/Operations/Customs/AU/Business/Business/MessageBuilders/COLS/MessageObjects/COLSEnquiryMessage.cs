namespace Enterprise.Customs.AU.Declaration.Business
{
	public class COLSEnquiryMessage
	{
		public string enquiryType { get; set; }

		public string lodgementReferenceNumber { get; set; }

		public string fullImportDeclarationNumber { get; set; }

		public string contactName { get; set; }

		public string contactPhone { get; set; }

		public string contactEmail { get; set; }

		public string additionalComments { get; set; }

		public string generalDeclaration { get; set; }

		public bool documentationRequired { get; set; }
	}
}
