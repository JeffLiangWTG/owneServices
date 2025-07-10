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
	[TestedType(typeof(APTransfer))]
	public class APTransferTest : TransferTest
	{
		public void TestAH_Ledger()
		{
			AssertEquals("internal transfers should be AP", ZArchitecture.Core.LedgerTypes.AccountsPayable, TestTransfer.TransferFrom.AH_Ledger);
			AssertEquals("internal transfers should be AP", ZArchitecture.Core.LedgerTypes.AccountsPayable, TestTransfer.TransferTo.AH_Ledger);
		}

		public override void TestDefaultExchangeRate()
		{
			base.TestDefaultExchangeRate();
			AssertEquals("exchange rate type should be buy", ExchangeRateType.Buy, TestTransfer.ExchangeRate.Type);
		}

		public void TestOrganizationBindToLists()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			query.AddSubQuery(subQuery, JoinCondition.And);

			OrgHeaderCollection creditorCollection = new CreditorCollection(new BusinessObjectFactory(), query);
			AssertEquals("From and To Account should be all creditors", creditorCollection.Count, TestTransfer.TransferFrom.Lookups.Headers.Count);
		}

		public override void TestValidateAH_FromAccount()
		{
			base.TestValidateAH_FromAccount();
			TestTransfer.AH_FromAccount = ZGuid.Empty;
			TestTransfer.AH_ToAccount = ZGuid.Empty;
			ZQuery filter = new ZQuery(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
			filter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			TestTransfer.AH_FromAccount = (Factory.LoadTop1(typeof(OrgCompanyData), filter) as OrgCompanyData).OB_OH;
			Assert("APTransfer FromAccount is a Creditor, should be no errors", !TestTransfer.AH_FromAccountInfo.HasErrors());

			filter = new ZQuery(OrgCompanyDataSchema.OB_IsCreditor, ZBool.False);
			filter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			TestTransfer.AH_FromAccount = (Factory.LoadTop1(typeof(OrgCompanyData), filter) as OrgCompanyData).OB_OH;
			Assert("APTransfer FromAccount is not a Creditor, should give errors", TestTransfer.AH_FromAccountInfo.HasErrors());
		}

		public override void TestValidateAH_ToAccount()
		{
			base.TestValidateAH_ToAccount();
			TestTransfer.AH_FromAccount = ZGuid.Empty;
			TestTransfer.AH_ToAccount = ZGuid.Empty;
			ZQuery filter = new ZQuery(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
			filter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			TestTransfer.AH_ToAccount = (Factory.LoadTop1(typeof(OrgCompanyData), filter) as OrgCompanyData).OB_OH;
			Assert("APTransfer ToAccount is a Creditor, should be no errors", !TestTransfer.AH_ToAccountInfo.HasErrors());

			filter = new ZQuery(OrgCompanyDataSchema.OB_IsCreditor, ZBool.False);
			filter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			TestTransfer.AH_ToAccount = (Factory.LoadTop1(typeof(OrgCompanyData), filter) as OrgCompanyData).OB_OH;
			Assert("APTransfer ToAccount is not a Creditor, should give errors", TestTransfer.AH_ToAccountInfo.HasErrors());
		}

		public void TestAPTransfer_PostingPositiveTransfer()
		{
			TestTransfer.AH_ExchangeRateAmount = 3;
			TestTransfer.AH_OSTotal = 45;

			DataRow fromRow = ((INeedRow)TestTransfer.TransferFrom).Row;
			AssertEquals(15M, fromRow["AH_InvoiceAmount"]);
			AssertEquals(45M, fromRow["AH_OSTotal"]);
			AssertEquals(15M, fromRow["AH_OutstandingAmount"]);

			DataRow toRow = ((INeedRow)TestTransfer.TransferTo).Row;
			AssertEquals(-15M, toRow["AH_InvoiceAmount"]);
			AssertEquals(-45M, toRow["AH_OSTotal"]);
			AssertEquals(-15M, toRow["AH_OutstandingAmount"]);
		}

		public void TestAPTransfer_PostingNegativeTransfer()
		{
			TestTransfer.AH_ExchangeRateAmount = 2;
			TestTransfer.AH_OSTotal = -100;

			DataRow fromRow = ((INeedRow)TestTransfer.TransferFrom).Row;
			AssertEquals(-50M, fromRow["AH_InvoiceAmount"]);
			AssertEquals(-100M, fromRow["AH_OSTotal"]);
			AssertEquals(-50M, fromRow["AH_OutstandingAmount"]);
			AssertEquals(50M, TestTransfer.AH_Calc_FromAfterTransfer);

			DataRow toRow = ((INeedRow)TestTransfer.TransferTo).Row;
			AssertEquals(50M, toRow["AH_InvoiceAmount"]);
			AssertEquals(100M, toRow["AH_OSTotal"]);
			AssertEquals(50M, toRow["AH_OutstandingAmount"]);
			AssertEquals(-50M, TestTransfer.AH_Calc_ToAfterTransfer);
		}

		public void TestAPTransactionNumberFountain()
		{
			string beforeSavingNumber = Env.NumberFountains.APTransferNo.GetTodaysPeriodFountain().PeekPreliminaryFormatted(Factory);
			Factory.Save();
			AssertEquals("APTransfer number fountain should be increased by 1", beforeSavingNumber, TestTransfer.TransferFrom.AH_TransactionNum);
			AssertEquals("ToTransfer and FromTransfer should have the same transactionNum", TestTransfer.TransferFrom.AH_TransactionNum, TestTransfer.TransferTo.AH_TransactionNum);
		}

		public override void TestReverseTransaction()
		{
			base.TestReverseTransaction();
			AssertEquals("TransferFromRow of TestReversingTransfer should have OSTotal = -500", -500M, TestReversingTransfer.TransferFrom.AH_OSTotal);
			AssertEquals("TransferFromRow of TestReversingTransfer should have InvoiceAmount = -526.32", -526.32M, TestReversingTransfer.TransferFrom.AH_InvoiceAmount);
			AssertEquals("TransferFromRow of TestReversingTransfer should have OutstandingAmt = -526.32", -526.32M, TestReversingTransfer.TransferFrom.AH_OutstandingAmount);

			AssertEquals("TransferToRow of TestReversingTransfer should have OSTotal = 500", 500M, TestReversingTransfer.TransferTo.AH_OSTotal);
			AssertEquals("TransferToRow of TestReversingTransfer should have InvoiceAmount = 526.32", 526.32M, TestReversingTransfer.TransferTo.AH_InvoiceAmount);
			AssertEquals("TransferToRow of TestReversingTransfer should have OutstandingAmt = 526.32", 526.32M, TestReversingTransfer.TransferTo.AH_OutstandingAmount);
		}

		public void TestUserAllowedToBackPost()
		{
			bool isPayablesPostAllowed = Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed;

			try
			{
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				AssertEquals("UserAllowedToBackPost", false, TestTransfer.UserAllowedToBackPost);

				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				AssertEquals("UserAllowedToBackPost", true, TestTransfer.UserAllowedToBackPost);
			}
			finally
			{
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = isPayablesPostAllowed;
			}
		}

		[TestedType(typeof(APTransfer))]
		public class APTransferMatchingTest : TransferMatchingTest
		{
			protected override Transfer GetNewTransfer()
			{
				return Transfer.New(typeof(APTransfer), Factory);
			}
		}

		#region TestUnmatchSystemGeneratedAPTransfer

		public void TestUnmatchSystemGeneratedAPTransfer()
		{
			TestTransfer.AH_InvoiceAmount = 70M;
			TestTransfer.AH_OSTotal = 70M;
			TestTransfer.TransferFrom.AH_OutstandingAmount = 0M;
			TestTransfer.TransferTo.AH_OutstandingAmount = 0M;
			TestTransfer.TransferFrom.AH_TransactionCreatedByMatching = true;
			TestTransfer.TransferTo.AH_TransactionCreatedByMatching = true;

			TransactionMatchLink transferFromMatch = ((IMatching)TestTransfer.TransferFrom).CurrentMatchGroup.AddNew();
			transferFromMatch.AP_AH = TestTransfer.TransferFrom.PK;
			transferFromMatch.AP_Amount = 70M;

			TransactionMatchLink transferToMatch = ((IMatching)TestTransfer.TransferFrom).CurrentMatchGroup.AddNew();
			transferToMatch.AP_AH = TestTransfer.TransferTo.PK;
			transferToMatch.AP_Amount = -70M;
			TestObjectCreator.SetupMatchLinkMatchDate(TestTransfer.TransferFrom);

			Factory.Save();

			((IMatching)TestTransfer).Unmatch(0M, 0m);
			ZDateTime expectedPostDate = ZDateTime.BrettsBirthday;
			(TestTransfer as IMatching).ChangeUnmatchDate(expectedPostDate);

			// In practice the original matchlinks will be deleted at the end
			transferFromMatch.Delete();
			transferToMatch.Delete();

			Factory.Save();

			// What should happen:
			// -2 APTransfer rows should be created to reverse the original TransferRows;
			//		these should have the same transaction number					
			// -4 Matchlinks should be created with the same matchgroup number and match date
			// -All APTransfer rows should be Cancelled

			// Pull out the Original APTransferFrom Row
			ZQuery origAPTransferFromRowFilter = new ZQuery(AccTransactionHeaderSchema.PK, TestTransfer.TransferFrom.PK);
			APTransferFromRow origAPTransferFromRow = Factory.LoadTop1(typeof(APTransferFromRow), origAPTransferFromRowFilter) as APTransferFromRow;
			AssertNotNull("Original FromRow should be in DB", origAPTransferFromRow);
			AssertEquals("Outstanding Amt should be 0", 0M, origAPTransferFromRow.AH_OutstandingAmount);
			AssertEquals("FullyPaidDate should be changed", expectedPostDate, origAPTransferFromRow.AH_FullyPaidDate.Date);

			// Pull out the Original APTransferToRow
			ZQuery origAPTransferToRowFilter = new ZQuery(AccTransactionHeaderSchema.PK, TestTransfer.TransferTo.PK);
			APTransferToRow origAPTransferToRow = Factory.LoadTop1(typeof(APTransferToRow), origAPTransferToRowFilter) as APTransferToRow;
			AssertNotNull("Original ToRow should be in DB", origAPTransferToRow);
			AssertEquals("Outstanding Amt should be 0", 0M, origAPTransferToRow.AH_OutstandingAmount);
			AssertEquals("FullyPaidDate should be changed", expectedPostDate, origAPTransferToRow.AH_FullyPaidDate.Date);

			// Pull out the Reversing APTransferFrom Row
			ZQuery revAPTransferFromRowFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Transfer);
			revAPTransferFromRowFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, TestTransfer.TransferFrom.PK);
			revAPTransferFromRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			ZQuery transferFromFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionCount, (byte)1);
			transferFromFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionCount, SQLComparisonOperator.Equal, (byte)3);
			revAPTransferFromRowFilter.AddToFilter(transferFromFilter, JoinCondition.And);

			APTransferFromRow revAPTransferFromRow = Factory.LoadTop1(typeof(APTransferFromRow), revAPTransferFromRowFilter) as APTransferFromRow;
			AssertNotNull("Reversing APTransferFromRow should be created", revAPTransferFromRow);
			AssertEquals("InvoiceAmount on Reversing APTransferFromRow should be -70", -70M, revAPTransferFromRow.AH_InvoiceAmount);
			AssertEquals("OSTotal on Reversing APTransferFromRow should be -70", -70M, revAPTransferFromRow.AH_OSTotal);
			AssertEquals("Outstanding amoung on Rev APTransferFromRow should be 0", 0M, revAPTransferFromRow.AH_OutstandingAmount);
			Assert("Reversing APTransferFromRow should be cancelled", revAPTransferFromRow.AH_IsCancelled);
			AssertEquals("PostDate should be changed", expectedPostDate, revAPTransferFromRow.AH_PostDate);
			AssertEquals("FullyPaidDate should be changed", expectedPostDate, revAPTransferFromRow.AH_FullyPaidDate.Date);

			// Pull out the Reversing APTransferToRow
			ZQuery revAPTransferToRowFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Transfer);
			revAPTransferToRowFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, TestTransfer.TransferTo.PK);
			revAPTransferToRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			ZQuery transferToFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionCount, (byte)2);
			transferToFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionCount, SQLComparisonOperator.Equal, (byte)4);
			revAPTransferToRowFilter.AddToFilter(transferToFilter, JoinCondition.And);

			APTransferToRow revAPTransferToRow = Factory.LoadTop1(typeof(APTransferToRow), revAPTransferToRowFilter) as APTransferToRow;
			AssertNotNull("Reversing APTransferToRow should be created", revAPTransferToRow);
			AssertEquals("InvoiceAmount on Reversing APTransferToRow should be 70", 70M, revAPTransferToRow.AH_InvoiceAmount);
			AssertEquals("OSTotal on Reversing APTransferToRow should be 70", 70M, revAPTransferToRow.AH_OSTotal);
			AssertEquals("Outstanding amount on Reversing APTransferToRow should be 0", 0M, revAPTransferToRow.AH_OutstandingAmount);
			Assert("Reversing APTransferToRow should be cancelled", revAPTransferToRow.AH_IsCancelled);
			AssertEquals("PostDate should be changed", expectedPostDate, revAPTransferToRow.AH_PostDate);
			AssertEquals("FullyPaidDate should be changed", expectedPostDate, revAPTransferToRow.AH_FullyPaidDate.Date);

			AssertEquals("Reversing TransferFrom and TransferTo Rows should have the same transaction number",
				revAPTransferFromRow.AH_TransactionNum, revAPTransferToRow.AH_TransactionNum);

			// Pull out the Matchlink for Reversing APTransferFromRow
			ZQuery revAPFromRowMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revAPTransferFromRow.PK);
			TransactionMatchLink revAPFromRowMatch = Factory.LoadTop1(typeof(TransactionMatchLink), revAPFromRowMatchFilter) as TransactionMatchLink;
			AssertNotNull("Reversing APTransferFromRow should have a matchlink", revAPFromRowMatch);
			AssertEquals("Match amount for Reversing APTransferFromRow = -70", -70M, revAPFromRowMatch.AP_Amount);
			AssertEquals("MatchDate should as changed post date.", expectedPostDate, revAPFromRowMatch.AP_MatchDate);
			ZString commonMatchGroupNum = revAPFromRowMatch.AP_MatchGroupNum;
			ZDateTime commonMatchDate = revAPFromRowMatch.AP_MatchDate;

			// Pull out the Matchlink for Original APTransferFromRow
			ZQuery aPFromRowMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestTransfer.TransferFrom.PK);
			TransactionMatchLink aPFromRowMatch = Factory.LoadTop1(typeof(TransactionMatchLink), aPFromRowMatchFilter) as TransactionMatchLink;
			AssertNotNull("Original APTransferFromRow should have a matchlink", aPFromRowMatch);
			AssertEquals("Match amount for Original APTransferFromRow = 70", 70M, aPFromRowMatch.AP_Amount);
			AssertEquals("Match group for the 4 Matchlinks should be the same", commonMatchGroupNum, aPFromRowMatch.AP_MatchGroupNum);
			AssertEquals("Match date for the 4 matchlinks should be the same", commonMatchDate, aPFromRowMatch.AP_MatchDate);

			// Pull out the Matchlink for Reversing APTransferToRow
			ZQuery revAPToRowMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revAPTransferToRow.PK);
			TransactionMatchLink revAPToRowMatch = Factory.LoadTop1(typeof(TransactionMatchLink), revAPToRowMatchFilter) as TransactionMatchLink;
			AssertNotNull("Reversing APTransferToRow should have a matchlink", revAPToRowMatch);
			AssertEquals("Match amount for Reversing APTransferToRow = 70", 70M, revAPToRowMatch.AP_Amount);
			AssertEquals("Match group for the 4 matchlinks should be the same", commonMatchGroupNum, revAPToRowMatch.AP_MatchGroupNum);
			AssertEquals("Match date for the 4 matchlinks should be the same", commonMatchDate, revAPToRowMatch.AP_MatchDate);

			// Pull out the Matchlink for Original APTransferToRow
			ZQuery aPToRowMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestTransfer.TransferTo.PK);
			TransactionMatchLink aPToRowMatch = Factory.LoadTop1(typeof(TransactionMatchLink), aPToRowMatchFilter) as TransactionMatchLink;
			AssertNotNull("Original APTransferToRow should have a matchlink", aPToRowMatch);
			AssertEquals("Match amount for Original APTransferToRow = -70", -70M, aPToRowMatch.AP_Amount);
			AssertEquals("Match group for the 4 matchlinks should be the same", commonMatchGroupNum, aPToRowMatch.AP_MatchGroupNum);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Transfer.New(typeof(APTransfer), Factory);
		}

		protected override Security.SecurityCheckpoint CheckPointForPostToPreviousOrFutureOpenPeriod
		{
			get { return Env.Security.PayablesPostToPreviousOrFutureOpenPeriod; }
		}

		#endregion
	}
}
