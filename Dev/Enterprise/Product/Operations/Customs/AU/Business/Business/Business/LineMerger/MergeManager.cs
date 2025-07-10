namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// For edifice 
	/// </summary>
	public class MergeManager : Customs.Business.MergeManager
	{
		public MergeManager(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override bool PersistsMergeState => false;

		protected override bool SupportsAmendments => false;

		protected override bool SupportsAutoMergeCore => false;// this should be revisited when AU uses merge strategies etc

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override Customs.Business.LineMerger GetNewLineMergerCore() => new LineMerger(Declaration);

		protected override string GetReasonCannotMerge()
		{
			var result = base.GetReasonCannotMerge();
			if (result.Length == 0)
			{
				var declaration = Declaration;
				if (!declaration.IsImport)
				{
					result = "You can only merge lines when doing an import declaration.";
				}
				else if (declaration.HasLodgeBeenSent)
				{
					result = "You can't merge lines because you have already lodged the declaration.";
				}
				else if (!ZArchitecture.Environment.Globals.IsTest)
				{
					result = "You are trying to re-merge a legacy job. As you cannot send messages for legacy jobs, you cannot merge this declaration";
				}
			}
			return result;
		}
	}
}
