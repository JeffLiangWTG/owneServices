
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.DocumentWrappers
{
	public class DocBatchARInvoiceLineTransactionLine : DocARInvoiceLine
	{
		protected DocBatchARInvoiceLineTransactionLine(InvoicingLineBase line, BusinessObjectFactory factoryToWrap)
			: base(line, factoryToWrap)
		{
			ValueMultiplier = line is ARCreditNoteLine ? -1 : 1;
		}

		public int ValueMultiplier;

		public static new DocBatchARInvoiceLineTransactionLine New(InvoicingLineBase line, BusinessObjectFactory factoryToWrap)
		{
			DocBatchARInvoiceLineTransactionLine result = null;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(line, factoryToWrap);
			}
			else if (line != null)
			{
				result = new DocBatchARInvoiceLineTransactionLine(line, factoryToWrap);
			}
			return result;
		}

		protected new delegate DocBatchARInvoiceLineTransactionLine NewDelegate(InvoicingLineBase line, BusinessObjectFactory factoryToWrap);
		protected new static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		protected override ZDecimal OSAmountCore
		{
			get { return base.OSAmountCore * ValueMultiplier; }
		}

		protected override ZDecimal OSExTaxAmountCore
		{
			get { return base.OSExTaxAmountCore * ValueMultiplier; }
		}

		protected override ZDecimal OSTaxAmountCore
		{
			get { return base.OSTaxAmountCore * ValueMultiplier; }
		}

		protected override ZDecimal LineAmountCore
		{
			get { return base.LineAmountCore * ValueMultiplier; }
		}
	}
}
