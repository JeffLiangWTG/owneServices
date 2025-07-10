using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Scripts.Customs.EU;
using Enterprise.Integration.Billing;
using NUnit.Framework;
namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs.Testing
{
	[TestedType(typeof(EUExitControlStandaloneDeclaration))]
	sealed class EUExitControlStandaloneDeclarationTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			var sqlText = """
				DECLARE @GcPk UNIQUEIDENTIFIER = newid();
				DECLARE @GbPk UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkOther UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkOther UNIQUEIDENTIFIER = newid();
				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_RN_NKCountryCode, GC_Name) VALUES
					(@GcPk, 'DEC', 'DE', 'The Company');
				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES
					(@GbPk, 'DEB', @GcPk);
				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_RN_NKCountryCode, GC_Name) VALUES
					(@GcPkOther, 'DC2', 'DE', 'The Other The Company');
				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES
					(@GbPkOther, 'DB2', @GcPkOther);
				INSERT dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_SystemCreateUser, JS_SystemCreateTimeUtc, JS_SystemLastEditUser, JS_SystemLastEditTimeUtc)
				VALUES	(0x11, '1', '~BP', '2025-01-01', '~BP', '2025-01-01'),
						(0x12, '2', '~BP', '2025-01-01', '~BP', '2025-01-01'),
						(0x13, '3', '~BP', '2025-01-01', '~BP', '2025-01-01');
				INSERT dbo.JobDeclaration (JE_PK, JE_DeclarationReference, JE_JS, JE_ClusterKey, JE_DataModel, JE_GB, JE_GC, JE_SystemCreateUser, JE_SystemCreateTimeUtc, JE_SystemLastEditUser, JE_SystemLastEditTimeUtc)
				VALUES	(0x21, '1', NULL, 3, 'DE', @GbPk, @GcPk, '~BP', '2025-01-01', '~BP', '2025-01-01'),
						(0x22, '2', 0x12, 6, 'DE', @GbPk, @GcPk, '~BP', '2025-01-01', '~BP', '2025-01-01'),
						(0x23, '3', 0x13, 7, 'DE', @GbPk, @GcPk, '~BP', '2025-01-01', '~BP', '2025-01-01');
				INSERT dbo.CusExitHeader (CXH_PK, CXH_ClusterKey, CXH_ParentTableCode, CXH_ParentId, CXH_ApplicationCode, CXH_JobReference, CXH_GB_Branch, CXH_GC_Company, CXH_SystemCreateUser, CXH_SystemCreateTimeUtc, CXH_SystemLastEditUser, CXH_SystemLastEditTimeUtc)
				VALUES	(0x1, 1, '',	NULL, 'XIT', 'E0001', @GbPk,		@GcPk,		'U1', '2025-08-01', '~BP', '2025-09-01'),
						(0x2, 2, 'JS',	0x11, 'XIT', 'E0002', @GbPk,		@GcPk,		'U2', '2025-08-02', '~BP', '2025-09-02'),
						(0x3, 3, 'JE',	0x21, 'XIT', 'E0003', @GbPk,		@GcPk,		'U3', '2025-08-03', '~BP', '2025-09-03'), -- not selected, has JE parent
						(0x4, 4, '',	NULL, 'XIT', 'E0004', @GbPk,		@GcPk,		'U4', '2025-07-04', '~BP', '2025-09-04'), -- not selected, outside date range
						(0x5, 5, '',	NULL, 'DAC', 'E0005', @GbPk,		@GcPk,		'U5', '2025-08-05', '~BP', '2025-09-05'), -- not selected, Application Code is not 'XIT'
						(0x6, 6, 'JS',	0x12, 'XIT', 'E0006', @GbPk,		@GcPk,		'U6', '2025-08-06', '~BP', '2025-09-06'), -- not selected, has Declaration in the same company context
						(0x7, 7, 'JS',	0x13, 'XIT', 'E0007', @GbPkOther,	@GcPkOther,	'U7', '2025-08-07', '~BP', '2025-09-07');
				""";
			TestConnection.Command(sqlText).ExecuteNonQuery();
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2025, 8);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("All elements", new[] { "E0001", "E0002", "E0007" }, transactions.Select(x => x.Reference1));

				var transaction1 = transactions.Single(x => x.Reference1 == "E0001");
				AssertEquals("CompanyCode", "DEC", transaction1.GetCompanyCode());
				AssertEquals("BranchCode", "DEB", transaction1.GetBranchCode());
				AssertEquals("ClientStaffCode", "U1", transaction1.ClientStaffCode);
				AssertEquals("TransactionDateUtc", new DateTime(2025, 8, 1), transaction1.ServiceOccuredUTC);
				AssertEquals("TransactionGuidReference", "00000001-0000-0000-0000-000000000000", transaction1.Reference5);

				var transaction2 = transactions.Single(x => x.Reference1 == "E0002");
				AssertEquals("CompanyCode", "DEC", transaction2.GetCompanyCode());
				AssertEquals("BranchCode", "DEB", transaction2.GetBranchCode());
				AssertEquals("ClientStaffCode", "U2", transaction2.ClientStaffCode);
				AssertEquals("TransactionDateUtc", new DateTime(2025, 8, 2), transaction2.ServiceOccuredUTC);
				AssertEquals("TransactionGuidReference", "00000002-0000-0000-0000-000000000000", transaction2.Reference5);

				var transaction3 = transactions.Single(x => x.Reference1 == "E0007");
				AssertEquals("CompanyCode", "DC2", transaction3.GetCompanyCode());
				AssertEquals("BranchCode", "DB2", transaction3.GetBranchCode());
				AssertEquals("ClientStaffCode", "U7", transaction3.ClientStaffCode);
				AssertEquals("TransactionDateUtc", new DateTime(2025, 8, 7), transaction3.ServiceOccuredUTC);
				AssertEquals("TransactionGuidReference", "00000007-0000-0000-0000-000000000000", transaction3.Reference5);
			});
		}

		public void TestVersions()
		{
			CombineAssertions("Test min and max CW1 version", () =>
			{
				AssertEquals("25.5.22.225", ScriptToTest.MinCW1Version);
				AssertEquals(string.Empty, ScriptToTest.MaxCW1Version);
			});
		}
	}
}
