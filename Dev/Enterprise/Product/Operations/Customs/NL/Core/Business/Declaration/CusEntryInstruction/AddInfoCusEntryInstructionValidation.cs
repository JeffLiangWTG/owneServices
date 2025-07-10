using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NL.Business.Declaration;

public class AddInfoCusEntryInstructionValidation : EU.Business.Declaration.AddInfoCusEntryInstructionValidation
{
	public AddInfoCusEntryInstructionValidation(AddInfoCusEntryInstruction parent) : base(parent)
	{
	}

	protected new AddInfoCusEntryInstruction Parent => (AddInfoCusEntryInstruction)base.Parent;

	protected new AddInfoCusEntryInstructionLookups Lookups => new(Parent);

	protected override void CheckZG_TransNature()
	{
		base.CheckZG_TransNature();

		if (Parent.Parent.HasEmptyTransactionNatureForMessage && Parent.Parent.InvoiceLines.Cast<JobComInvoiceLine>().All(l => l.ZG_TransNature.IsEmpty) &&
			Parent.Parent.Invoices.Cast<JobComInvoiceHeader>().All(l => l.JZ_ValuationCode.IsEmpty))
		{
				Parent.ZG_TransNatureInfo.AddMessageError(Res.GetString("fcd1da31-43bc-40c9-8aa8-20426021193c", "Transaction nature must be filled for declarations with type B1, B2, C1, H1, H3, H4, H5 or I1"));
		}

		else if (Parent.Parent.HasNonEmptyTransactionNatureForMessage && !Parent.ZG_TransNature.IsEmpty && (Parent.Parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(l => !l.ZG_TransNature.IsEmpty) || Parent.Parent.Invoices.Cast<JobComInvoiceHeader>().Any(l => !l.JZ_ValuationCode.IsEmpty)))
		{
				Parent.ZG_TransNatureInfo.AddMessageError(Res.GetString("967a78f0-6204-4692-8496-fe9b82a11e69", "Transaction nature must be filled at entry instruction OR at invoice."));
		}

		ListValidation.MessageErrorIfInvalidCode(Parent.ZG_TransNatureInfo, Lookups.TransNatureList);
	}
}
