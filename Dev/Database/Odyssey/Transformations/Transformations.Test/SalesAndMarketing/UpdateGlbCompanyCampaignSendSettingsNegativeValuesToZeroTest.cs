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
	[TestedType(typeof(UpdateGlbCompanyCampaignSendSettingsNegativeValuesToZero))]
	class UpdateGlbCompanyCampaignSendSettingsNegativeValuesToZeroTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateGlbCompanyCampaignSendSettingsNegativeValuesToZero();
		}

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(GlbCompanyCampaignSendSettingsSchema.Constants.TableName, "Constraint_GSC_ContactLimitPerOrganizationInHorizontal");
			var helper = new TransformationTestDataCreator();
			testGlbCompanyCampaignSendSettings1PK = helper.CreateGlbCompanyCampaignSendSettings(1);
			testGlbCompanyCampaignSendSettings2PK = helper.CreateGlbCompanyCampaignSendSettings(-1);
		}

		protected override void AssertTransformationResults()
		{
			var testNonNegative = GetDataRow(testGlbCompanyCampaignSendSettings1PK);
			AssertEquals((short)1, testNonNegative["GSC_ContactLimitPerOrganizationInHorizontal"]);
			var testNegative = GetDataRow(testGlbCompanyCampaignSendSettings2PK);
			AssertEquals((short)0, testNegative["GSC_ContactLimitPerOrganizationInHorizontal"]);
		}

		DataRow GetDataRow(Guid pK)
		{
			var dataTable = new DataTable();
			var sql = $"SELECT GSC_ContactLimitPerOrganizationInHorizontal FROM dbo.GlbCompanyCampaignSendSettings WHERE GSC_PK = '{pK}'";
			using (var cmd = Db.Connection.Command(sql))
			using (var adapter = cmd.NewDataAdapter())
			{
				adapter.Fill(dataTable);
			}

			return dataTable.Rows[0];
		}

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Set negative values to zero for non-negative GlbCompanyCampaignSendSettings column GSC_ContactLimitP_1] ON [dbo].[GlbCompanyCampaignSendSettings] ([GSC_ContactLimitPerOrganizationInHorizontal]) WHERE ([GSC_ContactLimitPerOrganizationInHorizontal]<(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		Guid testGlbCompanyCampaignSendSettings1PK;
		Guid testGlbCompanyCampaignSendSettings2PK;
	}
}
