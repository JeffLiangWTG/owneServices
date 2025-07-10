using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class CusAuthorizationUsageValidation : EU.Business.CusAuthorizationUsageValidation
{
	public CusAuthorizationUsageValidation(CusAuthorizationUsage parent) : base(parent)
	{
	}
	public new CusAuthorizationUsage Parent => (CusAuthorizationUsage)base.Parent;

	public override void ValidateAll()
	{
		Parent.ClearRowNotifications();
		base.ValidateAll();
		ValidateAuthorizationUsage_Code();
	}

	public void ValidateAuthorizationUsage_Code()
	{
		if (Parent.Instruction is CusEntryInstruction entryInstruction)
		{
			CusAuthorizationUsageValidationHelper.CheckAgcCodeMOP(entryInstruction, Parent, CusAuthorizationUsages);
		}
		else if (Parent.InvoiceLine is JobComInvoiceLine invoiceLine)
		{
			CusAuthorizationUsageValidationHelper.CheckAgcCodeMOP(invoiceLine.EntryInstruction, Parent, CusAuthorizationUsages);
		}
	}

	protected override void CheckAGC_Code()
	{
		base.CheckAGC_Code();
		ValidateTypeCodeAndReferenceNumberUnique();
	}

	protected override void CheckAGC_Number()
	{
		base.CheckAGC_Number();
		ValidateTypeCodeAndReferenceNumberUnique();
	}

	void ValidateTypeCodeAndReferenceNumberUnique()
	{
		var parent = Parent;
		var errorMessage = Res.GetString("C9F7C51A-E42A-41EB-A464-95D11AFC4E73", "[R9011] Combination of Type and Reference must be unique for authorizations.");
		var errMessageForR9010 = Res.GetString("A5F57151-A3A7-478E-9DD5-FA2D820F3061", "[R9010] Combination Type and Authorization number must be unique.");
		parent.RemoveRowMessageError(errorMessage);
		parent.RemoveRowMessageError(errMessageForR9010);

		var typeCode = parent.AGC_Code;
		var reference = parent.AGC_Number;

		if (parent.InvoiceLine is JobComInvoiceLine invoiceLine && !typeCode.IsEmpty && !reference.IsEmpty && invoiceLine.CusAuthorizationUsages.Any(x => x.PK != parent.PK && x.AGC_Code.Equals(typeCode) && x.AGC_Number.Equals(reference)))
		{
			parent.AddRowMessageError(errorMessage);
		}

		if (parent.Instruction is CusEntryInstruction entryInstruction && !typeCode.IsEmpty && !reference.IsEmpty && entryInstruction.CusAuthorizationUsages.Any(x => x.PK != parent.PK && x.AGC_Code.Equals(typeCode) && x.AGC_Number.Equals(reference)))
		{
			parent.AddRowMessageError(errMessageForR9010);
		}
	}

	IEnumerable<CusAuthorizationUsage> CusAuthorizationUsages
	{
		get
		{
			yield return Parent;
		}
	}
}
