using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Invoicing
{
	public sealed class InvoicingLineCacheUtility : IInvoicingLineCacheUtility
	{
		public InvoicingLineCacheUtility()
		{
			invoicingLineCache = new Dictionary<ZGuid, InvoicingLinePlainDataObject>();
			invoicingLinePlainDataObjects = new List<InvoicingLinePlainDataObject>();
		}

		public void GenerateInvoicingLineCache(IEnumerable<InvoicingLineBase> invoicingLines)
		{
			invoicingLineCache.Clear();
			invoicingLinePlainDataObjects.Clear();
			foreach (var invoiceLine in invoicingLines)
			{
				var invoiceLinePlainObject = InvoicingLinePlainDataObject.Create(invoiceLine);
				invoicingLineCache.Add(invoiceLinePlainObject.PK, invoiceLinePlainObject);
				invoicingLinePlainDataObjects.Add(invoiceLinePlainObject);
			}
		}

		public void ValidateInvoicingLineCache(IEnumerable<InvoicingLineBase> invoicingLines)
		{
			if (doesCacheStaleHappened)
			{
				throw CreateCacheStaleException();
			}

			var totalLines = 0;
			foreach (var invoiceLine in invoicingLines)
			{
				var cachedInvoiceLines = invoicingLineCache[invoiceLine.PK];
				if (cachedInvoiceLines.ChargeCodePK != (invoiceLine.ChargeCode?.PK ?? ZGuid.Empty) ||
					cachedInvoiceLines.BranchPK != invoiceLine.Branch.PK ||
					cachedInvoiceLines.DepartmentPK != invoiceLine.Department.PK ||
					cachedInvoiceLines.JobPK != (invoiceLine.Job?.PK ?? ZGuid.Empty) ||
					cachedInvoiceLines.RelatedJobPK != invoiceLine.AL_Calc_RelatedJobPK ||
					cachedInvoiceLines.Currrency != invoiceLine.AL_RX_NKTransactionCurrency ||
					cachedInvoiceLines.ExchangeRate != invoiceLine.AL_ExchangeRate)
				{
					ReportAndThrowCacheStaleException();
				}

				totalLines++;
			}

			if (totalLines != invoicingLineCache.Count)
			{
				ReportAndThrowCacheStaleException();
			}
		}

		void ReportAndThrowCacheStaleException()
		{
			doesCacheStaleHappened = true;
			ErrorReporter.ReportOnce("PostTransacrtionCacheStale", "Invoicing Lines have been changed after the cache was already created.");

			throw CreateCacheStaleException();
		}

		ZCannotSaveException CreateCacheStaleException()
		{
			var errorContent = Res.GetString("6eff9c8c-224c-4581-aa9e-cfa406f601ba", @"The transaction line cache is out of date.
Please turn off the '{0}' registry option, then close and reopen the form to try again.", AccountingConfigurationRegistry.Instance.EnablePostTransactionCalculatedPropertyCache.HumanReadableRegistryPath());
			var errorHeading = Res.GetString("7810556f-87f0-4dc2-846b-3b8da88ed174", "Cannot Save Invoice");
			var cannotSaveException = new ZCannotSaveException(errorContent, errorHeading);

			return cannotSaveException;
		}

		readonly Dictionary<ZGuid, InvoicingLinePlainDataObject> invoicingLineCache;

		readonly List<InvoicingLinePlainDataObject> invoicingLinePlainDataObjects;

		IReadOnlyDictionary<ZGuid, InvoicingLinePlainDataObject> IInvoicingLineCacheUtility.InvoicingLineCache => invoicingLineCache;

		IEnumerable<InvoicingLinePlainDataObject> IInvoicingLineCacheUtility.InvoicingLinePlainDataObjects => invoicingLinePlainDataObjects;

		bool doesCacheStaleHappened;
	}
}
