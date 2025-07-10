

using Enterprise.Accounting.Utility.Testing;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using CargoWise.Data;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.ARAP.Invoicing;
	using Enterprise.Accounting.Registry.Business;
	using Enterprise.Customs.Business;
	using Enterprise.Freight.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Business.Testing;
	using Enterprise.ZArchitecture.Schema;

	class Report_JobProfitForwardingAndCustomsSummaryByJobTest : ScriptTest
	{
		public void TestForwardingJobProfitAndCustomsSummaryReport_UseARSettlementGroup()
		{
			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0m, TestObjectCreator.ABIGAS);

			Factory.Save();

			DataTable jobForwardProfitTable = RunScript(Core.Constants.TransportModes.Sea);
			var records = jobForwardProfitTable.Select("JH_JOBNUM = 'S001001'");
			AssertEquals(1, records.Length);
			AssertEquals("Should use local client's code", TestObjectCreator.LocalClient.OH_Code, records[0]["JobLocalClientARSettlementGroupCode"]);
			AssertEquals("Should use overseas agent's code", TestObjectCreator.Agent.OH_Code, records[0]["JobOverseasAgentARSettlementGroupCode"]);

			TestObjectCreator.CreateARSettlementGroup(TestObjectCreator.LocalClient, TestObjectCreator.LocalClient2);
			TestObjectCreator.CreateARSettlementGroup(TestObjectCreator.Agent, TestObjectCreator.Agent2);
			Factory.Save();

			jobForwardProfitTable = RunScript(Core.Constants.TransportModes.Sea);
			records = jobForwardProfitTable.Select("JH_JOBNUM = 'S001001'");
			AssertEquals(1, records.Length);
			AssertEquals("Should use local client's AR Settlement Group code", TestObjectCreator.LocalClient2.OH_Code, records[0]["JobLocalClientARSettlementGroupCode"]);
			AssertEquals("Should use overseas agent's AR Settlement Group code", TestObjectCreator.Agent2.OH_Code, records[0]["JobOverseasAgentARSettlementGroupCode"]);
		}

		public void TestForwardingJobProfitAndCustomsSummaryReport_ForShipmentWithoutConsigneeOrConsignor()
		{
			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = TestObjectCreator.CreateJob(shipment);

			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0m, TestObjectCreator.ABIGAS);

			Factory.Save();

			AssertNull("Consignee is null", shipment.Consignee);
			AssertNull("Consignor is null", shipment.Consignor);

			DataTable jobForwardProfitTable = RunScript(Core.Constants.TransportModes.Sea);

			var records = jobForwardProfitTable.Select("JH_JOBNUM = 'S001001'");
			AssertEquals("Report should contain shipment even without consigee and consignor", 1, records.Length);
		}

		public void TestForwardingJobProfitAndCustomsSummaryReport_WithCustomsDeclarationJobs_WithMultipleContainers()
		{
			var declaration = TestObjectCreator.CreateDeclaration("B0000100");
			var job = TestObjectCreator.CreateJob((IJobInvoicingPlugIn)declaration, false);

			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);

			TestObjectCreator.CreateContainer(declaration, "QWER098765");

			Factory.Save();

			DataTable jobForwardProfitTable = RunScript(null);

			AssertEquals("Report returned one record", 1, jobForwardProfitTable.Rows.Count);
			AssertEquals("Report should show Acrual amount as -100.00", -100.00M, (decimal)jobForwardProfitTable.Rows[0]["ACRAMOUNT"]);

			TestObjectCreator.CreateContainer(declaration, "QRER009123");
			Factory.Save();

			jobForwardProfitTable = RunScript(null);

			AssertEquals("Report returned one record", 1, jobForwardProfitTable.Rows.Count);
			AssertEquals("Report should show Acrual amount as -100.00", -100.00M, (decimal)jobForwardProfitTable.Rows[0]["ACRAMOUNT"]);
		}

		public void TestForwardingJobProfitAndCustomsSummaryReport_WithCustomsDeclarationJobs_WithContainerTypeFortyFoot()
		{
			var declaration = TestObjectCreator.CreateDeclaration("B0000100");
			var job = TestObjectCreator.CreateJob((IJobInvoicingPlugIn)declaration, false);

			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);

			var container = TestObjectCreator.CreateContainer(declaration, "QWER098765");
			Factory.Save();

			DataTable jobForwardProfitTable = RunScript(null);

			AssertEquals("Report returned one record", 1, jobForwardProfitTable.Rows.Count);
			AssertEquals("Report should show FortyFootEquivalentUnit amount as 0", 0, (int)jobForwardProfitTable.Rows[0]["FortyFootEquivalentUnit"]);

			container.CO_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			Factory.Save();

			jobForwardProfitTable = RunScript(null);

			AssertEquals("Report returned one record", 1, jobForwardProfitTable.Rows.Count);
			AssertEquals("Report should show FortyFootEquivalentUnit amount as 1", 1, (int)jobForwardProfitTable.Rows[0]["FortyFootEquivalentUnit"]);
		}

		public void TestForwardingJobProfitAndCustomsSummaryReport_ForISFjob_WithBuyingAndSellingPartyAddress()
		{
			var importer = TestObjectCreator.CreateOrgHeader("DMIMPTR", false, false);
			importer.OH_FullName = "Dummy importer";

			var isfType10 = TestObjectCreator.CreateISFHeader("1", "ISF00002", importer);

			var addressProvider = isfType10 as IDocAddresses;

			var buyingAddress = addressProvider.DocAddresses.AddNew(MasterFiles.Integration.DocAddressType.BuyingParty);
			buyingAddress.E2_AddressOverride = true;
			buyingAddress.E2_CompanyName = "DummyBuyCompany";

			var sellingAddress1 = addressProvider.DocAddresses.AddNew(MasterFiles.Integration.DocAddressType.SellingParty);
			sellingAddress1.E2_AddressOverride = true;
			sellingAddress1.E2_CompanyName = "DummySellCompany1";

			var sellingAddress2 = addressProvider.DocAddresses.AddNew(MasterFiles.Integration.DocAddressType.SellingParty);
			sellingAddress2.E2_AddressOverride = true;
			sellingAddress2.E2_CompanyName = "DummySellCompany2";

			var job = TestObjectCreator.CreateJob((IJobInvoicingPlugIn)isfType10, false);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);
			Factory.Save();

			DataTable jobForwardProfitTable = RunScript(null);
			var records = jobForwardProfitTable.Select("JH_JOBNUM = 'ISF00002'");
			AssertEquals("Report should return one record", 1, records.Length);
			AssertEquals("The record is for -100 ACR", -100M, (decimal)records[0]["ACRAMOUNT"]);
			AssertEquals("Report should show overriden ConsigneeImporterCode", "ZDMIMPTR", records[0]["ConsigneeImporterCode"].ToString());
			AssertEquals("Report should show overriden ConsigneeImporterFullName", "Dummy importer", records[0]["ConsigneeImporterFullName"].ToString());
			AssertEquals("Report should show overriden ConsignorShipperSuplierFullName", "DummySellCompany1", records[0]["ConsignorShipperSuplierFullName"].ToString());

			var isfType5 = TestObjectCreator.CreateISFHeader("2", "ISF00005", importer);

			addressProvider = isfType5 as IDocAddresses;

			buyingAddress = addressProvider.DocAddresses.AddNew(MasterFiles.Integration.DocAddressType.BuyingParty);
			buyingAddress.E2_AddressOverride = true;
			buyingAddress.E2_CompanyName = "DummyBuyCompany";

			sellingAddress1 = addressProvider.DocAddresses.AddNew(MasterFiles.Integration.DocAddressType.SellingParty);
			sellingAddress1.E2_AddressOverride = true;
			sellingAddress1.E2_CompanyName = "DummySellCompany1";

			sellingAddress2 = addressProvider.DocAddresses.AddNew(MasterFiles.Integration.DocAddressType.SellingParty);
			sellingAddress2.E2_AddressOverride = true;
			sellingAddress2.E2_CompanyName = "DummySellCompany2";

			job = TestObjectCreator.CreateJob((IJobInvoicingPlugIn)isfType5, false);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);
			Factory.Save();

			jobForwardProfitTable = RunScript(null);
			records = jobForwardProfitTable.Select("JH_JOBNUM = 'ISF00005'");
			AssertEquals("Report should return one record", 1, records.Length);
			AssertEquals("The record is for -100 ACR", -100M, (decimal)records[0]["ACRAMOUNT"]);
			AssertEquals("Report should show overriden ConsigneeImporterCode", "ZDMIMPTR", records[0]["ConsigneeImporterCode"].ToString());
			AssertEquals("Report should show overriden ConsigneeImporterFullName", "Dummy importer", records[0]["ConsigneeImporterFullName"].ToString());
			Assert("Report should not show overriden ConsignorShipperSuplierFullName", String.IsNullOrEmpty(records[0]["ConsignorShipperSuplierFullName"].ToString()));
		}

		public void TestForwardingJobProfitAndCustomsSummaryReport_ShipmentTransportModeFilter()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S1");
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);
			var shipment2 = TestObjectCreator.CreateShipment("S2");
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);
			Factory.Save();

			DataTable jobForwardProfitTable = RunScript(Core.Constants.TransportModes.Air);
			var records = jobForwardProfitTable.Select();
			AssertEquals("Result should contain 1 record", 1, records.Length);
			AssertEquals("The record is for AIR shipment", Core.Constants.TransportModes.Air, (string)records[0]["TransportMode"]);

			jobForwardProfitTable = RunScript(Core.Constants.TransportModes.Sea);
			records = jobForwardProfitTable.Select();
			AssertEquals("Report should return one record", 1, records.Length);
			AssertEquals("The record is for SEA shipment", Core.Constants.TransportModes.Sea, (string)records[0]["TransportMode"]);
		}

		public void TestOutstandingWIPACR()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code, departmentCode: "FIP");
			var job1 = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code, departmentCode: "FIP");
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 102m, 101m);
			Factory.Save();
			charge.Accrual.AL_PostDate = new ZDateTime(2011, 1, 21);
			charge.WIP.AL_PostDate = new ZDateTime(2011, 1, 22);
			charge.ReverseAccrual(new ZDateTime(2011, 1, 23));
			charge.ReverseWIP(new ZDateTime(2011, 1, 23));

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "001", TestObjectCreator.AUD, 1.0m, 103.00m, 0m, 104.00m, 0m);
			var line1 = invoice.Lines[0];
			line1.AL_AC = TestObjectCreator.CC1.PK;
			line1.AL_JH = job1.PK;
			TestObjectCreator.CreateCharge(line1);
			Factory.Save();

			var result = RunScript(null, outstandingWIP: "Y");
			AssertEquals(2, result.Rows.Count);
			AssertEquals(1, result.Select("WIPAmount = 104 and CSTAmount = -104 and JH_JobNum = 'S00001001'").Length);
			AssertEquals(1, result.Select("WIPAmount = 101 and ACRAmount = -102 and JH_JobNum = 'S00001000'").Length);

			result = RunScript(null, outstandingACR: "Y");
			AssertEquals(1, result.Rows.Count);
			AssertEquals(1, result.Select("WIPAmount = 101 and ACRAmount = -102 and JH_JobNum = 'S00001000'").Length);
		}

		public void TestForwardingJobProfitAndCustomsSummaryReport_ShipmentContainerFilter()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S1");
			shipment1.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			TestObjectCreator.CreateCharge(job1, TestObjectCreator.FRT, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);
			Factory.Save();

			DataTable jobForwardProfitTable = RunScript(Core.Constants.TransportModes.Sea, true,"",null, Core.Constants.ContainerModes.FCL);
			var records = jobForwardProfitTable.Select();
			AssertEquals("Result should contain 0 record", 0, records.Length);

			jobForwardProfitTable = RunScript(Core.Constants.TransportModes.Sea, true, "", null, Core.Constants.ContainerModes.LCL);
			records = jobForwardProfitTable.Select();
			AssertEquals("Report should return one record", 1, records.Length);
			AssertEquals("The record is for LCL shipment", Core.Constants.ContainerModes.LCL, (string)records[0]["OPMODE"]);
		}

		public void TestForwardingJobProfitAndCustomsSummaryReport_NotIncludeDSBCharge()
		{
			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 0m, 100.00m, 0m);
			var line1 = invoice.Lines[0];
			line1.AL_AC = TestObjectCreator.CC1.PK;
			line1.AL_JH = job.PK;
			var charge1 = TestObjectCreator.CreateCharge(line1);

			var line2 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 50M);
			line2.AL_AC = TestObjectCreator.DSBChargeCode.PK;
			line2.AL_JH = job.PK;
			var charge2 = TestObjectCreator.CreateCharge(line2);

			Factory.Save();

			charge1.JR_LocalSellAmt = 0M;
			charge2.JR_LocalSellAmt = 0M;
			Factory.Save();

			var jobForwardProfitTable = RunScript(Core.Constants.TransportModes.Sea, true);
			var records = jobForwardProfitTable.Select("JH_JOBNUM = 'S001001'");
			AssertEquals(1, records.Length);
			AssertEquals("Should include DSB charge", -150M, records[0]["AL_LineAmount"]);
			AssertEquals("Should not include DSB charge", -150M, records[0]["AL_LineAmountAfterChargeExclusion"]);

			jobForwardProfitTable = RunScript(Core.Constants.TransportModes.Sea, false);
			records = jobForwardProfitTable.Select("JH_JOBNUM = 'S001001'");
			AssertEquals(1, records.Length);
			AssertEquals("Should not include DSB charge", -150M, records[0]["AL_LineAmount"]);
			AssertEquals("Should not include DSB charge", -100M, records[0]["AL_LineAmountAfterChargeExclusion"]);
		}

		public void TestForwardingJobProfitAndCustomsSummaryReport_WithNewGCN()
		{
			var gCNConsol = TestObjectCreator.CreateGatewayConsol("AUSYD", "NZAKL", "C002", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var gcnJob = TestObjectCreator.CreateJob(gCNConsol, false);
			TestObjectCreator.CreateCharge(gcnJob, TestObjectCreator.CC1, osCostAmt: 15M, creditor: TestObjectCreator.AALSHI, osSellAmt: 45M, debtor: TestObjectCreator.ABIGAS);
			Factory.Save();

			AssertEquals(true, gCNConsol.IsGatewayConsol);

			var dataTable = RunScript(null);
			AssertEquals(1, dataTable.Rows.Count);

			var headers = new string[] { "AL_LINEAMOUNT", "OPJOBTYPE", "JH_JOBNUM" };
			var lines = new List<object[]>
				{
					new object[] { 30M, "GW", "C002" },
				};

			AssertDataTableAllRowsByKeyColumns("", dataTable, headers, lines.ToArray());
		}

		public void TestForwardingJobProfitAndCustomsSummaryReport_JobBranchManagementCodeFilter()
		{
			var branch1 = GlbBranch.CurrentBranch;
			var branch2 = TestObjectCreator.NonCurrentBranch;

			var shipment1 = TestObjectCreator.CreateShipment("S1");
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			job1.JH_GB = branch1.PK;
			TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);
			var shipment2 = TestObjectCreator.CreateShipment("S2");
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			job2.JH_GB = branch2.PK;
			TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);
			Factory.Save();

			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.GlbBranch SET GB_AccountingGroupCode = 'BRA', GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_Code = '{0}' And GB_GC = '{1}'", branch1.GB_Code, branch1.GB_GC));
			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.GlbBranch SET GB_AccountingGroupCode = 'BRB', GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_Code = '{0}' And GB_GC = '{1}'", branch2.GB_Code, branch2.GB_GC));

			DataTable jobForwardProfitTable = RunScript(Core.Constants.TransportModes.Sea, true, "Where JobBranchManagementCode = 'BRA'");
			var records = jobForwardProfitTable.Select();
			AssertEquals("Result should contain 1 record", 1, records.Length);
			AssertEquals("The record is for branch1 with BranchManagementCode BRA", "BRA", (string)records[0]["JobBranchManagementCode"]);

			jobForwardProfitTable = RunScript(Core.Constants.TransportModes.Sea, true, "Where JobBranchManagementCode = 'BRB'");
			records = jobForwardProfitTable.Select();
			AssertEquals("Report should return one record", 1, records.Length);
			AssertEquals("The record is for branch2 with BranchManagementCode BRB", "BRB", (string)records[0]["JobBranchManagementCode"]);
		}

		public void TestForwardingJobProfitAndCustomsSummaryReport_ControllingCustomerAndAgent()
		{
			var runScript = new Func<string, ZGuid?, DataTable>((sql, pK) =>
			{
				return RunScript(Core.Constants.TransportModes.Sea, whereClause: sql, mNGOrgPK: pK);
			});

			var infoChecker = new CAGAndCCBInfoScriptTest();
			infoChecker.VerifyControllingCustomerAndAgentInfo(runScript);
		}

		public void TestForwardingJobProfitReport_WithCustomsDeclarationJobs_WithHouseBillAndMasterBillNumber()
		{
			var declaration = TestObjectCreator.CreateDeclaration();
			declaration.JE_HouseBill = "123456";
			declaration.JE_MasterBill = "MB123456";
			var job = TestObjectCreator.CreateJob((IJobInvoicingPlugIn)declaration, false);

			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);

			Factory.Save();

			DataTable table = RunScript(Core.Constants.TransportModes.Sea);

			AssertEquals("Report should return 1 records", 1, table.Rows.Count);

			AssertEquals("Report should have house bill number", "123456", table.Rows[0]["HouseBillNumber"]);
			AssertEquals("Report should have master bill number", "MB123456", table.Rows[0]["JK_MasterBillNum"]);
		}

		public void TestForwardingJobProfitAndCustomsSummaryReport_ActiveStatus()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, 10m, 0m);

			var shipment2 = TestObjectCreator.CreateShipment("S0002");
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			var charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, 0m, 11m);

			Factory.Save();

			var deActivateJobSql = $@"
UPDATE dbo.JobHeader
SET
	JH_IsActive = 0,
	JH_SystemLastEditTimeUtc = GETUTCDATE(),
	JH_SystemLastEditUser = '~BP'
WHERE
	JH_PK = '{job2.PK}'";
			DataUtils.GetDataTableFromQuery(Db.Connection, deActivateJobSql);

			var dataTable = RunScript(Core.Constants.TransportModes.Sea, activeStatus: "All");
			AssertEquals("Result should contain 2 records", 2, dataTable.Rows.Count);

			dataTable = RunScript(Core.Constants.TransportModes.Sea, activeStatus: "Active");
			AssertEquals("Result should contain 1 records", 1, dataTable.Select("JH_JobNum = 'S0001'").Length);

			dataTable = RunScript(Core.Constants.TransportModes.Sea, activeStatus: "Inactive");
			AssertEquals("Result should contain 1 records", 1, dataTable.Select("JH_JobNum = 'S0002'").Length);
		}

		public void TestChargeTypeIsCalculatedCorrectly_Shipment()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateChargeTypeOverride(testObjectCreator.CC1, chargeType: Core.Constants.ChargeType.Disbursement, margin: 100, jobType: JobInvoicingConsumerTypes.AgencyBillOfLading.Code);
			Factory.Save();

			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, 300m, 400m);

			var shipment2 = TestObjectCreator.CreateShipment("S0002");
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			var charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC3, 300m, 400m);

			testObjectCreator.CreateChargeTypeOverride(testObjectCreator.CC3, chargeType: Core.Constants.ChargeType.Disbursement, margin: 100, jobType: JobInvoicingConsumerTypes.Shipment.Code);
			Factory.Save();

			foreach (var regValue in new[] { true, false })
			{
				using (AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue))
				{
					var dataTable = RunScript(Core.Constants.TransportModes.Sea, includeDSBCharge: regValue, activeStatus: "All");
					AssertEquals("Result should contain 2 records", 2, dataTable.Rows.Count);

					var rows = dataTable.Rows.ToList<DataRow>().OrderBy((r) => Convert.ToString(r["JH_JobNum"])).ToArray();
					AssertEquals(Convert.ToDecimal(rows[0]["AL_LineAmount"]), Convert.ToDecimal(rows[0]["AL_LineAmountAfterChargeExclusion"]));
					AssertEquals(regValue ? Convert.ToDecimal(rows[1]["AL_LineAmount"]) : 0, Convert.ToDecimal(rows[1]["AL_LineAmountAfterChargeExclusion"]));
				}
			}
		}

		public void TestChargeTypeIsCalculatedCorrectly_JobDecleration()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateChargeTypeOverride(testObjectCreator.CC1, chargeType: Core.Constants.ChargeType.Disbursement, margin: 100, jobType: JobInvoicingConsumerTypes.AgencyBillOfLading.Code);
			Factory.Save();

			var declaration1 = TestObjectCreator.CreateDeclaration("B001");
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			var job1 = TestObjectCreator.CreateJob(declaration1 as IJobInvoicingPlugIn, testObjectCreator.LocalClient, 2.0M, testObjectCreator.Agent, 1.0M);
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, 300m, 400m);

			var declaration2 = TestObjectCreator.CreateDeclaration("B002");
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			var job2 = TestObjectCreator.CreateJob(declaration2 as IJobInvoicingPlugIn, testObjectCreator.LocalClient, 2.0M, testObjectCreator.Agent, 1.0M);
			var charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC3, 300m, 400m);

			testObjectCreator.CreateChargeTypeOverride(testObjectCreator.CC3, chargeType: Core.Constants.ChargeType.Disbursement, margin: 100, jobType: JobInvoicingConsumerTypes.Brokerage.Code);
			Factory.Save();

			foreach (var regValue in new[] { true, false })
			{
				using (AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue))
				{
					var dataTable = RunScript(Core.Constants.TransportModes.Sea, includeDSBCharge: regValue, activeStatus: "All");
					AssertEquals("Result should contain 2 records", 2, dataTable.Rows.Count);

					var rows = dataTable.Rows.ToList<DataRow>().OrderBy((r) => Convert.ToString(r["JH_JobNum"])).ToArray();
					AssertEquals(Convert.ToDecimal(rows[0]["AL_LineAmount"]), Convert.ToDecimal(rows[0]["AL_LineAmountAfterChargeExclusion"]));
					AssertEquals(regValue ? Convert.ToDecimal(rows[1]["AL_LineAmount"]) : 0, Convert.ToDecimal(rows[1]["AL_LineAmountAfterChargeExclusion"]));
				}
			}
		}

		public void TestChargeTypeIsCalculatedCorrectly_CFSLoadList()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateChargeTypeOverride(testObjectCreator.CC1, chargeType: Core.Constants.ChargeType.Disbursement, margin: 100, jobType: JobInvoicingConsumerTypes.AgencyBillOfLading.Code);
			Factory.Save();

			var jobParent1 = TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.CFSLoadList) as CommonConsol;
			jobParent1.JK_UniqueConsignRef = "C001";
			jobParent1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			var job1 = TestObjectCreator.CreateJob(jobParent1 as IJobInvoicingPlugIn, testObjectCreator.LocalClient, 2.0M, testObjectCreator.Agent, 1.0M);
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, 300m, 400m);

			var jobParent2 = TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.CFSLoadList) as CommonConsol;
			jobParent2.JK_UniqueConsignRef = "C002";
			jobParent2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			var job2 = TestObjectCreator.CreateJob(jobParent2 as IJobInvoicingPlugIn, testObjectCreator.LocalClient, 2.0M, testObjectCreator.Agent, 1.0M);
			var charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC3, 300m, 400m);

			testObjectCreator.CreateChargeTypeOverride(testObjectCreator.CC3, chargeType: Core.Constants.ChargeType.Disbursement, margin: 100, jobType: JobInvoicingConsumerTypes.GatewayConsol.Code);
			Factory.Save();

			foreach (var regValue in new[] { true, false })
			{
				using (AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue))
				{
					var dataTable = RunScript(Core.Constants.TransportModes.Sea, includeDSBCharge: regValue, activeStatus: "All");
					AssertEquals("Result should contain 2 records", 2, dataTable.Rows.Count);

					var rows = dataTable.Rows.ToList<DataRow>().OrderBy((r) => Convert.ToString(r["JH_JobNum"])).ToArray();
					AssertEquals(Convert.ToDecimal(rows[0]["AL_LineAmount"]), Convert.ToDecimal(rows[0]["AL_LineAmountAfterChargeExclusion"]));
					AssertEquals(regValue ? Convert.ToDecimal(rows[1]["AL_LineAmount"]) : 0, Convert.ToDecimal(rows[1]["AL_LineAmountAfterChargeExclusion"]));
				}
			}
		}

		public void TestChargeTypeIsCalculatedCorrectly_ISF()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateChargeTypeOverride(testObjectCreator.CC1, chargeType: Core.Constants.ChargeType.Disbursement, margin: 100, jobType: JobInvoicingConsumerTypes.AgencyBillOfLading.Code);
			Factory.Save();

			var jobParent1 = TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.ImporterSecurityFiling);
			var job1 = TestObjectCreator.CreateJob(jobParent1, testObjectCreator.LocalClient, 2.0M, testObjectCreator.Agent, 1.0M);
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, 300m, 400m);

			var jobParent2 = TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.ImporterSecurityFiling);
			var job2 = TestObjectCreator.CreateJob(jobParent2, testObjectCreator.LocalClient, 2.0M, testObjectCreator.Agent, 1.0M);
			var charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC3, 600m, 800m);

			testObjectCreator.CreateChargeTypeOverride(testObjectCreator.CC3, chargeType: Core.Constants.ChargeType.Disbursement, margin: 100, jobType: JobInvoicingConsumerTypes.ImporterSecurityFiling.Code);
			Factory.Save();

			foreach (var regValue in new[] { true, false })
			{
				using (AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue))
				{
					var dataTable = RunScript(Core.Constants.TransportModes.Sea, includeDSBCharge: regValue, activeStatus: "All");
					AssertEquals("Result should contain 2 records", 2, dataTable.Rows.Count);

					var rows = dataTable.Rows.ToList<DataRow>().OrderBy((r) => Convert.ToDecimal(r["WIPAMOUNT"])).ToArray();
					AssertEquals(Convert.ToDecimal(rows[0]["AL_LineAmount"]), Convert.ToDecimal(rows[0]["AL_LineAmountAfterChargeExclusion"]));
					AssertEquals(regValue ? Convert.ToDecimal(rows[1]["AL_LineAmount"]) : 0, Convert.ToDecimal(rows[1]["AL_LineAmountAfterChargeExclusion"]));
				}
			}
		}

		DataTable RunScript(string shipmentTransportMode, bool includeDSBCharge = true, string whereClause = "", ZGuid? mNGOrgPK = null, string shipmentContainer = null, string outstandingWIP = "", string outstandingACR = "", string activeStatus = "")
		{
			var sqlQuery = string.Format(@"
				SELECT JH_JOBNUM, JH_JOBLOCALREFERENCE, TRANSPORTMODE, OPMODE, JH_SYSTEMCREATETIMEUtc, JH_GB_CODE, JobBranchManagementCode, JH_GE_CODE, JH_STATUS, JH_LOCALCLIENTCODE, REVAMOUNT, WIPAMOUNT, CSTAMOUNT, ACRAMOUNT, AL_LINEAMOUNT, AL_LineAmountAfterChargeExclusion, JK_UNIQUECONSIGNREF, JK_AGENTTYPE, SHIPPINGLINECODE, JK_CONSOLMODE, CREDITORCODE, RECEIVINGFORWARDERCODE, SENDINGFORWARDERCODE, JK_TRANSPORTMODE, NKDESTINATION, DESTINATIONETA, DISCHARGEPORT, DISCHARGEPORTETA, JH_A_JCL, 
				JH_GS_NKREPOPS, JH_OVERSEASAGENTCODE, JH_GS_SALESREP_CODE, LOADPORT, LOADPORTETD, OPJOBTYPE, NKORIGIN, ORIGINETD, CONSIGNEEIMPORTERCODE, CONSIGNEEIMPORTERFULLNAME, CONSIGNORSHIPPERSUPLIERCODE, 
				CONSIGNORSHIPPERSUPLIERFULLNAME, JOBLOCALCLIENTARSETTLEMENTGROUPCODE, JOBLOCALCLIENTARSETTLEMENTGROUPFULLNAME, JOBOVERSEASAGENTARSETTLEMENTGROUPCODE, JOBOVERSEASAGENTARSETTLEMENTGROUPFULLNAME, ACTUALWEIGHT, UNITOFWEIGHT, 
				ACTUALVOLUME, UNITOFVOLUME, ACTUALCHARGEABLE, CHARGEABLEUNIT, TEU, TWENTYFOOTEQUIVALENTUNIT, FORTYFOOTEQUIVALENTUNIT, CCBOrgName, CAGOrgName, MNGName, HouseBillNumber, JK_MasterBillNum
				FROM Report_JobProfitForwardingAndCustomsSummaryByJob
				(
				N'{0}'								--@CurrentCountry
				, '{1}'								--@CompanyPK
				, '1900-01-01 00:00:00'				--@TransactionFrom
				, '2079-06-06 23:59:29'				--@TransactionTo
				, ''								--@JobType
				, N'{5}'							--@OutstandingWIP
				, N'{6}'							--@OutstandingACR
				, N''								--@ChargeCode
				, N''								--@ExcludeChargeCode
				, NULL								--@ChargeGroup
				, NULL								--@SalesGroup
				, NULL								--@ExpenseGroup
				, N''								--@TransactionBranch
				, N''								--@TransactionDepartment
				, N''								--@TransactionDebtor
				, N''								--@TransactionCreditor
				, ''								--@PostedOnly
				, '1900-01-01 00:00:00'				--@RevRecogFrom
				, '2079-06-06 23:59:29'				--@RevRecogTo
				, '{2}'								--@ShipmentTransport
				,NULL								--@DeclarationTransport
				, @ShipmentCont
				,NULL								--@DeclarationCont
				,NULL								--@ISFTransportMode
				,NULL								--@ISFShipmentType
				,{3}								--@IncludeDSBCharge
				, @MNGListValue
				, @MNGListIsEmptyValue
				, 'ALL'								--@Gateway
				, '{7}'								--@ActiveStatus
				) 
				{4}"
				, "AU"
				, GlbCompany.CurrentCompany.PK.ToString()
				, shipmentTransportMode
				, includeDSBCharge ? 1 : 0
				, whereClause
				, outstandingWIP
				, outstandingACR
				, activeStatus
			 );

			var command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@ShipmentCont", SqlDbType.Char, (object)shipmentContainer ?? DBNull.Value);
			AddTVPAndIsEmptyParameters(command, "@MNGListValue", "dbo.TVP_uniqueidentifier", "@MNGListIsEmptyValue", mNGOrgPK.HasValue ? new Guid[] { mNGOrgPK.Value.ToGuid() } : Array.Empty<Guid>());
			return DataUtils.GetDataTableFromCommand(command);
		}
	}
}


