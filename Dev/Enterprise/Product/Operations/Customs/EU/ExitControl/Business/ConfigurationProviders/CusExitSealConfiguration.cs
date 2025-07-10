namespace Enterprise.Customs.EU.ExitControl.Business;

public class CusExitSealConfiguration
{
	public ICusExitSealValidationDecider GetValidationDecider(CusExitSeal seal) => GetValidationDeciderCore(seal);

	protected virtual ICusExitSealValidationDecider GetValidationDeciderCore(CusExitSeal seal)
	{
		return seal switch
		{
			IUcc6ValueProvider { IsUCC6: true } => GetUcc6ValidationDecider(),
			_ => GetBaseValidationDecider()
		};
	}

	protected virtual ICusExitSealValidationDecider GetBaseValidationDecider() => null;

	protected virtual ICusExitSealValidationDecider GetUcc6ValidationDecider() => new CusExitSealUcc6ValidationDecider();
}
