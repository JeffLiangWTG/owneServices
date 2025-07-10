using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.SWT.ServiceTask;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.SWT.ServiceTasks.Testing
{
	[TestedType(typeof(SWTNotYetArrivedRepServiceTask))]
	class SWTNotYetArrivedRepServiceTaskTest : ServiceTaskTestCase<SWTNotYetArrivedRepServiceTask>
	{
		public void TestRunTask()
		{
			InitialiseAndRunTaskSchedule(ServiceTask);
			AssertEquals(true, ServiceTask.ServiceLogger.ToString().Contains("Please set up the Registry items required for the 'Not Yet Arrived' Report process in Admin->System->Registry->SWT Client Extensions->Documents->Not yet Arrived Report: Consignor Organisations"));
			((TestServiceLogger)ServiceTask.ServiceLogger).ClearLog();
			SetupRegistryItems();
			ServiceTask.RunTask();
			AssertEquals(true, ServiceTask.ServiceLogger.ToString().Contains("'Not Yet Arrived' report started"));
			AssertEquals(true, ServiceTask.ServiceLogger.ToString().Contains("'Not Yet Arrived' report finished"));
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("1MINUTES", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		void SetupRegistryItems()
		{
			var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));
			Guid[] orgsPKs = new Guid[] { consignor.PK.ToGuid() };
			SWTDataRegistry.Instance.ConsignorsList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, orgsPKs);
		}

		SWTNotYetArrivedRepServiceTask ServiceTask
		{
			get
			{
				return serviceTask ?? (serviceTask = new SWTNotYetArrivedRepServiceTask());
			}
		}

		SWTNotYetArrivedRepServiceTask serviceTask;
	}
}
