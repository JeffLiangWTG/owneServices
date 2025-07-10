using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.DocumentWrappers.Customs.NZ
{
	public class DocJobComInvoiceGroupHeaderCollection : DocBaseJobComInvoiceGroupHeaderCollection
	{
		public DocJobComInvoiceGroupHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocJobComInvoiceGroupHeaderCollection(Enterprise.Customs.Business.IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> collectionSource, BusinessObjectFactory factoryToWrap)
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
