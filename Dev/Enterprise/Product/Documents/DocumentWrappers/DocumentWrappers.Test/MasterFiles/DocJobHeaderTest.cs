using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocJobHeader))]
	public class DocJobHeaderTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocJobHeader.New(Header, Factory) };
		}

		#region Test Constructors

		public void TestNewWithDebtorProperty()
		{
			DocJobHeader headerWrapper = DocJobHeader.New(null, Factory);
			AssertNull("DocJobHeader", headerWrapper);

			var header = Factory.NewJobForTesting<JobHeader>();
			var debtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
			headerWrapper = DocJobHeader.New(header, debtor, Factory);
			AssertNotNull("JobHeader", headerWrapper);
			AssertEquals("JobHeader is of type DocJobHeader", typeof(DocJobHeader), headerWrapper.GetType());
			AssertEquals("Debtor", debtor.PK, headerWrapper.Debtor.PK);
		}

		#endregion

		#region Collections

		public void TestJobCharges()
		{
			var chargeCode1 = CreateChargeCode("TESTCH1");
			var chargeCode2 = CreateChargeCode("TESTCH2");
			var chargeCode3 = CreateChargeCode("TESTCH3");
			var chargeCode4 = CreateChargeCode("TESTCH4");
			var chargeCode5 = CreateChargeCode("TESTCH5");

			var charge1 = CreateLineCharge(Header, Header.LocalChargesPK, 10.000M, chargeCode1.PK);
			var charge2 = CreateLineCharge(Header, Header.LocalChargesPK, 30.000M, chargeCode2.PK);
			var charge3 = CreateLineCharge(Header, Header.AgentCollectPK, 15.000M, chargeCode3.PK);
			var charge4 = CreateLineCharge(Header, Header.AgentCollectPK, 40.000M, chargeCode4.PK);
			var charge5 = CreateLineCharge(Header, TestObjectCreator.Debtor.PK, 20.000M, chargeCode5.PK);

			Factory.ResetDatabaseLoadCount();
			AssertDbHits(new Dictionary<string, int>() { { JobChargeSchema.Constants.TableName, 0 } }, Factory);
			AssertEquals("JobCharges elements", 5, HeaderWrapper.JobCharges.Count);
			AssertEquals("JobCharges elements", 5, HeaderWrapper.JobCharges.Count);
			AssertDbHits(new Dictionary<string, int>() { { JobChargeSchema.Constants.TableName, 1 } }, Factory);
		}

		public void TestDocJobChargesSyncWithJobCharges()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			var jobHeader = TestObjectCreator.CreateJob(shipment);
			var charge = TestObjectCreator.CreateCharge(jobHeader, TestObjectCreator.CC1, debtor: TestObjectCreator.Debtor);
			Factory.Save();

			Func<DocJobHeader> getNewHeaderWrapper = () => DocJobHeader.New(jobHeader, Factory);
			AssertEquals(1, getNewHeaderWrapper().JobCharges.Count);

			charge.Delete();
			Factory.Save();
			AssertEquals(0, getNewHeaderWrapper().JobCharges.Count);
		}

		public void TestDocJobChargesSyncWithJobCharges_DataRefreshCase()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			var jobHeader = TestObjectCreator.CreateJob(shipment);
			var charge = TestObjectCreator.CreateCharge(jobHeader, TestObjectCreator.CC1, debtor: TestObjectCreator.Debtor);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			Func<DocJobHeader> getNewHeaderWrapper = () => DocJobHeader.New(newFactory.Load<JobHeader>(jobHeader.PK), newFactory);
			AssertEquals(1, getNewHeaderWrapper().JobCharges.Count);

			charge.Delete();
			Factory.Save();
			AssertEquals(0, getNewHeaderWrapper().JobCharges.Count);
		}

		public void TestJobChargesForLocalClient()
		{
			var orgHeader1 = TestObjectCreator.LocalClient;
			var orgHeader2 = TestObjectCreator.LocalClient2;

			var chargeCode1 = CreateChargeCode("TESTCH1");
			var chargeCode2 = CreateChargeCode("TESTCH2");
			var chargeCode3 = CreateChargeCode("TESTCH3");

			Header.LocalChargesPK = orgHeader1.PK;
			var charge1 = CreateLineCharge(Header, Header.LocalChargesPK, 10.000M, chargeCode1.PK);
			var charge2 = CreateLineCharge(Header, Header.LocalChargesPK, 30.000M, chargeCode2.PK);
			var charge3 = CreateLineCharge(Header, orgHeader2.PK, 20.000M, chargeCode3.PK);

			Factory.ResetDatabaseLoadCount();
			AssertDbHits(new Dictionary<string, int>() { { JobChargeSchema.Constants.TableName, 0 } }, Factory);
			AssertEquals("JobChargesForLocalClient elements", 2, HeaderWrapper.JobChargesForLocalClient.Count);
			AssertEquals("JobChargesForLocalClient elements", 2, HeaderWrapper.JobChargesForLocalClient.Count);
			AssertDbHits(new Dictionary<string, int>() { { JobChargeSchema.Constants.TableName, 1 } }, Factory);
		}

		public void TestJobChargesForLocalClientWhenLocalClientIsEmpty()
		{
			var orgHeader1 = TestObjectCreator.LocalClient;
			var orgHeader2 = TestObjectCreator.LocalClient2;

			var chargeCode1 = CreateChargeCode("TESTCH1");
			var chargeCode2 = CreateChargeCode("TESTCH2");
			var chargeCode3 = CreateChargeCode("TESTCH3");

			Header.LocalChargesPK = ZGuid.Empty;
			var charge1 = CreateLineCharge(Header, Header.LocalChargesPK, 10.000M, chargeCode1.PK);
			var charge2 = CreateLineCharge(Header, Header.LocalChargesPK, 30.000M, chargeCode2.PK);
			var charge3 = CreateLineCharge(Header, orgHeader2.PK, 20.000M, chargeCode3.PK);

			Factory.ResetDatabaseLoadCount();
			AssertDbHits(new Dictionary<string, int>() { { JobChargeSchema.Constants.TableName, 0 } }, Factory);
			AssertEquals("JobChargesForLocalClient elements", 0, HeaderWrapper.JobChargesForLocalClient.Count);
			AssertEquals("JobChargesForLocalClient elements", 0, HeaderWrapper.JobChargesForLocalClient.Count);
			AssertDbHits(new Dictionary<string, int>() { { JobChargeSchema.Constants.TableName, 0 } }, Factory);
		}

		public void TestJobChargesForAgentCollect()
		{
			var orgHeader1 = TestObjectCreator.Agent;
			var orgHeader2 = TestObjectCreator.Agent2;

			var chargeCode1 = CreateChargeCode("TESTCH1");
			var chargeCode2 = CreateChargeCode("TESTCH2");
			var chargeCode3 = CreateChargeCode("TESTCH3");

			Header.AgentCollectPK = orgHeader1.PK;
			var charge1 = CreateLineCharge(Header, Header.AgentCollectPK, 10.000M, chargeCode1.PK);
			var charge2 = CreateLineCharge(Header, Header.AgentCollectPK, 30.000M, chargeCode2.PK);
			var charge3 = CreateLineCharge(Header, orgHeader2.PK, 20.000M, chargeCode3.PK);

			Factory.ResetDatabaseLoadCount();
			AssertDbHits(new Dictionary<string, int>() { { JobChargeSchema.Constants.TableName, 0 } }, Factory);
			AssertEquals("JobChargesForAgentCollect elements", 2, HeaderWrapper.JobChargesForAgentCollect.Count);
			AssertEquals("JobChargesForAgentCollect elements", 2, HeaderWrapper.JobChargesForAgentCollect.Count);
			AssertDbHits(new Dictionary<string, int>() { { JobChargeSchema.Constants.TableName, 1 } }, Factory);
		}

		public void TestJobChargesForAgentCollectWhenAgentCollectIsEmpty()
		{
			var orgHeader1 = TestObjectCreator.Agent;
			var orgHeader2 = TestObjectCreator.Agent2;

			var chargeCode1 = CreateChargeCode("TESTCH1");
			var chargeCode2 = CreateChargeCode("TESTCH2");
			var chargeCode3 = CreateChargeCode("TESTCH3");

			Header.AgentCollectPK = ZGuid.Empty;
			var charge1 = CreateLineCharge(Header, Header.AgentCollectPK, 10.000M, chargeCode1.PK);
			var charge2 = CreateLineCharge(Header, Header.AgentCollectPK, 30.000M, chargeCode2.PK);
			var charge3 = CreateLineCharge(Header, orgHeader2.PK, 20.000M, chargeCode3.PK);

			Factory.ResetDatabaseLoadCount();
			AssertDbHits(new Dictionary<string, int>() { { JobChargeSchema.Constants.TableName, 0 } }, Factory);
			AssertEquals("JobChargesForAgentCollect elements", 0, HeaderWrapper.JobChargesForAgentCollect.Count);
			AssertEquals("JobChargesForAgentCollect elements", 0, HeaderWrapper.JobChargesForAgentCollect.Count);
			AssertDbHits(new Dictionary<string, int>() { { JobChargeSchema.Constants.TableName, 0 } }, Factory);
		}

		public void TestJobChargesForDebtorOrLocalClient()
		{
			Header.LocalChargesPK = TestObjectCreator.LocalClient.PK;

			var chargeCode1 = CreateChargeCode("TESTCH1");
			var chargeCode2 = CreateChargeCode("TESTCH2");
			var chargeCode3 = CreateChargeCode("TESTCH3");

			var charge1 = CreateLineCharge(Header, Header.LocalChargesPK, 10.000M, chargeCode1.PK);
			var charge2 = CreateLineCharge(Header, Header.LocalChargesPK, 30.000M, chargeCode2.PK);
			var charge3 = CreateLineCharge(Header, TestObjectCreator.LocalClient2.PK, 20.000M, chargeCode3.PK);

			Factory.ResetDatabaseLoadCount();
			AssertDbHits(new Dictionary<string, int>() { { JobChargeSchema.Constants.TableName, 0 } }, Factory);
			AssertEquals("JobChargesForDebtorOrLocalClient elements", 2, HeaderWrapper.JobChargesForDebtorOrLocalClient.Count);
			AssertEquals("JobChargesForDebtorOrLocalClient elements", 2, HeaderWrapper.JobChargesForDebtorOrLocalClient.Count);
			AssertDbHits(new Dictionary<string, int>() { { JobChargeSchema.Constants.TableName, 1 } }, Factory);
		}

		public void TestJobChargesForDebtorOrLocalClientWhenDebtorIsNominated()
		{
			Header.LocalChargesPK = TestObjectCreator.LocalClient.PK;

			var chargeCode1 = CreateChargeCode("TESTCH1");
			var chargeCode2 = CreateChargeCode("TESTCH2");
			var chargeCode3 = CreateChargeCode("TESTCH3");

			var charge1 = CreateLineCharge(Header, Header.LocalChargesPK, 10.000M, chargeCode1.PK);
			var charge2 = CreateLineCharge(Header, Header.LocalChargesPK, 30.000M, chargeCode2.PK);
			var charge3 = CreateLineCharge(Header, TestObjectCreator.LocalClient2.PK, 20.000M, chargeCode3.PK);

			HeaderWrapper = DocJobHeader.New(Header, TestObjectCreator.LocalClient2, Factory);
			Factory.ResetDatabaseLoadCount();
			AssertDbHits(new Dictionary<string, int>() { { JobChargeSchema.Constants.TableName, 0 } }, Factory);
			AssertEquals("JobChargesForDebtorOrLocalClient elements", 1, HeaderWrapper.JobChargesForDebtorOrLocalClient.Count);
			AssertEquals("JobChargesForDebtorOrLocalClient elements", 1, HeaderWrapper.JobChargesForDebtorOrLocalClient.Count);
			AssertDbHits(new Dictionary<string, int>() { { JobChargeSchema.Constants.TableName, 1 } }, Factory);
		}

		public void TestJobChargesForDebtorOrLocalClientWhenLocalClientIsEmpty()
		{
			Header.LocalChargesPK = ZGuid.Empty;

			var chargeCode1 = CreateChargeCode("TESTCH1");
			var chargeCode2 = CreateChargeCode("TESTCH2");
			var chargeCode3 = CreateChargeCode("TESTCH3");

			var charge1 = CreateLineCharge(Header, Header.LocalChargesPK, 10.000M, chargeCode1.PK);
			var charge2 = CreateLineCharge(Header, Header.LocalChargesPK, 30.000M, chargeCode2.PK);
			var charge3 = CreateLineCharge(Header, TestObjectCreator.LocalClient2.PK, 20.000M, chargeCode3.PK);

			Factory.ResetDatabaseLoadCount();
			AssertDbHits(new Dictionary<string, int>() { { JobChargeSchema.Constants.TableName, 0 } }, Factory);
			AssertEquals("JobChargesForDebtorOrLocalClient elements", 0, HeaderWrapper.JobChargesForDebtorOrLocalClient.Count);
			AssertEquals("JobChargesForDebtorOrLocalClient elements", 0, HeaderWrapper.JobChargesForDebtorOrLocalClient.Count);
			AssertDbHits(new Dictionary<string, int>() { { JobChargeSchema.Constants.TableName, 0 } }, Factory);
		}

		public void TestExchangeRates()
		{
			ExchangeRate rate1 = Factory.New<ExchangeRate>();
			rate1.JF_JH = Header.PK;

			Factory.ResetDatabaseLoadCount();
			AssertDbHits(new Dictionary<string, int>() { { JobExRateSchema.Constants.TableName, 0 } }, Factory);
			AssertEquals("ExchangeRates has 1 element", 1, HeaderWrapper.ExchangeRates.Count);
			AssertEquals("ExchangeRates has correct element", rate1.PK, ((BusinessObject)HeaderWrapper.ExchangeRates[0].WrappedObject).PK);
			AssertDbHits(new Dictionary<string, int>() { { JobExRateSchema.Constants.TableName, 1 } }, Factory);
		}

		#endregion

		public void TestJobHeaderExportCustomsHandlingNotes()
		{
			OrgHeader localClient = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Header.LocalChargesPK = localClient.PK;

			Assert(HeaderWrapper.JobHeaderExportCustomsHandlingNotes.IsEmpty);
			localClient.Notes.AddNew(false, PredefinedNoteTypes.Instance.ExportCustomsHandlingNotes.Description, "Export Customs Handling Notes");
			AssertEquals("Export Customs Handling Notes", HeaderWrapper.JobHeaderExportCustomsHandlingNotes);
		}

		public void TestJobHeaderImportCustomsHandlingNotes()
		{
			OrgHeader localClient = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Header.LocalChargesPK = localClient.PK;

			Assert(HeaderWrapper.JobHeaderImportCustomsHandlingNotes.IsEmpty);
			localClient.Notes.AddNew(false, PredefinedNoteTypes.Instance.ImportCustomsHandlingNotes.Description, "Import Customs Handling Notes");
			AssertEquals("Import Customs Handling Notes", HeaderWrapper.JobHeaderImportCustomsHandlingNotes);
		}

		public void TestJobHeaderInvoicingPreferences()
		{
			OrgHeader localClient = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Header.LocalChargesPK = localClient.PK;

			Assert(HeaderWrapper.JobHeaderInvoicingPreferences.IsEmpty);
			localClient.Notes.AddNew(false, PredefinedNoteTypes.Instance.InvoicingPreferences.Description, "Job Header Invoicing Preferences");
			AssertEquals("Job Header Invoicing Preferences", HeaderWrapper.JobHeaderInvoicingPreferences);
		}

		public void TestFormattedTotalLocalSellAmount()
		{
			RefCurrency currency = Factory.New<RefCurrency>();
			currency.RX_Code = "ZZZ";
			currency.RX_Symbol = "#";

			Header.JH_GB = GlbBranch.CurrentBranch.PK;
			Header.Branch.Country.RN_RX_NKLocalCurrency = currency.RX_Code;
			Header.LocalChargesPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			AccChargeCode testCode1 = CreateChargeCode("TESTCH1");
			AccChargeCode testCode2 = CreateChargeCode("TESTCH2");
			AccChargeCode testCode3 = CreateChargeCode("TESTCH3");

			JobCharge lineCharge1 = CreateLineChargeWithTax(Header, Header.LocalChargesPK, 10.000M, testCode1.PK, 1);
			JobCharge lineCharge2 = CreateLineChargeWithTax(Header, Header.LocalChargesPK, 30.000M, testCode2.PK, 3);

			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, Header.LocalChargesPK));
			JobCharge lineCharge3 = CreateLineChargeWithTax(Header, orgHeader.PK, 20.000M, testCode3.PK, 2);

			AssertEquals("#40.00 ZZZ", HeaderWrapper.FormattedTotalLocalSellAmount);
		}

		public void TestFormattedTotalLocalSellAmountIncTax()
		{
			RefCurrency currency = Factory.New<RefCurrency>();
			currency.RX_Code = "ZZZ";
			currency.RX_Symbol = "#";

			Header.JH_GB = GlbBranch.CurrentBranch.PK;
			Header.Branch.Country.RN_RX_NKLocalCurrency = currency.RX_Code;
			Header.LocalChargesPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			AccChargeCode testCode1 = CreateChargeCode("TESTCH1");
			AccChargeCode testCode2 = CreateChargeCode("TESTCH2");
			AccChargeCode testCode3 = CreateChargeCode("TESTCH3");

			JobCharge lineCharge1 = CreateLineChargeWithTax(Header, Header.LocalChargesPK, 10.000M, testCode1.PK, 1);
			JobCharge lineCharge2 = CreateLineChargeWithTax(Header, Header.LocalChargesPK, 30.000M, testCode2.PK, 3);

			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, Header.LocalChargesPK));
			JobCharge lineCharge3 = CreateLineChargeWithTax(Header, orgHeader.PK, 20.000M, testCode3.PK, 2);

			AssertEquals("#44.00 ZZZ", HeaderWrapper.FormattedTotalLocalSellAmountIncTax);
		}

		public void TestFormattedTotalTaxAmount()
		{
			RefCurrency currency = Factory.New<RefCurrency>();
			currency.RX_Code = "ZZZ";
			currency.RX_Symbol = "#";

			Header.JH_GB = GlbBranch.CurrentBranch.PK;
			Header.Branch.Country.RN_RX_NKLocalCurrency = currency.RX_Code;
			Header.LocalChargesPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			AccChargeCode testCode1 = CreateChargeCode("TESTCH1");
			AccChargeCode testCode2 = CreateChargeCode("TESTCH2");
			AccChargeCode testCode3 = CreateChargeCode("TESTCH3");

			JobCharge lineCharge1 = CreateLineChargeWithTax(Header, Header.LocalChargesPK, 10.000M, testCode1.PK, 1);
			JobCharge lineCharge2 = CreateLineChargeWithTax(Header, Header.LocalChargesPK, 30.000M, testCode2.PK, 3);

			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, Header.LocalChargesPK));
			JobCharge lineCharge3 = CreateLineChargeWithTax(Header, orgHeader.PK, 20.000M, testCode3.PK, 2);

			AssertEquals("#4.00 ZZZ", HeaderWrapper.FormattedTotalTaxAmount);
		}

		public void TestActualJCL()
		{
			ZDateTime actualJCL = new ZDateTime(2004, 01, 01);
			Header.JH_A_JCL = actualJCL;
			AssertEquals("ActualJCL", actualJCL, HeaderWrapper.ActualJCL);
		}

		public void TestActualJOP()
		{
			ZDateTime actualJOP = new ZDateTime(2004, 01, 01);
			Header.JH_A_JOP = actualJOP;
			AssertEquals("ActualJOP", actualJOP, HeaderWrapper.ActualJOP);
		}

		public void TestActualJOPString()
		{
			Header.JH_A_JOP = new ZDateTime(2004, 01, 01);
			AssertEquals("ActualJOPString", "01-Jan-04", HeaderWrapper.ActualJOPString);
		}

		public void TestActualJCLString()
		{
			Header.JH_A_JCL = new ZDateTime(2004, 01, 01);
			AssertEquals("ActualJCLString", "01-Jan-04", HeaderWrapper.ActualJCLString);
		}

		public void TestAgentChargesCFX()
		{
			ZDecimal agentChargesCFX = new ZDecimal(1);
			Header.JH_AgentChargesCFX = agentChargesCFX;
			AssertEquals("AgentChargesCFX", agentChargesCFX, HeaderWrapper.AgentChargesCFX);
		}

		public void TestLocalChargesCFX()
		{
			ZDecimal localChargesCFX = new ZDecimal(2);
			Header.JH_LocalChargesCFX = localChargesCFX;
			AssertEquals("LocalChargesCFX", localChargesCFX, HeaderWrapper.LocalChargesCFX);
		}

		public void TestForeignKey()
		{
			ZGuid foreignKey = ZGuid.NewZGuid();
			Header.JH_ParentID = foreignKey;
			AssertEquals("ForeignKey", foreignKey, HeaderWrapper.ParentID);
		}

		public void TestForeignTable()
		{
			ZString parentTableCode = new ZString("XX");
			Header.JH_ParentTableCode = parentTableCode;
			AssertEquals("ParentTableCode", parentTableCode, HeaderWrapper.ParentTableCode);
		}

		public void TestHoldReason()
		{
			ZString holdReason = new ZString("HoldReason");
			Header.JH_HoldReason = holdReason;
			AssertEquals("HoldReason", holdReason, HeaderWrapper.HoldReason);
		}

		public void TestJobNumber()
		{
			ZString jobNumber = new ZString("JobNumber");
			Header.JH_JobNum = jobNumber;
			AssertEquals("JobNumber", jobNumber, HeaderWrapper.JobNumber);
		}

		public void TestToString()
		{
			ZString jobNumber = new ZString("JobNumber");
			Header.JH_JobNum = jobNumber;
			AssertEquals("ToString()", jobNumber, HeaderWrapper.ToString());
		}

		public void TestStatus()
		{
			ZString status = new ZString("SSS");
			Header.JH_Status = status;
			AssertEquals("Status", status, HeaderWrapper.Status);
		}

		public void TestSingleAgentsInvoicePerConsol()
		{
			Header.JH_SingleAgentsInvoicePerConsol = ZBool.False;
			Assert("!SingleAgentsInvoicePerConsol", !HeaderWrapper.SingleAgentsInvoicePerConsol);

			Header.JH_SingleAgentsInvoicePerConsol = ZBool.True;
			Assert("SingleAgentsInvoicePerConsol", HeaderWrapper.SingleAgentsInvoicePerConsol);
		}

		public void TestInternalWorkNotes()
		{
			OrgHeader localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.Addresses.AddNew();
			HeaderWrapper.JobHeader.JH_OA_LocalChargesAddr = localClient.Addresses[0].PK;
			StmNote otherNotes = AddNotes(HeaderWrapper.JobHeader.LocalCharges, PredefinedNoteTypes.Instance.InternalWorkNotes.Description, "This is other notes and should be included.");
			StmNote agentNotes = AddNotes(HeaderWrapper.JobHeader.LocalCharges, PredefinedNoteTypes.Instance.GatePassNotes.Description, "Gate Pass Notes Stuff\nLine Two and should not be included");

			AssertEquals("Detailed goods description", "This is other notes and should be included.", HeaderWrapper.InternalWorkNotes);
		}

		public void TestBranch()
		{
			Header.JH_GB = ZGuid.Empty;
			AssertNull("Branch is null", HeaderWrapper.Branch);

			var branch = Factory.LoadTop1<GlbBranch>(new ZQuery());
			Header.JH_GB = branch.PK;
			AssertNotNull("Branch is not null", HeaderWrapper.Branch);
			AssertEquals("Branch is of type DocBranch", typeof(DocBranch), HeaderWrapper.Branch.GetType());
		}

		public void TestDepartment()
		{
			Header.JH_GE = ZGuid.Empty;
			AssertNull("Department is null", HeaderWrapper.Department);

			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery());
			Header.JH_GE = department.PK;
			AssertNotNull("Department is not null", HeaderWrapper.Department);
			AssertEquals("Department is of type DocBranch", typeof(DocDepartment), HeaderWrapper.Department.GetType());
		}

		public void TestOpsRep()
		{
			Header.JH_GS_NKRepOps = "";
			AssertNull("OpsRep is null", HeaderWrapper.OpsRep);

			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery());
			Header.JH_GS_NKRepOps = staff.GS_Code;
			AssertNotNull("OpsRep is not null", HeaderWrapper.OpsRep);
			AssertEquals("OpsRep is of type DocStaff", typeof(DocStaff), HeaderWrapper.OpsRep.GetType());
		}

		public void TestSalesRep()
		{
			AssertNull("SalesRep is null", HeaderWrapper.SalesRep);

			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery());
			Header.JH_GS_NKRepSales = staff.GS_Code;
			AssertNotNull("SalesRep is not null", HeaderWrapper.SalesRep);
			AssertEquals("SalesRep is of type DocStaff", typeof(DocStaff), HeaderWrapper.SalesRep.GetType());
		}

		public void TestAgentCollectAddress()
		{
			Header.JH_OA_AgentCollectAddr = ZGuid.Empty;
			AssertNull("AgentCollectAddress is null", HeaderWrapper.AgentCollectAddress);

			Header.JH_OA_AgentCollectAddr = Factory.NewWithValidTestData<OrgAddress>().PK;
			AssertNotNull("AgentCollectAddress is not null", HeaderWrapper.AgentCollectAddress);
			AssertEquals("AgentCollectAddress is of type DocAddress", typeof(DocAddress), HeaderWrapper.AgentCollectAddress.GetType());
		}

		public void TestAgentCollect()
		{
			Header.AgentCollectPK = ZGuid.Empty;
			AssertNull("AgentCollect is null", HeaderWrapper.AgentCollect);

			var organisationHeader = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Header.AgentCollectPK = organisationHeader.PK;
			AssertNotNull("AgentCollect is not null", HeaderWrapper.AgentCollect);
			AssertEquals("AgentCollect is of type DocOrganisation", typeof(DocOrganisation), HeaderWrapper.AgentCollect.GetType());
		}

		public void TestLocalCharges()
		{
			Header.LocalChargesPK = ZGuid.Empty;
			AssertNull("LocalCharges is null", HeaderWrapper.LocalCharges);

			var organisationHeader = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Header.LocalChargesPK = organisationHeader.PK;
			AssertNotNull("LocalCharges is not null", HeaderWrapper.LocalCharges);
			AssertEquals("LocalCharges is of type DocOrganisation", typeof(DocOrganisation), HeaderWrapper.LocalCharges.GetType());
		}

		public void TestDebtorOrLocalCharges()
		{
			Header.LocalChargesPK = ZGuid.Empty;
			AssertNull("DebtorOrLocalCharges is null", HeaderWrapper.DebtorOrLocalCharges);

			var localCharges = Factory.New<OrgHeader>();
			localCharges.Addresses.AddNew(OrgAddressType.Office, true);
			localCharges.OH_FullName = "LOCCHAR";
			Header.LocalChargesPK = localCharges.PK;
			AssertEquals("DebtorOrLocalCharges is LocalCharges", "LOCCHAR", HeaderWrapper.DebtorOrLocalCharges.Name);

			var debtor = Factory.New<OrgHeader>();
			debtor.OH_FullName = "DEBTOR";
			HeaderWrapper = DocJobHeader.New(Header, debtor, Factory);
			AssertEquals("DebtorOrLocalCharges is Debtor", "DEBTOR", HeaderWrapper.DebtorOrLocalCharges.Name);
		}

		public void TestQuoteNumber()
		{
			Header.JH_TH_NKQuoteNumber = "123456789";
			AssertEquals("Quote Number", "123456789", HeaderWrapper.QuoteNumber);
		}

		public void TestNewDocJobWrapperCacheStalenessPolicy()
		{
			var agent = TestObjectCreator.Agent2;
			agent.CompanyData.SetARTaxApplicable(false); // To get rid of GST Amounts when calculating TotalCharges
			var localClient = TestObjectCreator.LocalClient2;
			var creditor = TestObjectCreator.Creditor2;

			Header.AgentCollectPK = agent.PK;
			Header.LocalChargesPK = localClient.PK;

			var newHeaderWrapper1 = DocJobHeader.New(Header, Factory);
			AssertSame("Cached wrapper", newHeaderWrapper1, HeaderWrapper);
			AssertDocJobChargeCollectionsAreTheSame(newHeaderWrapper1, HeaderWrapper);

			var job = (Job)Header;
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, osCostAmt: 10m, creditor: creditor, osSellAmt: 10m, debtor: agent);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, osCostAmt: 30m, creditor: creditor, osSellAmt: 30m, debtor: agent);
			var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, osCostAmt: 20m, creditor: creditor, osSellAmt: 20m, debtor: localClient);

			newHeaderWrapper1 = DocJobHeader.New(Header, Factory);
			AssertNotSame("Wrapper replaced because of new JobCharges added", newHeaderWrapper1, HeaderWrapper);

			var newHeaderWrapper2 = GetNewWrapperAndAssertDocJobChargeCollectionsAreChanging(newHeaderWrapper1, () =>
			{
				Factory.Save();
				var newWrapper = DocJobHeader.New(Header, Factory);
				AssertNotSame("Cached wrapper replaced by Factory.Save", newHeaderWrapper1, newWrapper);
				return newWrapper;
			});

			var newHeaderWrapper3 = GetNewWrapperAndAssertDocJobChargeCollectionsAreChanging(newHeaderWrapper2, () =>
			{
				Factory.ClearCachedValue<DocumentWrapper>(job.PK.ToStringKey()); // Clear cached Wrapper for our job only
				var newWrapper = DocJobHeader.New(Header, Factory);
				AssertNotSame("Cached wrapper replaced by Factory.ClearCachedValue", newHeaderWrapper2, newWrapper);
				return newWrapper;
			});

			AssertDocJobChargeCollection(() => HeaderWrapper.JobCharges, "JobCharges", 3, 60m);
			AssertDocJobChargeCollection(() => HeaderWrapper.JobChargesForLocalClient, "JobChargesForLocalClient", 1, 20m);
			AssertDocJobChargeCollection(() => HeaderWrapper.JobChargesForAgentCollect, "JobChargesForAgentCollect", 2, 40m);
			AssertDocJobChargeCollection(() => HeaderWrapper.JobChargesForDebtorOrLocalClient, "JobChargesForDebtorOrLocalClient", 1, 20m);

			var newHeaderWrapper4 = GetNewWrapperAndAssertDocJobChargeCollectionsAreChanging(newHeaderWrapper3, () =>
			{
				charge1.JR_Desc = "Change something";
				var newWrapper = DocJobHeader.New(Header, Factory);
				AssertNotSame("Wrapper replaced because of changed JobCharge", newHeaderWrapper3, newWrapper);
				return newWrapper;
			});

			var newHeaderWrapper5 = GetNewWrapperAndAssertDocJobChargeCollectionsAreChanging(newHeaderWrapper4, () =>
			{
				charge1.Delete();

				var newWrapper = DocJobHeader.New(Header, Factory);
				AssertNotSame("Wrapper replaced because of deleted JobCharge", newHeaderWrapper4, newWrapper);
				return newWrapper;
			});

			AssertDocJobChargeCollection(() => newHeaderWrapper5.JobCharges, "JobCharges", 2, 50m);
			AssertDocJobChargeCollection(() => newHeaderWrapper5.JobChargesForLocalClient, "JobChargesForLocalClient", 1, 20m);
			AssertDocJobChargeCollection(() => newHeaderWrapper5.JobChargesForAgentCollect, "JobChargesForAgentCollect", 1, 30m);
			AssertDocJobChargeCollection(() => newHeaderWrapper5.JobChargesForDebtorOrLocalClient, "JobChargesForDebtorOrLocalClient", 1, 20m);

			// Delete by DataRefresh
			var newHeaderWrapper6 = GetNewWrapperAndAssertDocJobChargeCollectionsAreChanging(newHeaderWrapper5, () =>
			{
				var newFactory = new BusinessObjectFactory();
				var charge3inNewFactory = newFactory.Load<JobCharge>(charge3.PK);
				charge3inNewFactory.Delete();
				newFactory.Save();
				Assert("charge3.IsDeleted", charge3.IsDeleted);

				var newWrapper = DocJobHeader.New(Header, Factory);
				AssertNotSame("Wrapper replaced because of JobCharge deleted by DataRefresh", newHeaderWrapper5, newWrapper);
				return newWrapper;
			});

			AssertDocJobChargeCollection(() => newHeaderWrapper6.JobCharges, "JobCharges", 1, 30m);
			AssertDocJobChargeCollection(() => newHeaderWrapper6.JobChargesForLocalClient, "JobChargesForLocalClient", 0, 0m);
			AssertDocJobChargeCollection(() => newHeaderWrapper6.JobChargesForAgentCollect, "JobChargesForAgentCollect", 1, 30m);
			AssertDocJobChargeCollection(() => newHeaderWrapper6.JobChargesForDebtorOrLocalClient, "JobChargesForDebtorOrLocalClient", 0, 0m);

			void AssertDocJobChargeCollectionsAreTheSame(DocJobHeader firstWrapper, DocJobHeader secondWrapper)
			{
				AssertSame("JobCharges cached", firstWrapper.JobCharges, secondWrapper.JobCharges);
				AssertSame("JobChargesForLocalClient cached", firstWrapper.JobChargesForLocalClient, secondWrapper.JobChargesForLocalClient);
				AssertSame("JobChargesForAgentCollect cached", firstWrapper.JobChargesForAgentCollect, secondWrapper.JobChargesForAgentCollect);
				AssertSame("JobChargesForDebtorOrLocalClient cached", firstWrapper.JobChargesForDebtorOrLocalClient, secondWrapper.JobChargesForDebtorOrLocalClient);
			}

			DocJobHeader GetNewWrapperAndAssertDocJobChargeCollectionsAreChanging(DocJobHeader firstWrapper, Func<DocJobHeader> secondWrapperGetter)
			{
				var jobCharges = firstWrapper.JobCharges;
				var jobChargesForLocalClient = firstWrapper.JobChargesForLocalClient;
				var jobChargesForAgentCollect = firstWrapper.JobChargesForAgentCollect;
				var jobChargesForDebtorOrLocalClient = firstWrapper.JobChargesForDebtorOrLocalClient;

				var secondWrapper = secondWrapperGetter();

				AssertNotSame("JobCharges cache cleared", jobCharges, secondWrapper.JobCharges);
				AssertNotSame("JobChargesForLocalClient cache cleared", jobChargesForLocalClient, secondWrapper.JobChargesForLocalClient);
				AssertNotSame("JobChargesForAgentCollect cache cleared", jobChargesForAgentCollect, secondWrapper.JobChargesForAgentCollect);
				AssertNotSame("JobChargesForDebtorOrLocalClient cache cleared", jobChargesForDebtorOrLocalClient, secondWrapper.JobChargesForDebtorOrLocalClient);

				return secondWrapper;
			}

			void AssertDocJobChargeCollection(Func<DocJobChargeCollection> getter, string name, int count, decimal totalCharges)
			{
				ErrorReporter.Clear();
				var collection = getter();

				CombineAssertions(() =>
				{
					AssertNotNull(name, collection);

					AssertEquals(name + ".Count", count, collection.Count);
					AssertEquals(name + ".TotalCharges", totalCharges, collection.TotalCharges);
					AssertEquals(name + " - No Errors reported about accessing properties on deleted BizO", 0, ErrorReporter.TotalErrorCount);
				});
			}
		}

		#region Implementation

		JobHeader Header;
		DocJobHeader HeaderWrapper;

		protected override void SetUp()
		{
			Header = TestObjectCreator.Job1;
			HeaderWrapper = DocJobHeader.New(Header, Factory);
			AssertNotNull("PreCondition: Valid DocJobHeader", HeaderWrapper);

			base.SetUp();
		}

		AccChargeCode CreateChargeCode(string chargeCode)
		{
			var code = Factory.New<AccChargeCode>();
			code.AC_Code = chargeCode;
			code.AC_Desc = "Test Charge Code";
			code.AC_GC = GlbCompany.CurrentCompany.PK;
			code.AC_IsActive = ZBool.True;
			return code;
		}

		JobCharge CreateLineChargeWithTax(JobHeader jobHeaderBisObj, ZGuid localChargesPK, ZDecimal amount, ZGuid chargeCodePK, ZDecimal gSTAmt)
		{
			JobCharge lineCharge = CreateLineCharge(jobHeaderBisObj, localChargesPK, amount, chargeCodePK);
			TestObjectCreator.GST1.SetRateNumerator_ForTestOnly((int)(gSTAmt / amount * 100));
			lineCharge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			return lineCharge;
		}

		JobCharge CreateLineCharge(JobHeader jobHeaderBisObj, ZGuid localChargesPK, ZDecimal amount, ZGuid chargeCodePK)
		{
			var lineCharge = Factory.New<JobCharge>();
			lineCharge.JR_JH = jobHeaderBisObj.PK;
			lineCharge.JR_GE = jobHeaderBisObj.JH_GE;
			lineCharge.JR_GB = jobHeaderBisObj.JH_GB;
			lineCharge.JR_AC = chargeCodePK;
			lineCharge.JR_LocalSellAmt = amount;
			lineCharge.JR_OH_SellAccount = localChargesPK;
			return lineCharge;
		}

		#endregion
	}
}
