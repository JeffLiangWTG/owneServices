using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using CusTempStorageRegHeader = Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader;
using CusTempStorageRegLine = Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine;
using CusTempStorageRegLineTransaction = Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLineTransaction;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing;

[TestedType(typeof(TempStorageRegisterForm))]
class TempStorageRegisterFormTest : ZFormBasherTest
{
	public void TestFormCaption()
	{
		SetUpRegHeader();
		using var form = new TempStorageRegisterForm(regHeader);
		form.Show();
		CombineAssertions(() =>
		{
			AssertEquals("Reference Entered", "Temporary Storage Register - UNITTEST", form.FormCaption);
			regHeader.SRH_Reference = ZString.Empty;
			AssertEquals("No Reference", "Temporary Storage Register", form.FormCaption);
		});
	}

	public void TestCheckOnShowPreSaveDialogs_TransactionWarning_WithoutWriteOff_NoPendingAmount()
	{
		var expectedWarningMessage = "Transactions cannot be amended once saved. Do you want to continue saving the transactions?";

		SetUpRegHeader();

		var guarantee = SetUpGuaranteeForTempStorage(regHeader, 0m);

		_ = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);
		Factory.Save();

		var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -3, grossWeight: -3);
		regLineTransaction1.SRT_Reference = MRNCode;
		regLineTransaction1.SRT_ReferenceType = "MRN";

		using var form = new TempStorageRegisterForm(regHeader);
		form.Show();

		CombineAssertions(() =>
		{
			AssertEquals("Prereq: Line's status is empty", ZString.Empty, regLine.SRL_CustomsStatus);
			AssertEquals("Prereq: Header's status is empty", ZString.Empty, regHeader.SRH_Status);
			AssertEquals("Prereq: No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
			AssertEquals("Prereq: Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			form.FireSaveButton();
			AssertEquals("There is warning showing, answer is No", expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("regLineTransaction1 is not saved when aswer is No", expected: false, regLineTransaction1.IsInDatabase);
			AssertEquals("regLineTransaction1's BondAmount is not changed when aswer is No", 0m, regLineTransaction1.SRT_BondAmount);
			AssertEquals("Line's status is empty (not changed)", ZString.Empty, regLine.SRL_CustomsStatus);
			AssertEquals("Header's status is empty (not changed)", ZString.Empty, regHeader.SRH_Status);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			form.FireSaveButton();
			AssertEquals("There is warning showing, answer is Yes", expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("regLineTransaction1 is  saved when aswer is Yes", expected: true, regLineTransaction1.IsInDatabase);
			AssertEquals("SRT_TransactionStatus is changed to CON when aswer is Yes", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction1.SRT_TransactionStatus);
			AssertEquals("regLineTransaction1's BondAmount is not changed becasue calculation is 0 when aswer is Yes", -3.0m, regLineTransaction1.SRT_BondAmount);
			AssertEquals("Line's status is changed to CLS when remaining gross weight is 0", "CLS", regLine.SRL_CustomsStatus);
			AssertEquals("Header's status is changed to CLS when all regLines in regHeader are CLS", "CLS", regHeader.SRH_Status);

			AssertEquals("No Write off transactions in Guarantee are created", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
		});
	}

	public void TestCheckOnShowPreSaveDialogs_TransactionWarning_WithoutWriteOff_BondAmountZero()
	{
		var expectedWarningMessage = "Transactions cannot be amended once saved. Do you want to continue saving the transactions?";

		SetUpRegHeader();
		var guarantee = SetUpGuaranteeForTempStorage(regHeader, -8.0m);

		_ = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);
		Factory.Save();

		var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: -2, grossWeight: -2, bondAmount: -3m);
		Factory.Save();

		var regLineTransaction2 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -1, grossWeight: -1);
		regLineTransaction2.SRT_Reference = MRNCode;
		regLineTransaction2.SRT_ReferenceType = "MRN";

		using var form = new TempStorageRegisterForm(regHeader);
		form.Show();

		CombineAssertions(() =>
		{
			AssertEquals("Prereq: Line's status is empty", ZString.Empty, regLine.SRL_CustomsStatus);
			AssertEquals("Prereq: Header's status is empty", ZString.Empty, regHeader.SRH_Status);
			AssertEquals("Prereq: No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
			AssertEquals("Prereq: Bond Amount transaction1 is default", 0.0m, regLineTransaction2.SRT_BondAmount);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			_ = form.FireSaveButton();
			AssertEquals("There is warning showing, answer is No", expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("regLineTransaction2 is not saved when aswer is No", expected: false, regLineTransaction2.IsInDatabase);
			AssertEquals("regLineTransaction2's BondAmount is not changed when aswer is No", 0m, regLineTransaction2.SRT_BondAmount);
			AssertEquals("Line's status is empty (not changed)", ZString.Empty, regLine.SRL_CustomsStatus);
			AssertEquals("Header's status is empty (not changed)", ZString.Empty, regHeader.SRH_Status);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			_ = form.FireSaveButton();
			AssertEquals("There is warning showing, answer is Yes", expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("regLineTransaction2 is  saved when aswer is Yes", expected: true, regLineTransaction2.IsInDatabase);
			AssertEquals("SRT_TransactionStatus is changed to CON when aswer is Yes", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction2.SRT_TransactionStatus);
			AssertEquals("regLineTransaction2's BondAmount is not changed becasue calculation is 0 when aswer is Yes", 0m, regLineTransaction2.SRT_BondAmount);
			AssertEquals("Line's status is changed to CLS when remaining gross weight is 0", "CLS", regLine.SRL_CustomsStatus);
			AssertEquals("Header's status is changed to CLS when all regLines in regHeader are CLS", "CLS", regHeader.SRH_Status);

			AssertEquals("No Write off transactions in Guarantee are created ", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
		});
	}

	public void TestCheckOnShowPreSaveDialogs_TransactionWarning_WithWriteOff_PendingAmountLessThanBondAmount()
	{
		var expectedWarningMessage = "Transactions cannot be amended once saved. Do you want to continue saving the transactions?";

		SetUpRegHeader();
		var guarantee = SetUpGuaranteeForTempStorage(regHeader, -2.0m);
		var regLine2 = regHeader.CusTempStorageRegLines.AddNew();
		regLine2.SRL_LineNumber = 2;
		regLine2.SRL_PackageType = "BX";
		regLine2.SRL_CustomsStatus = "CLS";

		_ = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);
		Factory.Save();

		var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -3, grossWeight: -3);
		regLineTransaction1.SRT_Reference = MRNCode;
		regLineTransaction1.SRT_ReferenceType = "NUM";

		using var form = new TempStorageRegisterForm(regHeader);
		form.Show();

		CombineAssertions(() =>
		{
			AssertEquals("Prereq: Line's status is empty", ZString.Empty, regLine.SRL_CustomsStatus);
			AssertEquals("Prereq: Header's status is empty", ZString.Empty, regHeader.SRH_Status);
			AssertEquals("Prereq: No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
			AssertEquals("Prereq: Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			_ = form.FireSaveButton();
			AssertEquals("There is warning showing, answer is No", expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("regLineTransaction1 is not saved when aswer is No", expected: false, regLineTransaction1.IsInDatabase);
			AssertEquals("regLineTransaction1's BondAmount is not changed when aswer is No", 0m, regLineTransaction1.SRT_BondAmount);
			AssertEquals("Line's status is empty (not changed)", ZString.Empty, regLine.SRL_CustomsStatus);
			AssertEquals("Header's status is empty (not changed)", ZString.Empty, regHeader.SRH_Status);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			_ = form.FireSaveButton();
			AssertEquals("There is warning showing, answer is Yes", expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("regLineTransaction1 is  saved when aswer is Yes", expected: true, regLineTransaction1.IsInDatabase);
			AssertEquals("SRT_TransactionStatus is changed to CON when aswer is Yes", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction1.SRT_TransactionStatus);
			AssertEquals("regLineTransaction1's BondAmount is changed when aswer is Yes", -3.0m, regLineTransaction1.SRT_BondAmount);
			AssertEquals("Line's status is changed to CLS when remaining gross weight is 0", "CLS", regLine.SRL_CustomsStatus);
			AssertEquals("Header's status is changed to CLS when all regLines in regHeader are CLS", "CLS", regHeader.SRH_Status);

			var expectedComment = string.Format("Write-off TS {0} / NUM {1}", RegHeaderReference, MRNCode);
			var writeOffTransaction = guarantee.CusGuaranteeLineTransactions.Cast<BaseCusGuaranteeLineTransaction>().FirstOrDefault(x => x.CPL_Comment.StartsWith("Write-off"));
			AssertNotNull("New Write off transaction is created in Guarantee", writeOffTransaction);
			AssertWriteOffTransaction(writeOffTransaction, "Write-off transaction", RegHeaderReference, 2.0m, releaseDate, expectedComment);
		});
	}

	public void TestCheckOnShowPreSaveDialogs_TransactionWarning_WithWriteOff_PendingAmountSameAsBondAmount()
	{
		var expectedWarningMessage = "Transactions cannot be amended once saved. Do you want to continue saving the transactions?";

		SetUpRegHeader();
		var guarantee = SetUpGuaranteeForTempStorage(regHeader, -0.45m);
		regLine.SRL_CustomsStatus = "CLS";
		regHeader.SRH_Status = "CLS";

		_ = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 20, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);
		Factory.Save();

		var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -2, grossWeight: -3);
		regLineTransaction1.SRT_Reference = MRNCode;
		regLineTransaction1.SRT_ReferenceType = "MRN";

		using var form = new TempStorageRegisterForm(regHeader);
		form.Show();

		CombineAssertions(() =>
		{
			AssertEquals("Prereq: Line's status is CLS", "CLS", regLine.SRL_CustomsStatus);
			AssertEquals("Prereq: Header's status is CLS", "CLS", regHeader.SRH_Status);
			AssertEquals("Prereq: No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
			AssertEquals("Prereq: Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			_ = form.FireSaveButton();
			AssertEquals("There is warning showing, answer is No", expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("regLineTransaction1 is not saved when aswer is No", expected: false, regLineTransaction1.IsInDatabase);
			AssertEquals("regLineTransaction1's BondAmount is not changed when aswer is No", 0m, regLineTransaction1.SRT_BondAmount);
			AssertEquals("Line's status is CLS (not changed)", "CLS", regLine.SRL_CustomsStatus);
			AssertEquals("Header's status is CLS (not changed)", "CLS", regHeader.SRH_Status);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			_ = form.FireSaveButton();
			AssertEquals("There is warning showing, answer is Yes", expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("regLineTransaction1 is  saved when aswer is Yes", expected: true, regLineTransaction1.IsInDatabase);
			AssertEquals("SRT_TransactionStatus is changed to CON when aswer is Yes", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction1.SRT_TransactionStatus);
			AssertEquals("regLineTransaction1's BondAmount is changed when aswer is Yes", -0.45m, regLineTransaction1.SRT_BondAmount);
			AssertEquals("Line's status is changed to OPN when remaining gross weight is not 0", "OPN", regLine.SRL_CustomsStatus);
			AssertEquals("Header's status is changed to OPN when not all regLines in regHeader are CLS", "OPN", regHeader.SRH_Status);

			var expectedComment = string.Format("Write-off TS {0} / MRN {1}", RegHeaderReference, MRNCode);
			var writeOffTransaction = guarantee.CusGuaranteeLineTransactions.Cast<BaseCusGuaranteeLineTransaction>().FirstOrDefault(x => x.CPL_Comment.StartsWith("Write-off"));
			AssertNotNull("New Write off transaction is created in Guarantee", writeOffTransaction);
			AssertWriteOffTransaction(writeOffTransaction, "Write-off transaction", RegHeaderReference, 0.45m, releaseDate, expectedComment);
		});
	}

	public void TestCheckOnShowPreSaveDialogs_TransactionWarning_WithWriteOff_PendingAmountMoreThanAsBondAmount()
	{
		var expectedWarningMessage = "Transactions cannot be amended once saved. Do you want to continue saving the transactions?";

		SetUpRegHeader();
		var guarantee = SetUpGuaranteeForTempStorage(regHeader, -8.0m);
		regLine.SRL_CustomsStatus = "OPN";
		var regLine2 = regHeader.CusTempStorageRegLines.AddNew();
		regLine2.SRL_LineNumber = 2;
		regLine2.SRL_PackageType = "BX";
		regLine2.SRL_CustomsStatus = "OPN";
		regHeader.SRH_Status = "CLS";

		_ = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);
		Factory.Save();

		var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -3, grossWeight: -3);
		regLineTransaction1.SRT_Reference = MRNCode;
		regLineTransaction1.SRT_ReferenceType = "NUM";

		using var form = new TempStorageRegisterForm(regHeader);
		form.Show();

		CombineAssertions(() =>
		{
			AssertEquals("Prereq: Line's status is OPN", "OPN", regLine.SRL_CustomsStatus);
			AssertEquals("Prereq: Header's status is CLS", "CLS", regHeader.SRH_Status);
			AssertEquals("Prereq: No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
			AssertEquals("Prereq: Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			_ = form.FireSaveButton();
			AssertEquals("There is warning showing, answer is No", expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("regLineTransaction1 is not saved when aswer is No", expected: false, regLineTransaction1.IsInDatabase);
			AssertEquals("regLineTransaction1's BondAmount is not changed when aswer is No", 0m, regLineTransaction1.SRT_BondAmount);
			AssertEquals("Line's status is OPN (not changed)", "OPN", regLine.SRL_CustomsStatus);
			AssertEquals("Header's status is CLS (not changed)", "CLS", regHeader.SRH_Status);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			_ = form.FireSaveButton();
			AssertEquals("There is warning showing, answer is Yes", expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("regLineTransaction1 is  saved when aswer is Yes", expected: true, regLineTransaction1.IsInDatabase);
			AssertEquals("SRT_TransactionStatus is changed to CON when aswer is Yes", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction1.SRT_TransactionStatus);
			AssertEquals("regLineTransaction1's BondAmount is changed when aswer is Yes", -3.0m, regLineTransaction1.SRT_BondAmount);
			AssertEquals("Line's status is changed to CLS when remaining gross weight is 0", "CLS", regLine.SRL_CustomsStatus);
			AssertEquals("Header's status is changed to OPN when not all regLines in regHeader are CLS", "OPN", regHeader.SRH_Status);

			var expectedComment = string.Format("Write-off TS {0} / NUM {1}", RegHeaderReference, MRNCode);
			var writeOffTransaction = guarantee.CusGuaranteeLineTransactions.Cast<BaseCusGuaranteeLineTransaction>().FirstOrDefault(x => x.CPL_Comment.StartsWith("Write-off"));
			AssertNotNull("New Write off transaction is created in Guarantee", writeOffTransaction);
			AssertWriteOffTransaction(writeOffTransaction, "Write-off transaction", RegHeaderReference, 3.0m, releaseDate, expectedComment);
		});
	}

	public void TestCheckOnShowPreSaveDialogs_TransactionWarning_WithWriteOffError()
	{
		var expectedWarningMessage = "Transactions cannot be amended once saved. Do you want to continue saving the transactions?";
		var expectedWriteOffError = "Reference " + RegHeaderReference + " has a positive balance of 3.46 EUR. Please check the existing transactions for this reference and create a manual adjustment if needed.";

		SetUpRegHeader();
		var guarantee = SetUpGuaranteeForTempStorage(regHeader, 3.45698m);

		_ = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);
		Factory.Save();

		var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -3, grossWeight: -3);

		using var form = new TempStorageRegisterForm(regHeader);
		form.Show();

		CombineAssertions(() =>
		{
			AssertEquals("Prereq: Line's status is empty", ZString.Empty, regLine.SRL_CustomsStatus);
			AssertEquals("Prereq: Header's status is empty", ZString.Empty, regHeader.SRH_Status);
			AssertEquals("Prereq: No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
			AssertEquals("Prereq: Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			_ = form.FireSaveButton();
			AssertEquals("There is warning showing, answer is No", expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("regLineTransaction1 is not saved when aswer is No", expected: false, regLineTransaction1.IsInDatabase);
			AssertEquals("regLineTransaction1's BondAmount is not changed when aswer is No", 0m, regLineTransaction1.SRT_BondAmount);
			AssertEquals("Line's status is empty (not changed)", ZString.Empty, regLine.SRL_CustomsStatus);
			AssertEquals("Header's status is empty (not changed)", ZString.Empty, regHeader.SRH_Status);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			_ = form.FireSaveButton();
			AssertEquals("There is warning showing, answer is Yes", expected: true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedWarningMessage));
			AssertEquals("There is an error saying write off cannot be done", expectedWriteOffError, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("regLineTransaction1 is  saved when aswer is Yes", expected: true, regLineTransaction1.IsInDatabase);
			AssertEquals("SRT_TransactionStatus is changed to CON when aswer is Yes", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction1.SRT_TransactionStatus);
			AssertEquals("regLineTransaction1's BondAmount is changed when aswer is Yes", -3.0m, regLineTransaction1.SRT_BondAmount);
			AssertEquals("Line's status is changed to CLS when remaining gross weight is not 0", "CLS", regLine.SRL_CustomsStatus);
			AssertEquals("Header's status is changed to CLS when all regLines in regHeader are CLS", "CLS", regHeader.SRH_Status);

			AssertEquals("No Write off transactions in Guarantee are created ", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
		});
	}

	public void TestResetNewADJTransactionWhenSavingError()
	{
		var expectedWarningMessage = "Transactions cannot be amended once saved. Do you want to continue saving the transactions?";

		SetUpRegHeader();
		var guarantee = SetUpGuaranteeForTempStorage(regHeader, 3.45698m);

		_ = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);
		Factory.Save();

		var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: -3, grossWeight: -3);

		regHeader.SRH_Reference = "";

		using var form = new TempStorageRegisterForm(regHeader);
		form.Show();

		CombineAssertions(() =>
		{
			AssertEquals("Prereq: Line's status is empty", ZString.Empty, regLine.SRL_CustomsStatus);
			AssertEquals("Prereq: Header's status is empty", ZString.Empty, regHeader.SRH_Status);
			AssertEquals("Prereq: No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
			AssertEquals("Prereq: Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			_ = form.FireSaveButton();
			AssertEquals("There is warning showing, answer is No", expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("regLineTransaction1 is not saved when aswer is No", expected: false, regLineTransaction1.IsInDatabase);
			AssertEquals("regLineTransaction1's BondAmount is not changed when aswer is No", 0m, regLineTransaction1.SRT_BondAmount);
			AssertEquals("Line's status is empty (not changed)", ZString.Empty, regLine.SRL_CustomsStatus);
			AssertEquals("Header's status is empty (not changed)", ZString.Empty, regHeader.SRH_Status);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			_ = form.FireSaveButton();
			AssertEquals("There is warning showing, answer is Yes", expected: true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedWarningMessage));
			AssertEquals("regLineTransaction1 is not saved when aswer is Yes because of an error when saving", expected: false, regLineTransaction1.IsInDatabase);
			AssertEquals("regLineTransaction1's BondAmount is left as 0 when aswer is Yes because of an error when saving", 0m, regLineTransaction1.SRT_BondAmount);

			AssertEquals("No Write off transactions in Guarantee are created because of an error when saving", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
		});
	}

	void AssertWriteOffTransaction(BaseCusGuaranteeLineTransaction transaction, ZString transactionName, ZString expectedReference, ZDecimal expectedTranValue, ZDateTime expectedTransactionDate, ZString expectedComment)
	{
		AssertEquals(transactionName + "'s CPL_TransactionType is TRA", "TRA", transaction.CPL_TransactionType);
		AssertEquals(transactionName + "'s CPL_Reference", expectedReference, transaction.CPL_Reference);
		AssertEquals(transactionName + "'s CPL_TransactionDate", expectedTransactionDate, transaction.CPL_TransactionDate);
		AssertEquals(transactionName + "'s CPL_Comment", expectedComment, transaction.CPL_Comment);
		AssertEquals(transactionName + "'s CPL_TranValue", expectedTranValue, transaction.CPL_TranValue);
		AssertEquals(transactionName + "'s CPL_Transaction Status is CON", "CON", transaction.CPL_TransactionStatus);
	}

	protected override Form GetFormToBashCore()
	{
		SetUpRegHeader();
		var premises = SetupPremises();
		regHeader.SRH_SRP_Premises = premises.PK;

		Factory.Save();

		var result = new TempStorageRegisterForm(regHeader);
		result.ControllerID = ControllerIDs.Customs.ES.TemporaryStorageRegister;
		return result;
	}

	EU.TemporaryStorage.Business.CusTempStorageRegPremises SetupPremises()
	{
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "A";
		premises.SRP_Description = "Desc";
		var organisation = Factory.New<OrgHeader>();
		organisation.OH_Code = "AAA";
		premises.SRP_OA_PremisesAddress = organisation.MainAddress.PK;

		return premises;
	}

	void SetUpRegHeader()
	{
		regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_Reference = RegHeaderReference;
		regLine = regHeader.CusTempStorageRegLines.AddNew();
		regLine.SRL_LineNumber = 1;
	}

	CusTempStorageRegHeader regHeader;
	CusTempStorageRegLine regLine;

	const string RegHeaderReference = "UNITTEST";
	const string MRNCode = "20ES00999930006184";
	readonly ZDateTime releaseDate = new(2024, 06, 14, 11, 11, 11);

	CusGuaranteeHeader SetUpGuaranteeForTempStorage(CusTempStorageRegHeader regHeader, ZDecimal value)
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AAA";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";

		var cusGuarantee = Factory.New<CusGuaranteeHeader>();
		cusGuarantee.CPH_Number = "Test1";
		cusGuarantee.CPH_OH_PermitHolder = orgHeader.PK;
		cusGuarantee.CPH_Type = EUGuaranteeTypeList.Codes.TST;
		cusGuarantee.CPH_SubType = "1";
		cusGuarantee.CPH_StartDate = ZDate.BrettsBirthday;
		cusGuarantee.CPH_Balance = 1000.0m;

		var commonGuarantee = Factory.New<CommonGuarantee>();
		commonGuarantee.Parent = regHeader;
		commonGuarantee.PW_BondNumber = "Test1";
		commonGuarantee.PW_CPH_Guarantee = cusGuarantee.PK;

		var guarantee = regHeader.Guarantee.CusGuarantee;
		var guaranteeLineTransaction = guarantee.CusGuaranteeLineTransactions.AddNew();
		guaranteeLineTransaction.CPL_Reference = RegHeaderReference;
		guaranteeLineTransaction.CPL_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		guaranteeLineTransaction.CPL_TranValue = value;

		return guarantee;
	}

	CusTempStorageRegLineTransaction SetUpTransaction(CusTempStorageRegLine regLine, ZString transactionStatus, int packageQty = 0, decimal grossWeight = 0, string transactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction, decimal bondAmount = 0.0m)
	{
		var regLineTransaction = regLine.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction.SRT_TransactionType = transactionType;
		regLineTransaction.SRT_TransactionStatus = transactionStatus;
		regLineTransaction.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
		regLineTransaction.SRT_InternalReferenceNumber = "Number";
		regLineTransaction.SRT_ReferenceType = CusTempStorageRegLineTransactionReferenceTypeList.Codes.MovementReferenceNumber;
		regLineTransaction.SRT_Reference = "MRNCode";
		regLineTransaction.TransactionDate = ZDateTime.Now;
		regLineTransaction.PhysicalInOutDate = releaseDate;
		regLineTransaction.SRT_PackageQty = packageQty;
		regLineTransaction.SRT_GrossWeight = grossWeight;
		regLineTransaction.SRT_BondAmount = bondAmount;

		return regLineTransaction;
	}
}
