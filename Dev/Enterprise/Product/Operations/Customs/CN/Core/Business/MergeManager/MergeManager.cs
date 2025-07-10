namespace Enterprise.Customs.CN.Business
{
	public class MergeManager : Customs.Business.MergeManager
	{
		public MergeManager(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override Customs.Business.LineMerger GetNewLineMergerCore() => new LineMerger(Declaration);

		protected override bool ExecuteCore(Customs.Business.ISendsMessagesToCustoms notifier)
		{
			var result = false;
			if (!Declaration.IsDeclarationIntegrated)
			{
				result = base.ExecuteCore(notifier);
			}
			return result;
		}

		protected override bool SupportsAutoMergeCore => !Declaration.IsDeclarationIntegrated && base.SupportsAutoMergeCore;

		protected override bool RequiresMergeCore => !Declaration.IsDeclarationIntegrated && base.RequiresMergeCore;
	}
}
