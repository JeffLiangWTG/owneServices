using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.SalesAndMarketing.Testing
{
	[TestedType(typeof(UpdateIncorrectOpportunityProfitFieldsToRevenue))]
	class UpdateIncorrectOpportunityProfitFieldsToRevenueTest : DataTransformationTestCase
	{
		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Update profit fields to revenue for opportunities and scopes where the revenue is greater than 0 but_1] ON [dbo].[CrmOpportunity] ([COP_EstimatedProfit], [COP_EstimatedRevenue]) INCLUDE ([COP_OH_Organization], [COP_SystemLastEditTimeUtc], [COP_SystemLastEditUser]) WHERE ([COP_EstimatedRevenue]<>(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update profit fields to revenue for opportunities and scopes where the revenue is greater than 0 but_2] ON [dbo].[CrmOpportunityScope] ([COS_EstimatedProfit], [COS_EstimatedRevenue]) INCLUDE ([COS_COP_Opportunity], [COS_ScopeID], [COS_SystemLastEditTimeUtc], [COS_SystemLastEditUser]) WHERE ([COS_EstimatedRevenue]<>(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override void AssertTransformationResults()
		{
			var oppResult = new List<(string oppID, decimal revenue, decimal profit)>();
			Db.Connection.ExecuteReader(
				"SELECT COP_OpportunityID, COP_EstimatedRevenue, COP_EstimatedProfit FROM dbo.CrmOpportunity WHERE COP_OH_Organization = @orgPK",
				command => command.AddParameter("@orgPK", SqlDbType.UniqueIdentifier, orgPK),
				reader => oppResult.Add((reader["COP_OpportunityID"].ToString(), (decimal)reader["COP_EstimatedRevenue"], (decimal)reader["COP_EstimatedProfit"])));

			AssertContainsExactElementsInAnyOrder(new List<(string oppID, decimal revenue, decimal profit)>
			{
				("O001", revenue: 0, profit: 100),
				("O002", revenue: 50, profit: 50),
				("O003", revenue: 150, profit: 150),
				("O004", revenue: 200, profit: 200),
				("O005", revenue: 400, profit: 300)
			}, oppResult);

			var scopeResult = new List<(byte scopeID, decimal revenue, decimal profit)>();
			Db.Connection.ExecuteReader(
				"SELECT COS_ScopeID, COS_EstimatedRevenue, COS_EstimatedProfit FROM dbo.CrmOpportunityScope WHERE COS_COP_Opportunity = @oppPK",
				command => command.AddParameter("@oppPK", SqlDbType.UniqueIdentifier, scopeOppPK),
				reader => scopeResult.Add(((byte)reader["COS_ScopeID"], (decimal)reader["COS_EstimatedRevenue"], (decimal)reader["COS_EstimatedProfit"])));

			AssertContainsExactElementsInAnyOrder(new List<(byte scopeID, decimal revenue, decimal profit)>
			{
				(1, revenue: 0, profit: 50),
				(2, revenue: 25, profit: 25),
				(3, revenue: 75, profit: 75),
				(4, revenue: 100, profit: 100),
				(5, revenue: 200, profit: 150)
			}, scopeResult);
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateIncorrectOpportunityProfitFieldsToRevenue();
		}

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(CrmOpportunitySchema.Constants.TableName, "Constraint_COP_EstimatedProfitShouldNotExceedRevenue");
			DBTransformationTestHelper.DropConstraintIfExists(CrmOpportunityScopeSchema.Constants.TableName, "Constraint_COS_EstimatedProfitShouldNotExceedRevenue");

			var helper = new TransformationTestDataCreator();
			orgPK = helper.CreateOrg("TST01");
			var companyPK = helper.CreateCompany("TST", "AU");

			scopeOppPK = helper.CreateCrmOpportunity("O001", "Test Opp 1", orgPK, companyPK, "Y", revenue: 0, profit: 100);
			_ = helper.CreateCrmOpportunity("O002", "Test Opp 2", orgPK, companyPK, "Y", revenue: 50, profit: 100);
			_ = helper.CreateCrmOpportunity("O003", "Test Opp 3", orgPK, companyPK, "Y", revenue: 150, profit: 150);
			_ = helper.CreateCrmOpportunity("O004", "Test Opp 4", orgPK, companyPK, "Y", revenue: 200, profit: 250);
			_ = helper.CreateCrmOpportunity("O005", "Test Opp 5", orgPK, companyPK, "Y", revenue: 400, profit: 300);

			_ = helper.CreateCrmOpportunityScope(scopeOppPK, 1, "FWD", "AU", "AU", "Y", revenue: 0, profit: 50);
			_ = helper.CreateCrmOpportunityScope(scopeOppPK, 2, "FWD", "AU", "NZ", "Y", revenue: 25, profit: 50);
			_ = helper.CreateCrmOpportunityScope(scopeOppPK, 3, "FWD", "AUSYD", "AUMEL", "Y", revenue: 75, profit: 75);
			_ = helper.CreateCrmOpportunityScope(scopeOppPK, 4, "FWD", "AUMEL", "AUSYD", "Y", revenue: 100, profit: 125);
			_ = helper.CreateCrmOpportunityScope(scopeOppPK, 5, "FWD", "AUMEL", "NZAKL", "Y", revenue: 200, profit: 150);
		}

		Guid orgPK;
		Guid scopeOppPK;
	}
}
