using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusEntryInstructionInlandTransportModeAssessorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new CusEntryInstructionInlandTransportModeAssessor(null));
	}

	public void TestIsMandatory_WhenDeclarationIsNotUCC6()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			var inlandTransportModeAssessor = GetNewInlandTransportModeAssessor();
			AssertEquals("When Declaration is EXP but not UCC6, IsMandatory", false, inlandTransportModeAssessor.IsMandatory);
		}
	}

	public void TestIsRequiredInCustomsMessage_WhenDeclarationIsNotUcc6()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			var inlandTransportModeAssessor = GetNewInlandTransportModeAssessor();
			AssertEquals("When Declaration is EXP but not UCC6, IsRequiredInCustomsMessage", false, inlandTransportModeAssessor.IsRequiredInCustomsMessage);
		}
	}

	public void TestIsMandatory_WhenDeclarationIsNull()
	{
		var inlandTransportModeAssessor = new CusEntryInstructionInlandTransportModeAssessor(Factory.New<CusEntryInstruction>());
		AssertEquals("When Entry Instruction is not linked to any Declaration, IsMandatory", false, inlandTransportModeAssessor.IsMandatory);
	}

	public void TestIsRequiredInCustomsMessage_WhenDeclarationIsNull()
	{
		var inlandTransportModeAssessor = new CusEntryInstructionInlandTransportModeAssessor(Factory.New<CusEntryInstruction>());
		AssertEquals("When Entry Instruction is not linked to any Declaration, IsRequiredInCustomsMessage", false, inlandTransportModeAssessor.IsRequiredInCustomsMessage);
	}

	public void TestIsMandatory_Condition1()
	{
		CombineAssertions(() =>
		{
			officeOfPresentation.CY_Data = "PRE9999";
			officeOfExit.CY_Data = "EXT9999";
			declaration.JE_EntryStyle = "EX";
			entryInstruction.CEI_SubStyle = "A";
			AssertIsMandatory(expectedResult: true);

			officeOfPresentation.CY_Data = "";
			declaration.JE_CustomsOffice = "EXT9999";
			AssertIsMandatory(expectedResult: false);
		});
	}

	public void TestIsMandatory_Condition2()
	{
		CombineAssertions(() =>
		{
			officeOfPresentation.CY_Data = "PRE9999";
			officeOfExit.CY_Data = "EXT9999";
			declaration.JE_EntryStyle = "CO";
			entryInstruction.CEI_Procedure = "20";
			AssertIsMandatory(expectedResult: true);

			officeOfPresentation.CY_Data = "EXT9999";
			AssertIsMandatory(expectedResult: false);
		});
	}

	public void TestIsMandatory_Condition3()
	{
		CombineAssertions(() =>
		{
			officeOfExit.CY_Data = "EXT9999";
			declaration.JE_CustomsOffice = "CUS9999";
			declaration.JE_EntryStyle = "EX";
			entryInstruction.CEI_SubStyle = "A";
			AssertIsMandatory(expectedResult: true);

			entryInstruction.CEI_SubStyle = "B";
			AssertIsMandatory(expectedResult: false);
		});
	}

	public void TestIsMandatory_Condition4()
	{
		CombineAssertions(() =>
		{
			officeOfExit.CY_Data = "EXT9999";
			declaration.JE_CustomsOffice = "CUS9999";
			declaration.JE_EntryStyle = "CO";
			entryInstruction.CEI_Procedure = "20";
			AssertIsMandatory(expectedResult: true);

			officeOfExit.CY_Data = "CUS9999";
			AssertIsMandatory(expectedResult: false);
		});
	}

	public void TestIsRequiredInCustomsMessage_WhenOfficeOfPresentationIsNotEmpty()
	{
		CombineAssertions(() =>
		{
			officeOfPresentation.CY_Data = "PRE9999";
			officeOfExit.CY_Data = "EXT9999";
			entryInstruction.CEI_SubStyle = "A";
			declaration.JE_EntryStyle = "EX";
			AssertIsRequiredInCustomsMessage(expectedResult: true);

			entryInstruction.CEI_SubStyle = "B";
			AssertIsRequiredInCustomsMessage(expectedResult: false);

			entryInstruction.CEI_SubStyle = "A";
			declaration.JE_EntryStyle = "CO";
			entryInstruction.CEI_Procedure = "10";
			AssertIsRequiredInCustomsMessage(expectedResult: false);

			entryInstruction.CEI_Procedure = "20";
			AssertIsRequiredInCustomsMessage(expectedResult: true);
		});
	}

	public void TestIsRequiredInCustomsMessage_WhenOfficeOfPresentationIsEmpty()
	{
		CombineAssertions(() =>
		{
			officeOfPresentation.CY_Data = "";
			officeOfExit.CY_Data = "EXT9999";
			declaration.JE_CustomsOffice = "CUS9999";
			entryInstruction.CEI_SubStyle = "A";
			declaration.JE_EntryStyle = "EX";
			AssertIsRequiredInCustomsMessage(expectedResult: true);

			entryInstruction.CEI_SubStyle = "B";
			AssertIsRequiredInCustomsMessage(expectedResult: false);

			entryInstruction.CEI_SubStyle = "A";
			declaration.JE_EntryStyle = "CO";
			entryInstruction.CEI_Procedure = "10";
			AssertIsRequiredInCustomsMessage(expectedResult: false);

			entryInstruction.CEI_Procedure = "20";
			AssertIsRequiredInCustomsMessage(expectedResult: true);
		});
	}

	public void TestIsRequiredInCustomsMessage_WhenOfficePreAndOfficeExtHaveManyDifferentValue()
	{
		CombineAssertions(() =>
		{
			officeOfPresentation.CY_Data = "PRE9999";
			officeOfExit.CY_Data = "PRE9999";
			var inlandTransportModeAssessor = GetNewInlandTransportModeAssessor();
			AssertEquals("When Office of Presentation is equal Office of Exit, RequiredInCustomsMessage", false, inlandTransportModeAssessor.IsRequiredInCustomsMessage);

			officeOfPresentation.CY_Data = "";
			officeOfExit.CY_Data = "EXT9999";
			declaration.JE_CustomsOffice = "EXT9999";
			inlandTransportModeAssessor = GetNewInlandTransportModeAssessor();
			AssertEquals("When Office of Presentation is empty and Office of Exit is equal Office of Exit, RequiredInCustomsMessage", false, inlandTransportModeAssessor.IsRequiredInCustomsMessage);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		declaration.MessageVersion = "XML";
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		officeOfPresentation = GetEuOffice("PRE");
		officeOfExit = GetEuOffice("EXT");
	}

	CusEntryInstructionInlandTransportModeAssessor GetNewInlandTransportModeAssessor() => new CusEntryInstructionInlandTransportModeAssessor(entryInstruction);

	EuOfficeCode GetEuOffice(string officeCode)
	{
		var customsOffices = declaration.CustomsOffices;
		return customsOffices.Find(x => x.CY_Code == officeCode).SingleOrDefault()
				?? customsOffices.AddNew(officeCode);
	}

	void AssertIsMandatory(bool expectedResult)
	{
		var inlandTransportModeAssessor = GetNewInlandTransportModeAssessor();

		AssertEquals($"When Office of Presentation: {officeOfPresentation.CY_Data}, Office of Exit: {declaration.OfficeOfExit}, " +
			$"Customs Office: {declaration.JE_CustomsOffice}, Entry Style: {declaration.JE_EntryStyle}, " +
		$"Sub Style: {entryInstruction.CEI_SubStyle}, Procedure Code: {entryInstruction.CEI_Procedure}, IsMandatory"
			, expectedResult, inlandTransportModeAssessor.IsMandatory);
	}

	void AssertIsRequiredInCustomsMessage(bool expectedResult)
	{
		var inlandTransportModeAssessor = GetNewInlandTransportModeAssessor();

		AssertEquals($"When Office of Presentation: {officeOfPresentation.CY_Data}, Office of Exit: {declaration.OfficeOfExit}, " +
			$"Customs Office: {declaration.JE_CustomsOffice}, Entry Style: {declaration.JE_EntryStyle}, " +
		$"Sub Style: {entryInstruction.CEI_SubStyle}, Procedure Code: {entryInstruction.CEI_Procedure}, IsRequiredInCustomsMessage"
			, expectedResult, inlandTransportModeAssessor.IsRequiredInCustomsMessage);
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	EuOfficeCode officeOfPresentation;
	EuOfficeCode officeOfExit;
}
