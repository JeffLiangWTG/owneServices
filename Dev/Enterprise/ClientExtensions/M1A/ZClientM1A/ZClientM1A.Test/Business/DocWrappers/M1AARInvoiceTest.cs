using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.M1A.Business.Testing
{
	[TestedType(typeof(M1ADocARInvoice))]
	public class M1AARInvoiceTest : DocumentWrapperTestCase
	{
		public void TestClientOverride()
		{
			AssertEquals("Client's Doc Wrapper Override", typeof(M1ADocARInvoice), DocARInvoice.New(invoice, Factory).GetType());
		}

		public void TestPrintStandardProperty()
		{
			AssertEquals("PrintStandard", ZBool.False, wrapper.PrintStandard);
		}

		public void TestPrintClientSpecificProperty()
		{
			AssertEquals("PrintClientSpecific", ZBool.True, wrapper.PrintClientSpecific);
		}

		#region Implementation
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { M1ADocARInvoice.New(invoice, Factory), };
		}

		ARInvoice invoice;
		M1ADocARInvoice wrapper;
		protected override void SetUp()
		{
			invoice = Factory.NewWithValidTestData<ARInvoice>();
			wrapper = M1ADocARInvoice.New(invoice, Factory);
			base.SetUp();
		}
		#endregion
	}
}
