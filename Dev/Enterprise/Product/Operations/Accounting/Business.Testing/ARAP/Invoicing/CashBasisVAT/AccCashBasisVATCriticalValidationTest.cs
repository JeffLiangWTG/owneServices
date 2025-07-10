using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	internal class AccCashBasisVATCriticalValidationTest : CriticalValidationTest<AccCashBasisVAT>
	{
		protected override List<TestCaseDefinitionWithDelegate_Obsolete> GetTestCases()
		{
			var result = new List<TestCaseDefinitionWithDelegate_Obsolete>();

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Saved and changed", factory =>
		{
			var cashVATRecord = GetCashBasisVAT(factory);
			factory.Save();

			cashVATRecord.YC_MatchGroupNum = "";

			return cashVATRecord;
		}, true, CriticalValidationErrorType.CashBasisTaxRecognition_1, "Cash basis tax recognition record cannot be changed once it saved", "Cash VAT Recognition Record: PK = "));

			foreach (bool fail in new[] { true, false })
			{
				bool fail_cachedForDelegates = fail;

				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("Post Date is Empty Should fail: {0}", fail_cachedForDelegates), factory =>
			{
				var cashVATRecord = GetCashBasisVAT(factory);
				if (fail_cachedForDelegates)
				{
					cashVATRecord.YC_PostDate = ZDateTime.Empty;
				}

				return cashVATRecord;
			}, fail_cachedForDelegates, CriticalValidationErrorType.CashBasisTaxRecognitionWithIncorrectPostDate_1, "Cash Basis Tax Recognition record cannot be created with an empty Post Date", "Cash VAT Recognition Record: PK = "));

				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("Transaction line is not linked. Should fail: {0}", fail_cachedForDelegates), factory =>
			{
				var cashVATRecord = GetCashBasisVAT(factory);
				if (fail_cachedForDelegates)
				{
					cashVATRecord.YC_AL_TransactionLine = ZGuid.Empty;
				}

				return cashVATRecord;
			}, fail_cachedForDelegates, CriticalValidationErrorType.CashBasisTaxRecognition_1, "Cash basis tax recognition record must always have transaction line", "Cash VAT Recognition Record: PK = "));

				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("Transaction line without cash tax basis. Should fail: {0}", fail_cachedForDelegates), factory =>
			{
				var cashVATRecord = GetCashBasisVAT(factory);
				if (fail_cachedForDelegates)
				{
					cashVATRecord.TransactionLine.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Code;
				}

				return cashVATRecord;
			}, fail_cachedForDelegates, CriticalValidationErrorType.CashBasisTaxRecognitionWithIncorrectTaxRecord_1, "Cash Basis Tax Recognition record cannot be created for a line when the line's Tax Basis is not Cash", "Cash VAT Recognition Record: PK = "));

				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("Transaction line without Tax ID. Should fail: {0}", fail_cachedForDelegates), factory =>
			{
				var cashVATRecord = GetCashBasisVAT(factory);
				if (fail_cachedForDelegates)
				{
					cashVATRecord.TransactionLine.AL_AT = ZGuid.Empty;
				}

				return cashVATRecord;
			}, fail_cachedForDelegates, CriticalValidationErrorType.CashBasisTaxRecognitionWithIncorrectTaxRecord_1, "Cash Basis Tax Recognition records cannot be created for lines with no Tax ID", "Cash VAT Recognition Record: PK = "));

				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("Zero TaxBaseAmount. Should fail: {0}", fail_cachedForDelegates), factory =>
			{
				var cashVATRecord = GetCashBasisVAT(factory);
				if (fail_cachedForDelegates)
				{
					cashVATRecord.YC_TaxBaseAmount = 0;
				}

				return cashVATRecord;
			}, fail_cachedForDelegates, CriticalValidationErrorType.CashBasisTaxRecognitionWithIncorrectTaxRecord_1, "Cash basis tax recognition record cannot have zero tax basis amount", "Cash VAT Recognition Record: PK = "));

				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("Zero TaxAmount. Should fail: {0}", fail_cachedForDelegates), factory =>
			{
				var cashVATRecord = GetCashBasisVAT(factory);
				cashVATRecord.YC_TaxAmount = 0;
				if (!fail_cachedForDelegates)
				{
					cashVATRecord.TransactionLine.AL_GSTVAT = 0;
				}

				return cashVATRecord;
			}, fail_cachedForDelegates, CriticalValidationErrorType.CashBasisTaxRecognitionWithIncorrectTaxRecord_1, "Cash basis tax recognition record cannot have zero tax amount if related line tax amount is not zero", "Cash VAT Recognition Record: PK = "));

				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("TaxBaseAmount and TaxAmount have different signs. Should fail: {0}", fail_cachedForDelegates), factory =>
			{
				var cashVATRecord = GetCashBasisVAT(factory);
				if (fail_cachedForDelegates)
				{
					cashVATRecord.YC_TaxAmount *= -1;
				}

				return cashVATRecord;
			}, fail_cachedForDelegates, CriticalValidationErrorType.CashBasisTaxRecognitionWithDifferentSigns_1, "Cash basis tax recognition record has different signs", "Cash VAT Recognition Record: PK = "));

				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("Post Date must equal Match Date. Should fail: {0}", fail_cachedForDelegates), factory =>
			{
				var testObjectCreator = new TestObjectCreator(factory);
				AccTransactionLines line;
				var matchLink = GetMatchLinkAndLine(testObjectCreator, out line);
				var cashVATRecord = testObjectCreator.CreateCashBasisVAT(line, fail_cachedForDelegates ? -91 : -90, -9, matchLink.AP_MatchDate, matchLink);
				if (fail_cachedForDelegates)
				{
					cashVATRecord.YC_PostDate = ZDateTime.BrettsBirthday;
				}

				return cashVATRecord;
			}, fail_cachedForDelegates, CriticalValidationErrorType.CashBasisTaxRecognitionWithIncorrectPostDate_1, "The Post date of the Cash Basis Tax Recognition record must be the same as the related Match Group's match date", "Cash VAT Recognition Record: PK = "));

				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("No existing match group number. Should fail: {0}", fail_cachedForDelegates), factory =>
			{
				var cashVATRecord = GetCashBasisVAT(factory);
				if (fail_cachedForDelegates)
				{
					cashVATRecord.YC_MatchGroupNum = "XXXXXXX";
				}

				return cashVATRecord;
			}, fail_cachedForDelegates, CriticalValidationErrorType.CashBasisTaxRecognition_1, "Cash basis tax recognition record match group number does not exist", "Cash VAT Recognition Record: PK = "));

				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("No existing match group number. Case 2. Should fail: {0}", fail_cachedForDelegates), factory =>
			{
				var testObjectCreator = new TestObjectCreator(factory);
				AccTransactionLines line;
				var matchLink = GetMatchLinkAndLine(testObjectCreator, out line);
				var cashVATRecordReversed = testObjectCreator.CreateCashBasisVAT(line, 90, 9, ZDateTime.Now, matchLink);
				var cashVATRecord = testObjectCreator.CreateCashBasisVAT(line, fail_cachedForDelegates ? -91 : -90, -9, ZDateTime.Now, matchLink);

				matchLink.Delete();

				return cashVATRecord;
			}, fail_cachedForDelegates, CriticalValidationErrorType.CashBasisTaxRecognition_1, "Cash basis tax recognition record match group number does not exist", "Cash VAT Recognition Record: PK = "));

				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("No original tax record. Should fail: {0}", fail_cachedForDelegates), factory =>
			{
				var testObjectCreator = new TestObjectCreator(factory);
				AccTransactionLines line;
				var matchLink = GetMatchLinkAndLine(testObjectCreator, out line);
				var cashVATRecord = testObjectCreator.CreateCashBasisVAT(line, -90, fail_cachedForDelegates ? -10 : -9, ZDateTime.Now, matchLink);
				var cashVATRecordReversed = testObjectCreator.CreateCashBasisVAT(line, 90, 9, ZDateTime.Now, matchLink);

				matchLink.Delete();

				return cashVATRecordReversed;
			}, fail_cachedForDelegates, CriticalValidationErrorType.CashBasisTaxRecognitionWithOppositeToLineSigns_1, "Cash Basis Tax Recognition records with an opposite sign to the line are only permitted when reversing an existing record", "Cash VAT Recognition Record: PK = "));

				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("No original tax record. Case 2. Should fail: {0}", fail_cachedForDelegates), factory =>
			{
				var testObjectCreator = new TestObjectCreator(factory);
				AccTransactionLines line;
				var matchLink = GetMatchLinkAndLine(testObjectCreator, out line);
				matchLink.AP_MatchGroupNum = "M0001";

				AccTransactionLines line2;
				var matchLink2 = GetMatchLinkAndLine(testObjectCreator, out line2);
				matchLink2.AP_MatchGroupNum = "M0002";

				var cashVATRecord = testObjectCreator.CreateCashBasisVAT(line, -90, -9, ZDateTime.Now, matchLink);
				var cashVATRecordReversed = testObjectCreator.CreateCashBasisVAT(line, 90, 9, ZDateTime.Now, fail_cachedForDelegates ? matchLink2 : matchLink);

				matchLink.Delete();

				return cashVATRecordReversed;
			}, fail_cachedForDelegates, CriticalValidationErrorType.CashBasisTaxRecognitionWithOppositeToLineSigns_1, "Cash Basis Tax Recognition records with an opposite sign to the line are only permitted when reversing an existing record", "Cash VAT Recognition Record: PK = "));

				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("Match link for reversed tax blah blah record in not deleted. Should fail: {0}", fail_cachedForDelegates), factory =>
			{
				var testObjectCreator = new TestObjectCreator(factory);
				AccTransactionLines line;
				var matchLink = GetMatchLinkAndLine(testObjectCreator, out line);
				var cashVATRecord = testObjectCreator.CreateCashBasisVAT(line, -90, -9, ZDateTime.Now, matchLink);
				var cashVATRecordReversed = testObjectCreator.CreateCashBasisVAT(line, 90, 9, ZDateTime.Now, matchLink);

				if (!fail_cachedForDelegates)
				{
					matchLink.Delete();
				}

				return cashVATRecordReversed;
			}, fail_cachedForDelegates, CriticalValidationErrorType.CashBasisTaxRecognitionWithOppositeToLineSigns_1, "Cash Basis Tax Recognition record with an opposite sign to the line is only allowed when unmatching", "Cash VAT Recognition Record: PK = "));

				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("Tax base amount more than line amount. Should fail: {0}", fail_cachedForDelegates), factory =>
			{
				var testObjectCreator = new TestObjectCreator(factory);
				AccTransactionLines line;
				var matchLink = GetMatchLinkAndLine(testObjectCreator, out line);
				var cashVATRecord = testObjectCreator.CreateCashBasisVAT(line, -90, -8, matchLink);
				var cashVATRecord2 = testObjectCreator.CreateCashBasisVAT(line, -10, -1, matchLink);

				if (fail_cachedForDelegates)
				{
					var cashVATRecord3 = testObjectCreator.CreateCashBasisVAT(line, -10, -1, matchLink);
				}

				return cashVATRecord;
			}, fail_cachedForDelegates, CriticalValidationErrorType.CashBasisTaxRecognitionWithIncorrectTotal_1, "Sum of Tax Base Amounts for Cash basis tax recognition records cannot exceed linked Line Amount", "Cash VAT Recognition Record: PK = "));

				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("Tax amount more than line tax amount. Should fail: {0}", fail_cachedForDelegates), factory =>
			{
				var testObjectCreator = new TestObjectCreator(factory);
				AccTransactionLines line;
				var matchLink = GetMatchLinkAndLine(testObjectCreator, out line);
				var cashVATRecord = testObjectCreator.CreateCashBasisVAT(line, -80, -9, matchLink);
				var cashVATRecord2 = testObjectCreator.CreateCashBasisVAT(line, -10, -1, matchLink);

				if (fail_cachedForDelegates)
				{
					var cashVATRecord3 = testObjectCreator.CreateCashBasisVAT(line, -10, -1, matchLink);
				}

				return cashVATRecord;
			}, fail_cachedForDelegates, CriticalValidationErrorType.CashBasisTaxRecognitionWithIncorrectTotal_1, "The sum of all related Cash Basis Tax Recognition records for this Line cannot exceed the Line's original Tax Amount", "Cash VAT Recognition Record: PK = "));

				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("Cash Basis Tax Recognition record with an empty Match Group number have Tax Transactions. Should fail: {0}", fail_cachedForDelegates), factory =>
				{
					var testObjectCreator = new TestObjectCreator(factory);
					AccTransactionLines line;
					var matchLink = GetMatchLinkAndLine(testObjectCreator, out line, true);
					var cashVATRecord = testObjectCreator.CreateCashBasisVAT(line, -100, -10, matchLink);
					cashVATRecord.YC_MatchGroupNum = "";
					var header = line.TransactionHeader;
					AssertEquals(0m, header.AH_InvoiceAmount + header.AH_GSTAmount);

					if (fail_cachedForDelegates)
					{
						header.AH_LocalTaxAmountOtherTaxes = 10M;
					}

					return cashVATRecord;
				}, fail_cachedForDelegates, CriticalValidationErrorType.CashBasisTaxRecognitionWithEmptyMatchGroupNumber_1, "Cash Basis Tax Recognition record with an empty Match Group number can only be created for zero value transactions.", "Cash VAT Recognition Record: PK = "));
			}

			return result;
		}

		public void TestOnSavingChecks_OneInvoiceWithManyCashBasisVATs_LoadedOnce()
		{
			var factory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(factory);
			var matchLink = GetMatchLinkAndLine(testObjectCreator, out var invoiceLine);
			var cashVATBasis1 = testObjectCreator.CreateCashBasisVAT(invoiceLine, -10, -1, matchLink);
			var cashVATBasis2 = testObjectCreator.CreateCashBasisVAT(invoiceLine, -20, -2, matchLink);
			var cashVATBasis3 = testObjectCreator.CreateCashBasisVAT(invoiceLine, -30, -3, matchLink);
			var cashVATBasis4 = testObjectCreator.CreateCashBasisVAT(invoiceLine, -40, -4, matchLink);
			var factoryRowsLoadedCounterForAccTransactionLines = 0;
			var factoryRowsLoadedCounterForAccCashBasisVAT = 0;
			factory.RowsLoaded += Factory_RowsLoaded;

			((ISupportCriticalValidation)cashVATBasis1).CriticalValidation.RunOnSavingCheck();
			((ISupportCriticalValidation)cashVATBasis2).CriticalValidation.RunOnSavingCheck();
			((ISupportCriticalValidation)cashVATBasis3).CriticalValidation.RunOnSavingCheck();
			((ISupportCriticalValidation)cashVATBasis4).CriticalValidation.RunOnSavingCheck();
			AssertEquals("Number of Factory Loads of AccTransactionLines table per Invoice", 1, factoryRowsLoadedCounterForAccTransactionLines);
			AssertEquals("Number of Factory Loads of AccCashBasisVAT table per Invoice", 2, factoryRowsLoadedCounterForAccCashBasisVAT);

			factory.RowsLoaded -= Factory_RowsLoaded;

			void Factory_RowsLoaded(object sender, RowsLoadedEventArgs e)
			{
				if (e.TableOrViewName == AccTransactionLinesSchema.Constants.TableName)
				{
					factoryRowsLoadedCounterForAccTransactionLines++;
				}

				if (e.TableOrViewName == AccCashBasisVATSchema.Constants.TableName)
				{
					factoryRowsLoadedCounterForAccCashBasisVAT++;
				}
			}
		}

		public void TestOnSavingChecks_ValidationDoesCachingPerInvoice()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var invoice1 = (APInvoice)testObjectCreator.CreateInvoiceWithCashVATLine(typeof(APInvoice), 100, 10);
			var invoice2 = (APInvoice)testObjectCreator.CreateInvoiceWithCashVATLine(typeof(APInvoice), 200, 20);
			var invoice1_Line = invoice1.Lines[0];
			var invoice2_Line = invoice2.Lines[0];
			var matchLink1 = testObjectCreator.CreateMatchLinkToPayAPInvoice(invoice1, null, -100);
			var matchLink2 = testObjectCreator.CreateMatchLinkToPayAPInvoice(invoice2, null, -200);
			matchLink1.SkipCashBasisVATCreationForTestOnly = true;
			matchLink2.SkipCashBasisVATCreationForTestOnly = true;
			var validCashBasisVAT = testObjectCreator.CreateCashBasisVAT(invoice1_Line, -90, -9, matchLink1);
			var invalidCashBasisVAT = testObjectCreator.CreateCashBasisVAT(invoice2_Line, -240, -40, matchLink2);

			// The test simulates a scenario with 2 invoices where one of invoices have error, and confirm that AccCashBasisVAT validation checks on saving, correctly maintains separate caching for each invoice. So, a random error has been used to test here.
			// The below OnSavingCriticalCheckException error is when CashBasisVAT tax base amount is greater than invoice line amount.
			CombineAssertions(() =>
			{
				var expectedExceptionMessage = "Sum of Tax Base Amounts for Cash basis tax recognition records cannot exceed linked Line Amount.";
				AssertEquals("Precondition: No error for invoice1", false, Math.Abs(invoice1_Line.AL_LineAmount) < Math.Abs(validCashBasisVAT.YC_TaxBaseAmount));
				AssertEquals("Precondition: Has error for invoice2", true, Math.Abs(invoice2_Line.AL_LineAmount) < Math.Abs(invalidCashBasisVAT.YC_TaxBaseAmount));

				AssertNoExceptionThrown(() => ((ISupportCriticalValidation)validCashBasisVAT).CriticalValidation.RunOnSavingCheck());
				AssertExceptionThrown<OnSavingCriticalCheckException>(expectedExceptionMessage, () => ((ISupportCriticalValidation)invalidCashBasisVAT).CriticalValidation.RunOnSavingCheck());
			});
			ErrorReporter.Clear();
		}

		public void TestGetClearCacheDelegateImplementation_ClearsCachedValueInTheFactory()
		{
			var factory = new BusinessObjectFactory();
			var cashVATRecord = GetCashBasisVAT(factory);
			var criticalValidation = ((ISupportCriticalValidation)cashVATRecord).CriticalValidation;
			var clearCacheDelegate = ((IClearCacheProvider)criticalValidation).GetClearCacheDelegate();

			var factoryRowsLoadedCounterForAccTransactionLines = 0; //Number of Factory Loads of AccTransactionLines table
			var factoryRowsLoadedCounterForAccCashBasisVAT = 0; //Number of Factory Loads of AccCashBasisVAT table
			factory.RowsLoaded += Factory_RowsLoaded;

			CombineAssertions(() =>
			{
				criticalValidation.RunOnSavingCheck();
				Assert("Has cached value: factoryRowsLoadedCounterForAccTransactionLines", factoryRowsLoadedCounterForAccTransactionLines > 0);
				Assert("Has cached value: factoryRowsLoadedCounterForAccCashBasisVAT", factoryRowsLoadedCounterForAccCashBasisVAT > 0);

				factoryRowsLoadedCounterForAccTransactionLines = factoryRowsLoadedCounterForAccCashBasisVAT = 0;
				criticalValidation.RunOnSavingCheck();
				Assert("When cache NOT cleared: factoryRowsLoadedCounterForAccTransactionLines", factoryRowsLoadedCounterForAccTransactionLines == 0);
				Assert("When cache NOT cleared: factoryRowsLoadedCounterForAccCashBasisVAT", factoryRowsLoadedCounterForAccCashBasisVAT == 0);

				clearCacheDelegate(factory);
				criticalValidation.RunOnSavingCheck();
				Assert("After clearing cache: factoryRowsLoadedCounterForAccTransactionLines", factoryRowsLoadedCounterForAccTransactionLines > 0);
				Assert("After clearing cache: factoryRowsLoadedCounterForAccCashBasisVAT", factoryRowsLoadedCounterForAccCashBasisVAT > 0);
			});

			factory.RowsLoaded -= Factory_RowsLoaded;

			void Factory_RowsLoaded(object sender, RowsLoadedEventArgs e)
			{
				if (e.TableOrViewName == AccTransactionLinesSchema.Constants.TableName)
				{
					factoryRowsLoadedCounterForAccTransactionLines++;
				}

				if (e.TableOrViewName == AccCashBasisVATSchema.Constants.TableName)
				{
					factoryRowsLoadedCounterForAccCashBasisVAT++;
				}
			}
		}

		public void TestAccCashBasisVATPreloading()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var invoice = (APInvoice)testObjectCreator.CreateInvoiceWithCashVATLine(typeof(APInvoice), 100, 20);
			invoice.AH_TransactionNum = "TEST_TRANSACTIONUM";
			var line1 = invoice.Lines[0];
			var line2 = testObjectCreator.CreateCashVATLines(invoice, 150, 10);
			var line3 = testObjectCreator.CreateCashVATLines(invoice, 50, 7);
			var line4 = testObjectCreator.CreateCashVATLines(invoice, 200, 15);
			var line5 = testObjectCreator.CreateCashVATLines(invoice, 120, 10);
			var matchLink = testObjectCreator.CreateMatchLinkToPayAPInvoice(invoice, null, -300);
			matchLink.SkipCashBasisVATCreationForTestOnly = true;
			testObjectCreator.CreateCashBasisVAT(line1, -90, -9, matchLink);
			testObjectCreator.CreateCashBasisVAT(line2, -100, -7, matchLink);
			testObjectCreator.CreateCashBasisVAT(line2, -10, -1, matchLink);
			testObjectCreator.CreateCashBasisVAT(line3, -40, -5, matchLink);
			testObjectCreator.CreateCashBasisVAT(line4, -90, -9, matchLink);
			testObjectCreator.CreateCashBasisVAT(line5, -90, -9, matchLink);
			Factory.Save();

			ReleaseFactory();

			testObjectCreator = new TestObjectCreator(Factory);
			var invoiceLoaded = Factory.Load<APInvoice>(invoice.PK);
			var line2Loaded = Factory.Load<APInvoiceLine>(line2.PK);
			var line4Loaded = Factory.Load<APInvoiceLine>(line4.PK);
			var line5Loaded = Factory.Load<APInvoiceLine>(line5.PK);

			var matchLink2 = testObjectCreator.CreateMatchLinkToPayAPInvoice(invoiceLoaded, null, -100);
			matchLink2.AP_MatchGroupNum = "MMM";
			matchLink2.SkipCashBasisVATCreationForTestOnly = true;
			var cashVATInvalid = testObjectCreator.CreateCashBasisVAT(line2Loaded, -50, -2, matchLink2);
			var cashVATValid1 = testObjectCreator.CreateCashBasisVAT(line4Loaded, -20, -2, matchLink2);
			var cashVATValid2 = testObjectCreator.CreateCashBasisVAT(line5Loaded, -10, -1, matchLink2);

			var localCacheFilter = new ZQuery() { FetchOnlyFromLocalCache = true };
			var cstLinesFilter = new ZQuery(localCacheFilter);
			cstLinesFilter.AddToFilter(AccTransactionLinesSchema.AL_AH, invoice.PK);
			AssertEquals("Precondition: Amount of Invoice lines loaded in a Factory", 3, Factory.Load<AccTransactionLines>(cstLinesFilter).Length);
			AssertEquals("Precondition: Amount of Cash VAT records loaded in a Factory", 3, Factory.Load<AccCashBasisVAT>(localCacheFilter).Length);

			Factory.ResetDatabaseLoadCount();

			((ISupportCriticalValidation)cashVATValid1).CriticalValidation.RunOnSavingCheck();

			var tableHints = new Dictionary<string, int>();
			tableHints.Add(AccCashBasisVAT.Schema.TableName, 1);
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			AssertDbHits(tableHints, Factory);
			AssertEquals("DatabaseLoadCount", 1, Factory.DatabaseLoadCount);

			//Load one more line to test that loading more lines in local cache between validations doesn't make more db hits as no AccCashBasisVAT for this line in a Factory 
			//and so we won't run Critical Validation for them during this saving
			var line1Loaded = Factory.Load<APInvoiceLine>(line1.PK);

			Factory.ResetDatabaseLoadCount();

			((ISupportCriticalValidation)cashVATValid2).CriticalValidation.RunOnSavingCheck();
			try
			{
				((ISupportCriticalValidation)cashVATInvalid).CriticalValidation.RunOnSavingCheck();
				Fail("Critical Validation Exception must be thrown");
			}
			catch (OnSavingCriticalCheckException<AccCashBasisVAT> e)
			{
				AssertContains("Exception message", "Sum of Tax Base Amounts for Cash basis tax recognition records cannot exceed linked Line Amount.", e.Message);
			}

			tableHints = new Dictionary<string, int>();
			tableHints.Add(AccChargeCode.Schema.TableName, 1);
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			AssertDbHits(tableHints, Factory);
			AssertEquals("DatabaseLoadCount", 1, Factory.DatabaseLoadCount);

			AssertEquals("Precondition: Amount of Invoice lines loaded in a Factory", 4, Factory.Load<AccTransactionLines>(cstLinesFilter).Length);
			AssertEquals("Precondition: Amount of Cash VAT records loaded in a Factory", 7, Factory.Load<AccTransactionLines>(localCacheFilter).Length);
		}

		TransactionMatchLink GetMatchLinkAndLine(TestObjectCreator testObjectCreator, out AccTransactionLines line, bool zeroValueTransaction = false, string transactionNumber = "INV1")
		{
			var invoice = (APInvoice)testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), transactionNumber, testObjectCreator.AUD, 1, 100, 10, 100, 10);
			if (zeroValueTransaction)
			{
				testObjectCreator.CreateInvoiceLine(invoice, testObjectCreator.AUD, 1, -100, -10, 0, -100, -10, 0);
			}
			line = invoice.Lines[0];
			var lineGST = line.AL_GSTVAT;
			line.AL_AT = testObjectCreator.GST1.PK;
			line.AL_GSTVAT = lineGST;
			line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			var matchLink = testObjectCreator.CreateMatchLinkToPayAPInvoice(invoice);
			matchLink.SkipCashBasisVATCreationForTestOnly = true;

			return matchLink;
		}

		AccCashBasisVAT GetCashBasisVAT(BusinessObjectFactory factory, string transactionNumber = "INV1")
		{
			var testObjectCreator = new TestObjectCreator(factory);
			AccTransactionLines line;
			var matchLink = GetMatchLinkAndLine(testObjectCreator, out line, false, transactionNumber);
			var cashVATRecord = testObjectCreator.CreateCashBasisVAT(line, -90, -9, matchLink);

			return cashVATRecord;
		}
	}
}
