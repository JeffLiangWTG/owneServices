using System;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.Semaphores.Common.Testing
{
	class SemaphoreHandleExceptionTest : TransactionedTestCase
	{
		public void TestSemaphoreReachedMaxAllowedHandlesException()
		{
			try
			{
				string sqlText = String.Format("RAISERROR('{0}', 16, 1)", SemaphoreHandleException.MaxNumberOfHandlesException);
				TestConnection.ExecuteNonQuery(sqlText);
				Fail("Should throw exception");
			}
			catch (SqlException ex)
			{
				Exception semaphoreEx = SemaphoreHandleException.NewException(ex);
				Assert("Wrapping Exception should be SemaphoreReachedMaxAllowedHandlesException but was " + semaphoreEx.GetType().FullName,
					semaphoreEx is SemaphoreReachedMaxAllowedHandlesException);
			}
		}

		public void TestSemaphoreUniqueIndexViolationException()
		{
			try
			{
				string sqlText = @"
					INSERT dbo.StmServiceHeartbeat (SV_PK, SV_ParentID, SV_ParentTableCode, SV_ExpiresAtUtc) VALUES('00000000-0000-0000-0000-000000000000', '26A2134B-7BF4-4813-8AD3-CD0D2542A712', 'OH', DateAdd(hour, 1, GetUtcDate()))
					INSERT dbo.StmServiceSemaphore (SS_PK, SS_SV, SS_ServiceClass, SS_LockInfo) VALUES(NEWID(), '00000000-0000-0000-0000-000000000000', '', '')
					INSERT dbo.StmServiceSemaphore (SS_PK, SS_SV, SS_ServiceClass, SS_LockInfo) VALUES(NEWID(), '00000000-0000-0000-0000-000000000000', '', '')";
				TestConnection.ExecuteNonQuery(sqlText);
				Fail("Should throw exception");
			}
			catch (SqlException ex)
			{
				Exception semaphoreEx = SemaphoreHandleException.NewException(ex);
				string semaphoreExceptionTypeAndMessage = String.Format("{0} - {1}", semaphoreEx.GetType().FullName, semaphoreEx.Message);
				Assert("Wrapping Exception should be SemaphoreUniqueIndexViolationException but was\r\n" + semaphoreExceptionTypeAndMessage,
					semaphoreEx is SemaphoreUniqueIndexViolationException);
			}
		}

		public void TestSemaphoreInvalidSessionIdException()
		{
			try
			{
				string sqlText = @"
					DECLARE @CurrentUtcTime datetime = sysutcdatetime();
					DECLARE @NewHandleId uniqueidentifier = newid();
					EXEC InsertSemaphoreHandle @CurrentUtcTime, 0, '00000000-0000-0000-0000-000000000000', '', '', @NewHandleId, '00000000-0000-0000-0000-000000000000'";
				Db.Connection.ExecuteNonQuery(sqlText);
				Fail("Should throw exception");
			}
			catch (SqlException ex)
			{
				Exception semaphoreEx = SemaphoreHandleException.NewException(ex);
				Assert("Wrapping Exception should be SemaphoreInvalidSessionIdException but was " + semaphoreEx.GetType().FullName,
					semaphoreEx is SemaphoreInvalidSessionIdException);
			}
		}
	}

	class SemaphoreHandleExceptionNonTransactionalTest : TestCase
	{
		public void TestSemaphoreMustBeCreatedInTransaction()
		{
			try
			{
				string raiseErrorText = @"
					DECLARE @CurrentUtcTime datetime = sysutcdatetime();
					DECLARE @NewHandleId uniqueidentifier = newid();
					EXEC InsertSemaphoreHandle @CurrentUtcTime, 0, '00000000-0000-0000-0000-000000000000', '', '', @NewHandleId, '00000000-0000-0000-0000-000000000000'";
				Db.Connection.ExecuteNonQuery(raiseErrorText);
				Fail("Should throw exception");
			}
			catch (SqlException ex)
			{
				Exception semaphoreEx = SemaphoreHandleException.NewException(ex);
				Assert("Wrapping Exception should be SemaphoreMustBeCreatedInTransactionException but was " + semaphoreEx.GetType().FullName,
					semaphoreEx is SemaphoreMustBeCreatedInTransactionException);
			}
		}
	}
}
