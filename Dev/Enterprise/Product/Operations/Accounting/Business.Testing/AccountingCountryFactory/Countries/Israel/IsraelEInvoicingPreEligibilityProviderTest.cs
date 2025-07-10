using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Israel;
using Enterprise.Accounting.Business.AccountingCountryFactory.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.AccountingCountryFactory.Countries.Israel;

[TestedType(typeof(IsraelEInvoicingPreEligibilityProvider))]
public class IsraelEInvoicingPreEligibilityProviderTest : EInvoicingPreEligibilityProviderTest
{
	protected override string CountryCode => CountryCodes.Israel;

	[TestDate(2024, 1, 14, 18, 10, 12)]
	public override void TestCanEvaluateByComplianceDate()
	{
		AssertCanEvaluateByComplianceDate_ShouldUsePostDateForAR();
		AssertCanEvaluateByComplianceDate_ShouldUseInvoiceDateForAP();
	}

	void AssertCanEvaluateByComplianceDate_ShouldUsePostDateForAR()
	{
		var provider = GetInvoicingPreEligibilityProvider();

		var arTransaction = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
		arTransaction.AH_PostDate = ZDateTime.Now;

		AssertEquals(false, provider?.CanEvaluateByComplianceDate(arTransaction, DateTime.MinValue));
		var complianceDateBeforePostDate = arTransaction.AH_PostDate.AddDays(-1).ToDateTime();
		AssertEquals("AR, Post Date > Compliance Date", true, provider?.CanEvaluateByComplianceDate(arTransaction, complianceDateBeforePostDate));
		var complianceDateEqualToPostDate = arTransaction.AH_PostDate.ToDateTime();
		AssertEquals("AR, Post Date == Compliance Date", true, provider?.CanEvaluateByComplianceDate(arTransaction, complianceDateEqualToPostDate));
		var complianceDateAfterPostDate = arTransaction.AH_PostDate.AddDays(1).ToDateTime();
		AssertEquals("AR, Post Date < Compliance Date", false, provider?.CanEvaluateByComplianceDate(arTransaction, complianceDateAfterPostDate));
	}

	void AssertCanEvaluateByComplianceDate_ShouldUseInvoiceDateForAP()
	{
		var provider = GetInvoicingPreEligibilityProvider();

		var apTransaction = TestObjectCreator.CreateAPInvoice<APInvoice>("APINV001", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.ABIGAS);
		apTransaction.AH_InvoiceDate = ZDateTime.Now;

		AssertEquals(false, provider?.CanEvaluateByComplianceDate(apTransaction, DateTime.MinValue));
		var complianceDateBeforeInvoiceDate = apTransaction.AH_InvoiceDate.AddDays(-1).ToDateTime();
		AssertEquals("AP, Invoice Date > Compliance Date", true, provider?.CanEvaluateByComplianceDate(apTransaction, complianceDateBeforeInvoiceDate));
		var complianceDateEqualToInvoiceDate = apTransaction.AH_InvoiceDate.ToDateTime();
		AssertEquals("AP, Invoice Date == Compliance Date", true, provider?.CanEvaluateByComplianceDate(apTransaction, complianceDateEqualToInvoiceDate));
		var complianceDateAfterInvoiceDate = apTransaction.AH_InvoiceDate.AddDays(1).ToDateTime();
		AssertEquals("AP, Invoice Date < Compliance Date", false, provider?.CanEvaluateByComplianceDate(apTransaction, complianceDateAfterInvoiceDate));
	}

	public override void TestCanEvaluateByTransaction()
	{
		var complianceInfoEInvoice = GetInvoicingPreEligibilityProvider();
		AssertNotNull(complianceInfoEInvoice);

		var trans = Factory.NewWithValidTestData<AccTransactionHeader>();
		AssertEquals("Return true if transaction is not in DB", true, complianceInfoEInvoice.CanEvaluateByTransaction(trans));

		trans.AH_Ledger = LedgerTypes.IncompleteTransactions;
		trans.AH_TransactionType = TransactionTypes.IncompleteInvoice;
		Factory.Save();

		AssertEquals(false, complianceInfoEInvoice.CanEvaluateByTransaction(trans));

		trans.AH_Ledger = LedgerTypes.AccountsPayable;
		trans.AH_TransactionType = TransactionTypes.Invoice;
		AssertEquals(true, complianceInfoEInvoice.CanEvaluateByTransaction(trans));

		trans.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
		trans.AH_TransactionType = TransactionTypes.UAInvoice;
		AssertEquals(false, complianceInfoEInvoice.CanEvaluateByTransaction(trans));

		trans.AH_Ledger = LedgerTypes.AccountsPayable;
		trans.AH_TransactionType = TransactionTypes.Invoice;
		AssertEquals(true, complianceInfoEInvoice.CanEvaluateByTransaction(trans));

		trans.AH_Ledger = LedgerTypes.TransactionsPendingAllocation;
		trans.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
		AssertEquals(false, complianceInfoEInvoice.CanEvaluateByTransaction(trans));

		trans.AH_Ledger = LedgerTypes.AccountsPayable;
		trans.AH_TransactionType = TransactionTypes.Invoice;
		AssertEquals(true, complianceInfoEInvoice.CanEvaluateByTransaction(trans));
	}

	TestObjectCreator testObjectCreator;
	TestObjectCreator TestObjectCreator
	{
		get { return testObjectCreator ??= new TestObjectCreator(Factory); }
	}
}
