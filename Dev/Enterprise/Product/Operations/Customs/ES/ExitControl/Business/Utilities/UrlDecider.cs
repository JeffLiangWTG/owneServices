using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Registry;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class UrlDecider
	{
		public UrlDecider(CusExitReport exitReport)
		{
			this.exitReport = Argument.NotNull(exitReport, nameof(exitReport));
			_ = Argument.NotNull(exitReport.Consignment, nameof(exitReport.Consignment));
		}
		readonly CusExitReport exitReport;

		public ZString GetUrlExitReport()
		{
			if (exitReport.CanLaunchExitReportUrl())
			{
				return ComposeUrl(ESCustomsDataRegistry.Instance.ExitControlStatusQueryUrl.Value, exitReport.Consignment.CXC_MovementReference);
			}
			else
			{
				return ZString.Empty;
			}
		}

		static string ComposeUrl(ZString url, ZString mrn)
			=> url.Replace(CustomsWebsiteUrlCodes.MRNinRegistryUrl, mrn);
	}
}
