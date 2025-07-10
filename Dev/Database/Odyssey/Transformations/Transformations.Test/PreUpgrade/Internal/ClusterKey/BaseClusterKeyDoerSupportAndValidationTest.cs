using System;
using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Testing
{
	abstract class BaseClusterKeyDoerSupportAndValidationTest : TransactionedTestCase
	{
		public void TestClusterKeyColumnsAndSupportingIndexesAreCreated()
		{
			var definitions = new List<KeyDefinition>()
			{
				new KeyDefinition(DummyBizoSchema.PK, DummyDependentBizoSchema.ZD1_Z0, isMidLevelMaster: true),
				new KeyDefinition(DummyBizoSchema.PK, DummyPivotSchema.ZDP_Z0),
			};

			CombineAssertions("[PRE-CONDITIONS]", () =>
			{
				AssertTopLevelTableClusterKeyColumnAndIndexExist(DummyBizoSchema.Instance, false);

				foreach (var ckDefinition in definitions)
				{
					AssertWorkerClusterKeyColumnAndIndexesExist(ckDefinition, false);
				}
			});

			CallClusterKeyDoer(DummyBizoSchema.PK, definitions);

			CombineAssertions("[PRE-CONDITIONS]", () =>
			{
				AssertTopLevelTableClusterKeyColumnAndIndexExist(DummyBizoSchema.Instance, true);

				foreach (var ckDefinition in definitions)
				{
					AssertWorkerClusterKeyColumnAndIndexesExist(ckDefinition, true);
				}
			});
		}

		void AssertTopLevelTableClusterKeyColumnAndIndexExist(ITableSchema table, bool expected)
		{
			var tableName = table.TableName;
			var ckColumn = table.PK.ColumnPrefix + CargoWise.Schema.Schema.ClusterKeyColumnSuffix;
			var ckIndex = "IX_" + ckColumn;

			AssertEquals($"[{tableName}].[{ckColumn}] column exists?", expected, DbObjectCreator.ColumnExists(TestConnection, tableName, ckColumn));
			AssertEquals($"[{tableName}].[{ckIndex}] index exists?", expected, DbObjectCreator.IndexExists(TestConnection, tableName, ckIndex));
		}

		void AssertWorkerClusterKeyColumnAndIndexesExist(KeyDefinition ckDefinition, bool expected)
		{
			var childTableName = ckDefinition.ChildTableName;
			var childCkColumn = ckDefinition.ChildClusterKey;
			var childFkColumn = ckDefinition.ChildFkColumnName;
			var childTableIndex = $"IX_{childFkColumn}_Zero_{childCkColumn}";

			var parentTableName = ckDefinition.ParentTableName;
			var parentPkColumn = ckDefinition.ParentPkColumnName;
			var parentTableIndex = $"IX_{parentPkColumn}_NonZero_{ckDefinition.ParentClusterKey}";

			AssertEquals($"[{childTableName}].[{childCkColumn}] column exists?", expected, DbObjectCreator.ColumnExists(TestConnection, childTableName, childCkColumn));
			AssertEquals($"[{childTableName}].[{childTableIndex}] index exists?", expected, DbObjectCreator.IndexExists(TestConnection, childTableName, childTableIndex));
			AssertEquals($"[{parentTableName}].[{parentTableIndex}] index exists?", expected, DbObjectCreator.IndexExists(TestConnection, parentTableName, parentTableIndex));

			if (ckDefinition.IsMidLevelMaster)
			{
				var noParentCkIndex = $"IX_{childCkColumn}_Null_{childFkColumn}";
				AssertEquals($"[{childTableName}].[{noParentCkIndex}] index exists?", expected, DbObjectCreator.IndexExists(TestConnection, childTableName, noParentCkIndex));
			}
		}

		public void TestClusterKeyDoerThrowsExceptionIfKeyDefinitionsContainDuplicatedChildTable()
		{
			var definitions = new List<KeyDefinition>()
			{
				new KeyDefinition(JobDeclarationSchema.PK, CusEntryInstructionSchema.CEI_JE),
				new KeyDefinition(JobDeclarationSchema.PK, CusEntryHeaderSchema.CH_JE),
				new KeyDefinition(CusEntryHeaderSchema.PK, CusEntryLineSchema.CL_CH),
				new KeyDefinition(CusEntryInstructionSchema.PK, CusEntryHeaderSchema.CH_CEI_Instruction),
				new KeyDefinition(OrgHeaderSchema.PK, CusEntryInstructionSchema.CEI_OH_Owner),
			};

			AssertExceptionThrown(
				"Calling ClusterKeyDoer.Do with duplicated child tables.",
				typeof(ArgumentException),
				"The following cluster key child tables are duplicated:\r\n\t[CusEntryInstruction]\r\n\t[CusEntryHeader]",
				() => CallClusterKeyDoer(JobDeclarationSchema.PK, definitions)
			);
		}

		public void TestClusterKeyDoerThrowsExceptionIfKeyDefinitionsContainCircularReference()
		{
			var definitions = new List<KeyDefinition>()
			{
				new KeyDefinition(JobDeclarationSchema.PK, CusEntryInstructionSchema.CEI_JE),
				new KeyDefinition(JobDeclarationSchema.PK, CusEntryHeaderSchema.CH_JE),
				new KeyDefinition(CusEntryHeaderSchema.PK, CusEntryLineSchema.CL_CH),
				new KeyDefinition(CusEntryLineSchema.PK, JobDeclarationSchema.PK),
			};

			AssertExceptionThrown(
				"Calling ClusterKeyDoer.Do with circular reference in key definitions.",
				typeof(ArgumentException),
				"Cluster Key Definition [CusEntryLine/JobDeclaration] causes a circular reference.",
				() => CallClusterKeyDoer(JobDeclarationSchema.PK, definitions)
			);
		}

		public void TestClusterKeyDoerThrowsExceptionIfKeyDefinitionsContainGaps()
		{
			var definitions = new List<KeyDefinition>()
			{
				new KeyDefinition(JobDeclarationSchema.PK, CusEntryInstructionSchema.CEI_JE),
				new KeyDefinition(JobDeclarationSchema.PK, CusEntryHeaderSchema.CH_JE),
				new KeyDefinition(CusEntryHeaderSchema.PK, CusEntryPayInfoSchema.C9_CH),
				new KeyDefinition(CusEntryLineSchema.PK, CusEntryLineFeeSchema.CF_CL),
				new KeyDefinition(CusEntryLineSchema.PK, CusUnderbondDecSchema.BU_CL),
			};

			AssertExceptionThrown(
				"Calling ClusterKeyDoer.Do with gaps in key definition tree.",
				typeof(ArgumentException),
				"The following cluster key definitions do not have a path connecting to top level table:\r\n\t[CusEntryLine/CusEntryLineFee]\r\n\t[CusEntryLine/CusUnderbondDec]",
				() => CallClusterKeyDoer(JobDeclarationSchema.PK, definitions)
			);
		}

		void CallClusterKeyDoer(SchemaPKColumn topLevelTablePK, IEnumerable<KeyDefinition> definitions) => NewClusterKeyDoer.Do(topLevelTablePK, definitions, "DummyClusterKeyFountain");
		ClusterKeyDoer NewClusterKeyDoer => ClusterKeyDoer.New(new DummyUpgradeManager(), IsOnline);
		protected abstract bool IsOnline { get; }
	}
}
