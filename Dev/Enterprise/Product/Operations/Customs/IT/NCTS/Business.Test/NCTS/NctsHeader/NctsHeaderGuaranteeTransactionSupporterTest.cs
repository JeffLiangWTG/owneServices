using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsHeaderGuaranteeTransactionSupporterTest : TestCaseWithFactory
{
	public void TestDeletePendingTransactions()
	{
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		nctsHeader.BH_JobReference = Reference;
		var transactions = CusGuaranteeLineTestHelper.GetGuaranteeLineTransactionTestCases(Factory, AppId, Reference);

		var guaranteeTransactionSupporter = new NctsHeaderPhase5CustomsLinkedObjectAdapter(nctsHeader) as IGuaranteeTransactionSupporter;
		guaranteeTransactionSupporter.DeletePendingTransactions(AppId);

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
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		nctsHeader.BH_JobReference = Reference;
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "24ITQ0B8TEK17951J4";
		var transactions = CusGuaranteeLineTestHelper.GetGuaranteeLineTransactionTestCases(Factory, AppId, Reference);

		var guaranteeTransactionSupporter = new NctsHeaderPhase5CustomsLinkedObjectAdapter(nctsHeader) as IGuaranteeTransactionSupporter;
		guaranteeTransactionSupporter.ConfirmPendingTransactions(AppId);

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
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		nctsHeader.BH_JobReference = Reference;
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = ZString.Empty;
		var transactions = CusGuaranteeLineTestHelper.GetGuaranteeLineTransactionTestCases(Factory, AppId, Reference);

		var guaranteeTransactionSupporter = new NctsHeaderPhase5CustomsLinkedObjectAdapter(nctsHeader) as IGuaranteeTransactionSupporter;
		guaranteeTransactionSupporter.ConfirmPendingTransactions(AppId);

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

	void ReloadTransactions(BaseCusGuaranteeLineTransaction[] transactions)
	{
		for (var i = 0; i < transactions.Length; i++)
		{
			transactions[i] = Factory.Load<BaseCusGuaranteeLineTransaction>(transactions[i].PK);
		}
	}

	const string Reference = "A0001";
	const string AppId = "0001";
}
