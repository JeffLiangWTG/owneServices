using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class JobDeclarationFetchStrategy : EU.Business.FetchStrategies.JobDeclarationFetchStrategy
	{
		public JobDeclarationFetchStrategy(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		protected new JobDeclaration BusinessObject
		{
			get { return (JobDeclaration)base.BusinessObject; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(CusEntryHeaderSchema.CH_JE, BusinessObject.PK);
			Factory.AddFetchHint(CusEntryInstructionSchema.CEI_JE, BusinessObject.PK);
		}
	}
}
