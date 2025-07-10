using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ClientSpecific.EDI
{
	[UseSnapshotProtection]
	[TestedType(typeof(RenameColumn_AddUTCSuffixForIncidentMain_ResolvedTimeAndClosedDate))]
	public class RenameColumn_AddUTCSuffixForIncidentMain_ResolvedTimeAndClosedDateTest : DataTransformationTestCase
	{
		readonly string tableName = "IncidentMain";

		readonly string oldResolvedTimeColumnName = "IM_ResolvedTime";

		readonly string newCloseTimeColumnName = "IM_CloseTimeUtc";

		readonly string newResolveTimeColumnName = "IM_ResolveTimeUtc";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1122:Do Not Use DateTime Parse Method", Justification = "Baseline")]
		void AssertTransformationResults(string oldColumnName, string newColumnName)
		{
			CombineAssertions(() =>
			{
				var conn = Db.Connection;
				AssertEquals("Old column does not exist", false, DbObjectCreator.ColumnExists(Db.Connection, tableName, oldColumnName));
				AssertEquals("New column exists", true, DbObjectCreator.ColumnExists(Db.Connection, tableName, newColumnName));

				AssertEquals("Data is still there", DateTime.Parse("2023-06-01 12:00:00"), conn.ExecuteScalar<DateTime>($"SELECT {newCloseTimeColumnName} FROM dbo.IncidentMain WHERE IM_IncidentNumber = 'CS00001000'"));
				AssertEquals("Data is still there", System.DBNull.Value, conn.ExecuteScalar($"SELECT {newCloseTimeColumnName} FROM dbo.IncidentMain WHERE IM_IncidentNumber = 'CS00001111'"));

				AssertTransformationResults(oldResolvedTimeColumnName, newResolveTimeColumnName);
				AssertEquals("Data is still there", DateTime.Parse("2026-06-01 12:30:00"), conn.ExecuteScalar<DateTime>($"SELECT {newResolveTimeColumnName} FROM dbo.IncidentMain WHERE IM_IncidentNumber = 'CS00001000'"));
				AssertEquals("Data is still there", System.DBNull.Value, conn.ExecuteScalar($"SELECT {newResolveTimeColumnName} FROM dbo.IncidentMain WHERE IM_IncidentNumber = 'CS00001111'"));
			});
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RenameColumn_AddUTCSuffixForIncidentMain_ResolvedTimeAndClosedDate();
		}

		protected override void SetUp()
		{
			base.SetUp();
			disposableAdminConnection = ((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade();
		}

		protected override sealed void TearDown()
		{
			disposableAdminConnection.Dispose();
			base.TearDown();
		}

		IDisposable disposableAdminConnection;
	}
}
