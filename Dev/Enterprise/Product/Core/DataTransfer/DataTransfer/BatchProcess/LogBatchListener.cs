using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DataTransfer.BatchProcessor
{
	public interface ILogBatchListener : ILogBatchListenerProxy
	{
		Type BusinessObjectType { get; }
		string BusinessObjectTableName { get; }
		string HumanReadableName { get; }
		void Match(StmALog log, INotifications activityLog);
	}

	public abstract class LogBatchListener : ILogBatchListener
	{
		public abstract Type BusinessObjectType { get; }
		public abstract string BusinessObjectTableName { get; }
		public abstract string HumanReadableName { get; }

		public void Match(StmALog log, NotificationBuffer notify)
		{
			Factory = log.Factory;
			((ILogBatchListener)this).Match(log, notify);
			Factory = null;
		}

		#region Implementation

		void ILogBatchListener.Match(StmALog logRecord, INotifications notifications)
		{
			ArrayList matchingBusinessObjects = new ArrayList();
			var log = logRecord;

			if (MatchTableName(log) && AdditionalMatching(log))
			{
				BusinessObject found = LoadBusinessObject(log);
				if (found != null)
				{
					notifications.Notify(new BatchNotification(Res.GetString("a3147901-cb14-4d14-a2bf-923c08b3b667", "{0}: Found Event {1} for {2} posted at {3}", HumanReadableName, log.SL_SE_NKEvent, log.SL_Table, log.SL_PostedTimeUtc.ToISO8601String())));
					Process(found, log, notifications);
				}
				else
				{
					notifications.Notify(new BatchNotification(Res.GetString("e74b0db6-347b-4307-9586-db14513717f6", "{0} is missing record with PK '{1}'", log.SL_Table, log.SL_Parent.ToString())));
				}
			}
		}

		protected virtual bool AdditionalMatching(StmALog log)
		{
			return true;
		}

		protected virtual bool MatchTableName(StmALog log)
		{
			return log.SL_Table == BusinessObjectTableName;
		}

		protected virtual BusinessObject LoadBusinessObject(StmALog log)
		{
			BusinessObject result = Factory.Load(BusinessObjectType, log.SL_Parent);
			if (result != null && result.GetType().IsSubclassOf(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>()))
			{
				if (GlbBranch.CurrentBranch.Country.Code != ((ICustomsJobInfo)result).Branch.Country.Code)
				{
					result = null;
				}
			}
			return result;
		}

		protected virtual void SendEmailToNotificationGroup(NotificationBuffer notification, ZGuid notificationGroupPK, string subject)
		{
			GlbGroup notificationGroup = (GlbGroup)Factory.Load(typeof(GlbGroup), notificationGroupPK);
			if (notificationGroup != null)
			{
				notification.SendEmail(subject, notificationGroup.GG_Code);
			}
		}

		protected abstract void Process(BusinessObject matchingBusinessObject, StmALog log, INotifications notifications);

		protected BusinessObjectFactory Factory;

		#endregion
	}
}
