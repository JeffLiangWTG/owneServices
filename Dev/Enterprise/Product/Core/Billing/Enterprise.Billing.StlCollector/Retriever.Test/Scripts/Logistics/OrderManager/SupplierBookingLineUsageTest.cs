using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(SupplierBookingLineUsage))]
	sealed class SupplierBookingLineUsageTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			var sqlText = @"
				DECLARE @companyID UNIQUEIDENTIFIER = newid();

				DECLARE @supplierBookingPK01 UNIQUEIDENTIFIER = newid();
				DECLARE @supplierBookingPK02 UNIQUEIDENTIFIER = newid();

				DECLARE @supplierBookingLinePK01 UNIQUEIDENTIFIER = newid();
				DECLARE @supplierBookingLinePK02 UNIQUEIDENTIFIER = newid();

				DECLARE @orderLinePK01 UNIQUEIDENTIFIER = newid();
				DECLARE @orderLinePK02 UNIQUEIDENTIFIER = newid();

				DECLARE @orderHeaderPK01 UNIQUEIDENTIFIER = newid();
				DECLARE @orderHeaderPK02 UNIQUEIDENTIFIER = newid();

				DECLARE @organisationPK UNIQUEIDENTIFIER = (SELECT TOP (1) OH_PK FROM dbo.OrgHeader);

				DECLARE @AddressPK1 UNIQUEIDENTIFIER = (SELECT TOP 1 OA_PK FROM dbo.OrgAddress ORDER BY OA_PK);
				DECLARE @AddressPK2 UNIQUEIDENTIFIER = (SELECT TOP 1 OA_PK FROM dbo.OrgAddress WHERE OA_PK > @AddressPK1 ORDER BY OA_PK);

				INSERT INTO dbo.GlbCompany
					(GC_PK, GC_Code, GC_Name)
				VALUES
					(@companyID, 'TG1', 'TG company');

				INSERT INTO dbo.GlbBranch
					(GB_PK, GB_Code, GB_GC)
				VALUES
					(NEWID(), 'TD1', @companyID);

				INSERT INTO dbo.JobOrderHeader
					(JD_PK, JD_OrderNumber, JD_OA_BuyerAddress, JD_SystemCreateTimeUtc, JD_SystemCreateUser, JD_SystemLastEditTimeUtc, JD_SystemLastEditUser)
				VALUES
					(@orderHeaderPK01, 'JD01', @AddressPK1, '2014-11-30', 'US1', GetUtcDate(), '~BP'),
					(@orderHeaderPK02, 'JD02', @AddressPK2, '2014-11-30', 'US1', GetUtcDate(), '~BP');

				INSERT INTO dbo.JobOrderLine
					(JO_PK,  JO_LineNo, JO_SubLineNo, JO_JD, JO_SystemCreateTimeUtc, JO_SystemCreateUser, JO_SystemLastEditTimeUtc, JO_SystemLastEditUser)
				VALUES
					(@orderLinePK01, '111', '111', @orderHeaderPK01, '2014-11-30', 'US1', GetUtcDate(), '~BP'),
					(@orderLinePK02, '111', '222', @orderHeaderPK02, '2014-11-30', 'US1', GetUtcDate(), '~BP');

				INSERT INTO dbo.JobSupplierBooking
					(JSB_PK, JSB_BookingId, JSB_TransportMode, JSB_LoadMode, JSB_OH_BookingParty, JSB_SystemCreateTimeUtc, JSB_SystemCreateUser, JSB_SystemLastEditTimeUtc, JSB_SystemLastEditUser)
				VALUES
					(@supplierBookingPK01, 'JSB01', 'SEA', 'CY', @organisationPK, '2014-11-30', 'US1', GetUtcDate(), '~BP'),
					(@supplierBookingPK02, 'JSB02', 'SEA', 'CFS', @organisationPK, '2014-11-30', 'US1', GetUtcDate(), '~BP'),
					(newid(),              'JSB03', 'SEA', 'CY', @organisationPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT INTO dbo.JobSupplierBookingLine
					(JSL_PK, JSL_JSB_Booking, JSL_JO_OrderLine, JSL_BookingLineId, JSL_SystemCreateTimeUtc, JSL_SystemCreateUser, JSL_SystemLastEditTimeUtc, JSL_SystemLastEditUser)
				VALUES
					(@supplierBookingLinePK01, @supplierBookingPK01, @orderLinePK01, 'JSL01', '2014-11-30', 'US1', GetUtcDate(), '~BP'),
					(@supplierBookingLinePK02, @supplierBookingPK02, @orderLinePK02, 'JSL02', '2014-11-30', 'US1', GetUtcDate(), '~BP');

				INSERT INTO dbo.StmALog
					(SL_PK, SL_Table, SL_Parent, SL_SE_NKEvent, SL_PostedTimeUtc, SL_EventTime, SL_GS_NKUser, SL_GB_NKBranch)
				VALUES
					(newid(), 'JobSupplierBookingLine', @supplierBookingLinePK01, 'ADD', '2014-10-01', '2014-10-01', 'US1', 'TD1'),
					(newid(), 'JobSupplierBookingLine', @supplierBookingLinePK01, 'MSN', '2014-10-01', '2014-10-01', 'US1', 'TD1'),
					(newid(), 'JobSupplierBookingLine', @supplierBookingLinePK02, 'ADD', '2014-10-01', '2014-10-01', 'US1', 'TD1'),
					(newid(), 'JobSupplierBookingLine', @supplierBookingLinePK02, 'MSN', '2014-10-01', '2014-10-01', 'US1', 'TD1'),
					(newid(), 'JobDocumentData',		newid(),				  'ISN', '2014-10-01', '2014-10-01', 'US1', 'TD1'),
					(newid(), 'JobShipment',			newid(),				  'ADD', '2014-10-01', '2014-10-01', 'GB0', 'N');;
";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Transactions", 2, transactions.Count());
				AssertRow(transactions.Single(t => t.Reference1 == "JSL01"), "0", "TG1", "TD1", new DateTime(2014, 10, 1), "US1", 1, "JSL01", "JSB01", null, null);
				AssertRow(transactions.Single(t => t.Reference1 == "JSL02"), "1", "TG1", "TD1", new DateTime(2014, 10, 1), "US1", 1, "JSL02", "JSB02", null, null);
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
