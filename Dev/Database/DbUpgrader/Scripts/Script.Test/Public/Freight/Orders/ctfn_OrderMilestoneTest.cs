using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Orders;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Orders
{
	[TestedType(typeof(ctfn_OrderMilestone))]
	class ctfn_OrderMilestoneTest : DbCreateScriptTest
	{
		public void TestOnlyReturnsMilestoneHeaders()
		{
			var glbCompanyPK = Guid.NewGuid();
			var orgHeaderPK = Guid.NewGuid();
			var orgAddressPK = Guid.NewGuid();
			var jobOrderHeaderPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(GetInsertOrgHeaderCommand(orgHeaderPK));
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand(orgAddressPK, orgHeaderPK));
			TestConnection.ExecuteNonQuery(GetInsertGlbCompanyCommand(glbCompanyPK, orgHeaderPK));
			TestConnection.ExecuteNonQuery(GetInsertJobOrderHeaderCommand(jobOrderHeaderPK, orgAddressPK));
			TestConnection.ExecuteNonQuery(GetInsertProcessTaskCommand(jobOrderHeaderPK, "MIL", "DEP", glbCompanyPK));
			TestConnection.ExecuteNonQuery(GetInsertProcessTaskCommand(jobOrderHeaderPK, "TRG", "DEP", glbCompanyPK));

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.ctfn_OrderMilestone('DEP')");
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("MilestonePK", jobOrderHeaderPK, result.Rows[0]["JobOrderHeaderPK"]);
		}

		string GetInsertGlbCompanyCommand(Guid glbCompanyPK, Guid orgHeaderPK, string countryCode = "AU", string currencyCode = "AUD")
		{
			return string.Format(
			@"INSERT INTO dbo.GlbCompany
            ([GC_PK]
            ,[GC_Code]
            ,[GC_Name]
            ,[GC_OH_OrgProxy]
			,[GC_RN_NKCountryCode]
			,[GC_RX_NKLocalCurrency])
            VALUES
            ('{0}','TGC', 'AU company','{1}', '{2}', '{3}')", glbCompanyPK, orgHeaderPK, countryCode, currencyCode);
		}

		string GetInsertOrgHeaderCommand(Guid pk)
		{
			return string.Format(
			 @"INSERT INTO dbo.OrgHeader
             ([OH_PK]
             ,[OH_Code]
             ,[OH_FullName])
             VALUES
             ('{0}','HEADER','Test Organisation') ", pk);
		}

		string GetInsertOrgAddressCommand(Guid addressPK, Guid headerPK)
		{
			return string.Format(
				@"INSERT INTO dbo.OrgAddress
				([OA_PK]
				,[OA_OH]
				,[OA_Address1]
				,[OA_Code])
				VALUES
				('{0}', '{1}', 'Address 1', 'TEST')", addressPK, headerPK);
		}

		string GetInsertJobOrderHeaderCommand(Guid orderHeaderPK, Guid addressPK)
		{
			return string.Format(
				@"INSERT INTO dbo.JobOrderHeader
				([JD_PK]
				,[JD_JS]
				,[JD_JE]
				,[JD_EF_ShipmentPrePlanning]
				,[JD_OA_BuyerAddress]
				,[JD_OrderStatus]
				,[JD_SystemCreateTimeUTC]
				,[JD_SystemCreateUser]
				,[JD_IsValid]
				,[JD_IsCancelled])
				VALUES
				('{0}', NULL, NULL, NULL, '{1}', 'INC', '2020-01-01', 'TGS', '1','0')",
				orderHeaderPK,
				addressPK);
		}

		string GetInsertProcessTaskCommand(Guid parentID, string type, string eventCode, Guid glbCompanyPK)
		{
			return string.Format(
			@"INSERT INTO dbo.ProcessTasks 
            (P9_PK, 
             P9_GC,
             P9_ParentID, 
             P9_ParentTableCode, 
             P9_Type, 
             P9_SE_NKExceptionEvent,
             P9_SE_NKMilestoneEvent,
             P9_ActualDate) 
             VALUES
            (NEWID(), '{0}','{1}', 'JD', '{2}', 'EXC', '{3}', '2020-01-01') ", glbCompanyPK, parentID, type, eventCode);
		}
	}
}

