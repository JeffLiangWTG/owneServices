namespace Enterprise.Customs.EU.ExitControl.Business;

public class CusExitConsignmentPackageConfiguration
{
	public ICusExitConsignmentPackageValidationDecider GetValidationDecider(CusExitConsignmentPackage package) => GetValidationDeciderCore(package);

	protected virtual ICusExitConsignmentPackageValidationDecider GetValidationDeciderCore(CusExitConsignmentPackage package)
	{
		return package switch
		{
			IUcc6ValueProvider { IsUCC6: true } => GetUcc6ValidationDecider(),
			_ => GetBaseValidationDecider()
		};
	}

	protected virtual ICusExitConsignmentPackageValidationDecider GetBaseValidationDecider() => null;

	protected virtual ICusExitConsignmentPackageValidationDecider GetUcc6ValidationDecider() => new CusExitConsignmentPackageUcc6ValidationDecider();
}
