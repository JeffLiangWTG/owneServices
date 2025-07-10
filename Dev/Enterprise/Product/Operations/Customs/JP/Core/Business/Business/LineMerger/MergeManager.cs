using System.Linq;

namespace Enterprise.Customs.JP.Business
{
	public class MergeManager : Customs.Business.MergeManager
	{
		public MergeManager(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override bool ShouldCheckExistenceOfInvoices => !Declaration.IsECRSendingInProgress && !HasGeneratedEntryWithoutHeaderForECR;

		protected override bool ShouldCheckExistenceOfInvoiceLineForAllInvoices => !Declaration.IsECRSendingInProgress && !HasGeneratedEntryWithoutHeaderForECR;

		bool HasGeneratedEntryWithoutHeaderForECR => Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Any(x => x.InvoiceHeaders().Length == 0);

		protected override Customs.Business.LineMerger GetNewLineMergerCore()
		{
			return new LineMerger(Declaration);
		}

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;
	}
}
