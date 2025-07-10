using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public class GuaranteeForEntryInstruction : EU.Business.Declaration.GuaranteeForEntryInstruction
{
	public GuaranteeForEntryInstruction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

	public new GuaranteeForEntryInstructionLookups Lookups => (GuaranteeForEntryInstructionLookups)base.Lookups;

	protected override MasterFiles.Business.CusBondDetailLookups GetNewLookups() => new GuaranteeForEntryInstructionLookups(this);

	public new GuaranteeForEntryInstructionValidation Validation => (GuaranteeForEntryInstructionValidation)base.Validation;

	protected override MasterFiles.Business.CusBondDetailValidation GetNewValidation() => new GuaranteeForEntryInstructionValidation(this);

	protected override ZString HumanReadableNameCore => Res.GetString("ECB70829-F1EB-4D0C-AC1C-887932F31441", "Guarantee Reference");

	protected override ZBool ShouldSetupHolderIdentificationOnBondNumber2Change
		=> EntryInstruction is CusEntryInstruction itCusEntryInstruction && itCusEntryInstruction.RequireGuaranteeNumberInReference2;
}
