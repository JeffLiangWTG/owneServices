

using System.Data;
using CargoWise.Data;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_JobProfitTransactionTotalsByBranchAndDepartmentTest : ScriptTest
	{
		enum JobTypes { Shipment, Brokerage }

		public void TestOutstandingWIPandACRFiltering_Shipment()
		{
			AssertOutstandingWIPandACRFiltering(JobTypes.Shipment);
		}

		public void TestOutstandingWIPandACRFiltering_Brokerage()
		{
			AssertOutstandingWIPandACRFiltering(JobTypes.Brokerage);
		}
		public void TestOutstandingWIPandACRFiltering_Shipment_WithNullAgentType()
		{
			AssertOutstandingWIPandACRFiltering(JobTypes.Shipment, true);
		}

		public void TestOutstandingWIPandACRFiltering_Brokerage_WithNullAgentType()
		{
			AssertOutstandingWIPandACRFiltering(JobTypes.Brokerage, true);
		}

		public void TestJobInactive()
		{
			var plugIn1 = CreateShipmentOrDeclaration(JobTypes.Shipment, "0001");
			var job1 = TestObjectCreator.CreateJob(plugIn1, false);
			TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, 0m, 20m);

			var plugIn2 = CreateShipmentOrDeclaration(JobTypes.Shipment, "0002");
			var job2 = TestObjectCreator.CreateJob(plugIn2, false);
			Factory.Save();

			TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, 10m, 0m);
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

			var headers = new[] { "JH_JobNum", "ACRAmount", "WIPAmount", "CSTAmount", "REVAmount" };
			var result = RunScript(false, false, true);

			var lines = new object[][]
						{
							new object[] {  plugIn1.JobNumber,      0m,     20m,    0m,     0m  },
							new object[] {  plugIn2.JobNumber,      -10m,     0m,    0m,     0m  },
						};
			AssertDataTableAllRowsByKeyColumns("Should have data with inactive job", result, headers, lines, headers);
		}

		void AssertOutstandingWIPandACRFiltering(JobTypes jobType, bool isNullAgentType = false)
		{
			var plugIn1 = CreateShipmentOrDeclaration(jobType, "0001");
			var job1 = TestObjectCreator.CreateJob(plugIn1, false);
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, 0m, 0m);

			var plugIn2 = CreateShipmentOrDeclaration(jobType, "0002");
			var job2 = TestObjectCreator.CreateJob(plugIn2, false);
			TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, 10m, 0m);

			var plugIn3 = CreateShipmentOrDeclaration(jobType, "0003");
			var job3 = TestObjectCreator.CreateJob(plugIn3, false);
			TestObjectCreator.CreateCharge(job3, TestObjectCreator.CC1, 0m, 20m);

			var plugIn4 = CreateShipmentOrDeclaration(jobType, "0004");
			var job4 = TestObjectCreator.CreateJob(plugIn4, false);
			TestObjectCreator.CreateCharge(job4, TestObjectCreator.CC1, 30m, 40m);

			var plugIn5 = CreateShipmentOrDeclaration(jobType, "0005");
			var job5 = TestObjectCreator.CreateJob(plugIn5, false);
			TestObjectCreator.CreateCharge(job5, TestObjectCreator.CC1, 89m, 89m);
			TestObjectCreator.CreateCharge(job5, TestObjectCreator.CC2, -89m, -89m);

			var plugIn6 = CreateShipmentOrDeclaration(jobType, "0006");
			var job6 = TestObjectCreator.CreateJob(plugIn6, false);
			TestObjectCreator.CreateCharge(job6, TestObjectCreator.CC1, 79m, 79m);
			TestObjectCreator.CreateCharge(job6, TestObjectCreator.CC1, -79m, -79m);

			Factory.Save();

			var headers = new[] { "JH_JobNum", "ACRAmount", "WIPAmount", "CSTAmount", "REVAmount" };
			var result = RunScript(false, false, isNullAgentType);

			var lines = new object[][]
						{
							new object[] {	plugIn2.JobNumber,		-10m,	0m,		0m,		0m	},
							new object[] {	plugIn3.JobNumber,		0m,		20m,	0m,		0m	},
							new object[] {	plugIn4.JobNumber,		-30m,	40m,	0m,		0m	},
							new object[] {	plugIn5.JobNumber,		0m,		0m,		0m,		0m	},
							new object[] {	plugIn6.JobNumber,		0m,		0m,		0m,		0m	},
						};
			AssertDataTableAllRowsByKeyColumns("Filtering without outstanding WIP, without outstanding ACR", result, headers, lines, headers);

			result = RunScript(true, false, isNullAgentType);
			lines = new object[][]
						{
							new object[] {	plugIn3.JobNumber,		0m,		20m,	0m,		0m	},
							new object[] {	plugIn4.JobNumber,		-30m,	40m,	0m,		0m	},
							new object[] {	plugIn5.JobNumber,		0m,		0m,		0m,		0m	},
							new object[] {	plugIn6.JobNumber,		0m,		0m,		0m,		0m	},
						};
			AssertDataTableAllRowsByKeyColumns("Filtering with outstanding WIP, without outstanding ACR", result, headers, lines, headers);

			result = RunScript(false, true, isNullAgentType);
			lines = new object[][]
						{
							new object[] {	plugIn2.JobNumber,		-10m,	0m,		0m,		0m	},
							new object[] {	plugIn4.JobNumber,		-30m,	40m,	0m,		0m	},
							new object[] {	plugIn5.JobNumber,		0m,		0m,		0m,		0m	},
							new object[] {	plugIn6.JobNumber,		0m,		0m,		0m,		0m	},
						};
			AssertDataTableAllRowsByKeyColumns("Filtering without outstanding WIP, with outstanding ACR", result, headers, lines, headers);

			result = RunScript(true, true, isNullAgentType);
			lines = new object[][]
						{
							new object[] {	plugIn2.JobNumber,		-10m,	0m,		0m,		0m	},
							new object[] {	plugIn3.JobNumber,		0m,		20m,	0m,		0m	},
							new object[] {	plugIn4.JobNumber,		-30m,	40m,	0m,		0m	},
							new object[] {	plugIn5.JobNumber,		0m,		0m,		0m,		0m	},
							new object[] {	plugIn6.JobNumber,		0m,		0m,		0m,		0m	},
						};
			AssertDataTableAllRowsByKeyColumns("Filtering with outstanding WIP, with outstanding ACR", result, headers, lines, headers);
		}

		IJobInvoicingPlugIn CreateShipmentOrDeclaration(JobTypes jobType, string num)
		{
			IJobInvoicingPlugIn plugIn = null;
			switch (jobType)
			{
				case JobTypes.Shipment:
					plugIn = TestObjectCreator.CreateShipment("S" + num);
					break;
				case JobTypes.Brokerage:
					plugIn = (IJobInvoicingPlugIn)TestObjectCreator.CreateDeclaration("B" + num);
					break;
			}
			return plugIn;
		}

		DataTable RunScript(bool outstandingWIP, bool outstandingACR, bool isNullAgentType)
		{
			var sql = string.Format(@"
SELECT * FROM Report_JobProfitTransactionTotalsByBranchAndDepartment(
'{0}',					--@JH_GC
'All',					--@JobType
'{2}',					--@AL_OutstandingWIPOnly
'{3}',					--@AL_OutstandingACROnly
NULL,					--@AC_ChargeGroup
NULL,					--@AC_AR_SalesGroup
NULL,					--@AC_AR_ExpenseGroup
'',						--@AL_BranchPKList
'',						--@AL_DepartmentPKList
'',						--@AL_ChargeCodePKList
'',						--@AL_ExcludedChargeCodePKList
'',						--@AL_CreditorPKList
'',						--@AL_DebtorPKList
'1900-01-01 00:00:00',	--@AL_FromDate
'2079-06-06 23:59:29',	--@AL_ToDate
'',						--@JK_ConsolMode
'',						--@JK_TransportMode
{4},					--@JK_AgentType
NULL,					--@SendingForwarderPK
NULL,					--@ReceivingForwarderPK
NULL,					--@CreditorPK
NULL,					--@ShippingLinePK
NULL,					--@JK_JX_LoadPort
NULL,					--@JK_JX_DischargePort
'1900-01-01 00:00:00',	--@JK_JX_FromETD
'2079-06-06 23:59:29',	--@JK_JX_ToETD
'1900-01-01 00:00:00',	--@JK_JX_FromETA
'2079-06-06 23:59:29',	--@JK_JX_ToETA
'{1}',					--@CurrentCountry
'1900-01-01 00:00:00',	--@RevRecogFrom
'2079-06-06 23:59:29',	--@RevRecogTo
''					--@Gateway
)",
						GlbCompany.CurrentCompany.PK,							//@JH_GC
						GlbCompany.CurrentCompany.GC_RN_NKCountryCode,			//@CurrentCountry
						outstandingWIP ? "Y" : "",								//@AL_OutstandingWIPOnly
						outstandingACR ? "Y" : "",                              //@AL_OutstandingACROnly
						isNullAgentType ? "NULL" : @"''"                        //@JK_AgentType
						);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}

