using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(SchedulerServiceTaskTransactionAdapter))]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "Test Class")]
	public class SchedulerServiceTaskTransactionAdapterTest
	{
		class GetTaskGovernorTest : TestCaseWithFactory
		{
			public void TestSuccessfulGet()
			{
				// Arrange
				var factory = new BusinessObjectFactory();

				var serviceTask = (BusinessObject)factory.New<IServiceTaskSchedule>();
				serviceTask[StmScheduleTaskSchema.S5_IsActive] = true;
				serviceTask[StmScheduleTaskSchema.S5_IsPrivate] = false;
				serviceTask[StmScheduleTaskSchema.S5_ParentTableCode] = StmServiceHostSchema.Constants.Prefix;
				serviceTask[StmScheduleTaskSchema.S5_ScheduleType] = "ZZZ";
				serviceTask[StmScheduleTaskSchema.S5_ScheduleDescription] = "Temp. fake service for TESTING";
				factory.Save();

				IServiceTaskGovernor governor = null;

				// Act
				AssertNoExceptionThrown(() =>
				{
					using var adapter = new SchedulerServiceTaskTransactionAdapter();
					governor = adapter.GetServiceTaskGovernor(serviceTask.PK.ToGuid());
				});

				// Assert
				AssertNotNull(governor);
			}

			public void TestInvalidTask()
			{
				// Act
				using var adapter = new SchedulerServiceTaskTransactionAdapter();
				var governor = adapter.GetServiceTaskGovernor(Guid.Empty);

				// Assert
				AssertNull(governor);
			}

			public void TestGetGovernorForTaskInstance()
			{
				// Arrange
				var taskCode = "XYZ";

				var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				schedule.S5_ScheduleType = taskCode;
				schedule.S5_IsActive = false;

				var taskInstance = new SchedulerServiceTask(schedule);

				// Act
				var governor = taskInstance.AssignedGovernor;

				// Assert
				AssertEquals(schedule.PK, governor.GovernedTask.Pk);
				Assert(!governor.GovernedTask.IsActive);

				// Act
				schedule.S5_IsActive = true;

				// Assert
				Assert(governor.GovernedTask.IsActive);
			}

			public void TestGetCollectionGovernorForAllTasks()
			{
				// Arrange
				var taskSchedules = new ServiceTaskSchedule[5];
				for (int i = 0; i < taskSchedules.Length; i++)
				{
					taskSchedules[i] = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				}
				Factory.Save();

				using var adapter = new SchedulerServiceTaskTransactionAdapter();

				// Act
				var collectionGovernor = adapter.GetCollectionGovernorForAllTasks();

				// Assert
				AssertEquals(5, collectionGovernor.GovernedTasks.Count());
			}

			public void TestCreateTaskCanRunInAnyBranch()
			{
				// Arrange
				var taskCode = "XYZ";
				var taskDescription = "XYZ Description";

				var settings = new HostedServiceAttribute();
				settings.Code = taskCode;
				settings.Description = taskDescription;
				settings.Category = "TST";
				settings.CanRunInAnyBranch = true;

				using var adapter = new SchedulerServiceTaskTransactionAdapter();

				// Act
				var governor = adapter.GetNewServiceTaskGovernor(settings);

				// Assert
				AssertEquals(taskCode, governor.GovernedTask.Code);
				AssertEquals(taskDescription, governor.GovernedTask.Description);
				AssertEquals(ZGuid.Empty, governor.GovernedTask.BranchPk);

				// Act
				adapter.Commit();
				var savedTask = new SchedulerServiceTaskLoader(new Lazy<BusinessObjectFactory>(() => Factory)).Load(taskCode);

				AssertNotNull(savedTask);
				AssertEquals(taskCode, savedTask.Code);
			}

			public void TestCreateTaskNoDefaultBranch()
			{
				// Arrange
				var taskCode = "XYZ";
				var taskDescription = "XYZ Description";

				var settings = new HostedServiceAttribute();
				settings.Code = taskCode;
				settings.Description = taskDescription;
				settings.Category = "TST";
				settings.CanRunInAnyBranch = false;

				using var adapter = new SchedulerServiceTaskTransactionAdapter();

				// Act
				var governor = adapter.GetNewServiceTaskGovernor(settings);

				// Assert
				AssertEquals(taskCode, governor.GovernedTask.Code);
				AssertNotNull(governor.GovernedTask.BranchPk);
				AssertType<Guid>(governor.GovernedTask.BranchPk);
			}

			public void TestGetGovernorByCode()
			{
				// Arrange
				var taskCode = "~T~";
				var serviceTask = (BusinessObject)Factory.New<IServiceTaskSchedule>();
				serviceTask[StmScheduleTaskSchema.S5_IsActive] = true;
				serviceTask[StmScheduleTaskSchema.S5_IsPrivate] = false;
				serviceTask[StmScheduleTaskSchema.S5_ParentTableCode] = StmServiceHostSchema.Constants.Prefix;
				serviceTask[StmScheduleTaskSchema.S5_ScheduleType] = taskCode;
				serviceTask[StmScheduleTaskSchema.S5_ScheduleDescription] = "Temp. fake service for TESTING";
				Factory.Save();

				var taskPk = serviceTask.PK;

				IServiceTaskGovernor governor = null;

				// Act
				AssertNoExceptionThrown(() =>
				{
					using var adapter = new SchedulerServiceTaskTransactionAdapter();
					governor = adapter.GetServiceTaskGovernor(taskCode);
				});

				// Assert
				AssertEquals(taskPk.ToGuid(), governor.GovernedTask.Pk);
			}
		}

		class CommitTest : TestCaseWithFactory
		{
			public void TestSuccessfulCommitNewRecord()
			{
				// Arrange
				var taskCode = "XYZ";
				var taskDescription = "XYZ Description";

				var settings = new HostedServiceAttribute();
				settings.Code = taskCode;
				settings.Description = taskDescription;
				settings.Category = "TST";
				settings.CanRunInAnyBranch = true;

				using var adapter = new SchedulerServiceTaskTransactionAdapter();

				_ = adapter.GetNewServiceTaskGovernor(settings);

				// Act
				adapter.Commit();

				var committedTask = new SchedulerServiceTaskLoader(new Lazy<BusinessObjectFactory>(() => Factory)).Load(taskCode);

				// Assert
				AssertNotNull(committedTask);
				AssertEquals(taskCode, committedTask.Code);
			}

			public void TestSuccessfulCommitExistingRecord()
			{
				// Arrange
				var taskCode = "XYZ";

				var schedule = Factory.New<ServiceTaskSchedule>();
				schedule.S5_ScheduleType = taskCode;
				var settings = new HostedServiceAttribute();
				settings.IsMandatory = false;
				schedule.SetStaticServiceAttributesDebugOnly(settings);
				schedule.S5_IsActive = false;
				schedule.Factory.Save();

				using var adapter = new SchedulerServiceTaskTransactionAdapter();
				var governor = adapter.GetServiceTaskGovernor(schedule.PK.ToGuid());

				governor.SetActive(true);

				// Act
				adapter.Commit();

				var committedTask = new SchedulerServiceTaskLoader(new Lazy<BusinessObjectFactory>(() => Factory)).Load(taskCode);

				// Assert
				Assert(committedTask.IsActive);
			}
		}
	}
}
