using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsEuOfficeCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Code_Max9TXT_NCTS4()
		{
			const string message = "Only a maximum of 9 Customs Offices with Purpose 'TXT' may be specified.";
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.CustomsOffices.RemoveAndDeleteAll();
			for (int i = 0; i < 8; i++)
			{
				header.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
			}

			CombineAssertions(() =>
			{
				var office = header.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
				AssertNoMessageError("Customs Office 'TXT' #9", office.CY_CodeInfo, message);
				var office2 = header.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
				AssertHasMessageError("Customs Office 'TXT' #10", office2.CY_CodeInfo, message);
				var office3 = header.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
				AssertNoMessageError("Customs Office 'DES'", office3.CY_CodeInfo, message);
			});
		}

		public void TestCheckCY_Data_DestinationOfficeCannotBeInSanMarinoForT1Movements()
		{
			const string destinationOfficeCannotBeInSanMarinoForT1Movements = "Destination Office cannot be in San Marino for T1 Movements.";
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var officeCode = header.CustomsOffices.AddNew();
			var validation = officeCode.Validation;

			CombineAssertions(() =>
			{
				var sanMarino = Factory.New<RefCountry>();
				sanMarino.Code = Core.Constants.CountryCodes.SanMarino;
				officeCode.CY_Data = "SM000001";
				AssertNoWarning("Default", officeCode.CY_DataInfo, destinationOfficeCannotBeInSanMarinoForT1Movements);
				var movementHeader = header.MovementHeader;
				movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
				validation.ValidateCY_Data();
				AssertNoWarning("T1 only", officeCode.CY_DataInfo, destinationOfficeCannotBeInSanMarinoForT1Movements);
				movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T2;
				officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDestination;
				validation.ValidateCY_Data();
				AssertNoWarning("DES only", officeCode.CY_DataInfo, destinationOfficeCannotBeInSanMarinoForT1Movements);
				movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
				officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDestination;
				validation.ValidateCY_Data();
				AssertHasWarning("DES and T1", officeCode.CY_DataInfo, destinationOfficeCannotBeInSanMarinoForT1Movements);
			});
		}

		public void TestCheckCY_Data_TIRCanOnlyGoToEU()
		{
			const string tirCanOnlyGoToCountriesInTheEU = "TIR can only go to countries in the EU.";
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var officeCode = header.CustomsOffices.AddNew();

			CombineAssertions(() =>
			{
				var switzerland = Factory.New<RefCountry>();
				switzerland.Code = Core.Constants.CountryCodes.Switzerland;
				officeCode.CY_Data = "CH000001";
				AssertNoWarning("Default", officeCode.CY_DataInfo, tirCanOnlyGoToCountriesInTheEU);
				officeCode.Header.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
				officeCode.Validation.ValidateCY_Data();
				AssertHasWarning("TIR", officeCode.CY_DataInfo, tirCanOnlyGoToCountriesInTheEU);
			});
		}

		public void TestCheckCY_Data_MustBeMemberOfEUOrCommonTransit()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var tradeGroup = helper.CreateTradeGroup("EUN", "EUC", new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Spain, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			tradeGroup = helper.CreateTradeGroup("EUN", "EUCTP", new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Turkey, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));

			const string officeMustBeMemberOfEUOrCommonTransitMessage = "is not listed as being in the European Union or a member of the Common Transit convention";
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var officeCode = header.MovementHeader.CustomsOffices.AddNew();

			CombineAssertions(() =>
			{
				var spain = Factory.New<RefCountry>();
				spain.Code = Core.Constants.CountryCodes.Spain;
				officeCode.CY_Data = "ES000001";
				AssertNoWarningContaining("EU", officeCode.CY_DataInfo, officeMustBeMemberOfEUOrCommonTransitMessage);

				var turkey = Factory.New<RefCountry>();
				turkey.Code = Core.Constants.CountryCodes.Turkey;
				officeCode.CY_Data = "TR000001";
				AssertNoWarningContaining("CT", officeCode.CY_DataInfo, officeMustBeMemberOfEUOrCommonTransitMessage);

				var moldova = Factory.New<RefCountry>();
				moldova.Code = Core.Constants.CountryCodes.Moldova;
				officeCode.CY_Data = "MD000001";
				AssertHasWarningContaining("not EU or CT", officeCode.CY_DataInfo, officeMustBeMemberOfEUOrCommonTransitMessage);

				officeCode.CY_Data = "XX000001";
				AssertHasWarningContaining(officeCode.CY_DataInfo, "not listed as a country");

				officeCode.CY_Data = "XI000001";
				AssertNoWarningContaining(officeCode.CY_DataInfo, "not listed as a country");
			});
		}

		public void TestCheckCY_Data_DestinationCustomsOfficeForArrival_Phase4()
		{
			const string message = "An office code is needed. Example: FR000010.";
			var arrival = Factory.New<NctsHeader>();
			arrival.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			arrival.SetMovementType(NctsMovementType.Codes.Arrival);
			arrival.DestinationCustomsOfficeCodeForArrival = ZString.Empty;
			var destinationCustomsOffice = (NctsEuOfficeCode)arrival.DestinationCustomsOffice;

			CombineAssertions(() =>
			{
				destinationCustomsOffice.Validation.ValidateCY_Data();
				AssertHasMessageError("Destination Office is empty", destinationCustomsOffice.CY_DataInfo, message);
				destinationCustomsOffice.CY_Data = "FR000010";
				AssertNoWarning("Destination Office is entered", destinationCustomsOffice.CY_DataInfo, message);
			});
		}

		public void TestCheckCY_Data_DestinationCustomsOfficeForArrival_Phase5()
		{
			var arrival = Factory.New<NctsHeader>();
			arrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			arrival.SetMovementType(NctsMovementType.Codes.Arrival);

			var arrivalMovementHeader = arrival.ArrivalMovementHeader;
			arrivalMovementHeader.DestinationCustomsOfficeCodeForArrival = ZString.Empty;
			var destinationCustomsOffice = (NctsEuOfficeCode)arrivalMovementHeader.DestinationCustomsOffice;
			var message = MandatoryValidation.YouHaveNotEnteredMessage(arrivalMovementHeader.DestinationCustomsOfficeCodeForArrivalInfo.HumanReadableName);

			CombineAssertions(() =>
			{
				destinationCustomsOffice.Validation.ValidateCY_Data();
				AssertHasMessageError("Destination Office is empty", destinationCustomsOffice.CY_DataInfo, message);
				destinationCustomsOffice.CY_Data = "FR000010";
				AssertNoMessageError("Destination Office is entered", destinationCustomsOffice.CY_DataInfo, message);
			});
		}

		public void TestCheckCountryCodeSameAsCurrentCompany()
		{
			var messageError = ValidationMessages.CountryCodeDepartureOfficeShouldBeSameOfNCTSCountryCode;

			header.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = header.MovementHeader;
			movementHeader.CustomsOffices.RemoveAndDeleteAll();

			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var dep = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);

			using (NctsConfigurationTestHelper.TemporarilySetValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsCountryCodeRequiredToBeSameAsCurrentCompany), value: true))
			{
				dep.CY_Data = countryCode + "000001";
				AssertNoMessageError("The country code of NCTS departure office is same as NCTS country code.", dep.CY_DataInfo, messageError);

				dep.CY_Data = "AA000001";
				AssertHasMessageError("The country code of NCTS departure office is different from NCTS country code.", dep.CY_DataInfo, messageError);
			}

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			using (NctsConfigurationTestHelper.TemporarilySetValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsCountryCodeRequiredToBeSameAsCurrentCompany), value: true))
			{
				header.Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
				dep.CY_Data = "XI000001";
				AssertNoMessageError("Should allow Northern Ireland as the NCTS departure office for UK company", dep.CY_DataInfo, messageError);
			}

			using (NctsConfigurationTestHelper.TemporarilySetValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsCountryCodeRequiredToBeSameAsCurrentCompany), value: false))
			{
				dep.Validation.ValidateCY_Data();
				AssertNoMessageError("IsCountryCodeRequiredToBeSameAsCurrentCompany is inactive", dep.CY_DataInfo, messageError);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
		}

		NctsHeader header;
	}
}
