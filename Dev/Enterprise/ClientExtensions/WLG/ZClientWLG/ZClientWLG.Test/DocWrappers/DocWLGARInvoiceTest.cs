using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.WLG.Testing
{
	[TestedType(typeof(DocWLGARInvoice))]
	class DocWLGARInvoiceTest : DocumentWrapperTestCase
	{
		public void TestClientOverride()
		{
			AssertEquals("Client's Doc Wrapper Override", typeof(DocWLGARInvoice), DocARInvoice.New(invoice, Factory).GetType());
		}

		public void TestPrintStandard()
		{
			AssertNotNull("Document supporter", invoice.DocumentSupporter);
			AssertEquals("Document supporter type", typeof(WLGInvoicingBaseDocumentSupporter), invoice.DocumentSupporter.GetType());
			((WLGInvoicingBaseDocumentSupporter)invoice.DocumentSupporter).CurrentCommand = MenuItem;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "CNSHA";
			MenuItem.SU_MenuName = "Invoice";
			AssertEquals("Filter result for printing Standard Invoice", ZBool.True, wrapper.PrintStandard);
			MenuItem.SU_MenuName = "Class A Invoice";
			AssertEquals("Filter result for printing Standard Class A Invoice", ZBool.True, wrapper.PrintStandard);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "CNCAN";
			MenuItem.SU_MenuName = "Invoice";
			AssertEquals("Filter result for printing Standard Invoice", ZBool.True, wrapper.PrintStandard);
			MenuItem.SU_MenuName = "Class A Invoice";
			AssertEquals("Filter result for printing Standard Class A Invoice", ZBool.False, wrapper.PrintStandard);
		}

		public void TestPrintClientSpecific()
		{
			AssertNotNull("Document supporter", invoice.DocumentSupporter);
			AssertEquals("Document supporter type", typeof(WLGInvoicingBaseDocumentSupporter), invoice.DocumentSupporter.GetType());
			((WLGInvoicingBaseDocumentSupporter)invoice.DocumentSupporter).CurrentCommand = MenuItem;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "CNSHA";
			MenuItem.SU_MenuName = "Invoice";
			AssertEquals("Filter result for printing Standard Invoice", ZBool.False, wrapper.PrintClientSpecific);
			MenuItem.SU_MenuName = "Class A Invoice";
			AssertEquals("Filter result for printing Standard Class A Invoice", ZBool.False, wrapper.PrintClientSpecific);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "CNCAN";
			MenuItem.SU_MenuName = "Invoice";
			AssertEquals("Filter result for printing Standard Invoice", ZBool.False, wrapper.PrintClientSpecific);
			MenuItem.SU_MenuName = "Class A Invoice";
			AssertEquals("Filter result for printing Standard Class A Invoice", ZBool.True, wrapper.PrintClientSpecific);
		}

		#region Implementation
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocWLGARInvoice.New(invoice, Factory) };
		}

		StmMenuItem MenuItem;
		ARInvoice invoice;
		DocWLGARInvoice wrapper;
		protected override void SetUp()
		{
			MenuItem = Factory.NewWithValidTestData<StmMenuItem>();
			invoice = Factory.NewWithValidTestData<ARInvoice>();
			wrapper = DocWLGARInvoice.New(invoice, Factory);
			base.SetUp();
		}
		#endregion
	}
}
