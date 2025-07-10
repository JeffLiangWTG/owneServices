using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.Invoicing
{
	public interface IInvoicingLineCacheUtility : IService
	{
		public void GenerateInvoicingLineCache(IEnumerable<InvoicingLineBase> invoiceLines);

		public void ValidateInvoicingLineCache(IEnumerable<InvoicingLineBase> invoicingLines);

		public IReadOnlyDictionary<ZGuid, InvoicingLinePlainDataObject> InvoicingLineCache { get; }

		public IEnumerable<InvoicingLinePlainDataObject> InvoicingLinePlainDataObjects { get; }
	}
}
