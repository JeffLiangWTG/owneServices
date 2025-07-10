using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using static Enterprise.Accounting.Business.JobInvoicing.BranchLevelPostingHelper;

namespace Enterprise.Accounting.Business.CashBook.Testing
{
	public abstract class DirectTransactionHeaderBaseTest : TransactionHeaderWithLinesTest
	{
		protected override ZDecimal GetExpectedOutstandindAmount(ZDecimal expectedValue) => 0;

		protected override void SetupHeaderForReversing(ZDecimal aH_OSExTaxAmount, ZDecimal aH_OSTaxAmount)
		{
			base.SetupHeaderForReversing(aH_OSExTaxAmount, aH_OSTaxAmount);
			Header.AH_LocalOutstandingAmount = 0m;
		}

		public void TestNoErrorsWhenReversing()
		{
			DirectTransactionHeaderBase testHeader = (DirectTransactionHeaderBase)Factory.New(GetExpectedBusinessObjectType());
			DirectTransactionLineBase testLine;
			if (testHeader.Lines.Count == 0)
			{
				testLine = (DirectTransactionLineBase)testHeader.Lines.AddNew();
			}
			else
			{
				testLine = (DirectTransactionLineBase)testHeader.Lines[0];
			}

			testHeader.AH_ChequeDrawer = "a";
			testHeader.AH_DrawerBank = "b";
			testHeader.AH_DrawerBranch = "c";
			testLine.AL_AG = TestObjectCreator.CreateGLHeader().PK;
			testLine.AL_AT = TestObjectCreator.GST1.PK;
			testLine.AL_A9_VATClass = TestObjectCreator.TaxMsg1.PK;
			testLine.AL_OSExTaxAmount = 100m;
			testLine.AL_OSTaxAmount = 15m;
			testLine.AL_InputGSTVATRecoverable = 0.44m;
			Factory.Save();

			TestObjectCreator.GST1.AT_IsActive = false;
			testHeader.GenerateReverseTransaction(true);
			AssertNoErrors(((DirectTransactionHeaderBase)testHeader.FReverseTransaction_ForTestOnly).Lines[0].AL_ATInfo);
		}

		public void TestAL_SupplyWhenReversing()
		{
			var testHeader = (DirectTransactionHeaderBase)Factory.New(GetExpectedBusinessObjectType());
			DirectTransactionLineBase testLine;
			if (testHeader.Lines.Count == 0)
			{
				testLine = (DirectTransactionLineBase)testHeader.Lines.AddNew();
			}
			else
			{
				testLine = (DirectTransactionLineBase)testHeader.Lines[0];
			}

			testHeader.AH_ChequeDrawer = "a";
			testHeader.AH_DrawerBank = "b";
			testHeader.AH_DrawerBranch = "c";
			testLine.AL_AG = TestObjectCreator.CreateGLHeader().PK;
			testLine.AL_AT = TestObjectCreator.GST1.PK;
			testLine.AL_A9_VATClass = TestObjectCreator.TaxMsg1.PK;
			testLine.AL_OSExTaxAmount = 100m;
			testLine.AL_OSTaxAmount = 15m;
			testLine.AL_InputGSTVATRecoverable = 0.44m;
			testLine.AL_SupplyType = "LOC";
			Factory.Save();

			TestObjectCreator.GST1.AT_IsActive = false;
			testHeader.GenerateReverseTransaction(true);

			var reverseTransaction = (DirectTransactionHeaderBase)testHeader.ReverseTransaction;
			AssertEquals("Reversing transaction AL_Supply is 'LOC'", 1, reverseTransaction.Lines.Count(x => ((TransactionLine)x).AL_SupplyType == "LOC"));
		}

		public void TestTaxBranchWhenReversing()
		{
			var branch = TestObjectCreator.CreateBranch("TST", GlbCompany.CurrentCompany);

			var testHeader = (DirectTransactionHeaderBase)Factory.New(GetExpectedBusinessObjectType());
			DirectTransactionLineBase testLine;
			if (testHeader.Lines.Count == 0)
			{
				testLine = (DirectTransactionLineBase)testHeader.Lines.AddNew();
			}
			else
			{
				testLine = (DirectTransactionLineBase)testHeader.Lines[0];
			}

			testHeader.AH_ChequeDrawer = "a";
			testHeader.AH_DrawerBank = "b";
			testHeader.AH_DrawerBranch = "c";
			testHeader.AH_GB_TaxBranch = branch.PK;
			testLine.AL_AG = TestObjectCreator.CreateGLHeader().PK;
			testLine.AL_AT = TestObjectCreator.GST1.PK;
			testLine.AL_A9_VATClass = TestObjectCreator.TaxMsg1.PK;
			testLine.AL_OSExTaxAmount = 100m;
			testLine.AL_OSTaxAmount = 15m;
			testLine.AL_InputGSTVATRecoverable = 0.44m;
			testLine.AL_GB_TaxBranch = branch.PK;
			Factory.Save();

			TestObjectCreator.GST1.AT_IsActive = false;
			testHeader.GenerateReverseTransaction(true);

			var reverseTransaction = (DirectTransactionHeaderBase)testHeader.ReverseTransaction;
			AssertEquals("Reversing transaction AH_GB_TaxBranch", branch.PK, reverseTransaction.AH_GB_TaxBranch);
			Assert(reverseTransaction.Lines.All(x => ((TransactionLine)x).AL_GB_TaxBranch == branch.PK));
		}

		public void TestChequeBookIsRetainedOnReload()
		{
			if (TestBizO.AH_TransactionType == TransactionTypes.DirectPayment)
			{
				TestBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
				TestBizO.AH_AB = TestObjectCreator.AUDBankAccount.PK;
				TestBizO.ChequeBookPK = TestObjectCreator.AUDChequeBook.PK;
				TestBizO.AH_ChequeDrawer = "Cheque Drawer";

				DependentTransactionLine line = TestBizO.Lines.AddNew();
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
				line.AL_OSExTaxAmount = 100m;
				line.AL_AT = TestObjectCreator.GSTFREE1.PK;

				Factory.Save();

				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				DirectTransactionHeaderBase testBizOInNewFactory = (DirectTransactionHeaderBase)newFactory.Load(TestBizO.GetType(), TestBizO.PK);
				AssertEquals("Cheque Book", TestObjectCreator.AUDChequeBook.PK, testBizOInNewFactory.ChequeBookPK);
			}
			else
			{
				Assert("This test is only applicable to Direct Payments", true);
			}
		}

		public virtual void TestLookups()
		{
			Assert("Lookups is DirectTransactionHeaderBaseLookups", TestBizO.Lookups is DirectTransactionHeaderBaseLookups);
		}

		public void TestBankAccounts_ContainsOnlyActiveBanks()
		{
			AccBankAccount activeBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount inactiveBank = Factory.NewWithValidTestData<AccBankAccount>();
			inactiveBank.AB_IsActive = false;

			DirectTransactionHeaderBase testHeader = (DirectTransactionHeaderBase)Factory.New(GetExpectedBusinessObjectType());
			testHeader.BankAccounts.Load();
			AssertEquals("Should contain active bank", true, testHeader.BankAccounts.Contains(activeBank));
			AssertEquals("Should not contain inactive bank", false, testHeader.BankAccounts.Contains(inactiveBank));
		}

		public void TestSettingInvoiceDateSetsDueDate()
		{
			DirectTransactionHeaderBase testHeader = (DirectTransactionHeaderBase)Factory.New(GetExpectedBusinessObjectType());
			testHeader.AH_InvoiceDate = ZDateTime.Now.AddDays(10);
			AssertEquals(testHeader.AH_DueDate, testHeader.AH_InvoiceDate);
		}

		public void TestSettingHeaderCurrencySetsLineCurrencies()
		{
			DirectTransactionHeaderBase testHeader = (DirectTransactionHeaderBase)Factory.New(GetExpectedBusinessObjectType());
			DirectTransactionLineBase line = (DirectTransactionLineBase)testHeader.Lines.AddNew();

			ZString currency = Core.Constants.CurrencyCodes.Australia;
			testHeader.AH_RX_NKTransactionCurrency = currency;
			AssertEquals(line.AL_RX_NKTransactionCurrency, currency);
		}

		public void TestAH_TransactionNumAlwaysReadOnly()
		{
			AssertEquals("Transaction Number Should be ReadOnly", true, TestBizO.AH_TransactionNumInfo.ReadOnly);
		}

		public virtual void TestAH_ReceiptTypeDefaultValue()
		{
			AssertEquals("Default receipt type should be Cheque", ZArchitecture.Core.ReceiptTypes.Cheque, TestBizO.AH_ReceiptType);
		}

		public void TestAH_OSTotalAmountReadOnly()
		{
			AssertEquals("AH_OSTotalAmount should be readonly", true, TestBizO.AH_OSTotalAmountInfo.ReadOnly);
		}

		public void TestAH_LocalTotalAmountReadOnly()
		{
			AssertEquals("AH_LocalTotalAmount should be readonly", true, TestBizO.AH_LocalTotalAmountInfo.ReadOnly);
		}

		public void TestBankAccountCurrency()
		{
			TestBizO.AH_AB = BankAccount.PK;
			AssertEquals(TestBizO.AH_AB, TestBizO.BankAccount.PK);
			TestBizO.AH_AB = TestBankOtherCurrency.PK;
			AssertEquals(TestBizO.AH_RX_NKTransactionCurrency, TestBankOtherCurrency.AB_RX_NKAccountCurrency);
		}

		public void TestCurrencyListReadOnly()
		{
			AssertEquals(true, TestBizO.ExchangeRate.CurrencyInfo.ReadOnly);
		}

		public void TestDefaultReceiptType()
		{
			var header = (DirectTransactionHeaderBase)Factory.New(GetExpectedBusinessObjectType());

			if (header is BankTransferCharge)
			{
				AssertEquals(ReceiptTypes.EFT, header.AH_ReceiptType);

				AccountingConfigurationRegistry.Instance.DefaultCashBookPaymentType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReceiptTypes.Cash);
				header = (DirectTransactionHeaderBase)Factory.New(GetExpectedBusinessObjectType());
				AssertEquals(ReceiptTypes.EFT, header.AH_ReceiptType);

				AccountingConfigurationRegistry.Instance.DefaultCashBookReceiptType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReceiptTypes.Cash);
				header = (DirectTransactionHeaderBase)Factory.New(GetExpectedBusinessObjectType());
				AssertEquals(ReceiptTypes.EFT, header.AH_ReceiptType);
			}
			else
			{
				AssertEquals(ReceiptTypes.Cheque, header.AH_ReceiptType);

				if (header is DirectPayment.DirectPayment)
				{
					AccountingConfigurationRegistry.Instance.DefaultCashBookPaymentType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReceiptTypes.Cash);
					header = (DirectTransactionHeaderBase)Factory.New(GetExpectedBusinessObjectType());
					AssertEquals(ReceiptTypes.Cash, header.AH_ReceiptType);
				}
				else
				{
					AccountingConfigurationRegistry.Instance.DefaultCashBookReceiptType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReceiptTypes.Cash);
					header = (DirectTransactionHeaderBase)Factory.New(GetExpectedBusinessObjectType());
					AssertEquals(ReceiptTypes.Cash, header.AH_ReceiptType);
				}
			}
		}

		public void TestDefaultReceiptTypeReferenceNumber()
		{
			AccountingConfigurationRegistry.Instance.DefaultCashBookPaymentType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReceiptTypes.CreditCard);
			AccountingConfigurationRegistry.Instance.DefaultCashBookReceiptType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReceiptTypes.CreditCard);
			var list = AccountingConfigurationRegistry.Instance.PaymentReceiptTypeReferenceNumberRegistryDefaults.Value;
			var element1 = list.Cast<PaymentReceiptTypeReferenceNumber>().First(x => x.Type == ReceiptTypes.EFT);
			element1.ReferenceNumber = "Test1";
			var element2 = list.Cast<PaymentReceiptTypeReferenceNumber>().First(x => x.Type == ReceiptTypes.CreditCard);
			element2.ReferenceNumber = "Test2";
			AccountingConfigurationRegistry.Instance.PaymentReceiptTypeReferenceNumberRegistryDefaults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var header = (DirectTransactionHeaderBase)Factory.New(GetExpectedBusinessObjectType());

			if (header is BankTransferCharge)
			{
				AssertEquals(ReceiptTypes.EFT, header.AH_ReceiptType);
				AssertEquals("Test1", header.AH_ChequeOrReference);

				header.AH_ReceiptType = ReceiptTypes.Cash;
				AssertEquals(ReceiptTypes.Cash, header.AH_ReceiptType);
				AssertEquals("CASH", header.AH_ChequeOrReference);
			}
			else
			{
				AssertEquals(ReceiptTypes.CreditCard, header.AH_ReceiptType);
				AssertEquals("Test2", header.AH_ChequeOrReference);

				header.AH_ReceiptType = ReceiptTypes.Cash;
				AssertEquals(ReceiptTypes.Cash, header.AH_ReceiptType);
				AssertEquals("CASH", header.AH_ChequeOrReference);
			}
		}

		public void TestReceiptTypeLabel()
		{
			foreach (ICodeDescription paymentMethod in TestBizO.PaymentMethods)
			{
				TestBizO.AH_ReceiptType = paymentMethod.Code;

				if (TestBizO.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque)
				{
					AssertEquals("Check No.", TestBizO.AH_ChequeOrReferenceLabel_Calc);
					AssertEquals("Check No.:", TestBizO.AH_Calc_ReceiptTypeLabel);
				}
				else
				{
					AssertEquals("Reference No.", TestBizO.AH_ChequeOrReferenceLabel_Calc);
					AssertEquals("Reference No.:", TestBizO.AH_Calc_ReceiptTypeLabel);
				}
			}
		}

		public void TestReceiptTypeAndAH_ChequeOrReference()
		{
			foreach (ICodeDescription paymentMethod in TestBizO.PaymentMethods)
			{
				TestBizO.AH_ReceiptType = paymentMethod.Code;

				if (TestBizO.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cash)
				{
					AssertEquals("CASH", TestBizO.AH_ChequeOrReference);
				}
				else
				{
					AssertEquals(ZString.Empty, TestBizO.AH_ChequeOrReference);
				}
			}
		}

		public void TestReceiptTypeAndChequeDetailsReset()
		{
			TestBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			TestBizO.AH_ChequeDrawer = "ABC";
			TestBizO.AH_DrawerBank = "BANK";
			TestBizO.AH_DrawerBranch = "BRANCH";

			TestBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.CreditCard;

			AssertEquals(ZString.Empty, TestBizO.AH_ChequeDrawer);
			AssertEquals(ZString.Empty, TestBizO.AH_DrawerBank);
			AssertEquals(ZString.Empty, TestBizO.AH_DrawerBranch);
		}

		public void TestSetDefaultValues_AH_GB_TaxBranch()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				TestBizO.SetDefaultValues_ForTestOnly();
				AssertEquals(GlbBranch.CurrentBranch.PK, TestBizO.AH_GB_TaxBranch);
			}
		}

		public override void TestExchangeRateTypeDependsOnLedgerAndType()
		{
			if (TestBizO.AH_TransactionType == "DPY")
			{
				AssertEquals(ExchangeRateType.Buy, TestBizO.ExchangeRate.Type);
			}
			else
			{
				AssertEquals(ExchangeRateType.Sell, TestBizO.ExchangeRate.Type);
			}
		}

		public override void TestInternalOSAmountFieldsSetOnLoadCorrectly()
		{
			TestBizO.AH_AB = BankAccount.PK;
			TestBizO.AH_TransactionNum = "111";
			TestBizO.AH_TransactionReference = "abc";
			TestBizO.AH_Desc = "test receipt";
			TestBizO.AH_InvoiceDate = new ZDateTime(2000, 1, 20);
			TestBizO.AH_ChequeOrReference = "00123";
			TestBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			TestBizO.AH_ChequeDrawer = "bbb";
			TestBizO.AH_DrawerBank = "ccc";
			TestBizO.AH_DrawerBranch = "ddd";

			DirectTransactionLineBase testLine = (DirectTransactionLineBase)TestBizO.Lines.AddNew();
			testLine.AL_AG = GLAccount.PK;
			testLine.AL_GB = GlbBranch.CurrentBranch.PK;
			testLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			testLine.AL_OSExTaxAmount = 10;
			testLine.AL_AT = TestObjectCreator.GST1.PK;
			testLine.AL_Desc = "test line";
			Factory.Save();

			BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();

			DirectTransactionHeaderBase loadedHeader = (DirectTransactionHeaderBase)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), TestBizO.PK);

			AssertEquals(10m, loadedHeader.AH_LocalExTaxAmount);
			AssertEquals(11m, loadedHeader.AH_LocalTotalAmount);
			AssertEquals(10m, loadedHeader.AH_OSExTaxAmount);
			AssertEquals(11m, loadedHeader.AH_OSTotalAmount);
		}

		public void TestForeignCurrencyPosting()
		{
			AccBankAccount foreignBankAccount = TestObjectCreator.InsertBankAccount(TestObjectCreator.USD.RX_Code);
			foreignBankAccount.AB_Code = "zxcdgs";
			foreignBankAccount.AB_AccountNum = "9831zxv";
			foreignBankAccount.AB_BSB = "398195";
			TestBizO.AH_AB = foreignBankAccount.PK;
			TestBizO.AH_ExchangeRate = 2m;
			TestBizO.AH_Desc = "test receipt";
			TestBizO.AH_ChequeOrReference = "00123";
			TestBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			TestBizO.AH_ChequeDrawer = "bbb";
			TestBizO.AH_DrawerBank = "ccc";
			TestBizO.AH_DrawerBranch = "ddd";

			DirectTransactionLineBase testLine = (DirectTransactionLineBase)TestBizO.Lines.AddNew();
			testLine.AL_AG = GLAccount.PK;
			testLine.AL_GB = GlbBranch.CurrentBranch.PK;
			testLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			testLine.AL_OSExTaxAmount = 10;
			testLine.AL_AT = TestObjectCreator.GST1.PK;
			testLine.AL_Desc = "test line";
			Factory.Save();

			BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();

			DirectTransactionHeaderBase loadedHeader = (DirectTransactionHeaderBase)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), TestBizO.PK);

			AssertEquals(5m, loadedHeader.AH_LocalExTaxAmount);
			AssertEquals(5.5m, loadedHeader.AH_LocalTotalAmount);
			AssertEquals(10m, loadedHeader.AH_OSExTaxAmount);
			AssertEquals(11m, loadedHeader.AH_OSTotalAmount);
		}

		public virtual void TestSetReceiptTypeOnly()
		{
			TestBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			TestBizO.AH_DrawerBank = "123456789";
			TestBizO.AH_DrawerBranch = "123456";
			TestBizO.AH_ChequeDrawer = "ChequeDrawer";

			TestBizO.SetReceiptTypeOnly_ForTestOnly(ZArchitecture.Core.ReceiptTypes.DirectDebitLine);
			AssertEquals(ZArchitecture.Core.ReceiptTypes.DirectDebitLine, TestBizO.AH_ReceiptType);
			AssertEquals("123456789", TestBizO.AH_DrawerBank);
			AssertEquals("123456", TestBizO.AH_DrawerBranch);
			AssertEquals("ChequeDrawer", TestBizO.AH_ChequeDrawer);
		}

		public void TestRelatedStatementPK()
		{
			ZGuid testPK = ZGuid.NewZGuid();
			TestBizO.RelatedStatementPK = testPK;
			AssertEquals("RelatedStatementPK", testPK, TestBizO.RelatedStatementPK);
		}

		public override void TestGenerateReverseTransaction()
		{
			var testHeader = (DirectTransactionHeaderBase)Factory.New(GetExpectedBusinessObjectType());
			DirectTransactionLineBase testLine;
			if (testHeader.Lines.Count == 0)
			{
				testLine = (DirectTransactionLineBase)testHeader.Lines.AddNew();
			}
			else
			{
				testLine = (DirectTransactionLineBase)testHeader.Lines[0];
			}
			var testLine2 = (DirectTransactionLineBase)testHeader.Lines.AddNew();

			testHeader.AH_ChequeDrawer = "a";
			testHeader.AH_DrawerBank = "b";
			testHeader.AH_DrawerBranch = "c";
			testLine.AL_AG = TestObjectCreator.CreateGLHeader().PK;
			testLine.AL_AT = TestObjectCreator.GST1WithDates.PK;
			testLine.AL_TaxDate = TestObjectCreator.GST1WithDates_DateWithNoRate;
			testLine.AL_TaxRateNumerator = 4;
			testLine.AL_TaxRateDenominator = 7;
			testLine.AL_TaxExtraRateNumerator = 14;
			testLine.AL_TaxExtraRateDenominator = 17;
			testLine.AL_A9_VATClass = TestObjectCreator.TaxMsg1.PK;
			testLine.AL_OSExTaxAmount = 100m;
			testLine.AL_OSTaxAmount = 15m;
			testLine.AL_InputGSTVATRecoverable = 0.44m;
			testLine.AL_GovtChargeCode = "NTCC1";

			testLine2.AL_AT = TestObjectCreator.GST1.PK;
			testLine2.AL_TaxDate = ZDate.Empty;
			testLine2.AL_TaxRateNumerator = 41;
			testLine2.AL_TaxRateDenominator = 71;
			testLine2.AL_TaxExtraRateNumerator = 141;
			testLine2.AL_TaxExtraRateDenominator = 171;
			testLine.AL_OSExTaxAmount = 200m;
			testLine.AL_OSTaxAmount = 25m;

			Factory.Save();

			testHeader.GenerateReverseTransaction(true);

			AssertEquals(testHeader.FReverseTransaction_ForTestOnly.AH_ChequeDrawer, testHeader.AH_ChequeDrawer);
			AssertEquals(testHeader.FReverseTransaction_ForTestOnly.AH_DrawerBank, testHeader.AH_DrawerBank);
			AssertEquals(testHeader.FReverseTransaction_ForTestOnly.AH_DrawerBranch, testHeader.AH_DrawerBranch);

			var testReverseHeader = ((DirectTransactionHeaderBase)testHeader.FReverseTransaction_ForTestOnly);

			testReverseHeader.AH_IsCancelled = true;
			AssertType("Precondition: Reverse validation is used on lines", typeof(TransactionLineEmptyValidation), testReverseHeader.Lines[0].Validation);
			testReverseHeader.RunPreSaveValidation();

			var testReversedLine = testReverseHeader.Lines[0];

			AssertEquals(testLine.AL_AG, testReversedLine.AL_AG);
			AssertEquals(testLine.AL_GB, testReversedLine.AL_GB);
			AssertEquals(testLine.AL_GE, testReversedLine.AL_GE);
			AssertEquals(testLine.AL_Desc, testReversedLine.AL_Desc);
			AssertEquals(TestObjectCreator.GST1WithDates.PK, testReversedLine.AL_AT);
			AssertEquals(TestObjectCreator.GST1WithDates_DateWithNoRate, testReversedLine.AL_TaxDate);
			AssertHasError(testLine.AL_TaxDateInfo, "No rate found for selected date.");
			AssertNoErrors(testReversedLine.AL_TaxDateInfo);
			AssertEquals(4, testReversedLine.AL_TaxRateNumerator);
			AssertEquals(7, testReversedLine.AL_TaxRateDenominator);
			AssertEquals(14, testReversedLine.AL_TaxExtraRateNumerator);
			AssertEquals(17, testReversedLine.AL_TaxExtraRateDenominator);
			AssertEquals(testLine.AL_A9_VATClass, testReversedLine.AL_A9_VATClass);
			AssertEquals(testLine.AL_RX_NKTransactionCurrency, testReversedLine.AL_RX_NKTransactionCurrency);
			AssertEquals(testLine.AL_ExchangeRate, testReversedLine.AL_ExchangeRate);
			AssertEquals(testLine.AL_LineAmount * -1, testReversedLine.AL_LineAmount);
			AssertEquals(testLine.AL_OSAmount * -1, testReversedLine.AL_OSAmount);
			AssertEquals(testLine.AL_GSTVAT * -1, testReversedLine.AL_GSTVAT);
			AssertEquals(0.44m, testReversedLine.AL_InputGSTVATRecoverable);
			AssertEquals("NTCC1", testLine.AL_GovtChargeCode);

			testReversedLine = testReverseHeader.Lines[1];
			AssertEquals(TestObjectCreator.GST1.PK, testReversedLine.AL_AT);
			AssertEquals(ZDate.Empty, testReversedLine.AL_TaxDate);
			AssertHasError(testLine2.AL_TaxDateInfo, "Please enter a Tax Date.");
			AssertNoErrors(testReversedLine.AL_TaxDateInfo);
			AssertEquals(41, testReversedLine.AL_TaxRateNumerator);
			AssertEquals(71, testReversedLine.AL_TaxRateDenominator);
			AssertEquals(141, testReversedLine.AL_TaxExtraRateNumerator);
			AssertEquals(171, testReversedLine.AL_TaxExtraRateDenominator);

			AssertEquals(0m, testReverseHeader.AH_OutstandingAmount);
			AssertEquals(testHeader.AH_InvoiceAmount, -testReverseHeader.AH_InvoiceAmount);
			AssertEquals(testHeader.AH_GSTAmount, -testReverseHeader.AH_GSTAmount);
			AssertEquals(testHeader.AH_OSTotal, -testReverseHeader.AH_OSTotal);
		}

		public void TestChequeBooksCollectionOfValidType()
		{
			AssertEquals("ChequeBooksColleciton should be of valid type", typeof(ActiveChequeBookCollection), TestBizO.ChequeBooks.GetType());
		}

		public void TestCanApplyTaxBranch()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals(false, TestBizO.CanApplyTaxBranch);

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals(false, TestBizO.CanApplyTaxBranch);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals(false, TestBizO.CanApplyTaxBranch);

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals(true, TestBizO.CanApplyTaxBranch);
		}

		public void TestColumnsReadOnlyForBankStatement()
		{
			var affectedBasicProps = new [] {
				DirectTransactionHeaderBase.Schema.AH_TransactionType,
				DirectTransactionHeaderBase.Schema.AH_RX_NKTransactionCurrency,
				DirectTransactionHeaderBase.Schema.AH_ExchangeRate,
				DirectTransactionHeaderBase.Schema.AH_ChequeDrawer,
				DirectTransactionHeaderBase.Schema.AH_ChequeOrReference,
				DirectTransactionHeaderBase.Schema.AH_PostDate,
				DirectTransactionHeaderBase.Schema.AH_InvoiceDate,
				DirectTransactionHeaderBase.Schema.AH_DrawerBranch,
				DirectTransactionHeaderBase.Schema.AH_ReceiptType,
				DirectTransactionHeaderBase.Schema.AH_DrawerBank
			};

			var testProps = GetMostColumnsReadOnlyAffectedProp().ToArray();
			AssertEquals("PreCondition, must have all basic props testing", false, affectedBasicProps.Except(testProps.Select(x => x.PropertyName)).Any());

			CombineAssertions(() => {
				foreach (var testProp in testProps)
				{
					var testHeader = GetNewBusinessObject() as DirectTransactionHeaderBase;
					testProp.Init(testHeader);
					AssertEquals($"{testProp.PropertyName},should be not read only before setting ReadonlyConfigForBankStatement as true.", false, testHeader.ZPropertyInfoHash[testProp.PropertyName].ReadOnly);

					testHeader.SetReadOnlyForBankStatement(true);
					AssertEquals($"{testProp.PropertyName},should be read only after setting ReadonlyConfigForBankStatement as true.", true, testHeader.ZPropertyInfoHash[testProp.PropertyName].ReadOnly);
				}
			});
		}

		protected abstract IEnumerable<(string PropertyName, Action<DirectTransactionHeaderBase> Init)> GetMostColumnsReadOnlyAffectedProp();

		public virtual void TestIsChequeNumberAutoAllocated()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook autoPrintChequeBook = GetAutoPrintChequeBook(testBank, 1, 3, 2);
			DirectTransactionHeaderBase testHeader = (DirectTransactionHeaderBase)Factory.New(GetExpectedBusinessObjectType());
			testHeader.AH_AB = testBank.PK;
			testHeader.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testHeader.ChequeBookPK = autoPrintChequeBook.PK;
			Assert("Auto allocation mode should not be enabled by default", !testHeader.IsChequeNumberAutoAllocated_ForTestOnly);
			AssertEquals("current cheque number should be set", "2", testHeader.AH_ChequeOrReference);
			Assert("ChequeOrReference field should not be read only", !testHeader.AH_ChequeOrReferenceInfo.ReadOnly);
		}

		public void TestIsTaxed()
		{
			var testHeader = GetNewBusinessObject() as DirectTransactionHeaderBase;
			DirectTransactionLineBase line1 = (DirectTransactionLineBase)testHeader.Lines.AddNew();
			DirectTransactionLineBase line2 = (DirectTransactionLineBase)testHeader.Lines.AddNew();
			Assert(!testHeader.IsInDatabase);

			Factory.ClearCachedValue<ZBool>("IsTaxed:" + testHeader.PK.ToStringKey());
			Factory.GetCachedValue("IsTaxed:" + testHeader.PK.ToStringKey(), () => ZBool.True);

			AssertEquals("IsTaxed should be false as it ignores cached value and calclates based on lines when transaction is NOT in database", false, testHeader.IsTaxed);

			Factory.ClearCachedValue<ZBool>("IsTaxed:" + testHeader.PK.ToStringKey());
			Factory.GetCachedValue("IsTaxed:" + testHeader.PK.ToStringKey(), () => ZBool.False);

			line1.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
			AssertEquals("IsTaxed should be true as it ignores cached value and calclates based on lines when transaction is NOT in database", true, testHeader.IsTaxed);

			Factory.Save();
			Assert(testHeader.IsInDatabase);

			Factory.ClearCachedValue<ZBool>("IsTaxed:" + testHeader.PK.ToStringKey());
			Factory.GetCachedValue("IsTaxed:" + testHeader.PK.ToStringKey(), () => ZBool.False);
			AssertEquals("IsTaxed should be false as it reads from the cache when transaction is in database", false, testHeader.IsTaxed);
		}

		public void TestDirectTransactionAH_GB_SetTransactionHeaderBranch()
		{
			var newFactory = new BusinessObjectFactory();
			var objectCreator = new TestObjectCreator(newFactory);

			var expectedBranchPK = objectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany).PK;

			var directTransactionHeader = (DirectTransactionHeaderBase)newFactory.New(GetExpectedBusinessObjectType());
			directTransactionHeader.AH_GB = expectedBranchPK;
			var directTransactionLine = (DirectTransactionLineBase)directTransactionHeader.Lines.AddNew();
			if (!directTransactionHeader.IsSavedByFactory)
			{
				ForceSavingByFactoryIfApplicable(directTransactionHeader);
			}

			var mockIAccountingDependencyFactory = new Mock<IAccountingDependencyFactory>();
			var branchLevelPostingHelperMock = new Mock<IBranchLevelPostingHelper>();
			ObjectFactory.Substitute(mockIAccountingDependencyFactory.Object);

			mockIAccountingDependencyFactory.Setup(x => x.GetBranchLevelPostingHelper()).Returns(branchLevelPostingHelperMock.Object);

			var initialBranchPKValueInPassedTransaction = ZGuid.Empty;
			branchLevelPostingHelperMock.Setup(x => x.SetTransactionHeaderBranch(directTransactionHeader, InvoiceProcessingLevelIsAllowingToResetBranch.Saving, It.IsAny<ITransactionBranchCalculationDataProviderFromJobCharge>()))
				.Callback<TransactionHeaderWithLines, InvoiceProcessingLevelIsAllowingToResetBranch, ITransactionBranchCalculationDataProviderFromJobCharge>(
					(transaction, invoiceLevel, charges) =>
					{
						initialBranchPKValueInPassedTransaction = transaction.AH_GB;
					}
				);

			newFactory.Save();

			AssertEquals("initialBranchPKValueInPassedTransaction: ", expectedBranchPK, initialBranchPKValueInPassedTransaction);
			branchLevelPostingHelperMock.Verify(x => x.SetTransactionHeaderBranch(It.IsAny<DirectTransactionHeaderBase>(), It.IsAny<InvoiceProcessingLevelIsAllowingToResetBranch>(), It.IsAny<ITransactionBranchCalculationDataProviderFromJobCharge>()), Times.Once);
			branchLevelPostingHelperMock.Verify(x => x.SetTransactionHeaderBranch(directTransactionHeader, InvoiceProcessingLevelIsAllowingToResetBranch.Saving, It.IsAny<ITransactionBranchCalculationDataProviderFromJobCharge>()));
		}

		public virtual void TestDirectTransactionAH_GB_BranchLevelPostingEnabled()
		{
			var header = (DirectTransactionHeaderBase)Factory.New(GetExpectedBusinessObjectType());
			header.AH_GB = GlbBranch.CurrentBranch.PK;

			if (header.EnforceBranchLevelPostingRegistryItem != null)
			{
				var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
				header.EnforceBranchLevelPostingRegistryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

				var line = header.Lines.AddNew();
				line.AL_GB = TestObjectCreator.NonCurrentBranch.PK;

				Factory.Save();

				AssertEquals("Header and Line branch is the same after saving, since branch level posting is enabled", header.AH_GB, line.AL_GB);
			}
			else
			{
				Assert("This test is not applicable to this transaction", true);
			}
		}

		public virtual void TestDirectTransactionAH_GB_BranchLevelPostingEnabled_Reversed()
		{
			var header = (DirectTransactionHeaderBase)Factory.New(GetExpectedBusinessObjectType());
			header.AH_GB = GlbBranch.CurrentBranch.PK;

			if (header.EnforceBranchLevelPostingRegistryItem != null)
			{
				var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = false };
				header.EnforceBranchLevelPostingRegistryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

				var line = header.Lines.AddNew();
				line.AL_GB = TestObjectCreator.NonCurrentBranch.PK;

				Factory.Save();

				AssertNotEquals("Header and Line branch is not the same, since Branch level posting is not enabled", header.AH_GB, line.AL_GB);

				branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
				header.EnforceBranchLevelPostingRegistryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

				header.GenerateReverseTransaction(true);

				var reversedTransaction = (DirectTransactionHeaderBase)header.ReverseTransaction;

				AssertNotEquals("Header and Line branch is not the same, though branch level posting is enabled. This is because reversed transaction should follow the original invoice", reversedTransaction.AH_GB, reversedTransaction.Lines[0].AL_GB);
			}
			else
			{
				Assert("This test is not applicable to this transaction", true);
			}
		}

		public void TestBehaviourWhenCashAccountSelected()
		{
			var testBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			testBankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
			testBankAccount.AB_RX_NKAccountCurrency = "USD";

			var header = PrepareTransactionHeaderForTest() as DirectTransactionHeaderBase;
			header.AH_AB = testBankAccount.PK;

			AssertEquals("Receipt type's value should be 'Cash' after a Cash Account is selected", ReceiptTypes.Cash, header.AH_ReceiptType);
			Assert("Receipt type should not be readonly", !header.AH_ReceiptTypeInfo.ReadOnly);

			AssertEquals("The currency should be the same as selected cash account", testBankAccount.AB_RX_NKAccountCurrency, header.AH_RX_NKTransactionCurrency);
			AssertEquals("The currency should be the same as selected cash account", testBankAccount.AB_RX_NKAccountCurrency, header.ExchangeRate.Currency);
			Assert("Exchange rate currency should be readonly", header.ExchangeRate.CurrencyInfo.ReadOnly);
			Assert("Receipt amount currency should be readonly", header.AH_RX_NKTransactionCurrencyInfo.ReadOnly);
		}

		public override void TestDefaultPostDateReadOnly()
		{
			Assert("AH_PostDate should not be readonly", !Header.AH_PostDateInfo.ReadOnly);
		}

		public override void TestAH_PostDate_ReadOnly()
		{
			Assert("AH_PostDate should not be readonly", !Header.AH_PostDateInfo.ReadOnly);
		}

		public override void TestRevenueRecognitionTypeNotEmptyWhenJobUpdatedByDataRefreshAfterSetOnLine()
		{
			Assert("Not Job related transaction", true);
		}

		protected override bool IsJobRelatedTransaction => false;

		#region Implementation

		protected AccChequeBook GetAutoPrintChequeBook(AccBankAccount bankAccount, ZDecimal startNO, ZDecimal lastNO, ZDecimal currentNO)
		{
			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			BusinessObject printQueue = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.DocumentEngine.IStmPrintQueue)));
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			chequeBook.AK_StartNo = startNO;
			chequeBook.AK_LastNo = lastNO;
			chequeBook.AK_CurrentNo = currentNO;
			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			Factory.Save();
			return chequeBook;
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestBizO = (DirectTransactionHeaderBase)Header;
			TestBankOtherCurrency = TestObjectCreator.CreateBankAccount("BBB", "Other Currency Bank Account", TestObjectCreator.USD, null);
		}

		protected AccBankAccount TestBankOtherCurrency;
		protected DirectTransactionHeaderBase TestBizO;

		#endregion
	}
}
