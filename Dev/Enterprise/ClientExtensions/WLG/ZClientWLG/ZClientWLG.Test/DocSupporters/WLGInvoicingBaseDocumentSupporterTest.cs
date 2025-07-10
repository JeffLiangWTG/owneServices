using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.WLG.Testing
{
	class WLGInvoicingBaseDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestClientOverride()
		{
			AssertEquals("Client's InvoicingBase Doc Supporter Override", typeof(WLGInvoicingBaseDocumentSupporter), InvoicingBaseDocumentSupporter.New(Invoice).GetType());
		}

		public void TestUseClientSpecific()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "CNSHA";
			MenuItem.SU_MenuName = "Invoice";
			AssertEquals("Filter result for printing Standard Invoice", ZBool.False, DocumentSupporter.UseClientSpecific);
			MenuItem.SU_MenuName = "Class A Invoice";
			AssertEquals("Filter result for printing Standard Class A Invoice", ZBool.False, DocumentSupporter.UseClientSpecific);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "CNCAN";
			MenuItem.SU_MenuName = "Invoice";
			AssertEquals("Filter result for printing Standard Invoice", ZBool.False, DocumentSupporter.UseClientSpecific);
			MenuItem.SU_MenuName = "Class A Invoice";
			AssertEquals("Filter result for printing Standard Class A Invoice", ZBool.True, DocumentSupporter.UseClientSpecific);
		}

		public void TestMenuTemplateFilterValuesForPrintStandardInvoice()
		{
			ZString result = ZString.Empty;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "CNCAN";
			MenuItem.SU_MenuName = "Invoice";
			result = DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintStandardInvoice, DocWLGARInvoice.New(Invoice, Factory));
			AssertEquals("Filter result for printing Standard Invoice", ZBool.True.ToString(), result);
			MenuItem.SU_MenuName = "Class A Invoice";
			result = DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintStandardInvoice, DocWLGARInvoice.New(Invoice, Factory));
			AssertEquals("Filter result for printing Standard Class A Invoice", ZBool.False.ToString(), result);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "CNSHA";
			MenuItem.SU_MenuName = "Invoice";
			result = DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintStandardInvoice, DocWLGARInvoice.New(Invoice, Factory));
			AssertEquals("Filter result for printing Standard Invoice", ZBool.True.ToString(), result);
			MenuItem.SU_MenuName = "Class A Invoice";
			result = DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintStandardInvoice, DocWLGARInvoice.New(Invoice, Factory));
			AssertEquals("Filter result for printing Standard Class A Invoice", ZBool.True.ToString(), result);
		}

		public void TestMenuTemplateFilterValuesForPrintClientSpecificInvoice()
		{
			ZString result = ZString.Empty;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "CNCAN";
			MenuItem.SU_MenuName = "Invoice";
			result = DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintClientSpecificInvoice, DocWLGARInvoice.New(Invoice, Factory));
			AssertEquals("Filter result for printing Standard Invoice", ZBool.False.ToString(), result);
			MenuItem.SU_MenuName = "Class A Invoice";
			result = DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintClientSpecificInvoice, DocWLGARInvoice.New(Invoice, Factory));
			AssertEquals("Filter result for printing Standard Class A Invoice", ZBool.True.ToString(), result);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "CNSHA";
			MenuItem.SU_MenuName = "Invoice";
			result = DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintClientSpecificInvoice, DocWLGARInvoice.New(Invoice, Factory));
			AssertEquals("Filter result for printing Standard Invoice", ZBool.False.ToString(), result);
			MenuItem.SU_MenuName = "Class A Invoice";
			result = DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintClientSpecificInvoice, DocWLGARInvoice.New(Invoice, Factory));
			AssertEquals("Filter result for printing Standard Class A Invoice", ZBool.False.ToString(), result);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			Invoice = Factory.New<ARInvoice>();
			AssertNotNull("Invoice should not be null", Invoice);
			DocumentSupporter = (WLGInvoicingBaseDocumentSupporter)WLGInvoicingBaseDocumentSupporter.New(Invoice);
			MenuItem = Factory.NewWithValidTestData<StmMenuItem>();
			DocumentSupporter.CurrentCommand = MenuItem;
			AssertNotNull("Document Supporter should not be null", DocumentSupporter);
		}

		WLGInvoicingBaseDocumentSupporter DocumentSupporter;
		StmMenuItem MenuItem;
		ARInvoice Invoice;
		#endregion
	}
}
