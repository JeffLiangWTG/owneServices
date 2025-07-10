using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class WriteOffTransactionCreatorTest : TestCaseWithFactory
	{
		[TestDate(2021, 10, 05, 09, 36, 0)]
		public void TestAddGuaranteesWriteOffTransactionsEntryHeader()
		{
			var wrongGuaranteeReference = "16ESAGP9990000096";
			var wrongSecondGuaranteeReference = "16ESAGP9990000097";
			var extraGuaranteeReference = "16ESAGL9990000098";

			var (entryHeader, declaration, entryInstruction) = SetUpCusEntryHeader();

			SetUpGuarantees(EUGuaranteeTypeList.Codes.IMP);
			AddGuaranteesToDeclaration(declaration, entryInstruction.PK);

			SetUpGuarantees(EUGuaranteeTypeList.Codes.IMP, guaranteeReference: wrongGuaranteeReference, secondGuaranteeReference: wrongSecondGuaranteeReference);
			AddGuaranteesToDeclaration(declaration, entryInstruction.PK, guaranteeReference: wrongGuaranteeReference, secondGuaranteeReference: wrongSecondGuaranteeReference);
			SetUpGuarantee(extraGuaranteeReference, EUGuaranteeTypeList.Codes.TRA, 0m, 2000m, 1300m);
			GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction.PK, extraGuaranteeReference));
			declaration.Guarantees[6].PW_BondAmount = 1200m;
			AssertEquals(7, declaration.Guarantees.Count);

			var result = AddGuaranteesWriteOffTransactionsEntryHeader(entryHeader);
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();
			CombineAssertions(() =>
			{
				AssertEquals("Result should be Written Off since both guarantees have new transactions", "Written Off", result);

				var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for first guarantee", 10, guarantee1Transactions.Count());
				AssertNewTransaction("First guarantee's transaction", guarantee1Transactions.First(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)), 220m, new ZDateTime(2021, 10, 05, 09, 36, 0), false);

				var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == SecondGuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for second guarantee", 10, guarantee2Transactions.Count());
				AssertNewTransaction("Second guarantee's transaction", guarantee2Transactions.First(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)), 1200m, new ZDateTime(2021, 10, 05, 09, 36, 0), false);

				var guarantee3Transactions = guaranteeHeaderList.First(x => x.CPH_Number == wrongGuaranteeReference).GetTransactions();
				AssertEquals("Original transactions, no transactions added for third guarantee (balance)", 9, guarantee3Transactions.Count());
				AssertEquals("No new TRA Write off transaction for third guarantee", false, guarantee3Transactions.Any(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)));

				var guarantee4Transactions = guaranteeHeaderList.First(x => x.CPH_Number == wrongSecondGuaranteeReference).GetTransactions();
				AssertEquals("Original transactions, no transactions added for fourth guarantee (balance)", 9, guarantee4Transactions.Count());
				AssertEquals("No new TRA Write off transaction for fourth guarantee", false, guarantee4Transactions.Any(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)));

				var guarantee5Transactions = guaranteeHeaderList.First(x => x.CPH_Number == extraGuaranteeReference).GetTransactions();
				AssertEquals("Original transactions, no transactions added for fifth guarantee", 9, guarantee5Transactions.Count());
				AssertEquals("No new TRA Write off transaction for fifth guarantee", false, guarantee5Transactions.Any(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)));
			});
		}

		[TestDate(2021, 10, 05, 09, 36, 0)]
		public void TestAddGuaranteesWriteOffTransactionsEntryHeader_PositiveAmount()
		{
			var (entryHeader, declaration, entryInstruction) = SetUpCusEntryHeader();
			SetUpGuarantee(GuaranteeReference, EUGuaranteeTypeList.Codes.IMP, 500.123456789m, 2000m, 1300);

			GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction.PK, GuaranteeReference));
			declaration.Guarantees[0].PW_BondAmount = 1000m;
			AssertEquals(1, declaration.Guarantees.Count);

			var result = AddGuaranteesWriteOffTransactionsEntryHeader(entryHeader);
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			CombineAssertions(() =>
			{
				AssertEquals("Result should be Not Written Off since the guarantee has a positive balance", "Not Written Off (at least one positive balance)", result);

				var expectedError = "Reference 16ESAGL9990000096 has a positive balance of 880.25 EUR. Please check the existing transactions for this reference and create a manual adjustment if needed.";
				var concatenatedUserLogStrings = string.Concat(logger.UserLogStrings.Cast<string>());
				AssertContains("logger", expectedError, concatenatedUserLogStrings);
			});
		}

		[TestDate(2021, 10, 05, 09, 36, 0)]
		public void TestAddGuaranteesWriteOffTransactionsEntryHeader_PositiveAmount_WithoutLogger()
		{
			var (entryHeader, declaration, entryInstruction) = SetUpCusEntryHeader();
			SetUpGuarantee(GuaranteeReference, EUGuaranteeTypeList.Codes.IMP, 500m, 2000m, 1300);
			GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction.PK, GuaranteeReference));
			declaration.Guarantees[0].PW_BondAmount = 1000m;
			AssertEquals(1, declaration.Guarantees.Count);

			var writeOffTransactionCreator = new WriteOffTransactionCreator();
			var result = writeOffTransactionCreator.AddGuaranteesWriteOffTransactionsEntryHeader(entryHeader);
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			CombineAssertions(() =>
			{
				AssertEquals("Result should be Not Written Off since the guarantee has a positive balance", "Not Written Off (at least one positive balance)", result);

				var concatenatedUserLogStrings = string.Concat(logger.UserLogStrings.Cast<string>());
				AssertContains("logger", ZString.Empty, concatenatedUserLogStrings);
			});
		}

		[TestDate(2021, 10, 05, 09, 36, 0)]
		public void TestAddGuaranteesWriteOffTransactionsEntryHeader_NoPendingAmount()
		{
			var (entryHeader, declaration, entryInstruction) = SetUpCusEntryHeader();
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = GuaranteeReference;
			guaranteeHeader.CPH_StartDate = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_EndDate = new ZDate(2022, 7, 15);
			guaranteeHeader.CPH_SystemCreateTimeUtc = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.IMP;
			guaranteeHeader.CPH_RN_NKCountryCode = EsCode;
			guaranteeHeader.CPH_Balance = 0;

			AddOBLTransaction(guaranteeHeader, 1000m);
			AddTransaction(guaranteeHeader, "First-CON", 0, PermitTransactionStatusList.Codes.Confirmed);
			GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction.PK, GuaranteeReference));
			declaration.Guarantees[0].PW_BondAmount = 0m;
			AssertEquals(1, declaration.Guarantees.Count);

			var result = AddGuaranteesWriteOffTransactionsEntryHeader(entryHeader);
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			CombineAssertions(() =>
			{
				AssertEquals("Result should be Excluded since the guarantee has a balance of 0", "Excluded (no pending debt)", result);

				var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
				AssertEquals("3 Original transactions, no transactions added", 3, guarantee1Transactions.Count());
				AssertEquals("No new TRA Write off transaction for guarantee", false, guarantee1Transactions.Any(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)));
			});
		}

		[TestDate(2021, 10, 05, 09, 36, 0)]
		public void TestAddGuaranteesWriteOffTransactionsEntryHeader_TwoCorrectOnePositiveAmount()
		{
			var (entryHeader, declaration, entryInstruction) = SetUpCusEntryHeader();
			SetUpGuarantees(EUGuaranteeTypeList.Codes.IMP);
			AddGuaranteesToDeclaration(declaration, entryInstruction.PK);
			SetUpGuarantee(ThirdGuaranteeReference, EUGuaranteeTypeList.Codes.IMP, 500.123456789m, 2000m, 1300);
			GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction.PK, ThirdGuaranteeReference));
			declaration.Guarantees[3].PW_BondAmount = 1000m;
			AssertEquals(4, declaration.Guarantees.Count);

			var result = AddGuaranteesWriteOffTransactionsEntryHeader(entryHeader);
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			CombineAssertions(() =>
			{
				AssertEquals("Result should be Not Written Off since at least one guarantee has a positive balance", "Not Written Off (at least one positive balance)", result);

				var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for first guarantee", 10, guarantee1Transactions.Count());
				AssertNewTransaction("First guarantee's transaction", guarantee1Transactions.First(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)), 220m, new ZDateTime(2021, 10, 05, 09, 36, 0), false);

				var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == SecondGuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for second guarantee", 10, guarantee2Transactions.Count());
				AssertNewTransaction("Second guarantee's transaction", guarantee2Transactions.First(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)), 1200m, new ZDateTime(2021, 10, 05, 09, 36, 0), false);

				var expectedError = "Reference 18ESAGL9990000098 has a positive balance of 880.25 EUR. Please check the existing transactions for this reference and create a manual adjustment if needed.";
				var concatenatedUserLogStrings = string.Concat(logger.UserLogStrings.Cast<string>());
				AssertContains("logger", expectedError, concatenatedUserLogStrings);
			});
		}

		[TestDate(2021, 10, 05, 09, 36, 0)]
		public void TestAddGuaranteesWriteOffTransactionsEntryHeader_TwoCorrectOneNoPendingAmount()
		{
			var (entryHeader, declaration, entryInstruction) = SetUpCusEntryHeader();
			SetUpGuarantees(EUGuaranteeTypeList.Codes.IMP);
			AddGuaranteesToDeclaration(declaration, entryInstruction.PK);
			SetUpGuarantee(ThirdGuaranteeReference, EUGuaranteeTypeList.Codes.IMP, 60m, 1000m, 0);
			GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction.PK, ThirdGuaranteeReference));
			declaration.Guarantees[3].PW_BondAmount = 1000m;
			AssertEquals(4, declaration.Guarantees.Count);

			var result = AddGuaranteesWriteOffTransactionsEntryHeader(entryHeader);
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			CombineAssertions(() =>
			{
				AssertEquals("Result should be Written Off since all guarantees have negative or 0 balance", "Written Off", result);

				var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for first guarantee", 10, guarantee1Transactions.Count());
				AssertNewTransaction("First guarantee's transaction", guarantee1Transactions.First(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)), 220m, new ZDateTime(2021, 10, 05, 09, 36, 0), false);

				var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == SecondGuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for second guarantee", 10, guarantee2Transactions.Count());
				AssertNewTransaction("Second guarantee's transaction", guarantee2Transactions.First(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)), 1200m, new ZDateTime(2021, 10, 05, 09, 36, 0), false);

				var guarantee3Transactions = guaranteeHeaderList.First(x => x.CPH_Number == ThirdGuaranteeReference).GetTransactions();
				AssertEquals("9 Original transactions, no transactions added for third guarantee", 9, guarantee3Transactions.Count());
				AssertEquals("No new TRA Write off transaction for third guarantee", false, guarantee3Transactions.Any(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)));
			});
		}

		void AssertNewTransaction(string message, SharedCusPermitLineTransaction transaction, ZDecimal amount, ZDateTime acceptanceDate, ZBool isNcts)
		{
			AssertEquals(message + ".CPL_Reference", "MRN123", transaction.CPL_Reference);
			AssertEquals(message + ".CPL_Comment", isNcts ? "Write-off NCTS Departure AH3" : "Write-off AH3", transaction.CPL_Comment);
			AssertEquals(message + ".CPL_TranValue", amount, transaction.CPL_TranValue);
			AssertEquals(message + ".CPL_TransactionDate", acceptanceDate, transaction.CPL_TransactionDate);
			AssertEquals(message + ".CPL_TransactionStatus", PermitTransactionStatusList.Codes.Confirmed, transaction.CPL_TransactionStatus);
		}

		protected override void SetUp()
		{
			base.SetUp();

			logger = new LoggingInformation();
		}
		LoggingInformation logger;

		(CusEntryHeader, JobDeclaration, CusEntryInstruction) SetUpCusEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var mergeResult = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = LocalReferenceNumber;
			entryHeader.MovementReferenceNumberSetter(MRNCode, ZDateTime.Today);
			return (entryHeader, declaration, entryInstruction);
		}

		ZString AddGuaranteesWriteOffTransactionsEntryHeader(CusEntryHeader header)
		{
			var writeOffTransactionCreator = new WriteOffTransactionCreator();
			return writeOffTransactionCreator.AddGuaranteesWriteOffTransactionsEntryHeader(header, logger);
		}

		const string EsCode = CountryCodes.Spain;
		const string LocalReferenceNumber = "AH3";
		const string MRNCode = "MRN123";
		const string GuaranteeReference = "16ESAGL9990000096";
		const string SecondGuaranteeReference = "17ESAGL9990000097";
		const string ThirdGuaranteeReference = "18ESAGL9990000098";
		const string WriteOffTransactionCommentPrefix = "Write-off";

		void SetUpGuarantees(string type, string guaranteeReference = GuaranteeReference, string secondGuaranteeReference = SecondGuaranteeReference)
		{
			SetUpGuarantee(guaranteeReference, type, -50m, 1000m, 750);
			SetUpGuarantee(secondGuaranteeReference, type, -540m, 1200m, 460);
		}

		void AddGuaranteesToDeclaration(JobDeclaration declaration, ZGuid entryInstructionPK, string guaranteeReference = GuaranteeReference, string secondGuaranteeReference = SecondGuaranteeReference)
		{
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.B;

			GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstructionPK, guaranteeReference), (entryInstructionPK, secondGuaranteeReference), (entryInstruction2.PK, guaranteeReference));
			declaration.Guarantees[0].PW_BondAmount = 1000m;
			declaration.Guarantees[1].PW_BondAmount = 1200m;
			declaration.Guarantees[2].PW_BondAmount = 1500m;
		}

		void SetUpGuarantee(string reference, string type, decimal transactionValue, decimal oblTransactionValue, decimal balance)
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = reference;
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			guaranteeHeader.CPH_SystemCreateTimeUtc = ZDate.Today;
			guaranteeHeader.CPH_Type = type;
			guaranteeHeader.CPH_RN_NKCountryCode = EsCode;
			guaranteeHeader.CPH_Balance = balance;

			AddOBLTransaction(guaranteeHeader, oblTransactionValue);

			AddTransaction(guaranteeHeader, "First-CON", transactionValue, PermitTransactionStatusList.Codes.Confirmed);
			AddTransaction(guaranteeHeader, "Second-CON", 30m, PermitTransactionStatusList.Codes.Confirmed);
			AddTransaction(guaranteeHeader, "Third-CON", -90m, PermitTransactionStatusList.Codes.Confirmed);
			AddTransaction(guaranteeHeader, "Fourth-PEN", -40m, PermitTransactionStatusList.Codes.Pending);

			Factory.Save();
		}

		void AddOBLTransaction(CusGuaranteeHeader guaranteeHeader, ZDecimal value)
		{
			var transaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction.CPL_Reference = "OPENING";
			transaction.CPL_TranValue = value;
			transaction.CPL_Comment = "OPENING";
			transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
		}

		protected void AddTransaction(CusGuaranteeHeader guaranteeHeader, ZString comment, ZDecimal value, ZString status)
		{
			var transaction1 = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction1.CPL_Reference = MRNCode;
			transaction1.CPL_TranValue = value;
			transaction1.CPL_TransactionStatus = status;
			transaction1.CPL_Comment = comment;
			transaction1.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;

			var transaction2 = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction2.CPL_Reference = LocalReferenceNumber;
			transaction2.CPL_TranValue = value;
			transaction2.CPL_TransactionStatus = status;
			transaction2.CPL_Comment = comment;
			transaction2.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
		}

		CusGuaranteeHeader[] LoadCusGuaranteeHeaderList()
		{
			var query = new ZQuery(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Guarantee);
			return Factory.Load<CusGuaranteeHeader>(query);
		}
	}
}
