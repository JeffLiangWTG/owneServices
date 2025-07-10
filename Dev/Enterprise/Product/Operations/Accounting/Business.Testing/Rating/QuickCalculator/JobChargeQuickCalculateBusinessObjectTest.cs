using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Test;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.CustomsReferenceNumberType;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(JobChargeQuickCalculateBusinessObject))]
	public class JobChargeQuickCalculateBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		#region Chargeable - Transport Booking

		public void TestChargeable_GivenTransportBooking_ThenShouldUseTransportBookingChargeableFactor()
		{
			AssertEquals("Precondition: transport Booking Registry (3000 CC/KG = 333.3333 KG/M3)", "3000 CC/KG", TransportBookingChargeableFactor.MetricFactor.ToString());

			using (FreightDataRegistry.Instance.InternationalChargeableFactorRoad.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(150, Weight.Kilograms, Volume.CubicMetres),
				new ConversionFactor(100, Weight.Pounds, Volume.CubicInches))))
			{
				var booking = CreateBooking
				(
					Factory,
					Factory.NewWithValidTestData<OrgHeader>().PK,
					Factory.NewWithValidTestData<OrgHeader>().PK,
					Factory.NewWithValidTestData<OrgHeader>().PK,
					packageUnit: PkgUnit.Box,
					packageWeight: 2m,
					packageWeightUQ: QuantityUnit.KG,
					packageVolume: 3m,
					packageVolumeUQ: Volume.CubicMetres
				);

				var ratingProvider = (IRatingSupporter)booking;
				Charge.JR_AC = Env.Registry.FreightChargeCode;
				var quickCalculator = new JobChargeQuickCalculateBusinessObject(ratingProvider.AdaptersProvider.GetForQuickCalculate(null), Charge);
				quickCalculator.QuantityDescription = nameof(MeasureType.Chargeable);
				AssertEquals("Chargeable should use transport booking registry convertion factor: 3 M3 x 333.3333 KG/M3", 1000m, quickCalculator.Quantity);
			}
		}

		public void TestChargeable_GivenTransportBookingWithShipment()
		{
			AssertEquals("Precondition: transport Booking Registry (3000 CC/KG = 333.3333 KG/M3)", "3000 CC/KG", TransportBookingChargeableFactor.MetricFactor.ToString());

			using (FreightDataRegistry.Instance.InternationalChargeableFactorRoad.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(150, Weight.Kilograms, Volume.CubicMetres),
				new ConversionFactor(100, Weight.Pounds, Volume.CubicInches))))
			{
				var testObjectCreator = new TestObjectCreator(Factory);
				var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001");
				consol.JK_TransportMode = Core.Constants.TransportModes.Road;
				var shipment = consol.Shipments.AddNew();
				shipment.JS_ActualWeight = 4m;
				shipment.JS_ActualVolume = 5m;
				shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
				shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;

				var booking = CreateBooking
				(
					Factory,
					Factory.NewWithValidTestData<OrgHeader>().PK,
					Factory.NewWithValidTestData<OrgHeader>().PK,
					Factory.NewWithValidTestData<OrgHeader>().PK,
					packageUnit: PkgUnit.Box,
					packageWeight: 2m,
					packageWeightUQ: QuantityUnit.KG,
					packageVolume: 3m,
					packageVolumeUQ: Volume.CubicMetres,
					shipmentPK: shipment.PK
				);

				var ratingProvider = (IRatingSupporter)booking;
				Charge.JR_AC = Env.Registry.FreightChargeCode;
				var quickCalculator = new JobChargeQuickCalculateBusinessObject(ratingProvider.AdaptersProvider.GetForQuickCalculate(null), Charge);
				quickCalculator.QuantityDescription = nameof(MeasureType.Chargeable);
				AssertEquals("Booking: Chargeable should use transport booking registry convertion factor: 3 M3 x 333.3333 KG/M3", 1000m, quickCalculator.Quantity);

				ratingProvider = shipment;
				Charge.JR_AC = Env.Registry.FreightChargeCode;
				quickCalculator = new JobChargeQuickCalculateBusinessObject(ratingProvider.AdaptersProvider.GetForQuickCalculate(null), Charge);
				quickCalculator.QuantityDescription = nameof(MeasureType.Chargeable);
				AssertEquals("Shipment: Chargeable should use freight registry convertion factor: 5 M3 x 150 KG/M3", 750m, quickCalculator.Quantity);
			}
		}

		ChargeableFactor TransportBookingChargeableFactor
		{
			get
			{
				var transportRegistry = ObjectFactory.Get<ITransportBookingRegistryProvider>();
				var chargeableFactorRegistryItem = (ChargeableFactorRegistryItem)transportRegistry.TransportBookingChargeableFactor;
				return chargeableFactorRegistryItem.Value;
			}
		}

		static DtbBooking CreateBooking(BusinessObjectFactory factory, ZGuid bookingAddressOrganizationPK, ZGuid consignorPK, ZGuid consigneePK, string packageUnit, decimal packageWeight, string packageWeightUQ, decimal packageVolume = 0m, string packageVolumeUQ = "M3", ZGuid? shipmentPK = null)
		{
			var consol = factory.New<DtbBookingConsolidation>();

			if (shipmentPK != null)
			{
				consol.KB_ParentID = shipmentPK.Value;
				consol.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			}

			var booking = consol.Bookings.AddNew();
			booking.Address.OrganisationPK = bookingAddressOrganizationPK;
			booking.KM_RatingFreightMode = "LSE";

			var fromInstruction = booking.Instructions.AddNew();
			fromInstruction.KN_InstructionType = "PIC";
			fromInstruction.Address.OrganisationPK = consignorPK;

			var toInstruction = booking.Instructions.AddNew();
			toInstruction.KN_InstructionType = "DLV";
			toInstruction.Address.OrganisationPK = consigneePK;

			fromInstruction.KN_IsLooseRateable = true;
			toInstruction.KN_IsLooseRateable = true;

			var commodity = factory.New<RefCommodityCode>();
			commodity.RH_Code = "AAA";

			var package = CreatePackage(booking, 1, packageUnit, commodity, packageWeight, packageWeightUQ, packageVolume, packageVolumeUQ);
			CreateInstructionPkgDivots(fromInstruction, package);
			CreateInstructionPkgDivots(toInstruction, package);

			return booking;
		}

		static PkgPackage CreatePackage(DtbBooking booking, int quantity, string quantityUQ, RefCommodityCode commodity, decimal weight, string weightUQ, decimal volume, string volumeUQ)
		{
			var package = booking.AssignedPackages.AddNew();
			package.KP_PackageQty = quantity;
			package.KP_F3_NKPackType = quantityUQ;
			package.KP_Weight = weight;
			package.KP_WeightUQ = weightUQ;
			package.KP_Volume = volume;
			package.KP_VolumeUQ = volumeUQ;
			package.KP_RH_NKCommodityCode = commodity != null
				? commodity.RH_Code
				: ZString.Empty;
			package.KP_KJ_ParentPackageJob = booking.ConsolidationSingleJob.PackageJob.PK;

			return package;
		}

		static void CreateInstructionPkgDivots(DtbBookingInstruction instruction, params PkgPackage[] packages)
		{
			foreach (var package in packages)
			{
				var divot = instruction.PackageDivots.AddNew();
				divot.KD_KP_Package = package.PK;
				divot.KD_Quantity = package.KP_PackageQty;
			}
		}

		#endregion

		#region Default Values

		public void TestDefaultValues_NothingPosted()
		{
			JobChargeQuickCalculateBusinessObject quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), Charge);
			AssertEquals(true, quickCalculator.UpdateSell);
			AssertEquals(true, quickCalculator.UpdateCost);
			AssertEquals(false, quickCalculator.IsMinimum);
		}

		public void TestDefaultValues_RevenuePosted()
		{
			AccTransactionLines line = Factory.New<AccTransactionLines>();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			Charge.JR_AL_ARLine = line.PK;
			AssertEquals(true, Charge.JR_IsRevenuePosted);

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), Charge);
			AssertEquals(false, quickCalculator.UpdateSell);
			AssertEquals(true, quickCalculator.UpdateCost);

			AssertNoErrors(quickCalculator.UpdateSellInfo);
			AssertNoErrors(quickCalculator.UpdateCostInfo);

			quickCalculator.UpdateSell = true;
			quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), Charge);
			AssertHasWarnings(quickCalculator.UpdateSellInfo);
			Assert(quickCalculator.UpdateSellInfo.ReadOnly);
		}

		public void TestDefaultValues_CostPosted()
		{
			AccTransactionLines line = Factory.New<AccTransactionLines>();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			Charge.JR_AL_APLine = line.PK;
			AssertEquals(true, Charge.JR_IsCostPosted);

			JobChargeQuickCalculateBusinessObject quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), Charge);
			AssertEquals(true, quickCalculator.UpdateSell);
			AssertEquals(false, quickCalculator.UpdateCost);

			AssertNoErrors(quickCalculator.UpdateSellInfo);
			AssertNoErrors(quickCalculator.UpdateCostInfo);

			quickCalculator.UpdateCost = true;
			quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), Charge);
			AssertHasWarnings(quickCalculator.UpdateCostInfo);
			Assert(quickCalculator.UpdateCostInfo.ReadOnly);
		}

		public void TestDefaultValues_ApportionedNonDisbursementCharge()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsCreditor = true;

			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001");
			var shipment = consol.Shipments.AddNew();
			var testJob = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var consolCost = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC2, org);
			AssertEquals(1, consolCost.ApportionmentCharges.Count);
			var apportionedCharge = Factory.Load<Charge>(consolCost.ApportionmentCharges[0].PK);
			Assert(apportionedCharge.JR_IsApportioned);
			apportionedCharge.JR_ChargeType = Core.Constants.ChargeType.Margin;

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), apportionedCharge);
			Assert("UpdateSell should be ticked by default", quickCalculator.UpdateSell);
		}

		public void TestDefaultValues_PopulateInformationForSpotBehaviour()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsCreditor = true;

			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001");
			consol.JK_OverrideConsolChargeable = true;
			consol.JK_CorrectedConsolWeight = 1000M;
			var consolCost = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC2, org);
			consolCost.E6_RatingBehaviour = RatingBehaviours.AllInAdhocOverridingSpotRate;

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(consol.RatingAdapter, consolCost);

			quickCalculator.QuantityDescription = "Job Weight";
			quickCalculator.IsMinimum = true;
			quickCalculator.CostMinimum = 100M;
			quickCalculator.CostRate = 2.2M;
			quickCalculator.SetCalculationResults();

			var secondCalculator = new JobChargeQuickCalculateBusinessObject(consol.RatingAdapter, consolCost);

			AssertEquals("Should be able to store Measurement Basis after calculation, and populate it back", "Job Weight", secondCalculator.QuantityDescription);
			AssertEquals("Should be able to store Is Minimum after calculation, and populate it back", true, secondCalculator.IsMinimum);
			AssertEquals("Should be able to store Minimum Value after calculation, and populate it back", 100M, secondCalculator.CostMinimum);
			AssertEquals("Should be able to store Rate Per Unit after calculation, and populate it back", 2.2M, secondCalculator.CostRate);
		}

		#endregion

		#region Calculation

		#region Local/Non-local Debtor

		public void TestSetCalculationResults_RegistryDisable_NonLocalDebtor()
			=> AssertSetCalculationResults(isRegistryEnabled: false, isLocalDebtor: false, expectedChargeDescription: "Description CC1 - 10 LB @ INR 3.00/LB");

		public void TestSetCalculationResults_RegistryDisable_LocalDebtor()
			=> AssertSetCalculationResults(isRegistryEnabled: false, isLocalDebtor: true, expectedChargeDescription: "Description CC1 - 10 LB @ INR 3.00/LB");

		public void TestSetCalculationResults_RegistryEnable_NonLocalDebtor()
			=> AssertSetCalculationResults(isRegistryEnabled: true, isLocalDebtor: false, expectedChargeDescription: "Description CC1 - 10 LB @ INR 3.00/LB");

		public void TestSetCalculationResults_RegistryEnable_NonLocalDebtor_EmptyChargeCodeLocalDescription()
			=> AssertSetCalculationResults(isRegistryEnabled: true, isLocalDebtor: false, isEmptyChargeCodeLocalDescription: true, expectedChargeDescription: "Description CC1 - 10 LB @ INR 3.00/LB");

		public void TestSetCalculationResults_RegistryEnable_LocalDebtor()
			=> AssertSetCalculationResults(isRegistryEnabled: true, isLocalDebtor: true, expectedChargeDescription: "Local Description CC1 - 10 LB @ INR 3.00/LB");

		public void TestSetCalculationResults_RegistryEnable_LocalDebtor_EmptyChargeCodeLocalDescription()
			=> AssertSetCalculationResults(isRegistryEnabled: true, isLocalDebtor: true, isEmptyChargeCodeLocalDescription: true, expectedChargeDescription: "Description CC1 - 10 LB @ INR 3.00/LB");

		void AssertSetCalculationResults(bool isRegistryEnabled, bool isLocalDebtor, string expectedChargeDescription, bool isEmptyChargeCodeLocalDescription = false)
		{
			var newFactory = new BusinessObjectFactory();
			var testDebtor = newFactory.NewWithValidTestData<OrgHeader>();
			if (isLocalDebtor)
			{
				testDebtor.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				AssertEquals("Local Debtor", true, testDebtor.IsLocalCountry);
			}
			else
			{
				AssertEquals("NonLocal Debtor", false, testDebtor.IsLocalCountry);
			}

			testDebtor.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			var localClientGroup = testDebtor.CompanyData.InvoiceRollupOrGroups.AddNew();
			localClientGroup.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			localClientGroup.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.All;

			AssertNotNull("Create CompanyData before saving", testDebtor.CompanyData);

			var testObjectCreator = new TestObjectCreator(newFactory);
			var chargeCode1 = testObjectCreator.CreateChargeCode("CC1");
			chargeCode1.AC_Desc = "Description CC1";
			chargeCode1.AC_LocalLanguageDescription = isEmptyChargeCodeLocalDescription ? string.Empty : "Local Description CC1";

			newFactory.Save();

			var job = Factory.NewJobForTesting<Job>();
			var charge = job.Charges.AddNew();
			charge.JR_AC = chargeCode1.PK;
			charge.JR_ChargeType = "MRG";
			charge.JR_MarginPercentage = 0m;
			charge.JR_OH_SellAccount = testDebtor.PK;
			charge.JR_RX_NKCostCurrency = "INR";
			charge.JR_RX_NKSellCurrency = "INR";

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge)
			{
				QuantityDescription = "Weight",

				UpdateSell = true,
				UpdateCost = false,
				SellRate = 3m,
				CostRate = 6m
			};

			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isRegistryEnabled))
			{
				quickCalculator.SetCalculationResults();
				AssertEquals("Charge Description", expectedChargeDescription, charge.JR_Desc);
			}
		}

		#endregion

		public void TestStoreResults()
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			OrgHeader testDebtor = orgFactory.NewWithValidTestData<OrgHeader>();
			AssertNotNull("Create CompanyData before saving", testDebtor.CompanyData);
			orgFactory.Save();

			Job job = Factory.NewJobForTesting<Job>();
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_ChargeType = "MRG";
			charge.JR_MarginPercentage = 0m;
			charge.JR_OH_SellAccount = testDebtor.PK;
			charge.JR_RX_NKCostCurrency = "INR";
			charge.JR_RX_NKSellCurrency = "INR";

			JobChargeQuickCalculateBusinessObject quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);

			quickCalculator.QuantityDescription = "Weight";

			quickCalculator.UpdateSell = false;
			quickCalculator.UpdateCost = false;
			quickCalculator.SellRate = 3m;
			quickCalculator.CostRate = 6m;

			quickCalculator.SetCalculationResults();
			AssertEquals(0m, charge.JR_LocalSellAmt);
			AssertEquals("", charge.RevenueCalculationDescription.ToAscii());
			AssertEquals(0m, charge.JR_LocalCostAmt);
			AssertEquals("", charge.CostCalculationDescription.ToAscii());

			quickCalculator.UpdateSell = true;
			quickCalculator.SetCalculationResults();
			AssertEquals(30m, charge.JR_OSSellAmt);
			AssertEquals("International Freight", charge.JR_Desc);
			AssertEquals("FRT: 10 LB @ INR 3.00/LB\r\n\r\nSell Amount Entered using Quick Calculator\r\n", charge.RevenueCalculationDescription.ToAscii());
			AssertEquals(0m, charge.JR_LocalCostAmt);
			AssertEquals("", charge.CostCalculationDescription.ToAscii());
			AssertEquals(false, charge.JR_SellRated);
			AssertEquals(1, charge.SellPaymentBases.Count);
			var jobPaymentBasis = charge.SellPaymentBases.First();
			AssertEquals(10m, jobPaymentBasis.PBS_ChargeableAmount);
			AssertEquals("LB", jobPaymentBasis.PBS_ChargeableUnit);
			AssertEquals("INR", jobPaymentBasis.PBS_RX_NKRateCurrency);
			AssertEquals(3m, jobPaymentBasis.PBS_PerUnitRate);
			AssertEquals("LB", jobPaymentBasis.PBS_RateUnit);
			AssertEquals("QuickCalculator", jobPaymentBasis.PBS_AdapterID);

			quickCalculator.UpdateSell = true;
			testDebtor.CompanyData.InvoiceRollupOrGroups[0].PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.All;
			orgFactory.Save();

			quickCalculator.SetCalculationResults();
			AssertEquals(30m, charge.JR_OSSellAmt);
			AssertEquals("International Freight - 10 LB @ INR 3.00/LB", charge.JR_Desc);

			quickCalculator.UpdateCost = true;
			quickCalculator.SetCalculationResults();
			AssertEquals(60m, charge.JR_OSCostAmt);
			AssertEquals("FRT: 10 LB @ INR 6.00/LB\r\n\r\nCost Amount Entered using Quick Calculator\r\n", charge.CostCalculationDescription.ToAscii());
			AssertEquals("International Freight - 10 LB @ INR 3.00/LB", charge.JR_Desc);
			AssertEquals(false, charge.JR_CostRated);
			AssertEquals(1, charge.CostPaymentBases.Count);
			jobPaymentBasis = charge.CostPaymentBases.First();
			AssertEquals(10m, jobPaymentBasis.PBS_ChargeableAmount);
			AssertEquals("LB", jobPaymentBasis.PBS_ChargeableUnit);
			AssertEquals("INR", jobPaymentBasis.PBS_RX_NKRateCurrency);
			AssertEquals(6m, jobPaymentBasis.PBS_PerUnitRate);
			AssertEquals("LB", jobPaymentBasis.PBS_RateUnit);
			Assert(jobPaymentBasis.PBS_IsCost);
			AssertEquals("QuickCalculator", jobPaymentBasis.PBS_AdapterID);
			AssertEquals("UNT", jobPaymentBasis.PBS_RateReference);
			AssertEquals("", charge.JobChargeAttrib_MinimumRateUsed);

			quickCalculator.IsMinimum = true;
			quickCalculator.CostMinimum = 100;
			quickCalculator.SetCalculationResults();
			jobPaymentBasis = charge.CostPaymentBases.First();
			AssertEquals("MIN", jobPaymentBasis.PBS_RateReference);
			AssertEquals(false, charge.JobChargeAttrib_MinimumRateUsed.IsEmpty);
		}

		public void TestQuickChargeCalculatorDisplaysPercentage()
		{
			DummyAutoRating host = Factory.New<DummyAutoRating>();
			JobChargeQuickCalculateBusinessObject quickCalculator = new JobChargeQuickCalculateBusinessObject(host, Charge);
			Charge.JR_AC = Env.Registry.FreightChargeCode;

			quickCalculator.QuantityDescription = "Charge Percentage";
			quickCalculator.Quantity = 1m;
			quickCalculator.SellRate = 1000m;
			quickCalculator.CostRate = 2000m;
			quickCalculator.ChargeCodePK = ZGuid.Empty;

			quickCalculator.SetCalculationResults();

			AssertEquals(10m, Charge.JR_OSSellAmt);
			AssertEquals(20m, Charge.JR_OSCostAmt);
		}

		public void TestQuickChargeCalculatorDisplaysCorrectChargeableQuantityPercentage()
		{
			var orgFactory = new BusinessObjectFactory();
			var testDebtor = orgFactory.NewWithValidTestData<OrgHeader>();
			AssertNotNull("Create CompanyData before saving", testDebtor.CompanyData);
			orgFactory.Save();

			var job = Factory.NewJobForTesting<Job>();
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_ChargeType = "MRG";
			charge.JR_MarginPercentage = 0m;
			charge.JR_OH_SellAccount = testDebtor.PK;
			charge.JR_RX_NKSellCurrency = "AUD";

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);
			quickCalculator.QuantityDescription = "Charge Percentage";
			quickCalculator.Quantity = 2m;
			quickCalculator.SellRate = 1000m;
			quickCalculator.CostRate = 2000m;
			quickCalculator.ChargeCodePK = ZGuid.Empty;

			quickCalculator.SetCalculationResults();

			AssertEquals(20m, charge.JR_OSSellAmt);
			AssertEquals(40m, charge.JR_OSCostAmt);
			AssertEquals("FRT: 2.00% of (AUD 1000.00)\r\n\r\nSell Amount Entered using Quick Calculator\r\n", charge.RevenueCalculationDescription.ToAscii());

			quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);
			quickCalculator.QuantityDescription = "Charge Percentage";
			quickCalculator.Quantity = 0.1m;
			quickCalculator.SellRate = 1000m;
			quickCalculator.CostRate = 2000m;
			quickCalculator.ChargeCodePK = ZGuid.Empty;

			quickCalculator.SetCalculationResults();

			AssertEquals(1m, charge.JR_OSSellAmt);
			AssertEquals(2m, charge.JR_OSCostAmt);
			AssertEquals("FRT: 0.10% of (AUD 1000.00)\r\n\r\nSell Amount Entered using Quick Calculator\r\n", charge.RevenueCalculationDescription.ToAscii());
		}

		public void TestStoreResults_CurrencyDecimals()
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			OrgHeader testDebtor = orgFactory.NewWithValidTestData<OrgHeader>();
			AssertNotNull("Create CompanyData before saving", testDebtor.CompanyData);
			orgFactory.Save();

			Job job = Factory.NewJobForTesting<Job>();
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_ChargeType = "MRG";
			charge.JR_MarginPercentage = 0m;
			charge.JR_OH_SellAccount = testDebtor.PK;
			charge.JR_RX_NKSellCurrency = "JPY";
			JobChargeQuickCalculateBusinessObject quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);

			quickCalculator.QuantityDescription = "Weight";

			quickCalculator.UpdateSell = true;
			quickCalculator.SellRate = 3.12m;
			quickCalculator.SetCalculationResults();
			AssertEquals(31m, charge.JR_OSSellAmt);
			AssertEquals("International Freight", charge.JR_Desc);
			AssertEquals("FRT: 10 LB @ JPY 3.12/LB\r\n\r\nSell Amount Entered using Quick Calculator\r\n", charge.RevenueCalculationDescription.ToAscii());
		}

		public void TestCalculation_CarrierCommission_NoCarrier()
		{
			ZQuery chargeCodeQuery = new ZQuery(AccChargeCodeSchema.AC_Code, "BAF");
			chargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			AccChargeCode bAF = Factory.LoadTop1<AccChargeCode>(chargeCodeQuery);

			DummyAutoRating host = Factory.New<DummyAutoRating>();
			JobChargeQuickCalculateBusinessObject quickCalculator = new JobChargeQuickCalculateBusinessObject(host, Charge);
			Charge.JR_AC = Env.Registry.FreightChargeCode;

			Charge existingCharge = Job.Charges.AddNew();
			existingCharge.JR_AC = bAF.PK;
			existingCharge.JR_OSSellAmt = 100m;
			existingCharge.JR_OSCostAmt = 65m;

			quickCalculator.QuantityDescription = "Carrier Commission";
			quickCalculator.ChargeCodePK = bAF.PK;
			quickCalculator.Quantity = 40m;

			quickCalculator.SetCalculationResults();
			AssertEquals(40m, Charge.JR_OSSellAmt);
			AssertEquals(26m, Charge.JR_OSCostAmt);
			AssertContains("FRT: 40.00% of (AUD 100.00 (BAF)) - Carrier Commission for Unknown Carrier", Charge.RevenueCalculationDescription.ToAscii());
			AssertEquals(ZGuid.Empty, Charge.JR_OH_SellAccount);
			AssertEquals(1, Charge.SellPaymentBases.Count);
			var jobPaymentBasis = Charge.SellPaymentBases.First();
			AssertEquals(100m, jobPaymentBasis.PBS_ChargeableAmount);
			AssertEquals("AUD", jobPaymentBasis.PBS_ChargeableUnit);
			AssertEquals(40m, jobPaymentBasis.PBS_PerUnitRate);
			AssertEquals("QuickCalculator", jobPaymentBasis.PBS_AdapterID);
		}

		public void TestCalculation_CarrierCommission_Carrier()
		{
			ZQuery chargeCodeQuery = new ZQuery(AccChargeCodeSchema.AC_Code, "BAF");
			chargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			AccChargeCode bAF = Factory.LoadTop1<AccChargeCode>(chargeCodeQuery);

			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_FullName = "MY CARRIER";

			DummyAutoRating host = Factory.New<DummyAutoRating>();
			host.CarrierToUseForTest = carrier;

			JobChargeQuickCalculateBusinessObject quickCalculator = new JobChargeQuickCalculateBusinessObject(host, Charge);
			Charge.JR_AC = Env.Registry.FreightChargeCode;

			Charge existingCharge = Job.Charges.AddNew();
			existingCharge.JR_AC = bAF.PK;
			existingCharge.JR_OSSellAmt = 100m;
			existingCharge.JR_OSCostAmt = 65m;

			quickCalculator.QuantityDescription = "Carrier Commission";
			quickCalculator.ChargeCodePK = bAF.PK;
			quickCalculator.Quantity = 40m;

			quickCalculator.SetCalculationResults();
			AssertEquals(40m, Charge.JR_OSSellAmt);
			AssertEquals(26m, Charge.JR_OSCostAmt);
			AssertContains("FRT: 40.00% of (AUD 100.00 (BAF)) - Carrier Commission for MY CARRIER", Charge.RevenueCalculationDescription.ToAscii());
			AssertEquals(carrier.PK, Charge.JR_OH_SellAccount);
		}

		public void TestCaculation_UpdateSpotReferenceOnHost()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsCreditor = true;

			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001");
			consol.JK_OverrideConsolChargeable = true;
			consol.JK_CorrectedConsolWeight = 1000M;
			var consolCost = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC2, org);
			consolCost.E6_RatingBehaviour = RatingBehaviours.AllInAdhocOverridingSpotRate;

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(consol.RatingAdapter, consolCost);

			quickCalculator.QuantityDescription = "Job Weight";
			quickCalculator.IsMinimum = true;
			quickCalculator.CostMinimum = 100M;
			quickCalculator.CostRate = 2.2M;
			quickCalculator.SetCalculationResults();

			var spotReference = consol.Numbers.GetAllReferenceNumbersByType(CustomsAdditionalReferenceNumbersCodes.SpotReference).FirstOrDefault();
			AssertEquals("Should update Spot Reference on consol after calculation", "SAA ZZCC2 AUD 2.20@KG Min: 100.00", spotReference);
		}

		public void TestStoreResults_OrgsSet()
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			OrgHeader testDebtor = orgFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader testCreditor = orgFactory.NewWithValidTestData<OrgHeader>();
			orgFactory.Save();

			Job job = Factory.NewJobForTesting<Job>();
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_OH_SellAccount = testDebtor.PK;
			charge.JR_OH_CostAccount = testCreditor.PK;
			JobChargeQuickCalculateBusinessObject quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);

			quickCalculator.QuantityDescription = "Weight";

			quickCalculator.UpdateSell = true;
			quickCalculator.UpdateCost = true;
			quickCalculator.SellRate = 3m;
			quickCalculator.CostRate = 6m;

			quickCalculator.SetCalculationResults();
			AssertEquals(testCreditor.PK, charge.JR_OH_CostAccount);
			AssertEquals(testDebtor.PK, charge.JR_OH_SellAccount);
		}

		public void TestStoreResults_DisableCalculationLogs()
		{
			CalculationLogsLoader.Save(Charge, new CalculationLogsWrapper());
			AssertEquals("Precondition", false, CalculationLogsLoader.Load(Charge).IsDisabled);

			DummyAutoRating dummyAutoRating = Factory.New<DummyAutoRating>();
			CalculationLogsLoader.Save(dummyAutoRating, new CalculationLogsWrapper());
			AssertEquals("Precondition", false, CalculationLogsLoader.Load(dummyAutoRating).IsDisabled);

			JobChargeQuickCalculateBusinessObject quickCalculator = new JobChargeQuickCalculateBusinessObject(dummyAutoRating, Charge);
			quickCalculator.QuantityDescription = "Weight";
			quickCalculator.SetCalculationResults();

			AssertEquals("Calculation logs disabled", true, CalculationLogsLoader.Load(Charge).IsDisabled);
			AssertEquals("Calculation logs disabled", true, CalculationLogsLoader.Load(dummyAutoRating).IsDisabled);
		}

		public void TestRecalculatesWithRatingOverride()
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			OrgHeader testDebtor = orgFactory.NewWithValidTestData<OrgHeader>();
			AssertNotNull("Create CompanyData before saving", testDebtor.CompanyData);
			orgFactory.Save();

			Job job = Factory.NewJobForTesting<Job>();
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_ChargeType = "MRG";
			charge.JR_MarginPercentage = 0m;
			charge.JR_OH_SellAccount = testDebtor.PK;
			charge.JR_CostRatingOverride = true;
			charge.JR_SellRatingOverride = true;
			JobChargeQuickCalculateBusinessObject quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);

			quickCalculator.QuantityDescription = "Weight";

			quickCalculator.UpdateSell = true;
			quickCalculator.SellRate = 5m;
			quickCalculator.UpdateCost = true;
			quickCalculator.CostRate = 3m;
			quickCalculator.SetCalculationResults();
			AssertEquals(50m, charge.JR_OSSellAmt);
			AssertEquals(30m, charge.JR_OSCostAmt);
			Assert("JR_SellRatingOverride is true", charge.JR_SellRatingOverride);
			Assert("JR_CostRatingOverride is true", charge.JR_CostRatingOverride);
			AssertEquals("International Freight", charge.JR_Desc);
			AssertEquals("FRT: 10 LB @ AUD 5.00/LB\r\n\r\nSell Amount Entered using Quick Calculator\r\n", charge.RevenueCalculationDescription.ToAscii());
			AssertEquals("FRT: 10 LB @ AUD 3.00/LB\r\n\r\nCost Amount Entered using Quick Calculator\r\n", charge.CostCalculationDescription.ToAscii());
		}

		public void TestInvoiceTypeIsNotChanged()
		{
			var orgFactory = new BusinessObjectFactory();
			var testDebtor = orgFactory.NewWithValidTestData<OrgHeader>();
			AssertNotNull("Create CompanyData before saving", testDebtor.CompanyData);
			orgFactory.Save();

			var job = Factory.NewJobForTesting<Job>();
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_ChargeType = Core.Constants.ChargeType.Disbursement;
			charge.JR_OH_SellAccount = testDebtor.PK;
			charge.JR_RX_NKCostCurrency = "SGD";
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency;

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);
			quickCalculator.QuantityDescription = "Weight";

			quickCalculator.UpdateCost = true;
			quickCalculator.CostRate = 2.92m;
			quickCalculator.SetCalculationResults();
			AssertEquals(29.20m, charge.JR_OSSellAmt);
			AssertEquals(29.20m, charge.JR_OSCostAmt);
			AssertEquals("InvoiceType should not change", InvoiceTypesList.Codes.DisbursementInForeignCurrency, charge.JR_InvoiceType);
		}

		public void TestInvoiceLineDescriptionObtainedForDisbursementCharges()
		{
			var orgFactory = new BusinessObjectFactory();
			var testDebtor = orgFactory.NewWithValidTestData<OrgHeader>();
			AssertNotNull("Create CompanyData before saving", testDebtor.CompanyData);
			testDebtor.CompanyData.InvoiceRollupOrGroups[0].PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.All;

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "DSBC";
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			chargeCode.AC_Desc = "Test Charge";
			orgFactory.Save();

			var job = Factory.NewJobForTesting<Job>();
			var charge = job.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_OH_SellAccount = testDebtor.PK;
			charge.JR_RX_NKCostCurrency = "USD";
			charge.JR_RX_NKSellCurrency = "USD";

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);
			quickCalculator.QuantityDescription = "Weight";

			quickCalculator.UpdateCost = true;
			quickCalculator.UpdateSell = true;
			quickCalculator.CostRate = 100m;
			quickCalculator.SellRate = 200m;
			quickCalculator.SetCalculationResults();
			AssertEquals(2000m, charge.JR_OSSellAmt);
			AssertEquals(2000m, charge.JR_OSCostAmt);
			AssertEquals("Test Charge - 10 LB @ USD 200.00/LB", charge.JR_Desc);
		}

		public void TestCalculateByContainerCount_ShouldNotThrowExceptionWhenAllMeasureDimensionsAreNotProvided()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			var move = cartage.ContainerBookedMoves.AddNew();
			move.Container.JC_ContainerNum = "CONT00001";
			var leg = move.CartageLegs.AddNew();
			var cartageAdapter = new CartageLegRatingAdapter(leg);

			var rateableMeasures = (RateableMeasureSet)cartageAdapter.RateableMeasures;
			AssertEquals("Cartage Rating Adapter does not have Commodity", false, rateableMeasures.ContainerListHasCommodity);

			var job = Factory.NewJobForTesting<Job>();
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_ChargeType = "MRG";

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(cartageAdapter, charge);
			AssertNoExceptionThrown("Cartage Rating Adapter does not have all the dimension but there should not be any exception when selecting Container Count", () => quickCalculator.QuantityDescription = "Container Count");
		}

		public void TestCalculateByContainerTypes()
		{
			var orgFactory = new BusinessObjectFactory();
			var testDebtor = orgFactory.NewWithValidTestData<OrgHeader>();
			orgFactory.Save();

			var job = Factory.NewJobForTesting<Job>();
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_ChargeType = "MRG";
			charge.JR_MarginPercentage = 0m;
			charge.JR_OH_SellAccount = testDebtor.PK;

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);

			quickCalculator.QuantityDescription = "Container Count";
			var cont20GP = quickCalculator.Containers.Cast<ContainerCalculationData>().SingleOrDefault(x => x.ContainerType == "20GP");
			var cont40GP = quickCalculator.Containers.Cast<ContainerCalculationData>().SingleOrDefault(x => x.ContainerType == "40GP");

			cont20GP.Cost = 2000m;
			cont20GP.Sell = 3000m;

			cont40GP.Cost = 3000m;
			cont40GP.Sell = 4000m;

			quickCalculator.UpdateCost = true;
			quickCalculator.UpdateSell = true;
			quickCalculator.SetCalculationResults();

			AssertEquals(8000m, charge.JR_OSCostAmt);
			AssertEquals(@"FRT: 2 40GP @ AUD 3000.00/CN + 1 20GP @ AUD 2000.00/CN

Cost Amount Entered using Quick Calculator
", charge.CostCalculationDescription.ToAscii());

			AssertEquals(11000m, charge.JR_OSSellAmt);
			AssertEquals(@"FRT: 2 40GP @ AUD 4000.00/CN + 1 20GP @ AUD 3000.00/CN

Sell Amount Entered using Quick Calculator
", charge.RevenueCalculationDescription.ToAscii());

			AssertEquals(2, charge.SellPaymentBases.Count);
			var jobPaymentBasis1 = charge.SellPaymentBases.ToArray()[0];
			var jobPaymentBasis2 = charge.SellPaymentBases.ToArray()[1];

			AssertEquals(1m, jobPaymentBasis1.PBS_ChargeableAmount);
			AssertEquals("20GP", jobPaymentBasis1.PBS_ChargeableUnit);
			AssertEquals(3000m, jobPaymentBasis1.PBS_PerUnitRate);
			AssertEquals("AUD", jobPaymentBasis1.PBS_RX_NKRateCurrency);
			AssertEquals("CN", jobPaymentBasis1.PBS_RateUnit);
			AssertEquals("CONT00002", jobPaymentBasis1.PBS_ChargeableDescription);

			AssertEquals(2m, jobPaymentBasis2.PBS_ChargeableAmount);
			AssertEquals("40GP", jobPaymentBasis2.PBS_ChargeableUnit);
			AssertEquals(4000m, jobPaymentBasis2.PBS_PerUnitRate);
			AssertEquals("AUD", jobPaymentBasis2.PBS_RX_NKRateCurrency);
			AssertEquals("CN", jobPaymentBasis2.PBS_RateUnit);
			AssertEquals("CONT00003, CONT00004", jobPaymentBasis2.PBS_ChargeableDescription);

			AssertEquals("QuickCalculator", jobPaymentBasis1.PBS_AdapterID);
		}

		public void TestCalculateByContainerTypes_CostAndSellAreSetToZero()
		{
			var orgFactory = new BusinessObjectFactory();
			var testDebtor = orgFactory.NewWithValidTestData<OrgHeader>();
			orgFactory.Save();

			var job = Factory.NewJobForTesting<Job>();
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_ChargeType = "MRG";
			charge.JR_MarginPercentage = 0m;
			charge.JR_OH_SellAccount = testDebtor.PK;
			charge.JR_OSSellAmt = 100;
			charge.JR_OSCostAmt = 50;
			charge.JR_SellRatingOverride = false;
			charge.JR_CostRatingOverride = false;

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);

			quickCalculator.QuantityDescription = "Container Count";
			var cont20GP = quickCalculator.Containers.Cast<ContainerCalculationData>().SingleOrDefault(x => x.ContainerType == "20GP");
			var cont40GP = quickCalculator.Containers.Cast<ContainerCalculationData>().SingleOrDefault(x => x.ContainerType == "40GP");

			cont20GP.Cost = 0m;
			cont20GP.Sell = 0m;

			cont40GP.Cost = 0m;
			cont40GP.Sell = 0m;

			quickCalculator.UpdateSell = true;
			quickCalculator.SetCalculationResults();

			AssertEquals(0m, charge.JR_OSSellAmt);
			AssertEquals(@"FRT: 

Sell Amount Entered using Quick Calculator
", charge.RevenueCalculationDescription.ToAscii());
		}

		public void TestCalculateByContainerTypes_OnlyOneContainerTypeIsChanged()
		{
			var orgFactory = new BusinessObjectFactory();
			var testDebtor = orgFactory.NewWithValidTestData<OrgHeader>();
			orgFactory.Save();

			var job = Factory.NewJobForTesting<Job>();
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_ChargeType = "MRG";
			charge.JR_MarginPercentage = 0m;
			charge.JR_OH_SellAccount = testDebtor.PK;

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);

			quickCalculator.QuantityDescription = "Container Count";
			var cont20GP = quickCalculator.Containers.Cast<ContainerCalculationData>().SingleOrDefault(x => x.ContainerType == "20GP");

			cont20GP.Cost = 2000m;
			cont20GP.Sell = 3000m;

			quickCalculator.UpdateCost = true;
			quickCalculator.UpdateSell = true;
			quickCalculator.SetCalculationResults();

			AssertEquals(2000m, charge.JR_OSCostAmt);
			AssertEquals(@"FRT: 1 20GP @ AUD 2000.00/CN

Cost Amount Entered using Quick Calculator
", charge.CostCalculationDescription.ToAscii());

			AssertEquals(3000m, charge.JR_OSSellAmt);
			AssertEquals(@"FRT: 1 20GP @ AUD 3000.00/CN

Sell Amount Entered using Quick Calculator
", charge.RevenueCalculationDescription.ToAscii());
		}

		public void TestQuantityUpdatedWhenContainerSelectionsChanged()
		{
			var orgFactory = new BusinessObjectFactory();
			var testDebtor = orgFactory.NewWithValidTestData<OrgHeader>();
			orgFactory.Save();

			var job = Factory.NewJobForTesting<Job>();
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_ChargeType = "MRG";
			charge.JR_MarginPercentage = 0m;
			charge.JR_OH_SellAccount = testDebtor.PK;

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);

			quickCalculator.QuantityDescription = "Container Count";
			var cont20GP = quickCalculator.Containers.Cast<ContainerCalculationData>().SingleOrDefault(x => x.ContainerType == "20GP");
			var cont40GP = quickCalculator.Containers.Cast<ContainerCalculationData>().SingleOrDefault(x => x.ContainerType == "40GP");

			AssertEquals("Originally, all non-LCL containers should be selected and overall quantity should be 5.", 5m, quickCalculator.Quantity);

			cont20GP.Cost = 10m;
			cont20GP.Sell = 15m;
			cont40GP.Cost = 20m;
			cont40GP.Sell = 25m;

			quickCalculator.UpdateCost = true;
			quickCalculator.UpdateSell = true;
			quickCalculator.SetCalculationResults();

			AssertEquals("Total cost", 50m, charge.JR_OSCostAmt);
			AssertEquals(@"FRT: 2 40GP @ AUD 20.00/CN + 1 20GP @ AUD 10.00/CN

Cost Amount Entered using Quick Calculator
", charge.CostCalculationDescription.ToAscii());

			AssertEquals("Total revenue", 65m, charge.JR_OSSellAmt);
			AssertEquals(@"FRT: 2 40GP @ AUD 25.00/CN + 1 20GP @ AUD 15.00/CN

Sell Amount Entered using Quick Calculator
", charge.RevenueCalculationDescription.ToAscii());

			cont20GP.ContainerSelections[0].IsSelected = false;
			AssertEquals("Unselect first container of 20GPs. There should be 2 containers selected and overall quantity should be 2.", 2m, quickCalculator.Quantity);

			foreach (ContainerSelectionBusinessObject containerSelection in cont40GP.ContainerSelections)
			{
				containerSelection.IsSelected = false;
			}
			AssertEquals("Unselect all containers of 40GPs. There should be no container selected and overall quantity should be 0.", 0m, quickCalculator.Quantity);
		}

		public void TestCostOrSellRatingOverrideValue()
		{
			var host = Factory.New<DummyAutoRating>();
			var quickCalculator = new JobChargeQuickCalculateBusinessObject(host, Charge);
			Charge.JR_AC = Env.Registry.FreightChargeCode;
			quickCalculator.QuantityDescription = nameof(MeasureType.Chargeable);
			quickCalculator.Quantity = 10m;
			quickCalculator.SellRate = 10m;
			quickCalculator.CostRate = 20m;
			quickCalculator.ChargeCodePK = ZGuid.Empty;

			AssertEquals("Before calculation Cost override must be false", false, Charge.JR_CostRatingOverride);
			AssertEquals("Before calculation Sell override must be false", false, Charge.JR_SellRatingOverride);

			quickCalculator.SetCalculationResults();

			AssertEquals(100m, quickCalculator.SellTotal);
			AssertEquals(200m, quickCalculator.CostTotal);

			AssertEquals(100m, Charge.JR_OSSellAmt);
			AssertEquals(200m, Charge.JR_OSCostAmt);
			AssertEquals(100m, charge.JR_LocalSellAmt);
			AssertEquals(200m, charge.JR_LocalCostAmt);

			AssertEquals("After calculation Cost override must be true", true, Charge.JR_CostRatingOverride);
			AssertEquals("After calculation Sell override must be true", true, Charge.JR_SellRatingOverride);
		}

		#endregion

		#region Properties

		public void TestTotals()
		{
			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), Charge);

			quickCalculator.QuantityDescription = "Custom";
			quickCalculator.Quantity = 10m;

			quickCalculator.SellRate = 3m;
			quickCalculator.CostRate = 6m;

			AssertEquals(30m, quickCalculator.SellTotal);
			AssertEquals(60m, quickCalculator.CostTotal);

			quickCalculator.SellRate = 4m;
			AssertEquals(40m, quickCalculator.SellTotal);
			AssertEquals(60m, quickCalculator.CostTotal);

			quickCalculator.CostRate = 5m;
			AssertEquals(40m, quickCalculator.SellTotal);
			AssertEquals(50m, quickCalculator.CostTotal);

			quickCalculator.Quantity = 20m;
			AssertEquals(80m, quickCalculator.SellTotal);
			AssertEquals(100m, quickCalculator.CostTotal);

			quickCalculator.QuantityDescription = "Carrier Commission";
			AssertEquals(0m, quickCalculator.SellTotal);
			AssertEquals(0m, quickCalculator.CostTotal);

			quickCalculator.QuantityDescription = "Custom";
			quickCalculator.Quantity = 10m;
			quickCalculator.IsMinimum = true;
			AssertEquals(40m, quickCalculator.SellTotal);
			AssertEquals(50m, quickCalculator.CostTotal);

			AssertEquals(0m, quickCalculator.SellMinimum);
			AssertEquals(0m, quickCalculator.CostMinimum);

			quickCalculator.SellTotal = 45m;
			quickCalculator.CostTotal = 55m;
			AssertEquals(45m, quickCalculator.SellTotal);
			AssertEquals(55m, quickCalculator.CostTotal);

			quickCalculator.SellMinimum = 40m;
			AssertEquals(40m, quickCalculator.SellTotal);
			quickCalculator.CostMinimum = 50m;
			AssertEquals(50m, quickCalculator.CostTotal);

			quickCalculator.SellRate = 3m;
			quickCalculator.CostRate = 6m;
			quickCalculator.Quantity = 20m;

			AssertEquals(40m, quickCalculator.SellMinimum);
			AssertEquals(50m, quickCalculator.CostMinimum);
			AssertEquals(60m, quickCalculator.SellTotal);
			AssertEquals(120m, quickCalculator.CostTotal);

			quickCalculator.QuantityDescription = "Carrier Commission";

			AssertEquals(40m, quickCalculator.SellMinimum);
			AssertEquals(50m, quickCalculator.CostMinimum);
			AssertEquals(40m, quickCalculator.SellTotal);
			AssertEquals(50m, quickCalculator.CostTotal);

			quickCalculator.QuantityDescription = "Custom";
			quickCalculator.Quantity = 10m;
			quickCalculator.IsMinimum = false;
			AssertEquals(30m, quickCalculator.SellTotal);
			AssertEquals(60m, quickCalculator.CostTotal);
			AssertEquals(0m, quickCalculator.SellMinimum);
			AssertEquals(0m, quickCalculator.CostMinimum);

			quickCalculator.QuantityDescription = "Container Count";
			var cont20GP = quickCalculator.Containers.Cast<ContainerCalculationData>().SingleOrDefault(x => x.ContainerType == "20GP");
			var cont40GP = quickCalculator.Containers.Cast<ContainerCalculationData>().SingleOrDefault(x => x.ContainerType == "40GP");

			cont20GP.Cost = 2000m;
			cont20GP.Sell = 3000m;

			AssertEquals(2000m, quickCalculator.CostTotal);
			AssertEquals(3000m, quickCalculator.SellTotal);

			cont40GP.Cost = 3000m;
			cont40GP.Sell = 4000m;

			AssertEquals(8000m, quickCalculator.CostTotal);
			AssertEquals(11000m, quickCalculator.SellTotal);
		}

		public void TestTotals_Percentage()
		{
			JobChargeQuickCalculateBusinessObject quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), Charge);

			quickCalculator.QuantityDescription = "Charge Percentage";
			quickCalculator.CostRate = 100m;
			quickCalculator.SellRate = 200m;
			quickCalculator.Quantity = 75m;

			AssertEquals(75m, quickCalculator.CostTotal);
			AssertEquals(150m, quickCalculator.SellTotal);
		}

		public void TestChargeCodePK()
		{
			JobChargeQuickCalculateBusinessObject quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), Charge);

			Charge existingCharge = Job.Charges.AddNew();
			existingCharge.JR_AC = Env.Registry.FreightChargeCode;
			existingCharge.JR_OSSellAmt = 100m;
			existingCharge.JR_OSCostAmt = 65m;

			quickCalculator.QuantityDescription = "Charge Percentage";
			AssertEquals(100m, quickCalculator.SellRate);
			AssertEquals(65m, quickCalculator.CostRate);
		}

		public void TestQuantityDescriptionList()
		{
			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), Charge);

			// Not proper Measure Types should not appear in this list.
			AssertContainsExactElementsInAnyOrder(new string[] { "Chargeable", "Container Count", "Package", "TEU", "Volume", "Weight", "Carrier Commission", "Charge Percentage", "Custom" }, quickCalculator.QuantityDescriptionList.GetAllCodes());

			AssertEquals("Weight", quickCalculator.QuantityDescriptionList.GetDescriptionFromCode("Weight"));
			AssertEquals("ContainerCount", quickCalculator.QuantityDescriptionList.GetDescriptionFromCode("Container Count"));
		}

		public void TestShipmentChargeable()
		{
			var creator = new TestObjectCreator(Factory);
			var setup = creator.CreateGatewayConsolsAndShipments();
			var console = setup.gC0002;

			using (var job = creator.CreateJob(console))
			{
				var adapter = (job.PlugInData as IRatingSupporter).AdaptersProvider.GetAdapters(null, AutoRateOptions.AutorateCosts).FirstOrDefault();

				var chargeWithShipment1RelatedJob = job.Charges.AddNew();
				chargeWithShipment1RelatedJob.JR_Calc_RelatedJobNumber = setup.s0001.JobNumber;
				var chargeWithShipment2RelatedJob = job.Charges.AddNew();
				chargeWithShipment2RelatedJob.JR_Calc_RelatedJobNumber = setup.s0002.JobNumber;
				var chargeWithEmptyRelatedJob = job.Charges.AddNew();
				AssertNullOrEmpty("Precondition: JR_Calc_RelatedJobNumber should be empty", chargeWithEmptyRelatedJob.JR_Calc_RelatedJobNumber);

				CheckShipmentChargeable(adapter, chargeWithShipment1RelatedJob, true);
				CheckShipmentChargeable(adapter, chargeWithEmptyRelatedJob, false);
				CheckShipmentChargeable(adapter, chargeWithShipment2RelatedJob, true);
			}

			Charge.JR_Calc_RelatedJobNumber = "test";
			CheckShipmentChargeable(Factory.New<DummyAutoRating>(), Charge, false);

			void CheckShipmentChargeable(IAutoRating host, IQuickCalculatorCharge charge, bool isShipmentChargeable)
			{
				var quickCalculator = new JobChargeQuickCalculateBusinessObject(host, charge);
				if (isShipmentChargeable)
				{
					AssertCollectionContains("should contain Shipment Chargeable", quickCalculator.QuantityDescriptionList.GetAllCodes(), x => x == "Shipment Chargeable");
					AssertCollectionNotContains("should not contain Chargeable", quickCalculator.QuantityDescriptionList.GetAllCodes(), x => x == "Chargeable");
				}
				else
				{
					AssertCollectionContains("should contain Chargeable", quickCalculator.QuantityDescriptionList.GetAllCodes(), x => x == "Chargeable");
					AssertCollectionNotContains("should not contain Shipment Chargeable", quickCalculator.QuantityDescriptionList.GetAllCodes(), x => x == "Shipment Chargeable");
				}
			}
		}

		public void TestUsesContainerCountMeasure()
		{
			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), Charge);

			quickCalculator.QuantityDescription = "Container Count";
			Assert(quickCalculator.UsesContainerCountMeasure);

			quickCalculator.QuantityDescription = "Weight";
			Assert(!quickCalculator.UsesContainerCountMeasure);
		}

		public void TestContainers()
		{
			var factory = new BusinessObjectFactory();
			var debtor = factory.NewWithValidTestData<OrgHeader>();
			factory.Save();

			var job = Factory.NewJobForTesting<Job>();
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_ChargeType = "MRG";
			charge.JR_MarginPercentage = 0m;
			charge.JR_OH_SellAccount = debtor.PK;

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);

			quickCalculator.QuantityDescription = "Container Count";
			AssertContainsExactElementsInAnyOrder
			(
				"Containers should not return empty or invalid Container PK",
				Array.Empty<string>(),
				quickCalculator.Containers.Cast<ContainerCalculationData>().Where(x => x.ContainerType == "").Select(x => x.ContainerNumbers)
			);
		}

		public void TestContainerTypesList()
		{
			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), Charge);
			var expectedContainerTypes = new[] { "20GP", "40GP" };
			AssertContainsExactElementsInAnyOrder(expectedContainerTypes, quickCalculator.Containers.Cast<ContainerCalculationData>().Select(c => c.ContainerType));
		}

		public void TestContainerTypeSelection()
		{
			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), Charge);
			quickCalculator.QuantityDescription = "Container Count";
			Assert(!quickCalculator.IsMinimumVisible);
			quickCalculator.QuantityDescription = "Weight";
			AssertEquals(10m, quickCalculator.Quantity);
			AssertEquals("LB", quickCalculator.QuantityUnit);
			Assert(quickCalculator.IsMinimumVisible);
		}

		public void TestContainerTypeSelection_UpdateSellAndUpdateSell_ReadOnlyStateShouldBeIndependentFromValue()
		{
			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_OSSellAmt = 100m;
			charge.JR_OSCostAmt = 65m;
			charge.JR_ChargeType = Core.Constants.ChargeType.Margin;
			charge.JR_CostRatingOverride = false;

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);
			AssertEquals("Pre-condition: UpdateSell Read Only state", false, quickCalculator.UpdateSellInfo.ReadOnly);
			AssertEquals("Pre-condition: UpdateSell Value", true, quickCalculator.UpdateSell);

			quickCalculator.UpdateSell = ZBool.False;
			AssertEquals("UpdateSell Read Only state should not change", false, quickCalculator.UpdateSellInfo.ReadOnly);

			AssertEquals("Pre-condition: UpdateCost Read Only state", false, quickCalculator.UpdateCostInfo.ReadOnly);
			AssertEquals("Pre-condition: UpdateCost Value", true, quickCalculator.UpdateCost);
			quickCalculator.UpdateCost = ZBool.False;
			AssertEquals("UpdateCost Read Only state should not change", false, quickCalculator.UpdateCostInfo.ReadOnly);
		}

		public void TestQuantityDescription()
		{
			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), Charge)
			{
				QuantityDescription = "Weight"
			};

			AssertEquals(10m, quickCalculator.Quantity);
			AssertEquals("LB", quickCalculator.QuantityUnit);
			AssertEquals(true, quickCalculator.QuantityInfo.ReadOnly);

			quickCalculator.QuantityDescription = "Volume";
			AssertEquals(13m, quickCalculator.Quantity);
			AssertEquals("M3", quickCalculator.QuantityUnit);
			AssertEquals(true, quickCalculator.QuantityInfo.ReadOnly);

			quickCalculator.QuantityDescription = "Container Count";
			AssertEquals(5m, quickCalculator.Quantity);
			AssertEquals("CN", quickCalculator.QuantityUnit);
			AssertEquals(true, quickCalculator.QuantityInfo.ReadOnly);

			quickCalculator.QuantityDescription = "TEU";
			AssertEquals(7m, quickCalculator.Quantity);
			AssertEquals("TEU", quickCalculator.QuantityUnit);
			AssertEquals(true, quickCalculator.QuantityInfo.ReadOnly);

			quickCalculator.QuantityDescription = "Crap";
			AssertEquals(0m, quickCalculator.Quantity);
			AssertEquals("", quickCalculator.QuantityUnit);
			AssertEquals(true, quickCalculator.QuantityInfo.ReadOnly);

			quickCalculator.QuantityDescription = "Custom";
			AssertEquals(0m, quickCalculator.Quantity);
			AssertEquals("", quickCalculator.QuantityUnit);
			AssertEquals(false, quickCalculator.QuantityInfo.ReadOnly);
		}

		public void TestQuantityDescription_Chargeable()
		{
			var creator = new TestObjectCreator(Factory);
			var setup = creator.CreateGatewayConsolsAndShipments();
			var consol = setup.gC0002;

			using (var job = creator.CreateJob(consol))
			{
				var chargeWithShipment1RelatedJob = job.Charges.AddNew();
				chargeWithShipment1RelatedJob.JR_Calc_RelatedJobNumber = setup.s0001.JobNumber;
				var chargeWithShipment2RelatedJob = job.Charges.AddNew();
				chargeWithShipment2RelatedJob.JR_Calc_RelatedJobNumber = setup.s0002.JobNumber;
				var chargeWithEmptyRelatedJob = job.Charges.AddNew();
				AssertNullOrEmpty("Precondition: charge has no related job", chargeWithEmptyRelatedJob.JR_Calc_RelatedJobNumber);

				var adapter = (job.PlugInData as IRatingSupporter).AdaptersProvider.GetAdapters(null, AutoRateOptions.AutorateCosts).FirstOrDefault();
				var quickCalculator = new JobChargeQuickCalculateBusinessObject(adapter, chargeWithShipment1RelatedJob);

				quickCalculator.QuantityDescription = "Shipment Chargeable";
				AssertEquals("Shipment Chargeable: should pick Quantity from RelatedShipment JS_ActualChargeable", setup.s0001.JS_ActualChargeable, quickCalculator.Quantity);
				AssertEquals("Shipment Chargeable: should pick QuantityUnit from RelatedShipment JS_ChargeableUnit", setup.s0001.JS_ChargeableUnit, quickCalculator.QuantityUnit);
				AssertEquals("Shipment Chargeable: Quantity should be readonly", true, quickCalculator.QuantityInfo.ReadOnly);

				quickCalculator = new JobChargeQuickCalculateBusinessObject(adapter, chargeWithEmptyRelatedJob);
				quickCalculator.QuantityDescription = "Chargeable";
				AssertEquals("Chargeable: should pick Quantity from Console JK_ConsolChargeable", consol.JK_ConsolChargeable, quickCalculator.Quantity);
				AssertEquals("Chargeable: should pick QuantityUnit from Console JK_ConsolChargeableUnit", consol.JK_ConsolChargeableUnit, quickCalculator.QuantityUnit);
				AssertEquals("Chargeable: Quantity should be readonly", true, quickCalculator.QuantityInfo.ReadOnly);

				quickCalculator = new JobChargeQuickCalculateBusinessObject(adapter, chargeWithShipment2RelatedJob);
				quickCalculator.QuantityDescription = "Shipment Chargeable";
				AssertEquals("Shipment Chargeable: should pick Quantity from RelatedShipment JS_ActualChargeable", setup.s0002.JS_ActualChargeable, quickCalculator.Quantity);
				AssertEquals("Shipment Chargeable: should pick QuantityUnit from RelatedShipment JS_ChargeableUnit", setup.s0002.JS_ChargeableUnit, quickCalculator.QuantityUnit);
				AssertEquals("Shipment Chargeable: Quantity should be readonly", true, quickCalculator.QuantityInfo.ReadOnly);
			}
		}

		public void TestQuantityDescription_Percentage()
		{
			JobChargeQuickCalculateBusinessObject quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), Charge);

			quickCalculator.QuantityDescription = "Volume";
			AssertEquals(13m, quickCalculator.Quantity);
			AssertEquals("M3", quickCalculator.QuantityUnit);
			AssertEquals(true, quickCalculator.QuantityInfo.ReadOnly);
			AssertEquals(false, quickCalculator.IsPercentageCharge);
			AssertEquals(ZGuid.Empty, quickCalculator.ChargeCodePK);
			AssertEquals("Rate", quickCalculator.RateLabelText);

			quickCalculator.QuantityDescription = "Charge Percentage";
			AssertEquals(0m, quickCalculator.Quantity);
			AssertEquals("%", quickCalculator.QuantityUnit);
			AssertEquals(false, quickCalculator.QuantityInfo.ReadOnly);
			AssertEquals(true, quickCalculator.IsPercentageCharge);
			AssertEquals(Env.Registry.FreightChargeCode, quickCalculator.ChargeCodePK);
			AssertEquals("Amount", quickCalculator.RateLabelText);
		}

		public void TestQuantityDescription_Package()
		{
			var rateableMeasureSet = new RateableMeasureSet();
			rateableMeasureSet.SetPackageCountWithEmptyContainerType(3);
			var quickCalculateRatingMock = new Moq.Mock<IQuickCalculateRating>();
			quickCalculateRatingMock.Setup(q => q.QuickMeasures).Returns(rateableMeasureSet);
			var quickCalculator = new JobChargeQuickCalculateBusinessObject(quickCalculateRatingMock.Object, Charge);

			quickCalculator.QuantityDescription = "Package";

			AssertEquals(3m, quickCalculator.Quantity);
			Assert(true);
		}

		public void TestUpdateSell_ChargeIsDisbursement_ControlsShouldBeReadonly()
		{
			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_OSSellAmt = 100m;
			charge.JR_OSCostAmt = 65m;
			charge.JR_ChargeType = Core.Constants.ChargeType.Margin;
			charge.JR_CostRatingOverride = false;

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);
			quickCalculator.IsMinimum = true;

			AssertEquals("Is UpdateSell readonly", false, quickCalculator.UpdateSellInfo.ReadOnly);
			AssertEquals("UpdateSell value", true, quickCalculator.UpdateSell);
			AssertEquals("Is SellRate readonly", false, quickCalculator.SellRateInfo.ReadOnly);
			AssertEquals("Is SellTotal readonly", false, quickCalculator.SellTotalInfo.ReadOnly);

			charge.JR_ChargeType = Constants.ChargeType.Disbursement;
			quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);
			AssertEquals("Is UpdateSell readonly", true, quickCalculator.UpdateSellInfo.ReadOnly);
			AssertEquals("UpdateSell value", false, quickCalculator.UpdateSell);
			AssertEquals("Is SellRate readonly", true, quickCalculator.SellRateInfo.ReadOnly);
			AssertEquals("Is SellTotal readonly", true, quickCalculator.SellTotalInfo.ReadOnly);
		}

		public void TestUpdateCost_ChargeIsRevenue_ControlsShouldBeReadonly()
		{
			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_OSSellAmt = 50m;
			charge.JR_OSCostAmt = 25m;
			charge.JR_ChargeType = Core.Constants.ChargeType.Margin;
			charge.JR_SellRatingOverride = false;

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);
			quickCalculator.IsMinimum = true;

			AssertEquals("Is UpdateCost readonly", false, quickCalculator.UpdateCostInfo.ReadOnly);
			AssertEquals("UpdateCost value", true, quickCalculator.UpdateCost);
			AssertEquals("Is CostRate readonly", false, quickCalculator.CostRateInfo.ReadOnly);
			AssertEquals("Is CostTotal readonly", false, quickCalculator.CostTotalInfo.ReadOnly);

			charge.JR_ChargeType = Constants.ChargeType.Revenue;
			quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);
			AssertEquals("Is UpdateCost readonly", true, quickCalculator.UpdateCostInfo.ReadOnly);
			AssertEquals("UpdateCost value", false, quickCalculator.UpdateCost);
			AssertEquals("Is CostRate readonly", true, quickCalculator.CostRateInfo.ReadOnly);
			AssertEquals("Is CostTotal readonly", true, quickCalculator.CostTotalInfo.ReadOnly);
		}

		#endregion

		#region Validation

		public void TestChargeCodePKValidation()
		{
			Charge existingCharge1 = Job.Charges.AddNew();
			existingCharge1.JR_AC = Env.Registry.FreightChargeCode;

			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			JobChargeQuickCalculateBusinessObject quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), Charge);

			quickCalculator.QuantityDescription = "Charge Percentage";
			AssertNoErrors(quickCalculator.ChargeCodePKInfo);

			quickCalculator.ChargeCodePK = ZGuid.NewZGuid();
			AssertHasErrors(quickCalculator.ChargeCodePKInfo);

			quickCalculator.ChargeCodePK = ZGuid.Empty;
			AssertNoErrors(quickCalculator.ChargeCodePKInfo);

			quickCalculator.ChargeCodePK = Env.Registry.FreightChargeCode;
			AssertNoErrors(quickCalculator.ChargeCodePKInfo);

			quickCalculator.ChargeCodePK = chargeCode.PK;
			AssertHasErrors(quickCalculator.ChargeCodePKInfo);
		}

		public void TestApportionedCostAndRevenueValidation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsCreditor = true;

			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001");
			var shipment = consol.Shipments.AddNew();
			var testJob = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var consolCost = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC2, org);
			AssertEquals(1, consolCost.ApportionmentCharges.Count);
			var apportionedCharge = Factory.Load<Charge>(consolCost.ApportionmentCharges[0].PK);
			Assert(apportionedCharge.JR_IsApportioned);
			apportionedCharge.JR_ChargeType = Core.Constants.ChargeType.Disbursement;

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), apportionedCharge);
			Assert("Update Cost Should not be ticked", !quickCalculator.UpdateCost);
			Assert("Update Cost Should be Read Only", quickCalculator.UpdateCostInfo.ReadOnly);
			Assert("Update Cost should have warnings", quickCalculator.UpdateCostInfo.HasWarnings());
			Assert("Cost Rate Should be Read Only", quickCalculator.CostRateInfo.ReadOnly);
			Assert("Cost Total should be Read Only", quickCalculator.CostTotalInfo.ReadOnly);

			Assert(!quickCalculator.UpdateSell);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), Charge);
		}

		protected override void SetUp()
		{
			base.SetUp();
			AccChargeCode freight = Factory.NewWithValidTestData<AccChargeCode>();
			freight.AC_Code = "FRT";
			freight.AC_Desc = "International Freight";
			freight.AC_GC = GlbCompany.CurrentCompany.PK;
			Env.Registry.FreightChargeCode = freight.PK.ToGuid();
		}

		Job Job
		{
			get { return job ?? (job = Factory.NewJobForTesting<Job>()); }
		}
		Job job;

		Charge Charge
		{
			get { return charge ?? (charge = Job.Charges.AddNew()); }
		}
		Charge charge;

		#endregion
	}

	[TestedType(typeof(ContainerCalculationData))]
	class ContainerCalculationDataBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ContainerCalculationData(new ContainerCalculationDataCollection(null));
		}
	}

	[TestedType(typeof(ContainerCalculationDataCollection))]
	class ContainerCalculationDataCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ContainerCalculationDataCollection>
	{
		protected override ContainerCalculationDataCollection GetCollectionToTest()
		{
			return new ContainerCalculationDataCollection(null);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ContainerCalculationData(GetCollectionToTest());
		}
	}
}
