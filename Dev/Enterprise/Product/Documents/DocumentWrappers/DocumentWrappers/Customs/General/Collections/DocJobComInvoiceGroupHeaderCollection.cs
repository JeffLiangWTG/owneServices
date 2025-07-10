using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.DocumentWrappers.Customs.General
{
	public class DocJobComInvoiceGroupHeaderCollection : DocBaseJobComInvoiceGroupHeaderCollection
	{
		public DocJobComInvoiceGroupHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocJobComInvoiceGroupHeaderCollection(IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocJobComInvoiceGroupHeader this[int index]
		{
			get
			{
				return (DocJobComInvoiceGroupHeader)Elements[index];
			}
		}
	}
}
