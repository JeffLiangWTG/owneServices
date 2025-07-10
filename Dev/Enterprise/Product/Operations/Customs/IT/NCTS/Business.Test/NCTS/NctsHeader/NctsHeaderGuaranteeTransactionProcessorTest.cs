using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsHeaderGuaranteeTransactionProcessorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When nctsHeader is null", () => new NctsHeaderGuaranteeTransactionProcessor(nctsHeader: null));
		AssertNoExceptionThrown("When nctsHeader is not null", () => new NctsHeaderGuaranteeTransactionProcessor(nctsHeader: Factory.New<NctsHeader>()));
	}

	public void TestAddNewConsumeTransaction()
	{
		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var permitHolder = Factory.NewWithValidTestData<OrgHeader>();
		var guaranteeHeader = CusGuaranteeHeaderTestUtility.SetupGuarantee(Factory, permitHolder, "111", "XYZ");
		var openingBalanceTransaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
		openingBalanceTransaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
		openingBalanceTransaction.CPL_TranValue = 999m;
		nctsHeader.BH_JobReference = "123ABC";
		nctsHeader.Principal.E2_OA_Address = permitHolder.MainAddress.PK;
		nctsHeader.Guarantees.RemoveAndDeleteAll();

		var consumeTransactions = guaranteeProcessor.AddNewConsumeTransaction();
		AssertEquals("CASE 0: When NctsHeader does not have any Guarantee added, ConsumeTransaction", 0, consumeTransactions.Count);

		var guarantee = nctsHeader.Guarantees.AddNew();
		guarantee.PW_BondAmount = 999m;
		guarantee.PW_BondNumber = "AAA";
		guarantee.PW_BondType = "3";
		consumeTransactions = guaranteeProcessor.AddNewConsumeTransaction();
		AssertEquals("CASE 1: When NctsHeader has a Guarantee added but does not match any registered GuaranteeHeader, ConsumeTransaction", 0, consumeTransactions.Count);

		guarantee.PW_BondNumber = "XYZ";
		consumeTransactions = guaranteeProcessor.AddNewConsumeTransaction();
		AssertEquals("CASE 2: When NctsHeader has a Guarantee added and match a registered GuaranteeHeader, ConsumeTransaction", 1, consumeTransactions.Count);
		CombineAssertions("CASE 2: Created ConsumeTransaction Fields", () =>
		{
			CusGuaranteeLineTestHelper.AssertGuaranteeLineTransaction(consumeTransactions.ElementAt(0)
				, appId: ""
				, comment: "NCTS departure 123ABC"
				, transactionDate: ZDateTime.Now
				, transactionType: PermitTransactionTypeList.Codes.TRA
				, transactionCategory: PermitTransactionCategoryList.Codes.CUM
				, transactionStatus: PermitTransactionStatusList.Codes.Pending
				, reference: "123ABC"
				, transactionValue: -999m
			);
		});

		guaranteeHeader.CusGuaranteeLineTransactions.Delete(consumeTransactions.ElementAt(0));
		guarantee.PW_BondAmount = -888m;
		consumeTransactions = guaranteeProcessor.AddNewConsumeTransaction();
		AssertEquals("CASE 3: When PW_BondAmount is negative, NctsHeader has a Guarantee added and match a registered GuaranteeHeader, ConsumeTransaction", 1, consumeTransactions.Count);
		CombineAssertions("CASE 3: Created ConsumeTransaction Fields", () =>
		{
			CusGuaranteeLineTestHelper.AssertGuaranteeLineTransaction(consumeTransactions.ElementAt(0)
				, appId: ""
				, comment: "NCTS departure 123ABC"
				, transactionDate: ZDateTime.Now
				, transactionType: PermitTransactionTypeList.Codes.TRA
				, transactionCategory: PermitTransactionCategoryList.Codes.CUM
				, transactionStatus: PermitTransactionStatusList.Codes.Pending
				, reference: "123ABC"
				, transactionValue: -888m
			);
		});
	}

	public void TestAddNewWriteOffTransaction()
	{
		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var permitHolder = Factory.NewWithValidTestData<OrgHeader>();
		var guaranteeHeader = CusGuaranteeHeaderTestUtility.SetupGuarantee(Factory, permitHolder, "111", "XYZ");
		nctsHeader.BH_JobReference = "123ABC";
		nctsHeader.Principal.E2_OA_Address = permitHolder.MainAddress.PK;
		nctsHeader.Guarantees.RemoveAndDeleteAll();

		var writeOffTransactions = guaranteeProcessor.AddNewWriteOffTransaction(applicationId: "123456", new ZDate(2022, 01, 01));
		AssertEquals("CASE 0: When NctsHeader does not have any Guarantee added, WriteOffTransaction", 0, writeOffTransactions.Count);

		var guarantee = nctsHeader.Guarantees.AddNew();
		guarantee.PW_BondAmount = 999m;
		guarantee.PW_BondNumber = "AAA";
		guarantee.PW_BondType = "3";
		writeOffTransactions = guaranteeProcessor.AddNewWriteOffTransaction(applicationId: "123456", new ZDate(2022, 01, 01));
		AssertEquals("CASE 1: When NctsHeader has a Guarantee added but does not match any registered GuaranteeHeader, WriteOffTransaction", 0, writeOffTransactions.Count);

		guarantee.PW_BondNumber = "XYZ";
		writeOffTransactions = guaranteeProcessor.AddNewWriteOffTransaction(applicationId: "123456", new ZDate(2022, 01, 01));
		AssertEquals("CASE 2: When NctsHeader has a Guarantee added and match a registered GuaranteeHeader, WriteOffTransaction", 1, writeOffTransactions.Count);
		CombineAssertions("CASE 2: Created WriteOffTransaction Fields", () =>
		{
			CusGuaranteeLineTestHelper.AssertGuaranteeLineTransaction(writeOffTransactions.ElementAt(0)
				, appId: "123456"
				, comment: "NCTS write-off 123ABC"
				, transactionDate: new ZDate(2022, 01, 01)
				, transactionType: PermitTransactionTypeList.Codes.TRA
				, transactionCategory: PermitTransactionCategoryList.Codes.CUM
				, transactionStatus: PermitTransactionStatusList.Codes.Confirmed
				, reference: "123ABC"
				, transactionValue: 999m
			);
		});

		guaranteeHeader.CusGuaranteeLineTransactions.Delete(writeOffTransactions.ElementAt(0));
		guarantee.PW_BondAmount = -888m;
		writeOffTransactions = guaranteeProcessor.AddNewWriteOffTransaction(applicationId: "123456", new ZDate(2022, 01, 01));
		AssertEquals("CASE 3: When PW_BondAmount is negative, NctsHeader has a Guarantee added and match a registered GuaranteeHeader, WriteOffTransaction", 1, writeOffTransactions.Count);
		CombineAssertions("CASE 3: Created WriteOffTransaction Fields", () =>
		{
			CusGuaranteeLineTestHelper.AssertGuaranteeLineTransaction(writeOffTransactions.ElementAt(0)
				, appId: "123456"
				, comment: "NCTS write-off 123ABC"
				, transactionDate: new ZDate(2022, 01, 01)
				, transactionType: PermitTransactionTypeList.Codes.TRA
				, transactionCategory: PermitTransactionCategoryList.Codes.CUM
				, transactionStatus: PermitTransactionStatusList.Codes.Confirmed
				, reference: "123ABC"
				, transactionValue: 888m
			);
		});
	}

	public void TestDeletePendingTransactions()
	{
		var transactions = CusGuaranteeLineTestHelper.GetGuaranteeLineTransactionTestCases(Factory, AppId, Reference);
		guaranteeProcessor.DeletePendingTransactions(AppId);

		ReloadTransactions(transactions);

		CombineAssertions("Guarantee Transactions", () =>
		{
			AssertNull("First PND with matching AppId and Reference is deleted", transactions[0]);
			AssertNull("Second PND with matching AppId and Reference is deleted", transactions[1]);
			AssertNotNull("CON with matching AppId and Reference isn't deleted", transactions[2]);
			AssertNotNull("PND with matching Reference and different AppId isn't deleted", transactions[3]);
			AssertNotNull("PND with matching AppId and different Reference isn't deleted", transactions[4]);
		});
	}

	public void TestConfirmPendingTransactions()
	{
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "24ITQ0B8TEK17951J4";
		var transactions = CusGuaranteeLineTestHelper.GetGuaranteeLineTransactionTestCases(Factory, AppId, Reference);
		guaranteeProcessor.ConfirmPendingTransactions(AppId);

		ReloadTransactions(transactions);

		const string expectedPreviousComment = "NCTS departure test";
		const string expectedUpdatedComment = "NCTS departure 24ITQ0B8TEK17951J4";

		AssertTransaction("First PND transaction with matching AppId and Reference is updated", transactions[0], PermitTransactionStatusList.Codes.Confirmed, expectedUpdatedComment);
		AssertTransaction("Second PND transaction with matching AppId and Reference is updated", transactions[1], PermitTransactionStatusList.Codes.Confirmed, expectedUpdatedComment);
		AssertTransaction("CON transaction with matching AppId and Reference isn't updated", transactions[2], PermitTransactionStatusList.Codes.Confirmed, expectedPreviousComment);
		AssertTransaction("PND transaction with matching Reference and different AppId isn't updated", transactions[3], PermitTransactionStatusList.Codes.Pending, expectedPreviousComment);
		AssertTransaction("PND transaction with matching AppId and different Reference isn't updated", transactions[4], PermitTransactionStatusList.Codes.Pending, expectedPreviousComment);

		void AssertTransaction(string message, BaseCusGuaranteeLineTransaction transaction, string expectedStatus, string expectedComment)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("CPL_TransactionStatus", expectedStatus, transaction.CPL_TransactionStatus);
				AssertEquals("CPL_Comment", expectedComment, transaction.CPL_Comment);
			});
		}
	}

	public void TestConfirmPendingTransactions_WhenMovementReferenceEntryNumberIsEmpty()
	{
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = ZString.Empty;
		var transactions = CusGuaranteeLineTestHelper.GetGuaranteeLineTransactionTestCases(Factory, AppId, Reference);
		guaranteeProcessor.ConfirmPendingTransactions(AppId);

		ReloadTransactions(transactions);

		const string expectedComment = "NCTS departure test";

		CombineAssertions("First PND transaction with matching AppId and Reference isn't updated", () =>
		{
			AssertEquals("CPL_TransactionStatus", PermitTransactionStatusList.Codes.Pending, transactions[0].CPL_TransactionStatus);
			AssertEquals("CPL_Comment", expectedComment, transactions[0].CPL_Comment);
		});

		CombineAssertions("Second PND transaction with matching AppId and Reference isn't updated", () =>
		{
			AssertEquals("CPL_TransactionStatus", PermitTransactionStatusList.Codes.Pending, transactions[1].CPL_TransactionStatus);
			AssertEquals("CPL_Comment", expectedComment, transactions[1].CPL_Comment);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
		nctsHeader.BH_JobReference = Reference;
		guaranteeProcessor = new NctsHeaderGuaranteeTransactionProcessor(nctsHeader);
	}

	void ReloadTransactions(BaseCusGuaranteeLineTransaction[] transactions)
	{
		for (var i = 0; i < transactions.Length; i++)
		{
			transactions[i] = Factory.Load<BaseCusGuaranteeLineTransaction>(transactions[i].PK);
		}
	}

	const string Reference = "A0001";
	const string AppId = "0001";

	NctsHeader nctsHeader;
	NctsHeaderGuaranteeTransactionProcessor guaranteeProcessor;
}
