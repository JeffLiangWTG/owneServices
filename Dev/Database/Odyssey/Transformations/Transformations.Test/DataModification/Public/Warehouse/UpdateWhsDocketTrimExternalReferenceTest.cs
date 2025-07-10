using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.DataModification.Public.Warehouse
{
	[TestedType(typeof(UpdateWhsDocketTrimExternalReferenceLeadingWhitespace))]
	class UpdateWhsDocketTrimExternalReferenceLeadingWhitespaceTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1", " With Leading Whitespace")
			{
				WD_ExternalReferenceSplit = 1
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		protected override void AssertTransformationResults()
		{
			WhsDocket.ShallowLoadFromDB(TestConnection).Single()
				.BuildAssertion(TestConnection)
				.ExpectEquals("WD_ExternalReference", d => d.WD_ExternalReference, "With Leading Whitespace")
				.ExpectEquals("WD_DocketType", d => d.WD_DocketType, "INW")
				.ExpectEquals("WD_ExternalReferenceSplit ", d => d.WD_ExternalReferenceSplit, 1)
				.ExpectHasNoLogs()
				.VerifyAll("External Reference, Docket Type and External Reference Split correct");
		}

		public void TestTransformation_NoDockets()
		{
			AssertNoExceptionThrown(() => GetNewTestTransformationInstance().Run());
		}

		public void TestTransformation_NoTrimmingRequired()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			var docket = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1", "Sample").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GetNewTestTransformationInstance().Run();

			WhsDocket.ShallowLoadFromDB(TestConnection, docket.PK)
				.BuildAssertion(TestConnection)
				.ExpectEquals("WD_ExternalReference", d => d.WD_ExternalReference, "Sample")
				.VerifyAll();
		}

		public void TestTransformation_NoTrimmingRequired_ForDuplicateEntries()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var docket1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1", "Sample").InsertAndReturnObject(TestConnection);
			var docket2 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R2", "    Sample").InsertAndReturnObject(TestConnection);

			GetNewTestTransformationInstance().Run();

			WhsDocket.ShallowLoadFromDB(TestConnection, docket1.PK)
				.BuildAssertion(TestConnection)
				.ExpectEquals("WD_ExternalReference", d => d.WD_ExternalReference, "Sample")
				.VerifyAll();

			WhsDocket.ShallowLoadFromDB(TestConnection, docket2.PK)
				.BuildAssertion(TestConnection)
				.ExpectEquals("WD_ExternalReference", d => d.WD_ExternalReference, "    Sample")
				.VerifyAll();
		}

		public void TestTransformation_NoTrimmingRequired_DuplicateIfTrimmed()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var docket1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1", " Sample").InsertAndReturnObject(TestConnection);
			var docket2 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R2", "    Sample").InsertAndReturnObject(TestConnection);

			GetNewTestTransformationInstance().Run();

			WhsDocket.ShallowLoadFromDB(TestConnection, docket1.PK)
				.BuildAssertion(TestConnection)
				.ExpectEquals("WD_ExternalReference", d => d.WD_ExternalReference, " Sample")
				.VerifyAll();

			WhsDocket.ShallowLoadFromDB(TestConnection, docket2.PK)
				.BuildAssertion(TestConnection)
				.ExpectEquals("WD_ExternalReference", d => d.WD_ExternalReference, "    Sample")
				.VerifyAll();
		}

		public void TestTransformation_NoIndexCollision()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(sql);
			var client1 = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);
			var client2 = new OrgHeader("CL2").AppendInsertAndReturnObject(sql);

			var docket1 = new WhsDocket(client1.PK, whs.PK, "INW", "REC", "ENT", "R1", "REF").AppendInsertAndReturnObject(sql);
			var docket2 = new WhsDocket(client2.PK, whs.PK, "INW", "REC", "ENT", "R2", "    REF").AppendInsertAndReturnObject(sql);
			var docket3 = new WhsDocket(client1.PK, whs.PK, "ORD", "ORD", "ENT", "R3", "    REF").AppendInsertAndReturnObject(sql);
			var docket4 = new WhsDocket(client1.PK, whs.PK, "INW", "REC", "ENT", "R4", "    REF")
			{
				WD_ExternalReferenceSplit = 5,
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GetNewTestTransformationInstance().Run();

			WhsDocket.ShallowLoadFromDB(TestConnection, docket1.PK)
				.BuildAssertion(TestConnection)
				.ExpectEquals("WD_ExternalReference", d => d.WD_ExternalReference, "REF")
				.VerifyAll();

			WhsDocket.ShallowLoadFromDB(TestConnection, docket2.PK)
				.BuildAssertion(TestConnection)
				.ExpectEquals("WD_ExternalReference", d => d.WD_ExternalReference, "REF")
				.VerifyAll();

			WhsDocket.ShallowLoadFromDB(TestConnection, docket3.PK)
				.BuildAssertion(TestConnection)
				.ExpectEquals("WD_ExternalReference", d => d.WD_ExternalReference, "REF")
				.VerifyAll();

			WhsDocket.ShallowLoadFromDB(TestConnection, docket4.PK)
				.BuildAssertion(TestConnection)
				.ExpectEquals("WD_ExternalReference", d => d.WD_ExternalReference, "REF")
				.VerifyAll();
		}

		public void TestTransformation_MultiBatch()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			var pks = new List<Guid>();

			for (var i = 0; i < 5050; i++)
			{
				var docket = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", $"R{i}", $" ExtRef_{i}").AppendInsertAndReturnObject(sql);
				pks.Add(docket.PK);
			}

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var logger = new StringBuilder();
			((IOnlineTransformation)GetNewTestTransformationInstance()).Run(m => logger.AppendLine(m), CancellationToken.None);

			var dockets = WhsDocket.ShallowLoadFromDB(TestConnection);
			AssertEquals("All Dockets should still exist.", 5050, dockets.Length);
			AssertEquals("No docket should have leading whitespace.", 0, dockets.Where(d => char.IsWhiteSpace(d.WD_ExternalReference.First())).Count());

			AssertStartsWith("Logged messages should be correct.", $@"Trimmed External Reference for 1000 rows.
Trimmed External Reference for 1000 rows.
Trimmed External Reference for 1000 rows.
Trimmed External Reference for 1000 rows.
Trimmed External Reference for 1000 rows.
Trimmed External Reference for 50 rows.
", logger.ToString());
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateWhsDocketTrimExternalReferenceLeadingWhitespace();
	}
}
