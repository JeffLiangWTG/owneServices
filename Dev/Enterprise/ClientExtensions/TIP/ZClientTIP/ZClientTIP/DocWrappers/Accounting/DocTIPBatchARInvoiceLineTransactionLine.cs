using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentWrappers;

namespace Enterprise.Client.TIP
{
	public class DocTIPBatchARInvoiceLineTransactionLine : DocBatchARInvoiceLineTransactionLine
	{
		#region Constructors && Type Overidding
		protected DocTIPBatchARInvoiceLineTransactionLine(InvoicingLineBase line, BusinessObjectFactory factoryToWrap)
			: base(line, factoryToWrap) { }

		public new static DocTIPBatchARInvoiceLineTransactionLine New(InvoicingLineBase line, BusinessObjectFactory factory)
		{
			DocTIPBatchARInvoiceLineTransactionLine result = (line != null) ? new DocTIPBatchARInvoiceLineTransactionLine(line, factory) : null;

			return result;
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(OverriddenNewMethod);
		}

		static DocTIPBatchARInvoiceLineTransactionLine OverriddenNewMethod(InvoicingLineBase line, BusinessObjectFactory factory)
		{
			return DocTIPBatchARInvoiceLineTransactionLine.New(line, factory);
		}

		#endregion

		public new ZString OtherReference
		{
			get { return GetOtherReference(false); }
		}
	}
}
