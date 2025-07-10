using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(CommunicationSpecialEntryLandedCosting))]
	sealed class CommunicationSpecialEntryLandedCostingTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @JePk UNIQUEIDENTIFIER = newid();
				DECLARE @JdPk UNIQUEIDENTIFIER = newid();
				DECLARE @OaPk UNIQUEIDENTIFIER = (SELECT TOP(1) OA_PK FROM dbo.OrgAddress);
				DECLARE @GcPkSg UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'SIN');
				DECLARE @GcPkAu UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'DEM');

				INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_GB, JE_GC, JE_DeclarationReference, JE_SystemCreateUser, JE_ClusterKey) VALUES (@JePk, 'AU', @GbPk, @GcPkAu, 'DEC01', 'US1', 1);
				INSERT dbo.JobOrderHeader (JD_PK, JD_OrderNumber, JD_OA_BuyerAddress, JD_SystemCreateUser) VALUES (@JdPk, 'ORD01', @OaPk, 'US2');
				INSERT dbo.LandedCostHeader (LT_PK, LT_ParentID, LT_LandedCostType, LT_GC, LT_DateOfEntry, LT_ParentTableCode, LT_ClusterKey) VALUES
					(newid(), @JePk  , 'LT1', @GcPkSg, '2014-01-10', 'JE', 1),
					(newid(), newid(), 'LT2', @GcPkSg, '2014-01-10', 'JE', 2),
					(newid(), @JePk  , 'LT3', @GcPkAu, '2014-02-10', 'JE', 1),
					(newid(), @JdPk  , 'LT4', @GcPkAu, '2014-01-01', 'JD', 3);";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "LT4");
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", null, transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 1, 1), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US2", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference02", "Ord.ORD01", transaction1.Reference2);

			var transaction2 = FindRowByRef1(transactions, "LT1");
			AssertEquals("[T2] CompanyCode", "SIN", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", null, transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2014, 1, 10), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "US1", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference01", "LT1", transaction2.Reference1);
			AssertEquals("[T2] TransactionReference02", "Dec.DEC01", transaction2.Reference2);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 1);
			}
		}
	}
}
