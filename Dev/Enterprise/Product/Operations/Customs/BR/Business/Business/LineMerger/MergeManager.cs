namespace Enterprise.Customs.BR.Business
{
	public class MergeManager : Customs.Business.MergeManager
	{
		public MergeManager(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override bool SupportsAutoMergeCore
		{
			get { return !Declaration.IsDeclarationIntegrated && base.SupportsAutoMergeCore; }
		}

		#region Implementation

		protected override Customs.Business.LineMerger GetNewLineMergerCore()
		{
			return new LineMerger(Declaration);
		}

		protected new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		#endregion
	}
}
