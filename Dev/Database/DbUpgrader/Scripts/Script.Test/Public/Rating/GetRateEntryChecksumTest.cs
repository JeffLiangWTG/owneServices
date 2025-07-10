using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Rating;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Rating.Testing
{
	[TestedType(typeof(GetRateEntryChecksum))]
	class GetRateEntryChecksumTest : DbCreateScriptTest
	{
		public void TestGetRateEntryChecksum()
		{
			var pk = Guid.NewGuid();
			var sql =
				@"
DECLARE @companyPK UNIQUEIDENTIFIER = (SELECT TOP 1 GC_PK FROM dbo.GlbCompany);
DECLARE @ratingHeaderPK UNIQUEIDENTIFIER = '26FD44B5-0E1C-483D-989C-00003060201E';
DECLARE @orgHeaderPK UNIQUEIDENTIFIER = '26FD44B5-0E1C-483D-989C-00003060201E';

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@orgHeaderPK, 'RELPA1');

INSERT INTO dbo.RatingHeader (TH_PK, TH_GC, TH_OH, TH_RateType, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser, TH_SystemCreateTimeUtc, TH_SystemCreateUser) VALUES (@ratingHeaderPK, @companyPK, @orgHeaderPK, 'SAL', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

" +
				"INSERT INTO dbo.RateEntry(" +
				$"{RateEntrySchema.Constants.PK}, " +
				$"{RateEntrySchema.Constants.TI_TH}, " +
				$"{RateEntrySchema.Constants.TI_GC_Publisher}, " +
				$"{RateEntrySchema.Constants.TI_RateCategory}," +
				$"{RateEntrySchema.Constants.TI_Mode}," +
				$"{RateEntrySchema.Constants.TI_OriginLRC}," +
				$"{RateEntrySchema.Constants.TI_DestinationLRC}," +
				$"{RateEntrySchema.Constants.TI_ViaLRC}," +
				$"{RateEntrySchema.Constants.TI_PlannedLoadLRC}," +
				$"{RateEntrySchema.Constants.TI_PlannedDischargeLRC}," +
				$"{RateEntrySchema.Constants.TI_FirstLoadLRC}," +
				$"{RateEntrySchema.Constants.TI_LastDischargeLRC}," +
				$"{RateEntrySchema.Constants.TI_FirstRouteSetLoadPortLRC}," +
				$"{RateEntrySchema.Constants.TI_LastRouteSetDischargePortLRC}," +
				$"{RateEntrySchema.Constants.TI_RS_NKServiceLevel_NI}," +
				$"{RateEntrySchema.Constants.TI_PL_NKCarrierServiceLevel}," +
				$"{RateEntrySchema.Constants.TI_RS_NKGatewayServiceLevel}," +
				$"{RateEntrySchema.Constants.TI_RS_NKShipmentGatewayServiceLevel}," +
				$"{RateEntrySchema.Constants.TI_RH_NKCommodityCode}," +
				$"{RateEntrySchema.Constants.TI_RCC_ComponentCode}," +
				$"{RateEntrySchema.Constants.TI_ContainerUnitSection}," +
				$"{RateEntrySchema.Constants.TI_RRC_RepairCode}," +
				$"{RateEntrySchema.Constants.TI_RMC_Material}," +
				$"{RateEntrySchema.Constants.TI_FMCTariffID}," +
				$"{RateEntrySchema.Constants.TI_CartagePickupAddressPostCode}," +
				$"{RateEntrySchema.Constants.TI_CartageDeliveryAddressPostCode}," +
				$"{RateEntrySchema.Constants.TI_TransitTime}," +
				$"{RateEntrySchema.Constants.TI_PaymentTerm}," +
				$"{RateEntrySchema.Constants.TI_GatewayAgentType}," +
				$"{RateEntrySchema.Constants.TI_Frequency}," +
				$"{RateEntrySchema.Constants.TI_FrequencyUnit}," +
				$"{RateEntrySchema.Constants.TI_IsCrossTrade}," +
				$"{RateEntrySchema.Constants.TI_OH_TransportProvider}," +
				$"{RateEntrySchema.Constants.TI_OH_Supplier}," +
				$"{RateEntrySchema.Constants.TI_OH_Consignor}," +
				$"{RateEntrySchema.Constants.TI_OH_Consignee}," +
				$"{RateEntrySchema.Constants.TI_OH_ControllingCustomer}," +
				$"{RateEntrySchema.Constants.TI_OA_CartagePickupAddressOverride}," +
				$"{RateEntrySchema.Constants.TI_OA_CartageDeliveryAddressOverride}," +
				$"{RateEntrySchema.Constants.TI_RateOrigin}," +
				$"{RateEntrySchema.Constants.TI_RateDestination}," +
				$"{RateEntrySchema.Constants.TI_ParentID}," +
				$"{RateEntrySchema.Constants.TI_ParentTableCode}," +
				$"{RateEntrySchema.Constants.TI_TZ_OriginZone}," +
				$"{RateEntrySchema.Constants.TI_TZ_DestinationZone}," +
				$"{RateEntrySchema.Constants.TI_R9_FromSuburb}," +
				$"{RateEntrySchema.Constants.TI_R9_ToSuburb}," +
				$"{RateEntrySchema.Constants.TI_IsTact}," +
				$"{RateEntrySchema.Constants.TI_RateStartDate}, " +
				$"{RateEntrySchema.Constants.TI_ContractNumber}," +
				$"{RateEntrySchema.Constants.TI_AircraftType}," +
				$"{RateEntrySchema.Constants.TI_ShipmentConsolidationStatus}," +
				$"{RateEntrySchema.Constants.TI_HBLDeliveryMode}," +
				$"{RateEntrySchema.Constants.TI_IsNonOperatedReefer}," +
				$"{RateEntrySchema.Constants.TI_YardUnitType}," +
				$"{RateEntrySchema.Constants.TI_YardUnitLoad}," +
				$"{RateEntrySchema.Constants.TI_SystemLastEditTimeUtc}," +
				$"{RateEntrySchema.Constants.TI_SystemLastEditUser}," +
				$"{RateEntrySchema.Constants.TI_SystemCreateTimeUtc}," +
				$"{RateEntrySchema.Constants.TI_SystemCreateUser}) " +
				"VALUES (" +
				"@PK, " +
				"@ratingHeaderPK, " +
				"@companyPK, " +
				"@RateCategory, " +
				"@Mode, " +
				"@OriginLRC, " +
				"@DestinationLRC, " +
				"@RL_NKTranshipmentPort, " +
				"@PlannedLoadLRC, " +
				"@PlannedDischargeLRC, " +
				"@FirstLoadLRC, " +
				"@LastDischargeLRC, " +
				"@FirstRouteSetLoadPortLRC, " +
				"@LastRouteSetDischargePortLRC, " +
				"@RS_NKServiceLevel_NI, " +
				"@PL_NKCarrierServiceLevel, " +
				"@RS_NKGatewayServiceLevel, " +
				"@RS_NKShipmentGatewayServiceLevel, " +
				"@RH_NKCommodityCode, " +
				"@RCC_ComponentCode, " +
				"@ContainerUnitSection, " +
				"@RRC_RepairCode, " +
				"@RMC_Material, " +
				"@FMCTariffID, " +
				"@CartagePickupAddressPostCode, " +
				"@CartageDeliveryAddressPostCode, " +
				"@TransitTime, " +
				"@PaymentTerm, " +
				"@GatewayAgentType, " +
				"@Frequency, " +
				"@FrequencyUnit, " +
				"@IsCrossTrade, " +
				"@OH_TransportProvider, " +
				"@OH_Supplier, " +
				"@OH_Consignor, " +
				"@OH_Consignee, " +
				"@OH_ControllingCustomer, " +
				"@OA_CartagePickupAddressOverride, " +
				"@OA_CartageDeliveryAddressOverride, " +
				"@RateOrigin, " +
				"@RateDestination, " +
				"@ParentID, " +
				"@ParentTableCode, " +
				"@TZ_OriginZone, " +
				"@TZ_DestinationZone, " +
				"@R9_FromSuburb, " +
				"@R9_ToSuburb, " +
				"@TI_IsTact," +
				"@RateStartDate, " +
				"@ContractNumber, " +
				"@AircraftType," +
				"@ShipmentConsolidationStatus," +
				"@HBLDeliveryMode," +
				"@IsNonOperatedReefer, " +
				"@YardUnitType, " +
				"@YardUnitLoad, " +
				"GetUtcDate()," +
				"'~BP'," +
				"GetUtcDate()," +
				"'~BP')";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@RateCategory", SqlDbType.NVarChar, "WHS");
				command.AddParameter("@Mode", SqlDbType.NVarChar, "ALL");
				command.AddParameter("@OriginLRC", SqlDbType.NVarChar, "AUSYD");
				command.AddParameter("@DestinationLRC", SqlDbType.NVarChar, "USLAX");
				command.AddParameter("@RL_NKTranshipmentPort", SqlDbType.NVarChar, "");
				command.AddParameter("@PlannedLoadLRC", SqlDbType.NVarChar, "AUMEL");
				command.AddParameter("@PlannedDischargeLRC", SqlDbType.NVarChar, "AUPER");
				command.AddParameter("@FirstLoadLRC", SqlDbType.NVarChar, "AUADL");
				command.AddParameter("@LastDischargeLRC", SqlDbType.NVarChar, "AUHBA");
				command.AddParameter("@FirstRouteSetLoadPortLRC", SqlDbType.NVarChar, "AUBNE");
				command.AddParameter("@LastRouteSetDischargePortLRC", SqlDbType.NVarChar, "HKHKG");
				command.AddParameter("@RS_NKServiceLevel_NI", SqlDbType.NVarChar, "");
				command.AddParameter("@PL_NKCarrierServiceLevel", SqlDbType.NVarChar, "");
				command.AddParameter("@RS_NKGatewayServiceLevel", SqlDbType.NVarChar, "");
				command.AddParameter("@RS_NKShipmentGatewayServiceLevel", SqlDbType.NVarChar, "");
				command.AddParameter("@RH_NKCommodityCode", SqlDbType.NVarChar, "");
				command.AddParameter("@RCC_ComponentCode", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@ContainerUnitSection", SqlDbType.NVarChar, "");
				command.AddParameter("@RRC_RepairCode", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@RMC_Material", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@FMCTariffID", SqlDbType.NVarChar, "");
				command.AddParameter("@CartagePickupAddressPostCode", SqlDbType.NVarChar, "");
				command.AddParameter("@CartageDeliveryAddressPostCode", SqlDbType.NVarChar, "");
				command.AddParameter("@TransitTime", SqlDbType.NVarChar, "");
				command.AddParameter("@PaymentTerm", SqlDbType.NVarChar, "");
				command.AddParameter("@GatewayAgentType", SqlDbType.NVarChar, "SAG");
				command.AddParameter("@Frequency", SqlDbType.Int, 1);
				command.AddParameter("@FrequencyUnit", SqlDbType.NVarChar, "");
				command.AddParameter("@IsCrossTrade", SqlDbType.Bit, 0);
				command.AddParameter("@OH_TransportProvider", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@OH_Supplier", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@OH_Consignor", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@OH_Consignee", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@OH_ControllingCustomer", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@OA_CartagePickupAddressOverride", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@OA_CartageDeliveryAddressOverride", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@RateOrigin", SqlDbType.NVarChar, "AUBNE");
				command.AddParameter("@RateDestination", SqlDbType.NVarChar, "USSFO");
				command.AddParameter("@ParentID", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@ParentTableCode", SqlDbType.NVarChar, "");
				command.AddParameter("@TZ_OriginZone", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@TZ_DestinationZone", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@R9_FromSuburb", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@R9_ToSuburb", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@TI_IsTact", SqlDbType.Bit, 0);
				command.AddParameter("@RateStartDate", SqlDbType.Date, new DateTime(2017, 02, 08));
				command.AddParameter("@ContractNumber", SqlDbType.NVarChar, "A123456");
				command.AddParameter("@AircraftType", SqlDbType.NVarChar, "CAO");
				command.AddParameter("@ShipmentConsolidationStatus", SqlDbType.NVarChar, "STS");
				command.AddParameter("@HBLDeliveryMode", SqlDbType.NVarChar, "Door/Door");
				command.AddParameter("@IsNonOperatedReefer", SqlDbType.NVarChar, "");
				command.AddParameter("@YardUnitType", SqlDbType.NVarChar, "CNT");
				command.AddParameter("@YardUnitLoad", SqlDbType.NVarChar, "EMP");

				command.ExecuteNonQuery();
			}

			var query = $"SELECT TI_RateKey FROM dbo.RateEntry WHERE {RateEntrySchema.Constants.PK} = '{pk}'";
			using (var command = TestConnection.Command(query))
			{
				var result = command.ExecuteScalar();
				AssertEquals("Expected checksum value", 1045919524, result);
			}
		}
	}
}

