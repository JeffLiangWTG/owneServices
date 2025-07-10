using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Customs.Base
{
	public abstract class DocBaseJobComInvoiceHeaderCollection : DocumentWrapperCollection
	{
		protected DocBaseJobComInvoiceHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected DocBaseJobComInvoiceHeaderCollection(InvoiceHeaderActiveCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocBaseJobComInvoiceHeader this[int index]
		{
			get { return (DocBaseJobComInvoiceHeader)base[index]; }
		}
	}
}
