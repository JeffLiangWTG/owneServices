using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

class CusEntryInstructionValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCEI_OA_Warehouse2()
	{
		var expectedMessage = "Warehouse details are required for this procedure";

		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Netherlands, "", "71", "78", "", "Desc", "IMP", outOfWarehouse: true, group: "EZLL");
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		instruction.CEI_OA_Warehouse2 = ZGuid.Empty;
		AssertNoMessageError(instruction.CEI_OA_Warehouse2Info, expectedMessage);

		instruction.CEI_Style = "EZL";
		invoiceLine.JI_Procedure = "7178";
		instruction.CEI_OA_Warehouse2 = ZGuid.Empty;
		AssertHasMessageError(instruction.CEI_OA_Warehouse2Info, expectedMessage);

		instruction.CEI_OA_Warehouse2 = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
		AssertNoMessageError(instruction.CEI_OA_Warehouse2Info, expectedMessage);

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		instruction.CEI_OA_Warehouse2 = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
		AssertNoMessageError(instruction.CEI_OA_Warehouse2Info, expectedMessage);

		instruction.CEI_OA_Warehouse2 = ZGuid.Empty;
		AssertNoMessageError(instruction.CEI_OA_Warehouse2Info, expectedMessage);
	}

	public void TestCheckCEI_OA_Warehouse()
	{
		var expectedMessage = "Warehouse details are required for this procedure";

		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Netherlands, "", "72", "79", "", "Desc", "IMP", intoWarehouse: true, group: "VZL");
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		instruction.CEI_OA_Warehouse = ZGuid.Empty;
		AssertNoMessageError(instruction.CEI_OA_WarehouseInfo, expectedMessage);

		instruction.CEI_Style = "VZL";
		invoiceLine.JI_Procedure = "7279";
		instruction.CEI_OA_Warehouse = ZGuid.Empty;
		AssertHasMessageError(instruction.CEI_OA_WarehouseInfo, expectedMessage);

		instruction.CEI_OA_Warehouse = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
		AssertNoMessageError(instruction.CEI_OA_WarehouseInfo, expectedMessage);

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		instruction.CEI_OA_Warehouse = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
		AssertNoMessageError(instruction.CEI_OA_WarehouseInfo, expectedMessage);

		instruction.CEI_OA_Warehouse = ZGuid.Empty;
		AssertNoMessageError(instruction.CEI_OA_WarehouseInfo, expectedMessage);
	}

	public void TestCheckCEI_SubStyle()
	{
		var expectedMessage = "For Sub Style X or Y, a Previous Document with a Type of NMRN is required to be present on the Invoice Header or Invoice Line or Misc. tab";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var addInfoLine = invoiceLine.PreviousDocuments.AddNew();
		var addInfoHeader = invoiceHeader.PreviousDocuments.AddNew();
		var addInfoMisc = declaration.PreviousDocuments.AddNew();

		instruction.CEI_SubStyle = "X";
		addInfoLine.CSI_Code = "TST";
		addInfoHeader.CSI_Code = "TST";
		addInfoMisc.CSI_Code = "TST";

		AssertHasMessageError(instruction.CEI_SubStyleInfo, expectedMessage);

		instruction.CEI_SubStyle = "Z";
		AssertNoMessageError(instruction.CEI_SubStyleInfo, expectedMessage);

		addInfoLine.CSI_Code = "NMRN";
		instruction.CEI_SubStyle = "X";
		AssertNoMessageError(instruction.CEI_SubStyleInfo, expectedMessage);

		addInfoLine.CSI_Code = "TEST";
		addInfoHeader.CSI_Code = "NMRN";
		instruction.CEI_SubStyle = "Y";
		AssertNoMessageError(instruction.CEI_SubStyleInfo, expectedMessage);

		addInfoHeader.CSI_Code = "TEST";
		addInfoMisc.CSI_Code = "NMRN";
		instruction.CEI_SubStyle = "X";
		AssertNoMessageError(instruction.CEI_SubStyleInfo, expectedMessage);
	}

	public void TestCheckCEI_SubStyle_RuleBC9003()
	{
		var expectedMessage = "[C9003] If Sub style is B, C, E or F an additional document kind INF is required of type NRV";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var instruction = declaration.CustomsEntryInstructions.AddNew();

		instruction.CEI_SubStyle = "X";
		AssertNoMessageError(instruction.CEI_SubStyleInfo, expectedMessage);

		instruction.CEI_SubStyle = "B";
		AssertHasMessageError(instruction.CEI_SubStyleInfo, expectedMessage);

		instruction.CEI_SubStyle = "C";
		AssertHasMessageError(instruction.CEI_SubStyleInfo, expectedMessage);

		instruction.CEI_SubStyle = "E";
		AssertHasMessageError(instruction.CEI_SubStyleInfo, expectedMessage);

		instruction.CEI_SubStyle = "F";
		AssertHasMessageError(instruction.CEI_SubStyleInfo, expectedMessage);

		var additionalInfo = instruction.AdditionalInfos.AddNew();
		additionalInfo.CSI_SubType = "INF";
		additionalInfo.CSI_Code = "NRV001";

		instruction.CEI_SubStyle = "B";
		AssertNoMessageError(instruction.CEI_SubStyleInfo, expectedMessage);

		instruction.CEI_SubStyle = "C";
		AssertNoMessageError(instruction.CEI_SubStyleInfo, expectedMessage);

		instruction.CEI_SubStyle = "E";
		AssertNoMessageError(instruction.CEI_SubStyleInfo, expectedMessage);

		instruction.CEI_SubStyle = "F";
		AssertNoMessageError(instruction.CEI_SubStyleInfo, expectedMessage);
	}

	public void TestCheckGoodsLocationDescription()
	{
		var errorMessage = "Goods location is required for this Sub Style";
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var goodsLocation = entryInstruction.GoodsLocation;

		CombineAssertions(() =>
		{
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
			entryInstruction.Validation.ValidateGoodsLocationDescription();
			AssertNoMessageError("Goods Location is not empty", entryInstruction.GoodsLocationDescriptionInfo, errorMessage);

			entryInstruction.CEI_SubStyle = DeclarationSubTypeList.Codes.D;
			goodsLocation.CGL_Qualifier = ZString.Empty;
			goodsLocation.CGL_Type = ZString.Empty;
			entryInstruction.Validation.ValidateGoodsLocationDescription();
			AssertNoMessageError("Goods Location is empty but SubStyle is D", entryInstruction.GoodsLocationDescriptionInfo, errorMessage);

			entryInstruction.CEI_SubStyle = DeclarationSubTypeList.Codes.E;
			entryInstruction.Validation.ValidateGoodsLocationDescription();
			AssertNoMessageError("Goods Location is empty but SubStyle is E", entryInstruction.GoodsLocationDescriptionInfo, errorMessage);

			entryInstruction.CEI_SubStyle = DeclarationSubTypeList.Codes.F;
			entryInstruction.Validation.ValidateGoodsLocationDescription();
			AssertNoMessageError("Goods Location is empty but SubStyle is F", entryInstruction.GoodsLocationDescriptionInfo, errorMessage);

			entryInstruction.CEI_SubStyle = DeclarationSubTypeList.Codes.B;
			entryInstruction.Validation.ValidateGoodsLocationDescription();
			AssertHasMessageError("Goods Location is empty and SubStyle is not D, E, or F", entryInstruction.GoodsLocationDescriptionInfo, errorMessage);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			entryInstruction.Validation.ValidateGoodsLocationDescription();
			AssertNoMessageError("Import Declaration", entryInstruction.GoodsLocationDescriptionInfo, errorMessage);
		});
	}
}
