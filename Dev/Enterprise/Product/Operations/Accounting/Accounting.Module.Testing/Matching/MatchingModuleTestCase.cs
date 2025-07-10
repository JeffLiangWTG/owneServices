using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Unmatching;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class MatchingModuleTestCase : ZModuleBasherTest
	{
		protected MatchingModule fMatchingModule;

		protected override void TearDown()
		{
			if (fMatchingModule != null)
			{
				fMatchingModule.Dispose();
			}
			base.TearDown();
		}

		protected OrgHeader TestOrg;
		protected OrgHeader TestOrg2;

		#region TestPerformSearch

		public void TestPerformSearch()
		{
			ARInvoice testARINV = Factory.NewWithValidTestData<ARInvoice>();
			TransactionMatchLink matchlinkA = ((IMatching)testARINV).CurrentMatchGroup.AddNew();
			matchlinkA.AP_AH = testARINV.PK;
			matchlinkA.AP_MatchGroupNum = "M00001900";

			APInvoice testAPINV = Factory.NewWithValidTestData<APInvoice>();
			TransactionMatchLink matchlinkB = ((IMatching)testARINV).CurrentMatchGroup.AddNew();
			matchlinkB.AP_AH = testAPINV.PK;
			matchlinkB.AP_MatchGroupNum = "M00001900";
			TestObjectCreator.SetupMatchLinkMatchDate(testARINV);

			Factory.Save();

			using (fMatchingModule = (MatchingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				fMatchingModule.PerformSearch_ForTest();
				AssertEquals("Grid Collection should contain 1 element", 1,
					((IFilterGridModuleInternalsForTesting)fMatchingModule).GridCollection.Count);
			}
		}

		#endregion

		#region TestUpdateMatchedTransactionGrid

		public void TestUpdateMatchedTransactionGrid()
		{
			using (fMatchingModule = (MatchingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				UnmatchingRow testUnmatching = new UnmatchingRow(Factory);
				testUnmatching.MatchGroupNum = "M00001900";

				ARInvoice testARINV = Factory.NewWithValidTestData<ARInvoice>();
				TransactionMatchLink matchlinkA = ((IMatching)testARINV).CurrentMatchGroup.AddNew();
				matchlinkA.AP_AH = testARINV.PK;
				matchlinkA.AP_MatchGroupNum = "M00001900";

				APInvoice testAPINV = Factory.NewWithValidTestData<APInvoice>();
				TransactionMatchLink matchlinkB = ((IMatching)testARINV).CurrentMatchGroup.AddNew();
				matchlinkB.AP_AH = testAPINV.PK;
				matchlinkB.AP_MatchGroupNum = "M00001900";
				TestObjectCreator.SetupMatchLinkMatchDate(testARINV);

				Factory.Save();

				fMatchingModule.GridCollection.Add(testUnmatching);
				var testFilterBizO = (MatchingBaseFilterBusinessObject)((IFilterGridModuleInternalsForTesting)fMatchingModule).FilterBusinessObject;

				fMatchingModule.UpdateMatchedTransactionGridInFilterControl_ForTestOnly();

				AssertEquals("Should be 2 rows in MatchedTransactionsGrid", 2, testFilterBizO.TransactionHeaders.Count);
				Assert("One row should be TestARINV", testFilterBizO.TransactionHeaders.Contains(testARINV));
				Assert("One row should be TestAPINV", testFilterBizO.TransactionHeaders.Contains(testAPINV));

				((BusinessObjectCollection)fMatchingModule.GridCollection).RemoveAll();
				fMatchingModule.UpdateMatchedTransactionGridInFilterControl_ForTestOnly();

				AssertEquals("Should be no rows in MatchedTransactionsGrid", 0, testFilterBizO.TransactionHeaders.Count);
			}
		}

		#endregion

		#region TestGetNewController

		public void TestGetNewController()
		{
			using (fMatchingModule = (MatchingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				UnmatchingRow testUnmatching = new UnmatchingRow(Factory);

				ZController newController = fMatchingModule.GetNewController_ForTestOnly(testUnmatching);
				Assert("NewController should be a matching controller", newController is MatchingController);
			}
		}

		#endregion

		#region TestGetCollectionLoadCount

		protected void LoadCollection_Exposed()
		{
			if (fMatchingModule != null)
			{
				fMatchingModule.LoadCollection_ForTestOnly(fMatchingModule.GridCollection, new ZQuery());
			}
		}

		#region SetupTestData

		protected void SetupTestData()
		{
			fMatchingModule = (MatchingModule)ZModuleFactory.Instance.Create(GetModuleID());

			TestOrg = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg2 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			// Group 1
			ARInvoice testARInv1 = Factory.NewWithValidTestData<ARInvoice>();
			testARInv1.AH_OH = TestOrg.PK;
			TransactionMatchLink matchlink1 = ((IMatching)testARInv1).CurrentMatchGroup.AddNew();
			matchlink1.AP_AH = testARInv1.PK;
			matchlink1.AP_MatchGroupNum = "M00001334";

			APInvoice testAPInv2 = Factory.NewWithValidTestData<APInvoice>();
			testAPInv2.AH_OH = TestOrg.PK;
			TransactionMatchLink matchlink2 = ((IMatching)testARInv1).CurrentMatchGroup.AddNew();
			matchlink2.AP_AH = testAPInv2.PK;
			matchlink2.AP_MatchGroupNum = "M00001334";
			TestObjectCreator.SetupMatchLinkMatchDate(testARInv1);

			// Group 2
			ARInvoice testARInv3 = Factory.NewWithValidTestData<ARInvoice>();
			testARInv3.AH_OH = TestOrg.PK;
			TransactionMatchLink matchlink3 = ((IMatching)testARInv3).CurrentMatchGroup.AddNew();
			matchlink3.AP_AH = testARInv3.PK;
			matchlink3.AP_MatchGroupNum = "M00001442";

			ARCreditNote testARCrd4 = Factory.NewWithValidTestData<ARCreditNote>();
			testARCrd4.AH_OH = TestOrg2.PK;
			TransactionMatchLink matchlink4 = ((IMatching)testARInv3).CurrentMatchGroup.AddNew();
			matchlink4.AP_AH = testARCrd4.PK;
			matchlink4.AP_MatchGroupNum = "M00001442";
			TestObjectCreator.SetupMatchLinkMatchDate(testARInv3);

			// Group 3
			APInvoice testAPInv5 = Factory.NewWithValidTestData<APInvoice>();
			testAPInv5.AH_OH = TestOrg.PK;
			TransactionMatchLink matchlink5 = ((IMatching)testAPInv5).CurrentMatchGroup.AddNew();
			matchlink5.AP_AH = testAPInv5.PK;
			matchlink5.AP_MatchGroupNum = "M00001523";

			APCreditNote testAPCrd6 = Factory.NewWithValidTestData<APCreditNote>();
			testAPCrd6.AH_OH = TestOrg.PK;
			TransactionMatchLink matchlink6 = ((IMatching)testAPInv5).CurrentMatchGroup.AddNew();
			matchlink6.AP_AH = testAPCrd6.PK;
			matchlink6.AP_MatchGroupNum = "M00001523";
			TestObjectCreator.SetupMatchLinkMatchDate(testAPInv5);

			// Group 4
			APInvoice testAPInv7 = Factory.NewWithValidTestData<APInvoice>();
			testAPInv7.AH_OH = TestOrg.PK;
			TransactionMatchLink matchlink7 = ((IMatching)testAPInv7).CurrentMatchGroup.AddNew();
			matchlink7.AP_AH = testAPInv7.PK;
			matchlink7.AP_MatchGroupNum = "M00009332";

			APAdjustmentNote testAPAdj8 = Factory.NewWithValidTestData<APAdjustmentNote>();
			testAPAdj8.AH_OH = TestOrg2.PK;
			TransactionMatchLink matchlink8 = ((IMatching)testAPInv7).CurrentMatchGroup.AddNew();
			matchlink8.AP_AH = testAPAdj8.PK;
			matchlink8.AP_MatchGroupNum = "M00009332";
			TestObjectCreator.SetupMatchLinkMatchDate(testAPInv7);

			Factory.Save();
		}

		#endregion

		#endregion

		#region TestFilterHelperMaximumRows

		public void TestFilterHelperMaximumRows()
		{
			ARInvoice testARINV = Factory.NewWithValidTestData<ARInvoice>();
			TransactionMatchLink matchlinkA = ((IMatching)testARINV).CurrentMatchGroup.AddNew();
			matchlinkA.AP_AH = testARINV.PK;
			matchlinkA.AP_MatchGroupNum = "M00001900";

			APInvoice testAPINV = Factory.NewWithValidTestData<APInvoice>();
			TransactionMatchLink matchlinkB = ((IMatching)testARINV).CurrentMatchGroup.AddNew();
			matchlinkB.AP_AH = testAPINV.PK;
			matchlinkB.AP_MatchGroupNum = "M00001900";
			TestObjectCreator.SetupMatchLinkMatchDate(testARINV);

			Factory.Save();

			using (fMatchingModule = (MatchingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				UnmatchingRowCollection collection = (UnmatchingRowCollection)fMatchingModule.GridCollection;
				ZQuery filter = new ZQuery();

				fMatchingModule.LoadCollection_ForTestOnly(collection, filter);
				AssertEquals("Count", 1, collection.Count);

				filter.MaximumRows = 0;
				fMatchingModule.LoadCollection_ForTestOnly(collection, filter);
				AssertEquals("Count", 0, collection.Count);
			}
		}

		public void TestDeleteMenuItemText()
		{
			using (fMatchingModule = (MatchingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(fMatchingModule.GetDeleteMenuItemText_ForTestOnly().Caption, "Unmatch");
				AssertEquals(fMatchingModule.GetDeleteMenuItemText_ForTestOnly().FullDescription, "Un-matches the selected item to revert the transactions to outstanding status (shortcut Del)");
			}
		}

		#endregion
	}
}
