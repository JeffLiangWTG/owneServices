using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocJobComInvoiceHeaderCollection : DocBaseJobComInvoiceHeaderCollection
	{
		public DocJobComInvoiceHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocJobComInvoiceHeaderCollection(InvoiceHeaderActiveCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocJobComInvoiceHeader this[int index]
		{
			get
			{
				return (DocJobComInvoiceHeader)Elements[index];
			}
		}
	}
}
