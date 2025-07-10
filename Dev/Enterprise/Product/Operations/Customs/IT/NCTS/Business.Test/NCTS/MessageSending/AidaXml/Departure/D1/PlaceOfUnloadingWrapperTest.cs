using System;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class PlaceOfUnloadingWrapperTest : TestCaseWithFactory
{
	public void TestNewOrNull_TransitionPhaseOff()
	{
		using (TemporarilySetTransitionPeriod(false))
		{
			CombineAssertions("When TP OFF, Movement Header not valid, NewOrNull", () =>
			{
				AssertNull(PlaceOfUnloadingWrapper.NewOrNull(Factory.New<NctsDepartureMovementHeader>()));
				AssertNull("Movement Header with white spaces", PlaceOfUnloadingWrapper.NewOrNull(CreateMovementHeader(" ", " ")));
				AssertNull("Movement Header with port BM_ForeignDestPortKCode length is 1", PlaceOfUnloadingWrapper.NewOrNull(CreateMovementHeader("A", " ")));
				AssertNull("Movement Header with port BM_ForeignDestPortKCode length is 3", PlaceOfUnloadingWrapper.NewOrNull(CreateMovementHeader("ABC", " ")));
				AssertNull("Movement Header with port BM_ForeignDestPortKCode length is 4", PlaceOfUnloadingWrapper.NewOrNull(CreateMovementHeader("ABCD", " ")));
			});

			CombineAssertions("When TP OFF, Movement Header is valid, NewOrNull", () =>
			{
				AssertNull("SecurityType == NON", PlaceOfUnloadingWrapper.NewOrNull(CreateMovementHeader("IT", " ", NctsTypeOfSecurityList.Codes.NON)));
				AssertNotNull("SecurityType != NON, Movement Header CountryCode", PlaceOfUnloadingWrapper.NewOrNull(CreateMovementHeader("IT", " ")));
				AssertNotNull("SecurityType != NON, Movement Header UNLOCode", PlaceOfUnloadingWrapper.NewOrNull(CreateMovementHeader("ITMIL", " ")));
				AssertNotNull("SecurityType != NON, Movement Header Location", PlaceOfUnloadingWrapper.NewOrNull(CreateMovementHeader("", "a")));
			});
		}
	}

	public void TestNewOrNull_TransitionPhaseOn()
	{
		using (TemporarilySetTransitionPeriod(true))
		{
			CombineAssertions("When TP ON, Movement Header not valid, NewOrNull", () =>
			{
				AssertNull(PlaceOfUnloadingWrapper.NewOrNull(Factory.New<NctsDepartureMovementHeader>()));
				AssertNull("Movement Header with white spaces", PlaceOfUnloadingWrapper.NewOrNull(CreateMovementHeader(" ", " ")));
				AssertNull("Movement Header with port BM_ForeignDestPortKCode length is 1", PlaceOfUnloadingWrapper.NewOrNull(CreateMovementHeader("A", " ")));
				AssertNull("Movement Header with port BM_ForeignDestPortKCode length is 3", PlaceOfUnloadingWrapper.NewOrNull(CreateMovementHeader("ABC", " ")));
				AssertNull("Movement Header with port BM_ForeignDestPortKCode length is 4", PlaceOfUnloadingWrapper.NewOrNull(CreateMovementHeader("ABCD", " ")));
			});

			CombineAssertions("When TP ON, Movement Header is valid, NewOrNull", () =>
			{
				AssertNotNull("SecurityType == NON", PlaceOfUnloadingWrapper.NewOrNull(CreateMovementHeader("IT", " ", NctsTypeOfSecurityList.Codes.NON)));
				AssertNotNull("SecurityType != NON, Movement Header CountryCode", PlaceOfUnloadingWrapper.NewOrNull(CreateMovementHeader("IT", " ")));
				AssertNotNull("SecurityType != NON, Movement Header UNLOCode", PlaceOfUnloadingWrapper.NewOrNull(CreateMovementHeader("ITMIL", " ")));
				AssertNotNull("SecurityType != NON, Movement Header Location", PlaceOfUnloadingWrapper.NewOrNull(CreateMovementHeader("", "a")));
			});
		}
	}

	public void TestUNLOCode()
	{
		var wrapper = PlaceOfUnloadingWrapper.NewOrNull(CreateMovementHeader("  ITV", " "));
		AssertEquals(nameof(IGeoLocationDetails.UNLOCode), "ITV", wrapper.UNLOCode);

		wrapper = PlaceOfUnloadingWrapper.NewOrNull(CreateMovementHeader("IT", " "));
		AssertNullOrEmpty(nameof(IGeoLocationDetails.UNLOCode), wrapper.UNLOCode);
	}

	public void TestCountryCode()
	{
		var wrapper = PlaceOfUnloadingWrapper.NewOrNull(CreateMovementHeader("ITMIL", " "));
		AssertNullOrEmpty(nameof(IGeoLocationDetails.CountryCode), wrapper.CountryCode);

		wrapper = PlaceOfUnloadingWrapper.NewOrNull(CreateMovementHeader("IT", " "));
		AssertEquals(nameof(IGeoLocationDetails.CountryCode), "IT", wrapper.CountryCode);
	}

	public void TestLocation()
	{
		var wrapper = PlaceOfUnloadingWrapper.NewOrNull(CreateMovementHeader("ITMIL", " "));
		AssertNullOrEmpty(nameof(IGeoLocationDetails.Location), wrapper.Location);

		wrapper = PlaceOfUnloadingWrapper.NewOrNull(CreateMovementHeader("", "    Milano  "));
		AssertEquals(nameof(IGeoLocationDetails.Location), "Milano", wrapper.Location);
	}

	NctsDepartureMovementHeader CreateMovementHeader(string portOfForeignDesCode, string placeOfUnloading, string securityType = NctsTypeOfSecurityList.Codes.ENT)
	{
		var movementHeader = Factory.NewDepartureNctsHeader().MovementHeader;
		movementHeader.BM_ForeignDestPortKCode = portOfForeignDesCode;
		movementHeader.BM_PlaceOfUnloading = placeOfUnloading;
		movementHeader.BM_TypeOfSecurity = securityType;
		return movementHeader;
	}

	IDisposable TemporarilySetTransitionPeriod(bool isTransitionPeriodActive)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isTransitionPeriodActive);
}
