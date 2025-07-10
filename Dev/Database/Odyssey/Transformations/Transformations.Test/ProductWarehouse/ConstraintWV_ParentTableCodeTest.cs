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
	[TestedType(typeof(ConstraintWV_ParentTableCode))]
	sealed class ConstraintWV_ParentTableCodeTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new ConstraintWV_ParentTableCode();

		protected override void PrepareTestData()
		{
			Db.Connection.ExecuteNonQuery("ALTER TABLE WhsDocketJobPivot DROP CONSTRAINT IF EXISTS Constraint_WV_ParentTableCode");

			var sql = new SqlQueryBuilder();

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(sql);
			var docket = new WhsDocket(client.PK, warehouse.PK, "INW", "REC", "NEW", "DOC1").AppendInsertAndReturnObject(sql);

			new WhsDocketJobPivot(docket, "INW", Guid.NewGuid(), "CH").AppendInsertAndReturnObject(sql);
			new WhsDocketJobPivot(docket, "INW", Guid.NewGuid(), "BH").AppendInsertAndReturnObject(sql);
			new WhsDocketJobPivot(docket, "INW", Guid.NewGuid(), "JE").AppendInsertAndReturnObject(sql);
			new WhsDocketJobPivot(docket, "INW", Guid.NewGuid(), "JS").AppendInsertAndReturnObject(sql);
			new WhsDocketJobPivot(docket, "INW", Guid.NewGuid(), "JD").AppendInsertAndReturnObject(sql);

			new WhsDocketJobPivot(docket, "INW", Guid.NewGuid(), "VV").AppendInsertAndReturnObject(sql);
			new WhsDocketJobPivot(docket, "INW", Guid.NewGuid(), "ZZZ").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals("Invalid ParentTableCodes are deleted", 5, WhsDocketJobPivot.CountInDB(TestConnection));
			AssertEquals(1, WhsDocketJobPivot.CountInDB(TestConnection, row => row.WV_ParentTableCode == "CH"));
			AssertEquals(1, WhsDocketJobPivot.CountInDB(TestConnection, row => row.WV_ParentTableCode == "BH"));
			AssertEquals(1, WhsDocketJobPivot.CountInDB(TestConnection, row => row.WV_ParentTableCode == "JE"));
			AssertEquals(1, WhsDocketJobPivot.CountInDB(TestConnection, row => row.WV_ParentTableCode == "JS"));
			AssertEquals(1, WhsDocketJobPivot.CountInDB(TestConnection, row => row.WV_ParentTableCode == "JD"));
		}

		public void TestIsOfflinePostUpgrade()
		{
			PrepareTestData();
			AssertEquals("Precondition", 7, WhsDocketJobPivot.CountInDB(TestConnection));

			var transform = GetNewTestTransformationInstance();

			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			AssertEquals("No modification", 7, WhsDocketJobPivot.CountInDB(TestConnection));

			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			AssertEquals("No modification", 7, WhsDocketJobPivot.CountInDB(TestConnection));

			transform.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertEquals("Invalid data deleted", 5, WhsDocketJobPivot.CountInDB(TestConnection));

			AssertEquals(1, WhsDocketJobPivot.CountInDB(TestConnection, row => row.WV_ParentTableCode == "CH"));
			AssertEquals(1, WhsDocketJobPivot.CountInDB(TestConnection, row => row.WV_ParentTableCode == "BH"));
			AssertEquals(1, WhsDocketJobPivot.CountInDB(TestConnection, row => row.WV_ParentTableCode == "JE"));
			AssertEquals(1, WhsDocketJobPivot.CountInDB(TestConnection, row => row.WV_ParentTableCode == "JS"));
			AssertEquals(1, WhsDocketJobPivot.CountInDB(TestConnection, row => row.WV_ParentTableCode == "JD"));
		}

		public override string[] expectedIndex =>
		[
			"NONCLUSTERED INDEX [IX_ConstraintWV_ParentTableCode_Invalid_ParentTableCode] ON [dbo].[WhsDocketJobPivot] ([WV_ParentTableCode]) INCLUDE ([WV_SystemCreateTimeUtc], [WV_SystemLastEditTimeUtc]) WHERE ([WV_ParentTableCode]<>'BH' AND [WV_ParentTableCode]<>'CH' AND [WV_ParentTableCode]<>'JD' AND [WV_ParentTableCode]<>'JE' AND [WV_ParentTableCode]<>'JS') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
		];
	}
}
