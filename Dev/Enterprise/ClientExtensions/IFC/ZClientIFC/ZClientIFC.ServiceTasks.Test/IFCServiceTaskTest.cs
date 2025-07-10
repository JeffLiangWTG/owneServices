using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.IO;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.IFC.ConsolExport.ServiceTasks.Testing
{
	[TestedType(typeof(IFCServiceTask))]
	class IFCServiceTaskTest : ServiceTaskTestCase<IFCServiceTask>
	{
		public void TestEnvironmentSpecified()
		{
			RunTaskSchedule(ServiceTask);
			Assert(ServiceTask.Buffer.AsString.Contains("Export directory does not exist or invalid"));
			ServiceTask.Buffer.Clear();
			IFCDataRegistry.Instance.FSCExportDirectory = Env.TempPath;
			RunTaskSchedule(ServiceTask);
			Assert(!ServiceTask.Buffer.AsString.Contains("Export directory does not exist or invalid"));
		}

		public void TestRunTask()
		{
			IFCDataRegistry.Instance.FSCExportDirectory = Env.TempPath;
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingConsol consol = shipment.Consols.AddNew();
			Factory.Save();
			shipment.JS_HouseBill = "testHousbill";
			consol.JK_MasterBillNum = "Masterbill";
			Factory.Save();
			DocumentFactory docFactory = new DocumentFactoryProvider().GetFactory(Factory);
			StorageMain storageMain = docFactory.New<StorageMain>();
			storageMain.SM_ParentFK = shipment.PK;
			Factory.Save();
			try
			{
				RunTaskSchedule(ServiceTask);
				AssertEquals("File created", 1, Directory.GetFiles(Env.TempPath).Length);
			}
			finally
			{
				TempDirectory.DeleteDirectory(Env.TempPath);
			}
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("1minute", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
		IFCServiceTask ServiceTask;
		protected override void SetUpCore()
		{
			base.SetUpCore();
			ServiceTask = new IFCServiceTask();
			InitialiseTaskSchedule(ServiceTask);
		}
	}
}
