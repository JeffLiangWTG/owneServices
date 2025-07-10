using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Scripts.Logistics.Yard;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(YardUnitsUnloaded))]
	sealed class YardUnitsUnloadedTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @CompanyPK UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @BranchPK UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @CompanyPK);
				DECLARE @AddressPK UNIQUEIDENTIFIER = (SELECT TOP (1) OA_PK FROM dbo.OrgAddress);
				DECLARE @WarehousePK UNIQUEIDENTIFIER = newid();
				DECLARE @AreaPK UNIQUEIDENTIFIER = newid();
				DECLARE @RowPK UNIQUEIDENTIFIER = newid();
				DECLARE @LocationPK UNIQUEIDENTIFIER = newid();

				INSERT dbo.WhsWarehouse (WW_PK, WW_GB_RelatedCompanyBranch, WW_OA_WarehouseAddress, WW_WarehouseCode, WW_IsVirtualWarehouse, WW_WLT_DefaultLocationType, WW_SystemCreateTimeUtc, WW_SystemCreateUser, WW_SystemLastEditTimeUtc, WW_SystemLastEditUser) VALUES
					(@WarehousePK, @BranchPK, @AddressPK, 'W1', 1, '16C9FD62-730A-42ED-A20E-699606FFF360', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				INSERT dbo.WhsArea (WA_PK, WA_WW_Whs, WA_Name, WA_SystemCreateTimeUtc, WA_SystemCreateUser, WA_SystemLastEditTimeUtc, WA_SystemLastEditUser) VALUES
					(@AreaPK, @WarehousePK, '~!TransitWarehouse!~', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				INSERT dbo.WhsRow (WR_PK, WR_WW_Whs, WR_Name, WR_Columns, WR_Levels, WR_Trays, WR_SystemLastEditTimeUtc, WR_SystemCreateTimeUtc, WR_SystemCreateUser, WR_SystemLastEditUser) VALUES
					(@RowPK, @WarehousePK, 'A', 1, 1, 1, GETUTCDATE(), GETUTCDATE(), '~BP', '~BP');
				INSERT dbo.WhsLocation (WL_PK, WL_WA_PickingArea, WL_WA_PutawayArea, WL_WR, WL_WLT_LocationType, WL_PutawayPathSequence, WL_SystemCreateTimeUtc, WL_SystemLastEditTimeUtc, WL_SystemCreateUser, WL_SystemLastEditUser) VALUES
					(@LocationPK, @AreaPK, @AreaPK, @RowPK, '16C9FD62-730A-42ED-A20E-699606FFF360', 1, GETUTCDATE(), GETUTCDATE(), '~BP', '~BP');

				DECLARE @TransportationUnitPK UNIQUEIDENTIFIER = newid();

				INSERT dbo.CYDTransportationUnit (YTU_PK, YTU_WL_WaitingBayLocation, YTU_SystemCreateTimeUtc, YTU_SystemCreateUser, YTU_SystemLastEditTimeUtc, YTU_SystemLastEditUser, YTU_GateInTime, YTU_GateOutTime, YTU_WW_Yard, YTU_TransportationUnitID) VALUES
					(@TransportationUnitPK, @LocationPK, '2024-09-10 17:02', '~BP', '2024-09-10 17:02', '~BP', '2024-09-10 17:02', '2024-09-10 17:02', @WarehousePK, 'TPU00000001');

				DECLARE @UnitLineItemPK01 UNIQUEIDENTIFIER = newid();
				DECLARE @UnitLineItemPK02 UNIQUEIDENTIFIER = newid();
				DECLARE @UnitLineItemPK03 UNIQUEIDENTIFIER = newid();
				DECLARE @UnitLineItemPK04 UNIQUEIDENTIFIER = newid();
				DECLARE @UnitLineItemPK05 UNIQUEIDENTIFIER = newid();
				DECLARE @UnitLineItemPK06 UNIQUEIDENTIFIER = newid();
				DECLARE @UnitLineItemPK07 UNIQUEIDENTIFIER = newid();


				INSERT dbo.CYDUnitLineItem (YLI_PK, YLI_Type, YLI_SystemCreateTimeUtc, YLI_SystemCreateUser, YLI_SystemLastEditTimeUtc, YLI_SystemLastEditUser) VALUES
					(@UnitLineItemPK01, 'CNT', '2024-09-10 17:02', '~BP', '2024-09-10 17:02', '~BP'),
					(@UnitLineItemPK02, 'CNT', '2024-09-10 17:02', '~BP', '2024-09-10 17:02', '~BP'),
					(@UnitLineItemPK03, 'CNT', '2024-09-10 17:02', '~BP', '2024-09-10 17:02', '~BP'),
					(@UnitLineItemPK04, 'CNT', '2024-09-10 17:02', '~BP', '2024-09-10 17:02', '~BP'),
					(@UnitLineItemPK05, 'CNT', '2024-09-10 17:02', '~BP', '2024-09-10 17:02', '~BP'),
					(@UnitLineItemPK06, 'CNT', '2024-09-10 17:02', '~BP', '2024-09-10 17:02', '~BP'),
					(@UnitLineItemPK07, 'CNT', '2024-09-10 17:02', '~BP', '2024-09-10 17:02', '~BP');

				DECLARE @ReceiveAdvicePK01 UNIQUEIDENTIFIER = newid();

				INSERT dbo.CYDReceiveAdvice (YRA_PK, YRA_WW_Yard, YRA_JobNumber, YRA_AcceptanceNumber, YRA_FromDate, YRA_ToDate, YRA_Mode, YRA_SystemCreateTimeUtc, YRA_SystemCreateUser, YRA_SystemLastEditTimeUtc, YRA_SystemLastEditUser) VALUES
					(@ReceiveAdvicePK01, @WarehousePK, 'JOB001', 'ACC01', '2024-09-10 17:02', '2060-09-10 17:02', 'EMT', '2024-09-10 17:02', '~BP', '2024-09-10 17:02', '~BP');
		
				DECLARE @ReceiveAdviceLinePK01 UNIQUEIDENTIFIER = newid();
				DECLARE @ReceiveAdviceLinePK02 UNIQUEIDENTIFIER = newid();

				INSERT dbo.CYDReceiveAdviceLine (YRL_PK, YRL_YRA_ReceiveAdvice, YRL_YLI_UnitLineItem, YRL_SystemCreateTimeUtc, YRL_SystemCreateUser, YRL_SystemLastEditTimeUtc, YRL_SystemLastEditUser) VALUES
					(@ReceiveAdviceLinePK01, @ReceiveAdvicePK01, @UnitLineItemPK05, '2024-09-10 17:02', '~BP', '2024-09-10 17:02', '~BP'),
					(@ReceiveAdviceLinePK02, @ReceiveAdvicePK01, @UnitLineItemPK06, '2024-09-10 17:02', '~BP', '2024-09-10 17:02', '~BP');

				DECLARE @DeliveryHeaderPK UNIQUEIDENTIFIER = newid();
				DECLARE @DeliveryPK01 UNIQUEIDENTIFIER = newid();
				DECLARE @DeliveryPK02 UNIQUEIDENTIFIER = newid();
 
                INSERT INTO dbo.CYDDeliveryHeader(YDH_PK, YDH_WW_Yard, YDH_JobNumber, YDH_SystemLastEditTimeUtc, YDH_SystemLastEditUser, YDH_SystemCreateTimeUtc, YDH_SystemCreateUser) VALUES
					(@DeliveryHeaderPK, @WarehousePK, 'JOB123', '2024-09-10 17:02', '~BP', '2024-09-10 17:02', '~BP');
 
                INSERT INTO dbo.CYDDelivery(YDL_PK, YDL_YDH_DeliveryHeader, YDL_YLI_UnitLineItem, YDL_DeliveryID, YDL_SystemLastEditTimeUtc, YDL_SystemLastEditUser, YDL_SystemCreateTimeUtc, YDL_SystemCreateUser) VALUES
					(@DeliveryPK01, @DeliveryHeaderPK, @UnitLineItemPK07, 'YDL000000900001', '2024-09-10 17:02', '~BP', '2024-09-10 17:02', '~BP'),
					(@DeliveryPK02, @DeliveryHeaderPK, @UnitLineItemPK07, 'YDL000000900002', '2024-09-10 17:02', '~BP', '2024-09-10 17:02', '~BP');

				DECLARE @YardUnitStatePK01 UNIQUEIDENTIFIER = newid();
				DECLARE @YardUnitStatePK02 UNIQUEIDENTIFIER = newid();
				DECLARE @YardUnitStatePK03 UNIQUEIDENTIFIER = newid();
				DECLARE @YardUnitStatePK04 UNIQUEIDENTIFIER = newid();

				INSERT dbo.CYDYardUnitState (YUS_PK, YUS_UnitID, YUS_YTU_ReceiveTransportationUnit, YUS_WW_CurrentYard, YUS_WL_CurrentYardLocation, YUS_SystemCreateTimeUtc, YUS_SystemCreateUser, YUS_SystemLastEditTimeUtc, YUS_SystemLastEditUser, YUS_GS_NKLoadUser, YUS_GS_NKUnloadUser, YUS_LoadTime, YUS_UnloadTime, YUS_YLI_UnitLineItem, YUS_YRL_ReceiveLine, YUS_YDL_Delivery) VALUES
					(@YardUnitStatePK01, 'AAAA0000001', @TransportationUnitPK, @WarehousePK, @LocationPK, '2024-09-10 17:02', '~BP', '2024-09-10 17:02', '~BP', '', '~BP', NULL, '2024-09-10 17:02', @UnitLineItemPK01, @ReceiveAdviceLinePK01, @DeliveryPK01),
					(@YardUnitStatePK02, 'AAAA0000002', @TransportationUnitPK, @WarehousePK, @LocationPK, '2024-09-10 17:02', '~BP', '2024-09-10 17:02', '~BP', '', '~BP', NULL, '2024-09-10 17:02', @UnitLineItemPK02, @ReceiveAdviceLinePK02, @DeliveryPK02),
					(@YardUnitStatePK03, 'AAAA0000003', @TransportationUnitPK, @WarehousePK, @LocationPK, '2024-09-10 17:02', '~BP', '2024-09-10 17:02', '~BP', '', '', NULL, NULL, @UnitLineItemPK03, null, null),
					(@YardUnitStatePK04, 'AAAA0000004', @TransportationUnitPK, @WarehousePK, @LocationPK, '2024-09-10 17:02', '~BP', '2024-09-10 17:02', '~BP', '', '', NULL, NULL, @UnitLineItemPK04, null, null);
				";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2024, 9);
			}
		}
	}
}
