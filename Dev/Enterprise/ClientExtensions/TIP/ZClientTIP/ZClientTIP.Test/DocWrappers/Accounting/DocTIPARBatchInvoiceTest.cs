using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TIP.Testing
{
	[TestedType(typeof(DocTIPARBatchInvoice))]
	public class DocTIPARBatchInvoiceTest : DocumentWrapperTestCase
	{
		public void TestClientOverride()
		{
			AssertEquals("Client's Override", typeof(DocTIPARBatchInvoice), DocARBatchInvoice.New(InvoiceBatchHeader, Factory).GetType());
		}

		public void TestPrintClientSpecific()
		{
			Assert(Wrapper.PrintClientSpecific);
			Assert(!Wrapper.PrintStandard);
		}

		#region Overrides
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { Wrapper };
		}

		#endregion
		InvoiceBatchHeader InvoiceBatchHeader;
		DocTIPARBatchInvoice Wrapper;
		protected override void SetUp()
		{
			InvoiceBatchHeader = Factory.New<InvoiceBatchHeader>();
			Wrapper = (DocTIPARBatchInvoice)DocTIPARBatchInvoice.New(InvoiceBatchHeader, Factory);
			base.SetUp();
		}
	}
}
