using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Accounting.ARAP.Invoicing.Testing
{
	public class SurchargeLineCreatorTest : TestCaseWithFactory
	{
		public void TestAddSurchargeLine()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code, false);
			job.JH_GB = TestObjectCreator.NonCurrentBranch.PK;
			job.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;

			Factory.Save();

			var supplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOX;
			var placeOfSupply = PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry;
			var branch1 = TestObjectCreator.CreateBranch("TB1", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("TB2", GlbCompany.CurrentCompany);

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00000001", TestObjectCreator.AUD, 1M, TestObjectCreator.LocalClient);
			invoice.AH_JH = job.PK;
			var line1 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "desc", 100M);
			line1.AL_PlaceOfSupply = placeOfSupply;
			line1.AL_SupplyType = supplyType;
			line1.AL_GB = branch1.PK;
			var line2 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "desc", 200M);
			line2.AL_PlaceOfSupply = placeOfSupply;
			line2.AL_SupplyType = supplyType;
			line2.AL_GB = branch2.PK;

			AssertEquals("invoice has 2 line before apply surcharge", 2, invoice.Lines.Count);

			var surchargeCalculator = SetupAndAddSurchargeLine(invoice, job.PK);
			surchargeCalculator.Verify(x => x.GetSurchargeCalculationData(It.IsAny<InvoicingBase>()), Times.Once);

			AssertEquals("invoice has 4 lines after apply surcharge calculation", 5, invoice.Lines.Count);
			var newLine1 = invoice.Lines[2];
			var newLine2 = invoice.Lines[3];
			var newLine3 = invoice.Lines[4];

			Assert("invoice's IgnoreValidationSuspended is still true after surcharge.", invoice.IgnoreValidationSuspended);

			AssertNewSurchargeLine(invoice, newLine1, job.PK, TestObjectCreator.CC4.PK, "surchargeCode1", placeOfSupply, supplyType);
			AssertNewSurchargeLine(invoice, newLine2, job.PK, TestObjectCreator.CC4.PK, "surchargeCode2", placeOfSupply, supplyType);
			AssertNewSurchargeLine(invoice, newLine3, ZGuid.Empty, TestObjectCreator.CC5.PK, expectedPlaceOfSupply: placeOfSupply, expectedSupplyType: supplyType);
		}

		public void TestAddSurchargeLine_TaxBranch()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code, false);
			job.JH_GB = TestObjectCreator.NonCurrentBranch.PK;
			job.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;

			Factory.Save();

			var supplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOX;
			var placeOfSupply = PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry;
			var branch1 = TestObjectCreator.CreateBranch("TB1", GlbCompany.CurrentCompany);
			var taxBranch = TestObjectCreator.CreateBranch("TB2", GlbCompany.CurrentCompany);

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00000001", TestObjectCreator.AUD, 1M, TestObjectCreator.LocalClient);
			invoice.AH_JH = job.PK;
			invoice.AH_GB = branch1.PK;
			invoice.AH_GB_TaxBranch = taxBranch.PK;
			var line1 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "desc", 100M);
			line1.AL_PlaceOfSupply = placeOfSupply;
			line1.AL_SupplyType = supplyType;
			line1.AL_GB = branch1.PK;
			line1.AL_GB_TaxBranch = taxBranch.PK;

			AssertEquals("invoice has 1 line before apply surcharge", 1, invoice.Lines.Count);

			var surchargeCalculator = SetupAndAddSurchargeLine(invoice, job.PK);
			surchargeCalculator.Verify(x => x.GetSurchargeCalculationData(It.IsAny<InvoicingBase>()), Times.Once);

			AssertEquals("invoice has 4 lines after apply surcharge calculation", 4, invoice.Lines.Count);

			Assert(invoice.Lines.Cast<InvoicingLineBase>().All(x => x.AL_GB_TaxBranch == taxBranch.PK));

			var charges = Factory.Load<JobCharge>(new ZQuery());
			Assert(charges.All(x => x.JR_GB_SellTaxBranch == taxBranch.PK));
		}

		public void TestAddSurchargeLine_DifferentPlaceOfSupplyAndSupplyType()
		{
			var supplyType1 = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOX;
			var supplyType2 = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOA;
			var placeOfSupply1 = PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry;
			var placeOfSupply2 = ZString.Empty;

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00000001", TestObjectCreator.AUD, 1M, TestObjectCreator.LocalClient);
			var line1 = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "desc", 100M);
			line1.AL_PlaceOfSupply = placeOfSupply1;
			line1.AL_SupplyType = supplyType1;
			var line2 = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "desc", 200M);
			line2.AL_PlaceOfSupply = placeOfSupply2;
			line2.AL_SupplyType = supplyType2;

			AssertEquals("invoice has 2 line before apply surcharge", 2, invoice.Lines.Count);

			var calculationDataList = new List<SurchargeCalculationData>() { new SurchargeCalculationData(invoice.Lines.Select(x => x.PK), ZGuid.Empty, TestObjectCreator.NonAccrualChargeCode.PK, 50, "surchargeCode1") };
			var surchargeCalculator = SetupSurchargeCalculator(calculationDataList);

			((ISurchargeLineCreator)SurchargeLineCreator).AddSurchargeLine(invoice);

			surchargeCalculator.Verify(x => x.GetSurchargeCalculationData(It.IsAny<InvoicingBase>()), Times.Once);

			AssertEquals("invoice has 3 lines after apply surcharge calculation", 3, invoice.Lines.Count);
			var newLine1 = invoice.Lines[2];

			Assert(newLine1.AL_SupplyType.IsEmpty);
			Assert(newLine1.AL_PlaceOfSupply.IsEmpty);
		}

		public void TestSurchargeLineBranch_EnforceBranchLevelPostingEnable()
		{
			using (AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true }))
			{
				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code, false);
				job.JH_GB = TestObjectCreator.NonCurrentBranch.PK;
				job.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;

				Factory.Save();

				var branch1 = TestObjectCreator.CreateBranch("TB1", GlbCompany.CurrentCompany);
				var branch2 = TestObjectCreator.CreateBranch("TB2", GlbCompany.CurrentCompany);

				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00000001", TestObjectCreator.AUD, 1M, TestObjectCreator.LocalClient);
				invoice.AH_GB = GlbBranch.CurrentBranch.PK;
				invoice.AH_JH = job.PK;
				var line1 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "desc", 100M);
				line1.AL_GB = branch1.PK;
				var line2 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "desc", 200M);
				line2.AL_GB = branch2.PK;

				AssertEquals("invoice has 2 line before apply surcharge", 2, invoice.Lines.Count);

				var groupLinePKs = new ZGuid[2] { line1.PK, line2.PK };
				var calculationDataList = new List<SurchargeCalculationData>() { new SurchargeCalculationData(groupLinePKs, job.PK, TestObjectCreator.CC1.PK, 50, "surchargeCode1") };
				var surchargeCalculator = SetupSurchargeCalculator(calculationDataList);

				((ISurchargeLineCreator)SurchargeLineCreator).AddSurchargeLine(invoice);

				surchargeCalculator.Verify(x => x.GetSurchargeCalculationData(It.IsAny<InvoicingBase>()), Times.Once);

				AssertEquals("invoice has 3 lines after apply surcharge calculation", 3, invoice.Lines.Count);
				var newLine1 = invoice.Lines[2];

				var basisLines = invoice.Lines.Where(x => groupLinePKs.Contains(x.PK)).Cast<InvoicingLineBase>();
				AssertEquals(basisLines.First().AL_GB, newLine1.AL_GB);
			}
		}

		public void TestAddSurchargeLine_NONChargeType()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00000001", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M, TestObjectCreator.LocalClient, TestObjectCreator.CC3.PK);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 0m, TestObjectCreator.Agent, 0m);
			invoice.AH_JH = job.PK;

			var calculationDataList = new List<SurchargeCalculationData>() { new SurchargeCalculationData(invoice.Lines.Select(x => x.PK), job.PK, TestObjectCreator.NonAccrualChargeCode.PK, 50, "surchargeCode1") };
			var surchargeCalculator = SetupSurchargeCalculator(calculationDataList);

			AssertEquals("invoice has 1 line before apply surcharge", 1, invoice.Lines.Count);
			AssertEquals("job has 0 charge before apply surcharge", 0, job.Charges.Count);

			((ISurchargeLineCreator)SurchargeLineCreator).AddSurchargeLine(invoice);

			surchargeCalculator.Verify(x => x.GetSurchargeCalculationData(It.IsAny<InvoicingBase>()), Times.Once);
			AssertEquals("invoice has 2 lines after apply surcharge calculation", 2, invoice.Lines.Count);

			var newLine1 = invoice.Lines[1];
			Assert("No job related to this new line.", newLine1.AL_JH.IsEmpty);
			AssertEquals("No new charge added.", 0, job.Charges.Count);
		}

		public void TestAddSurchargeLine_NonJobInvalidCharge()
		{
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00000001", TestObjectCreator.AUD, 1M, TestObjectCreator.LocalClient);
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			var line1 = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "desc", 100M);
			line1.AL_GB = TestObjectCreator.NonCurrentBranch.PK;
			var line2 = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "desc", 200M);
			line2.AL_GB = TestObjectCreator.NonCurrentBranch.PK;

			var groupLinePKs = new ZGuid[2] { line1.PK, line2.PK };
			var calculatedAmount = 50M;
			var surchargeCalculationData1 = new SurchargeCalculationData(groupLinePKs, ZGuid.Empty, TestObjectCreator.ManualJobAccrualChargeCode.PK, calculatedAmount, "surchargeCode1");
			var surchargeCalculationData2 = new SurchargeCalculationData(groupLinePKs, ZGuid.Empty, TestObjectCreator.DSBChargeCode.PK, calculatedAmount, "surchargeCode2");
			var surchargeCalculationData3 = new SurchargeCalculationData(groupLinePKs, ZGuid.Empty, TestObjectCreator.MRG100.PK, calculatedAmount, "surchargeCode3");
			var calculationDataList = new List<SurchargeCalculationData>() { surchargeCalculationData1, surchargeCalculationData2, surchargeCalculationData3 };
			var surchargeCalculator = SetupSurchargeCalculator(calculationDataList);

			AssertEquals("invoice has 2 line before apply surcharge", 2, invoice.Lines.Count);

			((ISurchargeLineCreator)SurchargeLineCreator).AddSurchargeLine(invoice);

			surchargeCalculator.Verify(x => x.GetSurchargeCalculationData(It.IsAny<InvoicingBase>()), Times.Once);

			AssertEquals("invoice has 5 lines after apply surcharge calculation", 5, invoice.Lines.Count);

			var newLine1 = invoice.Lines[2];
			var newLine2 = invoice.Lines[3];
			var newLine3 = invoice.Lines[4];

			Assert(newLine1.HasErrors);
			Assert(newLine2.HasErrors);
			Assert(newLine3.HasErrors);

			AssertEquals(TestObjectCreator.NonCurrentBranch.PK, newLine1.AL_GB);
			AssertEquals(TestObjectCreator.NonCurrentBranch.PK, newLine2.AL_GB);
			AssertEquals(TestObjectCreator.NonCurrentBranch.PK, newLine3.AL_GB);
		}

		public void TestAddSurchargeLine_CreditNote()
		{
			var arCreditNote = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "00000001", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M, TestObjectCreator.LocalClient, TestObjectCreator.CC3.PK);
			AssertEquals("Precondition", 1, arCreditNote.Lines.Count);

			var surchargeCalculator = SetupAndAddSurchargeLine(arCreditNote, ZGuid.Empty);
			surchargeCalculator.Verify(x => x.GetSurchargeCalculationData(It.IsAny<InvoicingBase>()), Times.Never);

			Assert(!arCreditNote.IsInDatabase);
			AssertEquals(TransactionTypes.CreditNote, arCreditNote.AH_TransactionType);
			Assert(!SurchargeLineCreator.ShouldAddSurchargeLine_ExposedForTestOnly(arCreditNote));
			AssertEquals("Still 1 line because we don't apply surcharge to CRD", 1, arCreditNote.Lines.Count);
		}

		public void TestAddSurchargeLine_AmendTransaction()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "test2", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
			var amendTransaction = TestObjectCreator.AmendARTransaction(TransactionTypes.Invoice, invoice).amendTransaction as ARInvoice;
			AssertEquals("Precondition", 1, amendTransaction.Lines.Count);

			var surchargeCalculator = SetupAndAddSurchargeLine(amendTransaction, ZGuid.Empty);
			surchargeCalculator.Verify(x => x.GetSurchargeCalculationData(It.IsAny<InvoicingBase>()), Times.Never);

			Assert(!amendTransaction.IsInDatabase);
			Assert(amendTransaction.IsAmendingTransaction);
			Assert(!SurchargeLineCreator.ShouldAddSurchargeLine_ExposedForTestOnly(amendTransaction));
			AssertEquals("Still 1 line because we don't apply surcharge to amending", 1, amendTransaction.Lines.Count);
		}

		public void TestAddSurchargeLine_ReverseTransaction()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "test2", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
			var reversalTransaction = TestObjectCreator.ReverseTransaction(invoice, out _) as InvoicingBase;
			AssertEquals("Precondition", 1, reversalTransaction.Lines.Count);

			var surchargeCalculator = SetupAndAddSurchargeLine(reversalTransaction, ZGuid.Empty);
			surchargeCalculator.Verify(x => x.GetSurchargeCalculationData(It.IsAny<InvoicingBase>()), Times.Never);

			Assert(!reversalTransaction.IsInDatabase);
			Assert(reversalTransaction.IsReverseTransaction);
			Assert(!SurchargeLineCreator.ShouldAddSurchargeLine_ExposedForTestOnly(reversalTransaction));
			AssertEquals("Still 1 line because we don't apply surcharge to reversing", 1, reversalTransaction.Lines.Count);
		}

		public void TestShouldAddSurchargeLine()
		{
			var apCreditNote = TestObjectCreator.CreateAPCreditNote("111", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "desc");
			Assert(!SurchargeLineCreator.ShouldAddSurchargeLine_ExposedForTestOnly(apCreditNote));
			var arCreditNote = TestObjectCreator.CreateARCreditNote("222", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1M);
			Assert(!SurchargeLineCreator.ShouldAddSurchargeLine_ExposedForTestOnly(arCreditNote));

			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "test1", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
			apInvoice.AH_OH = TestObjectCreator.Creditor1.PK;
			Assert(!SurchargeLineCreator.ShouldAddSurchargeLine_ExposedForTestOnly(apInvoice));

			var invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "test2", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
			invoice1.AH_OH = TestObjectCreator.Debtor.PK;
			Assert(SurchargeLineCreator.ShouldAddSurchargeLine_ExposedForTestOnly(invoice1));

			Factory.Save();

			Assert(invoice1.IsInDatabase);
			Assert(!SurchargeLineCreator.ShouldAddSurchargeLine_ExposedForTestOnly(invoice1));

			var reversalTransaction = TestObjectCreator.ReverseTransaction(invoice1, out _) as InvoicingBase;
			Assert(!reversalTransaction.IsInDatabase);
			Assert(reversalTransaction.IsReverseTransaction);
			Assert(!SurchargeLineCreator.ShouldAddSurchargeLine_ExposedForTestOnly(reversalTransaction));

			var invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "test2", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
			Factory.Save();

			var amendTransaction = TestObjectCreator.AmendARTransaction(TransactionTypes.Invoice, invoice2).amendTransaction as ARInvoice;
			Assert(!amendTransaction.IsInDatabase);
			Assert(amendTransaction.IsAmendingTransaction);
			Assert(!SurchargeLineCreator.ShouldAddSurchargeLine_ExposedForTestOnly(amendTransaction));

			var invoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "test2", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
			invoice3.AH_OH = TestObjectCreator.Debtor.PK;
			Assert(SurchargeLineCreator.ShouldAddSurchargeLine_ExposedForTestOnly(invoice3));

			invoice3.AH_OH = ZGuid.Invalid;
			Assert(!SurchargeLineCreator.ShouldAddSurchargeLine_ExposedForTestOnly(invoice3));
		}

		public void TestSurchargeLineContext()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00000001", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M, TestObjectCreator.LocalClient, TestObjectCreator.CC3.PK);

			var calculationDataList = new List<SurchargeCalculationData>() { new SurchargeCalculationData(new ZGuid[1] { invoice.Lines[0].PK }, ZGuid.Empty, TestObjectCreator.ManualJobAccrualChargeCode.PK, 50, "surchargeCode1") };
			var surchargeCalculator = SetupSurchargeCalculator(calculationDataList);

			AssertEquals("invoice has 1 line before apply surcharge", 1, invoice.Lines.Count);

			((ISurchargeLineCreator)SurchargeLineCreator).AddSurchargeLine(invoice);

			surchargeCalculator.Verify(x => x.GetSurchargeCalculationData(It.IsAny<InvoicingBase>()), Times.Once);

			AssertEquals("invoice has 2 lines after apply surcharge calculation", 2, invoice.Lines.Count);

			var newLine1 = invoice.Lines[1];
			Assert(newLine1.HasContext(BusinessContext.SurchargeLine));
		}

		public void TestAddSurchargeLine_TaxRateAndMessage()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;

			var taxOverride = TestObjectCreator.CreateTaxOverride(TestObjectCreator.CC4, TestObjectCreator.GST2.PK, vatClassPK: TestObjectCreator.TaxMsg2.PK);
			taxOverride.AO_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
			Factory.Save();

			AssertNotEquals("PreCondition charge's default GST should not equal to taxOverride.AO_AT", taxOverride.AO_AT, TestObjectCreator.CC4.AC_AT_GSTRate);

			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code, false);

			Factory.Save();

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00000001", TestObjectCreator.AUD, 1M, TestObjectCreator.Debtor);
			invoice.AH_JH = job.PK;
			var line1 = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "desc", 100M);
			line1.AL_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;

			AssertEquals("invoice has 1 line before apply surcharge", 1, invoice.Lines.Count);

			var calculationDataList = new [] { new SurchargeCalculationData(invoice.Lines.Select(x => x.PK), job.PK, TestObjectCreator.CC4.PK, 50, "surchargeCode1") };
			var surchargeCalculator = SetupSurchargeCalculator(calculationDataList);

			((ISurchargeLineCreator)SurchargeLineCreator).AddSurchargeLine(invoice);

			AssertEquals("invoice has 2 lines after apply surcharge calculation", 2, invoice.Lines.Count);

			var newLine1 = invoice.Lines.Cast<InvoicingLineBase>().First(x => x.AL_AC == TestObjectCreator.CC4.PK);
			var jobCharge1 = newLine1.RelatedJobCharge;
			AssertEquals(TestObjectCreator.GST2.PK, jobCharge1.JR_AT_SellGSTRate);
			AssertEquals(TestObjectCreator.TaxMsg2.PK, jobCharge1.JR_A9_SellVATClass);
			AssertEquals(TestObjectCreator.GST2.PK, newLine1.AL_AT);
			AssertEquals(TestObjectCreator.TaxMsg2.PK, newLine1.AL_A9_VATClass);
		}

		public void TestCreateChargeFromSurchargeLine_SuspendJobChargeCalculationContext()
		{
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			service.ClearServiceCache();

			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			newCompany.GC_Code = "DCN";
			newCompany.GC_IsReciprocal = true;
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "BJN";
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, newBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				TestObjectCreator.CreateExchangeRate(TestObjectCreator.AED, "BUY", 0.937476611m, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(10));
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalSellAmtNotEqualRelatedLineAmount);
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.JobChargeOSSellAmtNotEqualRelatedLineOSAmount);

				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code, false);

				Factory.Save();

				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00000001", TestObjectCreator.AED, 0.937476611M, TestObjectCreator.Debtor);
				invoice.AH_JH = job.PK;
				invoice.AH_TransactionCategory = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
				var line = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC3, TestObjectCreator.AED, 0.937476611M, "desc", 14963.99M);

				AssertEquals("Precondition: AL_ExchangeRate", 0.937476611M, line.AL_ExchangeRate);
				AssertEquals("Precondition: AL_LineAmount", 14028.39M, line.AL_LineAmount);
				AssertEquals("Precondition: AH_ExchangeRate", 0.937477M, invoice.AH_ExchangeRate);

				var calculationDataList = new[] { new SurchargeCalculationData(invoice.Lines.Select(x => x.PK), job.PK, TestObjectCreator.CC4.PK, 14963.33333M, "surchargeCode1") };
				var surchargeCalculator = SetupSurchargeCalculator(calculationDataList);
				invoice.AH_ExchangeRate = 0.93733331M;
				((ISurchargeLineCreator)SurchargeLineCreator).AddSurchargeLine(invoice);
				var newLine = invoice.Lines[1];
				var jobCharge = newLine.RelatedJobCharge;

				AssertEquals("New Line AL_ExchangeRate", 0.937333M, newLine.AL_ExchangeRate);
				AssertEquals("New Line AL_LineAmount", 14025.63M, newLine.AL_LineAmount);

				AssertContains("Should not report any critical validation error: JobChargeOSSellAmtNotEqualRelatedLineOSAmount", "There is no data collected for this PK", service.GetInfo(jobCharge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSSellAmtNotEqualRelatedLineOSAmount));
				AssertContains("Should not report any critical validation error: JobChargeLocalSellAmtNotEqualRelatedLineAmount", "There is no data collected for this PK", service.GetInfo(jobCharge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalSellAmtNotEqualRelatedLineAmount));
				AssertEquals("JR_LocalSellAmt should be the same as AL_LineAmount", newLine.AL_LineAmount, jobCharge.JR_LocalSellAmt);
				AssertEquals("JR_OSSellAmt + JR_OSSellGSTAmt_Calc should be the same as AL_OSAmount ", newLine.AL_OSAmount, jobCharge.JR_OSSellAmt + jobCharge.JR_OSSellGSTAmt_Calc);
				//Job OS Sell Ex Rate is different from Line Exchange Rate because of rounding
				AssertEquals("JR_OSSellExRate", 0.937333M, jobCharge.JR_OSSellExRate);
			}
		}

		public void TestAddSurchargeLine_HasSkipTaxIdAndTaxMessageMappingValidationContext()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00000001", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M, TestObjectCreator.LocalClient, TestObjectCreator.CC3.PK);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 0m, TestObjectCreator.Agent, 0m);
			invoice.AH_JH = job.PK;

			var calculationDataList = new List<SurchargeCalculationData>() { new SurchargeCalculationData(invoice.Lines.Select(x => x.PK), job.PK, TestObjectCreator.CC4.PK, 50, "surchargeCode1") };
			var surchargeCalculator = SetupSurchargeCalculator(calculationDataList);

			((ISurchargeLineCreator)SurchargeLineCreator).AddSurchargeLine(invoice);
			var chargeLines = Factory.Load(typeof(Charge), new ZQuery(JobChargeSchema.JR_AL_ARLine, SQLComparisonOperator.Equal, invoice.Lines[1].PK));
			var taxMappingHelper = new TaxIdAndTaxMessageMappingHelper();

			AssertEquals(true, ((Charge)chargeLines[0]).ARLine.HasContext(BusinessContext.SkipTaxIdAndTaxMessageMappingValidation));
			AssertEquals(true, invoice.Lines[1].HasContext(BusinessContext.SkipTaxIdAndTaxMessageMappingValidation));
		}

		public void TestAddSurchargeLine_ShouldSetOneAsLocalCurrency()
		{
			var exRate = Factory.New<RefExchangeRate>();
			exRate.RE_GC = GlbCompany.CurrentCompany.PK;
			exRate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			exRate.RE_RX_NKExCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			exRate.RE_StartDate = new ZDateTime(ZDateTime.Today.Year, 1, 1);
			exRate.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.Year, 12, 31);
			exRate.RE_SellRate = 0.8m;
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00000001", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M, TestObjectCreator.LocalClient, TestObjectCreator.CC3.PK);
			invoice.AH_PostDate = ZDateTime.Today;
			invoice.AH_InvoiceDate = ZDateTime.Today;
			invoice.AH_PostedToEFT = true;
			invoice.UseJobExchangeRate = true;
			invoice.AH_TransactionCategory = "DCD";

			Assert(InvoiceTypeCalculationProvider.IsDeferredInvoiceType(invoice.AH_TransactionCategory));

			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 0m, TestObjectCreator.Agent, 0m);
			invoice.AH_JH = job.PK;

			var calculationDataList = new List<SurchargeCalculationData>() { new SurchargeCalculationData(invoice.Lines.Select(x => x.PK), job.PK, TestObjectCreator.CC4.PK, 50, "surchargeCode1") };
			var surchargeCalculator = SetupSurchargeCalculator(calculationDataList);
			((ISurchargeLineCreator)SurchargeLineCreator).AddSurchargeLine(invoice);

			AssertEquals("Local currency exchange rate should equal 1", 1.0m, invoice.Lines[0].AL_ExchangeRate);
		}

		Mock<ISurchargeCalculator> SetupAndAddSurchargeLine(InvoicingBase invoice, ZGuid jobPK)
		{
			var calculatedAmount = 50M;
			var lineCount = invoice.Lines.Count;
			var groupLinePKs = new ZGuid[lineCount];

			for (int i = 0; i < lineCount; i++)
			{
				groupLinePKs[i] = invoice.Lines[i].PK;
			}

			var surchargeCalculationData1 = new SurchargeCalculationData(groupLinePKs, jobPK, TestObjectCreator.CC4.PK, calculatedAmount, "surchargeCode1");
			var surchargeCalculationData2 = new SurchargeCalculationData(groupLinePKs, jobPK, TestObjectCreator.CC4.PK, calculatedAmount, "surchargeCode2");
			var surchargeCalculationData3 = new SurchargeCalculationData(groupLinePKs, ZGuid.Empty, TestObjectCreator.CC5.PK, calculatedAmount, "surchargeCode3");
			var calculationDataList = new List<SurchargeCalculationData>() { surchargeCalculationData1, surchargeCalculationData2, surchargeCalculationData3 };
			var surchargeCalculator = SetupSurchargeCalculator(calculationDataList);

			invoice.IgnoreValidationSuspended = true;
			Assert("Precondition: invoice's IgnoreValidationSuspended is true.", invoice.IgnoreValidationSuspended);

			((ISurchargeLineCreator)SurchargeLineCreator).AddSurchargeLine(invoice);

			return surchargeCalculator;
		}

		void AssertNewSurchargeLine(InvoicingBase invoice, InvoicingLineBase newTransactionLine, ZGuid jobPK, ZGuid chargePK, string expectedSurchargeCode = "", string expectedPlaceOfSupply = "", string expectedSupplyType = "")
		{
			if (jobPK.IsEmpty)
			{
				AssertEquals(GlbBranch.CurrentBranch.PK, newTransactionLine.AL_GB);
				AssertEquals(GlbDepartment.CurrentDepartment.PK, newTransactionLine.AL_GE);
			}
			else
			{
				AssertEquals(jobPK, newTransactionLine.AL_JH);
				AssertEquals(TestObjectCreator.NonCurrentBranch.PK, newTransactionLine.AL_GB);
				AssertEquals(TestObjectCreator.NonCurrentDepartment.PK, newTransactionLine.AL_GE);
			}

			AssertEquals(chargePK, newTransactionLine.GenericCharge);
			AssertEquals(invoice.AH_OH, newTransactionLine.AL_OH);

			AssertEquals(AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOX, newTransactionLine.AL_SupplyType);
			if (!string.IsNullOrEmpty(expectedPlaceOfSupply))
			{
				AssertEquals(expectedPlaceOfSupply, newTransactionLine.AL_PlaceOfSupply);
			}

			AssertEquals(expectedSupplyType, newTransactionLine.AL_SupplyType);
			AssertEquals(invoice.AH_RX_NKTransactionCurrency, newTransactionLine.AL_RX_NKTransactionCurrency);
			AssertEquals(invoice.AH_ExchangeRate, newTransactionLine.AL_ExchangeRate);
			AssertEquals(50M, newTransactionLine.AL_OSExTaxAmount);
			if (!string.IsNullOrEmpty(expectedSurchargeCode))
			{
				Assert(newTransactionLine.AL_Desc.Contains($" ({expectedSurchargeCode})"));
			}
			Assert(!newTransactionLine.AL_AT.IsEmpty);
			AssertEquals(ZDate.Today, newTransactionLine.AL_TaxDate);

			var newCharge = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_AL_ARLine, newTransactionLine.PK)).FirstOrDefault();
			var hasNewCharge = newCharge != null;
			AssertEquals(!jobPK.IsEmpty, hasNewCharge);

			if (hasNewCharge)
			{
				AssertEquals(newTransactionLine.AL_AC, newCharge.JR_AC);
				AssertEquals(newTransactionLine.AL_GB, newCharge.JR_GB);
				AssertEquals(newTransactionLine.AL_GE, newCharge.JR_GE);
				AssertEquals(newTransactionLine.AL_Desc, newCharge.JR_Desc);

				AssertEquals(newTransactionLine.AL_SupplyType, newCharge.JR_SellSupplyType);
				AssertEquals(newTransactionLine.AL_PlaceOfSupply, newCharge.JR_SellPlaceOfSupply);
				AssertEquals(newTransactionLine.AL_AT, newCharge.JR_AT_SellGSTRate);
				AssertEquals(ZDate.Today, newCharge.JR_SellTaxDate);
				AssertEquals(newTransactionLine.AL_RX_NKTransactionCurrency, newCharge.JR_RX_NKSellCurrency);
				AssertEquals(invoice.AH_OH, newCharge.JR_OH_SellAccount);
				AssertEquals(invoice.AH_TransactionCategory, newCharge.JR_InvoiceType);
			}
		}

		Mock<ISurchargeCalculator> SetupSurchargeCalculator(IEnumerable<SurchargeCalculationData> calculationDataList)
		{
			var surchargeCalculator = new Mock<ISurchargeCalculator>(MockBehavior.Strict);
			surchargeCalculator.Setup(x => x.GetSurchargeCalculationData(It.IsAny<InvoicingBase>())).Returns(calculationDataList);
			ObjectFactory.Substitute(surchargeCalculator.Object);

			return surchargeCalculator;
		}

		#region Implementation

		TestObjectCreator TestObjectCreator;
		SurchargeLineCreator SurchargeLineCreator;

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator = new TestObjectCreator(Factory);
			SurchargeLineCreator = new SurchargeLineCreator();
		}

		#endregion
	}
}
