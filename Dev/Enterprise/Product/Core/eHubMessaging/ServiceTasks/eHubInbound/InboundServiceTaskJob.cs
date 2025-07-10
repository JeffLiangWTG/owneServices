using System;
using System.Diagnostics;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Common.ErrorManagement;
using CargoWise.ComponentModel;
using CargoWise.Data.Utils;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	class InboundServiceTaskJob : ServiceTaskJobWithAdapter
	{
		public InboundServiceTaskJob(IeHubServiceTaskSupport serviceTaskSupport, INotifications notifier, IAdaptorFactory adaptorFactory)
			: base(serviceTaskSupport, notifier, adaptorFactory)
		{
		}

		internal override void ProcessMessagesCore()
		{
			if (!PreProcessingCheckPassed())
			{
				return;
			}

			var adapter = CreateAdapter();
			ExceptionAggregation.Using(adapter, () =>
			{
				bool atLeastOneMessageWasDownloaded;
				var retrieveStopWatch = new Stopwatch(); // do not stick to one Company for too long, cycle to the next.
				retrieveStopWatch.Start();
				do
				{
					ThrowIfCancellationRequested();
					NotifyVerbose(Res.GetString("0A22CF7E-1571-4356-8A53-678D8A0925EE", "Retrieving Messages"));
					adapter.RetrieveMessages();
					atLeastOneMessageWasDownloaded = DownloadMessages(CurrentCompany, adapter);
					if (atLeastOneMessageWasDownloaded)
					{
						OnAtLeastOneMessageProcessed();
					}
					adapter.Inbox.MarkAsRead();
				} while (atLeastOneMessageWasDownloaded && retrieveStopWatch.Elapsed.TotalSeconds < RetrieveForOneCompanyTimeLimitInSeconds);
				retrieveStopWatch.Stop();

				NotifyVerbose(Res.GetString("cb216158-e97d-451e-97ba-24baf2da78f1", "Adapter download completed and read."));
			});
		}

		internal override void ExecuteInternal()
		{
			NotifyVerbose(Res.GetString("9fa4e9f2-ea57-4564-9e45-c011ffea9aeb", "Start executing for each company."));

			var companyLocks = new DisposableList(0);
			ExceptionAggregation.Using(companyLocks, () =>
			{
				foreach (var company in CompaniesThatShouldBeServiced)
				{
					if (TryAcquireCompanyLock(company, out var companyMutex))
					{
						try
						{
							companyLocks.Add(companyMutex);
							CurrentCompany = company;
							using (DisposableEnvironment.ForCompany(company.GC_Code, reportInactive: false))
							{
								NotifyExecutingForCompany();
								ProcessMessages();
							}
						}
						catch (Exception ex) when (HandleCompanyLevelException(ex, ref companyLocks))
						{
						}
					}
				}
			});
		}

		bool TryAcquireCompanyLock(GlbCompany company, out SqlApplicationLock companyMutex)
		{
			if (DbConnection.TryGetLock(MutexPrefix + company.PK, out companyMutex))
			{
				return true;
			}

			NotifyVerbose(string.Format(CultureInfo.CurrentCulture, Res.GetString("2A0E00BA-3BC1-466E-84B0-228C5B0C6562", "Skipping retrieval of messages for company '{0}' as they have just been retrieved in another instance of the service task.", company.GC_Code, company.PK)));
			return false;
		}

		bool DownloadMessages(GlbCompany company, IeHubAdapter adapter)
		{
			var result = false;
			if (adapter.Inbox.Count > 0)
			{
				long totalSizeInBytes = 0;
				foreach (var message in adapter.Inbox)
				{
					var messageStreamLength = message.MessageStream.Length;
					if (messageStreamLength == 0)
					{
						Notifier.Notify(new WarningNotification(Res.GetString("7aecbd34-2ee5-4cfc-a188-34b97ab8c70e", "Retrieved eHub Message with empty message content for tracking ID - {0}, Sender - {1}, Receiver - {2}.", message.TrackingID, message.SenderID, message.RecipientID)));
					}

					totalSizeInBytes += messageStreamLength;
					if (ProcessMessage(company, message))
					{
						result = true;
					}
				}

				var totalSizeInKB = totalSizeInBytes / 1024;
				Notifier.Notify(new InfoNotification(Res.GetString("bb49825b-42ec-49ec-8e7d-f177488f5f81", "Retrieved {0} eHub Messages for company {1}. Total size(KB): {2}.", adapter.Inbox.Count, company.GC_Code, totalSizeInKB)));
			}
			return result;
		}

		internal virtual bool ProcessMessage(GlbCompany company, IeHubMessage message)
		{
			try
			{
				GetMessageHandler(message.SchemaName).SaveMessage(message, company, Notifier);
			}
			catch (ZSaveException ex)
			{
				Notifier.AddWarning(Res.GetString("6e19f87a-ff81-4bee-8e98-333a68bffb11", "{0} {1}", LogMessages.SaveInterchangeFailed, ex.FriendlyMessage));
				throw;
			}
			return true;
		}

		internal virtual IMessageHandler GetMessageHandler(string schemaName)
		{
			return HandlerFactory.GetHandler(schemaName);
		}

		protected sealed override void HandlePasswordPreProcessingCheckFail()
		{
		}

		internal override string MutexPrefix
		{
			get { return "EHI"; }
		}

		internal virtual int InboundProcessingRetryThreshold { get { return 15; } }
		protected internal virtual int RetryWaitingBase { get { return 1000; } }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		protected internal virtual int RetrieveForOneCompanyTimeLimitInSeconds { get { return 60; } }

		protected override bool CompanyShouldBeServiced(GlbCompany company)
		{
			return base.CompanyShouldBeServiced(company) && company.HasActiveBranch;
		}
	}
}
