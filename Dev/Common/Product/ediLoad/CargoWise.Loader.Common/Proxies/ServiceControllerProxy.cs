using System;
using System.ComponentModel;
using System.ServiceProcess;
using CargoWise.Common;

namespace CargoWise.Loader.Common
{
	public sealed class ServiceControllerProxy : IServiceControllerProxy
	{
		public bool TryGetStatus(string serviceName, out ServiceControllerStatus status)
		{
			using (ServiceController sc = new ServiceController(serviceName))
			{
				try
				{
					status = sc.Status;
					return true;
				}
				catch (Win32Exception ex)
				{
					LastError = ex.Message;
				}
				catch (InvalidOperationException ex)
				{
					Win32Exception win32Ex = ex.InnerException as Win32Exception;
					if (win32Ex != null)
					{
						LastError = win32Ex.Message;
					}
					else
					{
						throw;
					}
				}
			}

			status = 0;
			return false;
		}

		public bool Start(string serviceName)
		{
			return DoAction(serviceName, (sc) =>
				{
					if (sc.Status != ServiceControllerStatus.Running && sc.Status != ServiceControllerStatus.StartPending)
					{
						sc.Start();
						sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(90));
					}
				});
		}

		public bool Stop(string serviceName)
		{
			return DoAction(serviceName, (sc) =>
			{
				if (sc.Status != ServiceControllerStatus.Stopped && sc.Status != ServiceControllerStatus.StopPending)
				{
					sc.Stop();
					sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(120));
				}
			});
		}

		public string LastError { get; private set; }

		bool DoAction(string serviceName, Action<ServiceController> action)
		{
			Argument.NotNullOrEmpty(serviceName, nameof(serviceName));
			Argument.NotNull(action, nameof(action));
			using (ServiceController sc = new ServiceController(serviceName))
			{
				try
				{
					action(sc);
					return true;
				}
				catch (Win32Exception ex)
				{
					LastError = ex.Message;
				}
				catch (InvalidOperationException ex)
				{
					Win32Exception win32Ex = ex.InnerException as Win32Exception;
					if (win32Ex != null)
					{
						LastError = win32Ex.Message;
					}
					else
					{
						throw;
					}
				}
				catch (System.ServiceProcess.TimeoutException ex)
				{
					LastError = ex.Message;
				}
			}

			return false;
		}
	}
}
