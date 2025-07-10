using System;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.Org.Sales;
using CargoWise.Types;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.Sales.Testing
{
	[TestedType(typeof(TG_CheckNoOpportunityScopeOverlaps))]
	class TG_CheckNoOpportunityScopeOverlapsTest : DBCreateTriggerScriptTest
	{
		public void TestNonKeyColumns()
		{
			var testNonKeyColumns = CreateCrmOpportunityScope(baseOpportunityPK, notes: "Different notes", estimatedRevenue: 1.5m, estimatedProfit: 1.5m, estimatedVolume: 1.5m, estimatedWeight: 1.5m,
				currency: "USD", estimatedVolumeUQ: "CC", estimatedWeightUQ: "OZ");
			AssertExceptionThrown("The difference in these columns does not differentiate the scope from existing ones.", typeof(SqlException), () => testNonKeyColumns.Save());
		}

		public void TestThrowsErrorWhenUpdatingMultipleRows()
		{
			var scope = CreateCrmOpportunityScope(baseOpportunityPK, serviceLevel: "ABC");
			AssertNoExceptionThrown("Should be able to save non-overlapping scope", () => scope.Save());

			using (var command = TestConnection.Command("UPDATE dbo.CrmOpportunityScope SET COS_RS_NKServiceLevel = 'ZZZ', COS_SystemLastEditTimeUtc = GETUTCDATE(), COS_SystemLastEditUser = 'E'"))
			{
				var ex = AssertExceptionThrown<SqlException>(() => command.ExecuteNonQuery());
				AssertEquals("Cannot update more than 1 row at a time.\r\nThe transaction ended in the trigger. The batch has been aborted.", ex.Message);
			}
		}

		ActiveRowWrapper CreateCrmOpportunityScope(Guid oppPK, bool enabled = true, string disabledStatus = "", string notes = "NOTES", decimal estimatedRevenue = 100.0m, decimal estimatedProfit = 20.0m, decimal estimatedVolume = 30.0m, decimal estimatedWeight = 40.0m, string estimatedVolumeUQ = "M3", string estimatedWeightUQ = "KG", string currency = "AUD", string serviceLevel = "LVL")
		{
			var scope = new ActiveRowWrapper(CrmOpportunityScopeSchema.Instance)
			{
				[CrmOpportunityScopeSchema.COS_COP_Opportunity] = oppPK,
				[CrmOpportunityScopeSchema.COS_ProductCode] = "FWD",
				[CrmOpportunityScopeSchema.COS_Origin] = "AUSYD",
				[CrmOpportunityScopeSchema.COS_Destination] = "BDUSA",
				[CrmOpportunityScopeSchema.COS_TransportMode] = "AIR",
				[CrmOpportunityScopeSchema.COS_ContainerMode] = "ULD",
				[CrmOpportunityScopeSchema.COS_PickupAddressPostCode] = "PICKUPCODE",
				[CrmOpportunityScopeSchema.COS_DeliveryAddressPostCode] = "DELCODE",
				[CrmOpportunityScopeSchema.COS_RH_NKCommodityCode] = "COM",
				[CrmOpportunityScopeSchema.COS_IncoTerm] = "INC",
				[CrmOpportunityScopeSchema.COS_RS_NKServiceLevel] = serviceLevel,
				[CrmOpportunityScopeSchema.COS_Enabled] = enabled,
				[CrmOpportunityScopeSchema.COS_DisabledStatus] = disabledStatus,
				[CrmOpportunityScopeSchema.COS_Notes] = (byte[])ZBlob.FromUTF8(notes),
				[CrmOpportunityScopeSchema.COS_EstimatedRevenue] = estimatedRevenue,
				[CrmOpportunityScopeSchema.COS_EstimatedProfit] = estimatedProfit,
				[CrmOpportunityScopeSchema.COS_EstimatedVolume] = estimatedVolume,
				[CrmOpportunityScopeSchema.COS_EstimatedVolumeUQ] = estimatedVolumeUQ,
				[CrmOpportunityScopeSchema.COS_EstimatedWeight] = estimatedWeight,
				[CrmOpportunityScopeSchema.COS_EstimatedWeightUQ] = estimatedWeightUQ,
				[CrmOpportunityScopeSchema.COS_RX_NKScopeCurrency] = currency,
				[CrmOpportunityScopeSchema.COS_ScopeID] = scopeID,
			};

			scopeID++;

			return scope;
		}

		protected override void SetUp()
		{
			baseOrgPK = TestDataCreator.CreateOrganisation("BR1", "Test Organisation");
			baseCompanyPK = TestDataCreator.CreateCompany("TC1", "IT", "EUR");
			baseOpportunityPK = TestDataCreator.CreateCrmOpportunity("BA1", "Base Opportunity", baseOrgPK, baseCompanyPK, startDate: "2024-10-11", endDate: "2024-10-30");
			baseOpportunityScope = CreateCrmOpportunityScope(baseOpportunityPK);
			baseOpportunityScope.Save();

			base.SetUp();
		}

		byte scopeID;
		Guid baseOrgPK;
		Guid baseCompanyPK;
		Guid baseOpportunityPK;
		ActiveRowWrapper baseOpportunityScope;
	}
}
