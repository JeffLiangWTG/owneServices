#if DEBUG

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Data;
using CargoWise.Database.Abstractions;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using Enterprise.DbUpgrader.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Script.Test.TestSetup
{
	class UnitTestViewAndRoutineCreator : ViewAndRoutineCreator
	{
		public UnitTestViewAndRoutineCreator(IUpgradeManager manager, DbConnection upgConnection, string upgradingDb)
			: base(manager, upgConnection, upgradingDb, new List<string> { "TG_Test_Trigger_02" })
		{
		}

		protected override DbRoutineScriptCollection GetViewAndRoutineScriptCollection()
		{
			var result = new DbRoutineScriptCollection
			{
				new DbScript("XT_Test_Proc_01", "CREATE PROCEDURE XT_Test_Proc_01 AS SELECT 'something'", "SQL_STORED_PROCEDURE"),
				new DbScript("Test_Function_02", "CREATE FUNCTION Test_Function_02() RETURNS TABLE AS RETURN (SELECT 1 Col2)", "SQL_INLINE_TABLE_VALUED_FUNCTION"),
				new DbScript("TG_Test_Trigger_01", "CREATE TRIGGER TG_Test_Trigger_01 ON StmData AFTER INSERT AS if EXISTS (SELECT NULL FROM inserted WHERE SD_Name = 'Mike') begin RAISERROR('Test error', 16, 1) end", DbRoutineType.SqlTriggerTypeDesc),
				new DbScript("TG_Test_Trigger_02", "CREATE TRIGGER TG_Test_Trigger_02 ON StmData AFTER INSERT AS if EXISTS (SELECT NULL FROM inserted WHERE SD_Name = 'Mike') begin RAISERROR('Test error', 16, 1) end", DbRoutineType.SqlTriggerTypeDesc),
				new DbScript("", "TG_Test_DDL_Trigger", "CREATE TRIGGER TG_Test_DDL_Trigger ON DATABASE FOR ADD_ROLE_MEMBER AS ROLLBACK;", DbRoutineType.SqlTriggerTypeDesc),
				new ViewStmData()
			};

			AppendMockClientSpecificViewsAndRoutinesToCollection(result);

			return result;
		}

		#region Mock ClientSpecific Views and Routines

		/// <summary>
		/// Sets the client hook to the MockClientHookForClientSpecificDbSchemaUpgradeTest and
		/// calls the method to append client specific views and routines to script collection
		/// </summary>
		void AppendMockClientSpecificViewsAndRoutinesToCollection(DbRoutineScriptCollection scriptCollection)
		{
			var serviceProvider = GlobalServiceProvider.Instance;
			var mockServiceProvider = new Mock<IServiceProvider>();
			mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns<Type>(x => serviceProvider.GetService(x));
			mockServiceProvider.Setup(x => x.GetService(typeof(IExtensionObjectsSource))).Returns(new MockClientSpecificExtensionSource());

			using (GlobalServiceProvider.Configure(mockServiceProvider.Object))
			{
				AppendClientSpecificViewAndRoutineScriptsToCollection(scriptCollection);
			}
		}

		#endregion

		#region MockClientHookToCreateClientSpecificViewsForTesting

		protected class MockClientSpecificExtensionSource : IExtensionObjectsSource
		{
			public string DisplayName => "Testing";
			public string ExtensionCode => "XXX";
			public IExtensionObjects ExtensionObjects { get; } = new ExtensionObjects(
				ImmutableArray<DatabaseObjectCreateScript>.Empty,
				ImmutableArray.Create(new DatabaseViewAndRoutineCreateScript("ClientView", "CREATE VIEW ClientView AS SELECT 0 Col1", "drop table ClientView", "VIEW")));
		}

		#endregion
	}

	[TestClass]
	class ViewStmData : DbCreateIndexedViewScript, IIndexedViewWithTemporaryIndexesOrColumns
	{
		public override string Text => @"
CREATE VIEW ViewStmData
WITH SCHEMABINDING
AS
SELECT
	SD_PK,
	CAST(CASE WHEN CHARINDEX('*AddInfoString35=', '*'+SD_Name) > 0 THEN REPLACE(SUBSTRING('*'+SD_Name, CHARINDEX('*AddInfoString35=', '*'+SD_Name) + 17, CHARINDEX('*', '*'+SD_Name+'*', CHARINDEX('*AddInfoString35=', '*'+SD_Name)+1) - (CHARINDEX('*AddInfoString35=', '*'+SD_Name)+17)), '¤', '*') ELSE '' END AS VARCHAR(35)) AS SD_NameString35
FROM dbo.StmData T
WHERE SD_Type = 'ZZ'
";

		public override List<IndexInfo> Indexes => new List<IndexInfo>
		{
			IndexInfo.Builder.New(WellKnownSqlNames.DbOwnerSchema, Name, "NR_UC__SD_PK")
				.Unique(true)
				.Clustered(true)
				.Key("SD_PK")
				.Option(IndexOptions.ONLINE, value: false)
				.GetInfo(),
		};

		public List<IndexInfo> TemporaryIndexes
		{
			get
			{
				return new List<IndexInfo>
				{
					IndexInfo.Builder.New(WellKnownSqlNames.DbOwnerSchema, "StmData", IndexInfo.MANUALLY_CREATED_WTG_INDEX_PREFIX + "ViewStmData")
						.Unique(true)
						.Clustered(false)
						.Key("SD_PK")
						.Include(
							"SD_Name",
							"CW!!ViewStmData_AddInfoString35"
						)
						.Where("[SD_Type]='ZZ'")
						.Option(IndexOptions.ONLINE, value: true)
						.GetInfo(),
				};
			}
		}

		public List<ComputedColumnInfo> TemporaryComputedColumns
		{
			get
			{
				var builder = ComputedColumnInfo.Builder.New(WellKnownSqlNames.DbOwnerSchema, "StmData");
				return new List<ComputedColumnInfo>
				{
					builder.Name("CW!!ViewStmData_AddInfoString35")
						.ComputedExpression("CAST(CASE WHEN CHARINDEX('*AddInfoString35=', '*'+SD_Name) > 0 THEN REPLACE(SUBSTRING('*'+SD_Name, CHARINDEX('*AddInfoString35=', '*'+SD_Name) + 17, CHARINDEX('*', '*'+SD_Name+'*', CHARINDEX('*AddInfoString35=', '*'+SD_Name)+1) - (CHARINDEX('*AddInfoString35=', '*'+SD_Name)+17)), '¤', '*') ELSE '' END AS VARCHAR(35))")
						.GetInfo(),
				};
			}
		}
	}
}

#endif
