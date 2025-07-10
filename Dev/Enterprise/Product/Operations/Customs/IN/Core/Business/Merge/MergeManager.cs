namespace Enterprise.Customs.IN.Business;

public class MergeManager : Customs.Business.MergeManager
{
	public MergeManager(JobDeclaration declaration)
			: base(declaration)
	{
	}

	protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected override Customs.Business.LineMerger GetNewLineMergerCore() => new LineMerger(Declaration);
}
