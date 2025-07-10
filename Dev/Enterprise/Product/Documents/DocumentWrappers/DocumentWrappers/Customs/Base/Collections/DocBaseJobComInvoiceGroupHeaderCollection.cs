using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Customs.Base
{
	public abstract class DocBaseJobComInvoiceGroupHeaderCollection : DocumentWrapperCollection
	{
		protected DocBaseJobComInvoiceGroupHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected DocBaseJobComInvoiceGroupHeaderCollection(IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocBaseJobComInvoiceGroupHeader this[int index]
		{
			get { return (DocBaseJobComInvoiceGroupHeader)base[index]; }
		}
	}
}
