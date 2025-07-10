using System.Collections.Generic;
using System.Threading;
using CargoWise.Application;
using Enterprise.Customs.EU.NCTS.ServiceTasks;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	NctsDownloaderServiceTask.Code,
	NctsDownloaderServiceTask.FriendlyName,
	"EUC",
	typeof(NctsDownloaderServiceTask),
	MinimumPeriod = "120Seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "120Seconds")
]

namespace Enterprise.Customs.EU.NCTS.ServiceTasks
{
	public class NctsDownloaderServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		public const string Code = "NCD";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string FriendlyName = "NCTS Downloader Task";

		protected override void RunTaskCore(CancellationToken token)
		{
			var downloaderChooser = new NctsDownloadPoller("");
			if (downloaderChooser != null)
			{
				downloaderChooser.PollForDownloadsAndParseResultsAndSaveInterchanges(ServiceLogger, token);
			}
		}

		internal static List<string> NctsCountries()
		{
			var euAndCtCountries = new List<string>();
			euAndCtCountries.AddRange(ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers());
			foreach (var country in Core.Constants.CountryCodes.EuCommonTransitCountries)
			{
				if (!euAndCtCountries.Contains(country))
				{
					euAndCtCountries.Add(country);
				}
			}
			return euAndCtCountries;
		}
	}
}
