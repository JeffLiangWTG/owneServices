using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

class CusContainerValidationTest : EU.Business.Declaration.Testing.CusContainerValidationTest<JobDeclaration>
{
	public void TestCheckSealPartiesMandatory()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = Constants.EntryInstructionStyle._A;
		var container = (CusContainer)declaration.CusContainers.AddNew();
		var expectedMessageSealParty = MandatoryValidation.YouHaveNotEntered;

		container.CO_ContainerNumber = "CRXU1234568";
		container.CO_Seal = "123";
		container.CO_SecondSeal = "456";
		container.SealPartyForBinding = ZString.Empty;
		container.AdditionalSealPartyForBinding = ZString.Empty;

		AssertHasMessageErrorContaining(container.SealPartyForBindingInfo, expectedMessageSealParty);
		AssertHasMessageErrorContaining(container.AdditionalSealPartyForBindingInfo, expectedMessageSealParty);

		container.SealPartyForBinding = Core.Constants.ContainerSealParties.Codes.CarrierShippingLine;
		container.AdditionalSealPartyForBinding = Core.Constants.ContainerSealParties.Codes.ConsignorShipper;
		AssertNoMessageErrorContaining(container.SealPartyForBindingInfo, expectedMessageSealParty);
		AssertNoMessageErrorContaining(container.AdditionalSealPartyForBindingInfo, expectedMessageSealParty);

		entryInstruction.CEI_Style = Constants.EntryInstructionStyle._E;
		container.SealPartyForBinding = ZString.Empty;
		container.AdditionalSealPartyForBinding = ZString.Empty;
		AssertNoMessageErrorContaining(container.SealPartyForBindingInfo, expectedMessageSealParty);
		AssertNoMessageErrorContaining(container.AdditionalSealPartyForBindingInfo, expectedMessageSealParty);
	}

	public void TestCheckSealPartiesMandatoryMultiInstructions()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = Constants.EntryInstructionStyle._E;
		var container = (CusContainer)declaration.CusContainers.AddNew();
		var expectedMessageSealParty = MandatoryValidation.YouHaveNotEntered;

		container.CO_ContainerNumber = "CRXU1234568";
		container.CO_Seal = "123";
		container.CO_SecondSeal = "456";
		container.SealPartyForBinding = ZString.Empty;
		container.AdditionalSealPartyForBinding = ZString.Empty;

		AssertNoMessageErrorContaining(container.SealPartyForBindingInfo, expectedMessageSealParty);
		AssertNoMessageErrorContaining(container.AdditionalSealPartyForBindingInfo, expectedMessageSealParty);

		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.CEI_Style = Constants.EntryInstructionStyle._A;
		container.SealPartyForBinding = ZString.Empty;
		container.AdditionalSealPartyForBinding = ZString.Empty;
		AssertHasMessageErrorContaining(container.SealPartyForBindingInfo, expectedMessageSealParty);
		AssertHasMessageErrorContaining(container.AdditionalSealPartyForBindingInfo, expectedMessageSealParty);

		container.SealPartyForBinding = Core.Constants.ContainerSealParties.Codes.CarrierShippingLine;
		container.AdditionalSealPartyForBinding = Core.Constants.ContainerSealParties.Codes.ConsignorShipper;
		AssertNoMessageErrorContaining(container.SealPartyForBindingInfo, expectedMessageSealParty);
		AssertNoMessageErrorContaining(container.AdditionalSealPartyForBindingInfo, expectedMessageSealParty);
	}

	public void TestCheckSealPartiesCode()
	{
		var declaration = (JobDeclaration)GetJobDeclaration();
		var container = (CusContainer)declaration.CusContainers.AddNew();
		var expectedMessageSealParty = "Enter a valid";

		container.CO_ContainerNumber = "CRXU1234568";
		container.SealPartyForBinding = "QRT";
		container.AdditionalSealPartyForBinding = "QRT";
		AssertHasErrorContaining(container.SealPartyForBindingInfo, expectedMessageSealParty);
		AssertHasErrorContaining(container.AdditionalSealPartyForBindingInfo, expectedMessageSealParty);

		container.SealPartyForBinding = "XX";
		container.AdditionalSealPartyForBinding = "YY";
		AssertHasErrorContaining(container.SealPartyForBindingInfo, expectedMessageSealParty);
		AssertHasErrorContaining(container.AdditionalSealPartyForBindingInfo, expectedMessageSealParty);

		container.SealPartyForBinding = Core.Constants.ContainerSealParties.Codes.CarrierShippingLine;
		container.AdditionalSealPartyForBinding = Core.Constants.ContainerSealParties.Codes.Customs;
		AssertNoErrorContaining(container.SealPartyForBindingInfo, expectedMessageSealParty);
		AssertNoErrorContaining(container.AdditionalSealPartyForBindingInfo, expectedMessageSealParty);
	}
}
