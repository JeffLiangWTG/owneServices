using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ForwarderGeneralShipmentDisbursement))]
	sealed class ForwarderGeneralShipmentDisbursementTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			var sqlText = @"

DECLARE @InvoicePk UNIQUEIDENTIFIER = '00000001-0000-0000-0000-000000000001';
DECLARE @APLinePk UNIQUEIDENTIFIER = '00000001-0000-0000-0000-000000000002';
DECLARE @ARLinePk UNIQUEIDENTIFIER = '00000001-0000-0000-0000-000000000003';
DECLARE @JobHeaderPK UNIQUEIDENTIFIER = '00000001-0000-0000-0000-000000000004';
DECLARE @GlobalChargeCodePk UNIQUEIDENTIFIER = '00000001-0000-0000-0000-000000000005';
DECLARE @CompanyChargeCodePk UNIQUEIDENTIFIER = '00000001-0000-0000-0000-000000000006';

DECLARE @JsPk01 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk02 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk03 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk04 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk05 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk06 UNIQUEIDENTIFIER = newid();
DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'DEM');
DECLARE @GePk UNIQUEIDENTIFIER = (SELECT TOP(1) GE_PK FROM dbo.GlbDepartment);
INSERT dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_IsForwardRegistered, JS_ISBooking) VALUES
	(@JsPk01, 'SHP01', 1, 0),
	(@JsPk02, 'SHP02', 1, 0),
	(newid(), 'SHP03', 1, 0),
	(@JsPk03, 'SHP04', 0, 1),
	(@JsPk04, 'SHP05', 0, 1),
	(newid(), 'SHP06', 0, 1),
	(@JsPk05, 'SHP07', 1, 1),
	(@JsPk06, 'SHP08', 1, 1),
	(newid(), 'SHP09', 1, 1);
INSERT dbo.JobHeader (JH_PK, JH_GC, JH_ParentID, JH_JobNum, JH_SystemCreateTimeUtc, JH_GB, JH_GE, JH_SystemCreateUser, JH_Status, JH_IsDisbursement, JH_Direction) VALUES
	(newid(), @GcPk, @JsPk01, 'SHP01', '2014-01-01', @GbPk, @GePk, 'US1', 'WRK', 0, 'DOM'),
	(newid(), @GcPk, @JsPk02, 'SHP02', '2013-12-04', @GbPk, @GePk, 'US2', 'WRK', 1, ''),
	(newid(), @GcPk, @JsPk03, 'SHP04', '2014-01-01', @GbPk, @GePk, 'US1', 'WRK', 0, ''),
	(@JobHeaderPK, @GcPk, @JsPk04, 'SHP05', '2013-12-04', @GbPk, @GePk, 'US2', 'WRK', 1, 'IMP'),
	(newid(), @GcPk, @JsPk05, 'SHP07', '2014-01-01', @GbPk, @GePk, 'US1', 'WRK', 0, 'EXP'),
	(newid(), @GcPk, @JsPk06, 'SHP08', '2013-12-04', @GbPk, @GePk, 'US2', 'WRK', 1, '');

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal) VALUES
(@InvoicePk, @GcPk, @GbPk, @GePk, '2013-12-04', 'AP', 'JRJ', '001', '2013-12-04', 'UT', 'AUD', 66.00)

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_GC, AL_GB, AL_GE, AL_LineType, AL_LineAmount, AL_OSAmount, AL_RX_NKTransactionCurrency, AL_SystemCreateTimeUtc, AL_SystemCreateUser) VALUES
(@APLinePk, @InvoicePk, @GcPk, @GbPk, @GePk, 'CST', 44.0, 44.00, 'AUD', '2013-12-04', 'UT')

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_GC, AL_GB, AL_GE, AL_LineType, AL_LineAmount, AL_OSAmount, AL_RX_NKTransactionCurrency, AL_SystemCreateTimeUtc, AL_SystemCreateUser) VALUES
(@ARLinePk, null, @GcPk, @GbPk, @GePk, 'CST', 44.0, 44.00, 'AUD', '2013-12-04', 'UT')

--global charge code
INSERT INTO dbo.AccChargeCode (AC_PK, AC_Code, AC_ChargeGroup, AC_SystemCreateTimeUtc, AC_SystemCreateUser) VALUES
(@GlobalChargeCodePk, 'CCC', 'FRT', '2020-03-11 23:59', 'UT')

-- GC's charge code
INSERT INTO dbo.AccChargeCode (AC_PK, AC_GC, AC_Code, AC_ChargeGroup, AC_SystemCreateTimeUtc, AC_SystemCreateUser) VALUES
(@CompanyChargeCodePk, @GcPk, 'CCC', 'FRT', '2020-03-11 23:59', 'UT')

INSERT INTO dbo.JobCharge (JR_PK, JR_GC, JR_GB, JR_GE, JR_AL_ARLine, JR_AC, JR_AL_APLine, JR_JH, JR_SystemCreateTimeUtc, JR_SystemCreateUser, JR_RX_NKCostCurrency, JR_OSCostAmt) VALUES
(newid(), @GcPk, @GbPk, @GePk, @ARLinePk, @CompanyChargeCodePk, @APLinePk, @JobHeaderPK, '2013-12-04', 'UT', 'USD', 100)

DELETE dbo.StmData where SD_Name = 'ElectronicProcessingChargeCode';
INSERT dbo.StmData (SD_PK, SD_Name, SD_BinaryValue) VALUES (newid(), 'ElectronicProcessingChargeCode', convert(varbinary(max),  cast(@GlobalChargeCodePk as NVARCHAR(MAX))));

";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());
			var transaction = FindRowByRef1(transactions, "SHP05");
			AssertEquals("CompanyCode", "DEM", transaction.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2013, 12, 4), transaction.ServiceOccuredUTC);
			AssertEquals("UserCode", "UT", transaction.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction.BillableCount);
			AssertEquals("TransactionReference02", "USD", transaction.Reference2);
			AssertEquals("TransactionReference03", "100.00", transaction.Reference3);
			AssertEquals("TransactionReference04", "IMP", transaction.Reference4);
			AssertEquals("TransactionReference05", "00000001-0000-0000-0000-000000000004", transaction.Reference5);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2013, 12);
			}
		}
	}
}
