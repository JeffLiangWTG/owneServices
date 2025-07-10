using System;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Unmatching.Testing
{
	[TestedType(typeof(UnmatchingRow))]
	public class UnmatchingRowTestCase_OriginalLogic : UnmatchingRowTestCase
	{
	}

	[TestedType(typeof(UnmatchingRow))]
	public class UnmatchingRowTestCase_EnableNewOSOutstandingAmountFeature : UnmatchingRowTestCase
	{
		protected override void SetUp()
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			base.SetUp();
		}

		protected override void AssertAH_IsOSOutstandingAmountApplicable(TransactionHeader transactionHeader)
		{
			Assert("AH_IsOSOutstandingAmountApplicable is always true if New OS OutstandingAmount feature is enabled", transactionHeader.AH_IsOSOutstandingAmountApplicable);
			AssertNotEquals(0m, transactionHeader.LatestMatchLink.AP_OSAmount);
		}

		protected override void SetUpOSFeatureAmount(TransactionMatchLink matchLink)
		{
			matchLink.AP_OSAmount = 5;
		}
	}

	[TestedType(typeof(UnmatchingRow))]
	public abstract class UnmatchingRowTestCase : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UnmatchingRow(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestUnmatchingRow = (UnmatchingRow)CachedBusinessObject;
		}

		protected UnmatchingRow TestUnmatchingRow;

		protected OrgHeader TestOrg1;
		protected OrgHeader TestOrg2;

		protected ARInvoice ARINV1;
		protected ARInvoice ARINV2;
		protected ARReceipt ARREC1;

		protected ARCreditNote ARCRD1;

		protected void SetupDataSet1()
		{
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			ARINV1 = Factory.NewWithValidTestData<ARInvoice>();
			ARINV2 = Factory.NewWithValidTestData<ARInvoice>();

			ARREC1 = Factory.NewWithValidTestData<ARReceipt>();
			ARCRD1 = Factory.NewWithValidTestData<ARCreditNote>();
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		#endregion

		#region TestMatchLinks

		public void TestMatchLinks()
		{
			TestUnmatchingRow.MatchGroupNum = "M00001111";

			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();

			TransactionMatchLink matchLinkA = GenerateMatchLinks_ForTestOnly(aPInv);
			matchLinkA.AP_MatchGroupNum = "M00001111";
			matchLinkA.AP_AH = aPInv.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(aPInv);

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();

			TransactionMatchLink matchLinkB = GenerateMatchLinks_ForTestOnly(aRInv);
			matchLinkB.AP_MatchGroupNum = "M00001112";
			matchLinkB.AP_AH = aRInv.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(aRInv);

			Factory.Save();
			//TestUnmatchingRow.MatchLinks.Load();
			AssertEquals("There should be 1 matchlink in the collection", 1, TestUnmatchingRow.MatchLinks.Count);
			Assert("The match link should be MatchLinkA", TestUnmatchingRow.MatchLinks.Contains(matchLinkA));
		}

		#endregion

		#region TestUnmatchGroupWithPayment

		public void TestUnmatchGroupWithPayment()
		{
			TestUnmatchingRow.MatchGroupNum = "M00001488";

			APPayment aPPAY = Factory.NewWithValidTestData<APPayment>();
			APInvoice aPINV = Factory.NewWithValidTestData<APInvoice>();

			TransactionMatchLink aPPAYMatch = GenerateMatchLinks_ForTestOnly(aPPAY);
			aPPAYMatch.AP_AH = aPPAY.PK;
			aPPAYMatch.AP_MatchGroupNum = "M00001488";

			TransactionMatchLink aPINVMatch = GenerateMatchLinks_ForTestOnly(aPPAY);
			aPINVMatch.AP_AH = aPINV.PK;
			aPINVMatch.AP_MatchGroupNum = "M00001488";
			TestObjectCreator.SetupMatchLinkMatchDate(aPPAY);

			Factory.Save();

			bool oldPayablesSecurityValue = Env.Security.PayablesAllowUnmatchingPaymentMatching.IsAllowed;
			try
			{
				Env.Security.PayablesAllowUnmatchingPaymentMatching.IsAllowed = false;
				AssertEquals("Should not be able to unmatch since group contains a payment", UnmatchingResult.ContainsPayment,
					TestUnmatchingRow.CanUnmatchThisMatchGroup);

				Env.Security.PayablesAllowUnmatchingPaymentMatching.IsAllowed = true;
				AssertEquals("Should be able to unmatch though group contains a payment", UnmatchingResult.Success,
					TestUnmatchingRow.CanUnmatchThisMatchGroup);
			}
			finally
			{
				Env.Security.PayablesAllowUnmatchingPaymentMatching.IsAllowed = oldPayablesSecurityValue;
			}
		}

		#endregion

		public void TestUnmatchGroupWithAPJournal()
		{
			TestUnmatchingRow.MatchGroupNum = "M00001488";

			var aPJNL = Factory.NewWithValidTestData<APJournal>();
			var aPINV = Factory.NewWithValidTestData<APInvoice>();

			var aPPAYMatch = GenerateMatchLinks_ForTestOnly(aPJNL);
			aPPAYMatch.AP_AH = aPJNL.PK;
			aPPAYMatch.AP_MatchGroupNum = "M00001488";

			var aPINVMatch = GenerateMatchLinks_ForTestOnly(aPJNL);
			aPINVMatch.AP_AH = aPINV.PK;
			aPINVMatch.AP_MatchGroupNum = "M00001488";
			TestObjectCreator.SetupMatchLinkMatchDate(aPJNL);

			aPJNL.AH_TransactionCategory = Constants.TransactionCategory.Codes.CashAdvancePaid;
			Factory.Save();
			AssertEquals(UnmatchingResult.ContainsCashAdvanceAPJournal, TestUnmatchingRow.CanUnmatchThisMatchGroup);

			aPJNL.AH_TransactionCategory = Constants.TransactionCategory.Codes.CashAdvanceInvoice;
			Factory.Save();
			AssertEquals(UnmatchingResult.ContainsCashAdvanceAPJournal, TestUnmatchingRow.CanUnmatchThisMatchGroup);

			aPJNL.AH_TransactionCategory = ZString.Empty;
			Factory.Save();
			AssertEquals(UnmatchingResult.Success, TestUnmatchingRow.CanUnmatchThisMatchGroup);
		}

		public void TestUnmatchGroupWithBankFee()
		{
			TestUnmatchingRow.MatchGroupNum = "M00001488";

			var aPJNL = Factory.NewWithValidTestData<APJournal>();
			var aPINV = Factory.NewWithValidTestData<APInvoice>();

			aPJNL.AH_TransactionCreatedByMatching = true;
			aPJNL.AH_OSOutstandingAmount = 0;
			aPJNL.AH_OSTotal = 5;
			aPJNL.AH_InvoiceAmount = -5;
			aPJNL.AH_LocalOutstandingAmount = 5;
			var aPJNLMatch = GenerateMatchLinks_ForTestOnly(aPJNL);
			aPJNLMatch.AP_AH = aPJNL.PK;
			aPJNLMatch.AP_MatchGroupNum = "M00001488";
			aPJNLMatch.AP_Amount = -5;
			SetUpOSFeatureAmount(aPJNLMatch);

			var aPINVMatch = GenerateMatchLinks_ForTestOnly(aPINV);
			aPINVMatch.AP_AH = aPINV.PK;
			aPINVMatch.AP_MatchGroupNum = "M00001488";
			TestObjectCreator.SetupMatchLinkMatchDate(aPJNL);
			TestObjectCreator.SetupMatchLinkMatchDate(aPINV);
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			
			Assert("AP JNL InvoiceUnpaid is true", aPJNL.InvoiceUnpaid);
			AssertNoExceptionThrown(() => ((IMatching)aPJNL).Unmatch(aPJNLMatch.AP_Amount, aPJNLMatch.AP_OSAmount));
			AssertAH_IsOSOutstandingAmountApplicable(aPJNL);
		}

		protected virtual void SetUpOSFeatureAmount(TransactionMatchLink matchLink)
		{
			matchLink.AP_OSAmount = 0;
		}

		protected virtual void AssertAH_IsOSOutstandingAmountApplicable(TransactionHeader transactionHeader)
		{
			Assert("AH_IsOSOutstandingAmountApplicable is not set to true", !transactionHeader.AH_IsOSOutstandingAmountApplicable);
			AssertEquals(0m, transactionHeader.LatestMatchLink.AP_OSAmount);
		}

		public void TestUnmatchGroupWithARJournal()
		{
			TestUnmatchingRow.MatchGroupNum = "M00001488";

			var aRJNL = Factory.NewWithValidTestData<ARJournal>();
			var aRINV = Factory.NewWithValidTestData<ARInvoice>();

			var aRRECMatch = GenerateMatchLinks_ForTestOnly(aRJNL);
			aRRECMatch.AP_AH = aRJNL.PK;
			aRRECMatch.AP_MatchGroupNum = "M00001488";

			var aRINVMatch = GenerateMatchLinks_ForTestOnly(aRJNL);
			aRINVMatch.AP_AH = aRINV.PK;
			aRINVMatch.AP_MatchGroupNum = "M00001488";
			TestObjectCreator.SetupMatchLinkMatchDate(aRJNL);

			aRJNL.AH_TransactionCategory = Constants.TransactionCategory.Codes.CashAdvanceReceived;
			Factory.Save();
			AssertEquals(UnmatchingResult.ContainsCashAdvanceARJournal, TestUnmatchingRow.CanUnmatchThisMatchGroup);

			aRJNL.AH_TransactionCategory = Constants.TransactionCategory.Codes.CashAdvanceInvoice;
			Factory.Save();
			AssertEquals(UnmatchingResult.ContainsCashAdvanceARJournal, TestUnmatchingRow.CanUnmatchThisMatchGroup);

			aRJNL.AH_TransactionCategory = ZString.Empty;
			Factory.Save();
			AssertEquals(UnmatchingResult.Success, TestUnmatchingRow.CanUnmatchThisMatchGroup);
		}

		#region TestUnmatchSingleOrgSingleLedger

		public void TestUnmatchSingleOrgSingleLedger()
		{
			SetupDataSet1();
			TestUnmatchingRow.MatchGroupNum = "M00001000";

			ARINV1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(ARINV1, ARINV1.TransactionCurrency, ARINV1.AH_ExchangeRate, 10m, 0m, 0m, 10m, 0m, 0m);
			ARINV1.AH_OutstandingAmount = 0M;
			ARINV1.AH_FullyPaidDate = ZDateTime.Today;

			ARCRD1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(ARCRD1, ARCRD1.TransactionCurrency, ARCRD1.AH_ExchangeRate, 10m, 0m, 0m, 10m, 0m, 0m);
			ARCRD1.AH_OutstandingAmount = 0M;
			ARCRD1.AH_FullyPaidDate = ZDateTime.Today;

			TransactionMatchLink aRINV1Match = GenerateMatchLinks_ForTestOnly(ARINV1);
			aRINV1Match.AP_AH = ARINV1.PK;
			PayMatchLink_ForTestOnly(aRINV1Match, 10m);
			aRINV1Match.AP_MatchGroupNum = "M00001000";
			aRINV1Match.AP_MatchDate = ZDateTime.Today;

			TransactionMatchLink aRCRD1Match = GenerateMatchLinks_ForTestOnly(ARINV1);
			aRCRD1Match.AP_AH = ARCRD1.PK;
			PayMatchLink_ForTestOnly(aRCRD1Match, -10m);
			aRCRD1Match.AP_MatchGroupNum = "M00001000";
			aRCRD1Match.AP_MatchDate = ZDateTime.Today;
			Factory.Save();

			AssertEquals("M00001000 should be able to be unmatched", UnmatchingResult.Success, TestUnmatchingRow.CanUnmatchThisMatchGroup);
			TestUnmatchingRow.UnmatchNonPaymentGroup();
			Factory.Save();

			ARINV1.Reload();
			ARCRD1.Reload();

			AssertEquals("ARINV1 should have outstanding amount 10", 10M, ARINV1.AH_OutstandingAmount);
			AssertEquals("ARINV1 should have fully paid date = null", ZDateTime.Empty, ARINV1.AH_FullyPaidDate);
			AssertEquals("ARCRD1 should have outstanding amount -10", -10M, ARCRD1.AH_OutstandingAmount);
			AssertEquals("ARCRD1 should have fully paid date = null", ZDateTime.Empty, ARCRD1.AH_FullyPaidDate);
			Assert("Matchlink for ARINV1 should be deleted", aRINV1Match.IsDeleted);
			Assert("Matchlink for ARCRD1 should be deleted", aRCRD1Match.IsDeleted);

			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load(new ZQuery());
			AssertEquals("All matchlinks should be deleted", 0, matchLinks.Count);
		}

		#endregion

		#region TestUnmatchSingleOrgSingleLedger2

		public void TestUnmatchSingleOrgSingleLedger2()
		{
			SetupDataSet1();
			TestUnmatchingRow.MatchGroupNum = "M00001000";
			//Factory.Save();

			ARINV1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(ARINV1, ARINV1.TransactionCurrency, ARINV1.AH_ExchangeRate, 10m, 0m, 0m, 10m, 0m, 0m);
			ARINV1.AH_OutstandingAmount = 0M;
			ARINV1.AH_FullyPaidDate = ZDateTime.Today;

			ARCRD1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(ARCRD1, ARCRD1.TransactionCurrency, ARCRD1.AH_ExchangeRate, 20m, 0m, 0m, 20m, 0m, 0m);
			ARCRD1.AH_OutstandingAmount = -10M;
			ARCRD1.AH_FullyPaidDate = ZDateTime.Empty;

			TransactionMatchLink aRINV1Match = GenerateMatchLinks_ForTestOnly(ARINV1);
			aRINV1Match.AP_AH = ARINV1.PK;
			PayMatchLink_ForTestOnly(aRINV1Match, 5m);
			ARINV1.AH_GSTAmount = -5M;
			aRINV1Match.AP_MatchGroupNum = "M00001000";
			aRINV1Match.AP_MatchDate = ZDateTime.Today;

			TransactionMatchLink aRCRD1Match = GenerateMatchLinks_ForTestOnly(ARINV1);
			aRCRD1Match.AP_AH = ARCRD1.PK;
			PayMatchLink_ForTestOnly(aRCRD1Match, -5m);
			ARCRD1.AH_GSTAmount = 5M;
			aRCRD1Match.AP_MatchGroupNum = "M00001000";
			aRCRD1Match.AP_MatchDate = ZDateTime.Today;

			Factory.Save();

			AssertEquals("M00001000 should be able to be unmatched", UnmatchingResult.Success, TestUnmatchingRow.CanUnmatchThisMatchGroup);

			TestUnmatchingRow.UnmatchNonPaymentGroup();
			Factory.Save();

			ARINV1.Reload();
			ARCRD1.Reload();

			AssertEquals("ARINV1 should have outstanding amount 10", 10M, ARINV1.AH_OutstandingAmount);
			AssertEquals("ARINV1 should have fully paid date = null", ZDateTime.Empty, ARINV1.AH_FullyPaidDate);
			AssertEquals("ARCRD1 should have outstanding amount -20", -20M, ARCRD1.AH_OutstandingAmount);
			Assert("Matchlink for ARINV1 should be deleted", aRINV1Match.IsDeleted);
			Assert("Matchlink for ARCRD1 should be deleted", aRCRD1Match.IsDeleted);

			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load(new ZQuery());
			AssertEquals("All matchlinks should be deleted", 0, matchLinks.Count);
		}

		#endregion

		#region TestUnmatchMultipleOrgSingleLedger

		public void TestUnmatchMultipleOrgSingleLedger()
		{
			SetupDataSet1();
			TestUnmatchingRow.MatchGroupNum = "M00001001";

			ARINV1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(ARINV1, ARINV1.TransactionCurrency, ARINV1.AH_ExchangeRate, 40m, 0m, 0m, 40m, 0m, 0m);
			ARINV1.AH_OutstandingAmount = 30M;

			ARREC1.AH_OH = TestOrg2.PK;
			ARREC1.AH_LocalExTaxAmount = 20M;
			ARREC1.AH_OSExTaxAmount = 20M;
			ARREC1.AH_OutstandingAmount = 0M;
			ARREC1.AH_FullyPaidDate = ZDateTime.Now;

			ARINV2.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(ARINV2, ARINV2.TransactionCurrency, ARINV2.AH_ExchangeRate, 20m, 0m, 0m, 20m, 0m, 0m);
			ARINV2.AH_OutstandingAmount = 15M;

			ARCRD1.AH_OH = TestOrg2.PK;
			TestObjectCreator.CreateInvoiceLine(ARCRD1, ARCRD1.TransactionCurrency, ARCRD1.AH_ExchangeRate, 5m, 0m, 0m, 5m, 0m, 0m);
			ARCRD1.AH_OutstandingAmount = 0M;

			TransactionMatchLink aRINV1Match = GenerateMatchLinks_ForTestOnly(ARINV1);
			aRINV1Match.AP_AH = ARINV1.PK;
			ARINV1.AH_GSTAmount = 1M;
			PayMatchLink_ForTestOnly(aRINV1Match, 11m);
			aRINV1Match.AP_MatchGroupNum = "M00001001";
			aRINV1Match.AP_MatchDate = ZDateTime.Today;

			TransactionMatchLink aRREC1Match = GenerateMatchLinks_ForTestOnly(ARINV1);
			aRREC1Match.AP_AH = ARREC1.PK;
			PayMatchLink_ForTestOnly(aRREC1Match, -20m);
			aRREC1Match.AP_MatchGroupNum = "M00001001";
			aRREC1Match.AP_MatchDate = ZDateTime.Today;

			TransactionMatchLink aRINV2Match = GenerateMatchLinks_ForTestOnly(ARINV1);
			aRINV2Match.AP_AH = ARINV2.PK;
			PayMatchLink_ForTestOnly(aRINV2Match, 5m);
			aRINV2Match.AP_MatchGroupNum = "M00001001";
			aRINV2Match.AP_MatchDate = ZDateTime.Today;

			TransactionMatchLink aRCRD1Match = GenerateMatchLinks_ForTestOnly(ARINV1);
			aRCRD1Match.AP_AH = ARCRD1.PK;
			PayMatchLink_ForTestOnly(aRCRD1Match, 5m);
			ARCRD1.AH_GSTAmount = 10M;
			aRCRD1Match.AP_MatchGroupNum = "M00001001";
			aRCRD1Match.AP_MatchDate = ZDateTime.Today;

			AccTransactionHeader headerToMatch = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "12", TestObjectCreator.AUD, 1, 1, 0, 1, 0);
			headerToMatch.AH_OutstandingAmount = 0;
			TransactionMatchLink linkToMatch = GenerateMatchLinks_ForTestOnly(ARINV1);
			linkToMatch.AP_AH = headerToMatch.PK;
			PayMatchLink_ForTestOnly(linkToMatch, -1m);
			linkToMatch.AP_MatchGroupNum = "M00001001";
			linkToMatch.AP_MatchDate = ZDateTime.Today;

			AssertExceptionThrown<OnSavingCriticalCheckException<AccTransactionHeader>>("Should detect the wrong data in ARCRD1, the invoice amount and match link amount has opposite signs", () => Factory.Save());
			ErrorReporter.Clear();
		}

		#endregion

		#region TestUnmatchMatchGroupWithDiscount

		public void TestUnmatchMatchGroupWithDiscount()
		{
			TestUnmatchingRow.MatchGroupNum = "M00001001";

			ARINV1 = Factory.New<ARInvoice>();
			var arInvoiceLine = (ARInvoiceLine)ARINV1.Lines.AddNew();
			arInvoiceLine.AL_LocalExTaxAmount = 100m;
			arInvoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			ARINV1.AH_LocalOutstandingAmount = 0M;

			ARCRD1 = Factory.New<ARCreditNote>();
			var arCreditNoteLine = (ARCreditNoteLine)ARCRD1.Lines.AddNew();
			arCreditNoteLine.AL_LocalExTaxAmount = 80m;
			arCreditNoteLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			ARCRD1.AH_LocalOutstandingAmount = 0M;

			ARDiscount aRDSC = Factory.New<ARDiscount>();
			aRDSC.AH_LocalExTaxAmount = -20M;
			aRDSC.AH_OSExTaxAmount = -20M;
			aRDSC.AH_LocalOutstandingAmount = 0M;

			TransactionMatchLink aRINVMatch = GenerateMatchLinks_ForTestOnly(ARINV1);
			aRINVMatch.AP_AH = ARINV1.PK;
			PayMatchLink_ForTestOnly(aRINVMatch, 100m);
			aRINVMatch.AP_MatchGroupNum = "M00001001";

			TransactionMatchLink aRCRDMatch = GenerateMatchLinks_ForTestOnly(ARINV1);
			aRCRDMatch.AP_AH = ARCRD1.PK;
			PayMatchLink_ForTestOnly(aRCRDMatch, -80m);
			aRCRDMatch.AP_MatchGroupNum = "M00001001";

			TransactionMatchLink aRDSCMatch = GenerateMatchLinks_ForTestOnly(ARINV1);
			aRDSCMatch.AP_AH = aRDSC.PK;
			PayMatchLink_ForTestOnly(aRDSCMatch, -20m);
			aRDSCMatch.AP_MatchGroupNum = "M00001001";
			aRDSCMatch.AP_MatchDate = ZDateTime.Now.AddHours(-1);

			TestObjectCreator.SetupMatchLinkMatchDate(ARINV1);

			Factory.Save();

			TestUnmatchingRow.UnmatchDate = ZDateTime.BrettsBirthday;
			TestUnmatchingRow.UnmatchNonPaymentGroup();
			Factory.Save();

			Assert("Matchlink for DSC should be deleted", aRDSCMatch.IsDeleted);
			Assert("Matchlink for INV should be deleted", aRINVMatch.IsDeleted);
			Assert("Matchlink for CRD should be deleted", aRCRDMatch.IsDeleted);

			TransactionMatchLinkCollection matchlinks = new TransactionMatchLinkCollection(Factory);
			matchlinks.Load(new ZQuery());
			AssertEquals("There should be 2 new matchlinks in the collection - one for each of the discounts", 2, matchlinks.Count);

			ZString matchGroup = matchlinks[0].AP_MatchGroupNum;
			ZDateTime matchDate = matchlinks[0].AP_MatchDate;
			AssertEquals("Unmatch date should be used", ZDateTime.BrettsBirthday, matchDate);
			AssertEquals("The matchlinks should have the same match date", matchDate, matchlinks[1].AP_MatchDate);
			if (aRDSC.PK == matchlinks[0].AP_AH)
			{
				AssertEquals("Original Transaction post date should be unchanged", ZDateTime.Today, matchlinks[0].TransactionHeader.AH_PostDate.Date);
				AssertEquals("Transaction post date should have the same match date", matchDate, matchlinks[1].TransactionHeader.AH_PostDate);
			}
			else
			{
				AssertEquals("Original Transaction post date should be unchanged", ZDateTime.Today, matchlinks[1].TransactionHeader.AH_PostDate.Date);
				AssertEquals("Transaction post date should have the same match date", matchDate, matchlinks[0].TransactionHeader.AH_PostDate);
			}
			AssertEquals("The matchlinks should have the same group number", matchGroup, matchlinks[1].AP_MatchGroupNum);
		}

		#endregion

		#region TestUnmatchMatchGroupWithOverpayment

		public void TestUnmatchMatchGroupWithOverpayment()
		{
			TestUnmatchingRow.MatchGroupNum = "M00001001";

			ARINV1 = Factory.New<ARInvoice>();
			var line = (ARInvoiceLine)ARINV1.Lines.AddNew();
			line.AL_LocalExTaxAmount = 20m;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			ARINV1.AH_LocalOutstandingAmount = 0M;

			AROverpayment aROVP = Factory.New<AROverpayment>();
			aROVP.AH_LocalExTaxAmount = -20M;
			aROVP.AH_OSExTaxAmount = -20M;
			aROVP.AH_LocalOutstandingAmount = 0M;

			TransactionMatchLink aRINVMatch = GenerateMatchLinks_ForTestOnly(ARINV1);
			aRINVMatch.AP_AH = ARINV1.PK;
			PayMatchLink_ForTestOnly(aRINVMatch, 20m);
			aRINVMatch.AP_MatchGroupNum = "M00001001";

			TransactionMatchLink aROVPMatch = GenerateMatchLinks_ForTestOnly(ARINV1);
			aROVPMatch.AP_AH = aROVP.PK;
			PayMatchLink_ForTestOnly(aROVPMatch, -20m);
			aROVPMatch.AP_MatchGroupNum = "M00001001";
			aROVPMatch.AP_MatchDate = ZDateTime.Now.AddHours(-1);

			TestObjectCreator.SetupMatchLinkMatchDate(ARINV1);

			Factory.Save();

			TestUnmatchingRow.UnmatchNonPaymentGroup();
			TestUnmatchingRow.UnmatchDate = ZDateTime.BrettsBirthday;
			TestUnmatchingRow.UpdateUnmatchDateForAlreadyUnmatchedTransactions();
			Factory.Save();

			Assert("Matchlink for OVP should be deleted", aROVPMatch.IsDeleted);
			Assert("Matchlink for INV should be deleted", aRINVMatch.IsDeleted);

			TransactionMatchLinkCollection matchlinks = new TransactionMatchLinkCollection(Factory);
			matchlinks.Load(new ZQuery());
			AssertEquals("There should be 2 new matchlinks in the collection - one for each of the overpayments", 2, matchlinks.Count);

			ZString matchGroup = matchlinks[0].AP_MatchGroupNum;
			ZDateTime matchDate = matchlinks[0].AP_MatchDate;
			AssertEquals("Unmatch date should be used", ZDateTime.BrettsBirthday, matchDate);
			AssertEquals("The matchlinks should have the same match date", matchDate, matchlinks[1].AP_MatchDate);
			if (aROVP.PK == matchlinks[0].AP_AH)
			{
				AssertEquals("Original Transaction post date should be unchanged", ZDateTime.Today, matchlinks[0].TransactionHeader.AH_PostDate.Date);
				AssertEquals("Transaction post date should have the same match date", matchDate, matchlinks[1].TransactionHeader.AH_PostDate);
			}
			else
			{
				AssertEquals("Original Transaction post date should be unchanged", ZDateTime.Today, matchlinks[1].TransactionHeader.AH_PostDate.Date);
				AssertEquals("Transaction post date should have the same match date", matchDate, matchlinks[0].TransactionHeader.AH_PostDate);
			}
			AssertEquals("The matchlinks should have the same group number", matchGroup, matchlinks[1].AP_MatchGroupNum);
		}

		#endregion

		#region TestUnmatchMatchGroupWithExchangeDifference

		public void TestUnmatchMatchGroupWithExchangeDifference()
		{
			TestUnmatchingRow.MatchGroupNum = "M00001002";

			ARCRD1 = Factory.New<ARCreditNote>();
			var line = (ARCreditNoteLine)ARCRD1.Lines.AddNew();
			line.AL_LocalExTaxAmount = 40m;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			ARCRD1.AH_OutstandingAmount = 0M;

			ARExchangeDifference aREXX = Factory.New<ARExchangeDifference>();
			aREXX.AH_LocalExTaxAmount = -40M;
			aREXX.AH_OSExTaxAmount = -40M;
			aREXX.AH_LocalOutstandingAmount = 0M;

			TransactionMatchLink aRCRDMatch = GenerateMatchLinks_ForTestOnly(ARCRD1);
			aRCRDMatch.AP_AH = ARCRD1.PK;
			PayMatchLink_ForTestOnly(aRCRDMatch, -40m);
			aRCRDMatch.AP_MatchGroupNum = "M00001002";

			TransactionMatchLink aREXXMatch = GenerateMatchLinks_ForTestOnly(ARCRD1);
			aREXXMatch.AP_AH = aREXX.PK;
			PayMatchLink_ForTestOnly(aREXXMatch, 40m);
			aREXXMatch.AP_MatchGroupNum = "M00001002";
			aREXXMatch.AP_MatchDate = ZDateTime.Now.AddHours(-1);

			TestObjectCreator.SetupMatchLinkMatchDate(ARCRD1);

			Factory.Save();

			TestUnmatchingRow.UnmatchNonPaymentGroup();
			Factory.Save();
			Assert("CRD Matchlink should be deleted", aRCRDMatch.IsDeleted);
			Assert("EXX Matchlink should be deleted", aREXXMatch.IsDeleted);

			TransactionMatchLinkCollection matchlinks = new TransactionMatchLinkCollection(Factory);
			matchlinks.Load(new ZQuery());

			AssertEquals("There should be 2 new matchlinks, one for each EXX", 2, matchlinks.Count);

			ZQuery reversingMatchLinkFilter = new ZQuery(ZArchitecture.Schema.AccTransactionMatchLinkSchema.AP_AH, SQLComparisonOperator.NotEqual, aREXX.PK);
			BusinessObject[] bizOs = matchlinks.Find(reversingMatchLinkFilter);
			AssertEquals("There should be 1 matchlink for the reversing EXX", 1, bizOs.Length);
			TransactionMatchLink eXXRevMatch = bizOs[0] as TransactionMatchLink;
			AssertEquals("Amount on EXX Reversing MatchLink is -40", -40M, eXXRevMatch.AP_Amount);

			AssertEquals("The match date should be equal", ZDateTime.Today, matchlinks[0].AP_MatchDate.Date);
			AssertEquals("The match date should be equal", matchlinks[0].AP_MatchDate, matchlinks[1].AP_MatchDate);
			AssertEquals("Transaction post date should have the same match date", ZDateTime.Today, matchlinks[0].TransactionHeader.AH_PostDate.Date);
			AssertEquals("Transaction post date should have the same match date", ZDateTime.Today, matchlinks[1].TransactionHeader.AH_PostDate.Date);
		}

		#endregion

		#region TestUnmatchSystemGeneratedContra

		public void TestUnmatchSystemGeneratedContra()
		{
			//ClearTable(TransactionMatchLink.Schema.TableName);

			TestUnmatchingRow.MatchGroupNum = "M00001390";
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();

			ARINV1 = Factory.NewWithValidTestData<ARInvoice>();
			ARINV1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(ARINV1, ARINV1.TransactionCurrency, ARINV1.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);
			ARINV1.AH_OutstandingAmount = 0M;
			ARINV1.AH_FullyPaidDate = ZDateTime.Today;

			//Contra TestContra = Contra.New(Factory);

			APContraRow testAPContraRow = Factory.NewWithValidTestData(typeof(APContraRow)) as APContraRow;
			testAPContraRow.AH_TransactionNum = "00001338";
			testAPContraRow.AH_OH = TestOrg1.PK;
			testAPContraRow.AH_LocalExTaxAmount = 99M;
			testAPContraRow.AH_OSExTaxAmount = 99M;
			testAPContraRow.AH_OutstandingAmount = 0M;
			testAPContraRow.AH_FullyPaidDate = ZDateTime.Today;
			testAPContraRow.AH_TransactionCreatedByMatching = true;

			ARContraRow testARContraRow = Factory.NewWithValidTestData(typeof(ARContraRow)) as ARContraRow;
			testARContraRow.AH_TransactionNum = "00001338";
			testARContraRow.AH_OH = TestOrg1.PK;
			testARContraRow.AH_LocalExTaxAmount = 99M;      // inverted in the DB
			testARContraRow.AH_OSExTaxAmount = 99M;
			testARContraRow.AH_OutstandingAmount = 0M;
			testARContraRow.AH_FullyPaidDate = ZDateTime.Today;
			testARContraRow.AH_TransactionCreatedByMatching = true;

			TransactionMatchLink aRINVMatch = GenerateMatchLinks_ForTestOnly(ARINV1);
			aRINVMatch.AP_AH = ARINV1.PK;
			PayMatchLink_ForTestOnly(aRINVMatch, 100m);
			aRINVMatch.AP_MatchGroupNum = "M00001390";

			ARPayment aRPay = Factory.NewWithValidTestData<ARPayment>();
			aRPay.AH_OSExTaxAmount = -100m;
			aRPay.AH_OutstandingAmount = 0M;
			aRPay.AH_FullyPaidDate = ZDateTime.Today;

			TransactionMatchLink aRPayMatch = GenerateMatchLinks_ForTestOnly(ARINV1);
			aRPayMatch.AP_AH = aRPay.PK;
			aRPayMatch.AP_MatchGroupNum = "M00001390";
			PayMatchLink_ForTestOnly(aRPayMatch, -100m);

			TransactionMatchLink aRRowMatch = GenerateMatchLinks_ForTestOnly(ARINV1);
			aRRowMatch.AP_AH = testARContraRow.PK;
			PayMatchLink_ForTestOnly(aRRowMatch, -99m);
			aRRowMatch.AP_MatchGroupNum = "M00001390";

			TransactionMatchLink aPRowMatch = GenerateMatchLinks_ForTestOnly(ARINV1);
			aPRowMatch.AP_AH = testAPContraRow.PK;
			PayMatchLink_ForTestOnly(aPRowMatch, 99m);
			aPRowMatch.AP_MatchGroupNum = "M00001390";
			TestObjectCreator.SetupMatchLinkMatchDate(ARINV1);

			Factory.Save();

			ZDateTime expectedDate = ZDateTime.BrettsBirthday;
			TestUnmatchingRow.UnmatchDate = expectedDate;
			TestUnmatchingRow.UnmatchNonPaymentGroup();
			Factory.Save();

			// What should happen:
			// -CreditNote Matchlink should be deleted 
			// -Create a reversing APRow and ARRow in addition to the existing APRow and ARRow
			// -All 4 ContraRows should be in the same match group
			// -All 4 ContraRows should have the same AH_TransactionBelongsToGroup 
			// -Reversing ContraRows should have the same transaction number

			BusinessObjectFactory factoryForReLoading = new BusinessObjectFactory();

			// Test that no system generated contraRows are not cancelled in the unmatching
			var originalContraFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Contra);
			originalContraFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCreatedByMatching, SQLComparisonOperator.Equal, true);
			originalContraFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

			var rows = factoryForReLoading.Load(typeof(AccTransactionHeader), originalContraFilter) as AccTransactionHeader[];

			Assert("Contras are returned", rows.Any());
			foreach (var row in rows)
			{
				AssertEquals("All Rows should have AH_IsCancelled as True", true, row.AH_IsCancelled);
			}

			// Pull out the Reversing APContraRow
			ZQuery revAPContraFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Contra);
			revAPContraFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsPayable);
			revAPContraFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, testAPContraRow.PK);
			revAPContraFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

			APContraRow revAPContraRow = Factory.LoadTop1(typeof(APContraRow), revAPContraFilter) as APContraRow;
			AssertNotNull("Reversing APContraRow should be created", revAPContraRow);
			AssertEquals("InvoiceAmount of reversing APContraRow should be negative that of original APContraRow i.e. -99", -99M, revAPContraRow.AH_InvoiceAmount);
			AssertEquals("OSTotal of reversing APContraRow should be negative that of original APContraRow i.e. -99", -99M, revAPContraRow.AH_OSTotal);
			AssertEquals("OutstandingAmount of reversing APContraRow should be 0", 0M, revAPContraRow.AH_OutstandingAmount);
			Assert("Reversing APContraRow should be cancelled", revAPContraRow.AH_IsCancelled);
			AssertEquals("Original and Reversing APContraRows should have the same changed fullypaid date", expectedDate, testAPContraRow.AH_FullyPaidDate.Date);
			AssertEquals("Original and Reversing APContraRows should have the same fullypaid date", testAPContraRow.AH_FullyPaidDate.Date, revAPContraRow.AH_FullyPaidDate.Date);
			AssertEquals("Original APContraRows should have the unchanged post date", ZDateTime.Today, testAPContraRow.AH_PostDate.Date);
			AssertEquals("Reversing APContraRows should have the changed post date", expectedDate, revAPContraRow.AH_PostDate.Date);

			// Pull out the Reversing ARContraRow
			ZQuery revARContraFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Contra);
			revARContraFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			revARContraFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, testARContraRow.PK);
			revARContraFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

			ARContraRow revARContraRow = Factory.LoadTop1(typeof(ARContraRow), revARContraFilter) as ARContraRow;
			AssertNotNull("Reversing ARContraRow should be created", revARContraRow);
			AssertEquals("InvoiceAmount of reversing ARContraRow should be negative that of original ARContraRow i.e. 99", 99M, revARContraRow.AH_InvoiceAmount);
			AssertEquals("OSTotal of reversing ARContraRow should be negative that of original ARContraRow i.e. 99", 99M, revARContraRow.AH_OSTotal);
			AssertEquals("Outstanding Amount of reversing ARContraRow should be 0", 0M, revARContraRow.AH_OutstandingAmount);
			Assert("Reversing ARContraRow should be cancelled", revARContraRow.AH_IsCancelled);
			AssertEquals("Original and Reversing ARContraRows should have same fullypaid date", testARContraRow.AH_FullyPaidDate.Date, revARContraRow.AH_FullyPaidDate.Date);
			AssertEquals("Original and Reversing ARContraRows should have the same fullypaid date", testARContraRow.AH_FullyPaidDate.Date, revARContraRow.AH_FullyPaidDate.Date);
			AssertEquals("Original ARContraRows should have the unchanged post date", ZDateTime.Today, testARContraRow.AH_PostDate.Date);
			AssertEquals("Reversing ARContraRows should have the changed post date", expectedDate, revARContraRow.AH_PostDate.Date);

			AssertEquals("Reversing contra rows should have the same transactionnumber", revARContraRow.AH_TransactionNum, revAPContraRow.AH_TransactionNum);

			// Pull out Matchlink for original APContraRow
			ZQuery aPContraRowMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, testAPContraRow.PK);
			TransactionMatchLink aPContraRowMatch = Factory.LoadTop1(typeof(TransactionMatchLink), aPContraRowMatchFilter) as TransactionMatchLink;
			AssertNotNull("Original APContraRow should be matched", aPContraRowMatch);
			AssertEquals("Match amount should be 99", 99M, aPContraRowMatch.AP_Amount);
			ZString commonMatchGroupNum = aPContraRowMatch.AP_MatchGroupNum;
			ZDateTime commonMatchDate = aPContraRowMatch.AP_MatchDate;
			AssertEquals("MatchDate should be changed", ZDateTime.BrettsBirthday, commonMatchDate);

			// Pull out the Matchlink for Reversing APContraRow
			ZQuery revAPContraRowMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revAPContraRow.PK);
			TransactionMatchLink revAPContraRowMatch = Factory.LoadTop1(typeof(TransactionMatchLink), revAPContraRowMatchFilter) as TransactionMatchLink;
			AssertNotNull("Reversing APContraRow should be matched", revAPContraRowMatch);
			AssertEquals("Match amount should be -99", -99M, revAPContraRowMatch.AP_Amount);
			AssertEquals("Match group should be same for all 4 match links", commonMatchGroupNum, revAPContraRowMatch.AP_MatchGroupNum);
			AssertEquals("Match date should be same for all 4 match links", commonMatchDate.Date, revAPContraRowMatch.AP_MatchDate.Date);

			// Pull out the Matchlink for original ARContraRow
			ZQuery aRContraRowMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, testARContraRow.PK);
			TransactionMatchLink aRContraRowMatch = Factory.LoadTop1(typeof(TransactionMatchLink), aRContraRowMatchFilter) as TransactionMatchLink;
			AssertNotNull("Original ARContraRow should be matched", aRContraRowMatch);
			AssertEquals("Match amount should be -99", -99M, aRContraRowMatch.AP_Amount);
			AssertEquals("Match group should be same for all 4 match links", commonMatchGroupNum, aRContraRowMatch.AP_MatchGroupNum);
			AssertEquals("Match date should be same for all 4 match links", commonMatchDate.Date, aRContraRowMatch.AP_MatchDate.Date);

			// Pull out the Matchlink for reversing ARContraRow
			ZQuery revARContraRowMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revARContraRow.PK);
			TransactionMatchLink revARContraRowMatch = Factory.LoadTop1(typeof(TransactionMatchLink), revARContraRowMatchFilter) as TransactionMatchLink;
			AssertNotNull("Reversing ARContraRow should be matched", revARContraRowMatch);
			AssertEquals("Match amount should be 99", 99M, revARContraRowMatch.AP_Amount);
			AssertEquals("Match group should be same for all 4 match links", commonMatchGroupNum, revARContraRowMatch.AP_MatchGroupNum);
			AssertEquals("Match date should be same for all 4 match links", commonMatchDate.Date, revARContraRowMatch.AP_MatchDate.Date);

			// Check the AR Invoice
			Assert("Invoice matchlink should be deleted", aRINVMatch.IsDeleted);
			ARINV1.Reload();
			AssertEquals("Outstanding amount should be 100", 100M, ARINV1.AH_OutstandingAmount);
			Assert("Fully paid date should be empty", ARINV1.AH_FullyPaidDate.IsEmpty);

			expectedDate = ZDateTime.Today.AddDays(-100);
			TestUnmatchingRow.UnmatchDate = expectedDate;
			TestUnmatchingRow.UpdateUnmatchDateForAlreadyUnmatchedTransactions();

			// Here we have unmatched transactions, lately created AR/AP contra rows. Need to suspend post date validation for them all.
			using (ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.PostDateCriticalValidationSuspenderService>.GetSuspender(Factory))
			{
				Factory.Save();
			}

			AssertEquals("Original and Reversing APContraRows should have the same changed fullypaid date", expectedDate, testAPContraRow.AH_FullyPaidDate.Date);
			AssertEquals("Original and Reversing APContraRows should have the same fullypaid date", testAPContraRow.AH_FullyPaidDate.Date, revAPContraRow.AH_FullyPaidDate.Date);
			AssertEquals("Original APContraRows should have the unchanged post date", ZDateTime.Today, testAPContraRow.AH_PostDate.Date);
			AssertEquals("Reversing APContraRows should have the changed post date", expectedDate, revAPContraRow.AH_PostDate.Date);

			AssertEquals("Original and Reversing ARContraRows should have same fullypaid date", testARContraRow.AH_FullyPaidDate.Date, revARContraRow.AH_FullyPaidDate.Date);
			AssertEquals("Original and Reversing ARContraRows should have the same fullypaid date", testARContraRow.AH_FullyPaidDate.Date, revARContraRow.AH_FullyPaidDate.Date);
			AssertEquals("Original ARContraRows should have the unchanged post date", ZDateTime.Today, testARContraRow.AH_PostDate.Date);
			AssertEquals("Reversing ARContraRows should have the changed post date", expectedDate, revARContraRow.AH_PostDate.Date);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestUnmatchSystemGeneratedContraWhenMatchDateIsFutureDate()
		{
			var futureDate = ZDateTime.Today.AddDays(10);

			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg1.OH_IsDebtor = true;
			TestOrg1.OH_IsCreditor = true;
			Factory.Save();

			var arContra = Contra.New(Factory, LedgerTypes.AccountsReceivable);
			arContra.AH_OSTotal = 100M;
			arContra.AH_ARAccount = TestOrg1.PK;
			arContra.AH_APAccount = TestOrg1.PK;
			Factory.Save();

			var apContra = Contra.New(Factory, LedgerTypes.AccountsPayable);
			apContra.AH_OSTotal = 100M;
			apContra.AH_ARAccount = TestOrg1.PK;
			apContra.AH_APAccount = TestOrg1.PK;
			Factory.Save();

			var matchingBase = new APMatchingBase(Factory);
			matchingBase.PrimaryOrganization = TestOrg1.PK;
			matchingBase.MatchDate = futureDate;
			var arContraRows = Factory.Load<ARContraRow>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Contra).AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable));
			var apContraRows = Factory.Load<APContraRow>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Contra).AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable));
			matchingBase.MoveFromUnmatchToMatch(arContraRows);
			matchingBase.MoveFromUnmatchToMatch(apContraRows);
			matchingBase.MatchAndClearTransactions();
			Factory.Save();

			TestUnmatchingRow.MatchDate = futureDate;
			TestUnmatchingRow.UnmatchDate = futureDate;
			TestUnmatchingRow.MatchGroupNum = "M00001000";
			TestUnmatchingRow.UnmatchNonPaymentGroup();
			Factory.Save();

			var query = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Contra);
			query.AddToFilter(AccTransactionHeaderSchema.AH_Desc, "RECEIVABLE AND PAYABLE CONTRA (SYSTEM GENERATED)");
			var systemGeneratedARContra = Factory.LoadTop1<AccTransactionHeader>(query);
			AssertEquals(futureDate, systemGeneratedARContra.AH_PostDate);
			AssertEquals(futureDate, systemGeneratedARContra.AH_FullyPaidDate);

			query = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Contra);
			query.AddToFilter(AccTransactionHeaderSchema.AH_Desc, "RECEIVABLE AND PAYABLE CONTRA (SYSTEM GENERATED)");
			var systemGeneratedAPContra = Factory.LoadTop1<AccTransactionHeader>(query);
			AssertEquals(futureDate, systemGeneratedAPContra.AH_PostDate);
			AssertEquals(futureDate, systemGeneratedAPContra.AH_FullyPaidDate);
		}

		#endregion

		#region TestUnmatchSystemGeneratedAPTransfer

		public void TestUnmatchSystemGeneratedAPTransfer()
		{
			TestUnmatchingRow.MatchGroupNum = "M00001665";

			APCreditNote testAPCRD = Factory.NewWithValidTestData<APCreditNote>();
			TestObjectCreator.CreateInvoiceLine(testAPCRD, testAPCRD.TransactionCurrency, testAPCRD.AH_ExchangeRate, 98m, 0m, 0m, 98m, 0m, 0m);
			testAPCRD.AH_LocalOutstandingAmount = 0M;
			testAPCRD.AH_FullyPaidDate = ZDateTime.Now;

			APInvoice testAPINV = Factory.NewWithValidTestData<APInvoice>();
			TestObjectCreator.CreateInvoiceLine(testAPINV, testAPCRD.TransactionCurrency, testAPCRD.AH_ExchangeRate, 98m, 0m, 0m, 98m, 0m, 0m);
			testAPINV.AH_LocalOutstandingAmount = 0M;
			testAPINV.AH_FullyPaidDate = ZDateTime.Now;

			APTransfer testAPTransfer = (APTransfer)Transfer.New(typeof(APTransfer), Factory);
			testAPTransfer.TransferFrom.AH_TransactionCreatedByMatching = true;
			testAPTransfer.TransferTo.AH_TransactionCreatedByMatching = true;
			testAPTransfer.AH_InvoiceAmount = 30M;
			testAPTransfer.AH_OSTotal = 30M;
			testAPTransfer.TransferFrom.AH_OutstandingAmount = 0M;
			testAPTransfer.TransferTo.AH_OutstandingAmount = 0M;
			testAPTransfer.TransferFrom.AH_FullyPaidDate = ZDateTime.Now;
			testAPTransfer.TransferTo.AH_FullyPaidDate = ZDateTime.Now;

			TransactionMatchLink transferFromMatch = GenerateMatchLinks_ForTestOnly(testAPCRD);
			transferFromMatch.AP_AH = testAPTransfer.TransferFrom.PK;
			PayMatchLink_ForTestOnly(transferFromMatch, 30m);
			transferFromMatch.AP_MatchGroupNum = "M00001665";

			TransactionMatchLink transferToMatch = GenerateMatchLinks_ForTestOnly(testAPCRD);
			transferToMatch.AP_AH = testAPTransfer.TransferTo.PK;
			PayMatchLink_ForTestOnly(transferToMatch, -30m);
			transferToMatch.AP_MatchGroupNum = "M00001665";

			TransactionMatchLink aPCRDMatch = GenerateMatchLinks_ForTestOnly(testAPCRD);
			aPCRDMatch.AP_AH = testAPCRD.PK;
			PayMatchLink_ForTestOnly(aPCRDMatch, 98m);
			aPCRDMatch.AP_MatchGroupNum = "M00001665";

			TransactionMatchLink aPINVMatch = GenerateMatchLinks_ForTestOnly(testAPCRD);
			aPINVMatch.AP_AH = testAPINV.PK;
			PayMatchLink_ForTestOnly(aPINVMatch, -98m);
			aPINVMatch.AP_MatchGroupNum = "M00001665";
			TestObjectCreator.SetupMatchLinkMatchDate(testAPCRD);

			Factory.Save();

			TestUnmatchingRow.UnmatchNonPaymentGroup();
			Factory.Save();

			// What should happen:
			// -Reversing AP TransferFrom and TransferTo rows should be created
			// -Matchlinks for all 4 Transfer Rows should be created, Matchlinks for Original TransferRows deleted
			// -All TransferRows should be cancelled
			// -CreditNote Matchlink should be deleted, Outstanding amt of CreditNote should be restored

			BusinessObjectFactory factoryForReLoading = new BusinessObjectFactory();

			// Test that no system generated TransferRows are not cancelled in the unmatching
			var originalTransferFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Transfer);
			originalTransferFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCreatedByMatching, SQLComparisonOperator.Equal, true);
			originalTransferFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

			var rows = factoryForReLoading.Load(typeof(AccTransactionHeader), originalTransferFilter) as AccTransactionHeader[];

			Assert("Should Contain Transfers", rows.Any());
			foreach (var row in rows)
			{
				AssertEquals("All Rows should have AH_IsCancelled as True", true, row.AH_IsCancelled);
			}

			TransactionMatchLinkCollection matchlinks = new TransactionMatchLinkCollection(Factory, new ZQuery());
			matchlinks.Load();
			AssertEquals("There should be 4 matchlinks in DB - one for each of the TransferRows", 4, matchlinks.Count);

			// Pull out Reversing APTransferFromRow
			ZQuery revAPFromRowFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Transfer);
			revAPFromRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsPayable);
			revAPFromRowFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, testAPTransfer.TransferFrom.PK);
			revAPFromRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			ZQuery transferFromFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionCount, (byte)1);
			transferFromFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionCount,
				SQLComparisonOperator.Equal, (byte)3);
			revAPFromRowFilter.AddToFilter(transferFromFilter, JoinCondition.And);

			APTransferFromRow revAPFromRow = Factory.LoadTop1(typeof(APTransferFromRow), revAPFromRowFilter) as APTransferFromRow;
			AssertNotNull("DB should contain a Reversing APTransferFromRow", revAPFromRow);
			AssertEquals("InvoiceAMount on Reversing APTransferFromRow = -30", -30M, revAPFromRow.AH_InvoiceAmount);
			AssertEquals("OSTotal on Reversing APTransferFromRow = -30", -30M, revAPFromRow.AH_OSTotal);
			AssertEquals("Outstanding Amt on Reversing APTransferFromRow = 0", 0M, revAPFromRow.AH_OutstandingAmount);
			Assert("Reversing APTransferFromRow should be cancelled", revAPFromRow.AH_IsCancelled);

			// Pull out the Reversing APTransferToRow
			ZQuery revAPToRowFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Transfer);
			revAPToRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsPayable);
			revAPToRowFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, testAPTransfer.TransferTo.PK);
			revAPFromRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			ZQuery transferToFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionCount, (byte)2);
			transferToFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionCount,
				SQLComparisonOperator.Equal, (byte)4);
			revAPToRowFilter.AddToFilter(transferToFilter, JoinCondition.And);

			APTransferToRow revAPToRow = Factory.LoadTop1(typeof(APTransferToRow), revAPToRowFilter) as APTransferToRow;
			AssertNotNull("DB should contain a Reversing APTransferToRow", revAPToRow);
			AssertEquals("InvoiceAMount on reversing APTransferToRow = 30", 30M, revAPToRow.AH_InvoiceAmount);
			AssertEquals("OSTOtal on Reversing APTransferToRow = 30", 30M, revAPToRow.AH_OSTotal);
			AssertEquals("Outstanding amt on Reversing APTransferToRow = 0", 0M, revAPToRow.AH_OutstandingAmount);
			Assert("Reversing Transaction should be cancelled", revAPToRow.AH_IsCancelled);

			AssertEquals("Reversing Transactions should have the same TransactionNumber",
				revAPToRow.AH_TransactionNum, revAPFromRow.AH_TransactionNum);
			AssertEquals("Reversing Transaction should have the same fullypaid date",
				revAPToRow.AH_FullyPaidDate, revAPFromRow.AH_FullyPaidDate);

			// Pull out the Matchlink for Original APTransferFromRow
			ZQuery aPFromRowMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, testAPTransfer.TransferFrom.PK);
			TransactionMatchLink aPFromRowMatch = Factory.LoadTop1(typeof(TransactionMatchLink), aPFromRowMatchFilter) as TransactionMatchLink;
			AssertNotNull("Original APTransferFromRow should be matched", aPFromRowMatch);
			AssertEquals("MatchAmount for Original APTransferFromRow = 30", 30M, aPFromRowMatch.AP_Amount);
			ZString commonMatchGroupNum = aPFromRowMatch.AP_MatchGroupNum;
			ZDateTime commonMatchDate = aPFromRowMatch.AP_MatchDate;

			// Pull out Matchlink for Reversing APTransferFromRow
			ZQuery revAPFromRowMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revAPFromRow.PK);
			TransactionMatchLink revAPFromRowMatch = Factory.LoadTop1(typeof(TransactionMatchLink), revAPFromRowMatchFilter) as TransactionMatchLink;
			AssertNotNull("Reversing APTransferFromRow should be matched", revAPFromRowMatch);
			AssertEquals("MatchAmount for Reversing APTransferFromRow = -30", -30M, revAPFromRowMatch.AP_Amount);
			AssertEquals("All 4 Matchlinks should have same MatchGroupnum", commonMatchGroupNum, revAPFromRowMatch.AP_MatchGroupNum);
			AssertEquals("All 4 MAtchlinks should have same matchdate", commonMatchDate, revAPFromRowMatch.AP_MatchDate);

			// Pull out Matchlink for Original APTransferToRow
			ZQuery aPToRowMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, testAPTransfer.TransferTo.PK);
			TransactionMatchLink aPToRowMatch = Factory.LoadTop1(typeof(TransactionMatchLink), aPToRowMatchFilter) as TransactionMatchLink;
			AssertNotNull("Original APTransferToRow should be matched", aPToRowMatch);
			AssertEquals("MatchAmount for Original APTransferToRow = -30", -30M, aPToRowMatch.AP_Amount);
			AssertEquals("All 4 Matchlinks should have same MatchGroupNum", commonMatchGroupNum, aPToRowMatch.AP_MatchGroupNum);
			AssertEquals("All 4 Matchlinks should have same MatchDate", commonMatchDate, aPToRowMatch.AP_MatchDate);

			// Pull out Matchlink for Reversing APTransferToRow
			ZQuery revAPToRowMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revAPToRow.PK);
			TransactionMatchLink revAPToRowMatch = Factory.LoadTop1(typeof(TransactionMatchLink), revAPToRowMatchFilter) as TransactionMatchLink;
			AssertNotNull("Reversing APTransferToRow should be matched", revAPToRowMatch);
			AssertEquals("MatchAmount for Reversing APTransferToRow = 30", 30M, revAPToRowMatch.AP_Amount);
			AssertEquals("All 4 Matchlinks should have same MatchGroupNum", commonMatchGroupNum, revAPToRowMatch.AP_MatchGroupNum);
			AssertEquals("All 4 Matchlinks should have same MatchDate", commonMatchDate, revAPToRowMatch.AP_MatchDate);

			// Check the AP CreditNote
			Assert("Matchlink for AP CreditNote should be deleted", aPCRDMatch.IsDeleted);
			testAPCRD.Reload();
			AssertEquals("Outstanding amount on APCreditNote", 98M, testAPCRD.AH_OutstandingAmount);
			Assert("Fully Paid date should be empty", testAPCRD.AH_FullyPaidDate.IsEmpty);
		}

		#endregion

		#region TestUnmatchMultipleSystemGeneratedTransaction

		public void TestUnmatchMultipleSystemGeneratedTransaction()
		{
			TestUnmatchingRow.MatchGroupNum = "M00001558";
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();

			APContraRow testAPRow = Factory.NewWithValidTestData(typeof(APContraRow)) as APContraRow;
			testAPRow.AH_TransactionNum = "00001990";
			testAPRow.AH_LocalExTaxAmount = 90M;
			testAPRow.AH_OSExTaxAmount = 90M;
			testAPRow.AH_LocalOutstandingAmount = 0M;
			testAPRow.AH_FullyPaidDate = ZDateTime.Today;
			testAPRow.AH_TransactionCreatedByMatching = true;

			ARContraRow testARRow = Factory.NewWithValidTestData(typeof(ARContraRow)) as ARContraRow;
			testARRow.AH_TransactionNum = "00001990";
			testARRow.AH_LocalExTaxAmount = 90M;
			testARRow.AH_OSExTaxAmount = 90M;
			testARRow.AH_LocalOutstandingAmount = 0M;
			testARRow.AH_FullyPaidDate = ZDateTime.Today;
			testARRow.AH_TransactionCreatedByMatching = true;

			ARTransfer testARTransfer = Transfer.New(typeof(ARTransfer), Factory) as ARTransfer;
			testARTransfer.TransferFrom.AH_TransactionCreatedByMatching = true;
			testARTransfer.TransferTo.AH_TransactionCreatedByMatching = true;
			testARTransfer.AH_InvoiceAmount = 39M;
			testARTransfer.AH_OSTotal = 39M;
			testARTransfer.TransferTo.AH_OutstandingAmount = 0M;
			testARTransfer.TransferFrom.AH_OutstandingAmount = 0M;
			testARTransfer.TransferFrom.AH_FullyPaidDate = ZDateTime.Today;
			testARTransfer.TransferTo.AH_FullyPaidDate = ZDateTime.Today;

			TransactionMatchLink aPContraMatch = GenerateMatchLinks_ForTestOnly(testAPRow);
			aPContraMatch.AP_MatchGroupNum = "M00001558";
			aPContraMatch.AP_AH = testAPRow.PK;
			PayMatchLink_ForTestOnly(aPContraMatch, 90m);

			TransactionMatchLink aRContraMatch = GenerateMatchLinks_ForTestOnly(testAPRow);
			aRContraMatch.AP_MatchGroupNum = "M00001558";
			aRContraMatch.AP_AH = testARRow.PK;
			PayMatchLink_ForTestOnly(aRContraMatch, -90m);

			TransactionMatchLink aRTransferFromMatch = GenerateMatchLinks_ForTestOnly(testAPRow);
			aRTransferFromMatch.AP_MatchGroupNum = "M00001558";
			aRTransferFromMatch.AP_AH = testARTransfer.TransferFrom.PK;
			PayMatchLink_ForTestOnly(aRTransferFromMatch, -39m);

			TransactionMatchLink aRTransferToMatch = GenerateMatchLinks_ForTestOnly(testAPRow);
			aRTransferToMatch.AP_MatchGroupNum = "M00001558";
			aRTransferToMatch.AP_AH = testARTransfer.TransferTo.PK;
			PayMatchLink_ForTestOnly(aRTransferToMatch, 39m);
			TestObjectCreator.SetupMatchLinkMatchDate(testAPRow);

			Factory.Save();

			TestUnmatchingRow.UnmatchNonPaymentGroup();
			Factory.Save();

			// What should happen:
			// -Reversing contra rows should have the same transaction number
			// -Reversing transfer rows should have the same transaction number
			// -All original matchlinks should be deleted
			// -All matchlinks should have the same match group number

			Assert("Original matchlink for APContra should be deleted", aPContraMatch.IsDeleted);
			Assert("Original matchlink for ARContra should be deleted", aRContraMatch.IsDeleted);
			Assert("Original matchlink for ARTransferFrom should be deleted", aRTransferFromMatch.IsDeleted);
			Assert("original matchlink for ARTransferTo should be deleted", aRTransferToMatch.IsDeleted);

			TransactionMatchLinkCollection matchlinks = new TransactionMatchLinkCollection(Factory, new ZQuery());
			matchlinks.Load();
			AssertEquals("There should be 8 matchlinks in the DB", 8, matchlinks.Count);

			// Pull out Reversing TransferFrom
			ZQuery revTransferFromFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Transfer);
			revTransferFromFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, testARTransfer.TransferFrom.PK);
			revTransferFromFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			ZQuery transferFromFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionCount, (byte)1);
			transferFromFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionCount, SQLComparisonOperator.Equal, (byte)3);
			revTransferFromFilter.AddToFilter(transferFromFilter, JoinCondition.And);
			ARTransferFromRow revTransferFromRow = Factory.LoadTop1(typeof(ARTransferFromRow), revTransferFromFilter) as ARTransferFromRow;
			AssertNotNull("Reversing TransferFromRow should be created", revTransferFromRow);

			// Pull out Reversing TransferTo
			ZQuery revTransferToFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Transfer);
			revTransferToFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, testARTransfer.TransferTo.PK);
			revTransferToFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			ZQuery transferToFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionCount, (byte)2);
			transferToFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionCount, SQLComparisonOperator.Equal, (byte)4);
			revTransferToFilter.AddToFilter(transferToFilter, JoinCondition.And);
			ARTransferToRow revTransferToRow = Factory.LoadTop1(typeof(ARTransferToRow), revTransferToFilter) as ARTransferToRow;
			AssertNotNull("Reversing TransferToRow should be created", revTransferToRow);

			// Pull out Reversing APContraRow
			ZQuery revAPContraRowFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Contra);
			revAPContraRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsPayable);
			revAPContraRowFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, testAPRow.PK);
			revAPContraRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			APContraRow revAPContraRow = Factory.LoadTop1(typeof(APContraRow), revAPContraRowFilter) as APContraRow;
			AssertNotNull("Reversing APContraRow should be created", revAPContraRow);

			// Pull out Reversing ARContraRow
			ZQuery revARContraRowFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Contra);
			revARContraRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			revARContraRowFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, testARRow.PK);
			revARContraRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			ARContraRow revARContraRow = Factory.LoadTop1(typeof(ARContraRow), revARContraRowFilter) as ARContraRow;
			AssertNotNull("Reversing ARContraRow should be created", revARContraRow);

			// Pull out Reversing TransferFrom Matchlink
			ZQuery revTransferFromMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revTransferFromRow.PK);
			TransactionMatchLink revTransferFromMatch = Factory.LoadTop1(typeof(TransactionMatchLink), revTransferFromMatchFilter) as TransactionMatchLink;
			AssertNotNull("Reversing TransferFromRow should be matched", revTransferFromMatch);
			AssertEquals("Match amount should be 39", 39M, revTransferFromMatch.AP_Amount);
			ZString commonMatchGroupNum = revTransferFromMatch.AP_MatchGroupNum;

			// Pull out Reversing TransferTo Matchlink
			ZQuery revTransferToMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revTransferToRow.PK);
			TransactionMatchLink revTransferToMatch = Factory.LoadTop1(typeof(TransactionMatchLink), revTransferToMatchFilter) as TransactionMatchLink;
			AssertNotNull("Reversing TransferToRow should be matched", revTransferToMatch);
			AssertEquals("Match amount should be -39M", -39M, revTransferToMatch.AP_Amount);
			AssertEquals("Match number should be same for all matchlinks", commonMatchGroupNum, revTransferToMatch.AP_MatchGroupNum);

			// Pull out Reversing APContraRow Matchlink
			ZQuery revAPContraMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revAPContraRow.PK);
			TransactionMatchLink revAPContraMatch = Factory.LoadTop1(typeof(TransactionMatchLink), revAPContraMatchFilter) as TransactionMatchLink;
			AssertNotNull("Reversing APContraRow should be matched", revAPContraMatch);
			AssertEquals("Match amount should be -90", -90M, revAPContraMatch.AP_Amount);
			//AssertEquals("Match number should be same for all matchlinks", CommonMatchGroupNum, RevAPContraMatch.AP_MatchGroupNum);
		}

		#endregion

		#region TestTransactionsAreFilteredOnCurrentCompany

		[Integration.Testing.SuspendCriticalValidateTransactionHeaderSameCompany]
		public void TestMatchlinksAreFilteredOnCurrentCompany()
		{
			GlbCompany testCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch testBranch = Factory.NewWithValidTestData<GlbBranch>();
			testBranch.GB_GC = testCompany.PK;

			GlbBranch currCompBranch = Factory.NewWithValidTestData<GlbBranch>();
			currCompBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			// Invoices from another company
			APInvoice aPInvTestComp = Factory.NewWithValidTestData<APInvoice>();
			aPInvTestComp.AH_GB = testBranch.PK;
			TestObjectCreator.CreateInvoiceLine(aPInvTestComp, aPInvTestComp.TransactionCurrency, aPInvTestComp.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);
			aPInvTestComp.AH_LocalOutstandingAmount = 0M;

			TransactionMatchLink aPInvTestCompMatch = GenerateMatchLinks_ForTestOnly(aPInvTestComp);
			aPInvTestCompMatch.AP_AH = aPInvTestComp.PK;
			PayMatchLink_ForTestOnly(aPInvTestCompMatch, -100m);
			aPInvTestCompMatch.AP_MatchGroupNum = "M00001450";

			ARInvoice aRInvTestComp = Factory.NewWithValidTestData<ARInvoice>();
			aRInvTestComp.AH_GB = testBranch.PK;
			TestObjectCreator.CreateInvoiceLine(aRInvTestComp, aRInvTestComp.TransactionCurrency, aRInvTestComp.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);
			aRInvTestComp.AH_LocalOutstandingAmount = 0M;

			TransactionMatchLink aRInvTestCompMatch = GenerateMatchLinks_ForTestOnly(aPInvTestComp);
			aRInvTestCompMatch.AP_AH = aRInvTestComp.PK;
			PayMatchLink_ForTestOnly(aRInvTestCompMatch, 100m);
			aRInvTestCompMatch.AP_MatchGroupNum = "M00001450";

			// Invoice from current company current branch
			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_GB = GlbBranch.CurrentBranch.PK;
			TestObjectCreator.CreateInvoiceLine(aRInv, aRInv.TransactionCurrency, aRInv.AH_ExchangeRate, 99m, 0m, 0m, 99m, 0m, 0m);
			aRInv.AH_LocalOutstandingAmount = 0M;

			TransactionMatchLink aRInvMatch = GenerateMatchLinks_ForTestOnly(aPInvTestComp);
			aRInvMatch.AP_AH = aRInv.PK;
			aRInvMatch.AP_MatchGroupNum = "M00001450";
			PayMatchLink_ForTestOnly(aRInvMatch, 99m);

			// Invoice from current company different branch
			APInvoice aPInv2 = Factory.NewWithValidTestData<APInvoice>();
			aPInv2.AH_GB = currCompBranch.PK;
			TestObjectCreator.CreateInvoiceLine(aPInv2, aPInv2.TransactionCurrency, aPInv2.AH_ExchangeRate, 99m, 0m, 0m, 99m, 0m, 0m);
			aPInv2.AH_LocalOutstandingAmount = 0M;

			TransactionMatchLink aPInv2Match = GenerateMatchLinks_ForTestOnly(aPInvTestComp);
			aPInv2Match.AP_AH = aPInv2.PK;
			aPInv2Match.AP_MatchGroupNum = "M00001450";
			PayMatchLink_ForTestOnly(aPInv2Match, -99m);
			TestObjectCreator.SetupMatchLinkMatchDate(aPInvTestComp);

			Factory.Save();

			TestUnmatchingRow.MatchGroupNum = "M00001450";
			AssertEquals("Should be 2 TransactionMatchlinks in MatchlinkCollection", 2, TestUnmatchingRow.MatchLinks.Count);
			Assert("MatchlinkCollection should contain ARInvMatch", TestUnmatchingRow.MatchLinks.Contains(aRInvMatch));
			Assert("MatchlinkCollection should contain APInv2Match", TestUnmatchingRow.MatchLinks.Contains(aPInv2Match));
			Assert("MatchlinkCollection should not contain APInvMatch", !TestUnmatchingRow.MatchLinks.Contains(aPInvTestCompMatch));

			AssertEquals("Should be 2 Transactions in MatchedTransactions", 2, TestUnmatchingRow.MatchedTransactions.Count);
			Assert("MatchedTransactions should contain ARInv", TestUnmatchingRow.MatchedTransactions.Contains(aRInv));
			Assert("MatchedTransactions should not contain APInv", !TestUnmatchingRow.MatchedTransactions.Contains(aPInvTestComp));
			Assert("MatchedTransactions should contain APInv2", TestUnmatchingRow.MatchedTransactions.Contains(aPInv2));

			TestUnmatchingRow.UnmatchAnyGroup();
			Factory.Save();

			Assert("ARInv Matchlink from TestCompany should be in DB", aRInvTestCompMatch.IsInDatabase);
			Assert("APInv Matchlink from TestCompany should be in DB", aPInvTestCompMatch.IsInDatabase);
			Assert("ARInv Matchlink from current company should not be in DB", !aRInvMatch.IsInDatabase);
			Assert("APInv Matchlink from current company should not be in DB", !aPInv2Match.IsInDatabase);

			aRInv.Reload();
			aPInv2.Reload();
			aRInvTestComp.Reload();
			aPInvTestComp.Reload();

			AssertEquals("Outstanding amount of ARInv should = 99", 99M, aRInv.AH_OutstandingAmount);
			AssertEquals("Outstanding amount of APInv2 should = -99", -99M, aPInv2.AH_OutstandingAmount);
			AssertEquals("ARInv from Testcomp should have outstandingamt = 0", 0M, aRInvTestComp.AH_OutstandingAmount);
			AssertEquals("APInv from Testcomp should have outstandingamt = 0", 0M, aPInvTestComp.AH_OutstandingAmount);
		}

		#endregion

		public void TestUnmatchWithBankFeeTransaction()
		{
			SetupDataSet1();
			TestUnmatchingRow.MatchGroupNum = "M00001000";

			ARINV1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(ARINV1, ARINV1.TransactionCurrency, ARINV1.AH_ExchangeRate, 10m, 0m, 0m, 10m, 0m, 0m);
			ARINV1.AH_OutstandingAmount = 0M;
			ARINV1.AH_FullyPaidDate = ZDateTime.Today;

			ARJournal bankFeeJournal = Factory.New<ARJournal>();
			bankFeeJournal.AH_OH = TestOrg1.PK;
			bankFeeJournal.AH_OSExTaxAmount = -10m;
			bankFeeJournal.AH_OutstandingAmount = 0m;
			bankFeeJournal.AH_FullyPaidDate = ZDateTime.Today;
			bankFeeJournal.AH_TransactionCreatedByMatching = true;

			TransactionMatchLink aRINV1Match = GenerateMatchLinks_ForTestOnly(ARINV1);
			aRINV1Match.AP_AH = ARINV1.PK;
			PayMatchLink_ForTestOnly(aRINV1Match, 10m);
			aRINV1Match.AP_MatchGroupNum = "M00001000";
			aRINV1Match.AP_MatchDate = ZDateTime.Today;

			TransactionMatchLink aRJNLMatch = GenerateMatchLinks_ForTestOnly(ARINV1);
			aRJNLMatch.AP_AH = bankFeeJournal.PK;
			PayMatchLink_ForTestOnly(aRJNLMatch, -10m);
			aRJNLMatch.AP_MatchGroupNum = "M00001000";
			aRJNLMatch.AP_MatchDate = ZDateTime.Today;
			Factory.Save();

			AssertEquals("M00001000 should be able to be unmatched", UnmatchingResult.Success, TestUnmatchingRow.CanUnmatchThisMatchGroup);
			TestUnmatchingRow.UnmatchNonPaymentGroup();
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			ARINV1.Reload();
			AssertEquals("ARINV1 should have outstanding amount 10", 10M, ARINV1.AH_OutstandingAmount);
			AssertEquals("ARINV1 should have fully paid date = null", ZDateTime.Empty, ARINV1.AH_FullyPaidDate);

			bankFeeJournal.Reload();
			AssertEquals("Should still be fully paid", ZDateTime.Today.Date, bankFeeJournal.AH_FullyPaidDate.Date);
			AssertEquals("Should still be fully paid", 0m, bankFeeJournal.AH_OutstandingAmount);
			Assert("Should still be fully paid", bankFeeJournal.AH_IsCancelled);
			TransactionMatchLink[] matchLinks = Factory.Load<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, bankFeeJournal.PK));
			AssertEquals("Should only be one matchlink", 1, matchLinks.Length);
			TransactionMatchLink originalBankFeeMatchLink = matchLinks[0];
			AssertEquals("Should be for the correct amount", -10m, originalBankFeeMatchLink.AP_Amount);
			AssertEquals("Should be for the correct date", ZDateTime.Today.Date, originalBankFeeMatchLink.AP_MatchDate.Date);

			ARJournal[] reversingJournals = Factory.Load<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, bankFeeJournal.PK).AddToFilter(AccTransactionHeaderSchema.AH_GC, bankFeeJournal.AH_GC));
			AssertEquals("Should only be one reversing journal", 1, reversingJournals.Length);
			Assert("Should only be one reversing journal", reversingJournals[0].AH_IsCancelled);
			matchLinks = Factory.Load<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, reversingJournals[0].PK));
			AssertEquals("Should only be one matchlink", 1, matchLinks.Length);
			TransactionMatchLink reversingBankFeeJournalMatchLink = matchLinks[0];
			AssertEquals("Should be for the correct amount", 10m, reversingBankFeeJournalMatchLink.AP_Amount);
			AssertEquals("Should be for the correct date", ZDateTime.Today.Date, reversingBankFeeJournalMatchLink.AP_MatchDate.Date);

			AssertEquals("Should be in the same match group", originalBankFeeMatchLink.AP_MatchGroupNum, reversingBankFeeJournalMatchLink.AP_MatchGroupNum);
		}

		public void TestUnmatchClearingJournal()
		{
			SetupDataSet1();
			TestUnmatchingRow.MatchGroupNum = "M00001000";

			ARINV1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(ARINV1, ARINV1.TransactionCurrency, ARINV1.AH_ExchangeRate, 10m, 0m, 0m, 10m, 0m, 0m);
			ARINV1.AH_OutstandingAmount = 0M;
			ARINV1.AH_FullyPaidDate = ZDateTime.Today;

			ARJournal journal = Factory.New<ARJournal>();
			journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.Clearing;
			journal.AH_OH = TestOrg1.PK;
			journal.AH_OSExTaxAmount = -10m;
			journal.AH_OutstandingAmount = 0m;
			journal.AH_FullyPaidDate = ZDateTime.Today;
			journal.AH_TransactionCreatedByMatching = true;

			TransactionMatchLink aRINV1Match = GenerateMatchLinks_ForTestOnly(ARINV1);
			aRINV1Match.AP_AH = ARINV1.PK;
			PayMatchLink_ForTestOnly(aRINV1Match, 10m);
			aRINV1Match.AP_MatchGroupNum = "M00001000";
			aRINV1Match.AP_MatchDate = ZDateTime.Today;

			TransactionMatchLink aRJNLMatch = GenerateMatchLinks_ForTestOnly(ARINV1);
			aRJNLMatch.AP_AH = journal.PK;
			PayMatchLink_ForTestOnly(aRJNLMatch, -10m);
			aRJNLMatch.AP_MatchGroupNum = "M00001000";
			aRJNLMatch.AP_MatchDate = ZDateTime.Today;
			Factory.Save();

			AssertEquals("M00001000 should be able to be unmatched", UnmatchingResult.Success, TestUnmatchingRow.CanUnmatchThisMatchGroup);
			TestUnmatchingRow.UnmatchAnyGroup();

			AssertEquals("ARINV1 should have outstanding amount 10", 10M, ARINV1.AH_OutstandingAmount);
			AssertEquals("ARINV1 should have fully paid date = null", ZDateTime.Empty, ARINV1.AH_FullyPaidDate);

			AssertEquals("Should still be fully paid", ZDateTime.Today.Date, journal.AH_FullyPaidDate.Date);
			AssertEquals("Should still be fully paid", 0m, journal.AH_OutstandingAmount);
			Assert("Should still be fully paid", journal.AH_IsCancelled);
			TransactionMatchLink[] matchLinks = Factory.Load<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, journal.PK));
			AssertEquals("Should only be one matchlink", 1, matchLinks.Length);
			TransactionMatchLink originalJournalMatchLink = matchLinks[0];
			AssertEquals("Should be for the correct amount", -10m, originalJournalMatchLink.AP_Amount);
			AssertEquals("Should be for the correct date", ZDateTime.Today.Date, originalJournalMatchLink.AP_MatchDate.Date);

			ARJournal[] reversingJournals = Factory.Load<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, journal.PK).AddToFilter(AccTransactionHeaderSchema.AH_GC, journal.AH_GC));
			AssertEquals("Should only be one reversing journal", 1, reversingJournals.Length);
			Assert("Should only be one reversing journal", reversingJournals[0].AH_IsCancelled);
			matchLinks = Factory.Load<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, reversingJournals[0].PK));
			AssertEquals("Should only be one matchlink", 1, matchLinks.Length);
			TransactionMatchLink reversingJournalMatchLink = matchLinks[0];
			AssertEquals("Should be for the correct amount", 10m, reversingJournalMatchLink.AP_Amount);
			AssertEquals("Should be for the correct date", ZDateTime.Today.Date, reversingJournalMatchLink.AP_MatchDate.Date);

			AssertEquals("Should be in the same match group", originalJournalMatchLink.AP_MatchGroupNum, reversingJournalMatchLink.AP_MatchGroupNum);

			TestUnmatchingRow.UnmatchDate = ZDateTime.BrettsBirthday;
			TestUnmatchingRow.UpdateUnmatchDateForAlreadyUnmatchedTransactions();
			AssertEquals("Date should not be changed.", ZDateTime.Today.Date, journal.AH_PostDate.Date);
			AssertEquals("Date should be updated.", ZDateTime.BrettsBirthday.Date, reversingJournals[0].AH_PostDate.Date);
		}

		public void TestValidateUnmatchDate()
		{
			const string futurePostingErrorMessagesRegistryIsNotEnabled = @"The post date cannot be in the future.
Ability to future post cashbook transactions and matching is controlled by the Allow Future Posting of Cash Book Transactions registry, and Allow Future Posting security.";

			const string futurePostingErrorMessagesUserHasNoSecurity = @"The post date cannot be in the future.
If you need to set a future date, it requires the following security permission: Manage -> Cash Book -> Cashbook Transactions -> Allow Future Posting";

			AssertValidateUnmatchDate<APCreditNote>(1);
			AssertValidateUnmatchDate<APPayment>(2);

			void AssertValidateUnmatchDate<T>(int seq)
				where T : TransactionHeader , IMatching
			{
				var testUnmatchingRow = (UnmatchingRow)GetNewBusinessObject();

				var matchGroupNum =  $"M00001488_{seq}";
				testUnmatchingRow.MatchGroupNum = matchGroupNum;

				T aPPAY = (T)Factory.NewWithValidTestData(typeof(T));
				APInvoice aPINV = Factory.NewWithValidTestData<APInvoice>();

				TransactionMatchLink aPPAYMatch = GenerateMatchLinks_ForTestOnly(aPPAY);
				aPPAYMatch.AP_AH = aPPAY.PK;
				aPPAYMatch.AP_MatchGroupNum = matchGroupNum;

				TransactionMatchLink aPINVMatch = GenerateMatchLinks_ForTestOnly(aPPAY);
				aPINVMatch.AP_AH = aPINV.PK;
				aPINVMatch.AP_MatchGroupNum = matchGroupNum;
				TestObjectCreator.SetupMatchLinkMatchDate(aPPAY);

				AccountingConfigurationRegistry.Instance
					.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);

				testUnmatchingRow.UnmatchDate = ZDateTime.Today;
				AssertNoError(testUnmatchingRow.UnmatchDateInfo, futurePostingErrorMessagesRegistryIsNotEnabled);

				testUnmatchingRow.UnmatchDate = ZDateTime.Today.AddDays(1);
				AssertHasError(testUnmatchingRow.UnmatchDateInfo, futurePostingErrorMessagesRegistryIsNotEnabled);

				AccountingConfigurationRegistry.Instance
					.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
				testUnmatchingRow.UnmatchDate = ZDateTime.Today.AddDays(1);
				AssertNoError(testUnmatchingRow.UnmatchDateInfo, futurePostingErrorMessagesRegistryIsNotEnabled);

				TestObjectCreator.ResetSecurityCore();

				Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;
				testUnmatchingRow.UnmatchDate = ZDateTime.Today.AddDays(1);
				AssertHasError(testUnmatchingRow.UnmatchDateInfo, futurePostingErrorMessagesUserHasNoSecurity);

				Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;
				testUnmatchingRow.UnmatchDate = ZDateTime.Today.AddDays(1);
				AssertNoError(testUnmatchingRow.UnmatchDateInfo, futurePostingErrorMessagesUserHasNoSecurity);
			}
		}

		#region TestUnmatchAnyGroup

		public void TestUnmatchAnyGroup()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_IsCreditor = true;

			Factory.Save();

			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			aPPay.AH_LocalExTaxAmount = 10M;
			aPPay.AH_OSExTaxAmount = 10M;
			aPPay.AH_LocalOutstandingAmount = 0M;
			aPPay.AH_FullyPaidDate = ZDateTime.Now;
			aPPay.AH_OH = testOrg.PK;

			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			TestObjectCreator.CreateInvoiceLine(aPInv, aPInv.TransactionCurrency, aPInv.AH_ExchangeRate, 10m, 0m, 0m, 10m, 0m, 0m);
			aPInv.AH_LocalOutstandingAmount = 0M;
			aPInv.AH_FullyPaidDate = ZDateTime.Now;
			aPInv.AH_OH = testOrg.PK;

			TransactionMatchLink aPPayMatch = GenerateMatchLinks_ForTestOnly(aPPay);
			aPPayMatch.AP_AH = aPPay.PK;
			PayMatchLink_ForTestOnly(aPPayMatch, 10m);
			aPPayMatch.AP_MatchGroupNum = "M00001445";

			TransactionMatchLink aPInvMatch = GenerateMatchLinks_ForTestOnly(aPPay);
			aPInvMatch.AP_AH = aPInv.PK;
			PayMatchLink_ForTestOnly(aPInvMatch, -10m);
			aPInvMatch.AP_MatchGroupNum = "M00001445";
			TestObjectCreator.SetupMatchLinkMatchDate(aPPay);

			Factory.Save();

			TestUnmatchingRow.MatchGroupNum = "M00001445";
			TestUnmatchingRow.UnmatchAnyGroup();
			Factory.Save();

			aPPay.Reload();
			aPInv.Reload();

			AssertEquals("APPay should have outstandingamount = 10", 10M, aPPay.AH_OutstandingAmount);
			AssertEquals("APInvoice should have outstandingamount = -10", -10M, aPInv.AH_OutstandingAmount);
			Assert("Matchlink for APPay should be deleted", aPPayMatch.IsDeleted);
			Assert("Matchlink for APInv should be deleted", aPInvMatch.IsDeleted);

			TransactionMatchLinkCollection matchlinks = new TransactionMatchLinkCollection(Factory);
			matchlinks.Load();
			AssertEquals("There should be no matchlinks in the DB", 0, matchlinks.Count);
		}

		[TestDate(2018, 10, 10)]
		public void TestUnmatchFailingToDeleteMatchLinkTriggersCriticalValidation()
		{
			var org = TestObjectCreator.CreateOrgHeader("TESTCREDIT", true, false);

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, org, ZDateTime.Today);
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 100M, 0M, 0M, TestObjectCreator.GLHeader1.PK);

			var matchGroupNumber = "M001001";
			TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(invoice, ZDateTime.Today, null, matchGroupNumber, 1M);

			Factory.Save();

			TestUnmatchingRow.MatchGroupNum = matchGroupNumber;

			var matchlinks = TestUnmatchingRow.MatchLinks;
			AssertEquals("2 matchlinks linked to this match group", 2, matchlinks.Count);
			AssertEquals("Matchlinks not deleted", 0, matchlinks.Count(x => x.IsDeleted));

			TestUnmatchingRow.MatchLinks.Cast<TransactionMatchLink>().ForEach(x => x.SkipDeletingMatchLinksForTestOnly = true);

			string expectedErrMsg = "Incorrect outstanding amount";
			string expectedExtraExceptionMessage = "Could not delete match link";
			try
			{
				TestUnmatchingRow.UnmatchAnyGroup();
			}
			catch (CannotDeleteException)
			{ }

			try
			{
				Factory.Save();
				Fail("Critical Validation should prevent saving");
			}
			catch (OnSavingCriticalCheckException ex)
			{
				AssertContains("Critical Validation Error", expectedErrMsg, ex.Message);
				AssertContains("Critical Validation Error", expectedExtraExceptionMessage, ex.DeveloperErrorMessage);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestUnmatchAnyGroupWithCashBasisVAT()
		{
			AccountingConfigurationRegistry.Instance
				.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertUnmatchAnyGroupWithCashBasisVAT(1, ZDateTime.Now.AddDays(-7), ZDateTime.Now.AddDays(-3), ZDateTime.Now.AddDays(-3));
			AssertUnmatchAnyGroupWithCashBasisVAT(2, ZDateTime.Now.AddDays(-7), ZDateTime.Now.AddDays(7), ZDateTime.Today.Date);
			AssertUnmatchAnyGroupWithCashBasisVAT(3, ZDateTime.Now.AddDays(10), ZDateTime.Now.AddDays(7), ZDateTime.Today.Date);
			AssertUnmatchAnyGroupWithCashBasisVAT(4, ZDateTime.Now.AddDays(10), ZDateTime.Now.AddDays(-3), ZDateTime.Now.AddDays(-3));

			AccountingConfigurationRegistry.Instance
				.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;
			AssertUnmatchAnyGroupWithCashBasisVAT(5, ZDateTime.Now.AddDays(-7), ZDateTime.Now.AddDays(-3), ZDateTime.Now.AddDays(-3));
			AssertUnmatchAnyGroupWithCashBasisVAT(6, ZDateTime.Now.AddDays(-7), ZDateTime.Now.AddDays(7), ZDateTime.Today.Date);
			AssertUnmatchAnyGroupWithCashBasisVAT(7, ZDateTime.Now.AddDays(10), ZDateTime.Now.AddDays(7), ZDateTime.Today.Date);
			AssertUnmatchAnyGroupWithCashBasisVAT(8, ZDateTime.Now.AddDays(10), ZDateTime.Now.AddDays(-3), ZDateTime.Now.AddDays(-3));

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;
			AssertUnmatchAnyGroupWithCashBasisVAT(9, ZDateTime.Now.AddDays(-7), ZDateTime.Now.AddDays(-3), ZDateTime.Now.AddDays(-3));
			AssertUnmatchAnyGroupWithCashBasisVAT(10, ZDateTime.Now.AddDays(-7), ZDateTime.Now.AddDays(7), ZDateTime.Now.AddDays(7));
			AssertUnmatchAnyGroupWithCashBasisVAT(11, ZDateTime.Now.AddDays(5), ZDateTime.Now.AddDays(7), ZDateTime.Now.AddDays(7));
			AssertUnmatchAnyGroupWithCashBasisVAT(12, ZDateTime.Now.AddDays(5), ZDateTime.Now.AddDays(-3), ZDateTime.Now.AddDays(-3));
		}

		void AssertUnmatchAnyGroupWithCashBasisVAT(int seq, ZDateTime matchDate, ZDateTime unmatchDate, ZDateTime expectedUnmatchDate)
		{
			var testUnmatchingRow = (UnmatchingRow)GetNewBusinessObject();

			var apInvoice = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(APInvoice), 8, 2);
			apInvoice.AH_LocalOutstandingAmount = 0M;
			apInvoice.AH_FullyPaidDate = ZDateTime.Now;
			apInvoice.AH_TransactionNum += $"_{seq}";

			var apPayment = Factory.NewWithValidTestData<APPayment>();
			apPayment.AH_LocalExTaxAmount = 10M;
			apPayment.AH_OSExTaxAmount = 10M;
			apPayment.AH_LocalOutstandingAmount = 0M;
			apPayment.AH_FullyPaidDate = ZDateTime.Now;
			apPayment.AH_OH = apInvoice.AH_OH;
			apPayment.AH_TransactionNum += $"_{seq}";

			var matchGroupNumber = $"M00001445_{seq}";
			var apPayMatchLink = GenerateMatchLinks_ForTestOnly(apPayment);
			apPayMatchLink.AP_AH = apPayment.PK;
			PayMatchLink_ForTestOnly(apPayMatchLink, 10m);
			apPayMatchLink.AP_MatchGroupNum = matchGroupNumber;
			apPayMatchLink.AP_MatchDate = matchDate;

			var apInvMatchLink = GenerateMatchLinks_ForTestOnly(apPayment);
			apInvMatchLink.AP_AH = apInvoice.PK;
			PayMatchLink_ForTestOnly(apInvMatchLink, -10m);
			apInvMatchLink.AP_MatchGroupNum = matchGroupNumber;
			apInvMatchLink.AP_MatchDate = matchDate;

			Factory.Save();

			var chashVATs = Factory.Load<AccCashBasisVAT>(new ZQuery(AccCashBasisVATSchema.YC_MatchGroupNum, matchGroupNumber).AddToFilter(AccCashBasisVATSchema.YC_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("Precondition: Cash Basis VAT record must be created.", 1, chashVATs.Length);
			AssertEquals("Precondition: Cash Basis VAT record total amount", -10m, chashVATs.Sum(item => item.YC_TaxBaseAmount + item.YC_TaxAmount));
			AssertEquals("Precondition: Cash Basis VAT record post date", matchDate, chashVATs[0].YC_PostDate);
			AssertEquals("Precondition: Cash Basis VAT record must be linked to invoice line", apInvoice.Lines[0].PK, chashVATs[0].YC_AL_TransactionLine);

			testUnmatchingRow.MatchGroupNum = matchGroupNumber;
			testUnmatchingRow.UnmatchDate = unmatchDate;
			testUnmatchingRow.UnmatchAnyGroup();

			chashVATs = Factory.Load<AccCashBasisVAT>(new ZQuery(AccCashBasisVATSchema.YC_MatchGroupNum, matchGroupNumber).AddToFilter(AccCashBasisVATSchema.YC_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("Cash Basis VAT record must be reversed.", 2, chashVATs.Length);
			AssertEquals("Cash Basis VAT record total amount", 0m, chashVATs.Sum(item => item.YC_TaxBaseAmount + item.YC_TaxAmount));
			AssertEquals("Original Cash Basis VAT record post date", 2, chashVATs.Where(item => item.YC_AL_TransactionLine == apInvoice.Lines[0].PK).Count());
			AssertEquals("Original Cash Basis VAT record post date", 1, chashVATs.Where(item => item.YC_PostDate == matchDate).Count());
			AssertEquals("Reversed Cash Basis VAT record post date", 1, chashVATs.Where(item => item.YC_PostDate == expectedUnmatchDate).Count());
			AssertEquals("Both Cash Basis VAT records must be linked to invoice line", 2, chashVATs.Where(item => item.YC_AL_TransactionLine == apInvoice.Lines[0].PK).Count());

			var updatedUnmatchDate = ZDateTime.Now.AddDays(-1);
			testUnmatchingRow.UnmatchDate = updatedUnmatchDate;
			testUnmatchingRow.UpdateUnmatchDateForAlreadyUnmatchedTransactions();

			chashVATs = Factory.Load<AccCashBasisVAT>(new ZQuery(AccCashBasisVATSchema.YC_MatchGroupNum, matchGroupNumber).AddToFilter(AccCashBasisVATSchema.YC_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("Cash Basis VAT record must be reversed.", 2, chashVATs.Length);
			AssertEquals("Original Cash Basis VAT record post date", 1, chashVATs.Where(item => item.YC_PostDate == matchDate).Count());
			AssertEquals("Reversed Cash Basis VAT record post date must be updated.", 1, chashVATs.Where(item => item.YC_PostDate == updatedUnmatchDate).Count());

			Factory.Save();

			apPayment.Reload();
			apInvoice.Reload();

			AssertEquals("APPay should have outstandingamount = 10", 10M, apPayment.AH_OutstandingAmount);
			AssertEquals("APInvoice should have outstandingamount = -10", -10M, apInvoice.AH_OutstandingAmount);
			Assert("Matchlink for APPay should be deleted", apPayMatchLink.IsDeleted);
			Assert("Matchlink for APInv should be deleted", apInvMatchLink.IsDeleted);

			var matchlinks = new TransactionMatchLinkCollection(Factory);
			matchlinks.Load();
			AssertEquals("There should be no matchlinks in the DB", 0, matchlinks.Count);

			chashVATs = Factory.Load<AccCashBasisVAT>(new ZQuery(AccCashBasisVATSchema.YC_MatchGroupNum, matchGroupNumber).AddToFilter(AccCashBasisVATSchema.YC_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("Cash Basis VAT record must be reversed.", 2, chashVATs.Length);
			AssertEquals("Cash Basis VAT record total amount", 0m, chashVATs.Sum(item => item.YC_TaxBaseAmount + item.YC_TaxAmount));
			AssertEquals("Original Cash Basis VAT record post date", 2, chashVATs.Where(item => item.YC_AL_TransactionLine == apInvoice.Lines[0].PK).Count());
			AssertEquals("Original Cash Basis VAT record post date", 1, chashVATs.Where(item => item.YC_PostDate == matchDate).Count());
			AssertEquals("Reversed Cash Basis VAT record post date", 1, chashVATs.Where(item => item.YC_PostDate == updatedUnmatchDate).Count());
			AssertEquals("Both Cash Basis VAT records must be linked to invoice line", 2, chashVATs.Where(item => item.YC_AL_TransactionLine == apInvoice.Lines[0].PK).Count());

			testUnmatchingRow.UnmatchDate = ZDateTime.Now;
			testUnmatchingRow.UnmatchAnyGroup();
			testUnmatchingRow.UpdateUnmatchDateForAlreadyUnmatchedTransactions();
			chashVATs = Factory.Load<AccCashBasisVAT>(new ZQuery(AccCashBasisVATSchema.YC_MatchGroupNum, matchGroupNumber).AddToFilter(AccCashBasisVATSchema.YC_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("The second call of UnmatchAnyGroup shouldn't change anything in existed Cash Basis VAT records.", 2, chashVATs.Length);
			AssertEquals("Cash Basis VAT record total amount", 0m, chashVATs.Sum(item => item.YC_TaxBaseAmount + item.YC_TaxAmount));
			AssertEquals("Original Cash Basis VAT record post date", 2, chashVATs.Where(item => item.YC_AL_TransactionLine == apInvoice.Lines[0].PK).Count());
			AssertEquals("Original Cash Basis VAT record post date", 1, chashVATs.Where(item => item.YC_PostDate == matchDate).Count());
			AssertEquals("Reversed Cash Basis VAT record post date", 1, chashVATs.Where(item => item.YC_PostDate == updatedUnmatchDate).Count());
			AssertEquals("Both Cash Basis VAT records must be linked to invoice line", 2, chashVATs.Where(item => item.YC_AL_TransactionLine == apInvoice.Lines[0].PK).Count());
		}

		#endregion

		#region TestDelete

		public void TestDelete()
		{
			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			TestObjectCreator.CreateInvoiceLine(aRInv, aRInv.TransactionCurrency, aRInv.AH_ExchangeRate, 180m, 10m, 0m, 180m, 10m, 0m);
			aRInv.AH_LocalOutstandingAmount = 0M;
			aRInv.AH_FullyPaidDate = ZDateTime.Now;

			TransactionMatchLink aRInvMatch = GenerateMatchLinks_ForTestOnly(aRInv);
			aRInvMatch.AP_AH = aRInv.PK;
			aRInvMatch.AP_MatchGroupNum = "M00001840";
			PayMatchLink_ForTestOnly(aRInvMatch, 190m);

			ARPayment aRPay = Factory.NewWithValidTestData<ARPayment>();
			aRPay.AH_OSExTaxAmount = -180m;
			aRPay.AH_LocalExTaxAmount = -180m;
			aRPay.AH_GSTAmount = -10m;
			aRPay.AH_OSTaxAmount = -10m;
			aRPay.AH_OutstandingAmount = 0M;
			aRPay.AH_FullyPaidDate = ZDateTime.Now;
			TransactionMatchLink aRPayMatch = GenerateMatchLinks_ForTestOnly(aRInv);
			aRPayMatch.AP_AH = aRPay.PK;
			aRPayMatch.AP_MatchGroupNum = "M00001840";
			PayMatchLink_ForTestOnly(aRPayMatch, -190m);
			TestObjectCreator.SetupMatchLinkMatchDate(aRInv);

			Factory.Save();

			TestUnmatchingRow.MatchGroupNum = "M00001840";
			TestUnmatchingRow.Delete();
			Assert("The Matchlink should be deleted because even though the result is DataError we still allow them to unmatch", aRInvMatch.IsDeleted);
			AssertEquals("The outstanding amount should be 190", 190M, aRInv.AH_LocalOutstandingAmount);
		}

		public void TestContainMessageWithNotDeleteMatchLink()
		{
			var org = TestObjectCreator.CreateOrgHeader("BRAGLOMAH", true, false);

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1M, org, new ZDateTime(2019, 03, 31, 00, 00, 00));
			TestObjectCreator.CreateInvoiceLine(invoice1, invoice1.TransactionCurrency, invoice1.AH_ExchangeRate, 20333.94m, 0m, 0m, 20333.94m, 0m, 0m);
			invoice1.AH_LocalOutstandingAmount = 0M;
			invoice1.AH_FullyPaidDate = new ZDateTime(2019, 03, 31, 00, 00, 00);
			invoice1.AH_PostDate = new ZDateTime(2019, 03, 31, 00, 00, 00);
			invoice1.AH_TransactionNum = "HKG900408409";

			var aRInvMatch1 = GenerateMatchLinks_ForTestOnly(invoice1);
			aRInvMatch1.AP_AH = invoice1.PK;
			aRInvMatch1.AP_MatchGroupNum = "M00193120";
			PayMatchLink_ForTestOnly(aRInvMatch1, 20333.94m);

			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1M, org, new ZDateTime(2019, 05, 03, 00, 00, 00));
			TestObjectCreator.CreateInvoiceLine(invoice2, invoice2.TransactionCurrency, invoice2.AH_ExchangeRate, 26950.4000m, 0m, 0m, 26950.4000m, 0m, 0m);
			invoice2.AH_LocalOutstandingAmount = 0M;
			invoice2.AH_FullyPaidDate = new ZDateTime(2019, 05, 03, 00, 00, 00);
			invoice2.AH_PostDate = new ZDateTime(2019, 05, 03, 19, 51, 00);
			invoice2.AH_TransactionNum = "HKG900414805";

			var aRInvMatch2 = GenerateMatchLinks_ForTestOnly(invoice1);
			aRInvMatch2.AP_AH = invoice2.PK;
			aRInvMatch2.AP_MatchGroupNum = "M00193120";
			PayMatchLink_ForTestOnly(aRInvMatch2, 26950.4000m);

			var receipt1 = Factory.NewWithValidTestData<ARReceipt>();
			receipt1.AH_Ledger = "AR";
			receipt1.AH_TransactionType = "REC";
			receipt1.AH_ExchangeRate = 1m;
			receipt1.AH_InvoiceDate = new ZDateTime(2019, 07, 09, 16, 56, 00);
			receipt1.AH_PostDate = new ZDateTime(2019, 07, 09, 16, 56, 00);
			receipt1.AH_GSTAmount = 0m;
			receipt1.AH_OSTaxAmount = 0m;
			receipt1.AH_OSExTaxAmount = 47284.34m;
			receipt1.AH_FullyPaidDate = new ZDateTime(2019, 07, 09, 16, 56, 00);
			receipt1.AH_OutstandingAmount = 0m;
			receipt1.AH_TransactionNum = "HKG900111095";

			var aRPayMatch = GenerateMatchLinks_ForTestOnly(invoice1);
			aRPayMatch.AP_AH = receipt1.PK;
			aRPayMatch.AP_MatchGroupNum = "M00193120";
			PayMatchLink_ForTestOnly(aRPayMatch, -47284.3400M);

			TestObjectCreator.SetupMatchLinkMatchDate(invoice1);

			var invoice3 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1M, org, new ZDateTime(2019, 09, 09, 00, 00, 00));
			TestObjectCreator.CreateInvoiceLine(invoice3, invoice3.TransactionCurrency, invoice3.AH_ExchangeRate, 10333.94m, 0m, 0m, 10333.94m, 0m, 0m);
			invoice3.AH_LocalOutstandingAmount = 0M;
			invoice3.AH_FullyPaidDate = new ZDateTime(2019, 09, 09, 00, 00, 00);
			invoice3.AH_PostDate = new ZDateTime(2019, 09, 09, 00, 00, 00);
			invoice3.AH_TransactionNum = "HKG900400806";

			var aRInvMatch3 = GenerateMatchLinks_ForTestOnly(invoice3);
			aRInvMatch3.AP_AH = invoice3.PK;
			aRInvMatch3.AP_MatchGroupNum = "M00190806";
			PayMatchLink_ForTestOnly(aRInvMatch3, 10333.94m);

			var invoice4 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1M, org, new ZDateTime(2019, 05, 03, 00, 00, 00));
			TestObjectCreator.CreateInvoiceLine(invoice4, invoice4.TransactionCurrency, invoice4.AH_ExchangeRate, 16950.4000m, 0m, 0m, 16950.4000m, 0m, 0m);
			invoice4.AH_LocalOutstandingAmount = 0M;
			invoice4.AH_FullyPaidDate = new ZDateTime(2019, 05, 03, 00, 00, 00);
			invoice4.AH_PostDate = new ZDateTime(2019, 05, 03, 19, 51, 00);
			invoice4.AH_TransactionNum = "HKG900410807";

			var aRInvMatch4 = GenerateMatchLinks_ForTestOnly(invoice3);
			aRInvMatch4.AP_AH = invoice4.PK;
			aRInvMatch4.AP_MatchGroupNum = "M00190806";
			PayMatchLink_ForTestOnly(aRInvMatch4, 16950.4000m);

			var receipt2 = Factory.NewWithValidTestData<ARReceipt>();
			receipt2.AH_Ledger = "AR";
			receipt2.AH_TransactionType = "REC";
			receipt2.AH_ExchangeRate = 1m;
			receipt2.AH_InvoiceDate = new ZDateTime(2019, 07, 09, 16, 56, 00);
			receipt2.AH_PostDate = new ZDateTime(2019, 07, 09, 16, 56, 00);
			receipt2.AH_GSTAmount = 0m;
			receipt2.AH_OSTaxAmount = 0m;
			receipt2.AH_OSExTaxAmount = 27284.34m;
			receipt2.AH_FullyPaidDate = new ZDateTime(2019, 07, 09, 16, 56, 00);
			receipt2.AH_OutstandingAmount = 0m;
			receipt2.AH_TransactionNum = "HKG900110808";

			var aRPayMatch1 = GenerateMatchLinks_ForTestOnly(invoice3);
			aRPayMatch1.AP_AH = receipt2.PK;
			aRPayMatch1.AP_MatchGroupNum = "M00190806";
			PayMatchLink_ForTestOnly(aRPayMatch1, -27284.3400M);

			TestObjectCreator.SetupMatchLinkMatchDate(invoice3);

			AssertNoExceptionThrown("The invoices and receipt should save successfully", () => Factory.Save());

			using (new DisposableAction(() => TestUnmatchingRow.IsSkipDeleteMatckLinkForTestOnly = false))
			{
				TestUnmatchingRow.MatchGroupNum = "M00193120";
				TestUnmatchingRow.IsSkipDeleteMatckLinkForTestOnly = true;
				TestUnmatchingRow.Delete();
			}

			var ex = AssertExceptionThrown<OnSavingCriticalCheckException>("Critical Validation should prevent saving", () => Factory.Save());

			var expectedErrMsg1 = FormattableString.Invariant($"MatchLinkDeletionInfo: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.");

			var expectedErrMsg2 = FormattableString.Invariant($@"Related Match Links:
Match Link Factory Instance Number = {aRInvMatch1.Factory._Instance}.");

			var expectedErrMsg3 = FormattableString.Invariant($@"Match Link: Group Number = M00193120, Amount = {aRInvMatch1.AP_Amount}, OS Amount = {aRInvMatch1.AP_OSAmount}, Match Date = {new ZDateTime(2019, 03, 31, 00, 00, 00)}, Transaction PK = {invoice1.PK}, Is In DB = Yes, Has Changes = No.");

			var expectedErrMsg4 = FormattableString.Invariant($@"Match Link: Group Number = M00193120, Amount = {aRInvMatch2.AP_Amount}, OS Amount = {aRInvMatch2.AP_OSAmount}, Match Date = {new ZDateTime(2019, 05, 03, 00, 00, 00)}, Transaction PK = {invoice2.PK}, Is In DB = Yes, Has Changes = No.");

			var expectedErrMsg5 = FormattableString.Invariant($@"Match Link: Group Number = M00193120, Amount = {aRPayMatch.AP_Amount}, OS Amount = {aRPayMatch.AP_OSAmount}, Match Date = {new ZDateTime(2019, 07, 09, 00, 00, 00)}, Transaction PK = {receipt1.PK}, Is In DB = Yes, Has Changes = No.");

			AssertContains("Critical Validation Error", expectedErrMsg1, ex.DeveloperErrorMessage);
			AssertContains("Critical Validation Error", expectedErrMsg2, ex.DeveloperErrorMessage);
			AssertContains("Critical Validation Error", expectedErrMsg3, ex.DeveloperErrorMessage);
			AssertContains("Critical Validation Error", expectedErrMsg4, ex.DeveloperErrorMessage);
			AssertContains("Critical Validation Error", expectedErrMsg5, ex.DeveloperErrorMessage);

			var factorynew = NewFactory();
			TestUnmatchingRow = new UnmatchingRow(factorynew);
			using (new DisposableAction(() => TestUnmatchingRow.IsSkipDeleteMatckLinkForTestOnly = false))
			{
				TestUnmatchingRow.MatchGroupNum = "M00190806";
				TestUnmatchingRow.IsSkipDeleteMatckLinkForTestOnly = true;
				TestUnmatchingRow.Delete();
			}

			var ex1 = AssertExceptionThrown<OnSavingCriticalCheckException>("2nd Critical Validation of same type should show additional info", () => factorynew.Save());

			var expectedErrMsg6 = FormattableString.Invariant($@"Related Match Links:
Match Link Factory Instance Number = {factorynew._Instance}.");

			var expectedErrMsg7 = FormattableString.Invariant($@"Match Link: Group Number = M00190806, Amount = {aRInvMatch3.AP_Amount.ToString("0.0000", CultureInfo.CurrentCulture)}, OS Amount = {aRInvMatch3.AP_OSAmount.ToString("0.0000", CultureInfo.CurrentCulture)}, Match Date = {new ZDateTime(2019, 09, 09, 00, 00, 00)}, Transaction PK = {invoice3.PK}, Is In DB = Yes, Has Changes = No.");

			var expectedErrMsg8 = FormattableString.Invariant($@"Match Link: Group Number = M00190806, Amount = {aRInvMatch4.AP_Amount.ToString("0.0000", CultureInfo.CurrentCulture)}, OS Amount = {aRInvMatch4.AP_OSAmount.ToString("0.0000", CultureInfo.CurrentCulture)}, Match Date = {new ZDateTime(2019, 05, 03, 00, 00, 00)}, Transaction PK = {invoice4.PK}, Is In DB = Yes, Has Changes = No.");

			var expectedErrMsg9 = FormattableString.Invariant($@"Match Link: Group Number = M00190806, Amount = {aRPayMatch1.AP_Amount.ToString("0.0000", CultureInfo.CurrentCulture)}, OS Amount = {aRPayMatch1.AP_OSAmount.ToString("0.0000", CultureInfo.CurrentCulture)}, Match Date = {new ZDateTime(2019, 07, 09, 00, 00, 00)}, Transaction PK = {receipt2.PK}, Is In DB = Yes, Has Changes = No.");

			var expectedErrMsg10 = FormattableString.Invariant($@"MatchLinkDeletionInfo:
Current Factory Instance Number For MatchLink deleting:{factorynew._Instance}
The MatchLink Group Number Is:M00190806");

			var expectedErrMsg11 = FormattableString.Invariant($"Matchlink PK: {aRInvMatch3.PK}  Matchlink Parent Transaction: {invoice3.PK}  Is In MatchLinksToDelete: False");

			var expectedErrMsg12 = FormattableString.Invariant($"Matchlink PK: {aRInvMatch4.PK}  Matchlink Parent Transaction: {invoice4.PK}  Is In MatchLinksToDelete: False");

			var expectedErrMsg13 = FormattableString.Invariant($"Matchlink PK: {aRPayMatch1.PK}  Matchlink Parent Transaction: {receipt2.PK}  Is In MatchLinksToDelete: False");

			AssertContains("Critical Validation Error", expectedErrMsg6, ex1.DeveloperErrorMessage);
			AssertContains("Critical Validation Error", expectedErrMsg7, ex1.DeveloperErrorMessage);
			AssertContains("Critical Validation Error", expectedErrMsg8, ex1.DeveloperErrorMessage);
			AssertContains("Critical Validation Error", expectedErrMsg9, ex1.DeveloperErrorMessage);
			AssertContains("Critical Validation Error", expectedErrMsg10, ex1.DeveloperErrorMessage);
			AssertContains("Critical Validation Error", expectedErrMsg11, ex1.DeveloperErrorMessage);
			AssertContains("Critical Validation Error", expectedErrMsg12, ex1.DeveloperErrorMessage);
			AssertContains("Critical Validation Error", expectedErrMsg13, ex1.DeveloperErrorMessage);
			ExceptionReporterTestListener.Instance.Clear();
		}

		#endregion

		#region TestCanUnmatchThisMatchGroup

		public void TestCanUnmatchThisMatchGroup()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			aPInv.AH_LocalExTaxAmount = 10M;
			aPInv.AH_OSExTaxAmount = 10M;
			aPInv.AH_LocalOutstandingAmount = 1M;   // data error is here
			aPInv.AH_OH = testOrg.PK;

			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			aPPay.AH_LocalExTaxAmount = 10M;
			aPPay.AH_OSExTaxAmount = 10M;
			aPPay.AH_LocalOutstandingAmount = 0M;
			aPPay.AH_OH = testOrg.PK;

			TransactionMatchLink aPPayMatch = Factory.New<TransactionMatchLink>();
			aPPayMatch.AP_AH = aPPay.PK;
			PayMatchLink_ForTestOnly(aPPayMatch, 10M);
			aPPayMatch.AP_MatchGroupNum = "M00001445";

			TransactionMatchLink aPInvMatch = Factory.New<TransactionMatchLink>();
			aPInvMatch.AP_AH = aPInv.PK;
			PayMatchLink_ForTestOnly(aPInvMatch, -10M);
			aPInvMatch.AP_MatchGroupNum = "M00001445";

			TestUnmatchingRow.MatchGroupNum = "M00001445";

			TestUnmatchingRow.MatchLinks.RemoveAll();
			TestUnmatchingRow.MatchLinks.Add(aPInvMatch);   // make sure data error is the first element in the collection
			TestUnmatchingRow.MatchLinks.Add(aPPayMatch);
			AssertEquals("should be 2 matchlinks in collection", 2, TestUnmatchingRow.MatchLinks.Count);

			bool oldPayablesSecurityValue = Env.Security.PayablesAllowUnmatchingPaymentMatching.IsAllowed;
			try
			{
				Env.Security.PayablesAllowUnmatchingPaymentMatching.IsAllowed = false;
				AssertEquals("CanUnmatchThisMatchGroup should return ContainsPayment", UnmatchingResult.ContainsPayment,
					TestUnmatchingRow.CanUnmatchThisMatchGroup);

				Env.Security.PayablesAllowUnmatchingPaymentMatching.IsAllowed = true;
				AssertEquals("CanUnmatchThisMatchGroup should return DataError", UnmatchingResult.DataErrorAddingMatchAmountExceedOriginalInvoiceAmount,
					TestUnmatchingRow.CanUnmatchThisMatchGroup);
			}
			finally
			{
				Env.Security.PayablesAllowUnmatchingPaymentMatching.IsAllowed = oldPayablesSecurityValue;
			}
		}

		#endregion

		#region TestMatchLinksCollectionTypeIsCorrect

		public void TestMatchLinksCollectionTypeIsCorrect()
		{
			AssertEquals("Type of Matchlinks should be TransactionMatchLinkCollectionForUnmatching, as this does not support Load()",
				typeof(TransactionMatchLinkCollectionForUnmatching), TestUnmatchingRow.MatchLinks.GetType());
		}

		#endregion

		[ExpectNoExceptions]
		public void TestMatchGroupNumMaximumLength()
		{
			TestUnmatchingRow.MatchGroupNum = ZString.Empty.PadLeft(20, 'X');
		}

		[ExpectException(typeof(MaxLengthExceededException))]
		public void TestMatchGroupNumExcessiveLength()
		{
			try
			{
				TestUnmatchingRow.MatchGroupNum = ZString.Empty.PadLeft(21, 'X');
			}
			catch (MaxLengthExceededException)
			{
				ErrorReporter.Clear();
				throw;
			}
		}

		[TestDate(2013, 10, 17)]
		public void TestInitialize()
		{
			var unmatchingRow = new UnmatchingRow(Factory);
			var unmatchingRow2 = new UnmatchingRow(Factory);
			unmatchingRow2.MatchGroupNum = "111";
			unmatchingRow2.MatchDate = ZDateTime.BrettsBirthday;
			var matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_MatchGroupNum = "2222";
			matchLink.AP_MatchDate = ZDateTime.Today;
			var matchLinkFuture = Factory.New<TransactionMatchLink>();
			matchLinkFuture.AP_MatchGroupNum = "3333";
			matchLinkFuture.AP_MatchDate = ZDateTime.Today.AddDays(7);

			unmatchingRow.Initialize(unmatchingRow2);
			AssertEquals("111", unmatchingRow.MatchGroupNum);
			AssertEquals(ZDateTime.BrettsBirthday, unmatchingRow.MatchDate);
			AssertEquals(ZDateTime.Today, unmatchingRow.UnmatchDate);

			unmatchingRow.Initialize(matchLink);
			AssertEquals("2222", unmatchingRow.MatchGroupNum);
			AssertEquals(ZDateTime.Today, unmatchingRow.MatchDate.Date);
			AssertEquals(ZDateTime.Today, unmatchingRow.UnmatchDate);

			unmatchingRow.Initialize(matchLinkFuture);
			AssertEquals("3333", unmatchingRow.MatchGroupNum);
			AssertEquals(ZDateTime.Today.AddDays(7), unmatchingRow.MatchDate.Date);
			AssertEquals(ZDateTime.Today.AddDays(7), unmatchingRow.UnmatchDate);
		}

		public void TestSetDefaultValues()
		{
			var unmatchingRow = new UnmatchingRow(Factory);
			AssertEquals(ZDateTime.Today, unmatchingRow.UnmatchDate.Date);
		}

		public void TestUnmatchDateValidation()
		{
			var unmatchingRow = new UnmatchingRow(Factory);
			unmatchingRow.UnmatchDate = ZDateTime.Empty;
			AssertHasError(unmatchingRow.UnmatchDateInfo, "Please enter a value.");

			unmatchingRow.UnmatchDate = ZDateTime.Invalid;
			AssertHasError("Unmatch date is invalid", unmatchingRow.UnmatchDateInfo, "Enter a valid selection.");

			unmatchingRow.UnmatchDate = ZDateTime.BrettsBirthday;
			var unmatchDateInsideInvalidZDateTimeRange = string.Concat(unmatchingRow.UnmatchDate.ToShortDateString().Remove(7), unmatchingRow.UnmatchDate.Year);
			var expectedMessage = string.Format("The date '{0}' is more than 10 years old and thus is not valid.", unmatchDateInsideInvalidZDateTimeRange);
			AssertHasError("Unmatch date is inside invalid ZDateTime range", unmatchingRow.UnmatchDateInfo, expectedMessage);

			var someDate = ZDateTime.Today.AddMonths(-2);
			unmatchingRow.MatchDate = someDate;
			unmatchingRow.UnmatchDate = someDate.AddDays(-1);
			AssertHasError(unmatchingRow.UnmatchDateInfo, "The date should be greater then Match Date.");

			unmatchingRow.UnmatchDate = someDate.AddDays(1);
			AssertHasError(unmatchingRow.UnmatchDateInfo, "This date does not fall into a valid accounting period’s date range.\r\nPlease go to Manage > General Ledger > Period Management > Set Up Next Accounting Year, to ensure there is an accounting period for the date you wish to post to.");

			TestObjectCreator.CreateTestPeriods(someDate);
			unmatchingRow.RunPreSaveValidation();
			AssertNoErrors(unmatchingRow.UnmatchDateInfo);

			AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(Factory);
			AccPeriodManagement period = periodCalculator.GetPeriodManagementFromDate(unmatchingRow.UnmatchDate);
			period.AM_IsGeneralLedgerClosed = true;
			period.AM_IsSubLedgerClosed = true;
			unmatchingRow.RunPreSaveValidation();
			AssertHasError(unmatchingRow.UnmatchDateInfo, "This date falls into a period where the sub-ledger is closed");

			period.AM_IsGeneralLedgerClosed = false;
			period.AM_IsSubLedgerClosed = true;
			unmatchingRow.RunPreSaveValidation();
			AssertHasError(unmatchingRow.UnmatchDateInfo, "This date falls into a period where the sub-ledger is closed");

			period.AM_IsGeneralLedgerClosed = false;
			period.AM_IsSubLedgerClosed = false;
			unmatchingRow.RunPreSaveValidation();
			AssertNoErrors(unmatchingRow.UnmatchDateInfo);

			unmatchingRow.MatchDate = someDate.AddDays(2);
			unmatchingRow.UnmatchDate = someDate;
			unmatchingRow.RunPreSaveValidation();
			AssertHasError(unmatchingRow.UnmatchDateInfo, "The date should be greater then Match Date.");

			unmatchingRow.UnmatchDate = someDate.AddDays(3);
			AssertNoErrors(unmatchingRow.UnmatchDateInfo);
			using (unmatchingRow.GetValidationSuspender())
			{
				unmatchingRow.UnmatchDate = someDate;
				AssertNoErrors("No errors if validation is suspended.", unmatchingRow.UnmatchDateInfo);
			}
		}

		public void TestUnmatchDateValidation_Future()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			var unmatchingRow = new UnmatchingRow(Factory);
			var receipt = TestObjectCreator.CreateReceiptOrPayment(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 50, TestObjectCreator.AUDBankAccount.PK);
			unmatchingRow.MatchedTransactions.Add(receipt);
			unmatchingRow.MatchDate = ZDateTime.Today;
			unmatchingRow.UnmatchDate = ZDateTime.Today.AddDays(1);

			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			unmatchingRow.RunPreSaveValidation();
			AssertHasError(unmatchingRow.UnmatchDateInfo, AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);

			unmatchingRow.UnmatchDate = ZDateTime.Now;
			AssertNoErrors(unmatchingRow.UnmatchDateInfo);

			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();
			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;

			unmatchingRow.UnmatchDate = ZDateTime.Now.AddDays(2);
			AssertHasError("UnmatchDateInfo", unmatchingRow.UnmatchDateInfo, AccountingConstants.FuturePostingErrorMessages.UserHasNoSecurity);

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;
			unmatchingRow.UnmatchDate = ZDateTime.Now.AddDays(3);
			AssertNoErrors(unmatchingRow.UnmatchDateInfo);
		}

		public void TestUnmatchDate_ReadOnlyAPTransactionsMatched()
		{
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();

			var aPPay = Factory.NewWithValidTestData<APPayment>();
			var aPInv = Factory.NewWithValidTestData<APInvoice>();
			TransactionMatchLink aPPayMatch = GenerateMatchLinks_ForTestOnly(aPPay);
			aPPayMatch.AP_AH = aPPay.PK;
			aPPayMatch.AP_MatchGroupNum = "M00001";

			TransactionMatchLink aPInvMatch = GenerateMatchLinks_ForTestOnly(aPPay);
			aPInvMatch.AP_AH = aPInv.PK;
			aPInvMatch.AP_MatchGroupNum = "M00001";

			TestUnmatchingRow.MatchGroupNum = "M00001";
			TestUnmatchingRow.MatchLinks.AddRange(aPPayMatch);
			TestUnmatchingRow.MatchLinks.AddRange(aPInvMatch);
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(false, TestUnmatchingRow.UnmatchDateInfo.ReadOnly);
			AssertEquals(true, TestUnmatchingRow.AllowBackPosting);
			AssertEquals(false, TestUnmatchingRow.AllowFutureUnmatchDate_ForTestOnly);

			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(false, TestUnmatchingRow.UnmatchDateInfo.ReadOnly);
			AssertEquals(false, TestUnmatchingRow.AllowBackPosting);
			AssertEquals(false, TestUnmatchingRow.AllowFutureUnmatchDate_ForTestOnly);

			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, TestUnmatchingRow.UnmatchDateInfo.ReadOnly);
			AssertEquals(false, TestUnmatchingRow.AllowBackPosting);
			AssertEquals(false, TestUnmatchingRow.AllowFutureUnmatchDate_ForTestOnly);

			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, TestUnmatchingRow.UnmatchDateInfo.ReadOnly);
			AssertEquals(false, TestUnmatchingRow.AllowBackPosting);
			AssertEquals(false, TestUnmatchingRow.AllowFutureUnmatchDate_ForTestOnly);

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;
			AssertEquals(false, TestUnmatchingRow.UnmatchDateInfo.ReadOnly);
			AssertEquals(false, TestUnmatchingRow.AllowBackPosting);
			AssertEquals(true, TestUnmatchingRow.AllowFutureUnmatchDate_ForTestOnly);
		}

		public void TestUnmatchDate_ReadOnlyARTransactionsMatched()
		{
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();

			var aRPay = Factory.NewWithValidTestData<ARPayment>();
			var aRInv = Factory.NewWithValidTestData<ARInvoice>();
			TransactionMatchLink aRPayMatch = GenerateMatchLinks_ForTestOnly(aRPay);
			aRPayMatch.AP_AH = aRPay.PK;
			aRPayMatch.AP_MatchGroupNum = "M00001";

			TransactionMatchLink aRInvMatch = GenerateMatchLinks_ForTestOnly(aRPay);
			aRInvMatch.AP_AH = aRInv.PK;
			aRInvMatch.AP_MatchGroupNum = "M00001";

			TestUnmatchingRow = (UnmatchingRow)GetNewBusinessObject();
			TestUnmatchingRow.MatchGroupNum = "M00001";
			TestUnmatchingRow.MatchLinks.AddRange(aRPayMatch);
			TestUnmatchingRow.MatchLinks.AddRange(aRInvMatch);
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(false, TestUnmatchingRow.UnmatchDateInfo.ReadOnly);
			AssertEquals(true, TestUnmatchingRow.AllowBackPosting);
			AssertEquals(false, TestUnmatchingRow.AllowFutureUnmatchDate_ForTestOnly);

			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(false, TestUnmatchingRow.UnmatchDateInfo.ReadOnly);
			AssertEquals(false, TestUnmatchingRow.AllowBackPosting);
			AssertEquals(false, TestUnmatchingRow.AllowFutureUnmatchDate_ForTestOnly);

			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, TestUnmatchingRow.UnmatchDateInfo.ReadOnly);
			AssertEquals(false, TestUnmatchingRow.AllowBackPosting);
			AssertEquals(false, TestUnmatchingRow.AllowFutureUnmatchDate_ForTestOnly);

			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, TestUnmatchingRow.UnmatchDateInfo.ReadOnly);
			AssertEquals(false, TestUnmatchingRow.AllowBackPosting);
			AssertEquals(false, TestUnmatchingRow.AllowFutureUnmatchDate_ForTestOnly);

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;
			AssertEquals(false, TestUnmatchingRow.UnmatchDateInfo.ReadOnly);
			AssertEquals(false, TestUnmatchingRow.AllowBackPosting);
			AssertEquals(true, TestUnmatchingRow.AllowFutureUnmatchDate_ForTestOnly);
		}

		public void TestUnmatchDate_ReadOnlyARAndAPTransactionsMatched()
		{
			var aPPay = Factory.NewWithValidTestData<APPayment>();
			var aRInv = Factory.NewWithValidTestData<ARInvoice>();
			TransactionMatchLink aPPayMatch = GenerateMatchLinks_ForTestOnly(aPPay);
			aPPayMatch.AP_AH = aPPay.PK;
			aPPayMatch.AP_MatchGroupNum = "M00001";

			TransactionMatchLink aRInvMatch = GenerateMatchLinks_ForTestOnly(aPPay);
			aRInvMatch.AP_AH = aRInv.PK;
			aRInvMatch.AP_MatchGroupNum = "M00001";

			TestUnmatchingRow = (UnmatchingRow)GetNewBusinessObject();
			TestUnmatchingRow.MatchGroupNum = "M00001";
			TestUnmatchingRow.MatchLinks.AddRange(aPPayMatch);
			TestUnmatchingRow.MatchLinks.AddRange(aRInvMatch);
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(false, TestUnmatchingRow.UnmatchDateInfo.ReadOnly);
			AssertEquals(false, TestUnmatchingRow.AllowBackPosting);

			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(false, TestUnmatchingRow.UnmatchDateInfo.ReadOnly);
			AssertEquals(false, TestUnmatchingRow.AllowBackPosting);

			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, TestUnmatchingRow.UnmatchDateInfo.ReadOnly);
			AssertEquals(false, TestUnmatchingRow.AllowBackPosting);

			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, TestUnmatchingRow.UnmatchDateInfo.ReadOnly);
			AssertEquals(false, TestUnmatchingRow.AllowBackPosting);

			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(false, TestUnmatchingRow.UnmatchDateInfo.ReadOnly);
			AssertEquals(true, TestUnmatchingRow.AllowBackPosting);

			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(false, TestUnmatchingRow.UnmatchDateInfo.ReadOnly);
			AssertEquals(false, TestUnmatchingRow.AllowBackPosting);

			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, TestUnmatchingRow.UnmatchDateInfo.ReadOnly);
			AssertEquals(false, TestUnmatchingRow.AllowBackPosting);

			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, TestUnmatchingRow.UnmatchDateInfo.ReadOnly);
			AssertEquals(false, TestUnmatchingRow.AllowBackPosting);
		}

		public void TestIsAllowedReturnsFalseWhenCashBookAllowFuturePostingOfCashBookTransactionsIsNull()
		{
			TestObjectCreator.ResetSecurityCore();
			AssertEquals(false, Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowedWithConstraint());
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			Assert(AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.Value);

			var arPay = Factory.NewWithValidTestData<ARPayment>();
			var arInv = Factory.NewWithValidTestData<ARInvoice>();
			TransactionMatchLink arPayMatch = GenerateMatchLinks_ForTestOnly(arPay);
			arPayMatch.AP_AH = arPay.PK;
			arPayMatch.AP_MatchGroupNum = "M00001";

			TransactionMatchLink arInvMatch = GenerateMatchLinks_ForTestOnly(arPay);
			arInvMatch.AP_AH = arInv.PK;
			arInvMatch.AP_MatchGroupNum = "M00001";

			TestUnmatchingRow = (UnmatchingRow)GetNewBusinessObject();
			TestUnmatchingRow.MatchGroupNum = "M00001";
			TestUnmatchingRow.MatchLinks.AddRange(arPayMatch);
			TestUnmatchingRow.MatchLinks.AddRange(arInvMatch);

			Assert("CashBookAllowFuturePostingOfTransactions.IsAllowed should be false", !TestUnmatchingRow.AllowFutureUnmatchDate_ForTestOnly);
		}

		public void TestCheckpointsToUnmatchWithMultipleMiscTransactionTypes()
		{
			TestUnmatchingRow.MatchGroupNum = "M00001001";

			ARINV1 = Factory.New<ARInvoice>();
			var arInvoiceLine = (ARInvoiceLine)ARINV1.Lines.AddNew();
			arInvoiceLine.AL_LocalExTaxAmount = 150m;
			arInvoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			ARINV1.AH_LocalOutstandingAmount = 0M;

			ARCRD1 = Factory.New<ARCreditNote>();
			var arCreditNoteLine = (ARCreditNoteLine)ARCRD1.Lines.AddNew();
			arCreditNoteLine.AL_LocalExTaxAmount = 100m;
			arCreditNoteLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			ARCRD1.AH_LocalOutstandingAmount = 0M;

			ARDiscount aRDSC = Factory.New<ARDiscount>();
			aRDSC.AH_LocalExTaxAmount = -30M;
			aRDSC.AH_OSExTaxAmount = -30M;
			aRDSC.AH_LocalOutstandingAmount = 0M;

			AROverpayment aROVP = Factory.New<AROverpayment>();
			aROVP.AH_LocalExTaxAmount = -20M;
			aROVP.AH_OSExTaxAmount = -20M;
			aROVP.AH_LocalOutstandingAmount = 0M;

			TransactionMatchLink aRINVMatch = GenerateMatchLinks_ForTestOnly(ARINV1);
			aRINVMatch.AP_AH = ARINV1.PK;
			PayMatchLink_ForTestOnly(aRINVMatch, 150m);
			aRINVMatch.AP_MatchGroupNum = "M00001001";

			TransactionMatchLink aRCRDMatch = GenerateMatchLinks_ForTestOnly(ARINV1);
			aRCRDMatch.AP_AH = ARCRD1.PK;
			PayMatchLink_ForTestOnly(aRCRDMatch, -100m);
			aRCRDMatch.AP_MatchGroupNum = "M00001001";

			TransactionMatchLink aRDSCMatch = GenerateMatchLinks_ForTestOnly(ARINV1);
			aRDSCMatch.AP_AH = aRDSC.PK;
			PayMatchLink_ForTestOnly(aRDSCMatch, -30m);
			aRDSCMatch.AP_MatchGroupNum = "M00001001";
			aRDSCMatch.AP_MatchDate = ZDateTime.Now.AddHours(-1);

			TransactionMatchLink aROVPMatch = GenerateMatchLinks_ForTestOnly(ARINV1);
			aROVPMatch.AP_AH = aROVP.PK;
			PayMatchLink_ForTestOnly(aROVPMatch, -20m);
			aROVPMatch.AP_MatchGroupNum = "M00001001";
			aROVPMatch.AP_MatchDate = ZDateTime.Now.AddHours(-1);

			TestObjectCreator.SetupMatchLinkMatchDate(ARINV1);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Overpayment transaction type should have additional checkpoint, discount should not, and should not include null in IEnumerable", new[] { Env.Security.ReceivablesUnMatchTransactionOverpaymentType }, TestUnmatchingRow.CheckpointsToUnmatch);
		}

		TransactionMatchLink GenerateMatchLinks_ForTestOnly(TransactionHeader matchingTransaction)
		{
			var matchLink = ((IMatching)matchingTransaction).CurrentMatchGroup.AddNew();
			return matchLink;
		}

		void PayMatchLink_ForTestOnly(TransactionMatchLink matchingLink, decimal payAmount)
		{
			matchingLink.AP_Amount = payAmount;

			if (TransactionMatchLinkOSAmountProvider.IsFeatureEnabled(matchingLink))
			{
				matchingLink.AP_OSAmount = payAmount;
			}
		}
	}
}
