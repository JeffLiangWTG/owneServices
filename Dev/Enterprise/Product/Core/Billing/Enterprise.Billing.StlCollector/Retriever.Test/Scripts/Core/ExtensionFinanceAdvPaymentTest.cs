using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Core;
using CargoWise.Data;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Core
{
	[TestedType(typeof(ExtensionFinanceAdvPayment))]
	sealed class ExtensionFinanceAdvPaymentTest : RefStlScriptWithDefaultsTest
	{
		/// <summary>
		/// GenExportBatchSequence is unique by 
		/// Cannot insert duplicate key row in object 'dbo.GenExportBatchSequence'
		/// with unique index 'NR_UX__XB_ParentID_XB_SubSystem_XB_Type'.
		/// </summary>
		public void TestGenExportBatchSequenceUniqueByParentAndType()
		{
			string sqlText = @"
				DECLARE @ParentPk UNIQUEIDENTIFIER = newid();
				INSERT dbo.GenExportBatchSequence (XB_PK, XB_ParentID, XB_Type) VALUES
					(newid(), @ParentPk, 'PPF'),
					(newid(), @ParentPk, 'PPF');";

			try
			{
				TestConnection.ExecuteNonQuery(sqlText);
				Fail("An exception should be thrown");
			}
			catch (SqlException ex)
			{
				var error = new DbErrorMatch(ex);
				AssertEquals("Exception caught", DbErrorType.CannotInsertDuplicateUniqueIndexKey, error.ExceptionType);
			}
		}

		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @AhPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @AhPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'DEM');
				DECLARE @GePk UNIQUEIDENTIFIER = (SELECT TOP(1) GE_PK FROM dbo.GlbDepartment);
				DECLARE @OhPk UNIQUEIDENTIFIER = (SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = 'DEMORG');
				INSERT dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_TransactionType, AH_Ledger, AH_InvoiceDate, AH_OH) VALUES
					(@AhPk01, @GcPk, @GbPk, @GePk, 'AH001', '2014-07-01', 'US1', 'JNL', 'AR', getutcdate(), @OhPk),
					(@AhPk02, @GcPk, @GbPk, @GePk, 'AH002', '2014-07-23', 'US2', 'JNL', 'AR', getutcdate(), null ),
					(newid(), @GcPk, @GbPk, @GePk, 'AH003', '2014-07-10', 'US3', 'JNL', 'AR', getutcdate(), @OhPk),
					(newid(), @GcPk, @GbPk, @GePk, 'AH004', '2014-07-14', 'US4', 'DDB', 'CB', getutcdate(), null ),
					(newid(), @GcPk, @GbPk, @GePk, 'AH005', '2013-07-31', 'US5', 'DDB', 'CB', getutcdate(), @OhPk);
				INSERT dbo.GenExportBatchSequence (XB_PK, XB_ParentID, XB_Type, XB_SystemCreateTimeUtc, XB_SystemCreateUser) VALUES
					(newid(), @AhPk01, 'ERP', GETUTCDATE(), '~BP'),
					(newid(), @AhPk01, 'PPF', GETUTCDATE(), '~BP'),
					(newid(), @AhPk02, 'HEX', GETUTCDATE(), '~BP');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "AH001");
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("[T1] UserCode", "US1", transaction1.ClientStaffCode);
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 7, 1), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference02", "AR JNL #001 Org:DEMORG", transaction1.Reference2);

			var transaction2 = FindRowByRef1(transactions, "AH004");
			AssertEquals("[T2] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("[T2] UserCode", "US4", transaction2.ClientStaffCode);
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2014, 7, 14), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference02", "CB DDB #001 Org:<none>", transaction2.Reference2);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 7);
			}
		}
	}
}
