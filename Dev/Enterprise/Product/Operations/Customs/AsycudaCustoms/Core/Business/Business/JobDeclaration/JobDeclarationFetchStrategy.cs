using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class JobDeclarationFetchStrategy : Customs.Business.FetchStrategies.BaseJobDeclarationFetchStrategy
	{
		public JobDeclarationFetchStrategy(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override void AddMergeFetchHintsFor(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			base.AddMergeFetchHintsFor(invoiceLine);
			Factory.AddFetchHint(CusSupportingInfoSchema.CSI_ParentID, invoiceLine.PK);
		}
	}
}
