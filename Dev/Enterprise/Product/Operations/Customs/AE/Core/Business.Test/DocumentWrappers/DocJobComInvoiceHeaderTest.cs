using CargoWise.Types;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(DocJobComInvoiceHeader))]
sealed class DocJobComInvoiceHeaderTest : DocBaseJobComInvoiceHeaderAbstractTest<JobComInvoiceHeader, DocJobComInvoiceHeader>
{
	#region Overrides

	public override void TestIncoTermDescription()
	{
		InvoiceHeaderInternal.JZ_IncoTerm = "EXW";
		Assert("Inco term should be empty", InvoiceHeaderWrapperInternal.IncoTermDescription != ZString.Empty);

		InvoiceHeaderInternal.JZ_IncoTerm = "CIF";
		Assert("Inco term should be not be empty", InvoiceHeaderWrapperInternal.IncoTermDescription != ZString.Empty);
	}

	public override void TestConversionFactorIsWrapped()
	{
		InvoiceHeaderInternal.JZ_InvoiceAmount = 1000m;
		InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		AssertEquals(1m, InvoiceHeaderWrapperInternal.ConversionFactor);
	}

	#endregion

	#region Implementation

	protected override string TestingCountry
	{
		get { return Core.Constants.CountryCodes.UnitedArabEmirates; }
	}

	protected override DocJobComInvoiceHeader CreateInvoiceHeaderWrapper(JobComInvoiceHeader invoiceHeaderInternal)
	{
		return DocJobComInvoiceHeader.New(invoiceHeaderInternal, Factory);
	}

	#endregion
}
