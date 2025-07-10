using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using AccConstants = Enterprise.Accounting.Business.AccountingUtils;

namespace Enterprise.Accounting.Business.Base.Unmatching.Testing
{
	[TestedType(typeof(UnmatchingRowCollection))]
	public class UnmatchingRowCollectionTestCase : NonPersistentBusinessObjectCollectionTestCase<UnmatchingRowCollection>
	{
		#region Implementation

		protected override UnmatchingRowCollection GetCollectionToTest()
		{
			return new UnmatchingRowCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new UnmatchingRow(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			UnmatchingRows = new UnmatchingRowCollection(Factory);
		}

		protected UnmatchingRowCollection UnmatchingRows;

		protected OrgHeader TestOrg2;

		protected ARInvoice ARINV1;
		protected ARInvoice ARINV2;
		protected ARInvoice ARINV3;

		protected ARCreditNote ARCRD1;
		protected ARCreditNote ARCRD2;
		protected ARCreditNote ARCRD3;

		protected TransactionMatchLink Matchlink1;
		protected TransactionMatchLink Matchlink2;
		protected TransactionMatchLink Matchlink3;
		protected TransactionMatchLink Matchlink4;
		protected TransactionMatchLink Matchlink5;
		protected TransactionMatchLink Matchlink6;

		class UnmatchingRowCollection_ForTest : UnmatchingRowCollection
		{
			public UnmatchingRowCollection_ForTest(BusinessObjectFactory factory) : base(factory)
			{ }

			public int CallTimesForCountChanged => callTimesForCountChanged;
			int callTimesForCountChanged;

			protected override DisposableList GetAdditionalListChangedSuspenders()
			{
				callTimesForCountChanged++;
				return base.GetAdditionalListChangedSuspenders();
			}
		}

		#endregion

		#region SetupDataSet

		protected void SetupDataSet()
		{
			ARINV1 = Factory.New<ARInvoice>();
			ARINV1.AH_TransactionNum = "00001000";
			ARINV1.IsManuallySetTransactionNumber_ForTestOnly = true;

			ARINV2 = Factory.New<ARInvoice>();
			ARINV2.AH_TransactionNum = "00001001";
			ARINV2.IsManuallySetTransactionNumber_ForTestOnly = true;

			ARINV3 = Factory.New<ARInvoice>();
			ARINV3.AH_TransactionNum = "00001002";
			ARINV3.IsManuallySetTransactionNumber_ForTestOnly = true;

			ARCRD1 = Factory.New<ARCreditNote>();
			ARCRD1.AH_TransactionNum = "00001001";
			ARCRD1.IsManuallySetTransactionNumber_ForTestOnly = true;

			ARCRD2 = Factory.New<ARCreditNote>();
			ARCRD2.AH_TransactionNum = "00001003";
			ARCRD2.IsManuallySetTransactionNumber_ForTestOnly = true;

			ARCRD3 = Factory.New<ARCreditNote>();
			ARCRD3.AH_TransactionNum = "00001004";
			ARCRD3.IsManuallySetTransactionNumber_ForTestOnly = true;

			Factory.Save();

			Matchlink1 = ((IMatching)ARINV1).CurrentMatchGroup.AddNew();
			Matchlink1.AP_AH = ARINV1.PK;
			Matchlink1.AP_MatchGroupNum = "M00001000";
			Matchlink2 = ((IMatching)ARINV1).CurrentMatchGroup.AddNew();
			Matchlink2.AP_AH = ARINV2.PK;
			Matchlink2.AP_MatchGroupNum = "M00001000";
			Matchlink3 = ((IMatching)ARINV1).CurrentMatchGroup.AddNew();
			Matchlink3.AP_AH = ARINV3.PK;
			Matchlink3.AP_MatchGroupNum = "M00001001";
			Matchlink4 = ((IMatching)ARINV1).CurrentMatchGroup.AddNew();
			Matchlink4.AP_AH = ARCRD1.PK;
			Matchlink4.AP_MatchGroupNum = "M00001001";
			Matchlink5 = ((IMatching)ARINV1).CurrentMatchGroup.AddNew();
			Matchlink5.AP_AH = ARCRD2.PK;
			Matchlink5.AP_MatchGroupNum = "M00001002";
			Matchlink6 = ((IMatching)ARINV1).CurrentMatchGroup.AddNew();
			Matchlink6.AP_AH = ARCRD3.PK;
			Matchlink6.AP_MatchGroupNum = "M00001002";
			TestObjectCreator.SetupMatchLinkMatchDate(ARINV1);
		}

		#endregion

		#region TestLoadingFromFilterString

		public void TestLoadingFromFilterString()
		{
			SetupDataSet();
			Factory.Save();

			MatchGroupFilterHelper filterHelper = new ARMatchGroupFilterHelper(Factory);
			UnmatchingRows.SetFilterHelper(filterHelper);
			UnmatchingRows.Load();

			AssertEquals("There should be 3 match groups in the collection", 3,
				UnmatchingRows.Count);
			filterHelper.NumberType = AccConstants.NumberFilterTypes.TransactionNumber;
			filterHelper.NumberFilter = "00001001";

			UnmatchingRows.SetFilterHelper(filterHelper);
			UnmatchingRows.Load();
			AssertEquals("There should be 2 match groups in the collection", 2,
				UnmatchingRows.Count);

			filterHelper.NumberFilter = "00001004";
			UnmatchingRows.SetFilterHelper(filterHelper);
			UnmatchingRows.Load();
			AssertEquals("There should be 1 match group in the collection", 1,
				UnmatchingRows.Count);
			AssertEquals("The match group number should be M00001002", "M00001002",
				UnmatchingRows[0].MatchGroupNum);
		}

		public void TestSuspendListChangedWhenLoadUnmatchingRowCollection()
		{
			SetupDataSet();
			Factory.Save();

			MatchGroupFilterHelper filterHelper = new ARMatchGroupFilterHelper(Factory);
			var unmatchingRows = new UnmatchingRowCollection_ForTest(Factory);
			AssertEquals("Pre-condition: The callTimesForCountChanged should be 0 before collection load.", 0, unmatchingRows.CallTimesForCountChanged);
			unmatchingRows.SetFilterHelper(filterHelper);
			unmatchingRows.Load();

			AssertEquals("The callTimesForCountChanged should be 1 as the suspender was called.", 1, unmatchingRows.CallTimesForCountChanged);
		}

		#endregion

		#region TestSynchroniseFilterHelper

		public void TestSynchroniseFilterHelper()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader testOrg2 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			// Group 1
			ARInvoice testARInv1 = Factory.NewWithValidTestData<ARInvoice>();
			testARInv1.AH_OH = testOrg.PK;
			TransactionMatchLink matchlink1 = ((IMatching)testARInv1).CurrentMatchGroup.AddNew();
			matchlink1.AP_AH = testARInv1.PK;
			matchlink1.AP_MatchGroupNum = "M00001334";

			APInvoice testAPInv2 = Factory.NewWithValidTestData<APInvoice>();
			testAPInv2.AH_OH = testOrg.PK;
			TransactionMatchLink matchlink2 = ((IMatching)testARInv1).CurrentMatchGroup.AddNew();
			matchlink2.AP_AH = testAPInv2.PK;
			matchlink2.AP_MatchGroupNum = "M00001334";

			// Group 2
			ARInvoice testARInv3 = Factory.NewWithValidTestData<ARInvoice>();
			testARInv3.AH_OH = testOrg.PK;
			TransactionMatchLink matchlink3 = ((IMatching)testARInv1).CurrentMatchGroup.AddNew();
			matchlink3.AP_AH = testARInv3.PK;
			matchlink3.AP_MatchGroupNum = "M00001442";

			ARCreditNote testARCrd4 = Factory.NewWithValidTestData<ARCreditNote>();
			testARCrd4.AH_OH = testOrg2.PK;
			TransactionMatchLink matchlink4 = ((IMatching)testARInv1).CurrentMatchGroup.AddNew();
			matchlink4.AP_AH = testARCrd4.PK;
			matchlink4.AP_MatchGroupNum = "M00001442";

			// Group 3
			APInvoice testAPInv5 = Factory.NewWithValidTestData<APInvoice>();
			testAPInv5.AH_OH = testOrg.PK;
			TransactionMatchLink matchlink5 = ((IMatching)testARInv1).CurrentMatchGroup.AddNew();
			matchlink5.AP_AH = testAPInv5.PK;
			matchlink5.AP_MatchGroupNum = "M00001523";

			APCreditNote testAPCrd6 = Factory.NewWithValidTestData<APCreditNote>();
			testAPCrd6.AH_OH = testOrg.PK;
			TransactionMatchLink matchlink6 = ((IMatching)testARInv1).CurrentMatchGroup.AddNew();
			matchlink6.AP_AH = testAPCrd6.PK;
			matchlink6.AP_MatchGroupNum = "M00001523";

			TestObjectCreator.SetupMatchLinkMatchDate(testARInv1);

			Factory.Save();

			ARMatchGroupFilterHelper aRFilterHelper = new ARMatchGroupFilterHelper(Factory);

			UnmatchingRows.SetFilterHelper(aRFilterHelper);
			UnmatchingRows.Load(new ZQuery());

			AssertEquals("There should be 2 elements in the collection", 2, UnmatchingRows.Count);

			aRFilterHelper.Organisation = testOrg2.PK;

			UnmatchingRows.SetFilterHelper(aRFilterHelper);
			UnmatchingRows.Load(new ZQuery());

			AssertEquals("There should be 1 element in the collection since TestOrg2 only appears in the AR transactions for M00001442",
				1, UnmatchingRows.Count);
			Assert("Collections should contain M00001442", UnmatchingRows.ContainsMatchGroupNumber("M00001442"));
		}

		#endregion

		#region TestAfterLoaded Event

		public void TestAfterLoadedEvent()
		{
			ARMatchGroupFilterHelper aRFilterHelper = new ARMatchGroupFilterHelper(Factory);
			UnmatchingRows.SetFilterHelper(aRFilterHelper);

			AssertEquals("Pre-condition", false, eventFired);
			UnmatchingRows.AfterLoaded += AfterLoadedHandler;
			UnmatchingRows.Load();
			AssertEquals("EvenHandler should be called", true, eventFired);
			UnmatchingRows.AfterLoaded -= AfterLoadedHandler;
		}

		void AfterLoadedHandler(object source, EventArgs args)
		{
			eventFired = true;
		}
		bool eventFired;

		#endregion
	}
}
