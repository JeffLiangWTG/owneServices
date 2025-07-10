
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.DocumentWrappers.Customs.General
{
	public class DocJobComInvoiceGroupHeader : DocBaseJobComInvoiceGroupHeader
	{
		DocJobComInvoiceGroupHeader(BaseJobComInvoiceGroupHeader jobComInvoiceGroupHeader, BusinessObjectFactory factoryToWrap)
			: base(jobComInvoiceGroupHeader, factoryToWrap)
		{
		}

		public static DocJobComInvoiceGroupHeader New(BaseJobComInvoiceGroupHeader jobComInvoiceGroupHeader, BusinessObjectFactory factory)
		{
			if (jobComInvoiceGroupHeader == null)
			{
				return null;
			}
			else
			{
				return new DocJobComInvoiceGroupHeader(jobComInvoiceGroupHeader, factory);
			}
		}

		#region Overrides

		protected override DocBaseJobComInvoiceHeaderCollection CreateJobComInvoiceHeaderCollection(InvoiceHeaderActiveCollection collectionToWrap)
		{
			return new DocJobComInvoiceHeaderCollection(collectionToWrap, Factory);
		}

		#endregion
	}
}
