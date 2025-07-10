using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.MFI.DocWrappers.Testing
{
	public class MFIPrintStatementDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestGetDocBusinessObjects()
		{
			DocumentWrapper[] wrapperArray = DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Statement, null);
			AssertEquals("DocBusinessObjects Length", 1, wrapperArray.Length);
			AssertEquals("DocBusinessObjects type should be of DocMFIPrintStatement", typeof(DocMFIPrintStatement), wrapperArray[0].GetType());
			AssertNull("Return base DocBusinessObjects", DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Shipment, null));
		}

		public void TestAdditionalUserVisibleRegistryItems()
		{
			AssertNotNull("PrintStatement should not be null", PrintStatement);
			AssertNotNull("Document Supporter should not be null", DocumentSupporter);
			AssertNotNull(ClientOverride.Instance.AdditionalRegistryItemSet);
		}

		public void TestSupportedDataContexts()
		{
			AssertEquals("Core.Constants.DataContext.Statement is Supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.Statement)));
			DocumentWrapper[] wrapperArray = DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Statement, null);
			AssertEquals("Document Supporter BusinessObjects", 1, wrapperArray.Length);
		}

		public void TestMenuTemplateFilterValuesForPrintStandard()
		{
			GlbCompany.CurrentCompany.GC_Code = "BOB";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			Factory.Save();
			ZString result = DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintStandard, new DocumentWrapperTester());
			AssertEquals("Filter result for printing Standard Document", "Y", result);
			GlbCompany.CurrentCompany.GC_Code = "AKL";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "NZAKL";
			Factory.Save();
			result = DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintStandard, new DocumentWrapperTester());
			AssertEquals(GlbBranch.CurrentBranch.Country.Code, Core.Constants.CountryCodes.NewZealand);
			AssertEquals("Filter result for printing Standard Document", "N", result);
		}

		public void TestMenuTemplateFilterValuesForPrintClientSpecific()
		{
			GlbCompany.CurrentCompany.GC_Code = "BOB";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			Factory.Save();
			ZString result = DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintClientSpecific, new DocumentWrapperTester());
			AssertEquals("Filter result for printing Client Specifc Document", "N", result);
			GlbCompany.CurrentCompany.GC_Code = "AKL";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "NZAKL";
			Factory.Save();
			result = DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintClientSpecific, new DocumentWrapperTester());
			AssertEquals(GlbBranch.CurrentBranch.Country.Code, Core.Constants.CountryCodes.NewZealand);
			AssertEquals("Filter result for printing Client Specifc Document", "Y", result);
		}

		#region Implementation
		PrintStatementDocumentSupporter DocumentSupporter;
		PrintStatement PrintStatement;
		protected override void SetUp()
		{
			PrintStatement = new PrintStatement(Factory, GlbBranch.CurrentBranch);
			PrintStatement.CutOffDate = ZDateTime.Today.AddDays(1);
			base.SetUp();
			DocumentSupporter = PrintStatementDocumentSupporter.New(PrintStatement);
		}
		#endregion
	}
}
