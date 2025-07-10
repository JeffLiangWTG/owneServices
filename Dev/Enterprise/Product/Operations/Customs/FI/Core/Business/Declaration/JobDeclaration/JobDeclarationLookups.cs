namespace Enterprise.Customs.FI.Business;

public class JobDeclarationLookups : EU.Business.Declaration.JobDeclarationLookups
{
	public JobDeclarationLookups(JobDeclaration parent)
		: base(parent)
	{
	}

	protected new JobDeclaration Parent => (JobDeclaration)base.Parent;
}
