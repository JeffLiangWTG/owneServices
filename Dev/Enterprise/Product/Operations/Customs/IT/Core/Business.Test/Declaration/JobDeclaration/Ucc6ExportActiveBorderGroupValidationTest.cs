using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class Ucc6ExportActiveBorderGroupValidationTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new Ucc6ExportActiveBorderGroupValidation(declaration: null));
		AssertNoExceptionThrown(() => new Ucc6ExportActiveBorderGroupValidation(declaration));
	}

	public void TestCheckJE_RN_NKTransportNationality_StandardMandatoryValidationIsNotTriggered()
	{
		declaration.JE_EntryStyle = "AB";

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: false))
		{
			declaration.JE_RN_NKTransportNationality = default;
			declaration.Validation.ValidateJE_RN_NKTransportNationality();
			AssertHasMessageErrorContaining("When Non-UCC6, JE_RN_NKTransportNationality=EMPTY", declaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: true))
		{
			declaration.JE_RN_NKTransportNationality = default;
			declaration.Validation.ValidateJE_RN_NKTransportNationality();
			AssertNoMessageErrorContaining("When UCC6, JE_RN_NKTransportNationality=EMPTY", declaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}

	public void TestCheckJE_RN_NKTransportNationality_ActiveBorderGroupValidation()
		=> SetDataAndAssertForActiveBorderGroupProperty(declaration.JE_RN_NKTransportNationalityInfo, doValidate: (d) => d.Validation.ValidateJE_RN_NKTransportNationality());

	public void TestCheckJE_VesselName_ActiveBorderGroupValidation()
		=> SetDataAndAssertForActiveBorderGroupProperty(declaration.JE_VesselNameInfo, transportMode: "MAI", doValidate: (d) => d.Validation.ValidateJE_VesselName());

	public void TestCheckJE_VoyageFlightNo_ActiveBorderGroupValidation()
		=> SetDataAndAssertForActiveBorderGroupProperty(declaration.JE_VoyageFlightNoInfo, doValidate: (d) => d.Validation.ValidateJE_VoyageFlightNo());

	public void TestCheckZG_BorderTransportMeans_ActiveBorderGroupValidation()
		=> SetDataAndAssertForActiveBorderGroupProperty(declaration.ZG_BorderTransportMeansInfo, doValidate: (d) => d.AddInfoValidation.ValidateZG_BorderTransportMeans());

	public void TestCheckJE_RN_NKTransportNationality_ValidationRuleC0890ForEntryStyleExportNormal()
		=> SetDataAndAssertRuleC0890WithEntryStyleExportNormal(declaration.JE_RN_NKTransportNationalityInfo, doValidate: (d) => d.Validation.ValidateJE_RN_NKTransportNationality());

	public void TestCheckZG_BorderTransportMeans_ValidationRuleC0890ForEntryStyleExportNormal()
		=> SetDataAndAssertRuleC0890WithEntryStyleExportNormal(declaration.ZG_BorderTransportMeansInfo, doValidate: (d) => d.AddInfoValidation.ValidateZG_BorderTransportMeans());

	public void TestCheckJE_VesselName_ValidationRuleC0890ForEntryStyleExportNormal()
		=> SetDataAndAssertRuleC0890WithEntryStyleExportNormal(declaration.JE_VesselNameInfo, doValidate: (d) => d.Validation.ValidateJE_VesselName(), transportMode: "ROA");

	public void TestCheckJE_VoyageFlightNo_ValidationRuleC0890ForEntryStyleExportNormal()
	{
		SetDataAndAssertRuleC0890WithEntryStyleExportNormal(declaration.JE_VoyageFlightNoInfo, doValidate: (d) => d.Validation.ValidateJE_VoyageFlightNo(),
			adjustDeclarationSetupData: d =>
			{
				d.JE_VoyageFlightNo = "FL122";
				d.JE_VesselName = ZString.Empty;
			});
	}

	public void TestCheckJE_RN_NKTransportNationality_ValidationRuleC0890ForEntryStyleExportToSpecialTerritory()
		=> SetDataAndAssertRuleC0890WithEntryStyleExportToSpecialTerritory(declaration.JE_RN_NKTransportNationalityInfo, doValidate: (d) => d.Validation.ValidateJE_RN_NKTransportNationality());

	public void TestCheckZG_BorderTransportMeans_ValidationRuleC0890ForEntryStyleExportToSpecialTerritory()
		=> SetDataAndAssertRuleC0890WithEntryStyleExportToSpecialTerritory(declaration.ZG_BorderTransportMeansInfo, doValidate: (d) => d.AddInfoValidation.ValidateZG_BorderTransportMeans());

	public void TestCheckJE_VesselName_ValidationRuleC0890ForEntryStyleExportToSpecialTerritory()
		=> SetDataAndAssertRuleC0890WithEntryStyleExportToSpecialTerritory(declaration.JE_VesselNameInfo, doValidate: (d) => d.Validation.ValidateJE_VesselName(), transportMode: "ROA");

	public void TestCheckJE_VoyageFlightNo_ValidationRuleC0890ForEntryStyleExportToSpecialTerritory()
	{
		SetDataAndAssertRuleC0890WithEntryStyleExportToSpecialTerritory(declaration.JE_VoyageFlightNoInfo, doValidate: (d) => d.Validation.ValidateJE_VoyageFlightNo(),
			adjustDeclarationSetupData: d =>
			{
				d.JE_VoyageFlightNo = "FL122";
				d.JE_VesselName = ZString.Empty;
			});
	}

	public void TestCheckJE_VesselName_ValidateRuleC0890_WithTransportModeSEAAndFilledFlightNumber()
	{
		const string expectedMessageError = "[C0890] You have not entered";
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: true))
		{
			declaration.JE_EntryStyle = "EX";
			declaration.JE_TransportMode = "SEA";
			entryInstruction.CEI_Procedure = "10";
			declaration.ZG_BorderTransportMeans = "11";
			declaration.JE_RN_NKTransportNationality = "DE";
			declaration.JE_VesselName = "VE123";
			declaration.JE_VoyageFlightNo = ZString.Empty;

			declaration.Validation.ValidateJE_VesselName();
			AssertNoMessageErrorContaining("When VesselName is filled", declaration.JE_VesselNameInfo, expectedMessageError);

			declaration.JE_VesselName = ZString.Empty;
			declaration.JE_VoyageFlightNo = "FL123";
			declaration.Validation.ValidateJE_VesselName();
			AssertNoMessageErrorContaining("When VesselName is empty and FlightNo is filled", declaration.JE_VesselNameInfo, expectedMessageError);

			declaration.JE_VesselName = ZString.Empty;
			declaration.JE_VoyageFlightNo = ZString.Empty;
			declaration.Validation.ValidateJE_VesselName();
			AssertHasMessageErrorContaining("When VesselName and FlightNo are empty", declaration.JE_VesselNameInfo, expectedMessageError);
		}
	}

	public void TestCheckJE_VoyageFlightNo_ValidateRuleC0890_WithTransportModeSEAAndFilledFlightNumber()
	{
		const string expectedMessageError = "[C0890] You have not entered";
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: true))
		{
			declaration.JE_EntryStyle = "EX";
			declaration.JE_TransportMode = "SEA";
			entryInstruction.CEI_Procedure = "10";
			declaration.ZG_BorderTransportMeans = "11";
			declaration.JE_RN_NKTransportNationality = "DE";
			declaration.JE_VesselName = "VE123";
			declaration.JE_VoyageFlightNo = "FL123";

			declaration.Validation.ValidateJE_VesselName();
			AssertNoMessageErrorContaining("When JE_VoyageFlightNo is filled", declaration.JE_VoyageFlightNoInfo, expectedMessageError);

			declaration.JE_VesselName = "123";
			declaration.JE_VoyageFlightNo = ZString.Empty;
			declaration.Validation.ValidateJE_VesselName();
			AssertNoMessageErrorContaining("When JE_VoyageFlightNo is empty and Vessel is filled", declaration.JE_VoyageFlightNoInfo, expectedMessageError);

			declaration.JE_VesselName = ZString.Empty;
			declaration.JE_VoyageFlightNo = ZString.Empty;
			declaration.Validation.ValidateJE_VesselName();
			AssertHasMessageErrorContaining("When JE_VoyageFlightNo and Vessel are empty", declaration.JE_VoyageFlightNoInfo, expectedMessageError);
		}
	}

	public void TestJE_VoyageFlightNoValidateWithTransportMode()
	{
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: true))
		{
			declaration.JE_EntryStyle = "EX";
			entryInstruction.CEI_Procedure = "10";
			CombineAssertions(() =>
			{
				SetDataAndAssertHasMessageErrorForTransportMode(transportMode: "AIR", validateAction: ValidateVoyageFlightNo, getTargetInfoFunc: GetVoyageFlightNoInfo);
				SetDataAndAssertHasMessageErrorForTransportMode(transportMode: "SEA", validateAction: ValidateVoyageFlightNo, getTargetInfoFunc: GetVoyageFlightNoInfo);

				SetDataAndAssertNoMessageErrorForTransportMode(transportMode: "FIX", validateAction: ValidateVoyageFlightNo, getTargetInfoFunc: GetVoyageFlightNoInfo);
				SetDataAndAssertNoMessageErrorForTransportMode(transportMode: "IWT", validateAction: ValidateVoyageFlightNo, getTargetInfoFunc: GetVoyageFlightNoInfo);
				SetDataAndAssertNoMessageErrorForTransportMode(transportMode: "OWN", validateAction: ValidateVoyageFlightNo, getTargetInfoFunc: GetVoyageFlightNoInfo);
				SetDataAndAssertNoMessageErrorForTransportMode(transportMode: "MAI", validateAction: ValidateVoyageFlightNo, getTargetInfoFunc: GetVoyageFlightNoInfo);
				SetDataAndAssertNoMessageErrorForTransportMode(transportMode: "RAI", validateAction: ValidateVoyageFlightNo, getTargetInfoFunc: GetVoyageFlightNoInfo);
				SetDataAndAssertNoMessageErrorForTransportMode(transportMode: "ROA", validateAction: ValidateVoyageFlightNo, getTargetInfoFunc: GetVoyageFlightNoInfo);
				SetDataAndAssertNoMessageErrorForTransportMode(transportMode: "", validateAction: ValidateVoyageFlightNo, getTargetInfoFunc: GetVoyageFlightNoInfo);
			});
		}

		void ValidateVoyageFlightNo() => declaration.Validation.ValidateJE_VoyageFlightNo();
		ZPropertyInfo GetVoyageFlightNoInfo() => declaration.JE_VoyageFlightNoInfo;
	}

	public void TestJE_VesselNameValidateWithTransportMode()
	{
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: true))
		{
			declaration.JE_EntryStyle = "EX";
			entryInstruction.CEI_Procedure = "10";
			CombineAssertions(() =>
			{
				SetDataAndAssertHasMessageErrorForTransportMode(transportMode: "ROA", validateAction: ValidateVesselName, getTargetInfoFunc: GetVesselNameInfo);
				SetDataAndAssertHasMessageErrorForTransportMode(transportMode: "IWT", validateAction: ValidateVesselName, getTargetInfoFunc: GetVesselNameInfo);
				SetDataAndAssertHasMessageErrorForTransportMode(transportMode: "OWN", validateAction: ValidateVesselName, getTargetInfoFunc: GetVesselNameInfo);
				SetDataAndAssertHasMessageErrorForTransportMode(transportMode: "SEA", validateAction: ValidateVesselName, getTargetInfoFunc: GetVesselNameInfo);
				SetDataAndAssertHasMessageErrorForTransportMode(transportMode: "", validateAction: ValidateVesselName, getTargetInfoFunc: GetVesselNameInfo);

				SetDataAndAssertNoMessageErrorForTransportMode(transportMode: "AIR", validateAction: ValidateVesselName, getTargetInfoFunc: GetVesselNameInfo);
				SetDataAndAssertNoMessageErrorForTransportMode(transportMode: "FIX", validateAction: ValidateVesselName, getTargetInfoFunc: GetVesselNameInfo);
				SetDataAndAssertNoMessageErrorForTransportMode(transportMode: "MAI", validateAction: ValidateVesselName, getTargetInfoFunc: GetVesselNameInfo);
				SetDataAndAssertNoMessageErrorForTransportMode(transportMode: "RAI", validateAction: ValidateVesselName, getTargetInfoFunc: GetVesselNameInfo);
			});
		}

		void ValidateVesselName() => declaration.Validation.ValidateJE_VesselName();
		ZPropertyInfo GetVesselNameInfo() => declaration.JE_VesselNameInfo;
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(bool isActive)
		=> ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isActive);

	IDisposable TemporarilySetTransitionPeriod(bool isActive)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isActive);

	void SetDataAndAssertRuleC0890WithEntryStyleExportToSpecialTerritory(ZPropertyInfo targetPropertyInfo, Action<JobDeclaration> doValidate, string transportMode = "AIR", Action<JobDeclaration> adjustDeclarationSetupData = null)
	{
		var procedureCodesToBeConsidered = new[] { "76", "77", };
		var transportModeToBeIgnored = new[] { "MAI", "FIX" };
		SetDataAndAssertRuleC0890ForEntryStyleWithProcedureCodesAndWithTransportModesToBeIgnored(targetPropertyInfo,
			entryStyle: EntryStyleListExportUCC.Codes.ExportToSpecialTerritory,
			procedureCodesToBeConsidered: procedureCodesToBeConsidered,
			transportModeToBeIgnored: transportModeToBeIgnored,
			doValidate: doValidate,
			transportMode: transportMode,
			adjustDeclarationSetupData: adjustDeclarationSetupData);
	}

	void SetDataAndAssertRuleC0890WithEntryStyleExportNormal(ZPropertyInfo targetPropertyInfo, Action<JobDeclaration> doValidate, string transportMode = "AIR", Action<JobDeclaration> adjustDeclarationSetupData = null)
	{
		var procedureCodesToBeConsidered = new[] { "10", "11", "23", "31" };
		var transportModeToBeIgnored = new[] { "RAI", "MAI", "FIX" };
		SetDataAndAssertRuleC0890ForEntryStyleWithProcedureCodesAndWithTransportModesToBeIgnored(targetPropertyInfo,
			entryStyle: EntryStyleListExportUCC.Codes.ExportNormal,
			procedureCodesToBeConsidered: procedureCodesToBeConsidered,
			transportModeToBeIgnored: transportModeToBeIgnored,
			doValidate: doValidate,
			transportMode: transportMode,
			adjustDeclarationSetupData: adjustDeclarationSetupData);
	}

	void SetDataAndAssertForActiveBorderGroupProperty(ZPropertyInfo targetPropertyInfo, Action<JobDeclaration> doValidate, string transportMode = "AIR")
	{
		const string expectedMessageError = "Fields of group [21] must be all filled or all empty";
		var fieldName = targetPropertyInfo.Name;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: true))
		{
			declaration.JE_TransportMode = transportMode;
			declaration.ZG_BorderTransportMeans = "11";
			declaration.JE_RN_NKTransportNationality = "DE";
			declaration.JE_VesselName = "VE12343";
			declaration.JE_VoyageFlightNo = "FL12345";

			var originalValue = targetPropertyInfo.Value;

			targetPropertyInfo.Value = default;
			doValidate(declaration);
			AssertHasMessageErrorContaining($"When {fieldName} is empty", targetPropertyInfo, expectedMessageError);

			targetPropertyInfo.Value = originalValue;
			doValidate(declaration);
			AssertNoMessageErrorContaining($"When {fieldName} is not empty", targetPropertyInfo, expectedMessageError);

			var anotherProperty = GetAnotherProperty();
			anotherProperty.Value = default;
			doValidate(declaration);
			AssertHasMessageErrorContaining($"When {fieldName} is not empty and another property {anotherProperty.Name} from the group is empty", targetPropertyInfo, expectedMessageError);
		}

		ZPropertyInfo GetAnotherProperty()
		{
			var propertyToChange = declaration.JE_RN_NKTransportNationalityInfo;
			return propertyToChange.Name == targetPropertyInfo.Name ? declaration.ZG_BorderTransportMeansInfo : propertyToChange;
		}
	}

	void SetDataAndAssertRuleC0890ForEntryStyleWithProcedureCodesAndWithTransportModesToBeIgnored(ZPropertyInfo targetPropertyInfo,
		string entryStyle,
		string[] procedureCodesToBeConsidered,
		string[] transportModeToBeIgnored,
		Action<JobDeclaration> doValidate,
		string transportMode,
		Action<JobDeclaration> adjustDeclarationSetupData = null)
	{
		const string expectedMessageError = "[C0890] You have not entered";
		var fieldName = targetPropertyInfo.Name;
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: true))
		{
			declaration.JE_EntryStyle = entryStyle;
			using (TemporarilySetTransitionPeriod(isActive: false))
			{
				foreach (var procedureCode in procedureCodesToBeConsidered)
				{
					CombineAssertions($"When Transition Mode:Off, Procedure Code {procedureCode}", () =>
					{
						declaration.JE_TransportMode = transportMode;
						declaration.ZG_BorderTransportMeans = "11";
						declaration.JE_RN_NKTransportNationality = "DE";
						entryInstruction.CEI_Procedure = procedureCode;
						declaration.JE_VesselName = "VE123";
						adjustDeclarationSetupData?.Invoke(declaration);

						var originalValue = targetPropertyInfo.Value;
						targetPropertyInfo.Value = default;
						doValidate(declaration);
						AssertHasMessageErrorContaining($"When {fieldName} is empty", targetPropertyInfo, expectedMessageError);

						entryInstruction.CEI_Procedure = "33";
						entryInstruction2.CEI_Procedure = procedureCode;
						doValidate(declaration);
						AssertHasMessageErrorContaining($"When {fieldName} is empty and one of the Procedure Code is {procedureCode}", targetPropertyInfo, expectedMessageError);

						targetPropertyInfo.Value = originalValue;
						doValidate(declaration);
						AssertNoMessageErrorContaining($"When {fieldName} is not empty", targetPropertyInfo, expectedMessageError);

						transportModeToBeIgnored.ForEach(SetTransportModeAndAssertNoMessageError);
					});
				}

				targetPropertyInfo.Value = default;
				entryInstruction.CEI_Procedure = "99";
				entryInstruction2.CEI_Procedure = "34";
				doValidate(declaration);
				AssertNoMessageErrorContaining("When When Transition Mode:Off, CEI_Procedure are different than the allowed once", targetPropertyInfo, expectedMessageError);
			}

			using (TemporarilySetTransitionPeriod(isActive: true))
			{
				declaration.JE_TransportMode = "AIR";
				entryInstruction.CEI_Procedure = "23";
				targetPropertyInfo.Value = default;
				doValidate(declaration);
				AssertNoMessageErrorContaining("When Transition Period=ON, TransportMode=SEA, JE_VoyageFlightNo is EMPTY", targetPropertyInfo, expectedMessageError);
			}
		}

		void SetTransportModeAndAssertNoMessageError(string mode)
		{
			declaration.JE_TransportMode = mode;
			targetPropertyInfo.Value = default;
			doValidate(declaration);
			AssertNoMessageErrorContaining($"When When Transition Mode:Off, JE_TransportMode={mode}", targetPropertyInfo, expectedMessageError);
		}
	}

	void SetDataAndAssertHasMessageErrorForTransportMode(string transportMode, Action validateAction, Func<ZPropertyInfo> getTargetInfoFunc)
	{
		const string expectedMessageError = "[C0890] You have not entered";
		declaration.JE_TransportMode = transportMode;
		declaration.ZG_BorderTransportMeans = ZString.Empty;
		validateAction();
		AssertHasMessageErrorContaining($"When TransportMode = '{transportMode}'", getTargetInfoFunc(), expectedMessageError);
	}

	void SetDataAndAssertNoMessageErrorForTransportMode(string transportMode, Action validateAction, Func<ZPropertyInfo> getTargetInfoFunc)
	{
		const string expectedMessageError = "[C0890] You have not entered";
		declaration.JE_TransportMode = transportMode;
		declaration.ZG_BorderTransportMeans = ZString.Empty;
		validateAction();
		AssertNoMessageErrorContaining($"When TransportMode = '{transportMode}'", getTargetInfoFunc(), expectedMessageError);
	}
}
