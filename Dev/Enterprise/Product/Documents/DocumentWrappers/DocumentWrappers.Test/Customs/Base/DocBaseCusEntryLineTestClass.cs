using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	sealed class DocBaseCusEntryLineTestClass : DocBaseCusEntryLine
	{
		public DocBaseCusEntryLineTestClass(CusEntryLine cusEntryLine, BusinessObjectFactory factoryToWrap)
			: base(cusEntryLine, factoryToWrap)
		{
		}

		public static DocBaseCusEntryLineTestClass New(CusEntryLine cusEntryLine, BusinessObjectFactory factoryToWrap)
		{
			if (cusEntryLine == null)
			{
				return null;
			}
			else
			{
				return new DocBaseCusEntryLineTestClass(cusEntryLine, factoryToWrap);
			}
		}

		public DocBaseJobComInvoiceLineTestClass InvoiceLineInternalTestMethod
		{
			get { return (DocBaseJobComInvoiceLineTestClass)base.InvoiceLineInternal; }
		}

		protected override DocBaseJobComInvoiceLine CreateJobComInvoiceLine(BaseJobComInvoiceLine invoiceLineToWrap)
		{
			return DocBaseJobComInvoiceLineTestClass.New(invoiceLineToWrap, Factory);
		}
	}
}
