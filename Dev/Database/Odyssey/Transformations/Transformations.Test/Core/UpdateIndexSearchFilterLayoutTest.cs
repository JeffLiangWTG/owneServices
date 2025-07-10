using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core.Testing
{
	[TestedType(typeof(UpdateIndexSearchFilterLayout))]
	public class UpdateIndexSearchFilterLayoutTest : DataTransformationTestCase
	{
		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Update Index Search Filter Layout_1] ON [dbo].[StmModuleFilter] ([S9_FilterType]) WHERE ([S9_FilterType]='GLI') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override void AssertTransformationResults()
		{
			var table = GetTableResult(new List<Guid> { indexModuleFilter, sqlModuleFilter });
			AssertEquals(2, table.Rows.Count);

			var indexRow = table.Rows.Find(indexModuleFilter);
			AssertEquals(true, indexRow["S9_IsIndexSearch"]);
			AssertEquals("", indexRow["S9_FilterType"].ToString());

			var sqlRow = table.Rows.Find(sqlModuleFilter);
			AssertEquals(false, sqlRow["S9_IsIndexSearch"]);
			AssertEquals("", sqlRow["S9_FilterType"].ToString());
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateIndexSearchFilterLayout();
		}

		protected override void PrepareTestData()
		{
			using (TestWhsDataSetupHelper.DisableConstraint(StmModuleFilterSchema.Constants.TableName, "Constraint_S9_FilterType"))
			{
				var helper = new TransformationTestDataCreator();

				indexModuleFilter = CreateOldIndexSearchModuleFilter("JobShipment", "Index Search");
				sqlModuleFilter = helper.CreateModuleFilter("JobShipment", "SQL Search");

				var table = GetTableResult(new List<Guid> { indexModuleFilter, sqlModuleFilter });
				AssertEquals(2, table.Rows.Count);

				var indexRow = table.Rows.Find(indexModuleFilter);
				AssertEquals(OldIndexType, indexRow["S9_FilterType"].ToString());

				var sqlRow = table.Rows.Find(sqlModuleFilter);
				AssertNotEquals(OldIndexType, sqlRow["S9_FilterType"].ToString());
			}
		}

		DataTable GetTableResult(List<Guid> pkList)
		{
			var pkListForQuery = string.Join(",", pkList.Select(x => "'" + x + "'"));
			var schemeSql = $@"
							SELECT
							S9_PK, *
							FROM dbo.StmModuleFilter
							WHERE S9_PK in ({pkListForQuery})";

			var schemeTable = new DataTable();
			using (var schemeCmd = Db.Connection.Command(schemeSql))
			using (var schemeAdapter = schemeCmd.NewDataAdapter())
			{
				schemeAdapter.Fill(schemeTable);
			}
			schemeTable.PrimaryKey = new DataColumn[] { schemeTable.Columns[0] };

			return schemeTable;
		}

		Guid indexModuleFilter;
		Guid sqlModuleFilter;

		const string OldIndexType = "GLI";

		const string CreateOldIndexSearchStmModuleFilterSql = @"
INSERT INTO [dbo].[StmModuleFilter] ([S9_PK], [S9_ModuleID], [S9_FilterName], S9_FilterType, S9_SystemCreateTimeUtc, S9_SystemCreateUser, S9_SystemLastEditTimeUtc, S9_SystemLastEditUser)
VALUES (@pk, @moduleId, @filterName, 'GLI', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateOldIndexSearchModuleFilter(string moduleId, string filterName)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateOldIndexSearchStmModuleFilterSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@moduleId", SqlDbType.VarChar, StmModuleFilterSchema.S9_ModuleID.MaxLength, moduleId);
				command.AddParameter("@filterName", SqlDbType.NVarChar, StmModuleFilterSchema.S9_FilterName.MaxLength, filterName);

				command.ExecuteNonQuery();
			}

			return pk;
		}
	}
}
