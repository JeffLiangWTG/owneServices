using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Orders;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Orders
{
	[TestedType(typeof(Report_OrderLineByContainer))]
	class Report_OrderLineByContainerTest : DbCreateScriptTest
	{
		protected override bool RequiresSchemaBinding => false;

		public void TestOrderLinesWithHugeOrderLinePrice_DoNotThrowException()
		{
			var orgHeaderPK_Supplier = Guid.NewGuid();
			var orgAddressPK_Supplier = Guid.NewGuid();
			var glbCompanyPK = Guid.NewGuid();
			var glbBranchPK = Guid.NewGuid();
			var glbDepartmentPK = Guid.NewGuid();
			var orgHeaderPK_Buyer = Guid.NewGuid();
			var orgAddressPK_Buyer = Guid.NewGuid();
			var jobOrderHeaderPK = Guid.NewGuid();

			InsertOrgHeader("TOH00", orgHeaderPK_Supplier);
			InsertOrgAddress(orgAddressPK_Supplier, orgHeaderPK_Supplier, "ADDRESS 1", "TEST400");
			InsertGlbCompany(glbCompanyPK, orgHeaderPK_Supplier);
			InsertGlbBranch(glbBranchPK, glbCompanyPK);
			InsertGlbDepartment(glbDepartmentPK);
			InsertOrgHeader("TOH01", orgHeaderPK_Buyer);
			InsertOrgAddress(orgAddressPK_Buyer, orgHeaderPK_Buyer, "ADDRESS 2", "TEST500");
			InsertJobOrderHeader(jobOrderHeaderPK, Guid.Empty, orgAddressPK_Buyer, Guid.Empty);
			InsertJobDocAddressForOrder_Override(jobOrderHeaderPK, orgAddressPK_Buyer, "GAA");

			for (var lineNo = 1; lineNo <= 10; lineNo++)
			{
				InsertJobOrderLine(jobOrderHeaderPK, SqlMoney.MaxValue, lineNo);
			}

			var result = DataUtils.GetDataTableFromQuery(TestConnection,
				string.Format(CultureInfo.InvariantCulture, "SELECT * FROM Report_OrderLineByContainer ('{0}', '{1}', 'Buyer/Controlling Customer', NULL, NULL, 'N', NULL, '', '', '', '2013-01-01', '2013-01-01')", glbCompanyPK, orgHeaderPK_Buyer));
			AssertEquals("Result should have ten rows", 10, result.Rows.Count);
			for (var lineNo = 1; lineNo <= 10; lineNo++)
			{
				AssertEquals("Result should have ten rows", (decimal)SqlMoney.MaxValue * 10, result.Rows[0]["OrderTotalPrice"]);
			}
		}

		#region InsertNewTestData

		void InsertOrgHeader(string orgHeadercode, Guid orgHeaderPK)
		{
			var sql = string.Format(
				@"INSERT INTO dbo.OrgHeader
             ([OH_PK]
             ,[OH_Code]
             ,[OH_FullName])
             VALUES
             ('{0}','{1}','Test Organisation') ", orgHeaderPK, orgHeadercode);

			TestConnection.ExecuteNonQuery(sql);
		}

		void InsertOrgAddress(Guid orgAddressPK, Guid orgHeaderPK, string orgAddress, string code)
		{
			var sql = string.Format(
				@"INSERT INTO dbo.OrgAddress
			 ([OA_PK]
			 ,[OA_OH]
			 ,[OA_Address1]
			 ,[OA_Code])
             VALUES
             ('{0}','{1}','{2}', '{3}')", orgAddressPK, orgHeaderPK, orgAddress, code);

			TestConnection.ExecuteNonQuery(sql);
		}

		void InsertGlbCompany(Guid glbCompanyPK, Guid orgHeaderPK, string countryCode = "AU", string currencyCode = "AUD")
		{
			var sql = string.Format(
				@"INSERT INTO dbo.GlbCompany
            ([GC_PK]
            ,[GC_Code]
            ,[GC_Name]
            ,[GC_OH_OrgProxy]
			,[GC_RN_NKCountryCode]
			,[GC_RX_NKLocalCurrency])
            VALUES
            ('{0}','TGC', 'AU company','{1}', '{2}', '{3}')", glbCompanyPK, orgHeaderPK, countryCode, currencyCode);

			TestConnection.ExecuteNonQuery(sql);
		}

		void InsertGlbBranch(Guid glbBranchPK, Guid glbCompanyPK)
		{
			var sql = string.Format(
				@"INSERT INTO dbo.GlbBranch 
            ([GB_PK]
            ,[GB_Code]
            ,[GB_GC])
            VALUES
            ('{0}','TGB','{1}')", glbBranchPK, glbCompanyPK);

			TestConnection.ExecuteNonQuery(sql);
		}

		void InsertGlbDepartment(Guid glbDepartmentPK)
		{
			var sql = string.Format(
				@"INSERT INTO dbo.GlbDepartment
            ([GE_PK]
            ,[GE_Code])
            VALUES
            ('{0}','TGE')", glbDepartmentPK);

			TestConnection.ExecuteNonQuery(sql);
		}

		void InsertJobOrderHeader(Guid jobOrderHeaderPK, Guid jobShipmentPK, Guid orgAddressPK_Buyer, Guid orgAddressPK_Supplier)
		{
			var sql = string.Format(
				@"INSERT INTO dbo.JobOrderHeader
				([JD_PK]
				,[JD_JS]
				,[JD_OA_BuyerAddress]
                ,[JD_OA_SupplierAddress]
				,[JD_IsCancelled]
				,[JD_OrderStatus]
				,[JD_SystemCreateTimeUtc]
				,[JD_SystemCreateUser]
				,[JD_IsValid])
				VALUES
				('{0}',
				{1},
				'{2}',
                {3},
				'0',
				'INC',
				'2013-01-01',
				'TGS',
				'1')",
				jobOrderHeaderPK,
				jobShipmentPK == Guid.Empty ? "NULL" : string.Format("'{0}'", jobShipmentPK),
				orgAddressPK_Buyer,
				orgAddressPK_Supplier == Guid.Empty ? "NULL" : string.Format("'{0}'", orgAddressPK_Supplier));

			TestConnection.ExecuteNonQuery(sql);
		}

		void InsertJobOrderLine(Guid jobOrderHeaderPK, SqlMoney linePrice, int lineNo)
		{
			var sql = string.Format("INSERT INTO dbo.JobOrderLine ([JO_PK], [JO_JD], [JO_LinePrice], [JO_LineNo]) VALUES (NEWID(),'{0}', {1}, {2})", jobOrderHeaderPK, linePrice, lineNo);

			TestConnection.ExecuteNonQuery(sql);
		}

		void InsertJobDocAddressForOrder_Override(Guid orderPK, Guid orgAddressPK, string addressType)
		{
			var pk = Guid.NewGuid();

			const string sql = @"INSERT INTO dbo.JobDocAddress
(E2_PK, E2_IsValid, E2_AddressType, E2_OA_Address, E2_ParentID, E2_ParentTableCode, E2_AddressOverride, E2_CompanyName, E2_Address1, E2_Address2, E2_ValidationStatus)
VALUES(@pk, 1, @addressType, @orgAddressPK, @orderPK, 'JD', 1, 'Overridden Company', 'TEST Address1', 'Test Address2', 'MAN')";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@addressType", SqlDbType.VarChar, addressType);
				command.AddParameter("@orgAddressPK", SqlDbType.UniqueIdentifier, orgAddressPK);
				command.AddParameter("@orderPK", SqlDbType.UniqueIdentifier, orderPK);
				command.ExecuteNonQuery();
			}
		}

		#endregion
	}
}
