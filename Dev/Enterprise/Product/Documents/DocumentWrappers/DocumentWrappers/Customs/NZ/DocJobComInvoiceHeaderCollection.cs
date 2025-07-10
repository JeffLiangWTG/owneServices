using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.DocumentWrappers.Customs.NZ
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
