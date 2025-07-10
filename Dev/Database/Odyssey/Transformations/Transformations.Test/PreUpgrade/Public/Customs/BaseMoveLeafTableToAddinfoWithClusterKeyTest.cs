using System;
using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.PreUpgrade.Public.Customs.Testing
{
	[TestedType(typeof(BaseMoveLeafTableToAddinfoForTesting))]
	sealed class BaseMoveLeafTableToAddinfoWithClusterKeyTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new BaseMoveLeafTableToAddinfoForTesting();

		const string TestMainTableName = "BaseMoveLeafTableToAddinfoWithClusterKeyTest_Main";
		const string TestMainTablePrefix = "M";
		const string TestLeafTableName = "BaseMoveLeafTableToAddinfoWithClusterKeyTest_Leaf";
		const string TestLeafTablePrefix = "L";

		class BaseMoveLeafTableToAddinfoForTesting : BaseMoveLeafTableToAddInfo
		{
			protected override string MainTableName => TestMainTableName;

			protected override string MainTablePrefix => TestMainTablePrefix;

			protected override string LeafTableName => TestLeafTableName;

			protected override string LeafTablePrefix => TestLeafTablePrefix;

			protected override bool UseClusterKeys => true;

			protected override IEnumerable<(string addinfoName, string columnName, SchemaColumnType columnType)> Conversions()
			{
				yield return ("String1", $"L_String1", SchemaColumnType.String);
			}
		}

		protected override void PrepareTestData()
		{
			TestConnection.ExecuteNonQuery($@"
CREATE TABLE {TestMainTableName} (
	M_PK uniqueidentifier NOT NULL,
	M_ClusterKey int NOT NULL,
	M_AddInfo varchar(4096) NOT NULL,
	M_SystemLastEditTimeUtc SMALLDATETIME NULL,
	M_SystemLastEditUser VARCHAR(3) NULL);
CREATE TABLE {TestLeafTableName} (
	L_PK uniqueidentifier NOT NULL,
	L_ClusterKey int NOT NULL,
	L_String1 varchar(max),
	L_M uniqueidentifier NOT NULL);
INSERT INTO {TestMainTableName} (M_PK,    M_ClusterKey, M_AddInfo) VALUES
                                ('{pk1}', 1,            'OtherValue=other');
INSERT INTO {TestLeafTableName} (L_PK,    L_ClusterKey, L_M,     L_String1) VALUES
                                (NEWID(), 1,            '{pk1}', 'value1');
");
		}

		protected override void AssertTransformationResults()
		{
			var testHelper = new AddInfoTestHelper(TestConnection, TestMainTableName, TestMainTablePrefix);
			CombineAssertions(() =>
			{
				testHelper.AssertFieldValue("String value", pk1, "String1", "value1");
				testHelper.AssertFieldValue("Existing AddInfo value", pk1, "OtherValue", "other");
				testHelper.AssertTableEmpty(TestLeafTableName);
			});
		}

		readonly Guid pk1 = Guid.NewGuid();
	}
}
