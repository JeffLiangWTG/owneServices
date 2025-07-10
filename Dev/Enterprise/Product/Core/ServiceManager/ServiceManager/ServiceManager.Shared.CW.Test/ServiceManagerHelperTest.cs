using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Moq;
using NUnit.Framework;
using ServiceType = ServiceManager.Integration.ServiceHostUtilities.ServiceHostProcess.ServiceType;

namespace Enterprise.ServiceManager.Shared.Testing
{
	class ServiceManagerHelperTest : TransactionedTestCase
	{
		class WithProductRegistrationKey : IDisposable
		{
			readonly IDisposable substitute;
			readonly Mock<IProductRegistrationKey> productRegistrationKeyMock;
			readonly Mock<IProductRegistration> productRegistrationMock;

			public WithProductRegistrationKey(string enterpriseCode, string serverCode)
			{
				productRegistrationKeyMock = new Mock<IProductRegistrationKey>();
				productRegistrationKeyMock
					.SetupGet(key => key.EnterpriseCode)
					.Returns(enterpriseCode);
				productRegistrationKeyMock
					.SetupGet(key => key.ServerCode)
					.Returns(serverCode);

				productRegistrationMock = new Mock<IProductRegistration>();
				productRegistrationMock
					.SetupGet(registration => registration.Key)
					.Returns(productRegistrationKeyMock.Object);

				substitute = ObjectFactory.Substitute(productRegistrationMock.Object);
			}

			public void VerifyMockCalls(Func<Times> times)
			{
				AssertNoExceptionThrown(() =>
				{
					productRegistrationKeyMock.VerifyGet(key => key.EnterpriseCode, times);
					productRegistrationKeyMock.VerifyGet(key => key.ServerCode, times);
				});
			}

			public void Dispose()
			{
				substitute.Dispose();
				productRegistrationMock.VerifyNoOtherCalls();
				productRegistrationKeyMock.VerifyNoOtherCalls();
			}
		}

		static IEnumerable<(string serverName, string enterpriseCode, string serverCode, string expected)> GetTestCases(params string[] suffixes)
		{
			return new (string serverName, string enterpriseCode, string serverCode, string expected)[]
			{
				(null, "ent1", "srv1", $"http://localhost:7070/cargowise/processController/ent1srv1/{string.Join(string.Empty, suffixes)}"),
				(string.Empty, "ent2", "srv2", $"http://localhost:7070/cargowise/processController/ent2srv2/{string.Join(string.Empty, suffixes)}"),
				("host1", "ent3", "srv3", $"http://host1:7070/cargowise/processController/ent3srv3/{string.Join(string.Empty, suffixes)}"),
				("host2", "ent4", "srv4", $"http://host2:7070/cargowise/processController/ent4srv4/{string.Join(string.Empty, suffixes)}"),
				("test2.smth.com", "ent", "srv", $"http://test2.smth.com:7070/cargowise/processController/entsrv/{string.Join(string.Empty, suffixes)}"),
			};
		}

		static void Test<T>(Func<string, T> func, string serverName, string enterpriseCode, string serverCode, string expected)
		{
			using var disposable = new WithProductRegistrationKey(enterpriseCode, serverCode);
			AssertEquals(expected, func(serverName));
			disposable.VerifyMockCalls(Times.Once);
		}

		public void TestGetListenerStrongBinding_WithEnterpriseCodeAndServerCode()
		{
			CombineAssertions(() =>
			{
				foreach (var (serverName, enterpriseCode, serverCode, expected) in GetTestCases())
				{
					var replace = expected.Replace(string.IsNullOrEmpty(serverName) ? "localhost" : serverName, "+");
					ServiceTypeTest(ServiceManagerHelper.GetListenerStrongBinding, enterpriseCode, serverCode, new Dictionary<ServiceType, string>()
					{
						{ ServiceType.ProcessController, replace },
						{ ServiceType.LauncherSecurity, $"{replace}security/" },
					});
				}
			});
		}

		public void TestGetListenerStrongBinding_WithProductKey()
		{
			CombineAssertions(() =>
			{
				foreach (var (serverName, enterpriseCode, serverCode, expected) in GetTestCases())
				{
					var replace = expected.Replace(string.IsNullOrEmpty(serverName) ? "localhost" : serverName, "+");
					ServiceTypeTest(CallGetListenerStrongBindingWithProductKey, enterpriseCode, serverCode, new Dictionary<ServiceType, string>()
					{
						{ ServiceType.ProcessController, replace },
						{ ServiceType.LauncherSecurity, $"{replace}security/" },
					});
				}
			});
		}

		static string CallGetListenerStrongBindingWithProductKey(ServiceType serviceType, string enterpriseCode, string serverCode)
		{
			using var disposable = new WithProductRegistrationKey(enterpriseCode, serverCode);
			try
			{
				return ServiceManagerHelper.GetListenerStrongBinding(serviceType);
			}
			finally
			{
				disposable.VerifyMockCalls(Times.Once);
			}
		}

		public void TestGetLocalBaseUri()
		{
			CombineAssertions(() =>
			{
				foreach (var (serverName, enterpriseCode, serverCode, expected) in GetTestCases())
				{
					var replace = string.IsNullOrEmpty(serverName) ? expected : expected.Replace(serverName, "localhost");
					ServiceTypeTest(ServiceManagerHelper.GetLocalBaseUri, enterpriseCode, serverCode, new Dictionary<ServiceType, Uri>()
					{
						{ ServiceType.ProcessController, new Uri(replace) },
						{ ServiceType.LauncherSecurity, new Uri($"{replace}security/") },
					});
				}
			});
		}

		public void TestLogFilesUri()
		{
			CombineAssertions(() =>
			{
				foreach (var (serverName, enterpriseCode, serverCode, expected) in GetTestCases("logfiles"))
				{
					Test(ServiceManagerHelper.GetLogFilesUri, serverName, enterpriseCode, serverCode, expected);
				}
			});
		}

		public void TestGetStatusUri()
		{
			CombineAssertions(() =>
			{
				foreach (var (serverName, enterpriseCode, serverCode, expected) in GetTestCases("status"))
				{
					Test(ServiceManagerHelper.GetStatusUri, serverName, enterpriseCode, serverCode, expected);
				}
			});
		}

		public void TestGetTaskStatusUri()
		{
			CombineAssertions(() =>
			{
				foreach (var (serverName, enterpriseCode, serverCode, expected) in GetTestCases("taskStatus"))
				{
					Test(ServiceManagerHelper.GetTaskStatusUri, serverName, enterpriseCode, serverCode, expected);
				}
			});
		}

		public void TestGetIsAliveUri()
		{
			CombineAssertions(() =>
			{
				foreach (var (serverName, enterpriseCode, serverCode, expected) in GetTestCases("isAlive"))
				{
					Test(ServiceManagerHelper.GetIsAliveUri, serverName, enterpriseCode, serverCode, expected);
				}
			});
		}

		public void TestGetCommandUri()
		{
			CombineAssertions(() =>
			{
				foreach (var (serverName, enterpriseCode, serverCode, expected) in GetTestCases("command"))
				{
					Test(ServiceManagerHelper.GetCommandUri, serverName, enterpriseCode, serverCode, expected);
				}
			});
		}

		public void TestGetQueueStatusUri()
		{
			CombineAssertions(() =>
			{
				foreach (var (serverName, enterpriseCode, serverCode, expected) in GetTestCases("queueStatus"))
				{
					Test(ServiceManagerHelper.GetQueueStatusUri, serverName, enterpriseCode, serverCode, expected);
				}
			});
		}

		public void TestGetBindingListUri()
		{
			CombineAssertions(() =>
			{
				foreach (var (serverName, enterpriseCode, serverCode, expected) in GetTestCases("bindingList"))
				{
					Test(ServiceManagerHelper.GetBindingListUri, serverName, enterpriseCode, serverCode, expected);
				}
			});
		}

		public void TestGetLogFilesDirectory()
		{
			var appDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData);
			AssertEquals(Path.Combine(appDir, @"CargoWise edi\Process Controller\test1.smth.com\OdysseyDB"), ServiceManagerHelper.GetLogFilesDirectory("test1.smth.com", "OdysseyDB"));
			AssertEquals(Path.Combine(appDir, @"CargoWise edi\Process Controller\test$inst\Odyssey"), ServiceManagerHelper.GetLogFilesDirectory("test\\inst", null));
			AssertEquals(Path.Combine(appDir, @"CargoWise edi\Process Controller\localhost\Odyssey"), ServiceManagerHelper.GetLogFilesDirectory(".", ""));
		}

		public void TestIsLocalHost()
		{
			Assert(ServiceManagerHelper.IsLocalHost(null));
			Assert(ServiceManagerHelper.IsLocalHost(""));
			Assert(ServiceManagerHelper.IsLocalHost("."));
			Assert(ServiceManagerHelper.IsLocalHost("localhost"));
			Assert(ServiceManagerHelper.IsLocalHost("(local)"));
			Assert(ServiceManagerHelper.IsLocalHost(ServiceManagerHelper.GetHostName()));
			Assert(!ServiceManagerHelper.IsLocalHost("www.google.com"));

			Assert(!ServiceManagerHelper.IsLocalHost("blah!blah"));

			Assert(ServiceManagerHelper.IsLocalHost("localhost\\inst"));
			Assert(ServiceManagerHelper.IsLocalHost(ServiceManagerHelper.GetHostName() + "\\inst"));
			Assert(!ServiceManagerHelper.IsLocalHost("www.google.com\\inst"));
		}

		public void TestSQLServiceName()
		{
			AssertEquals("MSSQLServer", ServiceManagerHelper.SQLServiceName(null));
			AssertEquals("MSSQLServer", ServiceManagerHelper.SQLServiceName(""));
			AssertEquals("MSSQLServer", ServiceManagerHelper.SQLServiceName("."));
			AssertEquals("MSSQLServer", ServiceManagerHelper.SQLServiceName("localhost"));
			AssertEquals("MSSQLServer", ServiceManagerHelper.SQLServiceName("(local)"));
			AssertEquals("MSSQLServer", ServiceManagerHelper.SQLServiceName(ServiceManagerHelper.GetHostName()));
			AssertEquals("MSSQLServer", ServiceManagerHelper.SQLServiceName("www.google.com"));

			AssertEquals("MSSQL$inst", ServiceManagerHelper.SQLServiceName("localhost\\inst"));
			AssertEquals("MSSQL$inst", ServiceManagerHelper.SQLServiceName(ServiceManagerHelper.GetHostName() + "\\inst"));
			AssertEquals("MSSQL$inst", ServiceManagerHelper.SQLServiceName("www.google.com\\inst"));
		}

		public void TestAreSameHosts()
		{
			Assert(ServiceManagerHelper.AreSameHosts("localhost", "localhost"));
			Assert(ServiceManagerHelper.AreSameHosts("localhost", "127.0.0.1"));
			Assert(ServiceManagerHelper.AreSameHosts("localhost", System.Environment.MachineName));
			Assert(ServiceManagerHelper.AreSameHosts("127.0.0.1", System.Environment.MachineName));
		}

		public void TestGetDisplayName()
		{
			CombineAssertions(() =>
			{
				ServiceTypeTest(ServiceManagerHelper.GetDisplayName, ".", "", new Dictionary<ServiceType, string>
				{
					{ ServiceType.ProcessController, "Process Controller (localhost Odyssey)" },
					{ ServiceType.LauncherSecurity, "Process Launcher - Security (localhost Odyssey)" },
				});
				ServiceTypeTest(ServiceManagerHelper.GetDisplayName, "localhost", "Odyssey", new Dictionary<ServiceType, string>
				{
					{ ServiceType.ProcessController, "Process Controller (localhost Odyssey)" },
					{ ServiceType.LauncherSecurity, "Process Launcher - Security (localhost Odyssey)" },
				});
				ServiceTypeTest(ServiceManagerHelper.GetDisplayName, "dbServer", "databaseName", new Dictionary<ServiceType, string>
				{
					{ ServiceType.ProcessController, "Process Controller (dbServer databaseName)" },
					{ ServiceType.LauncherSecurity, "Process Launcher - Security (dbServer databaseName)" },
				});
			});
		}

		public void TestGetServiceDescription()
		{
			ServiceTypeTest(ServiceManagerHelper.GetServiceDescription, new Dictionary<ServiceType, string>()
			{
				{ ServiceType.ProcessController, "Manages and controls background tasks such as Email Processing, Report Scheduling, Database Consistency checks and backups." },
				{ ServiceType.LauncherSecurity, "Manages and controls background tasks such as access token generation." },
			});
		}

		public void TestGetExtraServiceTypes()
		{
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInExactOrder(
					Array.Empty<ServiceType>(),
					CallGetExtraServiceTypes(DataProtectionMechanisms.Codes.None));
				AssertContainsExactElementsInExactOrder(
					new[] { ServiceType.LauncherSecurity, },
					CallGetExtraServiceTypes(DataProtectionMechanisms.Codes.ActiveDirectory));
			});
			AssertContainsExactElementsInAnyOrder(
				"Unexpected mechanism, please add new expectation",
				new DataProtectionMechanisms().GetAllCodes(),
				new[] { DataProtectionMechanisms.Codes.None, DataProtectionMechanisms.Codes.ActiveDirectory });
		}

		ServiceType[] CallGetExtraServiceTypes(string dataProtectionMechanism)
		{
			using var mechanismOverride = SystemDataRegistry.Instance.SystemToSystemTrustDataProtectionMechanism.SetTemporaryValue(
				Guid.Empty,
				Guid.Empty,
				Guid.Empty,
				dataProtectionMechanism);
			return ServiceManagerHelper.GetExtraServiceTypes();
		}

		public void TestIncludeExtraServiceType_DependsOnDataProtectionMechanism()
		{
			CombineAssertions(() =>
			{
				ServiceTypeTest(CallIncludeExtraServiceType, DataProtectionMechanisms.Codes.None, new Dictionary<ServiceType, bool>()
				{
					{ ServiceType.ProcessController, false },
					{ ServiceType.LauncherSecurity, false },
				});
				ServiceTypeTest(CallIncludeExtraServiceType, DataProtectionMechanisms.Codes.ActiveDirectory, new Dictionary<ServiceType, bool>()
				{
					{ ServiceType.ProcessController, false },
					{ ServiceType.LauncherSecurity, true },
				});
			});
			AssertContainsExactElementsInAnyOrder(
				"Unexpected mechanism, please add new expectation",
				new DataProtectionMechanisms().GetAllCodes(),
				new[] { DataProtectionMechanisms.Codes.None, DataProtectionMechanisms.Codes.ActiveDirectory });
		}

		bool CallIncludeExtraServiceType(ServiceType serviceType, string dataProtectionMechanism)
		{
			using var mechanismOverride = SystemDataRegistry.Instance.SystemToSystemTrustDataProtectionMechanism.SetTemporaryValue(
				Guid.Empty,
				Guid.Empty,
				Guid.Empty,
				dataProtectionMechanism);
			return ServiceManagerHelper.IncludeExtraServiceType(serviceType);
		}

		void ServiceTypeTest<T>(Func<ServiceType, T> func, Dictionary<ServiceType, T> expectedDictionary)
		{
			CombineAssertions(() =>
			{
				ServiceTypeTestCore(null, func, expectedDictionary);
			});
		}

		void ServiceTypeTest<TArg, TResult>(Func<ServiceType, TArg, TResult> func, TArg arg, Dictionary<ServiceType, TResult> expectedDictionary)
		{
			ServiceTypeTestCore($"{arg}", serviceType => func(serviceType, arg), expectedDictionary);
		}

		void ServiceTypeTest<T1, T2, TResult>(Func<ServiceType, T1, T2, TResult> func, T1 arg1, T2 arg2, Dictionary<ServiceType, TResult> expectedDictionary)
		{
			ServiceTypeTestCore($"{arg1}|{arg2}", serviceType => func(serviceType, arg1, arg2), expectedDictionary);
		}

		void ServiceTypeTestCore<T>(string messagePrefix, Func<ServiceType, T> func, Dictionary<ServiceType, T> expectedDictionary)
		{
			messagePrefix = string.IsNullOrEmpty(messagePrefix) ? string.Empty : $"{messagePrefix}: ";
			foreach (ServiceType serviceType in Enum.GetValues(typeof(ServiceType)))
			{
				if (expectedDictionary.TryGetValue(serviceType, out var expected))
				{
					AssertEquals($"{messagePrefix}failure for {serviceType}", expected, func(serviceType));
				}
				else
				{
					Assert($"{messagePrefix}Unexpected process name, please add new expectation: {serviceType}", condition: false);
				}
			}

			const ServiceType unknownServiceType = (ServiceType)999;
			var e = AssertExceptionThrown<ArgumentOutOfRangeException>(() => func(unknownServiceType));
			AssertStartsWith($"{messagePrefix}Exception mismatch", "Unrecognized service type.", e?.Message);
			AssertEquals($"{messagePrefix}e.ParamName", "serviceType", e?.ParamName);
			AssertEquals($"{messagePrefix}e.ActualValue", unknownServiceType, e?.ActualValue);
		}
	}
}
