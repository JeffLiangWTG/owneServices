using System;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ProductWarehouse
{
	[TestedType(typeof(RemoveGenAddonColumnShortfallStatus))]
	sealed class RemoveGenAddonColumnShortfallStatusTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new RemoveGenAddonColumnShortfallStatus();

		protected override void PrepareTestData()
		{
			var sql = new SqlQueryBuilder();
			new GenAddOnColumn("PrioritizedShortfallStatusCode", "STR", "YSF", "WD", Guid.NewGuid()).AppendInsertAndReturnObject(sql);
			new GenAddOnColumn("PrioritizedShortfallStatusCode", "STR", "NSF", "WD", Guid.NewGuid()).AppendInsertAndReturnObject(sql);
			new GenAddOnColumn("PrioritizedShortfallStatusCode", "STR", "USF", "WD", Guid.NewGuid()).AppendInsertAndReturnObject(sql);
			new GenAddOnColumn("PrioritizedShortfallStatusCode", "STR", "SomeOtherVal", "WD", Guid.NewGuid()).AppendInsertAndReturnObject(sql);

			new GenAddOnColumn("SomeOtherCol", "STR", "YSF", "WD", Guid.NewGuid()).AppendInsertAndReturnObject(sql);
			new GenAddOnColumn("PrioritizedShortfallStatusCode", "STR", "YSF", "WW", Guid.NewGuid()).AppendInsertAndReturnObject(sql);
			new GenAddOnColumn("PrioritizedShortfallStatusCode", "INT", "66", "WD", Guid.NewGuid()).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(0, GenAddOnColumn.CountInDB(TestConnection, genCol => genCol.XA_Name == "PrioritizedShortfallStatusCode" && genCol.XA_Type == "STR" && genCol.XA_ParentTableCode == "WD"));

			AssertEquals(1, GenAddOnColumn.CountInDB(TestConnection, genCol => genCol.XA_Name == "SomeOtherCol" && genCol.XA_Type == "STR" && genCol.XA_ParentTableCode == "WD"));
			AssertEquals(1, GenAddOnColumn.CountInDB(TestConnection, genCol => genCol.XA_Name == "PrioritizedShortfallStatusCode" && genCol.XA_Type == "STR" && genCol.XA_ParentTableCode == "WW"));
			AssertEquals(1, GenAddOnColumn.CountInDB(TestConnection, genCol => genCol.XA_Name == "PrioritizedShortfallStatusCode" && genCol.XA_Type == "INT" && genCol.XA_ParentTableCode == "WD"));
		}

		public void TestBulkDelete()
		{
			var genAddOnColumns = new GenAddOnColumn[2500];
			for (var i = 0; i < genAddOnColumns.Length; i++)
			{
				genAddOnColumns[i] = new GenAddOnColumn("PrioritizedShortfallStatusCode", "STR", "YSF", "WD", Guid.NewGuid());
			}
			var sql = new SqlQueryBuilder();
			sql.AppendLine(GenAddOnColumn.GetBulkInsertStatement(genAddOnColumns));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, GenAddOnColumn.CountInDB(TestConnection, genCol => genCol.XA_Name == "PrioritizedShortfallStatusCode" && genCol.XA_Type == "STR" && genCol.XA_ParentTableCode == "WD"));
		}
	}
}
