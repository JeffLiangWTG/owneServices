namespace Enterprise.Customs.EU.ExitControl.Business;

public class CusExitContainerConfiguration
{
	public ICusExitContainerValidationDecider GetValidationDecider(CusExitContainer container) => GetValidationDeciderCore(container);

	protected virtual ICusExitContainerValidationDecider GetValidationDeciderCore(CusExitContainer container)
	{
		return container switch
		{
			IUcc6ValueProvider { IsUCC6: true } => GetUcc6ValidationDecider(),
			_ => GetBaseValidationDecider()
		};
	}

	protected virtual ICusExitContainerValidationDecider GetBaseValidationDecider() => null;

	protected virtual ICusExitContainerValidationDecider GetUcc6ValidationDecider() => new CusExitContainerUcc6ValidationDecider();
}
