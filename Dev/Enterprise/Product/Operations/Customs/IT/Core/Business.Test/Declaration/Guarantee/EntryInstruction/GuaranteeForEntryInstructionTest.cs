using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(GuaranteeForEntryInstruction))]
sealed class GuaranteeForEntryInstructionTest : EU.Business.Declaration.Testing.GuaranteeForEntryInstructionAbstractTest<GuaranteeForEntryInstruction>
{
	public override void TestLookups()
	{
		var guarantee = (GuaranteeForEntryInstruction)GetNewBusinessObject();
		AssertType<GuaranteeForEntryInstructionLookups>("Lookups Type", guarantee.Lookups);
	}

	public new void TestHumanReadableName()
	{
		var guarantee = (GuaranteeForEntryInstruction)GetNewBusinessObject();
		AssertEquals("HumanReadableName", "Guarantee Reference", guarantee.HumanReadableName);
	}

	public void TestShouldSetupHolderIdentificationOnBondNumber2Change()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		var guarantee = Factory.New<GuaranteeForEntryInstructionForTest>();
		guarantee.PW_ParentID = entryInstruction.PK;
		guarantee.PW_ParentTableCode = CusEntryInstructionSchema.Constants.Prefix;

		AssertEquals("[PRE-CONDITION] CEI_Style Empty", false, guarantee.ShouldSetupHolderIdentificationOnBondNumber2Change_Exposed);

		CombineAssertions("Declaration Type: IMP", () =>
		{
			entryInstruction.CEI_Style = "H1";
			AssertEquals("When CEI_Style=H1", false, guarantee.ShouldSetupHolderIdentificationOnBondNumber2Change_Exposed);

			entryInstruction.CEI_Style = "H3";
			AssertEquals("When CEI_Style=H3", true, guarantee.ShouldSetupHolderIdentificationOnBondNumber2Change_Exposed);

			entryInstruction.CEI_Style = "H2";
			AssertEquals("When CEI_Style=H2", false, guarantee.ShouldSetupHolderIdentificationOnBondNumber2Change_Exposed);

			entryInstruction.CEI_Style = "H4";
			AssertEquals("When CEI_Style=H4", true, guarantee.ShouldSetupHolderIdentificationOnBondNumber2Change_Exposed);
		});

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

		CombineAssertions("Declaration Type: EXP", () =>
		{
			entryInstruction.CEI_Style = "H3";
			AssertEquals("When CEI_Style=H3", false, guarantee.ShouldSetupHolderIdentificationOnBondNumber2Change_Exposed);

			entryInstruction.CEI_Style = "B1";
			AssertEquals("When CEI_Style=B1", false, guarantee.ShouldSetupHolderIdentificationOnBondNumber2Change_Exposed);
		});
	}

	public void TestEntryInstructionType()
	{
		var guarantee = (GuaranteeForEntryInstruction)GetNewBusinessObject();
		AssertType<CusEntryInstruction>("Entry Instruction Type", guarantee.EntryInstruction);
	}

	protected override GuaranteeForEntryInstruction GetNewGuaranteeForEntryInstruction() => Factory.New<CusEntryInstruction>().Guarantees.AddNew();

	protected override Type GetGuaranteeForEntryInstructionValidationType() => typeof(GuaranteeForEntryInstructionValidation);

	sealed class GuaranteeForEntryInstructionForTest : GuaranteeForEntryInstruction
	{
		public GuaranteeForEntryInstructionForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		internal ZBool ShouldSetupHolderIdentificationOnBondNumber2Change_Exposed => ShouldSetupHolderIdentificationOnBondNumber2Change;
	}
}
