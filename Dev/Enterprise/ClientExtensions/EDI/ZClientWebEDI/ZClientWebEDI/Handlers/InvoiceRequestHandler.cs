using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class InvoiceRequestHandler : DataRequestHandler<InvoiceRequestHelper>
	{
		public override string FileName => HasInvoiceDocs ? Path.ChangeExtension(InvoiceDocs.First().FileName, ".zip") : "Invoice.tif";

		public override string ContentType => HasInvoiceDocs ? DataContentTypes.Zip : DataContentTypes.Tiff;

		public override ZBlob GetBinaryData()
		{
			if (HasInvoiceDocs)
			{
				var entries = Enumerable.Empty<ZipStream>();
				try
				{
					entries = InvoiceDocs.Select(x => new ZipStream(x.FileName, new MemoryStream(x.ImageData)));
					using (var zipStream = new MemoryStream())
					{
						new ZipCreator().ZipStream(entries, zipStream);
						return zipStream.ToArray();
					}
				}
				finally
				{
					foreach (var entry in entries)
					{
						entry.Stream.Dispose();
					}
				}
			}

			return ZBlob.Empty;
		}

		bool HasInvoiceDocs => InvoiceDocs?.Any() ?? false;

		IEnumerable<IeDoc> InvoiceDocs
		{
			get
			{
				if (invoiceDocs == null)
				{
					var transaction = (BusinessObjects.Length > 0) ? BusinessObjects[0] as IDocManagerSupport : null;
					if (transaction != null)
					{
						var summaries = transaction.DocManagerInfo.AllEDocs.OfType<IeDoc>().Where(x => x.DocType == StlBill.ParentSummaryDocType).OrderByDescending(x => x.DateAdded);
						invoiceDocs = new[] {   transaction.DocManagerInfo.AllEDocs.GetMostRecentEDoc("INV"),
												summaries.FirstOrDefault(x => x.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)),
												summaries.FirstOrDefault(x => x.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
											}.WhereNotNull().ToArray();
					}
				}
				return invoiceDocs;
			}
		}
		IEnumerable<IeDoc> invoiceDocs;

		protected override BusinessObject[] GetNewBusinessObjects()
		{
			return (PKs.Length > 0)
				? new BusinessObject[] { Factory.Load<AccTransactionHeader>(PKs[0]) }
				: Array.Empty<BusinessObject>();
		}
	}
}

