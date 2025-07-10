using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.BatchProcessor.Testing
{
	public class LogBatchListenerTestClass : LogBatchListener
	{
		public override string BusinessObjectTableName
		{
			get { return OrgHeaderSchema.Constants.TableName; }
		}

		public override Type BusinessObjectType
		{
			get { return typeof(OrgHeader); }
		}

		public override string HumanReadableName
		{
			get { return "LogBatchListenerTestClass"; }
		}

		public bool ProcessWasCalled;
		protected override void Process(BusinessObject matchingBusinessObjects, StmALog log, INotifications notifications)
		{
			ProcessWasCalled = true;
		}

		public new void SendEmailToNotificationGroup(NotificationBuffer notification, ZGuid notificationGroupPK, string subject)
		{
			base.SendEmailToNotificationGroup(notification, notificationGroupPK, subject);
		}

		public new BusinessObjectFactory Factory
		{
			get { return base.Factory; }
			set { base.Factory = value; }
		}
	}
}
