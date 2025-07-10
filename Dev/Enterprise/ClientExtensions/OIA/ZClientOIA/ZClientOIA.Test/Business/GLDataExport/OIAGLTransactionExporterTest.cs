using System;
using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.OIA.Business.Testing
{
	public class OIAGLTransactionExporterTest : TestCaseWithFactory
	{
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestExportData()
		{
			TestHelper.SetValidRegistryAll();
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31, 23, 59, 00));
			ZGuid gLAccountFrom = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, "1000.00.00")).PK;
			ZGuid gLAccountTo = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, "9999.00.00")).PK;
			AccGLHeader aPSuspenseControl = Factory.NewWithValidTestData<AccGLHeader>();
			aPSuspenseControl.AG_AccountNum = "8888.00.00";
			aPSuspenseControl.AG_AccountType = "BSH";
			aPSuspenseControl.AG_Description = "TESTDESC";
			ZGuid gLAccount = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, SQLComparisonOperator.Equal, "4710.00.00")).PK;
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPSuspenseControl.PK.ToGuid());
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			AccChargeCode chargeCode = testObjectCreator.CC1;
			ForwardingShipment jobShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			jobShipment.DocsAndCartage.JP_CustomAttrib1 = "Text 1";
			jobShipment.DocsAndCartage.JP_CustomAttrib2 = "Text 2";
			jobShipment.JS_UniqueConsignRef = "S00001000";
			OrgHeader localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_Code = "ABCEFGSYD";
			OrgAddress orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = localClient.PK;
			orgAddress.OA_Code = "XYZ";
			Job job = new Job.Loader(Factory, jobShipment).TryCreate();
			job.JH_OA_LocalChargesAddr = orgAddress.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			OrgDebtorGroup orgDebtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			orgDebtorGroup.OJ_Code = "ABC";
			OrgCompanyData localClientData = localClient.CompanyData;
			localClientData.OB_IsDebtor = true;
			localClientData.OB_OJ_ARDebtorGroup = orgDebtorGroup.PK;
			Invoice invoice = (Invoice)testObjectCreator.CreateInvoice(typeof(APInvoice), GlbCompany.CurrentCompany.LocalCurrency, 1m);
			invoice.AH_PostDate = new ZDateTime(2006, 3, 10);
			invoice.AH_PostToGL = "Y";
			InvoiceLine invoiceLine = (InvoiceLine)testObjectCreator.CreateInvoiceLine(invoice, GlbCompany.CurrentCompany.LocalCurrency, 1m, -333m, GlbBranch.CurrentBranch.PK, false);
			invoiceLine.AL_JH = job.PK;
			invoiceLine.AL_AC = chargeCode.PK;
			invoiceLine.AL_PostDate = new ZDateTime(2006, 3, 10);
			invoiceLine.AL_RevRecognitionType = "IMM";
			invoiceLine.AL_ReverseDate = invoiceLine.AL_PostDate;
			AssertNotNull("reverse should be set when post is set due to new revenue recognition", invoiceLine.AL_ReverseDate);
			JobCharge jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = job.PK;
			jobCharge.JR_AC = chargeCode.PK;
			jobCharge.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge.JR_AL_APLine = invoiceLine.PK;
			jobCharge.SetAmountsFromLinkedLinesForTests();
			Factory.Save();
			GenExportBatchSequence exportBatchSequence = Factory.NewWithValidTestData<GenExportBatchSequence>();
			exportBatchSequence.XB_BatchNumber = 1001;
			exportBatchSequence.XB_ParentID = new ZGuid();
			exportBatchSequence.XB_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			OIAGLTransactionBusinessObject bizObj = new OIAGLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = SystemDataRegistry.Instance.GLTransactionsCSVExportDirectory.Value;
			bizObj.CreateAndExportBatch = true;
			bizObj.BatchNumber = 1001;
			string exportFileName = Path.Combine(SystemDataRegistry.Instance.GLTransactionsCSVExportDirectory.Value, GlbCompany.CurrentCompany.GC_Code + "_20060329033942_1001.csv");
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				OIAGLTransactionExporter exporter = new OIAGLTransactionExporter(bizObj, new NotificationBuffer());
				exporter.ExportData();
				AssertEquals("Expected export file should exist", true, File.Exists(exportFileName));
				StringBuilder expected = new StringBuilder();
				var branchCode = GlbBranch.CurrentBranch.GB_Code;
				var departmentCode = GlbDepartment.CurrentDepartment.GE_Code;
				expected.Append("\"TransactionType\",\"InvoiceDate\",\"PostDate\",\"DueDate\",\"Branch\",\"Department\",\"Ledger\",\"TransactionNum\",\"TransactionDesc\",\"GLAccount\",\"GLAccountDesc\",\"Job\",\"Account\",\"ChargeCode\",\"Period\",\"ReversePeriod\",\"Amount\",\"GSTAmount\",\"Debit\",\"Credit\",\"Balance\",\"IsLine\",\"TRPK\",\"OpeningPeriodDate\",\"ClosingPeriodDate\",\"OpeningBalance\",\"ClosingBalance\",\"IsControlTotal\",\"LocalClient\",\"ARGroup\",\"Text1\",\"Text2\"\r\n");
				expected.Append("\"\",\"\",\"\",\"\",\"\",\"\",\"AP \",\"\",\"AP Control Account\",\"8210.00.00\",\"TRADE CREDITORS CONTROL\",\"\",\"\",\"\",\"200603\",\"\",\"333.0000\",\"0.0000\",\"333.0000\",\"\",\"0.0000\",\"False\",\"\",\"\",\"\",\"\",\"\",\"True\",\"\",\"\",\"\",\"\"\r\n");
				expected.Append("\"INV\",\"29/03/2006 3:39:00 PM\",\"10/03/2006 12:00:00 AM\",\"29/03/2006 3:39:00 PM\",\"" + branchCode + "\",\"" + departmentCode + "\",\"AP \",\"" + invoice.AH_TransactionNum + "\",\"Test Invoice\",\"6310.00.00\",\"INPUT TAX RECEIVABLE\",\"S00001000\",\"\",\"ZZCC1\",\"200603\",\"\",\"0.0000\",\"0.0000\",\"\",\"\",\"0.0000\",\"False\",\"" + invoiceLine.PK.ToString() + "\",\"\",\"\",\"\",\"\",\"False\",\"\",\"\",\"\",\"\"\r\n");
				expected.Append("\"INV\",\"29/03/2006 3:39:00 PM\",\"10/03/2006 12:00:00 AM\",\"29/03/2006 3:39:00 PM\",\"" + branchCode + "\",\"" + departmentCode + "\",\"AP \",\"" + invoice.AH_TransactionNum + "\",\"Test Invoice\",\"@AccountNum\",\"@AccountDescription\",\"S00001000\",\"\",\"ZZCC1\",\"200603\",\"\",\"-333.0000\",\"0.0000\",\"\",\"333.0000\",\"0.0000\",\"False\",\"\",\"\",\"\",\"\",\"\",\"False\",\"ABCEFGSYD\",\"ABC\",\"Text 1\",\"Text 2\"\r\n");
				expected.Replace("@AccountDescription", aPSuspenseControl.AG_Description);
				expected.Replace("@AccountNum", aPSuspenseControl.AG_AccountNum);
				expected.Replace("@PK", invoiceLine.PK.ToString());
				string expectedExportData = expected.ToString();
				string actualExportData = File.ReadAllText(exportFileName);
				AssertMultilineASCIIEquals("Exporter is not exporting what is expected", expectedExportData, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}
		}

		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestExportDataWithColumnsContainingSingleDoubleQuote()
		{
			TestHelper.SetValidRegistryAll();
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31, 23, 59, 00));
			ZGuid gLAccountFrom = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, "1000.00.00")).PK;
			ZGuid gLAccountTo = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, "9999.00.00")).PK;
			AccGLHeader aPSuspenseControl = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountType, "BSH"));
			aPSuspenseControl.AG_Description = "TESTDESC" + "40\" X 35\"";
			Factory.Save();
			ZGuid gLAccount = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, SQLComparisonOperator.Equal, "4710.00.00")).PK;
			GlbBranch branch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			GlbDepartment department = Factory.Load<GlbDepartment>(GlbDepartment.CurrentDepartment.PK);
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPSuspenseControl.PK.ToGuid());
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			AccChargeCode chargeCode = testObjectCreator.CC1;
			chargeCode.AC_Code = "ZZ\"CC1";
			chargeCode.AC_AT_GSTRate = ZGuid.Empty;
			ForwardingShipment jobShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			jobShipment.DocsAndCartage.JP_CustomAttrib1 = "Text\"1";
			jobShipment.DocsAndCartage.JP_CustomAttrib2 = "Text\"2";
			jobShipment.JS_UniqueConsignRef = "S000\"1000";
			OrgHeader localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_Code = "ABC\"FGSYD";
			OrgAddress orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = localClient.PK;
			orgAddress.OA_Code = "XYZ";
			Job job = new Job.Loader(Factory, jobShipment).TryCreate();
			job.JH_OA_LocalChargesAddr = orgAddress.PK;
			job.JH_GB = branch.PK;
			job.JH_GE = department.PK;
			OrgDebtorGroup orgDebtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			orgDebtorGroup.OJ_Code = "A\"C";
			OrgCompanyData localClientData = localClient.CompanyData;
			localClientData.OB_IsDebtor = true;
			localClientData.OB_OJ_ARDebtorGroup = orgDebtorGroup.PK;
			OrgHeader orgHeader = testObjectCreator.AALSHI;
			orgHeader.OH_Code = "A\"LSHI";
			Invoice invoice = (Invoice)testObjectCreator.CreateInvoice(typeof(APInvoice), GlbCompany.CurrentCompany.LocalCurrency, 1m);
			invoice.AH_GB = branch.PK;
			invoice.AH_GE = department.PK;
			invoice.AH_OH = orgHeader.PK;
			invoice.AH_Desc = "Test 40\" X 35\"";
			invoice.AH_PostDate = new ZDateTime(2006, 3, 10);
			invoice.AH_TransactionNum = "0001 40\" X 35\"";
			invoice.AH_PostToGL = "Y";
			InvoiceLine invoiceLine = (InvoiceLine)testObjectCreator.CreateInvoiceLine(invoice, GlbCompany.CurrentCompany.LocalCurrency, 1m, -333m, branch.PK, false);
			invoiceLine.AL_JH = job.PK;
			invoiceLine.AL_AC = chargeCode.PK;
			invoiceLine.AL_GE = department.PK;
			invoiceLine.AL_PostDate = new ZDateTime(2006, 3, 10);
			invoiceLine.AL_RevRecognitionType = "IMM";
			invoiceLine.AL_ReverseDate = invoiceLine.AL_PostDate;
			AssertNotNull("reverse should be set when post is set due to new revenue recognition", invoiceLine.AL_ReverseDate);
			JobCharge jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = job.PK;
			jobCharge.JR_AC = chargeCode.PK;
			jobCharge.JR_GB = branch.PK;
			jobCharge.JR_GE = department.PK;
			jobCharge.JR_AL_APLine = invoiceLine.PK;
			jobCharge.SetAmountsFromLinkedLinesForTests();
			Factory.Save();
			GenExportBatchSequence exportBatchSequence = Factory.NewWithValidTestData<GenExportBatchSequence>();
			exportBatchSequence.XB_BatchNumber = 1001;
			exportBatchSequence.XB_ParentID = new ZGuid();
			exportBatchSequence.XB_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			OIAGLTransactionBusinessObject bizObj = new OIAGLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = Env.TempPath;
			bizObj.CreateAndExportBatch = true;
			bizObj.BatchNumber = 1001;
			bizObj.DepartmentPK = department.PK;
			bizObj.BranchPK = branch.PK;
			string exportFileName = Path.Combine(Env.TempPath, GlbCompany.CurrentCompany.GC_Code + "_20060329033942_1001.csv");
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				OIAGLTransactionExporter exporter = new OIAGLTransactionExporter(bizObj, new NotificationBuffer());
				Assert("Exporter should have exported data", exporter.ExportData());
				AssertEquals("Expected export file should exist", true, File.Exists(exportFileName));
				StringBuilder expected = new StringBuilder();
				var branchCode = GlbBranch.CurrentBranch.GB_Code;
				var departmentCode = GlbDepartment.CurrentDepartment.GE_Code;
				expected.Append("\"TransactionType\",\"InvoiceDate\",\"PostDate\",\"DueDate\",\"Branch\",\"Department\",\"Ledger\",\"TransactionNum\",\"TransactionDesc\",\"GLAccount\",\"GLAccountDesc\",\"Job\",\"Account\",\"ChargeCode\",\"Period\",\"ReversePeriod\",\"Amount\",\"GSTAmount\",\"Debit\",\"Credit\",\"Balance\",\"IsLine\",\"TRPK\",\"OpeningPeriodDate\",\"ClosingPeriodDate\",\"OpeningBalance\",\"ClosingBalance\",\"IsControlTotal\",\"LocalClient\",\"ARGroup\",\"Text1\",\"Text2\"\r\n");
				expected.Append("\"\",\"\",\"\",\"\",\"\",\"\",\"AP \",\"\",\"AP Control Account\",\"8210.00.00\",\"TRADE CREDITORS CONTROL\",\"\",\"\",\"\",\"200603\",\"\",\"333.0000\",\"0.0000\",\"333.0000\",\"\",\"0.0000\",\"False\",\"\",\"\",\"\",\"\",\"\",\"True\",\"\",\"\",\"\",\"\"\r\n");
				expected.Append("\"INV\",\"29/03/2006 3:39:00 PM\",\"10/03/2006 12:00:00 AM\",\"29/03/2006 3:39:00 PM\",\"" + branchCode + "\",\"" + departmentCode + "\",\"AP \",\"0001 40\"\" X 35\"\"\",\"Test 40\"\" X 35\"\"\",\"6310.00.00\",\"INPUT TAX RECEIVABLE\",\"S000\"\"1000\",\"A\"\"LSHI\",\"ZZ\"\"CC1\",\"200603\",\"\",\"0.0000\",\"0.0000\",\"\",\"\",\"0.0000\",\"False\",\"" + invoiceLine.PK.ToString() + "\",\"\",\"\",\"\",\"\",\"False\",\"\",\"\",\"\",\"\"\r\n");
				expected.Append("\"INV\",\"29/03/2006 3:39:00 PM\",\"10/03/2006 12:00:00 AM\",\"29/03/2006 3:39:00 PM\",\"" + branchCode + "\",\"" + departmentCode + "\",\"AP \",\"0001 40\"\" X 35\"\"\",\"Test 40\"\" X 35\"\"\",\"@AccountNum\",\"@AccountDescription\",\"S000\"\"1000\",\"A\"\"LSHI\",\"ZZ\"\"CC1\",\"200603\",\"\",\"-333.0000\",\"0.0000\",\"\",\"333.0000\",\"0.0000\",\"False\",\"\",\"\",\"\",\"\",\"\",\"False\",\"ABC\"\"FGSYD\",\"A\"\"C\",\"Text\"\"1\",\"Text\"\"2\"\r\n");
				expected.Replace("@AccountDescription", aPSuspenseControl.AG_Description.Replace("\"", "\"\""));
				expected.Replace("@AccountNum", aPSuspenseControl.AG_AccountNum);
				expected.Replace("@PK", invoiceLine.PK.ToString());
				string expectedExportData = expected.ToString();
				string actualExportData = File.ReadAllText(exportFileName);
				AssertContainsExactLinesInAnyOrder("Exporter is not exporting what is expected", expectedExportData, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}
		}

		OIATestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new OIATestHelper(Factory));
			}
		}

		OIATestHelper testHelper;
	}
}
