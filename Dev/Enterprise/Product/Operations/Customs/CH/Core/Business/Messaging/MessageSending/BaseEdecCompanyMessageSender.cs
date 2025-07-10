using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public abstract class BaseEdecCompanyMessageSender : BaseCompanyMessageSender<GlbExternalPassword>
{
	public BaseEdecCompanyMessageSender(LoggingInformation logger) : base(logger)
	{
	}

	protected override GlbExternalPassword GetCompanyCredentials(GlbCompanyWrapper companyWrapper) => companyWrapper.GlbExternalPassword;
}
