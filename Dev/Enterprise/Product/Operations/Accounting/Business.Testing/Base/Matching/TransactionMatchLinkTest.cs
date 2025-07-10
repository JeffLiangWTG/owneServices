using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(TransactionMatchLink))]
	public class TransactionMatchLinkTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZDecimalsHaveCorrectDecimalPlacesTransactionMatchLink_WhenTransactionHeaderIsNull()
		{
			var link = Factory.New<TransactionMatchLink>();
			AssertNull("Precondition", link.TransactionHeader);

			var localList = new List<string>
			{
				nameof(link.AP_GSTRealised),
				nameof(link.AP_Amount)
			};

			var osList = new List<string>
			{
				nameof(link.AP_OSAmount)
			};

			var tester = new DecimalPlacesAttributeTester(link);
			tester.CheckCompanyLocalCurrency(localList, nameof(link.LocalDecimals));
			tester.CheckCompanyLocalCurrency(osList, nameof(link.OSCurrencyDecimals));
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesTransactionMatchLink_WhenTransactionHeaderIsNotNull()
		{
			var link = Factory.NewWithValidTestData<TransactionMatchLink>();
			AssertNotNull("Precondition", link.TransactionHeader);

			var localList = new List<string>
			{
				nameof(link.AP_GSTRealised),
				nameof(link.AP_Amount)
			};

			var osList = new List<string>
			{
				nameof(link.AP_OSAmount)
			};

			var tester = new DecimalPlacesAttributeTester(link);
			tester.CheckLocalCurrency(localList, nameof(link.LocalDecimals));
			tester.CheckNonLocalCurrency(osList, nameof(link.OSCurrencyDecimals), nameof(link.TransactionHeader.AH_RX_NKTransactionCurrency), link.TransactionHeader);
		}

		public void TestLoadAlwaysReturnTypeOfTransacitonMatchLink()
		{
			APContraRow aPCTR1 = Factory.NewWithValidTestData<APContraRow>();
			aPCTR1.AH_LocalExTaxAmount = 5M;
			aPCTR1.AH_OSExTaxAmount = 5M;
			aPCTR1.AH_OutstandingAmount = 0M;
			aPCTR1.AH_FullyPaidDate = ZDateTime.Today;

			var testInvoice1 = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "0001", TestObjectCreator.AUD, 1m, 5m, 0m, 5m, 0m);
			testInvoice1.AH_OH = aPCTR1.AH_OH;
			testInvoice1.AH_OutstandingAmount = 0m;

			TransactionMatchLinkGroup matchLinks = new TransactionMatchLinkGroup(Factory);
			TransactionMatchLink invMatchLink = matchLinks.AddNew();
			invMatchLink.AP_AH = testInvoice1.PK;
			invMatchLink.AP_Amount = -5M;

			TransactionMatchLink matchLink = matchLinks.AddNew();
			matchLink.AP_AH = aPCTR1.PK;
			matchLink.AP_Amount = 5M;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLinks);
			Factory.Save();

			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			var matchLink1 = factory1.Load(typeof(AccTransactionMatchLink), matchLink.PK);
			AssertEquals(matchLink1.GetType(), typeof(TransactionMatchLink));

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			var matchLink2 = factory2.Load(typeof(TransactionMatchLink), matchLink.PK);
			AssertEquals(matchLink2.GetType(), typeof(TransactionMatchLink));
		}

		public void TestSetValues()
		{
			ZDateTime now = ZDateTime.Now;

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_LocalExTaxAmount = 100M;
			invoice.AH_LocalTaxAmount = 10M;
			invoice.AH_LocalTaxAmountOtherTaxes = -3M;
			invoice.AH_PostDate = now;

			TransactionMatchLink link = Factory.New<TransactionMatchLink>();
			link.SetValues(invoice);

			AssertEquals("AP_AH", invoice.PK, link.AP_AH);
			AssertEquals("Amount", -113M, link.AP_Amount);
			AssertEquals("Post Date", now, link.AP_MatchDate);

			AssertEquals("GST Realised", 0M, link.AP_GSTRealised);
			AssertEquals("Match Period", 0, link.AP_MatchPeriod);
			AssertEquals("Reason", ZString.Empty, link.AP_Reason.Trim());
		}

		public void TestUnmatch()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			ARInvoice aRINV1 = Factory.NewWithValidTestData<ARInvoice>();
			InvoicingLineBase line = testObjectCreator.CreateInvoiceLine(aRINV1, testObjectCreator.AUD, 1, 10);
			line.AL_AT = testObjectCreator.GSTFREE1.PK;

			aRINV1.AH_LocalOutstandingAmount = 5M;
			aRINV1.AH_FullyPaidDate = ZDateTime.Empty;

			TransactionMatchLink matchLink = ((IMatching)aRINV1).CurrentMatchGroup.AddNew();
			matchLink.AP_AH = aRINV1.PK;
			matchLink.AP_Amount = 6M;

			AssertEquals("Cannot unmatch this - should give a data error", UnmatchingResult.DataErrorAddingMatchAmountExceedOriginalInvoiceAmount, matchLink.CanUnmatch);

			matchLink.AP_Amount = 5M;

			AssertEquals("Should be able to unmatch this", UnmatchingResult.Success, matchLink.CanUnmatch);
			matchLink.Unmatch();

			AssertEquals("ARINV1 should have 10 outstanding", 10M, aRINV1.AH_OutstandingAmount);
		}

		public void TestUnmatchForTransactionNotImplementingImatching()
		{
			var directPayment = TestObjectCreator.CreateDirectPayment(ZDateTime.Today, 10m, 0m, 20m, 0m);
			directPayment.AH_OutstandingAmount = 0m;
			directPayment.AH_TransactionNum = "TEST0001";

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "", TestObjectCreator.AUD, 1m, 10m, 0m, 10m, 0m);
			invoice.AH_OutstandingAmount = 0m;
			invoice.AH_TransactionNum = "TEST0002";

			var matchLinks = new TransactionMatchLinkGroup(Factory);
			var invoiceMatchlink = matchLinks.AddNew();
			invoiceMatchlink.AP_AH = invoice.PK;
			invoiceMatchlink.AP_Amount = -10m;

			var directPaymentMatchLink = matchLinks.AddNew();
			directPaymentMatchLink.AP_AH = directPayment.PK;
			directPaymentMatchLink.AP_Amount = 10m;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLinks);

			Factory.Save();

			var result = directPaymentMatchLink.CanUnmatch;
			var expectedErrorMessage = string.Format("Matching Transaction does not implement IMatching interface. Transaction Object Type: '{0}', PK: '{1}', AH_Ledger: 'CB', AH_TransactionType: 'DPY', AH_TransactionCount: 1.", directPayment.GetType().ToString(), directPayment.PK);
			AssertEquals("Error should be reported via Error Reporter", expectedErrorMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestUnmatchContraRow()
		{
			APContraRow aPCTR1 = Factory.NewWithValidTestData<APContraRow>();
			aPCTR1.AH_LocalExTaxAmount = 5M;
			aPCTR1.AH_OSExTaxAmount = 5M;
			aPCTR1.AH_OutstandingAmount = 0M;
			aPCTR1.AH_FullyPaidDate = ZDateTime.Today;

			var testInvoice1 = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "0001", TestObjectCreator.AUD, 1m, 5m, 0m, 5m, 0m);
			testInvoice1.AH_OH = aPCTR1.AH_OH;
			testInvoice1.AH_OutstandingAmount = 0m;

			TransactionMatchLinkGroup matchLinks = new TransactionMatchLinkGroup(Factory);
			TransactionMatchLink invMatchLink = matchLinks.AddNew();
			invMatchLink.AP_AH = testInvoice1.PK;
			invMatchLink.AP_Amount = -5M;

			TransactionMatchLink matchLink = matchLinks.AddNew();
			matchLink.AP_AH = aPCTR1.PK;
			matchLink.AP_Amount = 5M;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLinks);

			Factory.Save();

			AssertEquals("Should be able to unmatch this", UnmatchingResult.Success, matchLink.CanUnmatch);
			matchLink.Unmatch();
			AssertEquals("APCTR1 should have 5 outstanding", 5M, aPCTR1.AH_OutstandingAmount);
		}

		public void TestDetectOrgWhenUnmatching()
		{
			var aPCTR1 = Factory.NewWithValidTestData<APContraRow>();
			aPCTR1.AH_LocalExTaxAmount = 5M;
			aPCTR1.AH_OSExTaxAmount = 5M;
			aPCTR1.AH_OutstandingAmount = 0M;
			aPCTR1.AH_FullyPaidDate = ZDateTime.Today;

			var testInvoice1 = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "0001", TestObjectCreator.AUD, 1m, 5m, 0m, 5m, 0m);
			testInvoice1.AH_OH = aPCTR1.AH_OH;
			testInvoice1.AH_OutstandingAmount = 0m;

			var matchLinks = new TransactionMatchLinkGroup(Factory);
			var invMatchLink = matchLinks.AddNew();
			invMatchLink.AP_AH = testInvoice1.PK;
			invMatchLink.AP_Amount = -5M;

			var matchLink = matchLinks.AddNew();
			matchLink.AP_AH = aPCTR1.PK;
			matchLink.AP_Amount = 5M;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLinks);

			Factory.Save();

			AssertNull("The associated org is null.", matchLink.MatchingTransaction.Header);

			AssertEquals("Should be able to unmatch with null org", UnmatchingResult.Success, matchLink.CanUnmatch);

			aPCTR1.AH_OH = TestObjectCreator.Creditor1.PK;
			TestObjectCreator.Creditor1.OH_IsActive = false;

			Factory.Save();

			AssertEquals("Shouldn't be able to unmatch with inactive org", UnmatchingResult.DataErrorTransactionOrganisationIsInactive, matchLink.CanUnmatch);

			TestObjectCreator.Creditor1.OH_IsActive = true;

			Factory.Save();

			AssertEquals("Should be able to unmatch with active org", UnmatchingResult.Success, matchLink.CanUnmatch);
		}

		public void TestCanUnmatchJournal()
		{
			var categoryCodeList = new string[] { Core.Constants.TransactionCategory.Codes.CashAdvanceReceived,
																		Core.Constants.TransactionCategory.Codes.CashAdvancePaid,
																		Core.Constants.TransactionCategory.Codes.CashAdvanceInvoice };

			var aRJournal = Factory.NewWithValidTestData<ARJournal>();
			aRJournal.AH_LocalExTaxAmount = 100M;
			aRJournal.AH_OSExTaxAmount = 100M;
			aRJournal.AH_InvoiceAmount = 100M;
			aRJournal.AH_OutstandingAmount = 0M;
			aRJournal.AH_FullyPaidDate = ZDateTime.Today;
			var matchLinks = new TransactionMatchLinkGroup(Factory);
			var matchlinkForReceivables = matchLinks.AddNew();
			matchlinkForReceivables.AP_AH = aRJournal.PK;
			matchlinkForReceivables.AP_Amount = 100M;
			foreach (var category in categoryCodeList)
			{
				aRJournal.AH_TransactionCategory = category;
				AssertEquals("Should not be able to unmatch this because it is a cash advance AR journal", UnmatchingResult.ContainsCashAdvanceARJournal,
						matchlinkForReceivables.CanUnmatch);
			}
			aRJournal.AH_TransactionCategory = ZString.Empty;
			AssertEquals(UnmatchingResult.Success, matchlinkForReceivables.CanUnmatch);

			var aPJournal = Factory.NewWithValidTestData<APJournal>();
			aPJournal.AH_LocalExTaxAmount = -100M;
			aPJournal.AH_OSExTaxAmount = -100M;
			aPJournal.AH_InvoiceAmount = -100M;
			aPJournal.AH_OutstandingAmount = 0M;
			aPJournal.AH_FullyPaidDate = ZDateTime.Today;
			var matchlinkForPayables = matchLinks.AddNew();
			matchlinkForPayables.AP_AH = aPJournal.PK;
			matchlinkForPayables.AP_Amount = -100M;
			foreach (var category in categoryCodeList)
			{
				aPJournal.AH_TransactionCategory = category;
				AssertEquals("Should not be able to unmatch this because it is a cash advance AP journal", UnmatchingResult.ContainsCashAdvanceAPJournal,
						matchlinkForPayables.CanUnmatch);
			}
			aPJournal.AH_TransactionCategory = ZString.Empty;
			AssertEquals(UnmatchingResult.Success, matchlinkForPayables.CanUnmatch);
		}

		public void TestCanUnmatchPayment()
		{
			ARPayment aRPAY = Factory.NewWithValidTestData<ARPayment>();
			aRPAY.AH_LocalExTaxAmount = 100M;
			aRPAY.AH_OSExTaxAmount = 100M;
			aRPAY.AH_OutstandingAmount = 0M;
			aRPAY.AH_FullyPaidDate = ZDateTime.Today;

			APPayment aPPAY = Factory.NewWithValidTestData<APPayment>();
			aPPAY.AH_LocalExTaxAmount = -100M;
			aPPAY.AH_OSExTaxAmount = -100M;
			aPPAY.AH_OutstandingAmount = 0M;
			aPPAY.AH_FullyPaidDate = ZDateTime.Today;

			TransactionMatchLinkGroup matchLinks = new TransactionMatchLinkGroup(Factory);

			TransactionMatchLink matchlinkForReceivables = matchLinks.AddNew();
			matchlinkForReceivables.AP_AH = aRPAY.PK;
			matchlinkForReceivables.AP_Amount = 100M;

			TransactionMatchLink matchlinkForPayables = matchLinks.AddNew();
			matchlinkForPayables.AP_AH = aPPAY.PK;
			matchlinkForPayables.AP_Amount = -100M;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLinks);

			Factory.Save();

			bool oldReceivablesSecurityValue = Env.Security.ReceivablesAllowUnmatchingPaymentMatching.IsAllowed;
			bool oldPayablesSecurityValue = Env.Security.PayablesAllowUnmatchingPaymentMatching.IsAllowed;
			try
			{
				Env.Security.ReceivablesAllowUnmatchingPaymentMatching.IsAllowed = false;
				Env.Security.PayablesAllowUnmatchingPaymentMatching.IsAllowed = false;
				AssertEquals("Should not be able to unmatch this because it is a payment", UnmatchingResult.ContainsPayment,
					matchlinkForReceivables.CanUnmatch);
				AssertEquals("Should be able to unmatch this though it is a payment", UnmatchingResult.ContainsPayment,
					matchlinkForPayables.CanUnmatch);

				Env.Security.ReceivablesAllowUnmatchingPaymentMatching.IsAllowed = false;
				Env.Security.PayablesAllowUnmatchingPaymentMatching.IsAllowed = true;
				AssertEquals("Should not be able to unmatch this because it is a payment", UnmatchingResult.ContainsPayment,
					matchlinkForReceivables.CanUnmatch);
				AssertEquals("Should be able to unmatch this though it is a payment", UnmatchingResult.Success,
					matchlinkForPayables.CanUnmatch);

				Env.Security.ReceivablesAllowUnmatchingPaymentMatching.IsAllowed = true;
				Env.Security.PayablesAllowUnmatchingPaymentMatching.IsAllowed = false;
				AssertEquals("Should not be able to unmatch this because it is a payment", UnmatchingResult.Success,
					matchlinkForReceivables.CanUnmatch);
				AssertEquals("Should be able to unmatch this though it is a payment", UnmatchingResult.ContainsPayment,
					matchlinkForPayables.CanUnmatch);

				Env.Security.ReceivablesAllowUnmatchingPaymentMatching.IsAllowed = true;
				Env.Security.PayablesAllowUnmatchingPaymentMatching.IsAllowed = true;
				AssertEquals("Should not be able to unmatch this because it is a payment", UnmatchingResult.Success,
					matchlinkForReceivables.CanUnmatch);
				AssertEquals("Should be able to unmatch this though it is a payment", UnmatchingResult.Success,
					matchlinkForPayables.CanUnmatch);
			}
			finally
			{
				Env.Security.ReceivablesAllowUnmatchingPaymentMatching.IsAllowed = oldReceivablesSecurityValue;
				Env.Security.PayablesAllowUnmatchingPaymentMatching.IsAllowed = oldPayablesSecurityValue;
			}
		}

		public void TestOnSavingCore()
		{
			TransactionMatchLink matchLink1 = null;
			TransactionMatchLink matchLink2 = null;
			Action createNewMatchLinks = () =>
				{
					TransactionMatchLinkGroup matchLinkGroup = new TransactionMatchLinkGroup(Factory);
					matchLink1 = matchLinkGroup.AddNew();
					matchLink1.AP_AH = Factory.NewWithValidTestData<APJournal>().PK;
					matchLink2 = matchLinkGroup.AddNew();
					matchLink2.AP_AH = Factory.NewWithValidTestData<ARJournal>().PK;
					TestObjectCreator.SetupMatchLinkMatchDate(matchLinkGroup);
					TransactionMatchLinkCollection matchLinkCollection = new TransactionMatchLinkCollection(Factory);
					matchLinkCollection.AddRange(new[] { matchLink1, matchLink2 });
					AssertEquals("Precondition: matchlink should have 2 parent collections", 2, ((IBusinessObjectInternals)matchLink1).ParentCollections.Length);
					AssertEquals("Precondition: matchlink should have 2 parent collections", 2, ((IBusinessObjectInternals)matchLink2).ParentCollections.Length);
					AssertEquals("Precondition: but matchlink should only one TransactionMatchLinkGroup in parent collections", 1, ((IBusinessObjectInternals)matchLink1).ParentCollections.OfType<TransactionMatchLinkGroup>().Count());
					AssertEquals("Precondition: but matchlink should only one TransactionMatchLinkGroup in parent collections", 1, ((IBusinessObjectInternals)matchLink2).ParentCollections.OfType<TransactionMatchLinkGroup>().Count());
				};
			createNewMatchLinks();
			matchLink1.AP_MatchGroupNum = "2";
			matchLink2.AP_MatchGroupNum = "3";
			Factory.Save();
			AssertEquals("Match link group number is already set and shouldn't be changed: matchLink1.AP_MatchGroupNum", "2", matchLink1.AP_MatchGroupNum);
			AssertEquals("Match link group number is already set and shouldn't be changed: matchLink2.AP_MatchGroupNum", "3", matchLink2.AP_MatchGroupNum);

			createNewMatchLinks();
			matchLink1.AP_MatchGroupNum = "2";
			matchLink2.AP_MatchGroupNum = "";
			string expectedMatchGroupNum = Env.NumberFountains.MatchNo.GetTodaysPeriodFountain().PeekPreliminaryFormatted(Factory);
			Factory.Save();
			AssertEquals("Even if one match link hasn't number new number should be generated for all group: matchLink1.AP_MatchGroupNum", expectedMatchGroupNum, matchLink1.AP_MatchGroupNum);
			AssertEquals("Even if one match link hasn't number new number should be generated for all group: matchLink2.AP_MatchGroupNum", expectedMatchGroupNum, matchLink2.AP_MatchGroupNum);

			createNewMatchLinks();
			TransactionMatchLinkGroup matchLinkGroup2 = new TransactionMatchLinkGroup(Factory);
			matchLinkGroup2.AddRange(new[] { matchLink1, matchLink2 });
			matchLink1.AP_MatchGroupNum = "";
			matchLink2.AP_MatchGroupNum = "";
			try
			{
				Factory.Save();
			}
			catch
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
			AssertEquals("Match link belongs to more than one match group and we can't correctly generate group number: matchLink1.AP_MatchGroupNum", "", matchLink1.AP_MatchGroupNum);
			AssertEquals("Match link belongs to more than one match group and we can't correctly generate group number: matchLink2.AP_MatchGroupNum", "", matchLink2.AP_MatchGroupNum);
		}

		#region CashBasisVAT

		public void TestRemovingCreatedCashVATAfUnsuccessfulSaving()
		{
			var apInvoice = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(APInvoice), 100, 10);
			apInvoice.AH_LocalOutstandingAmount = 100M;

			var apPayment = Factory.NewWithValidTestData<APPayment>();
			apPayment.AH_LocalExTaxAmount = 10M;
			apPayment.AH_OSExTaxAmount = 10M;
			apPayment.AH_LocalOutstandingAmount = 0M;
			apPayment.AH_FullyPaidDate = ZDateTime.Now;
			apPayment.AH_OH = apInvoice.AH_OH;

			var matchDate = ZDateTime.Now.AddDays(-7);
			var apPayMatchLink = ((IMatching)apPayment).CurrentMatchGroup.AddNew();
			apPayMatchLink.AP_AH = apPayment.PK;
			apPayMatchLink.AP_Amount = 10M;
			apPayMatchLink.AP_MatchGroupNum = "M001";
			apPayMatchLink.AP_MatchDate = matchDate;

			var apInvMatchLink = ((IMatching)apPayment).CurrentMatchGroup.AddNew();
			apInvMatchLink.AP_AH = apInvoice.PK;
			apInvMatchLink.AP_Amount = -10M;
			apInvMatchLink.AP_MatchGroupNum = "M001";
			apInvMatchLink.AP_MatchDate = matchDate;

			var invalidInvoiceInSavingFactory = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV", TestObjectCreator.AUD, 1, TestObjectCreator.Creditor1);
			Factory.Saving += FactorySavingWithException;
			try
			{
				Factory.Save();
				Fail("Exception should be thrown to test unsuccessful saving.");
			}
			catch (NotImplementedException)
			{
			}
			finally
			{
				Factory.Saving -= FactorySavingWithException;
			}

			invalidInvoiceInSavingFactory.Delete();
			Factory.Save();

			var chashVATs = Factory.Load<AccCashBasisVAT>(new ZQuery(AccCashBasisVATSchema.YC_MatchGroupNum, apInvMatchLink.AP_MatchGroupNum));
			AssertEquals("Cash Basis VAT record must be created.", 1, chashVATs.Length);
		}

		public void TestCashBasisVATWithoutCreationDBHits()
		{
			var matchGroupNumber = "M00001445";
			PrepareDataForCashBasisVATCreationDBHits(false, matchGroupNumber);

			int databaseLoadCount;
			var tableHits = GetTableHitsForSavingWithoutCashBasisVAT(out databaseLoadCount);
			tableHits[AccTransactionMatchLink.Schema.TableName] = 0;
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			AssertDbHits(tableHits, Factory);
			AssertEquals("DatabaseLoadCount", databaseLoadCount + 2, Factory.DatabaseLoadCount);

			var chashVATs = Factory.Load<AccCashBasisVAT>(new ZQuery(AccCashBasisVATSchema.YC_MatchGroupNum, matchGroupNumber));
			AssertEquals("Postcondition: Cash Basis VAT record must not be created.", 0, chashVATs.Length);
		}

		public void TestCashBasisVATCreationDBHits()
		{
			var matchGroupNumber = "M00001445";
			PrepareDataForCashBasisVATCreationDBHits(true, matchGroupNumber);

			int databaseLoadCount;
			var tableHits = GetTableHitsForSavingWithoutCashBasisVAT(out databaseLoadCount);
			tableHits[AccTransactionLines.Schema.TableName] += 1;
			tableHits[AccCashBasisVAT.Schema.TableName] += 2;
			databaseLoadCount += 7;

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			AssertDbHits(tableHits, Factory);
			AssertEquals("DatabaseLoadCount", databaseLoadCount, Factory.DatabaseLoadCount);

			var chashVATs = Factory.Load<AccCashBasisVAT>(new ZQuery(AccCashBasisVATSchema.YC_MatchGroupNum, matchGroupNumber));
			AssertEquals("Postcondition: Cash Basis VAT record must be created.", 10, chashVATs.Length);
		}

		public void TestCashBasisVATWithoutCreationDBHits_MatchingByLines()
		{
			var matchGroupNumber = "M00001445";
			PrepareDataForCashBasisVATCreationDBHits_MatchingByLines(false, matchGroupNumber);

			int databaseLoadCount;
			var tableHits = GetTableHitsForSavingWithoutCashBasisVAT_MatchingByLines(out databaseLoadCount);
			tableHits[AccTransactionMatchLink.Schema.TableName] = 0;
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			AssertDbHits(tableHits, Factory);
			AssertEquals("DatabaseLoadCount", databaseLoadCount + 2, Factory.DatabaseLoadCount);

			var chashVATs = Factory.Load<AccCashBasisVAT>(new ZQuery(AccCashBasisVATSchema.YC_MatchGroupNum, matchGroupNumber));
			AssertEquals("Postcondition: Cash Basis VAT record must not be created.", 0, chashVATs.Length);
		}

		public void TestCashBasisVATCreationDBHits_MatchingByLines()
		{
			var matchGroupNumber = "M00001445";
			PrepareDataForCashBasisVATCreationDBHits_MatchingByLines(true, matchGroupNumber);

			int databaseLoadCount;
			var tableHits = GetTableHitsForSavingWithoutCashBasisVAT_MatchingByLines(out databaseLoadCount);
			tableHits[AccTransactionLines.Schema.TableName] += 1;
			tableHits[AccCashBasisVAT.Schema.TableName] += 2;
			databaseLoadCount += 7;
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			AssertDbHits(tableHits, Factory);
			AssertEquals("DatabaseLoadCount", databaseLoadCount, Factory.DatabaseLoadCount);

			var chashVATs = Factory.Load<AccCashBasisVAT>(new ZQuery(AccCashBasisVATSchema.YC_MatchGroupNum, matchGroupNumber));
			AssertEquals("Postcondition: Cash Basis VAT record must be created.", 10, chashVATs.Length);
		}

		// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
		Dictionary<string, int> GetTableHitsForSavingWithoutCashBasisVAT(out int databaseLoadCount)
		{
			var tableHints = new Dictionary<string, int>();
			tableHints.Add(AccTransactionHeader.Schema.TableName, 1);
			tableHints.Add(AccTransactionLines.Schema.TableName, 20);
			tableHints.Add(AccTransactionMatchLink.Schema.TableName, 1);
			tableHints.Add(AccTransLinePay.Schema.TableName, 0);
			tableHints.Add(AccCashBasisVAT.Schema.TableName, 0);
			tableHints.Add(AccPaymentApproval.Schema.TableName, 1);
			tableHints.Add(ProcessTasks.Schema.TableName, 1);
			tableHints.Add(ProcessTaskTemplate.Schema.TableName, 3);
			tableHints.Add(OrgCommissionAgreement.Schema.TableName, 1);
			tableHints.Add(JobOrderHeaderSchema.Constants.TableName, 0);
			tableHints.Add(AccTaxTransactionSchema.Constants.TableName, 1);
			databaseLoadCount = 26;

			return tableHints;
		}

		// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
		Dictionary<string, int> GetTableHitsForSavingWithoutCashBasisVAT_MatchingByLines(out int databaseLoadCount)
		{
			var tableHints = new Dictionary<string, int>();
			tableHints.Add(AccTransactionHeader.Schema.TableName, 1);
			tableHints.Add(AccTransactionLines.Schema.TableName, 30);
			tableHints.Add(AccTransactionMatchLink.Schema.TableName, 1);
			tableHints.Add(AccTransLinePay.Schema.TableName, 0);
			tableHints.Add(AccCashBasisVAT.Schema.TableName, 0);
			tableHints.Add(AccPaymentApproval.Schema.TableName, 1);
			tableHints.Add(ProcessTasks.Schema.TableName, 1);
			tableHints.Add(ProcessTaskTemplate.Schema.TableName, 3);
			tableHints.Add(OrgCommissionAgreement.Schema.TableName, 1);
			tableHints.Add(JobOrderHeaderSchema.Constants.TableName, 0);
			databaseLoadCount = 35;

			return tableHints;
		}

		void PrepareDataForCashBasisVATCreationDBHits(bool createCashVAT, string matchGroupNumber)
		{
			var apInvoice = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(APInvoice), 100, 10);
			if (!createCashVAT)
			{
				GlbCompany.CurrentCompany.GC_IsGSTCashBasis = false;
				apInvoice.Lines[0].AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Code;
			}
			for (int i = 0; i < 9; i++)
			{
				TestObjectCreator.CreateCashVATLines(apInvoice, 100, 10);
			}
			apInvoice.AH_LocalOutstandingAmount = 0M;
			apInvoice.AH_FullyPaidDate = ZDateTime.Now;

			var apPayment = Factory.NewWithValidTestData<APPayment>();
			apPayment.AH_LocalExTaxAmount = 1100M;
			apPayment.AH_OSExTaxAmount = 1100M;
			apPayment.AH_LocalOutstandingAmount = 0M;
			apPayment.AH_FullyPaidDate = ZDateTime.Now;
			apPayment.AH_OH = apInvoice.AH_OH;

			var matchDate = ZDateTime.Now.AddDays(-7);
			var apPayMatchLink = ((IMatching)apPayment).CurrentMatchGroup.AddNew();
			apPayMatchLink.AP_AH = apPayment.PK;
			apPayMatchLink.AP_Amount = 1100M;
			apPayMatchLink.AP_MatchGroupNum = matchGroupNumber;
			apPayMatchLink.AP_MatchDate = matchDate;

			var apInvMatchLink = ((IMatching)apPayment).CurrentMatchGroup.AddNew();
			apInvMatchLink.AP_AH = apInvoice.PK;
			apInvMatchLink.AP_Amount = -1100M;
			apInvMatchLink.AP_MatchGroupNum = matchGroupNumber;
			apInvMatchLink.AP_MatchDate = matchDate;

			using (CashBasisVATManager.SetBatchSizeForTests(CashBasisVATManager.BatchSize))
			{
				Factory.ResetDatabaseLoadCount();
				Factory.Save();
			}
		}

		void PrepareDataForCashBasisVATCreationDBHits_MatchingByLines(bool createCashVAT, string matchGroupNumber)
		{
			var apInvoice = TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(APInvoice), 100, 10);
			if (!createCashVAT)
			{
				GlbCompany.CurrentCompany.GC_IsGSTCashBasis = false;
				apInvoice.Lines[0].AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Code;
			}
			for (int i = 0; i < 14; i++)
			{
				TestObjectCreator.CreateCashVATLines(apInvoice, 100, 10);
			}
			apInvoice.AH_LocalOutstandingAmount = 1150M;

			var apPayment = Factory.NewWithValidTestData<APPayment>();
			apPayment.AH_LocalExTaxAmount = 500M;
			apPayment.AH_OSExTaxAmount = 500M;
			apPayment.AH_LocalOutstandingAmount = 0M;
			apPayment.AH_FullyPaidDate = ZDateTime.Now;
			apPayment.AH_OH = apInvoice.AH_OH;

			var matchDate = ZDateTime.Now.AddDays(-7);
			var apPayMatchLink = ((IMatching)apPayment).CurrentMatchGroup.AddNew();
			apPayMatchLink.AP_AH = apPayment.PK;
			apPayMatchLink.AP_Amount = 500M;
			apPayMatchLink.AP_MatchGroupNum = matchGroupNumber;
			apPayMatchLink.AP_MatchDate = matchDate;

			var apInvMatchLink = ((IMatching)apPayment).CurrentMatchGroup.AddNew();
			apInvMatchLink.AP_AH = apInvoice.PK;
			apInvMatchLink.AP_Amount = -500M;
			apInvMatchLink.AP_MatchGroupNum = matchGroupNumber;
			apInvMatchLink.AP_MatchDate = matchDate;

			for (int i = 0; i < 10; i++)
			{
				var lineMatchLink = Factory.New<AccTransLinePay>();
				lineMatchLink.A7_AL = apInvoice.Lines[i].PK;
				lineMatchLink.A7_Amount = -50;
				lineMatchLink.A7_AP = apInvMatchLink.PK;
			}

			using (CashBasisVATManager.SetBatchSizeForTests(CashBasisVATManager.BatchSize))
			{
				Factory.ResetDatabaseLoadCount();
				Factory.Save();
			}
		}

		#endregion

		void FactorySavingWithException(BusinessObjectFactory factory)
		{
			throw new NotImplementedException();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			TransactionMatchLinkGroup collection = new TransactionMatchLinkGroup(factory);
			TransactionMatchLink result = (TransactionMatchLink)base.GetNewBusinessObjectForDeleteTest(factory);
			result.AP_MatchGroupNum = "0123456789";
			TestObjectCreator.SetupMatchLinkMatchDate(result);
			collection.Add(result);

			return result;
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
