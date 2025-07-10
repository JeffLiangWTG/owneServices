using System.Linq;

namespace Enterprise.Customs.NL.Business.Declaration;

public class JobComInvoiceHeaderValidation : EU.Business.Declaration.JobComInvoiceHeaderValidation
{
	public JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
	{
	}

	protected override bool IsJZ_ValuationCodeMandatory => !Parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(i => i.EntryInstruction != null && ((!i.ZG_TransNature.IsEmpty && i.EntryInstruction.IsValidForMessage) || i.EntryInstruction.HasNonEmptyTransactionNatureForMessage)) && !Parent.InvoiceLines.Cast<JobComInvoiceLine>().All(i => i.EntryInstruction == null || !i.EntryInstruction.IsValidForMessage);

	protected override void CheckJZ_ValuationCode()
	{
		if (IsJZ_ValuationCodeMandatory)
		{
			base.CheckJZ_ValuationCode();
		}
	}

	protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;
}
