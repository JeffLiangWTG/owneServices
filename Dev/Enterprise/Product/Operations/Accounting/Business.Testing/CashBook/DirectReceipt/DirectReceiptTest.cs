using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DirectReceipt.Testing
{
	[TestedType(typeof(DirectReceipt))]
	class DirectReceiptTest : DirectTransactionHeaderBaseTest
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<DirectReceipt>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		protected override Type GetExpectedBusinessObjectLineType()
		{
			return typeof(DirectReceiptLine);
		}

		protected override Type TypeOfValidation
		{
			get { return typeof(DirectReceiptValidation); }
		}

		protected override IEnumerable<(string PropertyName, Action<DirectTransactionHeaderBase> Init)> GetMostColumnsReadOnlyAffectedProp()
		{
			return new (string, Action<DirectTransactionHeaderBase>)[] {
				(DirectTransactionHeaderBase.Schema.AH_TransactionType, _ => { }),
				(DirectTransactionHeaderBase.Schema.AH_RX_NKTransactionCurrency,SetMostColumnsReadOnlyPreConditionEvn),
				(DirectTransactionHeaderBase.Schema.AH_ExchangeRate,SetMostColumnsReadOnlyPreConditionEvn),
				(DirectTransactionHeaderBase.Schema.AH_ChequeDrawer,SetMostColumnsReadOnlyPreConditionEvn),
				(DirectTransactionHeaderBase.Schema.AH_ChequeOrReference,SetMostColumnsReadOnlyPreConditionEvn),
				(DirectTransactionHeaderBase.Schema.AH_PostDate,SetMostColumnsReadOnlyPreConditionEvn),
				(DirectTransactionHeaderBase.Schema.AH_InvoiceDate,SetMostColumnsReadOnlyPreConditionEvn),
				(DirectTransactionHeaderBase.Schema.AH_DrawerBranch,SetMostColumnsReadOnlyPreConditionEvn),
				(DirectTransactionHeaderBase.Schema.AH_ReceiptType,SetMostColumnsReadOnlyPreConditionEvn),
				(DirectTransactionHeaderBase.Schema.AH_DrawerBank,SetMostColumnsReadOnlyPreConditionEvn)
			};

			void SetMostColumnsReadOnlyPreConditionEvn(DirectTransactionHeaderBase testHeader)
			{
				AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty
					, AccountingConstants.InvAndPstDateDefaultingRuleTypes.Default.Code);
				testHeader.AH_RX_NKTransactionCurrency = testHeader.AH_RX_NKTransactionCurrency == "USD"
					? "AUD"
					: "USD";
				testHeader.ExchangeRate.Currency_ReadOnly = false;

				testHeader.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			}
		}

		public override void TestLookups()
		{
			base.TestLookups();
			AssertEquals("Lookups", typeof(DirectReceiptLookups), TestBizO.Lookups.GetType());
		}

		public void TestChequeBookPKInfoReadOnly()
		{
			AssertEquals(true, TestBizO.ChequeBookPKInfo.ReadOnly);
		}

		public void TestRelatedDepositBatch()
		{
			AssertNull(TestDirectReceipt.RelatedDepositBatch);
			DepositBatch.DepositBatch testBatchInCurrentCompany = Factory.New<DepositBatch.DepositBatch>();
			testBatchInCurrentCompany.AH_TransactionNum = "00001000";
			TestDirectReceipt.AH_ReceiptBatchNo = "00001000";

			DepositBatch.DepositBatch testBatchInOtherBranch = Factory.New<DepositBatch.DepositBatch>();
			TestDirectReceipt.AH_ReceiptBatchNo = "00001000";
			testBatchInOtherBranch.AH_GB = ZGuid.NewZGuid();

			APPayment payment = Factory.New<APPayment>();
			payment.AH_TransactionNum = "00001000";

			AssertNotNull(TestDirectReceipt.RelatedDepositBatch);
			AssertEquals(testBatchInCurrentCompany.PK, TestDirectReceipt.RelatedDepositBatch.PK);
		}

		public void TestReceiptTypeAndChequeDetailsReadOnly()
		{
			foreach (ICodeDescription paymentMethod in TestBizO.PaymentMethods)
			{
				TestBizO.AH_ReceiptType = paymentMethod.Code;

				if (TestBizO.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque)
				{
					AssertEquals("AH_ChequeDrawer should be enabled", false, TestBizO.AH_ChequeDrawerInfo.ReadOnly);
					AssertEquals("AH_DrawerBank should be enabled", false, TestBizO.AH_DrawerBankInfo.ReadOnly);
					AssertEquals("AH_DrawerBranch should be enabled", false, TestBizO.AH_DrawerBranchInfo.ReadOnly);
				}
				else
				{
					AssertEquals("AH_ChequeDrawer should be readonly", true, TestBizO.AH_ChequeDrawerInfo.ReadOnly);
					AssertEquals("AH_DrawerBank should be readonly", true, TestBizO.AH_DrawerBankInfo.ReadOnly);
					AssertEquals("AH_DrawerBranch should be readonly", true, TestBizO.AH_DrawerBranchInfo.ReadOnly);
				}
			}
		}

		public void TestDefaultAH_Desc()
		{
			AssertEquals("Default AH_Desc should be", "CASH BOOK DIRECT RECEIPT", TestBizO.AH_Desc);
		}

		public void TestDebitCredit()
		{
			DirectReceipt testDirectReceipt = Factory.New(typeof(DirectReceipt)) as DirectReceipt;

			testDirectReceipt.AH_LocalExTaxAmount = 120.0m;
			testDirectReceipt.AH_OSTotalAmount = 220.0m;

			AssertEquals(220.0m, testDirectReceipt.Debit);
			AssertEquals(0.0m, testDirectReceipt.Credit);

			testDirectReceipt.AH_OSTotalAmount = -220.0m;

			AssertEquals(220.0m, testDirectReceipt.Credit);
			AssertEquals(0.0m, testDirectReceipt.Debit);
		}

		public override void TestLocalCredit()
		{
			TestBizO.AH_AB = TestBankOtherCurrency.PK;
			AssertEquals(true, TestBizO.BankAccount.AB_RX_NKAccountCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			TestBizO.AH_RX_NKTransactionCurrency = TestBizO.BankAccount.AB_RX_NKAccountCurrency;
			TestBizO.AH_ExchangeRate = 2m;
			TestBizO.Lines.AddNew();
			DirectReceiptLine line = (DirectReceiptLine)TestBizO.Lines[0];
			line.AL_OSExTaxAmount = -40;

			AssertEquals(-40m, TestBizO.AH_OSExTaxAmount);
			AssertEquals(20m, TestBizO.LocalCredit);
		}

		public override void TestLocalDebit()
		{
			TestBizO.AH_AB = TestBankOtherCurrency.PK;
			AssertEquals(true, TestBizO.BankAccount.AB_RX_NKAccountCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			TestBizO.AH_RX_NKTransactionCurrency = TestBizO.BankAccount.AB_RX_NKAccountCurrency;
			TestBizO.AH_ExchangeRate = 3m;
			TestBizO.Lines.AddNew();
			DirectReceiptLine line = (DirectReceiptLine)TestBizO.Lines[0];
			line.AL_OSExTaxAmount = 30;

			AssertEquals(30m, TestBizO.AH_OSExTaxAmount);
			AssertEquals(10m, TestBizO.LocalDebit);
		}

		public override void TestDepositBatchNumber()
		{
			TestBizO.AH_ReceiptBatchNo = "00003233";
			AssertEquals("DepositBatch number should be 00003233", "00003233", TestBizO.DepositBatchNumber);
		}

		[TestDate(2000, 1, 21)]
		public void TestSavedData()
		{
			DirectReceipt testReceipt = Factory.New<DirectReceipt>();

			AssertEquals("Should be the same currency", BankAccount.AB_RX_NKAccountCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			testReceipt.AH_AB = BankAccount.PK;
			testReceipt.AH_TransactionReference = "abc";
			testReceipt.AH_Desc = "test receipt";
			testReceipt.AH_InvoiceDate = new ZDateTime(2000, 1, 20);
			testReceipt.AH_ChequeOrReference = "00123";
			testReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testReceipt.AH_ChequeDrawer = "bbb";
			testReceipt.AH_DrawerBank = "ccc";
			testReceipt.AH_DrawerBranch = "ddd";
			testReceipt.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			testReceipt.AH_ExchangeRate = 1m;

			DirectReceiptLine testLine = (DirectReceiptLine)testReceipt.Lines.AddNew();
			testLine.AL_AG = GLAccount.PK;
			testLine.AL_GB = GlbBranch.CurrentBranch.PK;
			testLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			testLine.AL_OSExTaxAmount = 10;
			testLine.AL_Desc = "test line";
			Factory.Save();

			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			DirectReceipt savedReceipt = testFactory.Load<DirectReceipt>(testReceipt.PK);

			// AccTransactionHeader Columns
			AssertEquals("CB", savedReceipt.AH_Ledger);
			AssertEquals("DRC", savedReceipt.AH_TransactionType);
			AssertEquals(1, (int)savedReceipt.AH_TransactionCount);
			AssertEquals("abc", savedReceipt.AH_TransactionReference);
			AssertEquals("test receipt", savedReceipt.AH_Desc);
			AssertEquals(new ZDateTime(2000, 1, 20), savedReceipt.AH_InvoiceDate);
			AssertEquals("", savedReceipt.AH_TransactionCategory);
			AssertEquals(savedReceipt.AH_InvoiceDate, savedReceipt.AH_DueDate);
			AssertEquals(10m, savedReceipt.AH_InvoiceAmount);
			AssertEquals(0m, savedReceipt.AH_GSTAmount);
			AssertEquals(0m, savedReceipt.AH_WithholdingTax);
			AssertEquals(10m, savedReceipt.AH_OSTotal);
			AssertEquals(savedReceipt.AH_RX_NKTransactionCurrency, savedReceipt.AH_RX_NKTransactionCurrency);
			AssertEquals(1m, savedReceipt.AH_ExchangeRate);
			AssertEquals(0, savedReceipt.AH_AgePeriod);
			AssertEquals(0, savedReceipt.AH_PostPeriod);
			AssertEquals(new ZDateTime(2000, 1, 21), savedReceipt.AH_PostDate);
			AssertEquals(false, savedReceipt.AH_IsDisbursementCalc);
			AssertEquals("00123", savedReceipt.AH_ChequeOrReference);
			AssertEquals(ZArchitecture.Core.ReceiptTypes.Cheque, savedReceipt.AH_ReceiptType);
			AssertEquals(false, savedReceipt.AH_CashBasisGSTIndicator);
			AssertEquals(false, savedReceipt.AH_CashBasisGSTRealisedToGL);

			AssertEquals("bbb", savedReceipt.AH_ChequeDrawer);
			AssertEquals("ccc", savedReceipt.AH_DrawerBank);
			AssertEquals("ddd", savedReceipt.AH_DrawerBranch);
			AssertEquals(false, savedReceipt.AH_InvoiceApproved);
			AssertEquals("", savedReceipt.AH_ConsolidatedInvoiceRef);
			AssertEquals(true, savedReceipt.AH_FullyPaidDate.IsEmpty);
			AssertEquals(false, savedReceipt.AH_InvoicePrinted);
			AssertEquals(false, savedReceipt.AH_IsCancelled);
			AssertEquals(ZDateTime.Empty, savedReceipt.AH_DateClearedInCashbook);

			AssertEquals(false, savedReceipt.AH_NotAllocated);
			AssertEquals(0m, savedReceipt.AH_OutstandingAmount);
			AssertEquals(false, savedReceipt.AH_PostedToEFT);
			AssertEquals("N", savedReceipt.AH_PostToGL);
			AssertEquals("", savedReceipt.AH_ReceiptBatchNo);
			AssertEquals(false, savedReceipt.AH_TransactionCreatedByMatching);
			AssertEquals("", savedReceipt.AH_InvoiceTerm);
			AssertEquals(0, (int)savedReceipt.AH_InvoiceTermDays);

			AssertEquals(false, savedReceipt.AH_POST1);
			AssertEquals(false, savedReceipt.AH_POST2);
			AssertEquals(false, savedReceipt.AH_POST3);
			AssertEquals(false, savedReceipt.AH_POST4);

			AssertEquals(BankAccount.PK, savedReceipt.AH_AB);
			AssertEquals(true, savedReceipt.AH_OH.IsEmpty);
			AssertEquals(true, savedReceipt.AH_JH.IsEmpty);
			AssertEquals(GlbBranch.CurrentBranch.PK, savedReceipt.AH_GB);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, savedReceipt.AH_GE);
			AssertEquals(true, savedReceipt.AH_AG.IsEmpty);
			AssertEquals(true, savedReceipt.AH_TransactionBelongsToGroup.IsEmpty);
			AssertEquals(true, savedReceipt.AH_AH_InvoiceStatement.IsEmpty);

			// AccTransactionLines Columns
			AssertEquals(1, savedReceipt.Lines.Count);

			DirectReceiptLine savedLine = (DirectReceiptLine)savedReceipt.Lines[0];

			AssertEquals("DRC", savedLine.AL_LineType);
			AssertEquals(1, (int)savedLine.AL_Sequence);
			AssertEquals("test line", savedLine.AL_Desc);
			AssertEquals(10m, savedLine.AL_LineAmount);
			AssertEquals(true, savedLine.AL_AT.IsEmpty);
			AssertEquals(0m, savedLine.AL_GSTVAT);
			AssertEquals(true, savedLine.AL_AW.IsEmpty);
			AssertEquals(0m, savedLine.AL_WithholdingTax);
			AssertEquals(0, savedLine.AL_UnitQty);
			AssertEquals(0m, savedLine.AL_UnitPrice);
			AssertEquals(0m, savedLine.AL_OSUnitPrice);
			AssertEquals(10m, savedLine.AL_OSAmount);
			AssertEquals(1m, savedLine.AL_ExchangeRate);
			AssertEquals(0, savedLine.AL_PostPeriod);
			AssertEquals(new ZDateTime(2000, 1, 21), savedLine.AL_PostDate);
			AssertEquals("N", savedLine.AL_PostToGL);
			AssertEquals(0, savedLine.AL_ReversePeriod);
			AssertEquals(true, savedLine.AL_ReverseDate.IsEmpty);
			AssertEquals("N", savedLine.AL_ReverseToGL);
			AssertEquals(false, savedLine.AL_PreventInvoicePrintGrouping);
			AssertEquals(true, savedLine.AL_JH.IsEmpty);
			AssertEquals(true, savedLine.AL_AC.IsEmpty);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, savedLine.AL_GE);
			AssertEquals(GlbBranch.CurrentBranch.PK, savedLine.AL_GB);
			AssertEquals(GLAccount.PK, savedLine.AL_AG);
			AssertEquals(true, savedLine.AL_OH.IsEmpty);
			AssertEquals(true, savedLine.AL_AG_PercentOf.IsEmpty);
			AssertEquals(0, savedLine.AL_PercentageOfPeriod);
			//				AssertEquals(true, SavedLine.AL_AZ_TransactionGroup.IsEmpty);
		}

		[TestDate(2000, 1, 21)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestSavedDataSameCurrencyBankAccount()
		{
			DirectReceipt testReceipt = Factory.New<DirectReceipt>();

			AssertEquals("Should be the same currency", BankAccount.AB_RX_NKAccountCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			testReceipt.AH_AB = BankAccount.PK;
			testReceipt.AH_TransactionReference = "abc";
			testReceipt.AH_Desc = "test receipt";
			testReceipt.AH_InvoiceDate = new ZDateTime(2000, 1, 20);
			testReceipt.AH_ChequeOrReference = "00123";
			testReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testReceipt.AH_ChequeDrawer = "bbb";
			testReceipt.AH_DrawerBank = "ccc";
			testReceipt.AH_DrawerBranch = "ddd";

			DirectTransactionLineBase testLine = (DirectTransactionLineBase)testReceipt.Lines.AddNew();
			testLine.AL_AG = GLAccount.PK;
			testLine.AL_GB = GlbBranch.CurrentBranch.PK;
			testLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			testLine.AL_OSExTaxAmount = 10;
			testLine.AL_AT = TestObjectCreator.GST1.PK;
			testLine.AL_Desc = "test line";
			Factory.Save();

			BusinessObjectFactory testFactory = new BusinessObjectFactory();

			DirectReceipt savedReceipt = testFactory.Load<DirectReceipt>(testReceipt.PK);

			AssertEquals(savedReceipt.BankAccount.AB_RX_NKAccountCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertEquals(1m, savedReceipt.AH_ExchangeRate);
			AssertEquals(10m, savedReceipt.AH_InvoiceAmount);
			AssertEquals(11m, savedReceipt.AH_OSTotal);

			DirectReceiptLine savedLine = (DirectReceiptLine)savedReceipt.Lines[0];

			AssertEquals(10m, savedLine.AL_LineAmount);
			AssertEquals(1m, savedLine.AL_GSTVAT);
			AssertEquals(11m, savedLine.AL_OSAmount);
			AssertEquals(1m, savedLine.AL_ExchangeRate);

			// AH_ExchangeRate and AH_OSTotal get recalculated on load, test below checks the correct values in the table
			string sQL = "SELECT * FROM dbo.AccTransactionHeader WHERE AH_PK = '" + savedReceipt.PK + "'";
			DbCommand command = Db.Connection.Command(sQL);
			DataSet dataSetResult = new DataSet();
			var resultDataAdpter = command.NewDataAdapter();
			resultDataAdpter.Fill(dataSetResult);

			AssertEquals(1m, dataSetResult.Tables[0].Rows[0]["AH_ExchangeRate"]);
			AssertEquals(11m, dataSetResult.Tables[0].Rows[0]["AH_OSTotal"]);
		}

		public void TestNeedPlaceOfSupplyWhenMultipleFixedPlaceOfSupplyIsNotAllowed()
		{
			var testReceipt = Factory.NewWithValidTestData<DirectReceipt>();
			AssertEquals("NeedPlaceOfSupply", false, testReceipt.NeedPlaceOfSupplyAtHeaderLevel);

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				AssertEquals("NeedPlaceOfSupply", true, testReceipt.NeedPlaceOfSupplyAtHeaderLevel);
				AssertEquals("NeedPlaceOfSupply", true, testReceipt.NeedPlaceOfSupplyAtLineLevel);
			}
		}

		public void TestNeedPlaceOfSupplyWhenMultipleFixedPlaceOfSupplyIsAllowed()
		{
			var testReceipt = Factory.NewWithValidTestData<DirectReceipt>();
			AssertEquals("NeedPlaceOfSupply", false, testReceipt.NeedPlaceOfSupplyAtHeaderLevel);

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("NeedPlaceOfSupply", false, testReceipt.NeedPlaceOfSupplyAtHeaderLevel);
				AssertEquals("NeedPlaceOfSupply", true, testReceipt.NeedPlaceOfSupplyAtLineLevel);
			}
		}

		protected override SecurityCheckpoint GetOverrideTaxBranchSecurity(string ledger)
		{
			return Env.Security.NewCashBookAllowOverrideTaxBranch;
		}

		#region CreatingDepositBatch

		public void TestCreatingDepositBatch()
		{
			foreach (ICodeDescription receiptPaymentMethod in TestBizO.Lookups.ReceiptPaymentMethodsList)
			{
				AssertCreatingDepositBatch(receiptPaymentMethod.Code);
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

		void AssertCreatingDepositBatch(ZString receiptType)
		{
			DirectReceipt testBizO = GetNewBusinessObject() as DirectReceipt;
			testBizO.AH_AB = BankAccount.PK;
			testBizO.AH_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
			testBizO.AH_ExchangeRate = 2m;
			testBizO.AH_TransactionReference = "abc";
			testBizO.AH_Desc = "test receipt";
			testBizO.AH_InvoiceDate = new ZDateTime(2000, 1, 20);
			testBizO.AH_ChequeOrReference = "00123";
			testBizO.AH_ReceiptType = receiptType;
			testBizO.AH_ChequeDrawer = "bbb";

			DirectTransactionLineBase testLine = (DirectTransactionLineBase)testBizO.Lines.AddNew();
			testLine.AL_AG = GLAccount.PK;
			testLine.AL_GB = GlbBranch.CurrentBranch.PK;
			testLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			testLine.AL_OSExTaxAmount = 10;
			testLine.AL_AT = TestObjectCreator.GST1.PK;
			testLine.AL_Desc = "test line";
			Factory.Save();

			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, testBizO.AH_ReceiptBatchNo);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, testBizO.AH_GC);
			DepositBatch.DepositBatch testDepositBatch = testFactory.LoadTop1<DepositBatch.DepositBatch>(filter);
			AssertEquals(string.Format("DepositBatch should be created for {0} DirectReceipt", receiptType),
				DoesNeedCreatingDepositBatchForTheType(receiptType), testDepositBatch != null);
		}

		#endregion

		public void TestDocManagerCode()
		{
			AssertEquals("Wrong DocManagerCode. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "DRC", ((IDocManagerSupport)TestDirectReceipt).DocManagerInfo.DocManagerCode);
		}

		DirectReceipt TestDirectReceipt
		{
			get { return (DirectReceipt)TestBizO; }
		}
	}
}
