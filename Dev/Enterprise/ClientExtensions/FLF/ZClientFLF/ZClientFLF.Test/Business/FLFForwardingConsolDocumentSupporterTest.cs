using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.FLF.Testing;
using Enterprise.DocumentEngineCore;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.FLF
{
	public class FLFForwardingConsolDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestPrintClientSpecific()
		{
			menuItem.SU_MenuName = FLFConstants.MenuNames.AWBBarcodeLabel;
			Assert("Print client specific document", documentSupporter.PrintClientSpecific);
			menuItem.SU_MenuName = "blah";
			Assert("Print client specific document", !documentSupporter.PrintClientSpecific);
		}

		public void TestMenuTemplateFilterValues()
		{
			ZString resultForStandard;
			ZString resultForClientSpecific;
			menuItem.SU_MenuName = "blah";
			resultForStandard = documentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintStandard, new DummyWrapper());
			resultForClientSpecific = documentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintClientSpecific, new DummyWrapper());
			AssertEquals("Filter result for printing Standard document", ZBool.True.ToString(), resultForStandard);
			AssertEquals("Filter result for printing Client Specifc document", ZBool.False.ToString(), resultForClientSpecific);
			menuItem.SU_MenuName = FLFConstants.MenuNames.AWBBarcodeLabel;
			resultForStandard = documentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintStandard, new DummyWrapper());
			resultForClientSpecific = documentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintClientSpecific, new DummyWrapper());
			AssertEquals("Filter result for printing Standard document", ZBool.False.ToString(), resultForStandard);
			AssertEquals("Filter result for printing Client Specifc document", ZBool.True.ToString(), resultForClientSpecific);
		}

		#region Implementation
		StmMenuItem menuItem;
		FLFForwardingConsolDocumentSupporter documentSupporter;
		FLFForwardingConsol consol;
		protected override void SetUp()
		{
			consol = Factory.New<FLFForwardingConsol>();
			documentSupporter = new FLFForwardingConsolDocumentSupporter(consol);
			menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			documentSupporter.currentCommand = menuItem;
			base.SetUp();
		}
		#endregion
	}
}
