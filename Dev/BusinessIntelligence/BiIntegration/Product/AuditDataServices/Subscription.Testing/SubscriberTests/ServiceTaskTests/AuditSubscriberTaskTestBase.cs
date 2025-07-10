using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.ServiceManager.Business;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.AuditDataServices.Subscription.Testing
{
	public abstract class AuditSubscriberTaskTestBase<T> : ServiceTaskTestCase<T> where T : AuditSubscriberTask, new()
	{
		public void TestInitialiseSchedule()
		{
			var testTask = GenerateServiceTask();
			InitialiseTaskSchedule(testTask, out StmServiceTask taskSchedule);
			AssertEquals("ScheduleTask - IsActive", ZBool.True, taskSchedule.SST_Active);
			Assert("ScheduleTask - TaskPeriod", taskSchedule.Recurrence.MinutesRange);
			AssertEquals("ScheduleTask - WeekDaysOnly", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
			AssertEquals("DailyStartTime - Default", ZDateTime.Empty, taskSchedule.Recurrence.CalcDailyStartTimeUtc);
		}

		public virtual void TestIsLoaded()
		{
			var serviceTask = new T();
			var subscribers = new SubscriberLoader().EnumerateSubscribersOfType(serviceTask.AssemblyName, serviceTask.SubscriberNamespace);
			Assert(subscribers.Any(s => typeof(ActualDataChangesAuditSubscriber).IsAssignableFrom(s.GetType())));
		}

		protected virtual string[] GetExpectedHostedServiceRequirements()
		{
			return new[] { "CheckCdcIsEnabled", "IsAuditEnabled", "CheckCdcShouldBeDisabled" };
		}

		public virtual void TestTaskServiceRequirementsAttribute()
		{
			var serviceTaskType = GenerateServiceTask().GetType();
			List<string> methodList = new List<string>();

			do
			{
				var hostedServiceRequirementMethods = serviceTaskType.GetMethods().Where(m => m.GetCustomAttributes(typeof(HostedServiceRequirementAttribute), false).Length > 0).Select(m => m.Name);
				methodList.AddRange(hostedServiceRequirementMethods);
				serviceTaskType = serviceTaskType.BaseType;
			} while (serviceTaskType != typeof(ServiceProviderImpl));

			AssertContainsExactElementsInAnyOrder("Missing HostedServiceRequirement methods", GetExpectedHostedServiceRequirements(), methodList);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();

		public abstract T GenerateServiceTask();

		public abstract string ServiceTaskName();

		protected abstract bool IsClientSpecific { get; }

		public void TestSubscriberServiceTaskClientSpecific()
		{
			var subscriberTask = GenerateServiceTask();

			AssertEquals(IsClientSpecific, subscriberTask.GetType().IsSubclassOf(typeof(ClientSpecificAuditSubscriberTask)));
		}

		public virtual void TestSubscriberServiceTaskAssemblyName()
		{
			var subscriberTask = GenerateServiceTask();
			var expectedAssemblyNames = new[] { SubscriberLoader.ZClientEdiBusinessAssemblyName, SubscriberLoader.ZClientEdiAssemblyName };

			AssertEquals(IsClientSpecific, expectedAssemblyNames.Contains(subscriberTask.AssemblyName));
		}
	}
}
