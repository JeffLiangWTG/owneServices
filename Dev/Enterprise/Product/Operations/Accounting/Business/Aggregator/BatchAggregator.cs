using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using DbException = System.Data.Common.DbException;

namespace Enterprise.Accounting.Business.Aggregator
{
	public class BatchAggregator : IBatchAggregator
	{
		public bool Aggregate()
		{
			bool result = false;
			LastException = null;

			Guid lastAggregator = AccountingConfigurationRegistry.Instance.LastAggregationStaff.Value;
			DateTime lastAggregationTime = AccountingConfigurationRegistry.Instance.LastAggregationDate.Value;

			Guid companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			try
			{
				AccountingConfigurationRegistry.Instance.LastAggregationDate.SetValue(companyPK, Guid.Empty, Guid.Empty, ZDateTime.Now.ToDateTime());
				AccountingConfigurationRegistry.Instance.LastAggregationStaff.SetValue(companyPK, Guid.Empty, Guid.Empty, GlbStaff.CurrentUser.PK.ToGuid());

				using (var manager = Connection.BeginTransactionWithManager())
				{
					TakeUpSubledgers(companyPK);
					CommitTransaction(manager);
				}

				result = true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				LastException = ex;
				HandleException(ex, lastAggregator, lastAggregationTime);
			}
			return result;
		}

		public Exception LastException { get; private set; }

		DbConnection Connection
		{
			get { return connection ?? (connection = Db.Connection); }
		}
		protected DbConnection connection;

#if DEBUG
		protected virtual
#endif
		void CommitTransaction(ITransactionManager manager)
		{
			manager.CommitTransaction();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void TakeUpSubledgers(Guid companyPK)
		{
			using (DbCommand takeUpSubledgersCmd = Connection.Command("EXEC TakeUpSubledgers @Company, @SystemLastEditUser"))
			{
				takeUpSubledgersCmd.CommandTimeout = int.MaxValue;
				takeUpSubledgersCmd.AddParameter("@Company", System.Data.SqlDbType.UniqueIdentifier, companyPK);
				takeUpSubledgersCmd.AddParameterBasedOnDbColumn("@SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), GlbStaffSchema.GS_Code);

				takeUpSubledgersCmd.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Exception Does Not Need To Be Localised, New line is wanted before exception message")]
		void HandleException(Exception e, Guid lastAggregator, DateTime lastAggregationTime)
		{
			string contactSupportMessage = Res.GetString("4d1309f0-d170-4915-9311-71e409aaa763", "Please contact support for assistance with this error.") + "\r\n";
			try
			{
				var message = e.Message.ToUpper();
				if (e is DbException && new DbErrorMatch(e as DbException).ExceptionType == DbErrorType.DeadlockError)
				{
					AggregateResult = Res.GetString("757c984d-176b-4611-b0dd-0676341b96c8", "Database deadlock occurred most due to other batch operation running simultaneously.\r\nPlease try again later.");
				}
				else if (e is DbException && new DbErrorMatch(e as DbException).ExceptionType == DbErrorType.ArithmeticOverflowConvertingToDataType) // This Message will be shown when there is an arithmetic overflow
				{
					AggregateResult = Res.GetString("d458c8ef-c1ab-4711-aed6-84afa5778628", "An extremely large value transaction has been processed. Please find and reverse any transactions with a transaction or outstanding balances close to 999,999,999,999. ");
					AggregatorGLMessage(lastAggregator);
				}
				else if (message.StartsWith((NoResString)"Charge Codes Incorrectly Setup".ToUpper())
					|| message.StartsWith((NoResString)"Transaction line(s) have been posted to charge codes that are incorrectly setup.".ToUpper())
					|| message.StartsWith("Please set up the following Control Accounts in the registry".ToUpper())
					|| message.StartsWith("The following transactions have post or reverse dates for which accounting periods do not exist. Please create the appropriate periods.".ToUpper())
					|| message.StartsWith("The following transactions have Cash VAT post dates for which accounting periods do not exist. Please create the appropriate periods.".ToUpper())
					|| message.StartsWith("Please set up the Appropriation Account in the registry".ToUpper()))
				{
					AggregateResult = Res.GetString("0857e27b-9693-402c-8ee8-ace1be636616", "{0}\r\nPlease retry aggregation once you have corrected this error.", e.Message) + "\r\n";
					AggregatorGLMessage(lastAggregator);
				}
				else if (message.StartsWith("The following transactions have been posted with lines that do not have a Charge or GL code:".ToUpper())
					|| message.StartsWith("Cash Book Transactions With No Bank Account".ToUpper()))
				{
					AggregateResult = e.Message + System.Environment.NewLine + contactSupportMessage;
					AggregatorGLMessage(lastAggregator);
				}
				else if (message.StartsWith("Could not obtain lock - process is already running".ToUpper()))
				{
					GlbStaff glbStaffLastAggregator = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, lastAggregator));
					if (glbStaffLastAggregator != null)
					{
						AggregateResult = Res.GetString("2da193b1-90b5-49b1-8306-a14c6cb271b6", "GL Accounts for this Company are currently being taken up by user: {0}", glbStaffLastAggregator.GS_FullName);
					}
					else
					{
						AggregateResult = Res.GetString("f0f03009-b517-4a68-9d60-818c7fb34cfe", "GL Accounts for this Company are currently being taken up by another user.");
					}
				}
				else if (message.StartsWith("The following periods do not balance".ToUpper()))
				{
					AggregateResult = Res.GetString("c1e8e414-88fd-4f03-9dc0-a71e731b3546", "{0}\r\n\r\n**** WARNING SUB-LEDGER (A/R & A/P) LEDGER WAS NOT TAKEN UP ****\r\n\r\nThere is a transaction that would cause an Imbalanced Trial Balance, if taken up.\r\n\r\n{1}", e.Message, contactSupportMessage);
				}
				else if (message.StartsWith("No Accounting Periods are specified for the current company".ToUpper())
					|| message.StartsWith("The following accounting period has a gap/overlap from the previous period".ToUpper()))
				{
					AggregateResult += message;
				}
				else
				{
					AggregateResult = "During aggregation, unknown problem is encountered. GL Account is not taken up this time.\r\nPlease come back to GL Report at a later time.\r\n";
					AggregateResult += message;
					ErrorReporter.ReportOnce("Aggregator Has Encountered Error", e);
				}
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.LastAggregationDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, lastAggregationTime);
				AccountingConfigurationRegistry.Instance.LastAggregationStaff.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, lastAggregator);
			}
		}

		void AggregatorGLMessage(Guid lastAggregator)
		{
			GlbStaff glbStaffLastAggregator = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, lastAggregator));

			string takeUpGLAccountsMessage = glbStaffLastAggregator != null
				? Res.GetString("605f5c60-c747-4784-af82-8f2a8613392c", "GL accounts were last taken up on {0} by user: {1}", AccountingConfigurationRegistry.Instance.LastAggregationDate.Value, glbStaffLastAggregator.GS_FullName)
				: Res.GetString("8ed2d1e0-ef89-449d-8d66-ea7370691dac", "GL accounts were last taken up on {0}", AccountingConfigurationRegistry.Instance.LastAggregationDate.Value);

			AggregateResult += takeUpGLAccountsMessage;
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		public string AggregateResult { get; set; }
	}
}

#region Test
#if DEBUG
namespace Enterprise.Accounting.Business.Aggregator.Test
{
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;

	public class TestBatchAggregator : BatchAggregator
	{
		public TestBatchAggregator(BatchTestHelper testHelper)
		{
			SetControlAccount(AccountingUtils.ARSuspenseControlAccount, testHelper.ARSuspenseControlAccount);
			SetControlAccount(AccountingUtils.APSuspenseControlAccount, testHelper.APSuspenseControlAccount);
		}

		public TestBatchAggregator(DbConnection alternateConnectionForTesting, BatchTestHelper testHelper)
			: this(testHelper)
		{
			connection = alternateConnectionForTesting;
		}

		public TestBatchAggregator(DbConnection alternateConnectionForTesting, bool failOnCommit, BatchTestHelper testHelper)
			: this(testHelper)
		{
			connection = alternateConnectionForTesting;
			this.failOnCommit = failOnCommit;
		}

		readonly bool failOnCommit;
		public Exception ExceptionOnCommit { get; set; }

		protected override void CommitTransaction(ITransactionManager manager)
		{
			SleepToGiveForegroundThreadAChanceToCatchUp();

			if (ExceptionOnCommit != null)
			{
				throw ExceptionOnCommit;
			}

			if (failOnCommit)
			{
				throw new NotSupportedException("Any old expcetion to cause a failure");
			}
			base.CommitTransaction(manager);
		}

		void SleepToGiveForegroundThreadAChanceToCatchUp()
		{
			System.Threading.Thread.Sleep(1000);
		}

		internal decimal GetTotalForGL(ZGuid pK)
		{
			return GetTotalForGL(pK, 0);
		}

		internal decimal GetTotalForGL(ZGuid pK, int period)
		{
			decimal result = 0m;
			AccGLAggregateCollection filteredCollection = new AccGLAggregateCollection(new BusinessObjectFactory());
			ZQuery filter = new ZQuery(AccGLAggregateSchema.AA_AG, pK);
			if (period != 0)
			{
				filter.AddToFilter(AccGLAggregateSchema.AA_Period, period);
			}
			filteredCollection.LoadWithMoreFiltering(filter);

			foreach (AccGLAggregate row in filteredCollection)
			{
				result += row.AA_Amount;
			}
			return result;
		}

		internal AccGLAggregate GetAggregateRow(ZGuid gLAccount)
		{
			AccGLAggregate result = null;
			foreach (AccGLAggregate row in AllAggregatedData)
			{
				if (row.AA_AG == gLAccount)
				{
					result = row;
					break;
				}
			}
			return result;
		}

		internal void RunAggregateForTest()
		{
			this.Aggregate();
			Factory.Save();
			allAggregatedData = null;
		}

		internal AccGLAggregateCollection AllAggregatedData
		{
			get
			{
				if (allAggregatedData == null)
				{
					allAggregatedData = new AccGLAggregateCollection(Factory);
					var queryForSorting = new ZQuery();
					queryForSorting.OrderBy = "AA_Period, AA_AG desc, AA_GB, AA_GE";
					allAggregatedData.LoadWithMoreFiltering(queryForSorting);
				}
				return allAggregatedData;
			}
		}
		AccGLAggregateCollection allAggregatedData;

		public string GetBatchPostingPreviewDataAsString()
		{
			var result = new List<string>();

			foreach (AccGLAggregate previewData in AllAggregatedData)
			{
				result.Add(string.Join(" ", previewData.AA_Period, previewData.GLHeader.AG_AccountNum, previewData.GLHeader.AG_DescriptionMultilingual, previewData.AA_Amount.ToString(2).PadLeft(8)));
			}

			result.Sort();
			return string.Join("\r\n", result);
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		public string GetNameOfControlAccountRegistrySetting(string controlAccount)
		{
			switch (controlAccount)
			{
				case AccountingUtils.JobRevenueJournalControlAccount:
					return AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.Caption;
				case AccountingUtils.ARControl:
					return AccountingConfigurationRegistry.Instance.ARControlAccount.Caption;
				case AccountingUtils.ARSuspenseControlAccount:
					return AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.Caption;
				case AccountingUtils.APControl:
					return AccountingConfigurationRegistry.Instance.APControlAccount.Caption;
				case AccountingUtils.APSuspenseControlAccount:
					return AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.Caption;
				case AccountingUtils.Overpayments:
					return AccountingConfigurationRegistry.Instance.OverpaymentsAccount.Caption;
				case AccountingUtils.ARDiscount:
					return AccountingConfigurationRegistry.Instance.ARDiscountAccount.Caption;
				case AccountingUtils.APDiscount:
					return AccountingConfigurationRegistry.Instance.APDiscountAccount.Caption;
				case AccountingUtils.GSTInput:
					return AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Caption;
				case AccountingUtils.PendingGSTInput:
					return AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.Caption;
				case AccountingUtils.GSTOutput:
					return AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Caption;
				case AccountingUtils.PendingGSTOutput:
					return AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.Caption;
				case AccountingUtils.AccruedCost:
					return AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Caption;
				case AccountingUtils.AccruedRevenue:
					return AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Caption;
			}
			return "";
		}

		public Guid SetControlAccount(string controlAccount, Guid accountPK)
		{
			var oldAccountValue = Guid.Empty;
			switch (controlAccount)
			{
				case AccountingUtils.JobRevenueJournalControlAccount:
					if (AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.Value != accountPK)
					{
						oldAccountValue = AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.Value;
						AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, accountPK);
					}
					break;
				case AccountingUtils.CFXAccount:
					if (AccountingConfigurationRegistry.Instance.CFXAccount.Value != accountPK)
					{
						using (AccountingConfigurationRegistry.Instance.CFXAccount.DataType.SuspendValidation())
						{
							oldAccountValue = AccountingConfigurationRegistry.Instance.CFXAccount.Value;
							AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, accountPK);
						}
					}
					break;
				case AccountingUtils.ARControl:
					if (AccountingConfigurationRegistry.Instance.ARControlAccount.Value != accountPK)
					{
						oldAccountValue = AccountingConfigurationRegistry.Instance.ARControlAccount.Value;
						AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, accountPK);
					}
					break;
				case AccountingUtils.ARSuspenseControlAccount:
					if (AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.Value != accountPK)
					{
						oldAccountValue = AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.Value;
						AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, accountPK);
					}
					break;
				case AccountingUtils.APControl:
					if (AccountingConfigurationRegistry.Instance.APControlAccount.Value != accountPK)
					{
						oldAccountValue = AccountingConfigurationRegistry.Instance.APControlAccount.Value;
						AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, accountPK);
					}
					break;
				case AccountingUtils.APSuspenseControlAccount:
					if (AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.Value != accountPK)
					{
						oldAccountValue = AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.Value;
						AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, accountPK);
					}
					break;
				case AccountingUtils.Overpayments:
					if (AccountingConfigurationRegistry.Instance.OverpaymentsAccount.Value != accountPK)
					{
						oldAccountValue = AccountingConfigurationRegistry.Instance.OverpaymentsAccount.Value;
						AccountingConfigurationRegistry.Instance.OverpaymentsAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, accountPK);
					}
					break;
				case AccountingUtils.ARDiscount:
					if (AccountingConfigurationRegistry.Instance.ARDiscountAccount.Value != accountPK)
					{
						oldAccountValue = AccountingConfigurationRegistry.Instance.ARDiscountAccount.Value;
						AccountingConfigurationRegistry.Instance.ARDiscountAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, accountPK);
					}
					break;
				case AccountingUtils.APDiscount:
					if (AccountingConfigurationRegistry.Instance.APDiscountAccount.Value != accountPK)
					{
						oldAccountValue = AccountingConfigurationRegistry.Instance.APDiscountAccount.Value;
						AccountingConfigurationRegistry.Instance.APDiscountAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, accountPK);
					}
					break;
				case AccountingUtils.GSTInput:
					if (AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value != accountPK)
					{
						oldAccountValue = AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value;
						AccountingConfigurationRegistry.Instance.GSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, accountPK);
					}
					break;
				case AccountingUtils.PendingGSTInput:
					if (AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.Value != accountPK)
					{
						oldAccountValue = AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.Value;
						AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, accountPK);
					}
					break;
				case AccountingUtils.GSTOutput:
					if (AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value != accountPK)
					{
						oldAccountValue = AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value;
						AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, accountPK);
					}
					break;
				case AccountingUtils.PendingGSTOutput:
					if (AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.Value != accountPK)
					{
						oldAccountValue = AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.Value;
						AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, accountPK);
					}
					break;
				case AccountingUtils.AccruedCost:
					if (AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value != accountPK)
					{
						oldAccountValue = AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value;
						AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, accountPK);
					}
					break;
				case AccountingUtils.AccruedRevenue:
					if (AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value != accountPK)
					{
						oldAccountValue = AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value;
						AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, accountPK);
					}
					break;
			}
			return oldAccountValue;
		}
	}

	[SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Full name needed")]
	public static class BatchAggregatorTestSqlExceptionHelper
	{
		public static readonly int DeadlockErrNum = 1205;
		public static readonly int ArithmeticOverflowErrNum = 8115;

		public static TSqlException CreateArithmeticOverflowSqlException<TSqlException>()
			where TSqlException : DbException
		{
			return SqlExceptionBuilder.CreateSqlException<TSqlException>(ArithmeticOverflowErrNum, String.Empty);
		}

		public static TSqlException CreateDeadlockSqlException<TSqlException>()
			where TSqlException : DbException
		{
			return SqlExceptionBuilder.CreateSqlException<TSqlException>(DeadlockErrNum, String.Empty);
		}
	}
}
#endif
#endregion
