namespace Enterprise.Customs.EU.ExitControl.Business;

public class CusExitReportConfiguration
{
	public ICusExitReportValidationDecider GetValidationDecider(CusExitReport report) => GetValidationDeciderCore(report);

	protected virtual ICusExitReportValidationDecider GetValidationDeciderCore(CusExitReport report)
	{
		return report switch
		{
			IUcc6ValueProvider { IsUCC6: true } => GetUcc6ValidationDecider(),
			_ => GetBaseValidationDecider()
		};
	}

	protected virtual ICusExitReportValidationDecider GetBaseValidationDecider() => null;

	protected virtual ICusExitReportValidationDecider GetUcc6ValidationDecider() => new CusExitReportUcc6ValidationDecider();
}
