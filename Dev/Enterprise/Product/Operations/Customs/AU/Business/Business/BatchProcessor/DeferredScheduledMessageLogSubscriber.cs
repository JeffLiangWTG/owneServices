using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// This class will be called when a DSM event becomes due, and so deferred messages are to be generated and sent
	/// </summary>
	[Serializable]
	public class DeferredScheduledMessageLogSubscriber : LogSubscriber
	{
		public override bool IsRequired
		{
			get { return true; }
		}

		readonly NotificationBuffer notifications = new NotificationBuffer();

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			foreach (var queuedLog in queuedLogs)
			{
				var stmALogQuery = new ZQuery(StmALogSchema.SL_Parent, queuedLog.SJ_ParentID);
				stmALogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, queuedLog.SJ_SE_NKEvent);
				stmALogQuery.AddToFilter(StmALogSchema.PK, queuedLog.SJ_ALogReference);
				var stmALog = queuedLog.Factory.LoadTop1<StmALog>(stmALogQuery);
				if (stmALog != null && !stmALog.SL_IsCancelled)
				{
					notifications.Clear();
					DefaultLogger.Log(Integration.LogType.Information, string.Format("DeferredScheduledMessage, scheduled by {2} at {0} UTC, type {1}", queuedLog.SJ_EventTimeUtc.ToString("dd-MMM-yyyy HH:mm"), queuedLog.SJ_Reference, queuedLog.SJ_GS_NKUser));
					switch (queuedLog.SJ_ParentTableCode)
					{
						case CusMAWBSchema.Constants.Prefix:
							ProcessCusMAWB(queuedLog.Factory.Load<CusMAWB>(queuedLog.SJ_ParentID), queuedLog.SJ_Reference, queuedLog.SJ_GS_NKUser);
							break;
						case CusOutturnHeaderSchema.Constants.Prefix:
							ProcessCusOutturnHeader(queuedLog.Factory.Load<CusOutturnHeader>(queuedLog.SJ_ParentID), queuedLog.SJ_Reference, queuedLog.SJ_GS_NKUser);
							break;
						case CusSCAOceanBillSchema.Constants.Prefix:
							ProcessCusSCAOceanBill(queuedLog.Factory.Load<CusSCAOceanBill>(queuedLog.SJ_ParentID), queuedLog.SJ_Reference, queuedLog.SJ_GS_NKUser);
							break;
						case CusReconDeclarationSchema.Constants.Prefix:
							ProcessConsolidatedDeclaration(queuedLog.Factory.Load<ConsolidatedDeclaration>(queuedLog.SJ_ParentID), queuedLog.SJ_Reference.Trim(), stmALog.User);
							break;
						case JobDeclarationSchema.Constants.Prefix:
							ProcessNormalDeclaration(queuedLog.Factory.Load<JobDeclaration>(queuedLog.SJ_ParentID), queuedLog.SJ_Reference.Trim(), stmALog.User);
							break;
					}
				}
			}
		}

		#region CusMAWB
		void ProcessCusMAWB(CusMAWB cusMAWB, string messageType, string userNK)
		{
			if (cusMAWB != null)
			{
				if (messageType == CusMAWBBase.AirCargoReportLogReference)
				{
					SendAIRCRMessagesForCusMAWB(cusMAWB, userNK);
				}
				else if (messageType == CusMAWBBase.AirCargoOutturnLogReference)
				{
					SendAIROUTMessagesForCusMAWB(cusMAWB, userNK);
				}
			}
		}

		void SendAIRCRMessagesForCusMAWB(CusMAWB cusMAWB, string userNK)
		{
			try
			{
				using (var processorJob = new AirCargoProcessorJob(cusMAWB, true))
				{
					new CargoMessagingTriggerActionProcessor(Env.Registry.RawRegistry.AirCargoSendErrorsToGroup).Process(processorJob, notifications, userNK, DefaultLogger);
				}
			}
			finally
			{
				cusMAWB.CancelDeferredScheduledMessageLogs(CusMAWBBase.AirCargoReportLogReference);
			}
		}

		void SendAIROUTMessagesForCusMAWB(CusMAWB cusMAWB, string userNK)
		{
			try
			{
				using (var processorJob = new AirCargoOutturnProcessorJob(cusMAWB, true))
				{
					new CargoMessagingTriggerActionProcessor(Env.Registry.RawRegistry.AirCargoSendErrorsToGroup).Process(processorJob, notifications, userNK, DefaultLogger);
				}
			}
			finally
			{
				cusMAWB.CancelDeferredScheduledMessageLogs(CusMAWBBase.AirCargoOutturnLogReference);
			}
		}

		#endregion

		#region CusOutturnHeader
		void ProcessCusOutturnHeader(CusOutturnHeader outturnHeader, string messageType, string userNK)
		{
			if (outturnHeader != null && messageType == "SEAOUT")
			{
				SendSEAOUTMessagesForCusOutturnHeader(outturnHeader, userNK);
			}
		}

		void SendSEAOUTMessagesForCusOutturnHeader(CusOutturnHeader outturnHeader, string userNK)
		{
			try
			{
				using (var processorJob = new SeaCargoOutturnHeaderProcessorJob(outturnHeader))
				{
					new CargoMessagingTriggerActionProcessor(Env.Registry.RawRegistry.SeaCargoSendErrorsToGroup).Process(processorJob, notifications, userNK, DefaultLogger);
				}
			}
			finally
			{
				outturnHeader.CancelDeferredScheduledMessageLogs();
			}
		}
		#endregion

		#region CusSCAOCeanBill

		void ProcessCusSCAOceanBill(CusSCAOceanBill oceanBill, string messageType, string userNK)
		{
			if (oceanBill != null && messageType == "SEACR")
			{
				SendSEACRMessagesForOceanBill(oceanBill, userNK);
			}
		}

		void SendSEACRMessagesForOceanBill(CusSCAOceanBill oceanBill, string userNK)
		{
			try
			{
				using (var processorJob = new SeaCargoProcessorJob(oceanBill, true))
				{
					new CargoMessagingTriggerActionProcessor(Env.Registry.RawRegistry.SeaCargoSendErrorsToGroup).Process(processorJob, notifications, userNK, DefaultLogger);
				}
			}
			finally
			{
				oceanBill.CancelDeferredScheduledMessageLogs();
			}
		}

		#endregion

		#region JobDeclaration

		void ProcessNormalDeclaration(JobDeclaration jobDeclaration, ZString messageType, IGlbStaff user)
		{
			if (jobDeclaration != null)
			{
				try
				{
					ProcessJobDeclaration(jobDeclaration, messageType, user);
				}
				finally
				{
					jobDeclaration.CancelQueuedMessageLogs();
				}
			}
		}

		void ProcessConsolidatedDeclaration(ConsolidatedDeclaration consolidatedDeclaration, ZString messageType, IGlbStaff user)
		{
			if (consolidatedDeclaration != null)
			{
				try
				{
					var aggregateDeclaration = (JobDeclaration)consolidatedDeclaration.BuildAggregateJobDeclaration();
					ProcessJobDeclaration(aggregateDeclaration, messageType, user);

					consolidatedDeclaration.DeriveConsolidatedStatusAndImportAggregateDeclaration(aggregateDeclaration, true);
				}
				finally
				{
					consolidatedDeclaration.CancelQueuedMessageLogs();
				}
			}
		}

		void ProcessJobDeclaration(JobDeclaration jobDeclaration, ZString messageType, IGlbStaff user)
		{
			if (Enum.TryParse(messageType, ignoreCase: true, out CMRMessageTypes cmrMessageType))
			{
				var processor = new IMDMessagingTriggerActionProcessor(jobDeclaration, cmrMessageType);
				processor.Process(notifications, DefaultLogger, user);
			}
			else
			{
				DefaultLogger.Log(Integration.LogType.Error, $"{messageType} is not a recognised IMD Message Type.");
			}
		}

		#endregion

		#region Implementation

		public override string Name
		{
			get { return "DSMLogSubscriber"; }
		}

		public override string FriendlyName
		{
			get { return "DSM Log Subscriber"; }
		}

		public override string[] TableNames
		{
			get { return new string[] { CusMAWBSchema.Constants.TableName, CusOutturnHeaderSchema.Constants.TableName, CusSCAOceanBillSchema.Constants.TableName, JobDeclarationSchema.Constants.TableName, CusReconDeclarationSchema.Constants.TableName }; }
		}

		public override string[] EventTypes
		{
			get { return new string[] { Events.DeferredScheduledMessageCode }; }
		}

		#endregion

	}
}
