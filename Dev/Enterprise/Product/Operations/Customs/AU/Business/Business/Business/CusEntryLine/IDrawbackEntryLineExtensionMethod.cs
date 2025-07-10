using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class IDrawbackEntryLineExtensionMethod
	{
		public static JobComInvoiceLine[] GetInvoiceLines(this IDrawbackEntryLine entryLine)
		{
			return entryLine.Factory.GetCachedValue("JobComInvocieLineFor" + entryLine.PK, () =>
			{
				return entryLine.Factory.Load<JobComInvoiceLine>(new ZQuery(JobComInvoiceLineSchema.JI_CL, entryLine.PK));
			});
		}
	}
}
