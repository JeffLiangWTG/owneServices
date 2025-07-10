namespace Enterprise.Customs.IL.Business
{
	public class MergeManager : Customs.Business.MergeManager
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
