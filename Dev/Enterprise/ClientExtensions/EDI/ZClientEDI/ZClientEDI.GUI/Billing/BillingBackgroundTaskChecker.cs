using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public interface IBillingBackgroundTaskChecker
	{
		bool CanProceed();
	}

	public class BillingBackgroundTaskChecker : IBillingBackgroundTaskChecker
	{
		public BillingBackgroundTaskChecker(string taskCode)
		{
			serviceTaskCode = taskCode;
		}

		readonly string serviceTaskCode;

		public bool CanProceed()
		{
			if (ObjectFactory.Get<IProductRegistration>().LocalVerify() != ProductRegistrationVerifyResult.OK)
			{
				// An unregistered system can't be running service tasks
				return true;
			}

			var status = GetTaskInstanceStatus();
			var statusText = Enum.GetName(typeof(ServiceTaskStatus), status);
			string msg = null;
			switch (status)
			{
				case ServiceTaskStatus.AtLeastOneHostIsRunningHealthily:
					break;
				case ServiceTaskStatus.ServiceTaskIsInactive:
					msg = "Service Task is inactive.\r\nBilling data may not be generated.\r\nRecommend re-activating service task.";
					break;
				default:
					msg = $"Service Task status is {statusText}.\r\nBilling data may not be generated.\r\nRecommend resolving issue before proceeding.";
					break;
			}

			if (msg != null)
			{
				msg += "\r\nAre you sure you wish to continue?";
				var result = Globals.Message.Show(msg, "Billing", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No);
				return result == DialogResult.Yes;
			}

			return true;
		}

		ServiceTaskStatus GetTaskInstanceStatus()
		{
			var querier = ObjectFactory.Get<IServiceManagerQuerier>();
			try
			{
				return querier.CheckStateOfNamedServiceTask(serviceTaskCode);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return ServiceTaskStatus.NoAvailableHosts;
			}
		}
	}
}
