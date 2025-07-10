using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Registry.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Invoicing
{
	public class InvoiceTaxDateCacheProvider
	{
		public ZDateTime GetEarliestInvoiceTaxDate(IEnumerable<ITransactionLineTaxDate> lines, ZDateTime invoiceDate)
		{
			if (!invoiceTaxDateEarliest.IsValid || !AccountingConfigurationRegistry.Instance.EnablePostTransactionCalculatedPropertyCache.Value)
			{
				invoiceTaxDateEarliest = GetTaxDateCore(lines, invoiceDate, orderAscending: true);
			}

			return invoiceTaxDateEarliest;
		}

		public ZDateTime GetLatestInvoiceTaxDate(IEnumerable<ITransactionLineTaxDate> lines, ZDateTime invoiceDate)
		{
			if (!invoiceTaxDateLatest.IsValid || !AccountingConfigurationRegistry.Instance.EnablePostTransactionCalculatedPropertyCache.Value)
			{
				invoiceTaxDateLatest = GetTaxDateCore(lines, invoiceDate, orderAscending: false);
			}

			return invoiceTaxDateLatest;
		}

		public virtual void StaleCache()
		{
			invoiceTaxDateEarliest = ZDateTime.Invalid;
			invoiceTaxDateLatest = ZDateTime.Invalid;
		}

		//The return value of this method will be cached into 'class InvoiceTaxDateCacheProvider'.
		//If you need to change the properties used in this method,
		//please refer to 'class InvoicingLineTaxDateCacheProvider' to clear cache when the properties which you are using have changed.
		ZDateTime GetTaxDateCore(IEnumerable<ITransactionLineTaxDate> lines, ZDateTime invoiceDate, bool orderAscending)
		{
			var result = ZDateTime.Empty;

			var linesWithTaxID = lines.Where(x =>
			{
				var chargeCode = !x.AL_AC.IsEmpty ? x.ChargeCode : null;
				var isCommentLine = chargeCode != null && chargeCode.AC_ChargeType == ChargeType.Comment;
				return !isCommentLine && x.TaxRate != null;
			});

			if (linesWithTaxID.Any())
			{
				var taxDates = linesWithTaxID.Where(x => x.AL_TaxDate.IsValid).Select(y => y.AL_TaxDate);
				taxDates = orderAscending ? taxDates.OrderBy(z => z) : taxDates.OrderByDescending(z => z);
				var taxDate = taxDates.FirstOrDefault();

				result = taxDate.IsValid ? taxDate.ToDateTime() : invoiceDate;
			}

			return result;
		}

		ZDateTime invoiceTaxDateEarliest;

		ZDateTime invoiceTaxDateLatest;

#if DEBUG
		public ZDateTime InvoiceTaxDateEarliest_ForTestOnly => invoiceTaxDateEarliest;
		public ZDateTime InvoiceTaxDateLatest_ForTestOnly => invoiceTaxDateLatest;
#endif
	}
}
