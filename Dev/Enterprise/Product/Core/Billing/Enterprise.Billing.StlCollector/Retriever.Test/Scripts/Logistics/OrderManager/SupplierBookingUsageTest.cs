using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(SupplierBookingUsage))]
	sealed class SupplierBookingUsageTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			var sqlText = @"
				DECLARE @companyID UNIQUEIDENTIFIER = newid();
				DECLARE @supplierBookingPK01 UNIQUEIDENTIFIER = newid();
				DECLARE @supplierBookingPK02 UNIQUEIDENTIFIER = newid();
				DECLARE @organisationPK UNIQUEIDENTIFIER = (SELECT TOP (1) OH_PK FROM dbo.OrgHeader);

				INSERT INTO dbo.GlbCompany
					(GC_PK, GC_Code, GC_Name)
				VALUES
					(@companyID, 'TG1', 'TG company');

				INSERT INTO dbo.GlbBranch
					(GB_PK, GB_Code, GB_GC)
				VALUES
					(NEWID(), 'TD1', @companyID);

				INSERT INTO dbo.JobSupplierBooking
					(JSB_PK, JSB_BookingId, JSB_TransportMode, JSB_LoadMode, JSB_OH_BookingParty, JSB_SystemCreateTimeUtc, JSB_SystemCreateUser, JSB_SystemLastEditTimeUtc, JSB_SystemLastEditUser)
				VALUES
					(@supplierBookingPK01, 'JSB01', 'SEA', 'CY', @organisationPK, '2014-11-30', 'US1', GetUtcDate(), '~BP'),
					(@supplierBookingPK02, 'JSB02', 'SEA', 'CFS', @organisationPK, '2014-11-30', 'US1', GetUtcDate(), '~BP'),
					(newid(),              'JSB03', 'SEA', 'CY', @organisationPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT INTO dbo.StmALog
					(SL_PK, SL_Table, SL_Parent, SL_SE_NKEvent, SL_PostedTimeUtc, SL_EventTime, SL_GS_NKUser, SL_GB_NKBranch)
				VALUES
					(newid(), 'JobSupplierBooking', @supplierBookingPK01, 'ADD', '2014-10-01', '2014-10-01', 'US1', 'TD1'),
					(newid(), 'JobSupplierBooking', @supplierBookingPK01, 'MSN', '2014-10-01', '2014-10-01', 'US1', 'TD1'),
					(newid(), 'JobSupplierBooking', @supplierBookingPK02, 'ADD', '2014-10-01', '2014-10-01', 'US1', 'TD1'),
					(newid(), 'JobSupplierBooking', @supplierBookingPK02, 'MSN', '2014-10-01', '2014-10-01', 'US1', 'TD1'),
					(newid(), 'JobDocumentData',	newid(),			  'ISN', '2014-10-01', '2014-10-01', 'US1', 'TD1'),
					(newid(), 'JobShipment',		newid(),			  'ADD', '2014-10-01', '2014-10-01', 'GB0', 'N');
";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Transactions", 2, transactions.Count());
				AssertRow(transactions.Single(t => t.Reference1 == "JSB01"), "0", "TG1", "TD1", new DateTime(2014, 10, 1), "US1", 1, "JSB01", null, null, null);
				AssertRow(transactions.Single(t => t.Reference1 == "JSB02"), "1", "TG1", "TD1", new DateTime(2014, 10, 1), "US1", 1, "JSB02", null, null, null);
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
