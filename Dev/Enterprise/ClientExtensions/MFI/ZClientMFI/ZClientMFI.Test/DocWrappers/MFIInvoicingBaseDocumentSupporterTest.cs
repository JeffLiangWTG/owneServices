using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.MFI.DocWrappers.Testing
{
	public class MFIInvoicingBaseDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestGetDocBusinessObjects()
		{
			DocumentWrapper[] wrapperArray = DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ARInvoice, null);
			AssertEquals("DocBusinessObjects Length", 1, wrapperArray.Length);
			AssertEquals("DocBusinessObjects type should be of DocMFIARInvoice", typeof(DocMFIARInvoice), wrapperArray[0].GetType());
			AssertNull("Return base DocBusinessObjects", DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Shipment, null));
		}

		public void TestMenuTemplateFilterValuesForPrintStandard()
		{
			GlbCompany.CurrentCompany.GC_Code = "BOB";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			Factory.Save();
			ZString result = DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintStandardInvoice, new DocumentWrapperTester());
			AssertEquals("Filter result for printing Standard Document", "Y", result);
			GlbCompany.CurrentCompany.GC_Code = "AKL";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "NZAKL";
			Factory.Save();
			result = DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintStandardInvoice, new DocumentWrapperTester());
			AssertEquals(GlbBranch.CurrentBranch.Country.Code, Core.Constants.CountryCodes.NewZealand);
			AssertEquals("Filter result for printing Standard Document", "N", result);
		}

		public void TestMenuTemplateFilterValuesForPrintClientSpecific()
		{
			GlbCompany.CurrentCompany.GC_Code = "BOB";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			Factory.Save();
			ZString result = DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintClientSpecificInvoice, new DocumentWrapperTester());
			AssertEquals("Filter result for printing Client Specifc Document", "N", result);
			GlbCompany.CurrentCompany.GC_Code = "AKL";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "NZAKL";
			Factory.Save();
			result = DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintClientSpecificInvoice, new DocumentWrapperTester());
			AssertEquals(GlbBranch.CurrentBranch.Country.Code, Core.Constants.CountryCodes.NewZealand);
			AssertEquals("Filter result for printing Client Specifc Document", "Y", result);
		}

		public void TestAdditionalUserVisibleRegistryItems()
		{
			AssertNotNull("Invoice should not be null", Invoice);
			AssertNotNull("Document Supporter should not be null", DocumentSupporter);
			AssertNotNull(ClientOverride.Instance.AdditionalRegistryItemSet);
		}

		public void TestSupportedDataContexts()
		{
			AssertEquals("Core.Constants.DataContext.ARInvoice is Supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.ARInvoice)));
			DocumentWrapper[] wrapperArray = DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ARInvoice, null);
			AssertEquals("Document Supporter BusinessObjects", 1, wrapperArray.Length);
		}

		#region Implementation
		InvoicingBaseDocumentSupporter DocumentSupporter;
		ARInvoice Invoice;
		protected override void SetUp()
		{
			base.SetUp();
			Invoice = Factory.New<ARInvoice>();
			DocumentSupporter = InvoicingBaseDocumentSupporter.New(Invoice);
		}
		#endregion
	}
}
