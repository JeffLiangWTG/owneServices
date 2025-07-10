using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Consol;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Consol.Testing
{
	[TestedType(typeof(IATASales))]
	class IATASalesTest : DbCreateScriptTest
	{
		public void TestChargeableWeight()
		{
			var masterASMShipmentPK1 = Guid.NewGuid();
			var subShipmentPK11 = Guid.NewGuid();
			var masterBCNShipmentPK2 = Guid.NewGuid();
			var subShipmentPK21 = Guid.NewGuid();
			var master3PTShipmentPK3 = Guid.NewGuid();
			var subShipmentPK31 = Guid.NewGuid();
			var orgHeaderPK = Guid.NewGuid();
			var orgAddressPK = Guid.NewGuid();
			var consolPK = Guid.NewGuid();
			var awbHeaderPK = Guid.NewGuid();

			InsertShipment(masterASMShipmentPK1, "S00001010", "ASM", 100);
			InsertShipment(subShipmentPK11, "S00001011", "STD", 10, masterASMShipmentPK1);
			InsertShipment(masterBCNShipmentPK2, "S00001020", "BCN", 200);
			InsertShipment(subShipmentPK21, "S00001021", "STD", 20, masterBCNShipmentPK2);
			InsertShipment(master3PTShipmentPK3, "S00001030", "3PT", 300);
			InsertShipment(subShipmentPK31, "S00001031", "STD", 30, master3PTShipmentPK3);
			TestConnection.ExecuteNonQuery(GetInsertOrgHeaderCommand(orgHeaderPK));
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand(orgAddressPK, orgHeaderPK));
			TestConnection.ExecuteNonQuery(GetInsertConsolCommand(consolPK, orgAddressPK, "C00001000"));
			TestConnection.ExecuteNonQuery(GetInsertJobConShipLinkCommand(Guid.NewGuid(), consolPK, masterASMShipmentPK1));
			TestConnection.ExecuteNonQuery(GetInsertJobConShipLinkCommand(Guid.NewGuid(), consolPK, subShipmentPK11));
			TestConnection.ExecuteNonQuery(GetInsertJobConShipLinkCommand(Guid.NewGuid(), consolPK, masterBCNShipmentPK2));
			TestConnection.ExecuteNonQuery(GetInsertJobConShipLinkCommand(Guid.NewGuid(), consolPK, subShipmentPK21));
			TestConnection.ExecuteNonQuery(GetInsertJobConShipLinkCommand(Guid.NewGuid(), consolPK, master3PTShipmentPK3));
			TestConnection.ExecuteNonQuery(GetInsertJobConShipLinkCommand(Guid.NewGuid(), consolPK, subShipmentPK31));
			TestConnection.ExecuteNonQuery(GetInsertExportAWBHeaderCommand(awbHeaderPK, consolPK));
			TestConnection.ExecuteNonQuery(GetInsertExportAWBRateLineCommand(Guid.NewGuid(), awbHeaderPK));

			var chargeableWeight = GetChargeableWeightFromIATASales();
			AssertEquals("620.000", chargeableWeight);
		}

		#region Implementation

		string GetInsertOrgHeaderCommand(Guid orgHeaderPKParam)
		{
			return string.Format(@"
				INSERT INTO dbo.OrgHeader
				([OH_PK]
				,[OH_IsValid]
				,[OH_Code]
				,[OH_IsActive]
				,[OH_FullName]
				,[OH_IsShippingProvider]
				,[OH_IsShippingLine]
				,[OH_RL_NKClosestPort]
				,[OH_Language]
				,[OH_ScreeningStatus]
				,[OH_SystemLastEditTimeUtc]
				,[OH_SystemLastEditUser]
				,[OH_SystemCreateTimeUtc]
				,[OH_SystemCreateUser]
				,[OH_Category])
				
				VALUES
				
				('{0}'
				,1
				,'TARWITAQI'
				,1
				,'CARRIER WITH SHIPPING LINE'
				,1
				,1
				,'SAAQI'
				,'ENG'
				,'UNK'
				,'2017-05-16 05:50:00'
				,'E'	
				,'2017-05-16 05:50:00'	
				,'E'
				,'BUS')", orgHeaderPKParam);
		}

		string GetInsertOrgAddressCommand(Guid orgAddressPKParam, Guid orgHeaderPKParam)
		{
			return string.Format(@"
				INSERT INTO dbo.OrgAddress
				([OA_PK]
				,[OA_IsValid]
				,[OA_IsActive]
				,[OA_Code]
				,[OA_Language]
				,[OA_Address1]
				,[OA_RL_NKRelatedPortCode]
				,[OA_OH]
				,[OA_RN_NKCountryCode]
				,[OA_ValidationStatus]
				,[OA_AuthorityToLeave])
				
				VALUES
				
				('{0}'
				,1
				,1
				,'2012 ELEPHANT	AVENUE'
				,'ENG'
				,'2012 ELEPHANT AVENUE'
				,'SAAQI'
				,'{1}'
				,'SA'
				,'NYV'
				,'DEF')", orgAddressPKParam, orgHeaderPKParam);
		}

		string GetInsertShipmentCommand(Guid shipmentPKParam, string uniqueConsignRef, string shipmentType, decimal actualChargeable)
		{
			return string.Format(@"
				INSERT INTO dbo.JobShipment
				([JS_PK]
				,[JS_IsValid]
				,[JS_UniqueConsignRef]
				,[JS_AWBServiceLevel]
				,[JS_HouseBill]
				,[JS_ShipmentType]
				,[JS_IsForwardRegistered]
				,[JS_GoodsDescription]
				,[JS_FreightSpotRateAutoratingMode]
				,[JS_FreightCostRateAutoratingMode]
				,[JS_FreightGatewaySellRateAutoratingMode] 
				,[JS_ReleaseType]
				,[JS_RL_NKOrigin]
				,[JS_E_DEP]
				,[JS_RL_NKDestination]
				,[JS_RS_NKServiceLevel]
				,[JS_INCO]
				,[JS_TransportMode]
				,[JS_PackingMode]
				,[JS_ActualVolume]
				,[JS_DocumentedVolume]
				,[JS_ManifestedVolume]
				,[JS_UnitOfVolume]
				,[JS_ActualWeight]
				,[JS_DocumentedWeight]
				,[JS_ManifestedWeight]
				,[JS_UnitOfWeight]
				,[JS_ActualChargeable]
				,[JS_DocumentedChargeable]
				,[JS_ManifestedChargeable]
				,[JS_F3_NKTotalCountPackType]
				,[JS_OuterPacks]
				,[JS_F3_NKPackType]
				,[JS_NoOriginalBills]
				,[JS_NoCopyBills]
				,[JS_HouseBillOfLadingType]
				,[JS_ShippedOnBoard]
				,[JS_HBLAWBChargesDisplay]
				,[JS_Legacy_Support_Columns_Start]
				,[JS_ScreeningStatus]
				,[JS_JS_ColoadMasterShipment]
				,[JS_SystemLastEditTimeUtc]
				,[JS_SystemLastEditUser]
				,[JS_SystemCreateTimeUtc]
				,[JS_SystemCreateUser])

				VALUES				

				('{0}'
				,1
				,'{1}'
				,'STD'
				,'{1}'
				,'{2}'
				,1
				,'goods'
				,'STD'
				,'STD'
				,'STD'
				,'BTD'
				,'AUBNE'
				,'2017-05-16 15:50:00'
				,'CAYUL'
				,'STD'
				,'FOB'
				,'AIR'
				,'FCL'
				,32.000
				,32.000
				,32.000
				,'M3'
				,3222.000
				,3222.000
				,3222.000
				,'KG'
				,{3}
				,32.000
				,32.000
				,'CTN'
				,46
				,'PCE'
				,3
				,3
				,'IAU'
				,'SHP'
				,'SHW'
				,1
				,'UNK'
				,@ColoadMasterShipment
				,'2017-05-16 05:54:00'
				,'E'
				,'2017-05-11 05:10:00'
				,'E')", shipmentPKParam, uniqueConsignRef, shipmentType, actualChargeable);
		}

		void InsertShipment(Guid shipmentPKParam, string uniqueConsignRef, string shipmentType, decimal actualChargeable, object coloadMasterShipment = null)
		{
			var sql = GetInsertShipmentCommand(shipmentPKParam, uniqueConsignRef, shipmentType, actualChargeable);
			using (var command = TestConnection.Command(sql))
			{
				if (coloadMasterShipment == null)
				{
					command.AddParameter("@ColoadMasterShipment", System.Data.SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				else
				{
					command.AddParameter("@ColoadMasterShipment", System.Data.SqlDbType.UniqueIdentifier, coloadMasterShipment);
				}

				command.ExecuteScalar();
			}
		}

		string GetInsertConsolCommand(Guid consolPKParam, Guid orgAddressPKParam, string uniqueConsignRef)
		{
			return string.Format(@"
				INSERT INTO dbo.JobConsol
				([JK_PK]
				,[JK_IsValid]
				,[JK_ScreeningStatus]
				,[JK_Phase]
				,[JK_TransportMode]
				,[JK_ConsolMode]
				,[JK_AgentType]
				,[JK_AWBServiceLevel]
				,[JK_IsForwarding]
				,[JK_UniqueConsignRef]
				,[JK_OA_ShippingLineAddress]
				,[JK_RL_NKLoadPort]
				,[JK_RL_NKDischargePort]
				,[JK_NoOriginalBills]
				,[JK_NoCopyBills]
				,[JK_OverrideWaybillDefaults]
				,[JK_PrintOptionForColoadsOnManifest]
				,[JK_PrintOptionForColoadsOnOtherDocs]
				,[JK_PrintOptionForPackagesOnAWB]
				,[JK_SystemCreateTimeUtc]
				,[JK_SystemCreateUser]
				,[JK_SystemLastEditTimeUtc]
				,[JK_SystemLastEditUser]
				,[JK_RL_NKMasterBillIssuePlace])
				
				VALUES
				
				('{0}'
				,1
				,'UNK'
				,'ALL'
				,'AIR'
				,'FCL'
				,'AGT'
				,'STD'
				,1
				,'{2}'
				,'{1}'
				,'AUBNE'
				,'CAYUL'
				,3
				,3
				,0
				,'ALL'
				,'ALL'
				,'DEF'
				,'2017-05-16 05:50:00'
				,'E'
				,'2017-05-16 05:50:00'
				,'E'
				,'SAAQI')", consolPKParam, orgAddressPKParam, uniqueConsignRef);
		}

		string GetInsertJobConShipLinkCommand(Guid linkPK, Guid consolPK, Guid shipmentPK)
		{
			return string.Format(@"
				INSERT INTO dbo.JobConShipLink
				([JN_PK]
				,[JN_JK]
				,[JN_JS]
				,[JN_AutoVersion]
				,[JN_SystemCreateTimeUtc]
				,[JN_SystemCreateUser]
				,[JN_SystemLastEditTimeUtc]
				,[JN_SystemLastEditUser])
				
				VALUES
				
				('{0}'
				,'{1}'
				,'{2}'
				,0
				,'2017-05-16 05:50:00'
				,'E'
				,'2017-05-16 05:50:00'
				,'E')", linkPK, consolPK, shipmentPK);
		}

		string GetInsertExportAWBHeaderCommand(Guid awbPKParam, Guid consolPK)
		{
			return string.Format(@"
				INSERT INTO dbo.ExportAWBHeader
				([EH_PK]
				,[EH_ParentID]
				,[EH_Table]
				,[EH_AreRateLinesOverridden]
				,[EH_SystemCreateTimeUtc]
				,[EH_SystemCreateUser]
				,[EH_SystemLastEditTimeUtc]
				,[EH_SystemLastEditUser]
				,[EH_AWBIssueDate]
				,[EH_FinalizationDate])
				
				VALUES
				
				('{0}'
				,'{1}'
				,'JobConsol'
				,0
				,'2017-05-16 05:50:00'
				,'E'
				,'2017-05-16 05:50:00'
				,'E'
				,'2017-05-16 05:50:00'
				,'2017-05-16 05:50:00')", awbPKParam, consolPK);
		}

		string GetInsertExportAWBRateLineCommand(Guid awbPKParam, Guid headerPK)
		{
			return string.Format(@"
				INSERT INTO dbo.ExportAWBRateLine
				([ER_PK]
				,[ER_LineCount]
				,[ER_NoOfPiecesOrRCP]
				,[ER_WeightInLBsOrKGs]
				,[ER_GrossWeight]
				,[ER_RateClass]
				,[ER_ChargeableWeight]
				,[ER_RateChargeOrDiscount]
				,[ER_NatureAndQtyOfGoods]
				,[ER_Total]
				,[ER_EH]
				,[ER_NatureAndQtyOfGoodsType]
				,[ER_IsValid]
				,[ER_SystemCreateTimeUtc]
				,[ER_SystemCreateUser]
				,[ER_SystemLastEditTimeUtc]
				,[ER_SystemLastEditUser])
				
				VALUES
				
				('{0}'
				,1
				,3
				,'K'
				,100.100
				,'Q'
				,100.500
				,0.00
				,'DIMS 40x36x38 CM x 1'
				,0.00
				,'{1}'
				,'c'
				,1
				,'2017-05-16 05:50:00'
				,'E'
				,'2017-05-16 05:50:00'
				,'E')", awbPKParam, headerPK);
		}

		string GetChargeableWeightFromIATASales()
		{
			const string sql = "SELECT ChargeableWeight FROM IATASales('', NULL, NULL)";

			using (var command = TestConnection.Command(sql))
			{
				return command.ExecuteScalar().ToString();
			}
		}
		#endregion
	}
}

