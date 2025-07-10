using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ConsolCosting.Testing
{
	using System.Data;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Integration;
	using Enterprise.Accounting.Business.ConsolCosting;
	using Enterprise.Environment;
	using Enterprise.Freight.Business;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	class ApportionmentCreatorTest : TestCaseWithFactory
	{
		public void TestIsApportionmentMethodPerChargeableUnit()
		{
			Assert(!ApportionmentCreator.IsApportionmentMethodPerChargeableUnit(null));
			Assert(!ApportionmentCreator.IsApportionmentMethodPerChargeableUnit(""));
			var allocationPerChargeableUnit = new[] { AllocationMethod.ChargeableUnits, AllocationMethod.GrossWeight, AllocationMethod.CapacityPerContainer, AllocationMethod.FreeSpaceContribution, AllocationMethod.GrossVolume };
			var allocationNotPerChargeableUnit = new[] { AllocationMethod.ContainerCount, AllocationMethod.OuterPackTotal, AllocationMethod.Manual, AllocationMethod.Revenue, AllocationMethod.Shipment, AllocationMethod.TwentyFootEquivalentUnit, };
			foreach (ICodeDescription allocationMethodPair in new CodeDescriptionPairList(OLookUpEditType.AllocationMethod))
			{
				var allocationMethod = allocationMethodPair.Code;
				var result = ApportionmentCreator.IsApportionmentMethodPerChargeableUnit(allocationMethod);
				bool isAllocationMethodMissed = true;
				if (allocationPerChargeableUnit.Contains(allocationMethod))
				{
					isAllocationMethodMissed = false;
					Assert(result);
				}

				if (allocationNotPerChargeableUnit.Contains(allocationMethod))
				{
					isAllocationMethodMissed = false;
					Assert(!result);
				}

				if (isAllocationMethodMissed)
				{
					Fail($"'{allocationMethod}' code is missing. Add every new allocation method to either of arrays based on allocation logic.");
				}
			}
		}

		public void TestFreeSpaceContributionApportionment()
		{
			var splitCharge1 = Factory.New<MockIApportionedCharge>();
			splitCharge1.ChargeableUnits = 300;
			splitCharge1.ExcessActualVolumeWeight = 248.666564;
			splitCharge1.IsUsedForApportionment = true;
			var splitCharge2 = Factory.New<MockIApportionedCharge>();
			splitCharge2.ChargeableUnits = 6;
			splitCharge2.ExcessActualVolumeWeight = 1.133324;
			splitCharge2.IsUsedForApportionment = true;
			var splitCharge3 = Factory.New<MockIApportionedCharge>();
			splitCharge3.ChargeableUnits = 66;
			splitCharge3.ExcessActualVolumeWeight = 1.999872;
			splitCharge3.IsUsedForApportionment = true;
			var splitCharge4 = Factory.New<MockIApportionedCharge>();
			splitCharge4.ChargeableUnits = 1152;
			splitCharge4.ExcessChargeableVolumeWeight = 80.002304;
			splitCharge4.IsUsedForApportionment = true;
			var splitCharge5 = Factory.New<MockIApportionedCharge>();
			splitCharge5.ChargeableUnits = 114;
			splitCharge5.ExcessChargeableVolumeWeight = 19.766894;
			splitCharge5.IsUsedForApportionment = true;
			var splitCharge6 = Factory.New<MockIApportionedCharge>();
			splitCharge6.ChargeableUnits = 423.5;
			splitCharge6.ExcessChargeableVolumeWeight = 130.467513;
			splitCharge6.IsUsedForApportionment = true;
			var splitCharge7 = Factory.New<MockIApportionedCharge>();
			splitCharge7.ChargeableUnits = 62;
			splitCharge7.ExcessChargeableVolumeWeight = 22.400124;
			splitCharge7.IsUsedForApportionment = true;
			var apportionedCharges = new IApportionedCharge[] { splitCharge1, splitCharge2, splitCharge3, splitCharge4, splitCharge5, splitCharge6, splitCharge7 };
			var mock = new MockIApportionedChargesHeader { Charges = apportionedCharges, ChargeCode = TestObjectCreator.CC1, ApportionmentMethod = AllocationMethod.FreeSpaceContribution, Currency = TestObjectCreator.AUD, FreeSpace = 252.663, };
			mock.Apportion(DummyBizoSchema.Z0_Money, 5163.96, DummyBizoSchema.Z0_AnotherDecimal, 5163.96);
			AssertEquals("charge 1 Z0_Money", 483.71m, splitCharge1.Z0_Money);
			AssertEquals("charge 1 Z0_AnotherDecimal", 483.71m, splitCharge1.Z0_AnotherDecimal);
			AssertEquals("charge 2 Z0_Money", 14.99m, splitCharge2.Z0_Money);
			AssertEquals("charge 2 Z0_AnotherDecimal", 14.99m, splitCharge2.Z0_AnotherDecimal);
			AssertEquals("charge 3 Z0_Money", 179.41m, splitCharge3.Z0_Money);
			AssertEquals("charge 3 Z0_AnotherDecimal", 179.41m, splitCharge3.Z0_AnotherDecimal);
			AssertEquals("charge 4 Z0_Money", 3069.37m, splitCharge4.Z0_Money);
			AssertEquals("charge 4 Z0_AnotherDecimal", 3069.37m, splitCharge4.Z0_AnotherDecimal);
			AssertEquals("charge 5 Z0_Money", 287.38m, splitCharge5.Z0_Money);
			AssertEquals("charge 5 Z0_AnotherDecimal", 287.38m, splitCharge5.Z0_AnotherDecimal);
			AssertEquals("charge 6 Z0_Money", 988.88m, splitCharge6.Z0_Money);
			AssertEquals("charge 6 Z0_AnotherDecimal", 988.88m, splitCharge6.Z0_AnotherDecimal);
			AssertEquals("charge 7 Z0_Money", 140.22m, splitCharge7.Z0_Money);
			AssertEquals("charge 7 Z0_AnotherDecimal", 140.22m, splitCharge7.Z0_AnotherDecimal);
		}

		public void TestFreeSpaceContributionApportionment_WithAllZeros()
		{
			var splitCharge1 = Factory.New<MockIApportionedCharge>();
			splitCharge1.IsUsedForApportionment = true;
			var apportionedCharges = new IApportionedCharge[] { splitCharge1 };
			var mock = new MockIApportionedChargesHeader { Charges = apportionedCharges, ChargeCode = TestObjectCreator.CC1, ApportionmentMethod = AllocationMethod.FreeSpaceContribution, Currency = TestObjectCreator.AUD, FreeSpace = 100, };
			mock.Apportion(DummyBizoSchema.Z0_AnotherDecimal, 5163.96, null, 0);
			AssertEquals("charge 1 Z0_Decimal", 5163.96m, splitCharge1.Z0_AnotherDecimal);
		}

		public void TestFreeSpaceContributionApportionment_ShouldTreatAlmostZeroAsZero()
		{
			var splitCharge1 = Factory.New<MockIApportionedCharge>();
			splitCharge1.ChargeableUnits = 100;
			splitCharge1.ExcessActualVolumeWeight = 7;
			splitCharge1.IsUsedForApportionment = true;

			var splitCharge2 = Factory.New<MockIApportionedCharge>();
			splitCharge2.ChargeableUnits = 0.00000000000000000001;
			splitCharge2.ExcessChargeableVolumeWeight = 1;
			splitCharge2.IsUsedForApportionment = true;

			var apportionedCharges = new IApportionedCharge[] { splitCharge1, splitCharge2 };
			var mock = new MockIApportionedChargesHeader { Charges = apportionedCharges, ChargeCode = TestObjectCreator.CC1, ApportionmentMethod = AllocationMethod.FreeSpaceContribution, Currency = TestObjectCreator.AUD, FreeSpace = 100, };

			mock.Apportion(DummyBizoSchema.Z0_Decimal, 10, null, 0);
			AssertEquals("charge 1 Z0_Decimal", 5m, splitCharge1.Z0_Decimal);
			AssertEquals("charge 2 Z0_Decimal", 5m, splitCharge2.Z0_Decimal);
		}

		public void TestFreeSpaceContributionApportionment_IsUsedForApportionment()
		{
			var splitCharge1 = Factory.New<MockIApportionedCharge>();
			splitCharge1.ChargeableUnits = 300;
			splitCharge1.ExcessActualVolumeWeight = 99;
			splitCharge1.IsUsedForApportionment = true;
			var splitCharge2 = Factory.New<MockIApportionedCharge>();
			splitCharge2.ChargeableUnits = 300;
			splitCharge2.ExcessChargeableVolumeWeight = 1;
			splitCharge2.IsUsedForApportionment = true;
			var apportionedCharges = new IApportionedCharge[] { splitCharge1, splitCharge2 };
			var mock = new MockIApportionedChargesHeader { Charges = apportionedCharges, ChargeCode = TestObjectCreator.CC1, ApportionmentMethod = AllocationMethod.FreeSpaceContribution, Currency = TestObjectCreator.AUD, FreeSpace = 40, };
			mock.Apportion(DummyBizoSchema.Z0_Decimal, 1000, null, 0);
			AssertEquals("charge 1 Z0_Decimal", 500m, splitCharge1.Z0_Decimal);
			AssertEquals("charge 2 Z0_Decimal", 500m, splitCharge2.Z0_Decimal);
			splitCharge1.IsUsedForApportionment = false;
			mock.Apportion(DummyBizoSchema.Z0_Decimal, 1000, null, 0);
			AssertEquals("charge 1 Z0_Decimal", 0m, splitCharge1.Z0_Decimal);
			AssertEquals("charge 2 Z0_Decimal", 1000m, splitCharge2.Z0_Decimal);
		}

		public void TestApportionmentChargeSCurrnecy()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("KRSEL", "AUSYD", "C00000001");
			var shipment = creator.CreateShipment("S00000001", consol);
			var consolCost = creator.CreateConsolCost(consol, creator.CC1, creator.AALSHI);
			consolCost.E6_RX_NKCurrency = "KRW";
			consolCost.E6_OSCostAmount = 60000m;
			consolCost.E6_ExchangeRate = 83.1486m;
			Factory.Save();
			AssertEquals("should be one Apportionment Charge created", 1, consolCost.ApportionmentCharges.Count);
			ApportionSplitCharge charge = consolCost.ApportionmentCharges[0];
			consolCost.E6_RX_NKCurrency = "AUD";
			AssertEquals("Consol Cost's OS Cost Amt is set to the original local cost amount", 721.60m, charge.JR_OSCostAmt);
			AssertEquals("After apportionment, Apportionment Charge's cost currency is set to AUD which has 2 decimal points", "AUD", charge.JR_RX_NKCostCurrency);
			AssertEquals("After apportionment, Apportionment Charge's cost amount is now not rounded because AUD has decimal points", 721.60m, charge.JR_OSCostAmt);
		}

		[ExpectNoExceptions("No System.ArgumentNullException is Expected")]
		public void TestApportionmentChargeSCurrnecyWithNull()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("KRSEL", "AUSYD", "C00000001");
			var shipment = creator.CreateShipment("S00000001", consol);
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_ParentID = shipment.PK;
			var consolCost = creator.CreateConsolCost(consol, creator.CC1, creator.AALSHI);
			consolCost.E6_ApportionmentMethod = AllocationMethod.Shipment;
			consolCost.E6_RX_NKCurrency = "ZZZ";
			consolCost.E6_IsTaxAmountOverridden = true;
			var charge = consolCost.ApportionmentCharges.AddNew();
			charge.JR_OSCostGSTAmt_Calc = 6000m;
			consolCost.E6_OSCostAmount = 6000m;
			consolCost.E6_ExchangeRate = 1m;
			AssertEquals("When Invalid Currency, JR_OSCostGSTAmount should be Zero", 0m, charge.JR_OSCostGSTAmt_Calc);
			consolCost.E6_RX_NKCurrency = "USD";
			charge = consolCost.ApportionmentCharges.AddNew();
			charge.JR_OSCostGSTAmt_Calc = 6000m;
			consolCost.E6_OSCostAmount = 6000m;
			consolCost.E6_ExchangeRate = 1m;
			AssertNotEquals("When Invalid Currency, JR_OSCostGSTAmount should be Zero", 0m, charge.JR_OSCostGSTAmt_Calc);
		}

		public void TestGetAllJobsRevenue()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GB = Env.CurrentBranch.PK;
			job.JH_GE = Env.CurrentDepartment.PK;
			job.JH_JobNum = "J0000100";
			IApportionedCharge[] apportionedCharges = new IApportionedCharge[3];
			ApportionSplitCharge splitCharge = Factory.New<ApportionSplitCharge>();
			splitCharge.JR_JH = job.PK;
			ApportionSplitCharge splitCharge2 = Factory.New<ApportionSplitCharge>();
			splitCharge2.JR_JH = job.PK;
			ApportionSplitCharge splitCharge3 = null;
			apportionedCharges[0] = splitCharge;
			apportionedCharges[1] = splitCharge2;
			apportionedCharges[2] = splitCharge3;
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			MockIApportionedChargesHeader mock = new MockIApportionedChargesHeader();
			mock.Charges = apportionedCharges;
			mock.ChargeCode = chargeCode;
			mock.ApportionmentMethod = AllocationMethod.Shipment;
			mock.Currency = currency;
			mock.Apportion(JobChargeSchema.JR_OSCostAmt, 1000.00m, JobChargeSchema.JR_AgentDeclaredCostAmt, 100.00m);
			//NullReferenceException should not be thrown by above call
			Assert(true);
		}

		public void TestTEUApportionment()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GB = Env.CurrentBranch.PK;
			job.JH_GE = Env.CurrentDepartment.PK;
			job.JH_JobNum = "J0000100";
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job2 = new Job.Loader(Factory, shipment2).TryCreateWithoutMutexForTestOnly();
			job2.JH_GB = Env.CurrentBranch.PK;
			job2.JH_GE = Env.CurrentDepartment.PK;
			job2.JH_JobNum = "J0000200";
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			shipment.Consols.Add(consol);
			shipment2.Consols.Add(consol);
			RefContainer refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			RefContainer refContainer2 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40NOR");
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_RC = refContainer.PK;
			consol.Containers.Add(container);
			PackLine outerPack1 = shipment.OuterPackLines.AddNew();
			outerPack1.SetContainer(consol, container);
			container = Factory.New<CommonContainer>();
			container.JC_RC = refContainer2.PK;
			consol.Containers.Add(container);
			PackLine outerPack2 = shipment.OuterPackLines.AddNew();
			outerPack2.SetContainer(consol, container);
			AssertEquals("shipment.Containers.TEUCount", 3.0m, shipment.ContainerTEUCount);
			container = Factory.New<CommonContainer>();
			container.JC_RC = refContainer.PK;
			consol.Containers.Add(container);
			outerPack1 = shipment2.OuterPackLines.AddNew();
			outerPack1.SetContainer(consol, container);
			container = Factory.New<CommonContainer>();
			container.JC_RC = refContainer2.PK;
			container.JC_ContainerCount = 2;
			consol.Containers.Add(container);
			outerPack2 = shipment2.OuterPackLines.AddNew();
			outerPack2.SetContainer(consol, container);
			AssertEquals("shipment2.Containers.TEUCount", 5.0m, shipment2.ContainerTEUCount);
			IApportionedCharge[] apportionedCharges = new IApportionedCharge[2];
			ApportionSplitCharge splitCharge = Factory.New<ApportionSplitCharge>();
			splitCharge.JR_JH = job.PK;
			splitCharge.SetShipmentInfo(shipment);
			splitCharge.JR_IsUsedForApportionment = true;
			ApportionSplitCharge splitCharge2 = Factory.New<ApportionSplitCharge>();
			splitCharge2.JR_JH = job2.PK;
			splitCharge2.SetShipmentInfo(shipment2);
			splitCharge2.JR_IsUsedForApportionment = true;
			apportionedCharges[0] = splitCharge;
			apportionedCharges[1] = splitCharge2;
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			MockIApportionedChargesHeader mock = new MockIApportionedChargesHeader();
			mock.Charges = apportionedCharges;
			mock.ChargeCode = chargeCode;
			mock.ApportionmentMethod = AllocationMethod.TwentyFootEquivalentUnit;
			mock.Currency = currency;
			mock.Apportion(JobChargeSchema.JR_OSCostAmt, 800m, JobChargeSchema.JR_AgentDeclaredCostAmt, 1600m);
			AssertEquals("splitCharge.JR_OSCostAmt", 300m, splitCharge.JR_OSCostAmt);
			AssertEquals("splitCharge2.JR_OSCostAmt", 500m, splitCharge2.JR_OSCostAmt);
			AssertEquals("splitCharge.JR_AgentDeclaredCostAmt", 600m, splitCharge.JR_AgentDeclaredCostAmt);
			AssertEquals("splitCharge2.JR_AgentDeclaredCostAmt", 1000m, splitCharge2.JR_AgentDeclaredCostAmt);
		}

		public void TestContainerApportionment()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GB = Env.CurrentBranch.PK;
			job.JH_GE = Env.CurrentDepartment.PK;
			job.JH_JobNum = "J0000100";
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job2 = new Job.Loader(Factory, shipment2).TryCreateWithoutMutexForTestOnly();
			job2.JH_GB = Env.CurrentBranch.PK;
			job2.JH_GE = Env.CurrentDepartment.PK;
			job2.JH_JobNum = "J0000200";
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			shipment.Consols.Add(consol);
			shipment2.Consols.Add(consol);
			RefContainer refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			RefContainer refContainer2 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40NOR");
			RefContainer refContainer3 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40PL");
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_RC = refContainer.PK;
			consol.Containers.Add(container);
			PackLine outerPack1 = shipment.OuterPackLines.AddNew();
			outerPack1.SetContainer(consol, container);
			container = Factory.New<CommonContainer>();
			container.JC_RC = refContainer2.PK;
			consol.Containers.Add(container);
			PackLine outerPack2 = shipment.OuterPackLines.AddNew();
			outerPack2.SetContainer(consol, container);
			AssertEquals("shipment.Containers.Count", 2, shipment.Containers.Count());
			container = Factory.New<CommonContainer>();
			container.JC_RC = refContainer.PK;
			consol.Containers.Add(container);
			outerPack1 = shipment2.OuterPackLines.AddNew();
			outerPack1.SetContainer(consol, container);
			container = Factory.New<CommonContainer>();
			container.JC_RC = refContainer2.PK;
			consol.Containers.Add(container);
			outerPack2 = shipment2.OuterPackLines.AddNew();
			outerPack2.SetContainer(consol, container);
			container = Factory.New<CommonContainer>();
			container.JC_RC = refContainer3.PK;
			consol.Containers.Add(container);
			PackLine outerPack3 = shipment2.OuterPackLines.AddNew();
			outerPack3.SetContainer(consol, container);
			AssertEquals("shipment2.Containers.Count", 3, shipment2.Containers.Count());
			IApportionedCharge[] apportionedCharges = new IApportionedCharge[2];
			ApportionSplitCharge splitCharge = Factory.New<ApportionSplitCharge>();
			splitCharge.JR_JH = job.PK;
			splitCharge.SetShipmentInfo(shipment);
			splitCharge.JR_IsUsedForApportionment = true;
			ApportionSplitCharge splitCharge2 = Factory.New<ApportionSplitCharge>();
			splitCharge2.JR_JH = job2.PK;
			splitCharge2.SetShipmentInfo(shipment2);
			splitCharge2.JR_IsUsedForApportionment = true;
			apportionedCharges[0] = splitCharge;
			apportionedCharges[1] = splitCharge2;
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			MockIApportionedChargesHeader mock = new MockIApportionedChargesHeader();
			mock.Charges = apportionedCharges;
			mock.ChargeCode = chargeCode;
			mock.ApportionmentMethod = AllocationMethod.ContainerCount;
			mock.Currency = currency;
			mock.Apportion(JobChargeSchema.JR_OSCostAmt, 800m, JobChargeSchema.JR_AgentDeclaredCostAmt, 1600m);
			AssertEquals("splitCharge.JR_OSCostAmt", 320m, splitCharge.JR_OSCostAmt);
			AssertEquals("splitCharge2.JR_OSCostAmt", 480m, splitCharge2.JR_OSCostAmt);
			AssertEquals("splitCharge.JR_AgentDeclaredCostAmt", 640m, splitCharge.JR_AgentDeclaredCostAmt);
			AssertEquals("splitCharge2.JR_AgentDeclaredCostAmt", 960m, splitCharge2.JR_AgentDeclaredCostAmt);
		}

		public void TestOuterPackTotalApportionment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container = Factory.NewWithValidTestData<CommonContainer>();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			var shipment = consol.Shipments.AddNew();
			shipment.OuterPackLines.RemoveAndDeleteAll();
			var palletPackLine = shipment.OuterPackLines.AddNew();
			palletPackLine.JL_PackageCount = 11;
			palletPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			var vehiclePackLine = shipment.OuterPackLines.AddNew();
			vehiclePackLine.JL_PackageCount = 1;
			vehiclePackLine.JL_F3_NKPackType = "VE";
			AssertEquals("Outer packs are totalled regardless of pack type", 12, shipment.TotalOuterPacks);
			var shipment2 = consol.Shipments.AddNew();
			shipment2.OuterPackLines.RemoveAndDeleteAll();
			var outerPackLine = shipment2.OuterPackLines.AddNew();
			outerPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			outerPackLine.JL_PackageCount = 6;
			var innerPackLine = shipment2.InnerPackLines.AddNew();
			innerPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Box;
			innerPackLine.JL_PackageCount = 9;
			AssertEquals("Only outer packs should be counted", 6, shipment2.TotalOuterPacks);
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentID = shipment.PK;
			var splitCharge = Factory.New<ApportionSplitCharge>();
			splitCharge.JR_JH = job.PK;
			splitCharge.SetShipmentInfo(shipment);
			splitCharge.JR_IsUsedForApportionment = true;
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_ParentID = shipment2.PK;
			var splitCharge2 = Factory.New<ApportionSplitCharge>();
			splitCharge2.JR_JH = job2.PK;
			splitCharge2.SetShipmentInfo(shipment2);
			splitCharge2.JR_IsUsedForApportionment = true;
			var objectCreator = new TestObjectCreator(Factory);
			var mock = new MockIApportionedChargesHeader { Charges = new IApportionedCharge[] { splitCharge, splitCharge2 }, ChargeCode = objectCreator.FRT, Currency = objectCreator.AUD, ApportionmentMethod = AllocationMethod.OuterPackTotal };
			mock.Apportion(JobChargeSchema.JR_LocalCostAmt, 600m, JobChargeSchema.JR_AgentDeclaredCostAmt, 720m);
			AssertEquals("Should apportion 2:1", 400m, splitCharge.JR_LocalCostAmt);
			AssertEquals(200m, splitCharge2.JR_LocalCostAmt);
			AssertEquals(480m, splitCharge.JR_AgentDeclaredCostAmt);
			AssertEquals(240m, splitCharge2.JR_AgentDeclaredCostAmt);
			palletPackLine.JL_PackageCount = 0;
			vehiclePackLine.JL_PackageCount = 0;
			outerPackLine.JL_PackageCount = 0;
			AssertEquals("Pre-condition", 0, shipment.TotalOuterPacks);
			AssertEquals("Pre-condition", 0, shipment2.TotalOuterPacks);
			AssertNoExceptionThrown("Thou shall not try to divide by zero", () => mock.Apportion(JobChargeSchema.JR_LocalCostAmt, 600m, JobChargeSchema.JR_AgentDeclaredCostAmt, 720m));
		}

		public void TestGrossVolumeApportionmentMethod()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ActualWeight = 500m;
			shipment1.JS_UnitOfWeight = "KG";
			shipment1.JS_ActualVolume = 1m;
			shipment1.JS_UnitOfVolume = "D3";
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_ParentTableCode = "JS";
			job1.JH_ParentID = shipment1.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_ActualWeight = 150m;
			shipment2.JS_UnitOfWeight = "T";
			shipment2.JS_ActualVolume = 2m;
			shipment2.JS_UnitOfVolume = "M3";
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_ParentTableCode = "JS";
			job2.JH_ParentID = shipment2.PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);
			Factory.Save();

			var apps = new ApportionmentListing(Factory, consol);
			var cost = apps.CostsCollection.TryAddNew();
			var objectCreator = new TestObjectCreator(Factory);
			cost.E6_AC_ChargeCode = objectCreator.FRT.PK;
			cost.E6_RX_NKCurrency = objectCreator.USD.RX_Code;
			cost.E6_ExchangeRate = 0.8M;
			cost.E6_OSCostAmount = 250m;
			cost.E6_LocalCostAmount = 312.50m;
			cost.E6_ApportionmentMethod = "GVT";

			AssertEquals("Should be one consol cost created", 1, apps.CostsCollection.Count);
			AssertEquals("Should be two Apportionment Charges", 2, apps.CostsCollection[0].ApportionmentCharges.Count);

			var splitCharges1 = apps.CostsCollection[0].ApportionmentCharges[0];
			AssertNotNull(splitCharges1);
			AssertEquals(splitCharges1.JR_LocalCostAmt, 0.15m);
			AssertEquals(splitCharges1.JR_OSCostAmt, 0.12m);

			var splitCharges2 = apps.CostsCollection[0].ApportionmentCharges[1];
			AssertNotNull(splitCharges2);
			AssertEquals(splitCharges2.JR_LocalCostAmt, 312.35m);
			AssertEquals(splitCharges2.JR_OSCostAmt, 249.88m);

			AssertEquals(cost.E6_LocalCostAmount, splitCharges1.JR_LocalCostAmt + splitCharges2.JR_LocalCostAmt);
			AssertEquals(cost.E6_OSCostAmount, splitCharges1.JR_OSCostAmt + splitCharges2.JR_OSCostAmt);

			cost.E6_RX_NKCurrency = objectCreator.AUD.RX_Code;
			cost.E6_OSCostAmount = 312.50;
			cost.E6_LocalCostAmount = 312.50m;
			cost.E6_ApportionmentMethod = "GVT";

			AssertEquals(splitCharges1.JR_LocalCostAmt, 0.16m);
			AssertEquals(splitCharges1.JR_OSCostAmt, 0.16m);
			AssertEquals(splitCharges2.JR_LocalCostAmt, 312.34m);
			AssertEquals(splitCharges2.JR_OSCostAmt, 312.34m);
			AssertEquals(cost.E6_LocalCostAmount, splitCharges1.JR_LocalCostAmt + splitCharges2.JR_LocalCostAmt);
			AssertEquals(cost.E6_OSCostAmount, splitCharges1.JR_OSCostAmt + splitCharges2.JR_OSCostAmt);
		}

		public void TestMoreChargesThanUnitsOfGST()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ActualWeight = 0.05M;

			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_ParentTableCode = "JS";
			job1.JH_ParentID = shipment1.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_ActualWeight = 0.05M;

			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_ParentTableCode = "JS";
			job2.JH_ParentID = shipment2.PK;

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_ActualWeight = 0.10M;

			var job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job3.JH_ParentTableCode = "JS";
			job3.JH_ParentID = shipment3.PK;

			var shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment4.JS_ActualWeight = 0.10M;

			var job4 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job4.JH_ParentTableCode = "JS";
			job4.JH_ParentID = shipment4.PK;

			var shipment5 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment5.JS_ActualWeight = 0.15M;

			var job5 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job5.JH_ParentTableCode = "JS";
			job5.JH_ParentID = shipment5.PK;

			var shipment6 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment6.JS_ActualWeight = 0.32M;

			var job6 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job6.JH_ParentTableCode = "JS";
			job6.JH_ParentID = shipment6.PK;

			var shipment7 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment7.JS_ActualWeight = 0.12M;

			var job7 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job7.JH_ParentTableCode = "JS";
			job7.JH_ParentID = shipment7.PK;

			var shipment8 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment8.JS_ActualWeight = 0.11M;

			var job8 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job8.JH_ParentTableCode = "JS";
			job8.JH_ParentID = shipment8.PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);
			consol.Shipments.Add(shipment3);
			consol.Shipments.Add(shipment4);
			consol.Shipments.Add(shipment5);
			consol.Shipments.Add(shipment6);
			consol.Shipments.Add(shipment7);
			consol.Shipments.Add(shipment8);
			Factory.Save();

			var apps = new ApportionmentListing(Factory, consol);
			var cost = apps.CostsCollection.TryAddNew();

			cost.E6_ApportionmentMethod = "MAN";

			var objectCreator = new TestObjectCreator(Factory);
			cost.E6_AC_ChargeCode = objectCreator.FRT.PK;
			cost.E6_RX_NKCurrency = objectCreator.USD.RX_Code;
			cost.E6_ExchangeRate = 65.67M;

			cost.E6_OSCostAmount = 1m;
			cost.E6_IsTaxAmountOverridden = true;
			cost.E6_OSGSTAmount_Calc = 0.05m;
			cost.ApportionmentCharges[0].JR_OSCostAmt = 0.05m;
			cost.ApportionmentCharges[1].JR_OSCostAmt = 0.05m;
			cost.ApportionmentCharges[2].JR_OSCostAmt = 0.10m;
			cost.ApportionmentCharges[3].JR_OSCostAmt = 0.10m;
			cost.ApportionmentCharges[4].JR_OSCostAmt = 0.15m;
			cost.ApportionmentCharges[5].JR_OSCostAmt = 0.32m;
			cost.ApportionmentCharges[6].JR_OSCostAmt = 0.12m;
			cost.ApportionmentCharges[7].JR_OSCostAmt = 0.11m;

			cost.E6_ApportionmentMethod = "MAN";

			AssertEquals("Should be one consol cost created", 1, apps.CostsCollection.Count);
			var consolCost = apps.CostsCollection[0];
			AssertEquals("Should be eight Apportionment Charges", 8, consolCost.ApportionmentCharges.Count);
			var charges = consolCost.ApportionmentCharges;

			AssertEquals("Should have a total of $1 cost amount", 1m, charges.JR_OSCostAmtSum);

			var expectedCosts = new ZDecimal[] { 0.05M, 0.05M, 0.10M, 0.10M, 0.15M, 0.32M, 0.12M, 0.11M };

			var actualCosts = from BaseCharge charge in charges select charge.JR_OSCostAmt;

			AssertArrayEqualsByElements("Should have the same distribution of cost as initially entered", expectedCosts, actualCosts.ToArray());

			AssertEquals("Should have a total of $0.05 GST", 0.05m, charges.JR_OSCostGSTAmtSum);

			var expectedTax = new ZDecimal[] { 0m, 0m, 0m, 0m, 0.01m, 0.02m, 0.01m, 0.01m };
			var actualTax = from BaseCharge charge in charges select charge.JR_OSCostGSTAmt_Calc;

			AssertArrayEqualsByElements("Should have the 32c with 2c of tax, the 15c, 12c and 11c charges with 1c of tax, all else with 0", expectedTax, actualTax.ToArray());
		}

		public void TestMixedPositiveAndNegativeValuesForGSTApportionment()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ActualWeight = 0.05M;

			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_ParentTableCode = "JS";
			job1.JH_ParentID = shipment1.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_ActualWeight = 0.20M;

			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_ParentTableCode = "JS";
			job2.JH_ParentID = shipment2.PK;

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_ActualWeight = 0.15M;

			var job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job3.JH_ParentTableCode = "JS";
			job3.JH_ParentID = shipment3.PK;

			var shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment4.JS_ActualWeight = -0.10M;

			var job4 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job4.JH_ParentTableCode = "JS";
			job4.JH_ParentID = shipment4.PK;

			var shipment5 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment5.JS_ActualWeight = 0.15M;

			var job5 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job5.JH_ParentTableCode = "JS";
			job5.JH_ParentID = shipment5.PK;

			var shipment6 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment6.JS_ActualWeight = 0.32M;

			var job6 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job6.JH_ParentTableCode = "JS";
			job6.JH_ParentID = shipment6.PK;

			var shipment7 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment7.JS_ActualWeight = 0.12M;

			var job7 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job7.JH_ParentTableCode = "JS";
			job7.JH_ParentID = shipment7.PK;

			var shipment8 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment8.JS_ActualWeight = 0.11M;

			var job8 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job8.JH_ParentTableCode = "JS";
			job8.JH_ParentID = shipment8.PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);
			consol.Shipments.Add(shipment3);
			consol.Shipments.Add(shipment4);
			consol.Shipments.Add(shipment5);
			consol.Shipments.Add(shipment6);
			consol.Shipments.Add(shipment7);
			consol.Shipments.Add(shipment8);
			Factory.Save();

			var apps = new ApportionmentListing(Factory, consol);
			var cost = apps.CostsCollection.TryAddNew();
			var objectCreator = new TestObjectCreator(Factory);
			cost.E6_AC_ChargeCode = objectCreator.FRT.PK;
			cost.E6_RX_NKCurrency = objectCreator.USD.RX_Code;
			cost.E6_ExchangeRate = 65.67M;
			cost.E6_OSCostAmount = 1m;
			cost.E6_LocalCostAmount = 65.67m;
			cost.E6_IsTaxAmountOverridden = true;
			cost.E6_OSGSTAmount_Calc = 0.05m;
			cost.E6_ApportionmentMethod = "GWT";

			AssertEquals("Should be one consol cost created", 1, apps.CostsCollection.Count);
			var consolCost = apps.CostsCollection[0];
			AssertEquals("Should be eight Apportionment Charges", 8, consolCost.ApportionmentCharges.Count);
			var charges = consolCost.ApportionmentCharges;

			AssertEquals("Should have a total of $1 cost amount", 1m, charges.JR_OSCostAmtSum);
			AssertEquals("Should have a total of $0.05 GST", 0.05m, charges.JR_OSCostGSTAmtSum);
		}

		public void TestPositiveUnapportionedAmounts()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_ParentTableCode = "JS";
			job1.JH_ParentID = shipment1.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_ParentTableCode = "JS";
			job2.JH_ParentID = shipment2.PK;

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			var job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job3.JH_ParentTableCode = "JS";
			job3.JH_ParentID = shipment3.PK;

			var shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			var job4 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job4.JH_ParentTableCode = "JS";
			job4.JH_ParentID = shipment4.PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);
			consol.Shipments.Add(shipment3);
			consol.Shipments.Add(shipment4);
			Factory.Save();

			var apps = new ApportionmentListing(Factory, consol);
			var cost = apps.CostsCollection.TryAddNew();

			cost.E6_ApportionmentMethod = "MAN";

			var objectCreator = new TestObjectCreator(Factory);
			cost.E6_AC_ChargeCode = objectCreator.FRT.PK;
			cost.E6_RX_NKCurrency = objectCreator.USD.RX_Code;
			cost.E6_ExchangeRate = 65.67M;

			cost.E6_OSCostAmount = 1m;
			cost.E6_IsTaxAmountOverridden = true;
			cost.E6_OSGSTAmount_Calc = 0.02m;
			cost.ApportionmentCharges[0].JR_OSCostAmt = 0.4m;
			cost.ApportionmentCharges[1].JR_OSCostAmt = 0.2m;
			cost.ApportionmentCharges[2].JR_OSCostAmt = 0.2m;
			cost.ApportionmentCharges[3].JR_OSCostAmt = 0.2m;

			cost.E6_ApportionmentMethod = "MAN";

			AssertEquals("Should be one consol cost created", 1, apps.CostsCollection.Count);
			var consolCost = apps.CostsCollection[0];
			AssertEquals("Should be four Apportionment Charges", 4, consolCost.ApportionmentCharges.Count);
			var charges = consolCost.ApportionmentCharges;

			AssertEquals("Should have a total of $1 cost amount", 1m, charges.JR_OSCostAmtSum);

			var expectedCosts = new ZDecimal[] { 0.4m, 0.2m, 0.2m, 0.2m };

			var actualCosts = from BaseCharge charge in charges select charge.JR_OSCostAmt;

			AssertArrayEqualsByElements("Should have the same distribution of cost as initially entered", expectedCosts, actualCosts.ToArray());

			AssertEquals("Should have a total of $0.02 GST", 0.02m, charges.JR_OSCostGSTAmtSum);

			var expectedTax = new ZDecimal[] { 0.01m, 0.01m, 0m, 0m };
			var actualTax = from BaseCharge charge in charges select charge.JR_OSCostGSTAmt_Calc;

			AssertArrayEqualsByElements("Should have the 40c and first 20c with 1c of tax, all others with 0c of tax", expectedTax, actualTax.ToArray());
		}

		public void TestNegativeUnapportionedAmounts()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_ParentTableCode = "JS";
			job1.JH_ParentID = shipment1.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_ParentTableCode = "JS";
			job2.JH_ParentID = shipment2.PK;

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			var job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job3.JH_ParentTableCode = "JS";
			job3.JH_ParentID = shipment3.PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);
			consol.Shipments.Add(shipment3);
			Factory.Save();

			var apps = new ApportionmentListing(Factory, consol);
			var cost = apps.CostsCollection.TryAddNew();

			cost.E6_ApportionmentMethod = "MAN";

			var objectCreator = new TestObjectCreator(Factory);
			cost.E6_AC_ChargeCode = objectCreator.FRT.PK;
			cost.E6_RX_NKCurrency = objectCreator.USD.RX_Code;
			cost.E6_ExchangeRate = 65.67M;

			cost.E6_OSCostAmount = 1m;
			cost.E6_IsTaxAmountOverridden = true;
			cost.E6_OSGSTAmount_Calc = 0.02m;
			cost.ApportionmentCharges[0].JR_OSCostAmt = 0.4m;
			cost.ApportionmentCharges[1].JR_OSCostAmt = 0.3m;
			cost.ApportionmentCharges[2].JR_OSCostAmt = 0.3m;

			cost.E6_ApportionmentMethod = "MAN";

			AssertEquals("Should be one consol cost created", 1, apps.CostsCollection.Count);
			var consolCost = apps.CostsCollection[0];
			AssertEquals("Should be three Apportionment Charges", 3, consolCost.ApportionmentCharges.Count);
			var charges = consolCost.ApportionmentCharges;

			AssertEquals("Should have a total of $1 cost amount", 1m, charges.JR_OSCostAmtSum);

			var expectedCosts = new ZDecimal[] { 0.4m, 0.3m, 0.3m };

			var actualCosts = from BaseCharge charge in charges select charge.JR_OSCostAmt;

			AssertArrayEqualsByElements("Should have the same distribution of cost as initially entered", expectedCosts, actualCosts.ToArray());

			AssertEquals("Should have a total of $0.02 GST", 0.02m, charges.JR_OSCostGSTAmtSum);

			var expectedTax = new ZDecimal[] { 0.01m, 0m, 0.01m };
			var actualTax = from BaseCharge charge in charges select charge.JR_OSCostGSTAmt_Calc;

			AssertArrayEqualsByElements("Should have the 40c and second 30c with 1c of tax, the first 30c with 0c of tax", expectedTax, actualTax.ToArray());
		}

		public void TestValuesToApportionByAllZero()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ActualWeight = 0M;

			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_ParentTableCode = "JS";
			job1.JH_ParentID = shipment1.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_ActualWeight = 0M;

			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_ParentTableCode = "JS";
			job2.JH_ParentID = shipment2.PK;

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_ActualWeight = 0M;

			var job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job3.JH_ParentTableCode = "JS";
			job3.JH_ParentID = shipment3.PK;

			var shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment4.JS_ActualWeight = 0M;

			var job4 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job4.JH_ParentTableCode = "JS";
			job4.JH_ParentID = shipment4.PK;

			var shipment5 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment5.JS_ActualWeight = 0M;

			var job5 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job5.JH_ParentTableCode = "JS";
			job5.JH_ParentID = shipment5.PK;

			var shipment6 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment6.JS_ActualWeight = 0M;

			var job6 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job6.JH_ParentTableCode = "JS";
			job6.JH_ParentID = shipment6.PK;

			var shipment7 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment7.JS_ActualWeight = 0M;

			var job7 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job7.JH_ParentTableCode = "JS";
			job7.JH_ParentID = shipment7.PK;

			var shipment8 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment8.JS_ActualWeight = 0M;

			var job8 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job8.JH_ParentTableCode = "JS";
			job8.JH_ParentID = shipment8.PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);
			consol.Shipments.Add(shipment3);
			consol.Shipments.Add(shipment4);
			consol.Shipments.Add(shipment5);
			consol.Shipments.Add(shipment6);
			consol.Shipments.Add(shipment7);
			consol.Shipments.Add(shipment8);
			Factory.Save();

			var apps = new ApportionmentListing(Factory, consol);
			var cost = apps.CostsCollection.TryAddNew();
			var objectCreator = new TestObjectCreator(Factory);
			cost.E6_AC_ChargeCode = objectCreator.FRT.PK;
			cost.E6_RX_NKCurrency = objectCreator.USD.RX_Code;
			cost.E6_ExchangeRate = 65.67M;
			cost.E6_IsTaxAmountOverridden = true;
			cost.E6_OSCostAmount = 1m;
			cost.E6_LocalCostAmount = 65.67m;
			cost.E6_OSGSTAmount_Calc = 0.16m;
			cost.E6_ApportionmentMethod = "GWT";

			AssertEquals("Should be one consol cost created", 1, apps.CostsCollection.Count);
			var consolCost = apps.CostsCollection[0];
			AssertEquals("Should be eight Apportionment Charges", 8, consolCost.ApportionmentCharges.Count);
			var charges = consolCost.ApportionmentCharges;

			AssertEquals("Should have a total of $1 cost amount", 1m, charges.JR_OSCostAmtSum);
			AssertEquals("Should have a total of $0.16 GST", 0.16m, charges.JR_OSCostGSTAmtSum);
		}

		public void TestApportionmentValuesAddUpToZero()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ActualWeight = 1M;

			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_ParentTableCode = "JS";
			job1.JH_ParentID = shipment1.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_ActualWeight = -1M;

			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_ParentTableCode = "JS";
			job2.JH_ParentID = shipment2.PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);
			Factory.Save();

			var apps = new ApportionmentListing(Factory, consol);
			var cost = apps.CostsCollection.TryAddNew();
			var objectCreator = new TestObjectCreator(Factory);
			cost.E6_AC_ChargeCode = objectCreator.FRT.PK;
			cost.E6_RX_NKCurrency = objectCreator.USD.RX_Code;
			cost.E6_ExchangeRate = 65.67M;
			cost.E6_OSCostAmount = 1m;
			cost.E6_LocalCostAmount = 65.67m;
			cost.E6_IsTaxAmountOverridden = true;
			cost.E6_OSGSTAmount_Calc = 0.01m;
			cost.E6_ApportionmentMethod = "GWT";

			AssertEquals("Should be one consol cost created", 1, apps.CostsCollection.Count);
			var consolCost = apps.CostsCollection[0];
			AssertEquals("Should be two Apportionment Charges", 2, consolCost.ApportionmentCharges.Count);
			var charges = consolCost.ApportionmentCharges;

			AssertEquals("Should have a total of $1 cost amount", 1m, charges.JR_OSCostAmtSum);
			AssertEquals("Should have a total of $0.01 GST", 0.01m, charges.JR_OSCostGSTAmtSum);
		}

		public void TestValuesToApportionByAllZeroWithMoreChargesThanUnitsOfGST()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ActualWeight = 0M;

			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_ParentTableCode = "JS";
			job1.JH_ParentID = shipment1.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_ActualWeight = 0M;

			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_ParentTableCode = "JS";
			job2.JH_ParentID = shipment2.PK;

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_ActualWeight = 0M;

			var job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job3.JH_ParentTableCode = "JS";
			job3.JH_ParentID = shipment3.PK;

			var shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment4.JS_ActualWeight = 0M;

			var job4 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job4.JH_ParentTableCode = "JS";
			job4.JH_ParentID = shipment4.PK;

			var shipment5 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment5.JS_ActualWeight = 0M;

			var job5 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job5.JH_ParentTableCode = "JS";
			job5.JH_ParentID = shipment5.PK;

			var shipment6 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment6.JS_ActualWeight = 0M;

			var job6 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job6.JH_ParentTableCode = "JS";
			job6.JH_ParentID = shipment6.PK;

			var shipment7 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment7.JS_ActualWeight = 0M;

			var job7 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job7.JH_ParentTableCode = "JS";
			job7.JH_ParentID = shipment7.PK;

			var shipment8 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment8.JS_ActualWeight = 0M;

			var job8 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job8.JH_ParentTableCode = "JS";
			job8.JH_ParentID = shipment8.PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);
			consol.Shipments.Add(shipment3);
			consol.Shipments.Add(shipment4);
			consol.Shipments.Add(shipment5);
			consol.Shipments.Add(shipment6);
			consol.Shipments.Add(shipment7);
			consol.Shipments.Add(shipment8);
			Factory.Save();

			var apps = new ApportionmentListing(Factory, consol);
			var cost = apps.CostsCollection.TryAddNew();
			var objectCreator = new TestObjectCreator(Factory);
			cost.E6_AC_ChargeCode = objectCreator.FRT.PK;
			cost.E6_RX_NKCurrency = objectCreator.USD.RX_Code;
			cost.E6_ExchangeRate = 65.67M;
			cost.E6_OSCostAmount = 1m;
			cost.E6_LocalCostAmount = 65.67m;
			cost.E6_IsTaxAmountOverridden = true;
			cost.E6_OSGSTAmount_Calc = 0.05m;
			cost.E6_ApportionmentMethod = "GWT";

			AssertEquals("Should be one consol cost created", 1, apps.CostsCollection.Count);
			var consolCost = apps.CostsCollection[0];
			AssertEquals("Should be eight Apportionment Charges", 8, consolCost.ApportionmentCharges.Count);
			var charges = consolCost.ApportionmentCharges;

			AssertEquals("Should have a total of $1 cost amount", 1m, charges.JR_OSCostAmtSum);
			AssertEquals("Should have a total of $0.05 GST", 0.05m, charges.JR_OSCostGSTAmtSum);
		}

		public void TestUnApportionedAmount_SingleShipment()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("SHP001", consol);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1, 456M, true);
			Factory.Save();

			AssertEquals(456M, consolCost.ApportionmentCharges[0].JR_OSCostAmt);
			AssertEquals(0M, consolCost.UnApportionedAmount);

			consolCost.ApportionmentCharges[0].JR_IsUsedForApportionment = false;

			AssertEquals(456M, consolCost.UnApportionedAmount);
			AssertEquals(0M, consolCost.ApportionmentCharges[0].JR_OSCostAmt);
		}

		public void TestUnApportionedAmount_MultipleShipmentsOneUnticked()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("SHP001", consol);
			var shipment2 = testObjectCreator.CreateShipment("SHP002", consol);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1, 456M, true);
			Factory.Save();

			AssertEquals(228M, consolCost.ApportionmentCharges[0].JR_OSCostAmt);
			AssertEquals(228M, consolCost.ApportionmentCharges[1].JR_OSCostAmt);
			AssertEquals(0M, consolCost.UnApportionedAmount);

			consolCost.ApportionmentCharges[1].JR_IsUsedForApportionment = false;

			AssertEquals(0M, consolCost.UnApportionedAmount);
			AssertEquals(456M, consolCost.ApportionmentCharges[0].JR_OSCostAmt);
			AssertEquals(0M, consolCost.ApportionmentCharges[1].JR_OSCostAmt);
		}

		public void TestUnApportionedAmount_MultipleShipmentsAllUnticked()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("SHP001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("SHP002", consol);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1, 456M, true);
			Factory.Save();

			AssertEquals(228M, consolCost.ApportionmentCharges[0].JR_OSCostAmt);
			AssertEquals(228M, consolCost.ApportionmentCharges[1].JR_OSCostAmt);
			AssertEquals(0M, consolCost.UnApportionedAmount);

			consolCost.ApportionmentCharges[0].JR_IsUsedForApportionment = false;
			consolCost.ApportionmentCharges[1].JR_IsUsedForApportionment = false;

			AssertEquals(456M, consolCost.UnApportionedAmount);
			AssertEquals(0M, consolCost.ApportionmentCharges[0].JR_OSCostAmt);
			AssertEquals(0M, consolCost.ApportionmentCharges[1].JR_OSCostAmt);
		}

		class MockIApportionedChargesHeader : IApportionedChargesHeader
		{
			public IApportionedCharge[] Charges
			{
				get;
				set;
			}

			public AccChargeCode ChargeCode
			{
				get;
				set;
			}

			public ZString ApportionmentMethod
			{
				get;
				set;
			}

			public RefCurrency Currency
			{
				get;
				set;
			}

			public ZDecimal FreeSpace
			{
				get;
				set;
			}

			public bool IsChargeReadyToPost(IApportionedCharge charge)
			{
				return true;
			}
		}

		class MockIApportionedCharge : DummyBusinessObject, IApportionedCharge
		{
			public MockIApportionedCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public Job InvoicingJob
			{
				get;
				set;
			}

			public ZDecimal ChargeableUnits
			{
				get;
				set;
			}

			public ZDecimal GrossWeight
			{
				get;
				set;
			}

			public ZDecimal GrossVolume
			{
				get;
				set;
			}

			public ZBool IsUsedForApportionment
			{
				get;
				set;
			}

			public ZInt ContainerCount
			{
				get;
				set;
			}

			public ZDecimal TEUCount
			{
				get;
				set;
			}

			public ZInt OuterPackTotal
			{
				get;
				set;
			}

			public ZString CurrencyCode
			{
				get;
				set;
			}

			public ZDecimal GetContainersCostShare() => 0;
			public ZDecimal ExcessActualVolumeWeight
			{
				get;
				set;
			}

			public ZDecimal ExcessChargeableVolumeWeight
			{
				get;
				set;
			}
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
