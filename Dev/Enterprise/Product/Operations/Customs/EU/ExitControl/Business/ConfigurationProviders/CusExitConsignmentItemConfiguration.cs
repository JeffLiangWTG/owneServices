namespace Enterprise.Customs.EU.ExitControl.Business;

public class CusExitConsignmentItemConfiguration
{
	public ICusExitConsignmentItemValidationDecider GetValidationDecider(CusExitConsignmentItem item) => GetValidationDeciderCore(item);

	protected virtual ICusExitConsignmentItemValidationDecider GetValidationDeciderCore(CusExitConsignmentItem item)
	{
		return item switch
		{
			IUcc6ValueProvider { IsUCC6: true } => GetUcc6ValidationDecider(),
			_ => GetBaseValidationDecider()
		};
	}

	protected virtual ICusExitConsignmentItemValidationDecider GetBaseValidationDecider() => null;

	protected virtual ICusExitConsignmentItemUcc6ValidationDecider GetUcc6ValidationDecider() => new CusExitConsignmentItemUcc6ValidationDecider();
}
