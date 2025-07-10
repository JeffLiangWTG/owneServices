using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.AE.Business;

public class DocJobComInvoiceGroupHeaderCollection : DocBaseJobComInvoiceGroupHeaderCollection
{
	public DocJobComInvoiceGroupHeaderCollection(BusinessObjectFactory factory)
		: base(factory)
	{
	}

	public DocJobComInvoiceGroupHeaderCollection(Customs.Business.IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> collectionSource, BusinessObjectFactory factoryToWrap)
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
