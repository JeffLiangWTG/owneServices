using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(SupplierBookingApprovalUsage))]
	sealed class SupplierBookingApprovalUsageTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			var sqlText = @"
				DECLARE @companyID UNIQUEIDENTIFIER = newid();
				DECLARE @supplierBookingPK01 UNIQUEIDENTIFIER = newid(); -- No BKC event, not counted
				DECLARE @supplierBookingPK02 UNIQUEIDENTIFIER = newid(); -- BKC event counted
				DECLARE @supplierBookingPK03 UNIQUEIDENTIFIER = newid(); -- Multiple BKC events, counted only once
				DECLARE @supplierBookingPK04 UNIQUEIDENTIFIER = newid(); -- BKC event after time range, not counted
				DECLARE @supplierBookingPK05 UNIQUEIDENTIFIER = newid(); -- Previous BKC event exists, current one not counted
				DECLARE @supplierBookingPK06 UNIQUEIDENTIFIER = newid(); -- BKC event before time range, not counted
				DECLARE @supplierBookingPK07 UNIQUEIDENTIFIER = newid(); -- Edge case: BKC event at the same time are sorted by PK
				DECLARE @organisationPK UNIQUEIDENTIFIER = (SELECT TOP (1) OH_PK FROM dbo.OrgHeader);
				DECLARE @SmallGUID uniqueidentifier = '00000000-0000-0000-0000-000000000001';
				DECLARE @LargeGUID uniqueidentifier = '00000000-0000-0000-0000-000000000002';

				INSERT INTO dbo.GlbCompany
					(GC_PK, GC_Code, GC_Name)
				VALUES
					(@companyID, 'TG1', 'TG company');

				INSERT INTO dbo.GlbBranch
					(GB_PK, GB_Code, GB_GC)
				VALUES
				  (NEWID(), 'TD1', @companyID),
				  (NEWID(), 'TD2', @companyID);

				INSERT INTO dbo.JobSupplierBooking
					(JSB_PK, JSB_BookingId, JSB_TransportMode, JSB_LoadMode, JSB_OH_BookingParty, JSB_SystemCreateTimeUtc, JSB_SystemCreateUser, JSB_SystemLastEditTimeUtc, JSB_SystemLastEditUser)
				VALUES
					(@supplierBookingPK01, 'JSB01', 'SEA', 'CY',  @organisationPK, '2014-10-01', 'US1', GetUtcDate(), '~BP'),
					(@supplierBookingPK02, 'JSB02', 'SEA', 'CFS', @organisationPK, '2014-10-01', 'US1', GetUtcDate(), '~BP'),
					(@supplierBookingPK03, 'JSB03', 'SEA', 'CFS', @organisationPK, '2014-10-01', 'US1', GetUtcDate(), '~BP'),
					(@supplierBookingPK04, 'JSB04', 'SEA', 'CY',  @organisationPK, '2014-10-01', 'US1', GetUtcDate(), '~BP'),
					(@supplierBookingPK05, 'JSB05', 'SEA', 'CY',  @organisationPK, '1999-01-01', 'US1', GetUtcDate(), '~BP'),
					(@supplierBookingPK06, 'JSB06', 'SEA', 'CY',  @organisationPK, '2014-10-01', 'US1', GetUtcDate(), '~BP'),
					(@supplierBookingPK07, 'JSB07', 'SEA', 'CY',  @organisationPK, '2014-10-01', 'US1', GetUtcDate(), '~BP');

				INSERT INTO dbo.StmALog
					(SL_PK, SL_Table, SL_Parent, SL_SE_NKEvent, SL_PostedTimeUtc, SL_EventTime, SL_GS_NKUser, SL_GB_NKBranch)
				VALUES
					(newid(), 'JobSupplierBooking', @supplierBookingPK01, 'ADD', '2014-10-01', '2014-10-01', 'US1', 'TD1'),
					(newid(), 'JobSupplierBooking', @supplierBookingPK01, 'MSN', '2014-10-01', '2014-10-01', 'US1', 'TD1'),
					(newid(), 'JobSupplierBooking', @supplierBookingPK02, 'ADD', '2014-10-01', '2014-10-01', 'US1', 'TD1'),
					(newid(), 'JobSupplierBooking', @supplierBookingPK02, 'BKC', '2014-10-01', '2014-10-01', 'US1', 'TD1'),
					(newid(), 'JobSupplierBooking', @supplierBookingPK03, 'ADD', '2014-10-01 00:00', '2014-10-01 00:00', 'US1', 'TD1'),
					(newid(), 'JobSupplierBooking', @supplierBookingPK03, 'BKC', '2014-10-01 01:00', '2014-10-01 01:00', 'US1', 'TD1'),
					(newid(), 'JobSupplierBooking', @supplierBookingPK03, 'EDT', '2014-10-01 01:15', '2014-10-01 01:15', 'US1', 'TD1'),
					(newid(), 'JobSupplierBooking', @supplierBookingPK03, 'EDT', '2014-10-01 01:30', '2014-10-01 01:30', 'US1', 'TD1'),
					(newid(), 'JobSupplierBooking', @supplierBookingPK03, 'BKC', '2014-10-01 01:45', '2014-10-01 02:00', 'US1', 'TD1'),
					(newid(), 'JobSupplierBooking', @supplierBookingPK03, 'EDT', '2014-10-01 02:00', '2014-10-01 02:00', 'US2', 'TD2'),
					(newid(), 'JobSupplierBooking', @supplierBookingPK03, 'EDT', '2014-10-01 02:15', '2014-10-01 02:15', 'US2', 'TD2'),
					(newid(), 'JobSupplierBooking', @supplierBookingPK03, 'BKC', '2014-10-01 02:30', '2014-10-01 02:30', 'US2', 'TD2'),
					(newid(), 'JobSupplierBooking', @supplierBookingPK04, 'ADD', '2014-10-01', '2014-10-01', 'US1', 'TD1'),
					(newid(), 'JobSupplierBooking', @supplierBookingPK04, 'BKC', '2014-10-02', '2014-10-02', 'US1', 'TD1'),
					(newid(), 'JobSupplierBooking', @supplierBookingPK05, 'ADD', '1999-01-01', '1999-01-01', 'US1', 'TD1'),
					(newid(), 'JobSupplierBooking', @supplierBookingPK05, 'BKC', '1999-01-01', '1999-01-01', 'US1', 'TD1'),
					(newid(), 'JobSupplierBooking', @supplierBookingPK05, 'BKC', '2014-10-01', '2014-10-01', 'US1', 'TD1'),
					(newid(), 'JobSupplierBooking', @supplierBookingPK06, 'ADD', '2014-09-30', '2014-09-30', 'US1', 'TD1'),
					(newid(), 'JobSupplierBooking', @supplierBookingPK06, 'BKC', '2014-09-30', '2014-09-30', 'US1', 'TD1'),
					(newid(), 'JobSupplierBooking', @supplierBookingPK07, 'ADD', '2014-10-01', '2014-10-01', 'US1', 'TD1'),
					(@LargeGUID, 'JobSupplierBooking', @supplierBookingPK07, 'BKC', '2014-10-01', '2014-10-01', 'US2', 'TD2'),
					(@SmallGUID, 'JobSupplierBooking', @supplierBookingPK07, 'BKC', '2014-10-01', '2014-10-01', 'US1', 'TD1'),
					(newid(), 'JobDocumentData',	newid(),			  'BKC', '2014-10-01', '2014-10-01', 'US1', 'TD1'),
					(newid(), 'JobShipment',		newid(),			  'BKC', '2014-10-01', '2014-10-01', 'GB0', 'N');
";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Approval event of these bookings are collected",
					transactions.Select(t => t.Reference1), new string[] { "JSB02", "JSB03", "JSB07" });
				AssertRow(transactions.Single(t => t.Reference1 == "JSB02"), "2", "TG1", "TD1", new DateTime(2014, 10, 1), "US1", 1, "JSB02", null, null, null);
				AssertRow(transactions.Single(t => t.Reference1 == "JSB03"), "3", "TG1", "TD1", new DateTime(2014, 10, 1, 1, 0, 0), "US1", 1, "JSB03", null, null, null);
				AssertRow(transactions.Single(t => t.Reference1 == "JSB07"), "7", "TG1", "TD1", new DateTime(2014, 10, 1), "US1", 1, "JSB07", null, null, null);
			});
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				var startDate = new DateTime(2014, 10, 1);
				return new RecurringRange(startDate, startDate.AddDays(1));
			}
		}
	}
}
