using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(HVLVItemPlusUsageLegacy))]
	sealed class HVLVItemPlusUsageLegacyTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2021, 6);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 5, transactions.Count());
			AssertRow(transactions.Single(t => t.Reference1 == "TestHVI001" && t.ServiceOccuredUTC == new DateTime(2021, 6, 1)), "0", "TG1", "BG1", new DateTime(2021, 6, 1), string.Empty, 1, "TestHVI001", "TestBarcode001", null, "TestShipperRef001");
			AssertRow(transactions.Single(t => t.Reference1 == "TestHVI005" && t.ServiceOccuredUTC == new DateTime(2021, 6, 1)), "1", "TG1", "BG1", new DateTime(2021, 6, 1), string.Empty, 1, "TestHVI005", "TestBarcode005", null, "TestShipperRef005");
			AssertRow(transactions.Single(t => t.Reference1 == "TestHVI005" && t.ServiceOccuredUTC == new DateTime(2021, 6, 5)), "2", "TG2", "BG2", new DateTime(2021, 6, 5), string.Empty, 1, "TestHVI005", "TestBarcode005", null, "TestShipperRef005");
			AssertRow(transactions.Single(t => t.Reference1 == "TestHVI007" && t.ServiceOccuredUTC == new DateTime(2021, 6, 4)), "3", "TG3", "BG3", new DateTime(2021, 6, 4), string.Empty, 1, "TestHVI007", "TestBarcode007", null, "TestShipperRef007");
			AssertRow(transactions.Single(t => t.Reference1 == "TestHVI008" && t.ServiceOccuredUTC == new DateTime(2021, 6, 11)), "4", "TG3", "BG3", new DateTime(2021, 6, 11), string.Empty, 1, "TestHVI008", "TestBarcode008", null, "TestShipperRef008");
		}

		protected override void PrepareTestData()
		{
			TestConnection.ExecuteNonQuery("ALTER TABLE dbo.HVLVItem DISABLE TRIGGER [TG_HVLVItem_PopulateUsageTimes_InsertUpdate];");

			var sqlText = @"
				DECLARE @ClusterKey int = 99;

				DECLARE @GlbCompanyID1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @GlbCompanyID2 UNIQUEIDENTIFIER = NEWID();
				DECLARE @GlbCompanyID3 UNIQUEIDENTIFIER = NEWID();
				DECLARE @GlbCompanyID4 UNIQUEIDENTIFIER = NEWID();

				DECLARE @BookingHeaderPK UNIQUEIDENTIFIER = NEWID();
				DECLARE @ConsignmentPK UNIQUEIDENTIFIER = NEWID();

				DECLARE @Shipment1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @Shipment2 UNIQUEIDENTIFIER = NEWID();
				DECLARE @Shipment3 UNIQUEIDENTIFIER = NEWID();
				DECLARE @Shipment4 UNIQUEIDENTIFIER = NEWID();
				DECLARE @Shipment5 UNIQUEIDENTIFIER = NEWID();
				DECLARE @Shipment6 UNIQUEIDENTIFIER = NEWID();
				DECLARE @Shipment7 UNIQUEIDENTIFIER = NEWID();
				DECLARE @Shipment8 UNIQUEIDENTIFIER = NEWID();

				DECLARE @StandardItem1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @StandardItem2 UNIQUEIDENTIFIER = NEWID();
				DECLARE @StandardItem3 UNIQUEIDENTIFIER = NEWID();
				DECLARE @StandardItem4 UNIQUEIDENTIFIER = NEWID();
				DECLARE @PlusItem1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @PlusItem2 UNIQUEIDENTIFIER = NEWID();
				DECLARE @PlusItem3 UNIQUEIDENTIFIER = NEWID();
				DECLARE @PlusItem4 UNIQUEIDENTIFIER = NEWID();

				INSERT INTO dbo.GlbCompany
					(GC_PK, GC_Code, GC_Name)
				VALUES
					(@GlbCompanyID1, 'TG1', 'TG company1'),
					(@GlbCompanyID2, 'TG2', 'TG company2'),
					(@GlbCompanyID3, 'TG3', 'TG company3')

				INSERT INTO dbo.GlbBranch
					(GB_PK, GB_Code, GB_GC)
				VALUES
					(NEWID(), 'BG1', @GlbCompanyID1),
					(NEWID(), 'BG2', @GlbCompanyID2),
					(NEWID(), 'BG3', @GlbCompanyID3)

				INSERT dbo.HVLVBookingHeader
					(HVH_PK, HVH_BookingReference, HVH_ClusterKey, HVH_OA_BillToParty, HVH_SystemCreateTimeUtc, HVH_SystemLastEditTimeUtc, HVH_SystemCreateUser, HVH_SystemLastEditUser)
				VALUES
					(@BookingHeaderPK, 'TESTBOOKINGHEADER', @ClusterKey, (Select Top 1 OA_PK from dbo.OrgAddress), '2021-05-30', '2021-05-30', '~BP', '~BP')

				INSERT dbo.JobShipment
					(JS_PK, JS_UniqueConsignRef, JS_ShipmentType)
				VALUES
					(@Shipment1, 'StandardShipment1', 'HVL'),
					(@Shipment2, 'StandardShipment2', 'HVL'),
					(@Shipment3, 'StandardShipment3', 'HVL'),
					(@Shipment4, 'StandardShipment4', 'HVL'),
					(@Shipment5, 'StandardShipment5', 'HVL'),
					(@Shipment6, 'StandardShipment6', 'HVL'),
					(@Shipment7, 'StandardShipment7', 'HVL'),
					(@Shipment8, 'StandardShipment8', 'HVL')

				INSERT dbo.HVLVConsignment
					(HVC_PK, HVC_ConsignmentId, HVC_HVH_BookingHeader, HVC_ClusterKey, HVC_Status, HVC_SystemCreateTimeUtc, HVC_SystemLastEditTimeUtc, HVC_SystemCreateUser, HVC_SystemLastEditUser)
				VALUES
					(@ConsignmentPK, 'TESTHVC001', @BookingHeaderPK, @ClusterKey, 'CLR', '2021-05-30', '2021-05-30', '~BP', '~BP')

				INSERT dbo.HVLVItem
					(HVI_PK, HVI_ItemId, HVI_JS_LoadedOnShipment, HVI_CurrentBarcode, HVI_ShipperReference, HVI_ClusterKey, HVI_HVC_Consignment, HVI_UsageType, HVI_DestinationFirstUsageTimeUtc, HVI_SecurityFilingFirstUsageTimeUtc, HVI_SystemCreateTimeUtc, HVI_SystemLastEditTimeUtc, HVI_SystemCreateUser, HVI_SystemLastEditUser)
				VALUES 
					(@StandardItem1, 'TestHVI001', @Shipment1, 'TestBarcode001', 'TestShipperRef001', @ClusterKey, @ConsignmentPK,                         'S',                     '2021-06-03',                        '2021-06-01',            '2021-05-30',              '2021-06-05', '~BP', '~BP'),
					(@StandardItem2, 'TestHVI002',       NULL, 'TestBarcode002', 'TestShipperRef002', @ClusterKey, @ConsignmentPK,                         'S',                             NULL,                                NULL,            '2021-05-30',              '2021-06-04', '~BP', '~BP'),
					(@StandardItem3, 'TestHVI003', @Shipment3, 'TestBarcode003', 'TestShipperRef003', @ClusterKey, @ConsignmentPK,                         'S',                             NULL,                        '2021-05-31',            '2021-05-30',              '2021-06-05', '~BP', '~BP'),
					(@StandardItem4, 'TestHVI004', @Shipment4, 'TestBarcode004', 'TestShipperRef004', @ClusterKey, @ConsignmentPK,                         'S',                             NULL,                        '2021-07-01',            '2021-05-30',              '2021-06-22', '~BP', '~BP'),
					(@PlusItem1    , 'TestHVI005', @Shipment5, 'TestBarcode005', 'TestShipperRef005', @ClusterKey, @ConsignmentPK,                         'P',                     '2021-06-01',                        '2021-06-03',            '2021-05-30',              '2021-06-05', '~BP', '~BP'),
					(@PlusItem2    , 'TestHVI006',       NULL, 'TestBarcode006', 'TestShipperRef006', @ClusterKey, @ConsignmentPK,                         'P',                             NULL,                                NULL,            '2021-05-30',              '2021-06-05', '~BP', '~BP'),
					(@PlusItem3    , 'TestHVI007', @Shipment7, 'TestBarcode007', 'TestShipperRef007', @ClusterKey, @ConsignmentPK,                         'P',                     '2021-05-31',                                NULL,            '2021-05-30',              '2021-06-04', '~BP', '~BP'),
					(@PlusItem4    , 'TestHVI008', @Shipment8, 'TestBarcode008', 'TestShipperRef008', @ClusterKey, @ConsignmentPK,                         'P',                     '2021-07-01',                                NULL,            '2021-05-30',              '2021-06-05', '~BP', '~BP')

				INSERT INTO dbo.StmALog
					(SL_PK, SL_Table, SL_Parent, SL_EventTime, SL_PostedTimeUtc, SL_SE_NKEvent, SL_GB_NKBranch)
				VALUES
					(NEWID(), 'JobShipment', @Shipment1, '2021-06-03', '2021-06-03', 'ADD', 'BG1'), 
					(NEWID(), 'JobShipment', @Shipment2, '2021-06-03', '2021-06-03', 'ADD', 'BG1'), 
					(NEWID(), 'JobShipment', @Shipment3, '2021-06-03', '2021-06-03', 'ADD', 'BG1'), 
					(NEWID(), 'JobShipment', @Shipment4, '2021-06-03', '2021-06-03', 'ADD', 'BG1'),
					(NEWID(), 'JobShipment', @Shipment5, '2021-06-03', '2021-06-03', 'ADD', 'BG1'), 
					(NEWID(), 'JobShipment', @Shipment6, '2021-06-03', '2021-06-03', 'ADD', 'BG1'),
					(NEWID(), 'JobShipment', @Shipment7, '2021-06-03', '2021-06-03', 'ADD', 'BG1'),
					(NEWID(), 'JobShipment', @Shipment8, '2021-06-03', '2021-06-03', 'ADD', 'BG1')

				INSERT INTO dbo.StmALog
					(SL_PK, SL_Table, SL_Parent, SL_EventTime, SL_PostedTimeUtc, SL_SE_NKEvent, SL_GB_NKBranch)
				VALUES
					(NEWID(), 'HVLVItem', @StandardItem1, '2021-06-03', '2021-06-03', 'ADD', 'BG1'), /* Collect because SecurityFilingUsageTime is within TestDateTimeRange                                                    */
					(NEWID(), 'HVLVItem', @StandardItem2, '2021-06-03', '2021-06-03', 'ADD', 'BG1'), /* Do not collect because StandardItem2.HVI_DestinationFirstUsageTimeUtc is null                                               */
					(NEWID(), 'HVLVItem', @StandardItem3, '2021-06-03', '2021-06-03', 'ADD', 'BG1'), /* Do not collect because StandardItem3.HVI_DestinationFirstUsageTimeUtc is outside of TestDateTimeRange                       */
					(NEWID(), 'HVLVItem', @StandardItem4, '2021-06-03', '2021-06-03', 'ADD', 'BG1'), /* Do not collect because StandardItem4.HVI_DestinationFirstUsageTimeUtc is outside of TestDateTimeRange                       */

					(NEWID(), 'HVLVItem', @PlusItem1    , '2021-06-03', '2021-06-03', 'ADD', 'BG1'), /* Collect because DestinationUsageTime is within TestDateTimeRange                                                       */
					(NEWID(), 'HVLVItem', @PlusItem2    , '2021-06-03', '2021-06-03', 'ADD', 'BG1'), /* Do not collect because PlusItem2.SecurityFilingUsageTime is null									                           */
					(NEWID(), 'HVLVItem', @PlusItem3    , '2021-06-03', '2021-06-03', 'ADD', 'BG1'), /* Do not collect because PlusItem3.SecurityFilingUsageTime is outside of TestDateTimeRange                                    */
					(NEWID(), 'HVLVItem', @PlusItem4    , '2021-06-03', '2021-06-03', 'ADD', 'BG1'), /* Do not collect because PlusItem4.SecurityFilingUsageTime is outside of TestDateTimeRange                                    */

					(NEWID(), 'HVLVItem', @StandardItem1, '2021-06-04', '2021-06-04', 'EDT', 'BG1'), /* Do not collect because StandardItem1 was created by BG1. BG1 can edit it free of charge and because of same reason below    */
					(NEWID(), 'HVLVItem', @StandardItem1, '2021-06-05', '2021-06-05', 'EDT', 'BG2'), /* Do not collect because StandardItem1 is a 'S' type item in HVLVItemPlusUsage collector                                      */

					(NEWID(), 'HVLVItem', @PlusItem1    , '2021-06-04', '2021-06-04', 'EDT', 'BG1'), /* Do not collect because PlusItem1 was created by BG1. BG1 can edit it free of charge                                         */
					(NEWID(), 'HVLVItem', @PlusItem1    , '2021-06-05', '2021-06-05', 'EDT', 'BG2'), /* Collect them because BG2 is not the company that created PlusItem1. So charge BG2                                           */

					(NEWID(), 'HVLVItem', @StandardItem3, '2021-06-04', '2021-06-04', 'EDT', 'BG3'), /* Do not collect because StandardItem3 is a 'S' type item in HVLVItemPlusUsage collector and because of same reason below     */
					(NEWID(), 'HVLVItem', @StandardItem3, '2021-06-05', '2021-06-05', 'EDT', 'BG3'), /* Do not collect because BG3 has already edited StandardItem3. So don't double charge BG3                                     */

					(NEWID(), 'HVLVItem', @StandardItem4, '2021-06-11', '2021-06-11', 'EDT', 'BG3'), /* Do not collect because StandardItem4 is a 'S' type item in HVLVItemPlusUsage collector                                      */
					(NEWID(), 'HVLVItem', @StandardItem4, '2021-06-22', '2021-06-22', 'EDT', 'BG3'), /* Do not collect because BG3 has already edited StandardItem4. So don't double charge BG3                                     */

					(NEWID(), 'HVLVItem', @PlusItem3    , '2021-06-04', '2021-06-04', 'EDT', 'BG3'), /* Collect them because BG3 is not the company that created PlusItem3, so charge BG3                                           */
					(NEWID(), 'HVLVItem', @PlusItem3    , '2021-06-05', '2021-06-05', 'EDT', 'BG3'), /* Do not collect because BG3 has already edited PlusItem3. So don't double charge BG3                                         */

					(NEWID(), 'HVLVItem', @PlusItem4    , '2021-06-11', '2021-06-11', 'EDT', 'BG3'), /* Collect them because BG3 is not the company that created PlusItem4, so charge BG3                                           */
					(NEWID(), 'HVLVItem', @PlusItem4    , '2021-06-12', '2021-06-12', 'EDT', 'BG3'), /* Do not collect because BG3 has already edited PlusItem4. So don't double charge BG3                                         */

					(NEWID(), 'HVLVItem', @PlusItem3    , '2021-07-01', '2021-07-01', 'EDT', 'BG3'), /* Do not collect because it's outside the TestDateTimeRange                                                                   */
					(NEWID(), 'HVLVItem', @PlusItem4    , '2021-07-02', '2021-07-02', 'EDT', 'BG3'), /* Do not collect because it's outside the TestDateTimeRange                                                                   */

					(NEWID(), 'HVLVItem', @StandardItem2, '2021-06-04', '2021-06-04', 'EDT', 'BG3'), /* Do not collect because its HVI_SecurityFilingFirstUsageTimeUtc has not yet been populated                                   */
					(NEWID(), 'HVLVItem', @PlusItem2    , '2021-06-05', '2021-06-05', 'EDT', 'BG3')  /* Do not collect because its HVI_DestinationFirstUsageTimeUtc has not yet been populated                                      */
				";

			TestConnection.ExecuteNonQuery(sqlText);

			TestConnection.ExecuteNonQuery("ALTER TABLE dbo.HVLVItem ENABLE TRIGGER [TG_HVLVItem_PopulateUsageTimes_InsertUpdate];");
		}
	}
}
