using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	class WorkflowTriggerUserContextProvider : IWorkflowTriggerUserContextProvider
	{
		IDisposable IWorkflowTriggerUserContextProvider.SetTemporaryUserContext(IBaseTrigger trigger, BusinessObject job)
		{
			var notifications = new Notifications();
			return WorkflowUserContextSwitchCreator.GetTemporaryUserContext(trigger, job, new CurrentUserWorkflowTriggerSource(), notifications).Set(notifications);
		}

		ZString IWorkflowTriggerUserContextProvider.GetBranch(IBaseTrigger trigger, BusinessObject job, IWorkflowTriggerSource triggerSource)
		{
			return WorkflowUserContextSwitchCreator.GetTemporaryUserContext(trigger, job, triggerSource, new Notifications()).BranchCode;
		}

		sealed class CurrentUserWorkflowTriggerSource : IWorkflowTriggerSource
		{
			public ZGuid ParentID { get; set; }
			public ZDateTime EventTime { get; set; }
			public ZDateTimeOffset EventTimeOffset { get; set; }
			public ZDateTime PostedTimeUtc { get; set; }
			public ZString FriendlyTableName { get; set; }
			public ZString Reference { get; set; }
			public ZString SourceType { get; set; }
			public ZString DepartmentCode { get; set; }
			public ZString BranchCode { get; set; }
			public ZString CompanyCode { get; set; }
			public ZBool IsEstimate { get; set; }
			public ZGuid Identifier { get; set; }
			public IPropagationSettings PropagationSettings => new DefaultPropagationSettings();
			public ZString StaffCode => GlbStaff.CurrentUser.GS_Code;
			public ZString UserCode => StaffCode;
			public ZString Source => FriendlyTableName;
			ZDateTime IWorkflowTriggerSource.EventTimeUtc => EventTimeOffset.ToUtcDateTime();
			ZBool IWorkflowTriggerSource.IsCancelled => false;
		}

		class Notifications : INotifications
		{
			public void Add(INotification notification)
			{
			}
		}
	}
}
