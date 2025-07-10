using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Integration.ServiceTasks.CW.Test
{
	/// <summary>
	/// This will allow you to create fake service tasks and to set their values for TESTING.
	/// User the regular ServiceManagerQuerierHelper to query your service, as normal, 
	/// but first use this class to create a fake service.
	/// </summary>
	public class ServiceManagerQuerierHelperForTesting : IDisposable
	{
		readonly BusinessObjectFactory factory;
		readonly BusinessObject parent;
		readonly BusinessObject serviceTask;
		ZGuid parentIdStored;

		/// <summary>
		/// Will create a new fake test with a status of Active
		/// </summary>
		/// <param name="codeOfFakeServiceTaskToCreate"></param>
		public ServiceManagerQuerierHelperForTesting(string codeOfFakeServiceTaskToCreate)
		{
			parentIdStored = ZGuid.Empty;

			factory = new BusinessObjectFactory();

			parent = factory.New<StmServiceHost>();
			parent[StmServiceHostSchema.SH_HostName] = Guid.NewGuid().ToString();

			serviceTask = (BusinessObject)factory.New<IServiceTaskSchedule>();
			serviceTask[StmScheduleTaskSchema.S5_IsActive] = true;
			serviceTask[StmScheduleTaskSchema.S5_IsPrivate] = false;
			serviceTask[StmScheduleTaskSchema.S5_ParentTableCode] = StmServiceHostSchema.Constants.Prefix;
			serviceTask[StmScheduleTaskSchema.S5_ScheduleType] = codeOfFakeServiceTaskToCreate;
			serviceTask[StmScheduleTaskSchema.S5_ScheduleDescription] = "Temp. fake service for TESTING";
			factory.Save();
		}

		public void ActivateServiceForTesting()
		{
			serviceTask[StmScheduleTaskSchema.S5_IsActive] = true;
			factory.Save();
		}

		public void DeactivateServiceForTesting()
		{
			serviceTask[StmScheduleTaskSchema.S5_IsActive] = false;
			factory.Save();
		}

		public void SetNextRunTimeForTesting(ZDateTime nextRunTime)
		{
			serviceTask[StmScheduleTaskSchema.S5_NextScheduledPrintRunTimeUtc] = nextRunTime;
			factory.Save();
		}

		public void SetScheduleStateForTesting(ZString scheduleState)
		{
			serviceTask[StmScheduleTaskSchema.S5_ScheduleState] = Encoding.ASCII.GetBytes(scheduleState);
			factory.Save();
		}

		public void SetBranchPKForTesting(ZGuid branchPK)
		{
			serviceTask[StmScheduleTaskSchema.S5_GB] = branchPK;
			factory.Save();
		}

		public void SetParentAsNullToOrphanTaskScheduleForTesting()
		{
			parentIdStored = (ZGuid)serviceTask[StmScheduleTaskSchema.S5_ParentID];
			serviceTask[StmScheduleTaskSchema.S5_ParentID] = ZGuid.NewZGuid();
			factory.Save();
		}

		public void SetParentAsValidAgainAfterOrphaningTaskForTesting()
		{
			serviceTask[StmScheduleTaskSchema.S5_ParentID] = parentIdStored;
			factory.Save();
		}

		public void Dispose()
		{
			serviceTask.Delete();
			parent.Delete();
			factory.Save();
		}
	}
}
