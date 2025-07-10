namespace Enterprise.Customs.CH.Business;

public class MergeManager : Customs.Business.MergeManager
{
	public MergeManager(JobDeclaration declaration)
		: base(declaration)
	{
	}
	protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected override Customs.Business.LineMerger GetNewLineMergerCore() => new LineMerger(Declaration);

	protected override bool RequiresMergeCore => !IsExportDeclarationActivationOrWasExportDeclaration && base.RequiresMergeCore;

	protected override bool SupportsAutoMergeCore => !IsExportDeclarationActivationOrWasExportDeclaration && base.SupportsAutoMergeCore;

	bool IsExportDeclarationActivationOrWasExportDeclaration => (Declaration?.IsExportDeclarationActivation ?? false) || (Declaration?.WasExportDeclaration ?? false);
}
