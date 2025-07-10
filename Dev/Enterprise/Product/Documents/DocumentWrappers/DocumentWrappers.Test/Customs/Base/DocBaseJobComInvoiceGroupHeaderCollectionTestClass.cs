using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	sealed class DocBaseJobComInvoiceGroupHeaderCollectionTestClass : DocBaseJobComInvoiceGroupHeaderCollection
	{
		public DocBaseJobComInvoiceGroupHeaderCollectionTestClass(IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocBaseJobComInvoiceGroupHeaderTestClass this[int index]
		{
			get { return (DocBaseJobComInvoiceGroupHeaderTestClass)Elements[index]; }
		}
	}
}
