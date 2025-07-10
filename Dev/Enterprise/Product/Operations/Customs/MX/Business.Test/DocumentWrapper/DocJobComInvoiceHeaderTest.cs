using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(DocJobComInvoiceHeader))]
	sealed class DocJobComInvoiceHeaderTest : DocBaseJobComInvoiceHeaderAbstractTest<JobComInvoiceHeader, DocJobComInvoiceHeader>
	{
		public override void TestConversionFactorIsWrapped()
		{
			InvoiceHeaderInternal.JZ_InvoiceAmount = 1000m;
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals(1m, InvoiceHeaderWrapperInternal.ConversionFactor);
		}

		public override void TestIncoTermDescription()
		{
			CombineAssertions(() =>
			{
				invoiceHeader.JZ_IncoTerm = "EXW";
				AssertEquals("Inco term should be 'Ex Works'", "Ex Works", invoiceHeaderWrapper.IncoTermDescription);

				invoiceHeader.JZ_IncoTerm = "CIF";
				AssertEquals("Inco term should be 'Cost, Insurance And Freight'", "Cost, Insurance And Freight", invoiceHeaderWrapper.IncoTermDescription);
			});
		}

		#region Implementation

		JobComInvoiceHeader invoiceHeader;
		DocJobComInvoiceHeader invoiceHeaderWrapper;

		protected override string TestingCountry => Core.Constants.CountryCodes.Mexico;

		protected override void SetUp()
		{
			base.SetUp();
			invoiceHeader = InvoiceHeaderInternal;
			invoiceHeaderWrapper = InvoiceHeaderWrapperInternal;
		}

		protected override DocJobComInvoiceHeader CreateInvoiceHeaderWrapper(JobComInvoiceHeader invoiceHeaderInternal) => DocJobComInvoiceHeader.New(invoiceHeaderInternal, Factory);

		#endregion
	}
}
