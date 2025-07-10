using System;
using System.Diagnostics;
using CargoWise.Data;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Core.Environment.Semaphores.Testing
{
	sealed class EnterpriseHeartbeatInfoFactoryTest : TestCase
	{
		public void TestEnterpriseHeartbeatInfoFactory()
		{
			EnterpriseHeartbeatInfoFactory enterpriseHeartbeatInfoFactory = new EnterpriseHeartbeatInfoFactory();
			IHeartbeatInfoFactory factory = enterpriseHeartbeatInfoFactory;
			IHeartbeatInfo info = factory.New();
			AssertEquals("HostName", System.Environment.MachineName, info.HostName);
			AssertEquals("UserPk", EnvProxy.Instance.CurrentUser.PK, info.UserPk);
			AssertEquals("ProcessId", Process.GetCurrentProcess().Id, info.ProcessId);
			AssertEquals("ProcessId = SQL Host Process ID", GetMainConnectionHostProcessId(), info.ProcessId);
			AssertEquals("HeartbeatType", HeartbeatTypes.Enterprise, info.HeartbeatType);
			AssertNull("ClientIdentifier", info.ClientIdentifier);

			enterpriseHeartbeatInfoFactory.remoteClientName = "REMOTE";
			info = factory.New();
			AssertEquals("HostName", System.Environment.MachineName + "/REMOTE", info.HostName);
		}

		public void TestHostNameForWinzor()
		{
			using(Globals.SetClientIdentifierForTest("PC/GUID"))
			{
				EnterpriseHeartbeatInfoFactory enterpriseHeartbeatInfoFactory = new EnterpriseHeartbeatInfoFactory();
				IHeartbeatInfoFactory factory = enterpriseHeartbeatInfoFactory;
				IHeartbeatInfo info = factory.New();
				AssertEquals("HostName", $"{System.Environment.MachineName}/PC", info.HostName);
				AssertEquals("UserPk", EnvProxy.Instance.CurrentUser.PK, info.UserPk);
				AssertEquals("ProcessId", Process.GetCurrentProcess().Id, info.ProcessId);
				AssertEquals("ProcessId = SQL Host Process ID", GetMainConnectionHostProcessId(), info.ProcessId);
				AssertEquals("HeartbeatType", HeartbeatTypes.Enterprise, info.HeartbeatType);
				AssertEquals("ClientIdentifier", Globals.ClientIdentifier, info.ClientIdentifier);
			}
		}

		int GetMainConnectionHostProcessId()
		{
			string sqlText = "SELECT HOST_ID();";
			DbCommand cmd = Db.Connection.Command(sqlText); // SELECT HOST_ID cannot be loaded with business objects
			int result = Convert.ToInt32(cmd.ExecuteScalar());
			return result;
		}
	}
}
