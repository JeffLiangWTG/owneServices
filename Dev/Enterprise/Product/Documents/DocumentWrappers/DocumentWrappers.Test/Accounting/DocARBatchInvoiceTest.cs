using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocARBatchInvoice))]
	sealed class DocARBatchInvoiceTest : DocARBaseInvoiceTest
	{
		public void TestHasGSTANDQCTLine()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				var taxRate = Factory.New<AccTaxRate>();
				taxRate.AT_RN_NKCountry = Constants.CountryCodes.India;
				taxRate.AT_Type = AccTaxRate.Types.Rated;
				taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
				var invoice = Factory.New<ARInvoice>();
				var line = (ARInvoiceLine)invoice.Lines.AddNew();
				line.AL_AT = taxRate.PK;

				var batch = Factory.New<InvoiceBatchHeader>();
				batch.Line.Add(invoice);

				var wrapper = DocARBatchInvoice.New(batch, Factory);
				Assert(wrapper.HasGSTANDQCTLine);
			}
		}

		public void TestHasGSTANDQCTLine_SER()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				var taxRate = Factory.New<AccTaxRate>();
				taxRate.AT_RN_NKCountry = Constants.CountryCodes.India;
				taxRate.AT_Type = AccTaxRate.Types.ServiceTax;
				taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
				var invoice = Factory.New<ARInvoice>();
				var line = (ARInvoiceLine)invoice.Lines.AddNew();
				line.AL_AT = taxRate.PK;

				var batch = Factory.New<InvoiceBatchHeader>();
				batch.Line.Add(invoice);

				var wrapper = DocARBatchInvoice.New(batch, Factory);
				Assert(wrapper.HasGSTANDQCTLine);
			}
		}

		public void TestHasGSTANDEDULine()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			{
				var taxRate = Factory.New<AccTaxRate>();
				taxRate.AT_RN_NKCountry = Constants.CountryCodes.India;
				taxRate.AT_Type = AccTaxRate.Types.Rated;
				taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
				var invoice = Factory.New<ARInvoice>();
				var line = (ARInvoiceLine)invoice.Lines.AddNew();
				line.AL_AT = taxRate.PK;

				var batch = Factory.New<InvoiceBatchHeader>();
				batch.Line.Add(invoice);

				var wrapper = DocARBatchInvoice.New(batch, Factory);
				Assert(wrapper.HasGSTANDEDULine);
			}
		}

		public void TestHasGSTANDEDULine_SER()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			{
				var taxRate = Factory.New<AccTaxRate>();
				taxRate.AT_RN_NKCountry = Constants.CountryCodes.India;
				taxRate.AT_Type = AccTaxRate.Types.ServiceTax;
				taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
				var invoice = Factory.New<ARInvoice>();
				var line = (ARInvoiceLine)invoice.Lines.AddNew();
				line.AL_AT = taxRate.PK;

				var batch = Factory.New<InvoiceBatchHeader>();
				batch.Line.Add(invoice);

				var wrapper = DocARBatchInvoice.New(batch, Factory);
				Assert(wrapper.HasGSTANDEDULine);
			}
		}

		public void TestGetInvoicesCore()
		{
			InvoiceBatchHeader batchHeader = SetUpInvoiceBatch();
			DocARBatchInvoice batchHeaderWrapper = DocARBatchInvoice.New(batchHeader, Factory);

			DocARBatchInvoiceForTest test = new DocARBatchInvoiceForTest(batchHeader, Factory, false);

			AssertEquals(2, test.GetInvoicesCoreForTest().Count);
		}

		public void TestInvoiceSubTotalTotal()
		{
			InvoiceBatchHeader batchHeader = SetUpInvoiceBatch();
			DocARBatchInvoice batchHeaderWrapper = DocARBatchInvoice.New(batchHeader, Factory);
			AssertEquals("Invoice SubTotal Total", 480M, batchHeaderWrapper.InvoiceSubTotalTotal);
		}

		public void TestInvoiceOSTaxAmountTotal()
		{
			InvoiceBatchHeader batchHeader = SetUpInvoiceBatch();
			DocARBatchInvoice batchHeaderWrapper = DocARBatchInvoice.New(batchHeader, Factory);
			AssertEquals("Invoice SubTotal Total", 25M, batchHeaderWrapper.InvoiceOSTaxAmountTotal);
		}

		public void TestInvoiceTotal()
		{
			InvoiceBatchHeader batchHeader = SetUpInvoiceBatch();
			DocARBatchInvoice batchHeaderWrapper = DocARBatchInvoice.New(batchHeader, Factory);
			AssertEquals("Invoice SubTotal Total", 505M, batchHeaderWrapper.InvoiceTotal);
		}

		public void TestLineSignsByCharge()
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			ARCreditNote creditNote = Factory.New<ARCreditNote>();

			ARInvoiceLine cC1InvoiceLine = (ARInvoiceLine)invoice.Lines.AddNew();
			ARCreditNoteLine cC1CreditNoteLine = (ARCreditNoteLine)creditNote.Lines.AddNew();

			ARInvoiceLine cC2InvoiceLine = (ARInvoiceLine)invoice.Lines.AddNew();
			ARCreditNoteLine cC2CreditNoteLine = (ARCreditNoteLine)creditNote.Lines.AddNew();

			var chargeCode1 = TestObjectCreator.CC1;
			chargeCode1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			var chargeCode2 = TestObjectCreator.CC2;
			chargeCode2.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			cC1InvoiceLine.AL_AC = chargeCode1.PK;
			cC1CreditNoteLine.AL_AC = chargeCode1.PK;
			cC1InvoiceLine.AL_OSExTaxAmount = 100m;
			cC1CreditNoteLine.AL_OSExTaxAmount = 50m;

			cC2InvoiceLine.AL_AC = chargeCode2.PK;
			cC2CreditNoteLine.AL_AC = chargeCode2.PK;
			cC2InvoiceLine.AL_OSExTaxAmount = 160m;
			cC2CreditNoteLine.AL_OSExTaxAmount = 80m;

			InvoiceBatchHeader batch = Factory.New<InvoiceBatchHeader>();
			batch.Line.Add(invoice);
			batch.Line.Add(creditNote);

			DocARBatchInvoice batchWrapper = DocARBatchInvoice.New(batch, Factory);
			AssertEquals(2, batchWrapper.InvoiceLineByCharge.Count);

			IDocARInvoiceLine cC1Line = null;
			IDocARInvoiceLine cC2Line = null;

			foreach (IDocARInvoiceLine line in batchWrapper.InvoiceLineByCharge)
			{
				if (line.LineDescription == chargeCode1.AC_Desc)
				{
					cC1Line = line;
				}
				else if (line.LineDescription == chargeCode2.AC_Desc)
				{
					cC2Line = line;
				}
			}

			AssertEquals(50m, cC1Line.OSAmount);
			AssertEquals(80m, cC2Line.OSAmount);
		}

		public void TestLineSignsByJob()
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			ARCreditNote creditNote = Factory.New<ARCreditNote>();

			ARInvoiceLine job1InvoiceLine = (ARInvoiceLine)invoice.Lines.AddNew();
			ARCreditNoteLine job1CreditNoteLine = (ARCreditNoteLine)creditNote.Lines.AddNew();

			ARInvoiceLine job2InvoiceLine = (ARInvoiceLine)invoice.Lines.AddNew();
			ARCreditNoteLine job2CreditNoteLine = (ARCreditNoteLine)creditNote.Lines.AddNew();

			job1InvoiceLine.AL_JH = TestObjectCreator.Job1.PK;
			job1CreditNoteLine.AL_JH = TestObjectCreator.Job1.PK;
			job1InvoiceLine.AL_OSExTaxAmount = 100m;
			job1CreditNoteLine.AL_OSExTaxAmount = 50m;

			job2InvoiceLine.AL_JH = TestObjectCreator.Job2.PK;
			job2CreditNoteLine.AL_JH = TestObjectCreator.Job2.PK;
			job2InvoiceLine.AL_OSExTaxAmount = 160m;
			job2CreditNoteLine.AL_OSExTaxAmount = 80m;

			InvoiceBatchHeader batch = Factory.New<InvoiceBatchHeader>();
			batch.Line.Add(invoice);
			batch.Line.Add(creditNote);

			DocARBatchInvoice batchWrapper = DocARBatchInvoice.New(batch, Factory);
			AssertEquals(2, batchWrapper.InvoiceLineByJob.Count);

			IDocARInvoiceLine job1Line = null;
			IDocARInvoiceLine job2Line = null;

			foreach (IDocARInvoiceLine line in batchWrapper.InvoiceLineByJob)
			{
				if (line.JobHeader.JobNumber == TestObjectCreator.Job1.JH_JobNum)
				{
					job1Line = line;
				}
				else if (line.JobHeader.JobNumber == TestObjectCreator.Job2.JH_JobNum)
				{
					job2Line = line;
				}
			}

			AssertEquals(50m, job1Line.OSAmount);
			AssertEquals(80m, job2Line.OSAmount);
		}

		protected override DocARBaseInvoice GetBaseInvoiceWrapper()
		{
			return DocARBatchInvoice.New(InvoiceBatch, Factory);
		}

		protected override TransactionHeader GetWrappedInvoice()
		{
			return InvoiceBatch;
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocARBatchInvoice.New(InvoiceBatch, Factory), };
		}

		public void TestBalanceForStatement()
		{
			InvoiceBatchHeader batchHeader = SetUpInvoiceBatch();
			DocARBatchInvoice batchHeaderWrapper = DocARBatchInvoice.New(batchHeader, Factory);
			AssertEquals("Batch Header Balance for Statement", 505M, batchHeaderWrapper.Balance);
		}

		public void TestInvoiceBatchNumber()
		{
			string oldInvoiceTransactionNumberPrefix = AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.Value;
			try
			{
				AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ABC");
				InvoiceBatch.AH_TransactionNum = "00001001";
				AssertEquals(InvoiceBatch.TransactionNumberPrefixed, BatchInvoiceWrapper.BatchInvoiceNumber);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldInvoiceTransactionNumberPrefix);
			}
		}

		public void TestBatchInvoiceType()
		{
			//CREATE MSC JOBTYPE
			var jobType = (BatchInvoiceWrapper.BatchJobTypeList != null && BatchInvoiceWrapper.BatchJobTypeList.Any()) ? BatchInvoiceWrapper.BatchJobTypeList.First() : ZString.Empty;

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());

			//CREATE CONFIGURATIONS
			AddInvoiceTypeToCompanyData(header.CompanyData, "MSC", "ALL", "ALL", InvoiceTypeLayoutList.Codes.INV, InvoiceTypeLayoutList.Codes.INV);
			AddInvoiceTypeToCompanyData(header.CompanyData, "BRK", "ALL", "ALL", InvoiceTypeLayoutList.Codes.CHG, InvoiceTypeLayoutList.Codes.CHG);

			Factory.Save();

			//ADD INVOICE TO BATCH
			InvoiceBatch.AH_TransactionNum = "00001001";
			InvoiceBatch.AH_OH = header.PK;
			BatchedInvoice.AH_OH = InvoiceBatch.AH_OH;
			InvoiceBatch.Line.Add(BatchedInvoice);

			//SHOULD PICK UP THE MSC CONFIGURATION
			AssertEquals("BatchInvoicing should have picked up the MSC configuration.", InvoiceTypeLayoutList.Codes.INV, BatchInvoiceWrapper.BatchInvoiceType);
			AssertEquals("BatchInvoicing should have picked up the MSC configuration.", InvoiceTypeLayoutList.Codes.INV, BatchInvoiceWrapper.BatchInvoiceSecondaryType);
		}

		public void TestBatchInvoiceTypeForLocalTransport()
		{
			var testHeader = GetTestInvoiceBatch();
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.Parent = (IJobInvoicingPlugIn)Factory.NewWithValidTestData(JobInvoicingConsumerTypes.LocalCartage.BizoType);
			testHeader.Line[0].AH_JH = testJob.PK;
			testHeader.Line[1].AH_JH = testJob.PK;
			AssertEquals(1, testHeader.BatchInvoiceModuleList.Count);
			AssertEquals(InvoiceTypeModuleList.Codes.TPT, testHeader.BatchInvoiceModuleList[0]);

			var batchInvoiceWrapper = DocARBatchInvoice.New(testHeader, Factory);
			AssertEquals(1, batchInvoiceWrapper.BatchInvoiceModuleList.Length);
			AssertEquals(InvoiceTypeModuleList.Codes.TPT, batchInvoiceWrapper.BatchInvoiceModuleList[0]);
			AssertEquals("Statement Description", "AR INVOICE - VARIOUS JOBS", batchInvoiceWrapper.StatementDescription);
		}

		public void TestBatchInvoiceTypeForForwarding()
		{
			var testHeader = GetTestInvoiceBatch();
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testHeader.Line[0].AH_JH = testJob.PK;
			testHeader.Line[1].AH_JH = testJob.PK;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_IsCFSRegistered = false;
			testJob.Parent = shipment;

			AssertEquals(1, testHeader.BatchInvoiceModuleList.Count);
			AssertEquals(InvoiceTypeModuleList.Codes.FWD, testHeader.BatchInvoiceModuleList[0]);

			var batchInvoiceWrapper = DocARBatchInvoice.New(testHeader, Factory);
			AssertEquals(1, batchInvoiceWrapper.BatchInvoiceModuleList.Length);
			AssertEquals(InvoiceTypeModuleList.Codes.FWD, batchInvoiceWrapper.BatchInvoiceModuleList[0]);
		}

		public void TestBatchInvoiceTypeForCFS()
		{
			var testHeader = GetTestInvoiceBatch();
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testHeader.Line[0].AH_JH = testJob.PK;
			testHeader.Line[1].AH_JH = testJob.PK;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_IsCFSRegistered = true;
			shipment.JS_IsForwardRegistered = false;
			testJob.Parent = shipment;

			AssertEquals(1, testHeader.BatchInvoiceModuleList.Count);
			AssertEquals(InvoiceTypeModuleList.Codes.CFS, testHeader.BatchInvoiceModuleList[0]);

			var batchInvoiceWrapper = DocARBatchInvoice.New(testHeader, Factory);
			AssertEquals(1, batchInvoiceWrapper.BatchInvoiceModuleList.Length);
			AssertEquals(InvoiceTypeModuleList.Codes.CFS, batchInvoiceWrapper.BatchInvoiceModuleList[0]);
		}

		public void TestBatchInvoiceTypeForLoadList()
		{
			var testHeader = GetTestInvoiceBatch();
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.Parent = (IJobInvoicingPlugIn)Factory.NewWithValidTestData(JobInvoicingConsumerTypes.CFSLoadList.BizoType);
			testHeader.Line[0].AH_JH = testJob.PK;
			testHeader.Line[1].AH_JH = testJob.PK;

			AssertEquals(1, testHeader.BatchInvoiceModuleList.Count);
			AssertEquals(InvoiceTypeModuleList.Codes.CFS, testHeader.BatchInvoiceModuleList[0]);

			var batchInvoiceWrapper = DocARBatchInvoice.New(testHeader, Factory);
			AssertEquals(1, batchInvoiceWrapper.BatchInvoiceModuleList.Length);
			AssertEquals(InvoiceTypeModuleList.Codes.CFS, batchInvoiceWrapper.BatchInvoiceModuleList[0]);
		}

		public void TestBatchInvoiceTypeForCustoms()
		{
			var testHeader = GetTestInvoiceBatch();
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.Parent = (IJobInvoicingPlugIn)Factory.NewWithValidTestData(JobInvoicingConsumerTypes.Brokerage.BizoType);
			testHeader.Line[0].AH_JH = testJob.PK;
			testHeader.Line[1].AH_JH = testJob.PK;

			AssertEquals(1, testHeader.BatchInvoiceModuleList.Count);
			AssertEquals(InvoiceTypeModuleList.Codes.CUS, testHeader.BatchInvoiceModuleList[0]);

			var batchInvoiceWrapper = DocARBatchInvoice.New(testHeader, Factory);
			AssertEquals(1, batchInvoiceWrapper.BatchInvoiceModuleList.Length);
			AssertEquals(InvoiceTypeModuleList.Codes.CUS, batchInvoiceWrapper.BatchInvoiceModuleList[0]);
		}

		public void TestBatchInvoiceJobType()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_IsCFSRegistered = true;
			shipment.JS_IsForwardRegistered = false;
			shipment.JS_UniqueConsignRef = "TEST";

			InvoiceBatchHeader testHeader = GetTestInvoiceBatch();
			Job testJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			testJob.JH_JobNum = "TEST";
			testJob.JH_ParentID = shipment.PK;
			testJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			testHeader.Line[0].AH_JH = testJob.PK;
			testHeader.Line[1].AH_JH = testJob.PK;

			Factory.Save();

			BusinessObjectFactory readFactory = new BusinessObjectFactory();
			InvoiceBatchHeader batchReadFromFactory = readFactory.Load(typeof(InvoiceBatchHeader), testHeader.PK) as InvoiceBatchHeader;

			AssertEquals(1, batchReadFromFactory.BatchInvoiceModuleList.Count);
			AssertEquals(InvoiceTypeModuleList.Codes.CFS, batchReadFromFactory.BatchInvoiceModuleList[0]);

			DocARBatchInvoice batchInvoiceWrapper = DocARBatchInvoice.New(testHeader, Factory);
			AssertEquals(1, batchInvoiceWrapper.BatchInvoiceModuleList.Length);
			AssertEquals(InvoiceTypeModuleList.Codes.CFS, batchInvoiceWrapper.BatchInvoiceModuleList[0]);
		}

		public void TestGetInvoiceModuleType()
		{
			InvoiceTypeModuleList invoiceTypeModuleList = new InvoiceTypeModuleList();

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
		}

		public void TestInvoiceLineByJob()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code);

			ARInvoice arInvoice1, arInvoice2;

			var shipment = testObjectCreator.CreateShipment("JobNumber 02");
			shipment.JS_HouseBill = "Test 0101";
			var job1 = testObjectCreator.CreateJob(shipment);

			var consol = (CFSLoadListConsol)testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.CFSLoadList);
			consol.JK_UniqueConsignRef = "JobNumber 01";
			consol.JK_AgentsReference = "Test 0102";
			var job2 = testObjectCreator.CreateJob(consol);

			InvoiceBatch = SetUpForTestInvoiceLineByJob(out arInvoice1, out arInvoice2, job1, job2);

			var docBatchInvoice = DocARBatchInvoice.New(InvoiceBatch, Factory);

			AssertEquals("OS Tax Amount", 12m, arInvoice1.Lines[0].AL_OSTaxAmount);
			AssertEquals("OS Tax Amount", 7m, arInvoice1.Lines[1].AL_OSTaxAmount);
			AssertEquals("OS Tax Amount", 6m, arInvoice2.Lines[0].AL_OSTaxAmount);

			AssertNotNull(docBatchInvoice.InvoiceLineByJob);
			AssertEquals(2, docBatchInvoice.InvoiceLineByJob.Count);

			AssertDocARInvoiceLineWithJobNumber(docBatchInvoice.InvoiceLineByJob[0], "JobNumber 01", 230m, "7.00", "7.00");
			AssertDocARInvoiceLineWithJobNumber(docBatchInvoice.InvoiceLineByJob[1], "JobNumber 02", 250m, "18.00", "18.00");

			ARInvoice arInvoice3, arInvoice4;

			var declaration = (Enterprise.Customs.Business.BaseJobDeclaration)testObjectCreator.CreateDeclaration("JobNumber 03");
			declaration.JE_AgentsReference = "Test 0103";
			var job3 = testObjectCreator.CreateJob(declaration);

			var cartage = testObjectCreator.CreateCartage();
			cartage.JJ_ConsignmentID = "JobNumber 04";
			cartage.ContainerBookedMoves.AddNew();
			cartage.Containers.First().JC_ContainerNum = "Test 0103";
			var job4 = testObjectCreator.CreateJob(cartage);

			InvoiceBatch = SetUpForTestInvoiceLineByJob(out arInvoice3, out arInvoice4, job3, job4);

			docBatchInvoice = DocARBatchInvoice.New(InvoiceBatch, Factory);
			AssertNotNull(docBatchInvoice.InvoiceLineByJob);
			AssertEquals(2, docBatchInvoice.InvoiceLineByJob.Count);

			AssertDocARInvoiceLineWithJobNumber(docBatchInvoice.InvoiceLineByJob[0], "JobNumber 03", 250m, "18.00", "18.00");
			AssertDocARInvoiceLineWithJobNumber(docBatchInvoice.InvoiceLineByJob[1], "JobNumber 04", 230m, "7.00", "7.00");

			AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxRate.Code);
			docBatchInvoice = DocARBatchInvoice.New(InvoiceBatch, Factory);
			AssertNotNull(docBatchInvoice.InvoiceLineByJob);
			AssertEquals(4, docBatchInvoice.InvoiceLineByJob.Count);
			AssertDocARInvoiceLineWithJobNumber(docBatchInvoice.InvoiceLineByJob[0], "JobNumber 03", 180m, "10%");
			AssertDocARInvoiceLineWithJobNumber(docBatchInvoice.InvoiceLineByJob[1], "JobNumber 03", 70m, "Zero Rated");
			AssertDocARInvoiceLineWithJobNumber(docBatchInvoice.InvoiceLineByJob[2], "JobNumber 04", 70m, "10%");
			AssertDocARInvoiceLineWithJobNumber(docBatchInvoice.InvoiceLineByJob[3], "JobNumber 04", 160m, "Zero Rated");

			AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.Both.Code);
			docBatchInvoice = DocARBatchInvoice.New(InvoiceBatch, Factory);
			AssertNotNull(docBatchInvoice.InvoiceLineByJob);
			AssertEquals(4, docBatchInvoice.InvoiceLineByJob.Count);
			AssertDocARInvoiceLineWithJobNumber(docBatchInvoice.InvoiceLineByJob[0], "JobNumber 03", 180m, "10%=18.00");
			AssertDocARInvoiceLineWithJobNumber(docBatchInvoice.InvoiceLineByJob[1], "JobNumber 03", 70m, "Zero Rated");
			AssertDocARInvoiceLineWithJobNumber(docBatchInvoice.InvoiceLineByJob[2], "JobNumber 04", 70m, "10%=7.00");
			AssertDocARInvoiceLineWithJobNumber(docBatchInvoice.InvoiceLineByJob[3], "JobNumber 04", 160m, "Zero Rated");
		}

		InvoiceBatchHeader SetUpForTestInvoiceLineByJob(out ARInvoice invoice1, out ARInvoice invoice2, JobHeader job1, JobHeader job2)
		{
			invoice1 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			invoice2 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;

			invoice1.Lines.AddNew();
			invoice1.Lines.AddNew();
			invoice1.Lines.AddNew();

			invoice2.Lines.AddNew();
			invoice2.Lines.AddNew();

			var chargeCode1 = TestObjectCreator.CC1;
			chargeCode1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			var chargeCode2 = TestObjectCreator.CC2;
			chargeCode2.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			var chargeCode3 = TestObjectCreator.CC3;
			chargeCode3.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			SetLineDetails(invoice1.Lines[0], chargeCode1, TestObjectCreator.GST1, 120m, 12m, job1);
			TestObjectCreator.CreateJobCharge(invoice1.Lines[0], job1, chargeCode1, TestObjectCreator.AUD);

			SetLineDetails(invoice1.Lines[1], chargeCode2, TestObjectCreator.GST1, 70m, 7m, job2);
			TestObjectCreator.CreateJobCharge(invoice1.Lines[1], job2, chargeCode1, TestObjectCreator.AUD);

			SetLineDetails(invoice1.Lines[2], chargeCode1, TestObjectCreator.GSTFREE1, 70m, 0m, job1);
			TestObjectCreator.CreateJobCharge(invoice1.Lines[2], job1, chargeCode1, TestObjectCreator.AUD);

			SetLineDetails(invoice2.Lines[0], chargeCode1, TestObjectCreator.GST1, 60m, 6m, job1);
			TestObjectCreator.CreateJobCharge(invoice2.Lines[0], job1, chargeCode1, TestObjectCreator.AUD);

			SetLineDetails(invoice2.Lines[1], chargeCode3, TestObjectCreator.GSTFREE1, 160m, 0m, job2);
			TestObjectCreator.CreateJobCharge(invoice2.Lines[1], job2, chargeCode1, TestObjectCreator.AUD);

			var invoiceBatch = Factory.NewWithValidTestData(typeof(InvoiceBatchHeader)) as InvoiceBatchHeader;

			invoice1.AH_AH_InvoiceStatement = invoiceBatch.PK;
			invoice2.AH_AH_InvoiceStatement = invoiceBatch.PK;

			invoiceBatch.Line.Add(invoice1);
			invoiceBatch.Line.Add(invoice2);

			invoice1.AH_FullyPaidDate = ZDateTime.Empty;
			invoice2.AH_FullyPaidDate = ZDateTime.Empty;

			Factory.Save();

			return invoiceBatch;
		}

		public void TestInvoiceLineByCharge()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			ARInvoice aRInvoice1 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			ARInvoice aRInvoice2 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;

			aRInvoice1.Lines.AddNew();
			aRInvoice1.Lines.AddNew();
			aRInvoice1.Lines.AddNew();

			aRInvoice2.Lines.AddNew();
			aRInvoice2.Lines.AddNew();

			var chargeCode1 = testObjectCreator.CC1;
			chargeCode1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			SetLineDetails(aRInvoice1.Lines[0], chargeCode1, testObjectCreator.GST1, 120m, 12m);
			SetLineDetails(aRInvoice1.Lines[1], testObjectCreator.CC2, testObjectCreator.GST1, 70m, 7m);
			SetLineDetails(aRInvoice1.Lines[2], chargeCode1, testObjectCreator.GSTFREE1, 70m, 0m);

			SetLineDetails(aRInvoice2.Lines[0], chargeCode1, testObjectCreator.GST1, 60m, 6m);
			SetLineDetails(aRInvoice2.Lines[1], testObjectCreator.CC3, testObjectCreator.GSTFREE1, 160m, 0m);

			InvoiceBatchHeader invoiceBatch = Factory.NewWithValidTestData(typeof(InvoiceBatchHeader)) as InvoiceBatchHeader;

			aRInvoice1.AH_AH_InvoiceStatement = invoiceBatch.PK;
			aRInvoice2.AH_AH_InvoiceStatement = invoiceBatch.PK;

			invoiceBatch.Line.Add(aRInvoice1);
			invoiceBatch.Line.Add(aRInvoice2);

			DocARBatchInvoice docBatchInvoice = DocARBatchInvoice.New(invoiceBatch, Factory);

			AssertNotNull(docBatchInvoice.InvoiceLineByCharge);
			AssertEquals(4, docBatchInvoice.InvoiceLineByCharge.Count);

			AssertDocARInvoiceLineWithDescription(docBatchInvoice.InvoiceLineByCharge[0], testObjectCreator.CC1.AC_Desc, 180m, "10%=18.00", "18.00");
			AssertDocARInvoiceLineWithDescription(docBatchInvoice.InvoiceLineByCharge[1], testObjectCreator.CC1.AC_Desc, 70m, "Zero Rated", "Zero Rated");
			AssertDocARInvoiceLineWithDescription(docBatchInvoice.InvoiceLineByCharge[2], testObjectCreator.CC2.AC_Desc, 70m, "10%=7.00", "7.00");
			AssertDocARInvoiceLineWithDescription(docBatchInvoice.InvoiceLineByCharge[3], testObjectCreator.CC3.AC_Desc, 160m, "Zero Rated", "Zero Rated");

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				docBatchInvoice = DocARBatchInvoice.New(invoiceBatch, Factory);
				AssertDocARInvoiceLineWithDescription(docBatchInvoice.InvoiceLineByCharge[0], testObjectCreator.CC1.AC_Desc, 180m, "10%=18,00", "18,00");
				AssertDocARInvoiceLineWithDescription(docBatchInvoice.InvoiceLineByCharge[1], testObjectCreator.CC1.AC_Desc, 70m, "Zero Rated", "Zero Rated");
				AssertDocARInvoiceLineWithDescription(docBatchInvoice.InvoiceLineByCharge[2], testObjectCreator.CC2.AC_Desc, 70m, "10%=7,00", "7,00");
				AssertDocARInvoiceLineWithDescription(docBatchInvoice.InvoiceLineByCharge[3], testObjectCreator.CC3.AC_Desc, 160m, "Zero Rated", "Zero Rated");
			}
		}

		void AssertDocARInvoiceLineWithDescription(IDocARInvoiceLine line, ZString lineDescription, ZDecimal oSExTaxAmount, ZString oSTaxDisplay, ZString taxAmountDisplay)
		{
			AssertEquals("LineDescription", lineDescription, line.LineDescription);
			AssertEquals("OSExTaxAmount", oSExTaxAmount, line.OSExTaxAmount);
			AssertEquals("OSTaxDisplay", oSTaxDisplay, line.OSTaxDisplay);
			AssertEquals("TaxAmountDisplay", taxAmountDisplay, line.TaxAmountDisplay);
		}

		void AssertDocARInvoiceLineWithJobNumber(IDocARInvoiceLine line, ZString jobNumber, ZDecimal oSExTaxAmount, ZString oSTaxDisplay, ZString taxAmountDisplay)
		{
			AssertEquals("JobNumber", jobNumber, line.JobNumber);
			AssertEquals("OSExTaxAmount", oSExTaxAmount, line.OSExTaxAmount);
			AssertEquals("OSTaxDisplay", oSTaxDisplay, line.OSTaxDisplay);
			AssertEquals("TaxAmountDisplay", taxAmountDisplay, line.TaxAmountDisplay);
		}

		void AssertDocARInvoiceLineWithJobNumber(IDocARInvoiceLine line, ZString jobNumber, ZDecimal oSExTaxAmount, ZString oSTaxDisplayNoAsterisksWithRegistryRule)
		{
			AssertEquals("JobNumber", jobNumber, line.JobNumber);
			AssertEquals("OSExTaxAmount", oSExTaxAmount, line.OSExTaxAmount);
			AssertEquals("OSTaxDisplayNoAsterisksWithRegistryRule", oSTaxDisplayNoAsterisksWithRegistryRule, line.OSTaxDisplayNoAsterisksWithRegistryRule);
		}

		InvoiceBatchHeader SetUpInvoiceBatch()
		{
			ARInvoice aRInvoice1 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			ARInvoice aRInvoice2 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;

			aRInvoice1.Lines.AddNew();
			aRInvoice1.Lines.AddNew();
			aRInvoice1.Lines.AddNew();

			aRInvoice2.Lines.AddNew();
			aRInvoice2.Lines.AddNew();

			SetLineDetails(aRInvoice1.Lines[0], TestObjectCreator.CC1, TestObjectCreator.GST1, 120m, 12m);
			SetLineDetails(aRInvoice1.Lines[1], TestObjectCreator.CC2, TestObjectCreator.GST1, 70m, 7m);
			SetLineDetails(aRInvoice1.Lines[2], TestObjectCreator.CC1, TestObjectCreator.GSTFREE1, 70m, 0m);

			SetLineDetails(aRInvoice2.Lines[0], TestObjectCreator.CC1, TestObjectCreator.GST1, 60m, 6m);
			SetLineDetails(aRInvoice2.Lines[1], TestObjectCreator.CC3, TestObjectCreator.GSTFREE1, 160m, 0m);

			InvoiceBatchHeader invoiceBatch = Factory.NewWithValidTestData(typeof(InvoiceBatchHeader)) as InvoiceBatchHeader;

			aRInvoice1.AH_AH_InvoiceStatement = invoiceBatch.PK;
			aRInvoice2.AH_AH_InvoiceStatement = invoiceBatch.PK;

			invoiceBatch.Line.Add(aRInvoice1);
			invoiceBatch.Line.Add(aRInvoice2);
			return invoiceBatch;
		}

		public void TestOSTaxDisplayEDU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(10);
				TestObjectCreator.GSTFREE1.AT_Code = "FREEGST";

				InvoiceBatchHeader invoiceBatch = SetUpInvoiceBatchWithEDU();
				DocARBatchInvoice docBatchInvoice = DocARBatchInvoice.New(invoiceBatch, Factory);
				AssertEquals("Tax display heading", "SER/EDU", docBatchInvoice.OSTaxDisplayHeading);

				AssertEquals(3, docBatchInvoice.InvoiceLineByCharge.Count);

				AssertEquals(TestObjectCreator.CC1.AC_Desc, docBatchInvoice.InvoiceLineByCharge[0].LineDescription);
				AssertEquals(TestObjectCreator.CC2.AC_Desc, docBatchInvoice.InvoiceLineByCharge[1].LineDescription);
				AssertEquals(TestObjectCreator.CC3.AC_Desc, docBatchInvoice.InvoiceLineByCharge[2].LineDescription);

				AssertEquals("10%=18.00", docBatchInvoice.InvoiceLineByCharge[0].OSTaxDisplay);
				AssertEquals("18.00", docBatchInvoice.InvoiceLineByCharge[0].TaxAmountDisplay);
				AssertEquals(18m, docBatchInvoice.InvoiceLineByCharge[0].OSTaxAmount);
				AssertEquals("SER 12%=72.00,\r\nEDU 3%=2.16", docBatchInvoice.InvoiceLineByCharge[1].OSTaxDisplay);
				AssertEquals("74.16", docBatchInvoice.InvoiceLineByCharge[1].TaxAmountDisplay);
				AssertEquals(74.16m, docBatchInvoice.InvoiceLineByCharge[1].OSTaxAmount);
				AssertEquals("Zero Rated", docBatchInvoice.InvoiceLineByCharge[2].OSTaxDisplay);
				AssertEquals("Zero Rated", docBatchInvoice.InvoiceLineByCharge[2].TaxAmountDisplay);
				AssertEquals(0m, docBatchInvoice.InvoiceLineByCharge[2].OSTaxAmount);

				AssertEquals(6, docBatchInvoice.InvoiceLine.Count);
				AssertEquals("10%=12.00", docBatchInvoice.InvoiceLine[0].OSTaxDisplay);
				AssertEquals("12.00", docBatchInvoice.InvoiceLine[0].TaxAmountDisplay);
				AssertEquals(12m, docBatchInvoice.InvoiceLine[0].OSTaxAmount);
				AssertEquals("SER 12%=12.00,\r\nEDU 3%=0.36", docBatchInvoice.InvoiceLine[1].OSTaxDisplay);
				AssertEquals("12.36", docBatchInvoice.InvoiceLine[1].TaxAmountDisplay);
				AssertEquals(12.36m, docBatchInvoice.InvoiceLine[1].OSTaxAmount);
				AssertEquals("Zero Rated", docBatchInvoice.InvoiceLine[2].OSTaxDisplay);
				AssertEquals("Zero Rated", docBatchInvoice.InvoiceLine[2].TaxAmountDisplay);
				AssertEquals(0m, docBatchInvoice.InvoiceLine[2].OSTaxAmount);
				AssertEquals("10%=6.00", docBatchInvoice.InvoiceLine[3].OSTaxDisplay);
				AssertEquals("6.00", docBatchInvoice.InvoiceLine[3].TaxAmountDisplay);
				AssertEquals(6m, docBatchInvoice.InvoiceLine[3].OSTaxAmount);
				AssertEquals("SER 12%=24.00,\r\nEDU 3%=0.72", docBatchInvoice.InvoiceLine[4].OSTaxDisplay);
				AssertEquals("24.72", docBatchInvoice.InvoiceLine[4].TaxAmountDisplay);
				AssertEquals(24.72m, docBatchInvoice.InvoiceLine[4].OSTaxAmount);
				AssertEquals("SER 12%=36.00,\r\nEDU 3%=1.08", docBatchInvoice.InvoiceLine[5].OSTaxDisplay);
				AssertEquals("37.08", docBatchInvoice.InvoiceLine[5].TaxAmountDisplay);
				AssertEquals(37.08m, docBatchInvoice.InvoiceLine[5].OSTaxAmount);
			}
		}

		public void TestEDUAmounts()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(10);
				TestObjectCreator.GSTFREE1.AT_Code = "FREEGST";

				InvoiceBatchHeader invoiceBatch = SetUpInvoiceBatchWithEDU();
				DocARBatchInvoice docBatchInvoice1 = DocARBatchInvoice.New(invoiceBatch, Factory);
				AssertEquals("Tax display heading", "SER/EDU", docBatchInvoice1.OSTaxDisplayHeading);

				AssertEquals(942.16m, docBatchInvoice1.InvoiceTotal);
				AssertEquals(850m, docBatchInvoice1.InvoiceAmount);
				AssertEquals(90m, docBatchInvoice1.InvoiceOSTaxAmountTotal);
				AssertEquals(2.16m, docBatchInvoice1.TotalOSEDUAmount);
				AssertEquals(1.44m, docBatchInvoice1.TotalOSEDUPrimaryAmount);
				AssertEquals(0.72m, docBatchInvoice1.TotalOSEDUSecondaryAmount);

				using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
				{
					AssertEquals("1.44", docBatchInvoice1.TotalOSEDUPrimaryAmountFormatted);
					AssertEquals("2.16", docBatchInvoice1.TotalOSEDUAmountFormatted);
					AssertEquals("0.72", docBatchInvoice1.TotalOSEDUSecondaryAmountFormatted);
				}
				using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
				{
					AssertEquals("1,44", docBatchInvoice1.TotalOSEDUPrimaryAmountFormatted);
					AssertEquals("2,16", docBatchInvoice1.TotalOSEDUAmountFormatted);
					AssertEquals("0,72", docBatchInvoice1.TotalOSEDUSecondaryAmountFormatted);
				}
			}

			InvoiceBatch = SetUpInvoiceBatch();
			DocARBatchInvoice docBatchInvoice2 = DocARBatchInvoice.New(InvoiceBatch, Factory);
			AssertEquals("Tax display heading", "GST", docBatchInvoice2.OSTaxDisplayHeading);

			AssertEquals(505m, docBatchInvoice2.InvoiceTotal);
			AssertEquals(480m, docBatchInvoice2.InvoiceAmount);
			AssertEquals(25m, docBatchInvoice2.InvoiceOSTaxAmountTotal);
			AssertEquals(0m, docBatchInvoice2.TotalOSEDUAmount);
			AssertEquals(0m, docBatchInvoice2.TotalOSEDUPrimaryAmount);
			AssertEquals(0m, docBatchInvoice2.TotalOSEDUSecondaryAmount);
			AssertEquals("0.00", docBatchInvoice2.TotalOSEDUPrimaryAmountFormatted);
			AssertEquals("0.00", docBatchInvoice2.TotalOSEDUSecondaryAmountFormatted);
		}

		public void TestOSTaxDisplayEDU_SER()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(10);
				TestObjectCreator.GSTFREE1.AT_Code = "FREEGST";

				InvoiceBatchHeader invoiceBatch = SetUpInvoiceBatchWithEDU_SER();
				DocARBatchInvoice docBatchInvoice = DocARBatchInvoice.New(invoiceBatch, Factory);
				AssertEquals("Tax display heading", "SER/EDU", docBatchInvoice.OSTaxDisplayHeading);

				AssertEquals(3, docBatchInvoice.InvoiceLineByCharge.Count);

				AssertEquals(TestObjectCreator.CC1.AC_Desc, docBatchInvoice.InvoiceLineByCharge[0].LineDescription);
				AssertEquals(TestObjectCreator.CC2.AC_Desc, docBatchInvoice.InvoiceLineByCharge[1].LineDescription);
				AssertEquals(TestObjectCreator.CC3.AC_Desc, docBatchInvoice.InvoiceLineByCharge[2].LineDescription);

				AssertEquals("10%=18.00", docBatchInvoice.InvoiceLineByCharge[0].OSTaxDisplay);
				AssertEquals("18.00", docBatchInvoice.InvoiceLineByCharge[0].TaxAmountDisplay);
				AssertEquals(18m, docBatchInvoice.InvoiceLineByCharge[0].OSTaxAmount);
				AssertEquals("SER 12%=72.00,\r\nEDU 3%=2.16", docBatchInvoice.InvoiceLineByCharge[1].OSTaxDisplay);
				AssertEquals("74.16", docBatchInvoice.InvoiceLineByCharge[1].TaxAmountDisplay);
				AssertEquals(74.16m, docBatchInvoice.InvoiceLineByCharge[1].OSTaxAmount);
				AssertEquals("Zero Rated", docBatchInvoice.InvoiceLineByCharge[2].OSTaxDisplay);
				AssertEquals("Zero Rated", docBatchInvoice.InvoiceLineByCharge[2].TaxAmountDisplay);
				AssertEquals(0m, docBatchInvoice.InvoiceLineByCharge[2].OSTaxAmount);

				AssertEquals(6, docBatchInvoice.InvoiceLine.Count);
				AssertEquals("10%=12.00", docBatchInvoice.InvoiceLine[0].OSTaxDisplay);
				AssertEquals("12.00", docBatchInvoice.InvoiceLine[0].TaxAmountDisplay);
				AssertEquals(12m, docBatchInvoice.InvoiceLine[0].OSTaxAmount);
				AssertEquals("SER 12%=12.00,\r\nEDU 3%=0.36", docBatchInvoice.InvoiceLine[1].OSTaxDisplay);
				AssertEquals("12.36", docBatchInvoice.InvoiceLine[1].TaxAmountDisplay);
				AssertEquals(12.36m, docBatchInvoice.InvoiceLine[1].OSTaxAmount);
				AssertEquals("Zero Rated", docBatchInvoice.InvoiceLine[2].OSTaxDisplay);
				AssertEquals("Zero Rated", docBatchInvoice.InvoiceLine[2].TaxAmountDisplay);
				AssertEquals(0m, docBatchInvoice.InvoiceLine[2].OSTaxAmount);
				AssertEquals("10%=6.00", docBatchInvoice.InvoiceLine[3].OSTaxDisplay);
				AssertEquals("6.00", docBatchInvoice.InvoiceLine[3].TaxAmountDisplay);
				AssertEquals(6m, docBatchInvoice.InvoiceLine[3].OSTaxAmount);
				AssertEquals("SER 12%=24.00,\r\nEDU 3%=0.72", docBatchInvoice.InvoiceLine[4].OSTaxDisplay);
				AssertEquals("24.72", docBatchInvoice.InvoiceLine[4].TaxAmountDisplay);
				AssertEquals(24.72m, docBatchInvoice.InvoiceLine[4].OSTaxAmount);
				AssertEquals("SER 12%=36.00,\r\nEDU 3%=1.08", docBatchInvoice.InvoiceLine[5].OSTaxDisplay);
				AssertEquals("37.08", docBatchInvoice.InvoiceLine[5].TaxAmountDisplay);
				AssertEquals(37.08m, docBatchInvoice.InvoiceLine[5].OSTaxAmount);
			}
		}

		public void TestEDUAmounts_SER()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(10);
				TestObjectCreator.GSTFREE1.AT_Code = "FREEGST";

				InvoiceBatchHeader invoiceBatch = SetUpInvoiceBatchWithEDU_SER();
				DocARBatchInvoice docBatchInvoice1 = DocARBatchInvoice.New(invoiceBatch, Factory);
				AssertEquals("Tax display heading", "SER/EDU", docBatchInvoice1.OSTaxDisplayHeading);

				AssertEquals(942.16m, docBatchInvoice1.InvoiceTotal);
				AssertEquals(850m, docBatchInvoice1.InvoiceAmount);
				AssertEquals(90m, docBatchInvoice1.InvoiceOSTaxAmountTotal);
				AssertEquals(2.16m, docBatchInvoice1.TotalOSEDUAmount);
				AssertEquals(1.44m, docBatchInvoice1.TotalOSEDUPrimaryAmount);
				AssertEquals(0.72m, docBatchInvoice1.TotalOSEDUSecondaryAmount);
				using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
				{
					AssertEquals("1.44", docBatchInvoice1.TotalOSEDUPrimaryAmountFormatted);
					AssertEquals("2.16", docBatchInvoice1.TotalOSEDUAmountFormatted);
					AssertEquals("0.72", docBatchInvoice1.TotalOSEDUSecondaryAmountFormatted);
				}
				using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
				{
					AssertEquals("1,44", docBatchInvoice1.TotalOSEDUPrimaryAmountFormatted);
					AssertEquals("2,16", docBatchInvoice1.TotalOSEDUAmountFormatted);
					AssertEquals("0,72", docBatchInvoice1.TotalOSEDUSecondaryAmountFormatted);
				}
			}

			InvoiceBatch = SetUpInvoiceBatch();
			DocARBatchInvoice docBatchInvoice2 = DocARBatchInvoice.New(InvoiceBatch, Factory);
			AssertEquals("Tax display heading", "GST", docBatchInvoice2.OSTaxDisplayHeading);

			AssertEquals(505m, docBatchInvoice2.InvoiceTotal);
			AssertEquals(480m, docBatchInvoice2.InvoiceAmount);
			AssertEquals(25m, docBatchInvoice2.InvoiceOSTaxAmountTotal);
			AssertEquals(0m, docBatchInvoice2.TotalOSEDUAmount);
			AssertEquals(0m, docBatchInvoice2.TotalOSEDUPrimaryAmount);
			AssertEquals(0m, docBatchInvoice2.TotalOSEDUSecondaryAmount);
			AssertEquals("0.00", docBatchInvoice2.TotalOSEDUPrimaryAmountFormatted);
			AssertEquals("0.00", docBatchInvoice2.TotalOSEDUSecondaryAmountFormatted);
		}

		public void TestOSTaxDisplayHeading()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			var line1 = (InvoicingLineBase)invoice1.Lines.AddNew();
			var line2 = (InvoicingLineBase)invoice2.Lines.AddNew();
			SetLineDetails(line1, TestObjectCreator.CC1, null, 10m, 0m);
			SetLineDetails(line2, TestObjectCreator.CC2, null, 20m, 0m);
			var invoiceBatch = Factory.NewWithValidTestData<InvoiceBatchHeader>();
			invoice1.AH_AH_InvoiceStatement = invoiceBatch.PK;
			invoice2.AH_AH_InvoiceStatement = invoiceBatch.PK;
			invoiceBatch.Line.Add(invoice1);
			invoiceBatch.Line.Add(invoice1);

			var docBatchInvoice = DocARBatchInvoice.New(invoiceBatch, Factory);
			AssertEquals(ZString.Empty, docBatchInvoice.OSTaxDisplayHeading);

			SetLineDetails(line1, TestObjectCreator.CC1, TestObjectCreator.GST1, 10m, 0m);
			SetLineDetails(line2, TestObjectCreator.CC2, TestObjectCreator.GST1, 20m, 0m);

			Factory.ClearCachedValue<ZBool>(invoice1.PK.ToStringKey());
			Factory.ClearCachedValue<ZBool>(invoice2.PK.ToStringKey());
			AssertEquals("GST", docBatchInvoice.OSTaxDisplayHeading);
		}

		InvoiceBatchHeader SetUpInvoiceBatchWithEDU()
		{
			AccTaxRate gstAndEdu = Factory.NewWithValidTestData<AccTaxRate>();
			gstAndEdu.AT_RN_NKCountry = Constants.CountryCodes.India;
			gstAndEdu.AT_Type = AccTaxRate.Types.Rated;
			gstAndEdu.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
			gstAndEdu.SetRateNumerator_ForTestOnly(12);
			gstAndEdu.SetExtraRate_ForTestOnly(3, 1);

			ARInvoice aRInvoice1 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			ARInvoice aRInvoice2 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;

			aRInvoice1.Lines.AddNew();
			aRInvoice1.Lines.AddNew();
			aRInvoice1.Lines.AddNew();

			aRInvoice2.Lines.AddNew();
			aRInvoice2.Lines.AddNew();
			aRInvoice2.Lines.AddNew();

			var chargeCode1 = TestObjectCreator.CC1;
			chargeCode1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			var chargeCode2 = TestObjectCreator.CC2;
			chargeCode2.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			var chargeCode3 = TestObjectCreator.CC3;
			chargeCode3.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			SetLineDetails(aRInvoice1.Lines[0], chargeCode1, TestObjectCreator.GST1, 120m, 12m);
			SetLineDetails(aRInvoice1.Lines[1], chargeCode2, gstAndEdu, 100m, 12.36m);
			SetLineDetails(aRInvoice1.Lines[2], chargeCode3, TestObjectCreator.GSTFREE1, 70m, 0m);

			SetLineDetails(aRInvoice2.Lines[0], chargeCode1, TestObjectCreator.GST1, 60m, 6m);
			SetLineDetails(aRInvoice2.Lines[1], chargeCode2, gstAndEdu, 200m, 24.72m);
			SetLineDetails(aRInvoice2.Lines[2], chargeCode2, gstAndEdu, 300m, 37.08m);

			InvoiceBatchHeader invoiceBatch = Factory.NewWithValidTestData(typeof(InvoiceBatchHeader)) as InvoiceBatchHeader;

			aRInvoice1.AH_AH_InvoiceStatement = invoiceBatch.PK;
			aRInvoice2.AH_AH_InvoiceStatement = invoiceBatch.PK;

			invoiceBatch.Line.Add(aRInvoice1);
			invoiceBatch.Line.Add(aRInvoice2);
			return invoiceBatch;
		}

		InvoiceBatchHeader SetUpInvoiceBatchWithEDU_SER()
		{
			AccTaxRate gstAndEdu = Factory.NewWithValidTestData<AccTaxRate>();
			gstAndEdu.AT_RN_NKCountry = Constants.CountryCodes.India;
			gstAndEdu.AT_Type = AccTaxRate.Types.ServiceTax;
			gstAndEdu.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
			gstAndEdu.SetRateNumerator_ForTestOnly(12);
			gstAndEdu.SetExtraRate_ForTestOnly(3, 1);

			ARInvoice aRInvoice1 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			ARInvoice aRInvoice2 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;

			aRInvoice1.Lines.AddNew();
			aRInvoice1.Lines.AddNew();
			aRInvoice1.Lines.AddNew();

			aRInvoice2.Lines.AddNew();
			aRInvoice2.Lines.AddNew();
			aRInvoice2.Lines.AddNew();

			var chargeCode1 = TestObjectCreator.CC1;
			chargeCode1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			var chargeCode2 = TestObjectCreator.CC2;
			chargeCode2.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			var chargeCode3 = TestObjectCreator.CC3;
			chargeCode3.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			SetLineDetails(aRInvoice1.Lines[0], chargeCode1, TestObjectCreator.GST1, 120m, 12m);
			SetLineDetails(aRInvoice1.Lines[1], chargeCode2, gstAndEdu, 100m, 12.36m);
			SetLineDetails(aRInvoice1.Lines[2], chargeCode3, TestObjectCreator.GSTFREE1, 70m, 0m);

			SetLineDetails(aRInvoice2.Lines[0], chargeCode1, TestObjectCreator.GST1, 60m, 6m);
			SetLineDetails(aRInvoice2.Lines[1], chargeCode2, gstAndEdu, 200m, 24.72m);
			SetLineDetails(aRInvoice2.Lines[2], chargeCode2, gstAndEdu, 300m, 37.08m);

			InvoiceBatchHeader invoiceBatch = Factory.NewWithValidTestData(typeof(InvoiceBatchHeader)) as InvoiceBatchHeader;

			aRInvoice1.AH_AH_InvoiceStatement = invoiceBatch.PK;
			aRInvoice2.AH_AH_InvoiceStatement = invoiceBatch.PK;

			invoiceBatch.Line.Add(aRInvoice1);
			invoiceBatch.Line.Add(aRInvoice2);
			return invoiceBatch;
		}

		public void TestDocumentTitle()
		{
			ARInvoice aRInvoice1 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			aRInvoice1.Lines.AddNew();
			SetLineDetails(aRInvoice1.Lines[0], TestObjectCreator.CC1, null, 120m, 12m);

			InvoiceBatchHeader invoiceBatch = Factory.NewWithValidTestData(typeof(InvoiceBatchHeader)) as InvoiceBatchHeader;
			aRInvoice1.AH_AH_InvoiceStatement = invoiceBatch.PK;
			invoiceBatch.Line.Add(aRInvoice1);
			DocARBatchInvoice batchInvoice = DocARBatchInvoice.New(invoiceBatch, Factory);

			AccountingConfigurationRegistry.Instance.NonTaxInvoiceBatchTitle.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TEST NON TAX TITLE");
			AccountingConfigurationRegistry.Instance.TaxInvoiceBatchTitle.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TEST TAX TITLE");

			AssertEquals("TEST NON TAX TITLE", batchInvoice.DocumentTitle);

			aRInvoice1.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			Factory.ClearCachedValue<ZBool>(aRInvoice1.PK.ToStringKey());
			AssertEquals("TEST TAX TITLE", batchInvoice.DocumentTitle);
		}

		public void TestDocumentDetailsTitle()
		{
			ARInvoice aRInvoice1 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			aRInvoice1.Lines.AddNew();
			SetLineDetails(aRInvoice1.Lines[0], TestObjectCreator.CC1, null, 120m, 12m);
			InvoiceBatchHeader invoiceBatch = Factory.NewWithValidTestData(typeof(InvoiceBatchHeader)) as InvoiceBatchHeader;
			aRInvoice1.AH_AH_InvoiceStatement = invoiceBatch.PK;
			invoiceBatch.Line.Add(aRInvoice1);
			DocARBatchInvoice batchInvoice = DocARBatchInvoice.New(invoiceBatch, Factory);

			AccountingConfigurationRegistry.Instance.NonTaxInvoiceBatchDetailsTitle.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TEST NON TAX DETAILS TITLE");
			AccountingConfigurationRegistry.Instance.TaxInvoiceBatchDetailsTitle.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TEST TAX DETAILS TITLE");

			AssertEquals("TEST NON TAX DETAILS TITLE", batchInvoice.DetailsDocumentTitle);

			aRInvoice1.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			Factory.ClearCachedValue<ZBool>(aRInvoice1.PK.ToStringKey());
			AssertEquals("TEST TAX DETAILS TITLE", batchInvoice.DetailsDocumentTitle);
		}

		public void TestInvoices()
		{
			TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(10);
			TestObjectCreator.GSTFREE1.AT_Code = "FREEGST";

			// Setting up 2 invoices, 5 invoice lines
			InvoiceBatchHeader invoiceBatch = SetUpInvoiceBatch();

			DocARBatchInvoice docBatchInvoice = DocARBatchInvoice.New(invoiceBatch, Factory);

			AssertNotNull(docBatchInvoice.Invoices);
			AssertEquals(2, docBatchInvoice.Invoices.Count);
		}

		public void TestInvoiceLines()
		{
			TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(10);
			TestObjectCreator.GSTFREE1.AT_Code = "FREEGST";

			// Setting up 2 invoices, 5 invoice lines 
			InvoiceBatchHeader invoiceBatch = SetUpInvoiceBatch();

			DocARBatchInvoice docBatchInvoice = DocARBatchInvoice.New(invoiceBatch, Factory);

			AssertNotNull(docBatchInvoice.InvoiceLine);
			AssertEquals(5, docBatchInvoice.InvoiceLine.Count);
		}

		[ExpectNoExceptions]
		public void TestBatchInvoiceLineCollectionsNotCreated()
		{
			new DocARBatchInvoiceForTest(SetUpInvoiceBatch(), Factory, false); // this is overriden to throw exception when CreateBatchInvoiceLineCollections() is invoked
		}

		public void TestInvoiceLineCollections()
		{
			string oldInvoiceTransactionNumberPrefix = AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.Value;
			try
			{
				AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ABC");
				InvoiceBatchHeader testBatchHeader = Factory.NewWithValidTestData<InvoiceBatchHeader>();

				InvoicingBase invoiceLine = Factory.NewWithValidTestData<ARInvoice>();
				invoiceLine.AH_OSExTaxAmount = 10m;
				invoiceLine.Lines.AddNew();
				invoiceLine.Lines.AddNew();
				invoiceLine.Lines.AddNew();
				InvoicingBase invoiceLine1 = Factory.NewWithValidTestData<ARInvoice>();
				invoiceLine1.AH_OSExTaxAmount = 15m;
				invoiceLine1.Lines.AddNew();
				invoiceLine1.Lines.AddNew();
				invoiceLine1.Lines.AddNew();
				InvoicingBase invoiceLine2 = Factory.NewWithValidTestData<ARInvoice>();
				invoiceLine2.AH_OSExTaxAmount = 20m;
				invoiceLine2.Lines.AddNew();
				invoiceLine2.Lines.AddNew();
				invoiceLine2.Lines.AddNew();
				InvoicingBase invoiceLine3 = Factory.NewWithValidTestData<ARInvoice>();
				invoiceLine3.AH_OSExTaxAmount = 25m;
				invoiceLine3.Lines.AddNew();
				invoiceLine3.Lines.AddNew();
				invoiceLine3.Lines.AddNew();
				InvoicingBase invoiceLine4 = Factory.NewWithValidTestData<ARInvoice>();
				invoiceLine4.AH_OSExTaxAmount = 30m;
				invoiceLine4.Lines.AddNew();
				invoiceLine4.Lines.AddNew();
				invoiceLine4.Lines.AddNew();

				SetLineDetails(invoiceLine.Lines[0], TestObjectCreator.CC2, TestObjectCreator.GST1, 100m, 10m);
				invoiceLine.Lines[0].AL_AH = Factory.NewWithValidTestData<AccTransactionHeader>().PK;
				invoiceLine.Lines[0].TransactionHeader.AH_TransactionNum = "001";
				invoiceLine.Lines[0].TransactionHeader.AH_TransactionType = "INV";
				SetLineDetails(invoiceLine.Lines[1], TestObjectCreator.CC1, TestObjectCreator.GST1, 100m, 10m);
				invoiceLine.Lines[1].AL_AH = Factory.NewWithValidTestData<AccTransactionHeader>().PK;
				invoiceLine.Lines[1].TransactionHeader.AH_TransactionNum = "003";
				invoiceLine.Lines[1].TransactionHeader.AH_TransactionType = "INV";
				SetLineDetails(invoiceLine.Lines[2], TestObjectCreator.CC1, TestObjectCreator.GST1, 100m, 10m);
				invoiceLine.Lines[2].AL_AH = Factory.NewWithValidTestData<AccTransactionHeader>().PK;
				invoiceLine.Lines[2].TransactionHeader.AH_TransactionNum = "002";
				invoiceLine.Lines[2].TransactionHeader.AH_TransactionType = "INV";

				SetLineDetails(invoiceLine1.Lines[0], TestObjectCreator.CC2, TestObjectCreator.GST1, 110m, 11m);
				invoiceLine1.Lines[0].AL_AH = Factory.NewWithValidTestData<AccTransactionHeader>().PK;
				invoiceLine1.Lines[0].TransactionHeader.AH_TransactionType = "INV";
				invoiceLine1.Lines[0].TransactionHeader.AH_TransactionNum = "101";
				SetLineDetails(invoiceLine1.Lines[1], TestObjectCreator.CC1, TestObjectCreator.GST1, 110m, 11m);
				invoiceLine1.Lines[1].AL_AH = Factory.NewWithValidTestData<AccTransactionHeader>().PK;
				invoiceLine1.Lines[1].TransactionHeader.AH_TransactionType = "INV";
				invoiceLine1.Lines[1].TransactionHeader.AH_TransactionNum = "103";
				SetLineDetails(invoiceLine1.Lines[2], TestObjectCreator.CC1, TestObjectCreator.GST1, 110m, 11m);
				invoiceLine1.Lines[2].AL_AH = Factory.NewWithValidTestData<AccTransactionHeader>().PK;
				invoiceLine1.Lines[2].TransactionHeader.AH_TransactionType = "INV";
				invoiceLine1.Lines[2].TransactionHeader.AH_TransactionNum = "102";

				SetLineDetails(invoiceLine2.Lines[0], TestObjectCreator.CC2, TestObjectCreator.GST1, 120m, 12m);
				invoiceLine2.Lines[0].AL_AH = Factory.NewWithValidTestData<AccTransactionHeader>().PK;
				invoiceLine2.Lines[0].TransactionHeader.AH_TransactionType = "INV";
				invoiceLine2.Lines[0].TransactionHeader.AH_TransactionNum = "201";
				SetLineDetails(invoiceLine2.Lines[1], TestObjectCreator.CC1, TestObjectCreator.GST1, 120m, 12m);
				invoiceLine2.Lines[1].AL_AH = Factory.NewWithValidTestData<AccTransactionHeader>().PK;
				invoiceLine2.Lines[1].TransactionHeader.AH_TransactionType = "INV";
				invoiceLine2.Lines[1].TransactionHeader.AH_TransactionNum = "203";
				SetLineDetails(invoiceLine2.Lines[2], TestObjectCreator.CC1, TestObjectCreator.GST1, 120m, 12m);
				invoiceLine2.Lines[2].AL_AH = Factory.NewWithValidTestData<AccTransactionHeader>().PK;
				invoiceLine2.Lines[2].TransactionHeader.AH_TransactionType = "INV";
				invoiceLine2.Lines[2].TransactionHeader.AH_TransactionNum = "202";

				SetLineDetails(invoiceLine3.Lines[0], TestObjectCreator.CC2, TestObjectCreator.GST1, 130m, 13m);
				invoiceLine3.Lines[0].AL_AH = Factory.NewWithValidTestData<AccTransactionHeader>().PK;
				invoiceLine3.Lines[0].TransactionHeader.AH_TransactionType = "INV";
				invoiceLine3.Lines[0].TransactionHeader.AH_TransactionNum = "301";
				SetLineDetails(invoiceLine3.Lines[1], TestObjectCreator.CC1, TestObjectCreator.GST1, 130m, 13m);
				invoiceLine3.Lines[1].AL_AH = Factory.NewWithValidTestData<AccTransactionHeader>().PK;
				invoiceLine3.Lines[1].TransactionHeader.AH_TransactionType = "INV";
				invoiceLine3.Lines[1].TransactionHeader.AH_TransactionNum = "303";
				SetLineDetails(invoiceLine3.Lines[2], TestObjectCreator.CC1, TestObjectCreator.GST1, 130m, 13m);
				invoiceLine3.Lines[2].AL_AH = Factory.NewWithValidTestData<AccTransactionHeader>().PK;
				invoiceLine3.Lines[2].TransactionHeader.AH_TransactionType = "INV";
				invoiceLine3.Lines[2].TransactionHeader.AH_TransactionNum = "302";

				SetLineDetails(invoiceLine4.Lines[0], TestObjectCreator.CC2, TestObjectCreator.GST1, 140m, 14m);
				invoiceLine4.Lines[0].AL_AH = Factory.NewWithValidTestData<AccTransactionHeader>().PK;
				invoiceLine4.Lines[0].TransactionHeader.AH_TransactionType = "INV";
				invoiceLine4.Lines[0].TransactionHeader.AH_TransactionNum = "401";
				SetLineDetails(invoiceLine4.Lines[1], TestObjectCreator.CC1, TestObjectCreator.GST1, 140m, 14m);
				invoiceLine4.Lines[1].AL_AH = Factory.NewWithValidTestData<AccTransactionHeader>().PK;
				invoiceLine4.Lines[1].TransactionHeader.AH_TransactionType = "INV";
				invoiceLine4.Lines[1].TransactionHeader.AH_TransactionNum = "403";
				SetLineDetails(invoiceLine4.Lines[2], TestObjectCreator.CC1, TestObjectCreator.GST1, 140m, 14m);
				invoiceLine4.Lines[2].AL_AH = Factory.NewWithValidTestData<AccTransactionHeader>().PK;
				invoiceLine4.Lines[2].TransactionHeader.AH_TransactionType = "INV";
				invoiceLine4.Lines[2].TransactionHeader.AH_TransactionNum = "402";

				testBatchHeader.Line.Add(invoiceLine);
				testBatchHeader.Line.Add(invoiceLine1);
				testBatchHeader.Line.Add(invoiceLine2);
				testBatchHeader.Line.Add(invoiceLine3);
				testBatchHeader.Line.Add(invoiceLine4);

				//TPT
				var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
				testJob.Parent = (IJobInvoicingPlugIn)Factory.NewWithValidTestData(JobInvoicingConsumerTypes.LocalCartage.BizoType);
				testBatchHeader.Line[0].AH_JH = testJob.PK;

				//FWD
				var testJob1 = Factory.NewJobWithValidTestDataForTesting<Job>();
				testBatchHeader.Line[1].AH_JH = testJob1.PK;

				ForwardingShipment testShipment1 = Factory.New<ForwardingShipment>();
				testShipment1.JS_IsCFSRegistered = false;
				testJob1.Parent = testShipment1;

				//CFS
				var testJob2 = Factory.NewJobWithValidTestDataForTesting<Job>();
				testBatchHeader.Line[2].AH_JH = testJob2.PK;

				ForwardingShipment testShipment2 = Factory.New<ForwardingShipment>();
				testShipment2.JS_IsCFSRegistered = true;
				testShipment2.JS_IsForwardRegistered = false;
				testJob2.Parent = testShipment2;

				//CUS
				var testJob3 = Factory.NewJobWithValidTestDataForTesting<Job>();
				testJob3.Parent = (IJobInvoicingPlugIn)Factory.NewWithValidTestData(JobInvoicingConsumerTypes.Brokerage.BizoType);
				testBatchHeader.Line[3].AH_JH = testJob3.PK;

				Factory.Save();

				DocARBatchInvoice docBatchInvoice = DocARBatchInvoice.New(testBatchHeader, Factory);

				AssertEquals("TPT invoices", 3, docBatchInvoice.TPTInvoiceLine.Count);
				AssertEquals("TPT invoices", 100m, docBatchInvoice.TPTInvoiceLine[0].OSExTaxAmount);
				AssertEquals("TPT invoices", "INV ABC002", docBatchInvoice.TPTInvoiceLine[0].SubInvoiceRef);
				AssertEquals("TPT invoices", "INV ABC003", docBatchInvoice.TPTInvoiceLine[1].SubInvoiceRef);
				AssertEquals("TPT invoices", "INV ABC001", docBatchInvoice.TPTInvoiceLine[2].SubInvoiceRef);

				AssertEquals("FWD invoices", 3, docBatchInvoice.FWDInvoiceLine.Count);
				AssertEquals("FWD invoices", 110m, docBatchInvoice.FWDInvoiceLine[0].OSExTaxAmount);
				AssertEquals("FWD invoices", "INV ABC102", docBatchInvoice.FWDInvoiceLine[0].SubInvoiceRef);
				AssertEquals("FWD invoices", "INV ABC103", docBatchInvoice.FWDInvoiceLine[1].SubInvoiceRef);
				AssertEquals("FWD invoices", "INV ABC101", docBatchInvoice.FWDInvoiceLine[2].SubInvoiceRef);

				AssertEquals("CFS invoices", 3, docBatchInvoice.CFSInvoiceLine.Count);
				AssertEquals("CFS invoices", 120m, docBatchInvoice.CFSInvoiceLine[0].OSExTaxAmount);
				AssertEquals("CFS invoices", "INV ABC202", docBatchInvoice.CFSInvoiceLine[0].SubInvoiceRef);
				AssertEquals("CFS invoices", "INV ABC203", docBatchInvoice.CFSInvoiceLine[1].SubInvoiceRef);
				AssertEquals("CFS invoices", "INV ABC201", docBatchInvoice.CFSInvoiceLine[2].SubInvoiceRef);

				AssertEquals("CUS invoices", 3, docBatchInvoice.CUSInvoiceLine.Count);
				AssertEquals("CUS invoices", 130m, docBatchInvoice.CUSInvoiceLine[0].OSExTaxAmount);
				AssertEquals("CUS invoices", "INV ABC302", docBatchInvoice.CUSInvoiceLine[0].SubInvoiceRef);
				AssertEquals("CUS invoices", "INV ABC303", docBatchInvoice.CUSInvoiceLine[1].SubInvoiceRef);
				AssertEquals("CUS invoices", "INV ABC301", docBatchInvoice.CUSInvoiceLine[2].SubInvoiceRef);

				AssertEquals("MSC invoices", 3, docBatchInvoice.MSCInvoiceLine.Count);
				AssertEquals("MSC invoices", 140m, docBatchInvoice.MSCInvoiceLine[0].OSExTaxAmount);
				AssertEquals("MSC invoices", "INV ABC402", docBatchInvoice.MSCInvoiceLine[0].SubInvoiceRef);
				AssertEquals("MSC invoices", "INV ABC403", docBatchInvoice.MSCInvoiceLine[1].SubInvoiceRef);
				AssertEquals("MSC invoices", "INV ABC401", docBatchInvoice.MSCInvoiceLine[2].SubInvoiceRef);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldInvoiceTransactionNumberPrefix);
			}
		}

		public new void TestReceiptBankAccount()
		{
			AccBankAccount bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_Code = "ABC";
			bankAccount.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			Invoice.AH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			Invoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			InvoiceBatch.AH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			InvoiceBatch.Header.CompanyData.OB_AB_ARPayToAccount = bankAccount.PK;
			InvoiceBatch.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertNotNull("ARPayToAccount should not be null", Invoice.Header.CompanyData.ARPayToAccount);

			InvoiceWrapper = GetBaseInvoiceWrapper();

			DocBankAccount bankAccountWrapper = InvoiceWrapper.ReceiptBankAccount;

			AssertNull("Bank Account should be null", Invoice.ReceiptBankAccount);
			AssertNotNull("Bank account wrapper should not be null", bankAccountWrapper);
			AssertEquals("Bank Account", bankAccount.AB_Code, bankAccountWrapper.Code);
		}

		public void TestInvoiceLinesLocalDescription()
		{
			bool cachedDefault = AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			string cachedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				Job invoiceJob = Factory.NewJobWithValidTestDataForTesting<Job>();
				invoiceJob.Parent = Factory.New<ForwardingShipment>();

				string defaultDescription = "default description";
				string localDescription = "local language description";

				var chargeCode1 = TestObjectCreator.CC1;
				chargeCode1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

				ARInvoice invoice = Factory.New<ARInvoice>();
				ARInvoiceLine cC1InvoiceLine = (ARInvoiceLine)invoice.Lines.AddNew();
				cC1InvoiceLine.AL_Desc = defaultDescription;
				cC1InvoiceLine.AL_AC = chargeCode1.PK;
				cC1InvoiceLine.ChargeCode.AC_Desc = defaultDescription;
				cC1InvoiceLine.AL_OSExTaxAmount = 100m;

				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
				org.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
				org.CompanyData.SetARTaxApplicable(ZBool.True);

				invoice.AH_JH = invoiceJob.PK;
				invoice.AH_OH = org.PK;

				org.OH_RL_NKClosestPort = "AUSYD";
				AssertEquals("AU", org.Country.RN_Code);
				GlbCompany.CurrentCompany.SetCountry("AU");

				Factory.Save();

				InvoiceBatchHeader batch = Factory.New<InvoiceBatchHeader>();
				batch.AH_OH = org.PK;
				batch.Line.Add(invoice);

				AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				DocARBatchInvoice batchWrapper11 = DocARBatchInvoice.New(batch, Factory);
				AssertEquals("no local language description, registry enabled", defaultDescription, batchWrapper11.InvoiceLineByCharge[0].LineDescription);

				cC1InvoiceLine.ChargeCode.AC_LocalLanguageDescription = localDescription;
				DocARBatchInvoice batchWrapper12 = DocARBatchInvoice.New(batch, Factory);
				AssertEquals("local language description, registry enabled", localDescription, batchWrapper12.InvoiceLineByCharge[0].LineDescription);

				cC1InvoiceLine.ChargeCode.AC_LocalLanguageDescription = string.Empty;
				DocARBatchInvoice batchWrapper13 = DocARBatchInvoice.New(batch, Factory);
				AssertEquals("no local language description, registry disabled", defaultDescription, batchWrapper13.InvoiceLineByCharge[0].LineDescription);

				AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

				DocARBatchInvoice batchWrapper14 = DocARBatchInvoice.New(batch, Factory);
				cC1InvoiceLine.ChargeCode.AC_LocalLanguageDescription = localDescription;
				AssertEquals("no local language description, registry disabled", defaultDescription, batchWrapper14.InvoiceLineByCharge[0].LineDescription);

				org.OH_RL_NKClosestPort = "NZAKL";
				AssertEquals("NZ", org.Country.RN_Code);
				AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				DocARBatchInvoice batchWrapper21 = DocARBatchInvoice.New(batch, Factory);
				AssertEquals("no local language description, registry enabled", defaultDescription, batchWrapper21.InvoiceLineByCharge[0].LineDescription);

				cC1InvoiceLine.ChargeCode.AC_LocalLanguageDescription = localDescription;
				DocARBatchInvoice batchWrapper22 = DocARBatchInvoice.New(batch, Factory);
				AssertEquals("local language description, registry enabled", defaultDescription, batchWrapper22.InvoiceLineByCharge[0].LineDescription);

				AccountingConfigurationRegistry.Instance.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				DocARBatchInvoice batchWrapper23 = DocARBatchInvoice.New(batch, Factory);
				AssertEquals("local language description, registry enabled", localDescription, batchWrapper23.InvoiceLineByCharge[0].LineDescription);

				cC1InvoiceLine.ChargeCode.AC_LocalLanguageDescription = string.Empty;
				DocARBatchInvoice batchWrapper24 = DocARBatchInvoice.New(batch, Factory);
				AssertEquals("no local language description, registry disabled", defaultDescription, batchWrapper24.InvoiceLineByCharge[0].LineDescription);

				AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

				cC1InvoiceLine.ChargeCode.AC_LocalLanguageDescription = localDescription;
				DocARBatchInvoice batchWrapper25 = DocARBatchInvoice.New(batch, Factory);
				AssertEquals("no local language description, registry disabled", defaultDescription, batchWrapper25.InvoiceLineByCharge[0].LineDescription);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, cachedDefault);
				GlbCompany.CurrentCompany.SetCountry(cachedCountry);
			}
		}

		#region Implementation

		void AddInvoiceTypeToCompanyData(OrgCompanyData companyData, ZString jobTypeCode, ZString transportMode, ZString serviceDirection, ZString type, ZString secondaryType)
		{
			var invoiceType = companyData.InvoiceTypes.AddNew();
			invoiceType.PI_Module = jobTypeCode;
			invoiceType.PI_ServiceDirection = serviceDirection;
			invoiceType.PI_TransportMode = transportMode;
			invoiceType.PI_Type = type;
			invoiceType.PI_SecondaryType = secondaryType;
		}

		protected override void AssertRecipientTaxIDCoreForMalaysia(AccTaxRate taxRate1, AccTaxRate taxRate2)
		{
			taxRate1.AT_Code = "GST";
			taxRate1.AT_ExtraTaxRateType = ZString.Empty;
			taxRate2.AT_Code = "GST";
			taxRate2.AT_ExtraTaxRateType = ZString.Empty;
			AssertEquals(ZString.Empty, InvoiceWrapper.RecipientTaxIDHeading);
			AssertEquals(string.Empty, InvoiceWrapper.RecipientTaxIDNumber);

			taxRate1.AT_Code = "GST";
			taxRate1.AT_ExtraTaxRateType = ZString.Empty;
			taxRate2.AT_Code = "SVC";
			taxRate2.AT_ExtraTaxRateType = "SER";
			AssertEquals(ZString.Empty, InvoiceWrapper.RecipientTaxIDHeading);
			AssertEquals(string.Empty, InvoiceWrapper.RecipientTaxIDNumber);

			taxRate1.AT_Code = "SVC";
			taxRate1.AT_ExtraTaxRateType = "SER";
			taxRate2.AT_Code = "SVC";
			taxRate2.AT_ExtraTaxRateType = "SER";
			AssertEquals(ZString.Empty, InvoiceWrapper.RecipientTaxIDHeading);
			AssertEquals(string.Empty, InvoiceWrapper.RecipientTaxIDNumber);

			taxRate1.AT_Code = "XXX";
			taxRate1.AT_ExtraTaxRateType = "TST";
			taxRate2.AT_Code = "XXX";
			taxRate2.AT_ExtraTaxRateType = "TST";
			AssertEquals(ZString.Empty, InvoiceWrapper.RecipientTaxIDHeading);
			AssertEquals(string.Empty, InvoiceWrapper.RecipientTaxIDNumber);
		}

		protected override void SetupCountryForTestingInvoiceTaxMessages()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
		}

		protected override InvoicingBase SetupForTestingInvoiceTaxMessagesCore(string organisationUnloco)
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsDebtor = true;
			org.OH_RL_NKClosestPort = organisationUnloco;

			Invoice = GetWrappedInvoice();
			Invoice.AH_OH = org.PK;
			ARInvoice result = Factory.NewWithValidTestData<ARInvoice>();
			result.AH_OH = org.PK;
			Invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			result.AH_ReceiptBatchNo = "BATCHNUMBER";
			result.AH_GB = GlbBranch.CurrentBranch.PK;
			((InvoiceBatchHeader)Invoice).Line.Add(result);
			Invoice.AH_InvoiceAmount = 2500m;
			Invoice.AH_OutstandingAmount = 2500m;
			Invoice.AH_OSTotal = 2500m;
			Invoice.AH_GSTAmount = 0m;

			return result;
		}

		InvoiceBatchHeader GetTestInvoiceBatch()
		{
			InvoicingBase invoiceLine1 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			invoiceLine1.AH_OSExTaxAmount = 10m;

			InvoicingBase invoiceLine2 = Factory.NewWithValidTestData(typeof(ARCreditNote)) as ARCreditNote;
			invoiceLine2.AH_OSExTaxAmount = 20m;

			Factory.Save();

			InvoiceBatchHeader testHeader = Factory.NewWithValidTestData(typeof(InvoiceBatchHeader)) as InvoiceBatchHeader;

			testHeader.Line.Add(invoiceLine1);
			testHeader.Line.Add(invoiceLine2);

			return testHeader;
		}

		DocARBatchInvoice BatchInvoiceWrapper
		{
			get { return (DocARBatchInvoice)InvoiceWrapper; }
		}

		class DocARBatchInvoiceForTest : DocARBatchInvoice
		{
			public DocARBatchInvoiceForTest(InvoiceBatchHeader batchInvoice, BusinessObjectFactory factoryToWrap, bool createInvoiceLinesCollection)
				: base(batchInvoice, factoryToWrap, createInvoiceLinesCollection)
			{
			}

			protected override void CreateBatchInvoiceLineCollections()
			{
				throw new Exception("This should have not been called.");
			}

			public DocARBatchInvoiceLineCollection GetInvoicesCoreForTest()
			{
				return base.GetInvoicesCore();
			}
		}

		#endregion
	}
}
