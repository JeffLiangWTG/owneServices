using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class CusFiscalReferenceValidation : EU.Business.Declaration.CusFiscalReferenceValidation
{
	public CusFiscalReferenceValidation(EU.Business.Declaration.CusFiscalReference parent)
		: base(parent)
	{ }

	protected override void CheckCFR_Reference()
	{
		base.CheckCFR_Reference();
		var parent = Parent;

		if (parent.CFR_Code == FiscalReferenceCodeList.Codes.Buyer
			&& parent.CFR_Reference.StartsWith(Core.Constants.CountryCodes.Italy, System.StringComparison.OrdinalIgnoreCase)
			&& (parent.Instruction ?? parent.InvoiceLine?.EntryInstruction) is CusEntryInstruction instruction
			&& (!instruction.RandomProcedure?.IsCalculateVAT ?? ZBool.False))
		{
			parent.CFR_ReferenceInfo.AddMessageError(ValidationCaptions.CusFiscalReference.ReferenceMustNotStartWithIT(instruction.CEI_Procedure));
		}
	}
}
