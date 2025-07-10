using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DbHealth.Check.Testing
{
	[TestedType(typeof(DbServerAddressChecker))]
	sealed class DbServerAddressCheckerTest : CheckerTestCaseBase
	{
		public void TestServerAddressIsConfiguredWithIpAddress()
		{
			string[] itemsWithIpAddress =
			{
				@"101.102.103.104",
				@"101.102.103.104\Instance1",
				@"1.2.3.4",
				@"1.2.3.4\Instance1",
				@"2001:0db8:0001:0000:0000:0ab9:C0A8:0102",
				@"2001:0db8:0001:0000:0000:0ab9:C0A8:0102\Instance",
				@"::11.22.33.44",
				@"::11.22.33.44\Instance",
				@"2001:db8::123.123.123.123",
				@"2001:db8::123.123.123.123\Instance",
				@"::1234:5678:91.123.4.56",
				@"::1234:5678:91.123.4.56\Instance",
				@"::1234:5678:1.2.3.4",
				@"::1234:5678:1.2.3.4\Instance",
				@"2001:db8::1234:5678:5.6.7.8",
				@"2001:db8::1234:5678:5.6.7.8\Instance",
				@"::\Instance1",
				@"a::\Instance1",
				@"a:b::\Instance1",
				@"a:b:c::\Instance1",
				@"a:b:c:d::\Instance1",
				@"a:b:c:d:e::\Instance1",
				@"a:b:c:d:e:f::\Instance1",
				@"a:b:c:d:e:f:a::\Instance1",
				@"a:b:c:d:e:f:a:b\Instance1",
				@"::a\Instance1",
				@"::a:b\Instance1",
				@"::a:b:c\Instance1",
				@"::a:b:c:d\Instance1",
				@"::a:b:c:d:e\Instance1",
				@"::a:b:c:d:e:f\Instance1",
				@"::a:b:c:d:e:f:a\Instance1",
				@"::a:b:c:d:e:f:a:b\Instance1",
				@"a::b\Instance1",
				@"a::b:c\Instance1",
				@"a::b:c:d\Instance1",
				@"a::b:c:d:e\Instance1",
				@"a::b:c:d:e:f\Instance1",
				@"a::b:c:d:e:f:a\Instance1",
				@"a::b:c:d:e:f:a:b\Instance1",
				@"a:b::c\Instance1",
				@"a:b::c:d\Instance1",
				@"a:b::c:d:e\Instance1",
				@"a:b::c:d:e:f\Instance1",
				@"a:b::c:d:e:f:a\Instance1",
				@"a:b::c:d:e:f:a:b\Instance1",
				@"a:b:c::d\Instance1",
				@"a:b:c::d:e\Instance1",
				@"a:b:c::d:e:f\Instance1",
				@"a:b:c::d:e:f:a\Instance1",
				@"a:b:c::d:e:f:a:b\Instance1",
				@"a:b:c:d::e\Instance1",
				@"a:b:c:d::e:f\Instance1",
				@"a:b:c:d::e:f:a\Instance1",
				@"a:b:c:d::e:f:a:b\Instance1",
				@"a:b:c:d:e::f\Instance1",
				@"a:b:c:d:e::f:a\Instance1",
				@"a:b:c:d:e::f:a:b\Instance1",
				@"a:b:c:d:e:f::a\Instance1",
				@"a:b:c:d:e:f::a:b\Instance1",
				@"a:b:c:d:e:f:a::b\Instance1",
			};

			string[] itemsWithNoIpAddress =
			{
				null,
				string.Empty,
				" ",
				".",
				".:1430",
				"Localhost",
				"Localhost:1430",
				"Myserver",
				"Myserver:1430",
				"1",
				"1:1430",
				"Myserver.WTG.Zone",
				"Myserver.WTG.Zone:1430",
				@".\Instance1",
				@".:1430\Instance1",
				@"Localhost\Instance1",
				@"Localhost:1430\Instance1",
				@"Myserver\Instance1",
				@"Myserver:1430\Instance1",
				@"1\Instance1",
				@"1:1430\Instance1",
				@"Myserver.WTG.Zone\Instance1",
				@"Myserver.WTG.Zone:1430\Instance1",
				@"1.2.3",
				@"1.2.3\Instance1",
			};

			CombineAssertions(
			() =>
			{
				foreach (var item in itemsWithIpAddress)
				{
					Assert($"Expected True, but was False : {item}", DbServerAddressChecker.ContainsIpAddress(item));
				}
				foreach (var item in itemsWithNoIpAddress)
				{
					Assert($"Expected False, but was True : {item}", !DbServerAddressChecker.ContainsIpAddress(item));
				}
			});
		}

		[UseSnapshotProtection]
		public void TestCheck()
		{
			// BiAuditServer

			SystemDataRegistry.Instance.BiAuditServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.1.1:1433\\instance");
			SystemDataRegistry.Instance.BiDataWarehouseServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "OK\\instance");
			SystemDataRegistry.Instance.ReportingDbServerNames.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { "OK\\instance" });

			var logger = new TestServiceLogger();
			var warningList = new DbHealthWarningList();
			CheckerToTest.Check(Db.Connection, warningList, logger);

			var message = warningList.ToHtmlMessage();

			CombineAssertions("Audit Server", () =>
			{
				AssertEquals(1, warningList.Count);
				AssertContains("The registry item \"System -> BI -> Audit Server\" is configured using IP address instead of fully qualified domain name.", message);
				AssertContains("Avoid using IP address and ensure the fully qualified domain name of the server is used for the registry item \"System -> BI -> Audit Server\".", message);
			});

			// BiDataWarehouseServer

			SystemDataRegistry.Instance.BiAuditServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "OK\\instance");
			SystemDataRegistry.Instance.BiDataWarehouseServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "2.2.2.2:1433\\instance");
			SystemDataRegistry.Instance.ReportingDbServerNames.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { "OK\\instance" });

			warningList = new DbHealthWarningList();
			CheckerToTest.Check(Db.Connection, warningList, logger);

			message = warningList.ToHtmlMessage();

			CombineAssertions("Data Warehouse Server", () =>
			{
				AssertEquals(1, warningList.Count);
				AssertContains("The registry item \"System -> BI -> Data Warehouse Server\" is configured using IP address instead of fully qualified domain name.", message);
				AssertContains("Avoid using IP address and ensure the fully qualified domain name of the server is used for the registry item \"System -> BI -> Data Warehouse Server\".", message);
			});

			// ReportingDbServerNames

			SystemDataRegistry.Instance.BiAuditServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "OK\\instance");
			SystemDataRegistry.Instance.BiDataWarehouseServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "OK\\instance");
			SystemDataRegistry.Instance.ReportingDbServerNames.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { "3.3.3.3:1433\\instance" });

			warningList = new DbHealthWarningList();
			CheckerToTest.Check(Db.Connection, warningList, logger);

			message = warningList.ToHtmlMessage();

			CombineAssertions("Reporting databases full server names", () =>
			{
				AssertEquals(1, warningList.Count);
				AssertContains("The registry item \"System -> Reports -> Reporting databases full server names\" is configured using IP address instead of fully qualified domain name.", message);
				AssertContains("Avoid using IP address and ensure the fully qualified domain name of the server is used for the registry item \"System -> Reports -> Reporting databases full server names\".", message);
			});
		}

		[UseSnapshotProtection]
		public void TestCheckForModuleQueryDbServerNames()
		{
			using (SystemDataRegistry.Instance.ModuleQueryDbServerNames.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { "4.4.4.4:1433\\instance" }))
			{
				var logger = new TestServiceLogger();
				var warningList = new DbHealthWarningList();
				CheckerToTest.Check(Db.Connection, warningList, logger);

				var message = warningList.ToHtmlMessage();

				CombineAssertions("Secondary Databases Full Server Names", () =>
				{
					AssertEquals(1, warningList.Count);
					AssertContains("The registry item \"Optimization -> Secondary Databases Full Server Names\" is configured using IP address instead of fully qualified domain name.", message);
					AssertContains("Avoid using IP address and ensure the fully qualified domain name of the server is used for the registry item \"Optimization -> Secondary Databases Full Server Names\".", message);
				});
			}
		}

		public void TestCheckDbServerName()
		{
			var warningList = new DbHealthWarningList();
			var checker = new DbServerAddressChecker();
			checker.CheckAndAddWarningForDbServerName("1.1.1.1\\tst", warningList);

			var message = warningList.ToHtmlMessage();

			CombineAssertions("Reporting databases full server names", () =>
			{
				AssertEquals(1, warningList.Count);
				AssertContains("An IP address is being used to connect to the SQL server instead of the FQDN.", message);
				AssertContains("Avoid using IP address and ensure the fully qualified domain name of the server is used to launch the application.", message);
			});
		}

		protected override IChecker GetNewCheckerInstance()
		{
			return new DbServerAddressChecker();
		}
	}
}
