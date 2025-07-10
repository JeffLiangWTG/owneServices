using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.ServiceTasks
{
	public class NctsDownloadPoller : NctsChooser<INctsResponseDownloader>
	{
		public NctsDownloadPoller(ZString nctsDomainCountryCode)
			: base("EU.NCTS.ServiceTasks.INctsResponseDownloader", nctsDomainCountryCode)
		{
		}

		#region INctsResponseDownloader Members

		public void PollForDownloadsAndParseResultsAndSaveInterchanges(ILogger serviceLogger, CancellationToken token)
		{
			foreach (var nctsChooser in DictionaryOfAvailableChoosers)
			{
				// TODO work out what we do if there is an exception processing a country - move to next country, re-process?
				// TODO spread processing per country if many countries? 

				token.ThrowIfCancellationRequested();
				var factory = new BusinessObjectFactory();
				var countryCode = nctsChooser.Key == Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes ? Core.Constants.CountryCodes.UnitedKingdom : nctsChooser.Key;
				var branchesInCountry = new GlbBranch.Loader(factory).LoadAllBranchesInThisCountryActiveOnly(countryCode);

				foreach (var branch in branchesInCountry)
				{
					using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
					{
						// Download and process NCTS responses
						token.ThrowIfCancellationRequested();
						var interchangeDownloader = nctsChooser.Value;
						if (interchangeDownloader != null)
						{
							interchangeDownloader.ExecuteDownload(serviceLogger, branch, token);
						}
					}
				}
			}
		}

		#endregion
	}
}
