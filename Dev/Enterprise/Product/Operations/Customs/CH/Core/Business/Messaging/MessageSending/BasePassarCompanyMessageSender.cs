using Enterprise.BatchProcessor;

namespace Enterprise.Customs.CH.Business;

public abstract class BasePassarCompanyMessageSender : BaseCompanyMessageSender<GlbCompanyTokenCredentials>
{
	public BasePassarCompanyMessageSender(LoggingInformation logger) : base(logger)
	{
	}

	protected override GlbCompanyTokenCredentials GetCompanyCredentials(GlbCompanyWrapper companyWrapper) => companyWrapper.TokenCredentials;
}
