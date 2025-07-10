using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocARBatchInvoiceLineCollection : DocumentWrapperCollection
	{
		public DocARBatchInvoiceLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocARBatchInvoiceLine this[int index]
		{
			get { return (DocARBatchInvoiceLine)base[index]; }
		}
	}
}
