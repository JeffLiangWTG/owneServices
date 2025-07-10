using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Business.ARAP.Invoicing.ConsolCostImporter;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.Importer
{
	[TestedType(typeof(ConsolCostImporter))]
	public class ConsolCostImporterTest : TestCaseWithFactory
	{
		public void TestDependencyInjection()
		{
			AssertNotNull(ConsolCostImporter);
			AssertType<ConsolCostImporter>(ConsolCostImporter);
		}

		public void TestImportCostsToCollection_NotSyncParentInfo()
		{
			AssertImportCostsToCollection(false);
		}

		public void TestImportCostsToCollection_SyncParentInfo()
		{
			AssertImportCostsToCollection(true);
		}

		void AssertImportCostsToCollection(bool isSyncParentInfo)
		{
			var invoice = Factory.New<APInvoice>();

			var mockedImporter = new Mock<IConsolCostImporter>();
			var importer = CreateSelfMockingImporter_TestOnly(mockedImporter.Object);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "KRSEL", "C00001");
			var consolCosts = new[] {
				TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.USD, 1.5m, 150m, TestObjectCreator.AALSHI)
				,TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC2, TestObjectCreator.AUD, 1m, 200m, TestObjectCreator.AALSHI)
			};

			AssertEquals("PreCondition", 0, invoice.ConsolCosting.ConsolCosts.Count);
			mockedImporter
				.Setup(x => x.ImportCostIntoCosting(
					invoice
					, It.Is<JobConsolCost>(costToImport => consolCosts.Contains(costToImport))
					, It.IsAny<JobConsolCost>()
					, isSyncParentInfo))
				.Callback<InvoicingBase, JobConsolCost, JobConsolCost, bool>((_, costToImport, targetConsolCost, _) => {
					AssertEquals("The passing target consol cost is created by APInvoiceConsolCostCollection.", targetConsolCost, invoice.ConsolCosting.ConsolCosts.Last());
					targetConsolCost.PropertyValueChanged += (_, _) => Assert("After assigning target consol cost to this method, we should not change value anymore.", false);
				});
			importer.ImportCostsToCollection(invoice.ConsolCosting.ConsolCosts, consolCosts, isSyncParentInfo);
			AssertEquals(2, invoice.ConsolCosting.ConsolCosts.Count);

			mockedImporter.Verify(
				x => x.ImportCostIntoCosting(It.IsAny<InvoicingBase>(), It.IsAny<JobConsolCost>(), It.IsAny<JobConsolCost>(), It.IsAny<bool>())
				, Times.Exactly(2));
			mockedImporter.Verify(
				x => x.ImportCostIntoCosting(invoice, consolCosts[0], invoice.ConsolCosting.ConsolCosts[0], It.IsAny<bool>())
				, Times.Exactly(1));
			mockedImporter.Verify(
				x => x.ImportCostIntoCosting(invoice, consolCosts[1], invoice.ConsolCosting.ConsolCosts[1], It.IsAny<bool>())
				, Times.Exactly(1));
		}
		
		public void TestImportCostIntoCosting_NotSyncParentInfo()
		{
			var consol = Factory.New<ForwardingConsol>();
			var currentCompanyCost = consol.GetApportionments().CostsCollection.TryAddNew();
			AssertNotEquals(ZGuid.Empty, currentCompanyCost.E6_ParentID);
			AssertNotEquals(ZString.Empty, currentCompanyCost.E6_ParentTableCode);

			var invoice = Factory.New<APInvoice>();

			var originatingCost = invoice.ConsolCosting.ConsolCosts.AddNew();
			AssertEquals(ZGuid.Empty, originatingCost.E6_ParentID);
			AssertEquals(ZString.Empty, originatingCost.E6_ParentTableCode);

			ConsolCostImporter.ImportCostIntoCosting(invoice, currentCompanyCost, originatingCost, false);
			AssertEquals(ZGuid.Empty, originatingCost.E6_ParentID);
			AssertEquals(ZString.Empty, originatingCost.E6_ParentTableCode);
		}

		[TestDate(2024, 11, 13)]
		public void TestImportCostIntoCosting_SyncParentInfo()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.IDR, Core.Constants.ExchangeRateTypes.Code.BuyRate, 0.6m, new ZDateTime(2024, 11, 13), new ZDateTime(2024, 11, 13));
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			var currentCompanyCost = consol.GetApportionments().CostsCollection.TryAddNew();
			currentCompanyCost.E6_RX_NKCurrency = TestObjectCreator.IDR.Code;
			currentCompanyCost.E6_ExchangeRate = 2m;
			AssertNotEquals(ZGuid.Empty, currentCompanyCost.E6_ParentID);
			AssertNotEquals(ZString.Empty, currentCompanyCost.E6_ParentTableCode);

			var invoice = Factory.New<APInvoice>();

			var originatingCost = invoice.ConsolCosting.ConsolCosts.AddNew();
			originatingCost.E6_RX_NKCurrency = TestObjectCreator.IDR.Code;
			AssertEquals(0.6m, originatingCost.E6_ExchangeRate);
			AssertEquals(ZGuid.Empty, originatingCost.E6_ParentID);
			AssertEquals(ZString.Empty, originatingCost.E6_ParentTableCode);

			ConsolCostImporter.ImportCostIntoCosting(invoice, currentCompanyCost, originatingCost, true);
			AssertEquals(currentCompanyCost.E6_ParentID, originatingCost.E6_ParentID);
			AssertEquals(currentCompanyCost.E6_ParentTableCode, originatingCost.E6_ParentTableCode);
			AssertEquals(2m, originatingCost.E6_ExchangeRate);
		}

		public void TestImportCostIntoCosting_GovtChargeCode()
		{
			var consol = Factory.New<ForwardingConsol>();
			var currentCompanyCost = consol.GetApportionments().CostsCollection.TryAddNew();
			currentCompanyCost.E6_GC = GlbCompany.CurrentCompany.PK;
			currentCompanyCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			currentCompanyCost.E6_RX_NKCurrency = TestObjectCreator.AUD.RX_Code;
			currentCompanyCost.E6_CostGovtChargeCode = "S0023A";
			currentCompanyCost.E6_SellGovtChargeCode = "S0023B";

			var invoice = Factory.New<APInvoice>();

			var originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			originatingCost.E6_CostGovtChargeCode = "AAA";
			originatingCost.E6_SellGovtChargeCode = "AAA";

			ConsolCostImporter.ImportCostIntoCosting(invoice, currentCompanyCost, originatingCost, false);
			AssertEquals(((ZString)"S0023A", (ZString)"S0023B"), (originatingCost.E6_CostGovtChargeCode, originatingCost.E6_SellGovtChargeCode));
		}

		public void TestImportCostIntoCosting_ForeignCurrency()
		{
			AssertEquals(TestObjectCreator.AUD.Code, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			var consol = TestObjectCreator.CreateConsol("AUSYD", "KRSEL", "C00001");

			var invoice = Factory.New<APInvoice>();
			invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.Code;
			var originatingCostWithForeignCur = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			originatingCostWithForeignCur.E6_RX_NKCurrency = TestObjectCreator.IDR.Code;
			originatingCostWithForeignCur.E6_ExchangeRate = 2m;
			originatingCostWithForeignCur.E6_LocalCostAmount = 50m;
			originatingCostWithForeignCur.E6_OSCostAmount = 100m;

			var costWithForeignCurrency = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.USD, 1.5m, 150m, TestObjectCreator.AALSHI);
			ConsolCostImporter.ImportCostIntoCosting(invoice, costWithForeignCurrency, originatingCostWithForeignCur, false);
			AssertEquals(TestObjectCreator.USD.Code, originatingCostWithForeignCur.E6_RX_NKCurrency);
			AssertEquals(1.5m, originatingCostWithForeignCur.E6_ExchangeRate);
			AssertEquals(100m, originatingCostWithForeignCur.E6_LocalCostAmount);
			AssertEquals(150m, originatingCostWithForeignCur.E6_OSCostAmount);

			var originatingCostWithLocalCur = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			originatingCostWithLocalCur.E6_RX_NKCurrency = TestObjectCreator.IDR.Code;
			originatingCostWithLocalCur.E6_ExchangeRate = 2m;
			originatingCostWithLocalCur.E6_LocalCostAmount = 50m;
			originatingCostWithLocalCur.E6_OSCostAmount = 100m;

			var costWithLocalCurrency = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC2, TestObjectCreator.AUD, 1m, 200m, TestObjectCreator.AALSHI);
			ConsolCostImporter.ImportCostIntoCosting(invoice, costWithLocalCurrency, originatingCostWithLocalCur, false);
			AssertEquals(TestObjectCreator.AUD.Code, originatingCostWithLocalCur.E6_RX_NKCurrency);
			AssertEquals(1m, originatingCostWithLocalCur.E6_ExchangeRate);
			AssertEquals(200m, originatingCostWithLocalCur.E6_LocalCostAmount);
			AssertEquals(200m, originatingCostWithLocalCur.E6_OSCostAmount);

			var invoiceWithSetContext = Factory.New<APInvoice>();
			invoiceWithSetContext.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.Code;
			invoiceWithSetContext.Factory.SetContext(BusinessContext.LegacyXMLImport);

			var originatingCostWithForeignCur_Legacy = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			originatingCostWithForeignCur_Legacy.E6_RX_NKCurrency = TestObjectCreator.AUD.Code;
			originatingCostWithForeignCur_Legacy.E6_ExchangeRate = 1m;

			ConsolCostImporter.ImportCostIntoCosting(invoice, costWithForeignCurrency, originatingCostWithForeignCur_Legacy, false);
			AssertEquals(TestObjectCreator.AUD.Code, originatingCostWithForeignCur_Legacy.E6_RX_NKCurrency);
			AssertEquals(1m, originatingCostWithForeignCur_Legacy.E6_ExchangeRate);

			var originatingCostWithLocalCur_Legacy = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			originatingCostWithLocalCur_Legacy.E6_RX_NKCurrency = TestObjectCreator.IDR.Code;
			originatingCostWithLocalCur_Legacy.E6_ExchangeRate = 2m;

			ConsolCostImporter.ImportCostIntoCosting(invoice, costWithLocalCurrency, originatingCostWithLocalCur_Legacy, false);
			AssertEquals(TestObjectCreator.AUD.Code, originatingCostWithLocalCur.E6_RX_NKCurrency);
			AssertEquals(1m, originatingCostWithLocalCur.E6_ExchangeRate);
		}

		public void TestImportCostIntoCosting_WithCreditorInvoicedExchangeRateProvider()
		{
			PrepareCreditorInvoicedExchangeRateTestData(out APInvoice invoice, out JobConsolCost sourceConsolCost, out JobConsolCost targetConsolCost, out AccDraftInvoiceHeader draftInvoice);

			Factory.ServiceContainer.AddService(new CreditorInvoicedExchangeRateProvider(draftInvoice));

			ConsolCostImporter.ImportCostIntoCosting(invoice, sourceConsolCost, targetConsolCost, false);

			AssertEquals(TestObjectCreator.USD.Code, targetConsolCost.E6_RX_NKCurrency);
			AssertEquals(5m, targetConsolCost.E6_ExchangeRate);
			AssertEquals(200m, targetConsolCost.E6_OSCostAmount);
			AssertEquals(40m, targetConsolCost.E6_LocalCostAmount);
		}

		public void TestImportCostIntoCosting_WithoutCreditorInvoicedExchangeRateProvider()
		{
			PrepareCreditorInvoicedExchangeRateTestData(out APInvoice invoice, out JobConsolCost sourceConsolCost, out JobConsolCost targetConsolCost, out AccDraftInvoiceHeader draftInvoice);

			ConsolCostImporter.ImportCostIntoCosting(invoice, sourceConsolCost, targetConsolCost, false);

			AssertEquals(TestObjectCreator.USD.Code, targetConsolCost.E6_RX_NKCurrency);
			AssertEquals(2m, targetConsolCost.E6_ExchangeRate);
			AssertEquals(200m, targetConsolCost.E6_OSCostAmount);
			AssertEquals(100m, targetConsolCost.E6_LocalCostAmount);
		}

		void PrepareCreditorInvoicedExchangeRateTestData(out APInvoice invoice, out JobConsolCost sourceConsolCost, out JobConsolCost targetConsolCost, out AccDraftInvoiceHeader draftInvoice)
		{
			var consol = TestObjectCreator.CreateConsol();

			invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.Code;

			targetConsolCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			targetConsolCost.E6_RX_NKCurrency = TestObjectCreator.AUD.Code;
			targetConsolCost.E6_ExchangeRate = 1m;
			targetConsolCost.E6_LocalCostAmount = 100m;
			targetConsolCost.E6_OSCostAmount = 100m;

			sourceConsolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC2, TestObjectCreator.USD, 2m, 200m, TestObjectCreator.AALSHI);

			draftInvoice = TestObjectCreator.CreateDraftInvoice("00001001", "R00001001", TestObjectCreator.Creditor1.PK, 100m, 0m, TestObjectCreator.AUD.Code);

			TestObjectCreator.AddDraftInvocieExchangeRate(draftInvoice, TestObjectCreator.USD, 5m);
			TestObjectCreator.AddDraftInvocieExchangeRate(draftInvoice, TestObjectCreator.GBP, 10m);
		}

		public void TestChargeTaxIdIsNotResetIfJobBranchDifferentToConsolCost()
		{
			var chargeCode = TestObjectCreator.CC1;
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var shipment1Job = Job.CreateWithMutex(Factory, shipment1);
			shipment1Job.PlugInData = shipment1;
			shipment1Job.JH_JobNum = "S00003333";
			shipment1Job.JH_GB = TestObjectCreator.NonCurrentBranch.PK;
			shipment1Job.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;

			var costToImport = consol.GetApportionments().CostsCollection.TryAddNew();
			costToImport.E6_GC = GlbCompany.CurrentCompany.PK;
			costToImport.E6_AC_ChargeCode = chargeCode.PK;
			costToImport.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;
			costToImport.E6_RX_NKCurrency = TestObjectCreator.AUD.RX_Code;
			costToImport.E6_ExchangeRate = 1m;
			costToImport.E6_ApportionmentMethod = "SHP";
			costToImport.E6_OSCostAmount = 77.1M;
			costToImport.E6_LocalCostAmount = 77.1M;
			costToImport.E6_AT_TaxRate = TestObjectCreator.GSTFREE1.PK;

			var charge1 = costToImport.ApportionmentCharges.Count > 0 ? costToImport.ApportionmentCharges[0] : costToImport.ApportionmentCharges.AddNew();
			charge1.JR_OSCostAmt = 77.1M;
			charge1.JR_AC = chargeCode.PK;
			charge1.JR_JH = shipment1Job.PK;
			charge1.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			charge1.JR_AT_CostGSTRate = TestObjectCreator.GSTFREE1.PK;
			Factory.Save();

			var invoice = Factory.New<APInvoice>();
			invoice.ExchangeRate.Currency = "USD";
			invoice.ExchangeRate.Rate = 0.81M;
			var originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			originatingCost.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;

			ConsolCostImporter.ImportCostIntoCosting(invoice, costToImport, originatingCost, false);
			AssertEquals("apportion charge's tax id should not be changed", TestObjectCreator.GSTFREE1.PK, invoice.ConsolCosting.ConsolCosts[0].ApportionmentCharges[0].JR_AT_CostGSTRate);
		}

		public void TestImportCostIntoCostingDoesNotModifyApportionmentChargesOfSourceCost()
		{
			var chargeCode = TestObjectCreator.CC1;
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();

			var costToImport = consol.GetApportionments().CostsCollection.TryAddNew();
			costToImport.E6_GC = GlbCompany.CurrentCompany.PK;
			costToImport.E6_AC_ChargeCode = chargeCode.PK;
			costToImport.E6_RX_NKCurrency = TestObjectCreator.AUD.RX_Code;
			costToImport.E6_ExchangeRate = 1m;
			costToImport.E6_ApportionmentMethod = "SHP";
			costToImport.E6_OSCostAmount = 77.1M;
			costToImport.E6_LocalCostAmount = 77.1M;

			var charge1 = costToImport.ApportionmentCharges.AddNew();
			charge1.JR_OSCostAmt = 38.55M;
			charge1.JR_AC = chargeCode.PK;
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			charge1.JR_JH = job1.PK;
			var charge2 = costToImport.ApportionmentCharges.AddNew();
			charge2.JR_OSCostAmt = 38.55M;
			charge2.JR_AC = chargeCode.PK;
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			charge2.JR_JH = job2.PK;
			AssertEquals("Precondition - CostToImport should contain 2 apportionment charges", 2, costToImport.ApportionmentCharges.Count);

			var job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			Factory.Save();

			var invoice = Factory.New<APInvoice>();
			invoice.ExchangeRate.Currency = "USD";
			invoice.ExchangeRate.Rate = 0.81M;
			var originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			var originatingCharge1 = originatingCost.ApportionmentCharges.AddNew();
			originatingCharge1.JR_JH = job3.PK;
			AssertEquals("PreCondition, targetConsolCost has additional apportionment charge.", 1, originatingCost.ApportionmentCharges.Count);

			ConsolCostImporter.ImportCostIntoCosting(invoice, costToImport, originatingCost, false);
			AssertEquals("costToImport should still contain 2 apportionment charges", 2, costToImport.ApportionmentCharges.Count);
		}

		public void TestImportCostIntoCostingDoesNotOverrideJR_OH_SellAccount()
		{
			var consol = Factory.New<ForwardingConsol>();

			var costToImport = Factory.New<JobConsolCost>();
			using (costToImport.ReportSettingParentSuspender.GetSuspender())
			{
				costToImport.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
			}
			costToImport.E6_GC = GlbCompany.CurrentCompany.PK;
			costToImport.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			costToImport.E6_RX_NKCurrency = TestObjectCreator.AUD.RX_Code;
			costToImport.E6_ExchangeRate = 1m;
			costToImport.E6_ApportionmentMethod = "SHP";
			costToImport.E6_OSCostAmount = 77.1M;
			costToImport.E6_LocalCostAmount = 77.1M;

			var charge = costToImport.ApportionmentCharges.AddNew();
			charge.JR_OSCostAmt = 38.55M;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_JH = Factory.NewJobWithValidTestDataForTesting<Job>().PK;

			var arInvoice = Factory.New<ARInvoice>();
			var line = (InvoicingLineBase)arInvoice.Lines.AddNew();
			line.AL_OSExTaxAmount = 38.55M;
			line.AL_LocalExTaxAmount = 38.55M;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			charge.JR_AL_ARLine = line.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.ARLine.AL_OSAmount = charge.ARLine.AL_LineAmount = charge.JR_OSSellAmt;

			var invoice = Factory.New<APInvoice>();
			invoice.ExchangeRate.Currency = "USD";
			invoice.ExchangeRate.Rate = 0.81M;

			var originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			ConsolCostImporter.ImportCostIntoCosting(invoice, costToImport, originatingCost, false);

			var apportionSplitCharge = originatingCost.ApportionmentCharges.Where(x => x.JR_JH == charge.JR_JH).FirstOrDefault();
			AssertNotNull("ApportionSplitCharge should not be null", apportionSplitCharge);
			AssertEquals("JR_OH_SellAccount", ZGuid.Empty, apportionSplitCharge.JR_OH_SellAccount);
		}

		public void TestImportCostIntoCostingReApportionigAmount()
		{
			var consol = Factory.New<ForwardingConsol>();

			var currentCompanyCost = consol.GetApportionments().CostsCollection.TryAddNew();
			currentCompanyCost.E6_GC = GlbCompany.CurrentCompany.PK;
			currentCompanyCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			currentCompanyCost.E6_RX_NKCurrency = TestObjectCreator.AUD.RX_Code;
			currentCompanyCost.E6_ExchangeRate = 1m;
			currentCompanyCost.E6_ApportionmentMethod = "SHP";
			currentCompanyCost.E6_OSCostAmount = 77.1M;
			currentCompanyCost.E6_LocalCostAmount = 77.1M;

			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge1 = currentCompanyCost.ApportionmentCharges.AddNew();
			charge1.JR_OSCostAmt = 38.55M;
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_JH = job1.PK;

			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge2 = currentCompanyCost.ApportionmentCharges.AddNew();
			charge2.JR_OSCostAmt = 38.55M;
			charge2.JR_AC = TestObjectCreator.CC1.PK;
			charge2.JR_JH = job2.PK;

			Factory.Save();

			var invoice = Factory.New<APInvoice>();
			invoice.ExchangeRate.Currency = "USD";
			invoice.ExchangeRate.Rate = 0.81M;
			var originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			var originatingCharge1 = originatingCost.ApportionmentCharges.AddNew();
			originatingCharge1.JR_JH = job1.PK;
			var originatingCharge2 = originatingCost.ApportionmentCharges.AddNew();
			originatingCharge2.JR_JH = job2.PK;
			originatingCost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			originatingCost.E6_ExchangeRate = 0.81M;

			ConsolCostImporter.ImportCostIntoCosting(invoice, currentCompanyCost, originatingCost, false);

			AssertEquals("Should be one ConsolCost imported", 1, invoice.ConsolCosting.ConsolCosts.Count);
			AssertEquals("E6_LocalCostAmount", 77.1M, invoice.ConsolCosting.ConsolCosts[0].E6_LocalCostAmount);
			AssertEquals("E6_OSCostAmount", 62.45M, invoice.ConsolCosting.ConsolCosts[0].E6_OSCostAmount);

			AssertEquals("Should have two charges imported", 2, invoice.ConsolCosting.ConsolCosts[0].ApportionmentCharges.Count);
			AssertEquals("Sum of apportionments should be equal to ConsolCost LocalCost", invoice.ConsolCosting.ConsolCosts[0].E6_LocalCostAmount, invoice.ConsolCosting.ConsolCosts[0].ApportionmentCharges.JR_LocalCostAmtSum);
			AssertEquals("Sum of apportionments should be equal to ConsolCostOSCost", invoice.ConsolCosting.ConsolCosts[0].E6_OSCostAmount, invoice.ConsolCosting.ConsolCosts[0].ApportionmentCharges.JR_OSCostAmtSum);
			AssertEquals("First JR_LocalCostAmt", 38.54M, invoice.ConsolCosting.ConsolCosts[0].ApportionmentCharges[0].JR_LocalCostAmt);
			AssertEquals("First JR_OSCostAmt", 31.22M, invoice.ConsolCosting.ConsolCosts[0].ApportionmentCharges[0].JR_OSCostAmt);
			AssertEquals("Second JR_LocalCostAmt", 38.56M, invoice.ConsolCosting.ConsolCosts[0].ApportionmentCharges[1].JR_LocalCostAmt);
			AssertEquals("Second JR_OSCostAmt", 31.23M, invoice.ConsolCosting.ConsolCosts[0].ApportionmentCharges[1].JR_OSCostAmt);
		}

		public void TestGSTInclusiveAmountImport()
		{
			var consol = Factory.New<ForwardingConsol>();

			var currentCompanyCost = consol.GetApportionments().CostsCollection.TryAddNew();
			currentCompanyCost.E6_GC = GlbCompany.CurrentCompany.PK;
			currentCompanyCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			currentCompanyCost.E6_RX_NKCurrency = TestObjectCreator.AUD.RX_Code;

			var invoice = Factory.New<APInvoice>();
			var originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);

			invoice.GSTInclusiveAmounts = true;
			invoice.AH_TransactionNum = "TRAN101";
			currentCompanyCost.E6_IsTaxAmountOverridden = true;
			currentCompanyCost.E6_OSCostAmount = 77;
			currentCompanyCost.E6_LocalCostAmount = 77;
			currentCompanyCost.E6_OSGSTAmount_Calc = 333 - 77;
			originatingCost.GSTInclusiveAmount = 11;
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			currentCompanyCost.E6_AC_ChargeCode = chargeCode.PK;
			originatingCost.E6_AC_ChargeCode = chargeCode.PK;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();

			var charge1 = currentCompanyCost.ApportionmentCharges.AddNew();
			charge1.JR_OSCostAmt = 77;
			charge1.JR_AC = chargeCode.PK;
			charge1.JR_JH = job.PK;

			var charge2 = originatingCost.ApportionmentCharges.AddNew();
			charge2.JR_OSCostAmt = 11;
			charge2.JR_AC = chargeCode.PK;
			charge2.JR_JH = job.PK;
			charge2.JR_APInvoiceDate = originatingCost.E6_InvoiceDate;
			charge2.JR_PaymentDate = originatingCost.E6_PaymentDate;

			Factory.Save();

			ConsolCostImporter.ImportCostIntoCosting(invoice, currentCompanyCost, originatingCost, false);
			Assert(!originatingCost.GSTInclusiveAmountNeedUpdate);
			AssertEquals("GSTInclusiveAmount should be copied the first.", 333m, originatingCost.GSTInclusiveAmount);
			AssertEquals("E6_OSCostAmount should be copied the second.", 77m, originatingCost.E6_OSCostAmount);

			invoice.GSTInclusiveAmounts = false;
			currentCompanyCost.E6_OSCostAmount = 33;
			originatingCost.E6_OSCostAmount = 11;
			ConsolCostImporter.ImportCostIntoCosting(invoice, currentCompanyCost, originatingCost, false);
			Assert(!originatingCost.GSTInclusiveAmountNeedUpdate);
			AssertEquals("GSTInclusiveAmount should be copied the first.", 33m, originatingCost.GSTInclusiveAmount);
			AssertEquals("E6_OSCostAmount should be copied the second.", 33m, originatingCost.E6_OSCostAmount);

			invoice.GSTInclusiveAmounts = true;
			currentCompanyCost.E6_OSCostAmount = 77;
			originatingCost.GSTInclusiveAmount = 11;
			ConsolCostImporter.ImportCostIntoCosting(invoice, currentCompanyCost, originatingCost, false);
			Assert(!originatingCost.GSTInclusiveAmountNeedUpdate);
			AssertEquals("GSTInclusiveAmount should be copied the first.", 77m, originatingCost.GSTInclusiveAmount);
			AssertEquals("E6_OSCostAmount should be copied the second.", 77m, originatingCost.E6_OSCostAmount);
		}

		public void TestUsedApportionmentForRelatedShipmentsAreImported()
		{
			var consol = Factory.New<ForwardingConsol>();

			var consolCost = Factory.New<JobConsolCost>();
			using (consolCost.ReportSettingParentSuspender.GetSuspender())
			{
				consolCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
			}
			consolCost.E6_GC = GlbCompany.CurrentCompany.PK;
			consolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			consolCost.E6_RX_NKCurrency = TestObjectCreator.AUD.RX_Code;

			var originalCharge1 = consolCost.ApportionmentCharges.AddNew();
			var originalCharge2 = consolCost.ApportionmentCharges.AddNew();

			var masterShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			masterShipment.JS_ShipmentType = Enterprise.Core.Constants.ShipmentTypes.CoLoadMaster;
			var relatedShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			relatedShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			consol.Shipments.Add(masterShipment);
			consol.Shipments.Add(relatedShipment);

			var job1 = new Job.Loader(masterShipment).TryCreateWithoutMutexForTestOnly();
			originalCharge1.JR_JH = job1.PK;

			var job2 = new Job.Loader(relatedShipment).TryCreateWithoutMutexForTestOnly();
			originalCharge2.JR_JH = job2.PK;

			originalCharge1.JR_AC = TestObjectCreator.CC1.PK;
			originalCharge1.JR_IsUsedForApportionment = true;
			originalCharge2.JR_AC = TestObjectCreator.CC1.PK;
			originalCharge2.JR_IsUsedForApportionment = true;

			consolCost.E6_OSCostAmount = 50M;
			consolCost.E6_LocalCostAmount = 50M;
			consolCost.E6_ApportionmentMethod = "SHP";

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00001001", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			var line = TestObjectCreator.CreateARInvoiceLine(invoice, job2, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "ArInvoice Line", 25m);
			originalCharge2.JR_AL_ARLine = line.PK;
			originalCharge2.JR_OSSellAmt = line.AL_OSAmount;
			originalCharge2.JR_LocalSellAmt = line.AL_LineAmount;

			Factory.Save();

			var invoice1 = Factory.New<APInvoice>();
			var originatingCost = invoice1.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			ConsolCostImporter.ImportCostIntoCosting(invoice1, consolCost, originatingCost, false);

			AssertEquals("Should be 2 Charges", 2, originatingCost.ApportionmentCharges.Count);
			AssertEquals("[0].JR_IsUsedForApportionment", true, originatingCost.ApportionmentCharges[0].JR_IsUsedForApportionment);
			AssertEquals("[1].JR_IsUsedForApportionment", true, originatingCost.ApportionmentCharges[1].JR_IsUsedForApportionment);

			Assert("No ARLine should be populated during import", originatingCost.ApportionmentCharges[0].JR_AL_ARLine.IsEmpty);
			Assert("No ARLine should be populated during import", originatingCost.ApportionmentCharges[1].JR_AL_ARLine.IsEmpty);
		}

		[TestDate(2020, 8, 1)]
		public void TestImportCostIntoCosting_ConsolCostTaxDateIsValidAndApportionmentChargesTaxDateIsInvalid()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C0001");
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var costToImport = consol.GetApportionments().CostsCollection.TryAddNew();
			costToImport.E6_GC = GlbCompany.CurrentCompany.PK;
			costToImport.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			costToImport.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;
			costToImport.E6_ApportionmentMethod = "SHP";
			costToImport.E6_OSCostAmount = 77.1M;
			costToImport.E6_LocalCostAmount = 77.1M;
			costToImport.E6_AT_TaxRate = TestObjectCreator.GSTFREE1.PK;
			AssertEquals("Precondition", ZDate.Empty, costToImport.E6_TaxDate);

			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Creditor1);
			var originatingCost = apInvoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			originatingCost.E6_TaxDate = new ZDate(2020, 8, 1);
			originatingCost.E6_AT_TaxRate = ZGuid.Empty;

			AssertEquals("ApportionmentCharges count", 1, originatingCost.ApportionmentCharges.Count);
			var charge = originatingCost.ApportionmentCharges[0];
			charge.JR_CostTaxDate = ZDate.Empty;
			charge.JR_AT_CostGSTRate = ZGuid.Empty;

			ConsolCostImporter.ImportCostIntoCosting(apInvoice, costToImport, originatingCost, false);
			AssertEquals("Tax Date should be today due to defaulting based on registry", new ZDate(2020, 8, 1), originatingCost.E6_TaxDate);
			AssertEquals("An error should not be reported", string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestImportCostIntoCosting_PlaceOfSupply()
		{
			var consol = Factory.New<ForwardingConsol>();

			var currentCompanyCost = consol.GetApportionments().CostsCollection.TryAddNew();
			currentCompanyCost.E6_GC = GlbCompany.CurrentCompany.PK;
			currentCompanyCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			currentCompanyCost.E6_PlaceOfSupply = "DL";
			currentCompanyCost.E6_PlaceOfSupplyType = PlaceOfSupplyTypes.State.Code;

			var invoice = Factory.New<APInvoice>();
			var originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			var creditor = TestObjectCreator.CreateOrgHeader("ORG001", true, false);
			originatingCost.E6_OH_Creditor = creditor.PK;
			AssertNotEquals("DL", originatingCost.E6_PlaceOfSupply);
			AssertNotEquals(PlaceOfSupplyTypes.State.Code, originatingCost.E6_PlaceOfSupplyType);

			var importerFactory = new BusinessObjectFactory();
			var importer = new InvoicingBaseConsolCostImporter(importerFactory, originatingCost, invoice);
			ConsolCostImporter.ImportCostIntoCosting(invoice, currentCompanyCost, originatingCost, false);

			AssertEquals("DL", originatingCost.E6_PlaceOfSupply);
			AssertEquals(PlaceOfSupplyTypes.State.Code, originatingCost.E6_PlaceOfSupplyType);
		}

		public void TestImportCostIntoCosting_E6_SupplyType()
		{
			var consol = Factory.New<ForwardingConsol>();

			var loa = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOA;
			var currentCompanyCost = consol.GetApportionments().CostsCollection.TryAddNew();
			currentCompanyCost.E6_GC = GlbCompany.CurrentCompany.PK;
			currentCompanyCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			currentCompanyCost.E6_SupplyType = loa;

			var invoice = Factory.New<APInvoice>();
			var originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			var creditor = TestObjectCreator.CreateOrgHeader("ORG001", true, false);
			originatingCost.E6_OH_Creditor = creditor.PK;
			AssertNotEquals(loa, originatingCost.E6_SupplyType);

			ConsolCostImporter.ImportCostIntoCosting(invoice, currentCompanyCost, originatingCost, false);
			AssertEquals(loa, originatingCost.E6_SupplyType);
		}

		[TestDate(2024, 11, 06)]
		public void TestImportCostIntoCosting_E6_TaxDate()
		{
			var consol = Factory.New<ForwardingConsol>();

			var currentCompanyCost = consol.GetApportionments().CostsCollection.TryAddNew();
			currentCompanyCost.E6_GC = GlbCompany.CurrentCompany.PK;
			currentCompanyCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			currentCompanyCost.E6_RX_NKCurrency = TestObjectCreator.AUD.RX_Code;
			currentCompanyCost.E6_AT_TaxRate = TestObjectCreator.VATSPV.PK;
			currentCompanyCost.E6_TaxDate = new ZDate(2024, 11, 15);

			var invoice = Factory.New<APInvoice>();
			var originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			var creditor = TestObjectCreator.CreateOrgHeader("ORG001", true, false);
			creditor.CompanyData.SetAPTaxApplicable(true);
			originatingCost.E6_OH_Creditor = creditor.PK;
			originatingCost.E6_AT_TaxRate = TestObjectCreator.GST1.PK;
			originatingCost.E6_TaxDate = new ZDate(2024, 11, 10);

			ConsolCostImporter.ImportCostIntoCosting(invoice, currentCompanyCost, originatingCost, false);
			AssertEquals(TestObjectCreator.VATSPV.PK, originatingCost.E6_AT_TaxRate);
			AssertEquals(new ZDate(2024, 11, 15), originatingCost.E6_TaxDate);
		}

		[TestDate(2021, 05, 02)]
		public void TestImportCostIntoCosting_E6_TaxDate_DefaultFromRegistry()
		{
			var consol = Factory.New<ForwardingConsol>();

			var currentCompanyCost = consol.GetApportionments().CostsCollection.TryAddNew();
			currentCompanyCost.E6_GC = GlbCompany.CurrentCompany.PK;
			currentCompanyCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			currentCompanyCost.E6_RX_NKCurrency = TestObjectCreator.AUD.RX_Code;
			currentCompanyCost.E6_CostGovtChargeCode = "S0023";
			currentCompanyCost.E6_SellGovtChargeCode = "S0023";
			currentCompanyCost.E6_AT_TaxRate = TestObjectCreator.VATSPV.PK;
			currentCompanyCost.E6_TaxDate = ZDate.Empty;

			var invoice = Factory.New<APInvoice>();
			invoice.AH_InvoiceDate = new ZDate(2021, 04, 08);
			var originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			var creditor = TestObjectCreator.CreateOrgHeader("ORG001", true, false);
			creditor.CompanyData.SetAPTaxApplicable(true);
			originatingCost.E6_OH_Creditor = creditor.PK;

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption = collection.AddNew();
			taxDateOption.JobType = "FCN";
			taxDateOption.DirectionCode = "ALL";
			taxDateOption.Mode = "ALL";
			taxDateOption.Ledger = "AP";
			taxDateOption.TaxDateOption = TaxDateDefaultingOption.Code.InvoiceDate;
			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				ConsolCostImporter.ImportCostIntoCosting(invoice, currentCompanyCost, originatingCost, false);
				AssertEquals(TestObjectCreator.VATSPV.PK, originatingCost.E6_AT_TaxRate);
				AssertEquals("Tax Date should be invoice date due to defaulting based on registry", new ZDate(2021, 04, 08), originatingCost.E6_TaxDate);
			}
		}

		[TestDate(2021, 05, 02)]
		public void TestE6_OSGSTAmountRecalculatedAfterTaxDateDefaultFromRegistryWhenImportCostsIntoCosting()
		{
			var creditor = TestObjectCreator.CreateOrgHeader("ORG001", true, false);
			creditor.CompanyData.SetAPTaxApplicable(true);

			var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			var rate = taxRate.SetRate_ForTestOnly(10, 5, null, ZDate.Today.AddMonths(-2));
			var rate1 = taxRate.SetRate_ForTestOnly(10, 2, ZDate.Today.AddMonths(-2), ZDate.Today.AddMonths(-1));
			var rate2 = taxRate.SetRate_ForTestOnly(15, 5, ZDate.Today, null);
			taxRate.Factory.Save();

			var inv = Factory.New<APInvoice>();
			var currentCompanyCost = inv.ConsolCosting.ConsolCosts.AddNew();
			currentCompanyCost.E6_GC = GlbCompany.CurrentCompany.PK;
			currentCompanyCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			currentCompanyCost.E6_IsTaxAmountOverridden = true;
			currentCompanyCost.E6_OH_Creditor = creditor.PK;
			currentCompanyCost.E6_RX_NKCurrency = TestObjectCreator.AUD.RX_Code;
			currentCompanyCost.E6_OSCostAmount = 100m;
			currentCompanyCost.E6_AT_TaxRate = taxRate.PK;
			currentCompanyCost.E6_TaxDate = ZDate.Empty;

			AssertEquals("Pre-condition: tax calculated by today's tax rate", 3m, currentCompanyCost.E6_OSGSTAmount);

			var consol = Factory.New<ForwardingConsol>();
			var invoice = Factory.New<APInvoice>();
			invoice.AH_InvoiceDate = new ZDate(2021, 04, 01);
			var originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			originatingCost.E6_OH_Creditor = creditor.PK;

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption = collection.AddNew();
			taxDateOption.JobType = "FCN";
			taxDateOption.DirectionCode = "ALL";
			taxDateOption.Mode = "ALL";
			taxDateOption.Ledger = "AP";
			taxDateOption.TaxDateOption = TaxDateDefaultingOption.Code.InvoiceDate;

			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				AssertEquals("PreCondition", ZDate.Empty, currentCompanyCost.E6_TaxDate);

				ConsolCostImporter.ImportCostIntoCosting(invoice, currentCompanyCost, originatingCost, false);
				AssertEquals(taxRate.PK, originatingCost.E6_AT_TaxRate);
				AssertEquals("Tax Date should be invoice date due to defaulting based on registry", new ZDate(2021, 04, 01), originatingCost.E6_TaxDate);
				AssertEquals("Tax Amount should be recalculated due to defaulting based on registry", 5m, originatingCost.E6_OSGSTAmount);
			}
		}

		public void TestImportChargeIntoCosting()
		{
			var branchZZA = TestObjectCreator.CreateBranch("ZZA", GlbCompany.CurrentCompany);
			var branchZZB = TestObjectCreator.CreateBranch("ZZB", GlbCompany.CurrentCompany);
			var branchZZC = TestObjectCreator.CreateBranch("ZZC", GlbCompany.CurrentCompany);
			var consol = TestObjectCreator.CreateConsolWithShipmentJobs("C0001"
				, shipmentJobNums: new[] { "S00001001", "S00001002", "S00001003" }
				, shipmentInvoker: (shipment) => TestObjectCreator.CreateJob(shipment)
			);
			Factory.Save();

			var consolCostWithAllShipments = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC2, TestObjectCreator.USD, 5.5m, 200m, TestObjectCreator.AALSHI);
			AssertEquals("PreCondition", 3, consolCostWithAllShipments.ApportionmentCharges.Count);
			var apportionment_AllShipments_S00001001 = GetConsolApportionmentCharge(consolCostWithAllShipments, "S00001001");
			var apportionment_AllShipments_S00001002 = GetConsolApportionmentCharge(consolCostWithAllShipments, "S00001002");
			var apportionment_AllShipments_S00001003 = GetConsolApportionmentCharge(consolCostWithAllShipments, "S00001003");

			var consolCostWithFewShipments = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC3, TestObjectCreator.IDR, 7m, 200m, TestObjectCreator.AALSHI);
			consolCostWithFewShipments.ApportionmentCharges.Remove(GetConsolApportionmentCharge(consolCostWithFewShipments, "S00001001"));
			consolCostWithFewShipments.ApportionmentCharges.Remove(GetConsolApportionmentCharge(consolCostWithFewShipments, "S00001002"));
			AssertEquals("PreCondition", 1, consolCostWithFewShipments.ApportionmentCharges.Count);
			var apportionment_FewShipments_S00001003 = GetConsolApportionmentCharge(consolCostWithFewShipments, "S00001003");

			var dummyApportionmentList = new[] {
				CreateDummyApportionmentToImport(GetConsolShipmentJob(consol, "S00001001").PK, branchZZA.PK, TestObjectCreator.FIADepartment.PK
					, 33m, TestObjectCreator.IDR.Code, 2m, false, out _)
				, CreateDummyApportionmentToImport(GetConsolShipmentJob(consol, "S00001002").PK, branchZZB.PK, TestObjectCreator.FEADepartment.PK
					, 44m, TestObjectCreator.IDR.Code, 2m, true, out var dummyApportionment2_AsCharge)
				, CreateDummyApportionmentToImport(GetConsolShipmentJob(consol, "S00001003").PK, branchZZC.PK, TestObjectCreator.FISDepartment.PK
					, 55m, TestObjectCreator.IDR.Code, 2m, true, out var dummyApportionment3_AsCharge)
			};

			ConsolCostImporter.ImportChargeIntoCosting(consolCostWithAllShipments, dummyApportionmentList, false);
			AssertEquals(3, consolCostWithAllShipments.ApportionmentCharges.Count);
			AssertApportionmentCharge("", apportionment_AllShipments_S00001001
				, branchZZA.PK, TestObjectCreator.FIADepartment.PK
				, TestObjectCreator.USD.Code, 66.66m, 5.5m, 12.12m
				, expectedRelatedApportionChargeFromDB: null);
			AssertApportionmentCharge("", apportionment_AllShipments_S00001002
				, branchZZB.PK, TestObjectCreator.FEADepartment.PK
				, TestObjectCreator.USD.Code, 66.67m, 5.5m, 12.12m
				, dummyApportionment2_AsCharge);
			AssertApportionmentCharge("", apportionment_AllShipments_S00001003
				, branchZZC.PK, TestObjectCreator.FISDepartment.PK
				, TestObjectCreator.USD.Code, 66.67m, 5.5m, 12.12m
				, dummyApportionment3_AsCharge);

			ConsolCostImporter.ImportChargeIntoCosting(consolCostWithFewShipments, dummyApportionmentList, false);
			AssertEquals(3, consolCostWithFewShipments.ApportionmentCharges.Count);
			AssertApportionmentCharge("", GetConsolApportionmentCharge(consolCostWithFewShipments, "S00001001")
				, branchZZA.PK, TestObjectCreator.FIADepartment.PK
				, TestObjectCreator.IDR.Code, 67m, 7m, 9.57m
				, expectedRelatedApportionChargeFromDB: null);
			AssertApportionmentCharge("", GetConsolApportionmentCharge(consolCostWithFewShipments, "S00001002")
				, branchZZB.PK, TestObjectCreator.FEADepartment.PK
				, TestObjectCreator.IDR.Code, 67m, 7m, 9.57m
				, dummyApportionment2_AsCharge);
			AssertApportionmentCharge("", apportionment_FewShipments_S00001003
				, branchZZC.PK, TestObjectCreator.FISDepartment.PK
				, TestObjectCreator.IDR.Code, 66m, 7m, 9.43m
				, dummyApportionment3_AsCharge);
		}

		void AssertApportionmentCharge(string comment, ApportionSplitCharge targetCharge, ZGuid expectedBranch, ZGuid expectedDepartment
			, string expectedCurrency, decimal expectedOSAmount, decimal expectedExRate, decimal expectedLocalAmount, JobCharge expectedRelatedApportionChargeFromDB)
		{
			CombineAssertions(comment, () => {
				AssertEquals("JR_GB", expectedBranch, targetCharge.JR_GB);
				AssertEquals("JR_GE", expectedDepartment, targetCharge.JR_GE);
				AssertEquals("JR_RX_NKCostCurrency", expectedCurrency, targetCharge.JR_RX_NKCostCurrency);
				AssertEquals("JR_OSCostAmt", expectedOSAmount, targetCharge.JR_OSCostAmt);
				AssertEquals("JR_OSCostExRate", expectedExRate, targetCharge.JR_OSCostExRate);
				AssertEquals("JR_LocalCostAmt", expectedLocalAmount, targetCharge.JR_LocalCostAmt);
				AssertEquals("RelatedApportionChargeFromDB", expectedRelatedApportionChargeFromDB, targetCharge.RelatedApportionChargeFromDB);
			});
		}

		ApportionSplitCharge GetConsolApportionmentCharge(JobConsolCost consolCost, string jobNumber)
		{
			return consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.Job.JH_JobNum == jobNumber);
		}

		JobHeader GetConsolShipmentJob(ForwardingConsol consol, string jobNumber)
		{
			return consol.Shipments.Cast<ForwardingShipment>().Single(x => x.JobNumber == jobNumber).Job;
		}

		IApportionmentChargeToImport CreateDummyApportionmentToImport(ZGuid jobPK, ZGuid branchPK, ZGuid departPK
			, decimal osAmount, string currency, decimal exRate, bool isCharge, out ApportionSplitCharge asCharge)
		{
			if (isCharge)
			{
				asCharge = Factory.New<ApportionSplitCharge>();

				asCharge.JR_JH = jobPK;
				asCharge.JR_GB = branchPK;
				asCharge.JR_GE = departPK;
				asCharge.JR_OSCostAmt = osAmount;
				asCharge.JR_RX_NKCostCurrency = currency;
				asCharge.JR_OSCostExRate = exRate;
				return asCharge;
			}
			else
			{
				asCharge = null;
				var result = new Mock<IApportionmentChargeToImport>();
				result.Setup(x => x.JR_JH).Returns(jobPK);
				result.Setup(x => x.JR_GB).Returns(branchPK);
				result.Setup(x => x.JR_GE).Returns(departPK);
				result.Setup(x => x.JR_OSCostAmt).Returns(osAmount);
				result.Setup(x => x.JR_RX_NKCostCurrency).Returns(currency);
				result.Setup(x => x.JR_OSCostExRate).Returns(exRate);
				return result.Object;
			}
		}

		IConsolCostImporter ConsolCostImporter => consolCostImporter ??= ObjectFactory.Get<IConsolCostImporter>();
		IConsolCostImporter consolCostImporter;

		TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;
	}
}
