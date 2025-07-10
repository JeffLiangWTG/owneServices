
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.CH.Business;

public class DocJobComInvoiceGroupHeader : DocBaseJobComInvoiceGroupHeader
{
	DocJobComInvoiceGroupHeader(JobComInvoiceGroupHeader jobComInvoiceGroupHeader, BusinessObjectFactory factoryToWrap)
		: base(jobComInvoiceGroupHeader, factoryToWrap)
	{
	}

	public static DocJobComInvoiceGroupHeader New(JobComInvoiceGroupHeader jobComInvoiceGroupHeader, BusinessObjectFactory factoryToWrap)
	{
		return jobComInvoiceGroupHeader == null ? null : new DocJobComInvoiceGroupHeader(jobComInvoiceGroupHeader, factoryToWrap);
	}

	#region Overrides

	protected override DocBaseJobComInvoiceHeaderCollection CreateJobComInvoiceHeaderCollection(Customs.Business.InvoiceHeaderActiveCollection collectionToWrap)
	{
		return new DocJobComInvoiceHeaderCollection((InvoiceHeaderActiveCollection)collectionToWrap, Factory);
	}

	#endregion
}
