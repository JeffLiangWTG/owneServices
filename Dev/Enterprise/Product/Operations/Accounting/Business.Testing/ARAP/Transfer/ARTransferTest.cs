using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(ARTransfer))]
	public class ARTransferTest : TransferTest
	{
		public void TestAH_Ledger()
		{
			AssertEquals("internal transfers should be AR", ZArchitecture.Core.LedgerTypes.AccountsReceivable, TestTransfer.TransferFrom.AH_Ledger);
			AssertEquals("internal transfers should be AR", ZArchitecture.Core.LedgerTypes.AccountsReceivable, TestTransfer.TransferTo.AH_Ledger);
		}

		public override void TestDefaultExchangeRate()
		{
			base.TestDefaultExchangeRate();
			AssertEquals("exchange rate type should be sell", ExchangeRateType.Sell, TestTransfer.ExchangeRate.Type);
		}

		public void TestOrganizationBindToLists()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			query.AddSubQuery(subQuery, JoinCondition.And);

			OrgHeaderCollection debtorCollection = new DebtorCollection(new BusinessObjectFactory(), query);
			AssertEquals("From and To Account should be all debtors", debtorCollection.Count, TestTransfer.TransferFrom.Lookups.Headers.Count);
		}

		public override void TestValidateAH_FromAccount()
		{
			base.TestValidateAH_FromAccount();
			TestTransfer.AH_FromAccount = ZGuid.Empty;
			TestTransfer.AH_ToAccount = ZGuid.Empty;
			ZQuery filter = new ZQuery(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
			filter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			TestTransfer.AH_FromAccount = (Factory.LoadTop1(typeof(OrgCompanyData), filter) as OrgCompanyData).OB_OH;
			Assert("APTransfer FromAccount is a Debtor, should be no errors", !TestTransfer.AH_FromAccountInfo.HasErrors());

			filter = new ZQuery(OrgCompanyDataSchema.OB_IsDebtor, ZBool.False);
			filter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			TestTransfer.AH_FromAccount = (Factory.LoadTop1(typeof(OrgCompanyData), filter) as OrgCompanyData).OB_OH;
			Assert("APTransfer FromAccount is not a Debtor, should give errors", TestTransfer.AH_FromAccountInfo.HasErrors());
		}

		public override void TestValidateAH_ToAccount()
		{
			base.TestValidateAH_ToAccount();
			TestTransfer.AH_FromAccount = ZGuid.Empty;
			TestTransfer.AH_ToAccount = ZGuid.Empty;
			ZQuery filter = new ZQuery(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
			filter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			TestTransfer.AH_ToAccount = (Factory.LoadTop1(typeof(OrgCompanyData), filter) as OrgCompanyData).OB_OH;
			Assert("APTransfer ToAccount is a debtor, should be no errors", !TestTransfer.AH_ToAccountInfo.HasErrors());

			filter = new ZQuery(OrgCompanyDataSchema.OB_IsDebtor, ZBool.False);
			filter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			TestTransfer.AH_ToAccount = (Factory.LoadTop1(typeof(OrgCompanyData), filter) as OrgCompanyData).OB_OH;
			Assert("APTransfer ToAccount is not a debtor, should give errors", TestTransfer.AH_ToAccountInfo.HasErrors());
		}

		public void TestARTransfer_PostingPositiveAmount()
		{
			TestTransfer.AH_ExchangeRateAmount = 1;
			TestTransfer.AH_OSTotal = 21;

			DataRow fromRow = ((INeedRow)TestTransfer.TransferFrom).Row;
			AssertEquals(-21M, fromRow["AH_InvoiceAmount"]);
			AssertEquals(-21M, fromRow["AH_OSTotal"]);
			AssertEquals(-21M, fromRow["AH_OutstandingAmount"]);

			DataRow toRow = ((INeedRow)TestTransfer.TransferTo).Row;
			AssertEquals(21M, toRow["AH_InvoiceAmount"]);
			AssertEquals(21M, toRow["AH_OSTotal"]);
			AssertEquals(21M, toRow["AH_OutstandingAmount"]);
		}

		public void TestARTransfer_PostingNegativeAmount()
		{
			TestTransfer.AH_ExchangeRateAmount = 2;
			TestTransfer.AH_OSTotal = -12;

			DataRow fromRow = ((INeedRow)TestTransfer.TransferFrom).Row;
			AssertEquals(6M, fromRow["AH_InvoiceAmount"]);
			AssertEquals(12M, fromRow["AH_OSTotal"]);
			AssertEquals(6M, fromRow["AH_OutstandingAmount"]);
			AssertEquals(6M, TestTransfer.AH_Calc_FromAfterTransfer);

			DataRow toRow = ((INeedRow)TestTransfer.TransferTo).Row;
			AssertEquals(-6M, toRow["AH_InvoiceAmount"]);
			AssertEquals(-12M, toRow["AH_OSTotal"]);
			AssertEquals(-6M, toRow["AH_OutstandingAmount"]);
			AssertEquals(-6M, TestTransfer.AH_Calc_ToAfterTransfer);
		}

		public void TestARTransactionNumberFountain()
		{
			string beforeSavingNumber = Env.NumberFountains.ARTransferNo.GetTodaysPeriodFountain().PeekPreliminaryFormatted(Factory);
			Factory.Save();
			AssertEquals("ARTransfer number fountain should be increased by 1", beforeSavingNumber, TestTransfer.TransferFrom.AH_TransactionNum);
			AssertEquals("ToTransfer and FromTransfer should have the same transactionNum", TestTransfer.TransferFrom.AH_TransactionNum, TestTransfer.TransferTo.AH_TransactionNum);
		}

		public override void TestReverseTransaction()
		{
			base.TestReverseTransaction();
			AssertEquals("TransferFromRow of TestReversingTransfer should have OSTotal = 500", 500M, TestReversingTransfer.TransferFrom.AH_OSTotal);
			AssertEquals("TransferFromRow of TestReversingTransfer should have InvoiceAmount = 526.32", 526.32M, TestReversingTransfer.TransferFrom.AH_InvoiceAmount);
			AssertEquals("TransferFromRow of TestReversingTransfer should have OutstandingAmt = 526.32", 526.32M, TestReversingTransfer.TransferFrom.AH_OutstandingAmount);

			AssertEquals("TransferToRow of TestReversingTransfer should have OSTotal = -500", -500M, TestReversingTransfer.TransferTo.AH_OSTotal);
			AssertEquals("TransferToRow of TestReversingTransfer should have InvoiceAmount = -526.32", -526.32M, TestReversingTransfer.TransferTo.AH_InvoiceAmount);
			AssertEquals("TransferToRow of TestReversingTransfer should have OutstandingAmt = -526.32", -526.32M, TestReversingTransfer.TransferTo.AH_OutstandingAmount);
		}

		public void TestUserAllowedToBackPost()
		{
			bool isReceivablesPostAllowed = Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed;

			try
			{
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				AssertEquals("UserAllowedToBackPost", false, TestTransfer.UserAllowedToBackPost);

				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				AssertEquals("UserAllowedToBackPost", true, TestTransfer.UserAllowedToBackPost);
			}
			finally
			{
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = isReceivablesPostAllowed;
			}
		}

		[TestedType(typeof(ARTransfer))]
		public class ARTransferMatchingTest : TransferMatchingTest
		{
			protected override Transfer GetNewTransfer()
			{
				return Transfer.New(typeof(ARTransfer), Factory);
			}
		}

		#region TestUnmatchSystemGeneratedARTransfer

		public void TestUnmatchSystemGeneratedARTransfer()
		{
			TestTransfer.TransferFrom.AH_TransactionCreatedByMatching = true;
			TestTransfer.TransferTo.AH_TransactionCreatedByMatching = true;
			TestTransfer.AH_InvoiceAmount = 44M;
			TestTransfer.AH_OSTotal = 44M;
			TestTransfer.TransferFrom.AH_OutstandingAmount = 0M;
			TestTransfer.TransferTo.AH_OutstandingAmount = 0M;

			TransactionMatchLink transferFromMatch = ((IMatching)TestTransfer.TransferFrom).CurrentMatchGroup.AddNew();
			transferFromMatch.AP_AH = TestTransfer.TransferFrom.PK;
			transferFromMatch.AP_Amount = -44M;

			TransactionMatchLink transferToMatch = ((IMatching)TestTransfer.TransferFrom).CurrentMatchGroup.AddNew();
			transferToMatch.AP_AH = TestTransfer.TransferTo.PK;
			transferToMatch.AP_Amount = 44M;
			TestObjectCreator.SetupMatchLinkMatchDate(TestTransfer.TransferFrom);

			Factory.Save();

			((IMatching)TestTransfer).Unmatch(0M, 0m);
			ZDateTime expectedPostDate = ZDateTime.BrettsBirthday;
			(TestTransfer as IMatching).ChangeUnmatchDate(expectedPostDate);

			// In practice the transfer row Matchlinks will be deleted
			transferFromMatch.Delete();
			transferToMatch.Delete();

			Factory.Save();

			AssertEquals("AH_PostDate should be unchanged", ZDateTime.Today, TestTransfer.AH_PostDate.Date);
			AssertEquals("FullyPaidDate should be changed", expectedPostDate, TestTransfer.TransferFrom.AH_FullyPaidDate.Date);
			AssertEquals("FullyPaidDate should be changed", expectedPostDate, TestTransfer.TransferTo.AH_FullyPaidDate.Date);

			// Pull out the Reversing ARTransferFromRow
			ZQuery revARTransferFromRowFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Transfer);
			revARTransferFromRowFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, TestTransfer.TransferFrom.PK);
			ZQuery transferFromFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionCount, (byte)1);
			transferFromFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionCount, SQLComparisonOperator.Equal, (byte)3);
			revARTransferFromRowFilter.AddToFilter(transferFromFilter, JoinCondition.And);

			ARTransferFromRow revARTransferFromRow = Factory.LoadTop1(typeof(ARTransferFromRow), revARTransferFromRowFilter) as ARTransferFromRow;
			AssertNotNull("Reversing ARTransferFromRow should be created", revARTransferFromRow);
			AssertEquals("InvoiceAmount on Reversing ARTransferFromRow = 44", 44M, revARTransferFromRow.AH_InvoiceAmount);
			AssertEquals("OSTotal on Reversing ARTransferFromRow = 44", 44M, revARTransferFromRow.AH_OSTotal);
			AssertEquals("Outstanding amt on Reversing ARTransferFromRow = 0", 0M, revARTransferFromRow.AH_OutstandingAmount);
			Assert("Reversing ARTransferFromRow should be cancelled", revARTransferFromRow.AH_IsCancelled);
			AssertEquals("PostDate should be changed", expectedPostDate, revARTransferFromRow.AH_PostDate);
			AssertEquals("FullyPaidDate should be changed", expectedPostDate, revARTransferFromRow.AH_FullyPaidDate.Date);

			// Pull out the Reversing ARTransferToRow
			ZQuery revARTransferToRowFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Transfer);
			revARTransferToRowFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, TestTransfer.TransferTo.PK);
			ZQuery transferToFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionCount, (byte)2);
			transferToFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionCount, SQLComparisonOperator.Equal, (byte)4);
			revARTransferToRowFilter.AddToFilter(transferToFilter, JoinCondition.And);

			ARTransferToRow revARTransferToRow = Factory.LoadTop1(typeof(ARTransferToRow), revARTransferToRowFilter) as ARTransferToRow;
			AssertNotNull("Reversing transaction should be created for ARTransferToRow", revARTransferToRow);
			AssertEquals("InvoiceAmount on Reversing ARTransferToRow = -44", -44M, revARTransferToRow.AH_InvoiceAmount);
			AssertEquals("OSTotal on Reversing ARTransferToRow = -44", -44M, revARTransferToRow.AH_OSTotal);
			AssertEquals("Outstanding amount on Reversing ARTransferRow = 0", 0M, revARTransferToRow.AH_OutstandingAmount);
			Assert("Reversing ARTransferToRow is cancelled", revARTransferToRow.AH_IsCancelled);
			AssertEquals("PostDate should be changed", expectedPostDate, revARTransferToRow.AH_PostDate);
			AssertEquals("FullyPaidDate should be changed", expectedPostDate, revARTransferToRow.AH_FullyPaidDate.Date);

			AssertEquals("Reversing TransferFrom and TransferTo Rows should have same fullypaid date",
				revARTransferFromRow.AH_FullyPaidDate, revARTransferToRow.AH_FullyPaidDate);
			AssertEquals("Reversing TransferFrom and TransferTo Rows should have the same transaction number",
				revARTransferFromRow.AH_TransactionNum, revARTransferToRow.AH_TransactionNum);

			// Pull out the Matchlink for Original ARTransferFromRow
			ZQuery aRFromRowMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestTransfer.TransferFrom.PK);
			TransactionMatchLink aRFromRowMatch = Factory.LoadTop1(typeof(TransactionMatchLink), aRFromRowMatchFilter) as TransactionMatchLink;
			AssertNotNull("Original ARTransferFromRow should be matched", aRFromRowMatch);
			AssertEquals("MatchAmount for original ARTransferFromRow = -44", -44M, aRFromRowMatch.AP_Amount);
			AssertEquals("MatchDate should as changed post date.", expectedPostDate, aRFromRowMatch.AP_MatchDate);
			ZString commonMatchGroupNum = aRFromRowMatch.AP_MatchGroupNum;
			ZDateTime commonMatchDate = aRFromRowMatch.AP_MatchDate;

			// Pull out the Matchlink for Reversing ARTransferFromRow
			ZQuery revARFromRowMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revARTransferFromRow.PK);
			TransactionMatchLink revARFromRowMatch = Factory.LoadTop1(typeof(TransactionMatchLink), revARFromRowMatchFilter) as TransactionMatchLink;
			AssertNotNull("Reversing ARTRansferFromRow should be matched", revARFromRowMatch);
			AssertEquals("MatchAmount for reversing ARTransferFromRow = 44", 44M, revARFromRowMatch.AP_Amount);
			AssertEquals("All 4 matchlinks should have the same matchgroupnum", commonMatchGroupNum, revARFromRowMatch.AP_MatchGroupNum);
			AssertEquals("All 4 matchlinks should have the same matchdate", commonMatchDate, revARFromRowMatch.AP_MatchDate);

			// Pull out the Matchlink for Original ARTransferToRow
			ZQuery aRToRowMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestTransfer.TransferTo.PK);
			TransactionMatchLink aRToRowMatch = Factory.LoadTop1(typeof(TransactionMatchLink), aRToRowMatchFilter) as TransactionMatchLink;
			AssertNotNull("Original ARTransferToRow should be matched", aRToRowMatch);
			AssertEquals("MatchAmount for original ARTransferToRow = 44", 44M, aRToRowMatch.AP_Amount);
			AssertEquals("All 4 matchlinks should have same matchgroupnum", commonMatchGroupNum, aRToRowMatch.AP_MatchGroupNum);
			AssertEquals("All 4 matchlinks should have same matchdate", commonMatchDate, aRToRowMatch.AP_MatchDate);

			// Pull out the matchlink for Reversing ARTransferToRow
			ZQuery revARToRowMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revARTransferToRow.PK);
			TransactionMatchLink revARToRowMatch = Factory.LoadTop1(typeof(TransactionMatchLink), revARToRowMatchFilter) as TransactionMatchLink;
			AssertNotNull("Reversing ARTransferToRow should be matched", revARToRowMatch);
			AssertEquals("MatchAmount for reversing ARTransferToRow = -44", -44M, revARToRowMatch.AP_Amount);
			AssertEquals("All 4 Matchlinks should have the same matchgroup", commonMatchGroupNum, revARToRowMatch.AP_MatchGroupNum);
		}

		#endregion

		#region Implementation

		protected override Security.SecurityCheckpoint CheckPointForPostToPreviousOrFutureOpenPeriod
		{
			get { return Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod; }
		}

		#endregion
	}
}
