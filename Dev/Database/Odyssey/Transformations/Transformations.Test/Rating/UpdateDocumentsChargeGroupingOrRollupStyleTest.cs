using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;

using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations
{
	[TestedType(typeof(UpdateDocumentsChargeGroupingOrRollupStyle))]
	class UpdateDocumentsChargeGroupingOrRollupStyleTest : DataTransformationTestCase
	{
		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Updating constraints for RCG_Style and updating values for ONF => O&F AND FND => F&D_1] ON [dbo].[RatingDocumentsChargeGroupingOrRollup] ([RCG_Style]) WHERE ([RCG_Style] IN ('ONF', 'FND')) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateDocumentsChargeGroupingOrRollupStyle();

		readonly Guid GroupingOrRollupPK1 = Guid.NewGuid();
		readonly Guid GroupingOrRollupPK2 = Guid.NewGuid();

		protected override void AssertTransformationResults()
		{
			AssertStyle(GroupingOrRollupPK1, "O&F");
			AssertStyle(GroupingOrRollupPK2, "F&D");
		}

		void AssertStyle(Guid groupingOrRollupPK, string style)
		{
			var groupingOrRollup = GetGroupingOrRollup(groupingOrRollupPK);

			AssertEquals(style, groupingOrRollup["RCG_Style"]);
		}

		DataRow GetGroupingOrRollup(Guid groupingOrRollupPK)
		{
			var sql = $"SELECT RCG_Style FROM dbo.RatingDocumentsChargeGroupingOrRollup WHERE RCG_PK = @groupingOrRollupPK";
			var dataTable = new DataTable();
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@groupingOrRollupPK", SqlDbType.UniqueIdentifier, groupingOrRollupPK);
				using (var adapter = cmd.NewDataAdapter())
				{
					adapter.Fill(dataTable);
				}
			}

			AssertEquals(1, dataTable.Rows.Count);
			return dataTable.Rows[0];
		}

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists("RatingDocumentsChargeGroupingOrRollup", "Constraint_DisplayAndStyle");
			DBTransformationTestHelper.DropConstraintIfExists("RatingDocumentsChargeGroupingOrRollup", "RatingDocumentsChargeGroupingOrRollup_RCG_OB_CompanyData_FK2_OrgCompanyData_CRR_120N");

			const string createSQL = @"
INSERT INTO dbo.RatingDocumentsChargeGroupingOrRollup
	(RCG_PK, RCG_Module, RCG_JobType, RCG_TransportMode, RCG_Display, RCG_Style, RCG_OB_CompanyData, RCG_SystemCreateTimeUtc, RCG_SystemCreateUser, RCG_SystemLastEditTimeUtc, RCG_SystemLastEditUser)
VALUES
	(@GroupingOrRollupPK1, 'MOD', 'TRA', 'ALL', 'DEF', 'ONF', @CompanyFK1, GETDATE(), 'USR', GETDATE(), 'USR'),
	(@GroupingOrRollupPK2, 'MOD', 'TRA', 'ALL', 'DEF', 'FND', @CompanyFK2, GETDATE(), 'USR', GETDATE(), 'USR')";

			using (var command = Db.Connection.Command(createSQL))
			{
				command.AddParameter("@GroupingOrRollupPK1", SqlDbType.UniqueIdentifier, GroupingOrRollupPK1);
				command.AddParameter("@GroupingOrRollupPK2", SqlDbType.UniqueIdentifier, GroupingOrRollupPK2);
				command.AddParameter("@CompanyFK1", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@CompanyFK2", SqlDbType.UniqueIdentifier, Guid.NewGuid());

				command.ExecuteNonQuery();
			}
		}
	}
}
