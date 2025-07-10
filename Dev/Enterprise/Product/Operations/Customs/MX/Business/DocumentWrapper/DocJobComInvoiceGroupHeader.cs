using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.MX.Business
{
	public class DocJobComInvoiceGroupHeader : DocBaseJobComInvoiceGroupHeader
	{
		public DocJobComInvoiceGroupHeader(BaseJobComInvoiceGroupHeader baseJobComInvoiceGroupHeader, BusinessObjectFactory factoryToWrap) : base(baseJobComInvoiceGroupHeader, factoryToWrap)
		{
		}

		public static DocJobComInvoiceGroupHeader New(JobComInvoiceGroupHeader jobComInvoiceGroupHeader, BusinessObjectFactory factoryToWrap)
		{
			return jobComInvoiceGroupHeader == null ? null : new DocJobComInvoiceGroupHeader(jobComInvoiceGroupHeader, factoryToWrap);
		}

		protected override DocBaseJobComInvoiceHeaderCollection CreateJobComInvoiceHeaderCollection(Customs.Business.InvoiceHeaderActiveCollection collectionToWrap)
		{
			return new DocJobComInvoiceHeaderCollection((InvoiceHeaderActiveCollection)collectionToWrap, Factory);
		}
	}
}
