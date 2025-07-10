using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using CargoWise.ApplicationManager.Common;
using CargoWise.Common;

namespace Enterprise.Dat.Implementation.Preconditions
{
	public class SqlServerServiceRestarter : IPrecondition, IAppManagerInvocable
	{
		public bool CheckPreconditionMet()
		{
			bool result;
			if (reasonsToRestart != null && reasonsToRestart.Count > 0)
			{
				try
				{
					result = Invoke(false, SqlServerTools.SqlServerServiceName).Status == AppManagerResultStatus.Success;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					result = false;
				}

				if (!result)
				{
					try
					{
						var invokeResult = InvokeViaAppManager();
						result = invokeResult.Status == AppManagerResultStatus.Success;
						if (!result)
						{
							reasonForFailedRestart = invokeResult.Message;
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						result = false;
						reasonForFailedRestart = ex.ToString();
					}
				}
			}
			else
			{
				result = true;
			}
			return result;
		}

		AppManagerResult InvokeViaAppManager()
		{
			using (var appManagerClient = new AppManagerClient())
			{
				var type = typeof(SqlServerServiceRestarter);
				return appManagerClient.Invoke(type.Assembly.Location, type.FullName, SqlServerTools.SqlServerServiceName, null);
			}
		}

		public string ErrorMessage
		{
			get { return "SQL Server requires restart for the following reasons: " + string.Join(",", reasonsToRestart.ToArray()) + "\r\nSQL Server could not be restarted automatically: " + reasonForFailedRestart; }
		}

		string reasonForFailedRestart = string.Empty;

		public void RequireRestart(string reason)
		{
			if (reasonsToRestart == null)
			{
				reasonsToRestart = new List<string>();
			}
			reasonsToRestart.Add(reason);
		}

		List<string> reasonsToRestart;

		public AppManagerResult Invoke(bool waitedForMutex, object state)
		{
			var serviceController = ServiceController.GetServices().FirstOrDefault(service => service.ServiceName.Equals((string)state, StringComparison.OrdinalIgnoreCase));
			if (serviceController == null)
			{
				return AppManagerResult.GetError("Could not obtain service controller for service " + (string)state);
			}
			else
			{
				serviceController.Stop();
				serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(5));
				serviceController.Start();
				serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(5));
				return new AppManagerResult(AppManagerResultStatus.Success);
			}
		}
	}
}
