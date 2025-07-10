using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Shipment;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Shipment
{
	[TestedType(typeof(JobShipmentContainerCountAndType))]
	class JobShipmentContainerCountAndTypeTest : DbCreateScriptTest
	{
		public void TestJobShipmentContainerAndType()
		{
			shipmentPK = Guid.NewGuid();
			orgHeaderPK = Guid.NewGuid();
			orgAddressPK = Guid.NewGuid();
			consolPK = Guid.NewGuid();
			containerPK = Guid.NewGuid();
			packLine1PK = Guid.NewGuid();
			packLine2PK = Guid.NewGuid();
			packPivot1PK = Guid.NewGuid();
			packPivot2PK = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(GetInsertShipmentCommand(shipmentPK));
			TestConnection.ExecuteNonQuery(GetInsertOrgHeaderCommand(orgHeaderPK));
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand(orgAddressPK, orgHeaderPK));
			TestConnection.ExecuteNonQuery(GetInsertConsolCommand(consolPK, orgAddressPK));
			TestConnection.ExecuteNonQuery(GetInsertContainerCommand(containerPK, consolPK));
			TestConnection.ExecuteNonQuery(GetInsertPacklLineCommand(packLine1PK, shipmentPK));
			TestConnection.ExecuteNonQuery(GetInsertPacklLineCommand(packLine2PK, shipmentPK));
			TestConnection.ExecuteNonQuery(GetInsertPackPivotCommand(packLine1PK, containerPK, packPivot1PK));
			TestConnection.ExecuteNonQuery(GetInsertPackPivotCommand(packLine2PK, containerPK, packPivot2PK));

			var shipmentContainerAndType = GetSelectContainerCountAndType();
			AssertEquals("Grouping Should be one container", "1 X 40RE", shipmentContainerAndType);
		}
		Guid shipmentPK;
		Guid containerPK;
		Guid consolPK;
		Guid orgHeaderPK;
		Guid orgAddressPK;
		Guid packLine1PK;
		Guid packLine2PK;
		Guid packPivot1PK;
		Guid packPivot2PK;

		string GetSelectContainerCountAndType()
		{
			const string sql = "SELECT Value FROM JobShipmentContainerCountAndType(@ShipmentPK)";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@ShipmentPK", System.Data.SqlDbType.UniqueIdentifier, shipmentPK);
				return command.ExecuteScalar().ToString();
			}
		}

		string GetInsertShipmentCommand(Guid shipmentPKParam)
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
				,[JS_SystemLastEditTimeUtc]
				,[JS_SystemLastEditUser]
				,[JS_SystemCreateTimeUtc]
				,[JS_SystemCreateUser])

				VALUES				

				('{0}'
				,1
				,'S00002000'
				,'STD'
				,'S00002000'
				,'STD'
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
				,'SEA'
				,'FCL'
				,32.000
				,32.000
				,32.000
				,'M3'
				,3222.000
				,3222.000
				,3222.000
				,'KG'
				,32.000
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
				,'2017-05-16 05:54:00'
				,'E'
				,'2017-05-11 05:10:00'
				,'E')", shipmentPKParam);
		}

		string GetInsertContainerCommand(Guid containerPKParam, Guid consolPKParam)
		{
			return string.Format(@"
				INSERT INTO dbo.JobContainer
				([JC_PK]
				,[JC_IsSealOk]
				,[JC_ContainerMode]
				,[JC_ContainerJobID]
				,[JC_RC]
				,[JC_ContainerNum]
				,[JC_ContainerCount]
				,[JC_Purpose]
				,[JC_DeliveryMode]
				,[JC_TotalHeight]
				,[JC_TotalLength]
				,[JC_TotalWidth]
				,[JC_TotalUnitOfMeasure]
				,[JC_GrossWeight]
				,[JC_GrossWeightUQ]
				,[JC_TareWeight]
				,[JC_VolumeCapacityUQ]
				,[JC_WeightCapacityUQ]
				,[JC_JK]
				,[JC_SystemCreateUser]
				,[JC_SystemCreateTimeUtc]
				,[JC_SystemLastEditUser]
				,[JC_SystemLastEditTimeUtc]
				,[JC_SellSpotRateMode]
				,[JC_CostSpotRateMode]
				,[JC_GatewaySellSpotRateMode])
				
				VALUES
				
				('{0}'
				,1
				,'FCL' 
				,'D00002000'
				,'B43DB8A6-A562-4A9A-840B-84DE2DF65E3A'
				,'XYZ123'
				,1
				,'CFS'
				,'CY/ CY'
				,8.500
				,40.000
				,8.000
				,'FT'
				,7572.000
				,'KG'
				,4350.000
				,'M3'
				,'KG'
				,'{1}'
				,'E'
				,'2017-05-16 05:50:00'
				,'E'
				,'2017-05-16 05:53:00'
				,'STD'
				,'STD'
				,'STD')", containerPKParam, consolPKParam);
		}

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

		string GetInsertConsolCommand(Guid consolPKParam, Guid orgAddressPKParam)
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
				,'SEA'
				,'FCL'
				,'AGT'
				,'STD'
				,1
				,'C00002000'
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
				,'SAAQI')", consolPKParam, orgAddressPKParam);
		}

		string GetInsertPacklLineCommand(Guid packLinePKParam, Guid shipmentPKParam)
		{
			return string.Format(@"
				INSERT INTO dbo.JobPackLines
				([JL_PK]
				,[JL_IsValid]
				,[JL_FreightMode]
				,[JL_PackageCount]
				,[JL_F3_NKPackType]
				,[JL_ContainerPackingOrder]
				,[JL_ActualWeight]
				,[JL_ActualWeightUQ]
				,[JL_UnitOfDimension]
				,[JL_ActualVolume]
				,[JL_ActualVolumeUQ]
				,[JL_Description]
				,[JL_RH_NKCommodityCode]
				,[JL_JS])
				
				VALUES
				
				('{0}'
				,1
				,'OUT'
				,45
				,'PCE'
				,3
				,2222.000
				,'KG'
				,'M'
				,22.000
				,'M3'
				,'goods'
				,'GEN'
				,'{1}')", packLinePKParam, shipmentPK);
		}

		string GetInsertPackPivotCommand(Guid packLine1PKParam, Guid containerPKParam, Guid packPivot1PKParam)
		{
			return string.Format(@"
				INSERT INTO dbo.JobContainerPackPivot
				([J6_PK]
				,[J6_JC]
				,[J6_JL])
				
				VALUES
				
				('{0}'
				,'{1}'
				,'{2}')", packPivot1PKParam, containerPKParam, packLine1PKParam);
		}
	}
}

