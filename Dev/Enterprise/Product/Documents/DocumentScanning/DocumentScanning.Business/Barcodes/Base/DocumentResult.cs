using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentScanning.Business
{
	/// <summary>
	/// A placeholder used in scanning or importing as the pages are parsed.
	/// It equates to one document record once the page is later processed into a StorageDocs record.
	/// </summary>
	public class DocumentResult : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DocumentResult(DocumentFactory masterFactory)
			: base(masterFactory)
		{
		}

		public DocumentResult(DocumentFactory masterFactory, string filename)
			: this(masterFactory)
		{
			FilePath = filename;
		}

		public ZString FilePath;

		public ZString SourceFileName { get; set; }

		public ZString ScannedBarcodeValue { get; set; }

		#region MasterFactory
		public DocumentFactory MasterFactory
		{
			get { return (DocumentFactory)Factory; }
		}
		#endregion
	}
}
