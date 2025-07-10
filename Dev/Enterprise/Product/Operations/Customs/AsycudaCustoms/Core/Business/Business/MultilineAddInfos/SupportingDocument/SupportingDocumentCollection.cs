namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class SupportingDocumentCollection : Customs.Business.CusSupportingInfoCollection<SupportingDocument>
	{
		public SupportingDocumentCollection(JobComInvoiceHeader parent) : base(parent, Constants.CusSupportingInfoTypes.CusSupportingDocument) { }

		public SupportingDocumentCollection(JobComInvoiceLine parent) : base(parent, Constants.CusSupportingInfoTypes.CusSupportingDocument) { }
	}
}
