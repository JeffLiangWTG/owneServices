using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(DocJobComInvoiceLine))]
class DocJobComInvoiceLineTest : DocBaseJobComInvoiceLineAbstractTest<JobComInvoiceLine, DocJobComInvoiceLine>
{
	#region Implementation

	protected override string TestingCountry
	{
		get { return Enterprise.Core.Constants.CountryCodes.Switzerland; }
	}

	protected override DocJobComInvoiceLine CreateInvoiceLineWrapper(JobComInvoiceLine invoiceLineInternal)
	{
		return DocJobComInvoiceLine.New(invoiceLineInternal, Factory);
	}

	#endregion
}
