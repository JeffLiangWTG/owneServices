using System.Linq;

namespace Enterprise.Customs.BE.Business.Declaration;

public class InvoiceLineCompleteCollection : EU.Business.Declaration.InvoiceLineCompleteCollection
{
	public InvoiceLineCompleteCollection(JobDeclaration jobDeclaration)
		: base(jobDeclaration)
	{
	}

	public new JobComInvoiceLine this[int index] => (JobComInvoiceLine)Elements[index];

	public new JobComInvoiceLine AddNew() => (JobComInvoiceLine)base.AddNew();

	public bool AllInvoiceLinesHaveProcedureStartingWithString(params string[] startOfProcedure) => this.OfType<JobComInvoiceLine>().All(i => startOfProcedure.Any(p => i.JI_FormattedProcedure.StartsWith(p)));

	public bool AnyInvoiceLinesHaveProcedureStartingWithString(params string[] startOfProcedure) => this.OfType<JobComInvoiceLine>().Any(i => startOfProcedure.Any(p => i.JI_FormattedProcedure.StartsWith(p)));
}
