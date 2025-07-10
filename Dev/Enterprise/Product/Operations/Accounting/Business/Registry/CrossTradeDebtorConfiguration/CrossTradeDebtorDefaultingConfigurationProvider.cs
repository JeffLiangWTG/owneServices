using System.Collections.Generic;
using System.Linq;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public class CrossTradeDebtorDefaultingConfigurationProvider : ICrossTradeDebtorDefaultingConfigurationProvider
	{
		public List<ICrossTradeDebtorDefaultingConfigurationItem> GetConfiguration()
		{
			if (AccountingMasterFilesRegistry.Instance.EnableCrossTradeDebtorDefaultingFunctionality.Value)
			{
				var configs = AccountingConfigurationRegistry.Instance.CrossTradeDebtorDefaultingConfiguration.Value?.Configurations ?? new CrossTradeDebtorConfigurationCollection();
				return configs.OfType<ICrossTradeDebtorDefaultingConfigurationItem>().ToList();
			}
			else
			{
				return new List<ICrossTradeDebtorDefaultingConfigurationItem> { };
			}
		}
	}
}
