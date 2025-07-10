using System.Linq;
using Enterprise.Customs.IT.Business.Declaration;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.Business;

public sealed class CusAuthorizationUsageValidation : EU.Business.CusAuthorizationUsageValidation
{
	public CusAuthorizationUsageValidation(CusAuthorizationUsage parent) : base(parent)
	{
	}

	public new CusAuthorizationUsage Parent => (CusAuthorizationUsage)base.Parent;

	#region CheckAGC_OH_Owner

	protected override void CheckAGC_OH_Owner()
	{
		base.CheckAGC_OH_Owner();

		var parent = Parent;
		CheckOwnerHasEoriCode(parent);
		CheckOwnerMissingForEntryInstructionAuthorizationRuleC0848(parent);
		CheckOwnerMissingForEntryInstructionAuthorizationRuleG0089(parent);
	}

	void CheckOwnerMissingForEntryInstructionAuthorizationRuleC0848(CusAuthorizationUsage parent)
	{
		var declaration = parent?.Instruction?.JobDeclaration;

		if (declaration == null)
		{
			return;
		}

		if (parent.AGC_OH_Owner.IsEmpty
			&& declaration.IsUCC6AndIsExport
			&& IsAuthorizationTypeBindingTariffOrOriginInformation(parent.AGC_Code))
		{
			parent.AGC_OH_OwnerInfo.AddMessageError(ValidationCaptions.CusAuthorizationUsage.YouHaveNotEnteredAnOwnerC0848);
		}
	}

	void CheckOwnerMissingForEntryInstructionAuthorizationRuleG0089(CusAuthorizationUsage parent)
	{
		var declaration = parent?.Instruction?.JobDeclaration;

		if (declaration == null)
		{
			return;
		}

		if (parent.AGC_OH_Owner.IsEmpty
			&& declaration.IsUCC6AndIsExport
			&& declaration.IsMergeDone
			&& AreAllInvoiceLinesAuthorizationsBOIOrBTI(parent.Instruction as CusEntryInstruction))
		{
			parent.AGC_OH_OwnerInfo.AddMessageError(ValidationCaptions.CusAuthorizationUsage.YouHaveNotEnteredAnOwnerG0089);
		}
	}

	void CheckOwnerHasEoriCode(CusAuthorizationUsage parent)
	{
		var owner = parent.Owner;
		if (owner != null && owner.GetEoriCode().IsEmpty)
		{
			parent.AGC_OH_OwnerInfo.AddMessageError(ValidationCaptions.CusAuthorizationUsage.EoriCodeIsRequired);
		}
	}

	bool AreAllInvoiceLinesAuthorizationsBOIOrBTI(CusEntryInstruction entryInstruction)
	{
		var invoiceLines = entryInstruction.InvoiceLines.Cast<JobComInvoiceLine>();

		bool areAllAuthorizationsBOIOrBTI = !invoiceLines
			.SelectMany(x => x.CusAuthorizationUsages)
			.Any(a => IsAuthorizationNorBOINorBTI(a.AGC_Code));

		bool IsAuthorizationNorBOINorBTI(string code) => !IsBindingTariffInformation(code) && !IsBindingOriginInformation(code);
		bool isAtLeastOneAuthorizationForEachInvoiceLine = invoiceLines.All(x => x.CusAuthorizationUsages.Any());

		return areAllAuthorizationsBOIOrBTI && isAtLeastOneAuthorizationForEachInvoiceLine;
	}

	static bool IsAuthorizationTypeBindingTariffOrOriginInformation(string code) => IsBindingTariffInformation(code) || IsBindingOriginInformation(code);

	static bool IsBindingOriginInformation(string code) => code == CusAuthorisationHeaderType.BindingOriginInformation;

	static bool IsBindingTariffInformation(string code) => code == CusAuthorisationHeaderType.BindingTariffInformation;

	#endregion
}
