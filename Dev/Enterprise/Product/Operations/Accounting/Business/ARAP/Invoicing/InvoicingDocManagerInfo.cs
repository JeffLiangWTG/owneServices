using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class InvoicingDocManagerInfo : AccountingDocManagerInfo
	{
		public InvoicingDocManagerInfo(BusinessObject parent, string docManagerCode)
			: base(parent, docManagerCode)
		{
		}

		protected override IEDocsProvider[] GetEDocsProvidersCore()
		{
			IEDocsProvider[] result = null;

			InvoicingBase businessEntity = BusinessEntity as InvoicingBase;
			if (businessEntity != null)
			{
				JobHeader job = businessEntity.Job;
				if (job != null)
				{
					IEDocsProvider jobParent = job.Parent as IEDocsProvider;
					if (jobParent != null)
					{
						result = new IEDocsProvider[] { jobParent };
					}
				}
			}

			return result ?? System.Array.Empty<IEDocsProvider>();
		}

		internal StorageMain InvoiceDocStorageMain => base.StorageMain as StorageMain;
	}
}
