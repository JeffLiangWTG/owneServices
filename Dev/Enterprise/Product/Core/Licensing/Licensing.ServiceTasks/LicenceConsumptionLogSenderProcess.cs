using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Licensing.ServiceTasks
{
	public interface ILicenceConsumptionLogProcess
	{
		string Execute(ZDateTime dateFromUtcInclusive, ZDateTime dateToUtcExclusive);
	}

	public class LicenceConsumptionLogSenderProcess : ILicenceConsumptionLogProcess
	{
		public string Execute(ZDateTime dateFromUtcInclusive, ZDateTime dateToUtcExclusive)
		{
			LicenceUsageReportBuilder usageReport = new LicenceUsageReportBuilder(new BusinessObjectFactory() { RefreshEnabled = false });
			usageReport.BuildLatest(dateFromUtcInclusive, dateToUtcExclusive);
			return usageReport.GetCompressedEncryptedText();
		}
	}
}
