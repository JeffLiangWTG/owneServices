using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Workflow.StmALog;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmALogAddedToQueueOnly))]
	sealed class StmALogAddedToQueueOnlyTest : BaseStmALogTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<StmALogAddedToQueueOnly>();
		}

		public override void TestBizObjectFields()
		{
			var bizO = (StmALogAddedToQueueOnly)GetNewBusinessObject();
			using (bizO.LockForUpdatingKeyFieldsForTesting())
			{
				TestBizObjectFieldsCore(bizO);
			}
		}

		#endregion
	}

	[UseSnapshotProtection]
	sealed class StmALogAddedToQueueOnlyNonTransactionedTestCase : TestCase
	{
		public void TestAddToStmALogQueue()
		{
			var factory = new BusinessObjectFactory();
			(var log, var props) = NewStmALogAddedToQueueOnly(factory);
			factory.Save();

			Assert(!log.IsInDatabase);
			StmALogWriterTest.CheckForStmALogQueueRecord(props);
		}

		public void TestAddToStmALogQueue_WithMultipleSaves()
		{
			var factory = new BusinessObjectFactory();
			(var log, var props) = NewStmALogAddedToQueueOnly(factory);

			factory.Save();
			factory.Save();

			Assert(!log.IsInDatabase);
			StmALogWriterTest.CheckForStmALogQueueRecord(props);
		}

		public void TestAddToStmALogQueue_WithFailedSave()
		{
			var factory = new BusinessObjectFactory();
			var actionWithException = new TransactionActionWithException(factory);
			factory.SaveInTransactionActions.Add(actionWithException);
			(var log, var props) = NewStmALogAddedToQueueOnly(factory);

			try
			{
				factory.Save();
			}
			catch (SqlException)
			{
				// Ignore
			}

			Assert(!log.IsInDatabase);
			StmALogWriterTest.CheckForStmALogQueueRecord(props, shouldExist: false);
		}

		public void TestAddToStmALogQueue_SuccessOnRetry()
		{
			var factory = new BusinessObjectFactory();
			var actionWithException = new TransactionActionWithException(factory);
			factory.SaveInTransactionActions.Add(actionWithException);
			(var log, var props) = NewStmALogAddedToQueueOnly(factory);

			try
			{
				factory.Save();
			}
			catch (SqlException)
			{
				// Ignore
			}

			factory.SaveInTransactionActions.Remove(actionWithException);
			factory.Save();

			Assert(!log.IsInDatabase);
			StmALogWriterTest.CheckForStmALogQueueRecord(props);
		}

		public void TestAfterOnSavingDeleteRemovesRowBeforeCommit()
		{
			var factory = new BusinessObjectFactory();
			(var log, var props) = NewStmALogAddedToQueueOnly(factory);
			var service = new AfterOnSavingServiceQueuedLogDeleter();
			service.LogToBeDeleted = log;
			factory.ServiceContainer.AddAfterOnSavingService(service);

			factory.Save();

			Assert(log.IsDeleted);
			StmALogWriterTest.CheckForStmALogQueueRecord(props, shouldExist: false);
		}

		public void TestAfterOnSavingDeleteRemovesRowBeforeCommitAfterFailedSave()
		{
			var factory = new BusinessObjectFactory();
			var actionWithException = new TransactionActionWithException(factory);
			factory.SaveInTransactionActions.Add(actionWithException);
			(var log, var props) = NewStmALogAddedToQueueOnly(factory);

			try
			{
				factory.Save();
			}
			catch (SqlException)
			{
				// Ignore
			}

			factory.SaveInTransactionActions.Remove(actionWithException);
			log.Delete();
			factory.Save();

			Assert(!log.IsInDatabase);
			StmALogWriterTest.CheckForStmALogQueueRecord(props, shouldExist: false);
		}

		(StmALogAddedToQueueOnly, StmALogQueueProperties) NewStmALogAddedToQueueOnly(BusinessObjectFactory factory)
		{
			var log = factory.New<StmALogAddedToQueueOnly>();
			var props = SetPropertiesOfStmALogAddedToQueueOnly(log);
			return (log, props);
		}

		static StmALogQueueProperties SetPropertiesOfStmALogAddedToQueueOnly(StmALogAddedToQueueOnly log)
		{
			var expectedParams = new StmALogQueueProperties
			{
				TableName = "TestTable",
				ParentId = Guid.NewGuid(),
				UserCode = "USR",
				BranchCode = "BR1",
				DepartmentCode = "DEP",
				FireWorkflow = true,
				EventCode = "Z01",
				Reference = "TestReference",
				EventTime = new DateTimeOffset(new DateTime(2025, 1, 10, 20, 9, 9), TimeSpan.FromHours(2)),
				IsCancelled = false,
				IsEstimate = true,
				ALogReference = Guid.Empty
			};

			using (log.LockForUpdatingKeyFields(FireWorkflowMode.Suppress))
			{
				log.SL_Table = expectedParams.TableName;
				log.SL_Parent = expectedParams.ParentId;
				log.SL_GS_NKUser = expectedParams.UserCode;
				log.SL_GB_NKBranch = expectedParams.BranchCode;
				log.SL_GE_NKDepartment = expectedParams.DepartmentCode;
				log.SL_FireWorkflow = expectedParams.FireWorkflow;
				log.SL_SE_NKEvent = expectedParams.EventCode;
				log.SL_Reference = expectedParams.Reference;
				log.SL_EventTimeOffset = new ZDateTimeOffset(expectedParams.EventTime);
				log.SL_IsEstimate = expectedParams.IsEstimate;
				log.IsCancelled = expectedParams.IsCancelled;
			}

			return expectedParams;
		}

		class TransactionActionWithException : SaveInTransactionActionWithFactory
		{
			public TransactionActionWithException(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override IChangedTableNames SaveInTransaction()
			{
				((IDbConnected)Factory).Connection.Command("Insert into StmALog values blah").ExecuteNonQuery();
				return ChangedTableNames.Empty;
			}
		}

		class AfterOnSavingServiceQueuedLogDeleter : IAfterOnSavingBOProcessingService
		{
			public StmALogAddedToQueueOnly LogToBeDeleted { get; set; }
			public void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				LogToBeDeleted.Delete();
			}
		}
	}
}
