using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(DocJobComInvoiceLine))]
sealed class DocJobComInvoiceLineTest : DocBaseJobComInvoiceLineAbstractTest<JobComInvoiceLine, DocJobComInvoiceLine>
{
	public void TestInvoiceHeaderExchangeRate()
	{
		InvoiceLine.InvoiceHeader.JZ_InvoiceCurrExRate = 0.665544m;
		AssertEquals(0.665544m, InvoiceLineWrapper.InvoiceHeaderExchangeRate);
	}

	#region Implementation

	protected override string TestingCountry
	{
		get { return Enterprise.Core.Constants.CountryCodes.UnitedArabEmirates; }
	}

	protected override DocJobComInvoiceLine CreateInvoiceLineWrapper(JobComInvoiceLine invoiceLineInternal)
	{
		return DocJobComInvoiceLine.New(invoiceLineInternal, Factory);
	}

	#endregion
}
