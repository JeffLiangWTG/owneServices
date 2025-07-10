using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class MergeManager : Customs.Business.MergeManager
	{
		public MergeManager(BaseJobDeclaration decl) : base(decl)
		{
		}

		protected override string GetReasonCannotMerge()
		{
			var result = base.GetReasonCannotMerge();

			if (string.IsNullOrEmpty(result))
			{
				if (!Declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(l => l.EntryInstruction != null))
				{
					result = Res.GetString("7dda4ce8-482b-47c6-8c56-8b8dba79aaac", "Cannot merge as no invoice line has an entry instruction assigned.");
				}
			}
			return result;
		}

		protected override Customs.Business.LineMerger GetNewLineMergerCore()
		{
			var declaration = (JobDeclaration)Declaration;
			return new LineMerger(declaration);
		}
	}
}
