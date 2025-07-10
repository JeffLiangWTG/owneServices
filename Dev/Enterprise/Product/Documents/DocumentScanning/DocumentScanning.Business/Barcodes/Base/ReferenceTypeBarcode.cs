namespace Enterprise.DocumentScanning.Business
{
	/// <summary>
	/// A DocumentResult that is a reference type. 
	/// </summary>
	public abstract class ReferenceTypeBarcode : BaseBarcode
	{
		public ReferenceTypeBarcode(DocumentFactory masterFactory)
			: base(masterFactory)
		{
		}

		public ReferenceTypeBarcode(DocumentFactory masterFactory, string barcodeText)
			: base(masterFactory, barcodeText)
		{
		}
	}
}
