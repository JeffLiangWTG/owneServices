using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.DocumentWrappers
{
	public class DocARBatchInvoiceLine : DocARInvoice
	{
		DocARBatchInvoiceLine(InvoicingBase invoicingBase, BusinessObjectFactory factoryToWrap)
			: base(invoicingBase, factoryToWrap)
		{
			ValueMultiplier = invoicingBase is ARCreditNote ? -1 : 1;
		}

		readonly int ValueMultiplier;
		protected new delegate DocARBatchInvoiceLine NewDelegate(InvoicingBase invoicingBase, BusinessObjectFactory factoryToWrap);
		protected new static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		public static new DocARBatchInvoiceLine New(InvoicingBase invoicingBase, BusinessObjectFactory factoryToWrap)
		{
			DocARBatchInvoiceLine result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(invoicingBase, factoryToWrap);
			}
			else if (invoicingBase != null)
			{
				result = new DocARBatchInvoiceLine(invoicingBase, factoryToWrap);
			}
			return result;
		}

		protected override ZDecimal GetLineOSExTaxAmount(IDocARInvoiceLine line)
		{
			return base.GetLineOSExTaxAmount(line) * ValueMultiplier;
		}

		protected override ZDecimal OSTotalCore
		{
			get { return base.OSTotalCore * ValueMultiplier; }
		}

		protected override ZDecimal TotalOSTaxAmountCore
		{
			get { return base.TotalOSTaxAmountCore * ValueMultiplier; }
		}

		protected override ZDecimal InvoiceSubTotalCore
		{
			get { return base.InvoiceSubTotalCore * ValueMultiplier; }
		}
	}
}
