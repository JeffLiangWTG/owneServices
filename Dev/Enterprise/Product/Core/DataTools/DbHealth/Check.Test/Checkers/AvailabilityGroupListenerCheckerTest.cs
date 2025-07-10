using System;
using System.Collections.Generic;
using CargoWise.BrandManager;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DbHealth.Check
{
	[TestedType(typeof(AvailabilityGroupListenerChecker))]
	sealed class AvailabilityGroupListenerCheckerTest : CheckerTestCaseBase
	{
		public void TestCheckConnectionIsUsingTheAGListener()
		{
			IChecker listenerChecker = new AvailabilityGroupListenerChecker();
			AssertEquals("description", "Check database connections are using the AlwaysOn Listener", listenerChecker.Description);

			listenerChecker.Check(Db.Connection, null, new TestServiceLogger());
		}

		public void TestCheckWarningNotGeneratedForMultipleIPs()
		{
			var warningList = new DbHealthWarningList();

			var ag = new AvailabilityGroupInfo("GrpName", Guid.NewGuid(), Guid.NewGuid(), new List<string> { "10.11.12.13" });
			ag.ListenerIPs.Add("10.11.22.15");
			ag.ListenerIPs.Add("10.33.33.19");

			AvailabilityGroupListenerChecker listenerChecker = new AvailabilityGroupListenerChecker();
			listenerChecker.CheckAgAndConnectionIP(ag, "10.11.12.13", warningList);
			AssertEquals("Number of generated warnings", 0, warningList.Count);

			listenerChecker.CheckAgAndConnectionIP(ag, "10.11.22.15", warningList);
			AssertEquals("Number of generated warnings", 0, warningList.Count);

			listenerChecker.CheckAgAndConnectionIP(ag, "10.33.33.19", warningList);
			AssertEquals("Number of generated warnings", 0, warningList.Count);

			listenerChecker.CheckAgAndConnectionIP(ag, "20.20.20.20", warningList);
			AssertEquals("Number of generated warnings", 1, warningList.Count);

			AssertEquals("Warning text", "Ensure this application is using the availability group listener to connect to the database.", warningList[0].Action);
			AssertEquals("Warning text", "This application is not using the availability group listener to connect to the database.", warningList[0].Description);
			AssertEquals("Warning text", $"{BrandingFactory.Instance.ProductName} availability group checker", warningList[0].Source);
			AssertEquals("Warning text", "Database", warningList[0].SourceType);
			AssertEquals("Warning text", "AlwaysOn Availability Groups", warningList[0].WarningType);
		}

		protected override IChecker GetNewCheckerInstance()
		{
			return new AvailabilityGroupListenerChecker();
		}
	}
}
