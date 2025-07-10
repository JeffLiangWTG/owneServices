using CargoWise.EntityFramework;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	sealed class DocBaseJobComInvoiceHeaderCollectionTestClass : DocBaseJobComInvoiceHeaderCollection
	{
		public DocBaseJobComInvoiceHeaderCollectionTestClass(Enterprise.Customs.Business.InvoiceHeaderActiveCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocBaseJobComInvoiceHeaderTestClass this[int index]
		{
			get { return (DocBaseJobComInvoiceHeaderTestClass)Elements[index]; }
		}
	}
}
