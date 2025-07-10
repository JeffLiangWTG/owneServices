using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business.Declaration;

public class EntryInstructionCusFiscalReferenceValidation : CusFiscalReferenceValidation
{
	public EntryInstructionCusFiscalReferenceValidation(CusFiscalReference parent)
		: base(parent)
	{
	}

	protected override void CheckCFR_Code()
	{
		base.CheckCFR_Code();
		var entryInstruction = Parent.Instruction;
		var isF48ErrorAppliccable = false;
		var isF49ErrorAppliccable = false;
		var isNotFR5AndHaseOneF48 = false;
		var isNotFR5AndHasSomeOtherAdditionalProcedure = false;
		foreach (var invoiceLine in entryInstruction.InvoiceLines)
		{
			if (invoiceLine is JobComInvoiceLine jobComInvoiceLine)
			{
				var additionalProcedure = jobComInvoiceLine.AdditionalProcedureCode;

				if (additionalProcedure == NLConstants.ProcedureCodes.Concession.F49)
				{
					isF49ErrorAppliccable = true;
				}

				if (Parent.CFR_Code == FiscalReferenceCodeList.Codes.FR5_Seller && additionalProcedure != NLConstants.ProcedureCodes.Concession.F48)
				{
					isF48ErrorAppliccable = true;
				}

				if (Parent.CFR_Code != FiscalReferenceCodeList.Codes.FR5_Seller && additionalProcedure == NLConstants.ProcedureCodes.Concession.F48)
				{
					isNotFR5AndHaseOneF48 = true;
				}

				if (Parent.CFR_Code != FiscalReferenceCodeList.Codes.FR5_Seller && additionalProcedure != NLConstants.ProcedureCodes.Concession.F48)
				{
					isNotFR5AndHasSomeOtherAdditionalProcedure = true;
				}
			}
		}

		if (isF48ErrorAppliccable)
		{
			Parent.CFR_CodeInfo.AddMessageError(Res.GetString("0581245F-9C9D-44C2-A9EB-094B90809851", "When entering role code FR5, all invoice lines must have additional procedure F48."));
		}

		if (isF49ErrorAppliccable)
		{
			Parent.CFR_CodeInfo.AddMessageError(Res.GetString("A06E0E3C-F21B-4B74-90DE-20AC6C8890B2", "No fiscal reference is allowed to be filled in due to additional procedure F49. Please remove the fiscal reference or change the additional procedure in the invoice line"));
		}

		if (isNotFR5AndHaseOneF48 && !isNotFR5AndHasSomeOtherAdditionalProcedure)
		{
			Parent.CFR_CodeInfo.AddMessageError(Res.GetString("096318EE-9FF9-4CD8-9A8F-77F5C427D248", "When all invoice lines have additional procedure F48, then role code FR5 must be used."));
		}
	}

	protected override void CheckCFR_Reference()
	{
		base.CheckCFR_Reference();

		if (Parent.CFR_Code == FiscalReferenceCodeList.Codes.FR7_ReverseChargeVAT && !Parent.CFR_Reference.StartsWith(Core.Constants.CountryCodes.Netherlands))
		{
			Parent.CFR_ReferenceInfo.AddMessageError(Res.GetString("89602B52-9FA4-42F3-B72F-050A6B1B02C5", "Please fill in a Dutch VAT number starting with NL."));
		}
	}
}
