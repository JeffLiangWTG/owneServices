using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Shared.Testing
{
	using CargoWise.Database.TestFramework.ObjectModel;
	using static System.FormattableString;

	public static class TestWhsDataSetupHelper
	{
		// Admin Data Setup

		#region GetOrgHeader

		public static Guid GetOrgHeader(params Guid[] orgHeadersToIgnore)
		{
			Guid result;
			if (orgHeadersToIgnore != null && orgHeadersToIgnore.Length > 0)
			{
				var orgHeaderPKsList = string.Join(", ", orgHeadersToIgnore.Select(o => string.Format(CultureInfo.InvariantCulture, "'{0}'", o.ToString())));
				result = (Guid)Db.Connection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, "select OH_PK from dbo.OrgHeader where OH_PK not in ({0}) order by OH_PK", orgHeaderPKsList));
			}
			else
			{
				result = (Guid)Db.Connection.ExecuteScalar("select OH_PK from dbo.OrgHeader order by OH_PK");
			}
			return result;
		}

		#endregion

		// Transit Warehouse

		#region CreateWhsItemPackageState

		public static WhsItemPackageState CreateWhsItemPackageState(
			StringBuilder sql,
			PkgPackageJob pkgPackageJob,
			string status,
			IWhsWarehouseSQL whs,
			WhsItemReceiveConsignment rcn,
			WhsItemReceiveTransportationUnit rtu = null,
			WhsItemDispatchLoadList dll = null,
			WhsItemDispatchConsignment dcn = null,
			WhsItemDispatchTransportationUnit dtu = null,
			WhsItemReceiveASN asn = null,
			WhsLocation lastLocation = null,
			WhsLocation receiveLocation = null,
			bool isHandlingUnit = false,
			string customStatus = "NON"
			)
		{
			return CreateWhsItemPackageState(sql, new PkgPackage(pkgPackageJob, "BOX", 1).AppendInsertAndReturnObject(sql).PK, status, whs, rcn, rtu, dll, dcn, dtu, asn, lastLocation, receiveLocation, isHandlingUnit, customStatus: customStatus);
		}

		public static WhsItemPackageState CreateWhsItemPackageState(
			StringBuilder sql,
			Guid packagePK,
			string status,
			IWhsWarehouseSQL whs,
			WhsItemReceiveConsignment rcn,
			WhsItemReceiveTransportationUnit rtu = null,
			WhsItemDispatchLoadList dll = null,
			WhsItemDispatchConsignment dcn = null,
			WhsItemDispatchTransportationUnit dtu = null,
			WhsItemReceiveASN asn = null,
			WhsLocation lastLocation = null,
			WhsLocation receiveLocation = null,
			bool isHandlingUnit = false,
			string unitType = "PKG",
			string receivedAs = "SCN",
			string customStatus = "NON",
			string securityStatus = "REQ"
			)
		{
			var packageState = new WhsItemPackageState(packagePK, whs, rcn?.PK, status);
			packageState.WPS_UnitType = unitType;
			packageState.WPS_CustomsStatus = customStatus;
			packageState.WPS_SecurityStatus = securityStatus;

			if (rtu != null)
			{
				packageState.WPS_WRH_TransitReceiveHeader = rtu.PK;
				packageState.WPS_WL_LastLocation = rtu.WRH_WL_StagingLocation.FK;
				packageState.WPS_UnloadedTime = DateTimeOffset.Now;
				packageState.WPS_UnloadedNotYetProcessedTime = DateTimeOffset.Now;
				packageState.WPS_ReceivedAs = receivedAs;
			}

			if (dcn != null)
			{
				packageState.WPS_WDC_TransitDispatchConsignment = dcn.PK;
			}

			if (dtu != null)
			{
				packageState.WPS_WDH_TransitDispatchHeader = dtu.PK;
				packageState.WPS_IsSecure = true;
				packageState.WPS_SecurityStatus = "SEC";
				packageState.WPS_LoadedTime = DateTimeOffset.Now;
			}

			if (dll != null)
			{
				packageState.WPS_WDL_LoadList = dll.PK;
			}

			if (lastLocation != null)
			{
				packageState.WPS_WL_LastLocation = lastLocation.PK;
			}

			if (receiveLocation != null)
			{
				packageState.WPS_WL_ReceiveLocation = receiveLocation.PK;
			}

			if (asn != null)
			{
				packageState.WPS_WRP_ReceiveExpectedPacking = asn.PK;
			}

			packageState.WPS_IsHandlingUnit = isHandlingUnit;

			return packageState.AppendInsertAndReturnObject(sql);
		}

		public static PkgPackageExtension CreatePackageExtension(
			StringBuilder sql,
			Guid parentId,
			string parentTableCode,
			Guid packagePK,
			bool isContainerizedDTUActive
		)
		{
			var pkgPackageExtension = new PkgPackageExtension(parentId, parentTableCode, packagePK, isActive: isContainerizedDTUActive);
			return pkgPackageExtension.AppendInsertAndReturnObject(sql);
		}

		public static WhsItemPackageState CreateContainerPackageState(
			StringBuilder sql,
			Guid packagePK,
			Guid dtuPK,
			IWhsWarehouseSQL whs,
			WhsItemDispatchLoadList dll = null,
			bool isContainerizedDTUActive = true
		)
		{
			var packageState = CreateWhsItemPackageState(sql, packagePK, "BKD", whs, null, isHandlingUnit: true, dll: dll);
			CreatePackageExtension(sql, dtuPK, "WDH", packagePK, isContainerizedDTUActive);

			return packageState;
		}

		#endregion

		// Defer Trigger

		#region DeferTrigger

		/// <summary>
		/// This will Defer the Trigger (rather than disable it), via the SuspendTrigger Stored Proc (creates an App Lock)
		/// Each DeferTrigger() call requires a subsequent ResumeTrigger() call with the Same Trigger Name.
		/// </summary>
		public static void DeferTrigger(DbConnection connection, string triggerName)
		{
			connection.ExecuteNonQuery(Invariant($"EXEC dbo.SuspendTrigger '{triggerName}'"));

			// this lets us know how many times the above SuspendTrigger has been called
			var originalAppCount = GetAppLockCount();

			var dateTime = DateTime.Now;

			// Use a disposable object to make the test fail if the Trigger was never resumed.
			// As a bonus the object will be disposed at the end of the test and unhook the below event.
			MakeSureToResumeDeferredTriggers tracker = null;
			tracker = new MakeSureToResumeDeferredTriggers(() => SqlEventTracker.Instance.SqlCommandExecutedEvent -= CheckExecutedCommandsForErrors);

			SqlEventTracker.Instance.SqlCommandExecutedEvent += CheckExecutedCommandsForErrors;

			void CheckExecutedCommandsForErrors(SqlCommandExecutedEventArgs args)
			{
				// we check for the Resume Trigger command and the App Lock Check is so only one Defer Tracker is disposed at a time
				if (args.Time >= dateTime && args.Text.Contains(Invariant($"EXEC dbo.ResumeTrigger '{triggerName}'")) && (GetAppLockCount() + 1) == originalAppCount)
				{
					tracker.Dispose();
				}
			}

			int GetAppLockCount()
			{
				using (var command = connection.Command($@"
SELECT
	RequestedCount
FROM
	dbo.CheckAppLockFunctionCount_Test(@TriggerName)"))
				{
					command.AddParameter("@TriggerName", System.Data.SqlDbType.VarChar, 128, triggerName);
					var appCount = command.ExecuteScalar();
					return appCount == null ? 0 : (short)appCount;
				}
			}
		}

		sealed class MakeSureToResumeDeferredTriggers : DisposableObject
		{
			public MakeSureToResumeDeferredTriggers(Action actionOnDispose)
			{
				ActionOnDispose = actionOnDispose;
			}

			readonly Action ActionOnDispose;

			protected override void Dispose(bool isDisposing)
			{
				base.Dispose(isDisposing);
				ActionOnDispose();
			}

			public override string ToString() => nameof(MakeSureToResumeDeferredTriggers);
		}

		#endregion

		#region ResumeTrigger

		public static void ResumeTrigger(DbConnection connection, string triggerName)
		{
			connection.ExecuteNonQuery(Invariant($"EXEC dbo.ResumeTrigger '{triggerName}'"));
		}

		public static void ResumeTriggerAndRunCheckProcedure(DbConnection connection, string triggerName, string checkProcedureName, string pkColumnName)
		{
			ResumeTrigger(connection, triggerName);

			var prefix = Schema.GetPrefixFromColumnName(pkColumnName);
			var tableName = EnterpriseSchema.GetTableSchemaFromColumnNamePrefix(prefix).TableName;
			connection.ExecuteNonQuery($@"
IF EXISTS (SELECT NULL FROM sys.objects WHERE object_id = object_id(N'[dbo].[{checkProcedureName}]') AND type = 'P')
BEGIN
{ExecuteProcedureSQL(tableName, pkColumnName, checkProcedureName)}
END");
		}

		#endregion

		// Suspend Trigger

		#region SuspendTrigger

		public static IDisposable SuspendInsertAuditTrigger(string tableName, DbConnection connection = null)
		{
			return SuspendTrigger($"TG_{tableName}_AuditDetailsAreNotMissing_Insert", tableName, connection);
		}

		public static IDisposable SuspendTrigger(string triggerName, string tableName, DbConnection connection = null)
		{
			connection = connection ?? Db.Connection;
			return RethrowErrorFromSQLIfOccurredDuringUsing(new DisposableAction(
				() => connection.ExecuteNonQuery($"IF OBJECT_ID('{triggerName}', 'TR') IS NOT NULL DISABLE TRIGGER {triggerName} ON {tableName}"),
				() => connection.ExecuteNonQuery($"IF OBJECT_ID('{triggerName}', 'TR') IS NOT NULL ENABLE TRIGGER {triggerName} ON {tableName}")));
		}

		public static void SuspendTrigger_ForPreUpgradeTransformations(string triggerName, string tableName, DbConnection connection = null)
		{
			connection = connection ?? Db.Connection;
			connection.ExecuteNonQuery($"IF EXISTS (SELECT * FROM sys.objects WHERE [name] = '{triggerName}' AND [type] = 'TR') DISABLE TRIGGER {triggerName} ON {tableName}");
		}

		#endregion

		#region SuspendTriggerAndRunAtEnd

		public static IDisposable SuspendTriggerAndRunAtEnd(DbConnection connection, string triggerName, string pkColumnName, string checkProcedureName)
		{
			var prefix = Schema.GetPrefixFromColumnName(pkColumnName);
			var tableName = EnterpriseSchema.GetTableSchemaFromColumnNamePrefix(prefix).TableName;
			return SuspendTriggerAndRunAtEnd(connection, triggerName, tableName, pkColumnName, checkProcedureName);
		}

		public static IDisposable SuspendTriggerAndRunAtEnd(DbConnection connection, string triggerName, string tableName, string pkColumnName, string checkProcedureName)
		{
			return RethrowErrorFromSQLIfOccurredDuringUsing(new DisposableAction(
				() => connection.ExecuteNonQuery($"DISABLE TRIGGER {triggerName} ON {tableName};"),
				() =>
				{
					connection.ExecuteNonQuery($@"ENABLE TRIGGER {triggerName} ON {tableName};
{ExecuteProcedureSQL(tableName, pkColumnName, checkProcedureName)}
");
				}));
		}

		static string ExecuteProcedureSQL(string tableName, string pkColumnName, string checkProcedureName)
		{
			return $@"
DECLARE @PKs dbo.TVP_uniqueidentifier;
INSERT INTO @PKs
SELECT DISTINCT {pkColumnName} FROM {tableName}

EXEC {checkProcedureName} @PKs;
";
		}

		#endregion

		#region IgnoreExceptionWhenTrackingSqlErrors

		public static void IgnoreExceptionWhenTrackingSqlErrors(Func<Exception, bool> ignoreException)
		{
			var previousDelegate = IgnoreExceptionWhenSqlTracking.Value;
			IgnoreExceptionWhenSqlTracking.Value = ex => previousDelegate(ex) || ignoreException(ex);
		}

		public static void IgnoreExceptionWhenTrackingSqlErrors(Type exceptionType, string exceptionMessage)
		{
			IgnoreExceptionWhenTrackingSqlErrors(ex => ex.GetType() == exceptionType && ex.Message == exceptionMessage);
		}

		static readonly Overridable<Func<Exception, bool>> IgnoreExceptionWhenSqlTracking = new Overridable<Func<Exception, bool>>(ex => false);

		#endregion

		#region DeferrableTriggerCheckProcedure

		public static IEnumerable<Guid> AssertCheckProcedureRanAndReturnGuidsPassedIntoProcedure<T>(DbConnection connection, string checkProcedureToRun, string sqlToRun, T[] expectedObjectPksInTheProcedure)
			where T : SQLDataObject<T>
		{
			return AssertCheckProcedureRanAndReturnGuidsPassedIntoProcedure(connection, checkProcedureToRun, sqlToRun, expectedObjectPksInTheProcedure.Select(o => o.PK).ToArray());
		}

		public static IEnumerable<Guid> AssertCheckProcedureRanAndReturnGuidsPassedIntoProcedure(DbConnection connection, string checkProcedureToRun, string sqlToRun, Guid[] expectedPksInTheProcedure)
		{
			try
			{
				// mocked procedure will always throw exception if run
				using (MockDeferrableTriggerCheckProcedure(connection, checkProcedureToRun, thesePKsShouldNotRethrowSqlExceptionFromMockedProcedure: expectedPksInTheProcedure))
				using (connection.IsInTransaction ? null : connection.BeginTransactionWithManager())
				{
					connection.ExecuteNonQuery(sqlToRun);
				}

				return null;
			}
			catch (SqlException ex)
			{
				var actualPKsInTheProcedure = ex.Message.Split(new[] { ", ", System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
					.Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty)
					.Where(g => g != Guid.Empty)
					.ToArray();

				return actualPKsInTheProcedure;
			}
		}

		public static void AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(DbConnection connection, string checkProcedureName, string sqlToRun, bool commitChanges = false)
		{
			using (MockDeferrableTriggerCheckProcedureToAssertNotRun(connection, checkProcedureName))
			using (connection.IsInTransaction ? null : connection.BeginTransactionWithManager())
			{
				connection.ExecuteNonQuery(sqlToRun);

				if (commitChanges)
				{
					connection.CommitTransaction();
				}
			}
		}

		static IDisposable MockDeferrableTriggerCheckProcedureToAssertNotRun(DbConnection connection, string checkProcedureName)
		{
			return MockDeferrableTriggerCheckProcedure(connection, checkProcedureName, Enumerable.Empty<Guid>());
		}

		static IDisposable MockDeferrableTriggerCheckProcedure(DbConnection connection, string checkProcedureName, IEnumerable<Guid> thesePKsShouldNotRethrowSqlExceptionFromMockedProcedure)
		{
			var expectedPKsInExceptionMessage = thesePKsShouldNotRethrowSqlExceptionFromMockedProcedure.Select(v => v.ToString().ToUpperInvariant()).ToArray();
			if (expectedPKsInExceptionMessage.Length > 0)
			{
				IgnoreExceptionWhenTrackingSqlErrors(ex =>
				{
					var splitMessage = ex.Message.Split(new[] { ", ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToHashSet();
					return expectedPKsInExceptionMessage.All(pk => splitMessage.Contains(pk));
				});
			}

			var currentCheckProcedure = connection.ExecuteScalar($@"
SELECT
    definition
FROM
    sys.sql_modules
WHERE
    objectproperty(OBJECT_ID, 'IsProcedure') = 1
	and OBJECT_NAME(OBJECT_ID) = '{checkProcedureName}'").ToString();

			var mockedCheckProcedure = $@"
ALTER PROC {checkProcedureName} (@PKsToCheck dbo.TVP_uniqueidentifier READONLY) AS
BEGIN
	DECLARE @result varchar(max) = (select dbo.CLRCssvAgg(Value) from @PKsToCheck)
	RAISERROR(@result, 16, 1)
	RETURN 1
END";

			return RethrowErrorFromSQLIfOccurredDuringUsing(new DisposableAction(
				() => connection.ExecuteNonQuery(mockedCheckProcedure),
				() => connection.ExecuteNonQuery(currentCheckProcedure.Replace("CREATE PROC", "ALTER PROC"))));
		}

		#endregion

		// Constraint Helpers

		#region DropConstraint

		public static void DropConstraint(SqlQueryBuilder sql, string tableName, string constraintName)
		{
			sql.Append($@"
if (EXISTS (SELECT NULL FROM sys.objects WHERE type in ('C', 'D', 'F', 'PK', 'UQ') AND parent_object_id = OBJECT_ID(N{tableName.QuoteName('\'')}) AND name = N{constraintName.QuoteName('\'')}))
begin
	ALTER TABLE {tableName.QuoteName()} DROP CONSTRAINT {constraintName.QuoteName()}
end;");
		}

		#endregion

		#region DisableConstraint

		public static IDisposable DisableConstraint(string tableName, string constraintName, DbConnection connection = null)
		{
			var dbConn = connection ?? Db.Connection;
			return RethrowErrorFromSQLIfOccurredDuringUsing(new DisposableAction(
				 () => dbConn.ExecuteNonQuery($"IF OBJECT_ID('{constraintName}', 'C') IS NOT NULL ALTER TABLE {tableName} NOCHECK CONSTRAINT[{constraintName}]"),
				 () => dbConn.ExecuteNonQuery($"IF OBJECT_ID('{constraintName}', 'C') IS NOT NULL ALTER TABLE {tableName} CHECK CONSTRAINT[{constraintName}]")));
		}

		#endregion

		// Current Check Procedure and Trigger Names

		public const string WhsCheckInterWhsTransfersAreInSync = "WhsCheckInterWhsTransfersAreInSync";
		public const string WhsCheckTransactionAndPickQtyIsCorrect = "WhsCheckTransactionAndPickQtyIsCorrect_V3";
		public const string WhsCheckTransactionAndPickQtyIsCorrect_ForInsert = nameof(WhsCheckTransactionAndPickQtyIsCorrect_ForInsert);
		public const string WhsCheckStockOnHandIsBalanced = "WhsCheckStockOnHandIsBalanced";
		public const string WhsCheckStockOnHandIsBalanced_ForInsert = nameof(WhsCheckStockOnHandIsBalanced_ForInsert);
		public const string WhsCheckDocketStatusAndDateWithLines = "WhsCheckDocketStatusAndDateWithLines";
		public const string WhsCheckReleaseCaptureAttributesAreNotOverCommitting = "WhsCheckReleaseCaptureAttributesAreNotOverCommitting";
		public const string PkgHandlingUnitDivotCheckHaveSameTopLevelHandlingUnit = nameof(PkgHandlingUnitDivotCheckHaveSameTopLevelHandlingUnit);
		public const string PkgCheckHUAndChildPackageHaveSameTopLevelHandlingUnit = nameof(PkgCheckHUAndChildPackageHaveSameTopLevelHandlingUnit);

		public const string TG_PreventMismatchOnDocketStatusAndDateWithLines = nameof(TG_PreventMismatchOnDocketStatusAndDateWithLines);

		public const string TG_WhsDocket_AuditDetailsAreNotMissing_Insert = nameof(TG_WhsDocket_AuditDetailsAreNotMissing_Insert);

		public const string TG_WhsDocketLine_EnsureInterWhsChildLinesAreSynchronised = nameof(TG_WhsDocketLine_EnsureInterWhsChildLinesAreSynchronised);
		public const string TG_WhsDocketLine_LocationIsInCorrectWarehouse = nameof(TG_WhsDocketLine_LocationIsInCorrectWarehouse);
		public const string TG_WhsDocketLine_StockOnHandIsBalanced = nameof(TG_WhsDocketLine_StockOnHandIsBalanced);
		public const string TG_WhsDocketLine_StockOnHandIsBalanced_Insert = nameof(TG_WhsDocketLine_StockOnHandIsBalanced_Insert);
		public const string TG_WhsDocketLine_StockOnHandIsBalanced_InsertForParent = nameof(TG_WhsDocketLine_StockOnHandIsBalanced_InsertForParent);
		public const string TG_CheckDocketLineStatusAndDateForDocketLine = nameof(TG_CheckDocketLineStatusAndDateForDocketLine);
		public const string TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect = nameof(TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect);
		public const string TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert = nameof(TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert);
		public const string TG_WhsDocketLine_AuditDetailsAreNotMissing_Insert = nameof(TG_WhsDocketLine_AuditDetailsAreNotMissing_Insert);

		public const string TG_WhsPickLine_StockOnHandIsBalanced = nameof(TG_WhsPickLine_StockOnHandIsBalanced);
		public const string TG_WhsPickLine_TransactionAndPickedQtyIsCorrect = nameof(TG_WhsPickLine_TransactionAndPickedQtyIsCorrect);
		public const string TG_WhsPickLine_LinkedToCorrectTransactionLine = nameof(TG_WhsPickLine_LinkedToCorrectTransactionLine);
		public const string TG_PreventOverCommitOfStockViaPickLine = nameof(TG_PreventOverCommitOfStockViaPickLine);

		public const string TG_WhsReleaseCaptureAttributes_PreventOverReleaseOfPickedStockOrNonPickedStock = nameof(TG_WhsReleaseCaptureAttributes_PreventOverReleaseOfPickedStockOrNonPickedStock);

		public const string TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit = nameof(TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit);
		public const string TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit = nameof(TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit);

		public const string TG_WhsPutawayLine_PreventUnfinalizeIfJobIsFinalized = nameof(TG_WhsPutawayLine_PreventUnfinalizeIfJobIsFinalized);
		public const string TG_WhsPutawayJob_PreventFinalizeIfLinesAreUnfinalized = nameof(TG_WhsPutawayJob_PreventFinalizeIfLinesAreUnfinalized);
		public const string WhsCheckPreventFinalizeIfLinesAreUnfinalized = nameof(WhsCheckPreventFinalizeIfLinesAreUnfinalized);
		public const string TG_PreventAddingIncorrectOrdersToPicks = nameof(TG_PreventAddingIncorrectOrdersToPicks);
		public const string TG_WhsDockDoorAssignment_EnsureDockDoorAssignmentsAreReferenced_ByAPick = nameof(TG_WhsDockDoorAssignment_EnsureDockDoorAssignmentsAreReferenced_ByAPick);

		public const string TG_WhsPick_PackingStationIsValid = nameof(TG_WhsPick_PackingStationIsValid);
		public const string WhsCheckPickPackingStationCorrect = nameof(WhsCheckPickPackingStationCorrect);

		//

		#region Implementation

		static IDisposable RethrowErrorFromSQLIfOccurredDuringUsing(IDisposable disposable)
		{
			var dateTime = DateTime.Now;
			SqlCommandExecutedEventArgs eventArgsForErrorOnSqlCommand = null;

			SqlEventTracker.Instance.SqlCommandExecutedEvent += CheckExecutedCommandsForErrors;

			return new DisposableAction(() =>
			{
				SqlEventTracker.Instance.SqlCommandExecutedEvent -= CheckExecutedCommandsForErrors;

				if (eventArgsForErrorOnSqlCommand != null)
				{
					// no point re-throwing the same exception
					if (GetCurrentExceptionBeingThrown.Value?.Invoke()?.InnerException != eventArgsForErrorOnSqlCommand.Exception)
					{
						var errorMessage = $@"Error occured in test for attempted SQL Command, Error below:
{eventArgsForErrorOnSqlCommand.Exception.Message}

If you were expecting this Error in your test, most likely the scope of your deferral of Constraints/Triggers encompasses too much code
e.g. You are disabling a Constraint in Test Setup() to be re-enabled in Test TearDown().
Otherwise if you can't reduce the scope then make sure to specify that you want to ignore the exception using TestWhsDataSetupHelper.IgnoreExceptionWhenTrackingSqlErrors().

Command Text was:
{eventArgsForErrorOnSqlCommand.Text}";

						var exceptionToThrow = new TestSqlStatementFailedException(errorMessage, eventArgsForErrorOnSqlCommand.Exception);
						GetCurrentExceptionBeingThrown.Value = () => exceptionToThrow;
						throw exceptionToThrow;
					}
				}
				else
				{
					disposable.Dispose();
				}
			});

			void CheckExecutedCommandsForErrors(SqlCommandExecutedEventArgs args)
			{
				if (args.Time >= dateTime && args.Exception != null && !IgnoreExceptionWhenSqlTracking.Value(args.Exception))
				{
					SqlEventTracker.Instance.SqlCommandExecutedEvent -= CheckExecutedCommandsForErrors;
					eventArgsForErrorOnSqlCommand = args;
				}
			}
		}

		static readonly Overridable<Func<TestSqlStatementFailedException>> GetCurrentExceptionBeingThrown = new Overridable<Func<TestSqlStatementFailedException>>(null);

		[Serializable]
		public class TestSqlStatementFailedException : Exception
		{
			public TestSqlStatementFailedException()
			{
			}

			public TestSqlStatementFailedException(string message)
				: base(message)
			{
			}

			public TestSqlStatementFailedException(string message, Exception inner)
				: base(message, inner)
			{
			}

#if NETFRAMEWORK
			protected TestSqlStatementFailedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		#endregion
	}
}
