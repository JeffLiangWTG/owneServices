using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocJobComInvoiceLineCollection : DocBaseJobComInvoiceLineCollection
	{
		public DocJobComInvoiceLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocJobComInvoiceLineCollection(InvoiceLineCompleteCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public DocJobComInvoiceLineCollection(JobComInvoiceLineViewCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocJobComInvoiceLine this[int index]
		{
			get
			{
				return (DocJobComInvoiceLine)Elements[index];
			}
		}
	}
}
