
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocJobComInvoiceGroupHeader : DocBaseJobComInvoiceGroupHeader
	{
		DocJobComInvoiceGroupHeader(JobComInvoiceGroupHeader jobComInvoiceGroupHeader, BusinessObjectFactory factoryToWrap)
			: base(jobComInvoiceGroupHeader, factoryToWrap)
		{
		}

		public static DocJobComInvoiceGroupHeader New(JobComInvoiceGroupHeader jobComInvoiceGroupHeader, BusinessObjectFactory factoryToWrap)
		{
			if (jobComInvoiceGroupHeader == null)
			{
				return null;
			}
			else
			{
				return new DocJobComInvoiceGroupHeader(jobComInvoiceGroupHeader, factoryToWrap);
			}
		}

		#region Overrides

		protected override DocBaseJobComInvoiceHeaderCollection CreateJobComInvoiceHeaderCollection(Enterprise.Customs.Business.InvoiceHeaderActiveCollection collectionToWrap)
		{
			return new DocJobComInvoiceHeaderCollection((InvoiceHeaderActiveCollection)collectionToWrap, Factory);
		}

		#endregion

		#region ZString Fields

		public ZString CommissionType
		{
			get { return JobComInvoiceGroupHeader.JZ_CommissionType; }
		}

		#endregion

		#region Wrapper Fields

		public DocDeclaration Declaration
		{
			get { return DocDeclaration.New(JobComInvoiceGroupHeader.JobDeclaration, Factory); }
		}

		public DocJobComInvoiceHeaderCollection InvoiceHeaders
		{
			get { return (DocJobComInvoiceHeaderCollection)InvoiceHeadersInternal; }
		}

		#endregion

		#region Implementation

		JobComInvoiceGroupHeader JobComInvoiceGroupHeader
		{
			get { return (JobComInvoiceGroupHeader)WrappedObject; }
		}

		#endregion
	}
}
