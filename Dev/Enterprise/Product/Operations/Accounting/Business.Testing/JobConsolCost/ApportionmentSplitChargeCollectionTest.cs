using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.Business.JobInvoicing.Testing.ChargeCollectionTest;

namespace Enterprise.Accounting.Business.ConsolCosting.Testing
{
	[TestedType(typeof(ApportionmentSplitChargeCollection))]
	public class ApportionmentSplitChargeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultApportionmentChargeCreationInfoIsCollectedForCriticalValidation()
		{
			var collector = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment1 = TestObjectCreator.CreateShipment("S00001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S00002", consol);
			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);
			AssertEquals(2, consolCost1.ApportionmentCharges.Count);
			var expectedContainsMessage = "DefaultChargeCreationForConsolCost: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.";
			AssertContains("Data is not collected because collection frequesncy is CollectOnlyAfterErrorReportForCurrentUserSession", expectedContainsMessage, collector.GetInfo(consolCost1.PK, CriticalValidationInfoCollectorServiceKeyType.DefaultChargeCreationForConsolCost));

			var consolCost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC2, 200m);
			AssertEquals(2, consolCost2.ApportionmentCharges.Count);
			expectedContainsMessage = $@"Default Charge is created.
Default Charge PK: {consolCost2.ApportionmentCharges[1].PK}
Consol Cost PK: {consolCost2.PK}
Shipment Job PK: {shipment2.Job.PK}
StackTrace:    at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)
   at System.Environment.get_StackTrace()";
			AssertContains("Data is collected", expectedContainsMessage, collector.GetInfo(consolCost2.PK, CriticalValidationInfoCollectorServiceKeyType.DefaultChargeCreationForConsolCost));
			shipment1.Job.Dispose();
			shipment2.Job.Dispose();
		}

		[TestDate(2017, 01, 10)]
		[DisableZeroExchangeRateOverriding]
		public void TestCreatingDefaultApportionmentChargeDoesNotSetHasChanges_OnPreExistingCharge()
		{
			var expectedExRate = 2.5m;
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, expectedExRate, new ZDateTime(2017, 01, 01), new ZDateTime(2017, 01, 31));

			var debtorWithDefaultInvoicingCurrency = TestObjectCreator.CreateOrgHeader("ABCDEF", false, true);
			debtorWithDefaultInvoicingCurrency.CompanyData.OB_RX_NKARDDefltCurrency = Constants.CurrencyCodes.UnitedStates;
			TestObjectCreator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = Constants.CurrencyCodes.UnitedStates;
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment1 = TestObjectCreator.CreateShipment("S001001", consol);
			shipment1.JS_ActualWeight = 10m;
			shipment1.JS_ActualVolume = 10m;
			TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0m, TestObjectCreator.Agent, 0m);
			var shipment2 = TestObjectCreator.CreateShipment("S001002");
			shipment2.JS_ActualWeight = 20m;
			shipment2.JS_ActualVolume = 20m;
			var shipment2Job = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0m, debtorWithDefaultInvoicingCurrency, 0m);

			var charge = shipment2Job.FilteredCharges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_RX_NKSellCurrency = Constants.CurrencyCodes.UnitedStates;
			charge.JR_OSSellAmt = 500;

			var exRate = Factory.Load<ExchangeRate>(charge.RevenueExchangeRate.ExchangeRatePk);
			exRate.JF_OH_Org = ZGuid.Empty;
			exRate.JF_OrgType = "";
			exRate.JF_BaseRate += 0.5m;
			exRate.JF_IsTransformed = false;

			Factory.Save();
			AssertEquals(exRate.PK, charge.RevenueExchangeRate.ExchangeRatePk);
			AssertEquals(2, shipment2Job.ExchangeRates.Count);

			var newFactory = new BusinessObjectFactory();
			var newCreator = new TestObjectCreator(newFactory);
			var consolInNewFactory = newFactory.Load<ForwardingConsol>(consol.PK);
			var costListing = new ApportionmentListing(newFactory, consolInNewFactory);
			var consolCost = costListing.CostsCollection.TryAddNew();
			consolCost.E6_AC_ChargeCode = newCreator.FRT.PK;
			consolCost.E6_RX_NKCurrency = Constants.CurrencyCodes.UnitedStates;
			consolCost.E6_ExchangeRate += 2.5m;
			consolCost.E6_OSCostAmount = 300m;
			consolInNewFactory.Shipments.Add(shipment2);
			var chargeInNewFactory = newFactory.Load<Charge>(shipment2Job.Charges[0].PK);	// his is essential step we could not reproduce functionally - the existing charge should be loaded in the same factory
			newFactory.Save();

			Assert("Changes to the pre-existing charge should not occur in saving", !chargeInNewFactory.HasChanges);
			Assert("Changes to the pre-existing charge should not occur in saving", !ErrorReporter.HasBeenReported("ChargesInDbModifiedWithoutHasChangesSet_5"));
		}

		[TestDate(2017, 01, 10)]
		[DisableZeroExchangeRateOverriding]
		public void TestCreatingDefaultApportionmentChargeDoesNotSetHasChanges_OnJob()
		{
			var expectedExRate = 2.5m;
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, expectedExRate, new ZDateTime(2017, 01, 01), new ZDateTime(2017, 01, 31));

			var debtorWithDefaultInvoicingCurrency = TestObjectCreator.CreateOrgHeader("ABCDEF", false, true);
			debtorWithDefaultInvoicingCurrency.CompanyData.OB_RX_NKARDDefltCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment1 = TestObjectCreator.CreateShipment("S001001", consol);
			shipment1.JS_ActualWeight = 10m;
			shipment1.JS_ActualVolume = 10m;
			TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0m, TestObjectCreator.Agent, 0m);
			var shipment2 = TestObjectCreator.CreateShipment("S001002", consol);
			shipment2.JS_ActualWeight = 20m;
			shipment2.JS_ActualVolume = 20m;
			var shipment2Job = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0m, debtorWithDefaultInvoicingCurrency, 0m);
			Factory.Save();

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, 300m);
			consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().First(x => x.JR_JobNumber == shipment2Job.JH_JobNum).JR_IsUsedForApportionment = false;
			AssertEquals(2, consolCost.ApportionmentCharges.Count);
			AssertEquals(1, consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Count(x => x.JR_OSCostAmt == 0m));
			AssertEquals(1, consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Count(x => x.JR_OSCostAmt == 300m));
			Assert(shipment2Job.HasChanges);
			AssertEquals("Picked up USD Ex Rate", 1, shipment2Job.ExchangeRates.Count);
			AssertEquals("Picked up USD Ex Rate", expectedExRate, shipment2Job.ExchangeRates[0].JF_BaseRate);
			Assert("Exchange rate is not saved.", !shipment2Job.ExchangeRates[0].IsInDatabase);

			Factory.Save();
			Assert("Setting HasChanges should be suspended on the job when creating default charge.", !shipment2Job.HasChanges);
			AssertEquals("No Ex Rate added for Default Charge", 0, shipment2Job.ExchangeRates.Count);
			AssertEquals(2, consolCost.ApportionmentCharges.Count);
			AssertEquals(1, consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Count(x => x.JR_OSCostAmt == 0m));
			AssertEquals(1, consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Count(x => x.JR_OSCostAmt == 300m));

			consolCost.E6_Description = "Test";
			Factory.Save();
			Assert("Setting HasChanges should be suspended on the job when creating default charge.", !shipment2Job.HasChanges);
			AssertEquals("No Ex Rate added for Default Charge", 0, shipment2Job.ExchangeRates.Count);
			AssertEquals(2, consolCost.ApportionmentCharges.Count);
			AssertEquals(1, consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Count(x => x.JR_OSCostAmt == 0m));
			AssertEquals(1, consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Count(x => x.JR_OSCostAmt == 300m));

			consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().First(x => x.JR_JobNumber == shipment2Job.JH_JobNum).JR_IsUsedForApportionment = true;
			Factory.Save();
			Assert(!shipment2Job.HasChanges);
			AssertEquals("Picked up USD Ex Rate as Charge is used for Apportionment", 1, shipment2Job.ExchangeRates.Count);
			AssertEquals("Picked up USD Ex Rate", expectedExRate, shipment2Job.ExchangeRates[0].JF_BaseRate);
			Assert("Exchange rate is now saved.", shipment2Job.ExchangeRates[0].IsInDatabase);
			AssertEquals(2, consolCost.ApportionmentCharges.Count);
			AssertEquals(1, consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Count(x => x.JR_OSCostAmt == 100m));
			AssertEquals(1, consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Count(x => x.JR_OSCostAmt == 200m));
		}

		public void TestReadOnly()
		{
			UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";
			JobConsolCost cost = Factory.NewWithValidTestData<JobConsolCost>();
			cost.E6_AH_APInvoice = invoice.PK;
			cost.ParentAPInvoice = invoice;
			Factory.Save();
			ApportionmentSplitChargeCollection collection = new ApportionmentSplitChargeCollection(cost);
			Assert("Collection must be NOT read only because IsApprovingCosting is true", !collection.ReadOnly);
		}

		#region TestChargeCollectionErrorReportedWhenDeletedChargeRemainsInBusinessObjectCollection

		public void TestChargeCollectionErrorReportedWhenDeletedChargeRemainsInBusinessObjectCollection_Case1()
		{
			AssertChargeCollectionErrorReportedWhenDeletedChargeRemainsInBusinessObjectCollection(false);
		}

		public void TestChargeCollectionErrorReportedWhenDeletedChargeRemainsInBusinessObjectCollection_Case2()
		{
			AssertChargeCollectionErrorReportedWhenDeletedChargeRemainsInBusinessObjectCollection(true);
		}

		void AssertChargeCollectionErrorReportedWhenDeletedChargeRemainsInBusinessObjectCollection(bool deleteApprtionmentCharge)
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.ChargeCollectionRemoveMethodInfo);

			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job);
			Factory.Save();

			var apportionmentCharge = Factory.Load<ApportionSplitCharge>(charge.PK);

			var collection1 = new MyDummyBizObjCollectionWithInternalOverrides(Factory);
			collection1.Add(apportionmentCharge);
			AssertEquals(1, collection1.Count);

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100);
			Factory.Save();
			var collection2 = consolCost.ApportionmentCharges;
			AssertEquals(1, collection2.Count);

			Assert("Precondition: collection1.Contains", collection1.Contains(apportionmentCharge));
			Assert("Precondition: collection2.Contains", collection2.Contains(apportionmentCharge));

			AssertCollectionContains("collection1 in apportionmentCharge ParentCollections", collection1, ((IBusinessObjectInternals)apportionmentCharge).ParentCollections);
			AssertCollectionContains("collection2 in apportionmentCharge ParentCollections", collection2, ((IBusinessObjectInternals)apportionmentCharge).ParentCollections);

			var expectedKey = "Business_Object_Collections_With_Deleted_Charge_3";

			ErrorReporter.Clear();

			if (deleteApprtionmentCharge)
			{
				apportionmentCharge.Delete();
			}
			else
			{
				charge.Delete();
			}

			Assert("collection1.Contains", collection1.Contains(apportionmentCharge));
			Assert("collection2.Contains", !collection2.Contains(apportionmentCharge));

			AssertEquals("TotalErrorCount", 1, ErrorReporter.TotalErrorCount);
			AssertEquals("Error must be reported", expectedKey, ErrorReporter.LastKeyReported);

			AssertContains($@"Deleted Charge:
	PK = {charge.PK}
	Type = ApportionSplitCharge", ErrorReporter.LastMessageReported);
			AssertContains($@"Charge original values: PK = {charge.PK}, Job PK = {job.PK}", ErrorReporter.LastMessageReported);
			AssertContains($@"ChargeCollectionRemoveMethodInfo:", ErrorReporter.LastMessageReported);
			AssertContains("Contains?NotInCollection", ErrorReporter.LastMessageReported);
			AssertContains($@"Before Delete:
Parent collections:
BusinessObjectCollection Info:
	Collection Type = Enterprise.Accounting.Business.JobInvoicing.Testing.ChargeCollectionTest+MyDummyBizObjCollectionWithInternalOverrides
	Element Type = CargoWise.EntityFramework.Testing.DummyBusinessObject", ErrorReporter.LastMessageReported);
			AssertContains($@"BusinessObjectCollection Info:
	Collection Type = Enterprise.Accounting.Business.ConsolCosting.ApportionmentSplitChargeCollection
	Element Type = Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge", ErrorReporter.LastMessageReported);
			AssertContains($@"Master:
	PK = {consolCost.PK}
	Type = JobConsolCost
	Types around row = JobConsolCost", ErrorReporter.LastMessageReported);
			AssertContains($@"After Delete:
Parent collections:
BusinessObjectCollection Info:
	Collection Type = Enterprise.Accounting.Business.JobInvoicing.Testing.ChargeCollectionTest+MyDummyBizObjCollectionWithInternalOverrides
	Element Type = CargoWise.EntityFramework.Testing.DummyBusinessObject", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		#endregion

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JobConsolCost cost = Factory.New<JobConsolCost>();
			return new ApportionmentSplitChargeCollection(cost);
		}
	}

	public class TestApportionmentSplitChargeCollectionValidationSuspender : TestCaseWithFactory
	{
		ApportionmentSplitChargeCollection CollectionWithACost
		{
			get
			{
				if (fCollectionWithACost == null)
				{
					var creator = new TestObjectCreator(Factory);
					creator.CC3.AC_GovtChargeCode = "AAA999";
					JobConsolCost cost = Factory.New<JobConsolCost>();
					cost.E6_AC_ChargeCode = creator.CC3.PK;
					fCollectionWithACost = new ApportionmentSplitChargeCollection(cost);
					cost.E6_IsTaxAmountOverridden = true;
				}

				return fCollectionWithACost;
			}
		}

		ApportionmentSplitChargeCollection fCollectionWithACost;
		public void TestJR_OSCostAmtSum()
		{
			ApportionSplitCharge charge1 = CollectionWithACost.AddNew();
			ApportionSplitCharge charge2 = CollectionWithACost.AddNew();
			charge1.JR_OSCostAmt = 500;
			charge2.JR_OSCostAmt = 100;
			AssertEquals(600m, CollectionWithACost.JR_OSCostAmtSum);
		}

		public void TestJR_LocalCostAmtSum()
		{
			ApportionSplitCharge charge1 = CollectionWithACost.AddNew();
			ApportionSplitCharge charge2 = CollectionWithACost.AddNew();
			charge1.JR_LocalCostAmt = 500;
			charge2.JR_LocalCostAmt = 100;
			AssertEquals(600m, CollectionWithACost.JR_LocalCostAmtSum);
		}

		public void TestJR_OSCostGSTAmtSum()
		{
			ApportionSplitCharge charge1 = CollectionWithACost.AddNew();
			ApportionSplitCharge charge2 = CollectionWithACost.AddNew();
			charge1.JR_OSCostGSTAmt_Calc = 500;
			charge2.JR_OSCostGSTAmt_Calc = 100;
			AssertEquals(600m, CollectionWithACost.JR_OSCostGSTAmtSum);
		}

		public void TestCollectionSuspendsValidationOnelements()
		{
			CollectionWithACost.AddNew();
			Assert(CollectionWithACost.Count == 1);
			CollectionWithACost[0].Validation.ValidateAll();
			Assert(CollectionWithACost[0].HasErrors);
			using (CollectionWithACost[0].SuspendValidationTesting())
			{
				CollectionWithACost[0].ClearAllNotifications();
			}

			Assert(!CollectionWithACost[0].HasErrors);
			using (CollectionWithACost.GetApportionmentSplitChargeCollectionValidationSuspender())
			{
				CollectionWithACost[0].Validation.ValidateAll();
			}

			Assert(!CollectionWithACost[0].HasErrors);
			CollectionWithACost[0].Validation.ValidateAll();
			Assert(CollectionWithACost[0].HasErrors);
		}

		public void TestAddDefaultChargeForJobWithoutCurrency()
		{
			var creator = new TestObjectCreator(Factory);
			var job = creator.CreateJob("S000001", creator.LocalClient, 0m, creator.Agent, 0m);
			Assert(CollectionWithACost.Master.E6_RX_NKCurrency.IsEmpty);
			var charge = CollectionWithACost.AddDefaultChargeForJob(job);
			AssertNotEquals("charge exchange rate should not be updated with the master exchange rate if the master currency is null", 0, charge.JR_OSCostExRate);
		}

		public void TestAddDefaultChargeForJob()
		{
			AssertEquals("Pre-codition", false, AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value);

			AssertEquals("No Charges", 0, CollectionWithACost.Count);
			var job = TestObjectCreator.CreateJob("S000002", TestObjectCreator.LocalClient, 0m, TestObjectCreator.Agent, 0m);
			CollectionWithACost.Master.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			CollectionWithACost.Master.E6_SupplyType = "LOC";
			CollectionWithACost.Master.E6_A9_VATClass = TestObjectCreator.TaxMsg1.PK;
			var charge = CollectionWithACost.AddDefaultChargeForJob(job);
			AssertNotNull(charge);
			AssertEquals("One Charge", 1, CollectionWithACost.Count);
			AssertEquals(charge.PK, CollectionWithACost[0].PK);

			var propertiesNotNeedToSet = new List<string>
			{
				JobChargeSchema.Constants.JR_A9_SellVATClass,
				JobChargeSchema.Constants.JR_AgentDeclaredCostAmt,
				JobChargeSchema.Constants.JR_AgentDeclaredSellAmt,
				JobChargeSchema.Constants.JR_AL_APLine,
				JobChargeSchema.Constants.JR_AL_ARLine,
				JobChargeSchema.Constants.JR_AL_CFXLine,
				JobChargeSchema.Constants.JR_APLinePostingStatus,
				JobChargeSchema.Constants.JR_APNumberOfSupportingDocuments,
				JobChargeSchema.Constants.JR_ARLinePostingStatus,
				JobChargeSchema.Constants.JR_ARNumberOfSupportingDocuments,
				JobChargeSchema.Constants.JR_AT_SellGSTRate,
				JobChargeSchema.Constants.JR_AW_CostWHTRate,
				JobChargeSchema.Constants.JR_AW_SellWHTRate,
				JobChargeSchema.Constants.JR_CAL_ARLine,
				JobChargeSchema.Constants.JR_CAL_APLine,
				JobChargeSchema.Constants.JR_ChargeType,
				JobChargeSchema.Constants.JR_CostRated,
				JobChargeSchema.Constants.JR_CostRatingOverride,
				JobChargeSchema.Constants.JR_CostRatingOverrideComment,
				JobChargeSchema.Constants.JR_CostReference,
				JobChargeSchema.Constants.JR_CostTaxDate,
				JobChargeSchema.Constants.JR_DeclaredOSCostAmt,
				JobChargeSchema.Constants.JR_Desc,
				JobChargeSchema.Constants.JR_DisplaySequence,
				JobChargeSchema.Constants.JR_E6_GatewaySellHeader,
				JobChargeSchema.Constants.JR_EstimatedCost,
				JobChargeSchema.Constants.JR_EstimatedRevenue,
				JobChargeSchema.Constants.JR_GB_CostTaxBranch,
				JobChargeSchema.Constants.JR_GB_InternalBranch,
				JobChargeSchema.Constants.JR_GB_SellTaxBranch,
				JobChargeSchema.Constants.JR_GC,
				JobChargeSchema.Constants.JR_GE_InternalDept,
				JobChargeSchema.Constants.JR_InvoiceType,
				JobChargeSchema.Constants.JR_IsAPCashAdvance,
				JobChargeSchema.Constants.JR_IsARCashAdvance,
				JobChargeSchema.Constants.JR_IsCostTaxAmountOverridden,
				JobChargeSchema.Constants.JR_IsIncludedInProfitShare,
				JobChargeSchema.Constants.JR_IsValid,
				JobChargeSchema.Constants.JR_JH_InternalJob,
				JobChargeSchema.Constants.JR_JR_RevenueLine,
				JobChargeSchema.Constants.JR_LineCFX,
				JobChargeSchema.Constants.JR_LineType,
				JobChargeSchema.Constants.JR_LocalCostAmt,
				JobChargeSchema.Constants.JR_LocalSellAmt,
				JobChargeSchema.Constants.JR_MarginPercentage,
				JobChargeSchema.Constants.JR_OA_SellInvoiceAddress,
				JobChargeSchema.Constants.JR_OC_SellInvoiceContact,
				JobChargeSchema.Constants.JR_OH_SellAccount,
				JobChargeSchema.Constants.JR_OP_Product,
				JobChargeSchema.Constants.JR_OrderReference,
				JobChargeSchema.Constants.JR_OSCostAmt,
				JobChargeSchema.Constants.JR_OSCostGSTAmt,
				JobChargeSchema.Constants.JR_OSCostWHTAmt,
				JobChargeSchema.Constants.JR_OSSellAmt,
				JobChargeSchema.Constants.JR_OSSellExRate,
				JobChargeSchema.Constants.JR_OSSellWHTAmt,
				JobChargeSchema.Constants.JR_PreventInvoicePrintGrouping,
				JobChargeSchema.Constants.JR_ProductQuantity,
				JobChargeSchema.Constants.JR_ProFormaCost,
				JobChargeSchema.Constants.JR_ProFormaRevenue,
				JobChargeSchema.Constants.JR_RX_NKSellCurrency,
				JobChargeSchema.Constants.JR_RX_NKSellInvoiceCurrency,
				JobChargeSchema.Constants.JR_SellPlaceOfSupply,
				JobChargeSchema.Constants.JR_SellPlaceOfSupplyType,
				JobChargeSchema.Constants.JR_SellRated,
				JobChargeSchema.Constants.JR_SellRatingOverride,
				JobChargeSchema.Constants.JR_SellRatingOverrideComment,
				JobChargeSchema.Constants.JR_SellReference,
				JobChargeSchema.Constants.JR_SellSupplyType,
				JobChargeSchema.Constants.JR_SellTaxDate,
				JobChargeSchema.Constants.JR_SystemCreateTimeUtc,
				JobChargeSchema.Constants.JR_SystemCreateUser,
				JobChargeSchema.Constants.JR_SystemLastEditTimeUtc,
				JobChargeSchema.Constants.JR_SystemLastEditUser,
				JobChargeSchema.Constants.JR_IsSpotCost
			};

			var expectValueList = new Dictionary<string, object>
			{
				{ JobChargeSchema.Constants.JR_JH, job.PK },
				{ JobChargeSchema.Constants.JR_GB, job.JH_GB },
				{ JobChargeSchema.Constants.JR_GE, job.JH_GE },
				{ JobChargeSchema.Constants.JR_AC, CollectionWithACost.Master.E6_AC_ChargeCode },
				{ JobChargeSchema.Constants.JR_RX_NKCostCurrency, CollectionWithACost.Master.E6_RX_NKCurrency },
				{ JobChargeSchema.Constants.JR_OSCostExRate, CollectionWithACost.Master.E6_ExchangeRate },
				{ JobChargeSchema.Constants.JR_E6, CollectionWithACost.Master.PK },
				{ JobChargeSchema.Constants.JR_OH_CostAccount, CollectionWithACost.Master.E6_OH_Creditor },
				{ JobChargeSchema.Constants.JR_APInvoiceNum, CollectionWithACost.Master.E6_InvoiceNum },
				{ JobChargeSchema.Constants.JR_APInvoiceDate, CollectionWithACost.Master.E6_InvoiceDate },
				{ JobChargeSchema.Constants.JR_APDocumentReceivedDate, CollectionWithACost.Master.E6_DocumentReceivedDate },
				{ JobChargeSchema.Constants.JR_AT_CostGSTRate, CollectionWithACost.Master.E6_AT_TaxRate },
				{ JobChargeSchema.Constants.JR_PaymentDate, CollectionWithACost.Master.E6_PaymentDate },
				{ JobChargeSchema.Constants.JR_PaymentType, CollectionWithACost.Master.E6_PaymentType },
				{ JobChargeSchema.Constants.JR_AB, CollectionWithACost.Master.E6_AB_BankAccount },
				{ JobChargeSchema.Constants.JR_AK, CollectionWithACost.Master.E6_AK_ChequeBook },
				{ JobChargeSchema.Constants.JR_ChequeNo, CollectionWithACost.Master.E6_ChequeOrReference },
				{ JobChargeSchema.Constants.JR_CostPlaceOfSupply, CollectionWithACost.Master.E6_PlaceOfSupply },
				{ JobChargeSchema.Constants.JR_CostGovtChargeCode, ZString.Empty },
				{ JobChargeSchema.Constants.JR_SellGovtChargeCode, ZString.Empty },
				{ JobChargeSchema.Constants.JR_CostPlaceOfSupplyType, CollectionWithACost.Master.E6_PlaceOfSupplyType },
				{ JobChargeSchema.Constants.JR_CostSupplyType, CollectionWithACost.Master.E6_SupplyType },
				{ JobChargeSchema.Constants.JR_A9_CostVATClass, CollectionWithACost.Master.E6_A9_VATClass },
			};

			foreach (var property in charge.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(info => info.IsPersistent))
			{
				if (!propertiesNotNeedToSet.Contains(property.Name))
				{
					if (expectValueList.Keys.Contains(property.Name))
					{
						var result = charge.GetPropertyValue(property.Name);
						AssertEquals(property.Name, result, expectValueList[property.Name]);
					}
					else
					{
						Assert($"{property.Name} should be set in AddDefaultChargeForJob. Please set {property.Name} in AddDefaultChargeForJob and add in expectValueList.", false);
					}
				}
			}
		}

		public void TestAddDefaultChargeForJob_WithGovtChargeCodeRegistryOn()
		{
			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var job = TestObjectCreator.CreateJob("S000001", TestObjectCreator.LocalClient, 0m, TestObjectCreator.Agent, 0m);
			CollectionWithACost.Master.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			var charge = CollectionWithACost.AddDefaultChargeForJob(job);
			AssertEquals("JR_CostGovtChargeCode", CollectionWithACost.Master.ChargeCode.AC_GovtChargeCode, charge.JR_CostGovtChargeCode);
			AssertEquals("JR_SellGovtChargeCode", CollectionWithACost.Master.ChargeCode.AC_GovtChargeCode, charge.JR_SellGovtChargeCode);
		}

		public void TestAddDefaultChargeForJobWithJR_E6_GatewaySellHeader()
		{
			var gatewayAgentPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(gatewayAgentPort);
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			using (var job = new Job.Loader(Factory, consol).TryCreateWithoutMutexForTestOnly())
			{
				var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));
				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;
				charge.JR_OSSellAmt = 250m;
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				Assert(consol.IsGateway());
				Factory.Save();
				AssertEquals(1, consol.GetApportionments(true).CostsCollection.Count);
				var cost = consol.GetApportionments(true).CostsCollection[0];
				Assert(cost.IsGatewayConsolCost);
				AssertEquals(1, cost.ApportionmentCharges.Count);
				var charge2 = cost.ApportionmentCharges.AddDefaultChargeForJob(job);
				AssertEquals(cost.PK, charge2.JR_E6_GatewaySellHeader);
			}
		}

		public void TestAllowRemove()
		{
			Assert("IsUsedForApportment tick box is used instead of removing charges.", !CollectionWithACost.AllowRemove);
		}

		public void TestAllowNew()
		{
			Assert("Charges are added according to shipments attached in consol. No any other charges allowed.", !CollectionWithACost.AllowNew);
		}

		public void TestAddDefaultChargeForJob_JR_CostTaxDate()
		{
			AssertEquals("No Charges", 0, CollectionWithACost.Count);
			var job = TestObjectCreator.CreateJob("S000001", TestObjectCreator.LocalClient, 0m, TestObjectCreator.Agent, 0m);
			CollectionWithACost.Master.E6_AT_TaxRate = TestObjectCreator.VATSPV.PK;
			CollectionWithACost.Master.E6_TaxDate = ZDate.Today.AddDays(5);

			var charge = CollectionWithACost.AddDefaultChargeForJob(job);
			AssertNotNull(charge);
			AssertEquals("One Charge", 1, CollectionWithACost.Count);
			AssertEquals(charge.PK, CollectionWithACost[0].PK);
			AssertEquals("JR_AT_CostGSTRate", CollectionWithACost.Master.E6_AT_TaxRate, charge.JR_AT_CostGSTRate);
			AssertEquals("JR_CostTaxDate", CollectionWithACost.Master.E6_TaxDate, charge.JR_CostTaxDate);
		}

		public void TestAddDefaultChargeForJob_TaxBranch()
		{
			AssertEquals("No Charges", 0, CollectionWithACost.Count);
			var job = TestObjectCreator.CreateJob("S000001", TestObjectCreator.LocalClient, 0m, TestObjectCreator.Agent, 0m);
			job.JH_GB_TaxBranch = GlbBranch.CurrentBranch.PK;
			CollectionWithACost.Master.E6_GB_CostTaxBranch = TestObjectCreator.NonCurrentBranch.PK;

			AssertAddDefaultChargeForJob_TaxBranch(true);
			AssertAddDefaultChargeForJob_TaxBranch(false);

			void AssertAddDefaultChargeForJob_TaxBranch(bool enableTaxBranchReporting)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableTaxBranchReporting))
				{
					job.LocalChargesPK = TestObjectCreator.LocalClient.PK;
					var charge = CollectionWithACost.AddDefaultChargeForJob(job);
					AssertEquals("Precondition", true, charge.IsSellGSTApplicable);
					AssertEquals("JR_GB_CostTaxBranch", enableTaxBranchReporting ? TestObjectCreator.NonCurrentBranch.PK : ZGuid.Empty, charge.JR_GB_CostTaxBranch);
					AssertEquals("JR_GB_SellTaxBranch", enableTaxBranchReporting ? GlbBranch.CurrentBranch.PK : ZGuid.Empty, charge.JR_GB_SellTaxBranch);

					job.LocalChargesPK = ZGuid.Empty;
					charge = CollectionWithACost.AddDefaultChargeForJob(job);
					AssertEquals("Precondition", false, charge.IsSellGSTApplicable);
					AssertEquals("JR_GB_CostTaxBranch", enableTaxBranchReporting ? TestObjectCreator.NonCurrentBranch.PK : ZGuid.Empty, charge.JR_GB_CostTaxBranch);
					AssertEquals("JR_GB_SellTaxBranch", enableTaxBranchReporting ? ZGuid.Empty : ZGuid.Empty, charge.JR_GB_SellTaxBranch);
				}
			}
		}

		[TestDate(2021, 06, 29)]
		public void TestAreInvoiceDetailsInSyncWithPostedInvoiceLines_JR_CostTaxDate2()
		{
			//Creating Consol
			var consol = TestObjectCreator.CreateConsol("", "", "TSTCNSL");
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUMEL";
			var shipment1 = TestObjectCreator.CreateShipment("S00010001", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment1);
			Job job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			var shipment2 = TestObjectCreator.CreateShipment("S00010002", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment2);
			Job job2 = new Job.Loader(Factory, shipment2).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			var apportionmentListing = consol.GetApportionments();
			//Cost and charges are linked to correct APInvoice with sync Information
			var cost = CreateConsolCost(apportionmentListing);
			var invoice3 = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1.0m, TestObjectCreator.AALSHI) as APInvoice;
			invoice3.AH_TransactionNum = "ABC123";
			invoice3.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice3.AH_InvoiceDate = ZDateTime.Today;
			invoice3.AH_DueDate = ZDateTime.Today.AddDays(2);
			invoice3.AH_TransactionReference = "COST";
			var apline1 = TestObjectCreator.CreateAPInvoiceLine(invoice3, cost.ApportionmentCharges[0].Job as Job, cost.ChargeCode, TestObjectCreator.AUD, 1m, "Test Line 1", 100m);
			apline1.AL_AT = cost.ApportionmentCharges[0].JR_AT_CostGSTRate;
			apline1.AL_A9_VATClass = cost.ApportionmentCharges[0].JR_A9_CostVATClass;
			var apline2 = TestObjectCreator.CreateAPInvoiceLine(invoice3, cost.ApportionmentCharges[1].Job as Job, cost.ChargeCode, TestObjectCreator.AUD, 1m, "Test Line 2", 110m);
			apline2.AL_AT = cost.ApportionmentCharges[1].JR_AT_CostGSTRate;
			apline2.AL_A9_VATClass = cost.ApportionmentCharges[1].JR_A9_CostVATClass;
			cost.E6_AH_APInvoice = invoice3.PK;
			cost.ApportionmentCharges[0].JR_AL_APLine = invoice3.Lines[0].PK;
			cost.ApportionmentCharges[1].JR_AL_APLine = invoice3.Lines[1].PK;

			AssertEquals(true, cost.ApportionmentCharges[0].JR_CostTaxDate.IsEmpty);
			AssertEquals(true, cost.ApportionmentCharges[1].JR_CostTaxDate.IsEmpty);
			AssertEquals(ZDate.Today, invoice3.Lines[0].AL_TaxDate);
			AssertEquals(ZDate.Today, invoice3.Lines[1].AL_TaxDate);
			AssertEquals(false, cost.IsPostedCorrectly);
			AssertEquals(false, cost.ApportionmentCharges.AreInvoiceDetailsInSyncWithPostedInvoiceLines);

			cost.E6_TaxDate = ZDate.Today;
			AssertEquals(ZDate.Today, cost.ApportionmentCharges[0].JR_CostTaxDate);
			AssertEquals(ZDate.Today, cost.ApportionmentCharges[1].JR_CostTaxDate);
			AssertEquals(ZDate.Today, invoice3.Lines[0].AL_TaxDate);
			AssertEquals(ZDate.Today, invoice3.Lines[1].AL_TaxDate);
			AssertEquals(true, cost.IsPostedCorrectly);
			AssertEquals(true, cost.ApportionmentCharges.AreInvoiceDetailsInSyncWithPostedInvoiceLines);

			cost.E6_TaxDate = ZDate.Today.AddDays(4);
			AssertEquals(ZDate.Today.AddDays(4), cost.ApportionmentCharges[0].JR_CostTaxDate);
			AssertEquals(ZDate.Today.AddDays(4), cost.ApportionmentCharges[1].JR_CostTaxDate);
			AssertEquals(ZDate.Today, invoice3.Lines[0].AL_TaxDate);
			AssertEquals(ZDate.Today, invoice3.Lines[1].AL_TaxDate);
			AssertEquals(false, cost.IsPostedCorrectly);
			AssertEquals(false, cost.ApportionmentCharges.AreInvoiceDetailsInSyncWithPostedInvoiceLines);
		}

		public void TestAreInvoiceDetailsInSyncWithConsolCost_PlaceOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var cost = GetSyncCostForTestPlaceOfSupplyType();
				Assert(cost.IsPostedCorrectly);

				var consolCostPK = cost.ApportionmentCharges[0].JR_E6;
				cost.ApportionmentCharges[0].JR_E6 = ZGuid.Empty;
				cost.ApportionmentCharges[0].JR_CostPlaceOfSupply = "JH";
				cost.ApportionmentCharges[0].JR_E6 = consolCostPK;
				Assert(!cost.IsPostedCorrectly);
			}
		}

		public void TestAreInvoiceDetailsInSyncWithConsolCost_DocumentReceivedDate()
		{
			var cost = GetSyncCostCore();
			Assert(cost.IsPostedCorrectly);

			var consolCostPK = cost.ApportionmentCharges[0].JR_E6;
			cost.ApportionmentCharges[0].JR_E6 = ZGuid.Empty;
			cost.ApportionmentCharges[0].JR_APDocumentReceivedDate = ZDateTime.Today;
			cost.ApportionmentCharges[0].JR_E6 = consolCostPK;
			Assert(!cost.IsPostedCorrectly);
		}

		public void TestAreInvoiceDetailsInSyncWithConsolCost_PlaceOfSupplyType()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var cost = GetSyncCostForTestPlaceOfSupplyType();
				Assert(cost.IsPostedCorrectly);

				var consolCostPK = cost.ApportionmentCharges[0].JR_E6;
				cost.ApportionmentCharges[0].JR_E6 = ZGuid.Empty;
				cost.ApportionmentCharges[0].JR_CostPlaceOfSupplyType = PlaceOfSupplyTypes.PredefinedRule.Code;
				cost.ApportionmentCharges[0].JR_E6 = consolCostPK;
				Assert(!cost.IsPostedCorrectly);
			}
		}

		public void TestAreInvoiceDetailsInSyncWithPostedInvoiceLines_PlaceOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var cost = GetSyncCostForTestPlaceOfSupplyType();
				Assert(cost.IsPostedCorrectly);

				var consolCostPK = cost.ApportionmentCharges[0].JR_E6;
				cost.ApportionmentCharges[0].JR_E6 = ZGuid.Empty;
				cost.ApportionmentCharges[0].APLine.AL_PlaceOfSupply = "JH";
				cost.ApportionmentCharges[0].JR_E6 = consolCostPK;
				AssertEquals("DL", cost.ApportionmentCharges[0].JR_CostPlaceOfSupply);
				Assert(!cost.IsPostedCorrectly);
			}
		}

		public void TestAreInvoiceDetailsInSyncWithPostedInvoiceLines_DocumentReceivedDate()
		{
			var cost = GetSyncCostCore();
			Assert(cost.IsPostedCorrectly);

			var consolCostPK = cost.ApportionmentCharges[0].JR_E6;
			cost.ApportionmentCharges[0].JR_E6 = ZGuid.Empty;
			cost.ApportionmentCharges[0].APLine.TransactionHeader.AH_DocumentReceivedDate = ZDateTime.Today;
			cost.ApportionmentCharges[0].JR_E6 = consolCostPK;
			AssertEquals(ZDateTime.Empty, cost.ApportionmentCharges[0].JR_APDocumentReceivedDate);
			Assert(!cost.IsPostedCorrectly);
		}

		public void TestAreInvoiceDetailsInSyncWithPostedInvoiceLines_PlaceOfSupplyType()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var cost = GetSyncCostForTestPlaceOfSupplyType();
				Assert(cost.IsPostedCorrectly);

				var consolCostPK = cost.ApportionmentCharges[0].JR_E6;
				cost.ApportionmentCharges[0].JR_E6 = ZGuid.Empty;
				cost.ApportionmentCharges[0].APLine.AL_PlaceOfSupplyType = PlaceOfSupplyTypes.PredefinedRule.Code;
				cost.ApportionmentCharges[0].JR_E6 = consolCostPK;
				AssertEquals(PlaceOfSupplyTypes.State.Code, cost.ApportionmentCharges[0].JR_CostPlaceOfSupplyType);
				Assert(!cost.IsPostedCorrectly);
			}
		}

		public void TestAreInvoiceDetailsInSyncWithConsolCost_TaxBranch()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();

			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var cost = GetSyncCostCore();
				Assert(cost.IsPostedCorrectly);

				var consolCostPK = cost.ApportionmentCharges[0].JR_E6;
				cost.ApportionmentCharges[0].JR_E6 = ZGuid.Empty;
				cost.ApportionmentCharges[0].JR_GB_CostTaxBranch = branch1.PK;
				cost.ApportionmentCharges[0].JR_E6 = consolCostPK;

				Assert(!cost.IsPostedCorrectly);
			}
		}

		public void TestAreInvoiceDetailsInSyncWithPostedInvoiceLines_TaxBranch()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();

			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var cost = GetSyncCostCore();
				Assert(cost.IsPostedCorrectly);

				var consolCostPK = cost.ApportionmentCharges[0].JR_E6;
				cost.ApportionmentCharges[0].JR_E6 = ZGuid.Empty;
				cost.ApportionmentCharges[0].APLine.AL_GB_TaxBranch = branch1.PK;
				cost.ApportionmentCharges[0].JR_E6 = consolCostPK;

				Assert(!cost.IsPostedCorrectly);
			}
		}

		JobConsolCost GetSyncCostForTestPlaceOfSupplyType()
		{
			var cost = GetSyncCostCore();

			AssertEquals(PlaceOfSupplyTypes.State.Code, cost.E6_PlaceOfSupplyType);

			AssertEquals(1, cost.ApportionmentCharges.Count);
			AssertEquals("DL", cost.ApportionmentCharges[0].JR_CostPlaceOfSupply);
			Assert(cost.IsPostedCorrectly);

			return cost;
		}

		JobConsolCost GetSyncCostCore()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var invoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.USD, 2, TestObjectCreator.AALSHI);
			invoice.AH_TransactionNum = "ABC123";
			invoice.AH_InvoiceDate = ZDateTime.Today;
			invoice.AH_DueDate = ZDateTime.Today.AddDays(2);

			var cost = TestObjectCreator.CreateConsolCost(invoice, consol, TestObjectCreator.CC1, 100, TestObjectCreator.AALSHI);
			cost.E6_AH_APInvoice = invoice.PK;
			cost.E6_PlaceOfSupply = "DL";

			var apline1 = TestObjectCreator.CreateAPInvoiceLine(invoice, cost.ApportionmentCharges[0].Job as Job, cost.ChargeCode, TestObjectCreator.AUD, 1m, "Test Line 1", 100m);
			apline1.AL_PlaceOfSupply = cost.E6_PlaceOfSupply;
			apline1.AL_PlaceOfSupplyType = cost.E6_PlaceOfSupplyType;
			cost.ApportionmentCharges[0].JR_AL_APLine = invoice.Lines[0].PK;

			AssertEquals(1, cost.ApportionmentCharges.Count);
			Assert(cost.IsPostedCorrectly);

			return cost;
		}

		[TestDate(2021, 06, 29)]
		public void TestAreInvoiceDetailsInSyncWithConsolCost_JR_CostTaxDate()
		{
			//Creating Consol
			var consol = TestObjectCreator.CreateConsol("", "", "TSTCNSL");
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUMEL";
			var shipment1 = TestObjectCreator.CreateShipment("S00010001", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment1);
			Job job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			var shipment2 = TestObjectCreator.CreateShipment("S00010002", "USLAX", "AUMEL");
			consol.Shipments.Add(shipment2);
			Job job2 = new Job.Loader(Factory, shipment2).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			var apportionmentListing = consol.GetApportionments();
			//Cost and charges are linked to correct APInvoice with sync Information
			var cost = CreateConsolCost(apportionmentListing);
			var invoice3 = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1.0m, TestObjectCreator.AALSHI) as APInvoice;
			invoice3.AH_TransactionNum = "ABC123";
			invoice3.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice3.AH_InvoiceDate = ZDateTime.Today;
			invoice3.AH_DueDate = ZDateTime.Today.AddDays(2);
			invoice3.AH_TransactionReference = "COST";
			var apline1 = TestObjectCreator.CreateAPInvoiceLine(invoice3, cost.ApportionmentCharges[0].Job as Job, cost.ChargeCode, TestObjectCreator.AUD, 1m, "Test Line 1", 100m);
			apline1.AL_AT = cost.ApportionmentCharges[0].JR_AT_CostGSTRate;
			apline1.AL_A9_VATClass = cost.ApportionmentCharges[0].JR_A9_CostVATClass;
			var apline2 = TestObjectCreator.CreateAPInvoiceLine(invoice3, cost.ApportionmentCharges[1].Job as Job, cost.ChargeCode, TestObjectCreator.AUD, 1m, "Test Line 2", 110m);
			apline2.AL_AT = cost.ApportionmentCharges[1].JR_AT_CostGSTRate;
			apline2.AL_A9_VATClass = cost.ApportionmentCharges[1].JR_A9_CostVATClass;
			cost.E6_AH_APInvoice = invoice3.PK;
			cost.ApportionmentCharges[0].JR_AL_APLine = invoice3.Lines[0].PK;
			cost.ApportionmentCharges[1].JR_AL_APLine = invoice3.Lines[1].PK;

			AssertEquals(true, cost.ApportionmentCharges[0].JR_CostTaxDate.IsEmpty);
			AssertEquals(true, cost.ApportionmentCharges[1].JR_CostTaxDate.IsEmpty);
			AssertEquals(true, cost.E6_TaxDate.IsEmpty);
			AssertEquals(true, cost.ApportionmentCharges.AreInvoiceDetailsInSyncWithConsolCost);
			AssertEquals(false, cost.IsPostedCorrectly);

			cost.ApportionmentCharges[0].JR_CostTaxDate = ZDate.Today.AddDays(4);
			AssertEquals(true, cost.E6_TaxDate.IsEmpty);
			AssertEquals(ZDate.Today.AddDays(4), cost.ApportionmentCharges[0].JR_CostTaxDate);
			AssertEquals(false, cost.ApportionmentCharges.AreInvoiceDetailsInSyncWithConsolCost);
			AssertEquals(false, cost.IsPostedCorrectly);

			cost.E6_TaxDate = ZDate.Today;
			AssertEquals(ZDate.Today, cost.ApportionmentCharges[0].JR_CostTaxDate);
			AssertEquals(ZDate.Today, cost.ApportionmentCharges[1].JR_CostTaxDate);
			AssertEquals(true, cost.ApportionmentCharges.AreInvoiceDetailsInSyncWithConsolCost);
			AssertEquals(true, cost.IsPostedCorrectly);
		}

		public void TestSetCorrectValueForIsUsedForApportionmentWhenTheCollectionAddsAnElement()
		{
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_GC = GlbCompany.CurrentCompany.PK;
			var jobCharge1 = job1.Charges.AddNew();
			jobCharge1.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge1.JR_AC = Env.Registry.FreightChargeCode;
			jobCharge1.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge1.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			Factory.Save();
			var cost = Factory.NewWithValidTestData<JobConsolCost>();
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			var appCharge1 = Factory.Load<ApportionSplitCharge>(jobCharge1.PK);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			appCharge1.SetShipmentInfo(shipment);
			Assert("Precondition", !appCharge1.JR_IsUsedForApportionment);
			cost.ApportionmentCharges.Add(appCharge1);

			Assert("Precondition", appCharge1.ShouldIncludeInApportionment);
			Assert(appCharge1.JR_IsUsedForApportionment);
		}

		JobConsolCost CreateConsolCost(ApportionmentListing apportionmentListing)
		{
			var cost = apportionmentListing.CostsCollection.TryAddNew();
			if (cost != null)
			{
				cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
				cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.ChargeableUnits;
				cost.E6_OSCostAmount = 100m;
				cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
				cost.E6_InvoiceNum = "ABC123";
				cost.E6_InvoiceDate = ZDateTime.Today;
				cost.E6_PaymentDate = ZDateTime.Today.AddDays(2);
				cost.E6_CostReference = "COST";
				cost.E6_RX_NKCurrency = "AUD";
				cost.E6_AT_TaxRate = TestObjectCreator.GSTFREE1.PK;
				cost.E6_A9_VATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			}

			return cost;
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		TestObjectCreator TestObjectCreator;
	}
}
