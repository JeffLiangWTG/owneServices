using System;
using System.Collections.Generic;
using System.ServiceProcess;
using Enterprise.RemotePrinting.Client.CustomAction;
using Microsoft.Win32;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Setup.Testing
{
	public class WindowsServiceHelperTest : TestCase
	{
		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		public void TestGetAllServicesRunningFromServiceControllerWhenCannotAccessServicesRegistry()
		{
			var testKeypath = Constants.RegistryManager.WebPrintConfigKeyName + "-JerryTest1";
			try
			{
				var key = Registry.CurrentUser.CreateSubKey(testKeypath);
				AssertNotNull("Create JerryTest1 for test", key);

				var throwException = true;
				var serviceHelper = new Mock<WindowsServicesHelper>("Test");
				serviceHelper.Protected().Setup<RegistryKey>("DefaultRegistryRoot").Returns(() =>
				{
					if (throwException)
					{
						throwException = false;
						throw new ApplicationException("Error when get all services from services registry");
					}

					return Registry.CurrentUser;
				});
				serviceHelper.Protected().Setup<IEnumerable<string>>("RunningServices").Returns(new[] { "ewpcsrv", "RemotePrintingService_JerryTest1", "RemotePrintingService_JerryTest2" });

				List<string> services = null;
				AssertNoExceptionThrown(() => services = serviceHelper.Object.GetAllServicesRunning());
				AssertArrayEqualsByElements(new[] { "ewpcsrv", "RemotePrintingService_JerryTest1" }, services.ToArray());
			}
			finally
			{
				Registry.CurrentUser.DeleteSubKey(testKeypath, false);
			}
		}
	}

	public class WindowsServiceHelperForTest : IWindowsServicesHelper
	{
		public bool InstallNewService(string serviceName, string configName, string startMode, string login, string password, out string errorMessage)
		{
			throw new NotImplementedException();
		}

		public string CheckServiceControllerStatusWithRetry(string serviceName, int timeoutMs, Func<string, bool> retryFilter)
		{
			throw new NotImplementedException();
		}

		public string CheckServiceControllerStatus(string serviceName)
		{
			throw new NotImplementedException();
		}

		public bool DeleteService(string serviceName, out string errorMessage)
		{
			errorMessage = "Something";
			return DeleteServiceResultOverride;
		}
		public bool DeleteServiceResultOverride { get; set; }

		public bool StopProcess(string serviceName, out string errorMessage)
		{
			throw new NotImplementedException();
		}

		public bool StartProcess(string serviceName, out string errorMessage)
		{
			throw new NotImplementedException();
		}

		public string GetServiceNameFromConfigName(string configName)
		{
			throw new NotImplementedException();
		}

		public string GetConfigNameArgumentValue(string[] args)
		{
			throw new NotImplementedException();
		}

		public string GetConfigName(string serviceName)
		{
			return ConfigNameForTest;
		}
		public string ConfigNameForTest { get; set; }

		public void ChangeConfigName(string serviceName, string newConfigName, string newDisplayName = null)
		{
			ChangedConfigName = true;
			ChangedServiceName = serviceName;
			ChangedNewConfigName = newConfigName;
			ChangedNewDisplayName = newDisplayName;
		}
		public bool ChangedConfigName { get; private set; }
		public string ChangedServiceName { get; private set; }
		public string ChangedNewConfigName { get; private set; }
		public string ChangedNewDisplayName { get; private set; }

		public string GetDisplayName(string configName)
		{
			return "Test Service - " + configName;
		}

		public IServiceController GetServiceController(string serviceName)
		{
			if (serviceControllers.TryGetValue(serviceName, out var controller))
			{
				return controller;
			}
			return null;
		}

		public void AddServiceControllerForTest(string serviceName, IServiceController controller)
		{
			serviceControllers[serviceName] = controller;
		}

		readonly Dictionary<string, IServiceController> serviceControllers = new Dictionary<string, IServiceController>();

		public List<string> GetAllServicesRunning()
		{
			if (InstalledServicesOverride == null)
			{
				throw new ApplicationException("Error getting service names");
			}
			return InstalledServicesOverride;
		}
		public List<string> InstalledServicesOverride { get; set; }
	}

	class ServiceControllerForTest : IServiceController
	{
		public ServiceControllerForTest(ServiceControllerStatus status, ServiceStartMode startType)
		{
			Status = status;
			StartType = startType;
		}

		public void Dispose()
		{
			Disposed = true;
		}
		public bool Disposed { get; private set; }

		public ServiceControllerStatus Status { get; private set; }
		public ServiceStartMode StartType { get; }

		public Action OnStop { get; set; }
		public Action<ServiceControllerStatus, TimeSpan> OnWaitForStatus { get; set; }

		public void Stop()
		{
			if (OnStop != null)
			{
				OnStop();
			}
			else
			{
				Stopped = true;
			}
		}
		public bool Stopped { get; private set; }

		public void Start()
		{
			Started = true;
		}
		public bool Started { get; private set; }

		public void WaitForStatus(ServiceControllerStatus desiredStatus, TimeSpan timeout)
		{
			if (OnWaitForStatus != null)
			{
				OnWaitForStatus(desiredStatus, timeout);
			}
			else
			{
				Status = desiredStatus;
			}
		}
	}
}
