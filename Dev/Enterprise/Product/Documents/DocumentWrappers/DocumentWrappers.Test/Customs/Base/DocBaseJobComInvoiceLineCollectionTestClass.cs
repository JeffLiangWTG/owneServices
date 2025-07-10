using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	sealed class DocBaseJobComInvoiceLineCollectionTestClass : DocBaseJobComInvoiceLineCollection
	{
		public DocBaseJobComInvoiceLineCollectionTestClass(BaseJobComInvoiceLineViewCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public DocBaseJobComInvoiceLineCollectionTestClass(InvoiceLineCompleteCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocBaseJobComInvoiceLineTestClass this[int index]
		{
			get
			{
				return (DocBaseJobComInvoiceLineTestClass)Elements[index];
			}
		}
	}
}
