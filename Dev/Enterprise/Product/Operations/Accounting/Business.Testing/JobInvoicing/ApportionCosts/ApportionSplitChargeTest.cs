using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing.GatewayBilling.Testing;
using Enterprise.Accounting.Business.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using PrepaidCollectCodes = Enterprise.Accounting.Integration.PrepaidCollectFreightForwardingList.Codes;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(ApportionSplitCharge))]
	public class ApportionSplitChargeTest : BaseCharge_InnerTest
	{
		public override void TestJR_GB_CostTaxBranch_ReadOnly()
		{
			var charge = Factory.New<ApportionSplitCharge>();
			AssertEquals(true, charge.JR_GB_CostTaxBranchInfo.ReadOnly);
		}

		public override void TestJR_GB_SellTaxBranch_ReadOnly()
		{
			var charge = Factory.New<ApportionSplitCharge>();
			AssertEquals(true, charge.JR_GB_SellTaxBranchInfo.ReadOnly);
		}

		public void TestJR_SellSupplyType_ReadOnly()
		{
			var charge = Factory.New<ApportionSplitCharge>();
			AssertEquals(true, charge.JR_SellSupplyTypeInfo.ReadOnly);
		}

		public void TestJR_CostSupplyType_ReadOnly()
		{
			var charge = Factory.New<ApportionSplitCharge>();
			AssertEquals(true, charge.JR_CostSupplyTypeInfo.ReadOnly);
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesApportionSplitCharge()
		{
			var charge = Factory.New<ApportionSplitCharge>();

			var weightList = new List<string>
			{
				nameof(charge.GrossWeight),
				nameof(charge.ChargeableUnits)
			};

			var tester = new DecimalPlacesAttributeTester(charge, charge.Company);
			tester.CheckConstant(weightList, nameof(charge.WeightVolumeDecimals), DefaultNumberOfDecimals.Schema.DefaultNumberOfDecimalsForWeightAndVolumeUnits);
		}

		public void TestChargeableRate()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			shipment.JS_ActualChargeable = 100.0m;
			TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100);
			var charge = consolCost.ApportionmentCharges[0];
			AssertEquals("Precondition: ChargeableUnits", 100m, charge.ChargeableUnits);
			Assert("Precondition: IsApportionmentMethodPerChargeableUnit", ApportionmentCreator.IsApportionmentMethodPerChargeableUnit(consolCost.E6_ApportionmentMethod));

			charge.JR_OSCostAmt = 150;
			AssertEquals("Precondition: JR_OSCostAmt", 150m, charge.JR_LocalCostAmt);
			AssertEquals("1.5", charge.ChargeableRate);

			consolCost.E6_ApportionmentMethod = AllocationMethod.Shipment;
			AssertEquals("Not Applicable", charge.ChargeableRate);

			consolCost.E6_ApportionmentMethod = AllocationMethod.CapacityPerContainer;
			Assert("Precondition: IsApportionmentMethodPerChargeableUnit", ApportionmentCreator.IsApportionmentMethodPerChargeableUnit(consolCost.E6_ApportionmentMethod));
			charge.JR_OSCostAmt = 0;
			AssertEquals("Precondition: JR_OSCostAmt", 0m, charge.JR_LocalCostAmt);
			AssertEquals("0", charge.ChargeableRate);

			shipment.JS_ActualChargeable = 0;
			AssertEquals("0", charge.ChargeableRate);

			charge.JR_OSCostAmt = 10;
			AssertEquals("Precondition: JR_OSCostAmt", 10m, charge.JR_LocalCostAmt);
			AssertEquals("0", charge.ChargeableRate);

			shipment.JS_ActualChargeable = 20;
			AssertEquals("0.5", charge.ChargeableRate);
		}

		public override void TestCostRatingOverrideComment()
		{
			ApportionSplitCharge testSplitCharge = Factory.NewWithValidTestData<ApportionSplitCharge>();
			testSplitCharge.JR_OSCostAmt = 10;

			((ApportionSplitChargeValidation)testSplitCharge.Validation).ValidateJR_CostRatingOverrideComment();
			AssertNoNotifications(testSplitCharge.JR_CostRatingOverrideCommentInfo);
		}

		#region JR_EstimatedCost

		public override void TestJR_EstimatedCost_ReadOnly()
		{
			base.TestJR_EstimatedCost_ReadOnly();

			var charge = (ApportionSplitCharge)GetNewBusinessObject();
			Assert(charge.JR_EstimatedCostInfo.ReadOnly);
			charge.JR_IsUsedForApportionment = true;
			Assert(!charge.JR_EstimatedCostInfo.ReadOnly);
		}

		protected override void SetupChargeForTestJR_EstimatedCost_ReadOnly(BaseCharge charge)
		{
			((ApportionSplitCharge)charge).JR_IsUsedForApportionment = true;
		}

		public void TestChargeCodeChangeResetsJR_EstimatedCost()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = (BaseCharge)GetNewBusinessObject();
			charge.JR_JH = job.PK;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OSCostAmt = 10;
			charge.JR_EstimatedCost = 20;

			var expectedJR_EstimatedCost = 10m;
			AssertNotEquals("Precondition: JR_EstimatedCost", expectedJR_EstimatedCost, charge.JR_EstimatedCost);
			charge.JR_AC = TestObjectCreator.CC2.PK;
			AssertEquals("JR_EstimatedCost", expectedJR_EstimatedCost, charge.JR_EstimatedCost);

			charge.JR_OSCostAmt = 10;
			charge.JR_EstimatedCost = 20;
			Factory.Save();
			Assert("Precondition: IsInDatabase", charge.IsInDatabase);
			AssertNotEquals("Precondition: JR_EstimatedCost", expectedJR_EstimatedCost, charge.JR_EstimatedCost);
			charge.JR_AC = TestObjectCreator.CC1.PK;
			AssertEquals("JR_EstimatedCost", expectedJR_EstimatedCost, charge.JR_EstimatedCost);
		}

		#endregion

		[ExpectNoExceptions]
		[TestDate(2009, 1, 1, 10, 0, 0, 0)]
		public void TestApportionmentConcurrency()
		{
			BusinessObjectFactory factoryInSession1 = new BusinessObjectFactory();
			BusinessObjectFactory factoryInSession2 = new BusinessObjectFactory();
			factoryInSession1.RefreshEnabled = false;
			factoryInSession2.RefreshEnabled = false;

			ForwardingConsol consol = factoryInSession2.New<ForwardingConsol>();
			factoryInSession2.Save();
			consol.Shipments.AddNew();
			ApportionmentListing listing = new ApportionmentListing(factoryInSession2, consol);
			JobConsolCost consolCost = listing.CostsCollection.TryAddNew();
			var query = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccChargeCodeSchema.AC_AG_AccrualAccount, SQLComparisonOperator.NotEqual, null);
			consolCost.E6_AC_ChargeCode = Factory.LoadTop1<AccChargeCode>(query).PK;
			consolCost.E6_OSCostAmount = 100m;
			Assert("This should create an apportionment charge", consolCost.ApportionmentCharges.Count == 1);
			factoryInSession2.Save();

			Charge chargeInFactory1 = factoryInSession1.LoadTop1<Charge>(new ZQuery(JobChargeSchema.PK, consolCost.ApportionmentCharges[0].PK));
			chargeInFactory1.JR_MarginPercentage = 80;
			factoryInSession1.Save();

			consolCost.E6_OSCostAmount = 300m;

			try
			{
				BusinessObjectFactory.SaveTogether(factoryInSession2);
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		public void TestReadOnly()
		{
			UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";

			Factory.Save();

			JobConsolCost cost = Factory.NewWithValidTestData<JobConsolCost>();
			cost.ParentAPInvoice = invoice;
			cost.E6_AH_APInvoice = invoice.PK;

			ApportionSplitCharge firstCharge = cost.ApportionmentCharges.AddNew();
			firstCharge.ReadOnly = false;

			Assert("Read only must be false bacause IsConsolCostApprovingPosting is true", !firstCharge.ReadOnly);
		}

		public void TestBranchAndDepartmentReadOnlyWhenImporting()
		{
			var charge = Factory.NewWithValidTestData<ApportionSplitCharge>();
			Assert(!charge.JR_GBInfo.ReadOnly);
			Assert(!charge.JR_GEInfo.ReadOnly);
			Assert(!charge.JR_IsUsedForApportionmentInfo.ReadOnly);

			charge.RelatedApportionChargeFromDB = Factory.NewWithValidTestData<JobCharge>();
			Assert(charge.JR_GBInfo.ReadOnly);
			Assert(charge.JR_GEInfo.ReadOnly);
			Assert(charge.JR_IsUsedForApportionmentInfo.ReadOnly);

			var consolCost = Factory.NewWithValidTestData<JobConsolCost>();
			consolCost.RelatedConsolCostPK = ZGuid.NewZGuid();
			charge = consolCost.ApportionmentCharges.AddNew();
			Assert(charge.JR_GBInfo.ReadOnly);
			Assert(charge.JR_GEInfo.ReadOnly);
			Assert(charge.JR_IsUsedForApportionmentInfo.ReadOnly);
		}

		public void TestShouldValidateBranchAndDepartment()
		{
			ForwardingShipment coloadMaster = Factory.New<ForwardingShipment>();
			coloadMaster.JS_ShipmentType = Enterprise.Core.Constants.ShipmentTypes.CoLoadMaster;
			coloadMaster.JS_UniqueConsignRef = "S00005551";
			ForwardingShipment subHouseBill = Factory.New<ForwardingShipment>();
			subHouseBill.JS_UniqueConsignRef = "S00005552";
			subHouseBill.JS_JS_ColoadMasterShipment = coloadMaster.PK;
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.Shipments.Add(coloadMaster);
			consol.Shipments.Add(subHouseBill);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				cost.E6_ApportionToRelatedShipments = true;
				ApportionSplitCharge subBillCharge = cost.ApportionmentCharges.FindChargeForJob(subHouseBill);
				ApportionSplitCharge masterCharge = cost.ApportionmentCharges.FindChargeForJob(coloadMaster);

				AssertEquals("SubBillCharge's JR_IsUsedForApportionment set to false", false, subBillCharge.JR_IsUsedForApportionment);
				AssertEquals("Shouldn't validate branch and department", false, subBillCharge.ShouldValidateBranchAndDepartment);
				AssertEquals("MasterCharge's JR_IsUsedForApportionment set to true", true, masterCharge.JR_IsUsedForApportionment);
				AssertEquals("Should validate branch and department", true, masterCharge.ShouldValidateBranchAndDepartment);
			}
			finally
			{
				ErrorReporter.Clear();
				apps.ReleaseMutexes();
			}
		}

		public void TestAutoCorrectWhenIncorrectlySettingLocalValueToZero()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				cost.E6_OSCostAmount = 500m;
				cost.E6_ApportionmentMethod = "SHP";
				cost.E6_ExchangeRate = 0.8m;

				ApportionSplitCharge firstCharge = cost.ApportionmentCharges[0];
				firstCharge.JR_OSCostAmt = 260m;
				AssertEquals(325m, firstCharge.JR_LocalCostAmt);

				firstCharge.JR_LocalCostAmt = 0m;
				AssertEquals("Should just set amount correctly in this case", 325m, firstCharge.JR_LocalCostAmt);
				ErrorReporter.Clear();

				firstCharge.JR_LocalCostAmt = 319m;
				AssertEquals("Should just set amount correctly in this case", 325m, firstCharge.JR_LocalCostAmt);
				ErrorReporter.Clear();

				firstCharge.JR_LocalCostAmt = 321m;
				AssertEquals(321m, firstCharge.JR_LocalCostAmt);

				firstCharge.JR_OSCostExRate = 8000m;
				firstCharge.JR_OSCostAmt = 0.01m;
				AssertEquals(0m, firstCharge.JR_LocalCostAmt);
			}
			finally
			{
				ErrorReporter.Clear();
				apps.ReleaseMutexes();
			}
		}

		public void TestJR_HouseBill()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "Test";
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			ApportionSplitCharge charge = Factory.New<ApportionSplitCharge>();
			charge.JR_JH = job.PK;
			AssertEquals("TEST", charge.JR_HouseBill.ToUpper());
		}

		public override void TestJR_RL_NKOrigin()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = shipment.PK;
			var charge = Factory.New<ApportionSplitCharge>();
			charge.JR_JH = job.PK;
			charge.SetShipmentInfo(shipment);
			AssertEquals("Charge Origin property should read from underlying shipment", "AUSYD", charge.JR_RL_NKOrigin);
		}

		public override void TestJR_RL_NKDestination()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "NZAKL";
			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = shipment.PK;
			var charge = Factory.New<ApportionSplitCharge>();
			charge.JR_JH = job.PK;
			charge.SetShipmentInfo(shipment);
			AssertEquals("Charge Destination property should read from underlying shipment", "NZAKL", charge.JR_RL_NKDestination);
		}

		public void TestIApportionedCharge_GrossWeight()
		{
			ApportionSplitCharge charge = Factory.New<ApportionSplitCharge>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Job job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			shipment.JS_ActualWeight = 100.0m;
			charge.SetShipmentInfo(shipment);
			charge.JR_JH = job.PK;

			AssertEquals("Actual Weight", 100.0m, charge.JR_ActualWeight);
			AssertEquals("Actual Weight", 100.0m, ((IApportionedCharge)charge).GrossWeight);
		}

		public void TestIApportionedCharge_GrossVolume()
		{
			var charge = Factory.New<ApportionSplitCharge>();
			var shipment = Factory.New<ForwardingShipment>();
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			shipment.JS_ActualVolume = 3.0m;
			charge.SetShipmentInfo(shipment);
			charge.JR_JH = job.PK;

			AssertEquals("Actual Volume", 3.0m, charge.JR_ActualVolume);
			AssertEquals("Actual Volume", 3.0m, ((IApportionedCharge)charge).GrossVolume);
		}

		public void TestAmountSignShouldNotHaveToBeTheSameAsParentApportionment()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				AssertEquals("Should have 2 charge lines", 2, cost.ApportionmentCharges.Count);

				ApportionSplitCharge shipment1Charge = cost.ApportionmentCharges.FindChargeForJob(shipment1);
				ApportionSplitCharge shipment2Charge = cost.ApportionmentCharges.FindChargeForJob(shipment2);
				cost.E6_OSCostAmount = 100m;
				shipment1Charge.JR_OSCostAmt = 110m;
				shipment2Charge.JR_OSCostAmt = -10m;
				AssertNoErrors(shipment1Charge.JR_OSCostAmtInfo);
				AssertNoErrors(shipment2Charge.JR_OSCostAmtInfo);

				shipment1Charge.JR_OSCostAmt = 90m;
				shipment2Charge.JR_OSCostAmt = 10m;
				AssertNoErrors(shipment1Charge.JR_OSCostAmtInfo);
				AssertNoErrors(shipment2Charge.JR_OSCostAmtInfo);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		[DisableZeroExchangeRateOverriding]
		public void TestCopyValuesShouldNotUpdateTheCurrencyWithoutTheExchangeRate()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S00001", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			Factory.Save();

			var charge = TestObjectCreator.CreateCharge(job);
			charge.JR_AC = TestObjectCreator.FRT.PK;
			charge.JR_OH_SellAccount = ZGuid.Empty;
			charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(1.5m);

			Factory.Save();

			AssertEquals(TestObjectCreator.AUD.RX_Code, charge.JR_RX_NKCostCurrency);
			Assert(charge.JR_OH_SellAccount.IsEmpty);
			Assert(charge.JR_OH_CostAccount.IsEmpty);
			AssertEquals(0m, charge.JR_LocalCostAmt);
			AssertEquals(0m, charge.JR_LocalCostAmt);
			AssertEquals(1.5m, charge.JR_OSSellExRate);
			AssertEquals(TestObjectCreator.USD.RX_Code, charge.JR_RX_NKSellCurrency);
			AssertEquals(TestObjectCreator.AUD.RX_Code, TestObjectCreator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency);

			var apps = new ApportionmentListing(Factory, consol);

			try
			{
				var cost = apps.CostsCollection.TryAddNew();
				AssertEquals("Should have 1 charge lines", 1, cost.ApportionmentCharges.Count);

				var shipmentCharge = cost.ApportionmentCharges.FindChargeForJob(shipment);
				cost.E6_AC_ChargeCode = TestObjectCreator.FRT.PK;
				cost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
				cost.E6_OSCostAmount = 100m;
				cost.E6_LocalCostAmount = 120m;
				AssertNoErrors(shipmentCharge.JR_OSCostAmtInfo);

				AssertNoExceptionThrown(() => Factory.Save());
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		[DisableZeroExchangeRateOverriding]
		public void TestUpdateRevenueAmount()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.Add(shipment);
			Factory.Save();

			var apportionments = new ApportionmentListing(Factory, consol);
			var consolCost = apportionments.CostsCollection.TryAddNew();

			var apportionCharge = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().FirstOrDefault();
			AssertNotNull(apportionCharge);
			AssertEquals(job.PK, apportionCharge.JR_JH);
			AssertEquals(TestObjectCreator.AUD.RX_Code, apportionCharge.JR_RX_NKCostCurrency);
			AssertEquals(0m, apportionCharge.JR_OSCostAmt);
			AssertEquals(0m, apportionCharge.JR_LocalCostAmt);

			apportionCharge.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;

			AssertEquals(TestObjectCreator.AUD.RX_Code, apportionCharge.JR_RX_NKSellCurrency);
			AssertEquals(0m, apportionCharge.JR_OSSellAmt);
			AssertEquals(0m, apportionCharge.JR_LocalSellAmt);

			consolCost.E6_AC_ChargeCode = TestObjectCreator.DSBChargeCode.PK;
			consolCost.E6_OSCostAmount = 100m;
			consolCost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			consolCost.E6_ExchangeRate = 0.6m;

			AssertEquals(TestObjectCreator.USD.RX_Code, apportionCharge.JR_RX_NKCostCurrency);
			AssertEquals(100m, apportionCharge.JR_OSCostAmt);
			AssertEquals(166.67m, apportionCharge.JR_LocalCostAmt);

			AssertEquals("Sell Currency is not updated by the Apportionment Charge", TestObjectCreator.AUD.RX_Code, apportionCharge.JR_RX_NKSellCurrency);
			AssertEquals("OS Sell Amount is not updated by the Apportionment Charge", 0m, apportionCharge.JR_OSSellAmt);
			AssertEquals("Local Sell Amount is not updated by the Apportionment Charge", 0m, apportionCharge.JR_LocalSellAmt);

			Factory.Save();
			apportionCharge = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().FirstOrDefault();
			AssertNotNull(apportionCharge);

			AssertEquals("Sell Currency is updated by Consol Cost PrepareToPost", TestObjectCreator.USD.RX_Code, apportionCharge.JR_RX_NKSellCurrency);
			AssertEquals("OS Sell Amount is updated by Consol Cost PrepareToPost", 100m, apportionCharge.JR_OSSellAmt);
			AssertEquals("Local Sell AMOUNT is updated by Consol Cost PrepareToPost", 166.67m, apportionCharge.JR_LocalSellAmt);

			consolCost.E6_OSCostAmount = 150m;
			AssertEquals(TestObjectCreator.USD.RX_Code, apportionCharge.JR_RX_NKCostCurrency);
			AssertEquals(150m, apportionCharge.JR_OSCostAmt);
			AssertEquals(250m, apportionCharge.JR_LocalCostAmt);

			AssertEquals(TestObjectCreator.USD.RX_Code, apportionCharge.JR_RX_NKSellCurrency);
			AssertEquals("OS Sell Amount is not updated by the Apportionment Charge", 100m, apportionCharge.JR_OSSellAmt);
			AssertEquals("Local Sell Amount is not updated by the Apportionment Charge", 166.67m, apportionCharge.JR_LocalSellAmt);

			Factory.Save();
			apportionCharge = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().FirstOrDefault();
			AssertNotNull(apportionCharge);

			AssertEquals(TestObjectCreator.USD.RX_Code, apportionCharge.JR_RX_NKSellCurrency);
			AssertEquals("OS Sell Amount is updated by Consol Cost PrepareToPost", 150m, apportionCharge.JR_OSSellAmt);
			AssertEquals("Local Sell AMOUNT is updated by Consol Cost PrepareToPost", 250m, apportionCharge.JR_LocalSellAmt);

			var charge = Factory.Load<Charge>(apportionCharge.PK);
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("I0001", TestObjectCreator.USD, 0.6m, TestObjectCreator.AALSHI);
			var sellLine = TestObjectCreator.CreateRevenueLine(charge, arInvoice.PK);
			charge.ReverseWIP(ZDateTime.Today);
			charge.JR_AL_ARLine = sellLine.PK;

			consolCost.E6_RX_NKCurrency = TestObjectCreator.AUD.RX_Code;
			consolCost.E6_ExchangeRate = 1m;
			consolCost.E6_OSCostAmount = 200m;

			AssertEquals(TestObjectCreator.AUD.RX_Code, apportionCharge.JR_RX_NKCostCurrency);
			AssertEquals(200m, apportionCharge.JR_OSCostAmt);
			AssertEquals(200m, apportionCharge.JR_LocalCostAmt);

			AssertEquals(TestObjectCreator.USD.RX_Code, apportionCharge.JR_RX_NKSellCurrency);
			AssertEquals(150m, apportionCharge.JR_OSSellAmt);
			AssertEquals(250m, apportionCharge.JR_LocalSellAmt);

			Factory.Save();
			apportionCharge = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().FirstOrDefault();
			AssertNotNull(apportionCharge);

			AssertEquals(TestObjectCreator.USD.RX_Code, apportionCharge.JR_RX_NKSellCurrency);
			AssertEquals(150m, apportionCharge.JR_OSSellAmt);
			AssertEquals(250m, apportionCharge.JR_LocalSellAmt);
		}

		public void TestClearCostData()
		{
			ApportionSplitCharge charge = (ApportionSplitCharge)GetNewBusinessObject();
			charge.JR_AC = TestObjectCreator.MRG100.PK;
			charge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
			charge.JR_APInvoiceNum = "123";
			charge.JR_APInvoiceDate = TestObjectCreator.Today;
			charge.JR_PaymentDate = TestObjectCreator.Tomorrow;
			charge.JR_APDocumentReceivedDate = TestObjectCreator.Tomorrow;
			charge.JR_CostReference = "ABC";
			charge.JR_AB = TestObjectCreator.AUDBankAccount.PK;
			charge.JR_AK = TestObjectCreator.AUDChequeBook.PK;
			charge.JR_ChequeNo = "987";

			charge.ClearCostData();

			AssertEquals(0m, charge.JR_LocalCostAmt);
			AssertEquals(ZGuid.Empty, charge.JR_E6);
			AssertEquals(ZGuid.Empty, charge.JR_OH_CostAccount);
			AssertEquals("", charge.JR_APInvoiceNum);
			AssertEquals(ZDateTime.Empty, charge.JR_APInvoiceDate);
			AssertEquals(ZDateTime.Empty, charge.JR_PaymentDate);
			AssertEquals(ZDateTime.Empty, charge.JR_APDocumentReceivedDate);
			AssertEquals(ZString.Empty, charge.JR_CostReference);
			AssertEquals(ZGuid.Empty, charge.JR_AB);
			AssertEquals(ZGuid.Empty, charge.JR_AK);
			AssertEquals("", charge.JR_ChequeNo);
		}

		[SuspendCriticalValidation]
		public void TestRestoreSellData()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentTableCode = "JS";
			job.JH_ParentID = shipment.PK;

			ApportionSplitCharge charge = (ApportionSplitCharge)GetNewBusinessObject();
			charge.JR_JH = job.PK;
			var chargeCode = TestObjectCreator.CC1;
			charge.JR_AC = chargeCode.PK;
			var branch = GlbBranch.CurrentBranch;
			charge.JR_GB = branch.PK;
			var department = GlbDepartment.CurrentDepartment;
			charge.JR_GE = department.PK;

			var debtor = TestObjectCreator.ABIGAS;
			charge.JR_OH_SellAccount = debtor.PK;
			var sellAddress = TestObjectCreator.CreateAddress(debtor);
			charge.JR_OA_SellInvoiceAddress = sellAddress.PK;
			var contact = TestObjectCreator.CreateContact(debtor);
			charge.JR_OC_SellInvoiceContact = contact.PK;

			var gst = TestObjectCreator.GST1;
			charge.JR_AT_SellGSTRate = gst.PK;
			var taxMessage = TestObjectCreator.CreateTaxMsg("TXMSG1", "Tax message", "English Message", "Local Message");
			charge.JR_A9_SellVATClass = taxMessage.PK;

			var exchangeRate = 1M;
			charge.JR_OSSellExRate = exchangeRate;
			var currency = TestObjectCreator.AUD;
			charge.JR_RX_NKSellCurrency = currency.RX_Code;

			var agentDeclaredSellAmt = 12M;
			charge.JR_AgentDeclaredSellAmt = agentDeclaredSellAmt;
			var whtRate = TestObjectCreator.WHT1;
			charge.JR_AW_SellWHTRate = whtRate.PK;

			var localAmount = 100M;
			charge.JR_LocalSellAmt = localAmount;
			var osAmount = 100M;
			charge.JR_OSSellAmt = osAmount;

			var whtAmount = 5M;
			charge.JR_OSSellWHTAmt = whtAmount;

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice));
			var line = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Revenue, arInvoice, job, chargeCode, currency, exchangeRate, "Rev line", osAmount);
			line.AL_LocalExTaxAmount = localAmount;
			line.AL_GSTVAT = 10M;

			charge.JR_AL_ARLine = line.PK;

			var sellRated = ZBool.True;
			charge.JR_SellRated = sellRated;
			var sellRatingOverride = ZBool.False;
			charge.JR_SellRatingOverride = sellRatingOverride;
			var sellRatingOverrideComment = "No comment";
			charge.JR_SellRatingOverrideComment = sellRatingOverrideComment;

			var sellReference = "Sell Reference";
			charge.JR_SellReference = sellReference;

			Factory.Save();

			charge.JR_AC = TestObjectCreator.CC2.PK;
			charge.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;

			charge.JR_AT_SellGSTRate = TestObjectCreator.GSTFREE1.PK;
			charge.JR_A9_SellVATClass = TestObjectCreator.CreateTaxMsg("TXMSG2", "Tax message2", "English Message2", "Local Message2").PK;

			charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
			charge.JR_OA_SellInvoiceAddress = TestObjectCreator.CreateAddress(TestObjectCreator.Debtor).PK;
			charge.JR_OC_SellInvoiceContact = TestObjectCreator.CreateContact(TestObjectCreator.Debtor).PK;

			charge.JR_LocalSellAmt = 1M;
			charge.JR_OSSellAmt = 11M;

			charge.JR_AgentDeclaredSellAmt = 0M;
			charge.JR_AW_SellWHTRate = TestObjectCreator.WHTFREE1.PK;
			charge.JR_OSSellWHTAmt = 0M;

			charge.JR_RX_NKSellCurrency = TestObjectCreator.EUR.RX_Code;
			charge.JR_OSSellExRate = 100M;

			charge.JR_SellRated = false;
			charge.JR_SellRatingOverride = true;
			charge.JR_SellRatingOverrideComment = "";

			charge.JR_SellReference = "";

			charge.RestoreSellData();

			AssertEquals("JR_AC", chargeCode.PK, charge.JR_AC);
			AssertEquals("JR_GB", branch.PK, charge.JR_GB);
			AssertEquals("JR_GE", department.PK, charge.JR_GE);

			AssertEquals("JR_AT_SellGSTRate", gst.PK, charge.JR_AT_SellGSTRate);
			AssertEquals("JR_A9_SellVATClass", taxMessage.PK, charge.JR_A9_SellVATClass);

			AssertEquals("JR_OH_SellAccount", debtor.PK, charge.JR_OH_SellAccount);
			AssertEquals("JR_OA_SellInvoiceAddress", sellAddress.PK, charge.JR_OA_SellInvoiceAddress);
			AssertEquals("JR_OC_SellInvoiceContact", contact.PK, charge.JR_OC_SellInvoiceContact);

			AssertEquals("JR_LocalSellAmt", localAmount, charge.JR_LocalSellAmt);
			AssertEquals("JR_OSSellAmt", osAmount, charge.JR_OSSellAmt);

			AssertEquals("JR_OSSellExRate", exchangeRate, charge.JR_OSSellExRate);
			AssertEquals("JR_RX_NKSellCurrency", currency.RX_Code, charge.JR_RX_NKSellCurrency);

			AssertEquals("JR_AL_ARLine", line.PK, charge.JR_AL_ARLine);

			AssertEquals("JR_AgentDeclaredSellAmt", agentDeclaredSellAmt, charge.JR_AgentDeclaredSellAmt);
			AssertEquals("JR_AW_SellWHTRate", whtRate.PK, charge.JR_AW_SellWHTRate);
			AssertEquals("JR_OSSellWHTAmt", whtAmount, charge.JR_OSSellWHTAmt);

			AssertEquals("JR_SellRated", sellRated, charge.JR_SellRated);
			AssertEquals("JR_SellRatingOverride", sellRatingOverride, charge.JR_SellRatingOverride);
			AssertEquals("JR_SellRatingOverrideComment", sellRatingOverrideComment, charge.JR_SellRatingOverrideComment);

			AssertEquals("JR_SellReference", sellReference, charge.JR_SellReference);

			if (ExceptionReporterTestListener.Instance.Count == 1
			 && ErrorReporter.LastKeyReported.Equals("JobCharge.ModifyingTaxRateOnPostedCharge")
			 && ErrorReporter.LastMessageReported.StartsWith("Modifying Tax Rate on Posted Charge."))
			{
				ErrorReporter.Clear();
			}
		}

		[DisableZeroExchangeRateOverriding]
		public void TestUpdateCostData()
		{
			AccChargeCode chargeCodeMGR = TestObjectCreator.CC1;
			chargeCodeMGR.AC_MarginPercentage = 50;
			AccChargeCode chargeCodeDSB = TestObjectCreator.CC2;
			chargeCodeDSB.AC_MarginPercentage = 50;

			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentTableCode = "JS";
			job.JH_ParentID = shipment.PK;

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.Add(shipment);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost costWithMGRToEdit = apps.CostsCollection.TryAddNew();
			costWithMGRToEdit.E6_AC_ChargeCode = chargeCodeMGR.PK;
			costWithMGRToEdit.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			costWithMGRToEdit.E6_ExchangeRate = 1.5M;
			costWithMGRToEdit.E6_LocalCostAmount = 100M;
			JobConsolCost costWithDSBToEdit = apps.CostsCollection.TryAddNew();
			costWithDSBToEdit.E6_AC_ChargeCode = chargeCodeDSB.PK;
			costWithDSBToEdit.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			costWithDSBToEdit.E6_ExchangeRate = 1.5M;
			costWithDSBToEdit.E6_LocalCostAmount = 100M;

			JobConsolCost costWithMGR = apps.CostsCollection.TryAddNew();
			costWithMGR.E6_AC_ChargeCode = chargeCodeMGR.PK;
			costWithMGR.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			costWithMGR.E6_ExchangeRate = 1.5M;
			costWithMGR.E6_LocalCostAmount = 100M;
			JobConsolCost costWithDSB = apps.CostsCollection.TryAddNew();
			costWithDSB.E6_AC_ChargeCode = chargeCodeDSB.PK;
			costWithDSB.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			costWithDSB.E6_ExchangeRate = 1.5M;
			costWithDSB.E6_LocalCostAmount = 100M;

			Factory.Save();

			AssertEquals("Should be four consol costs created", 4, apps.CostsCollection.Count);
			AssertEquals("Should be four job charge created", 4, job.Charges.Count);
			JobCharge chargeWithMGRToEdit = (from c in job.Charges
											 where c.ChargeType == Enterprise.Core.Constants.ChargeType.Margin
											 select c).ToList()[0];
			chargeWithMGRToEdit.JR_Desc = "ChargeWithMGRToEdit";
			chargeWithMGRToEdit.JR_LocalSellAmt = 300;
			JobCharge chargeWithDSBToEdit = (from c in job.Charges
											 where c.ChargeType == Enterprise.Core.Constants.ChargeType.Disbursement
											 select c).ToList()[0];
			chargeWithDSBToEdit.JR_Desc = "ChargeWithDSBToEdit";
			chargeWithDSBToEdit.JR_OSSellAmt = 300M;

			JobCharge chargeWithMGR = (from c in job.Charges
									   where c.ChargeType == Enterprise.Core.Constants.ChargeType.Margin
									   select c).ToList()[1];
			chargeWithMGR.JR_Desc = "ChargeWithMGR";
			JobCharge chargeWithDSB = (from c in job.Charges
									   where c.ChargeType == Enterprise.Core.Constants.ChargeType.Disbursement
									   select c).ToList()[1];
			chargeWithDSB.JR_Desc = "ChargeWithDSB";

			AssertEquals(300m, chargeWithDSBToEdit.JR_OSSellAmt);
			Factory.Save();
			AssertEquals("Sell Amount is getting restored by the JobConSolCost.PrepareToPost to get a valid DSB Charge", 150m, chargeWithDSBToEdit.JR_OSSellAmt);

			// Try to modify it again
			chargeWithDSBToEdit.JR_OSSellAmt = 300M;
			apps.CostsCollection.RemoveAndDeleteAll();
			// Sell Amount is getting restored by the JobConsolCostCalculationStrategyBase.HandleDelete which cancels changes on the Charges
			// As a result the ChargeWithDSBToEdit is deleted
			Assert(chargeWithDSBToEdit.IsDeleted);

			Factory.Save();

			AssertEquals("Should be no consol costs exists", 0, apps.CostsCollection.Count);

			AssertEquals("Should be two job charge exists", 1, job.Charges.Count);
			AssertEquals("ChargeWithMGRToEdit should be exist", true, job.Charges.Contains(chargeWithMGRToEdit));
			AssertEquals("Charge should not be connected to consol cost", ZGuid.Empty, chargeWithMGRToEdit.JR_E6);
			AssertEquals("Cost currency should be set to local", TestObjectCreator.LocalCurrency.RX_Code, chargeWithMGRToEdit.JR_RX_NKCostCurrency);
			AssertEquals("Cost exchange rate should be set", 1M, chargeWithMGRToEdit.JR_OSCostExRate);
			AssertEquals("Cost local amount should be set", 150M, chargeWithMGRToEdit.JR_LocalCostAmt);
			AssertEquals("Cost amount should be set to local", 150M, chargeWithMGRToEdit.JR_OSCostAmt);
			AssertEquals("Revenue local amount should remain unchanged", 300M, chargeWithMGRToEdit.JR_LocalSellAmt);
			AssertEquals("Revenue amount should remain unchanged", 450M, chargeWithMGRToEdit.JR_OSSellAmt);
			AssertEquals("Revenue currency should remain unchanged", TestObjectCreator.USD.RX_Code, chargeWithMGRToEdit.JR_RX_NKSellCurrency);
			AssertEquals("Revenue exchange should remain unchanged", 1.5M, chargeWithMGRToEdit.JR_OSSellExRate);

			AssertEquals("ChargeWithDSBToEdit should not exist", false, job.Charges.Contains(chargeWithDSBToEdit));
			AssertEquals("Not edited charges should not exists", false, job.Charges.Contains(chargeWithMGR.PK));
			AssertEquals("Not edited charges should not exists", false, job.Charges.Contains(chargeWithDSB.PK));
		}

		public void TestUpdateCostDataSetCurrecyBeforeSettingAmounts()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "AAA";
			company.GC_RN_NKCountryCode = "JP";
			company.GC_IsReciprocal = true;
			company.GC_RX_NKLocalCurrency = "JPY";
			var branch = company.Branches.AddNew();
			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK)))
			{
				var chargeCode = TestObjectCreator.CC1;
				chargeCode.AC_MarginPercentage = 85;

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var job = Factory.NewJobWithValidTestDataForTesting<Job>();
				job.JH_ParentTableCode = "JS";
				job.JH_ParentID = shipment.PK;

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				Factory.Save();
				consol.Shipments.Add(shipment);

				var apps = new ApportionmentListing(Factory, consol);
				var cost = apps.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = chargeCode.PK;
				cost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
				cost.E6_ExchangeRate = 114.49M;
				cost.E6_OSCostAmount = 77.50m;
				Factory.Save();

				var jobCharge = job.Charges[0];
				AssertNotNull("Should found the job charge", jobCharge);
				jobCharge.JR_LocalSellAmt = 8873;
				Factory.Save();

				apps.CostsCollection.RemoveAndDeleteAll();
				AssertNoExceptionThrown("No Critical validation error", () => { Factory.Save(); });

				AssertEquals("Local Cost", 7542m, jobCharge.JR_LocalCostAmt);
				AssertEquals("OS Cost", 7542m, jobCharge.JR_OSCostAmt);
			}
		}
		public void TestUpdateCostData_ForMJA()
		{
			AccChargeCode chargeCodeMJA = TestObjectCreator.ManualJobAccrualChargeCode;
			chargeCodeMJA.AC_MarginPercentage = 0;

			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentTableCode = "JS";
			job.JH_ParentID = shipment.PK;

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.Add(shipment);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			JobConsolCost costWithMJAToEdit = apps.CostsCollection.TryAddNew();
			costWithMJAToEdit.E6_AC_ChargeCode = chargeCodeMJA.PK;
			costWithMJAToEdit.E6_RX_NKCurrency = TestObjectCreator.AUD.RX_Code;
			costWithMJAToEdit.E6_OSCostAmount = 100M;
			costWithMJAToEdit.ApportionmentCharges[0].JR_OSCostAmt = costWithMJAToEdit.E6_OSCostAmount;
			costWithMJAToEdit.ApportionmentCharges[0].JR_LocalCostAmt = costWithMJAToEdit.E6_LocalCostAmount;

			JobConsolCost costWithMJA = apps.CostsCollection.TryAddNew();
			costWithMJA.E6_AC_ChargeCode = chargeCodeMJA.PK;
			costWithMJA.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			costWithMJA.E6_OSCostAmount = 100M;
			costWithMJA.E6_ExchangeRate = 1M;
			costWithMJA.ApportionmentCharges[0].JR_OSCostAmt = costWithMJA.E6_OSCostAmount;
			costWithMJA.ApportionmentCharges[0].JR_LocalCostAmt = costWithMJA.E6_LocalCostAmount;

			Factory.Save();

			AssertEquals("Should be two consol costs created", 2, apps.CostsCollection.Count);
			AssertEquals("Should be two job charge created", 2, job.Charges.Count);

			JobCharge chargeWithMJAToEdit = (from c in job.Charges
											 where c.ChargeType == Enterprise.Core.Constants.ChargeType.ManualJobAccrual
											 select c).ToList()[0];
			chargeWithMJAToEdit.JR_Desc = "ChargeWithMJAToEdit";
			chargeWithMJAToEdit.JR_OSSellAmt = 300M;

			JobCharge chargeWithMJA = (from c in job.Charges
									   where c.ChargeType == Enterprise.Core.Constants.ChargeType.ManualJobAccrual
									   select c).ToList()[1];
			chargeWithMJA.JR_Desc = "ChargeWithMJA";

			Factory.Save();

			apps.CostsCollection.RemoveAndDeleteAll();
			Factory.Save();

			AssertEquals("Should be no consol costs exists", 0, apps.CostsCollection.Count);

			AssertEquals("One job charge should exist", 1, job.Charges.Count);

			AssertEquals("ChargeWithMJAToEdit should be exist", true, job.Charges.Contains(chargeWithMJAToEdit));
			AssertEquals("Charge should not be connected to consol cost", ZGuid.Empty, chargeWithMJAToEdit.JR_E6);
			AssertEquals("Cost exchange rate should be set", 1M, chargeWithMJAToEdit.JR_OSCostExRate);
			AssertEquals("Cost amount should be set", 0M, chargeWithMJAToEdit.JR_OSCostAmt);
			AssertEquals("Cost local amount should be set to local", 0M, chargeWithMJAToEdit.JR_LocalCostAmt);
			AssertEquals("Revenue local amount should remain unchanged", 300M, chargeWithMJAToEdit.JR_LocalSellAmt);
			AssertEquals("Revenue amount should remain unchanged", 300M, chargeWithMJAToEdit.JR_OSSellAmt);

			AssertEquals("Not edited charges should not exist", false, job.Charges.Contains(chargeWithMJA.PK));
		}

		public void TestSetJR_OSCostAmtAdjustsGST()
		{
			AccChargeCode chargeCodeMGR = TestObjectCreator.CC1;

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();

			Job job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_ParentTableCode = "JS";
			job1.JH_ParentID = shipment1.PK;
			Job job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_ParentTableCode = "JS";
			job2.JH_ParentID = shipment2.PK;
			Job job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job3.JH_ParentTableCode = "JS";
			job3.JH_ParentID = shipment3.PK;

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);
			consol.Shipments.Add(shipment3);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = chargeCodeMGR.PK;
			cost.E6_RX_NKCurrency = TestObjectCreator.AUD.RX_Code;
			consol.JK_OA_CreditorAddress = TestObjectCreator.AALSHI.MainAddress.PK;
			cost.E6_ExchangeRate = 1M;
			cost.E6_AT_TaxRate = TestObjectCreator.GST1.PK;
			cost.E6_LocalCostAmount = 35M;
			cost.E6_ApportionmentMethod = "SHP";
			cost.E6_IsTaxAmountOverridden = true;

			Factory.Save();

			AssertEquals("Should be one consol cost created", 1, apps.CostsCollection.Count);
			AssertEquals("Should be three Apportionment Charges", 3, apps.CostsCollection[0].ApportionmentCharges.Count);

			ZDecimal totalCostAmount = ZDecimal.Zero;
			ZDecimal totalCostGST = ZDecimal.Zero;

			foreach (ApportionSplitCharge charge in apps.CostsCollection[0].ApportionmentCharges)
			{
				totalCostAmount += charge.JR_OSCostAmt;
				totalCostGST += charge.JR_OSCostGSTAmt_Calc;
			}
			AssertEquals("TotalCostAmount", 35M, totalCostAmount);
			AssertEquals("TotalCostGST", 3.5M, totalCostGST);

			ZDecimal firstChargeGST = apps.CostsCollection[0].ApportionmentCharges[0].JR_OSCostGSTAmt_Calc;

			apps.CostsCollection[0].ApportionmentCharges[0].JR_OSCostAmt -= 5M;

			AssertEquals("Should be UnApportionedAmount", 5M, apps.CostsCollection[0].UnApportionedAmount);
			AssertNotEquals("GST for the modified charge should be recalculated", firstChargeGST, apps.CostsCollection[0].ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);

			totalCostGST = ZDecimal.Zero;
			foreach (ApportionSplitCharge charge in apps.CostsCollection[0].ApportionmentCharges)
			{
				totalCostGST += charge.JR_OSCostGSTAmt_Calc;
			}
			AssertNotEquals("TotalCostGST should not be adjusted because there is an UnApportionedAmount", 3.5M, totalCostGST);

			apps.CostsCollection[0].ApportionmentCharges[0].JR_OSCostAmt += 5.01M;
			apps.CostsCollection[0].ApportionmentCharges[2].JR_OSCostAmt -= 0.01M;

			AssertEquals("UnApportionedAmount should be 0", 0M, apps.CostsCollection[0].UnApportionedAmount);
			totalCostAmount = ZDecimal.Zero;
			totalCostGST = ZDecimal.Zero;

			foreach (ApportionSplitCharge charge in apps.CostsCollection[0].ApportionmentCharges)
			{
				totalCostAmount += charge.JR_OSCostAmt;
				totalCostGST += charge.JR_OSCostGSTAmt_Calc;
			}
			AssertEquals("TotalCostAmount", 35M, totalCostAmount);
			AssertEquals("TotalCostGST", 3.5M, totalCostGST);

			Job[] jobs = new Job[] { job1, job2, job3 };
			totalCostAmount = ZDecimal.Zero;
			totalCostGST = ZDecimal.Zero;

			foreach (Job job in jobs)
			{
				AssertEquals("Should be one job charge created per job", 1, job.Charges.Count);
			}

			totalCostAmount = ZDecimal.Zero;
			totalCostGST = ZDecimal.Zero;
			foreach (Job job in jobs)
			{
				totalCostAmount += job.Charges[0].JR_OSCostAmt;
				totalCostGST += job.Charges[0].JR_OSCostGSTAmt_Calc;
			}
			AssertEquals("TotalCostAmount", 35M, totalCostAmount);
			AssertEquals("TotalCostGST", 3.5M, totalCostGST);
		}

		public void TestSetJR_OSCostAmtSameValueMultipleTimesShouldNotChangeJR_OSCostGSTAmtValue()
		{
			var chargeCodeMGR = TestObjectCreator.CC1;
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_ParentTableCode = "JS";
			job1.JH_ParentID = shipment1.PK;
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();

			consol.Shipments.Add(shipment1);
			var apps = new ApportionmentListing(Factory, consol);
			var cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = chargeCodeMGR.PK;
			cost.E6_RX_NKCurrency = TestObjectCreator.AUD.RX_Code;
			consol.JK_OA_CreditorAddress = TestObjectCreator.AALSHI.MainAddress.PK;
			cost.E6_ExchangeRate = 1M;
			cost.E6_AT_TaxRate = TestObjectCreator.GST1.PK;
			cost.E6_LocalCostAmount = 35M;
			cost.E6_ApportionmentMethod = "SHP";
			cost.E6_IsTaxAmountOverridden = true;
			Factory.Save();

			var charge = cost.ApportionmentCharges[0];
			var originalChargeCostAmount = charge.JR_OSCostAmt;
			cost.E6_OSGSTAmount_Calc = 10M;
			AssertEquals(10M, charge.JR_OSCostGSTAmt);

			charge.JR_OSCostAmt = originalChargeCostAmount;
			AssertEquals(10M, charge.JR_OSCostGSTAmt);
		}

		public void TestDefaultingOfJR_IsUsedForApportionmentOnNonSubHouseBillShipment()
		{
			ForwardingShipment coloadMaster = Factory.New<ForwardingShipment>();
			coloadMaster.JS_ShipmentType = Enterprise.Core.Constants.ShipmentTypes.CoLoadMaster;
			coloadMaster.JS_UniqueConsignRef = "S00005551";
			ForwardingShipment subHouseBill = Factory.New<ForwardingShipment>();
			subHouseBill.JS_UniqueConsignRef = "S00005552";
			subHouseBill.JS_JS_ColoadMasterShipment = coloadMaster.PK;
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.Add(coloadMaster);
			consol.Shipments.Add(subHouseBill);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_ApportionToRelatedShipments = true;
			ApportionSplitCharge subBillCharge = cost.ApportionmentCharges.FindChargeForJob(subHouseBill);
			ApportionSplitCharge masterCharge = cost.ApportionmentCharges.FindChargeForJob(coloadMaster);

			AssertNotNull("SubBillCharge", subBillCharge);
			Assert(!subBillCharge.JR_IsUsedForApportionment);
			AssertNotNull("MasterCharge", masterCharge);
			Assert(masterCharge.JR_IsUsedForApportionment);

			cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost.E6_OSCostAmount = 100m;
			subBillCharge.JR_IsUsedForApportionment = true;
			subBillCharge.JR_OSCostAmt = 40m;
			masterCharge.JR_OSCostAmt = 60m;

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			apps = new ApportionmentListing(newFactory, consol);
			AssertEquals("Should be 1 cost", 1, apps.CostsCollection.Count);
			cost = apps.CostsCollection[0];

			subBillCharge = cost.ApportionmentCharges.FindChargeForJob(subHouseBill);
			masterCharge = cost.ApportionmentCharges.FindChargeForJob(coloadMaster);

			Assert(subBillCharge.JR_IsUsedForApportionment);
			Assert(masterCharge.JR_IsUsedForApportionment);

			Assert(((IApportionedCharge)subBillCharge).IsUsedForApportionment);
			Assert(((IApportionedCharge)masterCharge).IsUsedForApportionment);
		}

		public void TestParentConsolCostDeleted()
		{
			JobConsolCost cost = Factory.New<JobConsolCost>();
			ApportionSplitCharge charge = cost.ApportionmentCharges.AddNew();
			cost.Delete();
			AssertNull(charge.ParentConsolCost);
		}

		public void TestSavedParentConsolCostDeleted()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C000001");
			var shipment = TestObjectCreator.CreateShipment("S000001", consol);
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_OA_LocalChargesAddr = TestObjectCreator.ABIGAS.Addresses.DefaultAddressOfType(OrgAddressType.Office).PK; // This is necessary to avoid loading Charge instances by Validation in newFactory
			JobConsolCost cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1);
			ApportionSplitCharge charge = cost.ApportionmentCharges[0];
			cost.E6_OSCostAmount = 100m;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			consol = newFactory.Load<ForwardingConsol>(consol.PK);
			cost = new ApportionmentListing(newFactory, consol).CostsCollection[0];
			charge = cost.ApportionmentCharges[0];

			var wip = charge.WIP;
			AssertNotNull(wip);
			Assert("WIP should not be reversed ", wip.AL_ReverseDate.IsEmpty);
			var accrual = charge.Accrual;
			AssertNotNull(accrual);
			Assert("ACR should not be reversed", accrual.AL_ReverseDate.IsEmpty);
			AssertEquals("There is only ApportionSplitCharge instance", 1, newFactory.GetBizOsForPK(charge.PK.ToGuid()).Length);
			Assert("And it is ApportionSplitCharge, what else could it be?!", newFactory.GetBizOsForPK(charge.PK.ToGuid())[0] is ApportionSplitCharge);

			cost.Delete();
			Assert(charge.IsDeleted);
			AssertEquals("WIP must be reversed", false, wip.AL_ReverseDate.IsEmpty);
			AssertEquals("ACR must be reversed", false, accrual.AL_ReverseDate.IsEmpty);
		}

		public void TestSavedParentConsolCostDeleted_PostedRevenue()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C000001");
			var shipment = TestObjectCreator.CreateShipment("S000001", consol);
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_OA_LocalChargesAddr = TestObjectCreator.ABIGAS.Addresses.DefaultAddressOfType(OrgAddressType.Office).PK; // This is necessary to avoid loading Charge instances by Validation in newFactory
			JobConsolCost cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1);
			ApportionSplitCharge charge = cost.ApportionmentCharges[0];
			cost.E6_OSCostAmount = 100m;

			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "T0001", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
			job.LoadCharges_ForTestOnly();
			job.Charges[0].ReverseWIP(ZDateTime.Now);
			job.Charges[0].JR_AL_ARLine = invoice.Lines[0].PK;
			invoice.Lines[0].AL_JH = job.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			consol = newFactory.Load<ForwardingConsol>(consol.PK);
			cost = new ApportionmentListing(newFactory, consol).CostsCollection[0];
			charge = cost.ApportionmentCharges[0];

			AssertEquals("Should be REV line", invoice.Lines[0].PK, charge.JR_AL_ARLine);
			var accrual = charge.Accrual;
			AssertNotNull(accrual);
			Assert("ACR should not be reversed", accrual.AL_ReverseDate.IsEmpty);
			AssertEquals("There is only ApportionSplitCharge instance", 1, newFactory.GetBizOsForPK(charge.PK.ToGuid()).Length);
			Assert("And it is ApportionSplitCharge, what else could it be?!", newFactory.GetBizOsForPK(charge.PK.ToGuid())[0] is ApportionSplitCharge);

			cost.Delete();
			AssertEquals("Charge should not be deleted", false, charge.IsDeleted);
			AssertEquals("Cost details must be reset", 0m, charge.JR_OSCostAmt);

			newFactory.Save();

			AssertEquals("Original ACR must be reversed", false, accrual.AL_ReverseDate.IsEmpty);
			AssertNull("No current ACR", charge.Accrual);
		}

		public void TestChangingCostAmountOnApportionSplitChargeAlwaysUpdateAccrualAmount()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C000001");
			var shipment = TestObjectCreator.CreateShipment("S000001", consol);
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_OA_LocalChargesAddr = TestObjectCreator.ABIGAS.Addresses.DefaultAddressOfType(OrgAddressType.Office).PK; // This is necessary to avoid loading Charge instances by Validation in newFactory
			JobConsolCost cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1);
			ApportionSplitCharge charge = cost.ApportionmentCharges[0];
			cost.E6_OSCostAmount = 100m;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			consol = newFactory.Load<ForwardingConsol>(consol.PK);
			cost = new ApportionmentListing(newFactory, consol).CostsCollection[0];
			charge = cost.ApportionmentCharges[0];

			var accrual = charge.Accrual;
			AssertNotNull(accrual);
			AssertEquals("ACR must match Charge Cost Amount", charge.JR_OSCostAmt, accrual.AL_OSAmount);
			Assert("ACR should not be reversed", accrual.AL_ReverseDate.IsEmpty);

			cost.E6_OSCostAmount = 150m;
			AssertEquals("Charge Cost Amount must be updated", 150m, charge.JR_OSCostAmt);
			AssertEquals("There is only ApportionSplitCharge instance", 1, newFactory.GetBizOsForPK(charge.PK.ToGuid()).Length);
			Assert("And it is ApportionSplitCharge, what else could it be?!", newFactory.GetBizOsForPK(charge.PK.ToGuid())[0] is ApportionSplitCharge);

			newFactory.Save();

			AssertEquals("Original ACR must be reversed", false, accrual.AL_ReverseDate.IsEmpty);
			AssertNotNull("Current ACR", charge.Accrual);
			AssertNotEquals("New ACR", accrual, charge.Accrual);
			AssertEquals("New ACR should have updated Cost Amount", charge.JR_OSCostAmt, charge.Accrual.AL_OSAmount);
		}

		#region TestSellAmountShouldBeUpdatedWhenChangeChargeCode

		public void TestSellAmountShouldBeUpdatedWhenChangeChargeCodeWhenChargeIsInDB()
		{
			SellAmountShouldBeUpdatedWhenChangeChargeCode(true);
		}

		public void TestSellAmountShouldBeUpdatedWhenChangeChargeCodeWhenChargeIsNotInDB()
		{
			SellAmountShouldBeUpdatedWhenChangeChargeCode(false);
		}

		void SellAmountShouldBeUpdatedWhenChangeChargeCode(bool chargeIsInDB)
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var debtor = TestObjectCreator.Debtor;
			AssertEquals("Precondition: OB_RX_NKARDDefltCurrency", TestObjectCreator.AUD.RX_Code, debtor.CompanyData.OB_RX_NKARDDefltCurrency);
			job.AgentCollectPK = debtor.PK;

			var exchangeRate = job.ExchangeRates.AddNew();
			exchangeRate.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
			exchangeRate.JF_BaseRate = 2m;
			Factory.Save();

			IncoTermRegistry.Instance.ChargeAgentAlwaysCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.FRT.PK.ToString());

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);
			AssertEquals("Precondition: JR_OH_SellAccount", ZGuid.Empty, consolCost.ApportionmentCharges[0].JR_OH_SellAccount);
			consolCost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			consolCost.E6_ExchangeRate = 2m;
			if (chargeIsInDB)
			{
				Factory.Save();
			}
			else
			{
				//this is synthetic setup just to simulate the same input conditions as for charge in db case
				consolCost.ApportionmentCharges[0].JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
				consolCost.ApportionmentCharges[0].JR_OSSellExRate = 2;
				consolCost.ApportionmentCharges[0].JR_OSSellAmt = 100;
				consolCost.ApportionmentCharges[0].JR_LocalSellAmt = 50;
			}

			AssertCharge(consolCost.ApportionmentCharges[0], TestObjectCreator.USD.RX_Code, 100m, 50m, 2m);
			consolCost.E6_AC_ChargeCode = TestObjectCreator.FRT.PK;
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Postcondition: JR_OH_SellAccount", debtor.PK, consolCost.ApportionmentCharges[0].JR_OH_SellAccount);
			AssertCharge(consolCost.ApportionmentCharges[0], TestObjectCreator.AUD.RX_Code, 50m, 50m, 1m);

			void AssertCharge(ApportionSplitCharge charge, string currencyCode, ZDecimal sellAmount, ZDecimal localSellAmount, ZDecimal sellExchangeRate)
			{
				AssertEquals(currencyCode, charge.JR_SellCurrency);
				AssertEquals(sellAmount, charge.JR_OSSellAmt);
				AssertEquals(localSellAmount, charge.JR_LocalSellAmt);
				AssertEquals(sellExchangeRate, charge.JR_OSSellExRate);
			}
		}

		#endregion

		public void TestOtherChargesOfTheSameConsolCostAreValidatedWhenAmountChanges()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				AssertEquals("Should have 2 charge lines", 2, cost.ApportionmentCharges.Count);

				ApportionSplitCharge charge1 = cost.ApportionmentCharges[0];
				ApportionSplitCharge charge2 = cost.ApportionmentCharges[1];
				cost.E6_OSCostAmount = 200m;
				charge1.JR_OSCostAmt = 210m;
				AssertHasErrors("charge1", charge1.JR_OSCostAmtInfo);
				charge2.JR_OSCostAmt = -10m;
				AssertNoErrors("charge1", charge1.JR_OSCostAmtInfo);
				AssertNoErrors("charge2", charge2.JR_OSCostAmtInfo);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestJR_OSAmountValidationCanBeSuspendedByParentConsolCost()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				AssertEquals("Should have 2 charge lines", 2, cost.ApportionmentCharges.Count);

				using (cost.GetValidationSuspender())
				{
					ApportionSplitCharge charge1 = cost.ApportionmentCharges[0];
					ApportionSplitCharge charge2 = cost.ApportionmentCharges[1];
					cost.E6_OSCostAmount = 200m;
					charge1.JR_OSCostAmt = 210m;
					charge2.JR_OSCostAmt = 20m;
					AssertNoErrors("charge1", charge1.JR_OSCostAmtInfo);
					AssertNoErrors("charge2", charge1.JR_OSCostAmtInfo);
				}
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		[ExpectNoExceptions]
		public void TestIsChargeRevenueEditedReturnsFalseOnDeletedCharge()
		{
			JobConsolCost cost = Factory.New<JobConsolCost>();
			ApportionSplitCharge charge = cost.ApportionmentCharges.AddNew();
			charge.Delete();
			AssertEquals("Should return False", false, charge.IsChargeRevenueEdited);
		}

		public void TestIsSavedByFactory()
		{
			var consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			var apps = new ApportionmentListing(Factory, consol);

			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				AssertEquals("Should have 2 charge lines", 2, cost.ApportionmentCharges.Count);

				var charge = Factory.Load<Charge>(cost.ApportionmentCharges[0].PK);
				AssertEquals("ApportionSplitCharge.IsSavedByFactory should be the same as Charge", charge.IsSavedByFactory, cost.ApportionmentCharges[0].IsSavedByFactory);

				consol.Shipments[0].Job.Delete();
				consol.Shipments[0].Delete();
				AssertEquals("ApportionSplitCharge.IsSavedByFactory should be the same as Charge", charge.IsSavedByFactory, cost.ApportionmentCharges[0].IsSavedByFactory);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestIsSavedByFactory_WithEvaluationInfo()
		{
			var consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.AddNew();
			var apps = new ApportionmentListing(Factory, consol);

			var expectedEvaluationInfo1 = @"IsSavedByFactoryBase: Yes
AnyOtherInstanceWithDifferentIsSavedByFactoryValue: No
IsDeleted: No
IsForConsolCostForIncompleteInvoice: No
IsInvoicingJobNotNull: Yes
IsInvoicingPlugInDataNull: No
IsPluginDeleted: No";
			var expectedEvaluationInfo2 = @"IsSavedByFactoryBase: Yes
AnyOtherInstanceWithDifferentIsSavedByFactoryValue: No
IsDeleted: No
IsForConsolCostForIncompleteInvoice: No
IsInvoicingJobNotNull: No
IsInvoicingPlugInDataNull: No
IsPluginDeleted: No";

			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				AssertEquals("Should have 1 charge lines", 1, cost.ApportionmentCharges.Count);
				foreach (var chargeWithIsSavedByFactoryResult in new[]
					{
						(new { Charge = cost.ApportionmentCharges[0], IsSavedByFactory = true, ExpectedMessage = expectedEvaluationInfo1 }),
						(new { Charge = Factory.New<ApportionSplitCharge>(), IsSavedByFactory = false, ExpectedMessage = expectedEvaluationInfo2 })
					})
				{
					AssertEquals("ApportionSplitCharge.IsSavedByFactory should be the same as Charge", chargeWithIsSavedByFactoryResult.IsSavedByFactory, chargeWithIsSavedByFactoryResult.Charge.IsSavedByFactory);
					AssertMultilineASCIIEquals("ApportionSplitCharge.IsSavedByFactory Evaluation Info", chargeWithIsSavedByFactoryResult.ExpectedMessage, chargeWithIsSavedByFactoryResult.Charge.GetIsSavedByFactoryEvaluationInfo());
				}
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public override void TestJR_SellGovtChargeCode_ReadOnly()
		{
			ApportionSplitCharge charge = Factory.New<ApportionSplitCharge>();
			AssertEquals("JR_SellGovtChargeCodeInfo.ReadOnly", true, charge.JR_SellGovtChargeCodeInfo.ReadOnly);
		}

		public override void TestJR_CostGovtChargeCode_ReadOnly()
		{
			ApportionSplitCharge charge = Factory.New<ApportionSplitCharge>();
			AssertEquals("JR_CostGovtChargeCodeInfo.ReadOnly", true, charge.JR_CostGovtChargeCodeInfo.ReadOnly);
		}

		public void TestApportionChargePlaceOfSupply_ReadOnly()
		{
			var charge = Factory.New<ApportionSplitCharge>();
			Assert(charge.JR_SellPlaceOfSupplyInfo.ReadOnly);
			Assert(charge.JR_CostPlaceOfSupplyInfo.ReadOnly);
		}

		public void TestApportionChargeDoesNotDefaultCostPlaceOfSupply()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.Add(shipment);
			Factory.Save();

			var apportionments = new ApportionmentListing(Factory, consol);
			var consolCost = apportionments.CostsCollection.TryAddNew();

			var apportionCharge = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().FirstOrDefault();
			AssertNotNull(apportionCharge);
			AssertEquals(job.PK, apportionCharge.JR_JH);
			Assert(apportionCharge.JR_IsApportioned);

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code))
			{
				Assert(apportionCharge.JR_AC.IsEmpty);
				Assert(apportionCharge.JR_OH_CostAccount.IsEmpty);
				Assert(apportionCharge.JR_CostPlaceOfSupply.IsEmpty);

				var currentBranch = GlbBranch.CurrentBranch;
				AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(apportionCharge.InvoicingJob.PlugInData, CostSell.Cost, PlaceOfSupplyTypes.State.Code, "NSW", branch: currentBranch);

				apportionCharge.JR_AC = TestObjectCreator.CC1.PK;
				Assert(apportionCharge.JR_OH_CostAccount.IsEmpty);
				Assert(apportionCharge.JR_CostPlaceOfSupply.IsEmpty);

				apportionCharge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
				Assert(apportionCharge.JR_CostPlaceOfSupply.IsEmpty);

				apportionCharge.JR_CostPlaceOfSupply = "ACT";
				apportionCharge.JR_OH_CostAccount = apportionCharge.JR_OH_CostAccount;
				AssertEquals("ACT", apportionCharge.JR_CostPlaceOfSupply);

				apportionCharge.JR_AC = TestObjectCreator.CC10.PK;
				AssertEquals("ACT", apportionCharge.JR_CostPlaceOfSupply);

				apportionCharge.JR_CostPlaceOfSupply = "QLD";
				apportionCharge.JR_OH_CostAccount = TestObjectCreator.Creditor2.PK;
				AssertEquals("QLD", apportionCharge.JR_CostPlaceOfSupply);

				AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(apportionCharge.InvoicingJob.PlugInData, CostSell.Cost, PlaceOfSupplyTypes.State.Code, "WA", branch: currentBranch);
				apportionCharge.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
				AssertEquals("QLD", apportionCharge.JR_CostPlaceOfSupply);

				using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					apportionCharge.JR_GB = Env.CurrentBranchPK;
					AssertEquals("QLD", apportionCharge.JR_CostPlaceOfSupply);
				}
			}
		}

		public void TestUseExchangeRateFromChargeRatherThanGlobalCompany()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, TestObjectCreator.USD, 2m, 20m);
			Factory.Save();

			var charge = Factory.Load<ApportionSplitCharge>(cost.ApportionmentCharges[0].PK);
			Assert("Current company is AU", !GlbCompany.CurrentCompany.GC_IsReciprocal);
			AssertEquals(2m, charge.JR_OSCostExRate);
			AssertEquals(20m, charge.JR_OSCostAmt);
			AssertEquals(10m, charge.JR_LocalCostAmt);

			var company = TestObjectCreator.CreateNewCompany("DJP", "JP");
			company.GC_IsReciprocal = true;
			var branch = TestObjectCreator.CreateNewBranch(company, "TYO");
			Factory.Save();

			using (branch.SetAsTemporaryContext())
			{
				Assert("Current company is JP", GlbCompany.CurrentCompany.GC_IsReciprocal);
				Assert("Revenue should not be changed", !charge.IsChargeRevenueEdited);
				AssertEquals(2m, charge.JR_OSCostExRate);
				AssertEquals(20m, charge.JR_OSCostAmt);
				AssertEquals(10m, charge.JR_LocalCostAmt);

				charge.JR_OSCostExRate = 4m;
				AssertEquals("Should not change", 20m, charge.JR_OSCostAmt);
				AssertEquals("Should use the IsReciprocal of charge's company", 5m, charge.JR_LocalCostAmt);
			}
		}

		public void TestJR_IsUsedForApportionmentForOSCostAmtChanged()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = "JS";
			AssertNotNull("Shipment should have a Job", shipment.Job);
			JobConsolCost cost = consol.GetApportionments().CostsCollection.TryAddNew();
			cost.FillWithValidTestData();
			cost.E6_OSCostAmount = 1m;
			ApportionSplitCharge charge = cost.ApportionmentCharges.AddNew();
			charge.JR_JH = job.PK;
			charge.JR_OSCostExRate = 1;
			charge.JR_OSCostAmt = 0m;
			cost.UpdateApportionmentChargesListing();

			Assert("Precondition", !charge.JR_IsUsedForApportionment);
			AssertNotNull("Precondition", charge.ParentConsolCost);
			Assert("Precondition", !charge.ParentConsolCost.IsDeleted);
			AssertEquals("Precondition", charge.JR_OSCostAmt, 0m);
			Assert("Precondition", !charge.ShouldIncludeInApportionment);

			charge.JR_OSCostAmt = 100m;

			Assert(charge.JR_IsUsedForApportionment);
		}

		public void TestShouldIncludeInApportionment()
		{
			var (cost, charge, _) = CreateCostAndChargeForTestingShouldIncludeInApportionment();

			cost.E6_PPDCLT = PrepaidCollectList.Codes.DLV;

			AssertNotEquals("Precondition", cost.E6_PPDCLT, PrepaidCollectList.Codes.All);
			AssertNotEquals("Precondition", cost.E6_PPDCLT, charge.JR_PrepaidCollect);
			AssertNotEquals("Precondition", charge.JR_PrepaidCollect, PrepaidCollectList.Codes.Both);
			Assert(!charge.ShouldIncludeInApportionment);

			cost.E6_PPDCLT = PrepaidCollectList.Codes.All;
			charge.JR_OSCostAmt = 0M;

			AssertEquals("Precondition", cost.E6_PPDCLT, PrepaidCollectList.Codes.All);
			AssertEquals("Precondition", cost.E6_OSCostAmount, 0M);
			Assert(charge.JR_ShipmentNumberOfColoadMaster.IsEmpty);
			Assert(charge.ShouldIncludeInApportionment);

			cost.E6_OSCostAmount = 1M;
			charge.JR_OSCostAmt = 0M;

			AssertEquals("Precondition", cost.E6_PPDCLT, PrepaidCollectList.Codes.All);
			AssertEquals("Precondition", charge.JR_OSCostAmt, 0M);
			AssertNotEquals("Precondition", cost.E6_OSCostAmount, 0M);
			Assert(charge.JR_ShipmentNumberOfColoadMaster.IsEmpty);
			Assert(!charge.ShouldIncludeInApportionment);

			charge.JR_OSCostAmt = 100M;

			AssertEquals("Precondition", cost.E6_PPDCLT, PrepaidCollectList.Codes.All);
			AssertNotEquals("Precondition", charge.JR_OSCostAmt, 0M);
			Assert(charge.ShouldIncludeInApportionment);
		}

		(JobConsolCost cost, ApportionSplitCharge charge, ForwardingShipment shipment) CreateCostAndChargeForTestingShouldIncludeInApportionment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var shipment = consol.Shipments.AddNew();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = "JS";
			AssertNotNull("Shipment should have a Job", shipment.Job);

			var cost = consol.GetApportionments().CostsCollection.TryAddNew();
			cost.FillWithValidTestData();
			var charge = cost.ApportionmentCharges.AddNew();
			charge.JR_JH = job.PK;
			charge.JR_OSCostExRate = 1;
			charge.SetShipmentInfo(shipment);
			AssertNotNull("Precondition: charge.ShipmentInfo cannot be null", charge.ShipmentInfo);

			return (cost, charge, shipment);
		}

		public void TestShouldIncludeInApportionmentForLocalAndForeign_OriginAndDestination()
		{
			var australia = RefCountry.LoadFromCountryCode(Factory, "AU");
			var newZealand = RefCountry.LoadFromCountryCode(Factory, "NZ");
			var sydney = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var melbourne = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
			var auckland = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");
			var christchurch = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZCHC");
			var losAngeles = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");
			var (cost, charge, shipment) = CreateCostAndChargeForTestingShouldIncludeInApportionment();

			AssertShouldIncludeInApportionmentFor(companyLocation: australia, shipOrigin: melbourne, shipDestination: auckland, filterCode: PrepaidCollectCodes.LOG, expectedResult: true);
			AssertShouldIncludeInApportionmentFor(companyLocation: australia, shipOrigin: melbourne, shipDestination: auckland, filterCode: PrepaidCollectCodes.FOG, expectedResult: false);
			AssertShouldIncludeInApportionmentFor(companyLocation: australia, shipOrigin: melbourne, shipDestination: sydney, filterCode: PrepaidCollectCodes.LDT, expectedResult: true);
			AssertShouldIncludeInApportionmentFor(companyLocation: australia, shipOrigin: melbourne, shipDestination: sydney, filterCode: PrepaidCollectCodes.FDT, expectedResult: false);
			AssertShouldIncludeInApportionmentFor(companyLocation: australia, shipOrigin: melbourne, shipDestination: sydney, filterCode: PrepaidCollectCodes.All, expectedResult: true);

			AssertShouldIncludeInApportionmentFor(companyLocation: australia, shipOrigin: christchurch, shipDestination: auckland, filterCode: PrepaidCollectCodes.LOG, expectedResult: false);
			AssertShouldIncludeInApportionmentFor(companyLocation: australia, shipOrigin: christchurch, shipDestination: auckland, filterCode: PrepaidCollectCodes.FOG, expectedResult: true);
			AssertShouldIncludeInApportionmentFor(companyLocation: australia, shipOrigin: christchurch, shipDestination: sydney, filterCode: PrepaidCollectCodes.LDT, expectedResult: true);
			AssertShouldIncludeInApportionmentFor(companyLocation: australia, shipOrigin: christchurch, shipDestination: sydney, filterCode: PrepaidCollectCodes.FDT, expectedResult: false);
			AssertShouldIncludeInApportionmentFor(companyLocation: australia, shipOrigin: christchurch, shipDestination: sydney, filterCode: PrepaidCollectCodes.All, expectedResult: true);

			AssertShouldIncludeInApportionmentFor(companyLocation: newZealand, shipOrigin: melbourne, shipDestination: auckland, filterCode: PrepaidCollectCodes.LOG, expectedResult: false);
			AssertShouldIncludeInApportionmentFor(companyLocation: newZealand, shipOrigin: melbourne, shipDestination: auckland, filterCode: PrepaidCollectCodes.FOG, expectedResult: true);
			AssertShouldIncludeInApportionmentFor(companyLocation: newZealand, shipOrigin: melbourne, shipDestination: sydney, filterCode: PrepaidCollectCodes.LDT, expectedResult: false);
			AssertShouldIncludeInApportionmentFor(companyLocation: newZealand, shipOrigin: melbourne, shipDestination: sydney, filterCode: PrepaidCollectCodes.FDT, expectedResult: true);
			AssertShouldIncludeInApportionmentFor(companyLocation: newZealand, shipOrigin: melbourne, shipDestination: sydney, filterCode: PrepaidCollectCodes.All, expectedResult: true);

			AssertShouldIncludeInApportionmentFor(companyLocation: newZealand, shipOrigin: christchurch, shipDestination: auckland, filterCode: PrepaidCollectCodes.LOG, expectedResult: true);
			AssertShouldIncludeInApportionmentFor(companyLocation: newZealand, shipOrigin: christchurch, shipDestination: auckland, filterCode: PrepaidCollectCodes.FOG, expectedResult: false);
			AssertShouldIncludeInApportionmentFor(companyLocation: newZealand, shipOrigin: christchurch, shipDestination: sydney, filterCode: PrepaidCollectCodes.LDT, expectedResult: false);
			AssertShouldIncludeInApportionmentFor(companyLocation: newZealand, shipOrigin: christchurch, shipDestination: sydney, filterCode: PrepaidCollectCodes.FDT, expectedResult: true);
			AssertShouldIncludeInApportionmentFor(companyLocation: newZealand, shipOrigin: christchurch, shipDestination: sydney, filterCode: PrepaidCollectCodes.All, expectedResult: true);

			AssertShouldIncludeInApportionmentFor(companyLocation: australia, shipOrigin: losAngeles, shipDestination: sydney, filterCode: PrepaidCollectCodes.LOG, expectedResult: false);
			AssertShouldIncludeInApportionmentFor(companyLocation: australia, shipOrigin: losAngeles, shipDestination: sydney, filterCode: PrepaidCollectCodes.FOG, expectedResult: true);
			AssertShouldIncludeInApportionmentFor(companyLocation: australia, shipOrigin: melbourne, shipDestination: losAngeles, filterCode: PrepaidCollectCodes.LDT, expectedResult: false);
			AssertShouldIncludeInApportionmentFor(companyLocation: australia, shipOrigin: melbourne, shipDestination: losAngeles, filterCode: PrepaidCollectCodes.FDT, expectedResult: true);

			void AssertShouldIncludeInApportionmentFor(RefCountry companyLocation, RefUNLOCO shipOrigin, RefUNLOCO shipDestination, string filterCode, bool expectedResult)
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = companyLocation.Code;
				GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = losAngeles.Code;
				AssertNotEquals("When OrgProxy and GlbCompany countries are different, GlbCompany should be used to determine local-ness or foreign-ness.", GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.OrgProxy.CountryCode);
				shipment.JS_RL_NKOrigin = shipOrigin.Code;
				shipment.JS_RL_NKDestination = shipDestination.Code;
				cost.E6_PPDCLT = filterCode;
				var assertionMessage = $"Shipment from Origin {shipment.JS_RL_NKOrigin} to Destination {shipment.JS_RL_NKDestination} with company located at {GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort} and filter code {cost.E6_PPDCLT}, should{(expectedResult ? "" : " NOT")} be included in apportionment.";
				AssertEquals(assertionMessage, expectedResult, charge.ShouldIncludeInApportionment);
			}
		}

		public void TestSuspendSplittingApportionAmountChangeIsUsedForApportionment()
		{
			var cost = SetupConsolCostForAutoTickFinalFlag(10M);

			var charge1 = cost.ApportionmentCharges[0];

			Assert(charge1.JR_IsUsedForApportionment);

			using (charge1.SuspendSplittingApportionAmountChangeIsUsedForApportionment())
			{
				charge1.JR_OSCostAmt = 0M;
			}

			Assert(charge1.JR_IsUsedForApportionment);

			charge1.JR_OSCostAmt = 100M;

			Assert(charge1.JR_IsUsedForApportionment);

			charge1.JR_OSCostAmt = 0M;

			Assert(!charge1.JR_IsUsedForApportionment);
		}

		protected override string[] ZDecimalPropertiesNotRequiringRoundingByOSCurrency()
		{
			// Consol cost managed the rounding of apportion split charge. The list for properties is in JobConsolCost so we add that list here
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001");
			var consolcost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.USD, 1.5m, 150m, TestObjectCreator.AALSHI);
			return base.ZDecimalPropertiesNotRequiringRoundingByOSCurrency().Append(new[]
					{
						nameof(ApportionSplitCharge.JR_OSSellAmt),
						nameof(ApportionSplitCharge.JR_OSSellWHTAmt),
					}).Append(consolcost.AdditionalOSPropertiesRequiringRoundingApportionSplitCharge()).ToArray();
		}

		public void TestApportionChargeHasZeroLocalAmountAndNonZeroOSAmountCriticalValidationError()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);

			var shipment2 = TestObjectCreator.CreateShipment("S002", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);

			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.USD, 1.365M, 0.02M, TestObjectCreator.Creditor1);
			cost.PrepareForPosting();

			string expectedErrMsg1 = "Apportion Split Charge should not have 0 Cost Amount.";
			string expectedErrMsg2 = "Apportion Charge: OS Cost Amount is changed from 0.02 to 0.01 with exchange rate 1.365";

			var collectorService = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			collectorService.AddInfoWhenAllowed(cost.ApportionmentCharges[1].PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeLocalAmountIsZeroWithNonZeroOSAmount, () => expectedErrMsg1, CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);

			var charge1 = cost.ApportionmentCharges[0];
			charge1.JR_OSCostAmt = 0.01M;
			charge1.JR_LocalCostAmt = 0.01M;

			var charge2 = cost.ApportionmentCharges[1];
			using (charge2.SuspendAmountsCalculations())
			{
				charge2.JR_LocalCostAmt = 0M;
				charge2.JR_OSCostAmt = 0.01M;
			}

			var ex = AssertExceptionThrown<OnSavingCriticalCheckException>(() => Factory.Save());

			AssertEquals(nameof(CriticalValidationErrorType.JobChargeLinkedToConsolCostInvoiceHasZeroCostAmount_11), ex.ErrorType);
			AssertContains(expectedErrMsg1, ex.DeveloperErrorMessage);
			AssertContains(expectedErrMsg2, ex.DeveloperErrorMessage);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestApportionChargeOnClosedJobThrowsCriticalValidationError()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var validJob = TestObjectCreator.CreateJob(shipment);

			var shipment2 = TestObjectCreator.CreateShipment("S002", consol);
			var closedJob = TestObjectCreator.CreateJob(shipment2);
			closedJob.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 20, TestObjectCreator.Creditor1, AllocationMethod.Shipment);
			cost.PrepareForPosting();

			var ex = AssertExceptionThrown<OnSavingCriticalCheckException>(() => Factory.Save());

			CombineAssertions(() =>
			{
				var apportionedChargeOnClosedJob = closedJob.Charges[0];
				AssertNotNull("Pre-condition: charge should be apportioned", apportionedChargeOnClosedJob);

				AssertEquals(nameof(CriticalValidationErrorType.JobChargeLinkedToClosedJob), ex.ErrorType);
				AssertContains(CriticalValidationMessageTemplate.JobChargeLinkedToClosedJobErrorMessage, ex.DeveloperErrorMessage);
				AssertContains("Job Status HasChanges: False (CLS)", ex.DeveloperErrorMessage);
				AssertContains("\r\nUser can re-open jobs: True, True", ex.DeveloperErrorMessage);
				AssertContains("\r\nLast Job Opened Time: ", ex.DeveloperErrorMessage);
				AssertContains("\r\nLast Job Closed Time: ", ex.DeveloperErrorMessage);
				AssertContains("\r\nLast Job Closed User: E", ex.DeveloperErrorMessage);

				AssertContains("\r\n\r\nJobCharge:", ex.DeveloperErrorMessage);
				AssertContains("\r\n\r\nJobChargeCreatedOnClosedJobStackTrace:", ex.DeveloperErrorMessage);
				AssertContains("\r\n\r\nJobChargeConstructorStackTrace:", ex.DeveloperErrorMessage);
				AssertContains("\r\nConsolCost ConstructorStackTrace:", ex.DeveloperErrorMessage);
			});

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestApportionChargeDeleteCriticalValidationError()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var job = TestObjectCreator.CreateJob(shipment, createWithMutex: false);
			var shipment2 = TestObjectCreator.CreateShipment("S002", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2, createWithMutex: false);
			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.USD, 1.365M, 0.02M, TestObjectCreator.Creditor1);

			cost.ApportionmentCharges[0].JR_JH = ZGuid.Empty;
			cost.ApportionmentCharges[1].JR_JH = ZGuid.NewZGuid();

			cost.PrepareForPosting();

			AssertEquals(1, ErrorReporter.TotalErrorCount); //due to the fact that we are using ReportOnce, two charges only get reported once
			AssertEquals(expected: true, ErrorReporter.HasBeenReported("ApportionSplitCharge without InvoicingJob"));
			ErrorReporter.Clear();

			var expectedErrMsg = @"Overseas cost amount is not equal to the sum of the apportionment's overseas cost amount.";
			var ex = AssertExceptionThrown<OnSavingCriticalCheckException>(() => Factory.Save());
			AssertContains(expectedErrMsg, ex.DeveloperErrorMessage);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestApportionChargeDeleteDetailError()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var job = TestObjectCreator.CreateJob(shipment, createWithMutex: false);
			var shipment2 = TestObjectCreator.CreateShipment("S002", consol);
			TestObjectCreator.CreateJob(shipment2, createWithMutex: false);
			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.USD, 1.365M, 0.02M, TestObjectCreator.Creditor1);
			job.Delete();

			cost.ApportionmentCharges[1].JR_JH = ZGuid.NewZGuid();

			cost.PrepareForPosting();

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals(expected: true, ErrorReporter.HasBeenReported("ApportionSplitCharge without InvoicingJob"));
			Assert(ErrorReporter.LastMessageReported.Contains("ApportionSplitCharge without InvoicingJob. New Apportionment Charge will be deleted without finding a matching Job Charge or creating new one."));
			Assert(ErrorReporter.LastMessageReported.Contains("Job Deleted:"));
			Assert(ErrorReporter.LastMessageReported.Contains("Charge Details:"));
			ErrorReporter.Clear();
		}

		public void TestIsGatewaySellApportionmentCharge()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJDisabled_ForTestOnly();

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(consolNum: "C001", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment = gatewayConsol.Shipments.AddNew();

			using (TestObjectCreator.CreateJob(shipment))
			using (var gtwJob = TestObjectCreator.CreateJob(gatewayConsol))
			{
				var gtwCharge = gtwJob.Charges.AddNew();
				gtwCharge.JR_AC = TestObjectCreator.FRT.PK;
				gtwCharge.JR_OH_SellAccount = gtwCharge.Branch.OrgProxy.PK;
				gtwCharge.JR_OSSellAmt = 3333m;
				gtwCharge.JR_OSCostAmt = 0m;
				gtwCharge.JR_RX_NKSellCurrency = "AUD";
				gtwCharge.JR_JH_InternalJob = gtwJob.PK;
				gtwCharge.JR_GB_InternalBranch = gtwCharge.JR_GB;
				gtwCharge.JR_GE_InternalDept = gtwCharge.JR_GE;

				var gtwSellApportionmentList = gatewayConsol.GetApportionments(true);
				AssertEquals("PreCondition", 0, gtwSellApportionmentList.CostsCollection.Count);
				gtwSellApportionmentList.PrepareForConsolCosting();
				AssertEquals("PreCondition", 1, gtwSellApportionmentList.CostsCollection.Count);

				var gtwConsolSell = gtwSellApportionmentList.CostsCollection[0];
				AssertEquals("PreCondition", 1, gtwConsolSell.ApportionmentCharges.Count);
				AssertEquals("Gateway Sell Apportionment Charge.", true, gtwConsolSell.ApportionmentCharges[0].IsGatewaySellApportionmentCharge);

				var gtwCostApportionmentList = gatewayConsol.GetApportionments(false);
				AssertEquals("PreCondition", 0, gtwCostApportionmentList.CostsCollection.Count);
				gtwCostApportionmentList.PrepareForConsolCosting();
				AssertEquals("PreCondition", 0, gtwCostApportionmentList.CostsCollection.Count);

				using (gatewayConsol.SetTempContext(Enterprise.Integration.Accounting.BusinessContext.EnableDirectSettingConsolCostParent))
				{
					var gtwConsolCost = gtwCostApportionmentList.CostsCollection.AddNew();
					AssertEquals("PreCondition", 1, gtwConsolCost.ApportionmentCharges.Count);
					AssertEquals("Consol Cost Apoortionment Charge.", false, gtwConsolCost.ApportionmentCharges[0].IsGatewaySellApportionmentCharge);
				}
			}
		}

		public void TestApportionChargeCostAmountChangeFromZeroToOtherWhenCostAccountIsEmptyCritivalValidationError()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.ChargeSetsCreditorDifferentToConsolCost);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			TestObjectCreator.CreateJob(shipment, false);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100, TestObjectCreator.Creditor1);

			var apportionmentCharge = consolCost.ApportionmentCharges[0];
			apportionmentCharge.JR_OH_CostAccount = Guid.Empty;
			apportionmentCharge.JR_OSCostAmt = 0;
			apportionmentCharge.JR_OSCostAmt = 100;

			var errorInfo = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfoSafe(apportionmentCharge.PK, CriticalValidationInfoCollectorServiceKeyType.ChargeSetsCreditorDifferentToConsolCost);

			AssertContains($@"Charge account '00000000-0000-0000-0000-000000000000' Consol Cost account '{TestObjectCreator.Creditor1.PK}'.
Call stack:
", errorInfo);
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).ClearServiceCache();
		}

		public void TestApportionSplitChargeCostAmountIsSetWithZeroCritivalValidationError_JROSCostAmtSetFromNonZeroToZero()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			TestObjectCreator.CreateJob(shipment, false);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100, TestObjectCreator.Creditor1);

			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfoSafe(consolCost.PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeCostAmountIsSetWithZero);

			var apportionmentCharge = consolCost.ApportionmentCharges[0];
			apportionmentCharge.JR_OSCostAmt = 120;
			apportionmentCharge.JR_OH_CostAccount = Guid.Empty;

			var errorInfo = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfoSafe(consolCost.PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeCostAmountIsSetWithZero);
			AssertContains($@"ApportionSplitChargeCostAmountIsSetWithZero: There is no data collected for this PK.", errorInfo);

			apportionmentCharge.JR_OSCostAmt = 0;
			errorInfo = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfoSafe(consolCost.PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeCostAmountIsSetWithZero);
			AssertContains($@"OS Cost Amount is changed from 120 to 0. CostAccount is Empty : True.
Call stack:
", errorInfo);
		}

		public void TestApportionSplitChargeCostAmountIsSetWithZeroCritivalValidationError_JROSCostAmtSetFromZeroToNonZero()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			TestObjectCreator.CreateJob(shipment, false);

			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(Guid.NewGuid(), CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeCostAmountIsSetWithZero);

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100, TestObjectCreator.Creditor1);

			var errorInfo = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(consolCost.PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeCostAmountIsSetWithZero);
			AssertContains($@"OS Cost Amount is changed from 0 to 100. CostAccount is Empty : False.
Call stack:
", errorInfo);
		}

		public override void TestNewWIPIsCreatedOnSave_WhenPreviousWIPAmountEqualsNewAmountPlusCFX()
		{
			Assert("Test not applicable to ApportionSplitCharge: WIPs not created directly by ApportionSplitCharge", true);
		}

		public override void TestShouldReverseWIP_WithBillInInvoiceCurrencyWithLocalSellCurrency_AndExchangeRateCalculation()
		{
			Assert("Test not applicable to ApportionSplitCharge: WIPs not created directly by ApportionSplitCharge", true);
		}

		public void TestCheckJR_JH_InternalJobValidation_ApportionSplitChargeInGateWayConsol_WhenNewChargeIsAdded_BeforeSave_HasNoError()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());
			var branch1 = TestObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany);
			var consol = TestObjectCreator.CreateGatewayConsol("SGSIN", "AUSYD", "C0002", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var gatewayJob1 = TestObjectCreator.CreateJob(consol);
			var mainShipment = TestObjectCreator.CreateMasterShipment("S0001", consol);

			var subShipment1 = TestObjectCreator.CreateShipment("S0002", consol);
			subShipment1.JS_JS_ColoadMasterShipment = mainShipment.PK;

			var subShipment2 = TestObjectCreator.CreateShipment("S0003", consol);
			subShipment2.JS_JS_ColoadMasterShipment = mainShipment.PK;
			Factory.Save();

			try
			{
				GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob1, TestObjectCreator.CC1.PK, 100);
				GatewaySellToCostSynchroniser.Synchronise(gatewayJob1);
				var consolCost = Factory.LoadTop1<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));

				consolCost.E6_OSCostAmount = 100M;
				consolCost.E6_OH_Creditor = branch1.OrgProxy.PK;
				Assert(consol.IsGatewayBillingEnabled());
				consolCost.E6_ApportionToRelatedShipments = true;

				var splitCharge1 = consolCost.ApportionmentCharges[1];
				AssertNoErrors(splitCharge1.JR_JH_InternalJobInfo);

				var splitCharge2 = consolCost.ApportionmentCharges[2];
				AssertNoErrors(splitCharge2.JR_JH_InternalJobInfo);
			}
			finally
			{
				var apportionmentList = consol.GetApportionments(true);
				AssertNotNull(apportionmentList);
				apportionmentList.ReleaseMutexes();
			}
		}

		#region Auto Tick Final Flag

		public void TestAutoTickFinalFlag_CostVarianceAuthorisationRequirement()
		{
			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.JobAndChargeCode);

			var cost = SetupConsolCostForAutoTickFinalFlag(150M);

			AssertEquals("Should have 2 charge lines", 2, cost.ApportionmentCharges.Count);
			var charge1 = cost.ApportionmentCharges[0];
			var charge2 = cost.ApportionmentCharges[1];

			charge1.JR_OSCostAmt = 120M;
			charge2.JR_OSCostAmt = 30M;
			AssertEquals("The cost variance requires 1st level approval, 120 - 0 is above 100.", ZBool.False, charge1.IsFinal);
			AssertEquals("The cost variance requires None approval, 30 - 0 is NOT above 100.", ZBool.True, charge2.IsFinal);

			charge1.JR_OSCostAmt = 40M;
			charge2.JR_OSCostAmt = 110M;
			AssertEquals("The cost variance requires None approval, 40 - 0 is NOT above 100.", ZBool.True, charge1.IsFinal);
			AssertEquals("The cost variance requires 1st level approval, 110 - 0 is above 100.", ZBool.False, charge2.IsFinal);
		}

		public void TestAutoTickFinalFlag_CostVarianceAuthorisationRequirementForJCR()
		{
			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.JobAndCreditor);

			var cost = SetupConsolCostForAutoTickFinalFlag(170M);

			AssertEquals("Should have 2 charge lines", 2, cost.ApportionmentCharges.Count);
			var charge1 = cost.ApportionmentCharges[0];
			var charge2 = cost.ApportionmentCharges[1];
			charge1.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			charge2.JR_OH_CostAccount = TestObjectCreator.Creditor2.PK;

			charge1.JR_OSCostAmt = 140M;
			charge2.JR_OSCostAmt = 30M;
			AssertEquals("The cost variance requires 1st level approval, 140 - 0 is above 100.", ZBool.False, charge1.IsFinal);
			AssertEquals("The cost variance requires None approval, 30 - 0 is NOT above 100.", ZBool.True, charge2.IsFinal);

			charge1.JR_OSCostAmt = 40M;
			charge2.JR_OSCostAmt = 130M;
			AssertEquals("The cost variance requires None approval, 40 - 0 is NOT above 100.", ZBool.True, charge1.IsFinal);
			AssertEquals("The cost variance requires 1st level approval, 130 - 0 is above 100.", ZBool.False, charge2.IsFinal);
		}

		public void TestAutoTickFinalFlag_CostVarianceAuthorisationRequirement_WithImportedCharge()
		{
			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.ImportedChargeOrJobChargeCode);

			var cost = SetupConsolCostForAutoTickFinalFlag(150M);

			AssertEquals("Should have 2 charge lines", 2, cost.ApportionmentCharges.Count);
			var charge1 = cost.ApportionmentCharges[0];
			var charge2 = cost.ApportionmentCharges[1];

			var relatedConsolCost = Factory.NewWithValidTestData<JobConsolCost>();
			relatedConsolCost.ApportionmentCharges.AddNew();
			relatedConsolCost.ApportionmentCharges[0].JR_JH = charge1.JR_JH;
			relatedConsolCost.ApportionmentCharges.AddNew();
			relatedConsolCost.ApportionmentCharges[1].JR_JH = charge2.JR_JH;

			cost.RelatedConsolCostPK = relatedConsolCost.PK;

			charge1.JR_OSCostAmt = 120M;
			charge2.JR_OSCostAmt = 30M;
			AssertEquals("The cost variance requires 1st level approval, 120 - 0 is above 100.", ZBool.False, charge1.IsFinal);
			AssertEquals("The cost variance requires None approval, 30 - 0 is NOT above 100.", ZBool.True, charge2.IsFinal);

			charge1.JR_OSCostAmt = 40M;
			charge2.JR_OSCostAmt = 110M;
			AssertEquals("The cost variance requires None approval, 40 - 0 is NOT above 100.", ZBool.True, charge1.IsFinal);
			AssertEquals("The cost variance requires 1st level approval, 110 - 0 is above 100.", ZBool.False, charge2.IsFinal);
		}

		public void TestAutoTickFinalFlag_CostVarianceAuthorisationRequirementForJCR_WithImportedCharge()
		{
			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.ImportedChargeOrJobChargeCode);

			var cost = SetupConsolCostForAutoTickFinalFlag(150M);

			AssertEquals("Should have 2 charge lines", 2, cost.ApportionmentCharges.Count);
			var charge1 = cost.ApportionmentCharges[0];
			var charge2 = cost.ApportionmentCharges[1];
			charge1.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			charge2.JR_OH_CostAccount = TestObjectCreator.Creditor2.PK;

			var relatedConsolCost = Factory.NewWithValidTestData<JobConsolCost>();
			relatedConsolCost.ApportionmentCharges.AddNew();
			relatedConsolCost.ApportionmentCharges[0].JR_JH = charge1.JR_JH;
			relatedConsolCost.ApportionmentCharges[0].JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			relatedConsolCost.ApportionmentCharges.AddNew();
			relatedConsolCost.ApportionmentCharges[1].JR_JH = charge2.JR_JH;
			relatedConsolCost.ApportionmentCharges[1].JR_OH_CostAccount = TestObjectCreator.Creditor2.PK;

			cost.RelatedConsolCostPK = relatedConsolCost.PK;

			charge1.JR_OSCostAmt = 120M;
			charge2.JR_OSCostAmt = 30M;
			AssertEquals("The cost variance requires 1st level approval, 120 - 0 is above 100.", ZBool.False, charge1.IsFinal);
			AssertEquals("The cost variance requires None approval, 30 - 0 is NOT above 100.", ZBool.True, charge2.IsFinal);

			charge1.JR_OSCostAmt = 40M;
			charge2.JR_OSCostAmt = 110M;
			AssertEquals("The cost variance requires None approval, 40 - 0 is NOT above 100.", ZBool.True, charge1.IsFinal);
			AssertEquals("The cost variance requires 1st level approval, 110 - 0 is above 100.", ZBool.False, charge2.IsFinal);
		}

		public void TestAutoTickFinalFlag_TotalAuthorisationRequirement()
		{
			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.JobAndChargeCode);

			var cost = SetupConsolCostForAutoTickFinalFlag(180M);

			AssertEquals("Should have 2 charge lines", 2, cost.ApportionmentCharges.Count);
			var charge1 = cost.ApportionmentCharges[0];
			var charge2 = cost.ApportionmentCharges[1];

			charge1.JR_OSCostAmt = 90M;
			charge2.JR_OSCostAmt = 90M;
			AssertEquals("90 - 0 is NOT above 100.", ZBool.True, charge1.IsFinal);
			AssertEquals("90 - 0 is NOT above 100.", ZBool.True, charge2.IsFinal);

			var invoice = cost.ParentAPInvoice as APInvoice;
			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.JobAndChargeCode, true, invoice);

			charge1.JR_OSCostAmt = 91M;
			charge2.JR_OSCostAmt = 89M;
			AssertEquals("The total cost variance requires 1st level approval.", ZBool.False, charge1.IsFinal);
			AssertEquals("The total cost variance requires 1st level approval.", ZBool.False, charge2.IsFinal);

			cost.E6_OSCostAmount = 140M;
			charge1.JR_OSCostAmt = 90M;
			charge2.JR_OSCostAmt = 50M;
			AssertEquals("The total cost variance requires None approval, 90 - 0 is NOT above 100.", ZBool.True, charge1.IsFinal);
			AssertEquals("The total cost variance requires None approval, 50 - 0 is NOT above 100.", ZBool.True, charge2.IsFinal);
		}

		public void TestAutoTickFinalFlag_FinalFlagInTheSameJob()
		{
			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.Job);

			var cost = SetupConsolCostForAutoTickFinalFlag(180M);

			var invoice = cost.ParentAPInvoice;
			var cost2 = invoice.ConsolCosting.ConsolCosts.AddNew();
			cost2.E6_ParentID = cost.E6_ParentID;
			cost2.E6_ParentTableCode = "JK";
			cost2.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost2.E6_OSCostAmount = 20M;
			Assert(!cost2.HasErrors);

			AssertEquals("Should have 2 charge lines", 2, cost.ApportionmentCharges.Count);
			AssertEquals("Should have 2 charge lines", 2, cost2.ApportionmentCharges.Count);
			var charge1 = cost.ApportionmentCharges[0];
			var charge2 = cost.ApportionmentCharges[1];
			var charge3 = cost2.ApportionmentCharges.Cast<ApportionSplitCharge>().First(x => x.JR_JH == charge1.JR_JH);
			var charge4 = cost2.ApportionmentCharges.Cast<ApportionSplitCharge>().First(x => x.JR_JH == charge2.JR_JH);

			charge1.JR_OSCostAmt = 100M;
			charge2.JR_OSCostAmt = 80M;
			charge3.JR_OSCostAmt = 10M;
			charge4.JR_OSCostAmt = 10M;
			AssertEquals("The cost variance requires 1st level approval, 100 + 10 - 0 = 110 is above 100.", ZBool.False, charge1.IsFinal);
			AssertEquals("The cost variance requires None approval, 80 + 10 - 0 = 90 is NOT above 100", ZBool.True, charge2.IsFinal);
			AssertEquals("The cost variance requires 1st level approval, 100 + 10 - 0 = 110 is above 100.", ZBool.False, charge3.IsFinal);
			AssertEquals("The cost variance requires None approval, 80 + 10 - 0 = 90 is NOT above 100", ZBool.True, charge4.IsFinal);

			charge1.JR_OSCostAmt = 90M;
			charge2.JR_OSCostAmt = 90M;
			charge3.JR_OSCostAmt = 10M;
			charge4.JR_OSCostAmt = 10M;
			AssertEquals("The cost variance requires None approval, 90 + 10 - 0 = 100 is NOT above 100.", ZBool.True, charge1.IsFinal);
			AssertEquals("The cost variance requires None approval, 90 + 10 - 0 = 100 is NOT above 100.", ZBool.True, charge2.IsFinal);
			AssertEquals("The cost variance requires None approval, 90 + 10 - 0 = 100 is NOT above 100.", ZBool.True, charge3.IsFinal);
			AssertEquals("The cost variance requires None approval, 90 + 10 - 0 = 100 is NOT above 100.", ZBool.True, charge4.IsFinal);

			charge1.IsFinal = ZBool.False;
			AssertEquals("Manually changed.", ZBool.False, charge1.IsFinal);
			AssertEquals("Not related to the same job.", ZBool.True, charge2.IsFinal);
			AssertEquals("Be changed for it's in the same job.", ZBool.False, charge3.IsFinal);
			AssertEquals("Not related to the same job.", ZBool.True, charge4.IsFinal);
		}

		public void TestAutoTickFinalFlag_FinalFlagInTheSameJob_WithImportedCharge()
		{
			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.ImportedChargeOrJob);

			var cost = SetupConsolCostForAutoTickFinalFlag(180M);

			var invoice = cost.ParentAPInvoice;
			var cost2 = invoice.ConsolCosting.ConsolCosts.AddNew();
			cost2.E6_ParentID = cost.E6_ParentID;
			cost2.E6_ParentTableCode = "JK";
			cost2.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost2.E6_OSCostAmount = 20M;
			Assert(!cost2.HasErrors);

			AssertEquals("Should have 2 charge lines", 2, cost.ApportionmentCharges.Count);
			AssertEquals("Should have 2 charge lines", 2, cost2.ApportionmentCharges.Count);
			var charge1 = cost.ApportionmentCharges[0];
			var charge2 = cost.ApportionmentCharges[1];
			var charge3 = cost2.ApportionmentCharges.Cast<ApportionSplitCharge>().First(x => x.JR_JH == charge1.JR_JH);
			var charge4 = cost2.ApportionmentCharges.Cast<ApportionSplitCharge>().First(x => x.JR_JH == charge2.JR_JH);

			charge1.RelatedApportionChargeFromDB = Factory.NewWithValidTestData<ApportionSplitCharge>();
			charge2.RelatedApportionChargeFromDB = Factory.NewWithValidTestData<ApportionSplitCharge>();

			charge1.JR_OSCostAmt = 110M;
			charge2.JR_OSCostAmt = 120M;
			charge3.JR_OSCostAmt = 130M;
			charge4.JR_OSCostAmt = 140M;
			AssertEquals("The cost variance requires 1st level approval, 110 - 0 = 110 is above 100.", ZBool.False, charge1.IsFinal);
			AssertEquals("The cost variance requires 1st level approval, 120 - 0 = 120 is above 100.", ZBool.False, charge2.IsFinal);
			AssertEquals("The cost variance requires 1st level approval, 130 - 0 = 130 is above 100.", ZBool.False, charge3.IsFinal);
			AssertEquals("The cost variance requires 1st level approval, 140 - 0 = 140 is above 100.", ZBool.False, charge4.IsFinal);

			charge1.JR_OSCostAmt = 100M;
			charge2.JR_OSCostAmt = 80M;
			charge3.JR_OSCostAmt = 10M;
			charge4.JR_OSCostAmt = 10M;
			AssertEquals("The cost variance requires None approval, 100 - 0 = 100 is NOT above 100.", ZBool.True, charge1.IsFinal);
			AssertEquals("The cost variance requires None approval, 80 - 0 = 90 is NOT above 100", ZBool.True, charge2.IsFinal);
			AssertEquals("The cost variance requires None approval, 10 - 0 = 10 is above 100.", ZBool.True, charge3.IsFinal);
			AssertEquals("The cost variance requires None approval, 10 - 0 = 10 is NOT above 100", ZBool.True, charge4.IsFinal);

			charge1.IsFinal = ZBool.False;
			AssertEquals("Manually changed.", ZBool.False, charge1.IsFinal);
			AssertEquals("Not related to the same job.", ZBool.True, charge2.IsFinal);
			AssertEquals("Not changed for it's speicified charge.", ZBool.True, charge3.IsFinal);
			AssertEquals("Not related to the same job.", ZBool.True, charge4.IsFinal);
		}

		public void TestAutoTickFinalFlag_ChargeCode_Branch_Department_E6_ParentID()
		{
			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.JobAndChargeCode);

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var job = TestObjectCreator.CreateJob(shipment);
			var apps = new ApportionmentListing(Factory, consol);

			var consol2 = Factory.New<ForwardingConsol>();
			var shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_ActualChargeable = 1m;
			var job2 = TestObjectCreator.CreateJob(shipment2);
			var apps2 = new ApportionmentListing(Factory, consol2);

			try
			{
				var consolCost = apps.CostsCollection.TryAddNew();
				consolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
				consolCost.E6_LocalCostAmount = 100m;
				AssertEquals("Should have 1 charge", 1, consolCost.ApportionmentCharges.Count);

				var consolCost2 = apps2.CostsCollection.TryAddNew();
				consolCost2.E6_AC_ChargeCode = TestObjectCreator.CC2.PK;
				consolCost2.E6_LocalCostAmount = 100m;
				AssertEquals("Should have 1 charge", 1, consolCost2.ApportionmentCharges.Count);

				Factory.Save();

				var invoice = Factory.NewWithValidTestData<APInvoice>();
				var cost = invoice.ConsolCosting.ConsolCosts.AddNew();
				cost.E6_ParentID = consolCost.E6_ParentID;
				cost.E6_ParentTableCode = consolCost.E6_ParentTableCode;
				cost.E6_AC_ChargeCode = consolCost.E6_AC_ChargeCode;
				AssertEquals(100M, cost.E6_OSCostAmount);
				AssertEquals("Should have 1 charge", 1, cost.ApportionmentCharges.Count);
				var charge = cost.ApportionmentCharges[0];
				AssertEquals(100M, charge.JR_OSCostAmt);

				charge.JR_OSCostAmt = 150M;
				AssertEquals("150 - 100 is NOT above 100.", ZBool.True, charge.IsFinal);

				//ChargeCode
				cost.E6_AC_ChargeCode = TestObjectCreator.CC3.PK;
				AssertEquals("The cost variance requires 1st level approval, 150 - 0 is above 100.", ZBool.False, charge.IsFinal);

				cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
				AssertEquals("Reset the charge status.", ZBool.True, charge.IsFinal);

				//Branch
				charge.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
				AssertEquals("The cost variance requires 1st level approval, 150 - 0 is above 100.", ZBool.False, charge.IsFinal);

				charge.JR_GB = GlbBranch.CurrentBranch.PK;
				AssertEquals("Reset the charge status.", ZBool.True, charge.IsFinal);

				//Department
				charge.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;
				AssertEquals("The cost variance requires 1st level approval, 150 - 0 is above 100.", ZBool.False, charge.IsFinal);

				charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
				AssertEquals("Reset the charge status.", ZBool.True, charge.IsFinal);

				//ParentID
				charge.IsFinal = ZBool.False;

				cost.E6_ParentID = consolCost2.E6_ParentID;
				AssertEquals("The cost variance requires None approval, 100 - 0 is NOT above 100.", ZBool.True, cost.ApportionmentCharges[0].IsFinal);

				cost.E6_ParentID = consolCost.E6_ParentID;
				AssertEquals("Reset the charge status.", ZBool.True, cost.ApportionmentCharges[0].IsFinal);
			}
			finally
			{
				apps.ReleaseMutexes();
				apps2.ReleaseMutexes();
			}
		}

		public void TestAutoTickFinalFlag_ChangeFinalFlagForOriginalGroup()
		{
			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.Job);

			var cost = SetupConsolCostForAutoTickFinalFlag(180M);

			var invoice = cost.ParentAPInvoice;
			var cost2 = invoice.ConsolCosting.ConsolCosts.AddNew();
			cost2.E6_ParentID = cost.E6_ParentID;
			cost2.E6_ParentTableCode = "JK";
			cost2.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost2.E6_OSCostAmount = 20M;
			Assert(!cost2.HasErrors);

			AssertEquals("Should have 2 charge lines", 2, cost.ApportionmentCharges.Count);
			var charge1 = cost.ApportionmentCharges[0];
			var charge2 = cost2.ApportionmentCharges.Cast<ApportionSplitCharge>().First(x => x.JR_JH == charge1.JR_JH);

			charge1.JR_OSCostAmt = 60M;
			charge2.JR_OSCostAmt = 60M;
			AssertEquals("The cost variance requires 1st level approval, (60 - 0) + (60 - 0) = 120 is above 100.", ZBool.False, charge1.IsFinal);
			AssertEquals("The cost variance requires 1st level approval, (60 - 0) + (60 - 0) = 120 is above 100.", ZBool.False, charge2.IsFinal);

			//ChargeCode
			cost.E6_AC_ChargeCode = TestObjectCreator.CC3.PK;
			AssertEquals("Have no effect to the charge, since the Variance Comparison Option is Job.", ZBool.False, charge1.IsFinal);
			AssertEquals("Have no effect to the charge, since the Variance Comparison Option is Job.", ZBool.False, charge2.IsFinal);

			//Branch
			charge1.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			AssertEquals("The cost variance requires None approval, 60 - 0 is NOT above 100.", ZBool.True, charge1.IsFinal);
			AssertEquals("Should be recalculated, the cost variance requires None approval, 60 - 0 is NOT above 100.", ZBool.True, charge2.IsFinal);

			charge1.JR_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals("Reset the charge status.", ZBool.False, charge1.IsFinal);
			AssertEquals("Reset the charge status.", ZBool.False, charge2.IsFinal);

			//Department
			charge1.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;
			AssertEquals("The cost variance requires None approval, 60 - 0 is NOT above 100.", ZBool.True, charge1.IsFinal);
			AssertEquals("Should be recalculated, the cost variance requires None approval, 60 - 0 is NOT above 100.", ZBool.True, charge2.IsFinal);

			charge1.JR_GE = GlbDepartment.CurrentDepartment.PK;
			AssertEquals("Reset the charge status.", ZBool.False, charge1.IsFinal);
			AssertEquals("Reset the charge status.", ZBool.False, charge2.IsFinal);

			//Delete
			invoice.ConsolCosting.ConsolCosts.RemoveAndDelete(cost);
			AssertEquals("Should be recalculated, the cost variance requires None approval, 60 - 0 is NOT above 100.", ZBool.True, charge2.IsFinal);
		}

		public void TestAutoTickFinalFlag_CostVarianceNoApprovalRequired()
		{
			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.ImportedChargeOrJob);

			GlbDepartment.GetCurrentDepartment(Factory).GE_Misc = false;
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C001";
			var shipment1 = consol.Shipments.AddNew();
			var job1 = TestObjectCreator.CreateJob(shipment1);

			Factory.Save();

			var invoice = Factory.NewWithValidTestData<APInvoice>();

			var cost = invoice.ConsolCosting.ConsolCosts.AddNew();
			cost.E6_ParentID = consol.PK;
			cost.E6_ParentTableCode = "JK";
			cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost.E6_OSCostAmount = 100M;
			cost.E6_RX_NKCurrency = "USD";

			AssertEquals("Should have 1 charge lines", 1, cost.ApportionmentCharges.Count);
			var charge1 = cost.ApportionmentCharges[0];

			charge1.RelatedApportionChargeFromDB = Factory.NewWithValidTestData<ApportionSplitCharge>();
			charge1.RelatedApportionChargeFromDB.JR_RX_NKCostCurrency = "USD";
			charge1.RelatedApportionChargeFromDB.JR_OSCostAmt = 100M;

			charge1.JR_RX_NKCostCurrency = "USD";
			charge1.JR_OSCostAmt = 100M;
			charge1.JR_LocalCostAmt = 500M;

			Assert(AccountingConfigurationRegistry.Instance.CostVarianceNoApprovalRequired.Value);
			Assert(charge1.IsParentConsolCostImported);
			AssertNotEquals(charge1.JR_CostCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertEquals(charge1.JR_CostCurrency, charge1.RelatedApportionChargeFromDB.JR_RX_NKCostCurrency);
			AssertEquals(charge1.JR_OSCostAmt, charge1.RelatedApportionChargeFromDB.JR_OSCostAmt);
			AssertEquals("Both ACR and CST amount and currency matches, no need cost variance approval required, although 500 - 100 is above 100.", ZBool.True, charge1.IsFinal);

			charge1.JR_OSCostAmt = 200M;
			charge1.JR_LocalCostAmt = 800M;
			AssertEquals("The cost variance requires 1st level approval, 800 - 200 is above 100.", ZBool.False, charge1.IsFinal);

			charge1.JR_OSCostAmt = 100M;
			charge1.JR_RX_NKCostCurrency = "CNY";
			charge1.JR_LocalCostAmt = 400M;
			AssertEquals("The cost variance requires 1st level approval, 400 - 200 is above 100.", ZBool.False, charge1.IsFinal);
		}

		public void TestDeleteApportionChargeClearsExRatesOnChargeWithSameDataRow()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);

			var shipment2 = TestObjectCreator.CreateShipment("S002", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);

			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.USD, 1.3M, 10, TestObjectCreator.Creditor1);
			Factory.Save();

			var charge1 = Factory.Load<Charge>(cost.ApportionmentCharges[0].PK);
			charge1.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			charge1.JR_RX_NKCostCurrency = "EUR";
			charge1.JR_RX_NKSellInvoiceCurrency = "RUB";

			var charge2 = Factory.Load<Charge>(cost.ApportionmentCharges[1].PK);
			charge2.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			charge2.JR_RX_NKCostCurrency = "EUR";
			charge2.JR_RX_NKSellInvoiceCurrency = "RUB";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedConsol = newFactory.Load<ForwardingConsol>(consol.PK);
			var reloadedCost = new ApportionmentListing(newFactory, reloadedConsol).CostsCollection[0];
			var reloadedApportionmentCharge = reloadedCost.ApportionmentCharges[0];
			var reloadedCharge = newFactory.Load<Charge>(reloadedApportionmentCharge.PK);

			AssertNotNull(reloadedCharge.RevenueExchangeRate);
			AssertNull(reloadedCharge.CostExchangeRate);
			AssertNotNull(reloadedCharge.SellInvoiceExchangeRate);

			reloadedApportionmentCharge.Delete();

			AssertNull(reloadedCharge.RevenueExchangeRate);
			AssertNull(reloadedCharge.CostExchangeRate);
			AssertNull(reloadedCharge.SellInvoiceExchangeRate);
		}

		JobConsolCost SetupConsolCostForAutoTickFinalFlag(decimal amount)
		{
			GlbDepartment.GetCurrentDepartment(Factory).GE_Misc = false;
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C001";
			var shipment1 = consol.Shipments.AddNew();
			var job1 = TestObjectCreator.CreateJob(shipment1);
			var shipment2 = consol.Shipments.AddNew();
			var job2 = TestObjectCreator.CreateJob(shipment2);

			Factory.Save();

			var invoice = Factory.NewWithValidTestData<APInvoice>();

			var cost = invoice.ConsolCosting.ConsolCosts.AddNew();
			cost.E6_ParentID = consol.PK;
			cost.E6_ParentTableCode = "JK";
			cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost.E6_OSCostAmount = amount;
			Assert(!cost.HasErrors);

			return cost;
		}

		#endregion

		#region Implementation

		protected override BaseCharge GetChargeWithValidData(Job invoicingJob, AccChargeCode chargeCode)
		{
			var charge = TestObjectCreator.CreateCharge(invoicingJob, chargeCode);
			return Factory.Load<ApportionSplitCharge>(charge.PK);
		}

		#endregion
	}
}
