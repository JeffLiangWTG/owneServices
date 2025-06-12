using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading;
using eServices.eHubDataModel.eHubTransactions;

namespace CargoWise.eHub.Gateway.ITCustoms
{
	public class JobStatusManager : IJobStatusManager
	{
		public static Func<eHubTransactionsContext> DbContext = () => new eHubTransactionsContext();
		public const int MAX_RETRY_COUNT = 10;
		public const int RETRY_INTERVAL_MILLISECONDS = 500;
		private static readonly char[] ValidFileTypes = { 'U', 'X', 'Q', 'L' };
		private const int ExpectedSuffixLength = 3;

		public void TriggerJobStatus(string clientSystemID, Files files)
		{
			TriggerJobStatus(clientSystemID, files.File);
		}

		private void TriggerJobStatus(string clientSystemID, File[] files, int retryCount = 0)
		{
			if (files.Length > 0)
			{
				if (retryCount++ < MAX_RETRY_COUNT)
				{
					var unfinishedUpdatingFile = new List<File>();
					if (retryCount > 1)
					{
						Thread.Sleep(RETRY_INTERVAL_MILLISECONDS);
					}
					foreach (var file in files)
					{
						if (string.IsNullOrWhiteSpace(file.Name))
						{
							continue;
						}

						var dotIndex = file.Name.IndexOf('.');
						if (dotIndex == -1 || dotIndex == 0 || dotIndex >= file.Name.Length - ExpectedSuffixLength)
						{
							continue;
						}

						if (file.Name[dotIndex + 1] != 'R')
						{
							continue;
						}

						var prefix = file.Name.Substring(0, dotIndex + 1);
						var suffix = file.Name.Substring(dotIndex + 2);

						using (var context = DbContext())
						{
							var jobStatuses = context.eHubITCustomsJobStatuses
								.Where(x => x.eHubClientSystem.EH_ID == clientSystemID)
								.Where(x => x.IT_FileName.StartsWith(prefix))
								.Where(x => x.IT_FileName.EndsWith(suffix))
								.ToList();

							jobStatuses = jobStatuses
								.Where(x => ValidFileTypes.Contains(x.IT_FileName[dotIndex + 1]))
								.ToList();

							TriggerPollingStartUTC(jobStatuses);

							try
							{
								context.SaveChanges();
							}
							catch (DbUpdateConcurrencyException)
							{
								unfinishedUpdatingFile.Add(file);
							}
						}
					}
					TriggerJobStatus(clientSystemID, unfinishedUpdatingFile.ToArray(), retryCount);
				}
				else
				{
					throw new ApplicationException(string.Format("Unable to trigger the retrieval of file responses. Failed filename(s): {0}.", string.Join(", ", files.Select(x => x.Name))));
				}
			}
		}

		internal virtual void TriggerPollingStartUTC(List<eHubITCustomsJobStatus> jobStatuses)
		{
			jobStatuses.ForEach(x =>
			{
				var fileType = x.IT_FileName[x.IT_FileName.IndexOf('.') + 1];

				switch (fileType)
				{
	
					case 'Q':
					case 'L':
						x.IT_LastStatus = "NR ";
						break;
					case 'U':
					case 'X':
						x.IT_LastStatus = null;
						break;
				}

				x.IT_PollingStartUTC = DateTime.UtcNow;
			}); ;
		}
	}
}
