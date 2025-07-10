namespace Enterprise.Customs.GB.Business.Declaration
{
	public class MergeManager : EU.Business.Declaration.MergeManager
	{
		public MergeManager(JobDeclaration jobDec)
			: base(jobDec)
		{
		}

		protected override Customs.Business.LineMerger GetNewLineMergerCore()
		{
			return new LineMerger((JobDeclaration)Declaration);
		}
	}
}


