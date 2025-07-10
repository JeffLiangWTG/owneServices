using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Utility.Testing.TaxFrameworkTestObjectCreator;
using OpeningReceipt = Enterprise.Accounting.Business.CashBook.OpeningReceipt.OpeningReceipt;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	public abstract class MatchingBaseTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected MatchingBase TestMatchingBase
		{
			get
			{
				if (fTestMatchingBase == null)
				{
					fTestMatchingBase = GetTestMatchingBase();
				}

				return fTestMatchingBase;
			}
		}

		protected MatchingBase fTestMatchingBase;

		// Test Transactions
		protected ARInvoice TestARInvoice1;
		protected ARInvoice TestARInvoice2;
		protected ARInvoice TestARInvoice4;
		protected ARInvoice TestARInvoice4_2;

		protected APInvoice TestAPInvoice1;
		protected APInvoice TestAPInvoice2;
		protected APInvoice TestAPInvoice5;
		protected APInvoice TestAPInvoice5_2;

		protected ARReceipt TestARReceipt;
		protected APReceipt TestAPReceipt;

		protected APPayment TestAPPayment;

		// Test AR CreditNotes
		protected ARCreditNote ARCRD1;
		protected ARCreditNote ARCRD2;
		protected ARCreditNote ARCRD3;

		// Test AP CreditNotes
		protected APCreditNote APCRD1;
		protected APCreditNote APCRD2;
		protected APCreditNote APCRD3;
		protected APCreditNote APCRD6;
		protected APCreditNote APCRD6_2;

		protected Discount TestDSC;

		// Test Organizations
		protected OrgHeader TestOrg1;
		protected OrgHeader TestOrg2;
		protected OrgHeader TestOrg3;
		protected OrgHeader TestOrg4;
		protected OrgHeader TestOrg5;
		protected OrgHeader TestOrg6;

		protected abstract MatchingBase GetTestMatchingBaseInNewFactory(BusinessObjectFactory factoryForNewMatchingObject);
		protected abstract MatchingBase GetTestMatchingBase();
		protected abstract MatchingBase GetTestMatchingBaseWithReceiptPaymentDetail(ReceiptPaymentBase receiptPaymentBase);
		protected abstract Type InvoiceType { get; }
		protected abstract ZString LedgerType { get; }
		protected abstract ZBool ShouldTestPayLine { get; }

		#region SetUpTestDataSet

		public virtual void SetUpTestDataSet()
		{
			TestCaseHelper.ClearTable(AccTransactionMatchLink.Schema.TableName);

			TestARInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			TestObjectCreator.CreateInvoiceLine(TestARInvoice1, TestARInvoice1.TransactionCurrency, 1m, 0m);
			TestARInvoice2 = Factory.NewWithValidTestData<ARInvoice>();
			TestObjectCreator.CreateInvoiceLine(TestARInvoice2, TestARInvoice2.TransactionCurrency, 1m, 0m);
			TestARInvoice4 = Factory.NewWithValidTestData<ARInvoice>();
			TestObjectCreator.CreateInvoiceLine(TestARInvoice4, TestARInvoice4.TransactionCurrency, 1m, 0m);
			TestARInvoice4_2 = Factory.NewWithValidTestData<ARInvoice>();
			TestObjectCreator.CreateInvoiceLine(TestARInvoice4_2, TestARInvoice4_2.TransactionCurrency, 1m, 0m);

			TestAPInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice1, TestAPInvoice1.TransactionCurrency, 1m, 0m);
			TestAPInvoice2 = Factory.NewWithValidTestData<APInvoice>();
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice2, TestAPInvoice2.TransactionCurrency, 1m, 0m);
			TestAPInvoice5 = Factory.NewWithValidTestData<APInvoice>();
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice5, TestAPInvoice5.TransactionCurrency, 1m, 0m);
			TestAPInvoice5_2 = Factory.NewWithValidTestData<APInvoice>();
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice5_2, TestAPInvoice5_2.TransactionCurrency, 1m, 0m);

			TestARReceipt = Factory.NewWithValidTestData<ARReceipt>();
			TestAPPayment = Factory.NewWithValidTestData<APPayment>();

			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg1.CompanyData.OB_IsDebtor = true;
			TestOrg1.CompanyData.OB_IsCreditor = true;

			TestOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg2.OH_IsDebtor = true;
			TestOrg2.OH_IsCreditor = true;

			TestOrg3 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg4 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg5 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg6 = Factory.NewWithValidTestData<OrgHeader>();

			//Factory.Save();
		}

		#endregion

		#region SetUpTestDataSet1

		protected void SetUpTestDataSet1()
		{
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			// Test Org 1
			ARCRD1 = Factory.NewWithValidTestData<ARCreditNote>();
			ARCRD1.AH_OH = TestOrg1.PK;
			ARCRD1.AH_LocalExTaxAmount = 12.34M;
			ARCRD1.AH_OSTotal = 12.34M;
			InvoicingLineBase line = (InvoicingLineBase)ARCRD1.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_LineAmount = -12.34M;
			line.AL_OverseasTotal = 12.34M;
			ARCRD1.AH_FullyPaidDate = ZDateTime.Empty;

			TestAPInvoice1.AH_OH = TestOrg1.PK;
			TestAPInvoice1.AH_LocalExTaxAmount = 20.65M;
			TestAPInvoice1.AH_OSTotal = 20.65M;
			line = (InvoicingLineBase)TestAPInvoice1.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_LineAmount = -20.65M;
			line.AL_OverseasTotal = 20.65M;
			TestAPInvoice1.AH_FullyPaidDate = ZDateTime.Empty;

			APCRD1 = Factory.NewWithValidTestData<APCreditNote>();
			APCRD1.AH_OH = TestOrg1.PK;
			APCRD1.AH_LocalExTaxAmount = 31.73M;
			APCRD1.AH_OSTotal = 31.73M;
			line = (InvoicingLineBase)APCRD1.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_LineAmount = 31.73M;
			line.AL_OverseasTotal = 31.73M;
			APCRD1.AH_FullyPaidDate = ZDateTime.Empty;

			// Test Org 2
			ARCRD2 = Factory.NewWithValidTestData<ARCreditNote>();
			ARCRD2.AH_OH = TestOrg2.PK;
			ARCRD2.AH_LocalExTaxAmount = 14.56M;
			ARCRD2.AH_OSTotal = 14.56M;
			line = (InvoicingLineBase)ARCRD2.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_LineAmount = -14.56M;
			line.AL_OverseasTotal = 14.56M;
			ARCRD2.AH_FullyPaidDate = ZDateTime.Empty;

			TestARInvoice2.AH_OH = TestOrg2.PK;
			TestARInvoice2.AH_LocalExTaxAmount = 65.09M;
			TestARInvoice2.AH_OSTotalAmount = 65.09M;
			line = (InvoicingLineBase)TestARInvoice2.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_LineAmount = 65.09M;
			line.AL_OverseasTotal = 65.09M;
			TestARInvoice2.AH_FullyPaidDate = ZDateTime.Empty;

			TestAPInvoice2.AH_OH = TestOrg2.PK;
			TestAPInvoice2.AH_LocalExTaxAmount = 52.34M;
			TestAPInvoice2.AH_OSTotalAmount = 52.34M;
			line = (InvoicingLineBase)TestAPInvoice2.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_LineAmount = -52.34M;
			line.AL_OverseasTotal = 52.34M;
			TestAPInvoice2.AH_FullyPaidDate = ZDateTime.Empty;

			APCRD2 = Factory.NewWithValidTestData<APCreditNote>();
			APCRD2.AH_OH = TestOrg2.PK;
			APCRD2.AH_LocalExTaxAmount = 34.09M;
			APCRD2.AH_OSTotalAmount = 34.09M;
			line = (InvoicingLineBase)APCRD2.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_LineAmount = 34.09M;
			line.AL_OverseasTotal = 34.09M;
			APCRD2.AH_FullyPaidDate = ZDateTime.Empty;

			// Test Org 3
			ARCRD3 = Factory.NewWithValidTestData<ARCreditNote>();
			ARCRD3.AH_OH = TestOrg3.PK;
			ARCRD3.AH_LocalExTaxAmount = 100.9M;
			ARCRD3.AH_OSTotalAmount = 100.9M;
			line = (InvoicingLineBase)ARCRD3.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_LineAmount = -100.9M;
			line.AL_OverseasTotal = 100.9M;
			ARCRD3.AH_FullyPaidDate = ZDateTime.Empty;

			APCRD3 = Factory.NewWithValidTestData<APCreditNote>();
			APCRD3.AH_OH = TestOrg3.PK;
			APCRD3.AH_LocalExTaxAmount = 5.02M;
			APCRD3.AH_OSTotalAmount = 5.02M;
			line = (InvoicingLineBase)APCRD3.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_LineAmount = 5.02M;
			line.AL_OverseasTotal = 5.02M;
			APCRD3.AH_FullyPaidDate = ZDateTime.Empty;

			// Test Org 4
			TestARInvoice4.AH_OH = TestOrg4.PK;
			TestARInvoice4.AH_LocalExTaxAmount = 15.15M;
			TestARInvoice4.AH_OSTotalAmount = 15.15M;
			line = (InvoicingLineBase)TestARInvoice4.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_LineAmount = 15.15M;
			line.AL_OverseasTotal = 15.15M;
			TestARInvoice4.AH_FullyPaidDate = ZDateTime.Empty;

			TestARInvoice4_2 = Factory.NewWithValidTestData<ARInvoice>();
			TestARInvoice4_2.AH_OH = TestOrg4.PK;
			TestARInvoice4_2.AH_LocalExTaxAmount = 25.25M;
			TestARInvoice4_2.AH_OSTotalAmount = 25.25M;
			line = (InvoicingLineBase)TestARInvoice4_2.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_LineAmount = 25.25M;
			line.AL_OverseasTotal = 25.25M;
			TestARInvoice4_2.AH_FullyPaidDate = ZDateTime.Empty;

			// Test Org 5 
			TestAPInvoice5 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInvoice5.AH_OH = TestOrg5.PK;
			TestAPInvoice5.AH_LocalExTaxAmount = 12.12M;
			TestAPInvoice5.AH_OSTotalAmount = 12.12M;
			line = (InvoicingLineBase)TestAPInvoice5.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_LineAmount = -12.12M;
			line.AL_OverseasTotal = 12.12M;
			TestAPInvoice5.AH_FullyPaidDate = ZDateTime.Empty;

			TestAPInvoice5_2 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInvoice5_2.AH_OH = TestOrg5.PK;
			TestAPInvoice5_2.AH_LocalExTaxAmount = 22.22M;
			TestAPInvoice5_2.AH_OSTotalAmount = 22.22M;
			line = (InvoicingLineBase)TestAPInvoice5_2.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_LineAmount = -22.22M;
			line.AL_OverseasTotal = 22.22M;
			TestAPInvoice5_2.AH_FullyPaidDate = ZDateTime.Empty;

			// Test Org 6
			APCRD6 = Factory.NewWithValidTestData<APCreditNote>();
			APCRD6.AH_OH = TestOrg6.PK;
			APCRD6.AH_LocalExTaxAmount = 10.10M;
			APCRD6.AH_OSTotalAmount = 10.10M;
			line = (InvoicingLineBase)APCRD6.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_LineAmount = 10.10M;
			line.AL_OverseasTotal = 10.10M;
			APCRD6.AH_FullyPaidDate = ZDateTime.Empty;

			APCRD6_2 = Factory.NewWithValidTestData<APCreditNote>();
			APCRD6_2.AH_OH = TestOrg6.PK;
			APCRD6_2.AH_LocalExTaxAmount = 20.20M;
			APCRD6_2.AH_OSTotalAmount = 20.20M;
			line = (InvoicingLineBase)APCRD6_2.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_LineAmount = 20.20M;
			line.AL_OverseasTotal = 20.20M;
			APCRD6_2.AH_FullyPaidDate = ZDateTime.Empty;

			/*
			 * Adding the transactions to the MatchingCollection
			 */

			// Test Org 2
			TestMatchingBase.AddIMatching(ARCRD2);
			TestMatchingBase.AddIMatching(TestARInvoice2);
			TestMatchingBase.AddIMatching(TestAPInvoice2);
			TestMatchingBase.AddIMatching(APCRD2);

			// Test Org 3
			TestMatchingBase.AddIMatching(ARCRD3);
			TestMatchingBase.AddIMatching(APCRD3);

			// Test Org 4
			TestMatchingBase.AddIMatching(TestARInvoice4);
			TestMatchingBase.AddIMatching(TestARInvoice4_2);

			// Test Org 5
			TestMatchingBase.AddIMatching(TestAPInvoice5);
			TestMatchingBase.AddIMatching(TestAPInvoice5_2);

			// Test Org 6
			TestMatchingBase.AddIMatching(APCRD6);
			TestMatchingBase.AddIMatching(APCRD6_2);
		}

		#endregion

		#region Test Object Creation

		protected AccChequeBook GetTestChequeBook()
		{
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_StartNo = 1;
			chequeBook.AK_CurrentNo = 5;
			chequeBook.AK_LastNo = 10;
			return chequeBook;
		}

		protected OrgHeader GetNewTestOrg()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsCreditor = true;
			org.OH_IsDebtor = true;
			return org;
		}

		protected ARInvoice GetNewTestARInvoice(ZDecimal amount, OrgHeader org)
		{
			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_OH = org.PK;
			TestObjectCreator.CreateInvoiceLine(aRInv, aRInv.TransactionCurrency, 1m, amount);
			return aRInv;
		}

		#endregion

		#endregion

		#region AllowAlteringOfPayment

		public void TestAllowAlteringOfPayment()
		{
			var matchingBaseWithoutReceiptPaymentDetail = GetTestMatchingBase();
			AssertNull("Matching base is not referencing receipt.", matchingBaseWithoutReceiptPaymentDetail.ReceiptPaymentDetail_ForTestOnly);

			if (!(matchingBaseWithoutReceiptPaymentDetail is PaymentApprovalMatchingBase))
			{
				Assert("When matching base is not referencing receipt, change receipt amount button is disabled.", !matchingBaseWithoutReceiptPaymentDetail.AllowAlteringOfPayment);

				var receipt = TestObjectCreator.CreateReceiptOrPayment(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerType, 10m, TestObjectCreator.AUDBankAccount.PK);
				Assert(!receipt.IsInDatabase);

				var matchingBaseWithReceiptPaymentDetail = GetTestMatchingBaseWithReceiptPaymentDetail(receipt);
				AssertNotNull("Matching base is referencing receipt.", matchingBaseWithReceiptPaymentDetail.ReceiptPaymentDetail_ForTestOnly);
				Assert("When receipt is not in database, change receipt amount button is enabled.", matchingBaseWithReceiptPaymentDetail.AllowAlteringOfPayment);

				Factory.Save();
				Assert(receipt.IsInDatabase);

				matchingBaseWithReceiptPaymentDetail = GetTestMatchingBaseWithReceiptPaymentDetail(receipt);
				AssertNotNull("Matching base is referencing receipt.", matchingBaseWithReceiptPaymentDetail.ReceiptPaymentDetail_ForTestOnly);
				Assert("When receipt is in database, , change receipt amount button is disabled.", !matchingBaseWithReceiptPaymentDetail.AllowAlteringOfPayment);
			}
			else
			{
				Assert("PaymentApprovalMatching change receipt amount button is always enabled.", matchingBaseWithoutReceiptPaymentDetail.AllowAlteringOfPayment);
			}
		}

		#endregion

		#region DecimalPlaces

		public void TestZDecimalsHaveCorrectDecimalPlacesMatchingBase()
		{
			var localList = new List<string>
				{
					nameof(TestMatchingBase.LocalOverpaymentAmount),
					nameof(TestMatchingBase.DiscountAmount),
					nameof(TestMatchingBase.ExchangeDifferenceAmount),
					nameof(TestMatchingBase.BankFeeAmount),
					nameof(TestMatchingBase.Balance)
				};

			var oSList = new List<string>
				{
					nameof(TestMatchingBase.OSOverpaymentAmount)
				};

			var exList = new List<string>
				{
					nameof(TestMatchingBase.ExchangeRateAmount)
				};

			TestMatchingBase.AddMiscellaneousTransaction(Factory.New<APOverpayment>());

			var tester = new DecimalPlacesAttributeTester(TestMatchingBase);
			tester.CheckLocalCurrency(localList, nameof(TestMatchingBase.LocalDecimals));
			tester.CheckNonLocalCurrency(oSList, nameof(TestMatchingBase.OSDecimals), nameof(TestMatchingBase.OverpaymentBizO_ForTestOnly.AH_RX_NKTransactionCurrency), TestMatchingBase.OverpaymentBizO_ForTestOnly);
		}

		#endregion

		#region Matching Invariants for Testing

		#region MatchingSessionsWithNonZeroAmounts

		protected ZInt MatchingSessionsWithNonZeroAmounts
		{
			get
			{
				ZString sQLString = "SELECT " + TransactionMatchLink.Schema.AP_MatchGroupNum +
					", SUM(" + TransactionMatchLink.Schema.AP_Amount + ") AS Balance FROM " +
					TransactionMatchLink.Schema.TableName + " GROUP BY " +
					TransactionMatchLink.Schema.AP_MatchGroupNum + " HAVING SUM(" +
					TransactionMatchLink.Schema.AP_Amount + ") != 0";
				DynamicBusinessObjectCollection dynBizOCollection = new DynamicBusinessObjectCollection(Factory);
				dynBizOCollection.Load(sQLString, new ZSqlParameterCollection());
				return dynBizOCollection.Count;
			}
		}

		#endregion

		#region TransactionsNotOutstanding

		ZInt TransactionsNotOutstanding(BusinessObjectFactory factory = null)
		{
			var filter = new ZQuery(AccTransactionHeaderSchema.AH_FullyPaidDate, SQLComparisonOperator.NotEqual, null);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_OutstandingAmount, SQLComparisonOperator.NotEqual, 0);

			var ledgerFilter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			ledgerFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_Ledger, SQLComparisonOperator.Equal, ZArchitecture.Core.LedgerTypes.AccountsPayable);
			filter.AddToFilter(ledgerFilter, JoinCondition.And);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

			var headers = new TransactionHeaderCollection(factory ?? Factory, filter);
			headers.Load();

			return headers.Count;
		}

		#endregion

		#region DoesARAPBalance

		public ZBool DoesARAPBalance()
		{
			ZString sQLString = "SELECT " + TransactionHeader.Schema.AH_Ledger + ", SUM("
				+ TransactionHeader.Schema.AH_InvoiceAmount + " + " + TransactionHeader.Schema.AH_GSTAmount
				+ ") AS InvPlusGST, SUM(" + TransactionHeader.Schema.AH_OutstandingAmount
				+ ") AS Outstanding FROM " + TransactionHeader.Schema.TableName + " WHERE "
				+ TransactionHeader.Schema.AH_Ledger + " IN(@AR,@AP) GROUP BY "
				+ TransactionHeader.Schema.AH_Ledger;
			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add("@AR", "AR", AccTransactionHeaderSchema.AH_Ledger);
			@params.Add("@AP", "AP", AccTransactionHeaderSchema.AH_Ledger);
			DynamicBusinessObjectCollection dynBizOs = new DynamicBusinessObjectCollection(Factory);
			dynBizOs.Load(sQLString, @params);

			//SUM(InvoiceAmount + GSTAmount) = SUM(OutstandingAmount) for AP
			//SUM(InvoiceAmount + GSTAmount) = SUM(OutstandingAmount) for AR
			if (dynBizOs.Count == 1)
			{
				return ((ZDecimal)dynBizOs[0]["InvPlusGST"]).Equals(dynBizOs[0]["Outstanding"]);
			}
			else if (dynBizOs.Count == 2)
			{
				return !(dynBizOs[0]["InvPlusGST"] != dynBizOs[0]["Outstanding"] ||
					dynBizOs[1]["InvPlusGST"] != dynBizOs[1]["Outstanding"]);
			}
			else
			{
				return false;
			}
		}

		#endregion

		#region OrgsWithNonZeroAmounts

		public ZInt OrgsWithNonZeroAmounts
		{
			get
			{
				ZString sQLString = "SELECT " + OrgHeader.Schema.OH_Code + ", SUM("
					+ TransactionMatchLink.Schema.AP_Amount + ") AS AmountSum FROM "
					+ TransactionMatchLink.Schema.TableName + " INNER JOIN "
					+ TransactionHeader.Schema.TableName + " ON " + TransactionHeader.Schema.PK + "="
					+ TransactionMatchLink.Schema.AP_AH + " INNER JOIN " + OrgHeaderSchema.Constants.SqlSchemaName + "." + OrgHeaderSchema.Constants.TableName
					+ " ON " + TransactionHeader.Schema.AH_OH + "=" + OrgHeader.Schema.PK
					+ " GROUP BY " + OrgHeader.Schema.OH_Code + ", " + TransactionMatchLink.Schema.AP_MatchGroupNum
					+ " HAVING SUM(" + TransactionMatchLink.Schema.AP_Amount + ")!=0";

				DynamicBusinessObjectCollection dynBizOs = new DynamicBusinessObjectCollection(Factory);
				dynBizOs.Load(sQLString, new ZSqlParameterCollection());
				return dynBizOs.Count;
			}
		}

		#endregion

		#endregion

		#region Tests for Matching Invariants

		public void TestTransactionsOutstanding()
		{
			ARInvoice testAR = Factory.NewWithValidTestData<ARInvoice>();
			var line = (ARInvoiceLine)testAR.Lines.AddNew();
			line.AL_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			line.AL_LocalExTaxAmount = 9M;

			testAR.AH_OutstandingAmount = 4M;
			TransactionMatchLink testMatch = ((IMatching)testAR).CurrentMatchGroup.AddNew();
			testMatch.AP_Amount = 5M;
			testMatch.AP_MatchGroupNum = "M00001000";
			testMatch.AP_AH = testAR.PK;

			AccTransactionHeader headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_InvoiceAmount = -testMatch.AP_Amount;

			TransactionMatchLink linkToMatch = ((IMatching)testAR).CurrentMatchGroup.AddNew();
			linkToMatch.AP_AH = headerToMatch.PK;
			linkToMatch.AP_Amount = -testMatch.AP_Amount;
			linkToMatch.AP_MatchGroupNum = testMatch.AP_MatchGroupNum;
			TestObjectCreator.SetupMatchLinkMatchDate(testAR);

			Factory.Save();

			var sql = string.Format("Update dbo.AccTransactionHeader Set AH_FullyPaidDate = '{0}', AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST' Where AH_PK = '{1}'", ZDateTime.Now.ToISO8601String(), testAR.PK);
			Db.Connection.ExecuteNonQuery(sql);

			var count = TransactionsNotOutstanding(new BusinessObjectFactory());

			Assert("There is a transaction with nonzero outstanding amount", count != 0);
		}

		public void TestDoesARAPBalance()
		{
			ARInvoice testAR = Factory.NewWithValidTestData<ARInvoice>();
			TestObjectCreator.CreateInvoiceLine(testAR, GlbCompany.CurrentCompany.LocalCurrency, 1m, 10m, 0m, 0m, 10m, 0m, 0m);
			testAR.AH_OutstandingAmount = 9M;

			Assert("SUM(InvoiceAmount + GSTAmount) != SUM(OutstandingAmount) for AR", !DoesARAPBalance());

			testAR.AH_OutstandingAmount = 10M;
			Factory.Save();
			Assert("SUM(InvoiceAmount + GSTAmount) != SUM(OutstandingAmount) for AR", DoesARAPBalance());
		}

		public void TestOrgsWithNonZeroAmounts()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_Code = "AAAAA";
			Factory.Save();

			ARInvoice testAR = Factory.NewWithValidTestData<ARInvoice>();
			testAR.AH_OH = testOrg.PK;
			var line = (ARInvoiceLine)testAR.Lines.AddNew();
			line.AL_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			line.AL_LocalExTaxAmount = -90M;

			TransactionMatchLink testMatch = ((IMatching)testAR).CurrentMatchGroup.AddNew();
			testMatch.AP_Amount = -90M;
			testMatch.AP_MatchGroupNum = "M00001000";
			testMatch.AP_AH = testAR.PK;
			testAR.AH_OutstandingAmount = 0M;

			AccTransactionHeader headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_InvoiceAmount = -testMatch.AP_Amount;

			TransactionMatchLink linkToMatch = ((IMatching)testAR).CurrentMatchGroup.AddNew();
			linkToMatch.AP_AH = headerToMatch.PK;
			linkToMatch.AP_Amount = -testMatch.AP_Amount;
			linkToMatch.AP_MatchGroupNum = testMatch.AP_MatchGroupNum;
			TestObjectCreator.SetupMatchLinkMatchDate(testAR);

			Factory.Save();

			Assert("Balance of AP_Amount for TestOrg is non-zero", OrgsWithNonZeroAmounts != 0);
		}

		#endregion

		public void TestMatchingWithCanceledTransaction()
		{
			if (TestMatchingBase is PaymentApprovalMatchingBase)
			{
				Assert(true);
			}
			else
			{
				SetUpTestDataSet();
				Factory.Save();
				TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

				TestARInvoice1.AH_OH = TestOrg1.PK;
				TestARInvoice1.AH_ExchangeRate = 1m;
				TestARInvoice1.AH_IsCancelled = true;
				TestObjectCreator.CreateInvoiceLine(TestARInvoice1, TestARInvoice1.TransactionCurrency, TestARInvoice1.AH_ExchangeRate, -50m, 0m, 0m, -50m, 0m, 0m);
				((IMatching)TestARInvoice1).OSPartialPaymentAmount = -50M;
				AssertNoRowError(TestARInvoice1, "You cannot choose a canceled transaction for matching");
				TestMatchingBase.AddIMatching(TestARInvoice1);

				TestARInvoice2.AH_OH = TestOrg1.PK;
				TestARInvoice2.AH_ExchangeRate = 1m;
				TestObjectCreator.CreateInvoiceLine(TestARInvoice2, TestARInvoice2.TransactionCurrency, TestARInvoice2.AH_ExchangeRate, -50m, 0m, 0m, -50m, 0m, 0m);
				((IMatching)TestARInvoice2).OSPartialPaymentAmount = -50M;
				AssertNoRowError(TestARInvoice2, "You cannot choose a canceled transaction for matching");
				TestMatchingBase.AddIMatching(TestARInvoice2);

				ARPayment aRPayment = TestObjectCreator.CreateARPayment(1m, 100m, ZDateTime.Today, ZDateTime.Today, TestOrg1.PK, TestObjectCreator.AUDBankAccount.PK);
				((IMatching)aRPayment).OSPartialPaymentAmount = 100M;
				TestMatchingBase.AddIMatching(aRPayment);

				AssertHasRowError(TestARInvoice1, "You cannot choose a canceled transaction for matching");
				AssertNoRowError(TestARInvoice2, "You cannot choose a canceled transaction for matching");
				Assert("Transactions should not be matchable", !TestMatchingBase.Match_ForTestOnly());
			}
		}

		public void TestMatchingTransactionWithOpenClaim()
		{
			if (TestMatchingBase.LedgerType == LedgerTypes.AccountsReceivable)
			{
				Assert(true);
			}
			else
			{
				SetUpTestDataSet();
				Factory.Save();
				TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
				TestAPInvoice1.AH_OH = TestOrg1.PK;
				TestAPInvoice1.AH_ExchangeRate = 1m;
				TestObjectCreator.CreateInvoiceLine(TestAPInvoice1, TestAPInvoice1.TransactionCurrency, TestAPInvoice1.AH_ExchangeRate, -50m, 0m, 0m, -50m, 0m, 0m);
				((IMatching)TestAPInvoice1).OSPartialPaymentAmount = -50M;
				AssertNoRowError(TestAPInvoice1, "This transaction cannot be matched as it is linked to an open claim");
				TestMatchingBase.AddIMatching(TestAPInvoice1);
				AssertNoRowError(TestAPInvoice1, "This transaction cannot be matched as it is linked to an open claim");

				TestMatchingBase.MatchedTransactions.Remove(TestAPInvoice1);
				var claim = Factory.New<APAccQueryClaim>();
				claim.AY_OH_Debtor = TestObjectCreator.AALSHI.PK;
				claim.AY_QueryClaimReference = "ref";
				claim.AY_QueryClaimStatus = QueryClaimStatusCodeList.Codes.QCStatus1Open;
				claim.AY_AH = TestAPInvoice1.PK;

				AssertNoRowError(TestAPInvoice1, "This transaction cannot be matched as it is linked to an open claim");
				TestMatchingBase.AddIMatching(TestAPInvoice1);
				AssertHasRowError(TestAPInvoice1, "This transaction cannot be matched as it is linked to an open claim");
			}
		}

		public void TestMatchingTransactionWithHoldOptionAPClaim()
		{
			if (TestMatchingBase.LedgerType != LedgerTypes.AccountsPayable)
			{
				Assert(true);
				return;
			}

			SetUpTestDataSet();
			Factory.Save();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestAPInvoice1.AH_OH = TestOrg1.PK;
			TestAPInvoice1.AH_ExchangeRate = 1m;
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice1, TestAPInvoice1.TransactionCurrency, TestAPInvoice1.AH_ExchangeRate, -50m, 0m, 0m, -50m, 0m, 0m);
			((IMatching)TestAPInvoice1).OSPartialPaymentAmount = -50M;

			var claim = Factory.New<APAccQueryClaim>();
			claim.AY_OH_Debtor = TestObjectCreator.AALSHI.PK;
			claim.AY_QueryClaimReference = "ref";
			claim.AY_QueryClaimStatus = QueryClaimStatusCodeList.Codes.QCStatus1Open;
			claim.AY_AH = TestAPInvoice1.PK;

			claim.AY_HoldOption = HoldOptionType.Codes.DNM;

			Assert(!TestAPInvoice1.HasRowErrors);
			Assert(!TestAPInvoice1.HasRowNotifications);

			TestMatchingBase.AddIMatching(TestAPInvoice1);

			Assert(TestAPInvoice1.HasRowErrors);
			AssertHasRowError("Should show errors when hold option is DNM.", TestAPInvoice1, "This transaction cannot be matched as it is linked to an open claim");

			TestMatchingBase.MatchedTransactions.Remove(TestAPInvoice1);
			TestAPInvoice1.ClearAllNotifications();
			claim.AY_HoldOption = HoldOptionType.Codes.ALM;

			Assert(!TestAPInvoice1.HasRowErrors);
			Assert(!TestAPInvoice1.HasRowNotifications);

			TestMatchingBase.AddIMatching(TestAPInvoice1);

			Assert(!TestAPInvoice1.HasRowErrors);
			Assert(TestAPInvoice1.HasRowWarnings);
			AssertHasRowWarning("Should show warnings when hold option is ALM.", TestAPInvoice1, "This transaction is linked to an open claim with Allow-To-Match hold option.");
		}

		public void TestMoveFromUnmatchToMatch_PaidViaWebService()
		{
			SetUpTestDataSet();

			AccountingConfigurationRegistry.Instance.EnableInvoicePaymentWebService.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			TestObjectCreator.PartPayInvoiceViaWebService(TestARInvoice2, 1);

			TestMatchingBase.MoveFromUnmatchToMatch(new BusinessObject[] { TestARInvoice1, TestARInvoice2 });

			AssertNoRowError(TestARInvoice1, "This transaction cannot be matched as it is paid through Invoice Payment Web Service.");
			AssertHasRowError(TestARInvoice2, "This transaction cannot be matched as it is paid through Invoice Payment Web Service.");
		}
		public void TestMatching_NoCashAdvanceClearingAccount()
		{
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			var creator = new TestObjectCreator(Factory);

			var exportConsol = Factory.New<ForwardingConsol>();
			var shipment = exportConsol.Shipments.AddNew();
			var shipmentJob = new Job.Loader(shipment).TryCreate();
			shipmentJob.LocalChargesPK = creator.ABIGAS.PK;

			var charge = shipmentJob.Charges.AddNew();
			SetupCharge(charge, true);
			var charge2 = shipmentJob.Charges.AddNew();
			SetupCharge(charge2, false);
			Factory.Save();

			var header = TestObjectCreator.CreateCashAdvanceRequestHeader(shipmentJob, charge.SellAccount, LedgerTypes.AccountsPayable, 0M, 0M, "AUD");
			var line = TestObjectCreator.CreateCashAdvanceRequestLine(header, 1050M, 1050M);
			charge.JR_CAL_APLine = line.PK;

			//Creating an AP payment approval
			var apPaymentApproval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			apPaymentApproval.AV_OH = creator.ABIGAS.PK;
			apPaymentApproval.AV_Amount = 1050M;
			Factory.Save();
			var newFactory2 = new BusinessObjectFactory();
			newFactory2.RefreshEnabled = false;
			var cahInNewFactory2 = newFactory2.Load<CashAdvanceRequestHeader>(header.PK);
			var apPaymentApprovalInNewFactory2 = newFactory2.Load<APPaymentApprovalWithoutAuthorisation>(apPaymentApproval.PK);
			var matchingBase = new APPaymentApprovalMatching(newFactory2, apPaymentApprovalInNewFactory2);
			matchingBase.MatchDate = ZDateTime.Today;
			matchingBase.PrimaryOrganization = creator.ABIGAS.PK;
			matchingBase.UnmatchedCashAdvanceRequests.Add(cahInNewFactory2);
			matchingBase.UnmatchedTransactions.Add(apPaymentApprovalInNewFactory2);
			matchingBase.MoveAllFromUnmatchToMatch();
			AssertExceptionThrown<CannotGenerateCashAdvanceJournalException>($@"This Advance Payment can't be matched as there is no Advance Payment Clearing Account recorded in the {AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.HumanReadableRegistryPath()} registry. Please ensure this registry has a Advance Payment Clearing account recorded and then try the match again."
			, () =>
			{
				matchingBase.MoveAllCashAdvanceFromUnmatchToMatch();
			});
			void SetupCharge(JobCharge localCharge, bool isCashAdvanceRequired)
			{
				localCharge.JR_AC = creator.CC1.PK;
				localCharge.JR_OH_CostAccount = creator.ABIGAS.PK;
				localCharge.JR_OSCostAmt = 1050m;
				localCharge.JR_IsAPCashAdvance = isCashAdvanceRequired;
				localCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			}
		}

		public void TestMatchingTransactionWithZeroBalance()
		{
			if (TestMatchingBase is PaymentApprovalMatchingBase)
			{
				Assert(true);
				return;
			}

			SetUpTestDataSet();

			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.Lines[0].AL_OSExTaxAmount = 10m;

			TestARReceipt.AH_OH = TestOrg1.PK;
			TestARReceipt.AH_LocalExTaxAmount = 10M;
			TestARReceipt.AH_OSExTaxAmount = 10M;

			Factory.Save();

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestMatchingBase.AddIMatching(TestARInvoice1);
			TestMatchingBase.AddIMatching(TestARReceipt);

			((IMatching)TestARInvoice1).OSPartialPaymentAmount = 1m;

			AssertEquals("Transactions should not be matchable", false, TestMatchingBase.Match_ForTestOnly());
			var errors = TestMatchingBase.MatchingErrorsForGUINotificationOnlyInfo.GetErrors();
			AssertEquals(1, errors.Count());
			AssertContains("Balance of transaction match result is not zero.", errors.First().Message);

			((IMatching)TestARReceipt).OSPartialPaymentAmount = -1m;

			AssertEquals("Transactions should be re-matchable with correct context", true, TestMatchingBase.Match_ForTestOnly());
			AssertEquals(0, TestMatchingBase.MatchingErrorsForGUINotificationOnlyInfo.GetErrors().Count());
		}

		public void TestMatchingTransactionWithZeroOutstandingAmount()
		{
			if (TestMatchingBase is PaymentApprovalMatchingBase)
			{
				Assert(true);
				return;
			}

			SetUpTestDataSet();

			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.Lines[0].AL_OSExTaxAmount = 10m;

			TestARReceipt.AH_OH = TestOrg1.PK;
			TestARReceipt.AH_LocalExTaxAmount = 10M;
			TestARReceipt.AH_OSExTaxAmount = 10M;

			Factory.Save();

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestMatchingBase.AddIMatching(TestARInvoice1);
			TestMatchingBase.AddIMatching(TestARReceipt);

			TestARInvoice1.AH_OutstandingAmount = 0m;

			AssertEquals("Transactions should not be matchable", false, TestMatchingBase.Match_ForTestOnly());
			var errors = TestMatchingBase.MatchingErrorsForGUINotificationOnlyInfo.GetErrors();
			AssertEquals(1, errors.Count());
			AssertContains("Transaction for match is already fully paid. It might have been matched in other forms.", errors.First().Message);

			TestARInvoice1.AH_OutstandingAmount = 11m;
			((IMatching)TestARInvoice1).OSPartialPaymentAmount = 1m;
			((IMatching)TestARReceipt).OSPartialPaymentAmount = -1m;

			AssertEquals("Transactions should be re-matchable with correct context", true, TestMatchingBase.Match_ForTestOnly());
			AssertEquals(0, TestMatchingBase.MatchingErrorsForGUINotificationOnlyInfo.GetErrors().Count());
		}

		public void TestMatchingTransactionWithNonRelativeOrg()
		{
			if (TestMatchingBase is PaymentApprovalMatchingBase)
			{
				Assert(true);
				return;
			}

			SetUpTestDataSet();

			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.Lines[0].AL_OSExTaxAmount = 10m;

			TestARReceipt.AH_OH = TestOrg1.PK;
			TestARReceipt.AH_LocalExTaxAmount = 10M;
			TestARReceipt.AH_OSExTaxAmount = 10M;

			Factory.Save();

			TestMatchingBase.PrimaryOrganization = TestOrg2.PK;
			TestMatchingBase.AddIMatching(TestARInvoice1);
			TestMatchingBase.AddIMatching(TestARReceipt);

			AssertEquals("Transactions should not be matchable", false, TestMatchingBase.Match_ForTestOnly());
			var errors = TestMatchingBase.MatchingErrorsForGUINotificationOnlyInfo.GetErrors();
			AssertEquals(1, errors.Count());
			AssertContains("None of matched transactions link with Primary Organization.", errors.First().Message);

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestMatchingBase.AddIMatching(TestARInvoice1);
			TestMatchingBase.AddIMatching(TestARReceipt);
			((IMatching)TestARInvoice1).OSPartialPaymentAmount = 1m;
			((IMatching)TestARReceipt).OSPartialPaymentAmount = -1m;

			AssertEquals("Transactions should be re-matchable with correct context", true, TestMatchingBase.Match_ForTestOnly());
			AssertEquals(0, TestMatchingBase.MatchingErrorsForGUINotificationOnlyInfo.GetErrors().Count());
		}

		public void TestMatchStatusAndReasonForMatchingTransaction()
		{
			SetUpTestDataSet();

			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.Lines[0].AL_OSExTaxAmount = 10m;

			TestARReceipt.AH_OH = TestOrg1.PK;
			TestARReceipt.AH_LocalExTaxAmount = 10M;
			TestARReceipt.AH_OSExTaxAmount = 10M;

			Factory.Save();

			AssertEquals(ZString.Empty, TestARInvoice1.AH_MatchStatus);
			AssertEquals(ZString.Empty, TestARInvoice1.AH_MatchStatusReasonCode);
			AssertEquals("UAC", TestARReceipt.AH_MatchStatus);
			AssertEquals("ADV", TestARReceipt.AH_MatchStatusReasonCode);

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestMatchingBase.AddIMatching(TestARInvoice1);
			TestMatchingBase.AddIMatching(TestARReceipt);

			var matchARInvoice = (IMatching)TestARInvoice1;
			var matchARReceipt = (IMatching)TestARReceipt;
			matchARInvoice.OSPartialPaymentAmount = 1m;
			matchARInvoice.MatchStatus = "UAC";
			matchARInvoice.MatchStatusReasonCode = "ADV";
			matchARReceipt.MatchStatus = "AAA";
			matchARReceipt.MatchStatusReasonCode = "BBB";
			matchARReceipt.OSPartialPaymentAmount = -1m;
			AssertEquals("Transactions should be re-matchable with correct context", true, TestMatchingBase.Match_ForTestOnly());

			var loadedARInvoice = Factory.Load<ARInvoice>(TestARInvoice1.PK);
			AssertEquals("UAC", loadedARInvoice.AH_MatchStatus);
			AssertEquals("ADV", loadedARInvoice.AH_MatchStatusReasonCode);

			var loadedARReceipt = Factory.Load<ARReceipt>(TestARReceipt.PK);
			AssertEquals("AAA", loadedARReceipt.AH_MatchStatus);
			AssertEquals("BBB", loadedARReceipt.AH_MatchStatusReasonCode);
		}

		#region Corrupted Flag Tests

		public void TestCorruptedAfterCriticalValidationError()
		{
			OrgHeader orgHeader = SetupDataForTestWithCreditNoteAndInvoice();

			ARMatchingBase matchingBase = new ARMatchingBase(Factory);
			matchingBase.PrimaryOrganization = orgHeader.PK;

			matchingBase.MoveAllFromUnmatchToMatch();
			AssertEquals(2, matchingBase.MatchedTransactions.Count);

			matchingBase.RunPreSaveValidation();
			AssertNoErrors("Matching base should have no errors.", matchingBase);

			// It is not important for this test to throw Critical Validation exception. Code that is tested should raise Corrupted flag on any kind of exception that causes Save to fail.
			matchingBase.Factory.ForceCriticalValidationErrorForTestOnly(CriticalValidationErrorType.CannotSaveAfterError);
			try
			{
				matchingBase.MatchAndClearTransactions();
				Fail("Match was expected to fail.");
			}
			catch (OnSavingCriticalCheckException)
			{
				// this exeption was expected
			}
			finally
			{
				matchingBase.Factory.ForceCriticalValidationErrorForTestOnly(CriticalValidationErrorType.NoError);
				ExceptionReporterTestListener.Instance.Clear();
			}

			Assert("MatchingBase should be in corrupted state after concurrency error.", matchingBase.Corrupted);
		}

		/// <summary>
		/// We cannot properly emulate two separate applications in tests because PersistentFactoryCacheManager is shared between all instances of BusinessObjectFactory.
		/// Calling Save method on one factory causes PersistentFactoryCacheManager to invalidate caches for all other factories.
		/// In this test it causes Critical Validation exception to happen before concurrency exception.
		/// To avoid Critical Validation exception, and to get to concurrency exception SuspendCriticalValidation attribute is used.
		/// </summary>
		[SuspendCriticalValidation]
		public void TestCorruptedAfterConcurrencyError()
		{
			OrgHeader orgHeader = SetupDataForTestWithCreditNoteAndInvoice();

			ARMatchingBase matchingBase = new ARMatchingBase(Factory);
			matchingBase.PrimaryOrganization = orgHeader.PK;

			matchingBase.MoveAllFromUnmatchToMatch();
			AssertEquals(2, matchingBase.MatchedTransactions.Count);

			// We use partial paying, because if transaction is fully paid, than we will have critical validation error instead of concurrency error.
			// That is the way to reproduce error in ediProd. In this test it is not necessary, because test has SuspendCriticalValidation attribute.
			IMatching invoice = matchingBase.MatchedTransactions.Cast<IMatching>().First(tr => tr is ARInvoice);
			invoice.OSPartialPaymentAmount = 10;

			IMatching creditNote = matchingBase.MatchedTransactions.Cast<IMatching>().First(tr => tr is ARCreditNote);
			creditNote.OSPartialPaymentAmount = -10;

			matchingBase.RunPreSaveValidation();
			AssertNoErrors("Matching base should have no errors.", matchingBase);
			{
				var anotherFactory = new BusinessObjectFactory();
				anotherFactory.RefreshEnabled = false;
				var staff = anotherFactory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "TST";

				using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
				{
					ARMatchingBase anotherMatchingBase = new ARMatchingBase(anotherFactory);
					anotherMatchingBase.PrimaryOrganization = orgHeader.PK;

					anotherMatchingBase.MoveAllFromUnmatchToMatch();
					AssertEquals(2, anotherMatchingBase.MatchedTransactions.Count);

					// We use partial paying, because if transaction is fully paid, than we will have critical validation error instead of concurrency error.
					// That is the way to reproduce error in ediProd. In this test it is not necessary, because test has SuspendCriticalValidation attribute.
					IMatching anotherInvoice = anotherMatchingBase.MatchedTransactions.Cast<IMatching>().First(tr => tr is ARInvoice);
					anotherInvoice.OSPartialPaymentAmount = 10;

					IMatching anotherCreditNote = anotherMatchingBase.MatchedTransactions.Cast<IMatching>().First(tr => tr is ARCreditNote);
					anotherCreditNote.OSPartialPaymentAmount = -10;

					anotherMatchingBase.RunPreSaveValidation();
					Assert("Matching base should have no errors.", !anotherMatchingBase.HasErrors);

					anotherMatchingBase.MatchAndClearTransactionsWithSaveErrorHandling();

					Assert("MatchingBase should not be in corrupted state after matching by first user.", !anotherMatchingBase.Corrupted);
				}
			}

			matchingBase.MatchAndClearTransactionsWithSaveErrorHandling();

			Assert("MatchingBase should be in corrupted state after concurrency error.", matchingBase.Corrupted);
		}

		OrgHeader SetupDataForTestWithCreditNoteAndInvoice()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsCreditor = true;
			orgHeader.OH_IsDebtor = true;

			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();

			Factory.Save();

			ZDecimal sum = 50M;

			ARCreditNote creditNote = Factory.NewWithValidTestData<ARCreditNote>();
			creditNote.AH_OH = orgHeader.PK;
			creditNote.AH_LocalExTaxAmount = sum;
			creditNote.AH_OSExTaxAmount = sum;

			InvoicingLineBase creditNoteLine = (InvoicingLineBase)creditNote.Lines.AddNew();
			creditNoteLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			creditNoteLine.AL_LocalExTaxAmount = sum;
			creditNoteLine.AL_OSExTaxAmount = sum;
			creditNote.AH_FullyPaidDate = ZDateTime.Empty;

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = orgHeader.PK;
			invoice.AH_LocalExTaxAmount = sum;
			invoice.AH_OSExTaxAmount = sum;

			InvoicingLineBase invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			invoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			invoiceLine.AL_LocalExTaxAmount = sum;
			invoiceLine.AL_OSExTaxAmount = sum;
			invoice.AH_FullyPaidDate = ZDateTime.Empty;

			Factory.Save();

			return orgHeader;
		}

		#endregion

		#region RecordErrorForReportingWithCriticalValidation

		public void TestRecordAdditionalInformation_BalanceIsNotZero()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.NonZeroOutstandingAmountOnMiscTransaction);

			var orgHeader = SetupDataForTestWithCreditNoteAndInvoice();

			var matchingBase = new ARMatchingBase(Factory);
			matchingBase.PrimaryOrganization = orgHeader.PK;

			matchingBase.MoveAllFromUnmatchToMatch();
			AssertEquals(2, matchingBase.MatchedTransactions.Count);

			matchingBase.RunPreSaveValidation();
			Assert("Matching base should have no errors.", !matchingBase.HasErrors);

			var testARInvoice = Factory.NewWithValidTestData<ARInvoice>();
			TestObjectCreator.CreateInvoiceLine(testARInvoice, testARInvoice.TransactionCurrency, 1m, 0m);
			testARInvoice.AH_OH = orgHeader.PK;
			testARInvoice.AH_LocalExTaxAmount = 30M;
			testARInvoice.AH_OSTotalAmount = 30M;
			((IMatching)testARInvoice).OSPartialPaymentAmount = 30M;

			matchingBase.AddIMatching(testARInvoice);

			var exchangeDifference = (ExchangeDifference)matchingBase.GetMiscellaneousTransaction(TransactionTypes.ExchangeDifference);
			matchingBase.AddIMatching(exchangeDifference);

			testARInvoice.AH_LocalExTaxAmount = 20M;
			testARInvoice.AH_OSTotalAmount = 20M;
			((IMatching)testARInvoice).OSPartialPaymentAmount = 20M;

			matchingBase.Match_ForTestOnly();

			var actualMessage = CriticalValidationInfoCollectorService.GetOrCreateService(Factory)
				.GetInfo(exchangeDifference.PK, CriticalValidationInfoCollectorServiceKeyType.NonZeroOutstandingAmountOnMiscTransaction);
			AssertNotNullOrEmpty("Information should be collected by CriticalValidationInfoCollectorService against the Misc transaction", actualMessage);

			var expectedMessage = "Balance is not zero";
			AssertContains(expectedMessage, actualMessage);
		}

		public void TestRecordAdditionalInformation_TransactionAlreadyPaid()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.NonZeroOutstandingAmountOnMiscTransaction);

			var orgHeader = SetupDataForTestWithCreditNoteAndInvoice();

			var matchingBase = new ARMatchingBase(Factory);
			matchingBase.PrimaryOrganization = orgHeader.PK;

			matchingBase.MoveAllFromUnmatchToMatch();
			AssertEquals(2, matchingBase.MatchedTransactions.Count);

			matchingBase.RunPreSaveValidation();
			Assert("Matching base should have no errors.", !matchingBase.HasErrors);

			var testARInvoice = Factory.NewWithValidTestData<ARInvoice>();
			TestObjectCreator.CreateInvoiceLine(testARInvoice, testARInvoice.TransactionCurrency, 1m, 0m);
			testARInvoice.AH_OH = orgHeader.PK;
			testARInvoice.AH_LocalExTaxAmount = 30M;
			testARInvoice.AH_OSTotalAmount = 30M;
			((IMatching)testARInvoice).OSPartialPaymentAmount = 30M;

			matchingBase.AddIMatching(testARInvoice);

			var exchangeDifference = (ExchangeDifference)matchingBase.GetMiscellaneousTransaction(TransactionTypes.ExchangeDifference);
			matchingBase.AddIMatching(exchangeDifference);

			testARInvoice.AH_OutstandingAmount = 0M;

			matchingBase.Match_ForTestOnly();

			var actualMessage = CriticalValidationInfoCollectorService.GetOrCreateService(Factory)
				.GetInfo(exchangeDifference.PK, CriticalValidationInfoCollectorServiceKeyType.NonZeroOutstandingAmountOnMiscTransaction);
			AssertNotNullOrEmpty("Information should be collected by CriticalValidationInfoCollectorService against the Misc transaction", actualMessage);

			var expectedMessage = "Transaction already paid";
			AssertContains(expectedMessage, actualMessage);
		}

		public void TestRecordAdditionalInformation_InvalidBranchDeptCombo()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.NonZeroOutstandingAmountOnMiscTransaction);

			AccountingConfigurationRegistry.Instance.ClearingJournalClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ClearingJournalConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ClearingJournalConfigurationTypes.HeaderBranch.Code);

			var orgHeader = SetupDataForTestWithCreditNoteAndInvoice();

			var matchingBase = new ARMatchingBase(Factory);
			matchingBase.PrimaryOrganization = orgHeader.PK;

			matchingBase.MoveAllFromUnmatchToMatch();
			AssertEquals(2, matchingBase.MatchedTransactions.Count);

			matchingBase.RunPreSaveValidation();
			Assert("Matching base should have no errors.", !matchingBase.HasErrors);

			var testARInvoice = Factory.NewWithValidTestData<ARInvoice>();
			TestObjectCreator.CreateInvoiceLine(testARInvoice, testARInvoice.TransactionCurrency, 1m, 0m);
			testARInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			testARInvoice.AH_LocalExTaxAmount = 30M;
			testARInvoice.AH_OSTotalAmount = 30M;
			((IMatching)testARInvoice).OSPartialPaymentAmount = 30M;

			matchingBase.AddIMatching(testARInvoice);

			var exchangeDifference = (ExchangeDifference)matchingBase.GetMiscellaneousTransaction(TransactionTypes.ExchangeDifference);
			matchingBase.AddIMatching(exchangeDifference);

			var currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			GlbBranchCombinationValidationTest.SetAllowedBranchDepartmentCombinations(currentBranch, new GlbDepartment[] { TestObjectCreator.NonCurrentDepartment });

			matchingBase.Match_ForTestOnly();

			var actualMessage = CriticalValidationInfoCollectorService.GetOrCreateService(Factory)
				.GetInfo(exchangeDifference.PK, CriticalValidationInfoCollectorServiceKeyType.NonZeroOutstandingAmountOnMiscTransaction);
			AssertNotNullOrEmpty("Information should be collected by CriticalValidationInfoCollectorService against the Misc transaction", actualMessage);

			AssertContains("There are matching errors:", actualMessage);
			AssertContains("To change this configuration, set up the Branch/Department Combinations in the Edit Branch Window > Departments Tab.", actualMessage);
		}

		#endregion

		#region Validation Tests

		public void TestOSOverpaymentAmount()
		{
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			Overpayment testOVP = (Overpayment)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Overpayment);
			testOVP.AH_OSTotal = 90M;
			TestMatchingBase.OverpaymentBizO_ForTestOnly = testOVP;
			AssertEquals("OS Overpayment amount should be 90", 90M, TestMatchingBase.OSOverpaymentAmount);
		}

		public void TestValidatePrimaryOrganization()
		{
			SetUpTestDataSet();
			Factory.Save();
			TestMatchingBase.PrimaryOrganization = ZGuid.Empty;
			Assert("Primary Org cannot be empty", TestMatchingBase.PrimaryOrganizationInfo.HasErrors());
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			Assert("Primary Org should not have errors", !TestMatchingBase.PrimaryOrganizationInfo.HasErrors());
		}

		public void TestValidateBalance()
		{
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			TestARInvoice1 = Factory.NewWithValidTestData<ARInvoice>();

			Factory.Save();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			Assert("Balance should be 0, no errors", !TestMatchingBase.BalanceInfo.HasErrors());

			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_LocalExTaxAmount = 30M;
			TestARInvoice1.AH_OSTotalAmount = 30M;
			((IMatching)TestARInvoice1).OSPartialPaymentAmount = 30M;

			TestMatchingBase.AddIMatching(TestARInvoice1);
			Assert("Balance not 0, should have error", TestMatchingBase.BalanceInfo.HasErrors());

			using (TestMatchingBase.ValidateBalanceSuspender.GetSuspender())
			{
				TestMatchingBase.ValidateBalance();
			}
			Assert("Balance not 0 and ValidateBalanceSuspender is IsSuspended, no error", !TestMatchingBase.BalanceInfo.HasErrors());

			Discount testDSC = (Discount)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount);
			TestMatchingBase.AddIMatching(testDSC);
			Assert("Balance 0, no errors", !TestMatchingBase.BalanceInfo.HasErrors());

			string expectedError = "Accounting -> General Ledger Defaults -> Link Account -> Clearing Journal Clearing Account registry item can't be empty while Accounting -> Matching -> Clearing Journal Configuration registry item is set to create Clearing Journals.";
			AccountingConfigurationRegistry.Instance.ClearingJournalConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ClearingJournalConfigurationTypes.Standard.Code);
			TestMatchingBase.ValidateBalance();
			AssertNoError(TestMatchingBase.BalanceInfo, expectedError);

			foreach (CodeDescriptionPair codeDescPair in new AccountingConstants.ClearingJournalConfigurationTypes())
			{
				if (codeDescPair.Code == AccountingConstants.ClearingJournalConfigurationTypes.Standard.Code)
				{
					continue;
				}

				AccountingConfigurationRegistry.Instance.ClearingJournalConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ClearingJournalConfigurationTypes.LineBranchPerHeader.Code);
				TestMatchingBase.ValidateBalance();
				AssertHasError(string.Format("Should be an error, when ClearingJournalConfiguration is set to {0}", codeDescPair.Code), TestMatchingBase.BalanceInfo, expectedError);
			}
		}

		public virtual void TestMatchingMultiplePaymentsDoesNotAddRowValidationErrors()
		{
			RefCurrency testCurrency1 = Factory.NewWithValidTestData<RefCurrency>();
			AccBankAccount testBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			testBankAccount.AB_RX_NKAccountCurrency = testCurrency1.RX_Code;
			OrgHeader testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.OH_IsCreditor = true;

			Factory.Save();

			APPayment payment1 = Factory.NewWithValidTestData<APPayment>();
			payment1.AH_OH = testOrg1.PK;
			payment1.AH_AB = testBankAccount.PK;
			payment1.AH_RX_NKTransactionCurrency = testCurrency1.RX_Code;
			payment1.AH_LocalOutstandingAmount = 30M;
			payment1.AH_ExchangeRate = 1M;
			payment1.AH_ReceiptType = ReceiptTypes.Cash;
			payment1.AH_ChequeOrReference = "ABC123";

			APPayment payment2 = Factory.NewWithValidTestData<APPayment>();
			payment2.AH_OH = testOrg1.PK;
			payment2.AH_AB = testBankAccount.PK;
			payment2.AH_RX_NKTransactionCurrency = testCurrency1.RX_Code;
			payment2.AH_LocalOutstandingAmount = 70M;
			payment2.AH_ExchangeRate = 5M;
			payment2.AH_ReceiptType = ReceiptTypes.Cash;
			payment2.AH_ChequeOrReference = "XYZ987";

			Factory.Save();

			TestMatchingBase.PrimaryOrganization = testOrg1.PK;
			TestMatchingBase.UnmatchedTransactions.Load();

			TestMatchingBase.MatchedTransactions.RemoveAll();
			int initialMatchedTransactionsCount = TestMatchingBase.MatchedTransactions.Count;
			TestMatchingBase.MatchedTransactions.Add(payment1);
			TestMatchingBase.MatchedTransactions.Add(payment2);
			AssertEquals("MatchedTransactions count should be increased by 2", initialMatchedTransactionsCount + 2, TestMatchingBase.MatchedTransactions.Count);
			Assert("First Payment must not have error", !TestMatchingBase.MatchedTransactions[initialMatchedTransactionsCount].HasErrors());
			Assert("Second Payment must not have error", !TestMatchingBase.MatchedTransactions[initialMatchedTransactionsCount + 1].HasErrors());
		}

		public virtual void TestValidateRows()
		{
			RefCurrency testCurrency1 = Factory.NewWithValidTestData<RefCurrency>();
			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_RX_NKAccountCurrency = testCurrency1.RX_Code;
			OrgHeader testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.OH_IsDebtor = true;
			testOrg1.OH_IsCreditor = true;
			Factory.Save();

			APPayment payment1 = Factory.NewWithValidTestData<APPayment>();
			payment1.AH_OH = testOrg1.PK;
			payment1.AH_AB = bankAccount.PK;
			payment1.AH_RX_NKTransactionCurrency = testCurrency1.RX_Code;
			payment1.AH_ExchangeRate = 1M;
			payment1.AH_OSExTaxAmount = 30m;
			payment1.AH_LocalOutstandingAmount = 30M;
			payment1.AH_ReceiptType = ReceiptTypes.Cash;
			payment1.AH_ChequeOrReference = "123";

			APPayment payment2 = Factory.NewWithValidTestData<APPayment>();
			payment2.AH_OH = testOrg1.PK;
			payment2.AH_AB = bankAccount.PK;
			payment2.AH_RX_NKTransactionCurrency = testCurrency1.RX_Code;
			payment2.AH_ExchangeRate = 5M;
			payment2.AH_OSExTaxAmount = 350M;
			payment2.AH_LocalOutstandingAmount = 70M;
			payment2.AH_ReceiptType = ReceiptTypes.Cash;
			payment2.AH_ChequeOrReference = "321";

			Factory.Save();

			TestMatchingBase.PrimaryOrganization = testOrg1.PK;

			TestMatchingBase.UnmatchedTransactions.Load();

			TestMatchingBase.MatchedTransactions.Add(payment1);
			TestMatchingBase.MatchedTransactions.Add(payment2);
			AssertEquals("MatchedTransactions must contain two payments", 2, TestMatchingBase.MatchedTransactions.Count);
			Assert("First Payment must not have error", !TestMatchingBase.MatchedTransactions[0].HasErrors());
			Assert("Second Payment must not have error", !TestMatchingBase.MatchedTransactions[1].HasErrors());

			TestMatchingBase.MatchedTransactions.Remove(payment1);
			AssertEquals("MatchedTransactions must contain one payment", 1, TestMatchingBase.MatchedTransactions.Count);
			Assert("First Payment must not have error because it is only one in MatchedTransactions", !TestMatchingBase.MatchedTransactions[0].HasErrors());

			TestMatchingBase.MatchedTransactions.RemoveAll();
			TestMatchingBase.LoadedTransactions_ForTestOnly.Add(payment1);
			TestMatchingBase.LoadedTransactions_ForTestOnly.Add(payment2);
			TestMatchingBase.LoadTransactionsMatchingTheFilterCore_ForTestOnly(new ZQuery(AccTransactionHeaderSchema.AH_ExchangeRate, 1M).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			TestMatchingBase.MoveFilterMatchingTransactionsToSelected();
			AssertEquals("MatchedTransactions must contain one payment", 1, TestMatchingBase.MatchedTransactions.Count);
			Assert("First Payment must not have error because it is only one in MatchedTransactions", !TestMatchingBase.MatchedTransactions[0].HasErrors());

			TestMatchingBase.LoadTransactionsMatchingTheFilterCore_ForTestOnly(new ZQuery(AccTransactionHeaderSchema.AH_ExchangeRate, 5M).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			TestMatchingBase.MoveFilterMatchingTransactionsToSelected();
			AssertEquals("MatchedTransactions must contain two payments", 2, TestMatchingBase.MatchedTransactions.Count);
			Assert("First Payment must not have error", !TestMatchingBase.MatchedTransactions[0].HasErrors());
			Assert("Second Payment must not have error", !TestMatchingBase.MatchedTransactions[1].HasErrors());
		}

		public void TestExchangeRateAmount()
		{
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			Overpayment testOVP = (Overpayment)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Overpayment);
			testOVP.AH_ExchangeRate = 3.57M;
			TestMatchingBase.OverpaymentBizO_ForTestOnly = testOVP;
			AssertEquals("Exchange rate amount should be 3.57", 3.57M, TestMatchingBase.ExchangeRateAmount);
		}

		public void TestValidateMatchDate()
		{
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			TestARInvoice1 = Factory.NewWithValidTestData<ARInvoice>();

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			SetMatchDateSecurity(false);
			Assert("A user have rights on changing MatchDate", !TestMatchingBase.MatchDateInfo.ReadOnly);
			SetMatchDateSecurity(true);
			Assert("A user have rights on changing MatchDate", !TestMatchingBase.MatchDateInfo.ReadOnly);

			TestMatchingBase.MatchDate = ZDateTime.Now;
			TestMatchingBase.ValidateMatchDate();
			Assert("MatchDate is not valid.", TestMatchingBase.MatchDateInfo.HasErrors());

			Period testPeriod = Factory.NewWithValidTestData<Period>();
			testPeriod.AM_Year = (ZShort)ZDateTime.Now.Year;
			testPeriod.AM_StartDate = new ZDateTime(testPeriod.AM_Year, ZDateTime.Today.Month, 1);
			testPeriod.AM_EndDate = testPeriod.AM_StartDate.AddMonths(1).AddDays(-1);

			TestMatchingBase.ValidateMatchDate();
			Assert("MatchDate is valid.", !TestMatchingBase.MatchDateInfo.HasErrors());
			AssertEquals("MatchDate should be without time.", TestMatchingBase.MatchDate.Date, TestMatchingBase.MatchDate);

			TestMatchingBase.MatchDate = ZDateTime.Now.AddDays(1);
			Assert("MatchDate is not valid.", TestMatchingBase.MatchDateInfo.HasErrors());
			TestMatchingBase.MatchDateInfo.ClearAllNotifications();

			testPeriod.AM_IsSubLedgerClosed = true;
			TestMatchingBase.MatchDate = ZDateTime.Now;
			Assert("MatchDate is not valid.", TestMatchingBase.MatchDateInfo.HasErrors());
			TestMatchingBase.MatchDateInfo.ClearAllNotifications();

			testPeriod.AM_IsSubLedgerClosed = false;
			testPeriod.AM_IsGeneralLedgerClosed = true;
			TestMatchingBase.ValidateMatchDate();
			Assert("MatchDate is not valid.", TestMatchingBase.MatchDateInfo.HasErrors());
			TestMatchingBase.MatchDateInfo.ClearAllNotifications();

			testPeriod.AM_IsSubLedgerClosed = true;
			testPeriod.AM_IsGeneralLedgerClosed = true;
			TestMatchingBase.ValidateMatchDate();
			Assert("MatchDate is not valid.", TestMatchingBase.MatchDateInfo.HasErrors());
			TestMatchingBase.MatchDateInfo.ClearAllNotifications();

			testPeriod.AM_IsSubLedgerClosed = false;
			testPeriod.AM_IsGeneralLedgerClosed = false;

			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_PostDate = TestMatchingBase.MatchDate.AddDays(2);

			TestMatchingBase.AddIMatching(TestARInvoice1);
			TestMatchingBase.ValidateMatchDate();
			Assert("MatchDate is not valid.", TestMatchingBase.MatchDateInfo.HasErrors());
			TestMatchingBase.MatchDateInfo.ClearAllNotifications();

			TestARInvoice1.AH_PostDate = TestMatchingBase.MatchDate.AddDays(-2);

			TestMatchingBase.ValidateMatchDate();
			Assert("MatchDate is valid.", !TestMatchingBase.MatchDateInfo.HasErrors());
			TestMatchingBase.MatchDateInfo.ClearAllNotifications();

			TestARInvoice1.AH_PostDate = TestMatchingBase.MatchDate.AddHours(23).AddMinutes(59).AddSeconds(59);

			TestMatchingBase.ValidateMatchDate();
			Assert("MatchDate is valid.", !TestMatchingBase.MatchDateInfo.HasErrors());
			TestMatchingBase.MatchDateInfo.ClearAllNotifications();

			TestARInvoice1.AH_PostDate = TestMatchingBase.MatchDate.AddHours(24);

			TestMatchingBase.ValidateMatchDate();
			Assert("MatchDate is not valid.", TestMatchingBase.MatchDateInfo.HasErrors());
			TestMatchingBase.MatchDateInfo.ClearAllNotifications();
		}

		public void TestValidateMatchDate_Future()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			TestMatchingBase.PrimaryOrganization = TestObjectCreator.AALSHI.PK;
			TestMatchingBase.MatchDate = ZDateTime.Today.AddDays(1);
			TestMatchingBase.ValidateMatchDate();
			AssertHasError("Because registry value is false", TestMatchingBase.MatchDateInfo, AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);

			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();
			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;

			Factory.Save();

			TestMatchingBase.PrimaryOrganization = TestObjectCreator.AALSHI.PK;
			TestMatchingBase.MatchDate = ZDateTime.Today.AddDays(1);
			TestMatchingBase.ValidateMatchDate();

			// With APPaymentApprovalMatching the future date txn is already in the matched list, so these particular tests do not apply...
			bool doCheckWhatsInMatchedTransactionTests = !(TestMatchingBase is APPaymentApprovalMatching || TestMatchingBase is ARPaymentApprovalMatching);
			string expectedError = AccountingConstants.FuturePostingErrorMessages.UserHasNoSecurity;

			if (doCheckWhatsInMatchedTransactionTests)
			{
				AssertHasError("No future-post-date transactions in matched transactions list, so don't allow future date.", TestMatchingBase.MatchDateInfo, expectedError);

				if (TestMatchingBase is ARMatchingBase)
				{
					var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "1", TestObjectCreator.AUD, 1, 100, 0, 100, 0, TestObjectCreator.AALSHI, ZGuid.Empty);
					TestMatchingBase.UnmatchedTransactions.Add(arInvoice);
				}
				else if (TestMatchingBase is APMatchingBase)
				{
					var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "2", TestObjectCreator.AUD, 1, 100, 0, 100, 0, TestObjectCreator.AALSHI, ZGuid.Empty);
					TestMatchingBase.UnmatchedTransactions.Add(apInvoice);
				}
				else
				{
					AssertEquals("Precondition", 1, TestMatchingBase.MatchedTransactions.Count);
				}

				TestMatchingBase.MoveAllFromUnmatchToMatch();
				AssertEquals("MatchedTransactions has one txn", 1, TestMatchingBase.MatchedTransactions.Count);
				TestMatchingBase.ValidateMatchDate();
				AssertHasError("No future-post-date transactions in matched transactions list (still in unmatched list), so don't allow future date.", TestMatchingBase.MatchDateInfo, expectedError);

				TransactionHeader receiptOrPayment;
				if (this.LedgerType == LedgerTypes.AccountsReceivable)
				{
					receiptOrPayment = TestObjectCreator.CreateReceiptOrPayment(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 50, TestObjectCreator.AUDBankAccount.PK);
					receiptOrPayment.AH_OH = TestObjectCreator.AALSHI.PK;
					TestMatchingBase.UnmatchedTransactions.Add(receiptOrPayment);
				}
				else
				{
					receiptOrPayment = TestObjectCreator.CreateReceiptOrPayment(ReceiptTypes.Cash, TransactionTypes.Payment, LedgerTypes.AccountsPayable, 50, TestObjectCreator.AUDBankAccount.PK);
					receiptOrPayment.AH_OH = TestObjectCreator.AALSHI.PK;
					TestMatchingBase.UnmatchedTransactions.Add(receiptOrPayment);
				}

				TestMatchingBase.MoveAllFromUnmatchToMatch();
				Assert(TestMatchingBase.MatchedTransactions.Contains(receiptOrPayment));
			}

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;
			TestMatchingBase.ValidateMatchDate();
			AssertNoError("Future-post-date transactions in matched transactions list, so  allow future date.", TestMatchingBase.MatchDateInfo, expectedError);

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;
			TestMatchingBase.ValidateMatchDate();
			AssertHasError("Security denied, so don't allow future date", TestMatchingBase.MatchDateInfo, expectedError);
		}

		public void TestValidateMatchDate_NewBeforeExisting()
		{
			if ((TestMatchingBase is APMatchingBase) || (TestMatchingBase is ARMatchingBase))
			{
				new AccountingPeriodTestHelper(Factory).SetupPeriods();
				TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
				TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
				TestMatchingBase.MatchDate = ZDateTime.Today.AddDays(-1);

				var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "2", TestObjectCreator.AUD, 1, 100, 0, 100, 0, TestOrg1, ZGuid.Empty);
				arInvoice.AH_PostDate = ZDateTime.Today.AddDays(-5);
				TestMatchingBase.UnmatchedTransactions.Add(arInvoice);

				var receiptOrPayment = TestObjectCreator.CreateReceiptOrPayment(ReceiptTypes.Cash, TransactionTypes.Payment, LedgerTypes.AccountsPayable, 50, TestObjectCreator.AUDBankAccount.PK);
				receiptOrPayment.AH_OH = TestOrg1.PK;
				receiptOrPayment.AH_PostDate = ZDateTime.Today.AddDays(-5);
				TestMatchingBase.UnmatchedTransactions.Add(receiptOrPayment);

				TransactionMatchLink oldMatchLink = ((IMatching)arInvoice).CurrentMatchGroup.AddNew();
				oldMatchLink.AP_Amount = 0M;
				oldMatchLink.AP_MatchGroupNum = "M00001001";
				oldMatchLink.AP_AH = arInvoice.PK;
				oldMatchLink.AP_MatchDate = ZDateTime.Today.AddDays(-1);

				TestMatchingBase.MoveAllFromUnmatchToMatch();

				TestMatchingBase.ValidateMatchDate();
				AssertNoErrors(TestMatchingBase.MatchDateInfo);

				Factory.Save();

				var newMatching = GetTestMatchingBaseInNewFactory(Factory);
				newMatching.MatchDate = ZDateTime.Today.AddDays(-2);
				newMatching.UnmatchedTransactions.Add(arInvoice);
				newMatching.UnmatchedTransactions.Add(receiptOrPayment);

				TransactionMatchLink newMatchLink = ((IMatching)arInvoice).CurrentMatchGroup.AddNew();
				newMatchLink.AP_Amount = 0M;
				newMatchLink.AP_MatchGroupNum = "M00001002";
				newMatchLink.AP_AH = arInvoice.PK;
				newMatchLink.AP_MatchDate = ZDateTime.Today.AddDays(-3);

				newMatching.MoveAllFromUnmatchToMatch();

				newMatching.ValidateMatchDate();
				var expectedMessage = @"You are attempting to use a match date which is earlier than a previous match date recorded against one of the transactions contained in this matching.
Please use a match date equal to, or later than " + ZDateTime.Today.AddDays(-1) + ".";
				AssertHasErrors(expectedMessage, newMatching.MatchDateInfo);
			}
			else
			{
				Assert(true);
			}
		}

		void SetMatchDateSecurity(ZBool value)
		{
			if (TestMatchingBase.LedgerTypeCore_ForTestOnly == ZArchitecture.Core.LedgerTypes.AccountsPayable)
			{
				Env.Security.PayablesAllowMatchDateToBeBackDated.IsAllowed = value;
			}
			if (TestMatchingBase.LedgerTypeCore_ForTestOnly == ZArchitecture.Core.LedgerTypes.AccountsReceivable)
			{
				Env.Security.ReceivablesAllowMatchDateToBeBackDated.IsAllowed = value;
			}
		}

		[TestDate(2013, 10, 17)]
		public void TestValidatesPostDateOnAppropriateActions()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			TestMatchingBase.MatchDate = ZDateTime.Today.AddDays(1);
			SetUpTestDataSet();
			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_LocalExTaxAmount = 10M;
			TestARInvoice1.AH_OSTotalAmount = 10M;
			TestARInvoice1.AH_ExchangeRate = 1M;
			TestARInvoice1.AH_OutstandingAmount = 10M;

			TestMatchingBase.UnmatchedTransactions.Add(TestARInvoice1);

			Dictionary<BusinessObject, ZDecimal> selected = new Dictionary<BusinessObject, ZDecimal>();
			selected.Add(TestARInvoice1, 5m);

			TestMatchingBase.MatchDateInfo.ClearAllNotifications();
			AssertNoError(TestMatchingBase.MatchDateInfo, AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);
			TestMatchingBase.MoveFromUnmatchToMatch(selected);
			AssertHasError(TestMatchingBase.MatchDateInfo, AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);

			TestMatchingBase.MatchDateInfo.ClearAllNotifications();
			AssertNoError(TestMatchingBase.MatchDateInfo, AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);
			TestMatchingBase.MoveFromMatchToUnmatch(new[] { TestARInvoice1 });
			AssertHasError(TestMatchingBase.MatchDateInfo, AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);

			TestMatchingBase.MatchDateInfo.ClearAllNotifications();
			AssertNoError(TestMatchingBase.MatchDateInfo, AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);
			TestMatchingBase.MoveAllFromUnmatchToMatch();
			AssertHasError(TestMatchingBase.MatchDateInfo, AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);

			TestMatchingBase.MatchDateInfo.ClearAllNotifications();
			AssertNoError(TestMatchingBase.MatchDateInfo, AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);
			TestMatchingBase.MoveAllFromMatchToUnmatch();
			AssertHasError(TestMatchingBase.MatchDateInfo, AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);

			TestMatchingBase.MatchDateInfo.ClearAllNotifications();
			AssertNoError(TestMatchingBase.MatchDateInfo, AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);
			TestMatchingBase.PrimaryOrganization = TestObjectCreator.ABIGAS.PK;
			AssertHasError(TestMatchingBase.MatchDateInfo, AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);
		}

		#endregion

		#region Balancing Journals Testing

		public void TestCopyJournalWithOppositeAmount()
		{
			AccGLHeader controlAccount = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "DENTEST";

			APInvoice apInvoice = Factory.NewWithValidTestData<APInvoice>();
			ARInvoice arInvoice = Factory.NewWithValidTestData<ARInvoice>();

			Journal journal1 = Factory.NewWithValidTestData<APJournal>();
			journal1.AH_OH = org1.PK;
			journal1.AH_RX_NKTransactionCurrency = "EUR";
			journal1.AH_ExchangeRate = 0.5M;
			journal1.AH_OSExTaxAmount = 400M;
			journal1.AH_AG = controlAccount.PK;
			journal1.RelatedInvoice = apInvoice;

			AssertNotEquals("AH_OSExTaxAmount should not be zero", 0M, journal1.AH_OSExTaxAmount);
			Journal journal1Copy = TestMatchingBase.CopyJournalWithOppositeAmount(journal1);
			AssertJournalCopiedWithOppositeAmount(journal1, journal1Copy);

			Journal journal2 = Factory.NewWithValidTestData<ARJournal>();
			journal2.AH_OH = org1.PK;
			journal2.AH_RX_NKTransactionCurrency = "USD";
			journal2.AH_ExchangeRate = 0.9M;
			journal2.AH_OSExTaxAmount = 900M;
			journal2.AH_OSTotal = 900M;
			journal2.AH_InvoiceAmount = 1000M;
			journal2.AH_OutstandingAmount = 1000M;
			journal2.RelatedInvoice = arInvoice;
			journal2.IsAutoGenerated = true;

			AssertNotEquals("AH_OSExTaxAmount should not be zero", 0M, journal2.AH_OSExTaxAmount);
			Journal journal2Copy = TestMatchingBase.CopyJournalWithOppositeAmount(journal2);
			AssertJournalCopiedWithOppositeAmount(journal2, journal2Copy);
		}

		void AssertJournalCopiedWithOppositeAmount(Journal original, Journal copied)
		{
			AssertEquals(original.AH_Ledger, copied.AH_Ledger);
			AssertEquals(original.AH_OH, copied.AH_OH);
			AssertEquals(original.AH_InvoiceDate, copied.AH_InvoiceDate);
			AssertEquals(original.AH_PostDate, copied.AH_PostDate);
			AssertEquals(original.AH_DueDate, copied.AH_DueDate);
			AssertEquals(original.AH_Desc, copied.AH_Desc);
			AssertEquals(original.AH_ChequeOrReference, copied.AH_ChequeOrReference);
			AssertEquals(original.AH_RX_NKTransactionCurrency, copied.AH_RX_NKTransactionCurrency);
			AssertEquals(original.AH_ExchangeRate, copied.AH_ExchangeRate);
			AssertEquals(original.AH_TransactionCategory, copied.AH_TransactionCategory);
			AssertEquals(original.AH_AG, copied.AH_AG);
			AssertEquals(original.DebitCreditSign, copied.GetOppositeDebitCreditSign(copied.DebitCreditSign));
			AssertEquals(original.AH_OSExTaxAmount, copied.AH_OSExTaxAmount);
			AssertEquals(original.RelatedInvoice, copied.RelatedInvoice);
			AssertEquals(original, copied.RelatedJournal);
			AssertEquals(original.RelatedJournal, copied);
			AssertEquals(original.IsAutoGenerated, copied.IsAutoGenerated);
		}

		public void TestCopyJournalWithOppositeAmountWithMultipleSubAccounts()
		{
			AccGLHeader controlAccount = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			TestObjectCreator.CreateGLHeaderSubAccount(controlAccount, OrgHeaderSchema.Constants.Prefix, false);
			TestObjectCreator.CreateGLHeaderSubAccount(controlAccount, AccGroupsSchema.Constants.Prefix, false);
			TestObjectCreator.CreateGLHeaderSubAccount(controlAccount, GlbStaffSchema.Constants.Prefix, true);
			TestObjectCreator.CreateGLHeaderSubAccount(controlAccount, GlbGroupSchema.Constants.Prefix, false);
			Factory.Save();

			Journal journal = Factory.NewWithValidTestData<APJournal>();
			journal.AH_AG = controlAccount.PK;

			var subAccounts = journal.SubAccounts.Cast<ISupportSubAccount>();
			subAccounts.First(x => x.SubAccountTypeParentTableCode == OrgHeaderSchema.Constants.Prefix).SubAccountParentId = TestObjectCreator.ABIGAS.PK;
			subAccounts.First(x => x.SubAccountTypeParentTableCode == AccGroupsSchema.Constants.Prefix).SubAccountParentId = TestObjectCreator.AR1.PK;
			subAccounts.First(x => x.SubAccountTypeParentTableCode == GlbGroupSchema.Constants.Prefix).SubAccountParentId = TestObjectCreator.GG1.PK;

			Journal journalCopy = TestMatchingBase.CopyJournalWithOppositeAmount(journal);
			var subAccountsCopy = journal.RelatedJournal.SubAccounts.Cast<ISupportSubAccount>();
			AssertEquals(4, subAccountsCopy.Count());
			Assert(subAccountsCopy.Any(x => x.SubAccountTypeParentTableCode == OrgHeaderSchema.Constants.Prefix && x.SubAccountParentId == TestObjectCreator.ABIGAS.PK));
			Assert(subAccountsCopy.Any(x => x.SubAccountTypeParentTableCode == AccGroupsSchema.Constants.Prefix && x.SubAccountParentId == TestObjectCreator.AR1.PK));
			Assert(subAccountsCopy.Any(x => x.SubAccountTypeParentTableCode == GlbGroupSchema.Constants.Prefix && x.SubAccountParentId == TestObjectCreator.GG1.PK));
			Assert(subAccountsCopy.Any(x => x.SubAccountTypeParentTableCode == GlbStaffSchema.Constants.Prefix && x.SubAccountParentId == ZGuid.Empty));
			Assert(journal.NotificationsIncludingChildren.Contains("Error - AHS_SubClassParentId: Please enter a Sub Account."));
			Assert(journal.RelatedJournal.RelatedJournal.NotificationsIncludingChildren.Contains("Error - AHS_SubClassParentId: Please enter a Sub Account."));

			AssertEquals(journal, journalCopy.RelatedJournal);
			AssertEquals(TestObjectCreator.ABIGAS.PK, journal.RelatedJournal.AH_Calc_FirstSubClassParentId);
			journal.AH_Calc_FirstSubClassParentId = TestObjectCreator.AALSHI.PK;
			AssertEquals(TestObjectCreator.AALSHI.PK, journal.RelatedJournal.AH_Calc_FirstSubClassParentId);

			subAccounts.First(x => x.SubAccountTypeParentTableCode == GlbStaffSchema.Constants.Prefix).SubAccountParentId = TestObjectCreator.GS1.PK;
			Assert(subAccountsCopy.Any(x => x.SubAccountTypeParentTableCode == GlbStaffSchema.Constants.Prefix && x.SubAccountParentId == TestObjectCreator.GS1.PK));
			Assert(!journal.NotificationsIncludingChildren.Contains("Error - AHS_SubClassParentId: Please enter a Sub Account."));
			Assert(!journal.RelatedJournal.NotificationsIncludingChildren.Contains("Error - AHS_SubClassParentId: Please enter a Sub Account."));
		}

		public void TestCreateAndAddJournalsForMatching()
		{
			AccGLHeader controlAccount = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;

			APInvoice apInvoice = Factory.NewWithValidTestData<APInvoice>();
			ARInvoice arInvoice = Factory.NewWithValidTestData<ARInvoice>();

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "DENTEST";

			Journal journal1 = Factory.NewWithValidTestData<APJournal>();
			journal1.AH_OH = org1.PK;
			journal1.AH_RX_NKTransactionCurrency = "EUR";
			journal1.AH_ExchangeRate = 0.5M;
			journal1.AH_OSTotal = 400M;
			journal1.AH_InvoiceAmount = 800M;
			journal1.AH_OutstandingAmount = 800M;
			journal1.AH_AG = controlAccount.PK;
			journal1.RelatedInvoice = apInvoice;
			TestMatchingBase.AddToBalancingJournals(journal1);
			AssertEquals(1, TestMatchingBase.BalancingAPJournals.Count);

			Journal journal2 = Factory.NewWithValidTestData<ARJournal>();
			journal2.AH_OH = org1.PK;
			journal2.AH_RX_NKTransactionCurrency = "USD";
			journal2.AH_ExchangeRate = 0.9M;
			journal2.AH_OSTotal = 900M;
			journal2.AH_InvoiceAmount = 1000M;
			journal2.AH_OutstandingAmount = 1000M;
			journal2.RelatedInvoice = arInvoice;
			TestMatchingBase.AddToBalancingJournals(journal2);
			AssertEquals(1, TestMatchingBase.BalancingARJournals.Count);

			Journal journal3 = Factory.NewWithValidTestData<APJournal>();
			journal3.AH_OH = org1.PK;
			journal3.AH_RX_NKTransactionCurrency = "EUR";
			journal3.AH_ExchangeRate = 0.5M;
			journal3.AH_OSTotal = 400M;
			journal3.AH_InvoiceAmount = 800M;
			journal3.AH_OutstandingAmount = 800M;
			journal3.AH_AG = controlAccount.PK;
			journal3.RelatedInvoice = apInvoice;
			journal3.AH_TransactionCategory = Constants.TransactionCategory.Codes.PaymentBasisWithholding;
			TestMatchingBase.AddToBalancingJournals(journal3);
			AssertEquals(2, TestMatchingBase.BalancingAPJournals.Count);

			int count = TestMatchingBase.MatchedTransactions.Count;

			Dictionary<BusinessObject, ZDecimal> transactionsToMatch = new Dictionary<BusinessObject, ZDecimal>();
			TestMatchingBase.CreateAndAddJournalsForMatching(transactionsToMatch);

			AssertEquals(2, transactionsToMatch.Count);
			int index = 0;

			foreach (Journal journal in transactionsToMatch.Keys)
			{
				index++;
				Journal testJournal = index == 1 ? journal1 : journal2;
				AssertJournalCopiedWithOppositeAmount(testJournal, journal);
			}
			AssertEquals(2, index);

			index = 0;
			foreach (ZDecimal paidAmount in transactionsToMatch.Values)
			{
				AssertEquals(ZDecimal.Zero, paidAmount);
				index++;
			}
			AssertEquals(2, index);
		}

		public void TestRemoveAndDeleteBalancingJournalsFromMatchingTransactions()
		{
			AccGLHeader controlAccount = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "DENTEST";

			Journal journal1 = Factory.NewWithValidTestData<APJournal>();
			journal1.AH_OH = org1.PK;
			journal1.AH_RX_NKTransactionCurrency = "EUR";
			journal1.AH_ExchangeRate = 0.5M;
			journal1.AH_OSTotal = 400M;
			journal1.AH_InvoiceAmount = 800M;
			journal1.AH_OutstandingAmount = 800M;
			journal1.AH_AG = controlAccount.PK;
			Journal copiedJournal = TestMatchingBase.CopyJournalWithOppositeAmount(journal1);
			TestMatchingBase.MatchedTransactions.Add(journal1);

			TestMatchingBase.RemoveAndDeleteBalancingJournalsFromMatchingTransactions(new List<Journal> { journal1 });
			Assert(!TestMatchingBase.MatchedTransactions.Contains(journal1));
			Assert(journal1.IsDeleted);
			Assert(copiedJournal.IsDeleted);
		}

		public void TestRemoveAndDeleteLinkedToInvoiceBalancingJournals()
		{
			AccGLHeader controlAccount = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;

			APInvoice apInvoice = Factory.NewWithValidTestData<APInvoice>();

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "DENTEST";

			Journal journal1 = Factory.NewWithValidTestData<APJournal>();
			journal1.AH_OH = org1.PK;
			journal1.AH_RX_NKTransactionCurrency = "EUR";
			journal1.AH_ExchangeRate = 0.5M;
			journal1.AH_OSTotal = 400M;
			journal1.AH_InvoiceAmount = 800M;
			journal1.AH_OutstandingAmount = 800M;
			journal1.AH_AG = controlAccount.PK;
			journal1.RelatedInvoice = apInvoice;
			Journal copiedJournal = TestMatchingBase.CopyJournalWithOppositeAmount(journal1);

			TestMatchingBase.MatchedTransactions.Add(apInvoice);
			TestMatchingBase.MatchedTransactions.Add(journal1);

			TestMatchingBase.RemoveAndDeleteBalancingJournalsFromMatchingTransactions(TestMatchingBase.GetLinkedToInvoiceBalancingJournals(apInvoice));
			Assert(TestMatchingBase.MatchedTransactions.Contains(apInvoice));
			Assert(!TestMatchingBase.MatchedTransactions.Contains(journal1));
			Assert(journal1.IsDeleted);
			Assert(copiedJournal.IsDeleted);
		}

		public void TestOSPartialPaymentAmountInfo_ValueChanged_AskUserForConfirmationNotSubscribed()
		{
			TestMatchingBase.AskUserForConfirmation = null;

			APInvoice apInvoice = Factory.NewWithValidTestData<APInvoice>();
			apInvoice.AH_OutstandingAmount = -200m;
			TestMatchingBase.MatchedTransactions.Add(apInvoice);
			((IMatching)apInvoice).OSPartialPaymentAmount = -500m;

			TestMatchingBase.OSPartialPaymentAmountInfo_ValueChanged(apInvoice, null);

			Assert("Even if AskUserForConfirmation is not subscribed, Validate balance should be called", TestMatchingBase.BalanceInfo.HasErrors());
		}

		public void TestOSPartialPaymentAmountInfo_ValueChanged()
		{
			TestMatchingBase.AskUserForConfirmation += fAskUserForConfirmation;

			APInvoice apInvoice = Factory.NewWithValidTestData<APInvoice>();
			((IMatching)apInvoice).OSPartialPaymentAmount = -500m;
			apInvoice.AH_OutstandingAmount = -200m;
			apInvoice.AH_TransactionNum = "APINV55566";
			apInvoice.AH_ChequeOrReference = "CHQ229";
			int count = TestMatchingBase.MatchedTransactions.Count;
			TestMatchingBase.OSPartialPaymentAmountInfo_ValueChanged(apInvoice, null);

			AssertEquals(1 + count, TestMatchingBase.MatchedTransactions.Count);
			AssertEquals(1, TestMatchingBase.BalancingAPJournals.Count);
			Journal matchingJournal = (Journal)TestMatchingBase.MatchedTransactions[count];
			Journal balancingJournal = TestMatchingBase.BalancingAPJournals[0];

			AssertEquals(apInvoice.AH_Ledger, matchingJournal.AH_Ledger);
			AssertEquals(Constants.TransactionCategory.Codes.TransactionAlreadyPaid, matchingJournal.AH_TransactionCategory);
			AssertEquals(apInvoice.AH_OH, matchingJournal.AH_OH);
			AssertEquals("Transaction Already Paid: Trans. Num. APINV55566, Payment Reference: CHQ229".ToUpper(), matchingJournal.AH_Desc.ToUpper());
			AssertEquals(apInvoice.AH_RX_NKTransactionCurrency, matchingJournal.AH_RX_NKTransactionCurrency);
			AssertEquals(apInvoice.AH_ExchangeRate, matchingJournal.AH_ExchangeRate);
			AssertEquals(DebitCreditDataEntry.DR, matchingJournal.DebitCreditSign);
			AssertEquals(300m, matchingJournal.AH_OSExTaxAmount);
			AssertEquals("APINV55566", matchingJournal.AH_ChequeOrReference);
			AssertEquals(apInvoice, matchingJournal.RelatedInvoice);
			AssertJournalCopiedWithOppositeAmount(matchingJournal, balancingJournal);

			AssertEquals(-300m, ((IMatching)matchingJournal).OSPartialPaymentAmount);
			AssertEquals(-200m, ((IMatching)apInvoice).OSPartialPaymentAmount);

			((IMatching)apInvoice).OSPartialPaymentAmount = -600m;
			TestMatchingBase.OSPartialPaymentAmountInfo_ValueChanged(apInvoice, null);
			AssertEquals(-400m, ((IMatching)matchingJournal).OSPartialPaymentAmount);
			AssertEquals(-200m, ((IMatching)apInvoice).OSPartialPaymentAmount);
		}

		public void TestOSPartialPaymentAmountInfo_ValueChanged_CreatesJournalWithoutGLAccount_TriggeringCriticalValidationOnMatching()
		{
			//Arrange
			AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			AccountingConfigurationRegistry.Instance.ARMatchingSessionControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("Pre-condition: matching GL accounts in registry are empty", ZGuid.Empty, AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.Value);
			AssertEquals("Pre-condition: matching GL accounts in registry are empty", ZGuid.Empty, AccountingConfigurationRegistry.Instance.ARMatchingSessionControlAccount.Value);

			TestMatchingBase.AskUserForConfirmation += fAskUserForConfirmation;

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			((IMatching)apInvoice).OSPartialPaymentAmount = -123m;
			apInvoice.AH_TransactionNum = "GLTEST001";
			apInvoice.AH_ChequeOrReference = "GLTEST002";

			//Act
			TestMatchingBase.OSPartialPaymentAmountInfo_ValueChanged(apInvoice, null);
			try
			{
				Factory.Save(); //triggers the exception
			}
			//Assert
			catch (OnSavingCriticalCheckException ex)
			{
				AssertContains("Expected Critical Validation Error:", "Error Message: AP JNL with an empty GL account field", ex.Message);
				AssertEquals(nameof(CriticalValidationErrorType.TransactionWithEmptyGLAccountField_7), ex.ErrorType);

				var journal = GetJournalFromDB();
				AssertNotNull("partial payment amount should have triggered a journal to be created", journal);

				var expectedDeveloperMessage = $@"TransactionHeaderWithGLAccountThatShouldNotBeNull:
Transaction PK: {apInvoice.PK}
OS Partial Payment Amount: -123
OS Outstanding Amount: 0
Journal PK: {journal.PK}
Journal GL Account: {ZGuid.Empty}
Journal Transaction Category: TAP
Registry for APMatchingSessionControlAccount: {ZGuid.Empty}
Registry for ARMatchingSessionControlAccount: {ZGuid.Empty}
Is Validation Suspended on TransactionHeader: No
Journal Errors: Error - AH_AG: Please enter a GL Account.";

				AssertContains("Critical Validation Error", expectedDeveloperMessage, ex.DeveloperErrorMessage);
				AssertContains("IsValidationSuspended: False, IsDeleted: False, IsDeleting: False, Last Edit User: E", ex.DeveloperErrorMessage);
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		protected abstract Journal GetJournalFromDB();

		void fAskUserForConfirmation(object sender, UserQueryEventArgs e)
		{
			e.Response = true;
		}

		public void TestChangePrimaryOrganizationWillClearRelatedJournal()
		{
			SetUpTestDataSet();

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			var apJournal = TestMatchingBase.BalancingAPJournals.AddNew();
			var arJournal = TestMatchingBase.BalancingARJournals.AddNew();
			var relatedAPJournal = apJournal.RelatedJournal = Factory.New<APJournal>();
			var relatedARJournal = arJournal.RelatedJournal = Factory.New<ARJournal>();
			AssertEquals(1, TestMatchingBase.BalancingAPJournals.Count);
			AssertEquals(1, TestMatchingBase.BalancingARJournals.Count);
			AssertEquals(apJournal, TestMatchingBase.BalancingAPJournals[0]);
			AssertEquals(arJournal, TestMatchingBase.BalancingARJournals[0]);
			AssertEquals(relatedAPJournal, TestMatchingBase.BalancingAPJournals[0].RelatedJournal);
			AssertEquals(relatedARJournal, TestMatchingBase.BalancingARJournals[0].RelatedJournal);

			TestMatchingBase.PrimaryOrganization = TestOrg2.PK;
			AssertEquals(0, TestMatchingBase.BalancingAPJournals.Count);
			AssertEquals(0, TestMatchingBase.BalancingARJournals.Count);
			Assert(apJournal.IsDeleted);
			Assert(arJournal.IsDeleted);
			Assert(relatedAPJournal.IsDeleted);
			Assert(relatedARJournal.IsDeleted);
		}

		#endregion

		#region TestMainCurrencyFilter

		public virtual void TestMainCurrencyFilter()
		{
			RefCurrency testCurrency1 = Factory.NewWithValidTestData<RefCurrency>();
			RefCurrency testCurrency2 = Factory.NewWithValidTestData<RefCurrency>();

			SetUpTestDataSet();
			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_RX_NKTransactionCurrency = testCurrency1.RX_Code;
			TestARInvoice1.Lines[0].AL_OSExTaxAmount = 30M;

			TestARInvoice2.AH_OH = TestOrg1.PK;
			TestARInvoice2.AH_RX_NKTransactionCurrency = testCurrency1.RX_Code;
			TestARInvoice2.Lines[0].AL_OSExTaxAmount = 40M;

			TestARInvoice4.AH_OH = TestOrg1.PK;
			TestARInvoice4.AH_RX_NKTransactionCurrency = testCurrency2.RX_Code;
			TestARInvoice4.Lines[0].AL_OSExTaxAmount = 50M;

			TestARInvoice4_2.AH_OH = TestOrg2.PK;
			TestARInvoice4_2.AH_RX_NKTransactionCurrency = testCurrency1.RX_Code;
			TestARInvoice4_2.Lines[0].AL_OSExTaxAmount = 60M;

			Factory.Save();

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			AssertEquals("LoadedTransactions should contain 3 elements", 3, TestMatchingBase.LoadedTransactions_ForTestOnly.Count);
			Assert("LoadedTransactions should contain TestARInvoice1", TestMatchingBase.LoadedTransactions_ForTestOnly.Contains(TestARInvoice1));
			Assert("LoadedTransactions should contain TestARInvoice2", TestMatchingBase.LoadedTransactions_ForTestOnly.Contains(TestARInvoice2));
			Assert("LoadedTransactions should contain TestARInvoice4", TestMatchingBase.LoadedTransactions_ForTestOnly.Contains(TestARInvoice4));

			TestMatchingBase.MoveFromUnmatchToMatch(new BusinessObject[] { TestARInvoice1 });
			AssertEquals("UnmatchedTransactions should contain 2 elements", 2, TestMatchingBase.UnmatchedTransactions.Count);
			Assert("MatchedTransactions should contain TestARInvoice1", TestMatchingBase.MatchedTransactions.Contains(TestARInvoice1));

			var currencyFilter = (ModuleNkFilter)TestMatchingBase.MatchingFilterBizO[MatchingFilterBusinessObject.Currency];
			currencyFilter.IsActive = true;
			currencyFilter.Property = testCurrency2.RX_Code;
			TestMatchingBase.ReloadSettlementOrgTransactions();

			AssertEquals("UnmatchedTransactions should contain 1 element", 1, TestMatchingBase.UnmatchedTransactions.Count);
			Assert("UnmatchedTransactions should contain TestARReceipt4", TestMatchingBase.UnmatchedTransactions.Contains(TestARInvoice4));
			Assert("MatchedTransactions should contain TestARInvoice1", TestMatchingBase.MatchedTransactions.Contains(TestARInvoice1));
			TestMatchingBase.MoveFromUnmatchToMatch(new BusinessObject[] { TestARInvoice4 });
			AssertEquals("UnmatchedTransactions should be empty", 0, TestMatchingBase.UnmatchedTransactions.Count);
			Assert("MatchedTransactions should contain TestARInvoice1", TestMatchingBase.MatchedTransactions.Contains(TestARInvoice1));
			Assert("MatchedTransactions should contain TestARInvoice4", TestMatchingBase.MatchedTransactions.Contains(TestARInvoice4));
		}

		#endregion

		#region TestSettingPrimaryOrgClearsSelectedTransactions

		public virtual void TestSettingPrimaryOrgClearsSelectedTransactions()
		{
			TestAPInvoice1 = Factory.NewWithValidTestData<APInvoice>();

			TestOrg1 = GetNewTestOrg();
			TestOrg2 = GetNewTestOrg();

			Factory.Save();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestMatchingBase.MatchedTransactions.Add(TestAPInvoice1);
			TestMatchingBase.BalancingAPJournals.AddNew();
			TestMatchingBase.BalancingARJournals.AddNew();

			TestMatchingBase.PrimaryOrganization = TestOrg2.PK;
			AssertEquals("Should not be any selected transactions", 0, TestMatchingBase.MatchedTransactions.Count);
			AssertEquals("Should not be any balancing transactions", 0, TestMatchingBase.BalancingAPJournals.Count);
			AssertEquals("Should not be any balancing transactions", 0, TestMatchingBase.BalancingARJournals.Count);
		}

		#endregion

		#region TestInitialTransactionLoadingOccurs

		public virtual void TestInitialTransactionLoadingOccurs()
		{
			SetUpTestDataSet();
			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.Lines[0].AL_OSExTaxAmount = 30M;

			TestARInvoice2.AH_OH = TestOrg1.PK;
			TestARInvoice2.Lines[0].AL_OSExTaxAmount = 40M;

			TestARInvoice4.AH_OH = TestOrg1.PK;
			TestARInvoice4.Lines[0].AL_OSExTaxAmount = 50M;

			TestARInvoice4_2.AH_OH = TestOrg2.PK;
			TestARInvoice4_2.Lines[0].AL_OSExTaxAmount = 60M;

			ARCRD1 = Factory.NewWithValidTestData<ARCreditNote>();
			TestObjectCreator.CreateInvoiceLine(ARCRD1, ARCRD1.TransactionCurrency, 1M, 70M);
			ARCRD1.AH_OH = TestOrg2.PK;

			Factory.Save();

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			AssertEquals("LoadedTransactions should contain 3 elements", 3, TestMatchingBase.LoadedTransactions_ForTestOnly.Count);
			Assert("LoadedTransactions should contain TestARInvoice1", TestMatchingBase.LoadedTransactions_ForTestOnly.Contains(TestARInvoice1));
			Assert("LoadedTransactions should contain TestARInvoice2", TestMatchingBase.LoadedTransactions_ForTestOnly.Contains(TestARInvoice2));
			Assert("LoadedTransactions should contain TestARInvoice4", TestMatchingBase.LoadedTransactions_ForTestOnly.Contains(TestARInvoice4));

			TestMatchingBase.PrimaryOrganization = TestOrg2.PK;
			AssertEquals("LoadedTransactions should contain 2 elements", 2, TestMatchingBase.LoadedTransactions_ForTestOnly.Count);
			Assert("LoadedTransactions should contain TestARInvoice4_2", TestMatchingBase.LoadedTransactions_ForTestOnly.Contains(TestARInvoice4_2));
			Assert("LoadedTransactions should contain ARCRD1", TestMatchingBase.LoadedTransactions_ForTestOnly.Contains(ARCRD1));
		}

		public void TestLinesAreNotLoadedDuringInitialTransactionLoading()
		{
			BusinessObjectFactory businessObjectFactory = new BusinessObjectFactory();

			OrgHeader orgHeader = new TestObjectCreator(businessObjectFactory).CreateOrgHeader("ORGH1", true, true);

			ARInvoice invoice = businessObjectFactory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = orgHeader.PK;
			invoice.Lines.AddNew();
			invoice.Lines[0].FillWithValidTestData();
			invoice.Lines[0].AL_OSExTaxAmount = 70m;
			invoice.Lines[0].AL_AG = TestObjectCreator.GLHeader1.PK;

			businessObjectFactory.Save();

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538
			AssertMaxDbHits(delegate
			{ TestMatchingBase.PrimaryOrganization = orgHeader.PK; }, Factory, AccTransactionLinesSchema.Constants.TableName, 0);
		}

		void AssertMaxDbHits(AnonymousMethod codeToRun, BusinessObjectFactory factory, string tableName, int maxHits)
		{
			int hitsBefore = GetHits(factory, tableName);
			codeToRun();
			int hitsAfter = GetHits(factory, tableName);
			Assert(string.Format("Expected {0} or less db hits to {1} -- but was {2}", maxHits, tableName, hitsAfter - hitsBefore), hitsAfter - hitsBefore <= maxHits);
		}

		int GetHits(BusinessObjectFactory factory, string tableName)
		{
			int result = 0;

			foreach (var hitCount in factory.TableSelects)
			{
				if (hitCount.TableName == tableName)
				{
					result = hitCount.Value;
					break;
				}
			}

			return result;
		}

		#endregion

		#region Miscellaneous Transactions

		#region Matching Miscellaneous Transactions

		#region TestGetMiscellaneousTransaction

		public void TestGetMiscellaneousTransaction()
		{
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			TestARInvoice1 = Factory.New<ARInvoice>();
			TestARInvoice1.AH_LocalExTaxAmount = 100M;
			TestARInvoice1.AH_OSExTaxAmount = 100M;

			TestMatchingBase.AddIMatching(TestARInvoice1);
			TestMatchingBase.OverpaymentBizO_ForTestOnly = (Overpayment)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Overpayment);
			AssertEquals("Default description when created during match session", "MATCH NO.", TestMatchingBase.OverpaymentBizO_ForTestOnly.AH_Desc);

			AssertNotNull("OverpaymentTmp should be set", TestMatchingBase.OverpaymentTmp);

			//TestMatchingBase.MiscForm_TransactionPersistedEvent(new TransactionPersistedEventArgs(TestARInvoice1));
			TestMatchingBase.AddMiscellaneousTransaction(TestMatchingBase.OverpaymentBizO_ForTestOnly);

			AssertNull("OverpaymentTmp should be null", TestMatchingBase.OverpaymentTmp);

			TestMatchingBase.DiscountBizO_ForTestOnly = (Discount)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount, -11m);
			AssertEquals("Discount amount should be -11", -11m, TestMatchingBase.DiscountBizO_ForTestOnly.AH_InvoiceAmount);
			AssertEquals("Discount amount should be -11", -11m, TestMatchingBase.DiscountBizO_ForTestOnly.AH_OutstandingAmount);
			AssertEquals("ExchangeRate amount should be 1", 1m, TestMatchingBase.DiscountBizO_ForTestOnly.AH_ExchangeRate);
			AssertEquals("Default description when created during match session", "MATCH NO.", TestMatchingBase.DiscountBizO_ForTestOnly.AH_Desc);

			TestMatchingBase.ExchangeDifferenceBizO_ForTestOnly = (ExchangeDifference)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.ExchangeDifference, -22m);
			AssertEquals("ExchangeDifference amount should be -22", -22m, TestMatchingBase.ExchangeDifferenceBizO_ForTestOnly.AH_InvoiceAmount);
			AssertEquals("ExchangeDifference amount should be -22", -22m, TestMatchingBase.ExchangeDifferenceBizO_ForTestOnly.AH_OutstandingAmount);
			AssertEquals("ExchangeRate amount should be 1", 1m, TestMatchingBase.ExchangeDifferenceBizO_ForTestOnly.AH_ExchangeRate);
			AssertEquals("Default description when created during match session", "MATCH NO.", TestMatchingBase.ExchangeDifferenceBizO_ForTestOnly.AH_Desc);
		}

		[TestDate(2013, 10, 17)]
		public void TestGetMiscellaneousTransaction_Future()
		{
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestMatchingBase.MatchDate = ZDateTime.Today.AddDays(1);
			var miscTransaction = (Discount)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount, -11m);
			AssertEquals("Miscellaenous transaction should not have future date", ZDateTime.Today, miscTransaction.AH_PostDate);
			TestMatchingBase.MatchDate = ZDateTime.Today.AddDays(-1);
			miscTransaction = (Discount)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount, -11m);
			AssertEquals("Miscellaenous transaction has correct date", ZDateTime.Today.AddDays(-1), miscTransaction.AH_PostDate);
		}

		#endregion

		#region TestMiscTransactionsHaveCorrectOrg

		public void TestMiscTransactionsHaveCorrectOrg()
		{
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg2 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			Overpayment testOVP = (Overpayment)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Overpayment);
			AssertEquals("Overpayment's org should be testOrg1", TestOrg1.PK, testOVP.AH_OH);

			TestMatchingBase.PrimaryOrganization = TestOrg2.PK;
			testOVP = (Overpayment)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Overpayment);
			AssertEquals("Overpayment's org should be testOrg2", TestOrg2.PK, testOVP.AH_OH);
		}

		#endregion

		#region TestMiscTransactionsHaveCorrectMatchDate

		public void TestMiscTransactionsHaveCorrectMatchDate()
		{
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			ZDateTime expectedDate = ZDateTime.Today;
			TestMatchingBase.MatchDate = expectedDate;
			Overpayment testOVP = (Overpayment)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Overpayment);
			AssertEquals(string.Format("Overpayment's post date should be {0}", expectedDate), expectedDate, testOVP.AH_PostDate);
			Discount testDSC = (Discount)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount);
			AssertEquals(string.Format("Discount's post date should be {0}", expectedDate), expectedDate, testDSC.AH_PostDate);
			ExchangeDifference testEXX = (ExchangeDifference)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
			AssertEquals(string.Format("Exchange Difference's post date should be {0}", expectedDate), expectedDate, testEXX.AH_PostDate);
			Journal testJNL = (Journal)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Journal);
			AssertEquals(string.Format("Bank Fee Journal's post date should be {0}", expectedDate), expectedDate, testJNL.AH_PostDate);

			TestMatchingBase.MatchDate = ZDateTime.Today.AddDays(3);
			expectedDate = ZDateTime.Today;
			testOVP = (Overpayment)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Overpayment);
			TestMatchingBase.AddMiscellaneousTransaction(testOVP);
			AssertEquals(string.Format("Overpayment's post date should be {0}", expectedDate), expectedDate, testOVP.AH_PostDate);
			testDSC = (Discount)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount);
			TestMatchingBase.AddMiscellaneousTransaction(testDSC);
			AssertEquals(string.Format("Discount's post date should be {0}", expectedDate), expectedDate, testDSC.AH_PostDate);
			testEXX = (ExchangeDifference)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
			TestMatchingBase.AddMiscellaneousTransaction(testEXX);
			AssertEquals(string.Format("Exchange Difference's post date should be {0}", expectedDate), expectedDate, testEXX.AH_PostDate);
			testJNL = (Journal)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Journal);
			TestMatchingBase.AddMiscellaneousTransaction(testJNL);
			AssertEquals(string.Format("Bank Fee Journal's post date should be {0}", expectedDate), expectedDate, testJNL.AH_PostDate);

			TestMatchingBase.MatchDate = ZDateTime.Today.AddDays(-6);
			expectedDate = TestMatchingBase.MatchDate;
			testOVP = TestMatchingBase.OverpaymentBizO_ForTestOnly;
			AssertEquals(string.Format("Overpayment's post date should be {0}", expectedDate), expectedDate, testOVP.AH_PostDate);
			testDSC = TestMatchingBase.DiscountBizO_ForTestOnly;
			AssertEquals(string.Format("Discount's post date should be {0}", expectedDate), expectedDate, testDSC.AH_PostDate);
			testEXX = TestMatchingBase.ExchangeDifferenceBizO_ForTestOnly;
			AssertEquals(string.Format("Exchange Difference's post date should be {0}", expectedDate), expectedDate, testEXX.AH_PostDate);
			testJNL = TestMatchingBase.BankFeeBizO_ForTestOnly;
			AssertEquals(string.Format("Bank Fee Journal's post date should be {0}", expectedDate), expectedDate, testJNL.AH_PostDate);
		}

		#endregion

		#region TestMiscTransactionsHaveEditableDescription

		public void TestMiscTransactionsHaveEditableDescription()
		{
			AssertMiscTransactionsDescription();
		}

		public void TestMiscTransactionsDescription_ExceedMaxLength()
		{
			AssertMiscTransactionsDescription("Extra description that will be truncated");
		}

		void AssertMiscTransactionsDescription(string extraDescriptionToExceedMaxLength = "")
		{
			if (TestMatchingBase is ARMatchingBase || TestMatchingBase is APMatchingBase)
			{
				TestMatchingBase.PrimaryOrganization = TestObjectCreator.AALSHI.PK;
				TestMatchingBase.MatchDate = ZDateTime.Today;

				var expectedLengthofMatchGroupNumberAndSpaceChar = 10;
				var descriptionHavingSpaceToInsertMatchGroupNumber = "1234567890"
					+ "1234567890"
					+ "1234567890"
					+ "1234567890"
					+ "1234567890"
					+ "1234567890"
					+ "1234567890"
					+ "1234567890"
					+ "1234567890"
					+ "1234567890"
					+ "1234567890"
					+ "12345678";

				var description = descriptionHavingSpaceToInsertMatchGroupNumber +
					(extraDescriptionToExceedMaxLength.Length > expectedLengthofMatchGroupNumberAndSpaceChar ? extraDescriptionToExceedMaxLength.Substring(0, expectedLengthofMatchGroupNumberAndSpaceChar) : extraDescriptionToExceedMaxLength);
				AssertLessThanOrEqualTo("Precondition: Description should be less than or equal to max allowable length", description.Length, AccTransactionHeaderSchema.AH_Desc.MaxLength);
				AssertGreaterThanOrEqualTo("Precondition: Description should be greater than or equal to max allowable length when MatchGroupNumber is appended", description.Length + expectedLengthofMatchGroupNumberAndSpaceChar, AccTransactionHeaderSchema.AH_Desc.MaxLength);

				Overpayment testOVP = (Overpayment)TestMatchingBase.GetMiscellaneousTransaction(TransactionTypes.Overpayment, -10M);
				testOVP.AH_Desc = description;
				TestMatchingBase.AddMiscellaneousTransaction(testOVP);

				Discount testDSC = (Discount)TestMatchingBase.GetMiscellaneousTransaction(TransactionTypes.Discount, 10M);
				testDSC.AH_Desc = description;
				TestMatchingBase.AddMiscellaneousTransaction(testDSC);

				ExchangeDifference testEXX = (ExchangeDifference)TestMatchingBase.GetMiscellaneousTransaction(TransactionTypes.ExchangeDifference, 10M);
				testEXX.AH_Desc = description;
				TestMatchingBase.AddMiscellaneousTransaction(testEXX);

				Journal testJNL = (Journal)TestMatchingBase.GetMiscellaneousTransaction(TransactionTypes.Journal);
				testJNL.AH_Desc = description;
				testJNL.AH_OSTotal = -10M;
				testJNL.AH_InvoiceAmount = -10M;
				testJNL.AH_OutstandingAmount = -10M;
				TestMatchingBase.AddMiscellaneousTransaction(testJNL);

				TestMatchingBase.Match_ForTestOnly();

				var matchGroupNumberWithSpaceChar = " " + TestMatchingBase.MatchGroupNumber;
				AssertEquals(expectedLengthofMatchGroupNumberAndSpaceChar, matchGroupNumberWithSpaceChar.Length);
				AssertEquals("Description of matched Overpayment", descriptionHavingSpaceToInsertMatchGroupNumber + matchGroupNumberWithSpaceChar, testOVP.AH_Desc);
				AssertEquals("Description of matched Discount", descriptionHavingSpaceToInsertMatchGroupNumber + matchGroupNumberWithSpaceChar, testDSC.AH_Desc);
				AssertEquals("Description of matched ExchangeDifference", descriptionHavingSpaceToInsertMatchGroupNumber + matchGroupNumberWithSpaceChar, testEXX.AH_Desc);
				AssertEquals("Description of matched Journal", descriptionHavingSpaceToInsertMatchGroupNumber + matchGroupNumberWithSpaceChar, testJNL.AH_Desc);
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region TestBalanceIsUpdated

		public void TestBalanceIsUpdated()
		{
			SetUpTestDataSet();
			Factory.Save();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_LocalExTaxAmount = 40M;
			TestARInvoice1.AH_ExchangeRate = 1M;
			TestARInvoice1.AH_OSTotalAmount = 40M;
			((IMatching)TestARInvoice1).OSPartialPaymentAmount = 40M;
			TestMatchingBase.AddIMatching(TestARInvoice1);

			Overpayment testOVP = (Overpayment)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Overpayment);
			testOVP.AH_OSTotal = 100M;
			testOVP.AH_InvoiceAmount = 100M;
			testOVP.AH_OutstandingAmount = 100M;
			TestMatchingBase.OverpaymentBizO_ForTestOnly = testOVP;
			TestMatchingBase.AddIMatching(testOVP);

			Discount testDSC = (Discount)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount);
			testDSC.AH_OSTotal = 15M;
			testDSC.AH_InvoiceAmount = 15M;
			testDSC.AH_OutstandingAmount = 15M;
			TestMatchingBase.DiscountBizO_ForTestOnly = testDSC;
			TestMatchingBase.AddIMatching(testDSC);

			ExchangeDifference testEXX = (ExchangeDifference)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
			testEXX.AH_OSTotal = 2M;
			testEXX.AH_InvoiceAmount = 2M;
			testEXX.AH_OutstandingAmount = 2M;
			TestMatchingBase.ExchangeDifferenceBizO_ForTestOnly = testEXX;
			TestMatchingBase.AddIMatching(testEXX);

			AssertEquals("All amounts should contribute to balance", 157M, TestMatchingBase.Balance);
		}

		#endregion

		#region TestMatchDoesNotCallApplyRevenueRecognitionDate

		public void TestMatchDoesNotCallApplyRevenueRecognitionDate()
		{
			SetUpTestDataSet();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_ExchangeRate = 1M;
			TestObjectCreator.CreateInvoiceLine(TestARInvoice1, TestARInvoice1.TransactionCurrency, TestARInvoice1.AH_ExchangeRate, 50m, 0m, 0m, 50m, 0m, 0m);
			((IMatching)TestARInvoice1).OSPartialPaymentAmount = 50M;
			TestMatchingBase.AddIMatching(TestARInvoice1);

			TestAPInvoice1.AH_OH = TestOrg1.PK;
			TestAPInvoice1.AH_ExchangeRate = 1M;
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice1, TestAPInvoice1.TransactionCurrency, TestAPInvoice1.AH_ExchangeRate, 80m, 0m, 0m, 80m, 0m, 0m);
			((IMatching)TestAPInvoice1).OSPartialPaymentAmount = -50M;
			TestMatchingBase.AddIMatching(TestAPInvoice1);

			Factory.Save();

			RevenueRecognitionCollection registryCollection = new RevenueRecognitionCollection();
			RevenueRecognition registryValue = registryCollection.AddNew();
			registryValue.JobType = "SHP";
			registryValue.DirectionCode = Enterprise.Core.Constants.FreightShipmentDirection.Code.All;
			registryValue.Mode = RevenueRecognitionLookups.ModeAdditionalCodes.All;
			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
			AccountingConfigurationRegistry.Instance.RecognizeProfitOnWIPsAccrualsBeforePosting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			BusinessObjectFactory periodFactory = new BusinessObjectFactory();
			AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(periodFactory);
			helper.SetupPeriods();
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00012345");
			shipment.JS_E_ARV = helper.PreviousSubLedgerClosedPeriod.AM_StartDate;

			using (Job job = Accounting.Business.JobInvoicing.Job.CreateWithMutex(Factory, shipment))
			{
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				AssertEquals("Precondition", 0, job.RevenueRecognitionCollection.Count);

				BaseCharge charge = TestMatchingBase.Factory.NewWithValidTestData<Charge>();
				charge.JR_AC = TestObjectCreator.CC1.PK;
				charge.JR_JH = job.PK;
				charge.JR_LocalCostAmt = 10M;
				charge.JR_LocalSellAmt = 10M;

				Assert("Transactions should be matchable", TestMatchingBase.Match_ForTestOnly());
				AssertEquals("Job Revenue Recognition Date should not be set because it is suspended", 0, job.RevenueRecognitionCollection.Count);
			}
		}

		#endregion

		#region TestOverpaymentMatching

		public virtual void TestOverpaymentMatching()
		{
			SetUpTestDataSet();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_ExchangeRate = 1M;
			TestObjectCreator.CreateInvoiceLine(TestARInvoice1, TestARInvoice1.TransactionCurrency, TestARInvoice1.AH_ExchangeRate, 50m, 0m, 0m, 50m, 0m, 0m);
			((IMatching)TestARInvoice1).OSPartialPaymentAmount = 50M;
			TestMatchingBase.AddIMatching(TestARInvoice1);

			TestAPInvoice1.AH_OH = TestOrg1.PK;
			TestAPInvoice1.AH_ExchangeRate = 1M;
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice1, TestAPInvoice1.TransactionCurrency, TestAPInvoice1.AH_ExchangeRate, 80m, 0m, 0m, 80m, 0m, 0m);
			((IMatching)TestAPInvoice1).OSPartialPaymentAmount = -80M;
			TestMatchingBase.AddIMatching(TestAPInvoice1);

			Factory.Save();

			Overpayment testOVP = (Overpayment)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Overpayment);
			testOVP.AH_OSExTaxAmount = 60M;
			testOVP.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			testOVP.AH_ExchangeRate = 2M;
			TestMatchingBase.AddIMatching(testOVP);

			Assert("Transactions should be matchable", TestMatchingBase.Match_ForTestOnly());

			// Check Dynamic Transactions
			AssertEquals("One dynamic transaction should be created", 1, TestMatchingBase.DynamicTransactions.Count);
			Contra dynamicContra = TestMatchingBase.DynamicTransactions[0] as Contra;
			AssertNotNull("The dynamic transaction should be a contra", dynamicContra);

			AssertEquals("AP Row should be for TestOrg1", TestOrg1.PK, dynamicContra.APRow.AH_OH);
			AssertEquals("AR Row should be for TestOrg1", TestOrg1.PK, dynamicContra.ARRow.AH_OH);

			// Check that invoices are matched correctly
			Assert("ARINV should be fully paid", !TestARInvoice1.AH_FullyPaidDate.IsEmpty);
			AssertEquals("ARINV Outstanding amount should be 0", 0M, TestARInvoice1.AH_OutstandingAmount);

			Assert("APINV should be fully paid", !TestARInvoice1.AH_FullyPaidDate.IsEmpty);
			AssertEquals("APINV Outstanding amount should be 0", 0M, TestAPInvoice1.AH_OutstandingAmount);

			// Check Overpayment is correct

			AssertEquals("Invoice amount should be 30", 30M, testOVP.AH_LocalExTaxAmount);
			AssertEquals("OSTotal amount should be 60", 60M, testOVP.AH_OSTotalAmount);
			AssertEquals("Exchange Rate should be 2", 2M, testOVP.AH_ExchangeRate);

			AssertEquals("Overpayment account should be TestOrg1", TestOrg1.PK, testOVP.AH_OH);
			Assert("Overpayment Description should be set", testOVP.AH_Desc.StartsWith("MATCH NO."));
			Assert("Invoice date should not be null", !testOVP.AH_InvoiceDate.IsEmpty);
			Assert("Due date should not be empty", !testOVP.AH_DueDate.IsEmpty);
			Assert("Post date should not be empty", !testOVP.AH_PostDate.IsEmpty);
			AssertEquals("Branch should be set", GlbBranch.CurrentBranch.PK, testOVP.AH_GB);
			AssertEquals("Department should be set", GlbDepartment.CurrentDepartment.PK, testOVP.AH_GE);
			Assert("Fully paid date should be set", !testOVP.AH_FullyPaidDate.IsEmpty);
			AssertEquals("Outstanding amount should be 0", 0M, testOVP.AH_OutstandingAmount);

			// Check Matchlink rows
			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load(new ZQuery());
			AssertEquals("There should be 5 matchlinks", 5, matchLinks.Count);

			ZQuery oVP_Filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, testOVP.PK);
			TransactionMatchLink oVP_Match = Factory.LoadTop1<TransactionMatchLink>(oVP_Filter);
			AssertNotNull("There should be a matchlink for OVP", oVP_Match);
			Assert("Match group should not be empty", !oVP_Match.AP_MatchGroupNum.IsEmpty);
			Assert("Match date should not be empty", !oVP_Match.AP_MatchDate.IsEmpty);
			ZDateTime matchDate = oVP_Match.AP_MatchDate;
			ZString matchGroup = oVP_Match.AP_MatchGroupNum;

			AssertEquals("Amount should be 30", 30M, oVP_Match.AP_Amount);

			ZQuery aRINV_Filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestARInvoice1.PK);
			TransactionMatchLink aRINV_Match = Factory.LoadTop1<TransactionMatchLink>(aRINV_Filter);
			AssertNotNull("There should be a matchlink for ARINV", aRINV_Match);
			AssertEquals("Matchdate should be the same", matchDate, aRINV_Match.AP_MatchDate);
			AssertEquals("Matchgroup should be the same", matchGroup, aRINV_Match.AP_MatchGroupNum);
			AssertEquals("Match amount should be 50", 50M, aRINV_Match.AP_Amount);
		}

		#endregion

		#region TestOverpaymentAgainstSingleLedgerTransactions

		public virtual void TestOverpaymentAgainstSingleLedgerTransactions()
		{
			SetUpTestDataSet();

			if (TestMatchingBase is APMatchingBase)
			{
				TestOrg1.OH_IsCreditor = true;
			}
			else
			{
				TestOrg1.OH_IsDebtor = true;
			}

			TestAPInvoice1.AH_OH = TestOrg1.PK;
			TestAPInvoice1.AH_ExchangeRate = 1M;
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice1, TestAPInvoice1.TransactionCurrency, TestAPInvoice1.AH_ExchangeRate, 30m, 0m, 0m, 30m, 0m, 0m);
			((IMatching)TestAPInvoice1).OSPartialPaymentAmount = -30M;

			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_ExchangeRate = 1M;
			TestObjectCreator.CreateInvoiceLine(TestARInvoice1, TestARInvoice1.TransactionCurrency, TestARInvoice1.AH_ExchangeRate, 30m, 0m, 0m, 30m, 0m, 0m);
			((IMatching)TestARInvoice1).OSPartialPaymentAmount = 30M;

			Factory.Save();

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			if (TestMatchingBase is ARMatchingBase)
			{
				TestMatchingBase.AddIMatching(TestAPInvoice1);
			}
			else
			{
				TestMatchingBase.AddIMatching(TestARInvoice1);
			}

			// Add OVP
			Overpayment testOVP = (Overpayment)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Overpayment);
			TestMatchingBase.AddIMatching(testOVP);

			Assert("These transactions should be matchable", TestMatchingBase.Match_ForTestOnly());

			// Check dynamic transactions
			AssertEquals("One contra should be created", 1, TestMatchingBase.DynamicTransactions.Count);
			Contra dynamicContra = TestMatchingBase.DynamicTransactions[0] as Contra;
			AssertEquals("APRow should be for TestOrg1", TestOrg1.PK, dynamicContra.APRow.AH_OH);
			AssertEquals("ARRow should be for TestOrg1", TestOrg1.PK, dynamicContra.ARRow.AH_OH);
			AssertEquals("APRow should have InvoiceAmount 30", 30M, dynamicContra.APRow.AH_InvoiceAmount);
			AssertEquals("ARRow should have InvoiceAmount -30", -30M, dynamicContra.ARRow.AH_InvoiceAmount);

			Assert("ARRow should have non-empty fullypaid date", !dynamicContra.ARRow.AH_FullyPaidDate.IsEmpty);
			Assert("APRow should have non-empty fullypaid date", !dynamicContra.APRow.AH_FullyPaidDate.IsEmpty);

			// Check match link rows
			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load(new ZQuery());
			AssertEquals("4 matchlinks should be created", 4, matchLinks.Count);
		}

		#endregion

		#endregion

		#region Deleting Miscellaneous Transactions

		#region TestDeleteMiscTransaction

		public void TestDeleteMiscTransaction()
		{
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			Overpayment testOVP = (Overpayment)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Overpayment);
			TestMatchingBase.OverpaymentBizO_ForTestOnly = testOVP;
			TestMatchingBase.AddIMatching(testOVP);

			TestMatchingBase.DeleteMiscTransaction(testOVP);
			AssertNull("OverpaymentBizO should be null", TestMatchingBase.OverpaymentBizO_ForTestOnly);
			Assert("OverpaymentBizO should be deleted", testOVP.IsDeleted);
		}

		#endregion

		#region TestMoveFromMatchToUnmatchDeletesUnsavedMiscTransactions

		public void TestMoveFromMatchToUnmatchDeletesUnsavedMiscTransactions()
		{
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			TestARInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			TestMatchingBase.AddIMatching(TestARInvoice1);

			Overpayment testOVP = (Overpayment)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Overpayment);
			TestMatchingBase.AddIMatching(testOVP);
			TestMatchingBase.OverpaymentBizO_ForTestOnly = testOVP;

			ExchangeDifference testEXX = (ExchangeDifference)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
			TestMatchingBase.AddIMatching(testEXX);
			TestMatchingBase.ExchangeDifferenceBizO_ForTestOnly = testEXX;

			Discount testDSC = (Discount)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount);
			TestMatchingBase.AddIMatching(testDSC);
			TestMatchingBase.DiscountBizO_ForTestOnly = testDSC;

			Journal testJNL = (Journal)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Journal);
			TestMatchingBase.AddIMatching(testJNL);
			TestMatchingBase.BankFeeBizO_ForTestOnly = testJNL;

			BusinessObject[] selectedBizOs = new BusinessObject[5];
			selectedBizOs[0] = testOVP;
			selectedBizOs[1] = testEXX;
			selectedBizOs[2] = testDSC;
			selectedBizOs[3] = testJNL;
			selectedBizOs[4] = TestARInvoice1;
			TestMatchingBase.MoveFromMatchToUnmatch(selectedBizOs);

			Assert("Unsaved EXX should be deleted", testEXX.IsDeleted);
			AssertNull("EXX should be set to null", TestMatchingBase.ExchangeDifferenceBizO_ForTestOnly);
			AssertEquals("ExchangeDifference amount should be 0", 0M, TestMatchingBase.ExchangeDifferenceAmount);

			Assert("Unsaved OVP should be deleted", testOVP.IsDeleted);
			AssertNull("OVP should be set to null", TestMatchingBase.OverpaymentBizO_ForTestOnly);
			AssertEquals("Overpayment amount should be 0", 0M, TestMatchingBase.LocalOverpaymentAmount);

			Assert("Unsaved DSC should be deleted", testDSC.IsDeleted);

			Assert("Unsaved Bank Fee Journal should be deleted", testJNL.IsDeleted);
			AssertNull("Bank Fee Journal should be set to null", TestMatchingBase.BankFeeBizO_ForTestOnly);
			AssertEquals("Bank Fee amount should be 0", 0M, TestMatchingBase.BankFeeAmount);

			Assert("UnmatchTransactions should contain TestARInvoice", TestMatchingBase.UnmatchedTransactions.Contains(TestARInvoice1));
		}

		#endregion

		#region TestMoveAllFromMatchToUnmatchDeletesUnsavedMiscTransactions

		public void TestMoveAllFromMatchToUnmatchDeletesUnsavedMiscTransactions()
		{
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			TestARInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			TestMatchingBase.AddIMatching(TestARInvoice1);

			Overpayment testOVP = (Overpayment)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Overpayment);
			TestMatchingBase.AddIMatching(testOVP);
			TestMatchingBase.OverpaymentBizO_ForTestOnly = testOVP;

			Discount testDSC = (Discount)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount);
			TestMatchingBase.AddIMatching(testDSC);
			TestMatchingBase.DiscountBizO_ForTestOnly = testDSC;

			TestMatchingBase.MoveAllFromMatchToUnmatch();

			Assert("TestOVP should be deleted", testOVP.IsDeleted);
			Assert("MatchTransactions should not contain TestOVP", !TestMatchingBase.MatchedTransactions.Contains(testOVP));
			Assert("TestDSC should be deleted", testDSC.IsDeleted);
			Assert("MatchTransactions should not contain TestDSC", !TestMatchingBase.MatchedTransactions.Contains(testDSC));
		}

		#endregion

		public void TestMoveAllFromMatchToUnmatchWithDeleleBalancingJournalsAndRelatedJournal()
		{
			var apJournal = TestMatchingBase.BalancingAPJournals.AddNew();
			var arJournal = TestMatchingBase.BalancingARJournals.AddNew();

			var relatedAPJournal = apJournal.RelatedJournal = Factory.New<APJournal>();
			var relatedARJournal = arJournal.RelatedJournal = Factory.New<ARJournal>();

			AssertEquals("There is one apJournal in the BalancingAPJournals", 1, TestMatchingBase.BalancingAPJournals.Count);
			AssertEquals("There is one arJournal in the BalancingARJournals", 1, TestMatchingBase.BalancingARJournals.Count);
			AssertEquals("It is the invalid apJournal we created", apJournal, TestMatchingBase.BalancingAPJournals[0]);
			AssertEquals("It is the invalid arJournal we created", arJournal, TestMatchingBase.BalancingARJournals[0]);

			AssertEquals("The apJournal has a relatedAPJournal", relatedAPJournal, TestMatchingBase.BalancingAPJournals[0].RelatedJournal);
			AssertEquals("The arJournal has a relatedARJournal", relatedARJournal, TestMatchingBase.BalancingARJournals[0].RelatedJournal);

			TestMatchingBase.MoveAllFromMatchToUnmatch();

			AssertEquals("BalancingAPJournals were cleared", 0, TestMatchingBase.BalancingAPJournals.Count);
			AssertEquals("BalancingARJournals were cleared", 0, TestMatchingBase.BalancingARJournals.Count);
			Assert("apJournal was deleted", apJournal.IsDeleted);
			Assert("arJournal was deleted", arJournal.IsDeleted);
			Assert("relatedAPJournal was deleted", relatedAPJournal.IsDeleted);
			Assert("relatedARJournal was deleted", relatedARJournal.IsDeleted);
		}

		public void TestMoveAllFromMatchToUnmatchWithPreviouslySavedBalancingJournals()
		{
			var dbApJournal = Factory.New<APJournal>();
			var dbArJournal = Factory.New<ARJournal>();
			Factory.Save();
			var apJournal = TestMatchingBase.BalancingAPJournals.AddNew();
			var arJournal = TestMatchingBase.BalancingARJournals.AddNew();
			TestMatchingBase.BalancingAPJournals.Add(dbApJournal);
			TestMatchingBase.BalancingARJournals.Add(dbArJournal);
			AssertEquals("There is one apJournal in the BalancingAPJournals", 2, TestMatchingBase.BalancingAPJournals.Count);
			AssertEquals("There is one arJournal in the BalancingARJournals", 2, TestMatchingBase.BalancingARJournals.Count);
			AssertEquals("It is the invalid apJournal we created", apJournal, TestMatchingBase.BalancingAPJournals[0]);
			AssertEquals("It is the invalid arJournal we created", arJournal, TestMatchingBase.BalancingARJournals[0]);

			TestMatchingBase.MoveAllFromMatchToUnmatch();

			AssertEquals("BalancingAPJournals were cleared", 0, TestMatchingBase.BalancingAPJournals.Count);
			AssertEquals("BalancingARJournals were cleared", 0, TestMatchingBase.BalancingARJournals.Count);
			Assert("apJournal was deleted", apJournal.IsDeleted);
			Assert("arJournal was deleted", arJournal.IsDeleted);
			Assert("apJournal was not deleted", !dbApJournal.IsDeleted);
			Assert("arJournal was not deleted", !dbArJournal.IsDeleted);
		}

		#endregion

		#endregion

		#region TestMoveFromMatchToUnmatchResetsPartialPaidAmount

		public virtual void TestMoveFromUnMatchToMatchResetsPartialPaidAmount()
		{
			SetUpTestDataSet();
			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_LocalExTaxAmount = 10M;
			TestARInvoice1.AH_OSTotalAmount = 10M;
			((IMatching)TestARInvoice1).OSPartialPaymentAmount = 10M;

			TestMatchingBase.UnmatchedTransactions.Add(TestARInvoice1);

			BusinessObject[] tmpSelected = new BusinessObject[1];
			tmpSelected[0] = TestARInvoice1;

			TestMatchingBase.MoveFromUnmatchToMatch(tmpSelected);
			((IMatching)TestARInvoice1).OSPartialPaymentAmount = 5M;

			tmpSelected[0] = TestARInvoice1;
			TestMatchingBase.MoveFromMatchToUnmatch(tmpSelected);

			TestMatchingBase.MoveFromUnmatchToMatch(tmpSelected);

			AssertEquals("OSPartialPayment amt should be reset to 10", 10M, ((IMatching)TestARInvoice1).OSPartialPaymentAmount);
		}

		#endregion

		#region TestMoveFromMatchToUnmatchWithPartialPaidAmount

		public virtual void TestMoveFromMatchToUnmatchWithPartialPaidAmount()
		{
			SetUpTestDataSet();
			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_LocalExTaxAmount = 10M;
			TestARInvoice1.AH_OSTotalAmount = 10M;
			TestARInvoice1.AH_ExchangeRate = 1M;
			TestARInvoice1.AH_OutstandingAmount = 10M;

			TestMatchingBase.UnmatchedTransactions.Add(TestARInvoice1);

			Dictionary<BusinessObject, ZDecimal> tmpSelected = new Dictionary<BusinessObject, ZDecimal>();
			tmpSelected.Add(TestARInvoice1, 5m);

			TestMatchingBase.MoveFromUnmatchToMatch(tmpSelected);
			AssertEquals("Partial Payment Amount should be assigned", 5M, ((IMatching)TestARInvoice1).OSPartialPaymentAmount);

			TestMatchingBase.MoveFromMatchToUnmatch(new BusinessObject[] { TestARInvoice1 });
			tmpSelected = new Dictionary<BusinessObject, ZDecimal>();
			tmpSelected.Add(TestARInvoice1, ZDecimal.Zero);

			TestMatchingBase.MoveFromUnmatchToMatch(tmpSelected);
			AssertEquals(" Partial Payment Amount should be reset to Outstanding Amount", 10M, ((IMatching)TestARInvoice1).OSPartialPaymentAmount);
		}

		#endregion

		#region TestMoveFromUnmatchToMatchAddTransactionsToLineTotalPaidAmountPostedCalculator

		public void TestMoveFromUnmatchToMatchAddTransactionsToLineTotalPaidAmountPostedCalculator()
		{
			TransactionMatchLinkGroup matchlinks = new TransactionMatchLinkGroup(Factory);
			AccTransactionMatchLink link1 = Factory.NewWithValidTestData<AccTransactionMatchLink>();
			AccTransactionMatchLink link2 = Factory.NewWithValidTestData<AccTransactionMatchLink>();
			link1.AP_MatchGroupNum = link2.AP_MatchGroupNum = "001";
			matchlinks.Add(link1);
			matchlinks.Add(link2);

			List<APInvoice> inv = new List<APInvoice>();
			Dictionary<BusinessObject, ZDecimal> dict = new Dictionary<BusinessObject, ZDecimal>();

			for (int i = 0; i < 2; i++)
			{
				APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
				link1.AP_AH = link2.AP_AH = invoice.PK;

				APInvoiceLine line1 = (APInvoiceLine)invoice.Lines.AddNew();
				line1.AL_AG = TestObjectCreator.GLHeader1.PK;
				AccTransLinePay accline1 = line1.TransLinePays.AddNew();
				accline1.A7_Amount = 700 + i;
				accline1.A7_AP = link1.PK;

				APInvoiceLine line2 = (APInvoiceLine)invoice.Lines.AddNew();
				line2.AL_AG = TestObjectCreator.GLHeader1.PK;
				AccTransLinePay accline2 = line2.TransLinePays.AddNew();
				accline2.A7_Amount = 300 + i;
				accline2.A7_AP = link2.PK;

				inv.Add(invoice);
				dict.Add(invoice, ZDecimal.Zero);
			}
			Factory.Save();

			AssertNull("Should be null in Factory", Factory.ServiceContainer.GetService<InvoicingBase.LineTotalPaidAmountPostedCalculator>());

			TestMatchingBase.MoveFromUnmatchToMatch(dict);

			AssertNotNull("Should not be null in Factory", Factory.ServiceContainer.GetService<InvoicingBase.LineTotalPaidAmountPostedCalculator>());

			int hitsBefore = Db.Connection.ExecutedCommandCount;
			AssertEquals("Should Have 1000", 1000M, ((ISupportMatchingOfMyLines)inv[0]).LineTotalPaidAmountPosted);
			AssertEquals("Should Have 1002", 1002M, ((ISupportMatchingOfMyLines)inv[1]).LineTotalPaidAmountPosted);
			int hitsAfter = Db.Connection.ExecutedCommandCount;
			AssertEquals("Should have 0 DB Hits", 0, hitsAfter - hitsBefore);
		}

		#endregion

		#region TestMoveFilterMatchingTransactionsToSelectedDoesNotThrowException

		[ExpectNoExceptions]
		public void TestMoveFilterMatchingTransactionsToSelectedDoesNotThrowException()
		{
			TestOrg1 = GetNewTestOrg();
			Factory.Save();
			OpeningReceipt openingRec = Factory.NewWithValidTestData<OpeningReceipt>();
			openingRec.AH_OH = TestOrg1.PK;
			openingRec.AH_InvoiceAmount = 90M;
			openingRec.AH_OSTotal = 90M;
			openingRec.AH_OutstandingAmount = 90M;

			Factory.Save();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestMatchingBase.MoveFilterMatchingTransactionsToSelected();
		}

		#endregion

		#region TestFilteringWhenFilterDoesNotMatchOutstandingTransactions

		public virtual void TestFilteringWhenFilterDoesNotMatchOutstandingTransactions()
		{
			TestOrg1 = GetNewTestOrg();
			Factory.Save();

			TestARInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(TestARInvoice1, GlbCompany.CurrentCompany.LocalCurrency, 1m, 90M, 0m, 0m, 90M, 0m, 0m);

			TestAPInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInvoice1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice1, GlbCompany.CurrentCompany.LocalCurrency, 1m, 80M, 0m, 0m, 80M, 0m, 0m);

			Factory.Save();

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			var legderFilter = (DependentListFilter)TestMatchingBase.MatchingFilterBizO[MatchingFilterBusinessObject.LedgerTransactionType];
			legderFilter.Property1 = LedgerTypes.AccountsPayable;
			legderFilter.IsActive = true;

			TestMatchingBase.LoadTransactionsMatchingTheFilter();
			OverlapResult result = TestMatchingBase.GetTransactionOverlapResultAndMoveIfRequired();
			AssertEquals("Result should be CurrentContainsFound", OverlapResult.CurrentContainsFound, result);

			AssertEquals("There should be 2 transactions in unmatchedTransactions", 2,
				TestMatchingBase.UnmatchedTransactions.Count);

			Assert("ARInvoice should still appear as outstanding", TestMatchingBase.UnmatchedTransactions.Contains(TestARInvoice1));
		}

		#endregion

		#region TestGetTransactionOverlapResultAndMoveIfRequired

		public void TestGetTransactionOverlapResultAndMoveIfRequired()
		{
			TestARInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			TestARInvoice2 = Factory.NewWithValidTestData<ARInvoice>();

			TestAPInvoice1 = Factory.NewWithValidTestData<APInvoice>();

			TestMatchingBase.UnmatchedTransactions.Add(TestARInvoice1);
			TestMatchingBase.FMatchingLoadedBizOs_ForTestOnly = new BusinessObject[] { TestARInvoice1, TestARInvoice2 };

			AssertEquals("Overlap result should be current does not contain found",
				OverlapResult.CurrentDoesNotContainFound, TestMatchingBase.GetTransactionOverlapResultAndMoveIfRequired());
			AssertEquals("UnmatchedTransactions should contain 2 transactions", 2, TestMatchingBase.UnmatchedTransactions.Count);
			Assert("UnmatchedTransactions contains ARInv1", TestMatchingBase.UnmatchedTransactions.Contains(TestARInvoice1));
			Assert("UnmatchedTransactions contains ARInv2", TestMatchingBase.UnmatchedTransactions.Contains(TestARInvoice2));

			TestMatchingBase.UnmatchedTransactions.Add(TestARInvoice1);
			TestMatchingBase.UnmatchedTransactions.Add(TestARInvoice2);
			TestMatchingBase.UnmatchedTransactions.Add(TestAPInvoice1);
			AssertEquals("Overlap result should be current contains found",
				OverlapResult.CurrentContainsFound, TestMatchingBase.GetTransactionOverlapResultAndMoveIfRequired());
			AssertEquals("UnmatchedTransactions should be unchanged", 3, TestMatchingBase.UnmatchedTransactions.Count);

			TestMatchingBase.FMatchingLoadedBizOs_ForTestOnly = Array.Empty<BusinessObject>();

			AssertEquals("Overlap result should be 'none found'", OverlapResult.NoneFound,
				TestMatchingBase.GetTransactionOverlapResultAndMoveIfRequired());
		}

		#endregion

		#region TestSequentialMatchingSessionsCorrect

		public void TestSequentialMatchingSessionsCorrect()
		{
			SetUpTestDataSet();
			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_ExchangeRate = 1M;
			TestObjectCreator.CreateInvoiceLine(TestARInvoice1, TestARInvoice1.TransactionCurrency, TestARInvoice1.AH_ExchangeRate, 40m, 0m, 0m, 40m, 0m, 0m);

			Factory.Save();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			((IMatching)TestARInvoice1).OSPartialPaymentAmount = 10M;
			TestMatchingBase.AddIMatching(TestARInvoice1);
			Discount testDSC = (Discount)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount);
			TestMatchingBase.AddIMatching(testDSC);

			//Assert("These transactions should be matchable", TestMatchingBase.MatchAndClearTransactions());
			TestMatchingBase.MatchAndClearTransactions();

			ARInvoice aRINVReload = Factory.Load<ARInvoice>(TestARInvoice1.PK);
			((IMatching)aRINVReload).OSPartialPaymentAmount = 10M;
			TestMatchingBase.AddIMatching(aRINVReload);

			Discount testDSC2 = (Discount)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount);
			TestMatchingBase.AddIMatching(testDSC2);

			Assert("These transactions should be matchable", TestMatchingBase.Match_ForTestOnly());
		}

		#endregion

		#region TestMatchingCorrectWithConcurrency

		public virtual void TestMatchingSuccessWithConcurrency()
		{
			SetUpTestDataSet();

			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_ExchangeRate = 1M;
			TestObjectCreator.CreateInvoiceLine(TestARInvoice1, TestARInvoice1.TransactionCurrency, TestARInvoice1.AH_ExchangeRate, 40m, 0m, 0m, 40m, 0m, 0m);
			Factory.Save();

			var testMatchingBase = GetTestMatchingBaseInNewFactory(Factory);
			testMatchingBase.PrimaryOrganization = TestOrg1.PK;
			((IMatching)TestARInvoice1).OSPartialPaymentAmount = 10M;
			testMatchingBase.AddIMatching(TestARInvoice1);
			var testDSC = (Discount)testMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount);
			testMatchingBase.AddIMatching(testDSC);

			Factory.Saving += concurrencyEvent => InvoiceConcurrencyEvent(TestARInvoice1.PK);

			Assert("If Factory is saved, these transactions should not be matched, but no error should be thrown", !testMatchingBase.MatchAndClearTransactionsWithSaveErrorHandling());
			Assert("if Factory is saved, concurrency Error should corrupt matching", testMatchingBase.Corrupted);
		}

		void InvoiceConcurrencyEvent(ZGuid aRInvoicePK)
		{
			var anotherFactory = new BusinessObjectFactory();
			anotherFactory.RefreshEnabled = false;
			var staff = anotherFactory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				var reloadedARInvoice1 = anotherFactory.Load<ARInvoice>(aRInvoicePK);

				reloadedARInvoice1.AH_Desc = "Invoice Modified Elsewhere";
				anotherFactory.Save();
			}
		}

		#endregion

		#region TestShouldShowRelatedDisbursementTransactions

		public virtual void TestShouldShowRelatedDisbursementTransactions()
		{
			var matchingBaseForTest = GetTestMatchingBase();
			AssertEquals("Default value should be false.", false, matchingBaseForTest.ShouldShowRelatedDisbursementTransactions);
		}

		#endregion

		#region TestReloadSettlementOrgTransactions

		public void TestPrimaryOrganizationDoesNotLoadTransactionFromDBIfIsLoadedFromGUIIsFalse()
		{
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();
			TestMatchingBase.SetIsLoadedFromGUIForTest_ForTestOnly(false);
			int beforeHit = Factory.GetTableHitCount(AccTransactionHeaderSchema.Constants.TableName);
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			int afterHit = Factory.GetTableHitCount(AccTransactionHeaderSchema.Constants.TableName);
			AssertEquals("AccTransactionHeader table should not be loaded", beforeHit, afterHit);
		}

		public virtual void TestReloadSettlementOrgTransactions()
		{
			TestOrg1 = GetNewTestOrg();

			TestOrg2 = GetNewTestOrg();

			Factory.Save();

			TestARReceipt = Factory.New<ARReceipt>();
			TestARReceipt.AH_OH = TestOrg1.PK;
			TestARReceipt.AH_LocalExTaxAmount = 10M; // sets the outstanding amount as well
			TestARReceipt.AH_OSExTaxAmount = 10M;

			TestARInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			TestARInvoice1.AH_OH = TestOrg2.PK;
			TestObjectCreator.CreateInvoiceLine(TestARInvoice1, GlbCompany.CurrentCompany.LocalCurrency, 1m, 40M, 0m, 0m, 40M, 0m, 0m);

			Factory.Save();

			Factory.Save();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			AssertEquals("there should be 1 outstanding Transaction for TestOrg1", 1,
				TestMatchingBase.LoadedTransactions_ForTestOnly.Count);

			OrgLedgerFilter filter2 = TestMatchingBase.MatchingFilterBizO.SettlementOrgInfos.AddNew();
			filter2.OrganisationBizO = TestOrg2;

			TestMatchingBase.ReloadSettlementOrgTransactions();
			AssertEquals("There should be 2 Transactions in UnmatchedTransactions", 2,
				TestMatchingBase.UnmatchedTransactions.Count);
			Assert("Contains ARReceipt", TestMatchingBase.UnmatchedTransactions.Contains(TestARReceipt));
			Assert("Contais ARInvoice", TestMatchingBase.UnmatchedTransactions.Contains(TestARInvoice1));
		}

		public virtual void TestSetPrimaryOrganization_UsesExpectedIndex()
		{
			TestOrg1 = GetNewTestOrg();
			TestOrg2 = GetNewTestOrg();
			Factory.Save();

			for (int i = 0; i < 150; i++)
			{
				var invoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
				invoice.AH_OH = TestOrg1.PK;
				TestObjectCreator.CreateInvoiceLine(invoice, GlbCompany.CurrentCompany.LocalCurrency, 1m, 40M, 0m, 0m, 40M, 0m, 0m);
			}
			for (int i = 0; i < 2; i++)
			{
				var invoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
				invoice.AH_OH = TestOrg2.PK;
				TestObjectCreator.CreateInvoiceLine(invoice, GlbCompany.CurrentCompany.LocalCurrency, 1m, 40M, 0m, 0m, 40M, 0m, 0m);
			}
			Factory.Save();
			Db.Connection.ExecuteNonQuery($"UPDATE STATISTICS {AccTransactionHeaderSchema.Constants.TableName} WITH FULLSCAN");

			fTestMatchingBase = GetTestMatchingBase();
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538
			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				TestMatchingBase.PrimaryOrganization = TestOrg2.PK;
				AssertEquals("There should be 2 outstanding Transaction for TestOrg2", 2, TestMatchingBase.LoadedTransactions_ForTestOnly.Count);

				var queryPlan = TestConnection.ExecutedCommandsAndQueryPlans.First(t => t.Item1.Contains("AccTransactionHeader"));
				var queryPlanAnalyzer = new QueryPlanalyzer(queryPlan.Item2.First());
				var actualIndexs = string.Join(",", queryPlanAnalyzer.IndexScans.Select(x => x.IndexName)) + "," + string.Join(",", queryPlanAnalyzer.IndexSeeks.Select(x => x.IndexName));
				Assert("Non clustered index NR_RX__AH_GC_AH_Ledger_AH_OH_AH_TransactionType_AH_DueDate should be used for best performance. Indexes used: " + actualIndexs, queryPlanAnalyzer.IndexSeeks.Any(x => x.IndexName == "NR_RX__AH_GC_AH_Ledger_AH_OH_AH_TransactionType_AH_DueDate"));
			}
		}

		#endregion

		#region TestExcludeTransactionsFromUnmatchedList

		public virtual void TestExcludeTransactionsFromUnmatchedList()
		{
			TestOrg1 = GetNewTestOrg();
			TestARReceipt = Factory.NewWithValidTestData<ARReceipt>();
			TestARReceipt.AH_OH = TestOrg1.PK;
			TestARReceipt.AH_LocalExTaxAmount = 10M;
			TestARReceipt.AH_OSExTaxAmount = 10M;

			TestARInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(TestARInvoice1, GlbCompany.CurrentCompany.LocalCurrency, 1m, 40M);

			ARReceipt testARReceipt2 = Factory.NewWithValidTestData<ARReceipt>();
			testARReceipt2.AH_OH = TestOrg1.PK;
			testARReceipt2.AH_LocalExTaxAmount = 10M;
			testARReceipt2.AH_OSExTaxAmount = 10M;

			Factory.Save();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			AssertEquals("there should be 3 outstanding Transactions for TestOrg1", 3, TestMatchingBase.LoadedTransactions_ForTestOnly.Count);
			AssertEquals("there should be 3 outstanding Transactions for TestOrg1", 3, TestMatchingBase.UnmatchedTransactions.Count);

			IMatchingCollection excludedTransactions = new IMatchingCollection(Factory) { TestARReceipt, TestARInvoice1 };
			TestMatchingBase.ExcludeTransactionsFromUnmatchedList(excludedTransactions);
			AssertEquals("there should be only 1 outstanding Transactions for TestOrg1 in UnmatchedTransactions", 1, TestMatchingBase.UnmatchedTransactions.Count);
			Assert("Contains TestARReceipt2", TestMatchingBase.UnmatchedTransactions.Contains(testARReceipt2));

			TestMatchingBase.ReloadSettlementOrgTransactions();
			AssertEquals("There should be 1 Transaction in LoadedTransactions because 2 others were excluded", 1, TestMatchingBase.LoadedTransactions_ForTestOnly.Count);
			AssertEquals("there should be only 1 outstanding Transaction for TestOrg1 in UnmatchedTransactions", 1, TestMatchingBase.UnmatchedTransactions.Count);
			Assert("Contains TestARReceipt2", TestMatchingBase.UnmatchedTransactions.Contains(testARReceipt2));
		}

		#endregion

		#region TestLoadTransactionsFromCollection

		public virtual void TestLoadTransactionsFromCollection()
		{
			SetUpTestDataSet();

			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(TestARInvoice1, GlbCompany.CurrentCompany.LocalCurrency, 1m, 100m, 0m, 0m, 100m, 0m, 0m);

			TestARInvoice2.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(TestARInvoice2, GlbCompany.CurrentCompany.LocalCurrency, 1m, 200m, 0m, 0m, 200m, 0m, 0m);

			TestARInvoice4.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(TestARInvoice4, GlbCompany.CurrentCompany.LocalCurrency, 1m, 40m, 0m, 0m, 40m, 0m, 0m);

			Factory.Save();

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "Master bill";
			consol.Transports[0].JW_VoyageFlight = "Voyage";
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HouseBill = "House bill";
			shipment.Consols.Add(consol);
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			job.Parent = shipment;
			TestARInvoice1.AH_JH = job.PK;

			Factory.Save();

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestMatchingBase.MatchedTransactions.Add(TestARInvoice4);
			TestMatchingBase.ReloadSettlementOrgTransactions();

			AssertEquals("UnmatchTransactions should contain 2 elements only because ARINV4 is in MatchTransactions", 2, TestMatchingBase.UnmatchedTransactions.Count);
			Assert("UnmatchTransactions should contain ARINV1", TestMatchingBase.UnmatchedTransactions.Contains(TestARInvoice1));
			Assert("UnmatchTransactions should contain ARINV2", TestMatchingBase.UnmatchedTransactions.Contains(TestARInvoice2));

			var filter = (ModuleNumberFilter)TestMatchingBase.MatchingFilterBizO[MatchingFilterBusinessObject.AllNumbers];
			filter.Property = "Master bill";
			filter.IsActive = true;
			TestMatchingBase.ReloadSettlementOrgTransactions();

			AssertEquals("UnmatchTransactions should contain 1 element", 1, TestMatchingBase.UnmatchedTransactions.Count);
			Assert("UnmatchTransactions should contain ARINV1", TestMatchingBase.UnmatchedTransactions.Contains(TestARInvoice1));

			shipment.JS_IsShipping = true;
			Factory.Save();

			filter.Property = "Voyage";
			TestMatchingBase.ReloadSettlementOrgTransactions();

			AssertEquals("UnmatchTransactions should contain 1 element", 1, TestMatchingBase.UnmatchedTransactions.Count);
			Assert("UnmatchTransactions should contain ARINV1", TestMatchingBase.UnmatchedTransactions.Contains(TestARInvoice1));

			filter.Property = "House bill";
			TestMatchingBase.ReloadSettlementOrgTransactions();

			AssertEquals("UnmatchTransactions should contain 1 element", 1, TestMatchingBase.UnmatchedTransactions.Count);
			Assert("UnmatchTransactions should contain ARINV1", TestMatchingBase.UnmatchedTransactions.Contains(TestARInvoice1));

			filter.Property = "NonExistent";
			TestMatchingBase.ReloadSettlementOrgTransactions();

			AssertEquals("UnmatchTransactions should contain 0 elements", 0, TestMatchingBase.UnmatchedTransactions.Count);

			filter.Property = "bill";
			TestMatchingBase.ReloadSettlementOrgTransactions();

			AssertEquals("UnmatchTransactions should contain 0 elements", 0, TestMatchingBase.UnmatchedTransactions.Count);

			filter.Property = "bill";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			TestMatchingBase.ReloadSettlementOrgTransactions();

			AssertEquals("UnmatchTransactions should contain 1 element", 1, TestMatchingBase.UnmatchedTransactions.Count);
			Assert("UnmatchTransactions should contain ARINV1", TestMatchingBase.UnmatchedTransactions.Contains(TestARInvoice1));
		}

		public void TestTransactionPKsAreRegisteredForLoadingWHT()
		{
			TestOrg1 = GetNewTestOrg();
			Factory.Save();

			TestAPInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInvoice1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice1, GlbCompany.CurrentCompany.LocalCurrency, 1m, 80M, 0m, 0m, 80M, 0m, 0m);
			Factory.Save();

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			var legderFilter = (DependentListFilter)TestMatchingBase.MatchingFilterBizO[MatchingFilterBusinessObject.LedgerTransactionType];
			legderFilter.Property1 = LedgerTypes.AccountsPayable;
			legderFilter.IsActive = true;

			var loaderMock = new Mock<IWHTAmountLoader>();
			loaderMock.SetupProperty(x => x.IsWHTRealizationInProgress);
			loaderMock.Setup(x => x.RegisterForLoadingWHTAmounts(TestAPInvoice1.PK));
			TaxFrameworkObjectFactory.SubstituteWHTAmountLoader_ForTestOnly(Factory, loaderMock.Object);
			TestMatchingBase.LoadTransactionsMatchingTheFilter();

			if (TestMatchingBase.LoadedTransactions_ForTestOnly.Any())
			{
				AssertContainsExactElementsInAnyOrder(new[] { TestAPInvoice1 }, TestMatchingBase.LoadedTransactions_ForTestOnly);
				loaderMock.VerifySet(x => x.IsWHTRealizationInProgress = true);
				loaderMock.Verify(x => x.RegisterForLoadingWHTAmounts(TestAPInvoice1.PK), Times.Once);
			}
			else
			{
				Assert(true);
				loaderMock.VerifySet(x => x.IsWHTRealizationInProgress = true, Times.Never);
				loaderMock.Verify(x => x.RegisterForLoadingWHTAmounts(TestAPInvoice1.PK), Times.Never);
			}
		}

		#region LoadTransactionsFromCollection with many transactions

		public virtual void TestLoadTransactionsFromCollection_WhenManyTransactionsAreBeingMatched()
		{
			const int transactionCount = 128;
			var pks = SetupDataForLoadTransactionsFromCollectionWithManyTransactions(transactionCount);

			using (Db.Connection.TrackExecutedCommands())
			{
				TestMatchingBase.ReloadSettlementOrgTransactions();

				var expectedCount = GetExpectedCountOfAllMatchedWhenManyTransactions(transactionCount);
				AssertEquals(expectedCount, TestMatchingBase.MatchedTransactions.Count);

				var query = ExtractQueryForLoadTransactionsFromCollectionWithManyTransactions(Db.Connection.ExecutedCommands);
				foreach (var pk in pks)
				{
					var pkIsContainedInQueryWhereClause = query.IndexOf(pk.ToString(), StringComparison.InvariantCultureIgnoreCase) > -1;
					Assert("Filter which includes many AH_PK values should not inline PK values in query: " + pk, !pkIsContainedInQueryWhereClause);
				}
			}
		}

		protected virtual int GetExpectedCountOfAllMatchedWhenManyTransactions(int count) => count;

		IReadOnlyCollection<Guid> SetupDataForLoadTransactionsFromCollectionWithManyTransactions(int transactionCount)
		{
			SetUpTestDataSet();
			Factory.Save();

			var pks = new List<Guid>();
			var bulkDbHelper = new Build.Database.Script.TestFramework.TestDbHelper(TestConnection);
			for (int i = 0; i < transactionCount; i++)
			{
				pks.Add(
					bulkDbHelper.InsertTransactionHeader(LedgerType, TransactionTypes.Invoice, $"BULK{i:N4}", 1m, ZDateTime.Today.ToDateTime(), org: TestOrg1.PK.ToGuid())
				);
			}
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestMatchingBase.ReloadSettlementOrgTransactions();
			TestMatchingBase.MoveAllFromUnmatchToMatch();

			return pks;
		}

		string ExtractQueryForLoadTransactionsFromCollectionWithManyTransactions(IEnumerable<string> commands)
		{
			var ahQuery = commands.Single(c => c.Contains(AccTransactionHeader.Schema.TableName));
			var ahQueryWithoutParameterValues = ahQuery.Substring(0, ahQuery.IndexOf("/* Parameter Stats"));
			return ahQueryWithoutParameterValues;
		}

		#endregion

		#endregion

		#region TestMatchedTransactionsAreNotRematched

		public virtual void TestMatchedTransactionsAreNotRematched()
		{
			SetUpTestDataSet();
			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_LocalExTaxAmount = 10;
			TestARInvoice1.AH_OutstandingAmount = 0;
			TestARInvoice1.AH_FullyPaidDate = ZDateTime.Today;

			TestARReceipt.AH_OH = TestOrg1.PK;
			TestARReceipt.AH_LocalExTaxAmount = 10;

			TestMatchingBase.AddIMatching(TestARInvoice1);
			TestMatchingBase.AddIMatching(TestARReceipt);
			TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

			Assert("ARInvoice has been matched so matching should fail", !TestMatchingBase.Match_ForTestOnly());
		}

		#endregion

		#region Matching (with different Transactions, Orgs, Ledgers and Partial Amounts)

		#region TestARMatching_InvoiceAndReceiptFromSameOrg

		public virtual void TestARMatching_InvoiceAndReceiptFromSameOrg()
		{
			SetUpTestDataSet();
			if (TestMatchingBase is ARMatchingBase)
			{
				TestOrg1.OH_IsDebtor = true;
			}
			else
			{
				TestOrg1.OH_IsCreditor = true;
			}
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			TestARInvoice1.AH_OSTotal = 50M;
			TestARInvoice1.AH_LocalExTaxAmount = 50M;
			TestARInvoice1.AH_OH = TestOrg1.PK;
			InvoicingLineBase line = (InvoicingLineBase)TestARInvoice1.Lines.AddNew();
			line.AL_LineAmount = 50M;
			line.AL_OverseasTotal = 50M;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			TestARInvoice1.AH_FullyPaidDate = ZDateTime.Empty;

			TestAPInvoice1.AH_OSTotal = 50M;
			TestAPInvoice1.AH_LocalExTaxAmount = 50M;
			TestAPInvoice1.AH_OH = TestOrg1.PK;
			line = (InvoicingLineBase)TestAPInvoice1.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_LineAmount = -50M;
			line.AL_OverseasTotal = 50M;
			TestAPInvoice1.AH_FullyPaidDate = ZDateTime.Empty;

			TransactionMatchLink oldMatchLink = ((IMatching)TestAPInvoice1).CurrentMatchGroup.AddNew();
			oldMatchLink.AP_Amount = 0M;
			oldMatchLink.AP_MatchGroupNum = "M00001002";
			oldMatchLink.AP_AH = TestAPInvoice1.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(TestAPInvoice1);

			TestARReceipt.AH_LocalExTaxAmount = 60M;
			TestARReceipt.AH_OH = TestOrg1.PK;
			TestARReceipt.AH_OSTotalAmount = 60M;

			TestAPPayment.AH_LocalExTaxAmount = 60M;
			TestAPPayment.AH_OH = TestOrg1.PK;
			TestAPPayment.AH_OSTotalAmount = 60M;

			Factory.Save();

			if (TestMatchingBase is ARMatchingBase)
			{
				TestMatchingBase.AddIMatching(TestARInvoice1);
				TestMatchingBase.AddIMatching(TestARReceipt);
			}
			else
			{
				TestMatchingBase.AddIMatching(TestAPInvoice1);
				TestMatchingBase.AddIMatching(TestAPPayment);
			}

			TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();
			decimal expectedBalance = TestMatchingBase is APMatchingBase ? 10M : -10M;
			AssertEquals("Balance should be -10/+10 depending on Ledger", expectedBalance, TestMatchingBase.MatchedTransactions.Balance);

			Assert("Should not allow a match since balance is not 0", !TestMatchingBase.Match_ForTestOnly());

			decimal eXXAmount = TestMatchingBase is APMatchingBase ? -6M : 6M;

			ExchangeDifference testEXX = (ExchangeDifference)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
			testEXX.AH_OSTotal = eXXAmount;
			testEXX.AH_InvoiceAmount = eXXAmount;
			testEXX.AH_OutstandingAmount = eXXAmount;
			TestMatchingBase.AddIMatching(testEXX);

			decimal dSCAmount = TestMatchingBase is APMatchingBase ? -4M : 4M;

			Discount testDSC = (Discount)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount);
			testDSC.AH_OSTotal = dSCAmount;
			testDSC.AH_InvoiceAmount = dSCAmount;
			testDSC.AH_OutstandingAmount = dSCAmount;
			TestMatchingBase.AddIMatching(testDSC);

			TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();
			AssertEquals("Balance should be zero", 0M, TestMatchingBase.Balance);
			Assert("These transactions should be matchable", TestMatchingBase.Match_ForTestOnly());

			// Check that Discount and ExchangeDifference have been created

			AssertNotNull("There should be 1 discount", testDSC);
			Assert("DSC should be fully paid", !testDSC.AH_FullyPaidDate.IsEmpty);
			AssertEquals("AH_OSTOtal should be +4/-4", dSCAmount, testDSC.AH_OSTotal);
			AssertEquals("AH_InvoiceAMount should be +4/-4", dSCAmount, testDSC.AH_InvoiceAmount);
			AssertEquals("AH_OutstandingAmount should be 0", 0M, testDSC.AH_OutstandingAmount);
			AssertEquals("AH_ExchangeRate should be 1", 1M, testDSC.AH_ExchangeRate);

			AssertNotNull("There should be 1 exchange diff", testEXX);
			Assert("EXX should be fully paid", !testEXX.AH_FullyPaidDate.IsEmpty);
			AssertEquals("AH_OSTotal should be +6/-6", eXXAmount, testEXX.AH_OSTotal);
			AssertEquals("AH_InvoiceAmount should be +6/-6", eXXAmount, testEXX.AH_InvoiceAmount);
			AssertEquals("AH_Outstanding amount should be 0", 0M, testEXX.AH_OutstandingAmount);
			AssertEquals("AH_ExchangeRate should be 1", 1M, testEXX.AH_ExchangeRate);

			// Check that Matchlink rows are created correctly
			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load(new ZQuery());
			AssertEquals("There should be 5 matchlink rows", 5, matchLinks.Count);

			int numberOfTransLinePayReleatedToOldMatchLink = Factory.GetDatabaseCount(typeof(AccTransLinePay), new ZQuery(AccTransLinePaySchema.A7_AP, oldMatchLink.PK));
			AssertEquals("There should be NO Pay Lines attached to old MatchLink", 0, numberOfTransLinePayReleatedToOldMatchLink);

			ZQuery dSC_Filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, testDSC.PK);
			TransactionMatchLink dSC_Match = Factory.LoadTop1<TransactionMatchLink>(dSC_Filter);
			AssertNotNull("There should be a match link for DSC", dSC_Match);
			AssertEquals("DSC Matchlink amount should be +4/-4", dSCAmount, dSC_Match.AP_Amount);

			ZQuery eXX_Filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, testEXX.PK);
			TransactionMatchLink eXX_Match = Factory.LoadTop1<TransactionMatchLink>(eXX_Filter);
			AssertNotNull("There should e a match link for EXX", eXX_Match);
			AssertEquals("EXX Matchlink amount should be +6/-6", eXXAmount, eXX_Match.AP_Amount);
		}

		#endregion

		#region TestARMatching_TwoInvoiceAndOneReceiptFromSameOrg

		public virtual void TestARMatching_TwoInvoiceAndOneReceiptFromSameOrg()
		{
			SetUpTestDataSet();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			TestARInvoice1.AH_LocalExTaxAmount = 50M;
			TestARInvoice1.AH_OSTotal = 50M;
			TestARInvoice1.AH_OH = TestOrg1.PK;
			InvoicingLineBase line = (InvoicingLineBase)TestARInvoice1.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_LineAmount = 50M;
			line.AL_OverseasTotal = 50M;
			TestARInvoice1.AH_FullyPaidDate = ZDateTime.Empty;

			TestARInvoice2.AH_LocalExTaxAmount = 30M;
			TestARInvoice2.AH_OSTotal = 30M;
			TestARInvoice2.AH_OH = TestOrg2.PK;
			line = (InvoicingLineBase)TestARInvoice2.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_LineAmount = 30M;
			line.AL_OverseasTotal = 30M;
			TestARInvoice2.AH_FullyPaidDate = ZDateTime.Empty;

			TestARReceipt.AH_LocalExTaxAmount = 70M;
			TestARReceipt.AH_OSTotalAmount = 70M;
			TestARReceipt.AH_OH = TestOrg3.PK;

			Factory.Save();

			TestMatchingBase.AddIMatching(TestARInvoice1);
			TestMatchingBase.AddIMatching(TestARInvoice2);
			TestMatchingBase.AddIMatching(TestARReceipt);
			TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

			AssertEquals("Balance should be 10", 10M, TestMatchingBase.MatchedTransactions.Balance);

			Assert("Should not allow a match since balance is not 0", !TestMatchingBase.Match_ForTestOnly());

			TestDSC = (Discount)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount);
			TestMatchingBase.AddIMatching(TestDSC);

			Assert("These transactions should be matchable", TestMatchingBase.Match_ForTestOnly());
		}

		#endregion

		#region TestARMatching_TwoInvoiceAndOneReceiptFromDifferentOrg

		public virtual void TestARMatching_TwoInvoiceAndOneReceiptFromDifferentOrg()
		{
			SetUpTestDataSet();
			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.Lines[0].AL_AT = ZGuid.Empty;
			TestARInvoice1.Lines[0].AL_OSExTaxAmount = 50m;
			TestARInvoice1.AH_FullyPaidDate = ZDateTime.Empty;

			TestARInvoice2.AH_OH = TestOrg2.PK;
			TestARInvoice2.Lines[0].AL_AT = ZGuid.Empty;
			TestARInvoice2.Lines[0].AL_OSExTaxAmount = 60m;
			TestARInvoice2.AH_FullyPaidDate = ZDateTime.Empty;

			TestARReceipt.AH_LocalExTaxAmount = 100;
			TestARReceipt.AH_OSTotalAmount = 100;
			TestARReceipt.AH_OH = TestOrg1.PK;
			TestARReceipt.AH_FullyPaidDate = ZDateTime.Empty;

			/*
			 * Balance of TestOrg1 is -50
			 * Balance of TestOrg2 is 60
			 */

			Factory.Save(); // for DBOnlyQuery in PrimaryOrg
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestMatchingBase.AddIMatching(TestARInvoice1);
			TestMatchingBase.AddIMatching(TestARInvoice2);
			TestMatchingBase.AddIMatching(TestARReceipt);
			TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();
			TestMatchingBase.MatchDate = ZDateTime.Now.AddDays(-2).Date;

			AssertEquals("Balance should be 10", 10M, TestMatchingBase.MatchedTransactions.Balance);
			Assert("Should not allow a match since balance is not 0", !TestMatchingBase.Match_ForTestOnly());

			var testARInvoice3 = Factory.NewWithValidTestData<ARInvoice>();
			TestObjectCreator.CreateInvoiceLine(testARInvoice3, testARInvoice3.TransactionCurrency, 1m, 0m);
			testARInvoice3.AH_OH = TestOrg2.PK;
			testARInvoice3.AH_FullyPaidDate = ZDateTime.Empty;
			testARInvoice3.AH_LocalExTaxAmount = 50;
			testARInvoice3.Lines[0].AL_AT = ZGuid.Empty;
			testARInvoice3.Lines[0].AL_OSExTaxAmount = 50;

			TestMatchingBase.RemoveAllInMatchedTransactions();
			TestMatchingBase.AddIMatching(TestARInvoice1);
			TestMatchingBase.AddIMatching(testARInvoice3);
			TestMatchingBase.AddIMatching(TestARReceipt);
			TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();
			Assert("Should allow a match since balance is 0", TestMatchingBase.Match_ForTestOnly());

			AssertEquals("One ARTransfer from TestOrg2 to TestOrg1 should be dynamically created", 1, TestMatchingBase.DynamicTransactions.Count);
			Assert("One ARTransfer from TestOrg2 to TestOrg1 should be dynamically created", TestMatchingBase.DynamicTransactions[0] is ARTransfer);

			Transfer dynamicTransfer = (ARTransfer)TestMatchingBase.DynamicTransactions[0];

			AssertEquals("Should post with the same date as the match date", TestMatchingBase.MatchDate, dynamicTransfer.TransferFrom.AH_PostDate);
			AssertEquals("Should post with the same date as the match date", TestMatchingBase.MatchDate, dynamicTransfer.TransferTo.AH_PostDate);
			AssertEquals("The TransferFrom row should have PK of TestOrg2", TestOrg2.PK, dynamicTransfer.TransferFrom.AH_OH);
			AssertEquals("The TransferTo row should have PK of TestOrg1", TestOrg1.PK, dynamicTransfer.TransferTo.AH_OH);

			AssertEquals("The TransferFrom row should have amount of -50", -50M, dynamicTransfer.TransferFrom.AH_InvoiceAmount);
			AssertEquals("The TransferTo row should have amount of 50", 50M, dynamicTransfer.TransferTo.AH_InvoiceAmount);

			// Check that each organization balances to 0
			IMatchingCollection testIMatchings = new IMatchingCollection(Factory);
			testIMatchings.AddRange(TestMatchingBase.MatchedTransactions);
			testIMatchings.AddRange(TestMatchingBase.DynamicTransactions);

			AssertEquals("TestOrg1 should now balance to 0", 0M, testIMatchings.GetOrganizationBalanceAmount(TestOrg1.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("TestOrg2 should now balance to 0", 0M, testIMatchings.GetOrganizationBalanceAmount(TestOrg2.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
		}

		#endregion

		#region TestAPMatching_TwoInvoicesAndOnePaymentFromDifferentOrg

		public virtual void TestAPMatching_TwoInvoicesAndOnePaymentFromDifferentOrg()
		{
			SetUpTestDataSet();
			TestAPInvoice1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice1, TestAPInvoice1.TransactionCurrency, TestAPInvoice1.AH_ExchangeRate, 38m, 0m, 0m, 38m, 0m, 0m);

			TestAPInvoice2.AH_OH = TestOrg2.PK;
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice2, TestAPInvoice2.TransactionCurrency, TestAPInvoice2.AH_ExchangeRate, 42m, 0m, 0m, 42m, 0m, 0m);

			TestAPPayment.AH_LocalExTaxAmount = 81;
			TestAPPayment.AH_OSTotalAmount = 81;
			TestAPPayment.AH_OH = TestOrg2.PK;

			/*
			 * TestOrg1 Balance is -38
			 * TestOrg2 Balance is 39
			 */

			Factory.Save(); // ** required for Dirty Table test
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestMatchingBase.AddIMatching(TestAPInvoice1);
			TestMatchingBase.AddIMatching(TestAPInvoice2);
			TestMatchingBase.AddIMatching(TestAPPayment);
			TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

			AssertEquals("The balance should be 1", 1M, TestMatchingBase.MatchedTransactions.Balance);
			Assert("Should not allow a match since balance is not 0", !TestMatchingBase.Match_ForTestOnly());

			var testAPPayment2 = Factory.NewWithValidTestData<APPayment>();
			testAPPayment2.AH_LocalExTaxAmount = 80;
			testAPPayment2.AH_OSTotalAmount = 80;
			testAPPayment2.AH_OH = TestOrg2.PK;

			TestMatchingBase.RemoveAllInMatchedTransactions();
			TestMatchingBase.AddIMatching(TestAPInvoice1);
			TestMatchingBase.AddIMatching(TestAPInvoice2);
			TestMatchingBase.AddIMatching(testAPPayment2);

			TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();
			//TestAPPayment.AH_OutstandingAmount = 80;	// ** required because not set automatically
			Assert("Should allow a match since balance is 0", TestMatchingBase.Match_ForTestOnly());

			AssertEquals("One APTransfer from TestOrg2 to TestOrg1 should be created", 1, TestMatchingBase.DynamicTransactions.Count);
			Assert("One APTransfer from TestOrg2 to TestOrg1 should be created", TestMatchingBase.DynamicTransactions[0] is APTransfer);

			APTransfer dynamicTransfer = (APTransfer)TestMatchingBase.DynamicTransactions[0];

			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load(new ZQuery());

			AssertEquals("5 Match link Rows should be created", 5, matchLinks.Count);

			// test contents of Matchlink created for Invoice1
			ZQuery aPInvoice1MatchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestAPInvoice1.PK);
			TransactionMatchLinkCollection aPInvoice1MatchLinks = new TransactionMatchLinkCollection(Factory);
			aPInvoice1MatchLinks.Load(aPInvoice1MatchLinkFilter);

			TransactionMatchLink invoice1MatchLink = aPInvoice1MatchLinks[0];
			AssertEquals("1 Matchlink should be created for Invoice1", 1, aPInvoice1MatchLinks.Count);
			AssertEquals("Invoice1 Matchlink should have amount -38", -38M, invoice1MatchLink.AP_Amount);
			AssertEquals("Invoice1 Matchlink should have GST Realised equal to 0", 0M, invoice1MatchLink.AP_GSTRealised);

			ZString matchGroup = invoice1MatchLink.AP_MatchGroupNum;
			ZDateTime matchDate = invoice1MatchLink.AP_MatchDate;

			// Test contents of MatchLink created for invoice2
			ZQuery invoice2MatchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestAPInvoice2.PK);
			TransactionMatchLinkCollection invoice2MatchLinks = new TransactionMatchLinkCollection(Factory);
			invoice2MatchLinks.Load(invoice2MatchLinkFilter);

			TransactionMatchLink invoice2MatchLink = invoice2MatchLinks[0];
			AssertEquals("1 MatchLink should be created for Invoice2", 1, invoice2MatchLinks.Count);
			AssertEquals("Invoice2 Matchlink should have amount -42", -42M, invoice2MatchLink.AP_Amount);
			AssertEquals("Invoice2 Matchlink should have GST Realised equal to 0", 0M, invoice2MatchLink.AP_GSTRealised);
			AssertEquals("Invoice2 Matchlink should have the same match group number as invoice1 matchlink", matchGroup, invoice2MatchLink.AP_MatchGroupNum);
			AssertEquals("Invoice2 Matchlink should have the same match date as invoice1 matchlink", matchDate, invoice2MatchLink.AP_MatchDate);

			// Test contents of Matchlink created for APPayment
			ZQuery paymentMatchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, testAPPayment2.PK);
			TransactionMatchLinkCollection paymentMatchLinks = new TransactionMatchLinkCollection(Factory);
			paymentMatchLinks.Load(paymentMatchLinkFilter);

			TransactionMatchLink paymentMatchLink = paymentMatchLinks[0];
			AssertEquals("1 Matchlink should be created for APPayment", 1, paymentMatchLinks.Count);
			AssertEquals("APPayment matchlink should have amount 80", 80M, paymentMatchLink.AP_Amount);
			AssertEquals("APPayment matchlink should have GST Realised equal to 0", 0M, paymentMatchLink.AP_GSTRealised);
			AssertEquals("APPayment matchlink should have the same match group num as invoice1 matchlink", matchGroup, paymentMatchLink.AP_MatchGroupNum);
			AssertEquals("APPayment matchlink should have the same match date as invoice1 matchlink", matchDate, paymentMatchLink.AP_MatchDate);

			// Test contents of Matchlink created for Dynamic Transfer From row
			ZQuery transferFromMatchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, dynamicTransfer.TransferFrom.PK);
			TransactionMatchLinkCollection transferFromMatchLinks = new TransactionMatchLinkCollection(Factory);
			transferFromMatchLinks.Load(transferFromMatchLinkFilter);

			TransactionMatchLink transferFromMatchLink = transferFromMatchLinks[0];
			AssertEquals("1 Matchlink should be created for DynamicTransferFrom row", 1, transferFromMatchLinks.Count);
			AssertEquals("TransferFrom matchlink should have amount -38", -38M, transferFromMatchLink.AP_Amount);
			AssertEquals("TransferFrom matchlink should have the same match group num as invoice1 matchlink", matchGroup, transferFromMatchLink.AP_MatchGroupNum);
			AssertEquals("TransferFrom matchlink should have the same match date as invoice1 matchlink", matchDate, transferFromMatchLink.AP_MatchDate);

			// Test contents of Matchlink created for Dynamic Transfer To Row
			ZQuery transferToMatchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, dynamicTransfer.TransferTo.PK);
			TransactionMatchLinkCollection transferToMatchLinks = new TransactionMatchLinkCollection(Factory);
			transferToMatchLinks.Load(transferToMatchLinkFilter);

			TransactionMatchLink transferToMatchLink = transferToMatchLinks[0];
			AssertEquals("1 Matchlink should be created for DynamicTransferToRow", 1, transferToMatchLinks.Count);
			AssertEquals("TransferTo matchlink should have amount 38", 38M, transferToMatchLink.AP_Amount);
			AssertEquals("TransferTo matchlink should have the same match group num as invoice1 matchlink", matchGroup, transferToMatchLink.AP_MatchGroupNum);
			AssertEquals("TransferTo matchlink should have the same match date as invoice1 matchlink", matchDate, transferToMatchLink.AP_MatchDate);

			// Test contents of dynamically created Transfer
			AssertEquals("The TransferFrom row should have a PK of TestOrg2", TestOrg2.PK, dynamicTransfer.TransferFrom.AH_OH);
			AssertEquals("The TransferTo row should have PK of TestOrg1", TestOrg1.PK, dynamicTransfer.TransferTo.AH_OH);

			AssertEquals("The TransferFrom row should have amount of -38", -38M, dynamicTransfer.TransferFrom.AH_InvoiceAmount);
			AssertEquals("The TransferTo row should have amount of 38", 38M, dynamicTransfer.TransferTo.AH_InvoiceAmount);

			IMatchingCollection validatingCollection = new IMatchingCollection(Factory);
			validatingCollection.AddRange(TestMatchingBase.MatchedTransactions);
			validatingCollection.AddRange(TestMatchingBase.DynamicTransactions);

			AssertEquals("Balance of TestOrg1 should be 0", 0M, validatingCollection.GetOrganizationBalanceAmount(TestOrg1.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of TestOrg2 should be 0", 0M, validatingCollection.GetOrganizationBalanceAmount(TestOrg2.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));

			// Test that created transactions are matched
			AssertNotNull("Fully paid date should not be null on APInvoice1", TestAPInvoice1.AH_FullyPaidDate);
			AssertEquals("Outstanding amount should be 0 on APInvoice1", 0M, TestAPInvoice1.AH_OutstandingAmount);

			AssertNotNull("Fully paid date should not be null on APInvoice2", TestAPInvoice2.AH_FullyPaidDate);
			AssertEquals("Outstanding amount should be 0 on APInvoice2", 0M, TestAPInvoice2.AH_OutstandingAmount);

			AssertNotNull("Fully paid date should not be null on APPayment", testAPPayment2.AH_FullyPaidDate);
			AssertEquals("Outstanding amount should be 0 on APPayment", 0M, testAPPayment2.AH_OutstandingAmount);

			AssertNotNull("Fully paid date on TransferFrom row should not be null", dynamicTransfer.TransferFrom.AH_FullyPaidDate);
			AssertEquals("Outstanding amount should be 0 on TransferFrom row", 0M, dynamicTransfer.TransferFrom.AH_OutstandingAmount);

			AssertNotNull("Fully paid date on TransferTo row should not be null", dynamicTransfer.TransferTo.AH_FullyPaidDate);
			AssertEquals("Outstanding amount should be 0 on TransferTo row", 0M, dynamicTransfer.TransferTo.AH_OutstandingAmount);
		}

		#endregion

		#region TestARMatchingInvoiceAndTransfer

		public virtual void TestARMatchingInvoiceAndTransfer()
		{
			SetUpTestDataSet();
			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(TestARInvoice1, TestARInvoice1.TransactionCurrency, TestARInvoice1.AH_ExchangeRate, 20m, 0m, 0m, 20m, 0m, 0m);

			ARTransferFromRow testARFromRow = Factory.NewWithValidTestData<ARTransferFromRow>();
			testARFromRow.AH_OH = TestOrg2.PK;
			testARFromRow.AH_LocalExTaxAmount = 20M;
			testARFromRow.AH_OSTotalAmount = 20M;

			Factory.Save();

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestMatchingBase.AddIMatching(TestARInvoice1);
			TestMatchingBase.AddIMatching(testARFromRow);
			TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

			Assert("Should match since balance is 0", TestMatchingBase.Match_ForTestOnly());

			ARTransfer dynamicTransfer = (ARTransfer)TestMatchingBase.DynamicTransactions[0];
			AssertEquals("One transfer should be created", 1, TestMatchingBase.DynamicTransactions.Count);

			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load(new ZQuery());

			AssertEquals("4 matchlinks should be created", 4, matchLinks.Count);

			TransactionMatchLinkCollection invoiceMatchLinks = new TransactionMatchLinkCollection(Factory);
			ZQuery invoiceMatchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestARInvoice1.PK);
			invoiceMatchLinks.Load(invoiceMatchLinkFilter);

			TransactionMatchLink invoiceMatchLink = invoiceMatchLinks[0];
			AssertEquals("1 matchlink should be created for the invoice", 1, invoiceMatchLinks.Count);
			AssertEquals("Invoice matchlink should have amount 20", 20M, invoiceMatchLink.AP_Amount);
			AssertEquals("Value of GST Realised should be 0 on invoice matchlink", 0M, invoiceMatchLink.AP_GSTRealised);

			ZString matchGroupNumber = invoiceMatchLink.AP_MatchGroupNum;
			ZDateTime matchDate = invoiceMatchLink.AP_MatchDate;

			ZQuery originalTransferMatchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, testARFromRow.PK);
			TransactionMatchLinkCollection originalTransferMatchLinks = new TransactionMatchLinkCollection(Factory);
			originalTransferMatchLinks.Load(originalTransferMatchLinkFilter);

			TransactionMatchLink origTransferMatchLink = originalTransferMatchLinks[0];
			AssertEquals("1 matchlink should be created for the original transfer row", 1, originalTransferMatchLinks.Count);
			AssertEquals("Transfer matchlink should have amount -20", -20M, origTransferMatchLink.AP_Amount);
			AssertEquals("Value of GST Realised should be 0 on transfer matchlink", 0M, origTransferMatchLink.AP_GSTRealised);
			AssertEquals("MatchGroup num should be the same as for the invoice matchlink", matchGroupNumber, origTransferMatchLink.AP_MatchGroupNum);
			AssertEquals("MatchDate should be the same as for the invoice matchlink", matchDate, origTransferMatchLink.AP_MatchDate);

			// Testing contents of Matchlink for dynamically created Transfer From row
			ZQuery transferFromMatchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, dynamicTransfer.TransferFrom.PK);
			TransactionMatchLinkCollection transferFromMatchLinks = new TransactionMatchLinkCollection(Factory);
			transferFromMatchLinks.Load(transferFromMatchLinkFilter);

			TransactionMatchLink transferFromMatchLink = transferFromMatchLinks[0];
			AssertEquals("1 matchlink should be created for the dynamically created transfer from row", 1, transferFromMatchLinks.Count);
			AssertEquals("TransferFrom matchlink should have amount 20", 20M, transferFromMatchLink.AP_Amount);
			AssertEquals("Value of GST Realised should be 0 on Dynamic Transfer From matchlink", 0M, transferFromMatchLink.AP_GSTRealised);
			AssertEquals("MatchGroup num should be the same as for the invoice matchlink", matchGroupNumber, transferFromMatchLink.AP_MatchGroupNum);
			AssertEquals("MatchDate should be the same as for the invoice matchlink", matchDate, transferFromMatchLink.AP_MatchDate);

			// Testing contents of the Dynamically Created Transfer
			AssertEquals("FromRow of Dynamic Transfer should be for TestOrg2", dynamicTransfer.TransferFrom.AH_OH, TestOrg2.PK);
			AssertEquals("FromRow of Dynamic Transfer should have amount 20", 20M, dynamicTransfer.TransferFrom.AH_InvoiceAmount);

			AssertEquals("ToRow of Dynamic Transfer should be for TestOrg1", dynamicTransfer.TransferTo.AH_OH, TestOrg1.PK);
			AssertEquals("ToRow of Dynamic Transfer should have amount -20", -20M, dynamicTransfer.TransferTo.AH_InvoiceAmount);

			// Testing contents of dynamically created Transfer To row
			ZQuery transferToMatchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, dynamicTransfer.TransferTo.PK);
			TransactionMatchLinkCollection transferToMatchLinks = new TransactionMatchLinkCollection(Factory);
			transferToMatchLinks.Load(transferToMatchLinkFilter);

			TransactionMatchLink transferToMatchLink = transferToMatchLinks[0];
			AssertEquals("1 matchlink should be created for the dynamically created transfer to row", 1, transferToMatchLinks.Count);
			AssertEquals("TransferTo matchlink should have amount -20", -20M, transferToMatchLink.AP_Amount);
			AssertEquals("Value of GST Realised should be 0 on match link row for DynamicTransferFromRow", 0M, transferToMatchLink.AP_GSTRealised);
			AssertEquals("MatchGroup num should be the same as for the invoice matchlink", matchGroupNumber, transferToMatchLink.AP_MatchGroupNum);
			AssertEquals("MatchDate should be the same as for the invoice matchlink", matchDate, transferToMatchLink.AP_MatchDate);

			// Test that all accounts balance to 0
			IMatchingCollection validatingCollection = new IMatchingCollection(Factory);
			validatingCollection.AddRange(TestMatchingBase.MatchedTransactions);
			validatingCollection.AddRange(TestMatchingBase.DynamicTransactions);

			AssertEquals("Balance of TestOrg1 should be 0", 0M, validatingCollection.GetOrganizationBalanceAmount(TestOrg1.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of TestOrg2 should be 0", 0M, validatingCollection.GetOrganizationBalanceAmount(TestOrg2.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));

			// Test that all transactions are correctly matched
			AssertNotNull("FullyPaid date on TestARInvoice1 should not be null", TestARInvoice1.AH_FullyPaidDate);
			AssertEquals("Outstanding amount on TestARInvoice1 should be 0", 0M, TestARInvoice1.AH_OutstandingAmount);

			AssertNotNull("FullyPaid date on TestARFromRow should not be null", testARFromRow.AH_FullyPaidDate);
			AssertEquals("Outstanding amount on TestARFromRow should be 0", 0M, testARFromRow.AH_OutstandingAmount);

			AssertNotNull("FullyPaid date on DynamicFromRow should not be null", dynamicTransfer.TransferFrom.AH_FullyPaidDate);
			AssertEquals("Outstanding amount on DynamicFromRow should be 0", 0M, dynamicTransfer.TransferFrom.AH_OutstandingAmount);

			AssertNotNull("FullyPaid date on DynamicToRow should not be null", dynamicTransfer.TransferTo.AH_FullyPaidDate);
			AssertEquals("Outstanding amount on DynamicToRow should be 0", 0M, dynamicTransfer.TransferTo.AH_OutstandingAmount);
		}

		#endregion

		#region TestAPMatchingReceiptAndContra

		public virtual void TestAPMatchingReceiptAndContra()
		{
			SetUpTestDataSet();
			APReceipt testAPReceipt = Factory.NewWithValidTestData<APReceipt>();
			testAPReceipt.AH_OH = TestOrg1.PK;
			testAPReceipt.AH_LocalExTaxAmount = 20M;
			testAPReceipt.AH_OSTotalAmount = 20M;

			APContraRow testAPContraRow = Factory.NewWithValidTestData<APContraRow>();
			testAPContraRow.AH_OH = TestOrg2.PK;
			testAPContraRow.AH_InvoiceAmount = 20M;
			testAPContraRow.AH_OutstandingAmount = 20M;
			testAPContraRow.AH_OSTotal = 20M;

			Factory.Save();

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestMatchingBase.AddIMatching(testAPReceipt);
			TestMatchingBase.AddIMatching(testAPContraRow);
			TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

			Assert("Should match since balance is 0", TestMatchingBase.Match_ForTestOnly());

			AssertEquals("1 APTransfer should be dynamically created", 1, TestMatchingBase.DynamicTransactions.Count);
			APTransfer dynamicTransfer = (APTransfer)TestMatchingBase.DynamicTransactions[0];

			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load(new ZQuery());

			AssertEquals("4 Matchlink rows should be dynamically created", 4, matchLinks.Count);

			// Testing Contents of Matchlink row for APContraRow
			ZQuery aPContraRowMatchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, testAPContraRow.PK);
			TransactionMatchLinkCollection aPContraRowMatchLinks = new TransactionMatchLinkCollection(Factory);
			aPContraRowMatchLinks.Load(aPContraRowMatchLinkFilter);

			AssertEquals("1 matchlink row should be created for APContraRow", 1, aPContraRowMatchLinks.Count);

			TransactionMatchLink aPContraRowMatchLink = aPContraRowMatchLinks[0];
			AssertEquals("APContraRow matchlink should have amount 20", 20M, aPContraRowMatchLink.AP_Amount);
			AssertEquals("APContraRow matchlink should have GST Realised set to 0", 0M, aPContraRowMatchLink.AP_GSTRealised);

			ZString matchGroupNum = aPContraRowMatchLink.AP_MatchGroupNum;
			ZDateTime matchDate = aPContraRowMatchLink.AP_MatchDate;

			// Test contents of Matchlink row for APReceipt
			ZQuery aPReceiptMatchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, testAPReceipt.PK);
			TransactionMatchLinkCollection aPReceiptMatchLinks = new TransactionMatchLinkCollection(Factory);
			aPReceiptMatchLinks.Load(aPReceiptMatchLinkFilter);

			TransactionMatchLink aPReceiptMatchLink = aPReceiptMatchLinks[0];

			AssertEquals("1 Matchlink should be created for APReceipt", 1, aPReceiptMatchLinks.Count);
			AssertEquals("APReceipt matchlink should have amount -20", -20M, aPReceiptMatchLink.AP_Amount);
			AssertEquals("APReceipt matchlink should have the same match group num as APContraRow matchlink", matchGroupNum, aPReceiptMatchLink.AP_MatchGroupNum);
			AssertEquals("APReceipt matchlink should have the same match date as APContraRow matchlink", matchDate, aPReceiptMatchLink.AP_MatchDate);

			// Test contents of Matchlink row for TransferFrom
			ZQuery transferFromMatchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, dynamicTransfer.TransferFrom.PK);
			TransactionMatchLinkCollection transferFromMatchLinks = new TransactionMatchLinkCollection(Factory);
			transferFromMatchLinks.Load(transferFromMatchLinkFilter);

			TransactionMatchLink transferFromMatchLink = transferFromMatchLinks[0];

			AssertEquals("1 Matchlink should be created for TransferFrom", 1, transferFromMatchLinks.Count);
			AssertEquals("TransferFrom matchlink should have amount -20", -20M, transferFromMatchLink.AP_Amount);
			AssertEquals("TransferFrom matchlink should have the same match group num as APContraRow matchlink", matchGroupNum, transferFromMatchLink.AP_MatchGroupNum);
			AssertEquals("TransferFrom matchlink should have the same match date as APContraRow matchlink", matchDate, transferFromMatchLink.AP_MatchDate);

			// Test contents of Matchlink row for TransferTo
			ZQuery transferToMatchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, dynamicTransfer.TransferTo.PK);
			TransactionMatchLinkCollection transferToMatchLinks = new TransactionMatchLinkCollection(Factory);
			transferToMatchLinks.Load(transferToMatchLinkFilter);

			TransactionMatchLink transferToMatchLink = transferToMatchLinks[0];

			AssertEquals("1 Matchlink should be created for TransferTo", 1, transferToMatchLinks.Count);
			AssertEquals("TransferTo matchlink should have amount 20", 20M, transferToMatchLink.AP_Amount);
			AssertEquals("TransferTo matchlink should have the same match group num as APContraRow matchlink", matchGroupNum, transferToMatchLink.AP_MatchGroupNum);
			AssertEquals("TransferTo matchlink should have the same match date as APContraRow matchlink", matchDate, transferToMatchLink.AP_MatchDate);

			// Test that each account balances to 0
			IMatchingCollection validatingCollection = new IMatchingCollection(Factory);
			validatingCollection.AddRange(TestMatchingBase.MatchedTransactions);
			validatingCollection.AddRange(TestMatchingBase.DynamicTransactions);

			AssertEquals("Balance of TestOrg1 should be 0", 0M, validatingCollection.GetOrganizationBalanceAmount(TestOrg1.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of TestOrg2 should be 0", 0M, validatingCollection.GetOrganizationBalanceAmount(TestOrg2.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
		}

		#endregion

		#region TestMatchingARInvoiceAndAPInvoiceFromDifferentOrg

		public virtual void TestMatchingARInvoiceAndAPInvoiceFromDifferentOrg()
		{
			SetUpTestDataSet();
			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(TestARInvoice1, GlbCompany.CurrentCompany.LocalCurrency, 1m, 34.56M, 0m, 0m, 34.56M, 0m, 0m);

			TestAPInvoice1.AH_OH = TestOrg2.PK;
			TestAPInvoice1.AH_LocalExTaxAmount = 34.56M;
			TestAPInvoice1.AH_OSTotalAmount = 34.56M;
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice1, GlbCompany.CurrentCompany.LocalCurrency, 1m, 34.56M, 0m, 0m, 34.56M, 0m, 0m);

			Factory.Save(); // ** required for dirty table test
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			TestMatchingBase.AddIMatching(TestARInvoice1);
			TestMatchingBase.AddIMatching(TestAPInvoice1);
			TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

			Assert("Should match since balance is 0", TestMatchingBase.Match_ForTestOnly());

			Contra dynamicContra = (Contra)TestMatchingBase.DynamicTransactions[0];
			AssertNotNull("A Contra should be dynamically created", dynamicContra);

			AssertEquals("outstanding amount on ARInvoice should be 0", 0M, TestARInvoice1.AH_OutstandingAmount);
			AssertEquals("outstanding amount on APInvoice should be 0", 0M, TestAPInvoice1.AH_OutstandingAmount);
			AssertEquals("outstanding amount on ARContraRow should be 0", 0M, dynamicContra.ARRow.AH_OutstandingAmount);
			AssertEquals("outstanding amount on APContraRow should be 0", 0M, dynamicContra.APRow.AH_OutstandingAmount);

			AssertNotNull("Fully Paid Date should be set", TestARInvoice1.AH_FullyPaidDate);
			AssertNotNull("Fully Paid Date should be set", TestAPInvoice1.AH_FullyPaidDate);
			AssertNotNull("Fully Paid Date should be set", dynamicContra.ARRow.AH_FullyPaidDate);
			AssertNotNull("Fully Paid Date should be set", dynamicContra.APRow.AH_FullyPaidDate);

			// 4 matchlink rows should be created - 1 for each contra row, 1 for each invoice
			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load(new ZQuery());
			AssertEquals("4 matchlink rows should be created", 4, matchLinks.Count);

			// Check contents of matchlink row for ARContraRow
			TransactionMatchLinkCollection aRContraRowMatchLinks = new TransactionMatchLinkCollection(Factory);
			ZQuery aRContraRowMatchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, dynamicContra.ARRow.PK);
			aRContraRowMatchLinks.Load(aRContraRowMatchLinkFilter);

			TransactionMatchLink aRContraRowMatchLink = aRContraRowMatchLinks[0];

			AssertEquals("There should be a matchlink for the ARContraRow", 1, aRContraRowMatchLinks.Count);
			AssertEquals("The ARContraRow Matchlink should have amount -34.56", -34.56M, aRContraRowMatchLink.AP_Amount);
			AssertEquals("The value of GST Realised should be 0", 0M, aRContraRowMatchLink.AP_GSTRealised);

			ZString matchGroupNum = aRContraRowMatchLink.AP_MatchGroupNum;
			ZDateTime matchDate = aRContraRowMatchLink.AP_MatchDate;

			// Check contents of matchlink row for APContraRow
			ZQuery aPContraRowMatchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, dynamicContra.APRow.PK);
			TransactionMatchLink aPContraRowMatchLink = Factory.LoadTop1<TransactionMatchLink>(aPContraRowMatchLinkFilter);

			AssertNotNull("There should be a matchlink for the APContraRow", aPContraRowMatchLink);
			AssertEquals("The APContraRow Matchlink should have amount 34.56", 34.56M, aPContraRowMatchLink.AP_Amount);
			AssertEquals("The value of GST Realised should be 0", 0M, aPContraRowMatchLink.AP_GSTRealised);
			AssertEquals("The match group num should be the same", matchGroupNum, aPContraRowMatchLink.AP_MatchGroupNum);
			AssertEquals("The match date should be the same", matchDate, aPContraRowMatchLink.AP_MatchDate);

			// Check contents of matchlink row for APInvoice
			ZQuery aPInvoiceMatchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestAPInvoice1.PK);
			TransactionMatchLink aPInvoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(aPInvoiceMatchLinkFilter);

			AssertNotNull("There should be a matchlink for the APInvoice", aPInvoiceMatchLink);
			AssertEquals("The APInvoice Matchlink should have amount -34.56", -34.56M, aPInvoiceMatchLink.AP_Amount);
			AssertEquals("The match group num should be the same", matchGroupNum, aPInvoiceMatchLink.AP_MatchGroupNum);
			AssertEquals("The match date should be the same", matchDate, aPInvoiceMatchLink.AP_MatchDate);
		}

		#endregion

		#region TestMatchingContrasAndInvoicesFromBothLedgers

		public virtual void TestMatchingContrasAndInvoicesFromBothLedgers()
		{
			SetUpTestDataSet();
			TestMatchingBase.PrimaryOrganization = TestOrg3.PK;

			Contra testContra1 = Contra.New(Factory);
			testContra1.APRow.AH_OH = TestOrg1.PK;
			testContra1.APRow.AH_LocalExTaxAmount = 48M;
			testContra1.APRow.AH_OSTotalAmount = 48M;

			testContra1.ARRow.AH_OH = TestOrg2.PK;
			testContra1.ARRow.AH_LocalExTaxAmount = 48M;
			testContra1.ARRow.AH_OSTotalAmount = 48M;

			TestAPInvoice1.AH_OH = TestOrg3.PK;
			TestAPInvoice1.AH_LocalExTaxAmount = 80M;
			TestAPInvoice1.AH_OSTotalAmount = 80M;
			InvoicingLineBase line = (InvoicingLineBase)TestAPInvoice1.Lines.AddNew();
			line.AL_LineAmount = -80M;
			line.AL_OverseasTotal = 80M;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			TestAPInvoice1.AH_FullyPaidDate = ZDateTime.Empty;

			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_LocalExTaxAmount = 40M;
			TestARInvoice1.AH_OSTotalAmount = 40M;
			line = (InvoicingLineBase)TestARInvoice1.Lines.AddNew();
			line.AL_LineAmount = 40M;
			line.AL_OverseasTotal = 40M;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			TestARInvoice1.AH_FullyPaidDate = ZDateTime.Empty;

			TestARInvoice2.AH_OH = TestOrg2.PK;
			TestARInvoice2.AH_LocalExTaxAmount = 40M;
			TestARInvoice2.AH_OSTotalAmount = 40M;
			line = (InvoicingLineBase)TestARInvoice2.Lines.AddNew();
			line.AL_LineAmount = 40M;
			line.AL_OverseasTotal = 40M;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			TestARInvoice2.AH_FullyPaidDate = ZDateTime.Empty;

			Factory.Save();

			TestMatchingBase.AddIMatching(testContra1);
			TestMatchingBase.AddIMatching(TestAPInvoice1);
			TestMatchingBase.AddIMatching(TestARInvoice1);
			TestMatchingBase.AddIMatching(TestARInvoice2);
			TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

			Assert("Should allow matching since balance is zero", TestMatchingBase.Match_ForTestOnly());

			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load(new ZQuery());
			AssertEquals("11 matchlink rows should be created", 11, matchLinks.Count);
		}

		#endregion

		#region TestMatchingINV_CRD_APPaymentFromBothLedgers

		public virtual void TestMatchingINV_CRD_APPaymentFromBothLedgers()
		{
			SetUpTestDataSet();
			SetUpTestDataSet1();

			// TestOrg 1
			TestAPPayment.AH_OH = TestOrg1.PK;
			TestAPPayment.AH_LocalExTaxAmount = 24.24M;
			TestAPPayment.AH_OSTotalAmount = 24.24M;

			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_LocalExTaxAmount = 4.26M;
			TestARInvoice1.AH_OSTotalAmount = 4.26M;
			InvoicingLineBase line = (InvoicingLineBase)TestARInvoice1.Lines.AddNew();
			line.AL_LineAmount = 4.26M;
			line.AL_OverseasTotal = 4.26M;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			TestARInvoice1.AH_FullyPaidDate = ZDateTime.Empty;

			Factory.Save();

			TestMatchingBase.AddIMatching(ARCRD1);
			TestMatchingBase.AddIMatching(TestARInvoice1);
			TestMatchingBase.AddIMatching(TestAPInvoice1);
			TestMatchingBase.AddIMatching(APCRD1);
			TestMatchingBase.AddIMatching(TestAPPayment);
			TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

			TestMatchingBase.Match_ForTestOnly();

			AssertEquals("Eight Contras and Transfers should be created", 8, TestMatchingBase.DynamicTransactions.Count);
		}

		#endregion

		#region TestMatchingWithJournal

		public virtual void TestMatchingWithJournal()
		{
			SetUpTestDataSet();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_OSTotal = 110M;
			TestARInvoice1.AH_LocalExTaxAmount = 100M;
			TestARInvoice1.AH_LocalTaxAmount = 10M;
			TestARInvoice1.AH_LocalOutstandingAmount = 110M;
			InvoicingLineBase line = (InvoicingLineBase)TestARInvoice1.Lines.AddNew();
			line.AL_LineAmount = 100M;
			line.AL_LocalTaxAmount = 10M;
			line.AL_OverseasTotal = 110M;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			TestARInvoice1.AH_FullyPaidDate = ZDateTime.Empty;

			ARJournal testARJournal = Factory.NewWithValidTestData<ARJournal>();
			testARJournal.AH_OH = TestOrg1.PK;
			testARJournal.AH_LocalExTaxAmount = -60M;
			testARJournal.AH_LocalOutstandingAmount = -60M;
			testARJournal.AH_OSTotalAmount = -60M;
			TestMatchingBase.AddToBalancingJournals(testARJournal);

			ARTransferFromRow testFromRow = Factory.NewWithValidTestData<ARTransferFromRow>();
			testFromRow.AH_OH = TestOrg1.PK;
			testFromRow.AH_LocalExTaxAmount = 50M;
			testFromRow.AH_LocalOutstandingAmount = 50M;
			testFromRow.AH_OSTotalAmount = 50M;

			TestMatchingBase.AddIMatching(TestARInvoice1);
			TestMatchingBase.AddIMatching(testARJournal);
			TestMatchingBase.AddIMatching(testFromRow);
			TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

			Assert("These Transactions should be matchable", TestMatchingBase.MatchAndClearTransactions());

			// Check AR Invoice is matched
			AssertNotNull("Fully Paid Date on AR Invoice should not be null", TestARInvoice1.AH_FullyPaidDate);
			AssertEquals("Outstanding amount on AR Invoice should be 0", 0M, TestARInvoice1.AH_OutstandingAmount);

			// Check Transfer From row is matched
			AssertNotNull("Fully Paid Date on Transfer From row should not be null", testFromRow.AH_FullyPaidDate);
			AssertEquals("Outstanding amount on Transfer From row should be 0", 0M, testFromRow.AH_OutstandingAmount);

			// Check Journal is matched
			AssertNotNull("Fully Paid Date on Journal should not be null", testARJournal.AH_FullyPaidDate);
			AssertEquals("Outstanding amount on Journal should be 0", 0M, testARJournal.AH_OutstandingAmount);

			AssertEquals("AR Journal should be cleared", 0, TestMatchingBase.BalancingARJournals.Count);
		}

		public virtual void TestMatchingWithAPJournal()
		{
			SetUpTestDataSet();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			TestAPInvoice1.AH_OH = TestOrg1.PK;
			TestAPInvoice1.AH_ExchangeRate = 1M;
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice1, TestAPInvoice1.TransactionCurrency, TestAPInvoice1.AH_ExchangeRate, 40m, 0m, 0m, 40m, 0m, 0m);
			TestAPInvoice1.AH_LocalOutstandingAmount = 40M;

			APJournal testAPJournal = Factory.NewWithValidTestData<APJournal>();
			testAPJournal.AH_OH = TestOrg1.PK;
			testAPJournal.DebitCreditSign = "CR";
			testAPJournal.AH_LocalExTaxAmount = 30M;
			testAPJournal.AH_LocalOutstandingAmount = 30M;
			testAPJournal.AH_OSTotalAmount = 30M;
			Factory.Save();

			TestMatchingBase.AddToBalancingJournals(testAPJournal);

			APTransferFromRow testFromRow = Factory.NewWithValidTestData<APTransferFromRow>();
			testFromRow.AH_OH = TestOrg1.PK;
			testFromRow.AH_LocalExTaxAmount = 10M;
			testFromRow.AH_LocalOutstandingAmount = 10M;
			testFromRow.AH_OSTotalAmount = 10M;

			TestMatchingBase.AddIMatching(TestAPInvoice1);
			TestMatchingBase.AddIMatching(testAPJournal);
			TestMatchingBase.AddIMatching(testFromRow);
			TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

			Assert("These Transactions should be matchable", TestMatchingBase.MatchAndClearTransactions());

			AssertEquals("AP Journal should be cleared", 0, TestMatchingBase.BalancingAPJournals.Count);
		}

		#endregion

		#region TestPartialPaymentWithNoPartiallyPaidTransactions

		public virtual void TestPartialPaymentWithNoPartiallyPaidTransactions()
		{
			SetUpTestDataSet();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			// This tests the case when none of the transactions in the
			// collection have been partially paid, but one or more
			// must be partially paid for all amounts to balance to 0
			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestAPInvoice1.AH_ExchangeRate = 1M;
			TestObjectCreator.CreateInvoiceLine(TestARInvoice1, TestARInvoice1.TransactionCurrency, TestARInvoice1.AH_ExchangeRate, 10m, 0m, 0m, 10m, 0m, 0m);
			IMatching aRINV_IMatching = TestARInvoice1;
			aRINV_IMatching.OSPartialPaymentAmount = 10M;

			// Credit note that has not been partially paid -
			// we want to partially pay it
			ARCRD1 = Factory.NewWithValidTestData<ARCreditNote>();
			ARCRD1.AH_OH = TestOrg1.PK;
			ARCRD1.AH_ExchangeRate = 1M;
			TestObjectCreator.CreateInvoiceLine(ARCRD1, ARCRD1.TransactionCurrency, ARCRD1.AH_ExchangeRate, 30m, 0m, 0m, 30m, 0m, 0m);
			IMatching aRCRD_IMatching = ARCRD1;
			aRCRD_IMatching.OSPartialPaymentAmount = -10M;

			Factory.Save();

			TestMatchingBase.AddIMatching(aRCRD_IMatching);
			TestMatchingBase.AddIMatching(aRINV_IMatching);

			Assert("Transactions should be matchable", TestMatchingBase.Match_ForTestOnly());

			AssertEquals("No dynamic transactions should be created", 0, TestMatchingBase.DynamicTransactions.Count);

			// Check that original transactions are matched correctly
			AssertNotNull("ARInvoice should be fully paid", TestARInvoice1.AH_FullyPaidDate);
			AssertEquals("ARInvoice should have outstanding amount of 0", 0M, TestARInvoice1.AH_OutstandingAmount);

			Assert("ARCRD1 should not be fully paid", ARCRD1.AH_FullyPaidDate.IsEmpty);
			AssertEquals("ARCRD1 should have outstanding amount of -20", -20M, ARCRD1.AH_OutstandingAmount);

			// Check that matchlink rows are correct
			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load(new ZQuery());
			AssertEquals("There should be 2 matchlinks created", 2, matchLinks.Count);

			// Check the ARINV matchlink
			ZQuery aRINV_Filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestARInvoice1.PK);
			TransactionMatchLink aRINV_MatchLink = Factory.LoadTop1<TransactionMatchLink>(aRINV_Filter);
			AssertNotNull("Matchlink for ARInvoice should exist", aRINV_MatchLink);

			ZString matchGroup = aRINV_MatchLink.AP_MatchGroupNum;
			ZDateTime matchDate = aRINV_MatchLink.AP_MatchDate;
			AssertEquals("ARINV matchlink amount should be 10", 10M, aRINV_MatchLink.AP_Amount);

			// Check the ARCRD matchlink
			ZQuery aRCRD_Filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, ARCRD1.PK);
			TransactionMatchLink aRCRD_MatchLink = Factory.LoadTop1<TransactionMatchLink>(aRCRD_Filter);
			AssertNotNull("Matchlink for ARCRD should exist", aRCRD_MatchLink);

			AssertEquals("Matchgroup should be the same", matchGroup, aRCRD_MatchLink.AP_MatchGroupNum);
			AssertEquals("MatchDate should be the same", matchDate, aRCRD_MatchLink.AP_MatchDate);
			AssertEquals("ARCRD matchlink amount should be -10", -10M, aRCRD_MatchLink.AP_Amount);
		}

		#endregion

		#region Tax Framework Dependencies

		public void TestTaxFrameworkDependencies()
		{
			var realisationEnabler = new TaxRealisationEnabler();
			var withholdingJournalCreationManager = new WithholdingJournalCreationManager();

			TestMatchingBase.SubstituteTaxRealisationEnabler_ForTestOnly(realisationEnabler);
			TestMatchingBase.SubstituteWithholdingJournalCreationManager_ForTestOnly(withholdingJournalCreationManager);

			AssertType(typeof(TaxRealisationEnabler), TestMatchingBase.TaxRealisationEnabler_ExposedForTestOnly);
			AssertType(typeof(WithholdingJournalCreationManager), TestMatchingBase.WithholdingJournalCreationManager_ExposedForTestOnly);
		}

		#endregion

		public void TestIsNewBalancingJournal()
		{
			AssertEquals(false, TestMatchingBase.IsNewBalancingJournal(null));

			var journal = Factory.NewWithValidTestData<APJournal>();
			journal.AH_TransactionCategory = Core.Constants.TransactionCategory.Codes.Standard;
			AssertEquals(true, TestMatchingBase.IsNewBalancingJournal(journal));

			journal.AH_TransactionCategory = Core.Constants.TransactionCategory.Codes.PaymentBasisWithholding;
			AssertEquals(false, TestMatchingBase.IsNewBalancingJournal(journal));

			journal.AH_TransactionCategory = string.Empty;
			AssertEquals(true, TestMatchingBase.IsNewBalancingJournal(journal));

			Factory.Save();

			Assert("Precondition", journal.IsInDatabase);

			AssertEquals(false, TestMatchingBase.IsNewBalancingJournal(journal));
		}

		[ExpectNoExceptions]
		public void TestPrimaryOrganizationSetter_CallsDeleteAllWithholdingJournal_OnWithholdingJournalCreationManager()
		{
			TestOrg1 = TestObjectCreator.ABIGAS;
			TestOrg2 = TestObjectCreator.AALSHI;

			var whtJournalCreationManagerMock = new Mock<IWithholdingJournalCreationManager>();
			whtJournalCreationManagerMock.Setup(x => x.DeleteAllWithholdingJournals());

			TestMatchingBase.SubstituteWithholdingJournalCreationManager_ForTestOnly(null);

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			whtJournalCreationManagerMock.Verify(x => x.DeleteAllWithholdingJournals(), Times.Never);
			whtJournalCreationManagerMock.Invocations.Clear();

			TestMatchingBase.SubstituteWithholdingJournalCreationManager_ForTestOnly(whtJournalCreationManagerMock.Object);

			TestMatchingBase.PrimaryOrganization = TestOrg2.PK;
			Assert("Postcondition", !TestMatchingBase.PrimaryOrganizationInfo.HasErrors());
			whtJournalCreationManagerMock.Verify(x => x.DeleteAllWithholdingJournals(), Times.Once);
			whtJournalCreationManagerMock.Invocations.Clear();

			using (TestMatchingBase.GetValidationSuspender())
			{
				TestMatchingBase.PrimaryOrganizationInfo.AddError("Some Error");
				TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
				Assert("Postcondition", TestMatchingBase.PrimaryOrganizationInfo.HasErrors());
				whtJournalCreationManagerMock.Verify(x => x.DeleteAllWithholdingJournals(), Times.Never); //since there was error on org and setting org to non empty value

				TestMatchingBase.PrimaryOrganizationInfo.ClearAllNotifications();

				TestMatchingBase.PrimaryOrganization = ZGuid.Empty;
				whtJournalCreationManagerMock.Verify(x => x.DeleteAllWithholdingJournals(), Times.Once);
				whtJournalCreationManagerMock.Invocations.Clear();

				TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
				whtJournalCreationManagerMock.Verify(x => x.DeleteAllWithholdingJournals(), Times.Once);
			}
		}

		[ExpectNoExceptions]
		public void TestOnSuccessfulMatching_CallsResetCache_OnWithholdingJournalCreationManager()
		{
			var whtJournalCreationManagerMock = new Mock<IWithholdingJournalCreationManager>();
			whtJournalCreationManagerMock.Setup(x => x.ResetCache());

			TestMatchingBase.SubstituteWithholdingJournalCreationManager_ForTestOnly(null);
			TestMatchingBase.OnSuccessfulMatching_ForTestOnly();
			whtJournalCreationManagerMock.Verify(x => x.ResetCache(), Times.Never);

			TestMatchingBase.SubstituteWithholdingJournalCreationManager_ForTestOnly(whtJournalCreationManagerMock.Object);
			TestMatchingBase.OnSuccessfulMatching_ForTestOnly();
			whtJournalCreationManagerMock.Verify(x => x.ResetCache(), Times.Once);
		}

		public void TestRemovingTransactions_AfterSuccessfulMatching_DoesNotTriggerValidation()
		{
			var invoiceToBeMatched = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			var invoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);

			fTestMatchingBase = GetTestMatchingBase();
			TestMatchingBase.MatchedTransactions.AddTransactionThatMustBeMatched(invoiceToBeMatched);
			TestMatchingBase.MatchedTransactions.Add(invoice);
			var expectedInvoiceCountBefore = fTestMatchingBase is PaymentApprovalMatchingBase ? 3 : 2;
			AssertEquals("Precondition: MatchedTransactions contains expected invoices", expectedInvoiceCountBefore, TestMatchingBase.MatchedTransactions.Count);

			invoiceToBeMatched.AH_IsCancelled = true;       // Will trigger validation error in OnSuccessfulMatching(), when validation is enabled.
			invoice.AH_IsCancelled = true;
			AssertNoRowError("PreCondition: no validation errors for invoice to be matched", invoiceToBeMatched, "You cannot choose a canceled transaction for matching");
			AssertNoRowError("PreCondition: no validation errors for invoice", invoice, "You cannot choose a canceled transaction for matching");

			TestMatchingBase.OnSuccessfulMatching_ForTestOnly();
			var expectedInvoiceCountAfter = expectedInvoiceCountBefore - 1;
			AssertEquals("MatchedTransactions should contain one transaction", expectedInvoiceCountAfter, TestMatchingBase.MatchedTransactions.Count);
			AssertCollectionContains("And that transaction should be the one to be matched", invoiceToBeMatched, TestMatchingBase.MatchedTransactions);
			AssertNoRowError("No validation errors should be triggered for invoice. Validation is pointless after matching has completed successfully and can cause additional database queries.", invoice, "You cannot choose a canceled transaction for matching");

			var anotherInvoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			anotherInvoice.AH_IsCancelled = true;
			TestMatchingBase.MatchedTransactions.Add(anotherInvoice);
			AssertHasRowError("Adding another transaction outside OnSuccessfulMatching() should still trigger validation.", anotherInvoice, "You cannot choose a canceled transaction for matching");
		}

		#region TestWithholdingJournalCreationManager

		public void TestWithholdingJournalCreationManager_MoveFromUnmatchToMatch()
		{
			var matchDate = TestMatchingBase.MatchDate.Date;
			var whtJournalCreationManagerMock = new Mock<IWithholdingJournalCreationManager>();

			TestMatchingBase.SubstituteWithholdingJournalCreationManager_ForTestOnly(null);
			whtJournalCreationManagerMock.Setup(x => x.CreateAPWithholdingJournalsIfApplicable(It.IsAny<IMatching>(), Factory, matchDate)).Returns(Array.Empty<APJournal>());
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			TestMatchingBase.MoveFromUnmatchToMatch(new[] { apInvoice });
			whtJournalCreationManagerMock.Verify(x => x.CreateAPWithholdingJournalsIfApplicable(apInvoice, Factory, matchDate), Times.Never);

			TestMatchingBase.SubstituteWithholdingJournalCreationManager_ForTestOnly(whtJournalCreationManagerMock.Object);
			whtJournalCreationManagerMock.Setup(x => x.ShouldAPWithholdingJournalsBeCreated(It.IsAny<BusinessObjectFactory>())).Returns(true);
			whtJournalCreationManagerMock.Setup(x => x.CreateAPWithholdingJournalsIfApplicable(It.IsAny<IMatching>(), Factory, matchDate)).Returns(Array.Empty<APJournal>());
			var apInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			TestMatchingBase.MoveFromUnmatchToMatch(new[] { apInvoice1 });
			whtJournalCreationManagerMock.Verify(x => x.CreateAPWithholdingJournalsIfApplicable(apInvoice1, Factory, matchDate), Times.Once);

			Assert(TestMatchingBase.MatchedTransactions.Contains(apInvoice1));

			var apJournal1 = Factory.NewWithValidTestData<APJournal>();
			var apJournal2 = Factory.NewWithValidTestData<APJournal>();

			IMatching passedInvoice = null;
			whtJournalCreationManagerMock
				.Setup(x => x.CreateAPWithholdingJournalsIfApplicable(It.IsAny<IMatching>(), Factory, matchDate))
				.Callback<IMatching, BusinessObjectFactory, ZDate>((transaction, factory, date) => passedInvoice = transaction)
				.Returns(new[] { apJournal1, apJournal2 });

			var apInvoice2 = Factory.NewWithValidTestData<APInvoice>();
			TestMatchingBase.MoveFromUnmatchToMatch(new[] { apInvoice2 });
			whtJournalCreationManagerMock.Verify(x => x.CreateAPWithholdingJournalsIfApplicable(apInvoice2, Factory, matchDate), Times.Once);
			AssertEquals("passed invoice", apInvoice2, passedInvoice);

			Assert(TestMatchingBase.MatchedTransactions.Contains(apInvoice2));
			Assert(TestMatchingBase.MatchedTransactions.Contains(apJournal1));
			Assert(TestMatchingBase.MatchedTransactions.Contains(apJournal2));

			AssertEquals("Withholding journals added to balancing journals list", 2, TestMatchingBase.BalancingAPJournals.Count);
			Assert(TestMatchingBase.BalancingAPJournals.Contains(apJournal1));
			Assert(TestMatchingBase.BalancingAPJournals[0].ReadOnly);

			Assert(TestMatchingBase.BalancingAPJournals.Contains(apJournal2));
			Assert(TestMatchingBase.BalancingAPJournals[1].ReadOnly);
		}

		public void TestWithholdingJournalCreationManager_MoveFromMatchToUnmatch_DeletePromptNotShownUnnecessarily()
		{
			TestMatchingBase.AskUserForConfirmation += fAskShouldDeleteWithholdingJournals;

			var whtJournalCreationManagerMock = new Mock<IWithholdingJournalCreationManager>();
			TestMatchingBase.SubstituteWithholdingJournalCreationManager_ForTestOnly(whtJournalCreationManagerMock.Object);

			var exchangeDifference = Factory.NewWithValidTestData<APExchangeDifference>();
			TestMatchingBase.MatchedTransactions.Add(exchangeDifference);

			whtJournalCreationManagerMock.Setup(x => x.CheckIfAnyTransactionIsLinkedToWithholdingJournal(It.IsAny<IEnumerable<IMatching>>())).Returns(false);

			TestMatchingBase.MoveFromMatchToUnmatch(new[] { exchangeDifference });
			AssertEquals("None", UnitTestUserNotification.Instance.LastMessage.ToString().Trim());

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			TestMatchingBase.MatchedTransactions.Add(apInvoice);

			TestMatchingBase.MoveFromMatchToUnmatch(new[] { apInvoice });

			var expectedParameter = new List<IMatching>() { apInvoice };
			whtJournalCreationManagerMock.Verify(x => x.CheckIfAnyTransactionIsLinkedToWithholdingJournal(expectedParameter), Times.Once);

			AssertEquals("None", UnitTestUserNotification.Instance.LastMessage.ToString().Trim());

			Assert("apInvoice should be no longer in MatchedTranasctions", !TestMatchingBase.MatchedTransactions.Contains(apInvoice));
			Assert("apInvoice should be in UnmatchedTranasctions", TestMatchingBase.UnmatchedTransactions.Contains(apInvoice));
		}

		public void TestWithholdingJournalCreationManager_MoveFromMatchToUnmatch_TransactionsWithWithholdingJournals()
		{
			TestMatchingBase.AskUserForConfirmation += fAskShouldDeleteWithholdingJournals;
			var whtJournalCreationManagerMock = new Mock<IWithholdingJournalCreationManager>();
			TestMatchingBase.SubstituteWithholdingJournalCreationManager_ForTestOnly(whtJournalCreationManagerMock.Object);

			var apInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			var apJournal1_1 = Factory.NewWithValidTestData<APJournal>();
			var apJournal1_2 = Factory.NewWithValidTestData<APJournal>();
			TestMatchingBase.MatchedTransactions.Add(apInvoice1);
			TestMatchingBase.MatchedTransactions.Add(apJournal1_1);
			TestMatchingBase.MatchedTransactions.Add(apJournal1_2);

			TestMatchingBase.BalancingAPJournals.Add(apJournal1_1);
			TestMatchingBase.BalancingAPJournals.Add(apJournal1_2);

			var returnValue = new List<APJournal>() { apJournal1_1, apJournal1_2 };
			whtJournalCreationManagerMock.Setup(x => x.CheckIfAnyTransactionIsLinkedToWithholdingJournal(It.IsAny<IEnumerable<IMatching>>())).Returns(true);
			whtJournalCreationManagerMock.Setup(x => x.GetWithholdingJournalsToDelete(It.IsAny<IMatching>())).Returns(returnValue);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			TestMatchingBase.MoveFromMatchToUnmatch(new[] { apInvoice1 });
			var expectedMessage = "This operation will delete Withholding AP Journal(s). Do you want to proceed?";
			AssertContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
			whtJournalCreationManagerMock.Verify(x => x.GetWithholdingJournalsToDelete(It.IsAny<IMatching>()), Times.Never);

			Assert("apInvoice1 should remain in MatchedTransactions list", TestMatchingBase.MatchedTransactions.Contains(apInvoice1));
			Assert("apInvoice1 should not be in UnmatchedTransactions list", !TestMatchingBase.UnmatchedTransactions.Contains(apInvoice1));
			Assert("apJournal1_1 should remain in MatchedTransactions list", TestMatchingBase.MatchedTransactions.Contains(apJournal1_1));
			Assert("apJournal1_2 should remain in MatchedTransactions list", TestMatchingBase.MatchedTransactions.Contains(apJournal1_2));

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			TestMatchingBase.MoveFromMatchToUnmatch(new[] { apInvoice1 });
			AssertContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
			whtJournalCreationManagerMock.Verify(x => x.GetWithholdingJournalsToDelete(It.IsAny<IMatching>()), Times.Once);

			Assert("apInvoice1 not present in MatchedTransactions list", !TestMatchingBase.MatchedTransactions.Contains(apInvoice1));
			Assert("apInvoice1 is now present in UnmatchedTransactions list", TestMatchingBase.UnmatchedTransactions.Contains(apInvoice1));
			Assert("apJournal1_1 is removed from MatchedTransactions list", !TestMatchingBase.MatchedTransactions.Contains(apJournal1_1));
			Assert("apJournal1_2 is removed from MatchedTransactions list", !TestMatchingBase.MatchedTransactions.Contains(apJournal1_2));

			Assert("apJournal1_1 is removed from BalancingAPJournals list", !TestMatchingBase.BalancingAPJournals.Contains(apJournal1_1));
			Assert("apJournal1_2 is removed from BalancingAPJournals list", !TestMatchingBase.BalancingAPJournals.Contains(apJournal1_2));
		}

		public void TestWithholdingJournalCreationManager_MoveFromMatchToUnmatch_WithholdingJournals()
		{
			TestMatchingBase.AskUserForConfirmation += fAskShouldDeleteWithholdingJournals;
			var whtJournalCreationManagerMock = new Mock<IWithholdingJournalCreationManager>();
			TestMatchingBase.SubstituteWithholdingJournalCreationManager_ForTestOnly(whtJournalCreationManagerMock.Object);

			var apJournal1 = Factory.NewWithValidTestData<APJournal>();
			var returnValue = new List<APJournal>() { apJournal1 };
			whtJournalCreationManagerMock.Setup(x => x.CheckIfAnyTransactionIsLinkedToWithholdingJournal(It.IsAny<IEnumerable<IMatching>>())).Returns(true);
			whtJournalCreationManagerMock.Setup(x => x.GetWithholdingJournalsToDelete(It.IsAny<IMatching>())).Returns(returnValue);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			TestMatchingBase.MatchedTransactions.Add(apJournal1);
			TestMatchingBase.BalancingAPJournals.Add(apJournal1);
			TestMatchingBase.MoveFromMatchToUnmatch(new[] { apJournal1 });
			var expectedMessage = "This operation will delete Withholding AP Journal(s). Do you want to proceed?";
			AssertContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
			whtJournalCreationManagerMock.Verify(x => x.GetWithholdingJournalsToDelete(It.IsAny<IMatching>()), Times.Never);
			Assert("apJournal1 should remain in MatchedTranasctions", TestMatchingBase.MatchedTransactions.Contains(apJournal1));
			Assert("apJournal1 should not be in UnmatchedTranasctions", !TestMatchingBase.UnmatchedTransactions.Contains(apJournal1));

			whtJournalCreationManagerMock.Setup(x => x.GetWithholdingJournalsToDelete(It.IsAny<IMatching>())).Returns(returnValue);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			TestMatchingBase.MoveFromMatchToUnmatch(new[] { apJournal1 });
			AssertContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
			whtJournalCreationManagerMock.Verify(x => x.GetWithholdingJournalsToDelete(It.IsAny<IMatching>()), Times.Once);
			Assert("apJournal1 should be no longer in MatchedTranasctions", !TestMatchingBase.MatchedTransactions.Contains(apJournal1));
			Assert("apJournal1 should be not in UnmatchedTranasctions either", !TestMatchingBase.UnmatchedTransactions.Contains(apJournal1));
			Assert("apJournal1 should be no longer in BalancingAPJournals", !TestMatchingBase.BalancingAPJournals.Contains(apJournal1));

			var apJournal2 = Factory.NewWithValidTestData<APJournal>();
			TestMatchingBase.MatchedTransactions.Add(apJournal2);
			TestMatchingBase.MatchedTransactions.AddTransactionThatMustBeMatched(apJournal2);

			returnValue = new List<APJournal>() { };
			whtJournalCreationManagerMock.Setup(x => x.GetWithholdingJournalsToDelete(It.IsAny<IMatching>())).Returns(returnValue);

			TestMatchingBase.MoveFromMatchToUnmatch(new[] { apJournal2 });
			Assert("apJournal2 should still be in MatchedTranasctions", TestMatchingBase.MatchedTransactions.Contains(apJournal2));
			Assert("apJournal2 should not be in UnmatchedTranasctions", !TestMatchingBase.UnmatchedTransactions.Contains(apJournal2));

			returnValue = new List<APJournal>() { apJournal2 };
			whtJournalCreationManagerMock.Setup(x => x.GetWithholdingJournalsToDelete(It.IsAny<IMatching>())).Returns(returnValue);

			TestMatchingBase.MoveFromMatchToUnmatch(new[] { apJournal2 });
			Assert("apJournal2 should still be in MatchedTranasctions", TestMatchingBase.MatchedTransactions.Contains(apJournal2));
			Assert("apJournal2 should not be in UnmatchedTranasctions", !TestMatchingBase.UnmatchedTransactions.Contains(apJournal2));
		}

		public void TestWithholdingJournalCreationManager_MoveFromMatchToUnmatch_BothParentTransactionAndWithholdingJournal()
		{
			TestMatchingBase.AskUserForConfirmation += fAskShouldDeleteWithholdingJournals;
			var whtJournalCreationManagerMock = new Mock<IWithholdingJournalCreationManager>();
			TestMatchingBase.SubstituteWithholdingJournalCreationManager_ForTestOnly(whtJournalCreationManagerMock.Object);

			var apInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			var apJournal1_1 = Factory.NewWithValidTestData<APJournal>();
			var apJournal1_2 = Factory.NewWithValidTestData<APJournal>();

			TestMatchingBase.MatchedTransactions.Add(apInvoice1);
			TestMatchingBase.MatchedTransactions.Add(apJournal1_1);
			TestMatchingBase.MatchedTransactions.Add(apJournal1_2);

			TestMatchingBase.BalancingAPJournals.Add(apJournal1_1);
			TestMatchingBase.BalancingAPJournals.Add(apJournal1_2);

			var returnValue = new List<APJournal>() { apJournal1_1, apJournal1_2 };
			whtJournalCreationManagerMock.Setup(x => x.CheckIfAnyTransactionIsLinkedToWithholdingJournal(It.IsAny<IEnumerable<IMatching>>())).Returns(true);
			whtJournalCreationManagerMock.Setup(x => x.GetWithholdingJournalsToDelete(It.IsAny<IMatching>())).Returns(returnValue);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			TestMatchingBase.MoveFromMatchToUnmatch(new BusinessObject[] { apJournal1_1, apInvoice1 });
			var expectedMessage = "This operation will delete Withholding AP Journal(s). Do you want to proceed?";
			AssertContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
			whtJournalCreationManagerMock.Verify(x => x.GetWithholdingJournalsToDelete(It.IsAny<IMatching>()), Times.Never);
			Assert("apInvoice1 should remain in MatchedTranasctions", TestMatchingBase.MatchedTransactions.Contains(apInvoice1));
			Assert("apJournal1_1 should remain in MatchedTranasctions", TestMatchingBase.MatchedTransactions.Contains(apJournal1_1));
			Assert("apJournal1_2 should remain in MatchedTranasctions", TestMatchingBase.MatchedTransactions.Contains(apJournal1_2));

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			TestMatchingBase.MoveFromMatchToUnmatch(new BusinessObject[] { apJournal1_1, apInvoice1 });
			AssertContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
			whtJournalCreationManagerMock.Verify(x => x.GetWithholdingJournalsToDelete(It.IsAny<IMatching>()), Times.Exactly(2));
			Assert("apInvoice1 should not be in MatchedTranasctions", !TestMatchingBase.MatchedTransactions.Contains(apInvoice1));
			Assert("apInvoice1 should be in UnmatchedTranasctions", TestMatchingBase.UnmatchedTransactions.Contains(apInvoice1));
			Assert("apJournal1_1 should not be in MatchedTranasctions", !TestMatchingBase.MatchedTransactions.Contains(apJournal1_1));
			Assert("apJournal1_2 should not be in MatchedTranasctions", !TestMatchingBase.MatchedTransactions.Contains(apJournal1_2));
			Assert("apJournal1_1 should not be in UnmatchedTranasctions either", !TestMatchingBase.UnmatchedTransactions.Contains(apJournal1_1));
			Assert("apJournal1_2 should not be in UnmatchedTranasctions either", !TestMatchingBase.UnmatchedTransactions.Contains(apJournal1_2));
			Assert("apJournal1_1 should not be in BalancingAPJournals", !TestMatchingBase.BalancingAPJournals.Contains(apJournal1_1));
			Assert("apJournal1_2 should not be in BalancingAPJournals", !TestMatchingBase.BalancingAPJournals.Contains(apJournal1_2));
		}

		void fAskShouldDeleteWithholdingJournals(object sender, UserQueryEventArgs e)
		{
			e.Response = Globals.Message.Show(e.QueryMessage, "some caption", MessageBoxButtons.YesNo, DialogResult.Yes) == DialogResult.Yes;
		}

		#endregion

		#region TestTaxRealisationEnabler

		[TestDate(2020, 01, 15)]
		public virtual void TestTaxRealisationEnabler_CalledWithCorrectParameters()
		{
			var currency = TestObjectCreator.AUD;
			var org = TestObjectCreator.Creditor1;
			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), currency, 1M, org, ZDate.Today.AddDays(-10));
			TestObjectCreator.CreateInvoiceLine(apInvoice, currency, 1M, 100M, 0M, 0M, 100M, 0M, 0M);

			var apCreditNote = TestObjectCreator.CreateInvoice(typeof(APCreditNote), currency, 1M, org, ZDate.Today.AddDays(-5));
			TestObjectCreator.CreateInvoiceLine(apCreditNote, currency, 1M, 100M, 0M, 0M, 100M, 0M, 0M);

			Factory.Save();

			TestMatchingBase.PrimaryOrganization = org.PK;

			IMatching apINV_IMatching = apInvoice;
			apINV_IMatching.OSPartialPaymentAmount = -100M;
			IMatching apCRD_IMatching = apCreditNote;
			apCRD_IMatching.OSPartialPaymentAmount = 100M;

			TestMatchingBase.AddIMatching(apINV_IMatching);
			TestMatchingBase.AddIMatching(apCRD_IMatching);

			Factory.Save();

			var expectedMatchDate = ZDate.Today;
			var taxRealisationEnablerMock = new Mock<ITaxRealisationEnabler>();
			var withholdingJournalCreationManagerMock = new Mock<IWithholdingJournalCreationManager>();
			var withholdingJournalCreationManager = withholdingJournalCreationManagerMock.Object;
			TestMatchingBase.SubstituteTaxRealisationEnabler_ForTestOnly(taxRealisationEnablerMock.Object);
			TestMatchingBase.SubstituteWithholdingJournalCreationManager_ForTestOnly(withholdingJournalCreationManager);
			IMatchingCollection matchingCollectionPassedAsParameter = null;
			taxRealisationEnablerMock.Setup(x => x.RealiseTaxIfApplicable(It.IsAny<IMatchingCollection>(), withholdingJournalCreationManager, expectedMatchDate))
				.Callback<IMatchingCollection, IWithholdingJournalCreationManager, ZDate>((collection, journalCreationManager, date) => matchingCollectionPassedAsParameter = collection)
				.Returns(string.Empty);

			Assert("Precondition: APInvoice outstanding amount is not zero", apInvoice.AH_OutstandingAmount != 0);
			Assert("Precondition: APCreditNote outstanding amount is not zero", apCreditNote.AH_OutstandingAmount != 0);
			Assert("Should return true", TestMatchingBase.Match_ForTestOnly());
			taxRealisationEnablerMock.Verify(x => x.RealiseTaxIfApplicable(It.IsAny<IMatchingCollection>(), withholdingJournalCreationManager, expectedMatchDate), Times.Once);
			AssertEquals(nameof(matchingCollectionPassedAsParameter), TestMatchingBase.MatchedTransactions, matchingCollectionPassedAsParameter);

			Assert("No error found", !TestMatchingBase.MatchingErrorsForGUINotificationOnlyInfo.HasErrors());
			Assert("Postcondition: APInvoice outstanding amount is zero", apInvoice.AH_OutstandingAmount == 0);
			Assert("Postcondition: APCreditNote outstanding amount is zero", apCreditNote.AH_OutstandingAmount == 0);
		}

		[TestDate(2020, 01, 15)]
		public virtual void TestTaxRealisationEnablerIsCalledBeforeGLMovementProcessor()
		{
			var currency = TestObjectCreator.AUD;
			var org = TestObjectCreator.Creditor1;
			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), organisation: org);
			var invoiceLine = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.OverheadChargeCode.PK, 100);

			var taxTransactionsParams = new CreateTaxTransactionParameters
			{
				TransactionHeader = apInvoice,
				OsTaxAmount = 10,
				LocalTaxAmount = 10,
				AffectsSourceTransactionTotal = false,
			};
			var taxTransaction = TFObjectCreator.CreateTaxTransaction(taxTransactionsParams);
			TFObjectCreator.CreateTaxTransactionLinePivot(taxTransaction.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(invoiceLine));
			var glMovementProcessor = new Mock<IGLMovementProcessor>(MockBehavior.Strict);
			glMovementProcessor.Setup(x => x.CreateGLMovements(It.IsAny<AccTaxTransaction>()));
			taxTransaction.SubstituteGLMovementProcessor_ForTestOnly(glMovementProcessor.Object);
			var tfDependencyFactory = new Mock<ITaxFrameworkDependencyFactory>();
			var emptyValidatorMock = new Mock<IAccTaxTransactionCriticalValidator>();
			tfDependencyFactory.Setup(x => x.GetAccTaxTransactionCriticalValidator(taxTransaction)).Returns(emptyValidatorMock.Object);
			ObjectFactory.Substitute(tfDependencyFactory.Object);

			var apCreditNote = TestObjectCreator.CreateInvoice(typeof(APCreditNote), organisation: org);
			TestObjectCreator.CreateInvoiceLine(apCreditNote, TestObjectCreator.OverheadChargeCode.PK, 100);

			Factory.Save();

			AssertNotEquals("Precondition: APInvoice outstanding amount", 0M, apInvoice.AH_OutstandingAmount);
			AssertNotEquals("Precondition: APCreditNote outstanding amount", 0M, apCreditNote.AH_OutstandingAmount);

			TestMatchingBase.PrimaryOrganization = org.PK;

			IMatching apINV_IMatching = apInvoice;
			apINV_IMatching.OSPartialPaymentAmount = apInvoice.AH_OutstandingAmount;
			IMatching apCRD_IMatching = apCreditNote;
			apCRD_IMatching.OSPartialPaymentAmount = apCreditNote.AH_OutstandingAmount;

			TestMatchingBase.AddIMatching(apINV_IMatching);
			TestMatchingBase.AddIMatching(apCRD_IMatching);

			var sequence = new MockSequence();
			var taxRealisationEnablerMock = new Mock<ITaxRealisationEnabler>(MockBehavior.Strict);
			TestMatchingBase.SubstituteTaxRealisationEnabler_ForTestOnly(taxRealisationEnablerMock.Object);
			taxRealisationEnablerMock.InSequence(sequence).Setup(x => x.RealiseTaxIfApplicable(It.IsAny<IMatchingCollection>(), It.IsAny<IWithholdingJournalCreationManager>(), It.IsAny<ZDate>())).Callback(() => taxTransaction.ATT_RealisationDate = ZDate.Today).Returns(string.Empty);
			glMovementProcessor.Reset();
			glMovementProcessor.InSequence(sequence).Setup(x => x.CreateGLMovements(It.IsAny<AccTaxTransaction>()));

			var matchingResult = TestMatchingBase.Match_ForTestOnly();

			Assert($"Precondition: {nameof(matchingResult)}", matchingResult);
			AssertNoErrors("Postcondition: errors after matching", TestMatchingBase.MatchingErrorsForGUINotificationOnlyInfo);
			AssertEquals("Postcondition: APInvoice outstanding amount", 0M, apInvoice.AH_OutstandingAmount);
			AssertEquals("Postcondition: APCreditNote outstanding amount", 0M, apCreditNote.AH_OutstandingAmount);
			taxRealisationEnablerMock.Verify(x => x.RealiseTaxIfApplicable(It.IsAny<IMatchingCollection>(), It.IsAny<IWithholdingJournalCreationManager>(), It.IsAny<ZDate>()), Times.Once);
			glMovementProcessor.Verify(x => x.CreateGLMovements(It.IsAny<AccTaxTransaction>()), Times.Once);
		}

		public virtual void TestTaxRealisationEnabler_ReturnsErrorMessage()
		{
			var currency = TestObjectCreator.AUD;
			var org = TestObjectCreator.Creditor1;
			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), currency, 1M, org, ZDate.Today.AddDays(-10));
			TestObjectCreator.CreateInvoiceLine(apInvoice, currency, 1M, 100M, 0M, 0M, 100M, 0M, 0M);

			var apCreditNote = TestObjectCreator.CreateInvoice(typeof(APCreditNote), currency, 1M, org, ZDate.Today.AddDays(-5));
			TestObjectCreator.CreateInvoiceLine(apCreditNote, currency, 1M, 100M, 0M, 0M, 100M, 0M, 0M);

			Factory.Save();

			TestMatchingBase.PrimaryOrganization = org.PK;

			IMatching apINV_IMatching = apInvoice;
			apINV_IMatching.OSPartialPaymentAmount = -100M;
			IMatching apCRD_IMatching = apCreditNote;
			apCRD_IMatching.OSPartialPaymentAmount = 100M;

			TestMatchingBase.AddIMatching(apINV_IMatching);
			TestMatchingBase.AddIMatching(apCRD_IMatching);

			Factory.Save();

			var expectedMatchDate = ZDate.Today;
			var expectedErrorMessage = "invalid setup";
			var taxRealisationEnablerMock = new Mock<ITaxRealisationEnabler>();
			var withholdingJournalCreationManagerMock = new Mock<IWithholdingJournalCreationManager>();
			var withholdingJournalCreationManager = withholdingJournalCreationManagerMock.Object;
			TestMatchingBase.SubstituteTaxRealisationEnabler_ForTestOnly(taxRealisationEnablerMock.Object);
			TestMatchingBase.SubstituteWithholdingJournalCreationManager_ForTestOnly(withholdingJournalCreationManager);
			IMatchingCollection matchingCollectionPassedAsParameter = null;
			taxRealisationEnablerMock.Setup(x => x.RealiseTaxIfApplicable(It.IsAny<IMatchingCollection>(), withholdingJournalCreationManager, expectedMatchDate))
				.Callback<IMatchingCollection, IWithholdingJournalCreationManager, ZDate>((collection, journalCreationManager, date) => matchingCollectionPassedAsParameter = collection)
				.Returns(expectedErrorMessage);

			Assert("Precondition: APInvoice outstanding amount is not zero", apInvoice.AH_OutstandingAmount != 0);
			Assert("Precondition: APCreditNote outstanding amount is not zero", apCreditNote.AH_OutstandingAmount != 0);
			Assert("Should return false as there is error", !TestMatchingBase.Match_ForTestOnly());
			AssertEquals(nameof(matchingCollectionPassedAsParameter), TestMatchingBase.MatchedTransactions, matchingCollectionPassedAsParameter);

			taxRealisationEnablerMock.Verify(x => x.RealiseTaxIfApplicable(TestMatchingBase.MatchedTransactions, withholdingJournalCreationManager, expectedMatchDate), Times.Once);

			Assert(TestMatchingBase.MatchingErrorsForGUINotificationOnlyInfo.HasErrors());
			Assert(TestMatchingBase.MatchingErrorsForGUINotificationOnlyInfo.HasError(expectedErrorMessage));
			Assert("Postcondition: APInvoice outstanding amount remains non-zero", apInvoice.AH_OutstandingAmount != 0);
			Assert("Postcondition: APCreditNote outstanding amount remains non-zero", apCreditNote.AH_OutstandingAmount != 0);
		}

		#endregion

		#region TestFullPaymentWithPartiallyPaidTransactions

		public virtual void TestFullPaymentWithPartiallyPaidTransactions()
		{
			SetUpTestDataSet();

			TestAPInvoice1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice1, TestAPInvoice1.TransactionCurrency, TestAPInvoice1.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);
			TestAPInvoice1.AH_LocalOutstandingAmount = 30M;
			TestAPInvoice1.AH_GSTAmount = 70M;

			APCRD1 = Factory.NewWithValidTestData<APCreditNote>();
			APCRD1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(APCRD1, APCRD1.TransactionCurrency, APCRD1.AH_ExchangeRate, 130m, 0m, 0m, 130m, 0m, 0m);
			APCRD1.AH_LocalOutstandingAmount = 130M;

			Factory.Save();

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			IMatching aPINV_IMatching = TestAPInvoice1;
			aPINV_IMatching.OSPartialPaymentAmount = -100M;
			IMatching aPCRD_IMatching = APCRD1;
			aPCRD_IMatching.OSPartialPaymentAmount = 100M;

			TestMatchingBase.AddIMatching(aPINV_IMatching);
			TestMatchingBase.AddIMatching(aPCRD_IMatching);

			Factory.Save();

			Assert("The transactions should be matchable", TestMatchingBase.Match_ForTestOnly());
			AssertEquals("No dynamic transactions should be created", 0, TestMatchingBase.DynamicTransactions.Count);

			// Check that transactions are matched correctly
			AssertNotNull("AP Invoice should now be fully paid", TestAPInvoice1.AH_FullyPaidDate);
			AssertEquals("Outstanding amt on AP Invoice should be 0", 0M, TestAPInvoice1.AH_OutstandingAmount);

			AssertNotNull("AP CRD should be fully paid", APCRD1.AH_FullyPaidDate);
			AssertEquals("Outstanding amt on ARCRD should be 30", 30M, APCRD1.AH_OutstandingAmount);

			// Check that correct match link rows are created
			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load(new ZQuery());
			AssertEquals("2 MatchLinks should be created", 2, matchLinks.Count);

			ZQuery aPINV_Filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestAPInvoice1.PK);
			TransactionMatchLink aPINV_Matchlink = Factory.LoadTop1<TransactionMatchLink>(aPINV_Filter);

			ZString matchGroup = aPINV_Matchlink.AP_MatchGroupNum;
			ZDateTime matchDate = aPINV_Matchlink.AP_MatchDate;
			AssertEquals("APInvoice Matchlink amt should be -100", -100M, aPINV_Matchlink.AP_Amount);

			ZQuery aPCRD_Filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, APCRD1.PK);
			TransactionMatchLink aPCRD_MatchLink = Factory.LoadTop1<TransactionMatchLink>(aPCRD_Filter);
			AssertEquals("APCRD Matchlink amt should be 100", 100M, aPCRD_MatchLink.AP_Amount);
			AssertEquals("Matchgroup Num should be the same", matchGroup, aPCRD_MatchLink.AP_MatchGroupNum);
			AssertEquals("MatchDate should be the same", matchDate, aPCRD_MatchLink.AP_MatchDate);
		}

		#endregion

		#region TestPartialPaymentWithPartiallyPaidTransactions

		public virtual void TestPartialPaymentWithPartiallyPaidTransactions()
		{
			SetUpTestDataSet();

			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_ExchangeRate = 1M;
			TestObjectCreator.CreateInvoiceLine(TestARInvoice1, TestARInvoice1.TransactionCurrency, TestARInvoice1.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);
			TestARInvoice1.AH_LocalOutstandingAmount = 60M;
			TestARInvoice1.AH_GSTAmount = -40M;
			TestARInvoice1.AH_FullyPaidDate = ZDateTime.Empty;

			TestAPInvoice1.AH_OH = TestOrg1.PK;
			TestAPInvoice1.AH_ExchangeRate = 1M;
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice1, TestAPInvoice1.TransactionCurrency, TestAPInvoice1.AH_ExchangeRate, 40m, 0m, 0m, 40m, 0m, 0m);
			TestAPInvoice1.AH_LocalOutstandingAmount = 40M;
			TestAPInvoice1.AH_FullyPaidDate = ZDateTime.Empty;

			Factory.Save();

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			IMatching aPINV_IMatching = TestAPInvoice1;
			aPINV_IMatching.OSPartialPaymentAmount = -30M;
			IMatching aRINV_IMatching = TestARInvoice1;
			aRINV_IMatching.OSPartialPaymentAmount = 30M;

			TestMatchingBase.AddIMatching(aRINV_IMatching);
			TestMatchingBase.AddIMatching(aPINV_IMatching);

			Factory.Save();

			Assert("These transactions should be matchable", TestMatchingBase.Match_ForTestOnly());

			// Check that transactions are correctly matched
			AssertEquals("ARINV should have 70 outstanding", 70M, TestARInvoice1.AH_LocalOutstandingAmount);
			AssertEquals("ARINV should not be fully paid", ZDateTime.Empty, TestARInvoice1.AH_FullyPaidDate);

			AssertEquals("APINV should have 10 outstanding", 10M, TestAPInvoice1.AH_LocalOutstandingAmount);
			AssertEquals("APINV should not be fully paid", ZDateTime.Empty, TestAPInvoice1.AH_FullyPaidDate);

			// Check dynamic transactions
			AssertEquals("A contra should be created", 1, TestMatchingBase.DynamicTransactions.Count);

			Contra dynamicContra = (Contra)TestMatchingBase.DynamicTransactions[0];
			AssertNotNull("This should be a Contra", dynamicContra);
			AssertEquals("ARRow should be for TestOrg1", TestOrg1.PK, dynamicContra.ARRow.AH_OH);
			AssertEquals("APRow should be for TestOrg1", TestOrg1.PK, dynamicContra.APRow.AH_OH);

			AssertEquals("APRow should have invoice amount = 30", 30M, dynamicContra.APRow.AH_InvoiceAmount);
			AssertEquals("APRow should have outstanding amount of zero", 0M, dynamicContra.APRow.AH_OutstandingAmount);
			AssertEquals("ARRow should have invoice amount = -30", -30M, dynamicContra.ARRow.AH_InvoiceAmount);
			AssertEquals("ARRow should have outstanding amount of zero", 0M, dynamicContra.ARRow.AH_OutstandingAmount);

			// Check match link rows
			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load(new ZQuery());
			AssertEquals("4 matchlinks should be created", 4, matchLinks.Count);

			// AR Invoice Match link
			ZQuery aRINV_Filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestARInvoice1.PK);
			TransactionMatchLink aRINV_Match = Factory.LoadTop1<TransactionMatchLink>(aRINV_Filter);
			AssertNotNull("There should be a matchlink for ARINV", aRINV_Match);

			ZString matchGroup = aRINV_Match.AP_MatchGroupNum;
			ZDateTime matchDate = aRINV_Match.AP_MatchDate;

			AssertEquals("ARINV Matchlink should have amount of 30", 30M, aRINV_Match.AP_Amount);

			// AP Invoice MatchLink
			ZQuery aPINV_Filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestAPInvoice1.PK);
			TransactionMatchLink aPINV_Match = Factory.LoadTop1<TransactionMatchLink>(aPINV_Filter);
			AssertNotNull("There should be a matchlink for APINV", aPINV_Match);

			AssertEquals("Match group num should be the same", matchGroup, aPINV_Match.AP_MatchGroupNum);
			AssertEquals("Match date should be the same", matchDate, aPINV_Match.AP_MatchDate);
			AssertEquals("Match amount should be -30", -30M, aPINV_Match.AP_Amount);
		}

		#endregion

		#region TestPartialPaymentWithARREC_ARPAY

		public virtual void TestPartialPaymentWithARREC_ARPAY()
		{
			SetUpTestDataSet();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			TestARReceipt.AH_OH = TestOrg1.PK;
			TestARReceipt.AH_LocalExTaxAmount = 100M;
			TestARReceipt.AH_OSExTaxAmount = 100M;
			TestARReceipt.AH_ExchangeRate = 1M;
			IMatching aRREC_IMatching = TestARReceipt;
			aRREC_IMatching.OSPartialPaymentAmount = -50M;

			ARPayment aRPAY = Factory.NewWithValidTestData<ARPayment>();
			aRPAY.AH_OH = TestOrg2.PK;
			aRPAY.AH_LocalExTaxAmount = 100M;
			aRPAY.AH_OSExTaxAmount = 100M;
			aRPAY.AH_ExchangeRate = 1M;

			Factory.Save();

			IMatching aRPAY_IMatching = aRPAY;
			aRPAY_IMatching.OSPartialPaymentAmount = 50M;

			TestMatchingBase.AddIMatching(aRREC_IMatching);
			TestMatchingBase.AddIMatching(aRPAY_IMatching);

			Assert("These transactions should be matchable", TestMatchingBase.Match_ForTestOnly());
			AssertEquals("One Transfer should be created", 1, TestMatchingBase.DynamicTransactions.Count);
			ARTransfer dynamicTransfer = TestMatchingBase.DynamicTransactions[0] as ARTransfer;
			AssertNotNull("Should be an ARTransfer", dynamicTransfer);

			// Check that Transactions are correctly matched
			AssertEquals("OutstandingAmount on ARREC should be -50", -50M, TestARReceipt.AH_OutstandingAmount);
			AssertEquals("FullyPaid date on ARREC should be null", ZDateTime.Empty, TestARReceipt.AH_FullyPaidDate);
			AssertEquals("OutstandingAmount on ARPAY should be 50", 50M, aRPAY.AH_OutstandingAmount);
			AssertEquals("Fully Paid date on ARPAY should be null", ZDateTime.Empty, aRPAY.AH_FullyPaidDate);

			// Check that Transfer is correct
			AssertEquals("FromRow should be for TestOrg2", TestOrg2.PK, dynamicTransfer.TransferFrom.AH_OH);
			AssertEquals("InvoiceAmount should be -50", -50M, dynamicTransfer.TransferFrom.AH_InvoiceAmount);
			AssertEquals("Outstanding Amount should be 0", 0M, dynamicTransfer.TransferFrom.AH_OutstandingAmount);

			AssertEquals("ToRow should be for TestOrg1", TestOrg1.PK, dynamicTransfer.TransferTo.AH_OH);
			AssertEquals("Invoice Amount should be 50", 50M, dynamicTransfer.TransferTo.AH_InvoiceAmount);

			// Check that Matchlink rows are correct
			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load(new ZQuery());
			AssertEquals("4 MatchLinks should be created", 4, matchLinks.Count);

			ZQuery aRREC_Filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestARReceipt.PK);
			TransactionMatchLink aRREC_Match = Factory.LoadTop1<TransactionMatchLink>(aRREC_Filter);
			AssertNotNull("There should be a matchlink for ARREC", aRREC_Match);
			AssertEquals("MatchLink amt should be -50", -50M, aRREC_Match.AP_Amount);

			ZString matchGroup = aRREC_Match.AP_MatchGroupNum;
			ZDateTime matchDate = aRREC_Match.AP_MatchDate;

			ZQuery aRPAY_Filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, aRPAY.PK);
			TransactionMatchLink aRPAY_Match = Factory.LoadTop1<TransactionMatchLink>(aRPAY_Filter);
			AssertNotNull("There should be a matchlink for ARPAY", aRPAY_Match);
			AssertEquals("MatchLink amt should be 50", 50M, aRPAY_Match.AP_Amount);
		}

		#endregion

		#region TestPartialPaymentWithAPJNL_APCTR_APTRF

		public virtual void TestPartialPaymentWithAPJNL_APCTR_APTRF()
		{
			SetUpTestDataSet();
			Factory.Save();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			// APJNL - signs are inverted
			APJournal aPJNL = Factory.NewWithValidTestData<APJournal>();
			aPJNL.AH_OH = TestOrg1.PK;
			aPJNL.AH_LocalExTaxAmount = 200M;
			aPJNL.AH_ExchangeRate = 1M;
			aPJNL.AH_OSExTaxAmount = 200M;
			aPJNL.AH_GSTAmount = 130M;
			aPJNL.AH_OSTaxAmount = 130M;
			aPJNL.AH_LocalOutstandingAmount = 70M;
			((IMatching)aPJNL).OSPartialPaymentAmount = -70M;
			TransactionMatchLink link = ((IMatching)aPJNL).CurrentMatchGroup.AddNew();
			link.AP_AH = aPJNL.PK;
			link.AP_Amount = -260M;

			AccTransactionHeader headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_InvoiceAmount = -link.AP_Amount;

			TransactionMatchLink linkToMatch = ((IMatching)aPJNL).CurrentMatchGroup.AddNew();
			linkToMatch.AP_AH = headerToMatch.PK;
			linkToMatch.AP_Amount = -link.AP_Amount;
			TestObjectCreator.SetupMatchLinkMatchDate(aPJNL);

			// APCTR - signs not inverted
			APContraRow aPCTR = Factory.NewWithValidTestData<APContraRow>();
			aPCTR.AH_OH = TestOrg1.PK;
			aPCTR.AH_LocalExTaxAmount = 300M;
			aPCTR.AH_OSExTaxAmount = 300M;
			aPCTR.AH_ExchangeRate = 1M;
			((IMatching)aPCTR).OSPartialPaymentAmount = 35M;

			// APTRF From - signs not inverted
			APTransferFromRow aPTRF = Factory.NewWithValidTestData<APTransferFromRow>();
			aPTRF.AH_OH = TestOrg1.PK;
			aPTRF.AH_LocalExTaxAmount = 400M;
			aPTRF.AH_ExchangeRate = 1M;
			aPTRF.AH_OSExTaxAmount = 400M;
			aPTRF.AH_GSTAmount = -200M;
			aPTRF.AH_OSTaxAmount = -200M;
			aPTRF.AH_LocalOutstandingAmount = 200M;
			((IMatching)aPTRF).OSPartialPaymentAmount = 35M;

			TestMatchingBase.AddIMatching(aPJNL);
			TestMatchingBase.AddIMatching(aPCTR);
			TestMatchingBase.AddIMatching(aPTRF);

			Factory.Save();

			Assert("These transactions should be matchable", TestMatchingBase.Match_ForTestOnly());
			AssertEquals("There should be no dynamic transactions", 0, TestMatchingBase.DynamicTransactions.Count);

			// Check that original transactions are matched correctly
			AssertEquals("APCTR should have outstanding amt of 265", 265M, aPCTR.AH_OutstandingAmount);
			Assert("APCTR should not be fully paid", aPCTR.AH_FullyPaidDate.IsEmpty);

			AssertEquals("APTRF should have outstanding amt of 165", 165M, aPTRF.AH_OutstandingAmount);
			Assert("APTRF should not be fully paid", aPTRF.AH_FullyPaidDate.IsEmpty);

			AssertEquals("APJNL should have outstanding amt of 0", 0M, aPJNL.AH_OutstandingAmount);
			Assert("APJNL should be fully paid", !aPJNL.AH_FullyPaidDate.IsEmpty);

			// Check that the match link amounts are correct
			((IMatching)aPJNL).Matchlinks.Load();
			AssertEquals("There should be 2 MatchLinks for APJNL", 2, ((IMatching)aPJNL).Matchlinks.Count);
			Assert("One of APJNL Matchlink should have amt = -70", ((IMatching)aPJNL).Matchlinks[0].AP_Amount == -70M || ((IMatching)aPJNL).Matchlinks[1].AP_Amount == -70M);

			((IMatching)aPCTR).Matchlinks.Load();
			AssertEquals("There should be a match link for APCTR", 1, ((IMatching)aPCTR).Matchlinks.Count);
			AssertEquals("APCTR Matchlink should have amt = 35", 35M, ((IMatching)aPCTR).Matchlinks[0].AP_Amount);

			((IMatching)aPTRF).Matchlinks.Load();
			AssertEquals("There should be a match link for APTRF", 1, ((IMatching)aPTRF).Matchlinks.Count);
			AssertEquals("APTRF Matchlink should have amt = 35", 35M, ((IMatching)aPTRF).Matchlinks[0].AP_Amount);
		}

		#endregion

		#endregion

		#region TestOSOutstandingAmountMatching

		public void TestOSOutstandingAmountMatching()
		{
			SetUpTestDataSet();
			RefCurrency testCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency));

			ARInvoiceLine aRLine = (ARInvoiceLine)TestARInvoice1.Lines.AddNew();
			aRLine.AL_OSExTaxAmount = 20m;
			TestARInvoice1.AH_RX_NKTransactionCurrency = testCurrency.RX_Code;
			TestARInvoice1.AH_LocalOutstandingAmount = 10M; // must be set after TransactionCurrency; TransactionCurrency resets Outstanding amount
			AssertEquals("OSOutstandingAmt should be 10", 10M, TestARInvoice1.OSOutstandingAmountMatching);

			APInvoiceLine aPLine = (APInvoiceLine)TestAPInvoice1.Lines.AddNew();
			aPLine.AL_OSExTaxAmount = 20m;
			TestAPInvoice1.AH_RX_NKTransactionCurrency = testCurrency.RX_Code;
			TestAPInvoice1.AH_LocalOutstandingAmount = 10M;
			AssertEquals("OSOutstandingAmt should be -10", -10M, TestAPInvoice1.OSOutstandingAmountMatching);

			TestARInvoice1.AH_ExchangeRate = 4m;  // resets OSOutstandingAmount to OSTotal
			AssertEquals("OSOutstandingAmt should be 20", 20M, TestARInvoice1.OSOutstandingAmountMatching);

			ARCRD1 = Factory.New<ARCreditNote>();
			ARCreditNoteLine aRCrdLine = (ARCreditNoteLine)ARCRD1.Lines.AddNew();
			aRCrdLine.AL_OSExTaxAmount = 20M;
			ARCRD1.AH_RX_NKTransactionCurrency = testCurrency.RX_Code;
			ARCRD1.AH_LocalOutstandingAmount = 10M;
			AssertEquals("OSOutstandingAmt should be -10", -10M, ARCRD1.OSOutstandingAmountMatching);

			ARCRD1.AH_ExchangeRate = 0.5M;
			AssertEquals("OSOutstandingAmt should be -20", -20M, ARCRD1.OSOutstandingAmountMatching);

			APCRD1 = Factory.New<APCreditNote>();
			APCreditNoteLine aPCrdLine = (APCreditNoteLine)APCRD1.Lines.AddNew();
			aPCrdLine.AL_OSExTaxAmount = 40M;
			APCRD1.AH_RX_NKTransactionCurrency = testCurrency.RX_Code;
			APCRD1.AH_LocalOutstandingAmount = 20M;
			AssertEquals("OSOutstandingAmt should be 20", 20M, APCRD1.OSOutstandingAmountMatching);
		}

		#endregion

		#region TestIncorrectMatchingWithPrimaryOrgAsDebtor

		public void TestIncorrectMatchingWithPrimaryOrgAsAP()
		{
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg1.OH_Code = "APOOOO";
			TestOrg1.OH_IsCreditor = true;

			TestOrg2 = GetNewTestOrg();
			TestOrg2.OH_Code = "ARAPOO";

			Factory.Save();

			TestAPInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInvoice1.AH_OH = TestOrg1.PK;
			TestAPInvoice1.AH_LocalExTaxAmount = 90M;
			TestAPInvoice1.AH_OSExTaxAmount = 90M;
			((IMatching)TestAPInvoice1).OSPartialPaymentAmount = TestAPInvoice1.OSOutstandingAmountMatching;
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice1, TestAPInvoice1.TransactionCurrency, 1m, 90M);

			TestARInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			TestARInvoice1.AH_OH = TestOrg2.PK;
			TestARInvoice1.AH_LocalExTaxAmount = 100M;
			TestARInvoice1.AH_OSExTaxAmount = 100M;
			((IMatching)TestARInvoice1).OSPartialPaymentAmount = TestARInvoice1.OSOutstandingAmountMatching;
			TestObjectCreator.CreateInvoiceLine(TestARInvoice1, TestARInvoice1.TransactionCurrency, 1m, 100M);

			TestAPInvoice2 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInvoice2.AH_OH = TestOrg2.PK;
			TestAPInvoice2.AH_LocalExTaxAmount = 5M;
			TestAPInvoice2.AH_OSExTaxAmount = 5M;
			((IMatching)TestAPInvoice2).OSPartialPaymentAmount = TestAPInvoice2.OSOutstandingAmountMatching;
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice2, TestAPInvoice2.TransactionCurrency, 1m, 5M);

			Factory.Save();

			fTestMatchingBase = new ARMatchingBase(Factory);
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			TestMatchingBase.AddIMatching(TestAPInvoice1);
			TestMatchingBase.AddIMatching(TestARInvoice1);
			TestMatchingBase.AddIMatching(TestAPInvoice2);

			AssertEquals("Should be 3 transactions in MatchedTransactions", 3, TestMatchingBase.MatchedTransactions.Count);
			AssertEquals("Balance should be 5", 5M, TestMatchingBase.Balance);

			TransactionHeader miscTrans = TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount);
			TestMatchingBase.AddMiscellaneousTransaction(miscTrans);

			AssertEquals("Balance should be 0", 0M, TestMatchingBase.Balance);

			TestMatchingBase.Match_ForTestOnly();

			TransactionHeaderCollection headers = new TransactionHeaderCollection(Factory);
			headers.Load();

			//	AssertEquals("Should not match - should be 3 transactions in DB", 3, Headers.Count);

			ZQuery aRTransferFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Transfer);
			aRTransferFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			aRTransferFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

			headers.Load(aRTransferFilter);
			AssertEquals("There should be no AR Transfers", 0, headers.Count);
		}

		#endregion

		#region Removing Transactions

		#region TestDeleteCachedMiscTransactions

		public virtual void TestDeleteCachedMiscTransactions()
		{
			TestOrg1 = GetNewTestOrg();
			Factory.Save();

			TestAPInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInvoice1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice1, TestAPInvoice1.TransactionCurrency, TestAPInvoice1.AH_ExchangeRate, 38m, 0m, 0m, 38m, 0m, 0m);
			Factory.Save();

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestMatchingBase.MoveAllFromUnmatchToMatch();

			AssertEquals("There should be 1 transaction selected for matching", 1, TestMatchingBase.MatchedTransactions.Count);

			TransactionHeader oVPMisc = TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Overpayment);
			TestMatchingBase.AddMiscellaneousTransaction(oVPMisc);
			TransactionHeader dSCMisc = TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount);
			TestMatchingBase.AddMiscellaneousTransaction(dSCMisc);
			TransactionHeader eXXMisc = TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
			TransactionHeader jNLMisc = TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Journal);
			TestMatchingBase.AddMiscellaneousTransaction(jNLMisc);

			AssertNotNull("OverpaymentBizO was added", TestMatchingBase.OverpaymentBizO_ForTestOnly);
			AssertNotNull("DiscountBizO was added", TestMatchingBase.DiscountBizO_ForTestOnly);
			AssertNull("ExchangeDiffBizO was not added", TestMatchingBase.ExchangeDifferenceBizO_ForTestOnly);
			AssertNotNull("ExchangeDiffBizOTmp was added", TestMatchingBase.ExchangeDiffTmp);
			AssertNotNull("BankFeeBizO was added", TestMatchingBase.BankFeeBizO_ForTestOnly);

			TestMatchingBase.DeleteCachedMiscTransactions();

			AssertNull("OverpaymentBizO should be null", TestMatchingBase.OverpaymentBizO_ForTestOnly);
			Assert("OverpaymentBizO should be deleted from DB", oVPMisc.IsDeleted);
			AssertNull("DiscountBizO should be null", TestMatchingBase.DiscountBizO_ForTestOnly);
			Assert("DiscountBizO should be deleted from DB", dSCMisc.IsDeleted);
			AssertNull("ExchangeDiffTmp should be null", TestMatchingBase.ExchangeDiffTmp);
			Assert("ExchangeDiffTmp should be deleted from DB", eXXMisc.IsDeleted);
			AssertNull("BankFeeBizO should be null", TestMatchingBase.BankFeeBizO_ForTestOnly);
			Assert("BankFeeBizO should be deleted from DB", jNLMisc.IsDeleted);
		}

		#endregion

		#region TestRemoveAllFromMatchedTransactions

		public void TestRemoveAllFromMatchedTransactions()
		{
			TestARInvoice1 = Factory.New<ARInvoice>();
			TestAPInvoice1 = Factory.New<APInvoice>();
			fTestMatchingBase = new ARMatchingBase(Factory);
			TestMatchingBase.MatchedTransactions.AddTransactionThatMustBeMatched(TestARInvoice1);
			TestMatchingBase.MatchedTransactions.Add(TestAPInvoice1);
			AssertEquals("MatchedTransactions contains both invoices", 2, TestMatchingBase.MatchedTransactions.Count);
			Assert("MustBeMatched contains ARInvoice", TestMatchingBase.MatchedTransactions.MustTransactionBeMatched(TestARInvoice1));

			TestMatchingBase.RemoveAllInMatchedTransactions();
			AssertEquals("MatchedTransactions should be empty", 0, TestMatchingBase.MatchedTransactions.Count);
			Assert("ARInvoice was removed from MustBeMatched", !TestMatchingBase.MatchedTransactions.MustTransactionBeMatched(TestARInvoice1));
		}

		#endregion

		#region TestDeleteDiscountIfNotInMatchingCollection

		public void TestDeleteDiscountIfNotInMatchingCollection()
		{
			DeleteMiscellaneousTransactionIfNotInMatchingCollectionCore(ZArchitecture.Core.TransactionTypes.Discount);
			AssertNull("Discount should be cleared", TestMatchingBase.DiscountBizO_ForTestOnly);
		}

		public void TestDeleteExchangeDifferenceIfNotInMatchingCollection()
		{
			DeleteMiscellaneousTransactionIfNotInMatchingCollectionCore(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
			AssertNull("Exchange Difference should be cleared", TestMatchingBase.ExchangeDifferenceBizO_ForTestOnly);
		}

		public void TestDeleteOverpaymentIfNotInMatchingCollection()
		{
			DeleteMiscellaneousTransactionIfNotInMatchingCollectionCore(ZArchitecture.Core.TransactionTypes.Overpayment);
			AssertNull("Overpayment should be cleared", TestMatchingBase.OverpaymentBizO_ForTestOnly);
		}

		void DeleteMiscellaneousTransactionIfNotInMatchingCollectionCore(string miscTransactionType)
		{
			TestOrg1 = GetNewTestOrg();
			TestOrg2 = GetNewTestOrg();
			Factory.Save();

			Invoice inv = (Invoice)Factory.NewWithValidTestData(InvoiceType);
			inv.AH_OH = TestOrg1.PK;
			inv.AH_OutstandingAmount = 10m;
			inv.AH_InvoiceAmount = 10m;
			inv.AH_IsCancelled = false;
			Factory.Save();

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestMatchingBase.MoveAllFromUnmatchToMatch();
			TransactionHeader miscTransaction = TestMatchingBase.GetMiscellaneousTransaction(miscTransactionType);
			TestMatchingBase.AddMiscellaneousTransaction(miscTransaction);

			TestMatchingBase.PrimaryOrganization = TestOrg2.PK;
		}

		#endregion

		#endregion

		#region TestMatchingWithErrorOnSelectedTransactions

		public virtual void TestMatchingWithErrorOnSelectedTransactions()
		{
			TestOrg1 = GetNewTestOrg();

			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			aPInv.AH_OH = TestOrg1.PK;
			aPInv.AH_LocalOutstandingAmount = 1000M;
			TestObjectCreator.CreateInvoiceLine(aPInv, GlbCompany.CurrentCompany.LocalCurrency, 1m, 1000m, 0m, 0m, 1000m, 0m, 0m);

			APReceipt aPRec = Factory.NewWithValidTestData<APReceipt>();
			aPRec.AH_OH = TestOrg1.PK;
			aPRec.AH_LocalExTaxAmount = 90M;
			aPRec.AH_OSExTaxAmount = 90M;
			aPRec.AH_LocalOutstandingAmount = 90M;

			Factory.Save();

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestMatchingBase.MoveAllFromUnmatchToMatch();

			((IMatching)aPInv).OSPartialPaymentAmount = 400M; // set an invalid OSPartialPaymentAmt
			((IBusinessObjectInternals)aPInv).Validate(((IMatching)aPInv).OSPartialPaymentAmountInfo);

			TransactionHeader miscTrans = TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
			TestMatchingBase.AddMiscellaneousTransaction(miscTrans);

			Assert("The row for APInvoice should have errors", ((IMatching)aPInv).OSPartialPaymentAmountInfo.HasErrors());
			TestMatchingBase.RunPreSaveValidation();
			Assert("Should not be able to match these because the OSPartialPayment amount on APInvoice is positive",
				TestMatchingBase.HasErrors);
		}

		#endregion

		#region TestOverpaymentCurrencyDefaultsToLocal

		public void TestOverpaymentCurrencyDefaultsToLocal()
		{
			RefCurrency currency = GlbCompany.CurrentCompany.LocalCurrency;

			AssertEquals("Overpayment currency should default to LocalCurrency", currency.RX_Code, TestMatchingBase.ForeignCurrency);
		}

		#endregion

		#region Receipt and Payment

		#region TestConstructorSetIsLoadedFromGUI

		public virtual void TestConstructorSetIsLoadedFromGUI()
		{
			SetUpTestDataSet();

			BusinessObjectFactory factoryToDiscard = new BusinessObjectFactory();
			APPayment paymentToPass = factoryToDiscard.New(typeof(APPayment)) as APPayment;

			paymentToPass.AH_OH = TestOrg1.PK;
			paymentToPass.AH_InvoiceAmount = 100M;
			paymentToPass.AH_OutstandingAmount = 100M;

			MatchingBase testMatcher1 = new APMatchingBase(Factory);
			AssertEquals(true, testMatcher1.IsLoadedFromGUI_ForTestOnly);

			MatchingBase testMatcher2 = new APMatchingBase(Factory, paymentToPass);
			AssertEquals(true, testMatcher2.IsLoadedFromGUI_ForTestOnly);

			MatchingBase testMatcher3 = new APMatchingBase(Factory, paymentToPass, false);
			AssertEquals(false, testMatcher3.IsLoadedFromGUI_ForTestOnly);
		}

		#endregion

		#region TestConstructorForPaymentDetail

		public void TestConstructorForPaymentDetail()
		{
			SetUpTestDataSet();

			BusinessObjectFactory factoryToDiscard = new BusinessObjectFactory();
			APPayment paymentToPass = factoryToDiscard.New(typeof(APPayment)) as APPayment;

			paymentToPass.AH_OH = TestOrg1.PK;
			paymentToPass.AH_InvoiceAmount = 100M;
			paymentToPass.AH_OutstandingAmount = 100M;

			Factory.Save();
			MatchingBase testMatcher = new APMatchingBase(Factory, paymentToPass);

			AssertEquals(1, testMatcher.MatchedTransactions.Count);
			AssertEquals(paymentToPass.PK, testMatcher.MatchedTransactions[0].Identifier);
			AssertEquals("Balance should be 100M", 100M, testMatcher.Balance);
		}

		#endregion

		#region TestPaymentDetailCannotBeRemoved

		public void TestPaymentDetailCannotBeRemoved()
		{
			SetUpTestDataSet();

			BusinessObjectFactory factoryToDiscard = new BusinessObjectFactory();
			APPayment paymentToPass = factoryToDiscard.New(typeof(APPayment)) as APPayment;

			paymentToPass.AH_OH = TestOrg1.PK;

			Factory.Save();
			MatchingBase testMatcher = new APMatchingBase(Factory, paymentToPass);

			testMatcher.MoveFromMatchToUnmatch(new BusinessObject[] { (BusinessObject)testMatcher.MatchedTransactions[0] });
			AssertEquals(1, testMatcher.MatchedTransactions.Count);
			AssertEquals(0, testMatcher.UnmatchedTransactions.Count);
		}

		#endregion

		#region TestMatchingBaseWithPaymentDetailSetsMatchGroupNumbers

		public void TestMatchingBaseWithPaymentDetailSetsMatchGroupNumbers()
		{
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			TestAPInvoice1 = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			TestAPInvoice1.AH_OutstandingAmount = -100M;
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice1, GlbCompany.CurrentCompany.LocalCurrency, 1m, 100m, 0m, 0m, 100m, 0m, 0m);
			((IMatching)TestAPInvoice1).OSPartialPaymentAmount = ((IMatching)TestAPInvoice1).OSOutstandingAmount;
			TestAPInvoice1.AH_OH = TestOrg1.PK;

			TestAPPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			TestAPPayment.AH_LocalExTaxAmount = 100M;
			TestAPPayment.AH_OSExTaxAmount = 100M;
			TestAPPayment.AH_OutstandingAmount = 100M;
			TestAPPayment.AH_OH = TestOrg1.PK;

			Factory.Save();

			fTestMatchingBase = new ARMatchingBase(Factory, TestAPPayment);
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestMatchingBase.AddIMatching(TestAPInvoice1);
			TestMatchingBase.MatchAndClearTransactions();
			AssertEquals(2, TestMatchingBase.MatchedTransactions.Count);
			TransactionHeaderCollection headers = new TransactionHeaderCollection(new BusinessObjectFactory());
			headers.Load();
			Assert("The DB should not be updated", headers[0].AH_OutstandingAmount != 0M);
			Assert("The DB should not be updated", headers[0].AH_FullyPaidDate.IsEmpty);
			Assert("The DB should not be updated", headers[1].AH_OutstandingAmount != 0M);
			Assert("The DB should not be updated", headers[1].AH_FullyPaidDate.IsEmpty);

			Factory.Save();

			headers.Load();
			AssertEquals("The transcations should be save to DB", 0M, headers[0].AH_OutstandingAmount);

			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load();
			AssertEquals("The transactions should be matched", 2, matchLinks.Count);
			ZString commonMatchNum = matchLinks[0].AP_MatchGroupNum;
			AssertEquals("Matchlinks should have same group number", commonMatchNum, matchLinks[1].AP_MatchGroupNum);
		}

		#endregion

		#region TestMatchingBaseWithPaymentDetailSetsMatchGroupNumbers

		public void TestMatchingBaseWithReceiptDetailDoNotClearMatchedTransactions()
		{
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			TestAPInvoice1 = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			TestAPInvoice1.AH_OutstandingAmount = 100M;
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice1, GlbCompany.CurrentCompany.LocalCurrency, 1m, 100m, 0m, 0m, 100m, 0m, 0m);
			((IMatching)TestAPInvoice1).OSPartialPaymentAmount = ((IMatching)TestAPInvoice1).OSOutstandingAmount;
			TestAPInvoice1.AH_OH = TestOrg1.PK;

			TestAPReceipt = Factory.NewWithValidTestData<APReceipt>();
			TestAPReceipt.AH_LocalExTaxAmount = -100M;
			TestAPReceipt.AH_OSExTaxAmount = -100M;
			TestAPReceipt.AH_OH = TestOrg1.PK;

			Factory.Save();

			fTestMatchingBase = new ARMatchingBase(Factory, TestAPReceipt);
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestMatchingBase.AddIMatching(TestAPInvoice1);
			TestMatchingBase.MatchAndClearTransactions();
			AssertEquals(2, TestMatchingBase.MatchedTransactions.Count);
		}

		#endregion

		#region TestIsNotMatchingPayment

		public void TestIsNotMatchingPayment()
		{
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg1.OH_IsCreditor = true;
			TestOrg1.OH_IsDebtor = true;
			Factory.Save();

			APInvoice testInv = Factory.NewWithValidTestData<APInvoice>();
			testInv.AH_OH = TestOrg1.PK;
			testInv.AH_LocalOutstandingAmount = 100M;
			TestObjectCreator.CreateInvoiceLine(testInv, GlbCompany.CurrentCompany.LocalCurrency, 1m, 100m, 0m, 0m, 100m, 0m, 0m);
			Factory.Save();

			ARReceipt testRec = Factory.NewWithValidTestData<ARReceipt>();
			fTestMatchingBase = new ARMatchingBase(Factory, testRec);
			Assert("IsNotMatchingPayment is true", TestMatchingBase.IsNotMatchingPayment);
			Assert("PrimaryOrganisation should not be writable", TestMatchingBase.PrimaryOrganisationForGUINotificationInfo.ReadOnly);

			APPayment testPay = Factory.NewWithValidTestData<APPayment>();
			testPay.AH_OH = TestOrg1.PK;
			fTestMatchingBase = new APMatchingBase(Factory, testPay);
			Assert("IsNotMatchingPayment is false", !TestMatchingBase.IsNotMatchingPayment);
			AssertEquals("PrimaryOrganisation should be TestOrg1", TestOrg1.PK, TestMatchingBase.PrimaryOrganisationForGUINotification);
			Assert("PrimaryOrganisation should be readonly", TestMatchingBase.PrimaryOrganisationForGUINotificationInfo.ReadOnly);
			Assert("Outstanding organisations should contain TestInv", TestMatchingBase.UnmatchedTransactions.Contains(testInv));

			fTestMatchingBase = new ARMatchingBase(Factory);
			Assert("IsNotMatchingPayment is true", TestMatchingBase.IsNotMatchingPayment);
		}

		#endregion

		#region TestIsMatchingPaymentOrReceipt

		public void TestIsMatchingPaymentOrReceipt()
		{
			ARReceipt testRec = Factory.NewWithValidTestData<ARReceipt>();
			fTestMatchingBase = new ARMatchingBase(Factory, testRec);
			Assert("IsMatchingPaymentOrReceipt should be true", TestMatchingBase.IsMatchingPaymentOrReceipt);
			Assert("PrimaryOrganisation should not be writable", TestMatchingBase.PrimaryOrganisationForGUINotificationInfo.ReadOnly);

			APPayment testPay = Factory.NewWithValidTestData<APPayment>();
			fTestMatchingBase = new APMatchingBase(Factory, testPay);
			Assert("IsMatchingPaymentOrReceipt should be true", TestMatchingBase.IsMatchingPaymentOrReceipt);
			Assert("PrimaryOrganisation should not be writable", TestMatchingBase.PrimaryOrganisationForGUINotificationInfo.ReadOnly);

			fTestMatchingBase = new ARMatchingBase(Factory);
			Assert("IsMatchingPaymentOrReceipt should not be true", !TestMatchingBase.IsMatchingPaymentOrReceipt);
			Assert("PrimaryOrganisation should be writable", !TestMatchingBase.PrimaryOrganisationForGUINotificationInfo.ReadOnly);
		}

		#endregion

		#region TestIsCurrentPaymentChequeNumberInvalid

		public void TestIsCurrentPaymentChequeNumberInvalid()
		{
			AccChequeBook chequeBook = GetTestChequeBook();
			Factory.Save();

			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			aPPay.AH_OSExTaxAmount = 100m;
			aPPay.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			aPPay.ChequeBook = chequeBook.PK;
			aPPay.AH_ChequeOrReference = "5";

			fTestMatchingBase = new APMatchingBase(Factory, aPPay);
			Assert("Current Payment cheque number should be valid", !TestMatchingBase.IsReceiptPaymentDetailChequeNumberInvalid);

			chequeBook.AK_CurrentNo = 6;
			TestMatchingBase.RunPreSaveValidation();
			AssertEquals("Current Payment cheque number should be valid", false, TestMatchingBase.IsReceiptPaymentDetailChequeNumberInvalid);

			ARReceipt aRRec = Factory.NewWithValidTestData<ARReceipt>();
			aRRec.AH_OSExTaxAmount = 20m;
			aRRec.AH_ChequeOrReference = "7";

			fTestMatchingBase = new ARMatchingBase(Factory, aRRec);
			Assert("Current ReceiptPaymentDetail is not a payment thus Current Payment Cheque Num should not be valid", !TestMatchingBase.IsReceiptPaymentDetailChequeNumberInvalid);
		}

		public void TestIsReceiptPaymentChequeNumberInvalidWithoutReceiptPayment()
		{
			fTestMatchingBase = new APMatchingBase(Factory);
			Assert("Should be false because there is no Receipt/Payment associated with this MatchingBase", !TestMatchingBase.IsReceiptPaymentDetailChequeNumberInvalid);
		}

		#endregion

		#region TestMoveFromUnmatchToMatchMakesChequeNumReadonly

		public virtual void TestMoveFromUnmatchToMatchMakesChequeNumReadonly()
		{
			TestOrg1 = GetNewTestOrg();
			ARInvoice aRInv = GetNewTestARInvoice(378M, TestOrg1);

			Assert("Precondition: AH_ChequeOrReference should be readonly", aRInv.AH_ChequeOrReferenceInfo.ReadOnly);
			Assert("Precondition: ChequeOrReference should be readonly", ((IMatching)aRInv).ChequeOrReferenceInfo.ReadOnly);
			Factory.Save();

			((IMatching)aRInv).ChequeOrReference_ReadOnly = false;
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestMatchingBase.MoveAllFromUnmatchToMatch();
			Assert(((IMatching)aRInv).ChequeOrReference_ReadOnly);
			Assert("ChequeOrReference should be readonly", ((IMatching)aRInv).ChequeOrReferenceInfo.ReadOnly);
		}

		[TestDate(2010, 07, 05)]
		public virtual void TestMoveFromUnmatchToMatchValidateOSOutstandingAmount()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OrgHeader org1 = TestObjectCreator.AALSHI;
			APInvoice aPInvoiceToTest = Factory.NewWithValidTestData<APInvoice>();
			aPInvoiceToTest.AH_OH = org1.PK;
			aPInvoiceToTest.AH_InvoiceAmount = -90M;
			aPInvoiceToTest.AH_OutstandingAmount = -90M;

			Factory.Save();

			AccBankAccount bankAccount = newFactory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_Code = "TestBank";
			AccChequeBook checkBook = newFactory.NewWithValidTestData<AccChequeBook>();
			checkBook.AK_AB = bankAccount.PK;
			checkBook.AK_Code = "TestBook";

			ARPaymentApprovalWithAuthorisation payment1 = newFactory.NewWithValidTestData<ARPaymentApprovalWithAuthorisation>();
			payment1.AV_Amount = 40M;
			payment1.AV_AB = bankAccount.PK;
			PaymentApprovalItem item1 = newFactory.NewWithValidTestData<PaymentApprovalItem>();
			item1.A2_PaymentThisRun = 40M;
			item1.A2_AH = aPInvoiceToTest.PK;
			item1.A2_AV = payment1.PK;

			ARPaymentApprovalWithAuthorisation payment2 = newFactory.NewWithValidTestData<ARPaymentApprovalWithAuthorisation>();
			payment2.AV_Amount = 50M;
			payment2.AV_AB = bankAccount.PK;
			payment2.AV_AK = checkBook.PK;
			payment2.AV_ChequeOrReference = "Test223";
			PaymentApprovalItem item2 = newFactory.NewWithValidTestData<PaymentApprovalItem>();
			item2.A2_PaymentThisRun = 50M;
			item2.A2_AH = aPInvoiceToTest.PK;
			item2.A2_AV = payment2.PK;

			newFactory.Save();

			IMatchingCollection matchingCollection = new IMatchingCollection(Factory);
			matchingCollection.Add(aPInvoiceToTest);

			IMatching transactionAsMatching = aPInvoiceToTest;

			AssertNoErrors(transactionAsMatching.OSOutstandingAmountInfo);

			TestMatchingBase.MoveFromUnmatchToMatch(new BusinessObject[] { aPInvoiceToTest });

			Assert(transactionAsMatching.OSOutstandingAmountInfo.GetErrors().ContainsNotificationContaining("This transaction is fully paid by the following Unapproved payment(s):"));
			Assert(transactionAsMatching.OSOutstandingAmountInfo.GetErrors().ContainsNotificationContaining("Pay. Date   Bank Account   Check Book   Check/Reference   Amount"));
			Assert(transactionAsMatching.OSOutstandingAmountInfo.GetErrors().ContainsNotificationContaining("05-Jul-10  TestBank          TestBook       Test223     50.00 AUD"));
			Assert(transactionAsMatching.OSOutstandingAmountInfo.GetErrors().ContainsNotificationContaining("05-Jul-10  TestBank               N/A                N/A          40.00 AUD"));
		}

		#endregion

		#region TestMiscellaneousTransactionHasReadonlyChequeNum

		public void TestMiscellaneousTransactionHasReadonlyChequeNum()
		{
			TestOrg1 = GetNewTestOrg();
			Factory.Save();

			ARInvoice aRInv = GetNewTestARInvoice(200M, TestOrg1);

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestMatchingBase.MoveAllFromUnmatchToMatch();

			Discount miscTransaction = (Discount)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount);
			Assert("Precondition: AH_ChequeOrReference should not be read only", !miscTransaction.AH_ChequeOrReferenceInfo.ReadOnly);
			Assert("Precondition: ChequeOrReference should not be read only", !((IMatching)miscTransaction).ChequeOrReferenceInfo.ReadOnly);

			TestMatchingBase.AddMiscellaneousTransaction(miscTransaction);

			Assert("ChequeOrReference should be read only", ((IMatching)miscTransaction).ChequeOrReferenceInfo.ReadOnly);
		}

		#endregion

		public void TestPaymentDetail()
		{
			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			fTestMatchingBase = new APMatchingBase(Factory, aPPay);
			AssertEquals("Payment detail should return the payment", aPPay, TestMatchingBase.PaymentDetail);
		}

		public void TestIsAllPaidInTheSameCurrency()
		{
			APReceipt aPRec = Factory.NewWithValidTestData<APReceipt>();
			aPRec.AH_OSExTaxAmount = 10M;
			fTestMatchingBase = new APMatchingBase(Factory, aPRec);
			AssertEquals(true, TestMatchingBase.IsAllPaidInTheSameCurrency("AUD"));

			TestAPInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInvoice1.AH_OSExTaxAmount = 99.23M;
			TestAPInvoice1.AH_ExchangeRate = 0.78M;
			((IMatching)TestAPInvoice1).OSPartialPaymentAmount = -90.23M;

			TestARInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			TestARInvoice1.AH_OSExTaxAmount = 123.99M;
			TestARInvoice1.AH_ExchangeRate = 0.79M;
			((IMatching)TestARInvoice1).OSPartialPaymentAmount = 120.99M;

			TestARInvoice2 = Factory.NewWithValidTestData<ARInvoice>();
			TestARInvoice2.AH_RX_NKTransactionCurrency = "USD";
			TestARInvoice2.AH_OSExTaxAmount = 123.99M;
			TestARInvoice2.AH_ExchangeRate = 0.79M;
			((IMatching)TestARInvoice2).OSPartialPaymentAmount = 120.99M;

			TestMatchingBase.MatchedTransactions.Add(TestAPInvoice1);
			TestMatchingBase.MatchedTransactions.Add(TestARInvoice1);
			AssertEquals(true, TestMatchingBase.IsAllPaidInTheSameCurrency("AUD"));

			TestMatchingBase.MatchedTransactions.Add(TestARInvoice2);
			AssertEquals(false, TestMatchingBase.IsAllPaidInTheSameCurrency("AUD"));
			AssertEquals(false, TestMatchingBase.IsAllPaidInTheSameCurrency("AUD", ZString.Empty));
			AssertEquals(false, TestMatchingBase.IsAllPaidInTheSameCurrency("AUD", null));
			AssertEquals(true, TestMatchingBase.IsAllPaidInTheSameCurrency("AUD", TransactionTypes.Invoice));
		}

		public void TestCalculateOSBalanceExcludingPaymentReceiptInSpecificCurrencyOnly()
		{
			APReceipt aPRec = Factory.NewWithValidTestData<APReceipt>();
			aPRec.AH_OSExTaxAmount = 10M;
			fTestMatchingBase = new APMatchingBase(Factory, aPRec);
			AssertEquals("The OSBalance excluding Receipt is 0", 0M, TestMatchingBase.CalculateOSBalanceExcludingPaymentReceiptInSpecificCurrencyOnly("AUD", aPRec.PK));

			TestAPInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInvoice1.AH_OSExTaxAmount = 99.23M;
			TestAPInvoice1.AH_ExchangeRate = 0.78M;
			((IMatching)TestAPInvoice1).OSPartialPaymentAmount = -90.23M;

			TestARInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			TestARInvoice1.AH_OSExTaxAmount = 123.99M;
			TestARInvoice1.AH_ExchangeRate = 0.79M;
			((IMatching)TestARInvoice1).OSPartialPaymentAmount = 120.99M;

			TestARInvoice2 = Factory.NewWithValidTestData<ARInvoice>();
			TestARInvoice2.AH_RX_NKTransactionCurrency = "USD";
			TestARInvoice2.AH_OSExTaxAmount = 123.99M;
			TestARInvoice2.AH_ExchangeRate = 0.79M;
			((IMatching)TestARInvoice2).OSPartialPaymentAmount = 120.99M;

			TestMatchingBase.MatchedTransactions.Add(TestAPInvoice1);
			TestMatchingBase.MatchedTransactions.Add(TestARInvoice1);
			TestMatchingBase.MatchedTransactions.Add(TestARInvoice2);
			AssertEquals("The OSBalance excluding Receipt is 30.76", 30.76M, TestMatchingBase.CalculateOSBalanceExcludingPaymentReceiptInSpecificCurrencyOnly("AUD", aPRec.PK));
		}

		public void TestCalculateLocalBalanceExcludingReceipt()
		{
			APReceipt aPRec = Factory.NewWithValidTestData<APReceipt>();
			aPRec.AH_OSExTaxAmount = 10M;
			fTestMatchingBase = new APMatchingBase(Factory, aPRec);
			AssertEquals("The LocalBalance excluding Receipt is 0", 0M, TestMatchingBase.CalculateLocalBalanceExcludingReceipt());

			TestAPInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInvoice1.AH_ExchangeRate = 0.78M;
			TestAPInvoice1.AH_OSExTaxAmount = 99.23M;
			((IMatching)TestAPInvoice1).OSPartialPaymentAmount = -90.23M;

			TestARInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			TestARInvoice1.AH_ExchangeRate = 0.79M;
			TestARInvoice1.AH_OSExTaxAmount = 123.99M;
			((IMatching)TestARInvoice1).OSPartialPaymentAmount = 120.99M;

			TestMatchingBase.MatchedTransactions.Add(TestAPInvoice1);
			TestMatchingBase.MatchedTransactions.Add(TestARInvoice1);
			AssertEquals("The LocalBalance excluding Receipt is 37.47", 37.47M, TestMatchingBase.CalculateLocalBalanceExcludingReceipt());
		}

		public void TestCalculateOSBalanceExcludingReceipt()
		{
			APReceipt aPRec = Factory.NewWithValidTestData<APReceipt>();
			aPRec.AH_OSExTaxAmount = 10M;
			fTestMatchingBase = new APMatchingBase(Factory, aPRec);
			AssertEquals("The OSBalance excluding Receipt is 10", 10M, TestMatchingBase.CalculateOSBalanceExcludingReceipt());

			TestAPInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInvoice1.AH_ExchangeRate = 0.78M;
			TestAPInvoice1.AH_OSExTaxAmount = 99.23M;
			((IMatching)TestAPInvoice1).OSPartialPaymentAmount = -90.23M;

			TestARInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			TestARInvoice1.AH_ExchangeRate = 0.79M;
			TestARInvoice1.AH_OSExTaxAmount = 123.99M;
			((IMatching)TestARInvoice1).OSPartialPaymentAmount = 120.99M;

			TestMatchingBase.MatchedTransactions.Add(TestAPInvoice1);
			TestMatchingBase.MatchedTransactions.Add(TestARInvoice1);
			AssertEquals("The OSBalance excluding Receipt is 37.47", 37.47M, TestMatchingBase.CalculateOSBalanceExcludingReceipt());

			aPRec.AH_RX_NKTransactionCurrency = "USD";
			aPRec.AH_ExchangeRate = 0.5;
			AssertEquals("The OSBalance excluding Receipt is 18.74", 18.74M, TestMatchingBase.CalculateOSBalanceExcludingReceipt());
		}

		public void TestTransactionsHaveSameCurrency()
		{
			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			aPPay.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			TestARInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			TestARInvoice1.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			TestAPInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInvoice1.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			fTestMatchingBase = new APMatchingBase(Factory, aPPay);

			TestMatchingBase.MatchedTransactions.Add(TestARInvoice1);
			TestMatchingBase.MatchedTransactions.Add(TestAPInvoice1);

			Assert("All transactions are of the same currency", TestMatchingBase.TransactionsHaveSameCurrency);

			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			TestAPInvoice1.AH_RX_NKTransactionCurrency = currency.RX_Code;

			Assert("The transactions do not all have the same currency", !TestMatchingBase.TransactionsHaveSameCurrency);
		}

		public void TestUpdateAndValidateBalance()
		{
			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			aPPay.AH_OSExTaxAmount = 100m;
			((IMatching)aPPay).OSPartialPaymentAmount = 100m;
			fTestMatchingBase = new APMatchingBase(Factory, aPPay);

			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			aPInv.AH_OSExTaxAmount = 100m;
			((IMatching)aPInv).OSPartialPaymentAmount = -100m;
			TestMatchingBase.AddIMatching(aPInv);
			Assert("Balance should not have errors because invoice and payment amounts are equal", !TestMatchingBase.BalanceInfo.HasErrors());
			((IMatching)aPInv).OSPartialPaymentAmount = -99m;
			Assert("Precondition: Balance should not have errors", !TestMatchingBase.BalanceInfo.HasErrors());
			TestMatchingBase.UpdateAndValidateBalance();
			Assert("Balance is non-zero and should have errors", TestMatchingBase.BalanceInfo.HasErrors());
		}

		public void TestUpdatePaymentOSPartialPaidAmount()
		{
			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			aPPay.AH_OSExTaxAmount = 89m;

			IMatching paymentAsMatching = aPPay;
			paymentAsMatching.OSPartialPaymentAmount = 89m;
			fTestMatchingBase = new APMatchingBase(Factory, aPPay);

			aPPay.AH_OSExTaxAmount = 19m;
			AssertEquals("Precondition: OSPartialPaymentAmount on the Payment is 89", 89m, paymentAsMatching.OSPartialPaymentAmount);
			TestMatchingBase.UpdatePaymentOSPartialPaidAmount();
			AssertEquals("OSPartialPaymentAmount on the Payment is 19", 19m, paymentAsMatching.OSPartialPaymentAmount);

			APReceipt aPRec = Factory.NewWithValidTestData<APReceipt>();
			aPRec.AH_OSExTaxAmount = 89m;

			IMatching receiptAsMatching = aPRec;
			receiptAsMatching.OSPartialPaymentAmount = 89m;
			fTestMatchingBase = new APMatchingBase(Factory, aPRec);

			aPRec.AH_OSExTaxAmount = 19m;
			AssertEquals("Precondition: OSPartialPaymentAmount on the Receipt is -89", -89m, receiptAsMatching.OSPartialPaymentAmount);
			TestMatchingBase.UpdatePaymentOSPartialPaidAmount();
			AssertEquals("OSPartialPaymentAmount on the Receipt is -19", -19m, receiptAsMatching.OSPartialPaymentAmount);
		}

		#endregion

		#region Create Miscellaneous Transactions from Payment Approval Details

		public virtual void TestCreateMiscTransactionsFromPaymentApprovalDetails()
		{
			OrgHeader testOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var matchStatus = "UAC";
			var matchStatusReasonCode = "ADV";

			TestMatchingBase.PrimaryOrganization = testOrgHeader.PK;

			PaymentApprovalBase approval = Factory.New<APPaymentApprovalWithAuthorisation>();
			approval.AV_Amount = 20000M;
			approval.AV_ExchangeDifference = -10000M;
			approval.AV_Discount = -10000M;
			approval.AV_ExxMatchStatus = matchStatus;
			approval.AV_ExxMatchStatusReasonCode = matchStatusReasonCode;
			approval.AV_DscMatchStatus = matchStatus;
			approval.AV_DscMatchStatusReasonCode = matchStatusReasonCode;

			AssertEquals("Exchange Difference Amount", 0M, TestMatchingBase.ExchangeDifferenceAmount);
			AssertEquals("Discount Amount", 0M, TestMatchingBase.DiscountAmount);
			AssertEquals("Matched Transactions Count", 0, TestMatchingBase.MatchedTransactions.Count);

			TestMatchingBase.CreateMiscTransactionsFromPaymentApprovalDetails(approval);

			AssertEquals("Exchange Difference Amount", approval.AV_ExchangeDifference, TestMatchingBase.ExchangeDifferenceAmount);
			AssertEquals("Exchange Difference MatchStatus", approval.AV_ExxMatchStatus, TestMatchingBase.ExchangeDifferenceBizO_ForTestOnly.AH_MatchStatus);
			AssertEquals("Exchange Difference MatchStatusReasonCode", approval.AV_ExxMatchStatusReasonCode, TestMatchingBase.ExchangeDifferenceBizO_ForTestOnly.AH_MatchStatusReasonCode);
			AssertNotNull("Exchange Difference BizObj", TestMatchingBase.ExchangeDifferenceAmount);

			AssertEquals("Discount Amount", approval.AV_Discount, TestMatchingBase.DiscountAmount);
			AssertEquals("Discount MatchStatus", approval.AV_DscMatchStatus, TestMatchingBase.DiscountBizO_ForTestOnly.AH_MatchStatus);
			AssertEquals("Discount MatchStatusReasonCode", approval.AV_DscMatchStatusReasonCode, TestMatchingBase.DiscountBizO_ForTestOnly.AH_MatchStatusReasonCode);
			AssertNotNull("Discount BizObj", TestMatchingBase.DiscountAmount);

			AssertEquals("Matched Transactions Count", 2, TestMatchingBase.MatchedTransactions.Count);

			AssertEquals("Matched Transactions contains ExchangeDifferenceBizObj", true,
					TestMatchingBase.MatchedTransactions.Contains(TestMatchingBase.ExchangeDifferenceBizO_ForTestOnly));

			AssertEquals("Matched Transactions contains DiscountBizObj", true,
					TestMatchingBase.MatchedTransactions.Contains(TestMatchingBase.DiscountBizO_ForTestOnly));

			AssertEquals("BindableInvoiceAmount on ExchangeDifference should be set correctly", approval.AV_ExchangeDifference, TestMatchingBase.ExchangeDifferenceBizO_ForTestOnly.BindableInvoiceAmount);
			AssertEquals("AH_OutstandingAmount on ExchangeDifference should be set correctly", approval.AV_ExchangeDifference, TestMatchingBase.ExchangeDifferenceBizO_ForTestOnly.AH_OutstandingAmount);
			AssertEquals("BindableOSAmount on ExchangeDifference should be set correctly", approval.AV_ExchangeDifference, TestMatchingBase.ExchangeDifferenceBizO_ForTestOnly.BindableOSAmount);
			AssertEquals("MatchStatus on ExchangeDifference should be set correctly", approval.AV_ExxMatchStatus, TestMatchingBase.ExchangeDifferenceBizO_ForTestOnly.AH_MatchStatus);
			AssertEquals("MatchStatusReasonCode on ExchangeDifference should be set correctly", approval.AV_ExxMatchStatusReasonCode, TestMatchingBase.ExchangeDifferenceBizO_ForTestOnly.AH_MatchStatusReasonCode);

			AssertEquals("BindableInvoiceAmount on Discount should be set correctly", approval.AV_Discount, TestMatchingBase.DiscountBizO_ForTestOnly.BindableInvoiceAmount);
			AssertEquals("AH_OutstandingAmount on Discount should be set correctly", approval.AV_Discount, TestMatchingBase.DiscountBizO_ForTestOnly.AH_OutstandingAmount);
			AssertEquals("BindableOSAmount on Discount should be set correctly", approval.AV_Discount, TestMatchingBase.DiscountBizO_ForTestOnly.BindableOSAmount);
			AssertEquals("MatchStatus on Discount should be set correctly", approval.AV_DscMatchStatus, TestMatchingBase.DiscountBizO_ForTestOnly.AH_MatchStatus);
			AssertEquals("MatchStatusReasonCode on Discount should be set correctly", approval.AV_DscMatchStatusReasonCode, TestMatchingBase.DiscountBizO_ForTestOnly.AH_MatchStatusReasonCode);
		}

		#endregion

		#region TestMatchDateDefaultValue

		public virtual void TestMatchDateDefaultValue()
		{
			Assert("MatchingBase's MatchDate default value should be valid", !TestMatchingBase.MatchDate.IsEmpty && TestMatchingBase.MatchDate.IsValid);
		}

		#endregion

		#region TestMatchLinksPostDate

		public virtual void TestMatchLinksMatchDate()
		{
			SetUpTestDataSet();
			TestObjectCreator.CreateInvoiceLine(TestARInvoice1, TestARInvoice1.TransactionCurrency, TestARInvoice1.AH_ExchangeRate, 40m, 0m, 0m, 40m, 0m, 0m);

			Factory.Save();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestMatchingBase.AddIMatching(TestARInvoice1);
			TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();
			Discount testDSC = (Discount)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount);
			TestMatchingBase.AddIMatching(testDSC);

			TestMatchingBase.MatchDate = ZDateTime.Now.AddDays(7);
			Assert("Should allow a match since balance is 0", TestMatchingBase.Match_ForTestOnly());

			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load(new ZQuery());
			AssertEquals("4 Match link Rows should be created", 4, matchLinks.Count);
			foreach (TransactionMatchLink matchLink in matchLinks)
			{
				AssertEquals("MatchLink should have the same Match Date as in MatchingBase", TestMatchingBase.MatchDate, matchLink.AP_MatchDate);
			}

			foreach (TransactionHeader transaction in TestMatchingBase.MatchedTransactions)
			{
				AssertEquals("AH_FullyPaidDate should be equal to the Matchdate.", TestMatchingBase.MatchDate, transaction.AH_FullyPaidDate);
			}
			foreach (IPayablesAndReceivables transaction in TestMatchingBase.DynamicTransactions)
			{
				if (transaction is Transfer)
				{
					AssertEquals("AH_FullyPaidDate should be equal to the Matchdate.", TestMatchingBase.MatchDate, ((Transfer)transaction).TransferFrom.AH_FullyPaidDate);
					AssertEquals("AH_FullyPaidDate should be equal to the Matchdate.", TestMatchingBase.MatchDate, ((Transfer)transaction).TransferTo.AH_FullyPaidDate);
				}
				else if (transaction is Contra)
				{
					AssertEquals("AH_FullyPaidDate should be equal to the Matchdate.", TestMatchingBase.MatchDate, ((Contra)transaction).APRow.AH_FullyPaidDate);
					AssertEquals("AH_FullyPaidDate should be equal to the Matchdate.", TestMatchingBase.MatchDate, ((Contra)transaction).ARRow.AH_FullyPaidDate);
				}
			}
		}

		#endregion

		#region Line Matching Related Stuff

		public void TestResetAmountsCalledOnMovingFromMatchToUnmatch()
		{
			SetUpTestDataSet();
			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_LocalExTaxAmount = 10M;
			TestARInvoice1.AH_OSTotalAmount = 10M;
			TestARInvoice1.AH_ExchangeRate = 1M;
			((IMatching)TestARInvoice1).OSPartialPaymentAmount = 10M;
			ARInvoiceLine line = (ARInvoiceLine)TestARInvoice1.Lines.AddNew();
			((ILineMatching)line).PaidAmount = 100m;

			TestMatchingBase.UnmatchedTransactions.Add(TestARInvoice1);

			BusinessObject[] tmpSelected = new BusinessObject[1];
			tmpSelected[0] = TestARInvoice1;

			TestMatchingBase.MoveFromUnmatchToMatch(tmpSelected);
			((IMatching)TestARInvoice1).OSPartialPaymentAmount = 5M;

			tmpSelected[0] = TestARInvoice1;
			TestMatchingBase.MoveFromMatchToUnmatch(tmpSelected);

			AssertEquals("On Moving from Match to Unmatch reset must be called", 0M, ((ILineMatching)line).PaidAmount);
		}

		public void TestTransLinePayRecordsGeneratedOnMatching()
		{
			if (TestMatchingBase is APMatchingBase || TestMatchingBase is ARMatchingBase)
			{
				SetUpTestDataSet();
				TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

				TestARInvoice1.AH_OH = TestOrg1.PK;
				TestARInvoice1.AH_LocalExTaxAmount = 100M;
				TestARInvoice1.AH_LocalTaxAmount = 10M;
				TestARInvoice1.AH_LocalOutstandingAmount = 110M;
				TestARInvoice1.AH_OSTotalAmount = 110M;
				ARInvoiceLine line = (ARInvoiceLine)TestARInvoice1.Lines.AddNew();
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
				line.AL_LineAmount = 100M;
				line.AL_LocalTaxAmount = 10M;
				line.AL_OSExTaxAmount = 100M;
				line.AL_OSTaxAmount = 10M;

				ARJournal testARJournal = Factory.NewWithValidTestData<ARJournal>();
				testARJournal.AH_OH = TestOrg1.PK;
				testARJournal.AH_LocalExTaxAmount = -60M;
				testARJournal.AH_LocalOutstandingAmount = -60M;
				testARJournal.AH_OSTotalAmount = -60M;

				ARTransferFromRow testFromRow = Factory.NewWithValidTestData<ARTransferFromRow>();
				testFromRow.AH_OH = TestOrg1.PK;
				testFromRow.AH_LocalExTaxAmount = 50M;
				testFromRow.AH_LocalOutstandingAmount = 50M;
				testFromRow.AH_OSTotalAmount = 50M;

				TestMatchingBase.AddIMatching(TestARInvoice1);
				TestMatchingBase.AddIMatching(testARJournal);
				TestMatchingBase.AddIMatching(testFromRow);
				TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

				var mediator = new InvoicingBasePayLineMediator(TestMatchingBase, TestARInvoice1);
				var invoiceLineAsILineMatching = (from ILineMatching l in mediator.Lines where l.AL_OSExTaxAmount == 100m select l).First();
				invoiceLineAsILineMatching.PaidAmount = 110m;
				mediator.ConveyData();
				((IMatching)TestARInvoice1).OSPartialPaymentAmount = 110m;

				TestMatchingBase.Match_ForTestOnly();

				AssertEquals("TransLinePay Record must be generated", 1, line.TransLinePays.Count);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestInvoicingLineLocalPaidAmountWhenFullyPaidInTwoPartiallyPay()
			=> AssertInvoicingLineLocalPaidAmountWhenFullyPaidInTwoPartiallyPay(false);

		public void TestInvoicingLineLocalPaidAmountWhenFullyPaidInTwoPartiallyPay_EnableNewOSOutstandingAmountFeature()
			=> AssertInvoicingLineLocalPaidAmountWhenFullyPaidInTwoPartiallyPay(true);

		public void AssertInvoicingLineLocalPaidAmountWhenFullyPaidInTwoPartiallyPay(bool isEnableNewOSOutstandingAmountFeature)
		{
			if (!ShouldTestPayLine)
			{
				Assert(true);
				return;
			}

			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			TestMatchingBase.PrimaryOrganization = TestObjectCreator.Creditor1.PK;

			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("1", TestObjectCreator.IDR, 14000m, 3000000m, 300000m, 0m, 214.29m, 21.43m, 0m, TestObjectCreator.Creditor1);
			invoice.Lines.Cast<AccTransactionLines>().First().AL_OSAmount = -3300000m;
			invoice.AH_OSTotal = -3300000m;
			var targetLine = invoice.Lines.Cast<ILineMatching>().First();
			targetLine.SetDefaultValues();
			CombineAssertions("Paid Line Precondition", () => {
				AssertEquals("OutstandingAmount", -3300000m, targetLine.OutstandingAmount);
				AssertEquals("LocalOutstandingAmount", -235.72m, targetLine.LocalOutstandingAmount);
			});

			AssertMatchAPLine(
				invoice,
				paidAmount: -1800000m,
				expectedLocalPaidAmount: -128.57m,
				commentForLocalPaidAmount: "LocalPaidAmount, 1800000/14000 => -128.571 => -128.57",
				expectedOutstandingAmountAfterMatch: isEnableNewOSOutstandingAmountFeature ? -1500000m : -1500020m,
				expectedLocalOutstandingAmountAfterMatch: -107.15m
			);

			AssertMatchAPLine(
				invoice,
				paidAmount: targetLine.OutstandingAmount,
				expectedLocalPaidAmount: -107.15m,
				commentForLocalPaidAmount: $"LocalPaidAmount, {targetLine.OutstandingAmount}/14000 => -107.14, but it should be -107.15 that LocalOutstandingAmount since it is fully paid.",
				expectedOutstandingAmountAfterMatch: 0m,
				expectedLocalOutstandingAmountAfterMatch: 0m
			);
		}

		public void TestInvoicingLineIsFullyPaidWhenOSPaidAmountIsNotEqualedToOSOutstandingAmount()
			=> AssertInvoicingLineIsFullyPaidWhenOSPaidAmountIsNotEqualedToOSOutstandingAmount(false);

		public void TestInvoicingLineIsFullyPaidWhenOSPaidAmountIsNotEqualedToOSOutstandingAmount_EnableNewOSOutstandingAmountFeature()
			=> AssertInvoicingLineIsFullyPaidWhenOSPaidAmountIsNotEqualedToOSOutstandingAmount(true);

		public void AssertInvoicingLineIsFullyPaidWhenOSPaidAmountIsNotEqualedToOSOutstandingAmount(bool isEnableNewOSOutstandingAmountFeature)
		{
			if (!ShouldTestPayLine)
			{
				Assert(true);
				return;
			}

			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			TestMatchingBase.PrimaryOrganization = TestObjectCreator.Creditor1.PK;

			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("1", TestObjectCreator.IDR, 14000m, 5000000m, 500000m, 0m, 357.14m, 35.71m, 0m, TestObjectCreator.Creditor1);
			invoice.Lines.Cast<AccTransactionLines>().First().AL_OSAmount = -5500000m;
			invoice.AH_OSTotal = -5500000m;
			var targetLine = invoice.Lines.Cast<ILineMatching>().First();
			targetLine.SetDefaultValues();
			CombineAssertions("Paid Line Precondition", () => {
				AssertEquals("OutstandingAmount", -5500000m, targetLine.OutstandingAmount);
				AssertEquals("LocalOutstandingAmount", -392.85m, targetLine.LocalOutstandingAmount);
			});

			AssertMatchAPLine(
				invoice,
				paidAmount: -5499950m,
				expectedLocalPaidAmount: -392.85m,
				commentForLocalPaidAmount: "LocalPaidAmount, 5499950m/14000 => -392.853 => -392.85",
				expectedOutstandingAmountAfterMatch: 0m,
				expectedLocalOutstandingAmountAfterMatch: 0m
			);

			if (isEnableNewOSOutstandingAmountFeature)
			{
				var linePayMatchedResult = Factory.Load<AccTransLinePay>(
					new ZQuery(AccTransLinePaySchema.A7_AL, invoice.Lines.Cast<AccTransactionLines>().First().PK)
				).FirstOrDefault();

				AssertNotNull("PreCondition, AccTransLinePay should be stored into DB", linePayMatchedResult);
				AssertEquals("A7_OSAmount, we should store -5500000m in DB since it is fully paid, even PaidAmount is -5499950m,"
					, -5500000m
					, linePayMatchedResult.A7_OSAmount);
			}
		}

		void AssertMatchAPLine(APInvoice invoice, decimal paidAmount, decimal expectedLocalPaidAmount, string commentForLocalPaidAmount, decimal expectedOutstandingAmountAfterMatch, decimal expectedLocalOutstandingAmountAfterMatch)
		{
			TestMatchingBase.MoveFromUnmatchToMatch(new BusinessObject[] { invoice });

			var mediator = new InvoicingBasePayLineMediator(TestMatchingBase, invoice);
			var targetLine = mediator.Lines.Cast<ILineMatching>().First();
			targetLine.PaidAmount = paidAmount;
			AssertEquals(commentForLocalPaidAmount,
				expectedLocalPaidAmount,
				targetLine.LocalPaidAmount
			);
			mediator.ConveyData();

			var journal = TestObjectCreator.CreateJournal<APJournal>(((IMatching)invoice).LocalPartialPaymentAmount, ZDateTime.Today, TestObjectCreator.Creditor1.PK);
			((IMatching)journal).OSPartialPaymentAmount = journal.AH_LocalTotal;
			TestMatchingBase.MoveFromUnmatchToMatch(new BusinessObject[] { journal });
			AssertEquals("matching was successful", true, TestMatchingBase.MatchAndClearTransactions());

			targetLine.SetDefaultValues();
			AssertEquals("OutstandingAmount after matching",
				expectedOutstandingAmountAfterMatch,
				targetLine.OutstandingAmount
			);
			AssertEquals("LocalPaidAmount after matching",
				expectedLocalOutstandingAmountAfterMatch,
				targetLine.LocalOutstandingAmount
			);
		}

		[SuspendCriticalValidation]
		public void TestLoadingOfLinesWhenMatchingAtHeaderAndLineLevel()
		{
			if (TestMatchingBase is APMatchingBase || TestMatchingBase is ARMatchingBase)
			{
				var shipment = TestObjectCreator.CreateShipment("");
				var job = TestObjectCreator.CreateJob(shipment, false);
				var invoice1 = CreateInvoiceWithTwoLines(job, TestObjectCreator.Creditor1, "111"); // 330
				var invoice2 = CreateInvoiceWithTwoLines(job, TestObjectCreator.Creditor1, "222"); // 330
				var invoice3 = CreateInvoiceWithTwoLines(job, TestObjectCreator.Creditor1, "333"); // 330
				var invoice4 = CreateInvoiceWithTwoLines(job, TestObjectCreator.Creditor1, "444"); // 330
				var journal1 = TestObjectCreator.CreateJournal<APJournal>(-760m, ZDateTime.Today, TestObjectCreator.Creditor1.PK);
				var journal2 = TestObjectCreator.CreateJournal<APJournal>(-520m, ZDateTime.Today, TestObjectCreator.Creditor1.PK);
				Factory.Save();

				var localCacheQuery = new ZQuery();
				localCacheQuery.FetchOnlyFromLocalCache = true;

				var testFactory = new BusinessObjectFactory();
				invoice1 = testFactory.Load<APInvoice>(invoice1.PK);
				invoice2 = testFactory.Load<APInvoice>(invoice2.PK);
				invoice3 = testFactory.Load<APInvoice>(invoice3.PK);
				invoice4 = testFactory.Load<APInvoice>(invoice4.PK);
				journal1 = testFactory.Load<APJournal>(journal1.PK);
				journal2 = testFactory.Load<APJournal>(journal2.PK);

				fTestMatchingBase = GetTestMatchingBaseInNewFactory(testFactory);
				TestMatchingBase.PrimaryOrganization = TestObjectCreator.Creditor1.PK;
				AssertEquals("Precondition: there should be six unmatched transactions", 6, TestMatchingBase.UnmatchedTransactions.Count);

				TestMatchingBase.MoveFromUnmatchToMatch(new BusinessObject[] { journal1, invoice1, invoice2, invoice3 });
				AssertEquals("Precondition: there should be four matched transactions", 4, TestMatchingBase.MatchedTransactions.Count);
				TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

				((IMatching)journal1).OSPartialPaymentAmount = 760m; // 760 - Fully Paid
				((IMatching)invoice1).OSPartialPaymentAmount = -320m; // -320 - Fully Paid
				((IMatching)invoice2).OSPartialPaymentAmount = -220m; // -220 - Partially Paid at header level
				((IMatching)invoice3).OSPartialPaymentAmount = -220m; // -220 - Partially Paid at line level

				var mediator = new InvoicingBasePayLineMediator(TestMatchingBase, invoice3);
				var invoice3Line1 = (from ILineMatching line in mediator.Lines where line.AL_OSExTaxAmount == 100m select line).First();
				invoice3Line1.PaidAmount = 0m;
				var invoice3Line2 = (from ILineMatching line in mediator.Lines where line.AL_OSExTaxAmount == 200m select line).First();
				invoice3Line2.PaidAmount = -220m;
				mediator.ConveyData();

				AssertEquals("matching was successful", true, TestMatchingBase.Match_ForTestOnly());
				AssertEquals("One TransLinePay record should be generated", 1, testFactory.Load<AccTransLinePay>(localCacheQuery).Length);
				var linesInFactory = testFactory.Load<TransactionLine>(localCacheQuery);
				AssertEquals("Only two AccTransactionLines record should be loaded", 2, linesInFactory.Length);
				AssertEquals("Both lines should be for invoice3", 2, linesInFactory.Where(x => x.AL_AH == invoice3.PK).Count());

				var secondTestFactory = new BusinessObjectFactory();
				invoice1 = secondTestFactory.Load<APInvoice>(invoice1.PK);
				invoice2 = secondTestFactory.Load<APInvoice>(invoice2.PK);
				invoice3 = secondTestFactory.Load<APInvoice>(invoice3.PK);
				invoice4 = secondTestFactory.Load<APInvoice>(invoice4.PK);
				journal1 = secondTestFactory.Load<APJournal>(journal1.PK);
				journal2 = secondTestFactory.Load<APJournal>(journal2.PK);

				fTestMatchingBase = GetTestMatchingBaseInNewFactory(secondTestFactory);
				TestMatchingBase.PrimaryOrganization = TestObjectCreator.Creditor1.PK;
				AssertEquals("Precondition: there should be four unmatched transactions", 4, TestMatchingBase.UnmatchedTransactions.Count);

				TestMatchingBase.MoveFromUnmatchToMatch(new BusinessObject[] { journal2, invoice2, invoice3, invoice4 });
				AssertEquals("Precondition: there should be four matched transactions", 4, TestMatchingBase.MatchedTransactions.Count);
				TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

				((IMatching)journal2).OSPartialPaymentAmount = 520m;
				((IMatching)invoice2).OSPartialPaymentAmount = -100m;
				((IMatching)invoice3).OSPartialPaymentAmount = -100m;
				((IMatching)invoice4).OSPartialPaymentAmount = -320m;

				mediator = new InvoicingBasePayLineMediator(TestMatchingBase, invoice3);
				invoice3Line1 = (from ILineMatching line in mediator.Lines where line.AL_OSExTaxAmount == 100m select line).First();
				invoice3Line1.PaidAmount = -100m;
				mediator.ConveyData();

				bool resultOfMatching = TestMatchingBase.Match_ForTestOnly();
				AssertEquals("matching was successful", true, resultOfMatching);

				var payLinesInFactory = secondTestFactory.Load<AccTransLinePay>(localCacheQuery);
				AssertEquals("Two TransLinePay record should be in the factory (one new, one old)", 2, payLinesInFactory.Length);
				AssertEquals("Both TransLinePay records should be for invoice3", 2, payLinesInFactory.Where(x => x.TransactionLines.AL_AH == invoice3.PK).Count());

				linesInFactory = secondTestFactory.Load<TransactionLine>(localCacheQuery);
				AssertEquals("Only two TransactionLines record should be loaded", 2, linesInFactory.Length);
				AssertEquals("Both TransactionLines should be for invoice3 because this is the one that was paid at line level", 2, linesInFactory.Where(x => x.AL_AH == invoice3.PK).Count());
			}
			else
			{
				Assert(true);
			}
		}

		APInvoice CreateInvoiceWithTwoLines(Job job, OrgHeader creditor, ZString transactionNum)
		{
			var result = TestObjectCreator.CreateAPInvoice<APInvoice>(transactionNum, TestObjectCreator.AUD, 1.0m, 100m, 0m, 0m, 100m, 0m, 0m, creditor);
			TestObjectCreator.CreateAPInvoiceLine(result, null, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "", 200m);
			AssertEquals("Precondition: result should have 2 lines", 2, result.Lines.Count);
			return result;
		}

		#endregion

		public void TestSetDescriptionOnMatchLinksOnSaving()
		{
			if (TestMatchingBase is PaymentApprovalMatchingBase)
			{
				Assert(true);
				return;
			}

			SetUpTestDataSet();
			if (TestMatchingBase is ARMatchingBase)
			{
				TestOrg1.OH_IsDebtor = true;
			}
			else
			{
				TestOrg1.OH_IsCreditor = true;
			}
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			TestARReceipt.AH_LocalExTaxAmount = 60M;
			TestARReceipt.AH_OH = TestOrg1.PK;
			TestARReceipt.AH_OSTotalAmount = 60M;

			TestAPReceipt = Factory.NewWithValidTestData<APReceipt>();
			TestAPReceipt.AH_LocalExTaxAmount = 30M;
			TestAPReceipt.AH_OH = TestOrg1.PK;
			TestAPReceipt.AH_OSTotalAmount = 30M;

			TestAPPayment.AH_LocalExTaxAmount = 90M;
			TestAPPayment.AH_OH = TestOrg1.PK;
			TestAPPayment.AH_OSTotalAmount = 90M;

			Factory.Save();

			if (TestMatchingBase is ARMatchingBase)
			{
				TestMatchingBase.AddIMatching(TestARReceipt);
			}
			else
			{
				TestMatchingBase.AddIMatching(TestAPReceipt);
				TestMatchingBase.AddIMatching(TestAPPayment);
			}

			decimal eXXAmount = TestMatchingBase is APMatchingBase ? -60M : 60M;

			var testEXX = (ExchangeDifference)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
			testEXX.AH_OSTotal = eXXAmount;
			testEXX.AH_InvoiceAmount = eXXAmount;
			testEXX.AH_OutstandingAmount = eXXAmount;
			TestMatchingBase.AddIMatching(testEXX);

			TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();
			AssertEquals("Balance should be zero", 0M, TestMatchingBase.Balance);
			Assert("These transactions should be matchable", TestMatchingBase.Match_ForTestOnly());

			var reloadExx = Factory.Load<AccTransactionHeader>(testEXX.PK);
			var exx_Match = Factory.LoadTop1<AccTransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, testEXX.PK));

			if (TestMatchingBase is ARMatchingBase)
			{
				var receipt = Factory.Load<AccTransactionHeader>(TestARReceipt.PK);
				AssertEquals($"MATCH NO. {exx_Match.AP_MatchGroupNum} [{receipt.AH_TransactionType}:{receipt.AH_TransactionNum}]", reloadExx.AH_Desc);
			}
			else
			{
				var query = new ZQuery(AccTransactionHeaderSchema.PK, new ZGuid[] { TestAPReceipt.PK, TestAPPayment.PK });
				var receiptAndPayment = Factory.Load<AccTransactionHeader>(query)?.OrderBy(x => x.AH_TransactionNum).ToList();
				AssertEquals($"MATCH NO. {exx_Match.AP_MatchGroupNum} [{receiptAndPayment[0].AH_TransactionType}:{receiptAndPayment[0].AH_TransactionNum}, {receiptAndPayment[1].AH_TransactionType}:{receiptAndPayment[1].AH_TransactionNum}]", reloadExx.AH_Desc);
			}
		}

		public void TestSetDescriptionOnPaymentReceiptNumberSet()
		{
			var testMatchingBase1 = new APMatchingBase(Factory);
			var testMatchingBase2 = new APMatchingBase(Factory);
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.CompanyData.OB_IsDebtor = true;
			testOrg1.CompanyData.OB_IsCreditor = true;

			var testAPPayment1 = Factory.NewWithValidTestData<APPayment>();
			testAPPayment1.AH_LocalExTaxAmount = 90M;
			testAPPayment1.AH_OH = testOrg1.PK;
			testAPPayment1.AH_OSTotalAmount = 90M;

			var testAPPayment2 = Factory.NewWithValidTestData<APPayment>();
			testAPPayment2.AH_LocalExTaxAmount = 90M;
			testAPPayment2.AH_OH = testOrg1.PK;
			testAPPayment2.AH_OSTotalAmount = 90M;

			Factory.Save();

			testMatchingBase1.PrimaryOrganization = testOrg1.PK;
			testMatchingBase2.PrimaryOrganization = testOrg1.PK;

			var testAPReceipt1 = Factory.NewWithValidTestData<APReceipt>();

			testAPReceipt1.AH_LocalExTaxAmount = 30M;
			testAPReceipt1.AH_OH = testOrg1.PK;
			testAPReceipt1.AH_OSTotalAmount = 30M;

			var testAPReceipt2 = Factory.NewWithValidTestData<APReceipt>();

			testAPReceipt2.AH_LocalExTaxAmount = 30M;
			testAPReceipt2.AH_OH = testOrg1.PK;
			testAPReceipt2.AH_OSTotalAmount = 30M;

			testMatchingBase1.AddIMatching(testAPReceipt1);
			testMatchingBase1.AddIMatching(testAPPayment1);

			testMatchingBase2.AddIMatching(testAPReceipt2);
			testMatchingBase2.AddIMatching(testAPPayment2);

			decimal eXXAmount = -60M;
			var testEXX1 = (ExchangeDifference)testMatchingBase1.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
			testEXX1.AH_OSTotal = eXXAmount;
			testEXX1.AH_InvoiceAmount = eXXAmount;
			testEXX1.AH_OutstandingAmount = eXXAmount;
			testMatchingBase1.AddIMatching(testEXX1);

			var testEXX2 = (ExchangeDifference)testMatchingBase2.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
			testEXX2.AH_OSTotal = eXXAmount;
			testEXX2.AH_InvoiceAmount = eXXAmount;
			testEXX2.AH_OutstandingAmount = eXXAmount;
			testMatchingBase2.AddIMatching(testEXX2);

			testMatchingBase1.MatchedTransactions.SetPartialPaidAmount();
			testMatchingBase2.MatchedTransactions.SetPartialPaidAmount();

			testMatchingBase1.DoNotSaveFactoryOnMatching = true;
			testMatchingBase2.DoNotSaveFactoryOnMatching = true;
			Factory.SetContext(BusinessContext.UniversalTransactionBatchImport);

			testMatchingBase1.Match_ForTestOnly();
			testMatchingBase2.Match_ForTestOnly();

			Factory.Save();

			var reloadExx1 = Factory.Load<AccTransactionHeader>(testEXX1.PK);
			var exx_Match1 = Factory.LoadTop1<AccTransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, testEXX1.PK));
			var query1 = new ZQuery(AccTransactionHeaderSchema.PK, new ZGuid[] { testAPReceipt1.PK, testAPPayment1.PK });
			var receiptAndPayment1 = Factory.Load<AccTransactionHeader>(query1)?.OrderBy(x => x.AH_TransactionNum).ToList();

			AssertEquals($"MATCH NO. {exx_Match1.AP_MatchGroupNum} [{receiptAndPayment1[0].AH_TransactionType}:{receiptAndPayment1[0].AH_TransactionNum}, {receiptAndPayment1[1].AH_TransactionType}:{receiptAndPayment1[1].AH_TransactionNum}]", reloadExx1.AH_Desc);

			var reloadExx2 = Factory.Load<AccTransactionHeader>(testEXX2.PK);
			var exx_Match2 = Factory.LoadTop1<AccTransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, testEXX2.PK));
			var query2 = new ZQuery(AccTransactionHeaderSchema.PK, new ZGuid[] { testAPReceipt2.PK, testAPPayment2.PK });
			var receiptAndPayment2 = Factory.Load<AccTransactionHeader>(query2)?.OrderBy(x => x.AH_TransactionNum).ToList();

			AssertEquals($"MATCH NO. {exx_Match2.AP_MatchGroupNum} [{receiptAndPayment2[0].AH_TransactionType}:{receiptAndPayment2[0].AH_TransactionNum}, {receiptAndPayment2[1].AH_TransactionType}:{receiptAndPayment2[1].AH_TransactionNum}]", reloadExx2.AH_Desc);
		}

		public void TestSuspendValidationForUmmatchedTransactions()
		{
			SetUpTestDataSet();
			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(TestARInvoice1, GlbCompany.CurrentCompany.LocalCurrency, 1m, 100m, 0m, 0m, 100m, 0m, 0m);

			TestARInvoice2.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(TestARInvoice2, GlbCompany.CurrentCompany.LocalCurrency, 1m, 200m, 0m, 0m, 200m, 0m, 0m);

			TestARInvoice4.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(TestARInvoice4, GlbCompany.CurrentCompany.LocalCurrency, 1m, 300m, 0m, 0m, 300m, 0m, 0m);

			Factory.Save();

			TestMatchingBase.UnmatchedTransactions.Add(TestARInvoice1);
			TestMatchingBase.UnmatchingExcludedTransactions.Add(TestARInvoice2);
			TestMatchingBase.MatchedTransactions.Add(TestARInvoice4);

			((IMatching)TestARInvoice1).OSPartialPaymentAmount = -((IMatching)TestARInvoice1).OSOutstandingAmount;
			((IMatching)TestARInvoice2).OSPartialPaymentAmount = -((IMatching)TestARInvoice2).OSOutstandingAmount;
			((IMatching)TestARInvoice4).OSPartialPaymentAmount = -((IMatching)TestARInvoice4).OSOutstandingAmount;
			AssertNoErrors("Validation for unmatched transactions must be suspented.", ((IMatching)TestARInvoice1).OSPartialPaymentAmountInfo);
			AssertNoErrors("Validation for unmatched transactions must be suspented.", ((IMatching)TestARInvoice2).OSPartialPaymentAmountInfo);
			AssertHasErrors("Validation for matched transactions must work.", ((IMatching)TestARInvoice4).OSPartialPaymentAmountInfo);

			((IMatching)TestARInvoice4).OSPartialPaymentAmount = ((IMatching)TestARInvoice4).OSOutstandingAmount;
			AssertNoErrors("Validation for matched transactions must work.", ((IMatching)TestARInvoice4).OSPartialPaymentAmountInfo);

			TestMatchingBase.UnmatchedTransactions.Remove(TestARInvoice1);
			TestMatchingBase.UnmatchingExcludedTransactions.Remove(TestARInvoice2);
			TestMatchingBase.MatchedTransactions.Remove(TestARInvoice4);

			TestMatchingBase.MatchedTransactions.Add(TestARInvoice1);
			TestMatchingBase.MatchedTransactions.Add(TestARInvoice2);
			TestMatchingBase.UnmatchedTransactions.Add(TestARInvoice4);

			((IMatching)TestARInvoice1).OSPartialPaymentAmount = -((IMatching)TestARInvoice1).OSOutstandingAmount;
			((IMatching)TestARInvoice2).OSPartialPaymentAmount = -((IMatching)TestARInvoice2).OSOutstandingAmount;
			((IMatching)TestARInvoice4).OSPartialPaymentAmount = -((IMatching)TestARInvoice4).OSOutstandingAmount;
			AssertHasErrors("Validation for matched transactions must work.", ((IMatching)TestARInvoice1).OSPartialPaymentAmountInfo);
			AssertHasErrors("Validation for matched transactions must work.", ((IMatching)TestARInvoice2).OSPartialPaymentAmountInfo);
			AssertNoErrors("Validation for unmatched transactions must be suspented.", ((IMatching)TestARInvoice4).OSPartialPaymentAmountInfo);

			((IMatching)TestARInvoice1).OSPartialPaymentAmount = ((IMatching)TestARInvoice1).OSOutstandingAmount;
			((IMatching)TestARInvoice2).OSPartialPaymentAmount = ((IMatching)TestARInvoice2).OSOutstandingAmount;
			AssertNoErrors("Validation for matched transactions must work.", ((IMatching)TestARInvoice1).OSPartialPaymentAmountInfo);
			AssertNoErrors("Validation for matched transactions must work.", ((IMatching)TestARInvoice2).OSPartialPaymentAmountInfo);
		}

		public void TestLedgerType()
		{
			AssertEquals("Object should have correct LedgerType", LedgerType, TestMatchingBase.LedgerType);
		}

		#region Clearing Journals Creation

		public void TestClearingJournalsCreation_DontCreateJournals()
		{
			AccountingConfigurationRegistry.Instance.ClearingJournalConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ClearingJournalConfigurationTypes.Standard.Code);
			AssertClearingJournalsCreation(false);
		}

		public void TestClearingJournalsCreation_HeaderBranch()
		{
			AccountingConfigurationRegistry.Instance.ClearingJournalConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ClearingJournalConfigurationTypes.HeaderBranch.Code);
			AssertClearingJournalsCreation();
		}

		public void TestClearingJournalsCreation_LineBranchPerHeader()
		{
			AccountingConfigurationRegistry.Instance.ClearingJournalConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ClearingJournalConfigurationTypes.LineBranchPerHeader.Code);
			AssertClearingJournalsCreation();
		}

		public void TestClearingJournalsCreation_LineBranchPerMatching()
		{
			AccountingConfigurationRegistry.Instance.ClearingJournalConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ClearingJournalConfigurationTypes.LineBranchPerMatching.Code);
			AssertClearingJournalsCreation();
		}

		public void TestClearingJournalsCreation_BranchDepartmentCombinations_NoErrors()
		{
			GlbBranch.CurrentBranch.AllowedDepartments.DeleteAll();
			AccountingConfigurationRegistry.Instance.ClearingJournalConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ClearingJournalConfigurationTypes.HeaderBranch.Code);
			AssertClearingJournalsCreation();
			AssertEquals("Should not contain the error", TestMatchingBase.MatchingErrorsForGUINotificationOnlyInfo.HasErrors(), false);
		}

		public void TestClearingJournalsCreation_DescriptionExceedsMaxLength()
		{
			if (!(TestMatchingBase is PaymentApprovalMatchingBase))
			{
				GlbBranch.CurrentBranch.AllowedDepartments.DeleteAll();
				AccountingConfigurationRegistry.Instance.ClearingJournalConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ClearingJournalConfigurationTypes.HeaderBranch.Code);
				(var invoice1, var invoice2) = CreateSetupForClearingJournals();

				var description = "1234567890"
					+ "1234567890"
					+ "1234567890"
					+ "1234567890"
					+ "1234567890"
					+ "1234567890"
					+ "1234567890"
					+ "1234567890"
					+ "1234567890"
					+ "1234567890"
					+ "1234567890"
					+ "1234567890"
					+ "12345678";
				AssertEquals("Precondition: Description equal to max allowable length", description.Length, AccTransactionHeaderSchema.AH_Desc.MaxLength);

				TestMatchingBase.Match_ForTestOnly();

				var matchGroupNumberWithSpace = TestMatchingBase.MatchGroupNumber + " ";

				AssertEquals("Precondition: Count", 2, TestMatchingBase.DynamicTransactions.Count);
				var journal1 = (Journal)TestMatchingBase.DynamicTransactions[0];
				var journal2 = (Journal)TestMatchingBase.DynamicTransactions[1];

				AssertEquals("PreCondition: TransactionCategory", Constants.TransactionCategory.Codes.Clearing, journal1.TransactionCategory);
				AssertEquals("MatchGroupNumber is appended as prefix", true, journal1.AH_Desc.StartsWith(TestMatchingBase.MatchGroupNumber + " "));

				AssertEquals("PreCondition: TransactionCategory", Constants.TransactionCategory.Codes.Clearing, journal2.TransactionCategory);
				AssertEquals("MatchGroupNumber is appended as prefix", true, journal2.AH_Desc.StartsWith(TestMatchingBase.MatchGroupNumber + " "));

				journal1.AH_Desc = description;
				journal2.AH_Desc = description;

				TestMatchingBase.SetDescriptionOnMatchLinksOnSaving();

				AssertEquals("AH_Desc is truncated and MatchGroupNumber is appended as prefix", matchGroupNumberWithSpace + description.Substring(0, description.Length - matchGroupNumberWithSpace.Length), journal1.AH_Desc);
				AssertEquals("AH_Desc is truncated and MatchGroupNumber is appended as prefix", matchGroupNumberWithSpace + description.Substring(0, description.Length - matchGroupNumberWithSpace.Length), journal2.AH_Desc);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestClearingJournalsCreation_BranchDepartmentCombinations_Errors()
		{
			if (TestMatchingBase is PaymentApprovalMatchingBase)
			{
				Assert(true);
			}
			else
			{
				var bbbDepartment = Factory.NewWithValidTestData<GlbDepartment>();
				Factory.Save();
				var currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);

				GlbBranchCombinationValidationTest.SetAllowedBranchDepartmentCombinations(currentBranch, new GlbDepartment[] { bbbDepartment });
				AccountingConfigurationRegistry.Instance.ClearingJournalConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ClearingJournalConfigurationTypes.HeaderBranch.Code);
				CreateSetupForClearingJournals();

				AssertEquals("Must not contain the error now", TestMatchingBase.MatchingErrorsForGUINotificationOnlyInfo.HasErrors(), false);
				TestMatchingBase.Match_ForTestOnly();

				AssertEquals("Must contain the error", TestMatchingBase.MatchingErrorsForGUINotificationOnlyInfo.HasErrors(), true);

				var errors = string.Join("|", TestMatchingBase.MatchingErrorsForGUINotificationOnlyInfo.GetErrors().Select(e => e.Message).ToArray());
				AssertContains("Must contain the error message", string.Format(@"The department {0} cannot be used with the branch {1}.
To change this configuration, set up the Branch/Department Combinations in the Edit Branch Window > Departments Tab."
								, Env.CurrentDepartment.Code, Env.CurrentBranch.Code), errors);
			}
		}

		void AssertClearingJournalsCreation(bool areClearingJournalsSetToBeCreated = true)
		{
			(var invoice1, var invoice2) = CreateSetupForClearingJournals();
			TestMatchingBase.Match_ForTestOnly();

			if (TestMatchingBase is PaymentApprovalMatchingBase)
			{
				AssertEquals("DynamicTransactions should be created now because on payment saving we call base Match method that tested below.", 0, TestMatchingBase.DynamicTransactions.Count);
			}
			else
			{
				if (areClearingJournalsSetToBeCreated)
				{
					AssertEquals("DynamicTransactions.Count", 2, TestMatchingBase.DynamicTransactions.Count);
					AssertEquals("DynamicTransactions[0].TransactionType", TransactionTypes.Journal, TestMatchingBase.DynamicTransactions[0].TransactionType);
					AssertEquals("DynamicTransactions[0].TransactionCategory", Constants.TransactionCategory.Codes.Clearing, TestMatchingBase.DynamicTransactions[0].TransactionCategory);
					AssertEquals("MatchGroupNumber is appended as prefix", true, TestMatchingBase.DynamicTransactions[0].Description.StartsWith(TestMatchingBase.MatchGroupNumber + " "));
					AssertEquals("DynamicTransactions[1].TransactionType", TransactionTypes.Journal, TestMatchingBase.DynamicTransactions[1].TransactionType);
					AssertEquals("DynamicTransactions[1].TransactionCategory", Constants.TransactionCategory.Codes.Clearing, TestMatchingBase.DynamicTransactions[1].TransactionCategory);
					AssertEquals("MatchGroupNumber is appended as prefix", true, TestMatchingBase.DynamicTransactions[1].Description.StartsWith(TestMatchingBase.MatchGroupNumber + " "));

					TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
					matchLinks.Load(new ZQuery(AccTransactionMatchLinkSchema.AP_MatchGroupNum, TestMatchingBase.MatchGroupNumber));
					AssertEquals("MatchLinks.Count", 4, matchLinks.Count);

					AssertNotNull("There should be a match link for the first invoice", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice1.PK)));
					AssertNotNull("There should be a match link for the second invoice", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice2.PK)));
					AssertNotNull("There should be a match link for the first clearing journal", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, ((BusinessObject)TestMatchingBase.DynamicTransactions[0]).PK)));
					AssertNotNull("There should be a match link for the first clearing journal", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, ((BusinessObject)TestMatchingBase.DynamicTransactions[1]).PK)));
				}
				else
				{
					AssertEquals("DynamicTransactions.Count", 1, TestMatchingBase.DynamicTransactions.Count);
					AssertType("Contra should be created.", typeof(Contra), TestMatchingBase.DynamicTransactions[0]);
				}
			}
		}

		(InvoicingBase, InvoicingBase) CreateSetupForClearingJournals()
		{
			AccGLHeader glAccount = TestObjectCreator.CreateAPSuspenseControlAccount();
			AccountingConfigurationRegistry.Instance.ClearingJournalClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glAccount.PK.ToGuid());
			TestMatchingBase.PrimaryOrganization = TestObjectCreator.AALSHI.PK;

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1M);
			invoice1.AH_OH = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.AUD, 1M, 10M, GlbBranch.CurrentBranch.PK, false);

			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV3", TestObjectCreator.AUD, 1M);
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.AUD, 1M, 10M, GlbBranch.CurrentBranch.PK, false);

			TestMatchingBase.AddIMatching(invoice1);
			TestMatchingBase.AddIMatching(invoice2);
			TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

			return (invoice1, invoice2);
		}

		#endregion

		public void TestRefreshExistingPaymentApprovalItems()
		{
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			apInvoice.AH_InvoiceAmount = -90M;
			apInvoice.AH_OutstandingAmount = -90M;

			Factory.Save();

			var newFactory1 = new BusinessObjectFactory();
			var payment1 = newFactory1.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			payment1.AV_Amount = 90M;
			var item1 = newFactory1.NewWithValidTestData<PaymentApprovalItem>();
			item1.A2_PaymentThisRun = 90M;
			item1.A2_AH = apInvoice.PK;
			item1.A2_AV = payment1.PK;

			AssertEquals(0, apInvoice.ExistingPaymentApprovalItems.Count);
			newFactory1.Save();
			AssertEquals(0, apInvoice.ExistingPaymentApprovalItems.Count);

			fTestMatchingBase = new APMatchingBase(Factory);
			fTestMatchingBase.RefreshExistingPaymentApprovalItems(apInvoice);
			AssertEquals(1, apInvoice.ExistingPaymentApprovalItems.Count);
		}

		public virtual void TestDontDeleteMatchLinksIfDoNotSaveFactoryOnMatching()
		{
			MatchingBase matchingObject = GetTestMatchingBase();
			matchingObject.PrimaryOrganization = TestObjectCreator.AALSHI.PK;
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1M);
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 10M, GlbBranch.CurrentBranch.PK, false);

			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV3", TestObjectCreator.AUD, 1M);
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.AUD, 1M, 10M, GlbBranch.CurrentBranch.PK, false);

			matchingObject.AddIMatching(invoice);
			matchingObject.AddIMatching(invoice2);
			matchingObject.MatchedTransactions.SetPartialPaidAmount();

			matchingObject.DoNotSaveFactoryOnMatching = true;
			matchingObject.MatchAndClearTransactions();
			AssertEquals("MatchLinks should not be deleted.", true, matchingObject.MatchLinks.Count > 0);

			matchingObject = GetTestMatchingBase();
			matchingObject.AddIMatching(invoice);
			matchingObject.AddIMatching(invoice2);
			matchingObject.MatchedTransactions.SetPartialPaidAmount();
			matchingObject.DoNotSaveFactoryOnMatching = false;
			matchingObject.MatchAndClearTransactions();
			AssertEquals("MatchLinks can be deleted now.", 0, matchingObject.MatchLinks.Count);
		}

		public void FindJournalsWithOppositeAmountTest()
		{
			AccGLHeader controlAccount = TestObjectCreator.GetGLAccountFromDB("8810.00.00");

			APInvoice apInvoice = Factory.NewWithValidTestData<APInvoice>();
			ARInvoice arInvoice = Factory.NewWithValidTestData<ARInvoice>();

			Journal journal1 = Factory.NewWithValidTestData<APJournal>();
			journal1.AH_OH = TestObjectCreator.AALSHI.PK;
			journal1.AH_RX_NKTransactionCurrency = "EUR";
			journal1.AH_ExchangeRate = 0.5M;
			journal1.AH_OSExTaxAmount = 400M;
			journal1.AH_AG = controlAccount.PK;
			journal1.RelatedInvoice = apInvoice;

			AssertEquals("Nothing with opposite amount", 0, MatchingBase.FindJournalsWithOppositeAmount(journal1).Count());
			var journal1Copy = TestMatchingBase.CopyJournalWithOppositeAmount(journal1);
			AssertEquals("One with opposite amount", 1, MatchingBase.FindJournalsWithOppositeAmount(journal1).Count());
			journal1Copy.AH_OSExTaxAmount += 1;
			AssertEquals("Nothing with opposite amount", 0, MatchingBase.FindJournalsWithOppositeAmount(journal1).Count());
			TestMatchingBase.CopyJournalWithOppositeAmount(journal1);
			TestMatchingBase.CopyJournalWithOppositeAmount(journal1);
			AssertEquals("Nothing with opposite amount", 2, MatchingBase.FindJournalsWithOppositeAmount(journal1).Count());
		}

		public void TestMatchingWhenSubAccountChangeNonMandatoryToMandatory()
		{
			if (TestMatchingBase is PaymentApprovalMatchingBase)
			{
				Assert(true);
				return;
			}

			SetUpTestDataSet();
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			var glHeader = TestObjectCreator.CreateGLHeader();
			var subAccount = TestObjectCreator.CreateGLHeaderSubAccount(glHeader, "OH", false);

			var testARJournal = Factory.NewWithValidTestData<ARJournal>();
			testARJournal.AH_OH = TestOrg1.PK;
			testARJournal.AH_LocalExTaxAmount = -110M;
			testARJournal.AH_LocalOutstandingAmount = -110M;
			testARJournal.AH_OSTotalAmount = -110M;
			testARJournal.AH_AG = glHeader.PK;
			Factory.Save();

			AssertEquals("Precondition", 1, glHeader.SubAccountTypes.Count);
			Assert("Precondition", !glHeader.SubAccountTypes[0].ASA_IsSubClassValidationRuleMandatory);
			AssertEquals("GL Header Sub Account Mandatory is false, Journal Should not have Sub Accounts.", 0, testARJournal.SubAccounts.Count);

			subAccount.ASA_IsSubClassValidationRuleMandatory = true;
			Factory.Save();

			AssertEquals("Precondition", 1, glHeader.SubAccountTypes.Count);
			Assert("Precondition", glHeader.SubAccountTypes[0].ASA_IsSubClassValidationRuleMandatory);

			testARJournal = Factory.CreateNewFactory().Load<ARJournal>(testARJournal.PK);
			TestMatchingBase.AddIMatching(testARJournal);

			TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();
			TestMatchingBase.RunPreSaveValidation();

			Assert("Precondition", testARJournal.IsInDatabase);
			AssertEquals("GL Header Sub Account Mandatory is true, but Journal is in database, so it shouldn't change sub accounts.", 0, testARJournal.SubAccounts.Count);
		}

		public void TestIsAllowedReturnsFalseWhenCashBookAllowFuturePostingOfCashBookTransactionsIsNull()
		{
			TestObjectCreator.ResetSecurityCore();
			AssertEquals(false, Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowedWithConstraint());
			APPayment apPayment = Factory.NewWithValidTestData<APPayment>();
			var apMatchingBase = new APMatchingBase(Factory, apPayment) as MatchingBase;
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			Assert(AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.Value);
			Assert("IsApplicableTransactionTypeForFuturePosting should be true", MatchingBase.IsApplicableTransactionTypeForFuturePosting(apPayment));
			Assert("CashBookAllowFuturePostingOfTransactions.IsAllowed should be false", !apMatchingBase.AllowFutureMatchDate_ForTestOnly);
		}

		#region CheckpointForNewMiscTransaction

		public void TestCheckpointForNewMiscTransaction()
		{
			var matchingBaseForTest = GetTestMatchingBase();
			AssertNull(matchingBaseForTest.CheckpointForNewMiscTransaction(TransactionTypes.Journal));
			AssertNull(matchingBaseForTest.CheckpointForNewMiscTransaction(TransactionTypes.ExchangeDifference));
			AssertNull(matchingBaseForTest.CheckpointForNewMiscTransaction(TransactionTypes.Discount));
			// Assertions for TransactionTypes.Overpayment are done in sub-classes
		}

		#endregion

		#region Cash Advance

		public virtual void TestCanCashAdvanceRequestBeMatched()
		{
			CommonCashAdvanceAssert();
		}

		public virtual void TestLoadCashAdvanceRequests()
		{
			CommonCashAdvanceAssert();
		}

		public void TestClearCashAdvanceRequests()
		{
			var cashAdvance = Factory.NewWithValidTestData<CashAdvanceRequestHeader>();
			TestMatchingBase.UnmatchedCashAdvanceRequests.Add(cashAdvance);
			AssertEquals(1, TestMatchingBase.UnmatchedCashAdvanceRequests.Count);

			TestMatchingBase.ClearCashAdvanceRequests();
			AssertEquals(0, TestMatchingBase.UnmatchedCashAdvanceRequests.Count);
		}

		public virtual void TestCashAdvanceFilter()
		{
			AssertNull(TestMatchingBase.CashAdvanceFilter);
		}

		public virtual void TestCashAdvanceFilterExcludesMatchedCashAdvances()
		{
			CommonCashAdvanceAssert();
		}

		public virtual void TestCashAdvanceMatchingJournalsAreCreated()
		{
			CommonCashAdvanceAssert();
		}

		public virtual void TestMoveAllCashAdvanceFromUnmatchToMatch()
		{
			CommonCashAdvanceAssert();
		}

		public virtual void TestMoveCashAdvanceFromMatchToUnmatch()
		{
			CommonCashAdvanceAssert();
		}

		public virtual void TestCashAdvanceIsUpdatedOnCreationOfMatchingJournal()
		{
			CommonCashAdvanceAssert();
		}

		void CommonCashAdvanceAssert()
		{
			if (!CanCashAdvanceRequestBeMatched)
			{
				Assert(true);
				return;
			}
			Assert(FormattableString.Invariant($@"Please check implementation of {nameof(CanCashAdvanceRequestBeMatched)} in {this.GetType()}.
If implementatiion is correct, then override this test in {this.GetType()}."), false);
		}

		protected void AssertCashAdvanceMatchingJournal(Journal journal, AccCashAdvanceRequestHeader cah, string expectedTransactionCategory, ZGuid expectedOrgHeader, ZString expectedDebitCreditSign, ZDateTime invoiceDate)
		{
			AssertEquals(nameof(journal.AH_TransactionCategory), expectedTransactionCategory, journal.AH_TransactionCategory);
			AssertEquals(nameof(journal.AH_OH), expectedOrgHeader, journal.AH_OH);
			AssertEquals(nameof(journal.AH_Desc), FormattableString.Invariant($"Payment of Advance Payment Request [{cah.CAH_RequestReferenceNumber}]"), journal.AH_Desc);
			AssertEquals(nameof(journal.DebitCreditSign), expectedDebitCreditSign, journal.DebitCreditSign);
			AssertEquals(nameof(journal.AH_RX_NKTransactionCurrency), cah.CAH_RX_NKTransactionCurrency, journal.AH_RX_NKTransactionCurrency);
			AssertEquals("OSPartialPaymentAmount", cah.CAH_OSOutstandingAmount, Math.Abs(((IMatching)journal).OSPartialPaymentAmount));
			AssertEquals("LocalPartialPaymentAmount", cah.CAH_LocalOutstandingAmount, Math.Abs(((IMatching)journal).LocalPartialPaymentAmount));
			AssertEquals(nameof(journal.AH_OSExTaxAmount), cah.CAH_OSOutstandingAmount, journal.AH_OSExTaxAmount);
			AssertEquals(nameof(journal.AH_LocalExTaxAmount), cah.CAH_LocalOutstandingAmount, journal.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal.AH_AG), TestObjectCreator.GLHeader1.PK, journal.AH_AG);
			AssertEquals(nameof(journal.AH_PostDate), TestMatchingBase.MatchDate, journal.AH_PostDate);
			AssertEquals(nameof(journal.AH_InvoiceDate), invoiceDate, journal.AH_InvoiceDate);
		}

		protected virtual bool CanCashAdvanceRequestBeMatched => false;

		#endregion

		#region Implementation

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		TaxFrameworkTestObjectCreator TFObjectCreator => tfObjectCreator ?? (tfObjectCreator = new TaxFrameworkTestObjectCreator(Factory));
		TaxFrameworkTestObjectCreator tfObjectCreator;

		#endregion
	}
}
