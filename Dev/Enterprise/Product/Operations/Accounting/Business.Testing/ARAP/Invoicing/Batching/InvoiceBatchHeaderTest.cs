using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoiceBatchHeader))]
	public class InvoiceBatchHeaderTest : TransactionHeaderTest
	{
		ARInvoice InvoiceLine1;
		ARCreditNote InvoiceLine2;

		protected override Type TypeOfValidation
		{
			get { return typeof(InvoiceBatchHeaderValidation); }
		}

		protected virtual Type TypeOfFilterObject
		{
			get { return typeof(InvoiceBatchHeaderFilterBusinessObject); }
		}

		protected override ZDecimal GetExpectedOutstandindAmount(ZDecimal expectedValue) => 0;

		public override void TestTransactionNumberGenerator()
		{
			Assert("Not applicable", true);
		}

		public new void TestReverseTransactionSetComplianceSubTypeForPeru()
		{
			Assert(true);
		}

		public new void TestReverseTransactionDoeNotSetComplianceSubTypeForNonPeru()
		{
			Assert(true);
		}

		[ExpectNoExceptions]
		public void TestNoEditingErrorAfterCancel()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1, 400, 10, 0);

			var job = TestObjectCreator.CreateJob("Job1", TestObjectCreator.LocalClient, 1, TestObjectCreator.Agent, 1);
			job.JH_JobLocalReference = "123";
			line.AL_JH = job.PK;
			line.AL_AC = TestObjectCreator.CC1.PK;
			var charge = TestObjectCreator.CreateCharge(line);

			var header = Factory.New<InvoiceBatchHeader>();
			header.Line.Add(invoice);
			header.AH_InvoiceAmount = 400;
			header.AH_GSTAmount = 0;
			header.AH_OutstandingAmount = 400;
			header.AH_OSTotal = 400;

			Factory.Save();

			header.GenerateReverseTransactionCore_ForTestOnly(false);

			Factory.Save();
		}

		public void TestTransactionNumMaxLengthIsLessThanOrEqualToAH_ReceiptBatchNoMaxLength()
		{
			var header = Factory.New<InvoiceBatchHeader>();
			using (Db.Connection.BeginTransactionWithManager())
			{
				var transactionNum = header.NumberFountainForTransactionNumber_ForTestOnly.Generate(new AccountingNumberFountainDataSourceForTest(Factory, ZDateTime.Today, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment));
				var developerMessage = string.Format("{0} max length is {1}, but you will assign {2} with length {3} to {0} in SetReceiptBatchNo method",
					AccTransactionHeaderSchema.AH_ReceiptBatchNo.Name,
					AccTransactionHeaderSchema.AH_ReceiptBatchNo.MaxLength,
					AccTransactionHeaderSchema.AH_TransactionNum.Name,
					transactionNum.Length);
				Assert(developerMessage, transactionNum.Length <= AccTransactionHeaderSchema.AH_ReceiptBatchNo.MaxLength);
			}
		}

		public void TestDontHitJobHeaderWhenLoadingInvoiceBatch()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_IsCFSRegistered = true;
			shipment1.JS_IsForwardRegistered = false;
			shipment1.JS_UniqueConsignRef = "TEST";

			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Job testJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;

			testHeader.Line[0].AH_JH = testJob.PK;

			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			testHeader = newFactory.Load<InvoiceBatchHeader>(testHeader.PK);

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			AssertMaxDbHits(1, newFactory);
		}

		public void TestLoadingExistingResult()
		{
			AccTransactionHeader header = SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.InvoiceBatch, "TEST001", 120m);
			AccTransactionHeader line1 = SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, "TEST002", 10m);
			AccTransactionHeader line2 = SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, "TEST003", 10m);

			AccTransactionHeader nonLine1 = SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Payment, "TEST004", 10m);
			AccTransactionHeader nonLine2 = SetTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Invoice, "TEST005", 10m);

			line1.AH_AH_InvoiceStatement = header.PK;
			line2.AH_AH_InvoiceStatement = header.PK;

			Factory.Save();

			InvoiceBatchHeader resultHeader = Factory.Load(typeof(InvoiceBatchHeader), header.PK) as InvoiceBatchHeader;

			AssertNotNull(resultHeader);
			AssertEquals("Ledger Types", LedgerTypes.AccountsReceivable, resultHeader.AH_Ledger);
			AssertEquals("Transaction Types", TransactionTypes.InvoiceBatch, resultHeader.AH_TransactionType);
			AssertEquals("Line Count", 2, resultHeader.Line.Count);
		}

		public void TestSavingNewRecord()
		{
			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();
			Factory.Save();

			BusinessObjectFactory readonlyFactory = new BusinessObjectFactory();
			InvoiceBatchHeader retrievedHeader = readonlyFactory.Load(typeof(InvoiceBatchHeader), testHeader.PK) as InvoiceBatchHeader;

			AssertNotNull(retrievedHeader);
			AssertEquals("Ledger Types", LedgerTypes.AccountsReceivable, retrievedHeader.AH_Ledger);
			AssertEquals("Transaction Types", TransactionTypes.InvoiceBatch, retrievedHeader.AH_TransactionType);
			AssertEquals("Line Count", 2, retrievedHeader.Line.Count);
			AssertEquals("Amount", -10m, retrievedHeader.AH_OSExTaxAmount);
		}

		public void TestOrganisationSet()
		{
			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();

			testHeader.AH_OH = TestObjectCreator.ABIGAS.PK;
			Factory.Save();

			BusinessObjectFactory readonlyFactory = new BusinessObjectFactory();
			InvoiceBatchHeader retrievedHeader = readonlyFactory.Load(typeof(InvoiceBatchHeader), testHeader.PK) as InvoiceBatchHeader;

			AssertNotNull(retrievedHeader);
			AssertEquals("Organisation", TestObjectCreator.ABIGAS.PK, retrievedHeader.AH_OH);
		}

		public void TestCurrencySet()
		{
			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();

			testHeader.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			Factory.Save();

			BusinessObjectFactory readonlyFactory = new BusinessObjectFactory();
			InvoiceBatchHeader retrievedHeader = readonlyFactory.Load(typeof(InvoiceBatchHeader), testHeader.PK) as InvoiceBatchHeader;

			AssertNotNull(retrievedHeader);
			AssertEquals("Organisation", TestObjectCreator.USD.RX_Code, retrievedHeader.AH_RX_NKTransactionCurrency);
		}

		public void TestCurrencySetByFilter()
		{
			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();
			testHeader.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;

			AssertEquals("Organisation", TestObjectCreator.USD.RX_Code, testHeader.AH_RX_NKTransactionCurrency);
			AssertEquals(((ModuleNkFilter)testHeader.Filter["Currency"]).Property, testHeader.AH_RX_NKTransactionCurrency);
		}

		public void TestReverseAmount()
		{
			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();
			((ModuleGuidFilter)testHeader.Filter["Organisation"]).Property = TestObjectCreator.ABIGAS.PK;
			((ModuleNkFilter)testHeader.Filter["Currency"]).Property = TestObjectCreator.USD.RX_Code;

			Factory.Save();

			BusinessObjectFactory readonlyFactory = new BusinessObjectFactory();
			InvoiceBatchHeader retrievedHeader = readonlyFactory.Load(typeof(InvoiceBatchHeader), testHeader.PK) as InvoiceBatchHeader;

			AssertNotNull(retrievedHeader);
			AssertEquals("Amount", -10m, retrievedHeader.AH_OSExTaxAmount);

			retrievedHeader.GenerateReverseTransaction(true);
			AssertEquals(0m, retrievedHeader.ReverseTransaction_ForTestOnly.AH_OSTotal);
			AssertEquals(0m, retrievedHeader.ReverseTransaction_ForTestOnly.AH_InvoiceAmount);
			AssertEquals(testHeader.AH_RX_NKTransactionCurrency, retrievedHeader.ReverseTransaction_ForTestOnly.AH_RX_NKTransactionCurrency);
			AssertEquals(testHeader.AH_OH, retrievedHeader.ReverseTransaction_ForTestOnly.AH_OH);
		}

		public override void TestSetTransactionBelongsToGroupField()
		{
			AssertEquals("Initial belongs to group field", ZGuid.Empty, Header.AH_TransactionBelongsToGroup);

			((IReversing)Header).GenerateReverseTransaction(true);
			TransactionHeader reversingHeader = (TransactionHeader)((IReversing)Header).ReverseTransaction;

			ZGuid groupingGuid = ZGuid.NewZGuid();
			((IReversing)Header).SetTransactionBelongsToGroupField(groupingGuid);

			AssertEquals("Default behaviour at the moment is to set transaction belongs to group to PK of header," +
					"leaving the original transactionbelongstogroup as NULL",
					ZGuid.Empty, reversingHeader.AH_TransactionBelongsToGroup);

			AssertEquals("Original transaction's transactionbelongstogroup field should be emtpy",
					ZGuid.Empty, Header.AH_TransactionBelongsToGroup);
		}

		public void TestClearLines()
		{
			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();
			AssertEquals(2, testHeader.Line.Count);
			AssertEquals(-10m, testHeader.AH_InvoiceAmount);
			AssertEquals(-10m, testHeader.AH_OSTotal);

			testHeader.ClearLines();
			AssertEquals(0, testHeader.Line.Count);
			AssertEquals(0m, testHeader.AH_InvoiceAmount);
			AssertEquals(0m, testHeader.AH_OSTotal);
		}

		public void TestInvoiceBatchNumberReadOnly()
		{
			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();
			Assert(testHeader.AH_TransactionNumInfo.ReadOnly);
		}

		public void TestFilterObject()
		{
			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();
			AssertNotNull(testHeader.Filter);

			Assert(TypeOfFilterObject.IsAssignableFrom(testHeader.Filter.GetType()));
			Assert(testHeader.Filter.IsParentSet);
		}

		public void TestTermsAndDueDateCalculationProviderCorrectType()
		{
			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();
			Assert(typeof(ARTermsAndDueDateCalculationProvider).IsAssignableFrom(testHeader.TermsAndDueDateCalculationProvider.GetType()));
		}

		public void TestTermsAndInvoiceTermsDaysSetFromOrgHeader()
		{
			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();
			TestObjectCreator.AALSHI.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = (ZByte)3;
			TestObjectCreator.AALSHI.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;

			testHeader.AH_OH = TestObjectCreator.AALSHI.PK;

			AssertEquals(TestObjectCreator.AALSHI.PK, testHeader.AH_OH);
			AssertEquals(TestObjectCreator.AALSHI.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays, testHeader.AH_InvoiceTermDays);
			AssertEquals(TestObjectCreator.AALSHI.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm, testHeader.AH_InvoiceTerm);
		}

		public void TestDueDateSet()
		{
			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();
			TestObjectCreator.AALSHI.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = (ZByte)3;
			TestObjectCreator.AALSHI.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;

			testHeader.AH_OH = TestObjectCreator.AALSHI.PK;

			testHeader.AH_InvoiceDate = new ZDateTime(2006, 1, 25);

			AssertEquals(testHeader.AH_InvoiceDate.AddDays(TestObjectCreator.AALSHI.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Days), testHeader.AH_DueDate);
		}

		public void TestInvoiceAndDueDateSetForIndividualInvoices()
		{
			ZDateTime originalDate = new ZDateTime(2006, 1, 3);
			ZDateTime newInvoiceDate = new ZDateTime(2006, 1, 8);
			ZDateTime newDueDate = new ZDateTime(2006, 1, 13);

			InvoiceBatchHeader testHeader = GetTestInvoiceBatch(originalDate);

			testHeader.AH_InvoiceDate = newInvoiceDate;
			testHeader.AH_DueDate = newDueDate;

			Factory.Save();

			AssertEquals(newInvoiceDate, InvoiceLine1.AH_InvoiceDate);
			AssertEquals(newInvoiceDate, InvoiceLine2.AH_InvoiceDate);
			AssertEquals(newDueDate, InvoiceLine1.AH_DueDate);
			AssertEquals(newDueDate, InvoiceLine2.AH_DueDate);
		}

		public void TestInvoiceTermAndDueDateReadOnly()
		{
			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();
			Assert(!testHeader.AH_InvoiceTermDaysInfo.ReadOnly);
			Assert(!testHeader.AH_InvoiceTermInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			Assert(testHeader.AH_InvoiceTermDaysInfo.ReadOnly);
			Assert(testHeader.AH_InvoiceTermInfo.ReadOnly);
		}

		public void TestInvoiceTermsList()
		{
			AssertEquals(typeof(ARInvoiceTermsList), GetTestInvoiceBatch().InvoiceTerms_List.GetType());
		}

		public void TestBatchInvoiceTypeForLocalTransport()
		{
			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testHeader.Line[0].AH_JH = testJob.PK;
			testHeader.Line[1].AH_JH = testJob.PK;
			testJob.Parent = (IJobInvoicingPlugIn)Factory.New(JobInvoicingConsumerTypes.LocalCartage.BizoType);

			AssertEquals("All lines belong to only one Module Type", 1, testHeader.BatchInvoiceModuleList.Count);
			AssertEquals(InvoiceTypeModuleList.Codes.TPT, testHeader.BatchInvoiceModuleList[0]);
		}

		public void TestBatchInvoiceTypeForForwarding()
		{
			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testHeader.Line[0].AH_JH = testJob.PK;
			testHeader.Line[1].AH_JH = testJob.PK;

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_IsCFSRegistered = false;
			testJob.Parent = shipment;

			AssertEquals("All lines belong to only one Module Type", 1, testHeader.BatchInvoiceModuleList.Count);
			AssertEquals(InvoiceTypeModuleList.Codes.FWD, testHeader.BatchInvoiceModuleList[0]);
		}

		public void TestBatchInvoiceTypeForCFS()
		{
			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testHeader.Line[0].AH_JH = testJob.PK;
			testHeader.Line[1].AH_JH = testJob.PK;

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_IsCFSRegistered = true;
			shipment.JS_IsForwardRegistered = false;
			testJob.Parent = shipment;

			AssertEquals("All lines belong to only one Module Type", 1, testHeader.BatchInvoiceModuleList.Count);
			AssertEquals(InvoiceTypeModuleList.Codes.CFS, testHeader.BatchInvoiceModuleList[0]);
		}

		public void TestBatchInvoiceTypeForLoadList()
		{
			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testHeader.Line[0].AH_JH = testJob.PK;
			testHeader.Line[1].AH_JH = testJob.PK;

			testJob.Parent = (IJobInvoicingPlugIn)Factory.New(JobInvoicingConsumerTypes.CFSLoadList.BizoType);

			AssertEquals("All lines belong to only one Module Type", 1, testHeader.BatchInvoiceModuleList.Count);
			AssertEquals(InvoiceTypeModuleList.Codes.CFS, testHeader.BatchInvoiceModuleList[0]);
		}

		public void TestBatchInvoiceTypeForCustoms()
		{
			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testHeader.Line[0].AH_JH = testJob.PK;
			testHeader.Line[1].AH_JH = testJob.PK;
			testJob.Parent = (IJobInvoicingPlugIn)Factory.New(JobInvoicingConsumerTypes.Brokerage.BizoType);

			AssertEquals("All lines belong to only one Module Type", 1, testHeader.BatchInvoiceModuleList.Count);
			AssertEquals(InvoiceTypeModuleList.Codes.CUS, testHeader.BatchInvoiceModuleList[0]);
		}

		public void TestBatchInvoiceTypeForMscInvoices()
		{
			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();

			AssertEquals("All lines belong to only one Module Type", 1, testHeader.BatchInvoiceModuleList.Count);
			AssertEquals(InvoiceTypeModuleList.Codes.MSC, testHeader.BatchInvoiceModuleList[0]);
		}

		public void TestBatchInvoiceJobType()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_IsCFSRegistered = true;
			shipment.JS_IsForwardRegistered = false;
			shipment.JS_UniqueConsignRef = "TEST";

			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();
			Job testJob = Enterprise.Accounting.Business.JobInvoicing.Job.CreateWithMutex(Factory, shipment);
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			testJob.JH_JobNum = "TEST";
			testJob.JH_ParentID = shipment.PK;
			testJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var declaration = Factory.NewWithValidTestData(JobInvoicingConsumerTypes.Brokerage.BizoType);
			JobHeader testJob2 = TestObjectCreator.CreateJob((IJobInvoicingPlugIn)declaration);

			testHeader.Line[0].AH_JH = testJob.PK;

			Factory.Save();

			BusinessObjectFactory readFactory = new BusinessObjectFactory();
			InvoiceBatchHeader batchReadFromFactory = readFactory.Load(typeof(InvoiceBatchHeader), testHeader.PK) as InvoiceBatchHeader;

			AssertEquals("All lines belong to only two Module Types", 2, testHeader.BatchInvoiceModuleList.Count);
			Assert("There are lines of CFS module", batchReadFromFactory.BatchInvoiceModuleList.Contains(InvoiceTypeModuleList.Codes.CFS));
			Assert("There are lines of MSC module", batchReadFromFactory.BatchInvoiceModuleList.Contains(InvoiceTypeModuleList.Codes.MSC));

			testHeader.Line[1].AH_JH = testJob2.PK;

			Factory.Save();

			readFactory = new BusinessObjectFactory();
			batchReadFromFactory = readFactory.Load(typeof(InvoiceBatchHeader), testHeader.PK) as InvoiceBatchHeader;

			AssertEquals("All lines belong to only two Module Types", 2, testHeader.BatchInvoiceModuleList.Count);
			Assert("There are lines of CFS module", batchReadFromFactory.BatchInvoiceModuleList.Contains(InvoiceTypeModuleList.Codes.CFS));
			Assert("There are lines of CUS module", batchReadFromFactory.BatchInvoiceModuleList.Contains(InvoiceTypeModuleList.Codes.CUS));
		}

		public void TestGetInvoiceModuleType() //The current implementation of available Job Types doiesn't depend on AH_OH, if it is right logic, this test shoulb be slight modified(name, implementation...)
		{
			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes.AddNew();
			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes.AddNew();

			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes[0].PI_Module = InvoiceTypeModuleList.Codes.CFS;
			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes[1].PI_Module = InvoiceTypeModuleList.Codes.FWD;

			Factory.Save();

			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();

			testHeader.AH_OH = TestObjectCreator.AALSHI.PK;

			AssertEquals(5, testHeader.JobTypeList.Count);

			AssertEquals(testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.CFS), testHeader.JobTypeList[0].Description);
			AssertEquals(testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.CUS), testHeader.JobTypeList[1].Description);
			AssertEquals(testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.FWD), testHeader.JobTypeList[2].Description);
			AssertEquals(testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.MSC), testHeader.JobTypeList[3].Description);
			AssertEquals(testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.TPT), testHeader.JobTypeList[4].Description);

			Assert(!testHeader.JobTypeList[0].Value);
			Assert(!testHeader.JobTypeList[1].Value);
			Assert(!testHeader.JobTypeList[2].Value);
			Assert(!testHeader.JobTypeList[3].Value);
			Assert(!testHeader.JobTypeList[4].Value);
		}

		public void TestInvoiceTypeLookUp()
		{
			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();

			AssertEquals(5, testHeader.InvoiceTypeLookUp_ForTestOnly.Count);

			AssertEquals(testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.CFS), testHeader.InvoiceTypeLookUp_ForTestOnly[0].Description);
			AssertEquals(testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.CUS), testHeader.InvoiceTypeLookUp_ForTestOnly[1].Description);
			AssertEquals(testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.FWD), testHeader.InvoiceTypeLookUp_ForTestOnly[2].Description);
			AssertEquals(testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.MSC), testHeader.InvoiceTypeLookUp_ForTestOnly[3].Description);
			AssertEquals(testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.TPT), testHeader.InvoiceTypeLookUp_ForTestOnly[4].Description);

			testHeader.InvoiceTypeLookUp_ForTestOnly = null;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			AssertEquals(6, testHeader.InvoiceTypeLookUp_ForTestOnly.Count);

			AssertEquals(testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.CFS), testHeader.InvoiceTypeLookUp_ForTestOnly[0].Description);
			AssertEquals(testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.CUS), testHeader.InvoiceTypeLookUp_ForTestOnly[1].Description);
			AssertEquals(testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.FWD), testHeader.InvoiceTypeLookUp_ForTestOnly[2].Description);
			AssertEquals(testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.ISF), testHeader.InvoiceTypeLookUp_ForTestOnly[3].Description);
			AssertEquals(testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.MSC), testHeader.InvoiceTypeLookUp_ForTestOnly[4].Description);
			AssertEquals(testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.TPT), testHeader.InvoiceTypeLookUp_ForTestOnly[5].Description);
		}

		public void TestAH_OHSFilterResetJobTypeAndCurrency()
		{
			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();

			testHeader.JobTypeList[0].Value = true;
			testHeader.AH_RX_NKTransactionCurrency = TestObjectCreator.GBP.RX_Code;

			testHeader.AH_OH = TestObjectCreator.AALSHI.PK;

			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, testHeader.AH_RX_NKTransactionCurrency);
			Assert(!testHeader.JobTypeList[0].Value);
			AssertEquals(0, testHeader.SelectedJobTypeCodes.Count);
		}

		public void TestDefaultCurrencySetByDebtor()
		{
			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();

			TestObjectCreator.AALSHI.CompanyData.OB_RX_NKARDDefltCurrency = TestObjectCreator.GBP.RX_Code;

			testHeader.AH_OH = TestObjectCreator.AALSHI.PK;

			AssertEquals(TestObjectCreator.GBP.RX_Code, testHeader.AH_RX_NKTransactionCurrency);
			AssertEquals(TestObjectCreator.GBP.RX_Code, ((ModuleNkFilter)testHeader.Filter["Currency"]).Property);
		}

		public void TestInitialization()
		{
			ZGuid expectedAH_OH = TestObjectCreator.ABIGAS.PK;
			ZGuid expectedAH_OH_2 = TestObjectCreator.ZECTRA.PK;
			ZString expectedJobType = InvoiceTypeModuleList.Codes.FWD;
			ZString expectedJobType_2 = InvoiceTypeModuleList.Codes.CFS;
			ZString expectedAH_RX_NKTransactionCurrency = TestObjectCreator.GBP.RX_Code;
			ZString expectedAH_RX_NKTransactionCurrency2 = TestObjectCreator.USD.RX_Code;
			ZDateTime expectedAH_InvoiceDate = new ZDateTime(2006, 03, 14);
			ZDateTime expectedAH_InvoiceDate_2 = new ZDateTime(2003, 03, 14);
			ZString expectedAH_InvoiceTerm = InvoiceTermsList.PaymentInAdvance.Code;
			ZByte expectedAH_InvoiceTermDays = 11;

			InvoiceBatchHeader sourceHeader = Factory.New(GetExpectedBusinessObjectType()) as InvoiceBatchHeader;
			sourceHeader.AH_OH = expectedAH_OH;
			sourceHeader.JobTypeList[sourceHeader.GetDescriptionForJobTypeList(expectedJobType)].Value = true;
			sourceHeader.AH_RX_NKTransactionCurrency = expectedAH_RX_NKTransactionCurrency;
			sourceHeader.AH_InvoiceDate = expectedAH_InvoiceDate;
			sourceHeader.AH_InvoiceTerm = expectedAH_InvoiceTerm;
			sourceHeader.AH_InvoiceTermDays = expectedAH_InvoiceTermDays;

			InvoiceBatchHeader testHeader = Factory.New(GetExpectedBusinessObjectType()) as InvoiceBatchHeader;
			testHeader.Initialization(sourceHeader);
			AssertEquals("AH_OH", sourceHeader.AH_OH, testHeader.AH_OH);
			AssertEquals("SelectedJobTypeList", 1, testHeader.SelectedJobTypeCodes.Count);
			AssertEquals("SelectedJobTypeList", expectedJobType, testHeader.SelectedJobTypeCodes[0]);
			AssertEquals("AH_RX_NKTransactionCurrency", expectedAH_RX_NKTransactionCurrency, testHeader.AH_RX_NKTransactionCurrency);
			AssertEquals("AH_InvoiceDate", expectedAH_InvoiceDate, testHeader.AH_InvoiceDate);
			AssertEquals("AH_InvoiceTerm", expectedAH_InvoiceTerm, testHeader.AH_InvoiceTerm);
			AssertEquals("AH_InvoiceTermDays", expectedAH_InvoiceTermDays, testHeader.AH_InvoiceTermDays);

			testHeader.Initialization(sourceHeader, expectedAH_OH_2);
			AssertEquals("AH_OH", sourceHeader.AH_OH.IsEmpty ? ZGuid.Empty : expectedAH_OH_2, testHeader.AH_OH);
			AssertEquals("SelectedJobTypeList", 1, testHeader.SelectedJobTypeCodes.Count);
			AssertEquals("SelectedJobTypeList", expectedJobType, testHeader.SelectedJobTypeCodes[0]);
			AssertEquals("AH_RX_NKTransactionCurrency", expectedAH_RX_NKTransactionCurrency, testHeader.AH_RX_NKTransactionCurrency);
			AssertEquals("AH_InvoiceDate", expectedAH_InvoiceDate, testHeader.AH_InvoiceDate);
			AssertEquals("AH_InvoiceTerm", expectedAH_InvoiceTerm, testHeader.AH_InvoiceTerm);
			AssertEquals("AH_InvoiceTermDays", expectedAH_InvoiceTermDays, testHeader.AH_InvoiceTermDays);

			sourceHeader = Factory.New(GetExpectedBusinessObjectType()) as InvoiceBatchHeader;
			sourceHeader.AH_OH = expectedAH_OH_2;
			sourceHeader.JobTypeList[sourceHeader.GetDescriptionForJobTypeList(expectedJobType_2)].Value = true;
			sourceHeader.AH_RX_NKTransactionCurrency = expectedAH_RX_NKTransactionCurrency2;
			sourceHeader.AH_InvoiceDate = expectedAH_InvoiceDate_2;
			testHeader.Initialization(sourceHeader);
			AssertEquals("AH_OH", sourceHeader.AH_OH, testHeader.AH_OH);
			AssertEquals("SelectedJobTypeList", 1, testHeader.SelectedJobTypeCodes.Count);
			AssertEquals("SelectedJobTypeList", expectedJobType_2, testHeader.SelectedJobTypeCodes[0]);
			AssertEquals("AH_RX_NKTransactionCurrency", expectedAH_RX_NKTransactionCurrency2, testHeader.AH_RX_NKTransactionCurrency);
			AssertEquals("AH_InvoiceDate", expectedAH_InvoiceDate_2, testHeader.AH_InvoiceDate);
			AssertEquals("AH_InvoiceTerm", sourceHeader.Header == null ? expectedAH_InvoiceTerm :
					sourceHeader.Header.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Term, testHeader.AH_InvoiceTerm);
			AssertEquals("AH_InvoiceTermDays", sourceHeader.Header == null ? expectedAH_InvoiceTermDays :
					sourceHeader.Header.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Days, testHeader.AH_InvoiceTermDays);
		}

		public override void TestGenerateReverseTransaction()
		{
			InvoiceBatchHeader header = GetTestInvoiceBatch();

			header.AH_ExchangeRate = 0.8M;
			header.AH_OSExTaxAmount = 500M;
			header.AH_OSTaxAmount = 50M;
			header.AH_LocalExTaxAmount = 630M;
			header.AH_LocalTaxAmount = 64M;
			InvoiceLine1.AH_ReceiptBatchNo = "123";
			InvoiceLine1.AH_AH_InvoiceStatement = header.PK;

			header.GenerateReverseTransaction(true);
			InvoiceBatchHeader reversedHeader = (InvoiceBatchHeader)header.ReverseTransaction;
			AssertEquals("Reverse transaction should be the base transaction", reversedHeader, header);
			AssertEquals(ZDecimal.Zero, reversedHeader.AH_OSExTaxAmount);
			AssertEquals(ZDecimal.Zero, reversedHeader.AH_LocalExTaxAmount);
			AssertEquals(ZDecimal.Zero, reversedHeader.AH_LocalTaxAmount);
			AssertEquals(ZDecimal.Zero, reversedHeader.AH_OSTaxAmount);
			AssertEquals(ZDecimal.Zero, reversedHeader.AH_OutstandingAmount);
			AssertEquals(ZDecimal.Zero, reversedHeader.AH_WithholdingTax);
			AssertEquals(1M, reversedHeader.AH_ExchangeRate);
			Assert(reversedHeader.IsReverseTransaction);
			AssertEquals(reversedHeader.OriginalTransaction, header);
			AssertEquals(ZString.Empty, InvoiceLine1.AH_ReceiptBatchNo);
			AssertEquals(ZGuid.Empty, InvoiceLine1.AH_AH_InvoiceStatement);
		}

		public void TestAH_ExchangeRateWithOtherTaxes()
		{
			var arInvoiceBatchLine1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "APINV1", TestObjectCreator.CNY, 2m, 2000m, 200m, 1000m, 100m);
			var arInvoiceBatchLine2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "APINV2", TestObjectCreator.CNY, 4m, 4000m, 400m, 1000m, 100m);
			arInvoiceBatchLine1.AH_LocalTaxAmountOtherTaxes = 100M;
			arInvoiceBatchLine2.AH_LocalTaxAmountOtherTaxes = 100M;
			arInvoiceBatchLine1.AH_OSTaxAmountOtherTaxes = 200M;
			arInvoiceBatchLine2.AH_OSTaxAmountOtherTaxes = 400M;

			Factory.Save();

			var testHeader = Factory.NewWithValidTestData(GetExpectedBusinessObjectType()) as InvoiceBatchHeader;
			testHeader.AH_RX_NKTransactionCurrency = TestObjectCreator.CNY.RX_Code;
			testHeader.AH_LocalTaxAmountOtherTaxes = 200M;
			testHeader.Line.Add(arInvoiceBatchLine1);
			testHeader.Line.Add(arInvoiceBatchLine2);

			AssertEquals(7200m, testHeader.AH_OSTotal);
			AssertEquals(2400m, testHeader.AH_LocalTotal);
			AssertEquals(1m, testHeader.AH_ExchangeRate);

			AssertEquals(2m, arInvoiceBatchLine1.AH_ExchangeRate);
			AssertEquals(4m, arInvoiceBatchLine2.AH_ExchangeRate);

			Factory.Save();

			AssertEquals(2m, arInvoiceBatchLine1.AH_ExchangeRate);
			AssertEquals(4m, arInvoiceBatchLine2.AH_ExchangeRate);
			AssertEquals(3m, testHeader.AH_ExchangeRate);
		}

		public void TestWhenOnSavingUpdateInvoiceDateOfInvoiceWithoutUpdateExRate()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2m, new DateTime(2015, 1, 1), new DateTime(2015, 1, 31));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "SEL", 2m, new DateTime(2015, 1, 1), new DateTime(2015, 1, 31));

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 4m, new DateTime(2015, 2, 1), new DateTime(2015, 2, 28));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "SEL", 4m, new DateTime(2015, 2, 1), new DateTime(2015, 2, 28));
			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");

			Factory.Save();

			var invoiceBatchLine = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.CNY, 2m, TestObjectCreator.ABIGAS, new DateTime(2015, 1, 10));
			TestObjectCreator.CreateInvoiceLine(invoiceBatchLine, TestObjectCreator.CNY, 2m, 10m, 0m, 0m);
			Factory.Save();

			var testHeader = Factory.NewWithValidTestData(GetExpectedBusinessObjectType()) as InvoiceBatchHeader;
			testHeader.AH_RX_NKTransactionCurrency = TestObjectCreator.CNY.RX_Code;

			testHeader.Line.Add(invoiceBatchLine);

			testHeader.AH_InvoiceDate = new DateTime(2015, 2, 10);
			testHeader.AH_DueDate = new DateTime(2015, 2, 10);

			var oldInvAmount = invoiceBatchLine.AH_InvoiceAmount;
			AssertNotEquals(0m, oldInvAmount);
			AssertEquals("PreCondition", 2m, invoiceBatchLine.AH_ExchangeRate);
			AssertEquals("PreCondition", new DateTime(2015, 1, 10), invoiceBatchLine.AH_InvoiceDate);

			Factory.Save();

			AssertEquals(oldInvAmount, invoiceBatchLine.AH_InvoiceAmount);
			AssertEquals(2m, invoiceBatchLine.AH_ExchangeRate);
			AssertEquals(new DateTime(2015, 2, 10), invoiceBatchLine.AH_InvoiceDate);
		}

		protected override bool AreOriginalAndReverseTransactionsTheSame
		{
			get { return true; }
		}

		protected override void AssertReverseTransactionDueDateValue(TransactionHeader reverseHeader)
		{
			AssertEquals("Due Date should not change", Header.AH_DueDate.Date, reverseHeader.AH_DueDate.Date);
		}

		protected InvoiceBatchHeader GetTestInvoiceBatch()
		{
			InvoiceLine1 = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoiceLine arInvoiceLine = Factory.NewWithValidTestData<ARInvoiceLine>();
			arInvoiceLine.AL_OSExTaxAmount = 10m;
			arInvoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			InvoiceLine1.Lines.Add(arInvoiceLine);

			InvoiceLine2 = Factory.NewWithValidTestData<ARCreditNote>();
			ARCreditNoteLine arCreditNoteLine = Factory.NewWithValidTestData<ARCreditNoteLine>();
			arCreditNoteLine.AL_OSExTaxAmount = 20m;
			arCreditNoteLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			InvoiceLine2.Lines.Add(arCreditNoteLine);

			Factory.Save();

			InvoiceBatchHeader testHeader = Factory.NewWithValidTestData(GetExpectedBusinessObjectType()) as InvoiceBatchHeader;

			testHeader.Line.Add(InvoiceLine1);
			testHeader.Line.Add(InvoiceLine2);

			return testHeader;
		}

		InvoiceBatchHeader GetTestInvoiceBatch(ZDateTime invoiceDate)
		{
			InvoiceLine1 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			InvoiceLine1.AH_OSExTaxAmount = 10m;
			InvoiceLine1.AH_InvoiceDate = invoiceDate;
			InvoiceLine1.AH_DueDate = invoiceDate;

			InvoiceLine2 = Factory.NewWithValidTestData(typeof(ARCreditNote)) as ARCreditNote;
			InvoiceLine2.AH_OSExTaxAmount = 20m;
			InvoiceLine1.AH_InvoiceDate = invoiceDate;
			InvoiceLine1.AH_DueDate = invoiceDate;

			Factory.Save();

			InvoiceBatchHeader testHeader = Factory.NewWithValidTestData(GetExpectedBusinessObjectType()) as InvoiceBatchHeader;

			testHeader.Line.Add(InvoiceLine1);
			testHeader.Line.Add(InvoiceLine2);

			return testHeader;
		}

		AccTransactionHeader SetTransaction(string ledger, string transactionType, string transactionNumber, decimal amount)
		{
			AccTransactionHeader testHeader = Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader;
			testHeader.AH_Ledger = ledger;
			testHeader.AH_TransactionType = transactionType;
			testHeader.AH_TransactionNum = transactionNumber;
			testHeader.AH_InvoiceAmount = amount;
			testHeader.AH_OutstandingAmount = amount;
			testHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			testHeader.AH_GE = GlbDepartment.CurrentDepartment.PK;
			testHeader.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			return testHeader;
		}
	}
}
