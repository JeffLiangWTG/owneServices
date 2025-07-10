using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.EndpointManagement.Module
{
	public class EdiTrustedMessagingConfigFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			filters.AddTextFilter("Product", EdiTrustedMessagingConfigSchema.ETM_Product, new EdiTrustedMessagingConfigLookups(null).ProductTypeList);
			filters.AddTextFilter("Certificate Type", EdiTrustedMessagingConfigSchema.ETM_CertificateType, new CertificateTypeList());

			var globalFilter = filters.AddTextFilter("Global", GetGlobalConfigQuery);
			globalFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;

			return filters;
		}

		ZQuery GetGlobalConfigQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return EdiTrustedMessagingConfigGlobalCollection.GetGlobalTrustedMessagingConfigQuery();
		}
	}
}
