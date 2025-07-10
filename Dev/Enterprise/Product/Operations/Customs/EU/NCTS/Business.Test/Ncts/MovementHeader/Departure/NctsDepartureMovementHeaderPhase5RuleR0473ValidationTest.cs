using System;
using System.Linq.Expressions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class NctsDepartureMovementHeaderPhase5RuleR0473ValidationTest : TestCaseWithFactory
{
	public void TestNctsDepartureMovementHeaderPhase5RuleR0473ValidationConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new NctsPhase5RuleR0473Validation(movementHeader: null));
	}

	public void TestValidate_VesselNameAtDeparture_Lloyds_IMO_R0473()
	{
		AssertValidate_VesselNameAtDeparture_Lloyds_IMO(c => c.IsRuleR0473Active, R0473ErrorMessage);
	}

	public void TestValidate_VesselNameAtDeparture_Lloyds_IMO_R0473_1()
	{
		AssertValidate_VesselNameAtDeparture_Lloyds_IMO(c => c.IsRuleR0473_1Active, R0473_1ErrorMessage);
	}

	public void TestValidate_FixedTransportInstallations_IMO_R0473()
	{
		TestValidate_FixedTransportInstallations_TypeOfId(c => c.IsRuleR0473Active, R0473ErrorMessage);
	}

	public void TestValidate_FixedTransportInstallations_IMO_R0473_1()
	{
		TestValidate_FixedTransportInstallations_TypeOfId(c => c.IsRuleR0473_1Active, R0473_1ErrorMessage);
	}

	public void TestValidate_VesselNameAtDeparture_Vessel_R0473()
	{
		AssertValidate_VesselNameAtDeparture_Vessel(c => c.IsRuleR0473Active, R0473ErrorMessage);
	}

	public void TestValidate_VesselNameAtDeparture_Vessel_R0473_1()
	{
		AssertValidate_VesselNameAtDeparture_Vessel(c => c.IsRuleR0473_1Active, R0473_1ErrorMessage);
	}

	public void TestValidate_OwnPropulsion_TypeOfId_R0473()
	{
		TestValidate_OwnPropulsion_TypeOfId(c => c.IsRuleR0473Active, R0473ErrorMessage);
	}

	public void TestValidate_OwnPropulsion_TypeOfId_R0473_1()
	{
		TestValidate_OwnPropulsion_TypeOfId(c => c.IsRuleR0473_1Active, R0473_1ErrorMessage);
	}

	public void TestValidate_TransportAtDeparture_R0473()
	{
		AssertValidate_TransportAtDeparture(c => c.IsRuleR0473Active, R0473ErrorMessage);
	}

	public void TestValidate_TransportAtDeparture_R0473_1()
	{
		AssertValidate_TransportAtDeparture(c => c.IsRuleR0473_1Active, R0473_1ErrorMessage);
	}

	void AssertValidate_VesselNameAtDeparture_Lloyds_IMO(Expression<Func<INctsDepartureMovementHeaderPhase5ValidationDecider, bool>> rule, string errorMsg)
	{
		using var deciderTestContext = new MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider>(Factory);
		deciderTestContext.ClearCachedValidationDecider(departureMovement);
		deciderTestContext.DisableRule(c => c.IsRuleR0473Active);
		deciderTestContext.EnableRule(rule);

		departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._1_SeaTransport;
		departureMovement.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._10;

		CombineAssertions($"IsRuleR0473Active = ON, TransportMode = 1, TypeOfDeparture = 10", () =>
		{
			departureMovement.VesselNameAtDeparture = "DFF";
			departureMovement.Validation.ValidateBM_TransportAtDeparture();
			AssertNoMessageErrorContaining("Vessel Name = DFF", departureMovement.VesselNameAtDepartureInfo, errorMsg);

			departureMovement.VesselNameAtDeparture = "dff";
			departureMovement.Validation.ValidateBM_TransportAtDeparture();
			AssertHasMessageErrorContaining("Vessel Name = dff", departureMovement.VesselNameAtDepartureInfo, errorMsg);
		});

		deciderTestContext.ClearCachedValidationDecider(departureMovement);
		deciderTestContext.DisableRule(c => c.IsRuleR0473Active);
		deciderTestContext.DisableRule(rule);

		departureMovement.Validation.ValidateBM_TransportAtDeparture();
		AssertNoMessageErrorContaining("IsRuleR0473Active = OFF, TransportMode = 1, TypeOfDeparture = 10, Vessel Name = dff", departureMovement.VesselNameAtDepartureInfo, errorMsg);
	}

	void TestValidate_FixedTransportInstallations_TypeOfId(Expression<Func<INctsDepartureMovementHeaderPhase5ValidationDecider, bool>> rule, string errorMsg)
	{
		departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._7_FixedTransportInstallations;
		var typeOfTransportIdsFoFixedTransportInstallationsArray = NCTSTestHelper.SetupTransportIdsForFixedTransportInstallationsToTestR0473();
		using var deciderTestContext = new MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider>(Factory);
		CombineAssertions($"TransportMode = 7", () =>
		{
			foreach (var transportId in typeOfTransportIdsFoFixedTransportInstallationsArray)
			{
				deciderTestContext.ClearCachedValidationDecider(departureMovement);
				deciderTestContext.DisableRule(c => c.IsRuleR0473Active);
				deciderTestContext.EnableRule(rule);
				departureMovement.TransportTypeAtDeparture = transportId;
				departureMovement.TransportAtDeparture = "DFF";
				departureMovement.Validation.ValidateBM_TransportAtDeparture();
				AssertNoMessageErrorContaining($"IsRuleR0473Active = ON Transport ID = DFF TypeOfDeparture = {transportId}", departureMovement.TransportAtDepartureInfo, errorMsg);

				departureMovement.TransportAtDeparture = "dff";
				departureMovement.Validation.ValidateBM_TransportAtDeparture();
				AssertHasMessageErrorContaining($"IsRuleR0473Active = ON Transport ID = dff TypeOfDeparture = {transportId}", departureMovement.TransportAtDepartureInfo, errorMsg);

				deciderTestContext.ClearCachedValidationDecider(departureMovement);
				deciderTestContext.DisableRule(c => c.IsRuleR0473Active);
				deciderTestContext.DisableRule(rule);
				departureMovement.Validation.ValidateBM_TransportAtDeparture();
				AssertNoMessageErrorContaining($"IsRuleR0473Active = OFF, TypeOfDeparture = {transportId}, Transport ID = dff", departureMovement.TransportAtDepartureInfo, errorMsg);
			}
		});
	}

	void AssertValidate_VesselNameAtDeparture_Vessel(Expression<Func<INctsDepartureMovementHeaderPhase5ValidationDecider, bool>> rule, string errorMsg)
	{
		using var deciderTestContext = new MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider>(Factory);
		deciderTestContext.ClearCachedValidationDecider(departureMovement);
		deciderTestContext.DisableRule(c => c.IsRuleR0473Active);
		deciderTestContext.EnableRule(rule);

		CombineAssertions("IsRuleR0473Active = ON, TransportMode = 8, TypeOfDeparture = 80", () =>
		{
			departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
			departureMovement.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._80;

			departureMovement.VesselNameAtDeparture = "DFF";
			departureMovement.Validation.ValidateBM_TransportAtDeparture();
			AssertNoMessageErrorContaining("Vessel Name = DFF", departureMovement.VesselNameAtDepartureInfo, errorMsg);

			departureMovement.VesselNameAtDeparture = "dff";
			departureMovement.Validation.ValidateBM_TransportAtDeparture();
			AssertHasMessageErrorContaining("Vessel Name = dff", departureMovement.VesselNameAtDepartureInfo, errorMsg);
		});

		deciderTestContext.ClearCachedValidationDecider(departureMovement);
		deciderTestContext.DisableRule(c => c.IsRuleR0473Active);
		deciderTestContext.DisableRule(rule);

		departureMovement.Validation.ValidateBM_TransportAtDeparture();
		AssertNoMessageErrorContaining("IsRuleR0473Active = OFF, TransportMode = 8, Vessel Name = dff", departureMovement.VesselNameAtDepartureInfo, errorMsg);
	}

	void TestValidate_OwnPropulsion_TypeOfId(Expression<Func<INctsDepartureMovementHeaderPhase5ValidationDecider, bool>> rule, string errorMsg)
	{
		departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._9_OwnPropulsion;
		var typeOfTransportIdsForOwnPropulsionArray = NCTSTestHelper.SetupTransportIdsForOwnPropulsionToTestR0473();

		using var deciderTestContext = new MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider>(Factory);
		CombineAssertions($"TransportMode = 9", () =>
		{
			foreach (var transportId in typeOfTransportIdsForOwnPropulsionArray)
			{
				deciderTestContext.ClearCachedValidationDecider(departureMovement);
				deciderTestContext.DisableRule(c => c.IsRuleR0473Active);
				deciderTestContext.EnableRule(rule);
				departureMovement.TransportTypeAtDeparture = transportId;
				departureMovement.TransportAtDeparture = "DFF";
				departureMovement.Validation.ValidateBM_TransportAtDeparture();
				AssertNoMessageErrorContaining("IsRuleR0473Active = ON, Transport ID = DFF, TypeOfDeparture = {transportId}", departureMovement.TransportAtDepartureInfo, errorMsg);

				departureMovement.TransportAtDeparture = "dff";
				departureMovement.Validation.ValidateBM_TransportAtDeparture();
				AssertHasMessageErrorContaining("IsRuleR0473Active = ON, Transport ID = dff, TypeOfDeparture = {transportId}", departureMovement.TransportAtDepartureInfo, errorMsg);

				deciderTestContext.ClearCachedValidationDecider(departureMovement);
				deciderTestContext.DisableRule(c => c.IsRuleR0473Active);
				deciderTestContext.DisableRule(rule);
				departureMovement.Validation.ValidateBM_TransportAtDeparture();
				AssertNoMessageErrorContaining($"IsRuleR0473Active = OFF, TransportMode = 9, TypeOfDeparture = {transportId}, Transport ID = dff", departureMovement.TransportAtDepartureInfo, errorMsg);
			}
		});
	}

	void AssertValidate_TransportAtDeparture(Expression<Func<INctsDepartureMovementHeaderPhase5ValidationDecider, bool>> rule, string errorMsg)
	{
		using var deciderTestContext = new MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider>(Factory);
		deciderTestContext.ClearCachedValidationDecider(departureMovement);
		deciderTestContext.DisableRule(c => c.IsRuleR0473Active);

		CombineAssertions(() =>
		{
			AssertTransportAtDepartureContainsLowerCaseLetter(ModeOfTransportList.Codes._2_RailTransport);
			AssertTransportAtDepartureContainsLowerCaseLetter(ModeOfTransportList.Codes._3_RoadTransport);
			AssertTransportAtDepartureContainsLowerCaseLetter(ModeOfTransportList.Codes._4_AirTransport);
		});

		void AssertTransportAtDepartureContainsLowerCaseLetter(string transportMode)
		{
			deciderTestContext.EnableRule(rule);
			departureMovement.InlandTransportModeAtDeparture = transportMode;
			departureMovement.TransportAtDeparture = "DFF";
			departureMovement.Validation.ValidateBM_TransportAtDeparture();
			AssertNoMessageErrorContaining($"IsRuleR0473Active = ON, TransportMode = {transportMode}, TransportAtDeparture = DFF", departureMovement.TransportAtDepartureInfo, errorMsg);

			departureMovement.TransportAtDeparture = "dff";
			departureMovement.Validation.ValidateBM_TransportAtDeparture();
			AssertHasMessageErrorContaining($"IsRuleR0473Active = ON, TransportMode = {transportMode}, TransportAtDeparture = dff", departureMovement.TransportAtDepartureInfo, errorMsg);

			deciderTestContext.ClearCachedValidationDecider(departureMovement);
			deciderTestContext.DisableRule(c => c.IsRuleR0473Active);
			deciderTestContext.DisableRule(rule);
			departureMovement.Validation.ValidateBM_TransportAtDeparture();
			AssertNoMessageErrorContaining($"IsRuleR0473Active = OFF, TransportMode = {transportMode}, TransportAtDepartureInfo = dff", departureMovement.TransportAtDepartureInfo, errorMsg);
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
	}

	const string R0473ErrorMessage = "[R0473] Must not contain lower case letters.";
	const string R0473_1ErrorMessage = "[R0473-1] Must not contain lower case letters.";
	NctsHeader header;
	NctsDepartureMovementHeader departureMovement;

	#endregion
}
