using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public class CDSMergeManager : MergeManager
	{
		public CDSMergeManager(JobDeclaration jobDec)
			: base(jobDec)
		{
		}

		protected override Customs.Business.LineMerger GetNewLineMergerCore()
		{
			return new CDSLineMerger((JobDeclaration)Declaration);
		}
	}
}
