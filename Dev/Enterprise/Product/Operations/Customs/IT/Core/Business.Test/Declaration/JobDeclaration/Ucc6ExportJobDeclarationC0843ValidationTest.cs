using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class Ucc6ExportJobDeclarationC0843ValidationTest : TestCaseWithFactory
{
	public void TestTransportModeInland_WhenUcc6ExportDecIsNotExportToSplTerritory_PresentationNotEqualToExitOffice_EntrySubStyleDependentList()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			var officeOfPresentation = declaration.CustomsOffices.AddNew("PRE", "PRE123");

			CombineAssertions("When JE_TransportModeInland mandatory conditions met", () =>
			{
				declaration.JE_EntryStyle = "EX";
				entryInstruction.CEI_SubStyle = "A";
				declaration.Validation.ValidateJE_TransportModeInland();
				AssertHasMessageErrorContaining("When JE_TransportModeInland empty", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_TransportModeInland = "AIR";
				AssertNoMessageErrorContaining("When JE_TransportModeInland filled", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);
			});

			CombineAssertions("When JE_TransportModeInland is empty and mandatory conditions do not meet", () =>
			{
				entryInstruction.CEI_Procedure = "10";
				declaration.JE_CustomsOffice = "PRE123";

				declaration.JE_TransportModeInland = ZString.Empty;
				AssertNoMessageErrorOnTransportModeInlandForSubStyle(subStyle: "B");
				AssertNoMessageErrorOnTransportModeInlandForSubStyle(subStyle: "C");
				AssertNoMessageErrorOnTransportModeInlandForSubStyle(subStyle: "D");
				AssertNoMessageErrorOnTransportModeInlandForSubStyle(subStyle: "E");
				AssertNoMessageErrorOnTransportModeInlandForSubStyle(subStyle: "F");

				entryInstruction.CEI_SubStyle = "A";
				declaration.JE_EntryStyle = "CO";
				declaration.Validation.ValidateJE_TransportModeInland();
				AssertNoMessageErrorContaining("When JE_EntryStyle = CO", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_EntryStyle = "EX";
				officeOfExit.CY_Data = "PRE123";
				declaration.Validation.ValidateJE_TransportModeInland();
				AssertNoMessageErrorContaining("When Office of exit same as Presentation", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);

				officeOfPresentation.CY_Data = "";
				declaration.Validation.ValidateJE_TransportModeInland();
				AssertNoMessageErrorContaining("When Office of presentation empty", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}
	}

	public void TestTransportModeInland_WhenUcc6ExportDecIsExportToSplTerritory_PresentationNotEqualToExitOffice_EntryProcedureCodeDependentList()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			var officeOfPresentation = declaration.CustomsOffices.AddNew("PRE", "PRE123");

			CombineAssertions("When JE_TransportModeInland mandatory conditions met", () =>
			{
				declaration.JE_EntryStyle = "CO";
				entryInstruction.CEI_Procedure = "11";
				declaration.Validation.ValidateJE_TransportModeInland();
				AssertHasMessageErrorContaining("When JE_TransportModeInland empty", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_TransportModeInland = "AIR";
				AssertNoMessageErrorContaining("When JE_TransportModeInland filled", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);
			});

			CombineAssertions("When JE_TransportModeInland is empty and mandatory conditions do not meet", () =>
			{
				entryInstruction.CEI_SubStyle = "B";
				declaration.JE_CustomsOffice = "PRE123";

				declaration.JE_TransportModeInland = ZString.Empty;
				AssertNoMessageErrorOnTransportModeInlandForProcedureCode(procedureCode: "10");
				AssertNoMessageErrorOnTransportModeInlandForProcedureCode(procedureCode: "76");
				AssertNoMessageErrorOnTransportModeInlandForProcedureCode(procedureCode: "77");

				entryInstruction.CEI_Procedure = "11";
				declaration.JE_EntryStyle = "EX";
				declaration.Validation.ValidateJE_TransportModeInland();
				AssertNoMessageErrorContaining("When JE_EntryStyle = EX", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_EntryStyle = "CO";
				officeOfExit.CY_Data = "PRE123";
				declaration.Validation.ValidateJE_TransportModeInland();
				AssertNoMessageErrorContaining("When Office of exit same as Presentation", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);

				officeOfPresentation.CY_Data = "";
				declaration.Validation.ValidateJE_TransportModeInland();
				AssertNoMessageErrorContaining("When Office of presentation empty", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}
	}

	public void TestTransportModeInland_WhenUcc6ExportDecIsNotExportToSplTerritory_PresentationOfficeEmpty_ExitNotEqualToCustomsOffice_EntrySubStyleDependentList()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			declaration.JE_CustomsOffice = "CUS111";

			CombineAssertions("When JE_TransportModeInland mandatory conditions met", () =>
			{
				declaration.JE_EntryStyle = "EX";
				entryInstruction.CEI_SubStyle = "A";
				declaration.Validation.ValidateJE_TransportModeInland();
				AssertHasMessageErrorContaining("When JE_TransportModeInland empty", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_TransportModeInland = "AIR";
				AssertNoMessageErrorContaining("When JE_TransportModeInland filled", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);
			});

			CombineAssertions("When JE_TransportModeInland is empty and mandatory conditions do not meet", () =>
			{
				entryInstruction.CEI_Procedure = "10";

				declaration.JE_TransportModeInland = ZString.Empty;
				AssertNoMessageErrorOnTransportModeInlandForSubStyle(subStyle: "B");
				AssertNoMessageErrorOnTransportModeInlandForSubStyle(subStyle: "C");
				AssertNoMessageErrorOnTransportModeInlandForSubStyle(subStyle: "D");
				AssertNoMessageErrorOnTransportModeInlandForSubStyle(subStyle: "E");
				AssertNoMessageErrorOnTransportModeInlandForSubStyle(subStyle: "F");

				entryInstruction.CEI_SubStyle = "A";
				declaration.JE_EntryStyle = "CO";
				declaration.Validation.ValidateJE_TransportModeInland();
				AssertNoMessageErrorContaining("When JE_EntryStyle = CO", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_EntryStyle = "EX";
				officeOfExit.CY_Data = "CUS111";
				declaration.Validation.ValidateJE_TransportModeInland();
				AssertNoMessageErrorContaining("When Office of exit same as Customs office", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);

				officeOfExit.CY_Data = "EXT999";
				var officeOfPresentation = declaration.CustomsOffices.AddNew("PRE", "EXT999");
				declaration.Validation.ValidateJE_TransportModeInland();
				AssertNoMessageErrorContaining("When Office of presentation not empty", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}
	}

	public void TestTransportModeInland_WhenUcc6ExportDecIsExportToSplTerritory_PresentationOfficeEmpty_ExitNotEqualToCustomsOffice_EntryProcedureCodeDependentList()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			declaration.JE_CustomsOffice = "CUS111";

			CombineAssertions("When JE_TransportModeInland mandatory conditions met", () =>
			{
				declaration.JE_EntryStyle = "CO";
				entryInstruction.CEI_Procedure = "11";
				declaration.Validation.ValidateJE_TransportModeInland();
				AssertHasMessageErrorContaining("When JE_TransportModeInland empty", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_TransportModeInland = "AIR";
				AssertNoMessageErrorContaining("When JE_TransportModeInland filled", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);
			});

			CombineAssertions("When JE_TransportModeInland is empty and mandatory conditions do not meet", () =>
			{
				entryInstruction.CEI_SubStyle = "B";

				declaration.JE_TransportModeInland = ZString.Empty;
				AssertNoMessageErrorOnTransportModeInlandForProcedureCode(procedureCode: "10");
				AssertNoMessageErrorOnTransportModeInlandForProcedureCode(procedureCode: "76");
				AssertNoMessageErrorOnTransportModeInlandForProcedureCode(procedureCode: "77");

				entryInstruction.CEI_Procedure = "11";
				declaration.JE_EntryStyle = "EX";
				declaration.Validation.ValidateJE_TransportModeInland();
				AssertNoMessageErrorContaining("When JE_EntryStyle = EX", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_EntryStyle = "CO";
				officeOfExit.CY_Data = "CUS111";
				declaration.Validation.ValidateJE_TransportModeInland();
				AssertNoMessageErrorContaining("When Office of exit same as Customs office", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);

				officeOfExit.CY_Data = "EXT999";
				var officeOfPresentation = declaration.CustomsOffices.AddNew("PRE", "EXT999");
				declaration.Validation.ValidateJE_TransportModeInland();
				AssertNoMessageErrorContaining("When Office of presentation not empty", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}
	}

	void AssertNoMessageErrorOnTransportModeInlandForSubStyle(string subStyle)
	{
		entryInstruction.CEI_SubStyle = subStyle;
		declaration.Validation.ValidateJE_TransportModeInland();
		AssertNoMessageErrorContaining($"When CEI_SubStyle = {subStyle}", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);
	}

	void AssertNoMessageErrorOnTransportModeInlandForProcedureCode(string procedureCode)
	{
		entryInstruction.CEI_Procedure = procedureCode;
		declaration.Validation.ValidateJE_TransportModeInland();
		AssertNoMessageErrorContaining($"When CEI_Procedure = {procedureCode}", declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		officeOfExit = declaration.CustomsOffices.GetFirstElementHaving("EXT");
		officeOfExit.CY_Data = "EXT999";
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	EU.Business.EuOfficeCode officeOfExit;
}
