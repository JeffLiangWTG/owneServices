using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data.Testing;
using Enterprise.Registry.Business.eServices;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.UniversalDataBuss.ServiceTasks.Testing
{
	[TestedType(typeof(UMIServiceTaskWorker))]
	class UMIServiceTaskWorkerTest : ServiceTaskTestCase<UMIServiceTaskWorker>
	{
		public void TestWorkerDoesNotRunWhenGrengineDisallowed()
		{
			eAdaptorRegistry.Instance.AllowParallelUMI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var logger = new TestServiceLogger();
			new UMIServiceTaskWorker() { ServiceLogger = logger }.RunTask();

			AssertMultilineASCIIEquals("", "", logger.ToString());
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("1minute", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}

	[UseSnapshotProtection]
	class UMIServiceTaskWorkerTest_Snapshot : TestCase
	{
		public void TestDroppingConnectionsIsOk()
		{
				eAdaptorRegistry.Instance.AllowParallelUMI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var logger = new TestServiceLogger();
				new UMIServiceTaskWorker() { ServiceLogger = logger }.RunTask();

				AssertMultilineASCIIEquals("", "", logger.ToString());
		}
	}
}
