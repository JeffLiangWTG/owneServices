using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.Org.Sales;
using CargoWise.Types;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.Sales.Testing
{
	[TestedType(typeof(GetOverlappingOpportunityScopes))]
	class GetOverlappingOpportunityScopesTest : DbCreateScriptTest
	{
		public void TestOverlappingScope()
		{
			var result = RunStoredProcedure(Guid.NewGuid(), baseOrgPK);
			AssertEquals(1, result.Rows.Count);
			AssertEquals("Test when the new scope is identical to an existing scope.", baseOppScopePK, result.Rows[0]["COS_PK"]);
		}

		public void TestOverlappingDisabledScope()
		{
			var result = RunStoredProcedure(Guid.NewGuid(), baseOrgPK, enabled: 0);
			AssertEquals("Disabled scopes are not considered while checking for overlapping scopes", 0, result.Rows.Count);
		}

		public void TestNonOverlappingScopes()
		{
			CombineAssertions("The function does not return any rows when there is no overlap between the existing scope and the new scope.", () =>
			{
				var newOrg = TestDataCreator.CreateOrganisation("BR2", "Test Organisation 2");
				var result = RunStoredProcedure(Guid.NewGuid(), newOrg);
				AssertEquals(0, result.Rows.Count);

				result = RunStoredProcedure(Guid.NewGuid(), baseOrgPK, transportMode: "SEA");
				AssertEquals(0, result.Rows.Count);

				result = RunStoredProcedure(Guid.NewGuid(), baseOrgPK, containerMode: "LSE");
				AssertEquals(0, result.Rows.Count);

				result = RunStoredProcedure(Guid.NewGuid(), baseOrgPK, commodityCode: "ABCD");
				AssertEquals(0, result.Rows.Count);

				result = RunStoredProcedure(Guid.NewGuid(), baseOrgPK, incoTerm: "ABC");
				AssertEquals(0, result.Rows.Count);

				result = RunStoredProcedure(Guid.NewGuid(), baseOrgPK, serviceLevel: "ABC");
				AssertEquals(0, result.Rows.Count);
			});
		}

		public void TestPickupAndDeliveryAddresses()
		{
			var pickupAddress = TestDataCreator.CreateAddress(baseOrgPK, "OA1", "pickup address");
			var deliveryAddress = TestDataCreator.CreateAddress(baseOrgPK, "OA2", "delivery address");

			var result = RunStoredProcedure(Guid.NewGuid(), baseOrgPK, pickupAddressPostCode: "TSTCODE");
			AssertEquals(0, result.Rows.Count);

			result = RunStoredProcedure(Guid.NewGuid(), baseOrgPK, deliveryAddressPostCode: "TSTCODE");
			AssertEquals(0, result.Rows.Count);

			result = RunStoredProcedure(Guid.NewGuid(), baseOrgPK, pickupAddressPostCode: "", pickupAddress: pickupAddress);
			AssertEquals(0, result.Rows.Count);

			result = RunStoredProcedure(Guid.NewGuid(), baseOrgPK, deliveryAddressPostCode: "", deliveryAddress: deliveryAddress);
			AssertEquals(0, result.Rows.Count);

			result = RunStoredProcedure(Guid.NewGuid(), baseOrgPK, pickupAddressPostCode: "", deliveryAddressPostCode: "");
			AssertEquals(0, result.Rows.Count);

			var emptyPostCodeScope = CreateCrmOpportunityScope(baseOpportunityPK, pickupAddressPostCode: "", deliveryAddressPostCode: "");
			result = RunStoredProcedure(Guid.NewGuid(), baseOrgPK, pickupAddressPostCode: "", deliveryAddressPostCode: "");
			AssertEquals(1, result.Rows.Count);
			AssertEquals("Existing scope where addresses are empty", emptyPostCodeScope, result.Rows[0]["COS_PK"]);
		}

		public void TestRefContainer()
		{
			var refContainer = TestDataCreator.CreateRefContainer("ABC");

			var result = RunStoredProcedure(Guid.NewGuid(), baseOrgPK, refContainer: null);
			AssertEquals(1, result.Rows.Count);
			AssertEquals("Test when refContainers are both null", baseOppScopePK, result.Rows[0]["COS_PK"]);

			result = RunStoredProcedure(Guid.NewGuid(), baseOrgPK, refContainer: refContainer);
			AssertEquals("Test when there is no scope with matching refContainer", 0, result.Rows.Count);

			var scopeWithContainer = CreateCrmOpportunityScope(baseOpportunityPK, refContainer: refContainer);

			result = RunStoredProcedure(Guid.NewGuid(), baseOrgPK, refContainer: refContainer);
			AssertEquals(1, result.Rows.Count);
			AssertEquals("Test when a scope with a matching refContainer exists.", scopeWithContainer, result.Rows[0]["COS_PK"]);
		}

		public void TestOriginDestination()
		{
			var result = RunStoredProcedure(Guid.NewGuid(), baseOrgPK, origin: "ABCDE");
			AssertEquals(0, result.Rows.Count);

			result = RunStoredProcedure(Guid.NewGuid(), baseOrgPK, destination: "ABCDE");
			AssertEquals(0, result.Rows.Count);

			result = RunStoredProcedure(Guid.NewGuid(), baseOrgPK, destination: "AUCDE");
			AssertEquals(0, result.Rows.Count);

			result = RunStoredProcedure(Guid.NewGuid(), baseOrgPK, destination: "BD");
			AssertEquals(0, result.Rows.Count);

			var scopeWithNewDestination = CreateCrmOpportunityScope(baseOpportunityPK, destination: "BD");

			result = RunStoredProcedure(Guid.NewGuid(), baseOrgPK, origin: "AU");
			AssertEquals(1, result.Rows.Count);
			AssertEquals(@"Origin that matches country code in the scope and matches condition
				Len(Origin) of new scope (AU) = 2 AND Len(Origin) of existing scope (AUSYD) = 5
				AND
				Len(Destination) of new scope (BDUSA) = 5 AND Len(Destination) of existing scope (BD) = 2	
				", scopeWithNewDestination, result.Rows[0]["COS_PK"]);
		}

		public void TestEffectiveDates()
		{
			var result = RunStoredProcedure(Guid.NewGuid(), baseOrgPK, startDate: "2024-10-11", endDate: "2024-10-30");
			AssertEquals(1, result.Rows.Count);
			AssertEquals("Does not allow saving scope when effective start / end dates are identical.", baseOppScopePK, result.Rows[0]["COS_PK"]);

			result = RunStoredProcedure(Guid.NewGuid(), baseOrgPK, startDate: "2024-10-01", endDate: "2024-10-15");
			AssertEquals(1, result.Rows.Count);
			AssertEquals("Does not allow saving scope when its start date is before existing scope's start date and ends in between existing scope's start and end date.", baseOppScopePK, result.Rows[0]["COS_PK"]);

			result = RunStoredProcedure(Guid.NewGuid(), baseOrgPK, startDate: "2024-10-15", endDate: "2024-10-31");
			AssertEquals(1, result.Rows.Count);
			AssertEquals("Does not allow saving scope when its start date is in between existing scope's start and end date and ends after existing scope's end date.", baseOppScopePK, result.Rows[0]["COS_PK"]);
		}

		DataTable RunStoredProcedure(Guid scopePk, Guid org, string productCode = "FWD", string transportMode = "AIR", string containerMode = "ULD", string incoTerm = "INC", string serviceLevel = "LVL", string commodityCode = "COM", string origin = "AUSYD", string destination = "BDUSA", string pickupAddressPostCode = "PICKUPCODE", string deliveryAddressPostCode = "DELCODE",
				Guid? pickupAddress = null, Guid? deliveryAddress = null, Guid? refContainer = null, byte enabled = 1, string startDate = "2024-10-11", string endDate = "2024-10-30")
		{
			using (var command = Db.Connection.Command("GetOverlappingOpportunityScopes"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@COS_PK", SqlDbType.UniqueIdentifier, scopePk);
				command.AddParameter("@COS_ProductCode", SqlDbType.Char, productCode);
				command.AddParameter("@COS_TransportMode", SqlDbType.Char, transportMode);
				command.AddParameter("@COS_ContainerMode", SqlDbType.Char, containerMode);
				command.AddParameter("@COS_IncoTerm", SqlDbType.VarChar, incoTerm);
				command.AddParameter("@COS_RS_NKServiceLevel", SqlDbType.VarChar, serviceLevel);
				command.AddParameter("@COS_RH_NKCommodityCode", SqlDbType.VarChar, commodityCode);
				command.AddParameter("@COS_Origin", SqlDbType.VarChar, origin);
				command.AddParameter("@COS_Destination", SqlDbType.VarChar, destination);
				command.AddParameter("@COS_PickupAddressPostCode", SqlDbType.VarChar, pickupAddressPostCode);
				command.AddParameter("@COS_DeliveryAddressPostCode", SqlDbType.VarChar, deliveryAddressPostCode);
				command.AddParameter("@COS_OA_PickupAddress", SqlDbType.UniqueIdentifier, pickupAddress == null ? DBNull.Value : pickupAddress);
				command.AddParameter("@COS_OA_DeliveryAddress", SqlDbType.UniqueIdentifier, deliveryAddress == null ? DBNull.Value : deliveryAddress);
				command.AddParameter("@COS_RC_ContainerType", SqlDbType.UniqueIdentifier, refContainer == null ? DBNull.Value : refContainer);
				command.AddParameter("@COS_Enabled", SqlDbType.Bit, enabled);
				command.AddParameter("@COP_OH_Organization", SqlDbType.UniqueIdentifier, org);
				command.AddParameter("@COP_EffectiveStartDate", SqlDbType.Date, startDate);
				command.AddParameter("@COP_EffectiveEndDate", SqlDbType.Date, endDate);
				var result = DataUtils.GetDataTableFromCommand(command);

				return result;
			}
		}

		Guid CreateCrmOpportunityScope(Guid oppPK, Guid? refContainer = null, Guid? pickupAddress = null, Guid? deliveryAddress = null, string productCode = "FWD", string origin = "AUSYD", string destination = "BDUSA", string transportMode = "AIR", string containerMode = "ULD",
			string pickupAddressPostCode = "PICKUPCODE", string deliveryAddressPostCode = "DELCODE", string commodityCode = "COM", string incoTerm = "INC", string serviceLevel = "LVL", string notes = "NOTES")
		{
			var oppScopePK = Guid.NewGuid();

			var sql = @"INSERT INTO dbo.CrmOpportunityScope (COS_PK, COS_COP_Opportunity, COS_RC_ContainerType, COS_ProductCode, COS_Origin, COS_Destination, COS_TransportMode, COS_ContainerMode, COS_PickupAddressPostCode, COS_DeliveryAddressPostCode, COS_OA_PickupAddress, COS_OA_DeliveryAddress, COS_RH_NKCommodityCode, COS_IncoTerm, COS_RS_NKServiceLevel,
						COS_Enabled, COS_DisabledStatus, COS_Notes, COS_EstimatedRevenue, COS_EstimatedProfit, COS_EstimatedVolume, COS_EstimatedVolumeUQ, COS_EstimatedWeight, COS_EstimatedWeightUQ, COS_RX_NKScopeCurrency, COS_ScopeID, COS_SystemCreateTimeUtc, COS_SystemLastEditTimeUtc, COS_SystemCreateUser, COS_SystemLastEditUser)
					VALUES (@oppScopePK, @oppPK, @refContainer, @productCode, @origin, @destination, @transportMode, @containerMode, @pickupAddressPostCode, @deliveryAddressPostCode, @pickupAddress, @deliveryAddress, @commodityCode, @incoTerm, @serviceLevel, 1, '', @notes,
						100, 20, 30, 'M3', 40, 'KG', 'AUD', @scopeID, GETUTCDATE(), GETUTCDATE(), '~BP', '~BP')";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@oppScopePK", oppScopePK, CrmOpportunityScopeSchema.PK);
				command.AddParameterBasedOnDbColumn("@oppPK", oppPK, CrmOpportunityScopeSchema.COS_COP_Opportunity);
				command.AddParameterBasedOnDbColumn("@productCode", productCode, CrmOpportunityScopeSchema.COS_ProductCode);
				command.AddParameterBasedOnDbColumn("@origin", origin, CrmOpportunityScopeSchema.COS_Origin);
				command.AddParameterBasedOnDbColumn("@destination", destination, CrmOpportunityScopeSchema.COS_Destination);
				command.AddParameterBasedOnDbColumn("@transportMode", transportMode, CrmOpportunityScopeSchema.COS_TransportMode);
				command.AddParameterBasedOnDbColumn("@containerMode", containerMode, CrmOpportunityScopeSchema.COS_ContainerMode);
				command.AddParameterBasedOnDbColumn("@pickupAddressPostCode", pickupAddressPostCode, CrmOpportunityScopeSchema.COS_PickupAddressPostCode);
				command.AddParameterBasedOnDbColumn("@deliveryAddressPostCode", deliveryAddressPostCode, CrmOpportunityScopeSchema.COS_DeliveryAddressPostCode);
				command.AddParameterBasedOnDbColumn("@commodityCode", commodityCode, CrmOpportunityScopeSchema.COS_RH_NKCommodityCode);
				command.AddParameterBasedOnDbColumn("@incoTerm", incoTerm, CrmOpportunityScopeSchema.COS_IncoTerm);
				command.AddParameterBasedOnDbColumn("@serviceLevel", serviceLevel, CrmOpportunityScopeSchema.COS_RS_NKServiceLevel);
				command.AddParameterBasedOnDbColumn("@refContainer", refContainer == null ? DBNull.Value : refContainer, CrmOpportunityScopeSchema.COS_RC_ContainerType);
				command.AddParameterBasedOnDbColumn("@pickupAddress", pickupAddress == null ? DBNull.Value : pickupAddress, CrmOpportunityScopeSchema.COS_OA_PickupAddress);
				command.AddParameterBasedOnDbColumn("@deliveryAddress", deliveryAddress == null ? DBNull.Value : deliveryAddress, CrmOpportunityScopeSchema.COS_OA_DeliveryAddress);
				command.AddParameterBasedOnDbColumn("@notes", (byte[])ZBlob.FromUTF8(notes), CrmOpportunityScopeSchema.COS_Notes);
				command.AddParameterBasedOnDbColumn("@scopeID", scopeID, CrmOpportunityScopeSchema.COS_ScopeID);

				command.ExecuteNonQuery();
			}

			scopeID++;

			return oppScopePK;
		}

		protected override void SetUp()
		{
			baseOrgPK = TestDataCreator.CreateOrganisation("BR1", "Test Organisation");
			baseCompanyPK = TestDataCreator.CreateCompany("TC1", "IT", "EUR");
			baseOpportunityPK = TestDataCreator.CreateCrmOpportunity("BA1", "Base Opportunity", baseOrgPK, baseCompanyPK, startDate: "2024-10-11", endDate: "2024-10-30");
			baseOppScopePK = CreateCrmOpportunityScope(baseOpportunityPK);

			base.SetUp();
		}

		Guid baseOrgPK;
		Guid baseOppScopePK;
		Guid baseCompanyPK;
		Guid baseOpportunityPK;
		byte scopeID;
	}
}
