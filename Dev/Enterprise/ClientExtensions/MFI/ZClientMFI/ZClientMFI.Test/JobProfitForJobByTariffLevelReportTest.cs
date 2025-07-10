using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.MFI.Testing
{
	public class JobProfitForJobByTariffLevelReportTest : TestCaseWithFactory
	{
		#region TestJobProfitReportForShipmentAndJobDec
		[TestDate(2006, 12, 30)]
		public void TestJobProfitReportForShipmentAndJobDec()
		{
			//import Shipment
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneePK = ConsigneeIMP.PK;
			shipment.ConsignorPK = ConsignorIMP.PK;
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_UniqueConsignRef = "S00001001";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			ZDecimal amount = 1000;
			Job job = Helper.GetJobHeader(shipment.PK, JobShipmentSchema.Constants.TableName, shipment.JS_UniqueConsignRef, ConsigneeIMP, GlbStaff.CurrentUser, SalesRep);
			var apHeader = Helper.GetTransactionHeader(job.PK, amount, "SHIP1", LedgerTypes.AccountsPayable, ConsigneeIMP, TransactionTypes.Invoice);
			var arHeader = Helper.GetTransactionHeader(job.PK, amount, "SHIP1", LedgerTypes.AccountsReceivable, ConsigneeIMP, TransactionTypes.Invoice);
			Helper.GetTransactionLines(apHeader.PK, job.PK, TransactionLineTypes.Cost, -200m, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Registry.FreightChargeCode);
			Helper.GetTransactionLines(apHeader.PK, job.PK, TransactionLineTypes.Accrual, 100m, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value);
			Helper.GetTransactionLines(arHeader.PK, job.PK, TransactionLineTypes.Revenue, 1000m, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Registry.FreightChargeCode);
			Helper.GetTransactionLines(arHeader.PK, job.PK, TransactionLineTypes.WIP, -50m, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value);
			Factory.Save();
			//export Shipment
			TestDateAttribute.Date = new ZDateTime(2007, 1, 1).ToDateTime();
			shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneePK = ConsigneeEXP.PK;
			shipment.ConsignorPK = ConsignorEXP.PK;
			shipment.JS_RL_NKDestination = "HKHKG";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_UniqueConsignRef = "S00001002";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			amount = 2000m;
			Factory.Save();
			job = Helper.GetJobHeader(shipment.PK, JobShipmentSchema.Constants.TableName, shipment.JS_UniqueConsignRef, ConsigneeEXP, GlbStaff.CurrentUser, null);
			arHeader = Helper.GetTransactionHeader(job.PK, amount, "SHIP2", LedgerTypes.AccountsReceivable, ConsigneeEXP, TransactionTypes.Invoice);
			apHeader = Helper.GetTransactionHeader(job.PK, amount, "SHIP2", LedgerTypes.AccountsPayable, ConsigneeEXP, TransactionTypes.Invoice);
			Helper.GetTransactionLines(apHeader.PK, job.PK, TransactionLineTypes.Cost, -300m, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Registry.FreightChargeCode);
			Helper.GetTransactionLines(arHeader.PK, job.PK, TransactionLineTypes.Revenue, 2000m, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Registry.FreightChargeCode);
			Factory.Save();
			TestDateAttribute.Date = new ZDateTime(2007, 1, 2).ToDateTime();
			//Standalone Declaration
			JobDeclaration jobDec = Factory.New<JobDeclaration>();
			jobDec.JE_OH_Importer = ConsigneeIMP.PK;
			jobDec.JE_OH_Supplier = ConsignorIMP.PK;
			jobDec.JE_RL_NKOrigin = "NZCHC";
			jobDec.JE_RL_NKFinalDestination = "AUMEL";
			jobDec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			jobDec.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			jobDec.JE_DeclarationReference = "B00001001";
			amount = 3000m;
			job = Helper.GetJobHeader(jobDec.PK, JobDeclarationSchema.Constants.TableName, jobDec.JE_DeclarationReference, ConsigneeIMP, SalesRep, SalesRep);
			arHeader = Helper.GetTransactionHeader(job.PK, amount, "JOBDEC", LedgerTypes.AccountsReceivable, ConsigneeIMP, TransactionTypes.Invoice);
			apHeader = Helper.GetTransactionHeader(job.PK, amount, "JOBDEC", LedgerTypes.AccountsPayable, ConsigneeIMP, TransactionTypes.Invoice);
			Helper.GetTransactionLines(arHeader.PK, job.PK, TransactionLineTypes.WIP, -400m, GlbBranch.CurrentBranch, DummyDept, Env.Registry.FreightChargeCode);
			Helper.GetTransactionLines(apHeader.PK, job.PK, TransactionLineTypes.Accrual, 400m, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Registry.FreightChargeCode);
			Factory.Save();
			//No Filter
			DataTable table = Utilities.GetDataTableFromQuery(Db.Connection, "select * from Client_MFI_JobProfitForShipmentOrJobDec('" + GlbCompany.CurrentCompany.PK + "' , '', '', '01/01/1900', '01/01/2079', '" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', null, null, '', '', '', '') ORDER BY JH_JobNum");
			AssertEquals("Query should return 3 records", 3, table.Rows.Count);
			DataRow row = table.Rows[0];
			AssertJobData(row, "B00001001", GlbBranch.CurrentBranch.GB_Code, GlbDepartment.CurrentDepartment.GE_Code, "WRK", Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL, SalesRep.GS_Code, SalesRep.GS_Code, "2", 0m, 0m, 400m, 0m, -400m);
			row = table.Rows[1];
			AssertJobData(row, "S00001001", GlbBranch.CurrentBranch.GB_Code, GlbDepartment.CurrentDepartment.GE_Code, "WRK", Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.Loose, SalesRep.GS_Code, GlbStaff.CurrentUser.GS_Code, "2", 750m, 1000m, 50m, -200m, -100m);
			row = table.Rows[2];
			AssertJobData(row, "S00001002", "SYD", GlbDepartment.CurrentDepartment.GE_Code, "WRK", Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.Loose, ZString.Empty, GlbStaff.CurrentUser.GS_Code, "", 1700m, 2000m, 0m, -300m, 0m);
			//filtered by posted date
			table = Utilities.GetDataTableFromQuery(Db.Connection, "select * from Client_MFI_JobProfitForShipmentOrJobDec('" + GlbCompany.CurrentCompany.PK + "' , '', '', '01/02/2007', '01/02/2007', '" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', null, null, '', '', '', '') ORDER BY JH_JobNum");
			AssertEquals("Query should return 1 record", 1, table.Rows.Count);
			row = table.Rows[0];
			AssertEquals("Job returned is the standalone declaraton", "B00001001", row["JH_JobNum"]);
			//filtered by TariffLevel
			table = Utilities.GetDataTableFromQuery(Db.Connection, "select * from Client_MFI_JobProfitForShipmentOrJobDec('" + GlbCompany.CurrentCompany.PK + "' , '', '', '01/02/1900', '01/02/2079', '" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "', '2', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', null, null, '', '', '', '') ORDER BY JH_JobNum");
			AssertEquals("Query should return 2 records", 2, table.Rows.Count);
			row = table.Rows[0];
			AssertEquals("Job returned is the standalone declaraton", "B00001001", row["JH_JobNum"]);
			row = table.Rows[1];
			AssertEquals("Job returned is shipment 'S00001001'", "S00001001", row["JH_JobNum"]);
			//filtered by Department
			table = Utilities.GetDataTableFromQuery(Db.Connection, "select * from Client_MFI_JobProfitForShipmentOrJobDec('" + GlbCompany.CurrentCompany.PK + "', '', '" + DummyDept.PK + "', '01/02/1900', '01/02/2079', '" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "', '2', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', null, null, '', '', '', '') ORDER BY JH_JobNum");
			AssertEquals("Query should return 1 record", 1, table.Rows.Count);
			row = table.Rows[0];
			AssertJobData(row, "B00001001", GlbBranch.CurrentBranch.GB_Code, GlbDepartment.CurrentDepartment.GE_Code, "WRK", Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL, SalesRep.GS_Code, SalesRep.GS_Code, "2", 400m, 0m, 400m, 0m, 0m);
		}

		#endregion
		void AssertJobData(DataRow row, ZString jobNum, ZString br, ZString dep, ZString stat, ZString trans, ZString cont, string sales, string op, ZString tariffLvl, ZDecimal jobProfit, ZDecimal revenue, ZDecimal wIP, ZDecimal cost, ZDecimal accrual)
		{
			AssertEquals("Job Num", jobNum, row["JH_JobNum"]);
			AssertEquals("Branch", br, row["JH_BranchCode"]);
			AssertEquals("Dept", dep, row["JH_DepartmentCode"]);
			AssertEquals("Stat", stat, row["JH_Status"]);
			AssertEquals("Trans", trans, row["FW_TransportMode"]);
			AssertEquals("Cont", cont, row["FW_ContainerMode"]);
			AssertEquals("TariffLvl", tariffLvl, row["OrgTariffLvl"].ToString().Trim());
			AssertEquals("Sales", sales, row["JH_SalesRepInitials"].ToString());
			AssertEquals("Op", op, row["JH_OperatorInitials"].ToString().Trim());
			AssertEquals("JobProfit", jobProfit, row["JH_Profit"]);
			AssertEquals("Revenue", revenue, row["REVAmount"]);
			AssertEquals("WIP", wIP, row["WIPAmount"]);
			AssertEquals("CSTAmount", cost, row["CSTAmount"]);
			AssertEquals("ACRAmount", accrual, row["ACRAmount"]);
		}

		#region TestJobProfitReportForConsol
		[TestDate(2006, 12, 30)]
		public void TestJobProfitReportForConsol()
		{
			ZDateTime consol1ETD = ZDateTime.Now;
			ZDateTime consol1ETA = ZDateTime.Now.AddDays(1);
			//import Shipment
			ForwardingConsol consol = Helper.SetupConsol(Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.Loose, Core.Constants.AgentType.Agent, "NZAKL", "AUBNE", consol1ETD, consol1ETA);
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneePK = ConsigneeIMP.PK;
			shipment.ConsignorPK = ConsignorIMP.PK;
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_UniqueConsignRef = "S00001001";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			Factory.Save();
			ZDecimal amount = 1000;
			Job job = Helper.GetJobHeader(shipment.PK, JobShipmentSchema.Constants.TableName, shipment.JS_UniqueConsignRef, ConsigneeIMP, GlbStaff.CurrentUser, SalesRep);
			AccTransactionHeader apHeader = Helper.GetTransactionHeader(job.PK, amount, "SHIP1", LedgerTypes.AccountsPayable, ConsigneeIMP, TransactionTypes.Invoice);
			AccTransactionHeader arHeader = Helper.GetTransactionHeader(job.PK, amount, "SHIP1", LedgerTypes.AccountsReceivable, ConsigneeIMP, TransactionTypes.Invoice);
			Helper.GetTransactionLines(apHeader.PK, job.PK, TransactionLineTypes.Cost, -200m, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value);
			Helper.GetTransactionLines(apHeader.PK, job.PK, TransactionLineTypes.Accrual, 100m, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Registry.FreightChargeCode);
			Helper.GetTransactionLines(arHeader.PK, job.PK, TransactionLineTypes.Revenue, 1000m, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value);
			Helper.GetTransactionLines(arHeader.PK, job.PK, TransactionLineTypes.WIP, -50m, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Registry.FreightChargeCode);
			consol.Shipments.Add(shipment);
			Factory.Save();
			TestDateAttribute.Date = new ZDateTime(2007, 1, 1).ToDateTime();
			//export Shipment
			ZDateTime consol2ETD = ZDateTime.Now;
			ZDateTime consol2ETA = ZDateTime.Now.AddDays(1);
			consol = Helper.SetupConsol(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.LCL, Core.Constants.AgentType.Agent, "AUMEL", "HKHKG", consol2ETD, consol2ETA);
			amount = 2000m;
			ForwardingShipment shipmentA = Factory.New<ForwardingShipment>();
			shipmentA.ConsigneePK = ConsigneeEXP.PK;
			shipmentA.ConsignorPK = ConsignorEXP.PK;
			shipmentA.JS_RL_NKDestination = "HKHKG";
			shipmentA.JS_RL_NKOrigin = "AUMEL";
			shipmentA.JS_UniqueConsignRef = "S00001002";
			shipmentA.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentA.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			Factory.Save();
			job = Helper.GetJobHeader(shipmentA.PK, JobShipmentSchema.Constants.TableName, shipmentA.JS_UniqueConsignRef, ConsigneeEXP, GlbStaff.CurrentUser, SalesRep);
			apHeader = Helper.GetTransactionHeader(job.PK, amount, "SHIPA", "AP", ConsigneeEXP, "INV");
			arHeader = Helper.GetTransactionHeader(job.PK, amount, "SHIPA", "AR", ConsigneeEXP, "INV");
			Helper.GetTransactionLines(apHeader.PK, job.PK, TransactionLineTypes.Cost, -300m, GlbBranch.CurrentBranch, DummyDept, Env.Registry.FreightChargeCode);
			Helper.GetTransactionLines(arHeader.PK, job.PK, TransactionLineTypes.Revenue, 2000m, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Registry.FreightChargeCode);
			consol.Shipments.Add(shipmentA);
			ForwardingShipment shipmentB = Factory.New<ForwardingShipment>();
			shipmentB.ConsigneePK = ConsigneeEXP.PK;
			shipmentB.ConsignorPK = ConsignorWithNoSettings.PK;
			shipmentB.JS_RL_NKDestination = "HKHKG";
			shipmentB.JS_RL_NKOrigin = "AUMEL";
			shipmentB.JS_UniqueConsignRef = "S00001003";
			shipmentB.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentB.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			Factory.Save();
			job = Helper.GetJobHeader(shipmentB.PK, JobShipmentSchema.Constants.TableName, shipmentB.JS_UniqueConsignRef, ConsigneeEXP, GlbStaff.CurrentUser, SalesRep);
			arHeader = Helper.GetTransactionHeader(job.PK, amount, "SHIPB", "AR", ConsigneeEXP, "INV");
			apHeader = Helper.GetTransactionHeader(job.PK, amount, "SHIPB", "AP", ConsigneeEXP, "INV");
			Helper.GetTransactionLines(arHeader.PK, job.PK, TransactionLineTypes.Revenue, 2000m, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Registry.FreightChargeCode);
			consol.Shipments.Add(shipmentB);
			Factory.Save();
			//No Filter
			DataTable table = Utilities.GetDataTableFromQuery(Db.Connection, "select * from Client_MFI_JobProfitForConsol('" + GlbCompany.CurrentCompany.PK + "' , '', '', '01/01/1900', '01/01/2079', '" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "', '', '', '', '', '', '', Isnull(NULL, ''), Isnull(NULL, ''), '', '', '', '', '', '', '', '', '', '', null, null, '', '', '', '') ORDER BY JK_UniqueConsignRef");
			AssertEquals("Query should return 2 records", 2, table.Rows.Count);
			DataRow row = table.Rows[0];
			AssertConsolData(row, "C00001000", Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.Loose, "NZAKL", "AUBNE", consol1ETD, consol1ETA, 750m, 1000m, 50m, -200m, -100m);
			row = table.Rows[1];
			AssertConsolData(row, "C00001001", Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.LCL, "AUMEL", "HKHKG", consol2ETD, consol2ETA, 3700m, 4000m, 0m, -300m, 0m);
			//filtered by posted date
			table = Utilities.GetDataTableFromQuery(Db.Connection, "select * from Client_MFI_JobProfitForConsol('" + GlbCompany.CurrentCompany.PK + "' , '', '', '01/01/2007', '01/01/2007', '" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', null, null, '', '', '', '') ORDER BY JK_UniqueConsignRef");
			AssertEquals("Query should return 1 record", 1, table.Rows.Count);
			row = table.Rows[0];
			AssertEquals("Job returned is the consol 'C00001001'", "C00001001", row["JK_UniqueConsignRef"]);
			//filtered by TariffLevel
			table = Utilities.GetDataTableFromQuery(Db.Connection, "select * from Client_MFI_JobProfitForConsol('" + GlbCompany.CurrentCompany.PK + "' , '', '', '01/02/1900', '01/02/2079', '" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "', '1', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', null, null, '', '', '', '') ORDER BY JK_UniqueConsignRef");
			AssertEquals("Query should return 1 record", 1, table.Rows.Count);
			row = table.Rows[0];
			AssertConsolData(row, "C00001001", Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.LCL, "AUMEL", "HKHKG", consol2ETD, consol2ETA, 2000m, 2000m, 0m, 0m, 0m);
			//filtered by Department
			table = Utilities.GetDataTableFromQuery(Db.Connection, "select * from Client_MFI_JobProfitForConsol('" + GlbCompany.CurrentCompany.PK + "', '', '" + DummyDept.PK + "', '01/02/1900', '01/02/2079', '" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', null, null, '', '', '', '') ORDER BY JK_UniqueConsignRef");
			AssertEquals("Query should return 1 record", 1, table.Rows.Count);
			row = table.Rows[0];
			AssertConsolData(row, "C00001001", Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.LCL, "AUMEL", "HKHKG", consol2ETD, consol2ETA, -300m, 0m, 0m, -300m, 0m);
		}

		#endregion
		void AssertConsolData(DataRow row, ZString jobNum, ZString trans, ZString cont, ZString load, ZString disch, ZDateTime eTD, ZDateTime eTA, ZDecimal consolJobProfit, ZDecimal revenue, ZDecimal wIP, ZDecimal cost, ZDecimal accrual)
		{
			AssertEquals("Job Num", jobNum, row["JK_UniqueConsignRef"]);
			AssertEquals("Trans", trans, row["JK_TransportMode"]);
			AssertEquals("Cont", cont, row["JK_ConsolMode"]);
			AssertEquals("Load", load, row["JK_Load"]);
			AssertEquals("Disch", disch, row["JK_Discharge"]);
			AssertEquals("ConsolJobProfit", consolJobProfit, row["JH_Profit"]);
			AssertEquals("Revenue", revenue, row["REVAmount"]);
			AssertEquals("WIP", wIP, row["WIPAmount"]);
			AssertEquals("CSTAmount", cost, row["CSTAmount"]);
			AssertEquals("ACRAmount", accrual, row["ACRAmount"]);
		}

		#region Implementation
		#region SetupTariffLvl
		void SetupOrgTariffLvl(OrgHeader org, int lvl, ZString type)
		{
			OrgRateTariffLevel level = org.CompanyData.RateTariffLevels.AddNew();
			level.P7_TariffType = type;
			level.P7_TariffLevel = (byte)lvl;
			level.P7_GC = GlbCompany.CurrentCompany.PK;
		}

		#endregion
		#region SetupOrganisation
		void SetupOrganisation()
		{
			//Consignee with tariff lvl 2
			ConsigneeIMP = Factory.New<OrgHeader>();
			ConsigneeIMP.OH_Code = "CONSIGNI";
			ConsigneeIMP.OH_IsConsignee = true;
			ConsigneeIMP.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 2);
			ConsigneeEXP = Factory.New<OrgHeader>();
			ConsigneeEXP.OH_Code = "CONSIGNE";
			ConsigneeEXP.CompanyData.RateTariffLevels.RemoveAll();
			SetupOrgTariffLvl(ConsigneeEXP, 1, "DST");
			ConsignorIMP = Factory.New<OrgHeader>();
			ConsignorIMP.OH_Code = "CONSNORI";
			ConsignorIMP.OH_IsConsignor = true;
			ConsignorIMP.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 0);
			ConsignorEXP = Factory.New<OrgHeader>();
			ConsignorEXP.OH_Code = "CONSNORE";
			ConsignorEXP.CompanyData.RateTariffLevels.RemoveAll();
			SetupOrgTariffLvl(ConsigneeEXP, 1, "FRT");
			ConsignorWithNoSettings = Factory.New<OrgHeader>();
			ConsignorWithNoSettings.OH_Code = "CONSNOR2";
			ConsignorWithNoSettings.OH_IsConsignor = true;
			ConsignorWithNoSettings.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			GlbStaff.CurrentUser.GS_IsSalesRep = true;
			SalesRep = Factory.New<GlbStaff>();
			SalesRep.GS_Code = "SAL";
			SalesRep.GS_IsSalesRep = true;
			SalesRep.GS_LoginName = "SSS";
			Factory.Save();
		}

		#endregion
		OrgHeader ConsigneeIMP, ConsigneeEXP, ConsignorIMP, ConsignorEXP, ConsignorWithNoSettings;
		GlbStaff SalesRep;
		GlbDepartment DummyDept;
		TestHelper Helper;
		void SetupDummyDeptAndBranch()
		{
			DummyDept = Factory.New<GlbDepartment>();
			DummyDept.GE_Code = "DDP";
			DummyDept.GE_Import = true;
			DummyDept.GE_Sea = true;
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("AU");
			SetupOrganisation();
			SetupDummyDeptAndBranch();
			Helper = new TestHelper(Factory);
		}
		#endregion
	}
}
