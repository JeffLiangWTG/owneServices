using System;
using System.Linq;
using System.Net.NetworkInformation;
using Enterprise.Client.EDI.ServiceTasks.EventLogs;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ServiceTask.EventLogs.Testing
{
	class RegistryMachinesHandlerTest : TransactionedTestCase
	{
		public void TestValidRegistryList()
		{
			EDIDataRegistry.Instance.MachineHostNameList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Format("{0},{1}", System.Environment.MachineName, "InvalidName"));
			var instance = RegistryMachinesHandler.GetLatestAccessibleHostNameList();
			Assert("Not empty Registry list", instance.Any());
			foreach (var hostname in instance)
			{
				AssertNotEquals("Not empty element inside machine Registry list.", string.Empty, hostname);
				PingReply reply;
				Ping pinger = new Ping();
				AssertNoExceptionThrown("Should correct hostname" + hostname, () =>
				{
					reply = pinger.Send(hostname);
					Assert("The hostname " + hostname + " should be accessible.", reply.Status == IPStatus.Success);
				});
			}
		}

		public void TestEmptyRegistry()
		{
			EDIDataRegistry.Instance.MachineHostNameList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			var instance = RegistryMachinesHandler.GetLatestAccessibleHostNameList();
			AssertEquals(0, instance.Count());
		}

		public void TestValidGetListEventLogCache()
		{
			EDIDataRegistry.Instance.EventLogHighWaterMarkList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "MyMachine+123412,AnotherMachine+123");
			var list = RegistryMachinesHandler.GetLatestListEventLogCache();
			for (int i = 0; i < list.Count(); i++)
			{
				Assert("Expect able to load Cache if stored", !string.IsNullOrEmpty(list.ElementAt(i).Machine) && !string.IsNullOrEmpty(list.ElementAt(i).EventRecordID));
			}
		}
	}
}
