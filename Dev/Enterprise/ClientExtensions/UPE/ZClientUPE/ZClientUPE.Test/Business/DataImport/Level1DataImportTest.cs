using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.DataImport
{
	[TestedType(typeof(Level1DataImport))]
	public class Level1DataImportTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidateAll()
		{
			Level1DataImport.IsSurplus = true;
			Level1DataImport.PecentageOfDuplicateHAWBs = 20;
			Level1DataImport.FileName = "Test";
			Level1DataImport.ArrivalDate = ZDateTime.Now.AddDays(6);

			Level1DataImport.ValidateAll();
			AssertEquals(true, Level1DataImport.FlightNumberInfo.HasErrors());
			AssertEquals(true, Level1DataImport.ArrivalDateInfo.HasWarnings());
			AssertEquals(true, Level1DataImport.MasterBillInfo.HasErrors());
			AssertEquals(true, Level1DataImport.PortOfLoadingInfo.HasErrors());
			AssertEquals(true, Level1DataImport.PortOfDischargeInfo.HasErrors());
			AssertEquals(true, Level1DataImport.FlightNotInScheduleNoteInfo.HasErrors());
			AssertEquals(true, Level1DataImport.SurplusIndicatedNoteInfo.HasErrors());
			AssertEquals(true, Level1DataImport.DuplicateHAWBsNoteInfo.HasErrors());
			AssertEquals(true, Level1DataImport.UnmatchedFilenameNoteInfo.HasErrors());
			AssertEquals(true, Level1DataImport.ArrivalDateWarningNoteInfo.HasErrors());

			CusMAWB cusMAWB = Factory.New<CusMAWB>();
			cusMAWB.CM_MAWB = "08111111111";
			Factory.Save();

			Level1DataImport.FlightNumber = "QF123";
			Level1DataImport.MasterBill = "081-1111 1111";
			Level1DataImport.IsSurplus = false;

			Level1DataImport.ValidateAll();
			AssertEquals(true, Level1DataImport.MasterbillWarningNoteInfo.HasErrors());
		}

		public void TestLoadSummaryInformation()
		{
			Level1DataImport.LoadSummaryInformation = "TEST";
			AssertEquals("TEST", Level1DataImport.LoadSummaryInformation);
		}

		public void TestPortList()
		{
			AssertNotNull(Level1DataImport.PortList);
		}

		public void TestFlightNumberAndValidation()
		{
			AssertEquals("PreCondition", false, Level1DataImport.FlightNumberInfo.HasErrors());
			Level1DataImport.ValidateFlightNumber();
			AssertEquals(true, Level1DataImport.FlightNumberInfo.HasErrors());

			Level1DataImport.FlightNumber = "QF123";
			AssertEquals("081", Level1DataImport.MasterBill);
			AssertEquals("QF123", Level1DataImport.FlightNumber);
			AssertEquals(false, Level1DataImport.FlightNumberInfo.HasErrors());

			Level1DataImport.FlightNumber = "";
			AssertEquals(true, Level1DataImport.FlightNumberInfo.HasErrors());
			Level1DataImport.FlightNumber = "SQ111";
			AssertEquals(false, Level1DataImport.FlightNumberInfo.HasErrors());

			AssertEquals(CusMAWBSchema.CM_FlightNo.MaxLength, Level1DataImport.FlightNumberInfo.MaxLength);
		}

		public void TestFlightNumberAndValidationForManifest()
		{
			AssertEquals("PreCondition", false, Level1DataImportForManifest.FlightNumberInfo.HasErrors());
			Level1DataImportForManifest.ValidateFlightNumber();
			AssertHasErrorContaining(Level1DataImportForManifest.FlightNumberInfo, MandatoryValidation.MustBeEntered);
			Level1DataImportForManifest.FlightNumber = "QF123";
			AssertNoErrorContaining(Level1DataImportForManifest.FlightNumberInfo, MandatoryValidation.MustBeEntered);
			Level1DataImportForManifest.IsRoad = true;
			AssertEquals("ROAD", Level1DataImportForManifest.FlightNumber);
			Assert(Level1DataImportForManifest.FlightNumberInfo.ReadOnly);
		}

		public void TestDepartureDate()
		{
			AssertEquals("PreCondition", false, Level1DataImportForManifest.DepartureDateInfo.HasErrors());
			Level1DataImportForManifest.ValidateDepartureDate();
			AssertHasErrorContaining(Level1DataImportForManifest.DepartureDateInfo, MandatoryValidation.MustBeEntered);
			Level1DataImportForManifest.DepartureDate = ZDateTime.Today;
			AssertNoErrorContaining(Level1DataImportForManifest.DepartureDateInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCycleDetails()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				AssertEquals("PreCondition", false, Level1DataImportForManifest.CycleDateInfo.HasErrors());
				AssertEquals("PreCondition", false, Level1DataImportForManifest.CycleNumberInfo.HasErrors());
				Level1DataImportForManifest.ValidateCycleNumber();
				Level1DataImportForManifest.ValidateCycleDate();
				AssertNoErrorContaining(Level1DataImportForManifest.CycleDateInfo, MandatoryValidation.MustBeEntered);
				AssertNoErrorContaining(Level1DataImportForManifest.CycleNumberInfo, MandatoryValidation.MustBeEntered);
				Level1DataImportForManifest.PortOfLoading = "AUSYD";
				Level1DataImportForManifest.PortOfDischarge = "SGSIN";
				Level1DataImportForManifest.ValidateCycleNumber();
				Level1DataImportForManifest.ValidateCycleDate();
				AssertHasErrorContaining(Level1DataImportForManifest.CycleDateInfo, MandatoryValidation.MustBeEntered);
				AssertHasErrorContaining(Level1DataImportForManifest.CycleNumberInfo, MandatoryValidation.MustBeEntered);
				Level1DataImportForManifest.CycleDate = ZDateTime.Today;
				Level1DataImportForManifest.CycleNumber = "99";
				AssertHasErrorContaining(Level1DataImportForManifest.CycleNumberInfo, ListValidation.InvalidCodeError);
				Level1DataImportForManifest.CycleNumber = "1";
				AssertNoErrorContaining(Level1DataImportForManifest.CycleNumberInfo, ListValidation.InvalidCodeError);
				AssertNoErrorContaining(Level1DataImportForManifest.CycleDateInfo, MandatoryValidation.MustBeEntered);
				AssertNoErrorContaining(Level1DataImportForManifest.CycleNumberInfo, MandatoryValidation.MustBeEntered);
				AssertNoWarning(Level1DataImportForManifest.CycleDateInfo, "Cycle Date is not required for an export shipment");
				AssertNoWarning(Level1DataImportForManifest.CycleNumberInfo, "Cycle Number is not required for an export shipment");
				Level1DataImportForManifest.PortOfDischarge = "AUSYD";
				Level1DataImportForManifest.PortOfLoading = "SGSIN";
				Level1DataImportForManifest.ValidateCycleNumber();
				Level1DataImportForManifest.ValidateCycleDate();
				AssertHasWarning(Level1DataImportForManifest.CycleDateInfo, "Cycle Date is not required for an export shipment");
				AssertHasWarning(Level1DataImportForManifest.CycleNumberInfo, "Cycle Number is not required for an export shipment");
			}
		}

		[TestDate(2006, 01, 01)]
		public void TestValidateArrivalDateValidation()
		{
			AssertEquals("PreCondition", false, Level1DataImport.ArrivalDateInfo.HasErrors());
			Level1DataImport.ValidateArrivalDate();
			AssertEquals(true, Level1DataImport.ArrivalDateInfo.HasErrors());

			Level1DataImport.ArrivalDate = ZDateTime.Now;
			AssertEquals(false, Level1DataImport.ArrivalDateInfo.HasErrors());

			Level1DataImport.ArrivalDate = ZDateTime.Now.AddDays(-6);
			AssertEquals(true, Level1DataImport.ArrivalDateInfo.HasWarning("Selected date is not within 5 days of today."));
			Level1DataImport.ArrivalDate = ZDateTime.Now.AddDays(6);
			AssertEquals(true, Level1DataImport.ArrivalDateInfo.HasWarning("Selected date is not within 5 days of today."));

			Level1DataImport.ArrivalDate = ZDateTime.Now.AddDays(-5);
			AssertEquals(false, Level1DataImport.ArrivalDateInfo.HasWarnings());
			Level1DataImport.ArrivalDate = ZDateTime.Now.AddDays(5);
			AssertEquals(false, Level1DataImport.ArrivalDateInfo.HasWarnings());
		}

		public void TestPortOfLoadingValidation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertNoErrors("PreCondition", Level1DataImport.PortOfLoadingInfo);
				Level1DataImport.ValidatePortOfLoading();
				AssertHasErrors(Level1DataImport.PortOfLoadingInfo);

				Level1DataImport.PortOfLoading = "AUSYD";
				AssertNoErrors(Level1DataImport.PortOfLoadingInfo);

				Level1DataImport.PortOfLoading = ZString.Empty;
				AssertHasErrors(Level1DataImport.PortOfLoadingInfo);
			}
		}

		public void TestPortOfDischargeValidation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertNoErrors("PreCondition", Level1DataImport.PortOfDischargeInfo);
				Level1DataImport.ValidatePortOfDischarge();
				AssertHasErrors(Level1DataImport.PortOfDischargeInfo);

				Level1DataImport.PortOfDischarge = "AUSYD";
				AssertNoErrors(Level1DataImport.PortOfDischargeInfo);

				Level1DataImport.PortOfDischarge = ZString.Empty;
				AssertHasErrors(Level1DataImport.PortOfDischargeInfo);
			}
		}

		public void TestMasterBillAndValidation()
		{
			AssertEquals("PreCondition", false, Level1DataImport.MasterBillInfo.HasErrors());
			Level1DataImport.ValidateMasterBill();
			AssertEquals(true, Level1DataImport.MasterBillInfo.HasErrors());

			Level1DataImport.FlightNumber = "QF123";
			Level1DataImport.MasterBill = "081-1111 1111";
			AssertEquals(false, Level1DataImport.MasterBillInfo.HasErrors());
			AssertEquals(false, Level1DataImport.MasterBillInfo.HasWarnings());
			AssertEquals("08111111111", Level1DataImport.MasterBill);

			Level1DataImport.FlightNumber = "MH111";
			Level1DataImport.MasterBill = "08111111111";
			AssertEquals(true, Level1DataImport.MasterBillInfo.HasError("The MAWB may be incorrect. The airline prefix and flight number do not match."));

			Level1DataImport.MasterBill = "23211111112";
			AssertEquals(true, Level1DataImport.MasterBillInfo.HasError("Invalid check digit. The last digit should be '1'"));

			Level1DataImport.MasterBill = "232111111";
			AssertEquals(true, Level1DataImport.MasterBillInfo.HasError("The MAWB should contain 11 digits."));

			Level1DataImport.MasterBill = "XYZQQQQQQQQ";
			AssertEquals(true, Level1DataImport.MasterBillInfo.HasError("The MAWB can only contain numbers."));

			Level1DataImport.FlightNumber = "5X123";
			Level1DataImport.MasterBill = "406-1111 1111";
			AssertEquals(false, Level1DataImport.MasterBillInfo.HasErrors());
			AssertEquals(false, Level1DataImport.MasterBillInfo.HasWarnings());

			CusMAWB cusMAWB = Factory.New<CusMAWB>();
			cusMAWB.CM_MAWB = "40611111111";
			Factory.Save();

			Level1DataImport.ValidateMasterBill();
			AssertEquals(false, Level1DataImport.MasterBillInfo.HasErrors());
			AssertEquals(false, Level1DataImport.MasterBillInfo.HasWarnings());

			Level1DataImport.FlightNumber = "QF123";
			Level1DataImport.MasterBill = "406-1111 1111";
			AssertEquals("This Masterbill number is already in the database.", Level1DataImport.MasterBillInfo.GetWarnings().GetFirstMessage());

			Level1DataImport.IsSurplus = true;
			Level1DataImport.ValidateMasterBill();
			AssertEquals(false, Level1DataImport.MasterBillInfo.HasWarnings());
		}

		public void TestMasterBillAndValidationForManifest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.MasterBill;
			bill.ABL_BillNumber = "08121111112";
			Factory.Save();

			AssertEquals("PreCondition", false, Level1DataImportForManifest.MasterBillInfo.HasErrors());
			Level1DataImportForManifest.ValidateMasterBill();
			AssertHasErrorContaining(Level1DataImportForManifest.MasterBillInfo, MandatoryValidation.MustBeEntered);
			Level1DataImportForManifest.MasterBill = "08111111111";
			AssertNoErrorContaining(Level1DataImportForManifest.MasterBillInfo, MandatoryValidation.MustBeEntered);

			Level1DataImportForManifest.MasterBill = "23211111112";
			AssertHasErrorContaining(Level1DataImportForManifest.MasterBillInfo, "Invalid check digit. The last digit should be '1'");

			Level1DataImportForManifest.MasterBill = "232111111";
			AssertHasErrorContaining(Level1DataImportForManifest.MasterBillInfo, "The MAWB should contain 11 digits.");

			Level1DataImportForManifest.MasterBill = "XYZQQQQQQQQ";
			AssertHasErrorContaining(Level1DataImportForManifest.MasterBillInfo, "The MAWB can only contain numbers.");

			Level1DataImportForManifest.MasterBill = "08121111112";
			AssertHasWarning(Level1DataImportForManifest.MasterBillInfo, "This Masterbill number is already in the database.");

			UPEDataRegistry.Instance.AllowMultipleLevel1LoadsForMasterBill = false;
			Level1DataImportForManifest.ValidateMasterBill();
			AssertHasError(Level1DataImportForManifest.MasterBillInfo, "This Masterbill number is already in the database.");
			AssertNoWarnings(Level1DataImportForManifest.MasterBillInfo);

			bill.ABL_IsActive = false;
			Factory.Save();

			UPEDataRegistry.Instance.AllowMultipleLevel1LoadsForMasterBill = true;
			Level1DataImportForManifest.ValidateMasterBill();
			AssertNoErrors(Level1DataImportForManifest.MasterBillInfo);
			AssertNoWarnings(Level1DataImportForManifest.MasterBillInfo);
			UPEDataRegistry.Instance.AllowMultipleLevel1LoadsForMasterBill = false;
			Level1DataImportForManifest.ValidateMasterBill();
			AssertNoErrors(Level1DataImportForManifest.MasterBillInfo);
			AssertNoWarnings(Level1DataImportForManifest.MasterBillInfo);

			header.AMA_IsActive = false;
			bill.ABL_IsActive = true;
			Factory.Save();

			UPEDataRegistry.Instance.AllowMultipleLevel1LoadsForMasterBill = true;
			Level1DataImportForManifest.ValidateMasterBill();
			AssertNoErrors(Level1DataImportForManifest.MasterBillInfo);
			AssertNoWarnings(Level1DataImportForManifest.MasterBillInfo);
			UPEDataRegistry.Instance.AllowMultipleLevel1LoadsForMasterBill = false;
			Level1DataImportForManifest.ValidateMasterBill();
			AssertNoErrors(Level1DataImportForManifest.MasterBillInfo);
			AssertNoWarnings(Level1DataImportForManifest.MasterBillInfo);
		}

		public void TestSurplusIndicatedNoteValidation()
		{
			AssertEquals("PreCondition", false, Level1DataImport.SurplusIndicatedNoteInfo.HasErrors());
			Level1DataImport.ValidateSurplusIndicatedNote();
			AssertEquals(false, Level1DataImport.SurplusIndicatedNoteInfo.HasErrors());

			Level1DataImport.IsSurplus = true;
			Level1DataImport.ValidateSurplusIndicatedNote();
			AssertEquals(true, Level1DataImport.SurplusIndicatedNoteInfo.HasErrors());
		}

		public void TestFlightNotInScheduleNoteValidation()
		{
			AssertEquals("PreCondition", false, Level1DataImport.FlightNotInScheduleNoteInfo.HasErrors());
			Level1DataImport.ValidateFlightNotInScheduleNote();
			AssertEquals(true, Level1DataImport.FlightNotInScheduleNoteInfo.HasErrors());

			Level1DataImport.FlightNumber = "SQ123";
			Level1DataImport.ArrivalDate = ZDateTime.Now;
			Level1DataImport.ValidateFlightNotInScheduleNote();
			AssertEquals(true, Level1DataImport.FlightNotInScheduleNoteInfo.HasErrors());

			JobVoyage jobVoyage = Factory.New<JobVoyage>();
			jobVoyage.JV_VoyageFlight = "SQ123";
			Factory.Save();
			Level1DataImport.ValidateFlightNotInScheduleNote();
			AssertEquals(true, Level1DataImport.FlightNotInScheduleNoteInfo.HasErrors());

			jobVoyage.Destinations.AddNew();
			jobVoyage.Destinations[0].JB_E_ARV = Level1DataImport.ArrivalDate;
			Factory.Save();
			Level1DataImport.ValidateFlightNotInScheduleNote();
			AssertEquals(false, Level1DataImport.FlightNotInScheduleNoteInfo.HasErrors());
		}

		public void TestTrySetOriginDestinationDefaults()
		{
			JobVoyage jobVoyage = Factory.New<JobVoyage>();
			jobVoyage.JV_VoyageFlight = "SQ123";
			jobVoyage.Destinations.AddNew();
			jobVoyage.Destinations[0].JB_E_ARV = ZDateTime.Now;
			jobVoyage.Destinations[0].JB_RL_NKPortOfDischarge = "AUSYD";
			jobVoyage.Origins.AddNew();
			jobVoyage.Origins[0].JA_RL_NKPortOfLoading = "SGSIN";
			Factory.Save();

			Assert(Level1DataImport.PortOfLoading.IsEmpty);
			Assert(Level1DataImport.PortOfDischarge.IsEmpty);

			Level1DataImport.FlightNumber = "SQ123";

			Assert(Level1DataImport.PortOfLoading.IsEmpty);
			Assert(Level1DataImport.PortOfDischarge.IsEmpty);

			Level1DataImport.ArrivalDate = jobVoyage.Destinations[0].JB_E_ARV.AddDays(2);

			Assert(Level1DataImport.PortOfLoading.IsEmpty);
			Assert(Level1DataImport.PortOfDischarge.IsEmpty);

			Level1DataImport.FlightNumber = "QF123";
			Level1DataImport.ArrivalDate = jobVoyage.Destinations[0].JB_E_ARV;

			Assert(Level1DataImport.PortOfLoading.IsEmpty);
			Assert(Level1DataImport.PortOfDischarge.IsEmpty);

			Level1DataImport.FlightNumber = "SQ123";

			AssertEquals("SGSIN", Level1DataImport.PortOfLoading);
			AssertEquals("AUSYD", Level1DataImport.PortOfDischarge);
		}

		public void TestValidateDuplicateHAWBsNote()
		{
			AssertEquals("PreCondition", false, Level1DataImport.DuplicateHAWBsNoteInfo.HasErrors());
			Level1DataImport.ValidateDuplicateHAWBsNote();
			AssertEquals(false, Level1DataImport.DuplicateHAWBsNoteInfo.HasErrors());

			Level1DataImport.PecentageOfDuplicateHAWBs = 15;
			Level1DataImport.ValidateDuplicateHAWBsNote();
			AssertEquals(false, Level1DataImport.DuplicateHAWBsNoteInfo.HasErrors());

			Level1DataImport.PecentageOfDuplicateHAWBs = 16;
			Level1DataImport.ValidateDuplicateHAWBsNote();
			AssertEquals(true, Level1DataImport.DuplicateHAWBsNoteInfo.HasError("Please indicate why you are proceeding with the import when the number of duplicate HAWBs is greater than 15% of the total."));

			Level1DataImport.DuplicateHAWBsNote = "TEST";
			Level1DataImport.ValidateDuplicateHAWBsNote();
			AssertEquals(false, Level1DataImport.DuplicateHAWBsNoteInfo.HasErrors());
		}

		public void TestAirlinePrefix()
		{
			var dummyAirLine = Factory.NewWithValidTestData<RefAirline>(TestBusinessObjectKind.MinimumRequiredToSave);
			dummyAirLine.RM_AirlinePrefix = "699";
			dummyAirLine.RM_EagleAddedAirlinePrefixOrAccountingCode = "519";
			dummyAirLine.RM_TwoCharacterCode = "XU";

			Level1DataImport.FlightNumber = "XU123";
			var airLinePrefix = Level1DataImport.MasterBill.Left(3);
			Assert(ZString.Format("MAWB prefix should be '519' but was '{0}'", airLinePrefix), Level1DataImport.MasterBill.StartsWith(dummyAirLine.RM_EagleAddedAirlinePrefixOrAccountingCode));
		}

		public void TestPecentageOfDuplicateHAWBsNote()
		{
			Level1DataImport.PecentageOfDuplicateHAWBs = 10;
			AssertEquals(10, Level1DataImport.PecentageOfDuplicateHAWBs);
		}

		public void TestFileName()
		{
			Level1DataImport.FileName = "Test";
			AssertEquals("Test", Level1DataImport.FileName);
		}

		public void TestValidateUnmatchedFilenameNote()
		{
			AssertValidateUnmatchedFilenameNote("", true);
			PopulateRefUNLOCOMap();
			AssertValidateUnmatchedFilenameNote("USLAX", true);
			AssertValidateUnmatchedFilenameNote("AUBNE", true);
			AssertValidateUnmatchedFilenameNote("AUSYD", false);

			Level1DataImport.FileName = @"\SomePath\AU9639.txt";
			Level1DataImport.ValidateUnmatchedFilenameNote();
			AssertEquals(false, Level1DataImport.UnmatchedFilenameNoteInfo.HasErrors());
		}

		void AssertValidateUnmatchedFilenameNote(ZString portOfDischargeCode, bool invalidPortCode)
		{
			if (!portOfDischargeCode.IsEmpty)
			{
				Level1DataImport.PortOfDischarge = portOfDischargeCode;
			}
			Level1DataImport.FileName = "";
			Level1DataImport.ValidateUnmatchedFilenameNote();
			AssertEquals(false, Level1DataImport.UnmatchedFilenameNoteInfo.HasErrors());

			Level1DataImport.FileName = "Test.dat";
			Level1DataImport.ValidateUnmatchedFilenameNote();
			AssertEquals("The Filename of the file that you are loading, 'Test' does not match the Port of Discharge, '" + portOfDischargeCode + "'", Level1DataImport.UnmatchedFilenameNoteInfo.GetErrors().GetFirstMessage());

			Level1DataImport.FileName = "US9639.txt";
			Level1DataImport.ValidateUnmatchedFilenameNote();
			AssertEquals("The Filename of the file that you are loading, 'US9639' does not match the Port of Discharge, '" + portOfDischargeCode + "'", Level1DataImport.UnmatchedFilenameNoteInfo.GetErrors().GetFirstMessage());

			Level1DataImport.FileName = "US.txt";
			Level1DataImport.ValidateUnmatchedFilenameNote();
			AssertEquals("The Filename of the file that you are loading, 'US' does not match the Port of Discharge, '" + portOfDischargeCode + "'", Level1DataImport.UnmatchedFilenameNoteInfo.GetErrors().GetFirstMessage());

			Level1DataImport.UnmatchedFilenameNote = "SomeNote";
			Level1DataImport.ValidateUnmatchedFilenameNote();
			AssertEquals(false, Level1DataImport.UnmatchedFilenameNoteInfo.HasErrors());

			if (invalidPortCode)
			{
				Level1DataImport.UnmatchedFilenameNote = "";
				Level1DataImport.FileName = "AU9639.txt";
				Level1DataImport.ValidateUnmatchedFilenameNote();
				AssertEquals("The Filename of the file that you are loading, 'AU9639' does not match the Port of Discharge, '" + portOfDischargeCode + "'", Level1DataImport.UnmatchedFilenameNoteInfo.GetErrors().GetFirstMessage());
			}
		}

		void PopulateRefUNLOCOMap()
		{
			MasterFiles.Business.RefLocoMap sYDLocoMap = Factory.New<MasterFiles.Business.RefLocoMap>();
			sYDLocoMap.RY_LocalPortCode = "9639";
			sYDLocoMap.RY_RL_NKLocoPort = "AUSYD";
			sYDLocoMap.RY_RN = RefCountry.LoadFromCountryCode(Factory, "AU").PK;
			sYDLocoMap.RY_SystemUsage = UPEDataLine.Constants.RefLocoSystemUsage;
		}

		[TestDate(2005, 12, 13, 8, 0, 1)]
		public void TestValidateArrivalDateWarningNote()
		{
			Level1DataImport.ArrivalDate = ZDateTime.Now.AddDays(-5);
			Level1DataImport.ValidateArrivalDateWarningNote();
			AssertEquals(false, Level1DataImport.ArrivalDateWarningNoteInfo.HasErrors());

			Level1DataImport.ArrivalDate = ZDateTime.Now.AddDays(-6);
			Level1DataImport.ValidateArrivalDateWarningNote();
			AssertEquals("Please indicate why you are importing when the arrival date has warnings.", Level1DataImport.ArrivalDateWarningNoteInfo.GetErrors().GetFirstMessage());

			Level1DataImport.ArrivalDateWarningNote = "Test";
			Level1DataImport.ValidateArrivalDateWarningNote();
			AssertEquals(false, Level1DataImport.ArrivalDateWarningNoteInfo.HasErrors());
		}

		public void TestValidateMasterBillWarningNote()
		{
			Level1DataImport.ValidateMasterBillWarningNote();
			AssertEquals(false, Level1DataImport.MasterbillWarningNoteInfo.HasErrors());

			Level1DataImport.MasterBillInfo.AddWarning("some warning");
			Level1DataImport.ValidateMasterBillWarningNote();
			AssertEquals("Please indicate why you are importing a Masterbill which has warnings.", Level1DataImport.MasterbillWarningNoteInfo.GetErrors().GetFirstMessage());

			Level1DataImport.MasterbillWarningNote = "Test";
			Level1DataImport.ValidateMasterBillWarningNote();
			AssertEquals(false, Level1DataImport.MasterbillWarningNoteInfo.HasErrors());
		}

		public void TestDisableTradeNetDescisionProvider()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				Level1DataImportForManifest.PortOfLoading = "AUSYD";
				Level1DataImportForManifest.PortOfDischarge = "SGSIN";
				UPEDataRegistry.Instance.EnableDecisionSupportImportShipments = true;
				Assert(!Level1DataImportForManifest.DisableDecisionProvider);
				Assert(!Level1DataImportForManifest.DisableDecisionProviderInfo.ReadOnly);
				UPEDataRegistry.Instance.EnableDecisionSupportImportShipments = false;
				Level1DataImportForManifest.PortOfDischarge = "SGSLT";
				Assert(Level1DataImportForManifest.DisableDecisionProvider);
				Assert(Level1DataImportForManifest.DisableDecisionProviderInfo.ReadOnly);
				Level1DataImportForManifest.PortOfLoading = "SGSIN";
				Level1DataImportForManifest.PortOfDischarge = "AUSYD";
				UPEDataRegistry.Instance.EnableDecisionSupportExportShipments = true;
				Assert(!Level1DataImportForManifest.DisableDecisionProvider);
				Assert(!Level1DataImportForManifest.DisableDecisionProviderInfo.ReadOnly);
				UPEDataRegistry.Instance.EnableDecisionSupportExportShipments = false;
				Level1DataImportForManifest.PortOfLoading = "SGSLT";
				Assert(Level1DataImportForManifest.DisableDecisionProvider);
				Assert(Level1DataImportForManifest.DisableDecisionProviderInfo.ReadOnly);
			}
		}

		public void TestDefaultValues()
		{
			AssertDecisionSupport(false, false, true);
			AssertDecisionSupport(false, true, false);
			AssertDecisionSupport(true, false, false);
			AssertDecisionSupport(true, true, false);
		}

		public void TestErrorForNeitherExportOrImport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				Level1DataImportForManifest.PortOfLoading = "AUSYD";
				Level1DataImportForManifest.PortOfDischarge = "USLAX";
				AssertHasError(Level1DataImportForManifest.PortOfDischargeInfo, "Either the Port of Loading or Discharge must be a local port for Singapore");

				Level1DataImportForManifest.PortOfLoading = "SGSIN";
				Level1DataImportForManifest.PortOfDischarge = "AUMEL";
				AssertNoErrors(Level1DataImportForManifest.PortOfDischargeInfo);

				Level1DataImportForManifest.PortOfLoading = "SGSLT";
				Level1DataImportForManifest.PortOfDischarge = "SGSIN";
				AssertHasError(Level1DataImportForManifest.PortOfDischargeInfo, "Either the Port of Loading or Discharge must be a local port for Singapore");

				Level1DataImportForManifest.PortOfLoading = "AUSYD";
				Level1DataImportForManifest.PortOfDischarge = "SGSLT";
				AssertNoErrors(Level1DataImportForManifest.PortOfDischargeInfo);
			}
		}

		public void TestIsExport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				Level1DataImportForManifest.PortOfLoading = "SGSIN";
				Level1DataImportForManifest.PortOfDischarge = "AUMEL";
				Assert(Level1DataImportForManifest.IsExport);

				Level1DataImportForManifest.PortOfDischarge = "SGSIN";
				Level1DataImportForManifest.PortOfLoading = "AUMEL";
				Assert(!Level1DataImportForManifest.IsExport);

				Level1DataImportForManifest.PortOfLoading = "SGSLT";
				Level1DataImportForManifest.PortOfDischarge = "SGSIN";
				Assert(!Level1DataImportForManifest.IsExport);

				Level1DataImportForManifest.PortOfLoading = "AUSYD";
				Level1DataImportForManifest.PortOfDischarge = "NZAKL";
				Assert(!Level1DataImportForManifest.IsExport);
			}
		}

		public void TestCarrierAgent()
		{
			var importOrg = Factory.New<OrgHeader>();
			var importAddressPk = importOrg.MainAddress.PK;
			var importShippingAgent = new ShippingAgentObject
			{
				ShippingAgentAddress = importAddressPk
			};
			var exportOrg = Factory.New<OrgHeader>();
			var exportAddressPk = exportOrg.MainAddress.PK;
			var exportShippingAgent = new ShippingAgentObject
			{
				ShippingAgentAddress = exportAddressPk
			};

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			using (UPEDataRegistry.Instance.DefaultLevelOneExportCarrierAgentItem.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, exportShippingAgent))
			using (UPEDataRegistry.Instance.DefaultLevelOneImportCarrierAgentItem.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, importShippingAgent))
			{
				Assert(Level1DataImportForManifest.ShippingAgentAddress.IsEmpty);

				Level1DataImportForManifest.PortOfLoading = "SGSIN";
				Level1DataImportForManifest.PortOfDischarge = "AUSYD";
				AssertEquals(exportAddressPk, Level1DataImportForManifest.ShippingAgentAddress);

				Level1DataImportForManifest.ShippingAgentAddress = ZGuid.Empty;
				Level1DataImportForManifest.PortOfDischarge = "SGSIN";
				Assert(Level1DataImportForManifest.ShippingAgentAddress.IsEmpty);

				Level1DataImportForManifest.PortOfLoading = "AUSYD";
				AssertEquals(importAddressPk, Level1DataImportForManifest.ShippingAgentAddress);

				Level1DataImportForManifest.ShippingAgentAddress = ZGuid.Empty;
				Level1DataImportForManifest.PortOfDischarge = "AUSYD";
				Assert(Level1DataImportForManifest.ShippingAgentAddress.IsEmpty);

				var anotherShippingAgentAddressPk = Factory.New<OrgHeader>().MainAddress.PK;
				Level1DataImportForManifest.ShippingAgentAddress = anotherShippingAgentAddressPk;
				Level1DataImportForManifest.PortOfDischarge = "SGSIN";
				AssertEquals(anotherShippingAgentAddressPk, Level1DataImportForManifest.ShippingAgentAddress);
			}
		}

		public void TestCarrierAgentValidation()
		{
			var orgHeader = Factory.New<OrgHeader>();
			AssertNoErrors(Level1DataImport.ShippingAgentAddressInfo);
			Level1DataImport.ShippingAgentAddress_ZAddress.OrgPK = ZGuid.Invalid;
			AssertHasErrorContaining(Level1DataImport.ShippingAgentAddress_ZAddress.OrgPKInfo, ListValidation.InvalidCodeError);
			Level1DataImport.ShippingAgentAddress_ZAddress.OrgPK = orgHeader.PK;
			AssertNoErrorContaining(Level1DataImport.ShippingAgentAddress_ZAddress.OrgPKInfo, ListValidation.InvalidCodeError);
			Level1DataImport.ShippingAgentAddress = ZGuid.Invalid;
			AssertHasErrorContaining(Level1DataImport.ShippingAgentAddressInfo, ListValidation.InvalidCodeError);
			Level1DataImport.ShippingAgentAddress = orgHeader.MainAddress.PK;
			AssertNoErrorContaining(Level1DataImport.ShippingAgentAddressInfo, ListValidation.InvalidCodeError);
		}

		#region Implementation

		void AssertDecisionSupport(bool importSupportEnabled, bool exportSupportEnabled, bool decisionProviderDisabled)
		{
			UPEDataRegistry.Instance.EnableDecisionSupportImportShipments = importSupportEnabled;
			UPEDataRegistry.Instance.EnableDecisionSupportExportShipments = exportSupportEnabled;
			var level1 = new Level1DataImport(Factory, true);
			AssertEquals(decisionProviderDisabled, level1.DisableDecisionProvider);
			AssertEquals(decisionProviderDisabled, level1.DisableDecisionProviderInfo.ReadOnly);
		}

		#endregion

		#region Setup

		protected override BusinessObject GetNewBusinessObject()
		{
			return new Level1DataImport(Factory);
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;

			base.SetUp();
			Level1DataImport = new Level1DataImport(Factory);
			Level1DataImportForManifest = new Level1DataImport(Factory, isImportToManifest: true);
		}
		Level1DataImport Level1DataImport;
		Level1DataImport Level1DataImportForManifest;
		#endregion
	}
}
