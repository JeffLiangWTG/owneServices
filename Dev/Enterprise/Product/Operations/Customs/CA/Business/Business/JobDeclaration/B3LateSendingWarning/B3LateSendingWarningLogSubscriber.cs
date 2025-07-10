using System;
using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	[Serializable]
	public class B3LateSendingWarningLogSubscriber : LogSubscriber
	{
		#region Implementation

		public override string[] EventTypes
		{
			get { return new string[] { Events.CanadianCADLateSendingWarningCode }; }
		}

		public override string Name
		{
			get { return "B3LogSubscriber"; }
		}

		public override string FriendlyName
		{
			get { return "Canadian CAD Late Sending Warning"; }
		}

		public override string[] TableNames
		{
			get { return new string[] { JobDeclarationSchema.Constants.TableName }; }
		}

#if DEBUG
		public override bool EnableFactorySaveAlerterInTesting => true;
#endif

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			foreach (IQueuedLog queuedLog in queuedLogs)
			{
				var stmALogQuery = new ZQuery(StmALogSchema.SL_Parent, queuedLog.SJ_ParentID);
				stmALogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, queuedLog.SJ_SE_NKEvent);
				stmALogQuery.AddToFilter(StmALogSchema.PK, queuedLog.SJ_ALogReference);
				stmALogQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Equal, B3WarningComment);
				var stmALog = queuedLog.Factory.LoadTop1<StmALog>(stmALogQuery);
				if (stmALog != null && !stmALog.SL_IsCancelled)
				{
					var log = string.Format(CultureInfo.InvariantCulture, "CAD Late Sending Warning, scheduled by {1} at {0} UTC", queuedLog.SJ_EventTimeUtc.ToString("dd-MMM-yyyy HH:mm"), queuedLog.SJ_GS_NKUser);
					DefaultLogger.Log(LogType.Information, log);
					switch (queuedLog.SJ_ParentTableCode)
					{
						case JobDeclarationSchema.Constants.Prefix:
							ProcessJobDeclaration(stmALog, queuedLog.Factory.Load<JobDeclaration>(queuedLog.SJ_ParentID));
							break;
					}
				}
			}
		}

		void ProcessJobDeclaration(StmALog stmALog, JobDeclaration declaration)
		{
			try
			{
				if (declaration != null)
				{
					var entryHead = declaration.B3EntryHeader;
					if (entryHead == null || !entryHead.IsClearedB3CorCAD)
					{
						if (declaration.Branch != null)
						{
							using (DisposableEnvironment.ForBranch(declaration.Branch.PK.ToGuid()))
							{
								SendB3LateSendingWarning(declaration);
							}
						}
						else
						{
							SendB3LateSendingWarning(declaration);
						}
					}
				}
			}
			finally
			{
				stmALog.Cancel();
			}
		}

		internal const string B3WarningComment = "B3Warning";

		void SendB3LateSendingWarning(JobDeclaration declaration)
		{
			new B3LateSendingWarningProcessor(declaration).Process();
		}

		#endregion
	}
}
