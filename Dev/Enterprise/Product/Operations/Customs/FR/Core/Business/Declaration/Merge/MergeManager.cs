namespace Enterprise.Customs.FR.Business.Declaration
{
	public class MergeManager : EU.Business.Declaration.MergeManager
	{
		public MergeManager(JobDeclaration jobDec)
			: base(jobDec)
		{
		}

		protected override Customs.Business.LineMerger GetNewLineMergerCore()
		{
			var declaration = (JobDeclaration)Declaration;
			return new LineMerger(declaration);
		}
	}
}
