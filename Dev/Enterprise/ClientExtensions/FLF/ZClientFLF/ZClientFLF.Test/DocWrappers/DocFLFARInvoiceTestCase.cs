using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.FLF.DocWrappers.Testing
{
	[TestedType(typeof(DocFLFARInvoice))]
	public class DocFLFARInvoiceTestCase : DocumentWrapperTestCase
	{
		public void TestPrintClientSpecific()
		{
			AssertEquals("Should print client specific", ZBool.True, wrapper.PrintClientSpecific);
			AssertEquals("Should not print standard", ZBool.False, wrapper.PrintStandard);
		}

		public void TestClientOverride()
		{
			AssertEquals("Client's DocInvoice Override", typeof(DocFLFARInvoice), DocARInvoice.New(invoice, Factory).GetType());
		}

		#region SetUp && Overrides
		DocFLFARInvoice wrapper;
		InvoicingBase invoice;
		protected override void SetUp()
		{
			invoice = Factory.NewWithValidTestData<ARInvoice>();
			wrapper = DocFLFARInvoice.New(invoice, Factory);
			base.SetUp();
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { wrapper };
		}
		#endregion
	}
}
