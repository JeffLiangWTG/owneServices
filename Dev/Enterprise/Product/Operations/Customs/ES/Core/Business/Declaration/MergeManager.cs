namespace Enterprise.Customs.ES.Business.Declaration
{
	public class MergeManager : EU.Business.Declaration.MergeManager
	{
		public MergeManager(JobDeclaration jobDec)
			: base(jobDec)
		{
		}

		protected override Customs.Business.LineMerger GetNewLineMergerCore() => new LineMerger((JobDeclaration)Declaration);
	}
}
