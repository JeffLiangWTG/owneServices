using System;
using CargoWise.Application;

namespace Enterprise.BufferManagement.Business
{
	public class TransferRuleRunnerParams : ITransferRuleRunnerParams
	{
		readonly Lazy<bool> isCdcEnabled;

		public bool IsCdcEnabled => isCdcEnabled.Value;

		public bool IsResponsive { get; }
		public TransferRuleRunnerParams(bool responsive = false)
		{
			IsResponsive = responsive;
			isCdcEnabled = new Lazy<bool>(() => ObjectFactory.Get<IResponsiveManagementServiceTaskChecker>().AreDependentServiceTasksActive());
		}
	}
}
