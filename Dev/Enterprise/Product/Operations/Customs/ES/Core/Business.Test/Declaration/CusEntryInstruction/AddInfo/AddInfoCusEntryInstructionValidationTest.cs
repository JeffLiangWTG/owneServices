using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business.Declaration.Testing;

sealed class AddInfoCusEntryInstructionValidationTest : BusinessObjectValidationTestCase
{
	public void TestSealCountValidationIsNotPerformed()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		instruction.Seals.AddNew("TEST");
		instruction.ZG_SealsCount = 0;
		AssertEquals("Should be no errors", false, instruction.ZG_SealsCountInfo.HasMessageErrors());
	}

	public void TestCheckZG_RequestTypeRequiredForT2lPousAndExport()
	{
		var errorText = "Request Type is required";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;

		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;

		CombineAssertions(() =>
		{
			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.ProofOfUnionStatus))
			{
				entryInstruction1.AddInfoValidation.ValidateZG_RequestType();
				AssertNoMessageErrorContaining("POUS When Entry Instruction is not T2l and Export and ZG_RequestType empty", entryInstruction1.ZG_RequestTypeInfo, errorText);

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				entryInstruction1.AddInfoValidation.ValidateZG_RequestType();
				AssertHasMessageErrorContaining("POUS When Entry Instruction is T2l and Export and ZG_RequestType empty", entryInstruction1.ZG_RequestTypeInfo, errorText);

				entryInstruction1.ZG_RequestType = "AA";
				entryInstruction1.AddInfoValidation.ValidateZG_RequestType();
				AssertNoMessageErrorContaining("POUS When Entry Instruction is T2l and Export and ZG_RequestType not empty", entryInstruction1.ZG_RequestTypeInfo, errorText);

				entryInstruction1.ZG_RequestType = ZString.Empty;
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				entryInstruction1.AddInfoValidation.ValidateZG_RequestType();
				AssertNoMessageErrorContaining("POUS When Entry Instruction is T2l and Import and ZG_RequestType empty", entryInstruction1.ZG_RequestTypeInfo, errorText);
			}

			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.NoProofOfUnionStatus))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

				entryInstruction1.AddInfoValidation.ValidateZG_RequestType();
				AssertNoMessageErrorContaining("NOPOUS When Entry Instruction is T2l and Export and ZG_RequestType empty", entryInstruction1.ZG_RequestTypeInfo, errorText);
			}

			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.RequestJecAndReceptionPous))
			{
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;
				entryInstruction1.AddInfoValidation.ValidateZG_RequestType();
				AssertNoMessageErrorContaining("POUS2 When Entry Instruction is not T2l and Export and ZG_RequestType empty", entryInstruction1.ZG_RequestTypeInfo, errorText);

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				entryInstruction1.AddInfoValidation.ValidateZG_RequestType();
				AssertHasMessageErrorContaining("POUS2 When Entry Instruction is T2l and Export and ZG_RequestType empty", entryInstruction1.ZG_RequestTypeInfo, errorText);

				entryInstruction1.ZG_RequestType = "AA";
				entryInstruction1.AddInfoValidation.ValidateZG_RequestType();
				AssertNoMessageErrorContaining("POUS2 When Entry Instruction is T2l and Export and ZG_RequestType not empty", entryInstruction1.ZG_RequestTypeInfo, errorText);

				entryInstruction1.ZG_RequestType = ZString.Empty;
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				entryInstruction1.AddInfoValidation.ValidateZG_RequestType();
				AssertNoMessageErrorContaining("POUS2 When Entry Instruction is T2l and Import and ZG_RequestType empty", entryInstruction1.ZG_RequestTypeInfo, errorText);
			}
		});
	}

	public void TestCheckZG_RequestTypeNot02AndAuthorizationTypeACPForT2lPousAndExport()
	{
		var errorText = "Authorization Type ACP (C511) is required for Request Type 02";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;

		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
		entryInstruction1.ZG_RequestType = RequestTypeList.Codes.RegistrationRequest;

		CombineAssertions(() =>
		{
			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.ProofOfUnionStatus))
			{
				entryInstruction1.AddInfoValidation.ValidateZG_RequestType();
				AssertHasMessageErrorContaining("POUS When the Entry Instruction is T2l and Export and ZG_RequestType = 02 and no authorizations define", entryInstruction1.ZG_RequestTypeInfo, errorText);

				var auth1 = entryInstruction1.CusAuthorizationUsages.AddNew();
				auth1.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;

				var auth2 = entryInstruction1.CusAuthorizationUsages.AddNew();
				auth2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedIssuer;

				entryInstruction1.ZG_RequestType = RequestTypeList.Codes.RegistrationRequest;
				entryInstruction1.AddInfoValidation.ValidateZG_RequestType();
				AssertNoMessageErrorContaining("POUS When the Entry Instruction is T2l and Export and ZG_RequestType = 02 and authorizations ACP define", entryInstruction1.ZG_RequestTypeInfo, errorText);

				auth2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				entryInstruction1.AddInfoValidation.ValidateZG_RequestType();
				AssertHasMessageErrorContaining("POUS When the Entry Instruction is T2l and Export and ZG_RequestType = 02 and authorizations ACP not define", entryInstruction1.ZG_RequestTypeInfo, errorText);

				entryInstruction1.ZG_RequestType = RequestTypeList.Codes.EndorsementRequest;
				entryInstruction1.AddInfoValidation.ValidateZG_RequestType();
				AssertNoMessageErrorContaining("POUS When the Entry Instruction is T2l and Export and ZG_RequestType not 02 and authorizations ACP not define", entryInstruction1.ZG_RequestTypeInfo, errorText);

				entryInstruction1.ZG_RequestType = RequestTypeList.Codes.RegistrationRequest;
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				entryInstruction1.AddInfoValidation.ValidateZG_RequestType();
				AssertNoMessageErrorContaining("POUS When the Entry Instruction is T2l and Import and ZG_RequestType = 02 and authorizations ACP not define", entryInstruction1.ZG_RequestTypeInfo, errorText);

				entryInstruction1.ZG_RequestType = RequestTypeList.Codes.RegistrationRequest;
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;
				entryInstruction1.AddInfoValidation.ValidateZG_RequestType();
				AssertNoMessageErrorContaining("POUS When the Entry Instruction is not T2l and Export and ZG_RequestType = 02 and authorizations ACP not define", entryInstruction1.ZG_RequestTypeInfo, errorText);
			}

			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.NoProofOfUnionStatus))
			{
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

				var auth1 = entryInstruction1.CusAuthorizationUsages.AddNew();
				auth1.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;

				entryInstruction1.AddInfoValidation.ValidateZG_RequestType();
				AssertNoMessageErrorContaining("NOPOUS When Entry Instruction is T2l and Export and ZG_RequestType = 02 and authorizations ACP not define", entryInstruction1.ZG_RequestTypeInfo, errorText);
			}

			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.RequestJecAndReceptionPous))
			{
				entryInstruction1.CusAuthorizationUsages.RemoveAndDeleteAll();

				entryInstruction1.AddInfoValidation.ValidateZG_RequestType();
				AssertHasMessageErrorContaining("POUS2 When the Entry Instruction is T2l and Export and ZG_RequestType = 02 and no authorizations define", entryInstruction1.ZG_RequestTypeInfo, errorText);

				var auth1 = entryInstruction1.CusAuthorizationUsages.AddNew();
				auth1.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;

				var auth2 = entryInstruction1.CusAuthorizationUsages.AddNew();
				auth2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedIssuer;

				entryInstruction1.ZG_RequestType = RequestTypeList.Codes.RegistrationRequest;
				entryInstruction1.AddInfoValidation.ValidateZG_RequestType();
				AssertNoMessageErrorContaining("POUS2 When the Entry Instruction is T2l and Export and ZG_RequestType = 02 and authorizations ACP define", entryInstruction1.ZG_RequestTypeInfo, errorText);

				auth2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				entryInstruction1.AddInfoValidation.ValidateZG_RequestType();
				AssertHasMessageErrorContaining("POUS2 When the Entry Instruction is T2l and Export and ZG_RequestType = 02 and authorizations ACP not define", entryInstruction1.ZG_RequestTypeInfo, errorText);

				entryInstruction1.ZG_RequestType = RequestTypeList.Codes.EndorsementRequest;
				entryInstruction1.AddInfoValidation.ValidateZG_RequestType();
				AssertNoMessageErrorContaining("POUS2 When the Entry Instruction is T2l and Export and ZG_RequestType not 02 and authorizations ACP not define", entryInstruction1.ZG_RequestTypeInfo, errorText);

				entryInstruction1.ZG_RequestType = RequestTypeList.Codes.RegistrationRequest;
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				entryInstruction1.AddInfoValidation.ValidateZG_RequestType();
				AssertNoMessageErrorContaining("POUS2 When the Entry Instruction is T2l and Import and ZG_RequestType = 02 and authorizations ACP not define", entryInstruction1.ZG_RequestTypeInfo, errorText);

				entryInstruction1.ZG_RequestType = RequestTypeList.Codes.RegistrationRequest;
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;
				entryInstruction1.AddInfoValidation.ValidateZG_RequestType();
				AssertNoMessageErrorContaining("POUS2 When the Entry Instruction is not T2l and Export and ZG_RequestType = 02 and authorizations ACP not define", entryInstruction1.ZG_RequestTypeInfo, errorText);
			}
		});
	}
}
