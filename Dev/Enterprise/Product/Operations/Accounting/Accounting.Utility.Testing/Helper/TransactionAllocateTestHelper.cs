using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Utility.Testing
{
	public class TransactionAllocateTestHelper
	{
		public TransactionAllocateTestHelper(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}
		readonly BusinessObjectFactory Factory;

		public TransactionHeaderCollection GetCollectionByTheCountOfElements(List<TransactionHeaderCollection> transactions, int countOfElements)
		{
			foreach (TransactionHeaderCollection collection in transactions)
			{
				if (collection.Count == countOfElements)
				{
					return collection;
				}
			}
			return null;
		}

		public PaymentApprovalBase GetNewPaymentApprovalReadyToPost(Type paymentApprovalType, ZString invoiceTransactionNum, ZString chequeOrReference, ZDecimal amount)
		{
			PaymentApprovalBase approval = (PaymentApprovalBase)Factory.NewWithValidTestData(paymentApprovalType);

			approval.AV_PaymentType = ReceiptTypes.Cash;
			approval.AV_AB = Factory.NewWithValidTestData<AccBankAccount>().PK;
			approval.AV_OH = TestObjectCreator.TestOrganisation.PK;
			approval.AV_ChequeOrReference = chequeOrReference;
			approval.AV_Amount = amount;
			approval.AV_Status = PaymentApprovalStatus.FullyApproved;
			return approval;
		}

		public TransactionHeaderCollection GetCollectionWith2PaymentsReadyToAutoPrint()
		{
			TestAutoPrintChequeBook = GetAutoPrintChequeBook(1, 1, 3);
			APPayment newPayment = GetAutoAllocateAPPayment(TestAutoPrintChequeBook.PK, TestAutoPrintChequeBook.AK_AB);
			APPayment newPayment2 = GetAutoAllocateAPPayment(TestAutoPrintChequeBook.PK, TestAutoPrintChequeBook.AK_AB);
			TransactionHeaderCollection payments = new TransactionHeaderCollection(Factory);
			payments.Add(newPayment);
			payments.Add(newPayment2);
			return payments;
		}

		public TransactionHeaderCollection GetCollectionWith3Payments()
		{
			TestAutoPrintChequeBook = GetAutoPrintChequeBook(1, 2, 3);
			TestAutoPrintChequeBook2 = GetAutoPrintChequeBook(1, 2, 4);

			Approval1 = GetNewPaymentApprovalReadyToPost(typeof(APPaymentApprovalWithoutAuthorisation), "00001001", "", 1000M);
			Approval1.AV_Status = PaymentApprovalStatus.FullyApproved;
			Approval1.AV_PaymentType = ReceiptTypes.Cheque;
			Approval1.AV_AB = TestAutoPrintChequeBook.AK_AB;
			Approval1.AV_AK = TestAutoPrintChequeBook.PK;
			Assertion.Assert("Assert autoallocation is enabled", ((IChequeNumberAutoAllocation)Approval1).IsAutoAllocationEnabled);

			Approval2 = GetNewPaymentApprovalReadyToPost(typeof(ARPaymentApprovalWithoutAuthorisation), "00001002", "", 1000M);
			Approval2.AV_Status = PaymentApprovalStatus.FullyApproved;
			Approval2.AV_PaymentType = ReceiptTypes.Cheque;
			Approval2.AV_AB = TestAutoPrintChequeBook.AK_AB;
			Approval2.AV_AK = TestAutoPrintChequeBook.PK;
			Assertion.Assert("Assert autoallocation is enabled", ((IChequeNumberAutoAllocation)Approval2).IsAutoAllocationEnabled);

			Approval3 = GetNewPaymentApprovalReadyToPost(typeof(APPaymentApprovalWithoutAuthorisation), "00001003", "", 1000M);
			Approval3.AV_Status = PaymentApprovalStatus.FullyApproved;
			Approval3.AV_PaymentType = ReceiptTypes.Cheque;
			Approval3.AV_AB = TestAutoPrintChequeBook2.AK_AB;
			Approval3.AV_AK = TestAutoPrintChequeBook2.PK;
			Assertion.Assert("Assert autoallocation is enabled", ((IChequeNumberAutoAllocation)Approval3).IsAutoAllocationEnabled);

			Factory.Save();

			Assertion.AssertNotNull("Approval1 Payment", Approval1.TransactionHeader);
			Assertion.AssertNotNull("Approval4 Payment", Approval2.TransactionHeader);
			Assertion.AssertNotNull("Approval3 Payment", Approval3.TransactionHeader);

			TransactionHeaderCollection paymentCollection = new TransactionHeaderCollection(Factory);
			paymentCollection.Add(Approval1.NewPayment);
			paymentCollection.Add(Approval2.NewPayment);
			paymentCollection.Add(Approval3.NewPayment);

			return paymentCollection;
		}

		public APPaymentApprovalWithoutAuthorisationCollection GetCollectionWith3PaymentApprovals()
		{
			TestAutoPrintChequeBook = GetAutoPrintChequeBook(1, 2, 3);
			TestAutoPrintChequeBook2 = GetAutoPrintChequeBook(1, 2, 4);

			Approval1 = GetNewPaymentApprovalReadyToPost(typeof(APPaymentApprovalWithoutAuthorisation), "00001001", "", 1000M);
			Approval1.AV_Status = PaymentApprovalStatus.FullyApproved;
			Approval1.AV_PaymentType = ReceiptTypes.Cheque;
			Approval1.AV_AB = TestAutoPrintChequeBook.AK_AB;
			Approval1.AV_AK = TestAutoPrintChequeBook.PK;
			Assertion.Assert("Assert autoallocation is enabled", ((IChequeNumberAutoAllocation)Approval1).IsAutoAllocationEnabled);

			Approval2 = GetNewPaymentApprovalReadyToPost(typeof(APPaymentApprovalWithoutAuthorisation), "00001002", "", 1000M);
			Approval2.AV_Status = PaymentApprovalStatus.FullyApproved;
			Approval2.AV_PaymentType = ReceiptTypes.Cheque;
			Approval2.AV_AB = TestAutoPrintChequeBook.AK_AB;
			Approval2.AV_AK = TestAutoPrintChequeBook.PK;
			Assertion.Assert("Assert autoallocation is enabled", ((IChequeNumberAutoAllocation)Approval2).IsAutoAllocationEnabled);

			Approval3 = GetNewPaymentApprovalReadyToPost(typeof(APPaymentApprovalWithoutAuthorisation), "00001003", "", 1000M);
			Approval3.AV_Status = PaymentApprovalStatus.FullyApproved;
			Approval3.AV_PaymentType = ReceiptTypes.Cheque;
			Approval3.AV_AB = TestAutoPrintChequeBook2.AK_AB;
			Approval3.AV_AK = TestAutoPrintChequeBook2.PK;
			Assertion.Assert("Assert autoallocation is enabled", ((IChequeNumberAutoAllocation)Approval3).IsAutoAllocationEnabled);

			Factory.Save();

			Assertion.AssertNotNull("Approval1 Payment", Approval1.TransactionHeader);
			Assertion.AssertNotNull("Approval4 Payment", Approval2.TransactionHeader);
			Assertion.AssertNotNull("Approval3 Payment", Approval3.TransactionHeader);

			return new APPaymentApprovalWithoutAuthorisationCollection(Factory) { Approval1, Approval2, Approval3 };
		}

		protected Job CreateJob(ZString jobNumber, OrgHeader localClient, bool billLocalClientInLocalCurrency, decimal localClientCFX,
		OrgHeader agent, bool billAgentInLocalCurrency, decimal agentCFX)
		{
			Job job = Factory.NewJobForTesting<Job>();
			job.JH_JobNum = jobNumber;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.LocalChargesPK = localClient.PK;
			job.AgentCollectPK = agent.PK;
			job.JH_LocalChargesCFX = localClientCFX;
			job.JH_AgentChargesCFX = agentCFX;
			return job;
		}

		protected Charge CreateCharge(Job parentJob, AccChargeCode chargeCode, ZString desc, RefCurrency costCurrency, ZDecimal oSCostAmt, OrgHeader creditor,
				RefCurrency sellCurrency, ZDecimal oSSellAmt, OrgHeader debtor)
		{
			Charge charge = parentJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_Desc = desc;

			charge.JR_OH_CostAccount = creditor != null ? creditor.PK : ZGuid.Empty;
			charge.JR_RX_NKCostCurrency = costCurrency != null ? costCurrency.RX_Code : ZString.Empty;
			charge.JR_OSCostAmt = oSCostAmt;

			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_RX_NKSellCurrency = sellCurrency.RX_Code;
			charge.JR_OSSellAmt = oSSellAmt;
			return charge;
		}

		protected AccChargeCode CreateChargeCode(string code, string description, string chargeType, decimal marginPercentage, AccTaxRate gST, AccWithholding wHT)
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = code;
			chargeCode.AC_Desc = description;
			chargeCode.AC_ChargeType = chargeType;
			chargeCode.AC_MarginPercentage = marginPercentage;
			chargeCode.AC_AT_GSTRate = gST != null ? gST.PK : ZGuid.Empty;
			chargeCode.AC_AW_WithholdingTaxRate = wHT != null ? wHT.PK : ZGuid.Empty;
			return chargeCode;
		}

		protected AccTaxRate CreateTaxRate(string code, string description, int rate)
		{
			AccTaxRate taxRate = Factory.New<AccTaxRate>();
			taxRate.AT_Code = code;
			taxRate.AT_Description = description;
			taxRate.AT_IsActive = true;
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.SetRateNumerator_ForTestOnly(rate);
			return taxRate;
		}

		public AccChequeBook GetAutoPrintChequeBook(ZDecimal startNo, ZDecimal currentNo, ZDecimal lastNo)
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			var printQueue = TestObjectCreator.CreatePrintQueue(Factory);
			var chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			chequeBook.AK_StartNo = startNo;
			chequeBook.AK_CurrentNo = currentNo;
			chequeBook.AK_LastNo = lastNo;
			Assertion.Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			Factory.Save();
			return chequeBook;
		}

		public AccChequeBook GetChequeBook(ZDecimal startNo, ZDecimal currentNo, ZDecimal lastNo)
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_ChequeNumDigits = 1;
			var chequeTemplate = Factory.NewWithValidTestData<StmTemplate>();
			bankAccount.AB_SO_ChequeTemplate = chequeTemplate.PK;
			var printQueue = TestObjectCreator.CreatePrintQueue(Factory);
			var chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			chequeBook.AK_StartNo = startNo;
			chequeBook.AK_CurrentNo = currentNo;
			chequeBook.AK_LastNo = lastNo;
			Assertion.Assert("Cheque Book should NOT be AutoPrint", !chequeBook.IsAutoPrint);
			Factory.Save();
			return chequeBook;
		}

		public APPayment GetAutoAllocateAPPayment(ZGuid chequeBookPK, ZGuid bankAccountPK)
		{
			APPayment newPayment = Factory.NewWithValidTestData<APPayment>();
			newPayment.AH_OH = TestObjectCreator.TestOrganisation.PK;
			newPayment.AH_ReceiptType = ReceiptTypes.Cheque;
			newPayment.AH_AB = bankAccountPK;
			newPayment.ChequeBook = chequeBookPK;
			return newPayment;
		}

		public AccChequeBook TestAutoPrintChequeBook;
		public AccChequeBook TestAutoPrintChequeBook2;

		public PaymentApprovalBase Approval1;
		public PaymentApprovalBase Approval2;
		public PaymentApprovalBase Approval3;

		public APPayment TestPayment1;
		public APPayment TestPayment2;

		public Charge Charge1;
		public Charge Charge2;
		public Charge Charge3;

		public JobConsolCost Cost;
		public JobConsolCost Cost2;

		public TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
