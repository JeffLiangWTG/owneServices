using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Printing
{
	public sealed class ResetEDocStatusService : IService
	{
		public void ResetEDocStatus(InvoicingBase invoiceToGenerate)
		{
			var docManagerSupport = invoiceToGenerate as IDocManagerSupport;
			if (docManagerSupport == null)
			{
				return;
			}

			var publishedEDocs = docManagerSupport.DocManagerInfo.AllEDocs.OfType<StorageDocsBase>()
								?.Where(x => x.SC_IsPublished
									&& x.SC_IsSystemGenerated
									&& x.SC_DocType == Core.Constants.RefDocTypes.Invoice
									&& x.SC_FileName.Contains(InvoiceFileNameProvider.GetInvoiceFileName(invoiceToGenerate))
									&& x.IsInDatabase);
			if (publishedEDocs == null)
			{
				return;
			}

			foreach (var eDoc in publishedEDocs)
			{
				eDoc.SC_IsPublished = false;
			}
		}
	}
}
