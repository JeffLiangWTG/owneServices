using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.SalesAndMarketing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.SalesAndMarketing
{
	[TestedType(typeof(UpdateGlbCompanyCampaignGroupNegativeValuesToZero))]
	class UpdateGlbCompanyCampaignGroupNegativeValuesToZeroTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateGlbCompanyCampaignGroupNegativeValuesToZero();
		}

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(GlbCompanyCampaignGroupSchema.Constants.TableName, "Constraint_GCG_GroupColor");
			var helper = new TransformationTestDataCreator();
			testGlbCompanyCampaignGroup1PK = helper.CreateGlbCompanyCampaignGroup(1);
			testGlbCompanyCampaignGroup2PK = helper.CreateGlbCompanyCampaignGroup(-1);
		}

		protected override void AssertTransformationResults()
		{
			var testNonNegative = GetDataRow(testGlbCompanyCampaignGroup1PK);
			AssertEquals(1, testNonNegative["GCG_GroupColor"]);
			var testNegative = GetDataRow(testGlbCompanyCampaignGroup2PK);
			AssertEquals(0, testNegative["GCG_GroupColor"]);
		}

		DataRow GetDataRow(Guid pK)
		{
			var dataTable = new DataTable();
			var sql = $"SELECT GCG_GroupColor FROM dbo.GlbCompanyCampaignGroup WHERE GCG_PK = '{pK}'";
			using (var cmd = Db.Connection.Command(sql))
			using (var adapter = cmd.NewDataAdapter())
			{
				adapter.Fill(dataTable);
			}

			return dataTable.Rows[0];
		}

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Set negative values to zero for non-negative GlbCompanyCampaignGroup column GCG_GroupColor_1] ON [dbo].[GlbCompanyCampaignGroup] ([GCG_GroupColor]) WHERE ([GCG_GroupColor]<(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		Guid testGlbCompanyCampaignGroup1PK;
		Guid testGlbCompanyCampaignGroup2PK;
	}
}
