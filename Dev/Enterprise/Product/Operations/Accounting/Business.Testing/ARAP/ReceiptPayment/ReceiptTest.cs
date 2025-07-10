using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	public abstract class ReceiptTest : ReceiptPaymentBaseTest
	{
		#region TestCreateDeveloperExceptionIfFractionAmountIsLongerThanAllowed

		public void TestCreateDeveloperExceptionIfFractionAmountIsLongerThanAllowed()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OrgHeader testOrg = newFactory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_IsCreditor = true;
			testOrg.OH_IsDebtor = true;
			newFactory.Save();

			ReceiptPaymentBase.AH_OH = testOrg.PK;
			ReceiptPaymentBase.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			ReceiptPaymentBase.AH_ChequeDrawer = "Test1";
			ReceiptPaymentBase.AH_DrawerBank = "Test2";
			ReceiptPaymentBase.AH_DrawerBranch = "Test3";
			ReceiptPaymentBase.AH_ChequeOrReference = "123";

			ReceiptPaymentBase.AH_OSExTaxAmount = 90.11M;

			var defaultValue = GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio;
			try
			{
				ReceiptPaymentBase.Company.LocalCurrency.RX_SubUnitRatio = 0;

				ReceiptPaymentBase.RunPreSaveValidation();

				string expectedMessage = "Following field(s) have decimal places longer than allowed. [Transaction Number: <NEW>]\n" +
@"Field Name: AH_InvoiceAmount. Value: -90.11, Decimal Length: 2, Transaction Currency: AUD, Local Currency:AUD, Allowed Decimal Place: 0
Field Name: AH_OutstandingAmount. Value: -90.11, Decimal Length: 2, Transaction Currency: AUD, Local Currency:AUD, Allowed Decimal Place: 0
Field Name: AH_OSTotal. Value: -90.11, Decimal Length: 2, Transaction Currency: AUD, Local Currency:AUD, Allowed Decimal Place: 0
Field Name: AH_OSExTaxAmount. Value: 90.11, Decimal Length: 2, Transaction Currency: AUD, Local Currency:AUD, Allowed Decimal Place: 0
Field Name: AH_LocalExTaxAmount. Value: 90.11, Decimal Length: 2, Transaction Currency: AUD, Local Currency:AUD, Allowed Decimal Place: 0";
				AssertEquals(expectedMessage, ExceptionReporterTestListener.Instance[0].Message);
				ExceptionReporterTestListener.Instance.Clear();
			}
			finally
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = defaultValue;
			}
		}

		public void TestCreateDeveloperExceptionWithNullCurrency()
		{
			ReceiptPaymentBase.AH_RX_NKTransactionCurrency = "";
			AssertNoExceptionThrown(ReceiptPaymentBase.RunPreSaveValidation);
			AssertEquals("Developer Exception should not be report when currency is null", 0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestBehaviourWhenCashAccountSelected()
		{
			var testCurrency = Factory.NewWithValidTestData<RefCurrency>();

			var testBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			testBankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
			testBankAccount.AB_RX_NKAccountCurrency = testCurrency.RX_Code;

			Factory.Save();

			var receipt = PrepareTransactionHeaderForTest() as Receipt;
			receipt.AH_AB = testBankAccount.PK;

			AssertEquals("Receipt type's value should be 'Cash' after a Cash Account is selected", ReceiptTypes.Cash, receipt.AH_ReceiptType);
			Assert("Receipt type should not be readonly", !receipt.AH_ReceiptTypeInfo.ReadOnly);

			AssertEquals("The currency should be the same as selected cash account", testBankAccount.AB_RX_NKAccountCurrency, receipt.AH_RX_NKTransactionCurrency);
			AssertEquals("The currency should be the same as selected cash account", testBankAccount.AB_RX_NKAccountCurrency, receipt.ExchangeRate.Currency);
			Assert("Exchange rate currency should be readonly", receipt.ExchangeRate.CurrencyInfo.ReadOnly);
			Assert("Receipt amount currency should be readonly", receipt.AH_RX_NKTransactionCurrencyInfo.ReadOnly);
		}

		#endregion

		#region TestDefaultBankDetailCalculation

		public void TestDefaultBankDetailCalculation()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.CompanyData.OB_ARPreviousChequeDrawer = "AAA";
			testOrg.CompanyData.OB_ARPreviousChequeDrawerBank = "BBB";
			testOrg.CompanyData.OB_ARPreviousChequeDrawerBankBranch = "CCC";

			Factory.Save();

			ReceiptPaymentBase.AH_DrawerBank = ZString.Empty;
			ReceiptPaymentBase.AH_DrawerBranch = ZString.Empty;
			ReceiptPaymentBase.AH_ChequeDrawer = ZString.Empty;

			ReceiptPaymentBase.AH_OH = testOrg.PK;

			AssertEquals("Cheque Drawer should default to value on CompanyData", "AAA", ReceiptPaymentBase.AH_ChequeDrawer);
			AssertEquals("Bank should default to value on CompanyData", "BBB", ReceiptPaymentBase.AH_DrawerBank);
			AssertEquals("Branch should default to value on CompanyData", "CCC", ReceiptPaymentBase.AH_DrawerBranch);

			ReceiptPaymentBase.AH_OH = ZGuid.Invalid;
		}

		#endregion

		#region TestCompayReceiptBatching

		public void TestCompayReceiptBatching()
		{
			ZDateTime compayBatchDate = ZDateTime.Today;

			Receipt receipt1 = CreateReceipt("Receipt 1", ReceiptTypes.eNettDirectCredit, 500m, compayBatchDate);
			Factory.Save();
			AssertNotEquals("Receipt 1 should have a Batch", ZString.Empty, receipt1.AH_ReceiptBatchNo);

			Receipt receipt2 = CreateReceipt("Receipt 2", ReceiptTypes.eNettDirectCredit, 1000m, compayBatchDate);
			Factory.Save();
			AssertNotEquals("Receipt 2 should have a Batch", ZString.Empty, receipt2.AH_ReceiptBatchNo);
			AssertEquals("Receipt 1 & 2 should be in the same batch", receipt1.AH_ReceiptBatchNo, receipt2.AH_ReceiptBatchNo);

			DepositBatch firstBatch = LoadDepositBatch(receipt1.AH_ReceiptBatchNo);
			AssertEquals("First Deposit Batch Amount", 1500m, firstBatch.TotalDepositOSAmount);
			firstBatch.AH_DateClearedInCashbook = ZDateTime.Today;
			firstBatch.Factory.Save();

			Receipt receipt3 = CreateReceipt("Receipt 3", ReceiptTypes.eNettDirectCredit, 2000m, compayBatchDate);
			Factory.Save();
			AssertNotEquals("Receipt 3 should have a Batch", ZString.Empty, receipt2.AH_ReceiptBatchNo);
			AssertNotEquals("Receipt 3 should be in a NEW batch", receipt1.AH_ReceiptBatchNo, receipt3.AH_ReceiptBatchNo);

			DepositBatch secondBatch = LoadDepositBatch(receipt3.AH_ReceiptBatchNo);
			AssertEquals("Second Deposit Batch Amount", 2000m, secondBatch.TotalDepositOSAmount);
			AssertEquals("First Deposit Batch Amount shouldn't have changed", 1500m, firstBatch.TotalDepositOSAmount);
		}

		Receipt CreateReceipt(ZString description, ZString receiptType, ZDecimal amount, ZDateTime compayReceiptBatchDate)
		{
			Receipt receipt = (Receipt)GetNewBusinessObject();
			receipt.AH_OH = TestObjectCreator.ABIGAS.PK;
			receipt.AH_ReceiptType = receiptType;
			receipt.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			receipt.AH_ChequeOrReference = description;
			receipt.AH_OSExTaxAmount = amount;
			receipt.CompayReceiptBatchDate = compayReceiptBatchDate;
			return receipt;
		}

		DepositBatch LoadDepositBatch(ZString depositBatchNumber)
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ZQuery findDepositBatchQuery = new ZQuery();
			findDepositBatchQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ReceiptBatch);
			findDepositBatchQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, depositBatchNumber);
			findDepositBatchQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

			AssertEquals("Should be only one batch", 1, Factory.GetDatabaseCount(typeof(DepositBatch), findDepositBatchQuery));
			DepositBatch depositBatch = newFactory.LoadTop1<DepositBatch>(findDepositBatchQuery);
			return depositBatch;
		}

		#endregion

		#region TestUnmatch

		public void TestUnmatch()
		{
			AssertUnmatch(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestUnmatch_EnableNewOSOutstandingAmountFeature()
		{
			AssertUnmatch(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertUnmatch(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			// Note: everything is negative in the DB for both ledgers
			ReceiptPaymentBase.AH_LocalExTaxAmount = 90M;
			ReceiptPaymentBase.AH_LocalOutstandingAmount = 40M;
			ReceiptPaymentBase.AH_FullyPaidDate = ZDateTime.Empty;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				var osOutstandingAmount = 40m * ReceiptPaymentBase.Multiplier_ForTestOnly;
				ReceiptPaymentBase.MakeOSOutstandingAmountApplicable(osOutstandingAmount);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, ReceiptPaymentBase.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 40m, ReceiptPaymentBase.AH_OSOutstandingAmount_WithMultiplier_ForTestOnly);
			}

			AssertEquals("Should not be able to unmatch -51", UnmatchingResult.DataErrorAddingMatchAmountExceedOriginalInvoiceAmount, ((IMatching)ReceiptPaymentBase).CanUnmatch(-51M));
			AssertEquals("Should not be able to unmatch 1", UnmatchingResult.DataErrorInvoiceAmountAndMatchLinkAmountHasOppositeSigns, ((IMatching)ReceiptPaymentBase).CanUnmatch(1M));

			AssertEquals("Should be able to unmatch -50", UnmatchingResult.Success, ((IMatching)ReceiptPaymentBase).CanUnmatch(-50M));
			((IMatching)ReceiptPaymentBase).Unmatch(-50M, -50M);
			AssertEquals("Outstanding amt should be -90", -90M, ReceiptPaymentBase.AH_OutstandingAmount);
			AssertEquals("AH_OSOutstandingAmount", isEnableNewOSOutstandingAmountFeature ? -90m : 0m, ReceiptPaymentBase.AH_OSOutstandingAmount);
			AssertEquals("AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, ReceiptPaymentBase.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("Fully paid date should be null", ZDateTime.Empty, ReceiptPaymentBase.AH_FullyPaidDate);
		}

		#endregion

		#region TestUnmatchWithTax

		public void TestUnmatchWithTax()
		{
			AssertUnmatchWithTax(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestUnmatchWithTax_EnableNewOSOutstandingAmountFeature()
		{
			AssertUnmatchWithTax(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertUnmatchWithTax(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			ReceiptPaymentBase.AH_LocalExTaxAmount = 90M;
			ReceiptPaymentBase.AH_LocalOutstandingAmount = 40M;
			ReceiptPaymentBase.AH_LocalTaxAmount = 10M;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				var osOutstandingAmount = 40m * ReceiptPaymentBase.Multiplier_ForTestOnly;
				ReceiptPaymentBase.MakeOSOutstandingAmountApplicable(osOutstandingAmount);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, ReceiptPaymentBase.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 40m, ReceiptPaymentBase.AH_OSOutstandingAmount_WithMultiplier_ForTestOnly);
			}

			AssertEquals("Cannot unmatch -61 since less than tax amt + invoice amt - outstanding amt", UnmatchingResult.DataErrorAddingMatchAmountExceedOriginalInvoiceAmount,
				((IMatching)ReceiptPaymentBase).CanUnmatch(-61M));

			AssertEquals("Can unmatch -60 since equals tax amt + invoice amt - outstanding amt", UnmatchingResult.Success,
				((IMatching)ReceiptPaymentBase).CanUnmatch(-60M));

			((IMatching)ReceiptPaymentBase).Unmatch(-60M, -60M);

			AssertEquals("Outstanding amt should be -100", -100M, ReceiptPaymentBase.AH_OutstandingAmount);
			AssertEquals("AH_OSOutstandingAmount", isEnableNewOSOutstandingAmountFeature ? -100m : 0m, ReceiptPaymentBase.AH_OSOutstandingAmount);
			AssertEquals("AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, ReceiptPaymentBase.AH_IsOSOutstandingAmountApplicable);
		}

		#endregion

		#region CreatingDepositBatch

		public void TestCreatingDepositBatch()
		{
			foreach (ICodeDescription receiptPaymentMethod in ReceiptPaymentBase.ReceiptMethods)
			{
				AssertCreatingDepositBatch(receiptPaymentMethod.Code, false);
				AssertCreatingDepositBatch(receiptPaymentMethod.Code, true);
			}
		}

		protected virtual bool DoesNeedCreatingDepositBatchForTheType(ZString receiptType)
		{
			return receiptType == ZArchitecture.Core.ReceiptTypes.DirectCredit ||
						receiptType == ZArchitecture.Core.ReceiptTypes.eNettDirectCredit ||
						receiptType == ZArchitecture.Core.ReceiptTypes.AccountMaintenanceFee ||
						receiptType == ZArchitecture.Core.ReceiptTypes.BankDepositFee ||
						receiptType == ZArchitecture.Core.ReceiptTypes.BankDebitTax ||
						receiptType == ZArchitecture.Core.ReceiptTypes.InterestPaid ||
						receiptType == ZArchitecture.Core.ReceiptTypes.InterestReceived ||
						receiptType == ZArchitecture.Core.ReceiptTypes.PeriodicPayment ||
						receiptType == ZArchitecture.Core.ReceiptTypes.StampDuty ||
						receiptType == ZArchitecture.Core.ReceiptTypes.MiscellaneousReceipt ||
						receiptType == ZArchitecture.Core.ReceiptTypes.MiscellaneousFees;
		}

		void AssertCreatingDepositBatch(ZString receiptType, bool skipCreateDepositBatch)
		{
			Receipt testBizO = GetNewBusinessObject() as Receipt;
			testBizO.AH_AB = BankAccount.PK;
			testBizO.AH_ExchangeRate = 2m;
			testBizO.AH_TransactionReference = "abc";
			testBizO.AH_Desc = "test receipt";
			testBizO.AH_InvoiceDate = new ZDateTime(2000, 1, 20);
			testBizO.AH_ChequeOrReference = "00123";
			testBizO.AH_ReceiptType = receiptType;
			testBizO.AH_ChequeDrawer = "bbb";
			testBizO.SkipCreateDepositBatch = skipCreateDepositBatch;

			Factory.Save();

			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, testBizO.AH_ReceiptBatchNo);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, testBizO.AH_GC);
			DepositBatch testDepositBatch = Factory.LoadTop1<DepositBatch>(filter);

			AssertEquals(string.Format("DepositBatch should {0} be created for {1} DirectReceipt", !skipCreateDepositBatch ? "" : "not", receiptType),
					DoesNeedCreatingDepositBatchForTheType(receiptType) && !skipCreateDepositBatch, testDepositBatch != null);
		}

		#endregion

		#region TestRelatedDepositBatch

		public void TestRelatedDepositBatch()
		{
			GlbCompany anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch anotherCompanyBranch = Factory.NewWithValidTestData<GlbBranch>();
			anotherCompanyBranch.GB_GC = anotherCompany.PK;
			Factory.Save();

			DepositBatch testDepositBat = Factory.NewWithValidTestData<DepositBatch>();
			testDepositBat.AH_GB = anotherCompanyBranch.PK;
			testDepositBat.AH_TransactionNum = "00001580";
			testDepositBat.AH_ReceiptBatchNo = "00001580";
			Factory.Save();

			ReceiptPaymentBase.AH_ReceiptBatchNo = "00001580";
			AssertNull("There is no related deposit batch from the same company", ((Receipt)ReceiptPaymentBase).RelatedDepositBatch);
		}

		public void TestRelatedDepositBatchLooksUpCorrectField()
		{
			DepositBatch testDepositBat = Factory.NewWithValidTestData<DepositBatch>();
			testDepositBat.AH_TransactionNum = "00001580";
			testDepositBat.AH_ReceiptBatchNo = "00001580";
			Factory.Save();

			ReceiptPaymentBase.AH_ReceiptBatchNo = "00001580";
			AssertNotNull("Should pick up related batch", ((Receipt)ReceiptPaymentBase).RelatedDepositBatch);

			testDepositBat.AH_ReceiptBatchNo = "";
			Factory.Save();

			Header = GetNewBusinessObject() as ReceiptPaymentBase;
			ReceiptPaymentBase.AH_ReceiptBatchNo = "";
			AssertNull("Should not find batch because batch number is empty", ((Receipt)ReceiptPaymentBase).RelatedDepositBatch);
		}

		#endregion

		#region TestMatchingBaseObject

		public override void TestMatchingBaseObject()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			ReceiptPaymentBase.AH_OH = org.PK;
			ReceiptPaymentBase.AH_LocalExTaxAmount = 94M;
			ReceiptPaymentBase.AH_OSExTaxAmount = 64M;
			ZDateTime expectedDate = ZDateTime.Now;
			ReceiptPaymentBase.CurrentMatchingDate = expectedDate;
			Factory.Save();

			fMatchingBaseObject = ReceiptPaymentBase.MatchingBaseObject;
			Assert("MatchingBaseObject should be for non-Payments", fMatchingBaseObject.IsNotMatchingPayment);
			Assert("MatchingBaseObject should be for payments or receipts", fMatchingBaseObject.IsMatchingPaymentOrReceipt);
			AssertEquals("Primary Org should be Org", org.PK, fMatchingBaseObject.PrimaryOrganization);
			AssertEquals("Matching object's MatchDate should have CurrentMatchingDate", expectedDate.Date,
				fMatchingBaseObject.MatchDate);
		}

		#endregion

		#region TestSaveChequeDetailsAsDefault
		public void TestSaveChequeDetailsAsDefault()
		{
			Receipt fReceipt = (Receipt)ReceiptPaymentBase;
			fReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			fReceipt.AH_ChequeDrawer = "Test1";
			fReceipt.AH_DrawerBank = "Test2";
			fReceipt.AH_DrawerBranch = "Test3";

			fReceipt.SaveChequeDetailsAsDefault_ForTestOnly();
			AssertEquals("Default AH_ChequeDrawer should be set", fReceipt.AH_ChequeDrawer, fReceipt.Default_AH_ChequeDrawer_ForTestOnly);
			AssertEquals("Default AH_DrawerBank should be set", fReceipt.AH_DrawerBank, fReceipt.Default_AH_DrawerBank_ForTestOnly);
			AssertEquals("Default AH_DrawerBranch should be set", fReceipt.AH_DrawerBranch, fReceipt.Default_AH_DrawerBranch_ForTestOnly);
		}
		#endregion

		#region TestSetReceiptType

		public override void TestSetReceiptType()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();

			AssertEquals("Precondition: Default receipt type is cheque", ZArchitecture.Core.ReceiptTypes.Cheque, ReceiptPaymentBase.AH_ReceiptType);

			ReceiptPaymentBase.AH_AB = testBank.PK;
			ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			AssertEquals("Bank account should be left the same", testBank.PK, ReceiptPaymentBase.AH_AB);
			AssertEquals("Cheque/Reference number should change to CASH", ZArchitecture.Core.ReceiptTypes.Cash, ReceiptPaymentBase.AH_ChequeOrReference);

			// Cheque
			ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertEquals("Bank account should be left the same", testBank.PK, ReceiptPaymentBase.AH_AB);
			Assert("Cheque/Reference number should become empty", ReceiptPaymentBase.AH_ChequeOrReference.IsEmpty);
			Assert("Drawer shouldn't be readonly", !ReceiptPaymentBase.AH_ChequeDrawerInfo.ReadOnly);
			Assert("Drawer Branch shouldn't be readonly", !ReceiptPaymentBase.AH_DrawerBranchInfo.ReadOnly);
			Assert("Drawer Bank shouldn't be readonly", !ReceiptPaymentBase.AH_DrawerBankInfo.ReadOnly);

			// Credit Card
			ReceiptPaymentBase.AH_ChequeOrReference = "798";
			ReceiptPaymentBase.AH_ChequeDrawer = "Test1";
			ReceiptPaymentBase.AH_DrawerBank = "Test2";
			ReceiptPaymentBase.AH_DrawerBranch = "Test3";

			ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.CreditCard;
			Assert("Cheque/Reference number should become empty", ReceiptPaymentBase.AH_ChequeOrReference.IsEmpty);
			Assert("Drawer should be readonly", ReceiptPaymentBase.AH_ChequeDrawerInfo.ReadOnly);
			Assert("Drawer Branch should be readonly", ReceiptPaymentBase.AH_DrawerBranchInfo.ReadOnly);
			Assert("Drawer Bank should be readonly", ReceiptPaymentBase.AH_DrawerBankInfo.ReadOnly);
			Assert("Cheque Drawer should be empty", ReceiptPaymentBase.AH_ChequeDrawer.IsEmpty);
			Assert("Drawer Bank should be empty", ReceiptPaymentBase.AH_DrawerBank.IsEmpty);
			Assert("Drawer Branch should be empty", ReceiptPaymentBase.AH_DrawerBranch.IsEmpty);

			// Direct Credit
			ReceiptPaymentBase.AH_ChequeOrReference = "324";
			ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			ReceiptPaymentBase.AH_ChequeDrawer = "Test1";
			ReceiptPaymentBase.AH_DrawerBank = "Test2";
			ReceiptPaymentBase.AH_DrawerBranch = "Test3";

			ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			Assert("Cheque/Reference number should become empty", ReceiptPaymentBase.AH_ChequeOrReference.IsEmpty);
			Assert("Drawer should be readonly", ReceiptPaymentBase.AH_ChequeDrawerInfo.ReadOnly);
			Assert("Drawer Branch should be readonly", ReceiptPaymentBase.AH_DrawerBranchInfo.ReadOnly);
			Assert("Drawer Bank should be readonly", ReceiptPaymentBase.AH_DrawerBankInfo.ReadOnly);
			Assert("Cheque Drawer should be empty", ReceiptPaymentBase.AH_ChequeDrawer.IsEmpty);
			Assert("Drawer Bank should be empty", ReceiptPaymentBase.AH_DrawerBank.IsEmpty);
			Assert("Drawer Branch should be empty", ReceiptPaymentBase.AH_DrawerBranch.IsEmpty);

			ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			ReceiptPaymentBase.AH_ChequeDrawer = "Test1";
			ReceiptPaymentBase.AH_DrawerBank = "Test2";
			ReceiptPaymentBase.AH_DrawerBranch = "Test3";
			ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			Assert("Drawer should be readonly", ReceiptPaymentBase.AH_ChequeDrawerInfo.ReadOnly);
			Assert("Drawer Branch should be readonly", ReceiptPaymentBase.AH_DrawerBranchInfo.ReadOnly);
			Assert("Drawer Bank should be readonly", ReceiptPaymentBase.AH_DrawerBankInfo.ReadOnly);
			Assert("Cheque Drawer should be empty", ReceiptPaymentBase.AH_ChequeDrawer.IsEmpty);
			Assert("Drawer Bank should be empty", ReceiptPaymentBase.AH_DrawerBank.IsEmpty);
			Assert("Drawer Branch should be empty", ReceiptPaymentBase.AH_DrawerBranch.IsEmpty);

			ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			Assert("Drawer should not be readonly", !ReceiptPaymentBase.AH_ChequeDrawerInfo.ReadOnly);
			Assert("Drawer Branch not should be readonly", !ReceiptPaymentBase.AH_DrawerBranchInfo.ReadOnly);
			Assert("Drawer Bank not should be readonly", !ReceiptPaymentBase.AH_DrawerBankInfo.ReadOnly);
			AssertEquals("Cheque Drawer should not be empty", ReceiptPaymentBase.AH_ChequeDrawer, "Test1");
			AssertEquals("Drawer Bank should not be empty", ReceiptPaymentBase.AH_DrawerBank, "Test2");
			AssertEquals("Drawer Branch should not be empty", ReceiptPaymentBase.AH_DrawerBranch, "Test3");
		}

		#endregion

		#region TestCreateReceiptBatch

		public void TestCreateReceiptBatch()
		{
			ZString nextNumber = Env.NumberFountains.BatchReceiptNo.GetTodaysPeriodFountain().PeekPreliminaryFormatted(Factory);

			ReceiptPaymentBase.AH_AB = BankAccount.PK;
			ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			ReceiptPaymentBase.AH_OSExTaxAmount = 90M;
			ReceiptPaymentBase.AH_ExchangeRate = 1M;
			ReceiptPaymentBase.AH_TransactionReference = "abc";
			ReceiptPaymentBase.AH_Desc = "test receipt";
			ReceiptPaymentBase.AH_ChequeOrReference = "00123";
			ReceiptPaymentBase.AH_ChequeDrawer = "bbb";

			Factory.Save();

			TransactionHeaderCollection headers = new TransactionHeaderCollection(Factory);
			headers.Load();

			AssertEquals("There should be 2 transactions in DB", 2, headers.Count);

			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			DepositBatch receiptBatch = Factory.LoadTop1(typeof(DepositBatch), filter) as DepositBatch;

			AssertNotNull("Deposit Batch should be created", receiptBatch);

			ReceiptPaymentBase.Reload();

			// existing row should have AH_ReceiptBatchNo set
			AssertEquals("Existing ReceiptPaymentBase should have AH_ReceiptBatchNo set",
				nextNumber, ReceiptPaymentBase.AH_ReceiptBatchNo);
			AssertEquals("New Row should have its transactionNumber set",
				nextNumber, receiptBatch.AH_TransactionNum);

			AssertEquals("Date should be current date", ZDateTime.Today, receiptBatch.AH_InvoiceDate.Date);

			ZDecimal signedAmount = ReceiptPaymentBase is Payment ? -90M : 90M;
			AssertEquals("OS Total", signedAmount, receiptBatch.AH_OSTotal);
			AssertEquals("InvoiceAmount", signedAmount, receiptBatch.AH_InvoiceAmount);
			AssertEquals("Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, receiptBatch.AH_RX_NKTransactionCurrency);
			AssertEquals("Receipt Batch", nextNumber, receiptBatch.AH_ReceiptBatchNo);
			AssertEquals("Branch", GlbBranch.CurrentBranch.PK, receiptBatch.AH_GB);
			AssertEquals("Department", GlbDepartment.CurrentDepartment.PK, receiptBatch.AH_GE);
			AssertEquals("PostDate", ReceiptPaymentBase.AH_PostDate.Date, receiptBatch.AH_PostDate.Date);
			AssertEquals("InvoiceDate", ReceiptPaymentBase.AH_InvoiceDate.Date, receiptBatch.AH_InvoiceDate.Date);
			//AssertEquals("DueDate", ReceiptPaymentBase.AH_DueDate.Date, ReceiptBatch.AH_DueDate.Date);
			AssertEquals("BankAccount", ReceiptPaymentBase.AH_AB, receiptBatch.AH_AB);
		}

		#endregion

		#region TestPreviousBankDetailsUpdated

		public void TestPreviousBankDetailsUpdated()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OrgHeader testOrg = newFactory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_IsDebtor = true;
			newFactory.Save();

			ReceiptPaymentBase.AH_OH = testOrg.PK;
			ReceiptPaymentBase.AH_ChequeDrawer = "Drawer@@";
			ReceiptPaymentBase.AH_DrawerBranch = "Branch@@";
			ReceiptPaymentBase.AH_DrawerBank = "Bank@@";

			ReceiptPaymentBase.Factory.Save();
			testOrg.Reload();

			AssertEquals("Cheque Drawer should be saved to company data", "Drawer@@", testOrg.CompanyData.OB_ARPreviousChequeDrawer);
			AssertEquals("Cheque Branch should be saved to company data", "Branch@@", testOrg.CompanyData.OB_ARPreviousChequeDrawerBankBranch);
			AssertEquals("Cheque Bank should be saved to company data", "Bank@@", testOrg.CompanyData.OB_ARPreviousChequeDrawerBank);
		}

		#endregion

		#region TestReverseReceiptWithoutDepositBatch

		public void TestReverseReceiptWithoutDepositBatch()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();

			ReceiptPaymentBase.AH_OH = testOrg.PK;
			ReceiptPaymentBase.AH_LocalExTaxAmount = 30M;
			ReceiptPaymentBase.AH_OSExTaxAmount = 30M;
			ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			Factory.Save();

			ReversingFactory reversingFactory = new ReversingFactory();
			ReversingBase reversing = reversingFactory.NewReversing(ReceiptPaymentBase);
			reversing.Reverse();

			// Assert that no deposit batch row is created
			ZQuery depositBatchFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			TransactionHeaderCollection headers = new TransactionHeaderCollection(Factory, depositBatchFilter);
			headers.Load();
			AssertEquals("No deposit batch rows should be in the DB", 0, headers.Count);
		}

		#endregion

		#region TestReverseReceiptWithDepositBatch

		public void TestReverseReceiptWithDepositBatch()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OrgHeader testOrg = newFactory.NewWithValidTestData<OrgHeader>();
			newFactory.Save();

			DepositBatch testDepositBatch = Factory.NewWithValidTestData<DepositBatch>();
			testDepositBatch.AH_TransactionNum = "00001580";
			testDepositBatch.AH_ReceiptBatchNo = "00001580";

			ReceiptPaymentBase.AH_OH = testOrg.PK;
			ReceiptPaymentBase.AH_OSExTaxAmount = 52M;  // -52 in DB for both ledgers since Receipt inverts sign for both ledgers
			ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			ReceiptPaymentBase.AH_ReceiptBatchNo = "00001580";
			Factory.Save();

			// Assert that DepositBatch is saved
			ZQuery depositBatchFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			TransactionHeaderCollection headers = new TransactionHeaderCollection(Factory, depositBatchFilter);
			headers.Load();
			AssertEquals("There should be 1 deposit batch in the DB", 1, headers.Count);
			DepositBatch depositBatchBizO = (DepositBatch)headers[0];

			ReversingFactory reversingFactory = new ReversingFactory();
			ReversingBase reversing = reversingFactory.NewReversing(ReceiptPaymentBase);
			reversing.Reverse();
			ReceiptPaymentBase.Factory.Save();

			// Check the reversing receipt
			ZQuery revReceiptFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Receipt);
			revReceiptFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, Header.PK);
			TransactionHeader revReceipt = Factory.LoadTop1<TransactionHeader>(revReceiptFilter);

			Assert("Transaction should be a receipt", revReceipt is Receipt);
			AssertEquals("ReversingReceipt should have Local Amount 52", 52M, revReceipt.AH_InvoiceAmount);
			AssertEquals("ReversingReceipt should have Overseas Amount 52", 52M, revReceipt.AH_OSTotal);

			// Assert that depositbatch is reversed

			ZQuery secondDepositBatchFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			secondDepositBatchFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, depositBatchBizO.PK);
			TransactionHeaderCollection secondHeaders = new TransactionHeaderCollection(Factory, secondDepositBatchFilter);

			Factory.Save();
			secondHeaders.Load();
			AssertEquals("There should be 1 reversing Deposit Batch", 1, secondHeaders.Count);
			TransactionHeader reversingDepositBatch = secondHeaders[0];

			AssertEquals("The Local Amount on reversing Deposit Batch should be -52", -52M, reversingDepositBatch.AH_InvoiceAmount);
			AssertEquals("The Overseas amount of reversing Deposit Batch should be -52", -52M, reversingDepositBatch.AH_OSTotal);
			AssertEquals("The ReceiptBatchNo and TransactionNo of reversing DepositBatch should be the same", reversingDepositBatch.AH_ReceiptBatchNo, reversingDepositBatch.AH_TransactionNum);

			AssertEquals("ReceiptBatchNo of RevReceipt should be same as TransactionNo on ReversingDepositBatch",
				reversingDepositBatch.AH_ReceiptBatchNo, revReceipt.AH_ReceiptBatchNo);
		}

		#endregion

		#region TestOrgMiscServ

		public void TestOrgMiscServ()
		{
			OrgHeader org1 = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()) as OrgHeader;

			Assert(org1.MiscServ.OM_ARPreviousChequeDrawer != "ChequeDrawer");
			Assert(org1.MiscServ.OM_ARPreviousChequeDrawerBank != "DrawerBank");
			Assert(org1.MiscServ.OM_ARPreviousChequeDrawerBankBranch != "DrawerBranch");

			org1.OH_IsDebtor = ZBool.True;

			ReceiptPaymentBase.AH_OH = org1.PK;
			ReceiptPaymentBase.AH_ReceiptType = "CHQ";
			ReceiptPaymentBase.AH_ChequeDrawer = "ChequeDrawer";
			ReceiptPaymentBase.AH_DrawerBank = "DrawerBank";
			ReceiptPaymentBase.AH_DrawerBranch = "DrawerBranch";

			Factory.Save();

			Assert(org1.MiscServ.OM_ARPreviousChequeDrawer == "ChequeDrawer");
			Assert(org1.MiscServ.OM_ARPreviousChequeDrawerBank == "DrawerBank");
			Assert(org1.MiscServ.OM_ARPreviousChequeDrawerBankBranch == "DrawerBranch");
		}

		#endregion

		#region TestOrgMiscServNotUpdatedIfNotDebtor

		public void TestOrgMiscServNotUpdatedIfNotDebtor()
		{
			OrgHeader org1 = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()) as OrgHeader;

			Assert(org1.MiscServ.OM_ARPreviousChequeDrawer != "ChequeDrawer");
			Assert(org1.MiscServ.OM_ARPreviousChequeDrawerBank != "DrawerBank");
			Assert(org1.MiscServ.OM_ARPreviousChequeDrawerBankBranch != "DrawerBranch");

			org1.OH_IsDebtor = ZBool.False;

			ReceiptPaymentBase.AH_OH = org1.PK;
			ReceiptPaymentBase.AH_ChequeDrawer = "ChequeDrawer";
			ReceiptPaymentBase.AH_DrawerBank = "DrawerBank";
			ReceiptPaymentBase.AH_DrawerBranch = "DrawerBranch";
			ReceiptPaymentBase.AH_ReceiptType = "CHQ";

			Factory.Save();

			Assert(org1.MiscServ.OM_ARPreviousChequeDrawer != "ChequeDrawer");
			Assert(org1.MiscServ.OM_ARPreviousChequeDrawerBank != "DrawerBank");
			Assert(org1.MiscServ.OM_ARPreviousChequeDrawerBankBranch != "DrawerBranch");
		}

		#endregion

		#region TestOrgMiscServNotUpdatedIfNotCheque

		public void TestOrgMiscServNotUpdatedIfNotCheque()
		{
			OrgHeader org1 = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()) as OrgHeader;

			Assert(org1.MiscServ.OM_ARPreviousChequeDrawer != "ChequeDrawer");
			Assert(org1.MiscServ.OM_ARPreviousChequeDrawerBank != "DrawerBank");
			Assert(org1.MiscServ.OM_ARPreviousChequeDrawerBankBranch != "DrawerBranch");

			org1.OH_IsDebtor = ZBool.True;

			ReceiptPaymentBase.AH_OH = org1.PK;
			ReceiptPaymentBase.AH_ChequeDrawer = "ChequeDrawer";
			ReceiptPaymentBase.AH_DrawerBank = "DrawerBank";
			ReceiptPaymentBase.AH_DrawerBranch = "DrawerBranch";
			ReceiptPaymentBase.AH_ReceiptType = "CSH";

			Factory.Save();

			Assert(org1.MiscServ.OM_ARPreviousChequeDrawer != "ChequeDrawer");
			Assert(org1.MiscServ.OM_ARPreviousChequeDrawerBank != "DrawerBank");
			Assert(org1.MiscServ.OM_ARPreviousChequeDrawerBankBranch != "DrawerBranch");
		}

		#endregion

		#region TestPrepareReceiptPaymentForMatching

		public override void TestPrepareReceiptPaymentForMatching()
		{
			((Receipt)ReceiptPaymentBase).IsMatching_ForTestOnly = true;
			Assert("Precondition: OSPartialPaymentAmount should be readonly", ((IMatching)ReceiptPaymentBase).OSPartialPaymentAmountInfo.ReadOnly);
			((Receipt)ReceiptPaymentBase).PrepareReceiptPaymentForMatching_ForTestOnly();
			Assert("OSPartialPaymentAmount should be writable", !((IMatching)ReceiptPaymentBase).OSPartialPaymentAmountInfo.ReadOnly);
		}

		#endregion

		#region TestDepositBatchNumber

		public override void TestDepositBatchNumber()
		{
			ReceiptPaymentBase.AH_ReceiptBatchNo = "00009999";
			AssertEquals("DepositBatchNumber should be 00009999", "00009999", ReceiptPaymentBase.DepositBatchNumber);
		}

		#endregion

		public void TestGetNewMatchingValidation()
		{
			MatchingValidation matchValidation = ((Receipt)ReceiptPaymentBase).GetNewMatchingValidation_ForTestOnly() as MatchingValidation;
			AssertNotNull(matchValidation);
			((Receipt)ReceiptPaymentBase).UseReceiptValidation = true;
			ReceiptValidation receiptValidation = ((Receipt)ReceiptPaymentBase).GetNewMatchingValidation_ForTestOnly() as ReceiptValidation;
			AssertNotNull(receiptValidation);
		}

		public void TestAH_OH_ReadOnly()
		{
			Receipt receipt = (Receipt)ReceiptPaymentBase;
			Assert(!receipt.AH_OHInfo.ReadOnly);
			receipt.IsPostWithMatching = true;
			Assert(receipt.AH_OHInfo.ReadOnly);
		}

		protected override Type TypeOfValidation
		{
			get { return typeof(ReceiptValidation); }
		}
	}
}
