using System;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ProductWarehouse
{
	[TestedType(typeof(ConstraintSDP_SubjectTableCode))]
	sealed class ConstraintSDP_SubjectTableCodeTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new ConstraintSDP_SubjectTableCode();

		protected override void PrepareTestData()
		{
			Db.Connection.ExecuteNonQuery("ALTER TABLE StmDefaultPrinter DROP CONSTRAINT IF EXISTS Constraint_SDP_SubjectTableCode");

			var sql = new SqlQueryBuilder();

			var printServer = new StmPrintServer("Server").AppendInsertAndReturnObject(sql);
			var printQueue = new StmPrintQueue(printServer).AppendInsertAndReturnObject(sql);

			new StmDefaultPrinter(printQueue, Guid.NewGuid(), "GS").AppendInsertAndReturnObject(sql);
			new StmDefaultPrinter(printQueue, Guid.NewGuid(), "KP").AppendInsertAndReturnObject(sql);
			new StmDefaultPrinter(printQueue, Guid.NewGuid(), "WA").AppendInsertAndReturnObject(sql);
			new StmDefaultPrinter(printQueue, Guid.NewGuid(), "WW").AppendInsertAndReturnObject(sql);

			new StmDefaultPrinter(printQueue, Guid.NewGuid(), "ZZZ").AppendInsertAndReturnObject(sql);
			new StmDefaultPrinter(printQueue, Guid.NewGuid(), "VV").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals("Invalid SubjectTableCodes are deleted", 4, StmDefaultPrinter.CountInDB(TestConnection));
			AssertEquals(1, StmDefaultPrinter.CountInDB(TestConnection, pick => pick.SDP_SubjectTableCode == "GS"));
			AssertEquals(1, StmDefaultPrinter.CountInDB(TestConnection, pick => pick.SDP_SubjectTableCode == "KP"));
			AssertEquals(1, StmDefaultPrinter.CountInDB(TestConnection, pick => pick.SDP_SubjectTableCode == "WA"));
			AssertEquals(1, StmDefaultPrinter.CountInDB(TestConnection, pick => pick.SDP_SubjectTableCode == "WW"));
			AssertEquals("Invalid SubjectTableCodes are deleted", 0, StmDefaultPrinter.CountInDB(TestConnection, pick => pick.SDP_SubjectTableCode == "ZZZ"));
			AssertEquals("Invalid SubjectTableCodes are deleted", 0, StmDefaultPrinter.CountInDB(TestConnection, pick => pick.SDP_SubjectTableCode == "VV"));
		}

		public void TestIsOfflinePostUpgrade()
		{
			PrepareTestData();
			AssertEquals("Precondition", 6, StmDefaultPrinter.CountInDB(TestConnection));
			AssertEquals("Precondition", 1, StmDefaultPrinter.CountInDB(TestConnection, pick => pick.SDP_SubjectTableCode == "GS"));
			AssertEquals("Precondition", 1, StmDefaultPrinter.CountInDB(TestConnection, pick => pick.SDP_SubjectTableCode == "KP"));
			AssertEquals("Precondition", 1, StmDefaultPrinter.CountInDB(TestConnection, pick => pick.SDP_SubjectTableCode == "WA"));
			AssertEquals("Precondition", 1, StmDefaultPrinter.CountInDB(TestConnection, pick => pick.SDP_SubjectTableCode == "WW"));

			var transform = GetNewTestTransformationInstance();

			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			AssertEquals("No modification", 6, StmDefaultPrinter.CountInDB(TestConnection));

			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			AssertEquals("No modification", 6, StmDefaultPrinter.CountInDB(TestConnection));

			transform.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertEquals("Invalid data deleted", 4, StmDefaultPrinter.CountInDB(TestConnection));
			AssertEquals("Valid data still exists", 1, StmDefaultPrinter.CountInDB(TestConnection, pick => pick.SDP_SubjectTableCode == "GS"));
			AssertEquals("Valid data still exists", 1, StmDefaultPrinter.CountInDB(TestConnection, pick => pick.SDP_SubjectTableCode == "KP"));
			AssertEquals("Valid data still exists", 1, StmDefaultPrinter.CountInDB(TestConnection, pick => pick.SDP_SubjectTableCode == "WA"));
			AssertEquals("Valid data still exists", 1, StmDefaultPrinter.CountInDB(TestConnection, pick => pick.SDP_SubjectTableCode == "WW"));
		}

		public override string[] expectedIndex =>
		[
			"NONCLUSTERED INDEX [IX_ConstraintSDP_SubjectTableCode_Invalid_SubjectTableCode] ON [dbo].[StmDefaultPrinter] ([SDP_SubjectTableCode]) INCLUDE ([SDP_SystemCreateTimeUtc], [SDP_SystemLastEditTimeUtc]) WHERE ([SDP_SubjectTableCode]<>'GS' AND [SDP_SubjectTableCode]<>'KP' AND [SDP_SubjectTableCode]<>'WA' AND [SDP_SubjectTableCode]<>'WW') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
		];
	}
}
