using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class NctsDepartureMovementHeaderPhase5RuleB1897ValidationTest : TestCaseWithFactory
{
	static readonly (string TransportID, string TransportCountry, int CNT, int NCT, bool Err)[] CheckBmWithContainersTestCases =
	{
		(TransportID: string.Empty, TransportCountry: string.Empty, CNT: 0, NCT: 0, Err: false),
		(TransportID: string.Empty, TransportCountry: string.Empty, CNT: 1, NCT: 0, Err: false),
		(TransportID: string.Empty, TransportCountry: string.Empty, CNT: 0, NCT: 1, Err: false),
		(TransportID: string.Empty, TransportCountry: string.Empty, CNT: 1, NCT: 1, Err: false),
		(TransportID: string.Empty, TransportCountry: "PL", CNT: 0, NCT: 0, Err: false),
		(TransportID: string.Empty, TransportCountry: "PL", CNT: 1, NCT: 0, Err: false),
		(TransportID: string.Empty, TransportCountry: "PL", CNT: 0, NCT: 1, Err: false),
		(TransportID: string.Empty, TransportCountry: "PL", CNT: 1, NCT: 1, Err: false),
		(TransportID: "DFF", TransportCountry: "PL", CNT: 0, NCT: 0, Err: false),
		(TransportID: "DFF", TransportCountry: "PL", CNT: 1, NCT: 0, Err: false),
		(TransportID: "DFF", TransportCountry: "PL", CNT: 0, NCT: 1, Err: false),
		(TransportID: "DFF", TransportCountry: "PL", CNT: 1, NCT: 1, Err: false),
		(TransportID: "DFF", TransportCountry: "PL", CNT: 0, NCT: 0, Err: false),
		(TransportID: "DFF", TransportCountry: "PL", CNT: 10, NCT: 0, Err: false),
		(TransportID: "DFF", TransportCountry: "PL", CNT: 0, NCT: 10, Err: false),
		(TransportID: "DFF", TransportCountry: "PL", CNT: 10, NCT: 10, Err: false),
		(TransportID: "DFF", TransportCountry: string.Empty, CNT: 0, NCT: 0, Err: true),
		(TransportID: "DFF", TransportCountry: string.Empty, CNT: 1, NCT: 0, Err: false),
		(TransportID: "DFF", TransportCountry: string.Empty, CNT: 0, NCT: 1, Err: true),
		(TransportID: "DFF", TransportCountry: string.Empty, CNT: 1, NCT: 1, Err: false),
		(TransportID: "DFF", TransportCountry: string.Empty, CNT: 0, NCT: 0, Err: true),
		(TransportID: "DFF", TransportCountry: string.Empty, CNT: 10, NCT: 0, Err: false),
		(TransportID: "DFF", TransportCountry: string.Empty, CNT: 0, NCT: 10, Err: true),
		(TransportID: "DFF", TransportCountry: string.Empty, CNT: 10, NCT: 10, Err: false)
	};

	static readonly (string TransportID, string TransportCountry, int CNT, int NCT, bool Err)[] CheckBmWithContainersWhereNationalityNotRequiredTestCases =
	{
		(TransportID: string.Empty, TransportCountry: string.Empty, CNT: 0, NCT: 0, Err: false),
		(TransportID: string.Empty, TransportCountry: string.Empty, CNT: 1, NCT: 0, Err: false),
		(TransportID: string.Empty, TransportCountry: string.Empty, CNT: 0, NCT: 1, Err: false),
		(TransportID: string.Empty, TransportCountry: string.Empty, CNT: 1, NCT: 1, Err: false),
		(TransportID: string.Empty, TransportCountry: "PL", CNT: 0, NCT: 0, Err: false),
		(TransportID: string.Empty, TransportCountry: "PL", CNT: 1, NCT: 0, Err: false),
		(TransportID: string.Empty, TransportCountry: "PL", CNT: 0, NCT: 1, Err: false),
		(TransportID: string.Empty, TransportCountry: "PL", CNT: 1, NCT: 1, Err: false),
		(TransportID: "DFF", TransportCountry: string.Empty, CNT: 0, NCT: 0, Err: false),
		(TransportID: "DFF", TransportCountry: string.Empty, CNT: 1, NCT: 0, Err: false),
		(TransportID: "DFF", TransportCountry: string.Empty, CNT: 0, NCT: 1, Err: false),
		(TransportID: "DFF", TransportCountry: string.Empty, CNT: 1, NCT: 1, Err: false),
		(TransportID: "DFF", TransportCountry: string.Empty, CNT: 0, NCT: 0, Err: false),
		(TransportID: "DFF", TransportCountry: string.Empty, CNT: 10, NCT: 0, Err: false),
		(TransportID: "DFF", TransportCountry: string.Empty, CNT: 0, NCT: 10, Err: false),
		(TransportID: "DFF", TransportCountry: string.Empty, CNT: 10, NCT: 10, Err: false),
		(TransportID: "DFF", TransportCountry: "PL", CNT: 0, NCT: 0, Err: true),
		(TransportID: "DFF", TransportCountry: "PL", CNT: 1, NCT: 0, Err: true),
		(TransportID: "DFF", TransportCountry: "PL", CNT: 0, NCT: 1, Err: true),
		(TransportID: "DFF", TransportCountry: "PL", CNT: 1, NCT: 1, Err: true),
		(TransportID: "DFF", TransportCountry: "PL", CNT: 0, NCT: 0, Err: true),
		(TransportID: "DFF", TransportCountry: "PL", CNT: 10, NCT: 0, Err: true),
		(TransportID: "DFF", TransportCountry: "PL", CNT: 0, NCT: 10, Err: true),
		(TransportID: "DFF", TransportCountry: "PL", CNT: 10, NCT: 10, Err: true)
	};

	public void TestCheckBM_InlandTransport_WithContainers_Rail_TransportAtDeparture()
	{
		var errorMessage = "[B1897] Nationality for Train Number is not required.";
		departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._2_RailTransport;
		CheckBM_WithContainers_ExecuteMultipleValidationConfigurations_ForTransportDeparture(errorMessage, CheckBmWithContainersWhereNationalityNotRequiredTestCases);
	}

	public void TestCheckBM_InlandTransport_WithContainers_Rail_Trailer1IDAtDeparture()
	{
		var errorMessage = "[B1897] Nationality for Wagon is not required.";
		departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._2_RailTransport;

		var setName = (string name) => { departureMovement.Trailer1IDAtDeparture = name; };
		var setCountry = (string country) => { departureMovement.Trailer1NationalityAtDeparture = country; };
		var getInfo = () => departureMovement.Trailer1NationalityAtDepartureInfo;
		var validate = () => { departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer1Nationality(); };

		CheckBM_WithContainers_ExecuteMultipleValidationConfigurations(errorMessage, CheckBmWithContainersWhereNationalityNotRequiredTestCases, setName, setCountry, getInfo, validate);
	}

	public void TestCheckBM_InlandTransport_WithContainers_Sea()
	{
		departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._1_SeaTransport;
		var errorMessage = "[B1897] You have not entered Nationality for Vessel.";

		var setName = (string name) => { departureMovement.VesselNameAtDeparture = name; };
		var setCountry = (string country) => { departureMovement.VesselCountryAtDeparture = country; };
		var getInfo = () => departureMovement.VesselCountryAtDepartureInfo;
		var validate = () => { departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureCountry(); };

		CheckBM_WithContainers_ExecuteMultipleValidationConfigurations(errorMessage, CheckBmWithContainersTestCases, setName, setCountry, getInfo, validate);
	}

	public void TestCheckBM_InlandTransport_WithContainers_Road_TransportAtDeparture()
	{
		var errorMessage = "[B1897] You have not entered Nationality for Transport ID.";
		departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._3_RoadTransport;
		CheckBM_WithContainers_ExecuteMultipleValidationConfigurations_ForTransportDeparture(errorMessage, CheckBmWithContainersTestCases);
	}

	public void TestCheckBM_InlandTransport_WithContainers_Road_Trailer1IDAtDeparture()
	{
		departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._3_RoadTransport;
		var errorMessage = "[B1897] You have not entered Nationality for Trailer 1 ID.";

		var setName = (string name) => { departureMovement.Trailer1IDAtDeparture = name; };
		var setCountry = (string country) => { departureMovement.Trailer1NationalityAtDeparture = country; };
		var getInfo = () => departureMovement.Trailer1NationalityAtDepartureInfo;
		var validate = () => { departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer1Nationality(); };

		CheckBM_WithContainers_ExecuteMultipleValidationConfigurations(errorMessage, CheckBmWithContainersTestCases, setName, setCountry, getInfo, validate);
	}

	public void TestCheckBM_InlandTransport_WithContainers_Road_Trailer2IDAtDeparture()
	{
		departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._3_RoadTransport;
		var errorMessage = "[B1897] You have not entered Nationality for Trailer 2 ID.";

		var setName = (string name) => { departureMovement.Trailer2IDAtDeparture = name; };
		var setCountry = (string country) => { departureMovement.Trailer2NationalityAtDeparture = country; };
		var getInfo = () => departureMovement.Trailer2NationalityAtDepartureInfo;
		var validate = () => { departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer2Nationality(); };

		CheckBM_WithContainers_ExecuteMultipleValidationConfigurations(errorMessage, CheckBmWithContainersTestCases, setName, setCountry, getInfo, validate);
	}

	public void TestCheckBM_InlandTransport_WithContainers_Air()
	{
		var errorMessage = "[B1897] You have not entered Nationality for Flight Number.";
		departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._4_AirTransport;
		CheckBM_WithContainers_ExecuteMultipleValidationConfigurations_ForTransportDeparture(errorMessage, CheckBmWithContainersTestCases);
	}

	public void TestCheckBM_InlandTransport_WithContainers_FixedTransportInstallations()
	{
		var errorMessage = "[B1897] You have not entered Nationality for Transport ID.";
		departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._7_FixedTransportInstallations;
		CheckBM_WithContainers_ExecuteMultipleValidationConfigurations_ForTransportDeparture(errorMessage, CheckBmWithContainersTestCases);
	}

	public void TestCheckBM_InlandTransport_WithContainers_InlandWaterway()
	{
		var errorMessage = "[B1897] You have not entered Nationality for Vessel.";
		departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._8_InlandWaterwayTransport;

		var setName = (string name) => { departureMovement.VesselNameAtDeparture = name; };
		var setCountry = (string country) => { departureMovement.VesselCountryAtDeparture = country; };
		var getInfo = () => departureMovement.VesselCountryAtDepartureInfo;
		var validate = () => { departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureCountry(); };

		CheckBM_WithContainers_ExecuteMultipleValidationConfigurations(errorMessage, CheckBmWithContainersTestCases, setName, setCountry, getInfo, validate);
	}

	public void TestCheckBM_InlandTransport_WithContainers_OwnPropulsion()
	{
		var errorMessage = "[B1897] You have not entered Nationality for Transport ID.";
		departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._9_OwnPropulsion;
		CheckBM_WithContainers_ExecuteMultipleValidationConfigurations_ForTransportDeparture(errorMessage, CheckBmWithContainersTestCases);
	}

	public void TestCheckBM_RN_NKTransportAtDepartureTrailer1Nationality() => CombineAssertions(() =>
	{
		var targetInfo = departureMovement.BM_RN_NKTransportAtDepartureTrailer1NationalityInfo;
		ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);

		departureMovement.BM_TransportAtDepartureTrailer1RegNo = "ABC";
		testContext.SetNctsTransitionPeriod(true);
		testContext.DisableRule(x => x.IsRuleB1897Active);
		departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer1Nationality();
		AssertHasMessageErrorContaining("When RuleB1897 is disabled and Transition Period is ON", targetInfo, "You have not entered");

		testContext.EnableRule(x => x.IsRuleB1897Active);
		departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer1Nationality();
		AssertNoMessageErrorContaining("When RuleB1897 is enabled and Transition Period is ON", targetInfo, "You have not entered");

		testContext.SetNctsTransitionPeriod(false);
		testContext.DisableRule(x => x.IsRuleB1897Active);
		departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer1Nationality();
		AssertHasMessageErrorContaining("When RuleB1897 is disabled and Transition Period is OFF", targetInfo, "You have not entered");

		testContext.EnableRule(x => x.IsRuleB1897Active);
		departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer1Nationality();
		AssertHasMessageErrorContaining("When RuleB1897 is enabled and Transition Period is OFF", targetInfo, "You have not entered");
	});

	public void TestCheckBM_RN_NKTransportAtDepartureTrailer2Nationality() => CombineAssertions(() =>
	{
		var targetInfo = departureMovement.BM_RN_NKTransportAtDepartureTrailer2NationalityInfo;

		ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);

		departureMovement.BM_TransportAtDepartureTrailer2RegNo = "ABC";
		testContext.SetNctsTransitionPeriod(true);
		testContext.DisableRule(x => x.IsRuleB1897Active);
		departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer2Nationality();
		AssertHasMessageErrorContaining("When RuleB1897 is disabled and Transition Period is ON", targetInfo, "You have not entered");

		testContext.EnableRule(x => x.IsRuleB1897Active);
		departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer2Nationality();
		AssertNoMessageErrorContaining("When RuleB1897 is enabled and Transition Period is ON", targetInfo, "You have not entered");

		testContext.SetNctsTransitionPeriod(false);
		testContext.DisableRule(x => x.IsRuleB1897Active);
		departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer2Nationality();
		AssertHasMessageErrorContaining("When RuleB1897 is disabled and Transition Period is OFF", targetInfo, "You have not entered");

		testContext.EnableRule(x => x.IsRuleB1897Active);
		departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer2Nationality();
		AssertHasMessageErrorContaining("When RuleB1897 is enabled and Transition Period is OFF", targetInfo, "You have not entered");
	});

	void CheckBM_WithContainers_ExecuteMultipleValidationConfigurations_ForTransportDeparture(string errorMessage,
		(string TransportID, string TransportCountry, int CNT, int NCT, bool Err)[] testCases)
	{
		var setName = (string name) => { departureMovement.TransportAtDeparture = name; };
		var setCountry = (string country) => { departureMovement.TransportCountryAtDeparture = country; };
		var getInfo = () => departureMovement.TransportCountryAtDepartureInfo;
		var validate = () => { departureMovement.Validation.ValidateBM_RN_NKTransportAtDepartureCountry(); };

		CheckBM_WithContainers_ExecuteMultipleValidationConfigurations(errorMessage, testCases, setName, setCountry, getInfo, validate);
	}

	void CheckBM_WithContainers_ExecuteMultipleValidationConfigurations(string errorMessage,
		(string TransportID, string TransportCountry, int CNT, int NCT, bool Err)[] testCases,
		Action<string> setNameAtDeparture, Action<string> setCountryAtDeparture,
		Func<ZPropertyInfo> getInfo, Action validate)
	{
		testContext.SetNctsTransitionPeriod(true);
		CombineAssertions("When TP: ON", () =>
		{
			testContext.EnableRule(x => x.IsRuleB1897Active);
			CheckBM_WithContainers(errorMessage, true, true, testCases, setNameAtDeparture, setCountryAtDeparture, getInfo, validate);
			testContext.DisableRule(x => x.IsRuleB1897Active);
			CheckBM_WithContainers(errorMessage, true, false, testCases, setNameAtDeparture, setCountryAtDeparture, getInfo, validate);
		});

		testContext.SetNctsTransitionPeriod(false);
		CombineAssertions("When TP: OFF", () =>
		{
			testContext.EnableRule(x => x.IsRuleB1897Active);
			CheckBM_WithContainers(errorMessage, false, true, testCases, setNameAtDeparture, setCountryAtDeparture, getInfo, validate);
			testContext.DisableRule(x => x.IsRuleB1897Active);
			CheckBM_WithContainers(errorMessage, false, false, testCases, setNameAtDeparture, setCountryAtDeparture, getInfo, validate);
		});
	}

	void CheckBM_WithContainers(string errorMessage, bool transitionPeriod, bool ruleEnabled,
		(string TransportID, string TransportCountry, int CNT, int NCT, bool Err)[] testCases,
		Action<string> setNameAtDeparture, Action<string> setCountryAtDeparture,
		Func<ZPropertyInfo> getInfo, Action validate)
	{
		foreach (var testCase in testCases)
		{
			setNameAtDeparture(testCase.TransportID);
			setCountryAtDeparture(testCase.TransportCountry);
			header.DepartureHeaderContainers.Clear();

			for (var i = 0; i < testCase.CNT; i++)
			{
				_ = header.DepartureHeaderContainers.Add(headerContainerWithCNTMode);
			}

			for (var i = 0; i < testCase.NCT; i++)
			{
				_ = header.DepartureHeaderContainers.Add(headerContainerWithNCTMode);
			}

			validate();

			var assertMsg = $"TransitionPeriod: '{transitionPeriod}', Rule1897: '{ruleEnabled}', TransportIDAtDeparture: '{testCase.TransportID}', TransportCountryAtDeparture: '{testCase.TransportCountry}', HeaderContainerWithCNTMode: '{testCase.CNT}', HeaderContainerWithNCTMode: '{testCase.NCT}'";
			if (!transitionPeriod || !ruleEnabled || !testCase.Err)
			{
				AssertNoMessageError(assertMsg, getInfo(), errorMessage);
			}
			else
			{
				AssertHasMessageError(assertMsg, getInfo(), errorMessage);
			}
		}
	}

	#region SetUp

	protected override void SetUp()
	{
		base.SetUp();

		header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(NctsMovementType.Codes.Departure);

		departureMovement = header.MovementHeader;

		headerContainerWithNCTMode = Factory.New<NctsDepartureHeaderContainer>();
		headerContainerWithNCTMode.BC_Mode = Enterprise.Core.Constants.ContainerModes.NonContainerised;

		headerContainerWithCNTMode = Factory.New<NctsDepartureHeaderContainer>();
		headerContainerWithCNTMode.BC_Mode = Enterprise.Core.Constants.ContainerModes.Containerised;
		testContext = departureMovement.CreateDeparturePhase5ValidationTestContext();
	}

	protected override void TearDown()
	{
		base.TearDown();
		testContext?.Dispose();
	}

	MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider> testContext;
	NctsHeader header;
	NctsDepartureMovementHeader departureMovement;
	NctsDepartureHeaderContainer headerContainerWithNCTMode;
	NctsDepartureHeaderContainer headerContainerWithCNTMode;

	#endregion
}
