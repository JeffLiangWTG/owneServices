using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CusEntryInstructionExtensionTest : TestCaseWithFactory
{
	public void TestArguments()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertExceptionThrown<ArgumentNullException>(() => ((IEnumerable<CusEntryInstruction>)null).AnyEntryInstructionIsNonWarehouseProcedure());
		AssertExceptionThrown<ArgumentNullException>(() => ((CusEntryInstructionCollection)null).AnyEntryInstructionIsNonWarehouseProcedure());
		AssertNoExceptionThrown(() => (new List<CusEntryInstruction>()).AnyEntryInstructionIsNonWarehouseProcedure());
		AssertNoExceptionThrown(() => declaration.CustomsEntryInstructions.AnyEntryInstructionIsNonWarehouseProcedure());

		AssertExceptionThrown<ArgumentNullException>(() => ((CusEntryInstructionCollection)null).AnyEntryInstructionIsAtCustoms());
		AssertNoExceptionThrown(() => declaration.CustomsEntryInstructions.AnyEntryInstructionIsAtCustoms());

		AssertExceptionThrown<ArgumentNullException>(() => ((CusEntryInstructionCollection)null).AnyEntryInstructionIsAtPlace());
		AssertNoExceptionThrown(() => declaration.CustomsEntryInstructions.AnyEntryInstructionIsAtPlace());

		AssertExceptionThrown<ArgumentNullException>(() => ((CusEntryInstructionCollection)null).AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(ZGuid.Empty));
		AssertNoExceptionThrown(() => declaration.CustomsEntryInstructions.AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(ZGuid.Empty));

		AssertExceptionThrown<ArgumentNullException>(() => ((List<CusEntryInstruction>)null).AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(ZGuid.Empty));
		AssertNoExceptionThrown(() => (new List<CusEntryInstruction>()).AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(ZGuid.Empty));
	}

	public void TestAnyEntryInstructionIsNonWarehouseProcedure()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		Assert("There are no entry instructions", !declaration.CustomsEntryInstructions.AnyEntryInstructionIsNonWarehouseProcedure());
		AssertEquals(declaration.CustomsEntryInstructions.AnyEntryInstructionIsNonWarehouseProcedure(), declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().AnyEntryInstructionIsNonWarehouseProcedure());

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		Assert("There is one non into warehouse entry instruction (empty procedure)", declaration.CustomsEntryInstructions.AnyEntryInstructionIsNonWarehouseProcedure());

		entryInstruction.CEI_Procedure = "71";
		Assert("There is one into warehouse entry instruction", !declaration.CustomsEntryInstructions.AnyEntryInstructionIsNonWarehouseProcedure());

		entryInstruction.CEI_Procedure = "40";
		Assert("There is one non into warehouse entry instruction", declaration.CustomsEntryInstructions.AnyEntryInstructionIsNonWarehouseProcedure());

		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Procedure = "71";
		entryInstruction1.CEI_Procedure = "71";
		Assert("There are no non into warehouse entry instructions", !declaration.CustomsEntryInstructions.AnyEntryInstructionIsNonWarehouseProcedure());

		entryInstruction.CEI_Procedure = "71";
		entryInstruction1.CEI_Procedure = "40";
		Assert("Any of the entry instructions is non into warehouse", declaration.CustomsEntryInstructions.AnyEntryInstructionIsNonWarehouseProcedure());
	}

	public void TestAnyEntryInstructionIsAtCustoms()
	{
		var declaration = Factory.New<JobDeclaration>();
		Assert("There are no entry instructions", !declaration.CustomsEntryInstructions.AnyEntryInstructionIsAtCustoms());

		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		Assert("No entry instructions 'at customs'", !declaration.CustomsEntryInstructions.AnyEntryInstructionIsAtCustoms());

		entryInstruction1.CEI_Style = SADDeclarationTypeList.Codes.ProceduraOrdinariaCODogana;
		Assert("One entry instruction 'at customs'", declaration.CustomsEntryInstructions.AnyEntryInstructionIsAtCustoms());

		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.CEI_Style = SADDeclarationTypeList.Codes.ProceduraOrdinariaCODogana;
		Assert("Two entry instructions 'at customs'", declaration.CustomsEntryInstructions.AnyEntryInstructionIsAtCustoms());

		entryInstruction1.CEI_Style = ZString.Empty;
		entryInstruction2.CEI_Style = ZString.Empty;
		Assert(!declaration.CustomsEntryInstructions.AnyEntryInstructionIsAtCustoms());
		Assert("Both entry instructions are not 'at customs'", !declaration.CustomsEntryInstructions.AnyEntryInstructionIsAtCustoms());
	}

	public void TestAnyEntryInstructionIsAtPlace()
	{
		var declaration = Factory.New<JobDeclaration>();
		Assert("There are no entry instructions", !declaration.CustomsEntryInstructions.AnyEntryInstructionIsAtPlace());

		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		Assert("No entry instructions 'at place'", !declaration.CustomsEntryInstructions.AnyEntryInstructionIsAtPlace());

		entryInstruction1.CEI_Style = SADDeclarationTypeList.Codes.ProceduraOrdinariaCOLuogo;
		Assert("One entry instruction 'at place'", declaration.CustomsEntryInstructions.AnyEntryInstructionIsAtPlace());

		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.CEI_Style = SADDeclarationTypeList.Codes.ProceduraOrdinariaCOLuogo;
		Assert("Two entry instructions 'at place'", declaration.CustomsEntryInstructions.AnyEntryInstructionIsAtPlace());

		entryInstruction1.CEI_Style = ZString.Empty;
		entryInstruction2.CEI_Style = ZString.Empty;
		Assert(!declaration.CustomsEntryInstructions.AnyEntryInstructionIsAtPlace());
		Assert("Both entry instructions are not 'at place'", !declaration.CustomsEntryInstructions.AnyEntryInstructionIsAtPlace());
	}

	public void TestAnyEntryInstructionIsMissingDPO()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("There are no entry instructions", false, declaration.CustomsEntryInstructions.AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(ZGuid.BrettsGuid));

		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		AssertEquals("DPO authorization not added", true, declaration.CustomsEntryInstructions.AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(ZGuid.BrettsGuid));

		var cusAuthorizationUsage1 = entryInstruction1.CusAuthorizationUsages.AddNew();
		cusAuthorizationUsage1.AGC_Code = "DPO";
		cusAuthorizationUsage1.AGC_OH_Owner = ZGuid.BrettsGuid;
		AssertEquals("DPO authorization added", false, declaration.CustomsEntryInstructions.AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(ZGuid.BrettsGuid));

		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		AssertEquals("With multiple entry instructions and DPO authorization missing for one", true, declaration.CustomsEntryInstructions.AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(ZGuid.BrettsGuid));

		var cusAuthorizationUsage2 = entryInstruction2.CusAuthorizationUsages.AddNew();
		cusAuthorizationUsage2.AGC_Code = "DPO";
		cusAuthorizationUsage2.AGC_OH_Owner = ZGuid.BrettsGuid;
		AssertEquals("With multiple entry instructions and DPO authorizations added", false, declaration.CustomsEntryInstructions.AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(ZGuid.BrettsGuid));
	}

	public void TestAnyEntryInstructionWithDPOAndEmptyOwner()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var cusAuthorizationUsage1 = entryInstruction1.CusAuthorizationUsages.AddNew();
		cusAuthorizationUsage1.AGC_Code = "DPO";
		AssertEquals("DPO authorization added with empty owner", true, declaration.CustomsEntryInstructions.AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(ZGuid.BrettsGuid));

		cusAuthorizationUsage1.AGC_OH_Owner = ZGuid.BrettsGuid;
		AssertEquals("DPO authorization added with owner", false, declaration.CustomsEntryInstructions.AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(ZGuid.BrettsGuid));
		AssertEquals("DPO authorization added with owner and owner from declaration is empty", true, declaration.CustomsEntryInstructions.AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(ZGuid.Empty));

		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		var cusAuthorizationUsage2 = entryInstruction2.CusAuthorizationUsages.AddNew();
		cusAuthorizationUsage2.AGC_Code = "DPO";
		AssertEquals("With multiple entry instructions and DPO authorization owner empty for one", true, declaration.CustomsEntryInstructions.AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(ZGuid.BrettsGuid));

		cusAuthorizationUsage2.AGC_OH_Owner = ZGuid.BrettsGuid;
		AssertEquals("With multiple entry instructions and DPO authorization owner added for all", false, declaration.CustomsEntryInstructions.AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(ZGuid.BrettsGuid));
		AssertEquals("With multiple entry instructions and DPO authorization owner added for all, owner from declaration is empty", true, declaration.CustomsEntryInstructions.AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(ZGuid.Empty));
	}

	public void TestAnyEntryInstructionWithDPOAndDifferentOwnerThanDeclaration()
	{
		var owner1 = ZGuid.NewZGuid();
		var owner2 = ZGuid.NewZGuid();

		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var cusAuthorizationUsage1 = entryInstruction1.CusAuthorizationUsages.AddNew();
		cusAuthorizationUsage1.AGC_Code = "DPO";
		cusAuthorizationUsage1.AGC_OH_Owner = owner1;
		AssertEquals("DPO authorization added with same owner as declaration", false, declaration.CustomsEntryInstructions.AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(owner1));
		AssertEquals("DPO authorization added with different owner as declaration", true, declaration.CustomsEntryInstructions.AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(owner2));

		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		var cusAuthorizationUsage2 = entryInstruction2.CusAuthorizationUsages.AddNew();
		cusAuthorizationUsage2.AGC_Code = "DPO";
		cusAuthorizationUsage2.AGC_OH_Owner = owner2;
		AssertEquals("With multiple entry instructions and DPO authorizations owner different for one", true, declaration.CustomsEntryInstructions.AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(owner1));

		cusAuthorizationUsage2.AGC_OH_Owner = owner1;
		AssertEquals("With multiple entry instructions and DPO authorizations same owner and as same as declaration", false, declaration.CustomsEntryInstructions.AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(owner1));
		AssertEquals("With multiple entry instructions and DPO authorizations same owner and different declaration owner", true, declaration.CustomsEntryInstructions.AnyEntryInstructionIsMissingDPOAuthorizationOrEmptyOrDifferentOwner(owner2));
	}

	protected override void SetUp()
	{
		base.SetUp();
		SetUpRefData();
	}

	void SetUpRefData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure("IT", "IM", "40", "", "", "Procedure1", "IMP", intoWarehouse: false);
		helper.CreateRefCusProcedure("IT", "IM", "71", "", "", "Procedure1", "IMP", intoWarehouse: true);
	}
}
