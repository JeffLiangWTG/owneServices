using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing;

[TestedType(typeof(InventoryInfoValidation))]
sealed class InventoryInfoValidationTest : BusinessObjectValidationTestCase
{
	public void TestCSI_Code()
	{
		InventoryInfo.Validation.ValidateCSI_Code();
		AssertNoMessageErrorContaining(InventoryInfo.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

		CusEntryInstruction.SetupECRTestingContext();
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(InventoryInfo.CSI_CodeInfo);
	}

	public void TestCheckCSI_Quantity()
	{
		InventoryInfo.Validation.ValidateCSI_Quantity();
		AssertNoMessageErrorContaining(InventoryInfo.CSI_QuantityInfo, MandatoryValidation.YouHaveNotEntered);

		CusEntryInstruction.SetupECRTestingContext();
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(InventoryInfo.CSI_QuantityInfo);

		var expectedMessage = "The total quantity (Inventory) must not exceed the quantity (Move-In).";
		var inventoryInfo2 = MoveInDestination.InventoryNumbers.AddNew();

		MoveInDestination.CSI_Quantity = 2;
		InventoryInfo.CSI_Quantity = 1;
		AssertNoMessageError(InventoryInfo.CSI_QuantityInfo, expectedMessage);

		inventoryInfo2.CSI_Quantity = 2;
		AssertHasMessageError(inventoryInfo2.CSI_QuantityInfo, expectedMessage);
	}

	InventoryInfo InventoryInfo => inventoryInfo ??= MoveInDestination.InventoryNumbers.AddNew();
	InventoryInfo inventoryInfo;

	MoveInDestination MoveInDestination => moveInDestination ??= CusEntryInstruction.MoveInDestinationInfos.AddNew();
	MoveInDestination moveInDestination;

	CusEntryInstruction CusEntryInstruction => cusEntryInstruction ??= JobDeclaration.CustomsEntryInstructions.AddNew();
	CusEntryInstruction cusEntryInstruction;

	JobDeclaration JobDeclaration => jobDeclaration ??= Factory.New<JobDeclaration>();
	JobDeclaration jobDeclaration;
}

