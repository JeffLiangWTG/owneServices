using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class QuarantineExDocEstablishmentAndTimeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckEE_TreatmentDuration_ProduceType()
		{
			TestPropertyForProduceType("Treatment Duration", process.EE_TreatmentDurationInfo, true);
		}

		public void TestCheckEE_TreatmentDuration_WithUQ()
		{
			TestPropertyValue_WithUQ("Treatment Duration", "Treatment Duration UQ", process.EE_TreatmentDurationInfo, process.EE_TreatmentDurationUQInfo);
		}

		public void TestCheckEE_TreatmentDuration_NonNegative()
		{
			const string messageError = "Treatment Duration cannot be negative.";
			process.EE_TreatmentDuration = -1;
			process.Validation.ValidateEE_TreatmentDuration();
			CombineAssertions(() =>
			{
				AssertHasError("Negative value", process.EE_TreatmentDurationInfo, messageError);
				process.Validation.ValidateEE_TreatmentDuration();
				process.EE_TreatmentDuration = 1;
				AssertNoError("Non negative value", process.EE_TreatmentDurationInfo, messageError);
			});
		}

		public void TestCheckEE_TreatmentDurationUQ_ProduceType()
		{
			TestPropertyForProduceType("Treatment Duration UQ", process.EE_TreatmentDurationUQInfo, false);
		}

		public void TestCheckEE_TreatmentDurationUQ_NotInList()
		{
			TestPropertyNotInList("SEC", process.EE_TreatmentDurationUQInfo);
		}

		public void TestCheckEE_TreatmentConcentration_ProduceType()
		{
			TestPropertyForProduceType("Treatment Concentration", process.EE_TreatmentConcentrationInfo, true);
		}

		public void TestCheckEE_TreatmentConcentration_WithUQ()
		{
			TestPropertyValue_WithUQ("Treatment Concentration", "Treatment Concentration UQ", process.EE_TreatmentConcentrationInfo, process.EE_TreatmentConcentrationUQInfo);
		}

		public void TestCheckEE_TreatmentConcentration_NonNegative()
		{
			const string messageError = "Treatment Concentration cannot be negative.";
			process.EE_TreatmentConcentration = -1;
			process.Validation.ValidateEE_TreatmentConcentration();
			CombineAssertions(() =>
			{
				AssertHasError("Negative value", process.EE_TreatmentConcentrationInfo, messageError);
				process.Validation.ValidateEE_TreatmentConcentration();
				process.EE_TreatmentConcentration = 1;
				AssertNoError("Non negative value", process.EE_TreatmentConcentrationInfo, messageError);
			});
		}

		public void TestCheckEE_TreatmentConcentrationUQ_ProduceType()
		{
			TestPropertyForProduceType("Treatment Concentration UQ", process.EE_TreatmentConcentrationUQInfo, false);
		}

		public void TestCheckEE_TreatmentConcentrationUQ_NotInList()
		{
			var concentrationUqList = new CodeDescriptionPairList();
			concentrationUqList.AddPair("VAL", "Desc");
			var processMock = Factory.NewMoq<QuarantineExDocEstablishmentAndTime>();
			var lookupMock = new Mock<QuarantineExDocEstablishmentAndTimeLookups>(processMock.Object);
			lookupMock.SetupGet(x => x.TreatmentConcentrationUQ).Returns(concentrationUqList);
			processMock.Protected().Setup<QuarantineExDocEstablishmentAndTimeLookups>("GetNewLookups").Returns(lookupMock.Object);
			TestPropertyNotInList("VAL", processMock.Object.EE_TreatmentConcentrationUQInfo);
		}

		public void TestCheckEE_TreatmentTemperature_ProduceType()
		{
			TestPropertyForProduceType("Treatment Temperature", process.EE_TreatmentTemperatureInfo, true);
		}

		public void TestCheckEE_TreatmentTemperature_WithUQ()
		{
			TestPropertyValue_WithUQ("Treatment Temperature", "Treatment Temperature UQ", process.EE_TreatmentTemperatureInfo, process.EE_TreatmentTemperatureUQInfo, false);
		}

		public void TestCheckEE_TreatmentTemperatureUQ_ProduceType()
		{
			TestPropertyForProduceType("Treatment Temperature UQ", process.EE_TreatmentTemperatureUQInfo, false);
		}

		public void TestCheckEE_TreatmentTemperatureUQ_NotInList()
		{
			TestPropertyNotInList("CEL", process.EE_TreatmentTemperatureUQInfo);
		}

		[TestDate(2004, 12, 12)]
		public void TestCheckEE_EndDate()
		{
			Assert("Pre-Condition", !process.EE_EndDateInfo.HasMessageErrors());
			process.EE_StartDate = new ZDateTime(2004, 12, 12);
			process.EE_EndDate = ZDateTime.Empty;
			Assert("End Date is not set, start date is, invalid", !process.EE_EndDateInfo.HasMessageErrors());
			process.EE_EndDate = new ZDateTime(2004, 11, 11);
			Assert("End Date is set earlier then start date, invalid", process.EE_EndDateInfo.HasMessageErrors());
			process.EE_EndDate = new ZDateTime(2004, 12, 13);
			Assert("End Date is set, start date is set, valid", !process.EE_EndDateInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Packing;
			process.QuarantineExDocHeader.QH_InspectionRequestedDate = new ZDateTime(2004, 1, 8);
			process.EE_StartDate = new ZDateTime(2004, 1, 5);
			process.EE_EndDate = new ZDateTime(2004, 1, 9);
			Assert("End Date is set larger than authorised start date, invalid", process.EE_EndDateInfo.HasMessageErrors());
			process.EE_EndDate = new ZDateTime(2004, 1, 8);
			Assert("End Date is set equal to authorised start date, invalid", !process.EE_EndDateInfo.HasMessageErrors());
			process.EE_EndDate = new ZDateTime(2004, 1, 7);
			Assert("End Date is set less than authorised start date, invalid", !process.EE_EndDateInfo.HasMessageErrors());
			process.EE_EndDate = new ZDateTime(2004, 12, 13);
			Assert("End Date is set greater than authorised end date, invalid", process.EE_EndDateInfo.HasMessageErrors());
		}

		public void TestCheckEE_EndDate_Nexdocs()
		{
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			Assert("Pre-Condition", eXDOCHeader.IsNEXDOCSActive);
			Assert("Pre-Condition", !process.EE_EndDateInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			process.EE_StartDate = new ZDateTime(2004, 1, 5);
			process.EE_HarvestArea = string.Empty;
			process.Validation.ValidateEE_EndDate();
			Assert("Has message error", process.EE_EndDateInfo.HasMessageErrors());
			process.EE_HarvestArea = "1234";
			process.Validation.ValidateEE_EndDate();
			Assert("No message error", !process.EE_EndDateInfo.HasMessageErrors());
		}

		public void TestCheckEE_StartDate()
		{
			Assert("Pre-Condition", !process.EE_StartDateInfo.HasMessageErrors());
			process.EE_StartDate = ZDateTime.Empty;
			AssertHasMessageError(process.EE_StartDateInfo, "Process start date must be entered.");
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherVessel;
			process.EE_E2_Address = address.PK;
			process.Validation.ValidateEE_StartDate();
			AssertNoMessageError(process.EE_StartDateInfo, "Process start date must be entered.");
			process.EE_EndDate = new ZDateTime(2004, 12, 12);
			process.Validation.ValidateEE_StartDate();
			AssertHasMessageError(process.EE_StartDateInfo, "Process start date is required when process end date is entered.");
			process.EE_StartDate = new ZDateTime(2004, 12, 13);
			AssertHasMessageError(process.EE_StartDateInfo, "Process start date must be less than or equal to process end date.");
			process.EE_StartDate = new ZDateTime(2004, 11, 11);
			Assert("End Date is set, start date is set, valid", !process.EE_StartDateInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Treatment;
			process.EE_StartDate = ZDateTime.Now.AddDays(1);
			AssertHasMessageError(process.EE_StartDateInfo, "Process start date cannot be greater than today.");
		}

		public void TestCheckEE_ProcessingTypeForHorticulture()
		{
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			AssertEquals(false, UniversalReferenceHelper.Errata53Enabled());

			QuarantineExDocEstablishmentAndTime process2 = eXDOCLine.Processes.AddNew();
			Assert("Pre-Condition", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Pre-Condition", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Treatment;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Treatment;
			Assert("First treatment record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second treatment record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			Assert("First harvest record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second harvest record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherVessel;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherVessel;
			Assert("First catcher record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second catcher record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.AquacultureFarm;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.AquacultureFarm;
			Assert("First aquaculture farm record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second aquaculture farm record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Freezing;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Freezing;
			Assert("First freezing record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second freezing record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			Assert("First processing record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second processing record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Packing;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Packing;
			Assert("First packing record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second packing record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Slaughter;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Slaughter;
			Assert("First slaughter record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second slaughter record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Storage;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Storage;
			Assert("First storage record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second storage record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata53_1, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				AssertEquals(true, UniversalReferenceHelper.Errata53Enabled());

				process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Storage;
				process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Storage;
				Assert("First storage record", process.EE_ProcessingTypeInfo.HasMessageErrors());
				Assert("Second storage record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
				AssertHasMessageErrorContaining(process.EE_ProcessingTypeInfo, "Processing Type of ST must not be present when Produce Type is Horticulture or Grains and Seeds");
				AssertHasMessageErrorContaining(process2.EE_ProcessingTypeInfo, "Processing Type of ST must not be present when Produce Type is Horticulture or Grains and Seeds");
			}
		}

		public void TestCheckEE_ProcessingTypeForGrainsAndPlants()
		{
			AssertEquals(false, UniversalReferenceHelper.Errata53Enabled());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;

			QuarantineExDocEstablishmentAndTime process2 = eXDOCLine.Processes.AddNew();
			Assert("Pre-Condition", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Pre-Condition", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Treatment;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Treatment;
			Assert("First treatment record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second treatment record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			Assert("First harvest record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second harvest record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherVessel;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherVessel;
			Assert("First catcher record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second catcher record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.AquacultureFarm;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.AquacultureFarm;
			Assert("First aquaculture farm record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second aquaculture farm record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Freezing;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Freezing;
			Assert("First freezing record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second freezing record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			Assert("First processing record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second processing record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Packing;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Packing;
			Assert("First packing record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second packing record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Slaughter;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Slaughter;
			Assert("First slaughter record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second slaughter record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Storage;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Storage;
			Assert("First storage record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second storage record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata53_1, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				AssertEquals(true, UniversalReferenceHelper.Errata53Enabled());

				process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Storage;
				process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Storage;
				Assert("First storage record", process.EE_ProcessingTypeInfo.HasMessageErrors());
				Assert("Second storage record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
				AssertHasMessageErrorContaining(process.EE_ProcessingTypeInfo, "Processing Type of ST must not be present when Produce Type is Horticulture or Grains and Seeds");
				AssertHasMessageErrorContaining(process2.EE_ProcessingTypeInfo, "Processing Type of ST must not be present when Produce Type is Horticulture or Grains and Seeds");
			}
		}

		public void TestCheckEE_ProcessingTypeForDairy()
		{
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			QuarantineExDocEstablishmentAndTime process2 = eXDOCLine.Processes.AddNew();
			Assert("Pre-Condition", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Pre-Condition", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Treatment;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Treatment;
			Assert("First treatment record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second treatment record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			Assert("First harvest record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second harvest record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Product has error requiring Processing value", eXDOCLine.QL_ProduceTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherVessel;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherVessel;
			Assert("First catcher record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second catcher record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.AquacultureFarm;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.AquacultureFarm;
			Assert("First aquaculture farm record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second aquaculture farm record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Freezing;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Freezing;
			Assert("First freezing record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second freezing record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			Assert("First processing record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second processing record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Packing;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Packing;
			Assert("First packing record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second packing record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Slaughter;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Slaughter;
			Assert("First slaughter record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second slaughter record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Storage;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Storage;
			Assert("First storage record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second storage record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
		}

		public void TestCheckEE_ProcessingTypeForFish()
		{
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			QuarantineExDocEstablishmentAndTime process2 = eXDOCLine.Processes.AddNew();
			Assert("Pre-Condition", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Pre-Condition", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Treatment;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Treatment;
			Assert("First treatment record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second treatment record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			Assert("First harvest record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second harvest record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherVessel;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherVessel;
			Assert("First catcher record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second catcher record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.AquacultureFarm;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.AquacultureFarm;
			Assert("First aquaculture farm record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second aquaculture farm record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Freezing;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Freezing;
			Assert("First freezing record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second freezing record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			Assert("First processing record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second processing record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Packing;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Packing;
			Assert("First packing record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second packing record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Slaughter;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Slaughter;
			Assert("First slaughter record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second slaughter record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Product has error requiring Processing value", eXDOCLine.QL_ProduceTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Storage;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Storage;
			Assert("First storage record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second storage record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
		}

		public void TestCheckEE_ProcessingTypeForMeat()
		{
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			QuarantineExDocEstablishmentAndTime process2 = eXDOCLine.Processes.AddNew();
			Assert("Pre-Condition", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Pre-Condition", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Treatment;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Treatment;
			Assert("First treatment record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second treatment record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			Assert("First harvest record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second harvest record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Product has error requiring Processing value", eXDOCLine.QL_ProduceTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherVessel;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherVessel;
			Assert("First catcher record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second catcher record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.AquacultureFarm;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.AquacultureFarm;
			Assert("First aquaculture farm record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second aquaculture farm record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Freezing;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Freezing;
			Assert("First freezing record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second freezing record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			Assert("First processing record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second processing record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Packing;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Packing;
			Assert("First packing record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second packing record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Slaughter;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Slaughter;
			Assert("First slaughter record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second slaughter record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Storage;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Storage;
			Assert("First storage record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second storage record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
		}

		public void TestCheckEE_ProcessingTypeForSkinsAndHides()
		{
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
			QuarantineExDocEstablishmentAndTime process2 = eXDOCLine.Processes.AddNew();
			Assert("Pre-Condition", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Pre-Condition", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Treatment;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Treatment;
			Assert("First treatment record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second treatment record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			Assert("First harvest record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second harvest record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherVessel;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherVessel;
			Assert("First catcher record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second catcher record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.AquacultureFarm;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.AquacultureFarm;
			Assert("First aquaculture farm record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second aquaculture farm record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Freezing;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Freezing;
			Assert("First freezing record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second freezing record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			Assert("First processing record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second processing record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Packing;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Packing;
			Assert("First packing record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second packing record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Slaughter;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Slaughter;
			Assert("First slaughter record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second slaughter record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Storage;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Storage;
			Assert("First storage record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second storage record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
		}

		public void TestCheckEE_ProcessingTypeForWool()
		{
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
			QuarantineExDocEstablishmentAndTime process2 = eXDOCLine.Processes.AddNew();
			Assert("Pre-Condition", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Pre-Condition", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Treatment;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Treatment;
			Assert("First treatment record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second treatment record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			Assert("First harvest record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second harvest record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherVessel;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherVessel;
			Assert("First catcher record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second catcher record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.AquacultureFarm;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.AquacultureFarm;
			Assert("First aquaculture farm record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second aquaculture farm record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Freezing;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Freezing;
			Assert("First freezing record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second freezing record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			Assert("First processing record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second processing record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Packing;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Packing;
			Assert("First packing record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second packing record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Slaughter;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Slaughter;
			Assert("First slaughter record", process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second slaughter record", process2.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Storage;
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Storage;
			Assert("First storage record", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			Assert("Second storage record", !process2.EE_ProcessingTypeInfo.HasMessageErrors());
		}

		public void TestCheckEE_AuthorisationEstablishmentID()
		{
			Assert("Pre-Condition", !process.EE_AuthorisationEstablishmentIDInfo.HasMessageErrors());
			process.EE_E2_Address = ZGuid.Empty;
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			process.EE_AuthorisationEstablishmentID = ZString.Empty;
			AssertHasMessageError(process.EE_AuthorisationEstablishmentIDInfo, "Process establishment address or ID must be entered");
			process.EE_AuthorisationEstablishmentID = "14";
			AssertNoMessageError(process.EE_AuthorisationEstablishmentIDInfo, "Process establishment address or ID must be entered");
			AssertNoMessageError(process.EE_AuthorisationEstablishmentIDInfo, "Process establishment ID must not be entered");
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherVessel;
			process.Validation.ValidateEE_AuthorisationEstablishmentID();
			AssertHasMessageError(process.EE_AuthorisationEstablishmentIDInfo, "Process establishment ID must not be entered");
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.AquacultureFarm;
			process.Validation.ValidateEE_AuthorisationEstablishmentID();
			AssertHasMessageError(process.EE_AuthorisationEstablishmentIDInfo, "Process establishment ID must not be entered");
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			process.EE_AuthorisationEstablishmentID = ZString.Empty;
			AssertNoMessageError(process.EE_AuthorisationEstablishmentIDInfo, "Process establishment address or ID must be entered");
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Treatment;
			process.Validation.ValidateEE_AuthorisationEstablishmentID();
			AssertNoMessageError(process.EE_AuthorisationEstablishmentIDInfo, "Process establishment address or ID must be entered");
		}

		public void TestCheckEE_E2_Address()
		{
			var emptyAddress = Factory.NewWithValidTestData<JobDocAddress>();
			Assert("Pre-Condition", !process.EE_E2_AddressInfo.HasMessageErrors());
			process.EE_AuthorisationEstablishmentID = ZString.Empty;
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			process.EE_E2_Address = ZGuid.Empty;
			AssertHasMessageError(process.EE_E2_AddressInfo, "Process establishment address or ID must be entered");
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherVessel;
			process.Validation.ValidateEE_E2_Address();
			AssertHasMessageError(process.EE_E2_AddressInfo, "Process establishment address must be entered");
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.AquacultureFarm;
			process.Validation.ValidateEE_E2_Address();
			AssertHasMessageError(process.EE_E2_AddressInfo, "Process establishment address must be entered");
			process.EE_E2_Address = emptyAddress.PK;
			AssertNoMessageError(process.EE_E2_AddressInfo, "Process establishment address or ID must be entered");
			AssertHasMessageError(process.EE_E2_AddressInfo, "Process establishment address must be entered");
			process.EE_E2_Address = address.PK;
			AssertNoMessageError(process.EE_E2_AddressInfo, "Process establishment address must be entered");
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			process.EE_E2_Address = ZGuid.Empty;
			AssertNoMessageError(process.EE_E2_AddressInfo, "Process establishment address or ID must be entered");
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Treatment;
			process.Validation.ValidateEE_E2_Address();
			AssertNoMessageError(process.EE_E2_AddressInfo, "Process establishment address or ID must be entered");
		}

		public void TestCheckEE_Depuration()
		{
			Assert("Pre-Condition", !process.EE_DepurationInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			process.EE_Depuration = new ZDateTime(2006, 12, 12);
			Assert("Cannot have depuration date on non harvest process", process.EE_DepurationInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			process.EE_Depuration = new ZDateTime(2006, 12, 13);
			Assert("Can have depuration date on harvest process", !process.EE_DepurationInfo.HasMessageErrors());
			process.EE_AuthorisationEstablishmentID = "34";
			process.EE_Depuration = ZDateTime.Empty;
			Assert("Must have depuration date when plant is filled in", process.EE_DepurationInfo.HasMessageErrors());
		}

		public void TestCheckEE_HarvestArea()
		{
			Assert("Pre-Condition", !process.EE_HarvestAreaInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			process.EE_HarvestArea = "1234";
			Assert("Cannot have harvest area on non harvest process", process.EE_HarvestAreaInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			process.EE_HarvestArea = "143234";
			Assert("Can have harvest area on harvest process", !process.EE_HarvestAreaInfo.HasMessageErrors());
			process.EE_HarvestArea = ZString.Empty;
			Assert("Cannot have empty harvest area on harvest process", process.EE_HarvestAreaInfo.HasMessageErrors());
		}

		public void TestCheckEE_HarvestArea_Nexdocs()
		{
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			Assert("Pre-Condition", !process.EE_HarvestAreaInfo.HasMessageErrors());
			Assert("Pre-Condition", eXDOCHeader.IsNEXDOCSActive);
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			process.EE_HarvestArea = "1234";
			Assert("Cannot have harvest area on non harvest process", process.EE_HarvestAreaInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			process.EE_HarvestArea = "143234";
			Assert("Can have harvest area on harvest process", !process.EE_HarvestAreaInfo.HasMessageErrors());
			process.EE_HarvestArea = ZString.Empty;
			Assert("Can have empty harvest area on harvest process", !process.EE_HarvestAreaInfo.HasMessageErrors());
		}

		public void TestCheckEE_LeaseNumber()
		{
			Assert("Pre-Condition", !process.EE_LeaseNumberInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			process.EE_LeaseNumber = "1234";
			Assert("Cannot have harvest number on non harvest process", process.EE_LeaseNumberInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			process.EE_LeaseNumber = "143234";
			Assert("Can have harvest number on harvest process", !process.EE_LeaseNumberInfo.HasMessageErrors());
		}

		public void TestFreezingAndPackingStartDates()
		{
			Assert("Pre-Condition", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Packing;
			process.EE_StartDate = ZDateTime.Now;
			QuarantineExDocEstablishmentAndTime process2 = eXDOCLine.Processes.AddNew();
			process2.EE_StartDate = ZDateTime.Now.AddDays(-1);
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Freezing;
			AssertHasMessageError("Freezing Process has message error", process2.EE_ProcessingTypeInfo, "Freezing start date must be on or after packing start date.");
			process2.EE_StartDate = ZDateTime.Now.AddDays(2);
			AssertNoMessageError(process.EE_ProcessingTypeInfo, "Freezing start date must be on or after packing start date.");
		}

		public void TestSlaughterAndPackingEndDates()
		{
			Assert("Pre-Condition", !process.EE_ProcessingTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Packing;
			process.EE_EndDate = ZDateTime.Now;
			QuarantineExDocEstablishmentAndTime process2 = eXDOCLine.Processes.AddNew();
			process2.EE_EndDate = ZDateTime.Now.AddDays(1);
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Slaughter;
			AssertHasMessageError("Slaughter Process has message error", process2.EE_ProcessingTypeInfo, "Slaughter end date must be before or equal to packing end date.");
			process2.EE_EndDate = ZDateTime.Now.AddDays(-1);
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Slaughter;
			AssertNoMessageError(process2.EE_ProcessingTypeInfo, "Slaughter end date must be before or equal to packing end date.");
		}

		public void TestCheckEE_TreatmentCode()
		{
			Assert("Pre-Condition", !process.EE_TreatmentCodeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Slaughter;
			process.EE_TreatmentCode = "Hardco";
			AssertHasMessageError("Slaughter Process cannot have treatment code", process.EE_TreatmentCodeInfo, "Treatment code can only be entered on a treatment process.");
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Treatment;
			process.EE_TreatmentCode = "Sexy";
			AssertNoMessageError(process.EE_TreatmentCodeInfo, "Treatment code can only be entered on a treatment process.");
			process.EE_TreatmentInfo = "Here is some info on the treatment we are about to deliver up your a hole";
			process.EE_TreatmentCode = ZString.Empty;
			AssertHasMessageError("Treatment code cannot be emtpy when treatment info is not empty", process.EE_TreatmentCodeInfo, "Treatment code can not be empty when treatment information is entered.");
			process.EE_TreatmentCode = "Whate";
			AssertNoMessageError(process.EE_TreatmentCodeInfo, "Treatment code can not be empty when treatment information is entered.");
		}

		public void TestCheckEE_TreatmentInfo()
		{
			Assert("Pre-Condition", !process.EE_TreatmentInfoInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Slaughter;
			process.EE_TreatmentInfo = "I have a problem with exchange stores";
			AssertHasMessageError("Slaughter Process cannot have treatment information", process.EE_TreatmentInfoInfo, "Treatment information can only be entered on a treatment process.");
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Treatment;
			process.EE_TreatmentInfo = "I have a problem with LT not listening to what I say";
			AssertNoMessageError(process.EE_TreatmentInfoInfo, "Treatment information can only be entered on a treatment process.");
			process.EE_TreatmentCode = "shit";
			process.EE_TreatmentInfo = ZString.Empty;
			AssertHasMessageError("Treatment Info cannot be empty when treatment code is not", process.EE_TreatmentInfoInfo, "Treatment information can not be empty when treatment code is entered.");
			process.EE_TreatmentInfo = "blah some crap really tired of this";
			AssertNoMessageError(process.EE_TreatmentInfoInfo, "Treatment information can not be empty when treatment code is entered.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			eXDOCHeader = invoiceHeader.QuarantineExDocHeader;
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			eXDOCLine = invoiceLine.QuarantineExDocLine;
			process = eXDOCLine.Processes.AddNew();
			address = Factory.New<JobDocAddress>();
			address.E2_AddressOverride = true;
			address.E2_CompanyName = "Company";
		}

		void TestPropertyNotInList(string valueFromList, ZPropertyInfo propertyInfo)
		{
			const string messageError = "The code you have selected is not in the list.";
			propertyInfo.SetValueFromString("123");
			process.Validation.ValidateAll();
			CombineAssertions(() =>
			{
				AssertHasMessageError("Value not from the list", propertyInfo, messageError);
				propertyInfo.SetValueFromString(valueFromList);
				process.Validation.ValidateAll();
				AssertNoMessageError("Value from the list", propertyInfo, messageError);
			});
		}

		void TestPropertyValue_WithUQ(string valuePropertyName, string uqPropertyName, ZPropertyInfo valuePropertyInfo, ZPropertyInfo uqPropertyInfo, bool checkValue = true)
		{
			var uqMessageError = $"Please enter a {uqPropertyName} when {valuePropertyName} is entered.";
			var valueMessageError = $"Please enter a {valuePropertyName} when {uqPropertyName} is entered.";
			CombineAssertions(() =>
			{
				if (checkValue)
				{
					uqPropertyInfo.SetValueFromString("UQ");
					process.Validation.ValidateAll();
					AssertHasError($"{uqPropertyName} is not empty, {valuePropertyName} is empty", valuePropertyInfo, valueMessageError);
				}

				uqPropertyInfo.SetValueFromString(string.Empty);
				valuePropertyInfo.SetValueFromString("10");
				process.Validation.ValidateAll();
				AssertHasError($"{uqPropertyName} is empty, {valuePropertyName} is not empty", uqPropertyInfo, uqMessageError);
				uqPropertyInfo.SetValueFromString(string.Empty);
				valuePropertyInfo.SetValueFromString("0");
				process.Validation.ValidateAll();
				AssertNoError("Both empty - uq check", uqPropertyInfo, uqMessageError);
				if (checkValue)
				{
					AssertNoError("Both empty - value check", valuePropertyInfo, valueMessageError);
				}

				uqPropertyInfo.SetValueFromString("UQ");
				valuePropertyInfo.SetValueFromString("10");
				process.Validation.ValidateAll();
				AssertNoError("Both not empty - uq check", uqPropertyInfo, uqMessageError);
				if (checkValue)
				{
					AssertNoError("Both not empty - value check", valuePropertyInfo, valueMessageError);
				}
			});
		}

		void TestPropertyForProduceType(string propertyName, ZPropertyInfo propertyInfo, bool isNumber)
		{
			var messageError = $"{propertyName} may only be present when Produce Type is Horticulture or Grains and Seeds";
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			process.Validation.ValidateAll();
			CombineAssertions(() =>
			{
				propertyInfo.SetValueFromString("5");
				AssertHasMessageError("Dairy produce type - not empty", propertyInfo, messageError);
				propertyInfo.SetValueFromString(isNumber ? "0" : "");
				process.Validation.ValidateAll();
				AssertNoMessageError("Dairy produce type - empty", propertyInfo, messageError);
				propertyInfo.SetValueFromString("5");
				eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
				process.Validation.ValidateAll();
				AssertNoMessageError("Grains produce type - not empty", propertyInfo, messageError);
			});
		}

		QuarantineExDocEstablishmentAndTime process;
		QuarantineExDocLine eXDOCLine;
		QuarantineExDocHeader eXDOCHeader;
		JobDocAddress address;
	}
}
