using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Registry
{
	public static class VATReportConfigurationLookups
	{
		public static CodeDescriptionPairList ConfigurationLookups
		{
			get
			{
				var manager = new ColumnConfigurationsManager(FRCustomsDataRegistry.FRVATReportPK, false);
				var list = manager.ConfigurationManagersForAllSavedConfigurations;
				var result = new CodeDescriptionPairList();
				foreach (var item in list)
				{
					result.AddPair(item.Description);
				}
				return result;
			}
		}
	}
}
