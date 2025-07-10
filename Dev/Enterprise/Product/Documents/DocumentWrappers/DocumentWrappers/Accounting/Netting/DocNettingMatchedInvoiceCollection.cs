using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocNettingMatchedInvoiceCollection : DocumentWrapperCollection<DocNettingMatchedInvoice>
	{
		protected DocNettingMatchedInvoiceCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static DocNettingMatchedInvoiceCollection New(BusinessObjectFactory factory)
		{
			return new DocNettingMatchedInvoiceCollection(factory);
		}
	}
}
