using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.SalesAndMarketing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.SalesAndMarketing
{
	[TestedType(typeof(SetDefaultCrmOpportunityScopeFrequencyUnit))]
	class SetDefaultCrmOpportunityScopeFrequencyUnitTest : DataTransformationTestCase
	{
		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Set default opportunity scope overall Frequency unit to ('Y')_1] ON [dbo].[CrmOpportunityScope] ([COS_ScopeFrequencyUnit]) WHERE ([COS_ScopeFrequencyUnit]='') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override DataTransformation GetNewTestTransformationInstance()
			=> new SetDefaultCrmOpportunityScopeFrequencyUnit();

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(CrmOpportunityScopeSchema.Constants.TableName, "Constraint_COS_ScopeFrequencyUnit");

			var helper = new TransformationTestDataCreator();
			var orgHeader = helper.CreateOrgHeader("US1", "US WHS Org1");
			var companyPK = helper.CreateCompany("VN1", "VN");

			crmOpportunity = helper.CreateCrmOpportunity("O001", "Test Opportunity", orgHeader, companyPK, "M");

			crmOpportunityScope1 = helper.CreateCrmOpportunityScope(crmOpportunity, 1, "FWD", "AU", "AU", "");
			crmOpportunityScope2 = helper.CreateCrmOpportunityScope(crmOpportunity, 2, "FWD", "AD", "AD", "M");
		}

		protected override void AssertTransformationResults()
		{
			var testEmptyFrequencyUnit = GetDataRow(crmOpportunityScope1);

			CombineAssertions("All empty value will be replaced by Y", () =>
			{
				AssertEquals(testEmptyFrequencyUnit["COS_ScopeFrequencyUnit"], "Y");
			});

			var testNonEmptyFrequencyUnit = GetDataRow(crmOpportunityScope2);

			CombineAssertions("All non empty value should not be changed.", () =>
			{
				AssertEquals(testNonEmptyFrequencyUnit["COS_ScopeFrequencyUnit"], "M");
			});
		}

		DataRow GetDataRow(Guid pk)
		{
			var sql = $"SELECT COS_ScopeFrequencyUnit FROM dbo.CrmOpportunityScope WHERE COS_PK = '{pk}'";
			var dataTable = new DataTable();

			using (var cmd = Db.Connection.Command(sql))
			using (var adapter = cmd.NewDataAdapter())
			{
				adapter.Fill(dataTable);
			}

			return dataTable.Rows[0];
		}

		Guid crmOpportunityScope1;
		Guid crmOpportunityScope2;
		Guid crmOpportunity;
	}
}
