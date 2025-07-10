using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface IFactLoaderProvider
	{
		IFactLoader GetFactLoader(string consumerType, RulesContextType rulesContextType);
	}
}
