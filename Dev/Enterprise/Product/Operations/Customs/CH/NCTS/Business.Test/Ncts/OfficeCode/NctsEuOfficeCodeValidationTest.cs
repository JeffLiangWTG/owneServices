using System.Runtime.CompilerServices;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using NctsTypeOfDeclaration = Enterprise.Customs.EU.NCTS.Business.NctsConstants.NctsTypeOfDeclaration.Codes;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsEuOfficeCodeValidation))]
sealed class NctsEuOfficeCodeValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckEstimatedNumberOfDays()
	{
		const string errorMessage = "Estimated Days must be empty or a number between 0 and 99.";

		var customsOffice = Factory.New<NctsEuOfficeCode>();

		customsOffice.EstimatedNumberOfDays = ZString.Empty;
		AssertNoError(customsOffice.EstimatedNumberOfDaysInfo, errorMessage);

		customsOffice.EstimatedNumberOfDays = "0";
		AssertNoError(customsOffice.EstimatedNumberOfDaysInfo, errorMessage);

		customsOffice.EstimatedNumberOfDays = "99";
		AssertNoError(customsOffice.EstimatedNumberOfDaysInfo, errorMessage);

		customsOffice.EstimatedNumberOfDays = "X";
		AssertHasError(customsOffice.EstimatedNumberOfDaysInfo, errorMessage);

		customsOffice.EstimatedNumberOfDays = "-1";
		AssertHasError(customsOffice.EstimatedNumberOfDaysInfo, errorMessage);
	}

	public void TestCheckCY_Code_NP70041() => CombineAssertions(() =>
	{
		const string errorMessage = "[NP70041] At least one Customs Office of Transit must be from the same country as that of Destination Customs Office.";

		PrepareRefCusCodeList();

		AssertOfficeCountry(CountryCodes.Estonia, CountryCodes.Switzerland, CountryCodes.Spain, NctsTypeOfDeclaration.SGI, true, errorMessage);
		AssertOfficeCountry(CountryCodes.Estonia, CountryCodes.Spain, CountryCodes.Spain, NctsTypeOfDeclaration.SGI, false, errorMessage);
		AssertOfficeCountry(CountryCodes.Estonia, CountryCodes.Switzerland, CountryCodes.Switzerland, NctsTypeOfDeclaration.SGI, false, errorMessage);
		AssertOfficeCountry(CountryCodes.Estonia, CountryCodes.Switzerland, CountryCodes.Norway, NctsTypeOfDeclaration.SGI, true, errorMessage);
		AssertOfficeCountry(CountryCodes.Estonia, CountryCodes.Switzerland, CountryCodes.Spain, NctsTypeOfDeclaration.NationalTransitSwitzerland, false, errorMessage);
		AssertOfficeCountry(CountryCodes.Estonia, CountryCodes.Switzerland, CountryCodes.Norway, NctsTypeOfDeclaration.NationalTransitSwitzerland, false, errorMessage);
	});

	public void TestCheckCY_Code_NP70231() => CombineAssertions(() =>
	{
		const string errorMessage = "[NP70231] At least one Customs Office of Transit must be declared belonging to CL010 (Country Codes Community) set of countries.";

		PrepareRefCusCodeList();

		AssertOfficeCountry(CountryCodes.Switzerland, CountryCodes.Estonia, CountryCodes.Poland, NctsTypeOfDeclaration.SGI, true, errorMessage);
		AssertOfficeCountry(CountryCodes.Spain, CountryCodes.Estonia, CountryCodes.Poland, NctsTypeOfDeclaration.SGI, true, errorMessage);
		AssertOfficeCountry(CountryCodes.Spain, CountryCodes.Poland, CountryCodes.Poland, NctsTypeOfDeclaration.SGI, false, errorMessage);
		AssertOfficeCountry(CountryCodes.Spain, CountryCodes.Estonia, CountryCodes.Estonia, NctsTypeOfDeclaration.SGI, false, errorMessage);
		AssertOfficeCountry(CountryCodes.Switzerland, CountryCodes.Estonia, CountryCodes.Poland, NctsTypeOfDeclaration.NationalTransitSwitzerland, false, errorMessage);
		AssertOfficeCountry(CountryCodes.Romania, CountryCodes.Estonia, CountryCodes.Poland, NctsTypeOfDeclaration.SGI, true, errorMessage);
	});

	void PrepareRefCusCodeList()
	{
		UniversalReferenceTestDataHelper.EnsureCountriesAreInRefCusCodeList(Factory, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, (CountryCodes.Estonia, "Estonia"), (CountryCodes.Romania, "Romania"), (CountryCodes.Lithuania, "Lithuania"));
		UniversalReferenceTestDataHelper.EnsureCountriesAreInRefCusCodeList(Factory, EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, (CountryCodes.Switzerland, "Switzerland"), (CountryCodes.Norway, "Norway"), (CountryCodes.Serbia, "Serbia"));
		Factory.Save();
	}

	void AssertOfficeCountry(string departure, string destination, string transit, string inBondEntryType, bool errorExpected, string errorMessage, [CallerLineNumber] int lineNumber = 0)
	{
		var movementHeader = SetUpDepartureMovementHeader();
		movementHeader.BM_InBondEntryType = inBondEntryType;

		movementHeader.CustomsOffices.AddNew(EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, CountryCodes.Australia);

		var officeDeparture = movementHeader.CustomsOffices.AddNew(EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
		officeDeparture.CY_Data = departure + "000";

		var officeDestination = movementHeader.CustomsOffices.AddNew(EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
		officeDestination.CY_Data = destination + "000";

		var officeTransit = movementHeader.CustomsOffices.AddNew(EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
		officeTransit.CY_Data = transit + "000";

		officeDestination.Validation.ValidateCY_Code();

		if (errorExpected)
		{
			AssertHasMessageError($"[{lineNumber}] departure='{departure}', destination='{destination}', transit='{transit}', inBondEntryType='{inBondEntryType}'", officeDestination.CY_CodeInfo, errorMessage);
		}
		else
		{
			AssertNoMessageError($"[{lineNumber}] departure='{departure}', destination='{destination}', transit='{transit}', inBondEntryType='{inBondEntryType}'", officeDestination.CY_CodeInfo, errorMessage);
		}

		NctsDepartureMovementHeader SetUpDepartureMovementHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			return nctsHeader.MovementHeader;
		}
	}
}
