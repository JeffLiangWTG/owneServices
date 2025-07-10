using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class WriteOffTransactionCreatorTest : TestCaseWithFactory
	{
		public void TestAddGuaranteesWriteOffTransactionsNcts_WithAdmissionDate()
		{
			var nctsHeader = SetUpNctsHeader();
			SetUpGuarantees(EUGuaranteeTypeList.Codes.TRA);
			AddGuaranteesToNcts(nctsHeader);

			var result = AddGuaranteesWriteOffTransactionsNcts(nctsHeader, admissionDate);
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			CombineAssertions(() =>
			{
				AssertEquals("Result should be Written Off since both guarantees have new transactions", "Written Off", result);

				var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for first guarantee", 10, guarantee1Transactions.Count());
				AssertNewTransaction("First guarantee's transaction", guarantee1Transactions.First(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)), 220m, admissionDate, true);

				var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == SecondGuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for second guarantee", 10, guarantee2Transactions.Count());
				AssertNewTransaction("Second guarantee's transaction", guarantee2Transactions.First(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)), 1200m, admissionDate, true);
			});
		}

		[TestDate(2021, 10, 05, 09, 36, 0)]
		public void TestAddGuaranteesWriteOffTransactionsNcts_WithNoAdmissionDate()
		{
			var nctsHeader = SetUpNctsHeader();
			SetUpGuarantees(EUGuaranteeTypeList.Codes.TRA);
			AddGuaranteesToNcts(nctsHeader);

			var result = AddGuaranteesWriteOffTransactionsNcts(nctsHeader, ZDateTime.Empty);
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			CombineAssertions(() =>
			{
				AssertEquals("Result should be Written Off since both guarantees have new transactions", "Written Off", result);

				var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for first guarantee", 10, guarantee1Transactions.Count());
				AssertNewTransaction("First guarantee's transaction", guarantee1Transactions.First(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)), 220m, new ZDateTime(2021, 10, 05, 09, 36, 0), true);

				var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == SecondGuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for second guarantee", 10, guarantee2Transactions.Count());
				AssertNewTransaction("Second guarantee's transaction", guarantee2Transactions.First(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)), 1200m, new ZDateTime(2021, 10, 05, 09, 36, 0), true);
			});
		}

		public void TestAddGuaranteesWriteOffTransactionsNcts_PositiveAmount()
		{
			var nctsHeader = SetUpNctsHeader();
			SetUpGuarantee(GuaranteeReference, EUGuaranteeTypeList.Codes.TRA, 500.123456789m, 2000m, 1300);
			var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee.PW_BondNumber = GuaranteeReference;
			guarantee.PW_BondAmount = 1000m;

			var result = AddGuaranteesWriteOffTransactionsNcts(nctsHeader, admissionDate);
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			CombineAssertions(() =>
			{
				AssertEquals("Result should be Not Written Off since the guarantee has a positive balance", "Not Written Off (at least one positive balance)", result);

				var expectedError = "Reference 16ESAGL9990000096 has a positive balance of 880.25 EUR. Please check the existing transactions for this reference and create a manual adjustment if needed.";
				var concatenatedUserLogStrings = string.Concat(logger.UserLogStrings.Cast<string>());
				AssertContains("logger", expectedError, concatenatedUserLogStrings);
			});
		}

		public void TestAddGuaranteesWriteOffTransactionsNcts_PositiveAmount_WithoutLogger()
		{
			var nctsHeader = SetUpNctsHeader();
			SetUpGuarantee(GuaranteeReference, EUGuaranteeTypeList.Codes.TRA, 500m, 2000m, 1300);
			var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee.PW_BondNumber = GuaranteeReference;
			guarantee.PW_BondAmount = 1000m;

			var writeOffTransactionCreator = new WriteOffTransactionCreator();
			var result = writeOffTransactionCreator.AddGuaranteesWriteOffTransactionsNcts(nctsHeader, admissionDate);
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			CombineAssertions(() =>
			{
				AssertEquals("Result should be Not Written Off since the guarantee has a positive balance", "Not Written Off (at least one positive balance)", result);

				var concatenatedUserLogStrings = string.Concat(logger.UserLogStrings.Cast<string>());
				AssertContains("logger", ZString.Empty, concatenatedUserLogStrings);
			});
		}

		public void TestAddGuaranteesWriteOffTransactionsNcts_NoPendingAmount()
		{
			var nctsHeader = SetUpNctsHeader();
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = GuaranteeReference;
			guaranteeHeader.CPH_StartDate = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_EndDate = new ZDate(2022, 7, 15);
			guaranteeHeader.CPH_SystemCreateTimeUtc = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.CPH_RN_NKCountryCode = EsCode;
			guaranteeHeader.CPH_Balance = 0;

			AddOBLTransaction(guaranteeHeader, 1000m);
			AddTransaction(guaranteeHeader, "First-CON", 0, PermitTransactionStatusList.Codes.Confirmed);

			var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee.PW_BondNumber = GuaranteeReference;
			guarantee.PW_BondAmount = 0m;

			var result = AddGuaranteesWriteOffTransactionsNcts(nctsHeader, admissionDate);
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			CombineAssertions(() =>
			{
				AssertEquals("Result should be Excluded since the guarantee has a balance of 0", "Excluded (no pending debt)", result);

				var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
				AssertEquals("3 Original transactions, no transactions added", 3, guarantee1Transactions.Count());
			});
		}

		public void TestAddGuaranteesWriteOffTransactionsNcts_TwoCorrectOnePositiveAmount()
		{
			var nctsHeader = SetUpNctsHeader();
			SetUpGuarantees(EUGuaranteeTypeList.Codes.TRA);
			AddGuaranteesToNcts(nctsHeader);
			SetUpGuarantee(ThirdGuaranteeReference, EUGuaranteeTypeList.Codes.TRA, 500.123456789m, 2000m, 1300);
			var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee.PW_BondNumber = ThirdGuaranteeReference;
			guarantee.PW_BondAmount = 1000m;

			var result = AddGuaranteesWriteOffTransactionsNcts(nctsHeader, admissionDate);
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			CombineAssertions(() =>
			{
				AssertEquals("Result should be Not Written Off since at least one guarantee has a positive balance", "Not Written Off (at least one positive balance)", result);

				var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for first guarantee", 10, guarantee1Transactions.Count());
				AssertNewTransaction("First guarantee's transaction", guarantee1Transactions.First(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)), 220m, admissionDate, true);

				var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == SecondGuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for second guarantee", 10, guarantee2Transactions.Count());
				AssertNewTransaction("Second guarantee's transaction", guarantee2Transactions.First(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)), 1200m, admissionDate, true);

				var expectedError = "Reference 18ESAGL9990000098 has a positive balance of 880.25 EUR. Please check the existing transactions for this reference and create a manual adjustment if needed.";
				var concatenatedUserLogStrings = string.Concat(logger.UserLogStrings.Cast<string>());
				AssertContains("logger", expectedError, concatenatedUserLogStrings);
			});
		}

		public void TestAddGuaranteesWriteOffTransactionsNcts_TwoCorrectOneNoPendingAmount()
		{
			var nctsHeader = SetUpNctsHeader();
			SetUpGuarantees(EUGuaranteeTypeList.Codes.TRA);
			AddGuaranteesToNcts(nctsHeader);
			SetUpGuarantee(ThirdGuaranteeReference, EUGuaranteeTypeList.Codes.TRA, 60m, 1000m, 0);
			var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee.PW_BondNumber = ThirdGuaranteeReference;
			guarantee.PW_BondAmount = 1000m;

			var result = AddGuaranteesWriteOffTransactionsNcts(nctsHeader, admissionDate);
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			CombineAssertions(() =>
			{
				AssertEquals("Result should be Written Off since all guarantees have negative or 0 balance", "Written Off", result);

				var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for first guarantee", 10, guarantee1Transactions.Count());
				AssertNewTransaction("First guarantee's transaction", guarantee1Transactions.First(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)), 220m, admissionDate, true);

				var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == SecondGuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for second guarantee", 10, guarantee2Transactions.Count());
				AssertNewTransaction("Second guarantee's transaction", guarantee2Transactions.First(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)), 1200m, admissionDate, true);

				var guarantee3Transactions = guaranteeHeaderList.First(x => x.CPH_Number == ThirdGuaranteeReference).GetTransactions();
				AssertEquals("9 Original transactions, no transactions added", 9, guarantee3Transactions.Count());
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

		NctsHeader SetUpNctsHeader()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.LocalReferenceNumber = LocalReferenceNumber;
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = MRNCode;
			return nctsHeader;
		}

		ZString AddGuaranteesWriteOffTransactionsNcts(NctsHeader header, ZDateTime admissionDate)
		{
			var writeOffTransactionCreator = new WriteOffTransactionCreator();
			return writeOffTransactionCreator.AddGuaranteesWriteOffTransactionsNcts(header, admissionDate, logger);
		}

		const string EsCode = CountryCodes.Spain;
		const string LocalReferenceNumber = "AH3";
		const string MRNCode = "MRN123";
		const string GuaranteeReference = "16ESAGL9990000096";
		const string SecondGuaranteeReference = "17ESAGL9990000097";
		const string ThirdGuaranteeReference = "18ESAGL9990000098";
		readonly ZDateTime admissionDate = new ZDateTime(2020, 11, 20, 18, 56, 00);

		const string WriteOffTransactionCommentPrefix = "Write-off";

		void SetUpGuarantees(string type, string guaranteeReference = GuaranteeReference, string secondGuaranteeReference = SecondGuaranteeReference)
		{
			SetUpGuarantee(guaranteeReference, type, -50m, 1000m, 750);
			SetUpGuarantee(secondGuaranteeReference, type, -540m, 1200m, 460);
		}

		void AddGuaranteesToNcts(NctsHeader header)
		{
			var guarantee = header.GetEffectiveGuarantees().AddNew();
			guarantee.PW_BondNumber = GuaranteeReference;
			guarantee.PW_BondAmount = 1000m;

			var guarantee2 = header.GetEffectiveGuarantees().AddNew();
			guarantee2.PW_BondNumber = SecondGuaranteeReference;
			guarantee2.PW_BondAmount = 1200m;
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
