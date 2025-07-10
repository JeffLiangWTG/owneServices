using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.Testing.ScriptTests.BaseTests;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_AllJobProfitChargeTest : JobProfitFilterTest
	{
		protected override DataTable RunScriptWithFilter(string activeStatus = "", string notIncludeReversedWIPACR = "")
		{
			return RunScript(ZDateTime.Empty, ZDateTime.Empty, false, false, activeStatus, notIncludeReversedWIPACR);
		}

		public void TestOutstandingWIPandACRFiltering()
		{
			var branch1 = GlbBranch.CurrentBranch;
			var branch2 = TestObjectCreator.NonCurrentBranch;

			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			job1.JH_GB = branch1.PK;
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, 100m, 100m);

			charge1.JR_OSCostAmt = 0m;
			charge1.JR_OSSellAmt = 0m;

			var shipment2 = TestObjectCreator.CreateShipment("S0002");
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			job2.JH_GB = branch2.PK;
			TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, 10.0m, 0m);

			var shipment3 = TestObjectCreator.CreateShipment("S0003");
			var job3 = TestObjectCreator.CreateJob(shipment3, false);
			job3.JH_GB = branch1.PK;
			TestObjectCreator.CreateCharge(job3, TestObjectCreator.CC1, 0m, 10.2m);

			var shipment4 = TestObjectCreator.CreateShipment("S0004");
			var job4 = TestObjectCreator.CreateJob(shipment4, false);
			job4.JH_GB = branch2.PK;
			TestObjectCreator.CreateCharge(job4, TestObjectCreator.CC1, 89m, 89m);
			TestObjectCreator.CreateCharge(job4, TestObjectCreator.CC2, -89m, -89m);

			var shipment5 = TestObjectCreator.CreateShipment("S0005");
			var job5 = TestObjectCreator.CreateJob(shipment5, false);
			job5.JH_GB = branch1.PK;
			TestObjectCreator.CreateCharge(job5, TestObjectCreator.CC1, 79m, 79m);
			TestObjectCreator.CreateCharge(job5, TestObjectCreator.CC1, -79m, -79m);

			Factory.Save();

			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.GlbBranch SET GB_AccountingGroupCode = 'BRA', GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_Code = '{0}' And GB_GC = '{1}'", branch1.GB_Code, branch1.GB_GC));
			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.GlbBranch SET GB_AccountingGroupCode = 'BRB', GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_Code = '{0}' And GB_GC = '{1}'", branch2.GB_Code, branch2.GB_GC));

			var headers = new[] { "JH_JobNum", "AC_Code", "ACRAmount", "WIPAmount", "CSTAmount", "REVAmount", "JobBranchManagementCode" };
			var result = RunScript();
			var lines = new object[][]
						{
							new object[] {	"S0002",	TestObjectCreator.CC1.AC_Code,	-10m,		0m,		0m,		0m, "BRB"	},
							new object[] {	"S0003",	TestObjectCreator.CC1.AC_Code,	0m,			10.2m,	0m,		0m, "BRA"	},
							new object[] {	"S0004",	TestObjectCreator.CC1.AC_Code,	-89m,		89m,	0m,		0m, "BRB"	},
							new object[] {	"S0004",	TestObjectCreator.CC2.AC_Code,	89m,		-89m,	0m,		0m, "BRB"	},
							new object[] {	"S0005",	TestObjectCreator.CC1.AC_Code,	0m,			0m,		0m,		0m, "BRA"	},
						};
			AssertDataTableAllRowsByKeyColumns("Filtering Without outstanding WIP, without outstanding ACR", result, headers, lines, headers);

			result = RunScript(ZDateTime.Empty, ZDateTime.Empty, true, false);
			lines = new object[][]
						{
							new object[] {	"S0003",	TestObjectCreator.CC1.AC_Code,	0m,			10.2m,	0m,		0m, "BRA"	},
							new object[] {	"S0004",	TestObjectCreator.CC1.AC_Code,	-89m,		89m,	0m,		0m, "BRB"	},
							new object[] {	"S0004",	TestObjectCreator.CC2.AC_Code,	89m,		-89m,	0m,		0m, "BRB"	},
							new object[] {	"S0005",	TestObjectCreator.CC1.AC_Code,	0m,			0m,		0m,		0m, "BRA"	},
						};
			AssertDataTableAllRowsByKeyColumns("Filtering With outstanding WIP, without outstanding ACR", result, headers, lines, headers);

			result = RunScript(ZDateTime.Empty, ZDateTime.Empty, false, true);
			lines = new object[][]
						{
							new object[] {	"S0002",	TestObjectCreator.CC1.AC_Code,	-10m,		0m,		0m,		0m, "BRB"	},
							new object[] {	"S0004",	TestObjectCreator.CC1.AC_Code,	-89m,		89m,	0m,		0m, "BRB"	},
							new object[] {	"S0004",	TestObjectCreator.CC2.AC_Code,	89m,		-89m,	0m,		0m, "BRB"	},
							new object[] {	"S0005",	TestObjectCreator.CC1.AC_Code,	0m,			0m,		0m,		0m, "BRA"	},
						};
			AssertDataTableAllRowsByKeyColumns("Filtering Without outstanding WIP, with outstanding ACR", result, headers, lines, headers);

			result = RunScript(ZDateTime.Empty, ZDateTime.Empty, true, true);
			lines = new object[][]
						{
							new object[] {	"S0002",	TestObjectCreator.CC1.AC_Code,	-10m,		0m,		0m,		0m, "BRB"	},
							new object[] {	"S0003",	TestObjectCreator.CC1.AC_Code,	0m,			10.2m,	0m,		0m, "BRA"	},
							new object[] {	"S0004",	TestObjectCreator.CC1.AC_Code,	-89m,		89m,	0m,		0m, "BRB"	},
							new object[] {	"S0004",	TestObjectCreator.CC2.AC_Code,	89m,		-89m,	0m,		0m, "BRB"	},
							new object[] {	"S0005",	TestObjectCreator.CC1.AC_Code,	0m,			0m,		0m,		0m, "BRA"	},
						};
			AssertDataTableAllRowsByKeyColumns("Filtering With outstanding WIP, with outstanding ACR", result, headers, lines, headers);
		}

		[TestDate(2018, 04, 25, 0, 0, 0)]
		public void TestJobOpenDate()
		{
			var branch1 = GlbBranch.CurrentBranch;
			var branch2 = TestObjectCreator.NonCurrentBranch;

			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			job1.JH_GB = branch1.PK;
			job1.JH_A_JOP = ZDateTime.Today.AddDays(-2);
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, 100m, 100m);

			charge1.JR_OSCostAmt = 0m;
			charge1.JR_OSSellAmt = 0m;

			var shipment2 = TestObjectCreator.CreateShipment("S0002");
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			job2.JH_GB = branch2.PK;
			job2.JH_A_JOP = ZDateTime.Today.AddDays(-2);
			TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, 10.0m, 0m);

			var shipment3 = TestObjectCreator.CreateShipment("S0003");
			var job3 = TestObjectCreator.CreateJob(shipment3, false);
			job3.JH_GB = branch1.PK;
			job3.JH_A_JOP = ZDateTime.Today.AddDays(-2);
			TestObjectCreator.CreateCharge(job3, TestObjectCreator.CC1, 0m, 10.2m);

			var shipment4 = TestObjectCreator.CreateShipment("S0004");
			var job4 = TestObjectCreator.CreateJob(shipment4, false);
			job4.JH_GB = branch2.PK;
			job4.JH_A_JOP = ZDateTime.Today.AddDays(-2);
			TestObjectCreator.CreateCharge(job4, TestObjectCreator.CC1, 89m, 89m);
			TestObjectCreator.CreateCharge(job4, TestObjectCreator.CC2, -89m, -89m);

			var shipment5 = TestObjectCreator.CreateShipment("S0005");
			var job5 = TestObjectCreator.CreateJob(shipment5, false);
			job5.JH_GB = branch1.PK;
			job5.JH_A_JOP = ZDateTime.Today.AddDays(-2);
			TestObjectCreator.CreateCharge(job5, TestObjectCreator.CC1, 79m, 79m);
			TestObjectCreator.CreateCharge(job5, TestObjectCreator.CC1, -79m, -79m);

			Factory.Save();

			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.GlbBranch SET GB_AccountingGroupCode = 'BRA', GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_Code = '{0}' And GB_GC = '{1}'", branch1.GB_Code, branch1.GB_GC));
			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.GlbBranch SET GB_AccountingGroupCode = 'BRB', GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_Code = '{0}' And GB_GC = '{1}'", branch2.GB_Code, branch2.GB_GC));

			var headers = new[] { "JH_JobNum", "JH_JobOpened", "AC_Code", "ACRAmount", "WIPAmount", "CSTAmount", "REVAmount", "JobBranchManagementCode" };
			var result = RunScript();
			var lines = new object[][]
						{
							new object[] {  "S0002", new ZDateTime(2018, 04, 23, 0, 0, 0), TestObjectCreator.CC1.AC_Code,  -10m,       0m,     0m,     0m, "BRB"   },
							new object[] {  "S0003", new ZDateTime(2018, 04, 23, 0, 0, 0), TestObjectCreator.CC1.AC_Code,  0m,         10.2m,  0m,     0m, "BRA"   },
							new object[] {  "S0004", new ZDateTime(2018, 04, 23, 0, 0, 0), TestObjectCreator.CC1.AC_Code,  -89m,       89m,    0m,     0m, "BRB"   },
							new object[] {  "S0004", new ZDateTime(2018, 04, 23, 0, 0, 0), TestObjectCreator.CC2.AC_Code,  89m,        -89m,   0m,     0m, "BRB"   },
							new object[] {  "S0005", new ZDateTime(2018, 04, 23, 0, 0, 0), TestObjectCreator.CC1.AC_Code,  0m,         0m,     0m,     0m, "BRA"   },
						};
			AssertDataTableAllRowsByKeyColumns("Filtering Without outstanding WIP, without outstanding ACR", result, headers, lines, headers);

			result = RunScript(ZDateTime.Empty, ZDateTime.Empty, true, false);
			lines = new object[][]
						{
							new object[] {  "S0003", new ZDateTime(2018, 04, 23, 0, 0, 0), TestObjectCreator.CC1.AC_Code,  0m,         10.2m,  0m,     0m, "BRA"   },
							new object[] {  "S0004", new ZDateTime(2018, 04, 23, 0, 0, 0), TestObjectCreator.CC1.AC_Code,  -89m,       89m,    0m,     0m, "BRB"   },
							new object[] {  "S0004", new ZDateTime(2018, 04, 23, 0, 0, 0), TestObjectCreator.CC2.AC_Code,  89m,        -89m,   0m,     0m, "BRB"   },
							new object[] {  "S0005", new ZDateTime(2018, 04, 23, 0, 0, 0), TestObjectCreator.CC1.AC_Code,  0m,         0m,     0m,     0m, "BRA"   },
						};
			AssertDataTableAllRowsByKeyColumns("Filtering With outstanding WIP, without outstanding ACR", result, headers, lines, headers);

			result = RunScript(ZDateTime.Empty, ZDateTime.Empty, false, true);
			lines = new object[][]
						{
							new object[] {  "S0002", new ZDateTime(2018, 04, 23, 0, 0, 0), TestObjectCreator.CC1.AC_Code,  -10m,       0m,     0m,     0m, "BRB"   },
							new object[] {  "S0004", new ZDateTime(2018, 04, 23, 0, 0, 0), TestObjectCreator.CC1.AC_Code,  -89m,       89m,    0m,     0m, "BRB"   },
							new object[] {  "S0004", new ZDateTime(2018, 04, 23, 0, 0, 0), TestObjectCreator.CC2.AC_Code,  89m,        -89m,   0m,     0m, "BRB"   },
							new object[] {  "S0005", new ZDateTime(2018, 04, 23, 0, 0, 0), TestObjectCreator.CC1.AC_Code,  0m,         0m,     0m,     0m, "BRA"   },
						};
			AssertDataTableAllRowsByKeyColumns("Filtering Without outstanding WIP, with outstanding ACR", result, headers, lines, headers);

			result = RunScript(ZDateTime.Empty, ZDateTime.Empty, true, true);
			lines = new object[][]
						{
							new object[] {  "S0002", new ZDateTime(2018, 04, 23, 0, 0, 0), TestObjectCreator.CC1.AC_Code,  -10m,       0m,     0m,     0m, "BRB"   },
							new object[] {  "S0003", new ZDateTime(2018, 04, 23, 0, 0, 0), TestObjectCreator.CC1.AC_Code,  0m,         10.2m,  0m,     0m, "BRA"   },
							new object[] {  "S0004", new ZDateTime(2018, 04, 23, 0, 0, 0), TestObjectCreator.CC1.AC_Code,  -89m,       89m,    0m,     0m, "BRB"   },
							new object[] {  "S0004", new ZDateTime(2018, 04, 23, 0, 0, 0), TestObjectCreator.CC2.AC_Code,  89m,        -89m,   0m,     0m, "BRB"   },
							new object[] {  "S0005", new ZDateTime(2018, 04, 23, 0, 0, 0), TestObjectCreator.CC1.AC_Code,  0m,         0m,     0m,     0m, "BRA"   },
						};
			AssertDataTableAllRowsByKeyColumns("Filtering With outstanding WIP, with outstanding ACR", result, headers, lines, headers);
		}

		[TestDate(2018, 04, 25, 0, 0, 0)]
		public void TestJobInactive()
		{
			var branch1 = GlbBranch.CurrentBranch;
			var branch2 = TestObjectCreator.NonCurrentBranch;

			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			job1.JH_GB = branch1.PK;
			job1.JH_A_JOP = ZDateTime.Today.AddDays(-2);
			TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, 0m, 10.2m);

			var shipment2 = TestObjectCreator.CreateShipment("S0002");
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			Factory.Save();

			job2.JH_GB = branch2.PK;
			job2.JH_A_JOP = ZDateTime.Today.AddDays(-2);

			TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, 10.0m, 0m);
			Factory.Save();

			string deActivateJobSql = $@"
UPDATE dbo.JobHeader
SET
	JH_IsActive = 0,
	JH_SystemLastEditTimeUtc = GETUTCDATE(),
	JH_SystemLastEditUser = '~BP'
WHERE
	JH_PK = '{job2.PK}'";
			DataUtils.GetDataTableFromQuery(Db.Connection, deActivateJobSql);

			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.GlbBranch SET GB_AccountingGroupCode = 'BRA', GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_Code = '{0}' And GB_GC = '{1}'", branch1.GB_Code, branch1.GB_GC));
			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.GlbBranch SET GB_AccountingGroupCode = 'BRB', GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_Code = '{0}' And GB_GC = '{1}'", branch2.GB_Code, branch2.GB_GC));
			var headers = new[] { "JH_JobNum", "JH_JobOpened", "AC_Code", "ACRAmount", "WIPAmount", "CSTAmount", "REVAmount", "JobBranchManagementCode" };
			var result = RunScript();
			var lines = new object[][]
						{
							new object[] {  "S0001", new ZDateTime(2018, 04, 23, 0, 0, 0), TestObjectCreator.CC1.AC_Code,  0m,         10.2m,  0m,     0m, "BRA"   },
							new object[] {  "S0002", new ZDateTime(2018, 04, 23, 0, 0, 0), TestObjectCreator.CC1.AC_Code,  -10.0m,         0m,  0m,     0m, "BRB"   },
						};
			AssertDataTableAllRowsByKeyColumns("Should have data with inactive job", result, headers, lines, headers);
		}

		DataTable RunScript()
		{
			return RunScript(ZDateTime.Empty, ZDateTime.Empty, false, false);
		}

		DataTable RunScript(ZDateTime transactionFrom, ZDateTime transactionTo, bool outstandingWIP, bool outstandingACR, string activeStatus = "", string notIncludeReversedWIPACR = "")
		{
			string sql = string.Format(@"
SELECT * FROM Report_AllJobProfitCharge(
'{0}',					--@CompanyPK
'{1}',					--@TransactionFrom
'{2}',					--@TransactionTo
'',						--@JobType
'{3}',					--@OutstandingWIP
'{4}',					--@OutstandingACR
'',						--@ChargeCode
'',						--@ChargeCodeNOTIN
NULL,					--@ChargeGroup
NULL,					--@SalesGroup
NULL,					--@ExpenseGroup
'',						--@TransactionBranch
'',						--@TransactionDepartment
'',						--@TransactionDebtor
'',						--@TransactionCreditor
'',						--@IsCommissionable
NULL,					--@ProfitLossReasonCode
'1900-01-01 00:00:00',	--@RevRecogFrom
'2079-06-06 23:59:29',	--@RevRecogTo
NULL,					--@CFSJobType
'{5}',					--@ActiveStatus
'{6}'					--@NotIncludeReversedWIPACR
) ORDER BY JH_JobNum",
				GlbCompany.CurrentCompany.PK,			//@CompanyPK
				GetMinDateTimeString(transactionFrom),	//@TransactionFrom
				GetMaxDateTimeString(transactionTo),	//@TransactionTo
				outstandingWIP ? "Y" : "",				//@OutstandingWIP
				outstandingACR ? "Y" : "",				//@OutstandingACR
				activeStatus,                           //@ActiveStatus
				notIncludeReversedWIPACR				//@NotIncludeReversedWIPACR
			);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		protected override object[][] DoNotShowReversedWIPACRFilterForShow =>
		[
			["S0001", 0m, 0m],
			["S0002", 0m, 0m],
			["S0002", 0m, 12m],
			["S0003", -89.0m,  89.0m],
		];

		protected override object[][] DoNotShowReversedWIPACRFilterForNotShow =>
		[
			["S0002", 0m, 12m],
			["S0003", -89.0m,  89.0m],
		];
	}
}
