using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	sealed class DocBaseJobComInvoiceLineTestClass : DocBaseJobComInvoiceLine
	{
		DocBaseJobComInvoiceLineTestClass(BaseJobComInvoiceLine invoiceLine, BusinessObjectFactory factoryToWrap)
			: base(invoiceLine, factoryToWrap)
		{
		}

		public new static DocBaseJobComInvoiceLineTestClass New(BaseJobComInvoiceLine invoiceLine, BusinessObjectFactory factoryToWrap)
		{
			if (invoiceLine == null)
			{
				return null;
			}
			else
			{ return new DocBaseJobComInvoiceLineTestClass(invoiceLine, factoryToWrap); }
		}

		public DocBaseJobComInvoiceHeader InvoiceHeaderInternalTestMethod
		{
			get { return base.InvoiceHeaderInternal; }
		}

		public DocBaseCusEntryLine CusEntryLineInternalTestMethod
		{
			get { return base.CusEntryLineInternal; }
		}

		public ZString GetFormattedNumberToMinDecimals(ZDecimal value, ZInt minDec)
		{
			return base.FormatNumberToMinDecimals(value, minDec);
		}
		protected override DocBaseJobComInvoiceHeader CreateJobComInvoiceHeader(BaseJobComInvoiceHeader invoiceToWrap)
		{
			return DocBaseJobComInvoiceHeaderTestClass.New(invoiceToWrap, Factory);
		}

		protected override DocBaseCusEntryLine CreateCusEntryLine(CusEntryLine entryLineToWrap)
		{
			return DocBaseCusEntryLineTestClass.New(entryLineToWrap, Factory);
		}
	}
}
