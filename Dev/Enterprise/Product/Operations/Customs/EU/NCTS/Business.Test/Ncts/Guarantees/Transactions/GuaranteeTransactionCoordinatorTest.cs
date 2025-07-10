using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class GuaranteeTransactionCoordinatorTest : TestCaseWithFactory
{
	public void TestLocalRefenceNumber()
	{
		AssertEquals("LocalReferenceNumber should be equal to BM_PaperlessInbondNum", movementHeader.BM_PaperlessInbondNum, movementHeader.GuaranteeTransactionCoordinator.LocalReferenceNumber);
	}

	public void TestCreatePendingTransactions()
	{
		var (guaranteeHeader, guaranteeHeader2) = SetupGuaranteeHeaders();

		var guarantee = movementHeader.Guarantees.AddNew();
		guarantee.PW_BondNumber = "GUA1";
		guarantee.PW_BondAmount = 150.0m;
		var guarantee2 = movementHeader.Guarantees.AddNew();
		guarantee2.PW_BondNumber = "GUA2";
		guarantee2.PW_BondAmount = 300.0m;

		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Initial situation GUA1", 1, guaranteeHeader.GetTransactions().Count());
			AssertEquals("Initial situation GUA2", 1, guaranteeHeader2.GetTransactions().Count());

			movementHeader.GuaranteeTransactionCoordinator.CreatePendingTransactions("messagenum");

			AssertEquals("Extra transaction on GUA1", 2, guaranteeHeader.GetTransactions().Count());
			var newTransactionGUA1 = guaranteeHeader.GetTransactions().Last();
			AssertEquals("New transaction GUA1 should have status 'PND'", Customs.Business.PermitTransactionStatusList.Codes.Pending, newTransactionGUA1.CPL_TransactionStatus);
			AssertEquals("New transaction GUA1 should reference the LRN of the NCTS Movement Header", "LRN123456789", newTransactionGUA1.CPL_Reference);
			AssertEquals("New transaction GUA1 amount must be -150", -150.0m, newTransactionGUA1.CPL_TranValue);
			AssertEquals("Extra transaction on GUA2", 2, guaranteeHeader2.GetTransactions().Count());
			var newTransactionGUA2 = guaranteeHeader2.GetTransactions().Last();
			AssertEquals("New transaction GUA2 should have status 'PND'", Customs.Business.PermitTransactionStatusList.Codes.Pending, newTransactionGUA2.CPL_TransactionStatus);
			AssertEquals("New transaction GUA2 should reference the LRN of the NCTS Movement Header", "LRN123456789", newTransactionGUA2.CPL_Reference);
			AssertEquals("New transaction GUA2 amount must be -300", -300.0m, newTransactionGUA2.CPL_TranValue);
		});
	}

	public void TestUpdatePendingTransactions()
	{
		var (guaranteeHeader, guaranteeHeader2) = SetupGuaranteeHeaders();

		movementHeader.Guarantees.RemoveAndDeleteAll();
		var guarantee = movementHeader.Guarantees.AddNew();
		guarantee.PW_BondNumber = "GUA1";
		guarantee.PW_BondAmount = 145.0m;

		guaranteeHeader.AddTransaction(movementHeader.BM_PaperlessInbondNum,
			"NCTS write-off " + movementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
			"messagenum",
			ZString.Empty,
			guarantee.PW_BondAmount * -1,
			0,
			status: Customs.Business.PermitTransactionStatusList.Codes.Pending);

		guarantee.PW_BondAmount = 150.0m;

		var guarantee2 = movementHeader.Guarantees.AddNew();
		guarantee2.PW_BondNumber = "GUA2";
		guarantee2.PW_BondAmount = 290.0m;

		guaranteeHeader2.AddTransaction(movementHeader.BM_PaperlessInbondNum,
			"NCTS write-off " + movementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
			"messagenum",
			ZString.Empty,
			guarantee2.PW_BondAmount * -1,
			0,
			status: Customs.Business.PermitTransactionStatusList.Codes.Pending);

		guarantee2.PW_BondAmount = 300.0m;

		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Initial situation GUA1", 2, guaranteeHeader.GetTransactions().Count());
			AssertEquals("Initial situation GUA2", 2, guaranteeHeader2.GetTransactions().Count());

			movementHeader.GuaranteeTransactionCoordinator.UpdatePendingTransactions("messagenum");

			AssertEquals("Extra transaction on GUA1", 3, guaranteeHeader.GetTransactions().Count());
			var newTransactionGUA1 = guaranteeHeader.GetTransactions().Last();
			AssertEquals("New transaction GUA1 should have status 'PND'", Customs.Business.PermitTransactionStatusList.Codes.Pending, newTransactionGUA1.CPL_TransactionStatus);
			AssertEquals("New transaction GUA1 should reference the LRN of the NCTS Movement Header", "LRN123456789", newTransactionGUA1.CPL_Reference);
			AssertEquals("New transaction GUA1 amount must be -5", -5.0m, newTransactionGUA1.CPL_TranValue);

			AssertEquals("Extra transaction on GUA2", 3, guaranteeHeader2.GetTransactions().Count());
			var newTransactionGUA2 = guaranteeHeader2.GetTransactions().Last();
			AssertEquals("New transaction GUA2 should have status 'PND'", Customs.Business.PermitTransactionStatusList.Codes.Pending, newTransactionGUA2.CPL_TransactionStatus);
			AssertEquals("New transaction GUA2 should reference the LRN of the NCTS Movement Header", "LRN123456789", newTransactionGUA2.CPL_Reference);
			AssertEquals("New transaction GUA2 amount must be -10", -10.0m, newTransactionGUA2.CPL_TranValue);
		});
	}

	public void TestUpdatePendingTransactions_GUA2Unused()
	{
		var (guaranteeHeader, guaranteeHeader2) = SetupGuaranteeHeaders();

		movementHeader.Guarantees.RemoveAndDeleteAll();
		var guarantee = movementHeader.Guarantees.AddNew();
		guarantee.PW_BondNumber = "GUA1";
		guarantee.PW_BondAmount = 145.0m;

		guaranteeHeader.AddTransaction(movementHeader.BM_PaperlessInbondNum,
			"NCTS write-off " + movementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
			"messagenum",
			ZString.Empty,
			guarantee.PW_BondAmount * -1,
			0,
			status: Customs.Business.PermitTransactionStatusList.Codes.Pending);

		guarantee.PW_BondAmount = 150.0m;

		var guarantee2 = movementHeader.Guarantees.AddNew();
		guarantee2.PW_BondNumber = "GUA2";
		guarantee2.PW_BondAmount = 290.0m;

		guaranteeHeader2.AddTransaction(movementHeader.BM_PaperlessInbondNum,
			"NCTS write-off " + movementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
			"messagenum",
			ZString.Empty,
			guarantee2.PW_BondAmount * -1,
			0,
			status: Customs.Business.PermitTransactionStatusList.Codes.Pending);

		guarantee2.PW_BondNumber = "GUA1";
		guarantee2.PW_BondAmount = 300.0m;

		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Initial situation GUA1", 2, guaranteeHeader.GetTransactions().Count());
			AssertEquals("Initial situation GUA2", 2, guaranteeHeader2.GetTransactions().Count());

			movementHeader.GuaranteeTransactionCoordinator.UpdatePendingTransactions("messagenum");

			AssertEquals("Extra transaction on GUA1", 4, guaranteeHeader.GetTransactions().Count());
			var newTransactionGUA1 = guaranteeHeader.GetTransactions().Last();
			AssertEquals("New transaction GUA1 should have status 'PND'", Customs.Business.PermitTransactionStatusList.Codes.Pending, newTransactionGUA1.CPL_TransactionStatus);
			AssertEquals("New transaction GUA1 should reference the LRN of the NCTS Movement Header", "LRN123456789", newTransactionGUA1.CPL_Reference);
			AssertEquals("New transaction GUA1 amount must be -150", -150.0m, newTransactionGUA1.CPL_TranValue);

			AssertEquals("Extra transaction on GUA2", 3, guaranteeHeader2.GetTransactions().Count());
			var newTransactionGUA2 = guaranteeHeader2.GetTransactions().Last();
			AssertEquals("Counterbalanced transaction GUA2 should have status 'PND'", Customs.Business.PermitTransactionStatusList.Codes.Pending, newTransactionGUA2.CPL_TransactionStatus);
			AssertEquals("Counterbalanced transaction GUA2 should reference the LRN of the NCTS Movement Header", "LRN123456789", newTransactionGUA2.CPL_Reference);
			AssertEquals("Counterbalanced transaction GUA2 amount must be 290", 290.0m, newTransactionGUA2.CPL_TranValue);
		});
	}

	public void TestConfirmTransactions()
	{
		var (guaranteeHeader, guaranteeHeader2) = SetupGuaranteeHeaders();

		var guarantee = movementHeader.Guarantees.AddNew();
		guarantee.PW_BondNumber = "GUA1";
		guarantee.PW_BondAmount = 150.0m;

		guaranteeHeader.AddTransaction(movementHeader.BM_PaperlessInbondNum,
			"NCTS write-off " + movementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
			movementHeader.BM_PaperlessInbondNum,
			ZString.Empty,
			guarantee.PW_BondAmount * -1,
			0,
			status: PermitTransactionStatusList.Codes.Pending);

		var guarantee2 = movementHeader.Guarantees.AddNew();
		guarantee2.PW_BondNumber = "GUA2";
		guarantee2.PW_BondAmount = 300.0m;

		guaranteeHeader2.AddTransaction(movementHeader.BM_PaperlessInbondNum,
			"NCTS write-off " + movementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
			movementHeader.BM_PaperlessInbondNum,
			ZString.Empty,
			guarantee2.PW_BondAmount * -1,
			0,
			status: PermitTransactionStatusList.Codes.Pending);

		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Initial situation GUA1", 2, guaranteeHeader.GetTransactions().Count());
			AssertEquals("Initial situation GUA2", 2, guaranteeHeader2.GetTransactions().Count());

			movementHeader.GuaranteeTransactionCoordinator.ConfirmTransactions();

			AssertEquals("Situation GUA1", 2, guaranteeHeader.GetTransactions().Count());

			var newTransactionGUA1 = guaranteeHeader.GetTransactions().Last();
			AssertEquals("New transaction GUA1 should have status 'CON'", PermitTransactionStatusList.Codes.Confirmed, newTransactionGUA1.CPL_TransactionStatus);
			AssertEquals("New transaction GUA1 should reference the LRN of the NCTS Movement Header", "LRN123456789", newTransactionGUA1.CPL_Reference);
			AssertEquals("New transaction GUA1 amount must be -150", -150.0m, newTransactionGUA1.CPL_TranValue);
			AssertEquals("Situation GUA2", 2, guaranteeHeader2.GetTransactions().Count());

			var newTransactionGUA2 = guaranteeHeader2.GetTransactions().Last();
			AssertEquals("New transaction GUA2 should have status 'CON'", PermitTransactionStatusList.Codes.Confirmed, newTransactionGUA2.CPL_TransactionStatus);
			AssertEquals("New transaction GUA2 should reference the LRN of the NCTS Movement Header", "LRN123456789", newTransactionGUA2.CPL_Reference);
			AssertEquals("New transaction GUA2 amount must be -300", -300.0m, newTransactionGUA2.CPL_TranValue);
		});
	}

	public void TestDeleteTransactions()
	{
		var (guaranteeHeader, guaranteeHeader2) = SetupGuaranteeHeaders();

		var guarantee = movementHeader.Guarantees.AddNew();
		guarantee.PW_BondNumber = "GUA1";
		guarantee.PW_BondAmount = 150.0m;

		guaranteeHeader.AddTransaction(movementHeader.BM_PaperlessInbondNum,
			"NCTS write-off " + movementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
			movementHeader.BM_PaperlessInbondNum,
			ZString.Empty,
			guarantee.PW_BondAmount * -1,
			0,
			status: PermitTransactionStatusList.Codes.Pending);
		var guarantee2 = movementHeader.Guarantees.AddNew();
		guarantee2.PW_BondNumber = "GUA2";
		guarantee2.PW_BondAmount = 300.0m;

		guaranteeHeader2.AddTransaction(movementHeader.BM_PaperlessInbondNum,
			"NCTS write-off " + movementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
			movementHeader.BM_PaperlessInbondNum,
			ZString.Empty,
			guarantee2.PW_BondAmount * -1,
			0,
			status: PermitTransactionStatusList.Codes.Pending);

		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Initial situation GUA1", 2, guaranteeHeader.GetTransactions().Count());
			AssertEquals("Initial situation GUA2", 2, guaranteeHeader2.GetTransactions().Count());

			movementHeader.GuaranteeTransactionCoordinator.DeleteTransactions();

			AssertEquals("Situation GUA1", 2, guaranteeHeader.GetTransactions().Count());

			var newTransactionGUA1 = guaranteeHeader.GetTransactions().Last();
			AssertEquals("New transaction GUA1 should have status 'DEL'", PermitTransactionStatusList.Codes.Deleted, newTransactionGUA1.CPL_TransactionStatus);
			AssertEquals("New transaction GUA1 should reference the LRN of the NCTS Movement Header", "LRN123456789", newTransactionGUA1.CPL_Reference);
			AssertEquals("New transaction GUA1 amount must be -150", -150.0m, newTransactionGUA1.CPL_TranValue);
			AssertEquals("Situation GUA2", 2, guaranteeHeader2.GetTransactions().Count());

			var newTransactionGUA2 = guaranteeHeader2.GetTransactions().Last();
			AssertEquals("New transaction GUA2 should have status 'DEL'", PermitTransactionStatusList.Codes.Deleted, newTransactionGUA2.CPL_TransactionStatus);
			AssertEquals("New transaction GUA2 should reference the LRN of the NCTS Movement Header", "LRN123456789", newTransactionGUA2.CPL_Reference);
			AssertEquals("New transaction GUA2 amount must be -300", -300.0m, newTransactionGUA2.CPL_TranValue);
		});
	}

	public void TestCounterBalanceConfirmedTransactions()
	{
		var (guaranteeHeader, guaranteeHeader2) = SetupGuaranteeHeaders();

		var guarantee = movementHeader.Guarantees.AddNew();
		guarantee.PW_BondNumber = "GUA1";
		guarantee.PW_BondAmount = 150.0m;

		guaranteeHeader.AddTransaction(movementHeader.BM_PaperlessInbondNum,
			"NCTS write-off " + movementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
			movementHeader.BM_PaperlessInbondNum,
			ZString.Empty,
			-100.0m,
			0,
			status: PermitTransactionStatusList.Codes.Confirmed);

		guaranteeHeader.AddTransaction(movementHeader.BM_PaperlessInbondNum,
			"NCTS write-off " + movementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
			movementHeader.BM_PaperlessInbondNum,
			ZString.Empty,
			-50.0m,
			0,
			status: PermitTransactionStatusList.Codes.Confirmed);

		var guarantee2 = movementHeader.Guarantees.AddNew();
		guarantee2.PW_BondNumber = "GUA2";
		guarantee2.PW_BondAmount = 300.0m;

		guaranteeHeader2.AddTransaction(movementHeader.BM_PaperlessInbondNum,
			"NCTS write-off " + movementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
			movementHeader.BM_PaperlessInbondNum,
			ZString.Empty,
			-200.0m,
			0,
			status: PermitTransactionStatusList.Codes.Confirmed);

		guaranteeHeader2.AddTransaction(movementHeader.BM_PaperlessInbondNum,
			"NCTS write-off " + movementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
			movementHeader.BM_PaperlessInbondNum,
			ZString.Empty,
			-100.0m,
			0,
			status: PermitTransactionStatusList.Codes.Confirmed);

		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Initial situation GUA1", 3, guaranteeHeader.GetTransactions().Count());
			AssertEquals("Initial situation GUA2", 3, guaranteeHeader2.GetTransactions().Count());

			movementHeader.GuaranteeTransactionCoordinator.CounterBalanceConfirmedTransactions("messagenum");

			AssertEquals("Situation GUA1", 4, guaranteeHeader.GetTransactions().Count());

			var newTransactionGUA1 = guaranteeHeader.GetTransactions().Last();
			AssertEquals("New transaction GUA1 should have status 'CON'", PermitTransactionStatusList.Codes.Confirmed, newTransactionGUA1.CPL_TransactionStatus);
			AssertEquals("New transaction GUA1 should reference the LRN of the NCTS Movement Header", "LRN123456789", newTransactionGUA1.CPL_Reference);
			AssertEquals("New transaction GUA1 amount must be 150", 150.0m, newTransactionGUA1.CPL_TranValue);

			AssertEquals("Situation GUA2", 4, guaranteeHeader2.GetTransactions().Count());

			var newTransactionGUA2 = guaranteeHeader2.GetTransactions().Last();
			AssertEquals("New transaction GUA2 should have status 'CON'", PermitTransactionStatusList.Codes.Confirmed, newTransactionGUA2.CPL_TransactionStatus);
			AssertEquals("New transaction GUA2 should reference the LRN of the NCTS Movement Header", "LRN123456789", newTransactionGUA2.CPL_Reference);
			AssertEquals("New transaction GUA2 amount must be 300", 300.0m, newTransactionGUA2.CPL_TranValue);
		});
	}

	public void TestCounterBalanceTransactionsOfGuaranteesNoLongerInDeclaration()
	{
		var (guaranteeHeader, guaranteeHeader2) = SetupGuaranteeHeaders();

		var guarantee1 = movementHeader.Guarantees.AddNew();
		guarantee1.PW_BondNumber = "GUA1";
		guarantee1.PW_BondAmount = 150.0m;

		guaranteeHeader.AddTransaction(movementHeader.BM_PaperlessInbondNum,
			"NCTS write-off " + movementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
			movementHeader.BM_PaperlessInbondNum,
			ZString.Empty,
			guarantee1.PW_BondAmount * -1,
			0,
			status: PermitTransactionStatusList.Codes.Pending);

		var guarantee2 = movementHeader.Guarantees.AddNew();
		guarantee2.PW_BondNumber = "GUA2";
		guarantee2.PW_BondAmount = 300.0m;

		guaranteeHeader2.AddTransaction(movementHeader.BM_PaperlessInbondNum,
			"NCTS write-off " + movementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
			movementHeader.BM_PaperlessInbondNum,
			ZString.Empty,
			guarantee2.PW_BondAmount * -1,
			0,
			status: PermitTransactionStatusList.Codes.Pending);

		movementHeader.Guarantees.RemoveAndDelete(guarantee1);
		movementHeader.Guarantees.RemoveAndDelete(guarantee2);

		Factory.Save();

		AssertEquals("Guarantee1 should be deleted from movementHeader", false, movementHeader.Guarantees.Contains(guarantee1));
		AssertEquals("Guarantee2 should be deleted from movementHeader", false, movementHeader.Guarantees.Contains(guarantee2));

		CombineAssertions(() =>
		{
			AssertEquals("Initial situation GUA1", 2, guaranteeHeader.GetTransactions().Count());
			AssertEquals("Initial situation GUA2", 2, guaranteeHeader2.GetTransactions().Count());

			movementHeader.GuaranteeTransactionCoordinator.CounterBalanceTransactionsOfGuaranteesNoLongerInDeclaration("messagenum");

			AssertEquals("Situation GUA1", 2, guaranteeHeader.GetTransactions().Count());

			var newTransactionGUA1 = guaranteeHeader.GetTransactions().Last();
			AssertEquals("New transaction GUA1 should have status 'DEL'", PermitTransactionStatusList.Codes.Deleted, newTransactionGUA1.CPL_TransactionStatus);
			AssertEquals("New transaction GUA1 should reference the LRN of the NCTS Movement Header", "LRN123456789", newTransactionGUA1.CPL_Reference);
			AssertEquals("New transaction GUA1 amount must be -150", -150.0m, newTransactionGUA1.CPL_TranValue);

			AssertEquals("Situation GUA2", 2, guaranteeHeader2.GetTransactions().Count());

			var newTransactionGUA2 = guaranteeHeader2.GetTransactions().Last();
			AssertEquals("New transaction GUA2 should have status 'DEL'", PermitTransactionStatusList.Codes.Deleted, newTransactionGUA2.CPL_TransactionStatus);
			AssertEquals("New transaction GUA2 should reference the LRN of the NCTS Movement Header", "LRN123456789", newTransactionGUA2.CPL_Reference);
			AssertEquals("New transaction GUA2 amount must be -300", -300.0m, newTransactionGUA2.CPL_TranValue);
		});
	}

	public void TestGetBookedAmount()
	{
		var (guaranteeHeader, _) = SetupGuaranteeHeaders();

		var guarantee = movementHeader.Guarantees.AddNew();
		guarantee.PW_BondNumber = "GUA1";
		guarantee.PW_BondAmount = 150.0m;

		Factory.Save();
		movementHeader.GuaranteeTransactionCoordinator.CreatePendingTransactions(movementHeader.BM_PaperlessInbondNum);

		var x = movementHeader.GuaranteeTransactionCoordinator.GetBookedAmountOnGuarantee(guaranteeHeader, movementHeader.BM_PaperlessInbondNum);
		AssertEquals(-150m, x);
	}

	public void TestConfirmValidAndDeleteInvalidTransactions()
	{
		var appId = "111";
		var (guaranteeHeader, guaranteeHeader2) = SetupGuaranteeHeaders();

		var guarantee = movementHeader.Guarantees.AddNew();
		guarantee.PW_BondNumber = "GUA1";
		guarantee.PW_BondAmount = 150.0m;

		guaranteeHeader.AddTransaction(movementHeader.BM_PaperlessInbondNum,
			"NCTS write-off " + movementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
			appId,
			ZString.Empty,
			guarantee.PW_BondAmount * -1,
			0,
			status: PermitTransactionStatusList.Codes.Pending);

		var guarantee2 = movementHeader.Guarantees.AddNew();
		guarantee2.PW_BondNumber = "GUA2";
		guarantee2.PW_BondAmount = 300.0m;

		guaranteeHeader2.AddTransaction(movementHeader.BM_PaperlessInbondNum,
			"NCTS write-off " + movementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
			appId,
			ZString.Empty,
			guarantee2.PW_BondAmount * -1,
			0,
			status: PermitTransactionStatusList.Codes.Pending);

		movementHeader.Guarantees.RemoveAndDelete(guarantee2);

		Factory.Save();

		AssertEquals("Guarantee2 should be deleted from movementHeader", false, movementHeader.Guarantees.Contains(guarantee2));

		CombineAssertions(() =>
		{
			AssertEquals("Initial situation GUA1", 2, guaranteeHeader.GetTransactions().Count());
			AssertEquals("Initial situation GUA2", 2, guaranteeHeader2.GetTransactions().Count());

			movementHeader.GuaranteeTransactionCoordinator.ConfirmValidAndDeleteInvalidTransactions();

			AssertEquals("Situation GUA1", 2, guaranteeHeader.GetTransactions().Count());

			var newTransactionGUA1 = guaranteeHeader.GetTransactions().Last();
			AssertEquals("New transaction GUA1 should have status 'CON'", PermitTransactionStatusList.Codes.Confirmed, newTransactionGUA1.CPL_TransactionStatus);
			AssertEquals("New transaction GUA1 should reference the LRN of the NCTS Movement Header", "LRN123456789", newTransactionGUA1.CPL_Reference);
			AssertEquals("New transaction GUA1 amount must be -150", -150.0m, newTransactionGUA1.CPL_TranValue);

			AssertEquals("Situation GUA2", 2, guaranteeHeader2.GetTransactions().Count());

			var newTransactionGUA2 = guaranteeHeader2.GetTransactions().Last();
			AssertEquals("New transaction GUA2 should have status 'DEL'", PermitTransactionStatusList.Codes.Deleted, newTransactionGUA2.CPL_TransactionStatus);
			AssertEquals("New transaction GUA2 should reference the LRN of the NCTS Movement Header", "LRN123456789", newTransactionGUA2.CPL_Reference);
			AssertEquals("New transaction GUA2 amount must be -300", -300.0m, newTransactionGUA2.CPL_TranValue);
		});
	}

	public void TestUpdateGuaranteeAmount()
	{
		var (guaranteeHeader, _) = SetupGuaranteeHeaders();

		var guarantee = movementHeader.Guarantees.AddNew();
		guarantee.PW_BondNumber = "GUA1";
		guarantee.PW_BondAmount = 150.0m;

		guaranteeHeader.AddTransaction(movementHeader.BM_PaperlessInbondNum, "NCTS departure", "111", ZString.Empty, guarantee.PW_BondAmount * -1, 0, status: PermitTransactionStatusList.Codes.Confirmed);
		guaranteeHeader.AddTransaction(movementHeader.BM_PaperlessInbondNum, "NCTS departure", "111", ZString.Empty, guarantee.PW_BondAmount * -1, 0, status: PermitTransactionStatusList.Codes.Deleted);
		Factory.Save();

		AssertEquals("Initial transaction count for guaranteeHeader", 3, guaranteeHeader.GetTransactions().Count());

		movementHeader.GuaranteeTransactionCoordinator.UpdateGuaranteeAmount(guarantee, "111", new Money(100, new ZArchitecture.Environment.Currency("EUR")));
		AssertEquals("Transaction count for guaranteeHeader after adding an adjustment transaction", 4, guaranteeHeader.GetTransactions().Count());

		var newTransaction = guaranteeHeader.GetTransactions().Last();
		AssertEquals("New transaction should have confirmed status", PermitTransactionStatusList.Codes.Confirmed, newTransaction.CPL_TransactionStatus);
		AssertEquals("New transaction amount should be 50 as per adjustment", 50m, newTransaction.CPL_TranValue);
		AssertEquals("PW_Override of the guarantee should be set to true", true, guarantee.PW_Override);
		AssertEquals("PW_BondAmount of the guarantee should be updated to 100", 100m, guarantee.PW_BondAmount);

		movementHeader.GuaranteeTransactionCoordinator.UpdateGuaranteeAmount(guarantee, "111", new Money(100, new ZArchitecture.Environment.Currency("EUR")));
		AssertEquals("No adjustment transaction should be created when the difference between PW_BondAmount and incomingAmount is 0.", 4, guaranteeHeader.GetTransactions().Count());
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		movementHeader = nctsHeader.MovementHeader;
	}

	(CusGuaranteeHeader, CusGuaranteeHeader) SetupGuaranteeHeaders()
	{
		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

		var org1 = Factory.NewWithValidTestData<OrgHeader>();
		var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
		guaranteeHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		guaranteeHeader.CPH_Number = "GUA1";
		guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
		guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
		guaranteeHeader.CPH_SystemCreateTimeUtc = ZDate.Today;
		guaranteeHeader.CPH_Type = "TRA";
		guaranteeHeader.CPH_RN_NKCountryCode = "DE";
		guaranteeHeader.CPH_Balance = 1000.0m;
		guaranteeHeader.CPH_OH_PermitHolder = org1.PK;

		var guaranteeHeader2 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
		guaranteeHeader2.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		guaranteeHeader2.CPH_Number = "GUA2";
		guaranteeHeader2.CPH_StartDate = ZDate.Today.AddMonths(-1);
		guaranteeHeader2.CPH_EndDate = ZDate.Today.AddMonths(1);
		guaranteeHeader2.CPH_SystemCreateTimeUtc = ZDate.Today;
		guaranteeHeader2.CPH_Type = "TRA";
		guaranteeHeader2.CPH_RN_NKCountryCode = "DE";
		guaranteeHeader2.CPH_Balance = 2000.0m;
		guaranteeHeader2.CPH_OH_PermitHolder = org1.PK;

		nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;
		movementHeader.BM_PaperlessInbondNum = "LRN123456789";

		guaranteeHeader.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: PermitTransactionTypeList.Codes.OBL, isAggregated: true);
		guaranteeHeader2.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 2000.0m, 0, transactionType: PermitTransactionTypeList.Codes.OBL, isAggregated: true);

		return (guaranteeHeader, guaranteeHeader2);
	}

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader movementHeader;
}
