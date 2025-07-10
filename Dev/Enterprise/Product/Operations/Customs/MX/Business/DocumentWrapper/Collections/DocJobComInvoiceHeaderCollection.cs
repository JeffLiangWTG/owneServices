using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.MX.Business
{
	public class DocJobComInvoiceHeaderCollection : DocBaseJobComInvoiceHeaderCollection
	{
		public DocJobComInvoiceHeaderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public DocJobComInvoiceHeaderCollection(InvoiceHeaderActiveCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocJobComInvoiceHeader this[int index] => (DocJobComInvoiceHeader)Elements[index];
	}
}
