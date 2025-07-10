using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentEnterpriseMenuSectionListBuilder : IEnterpriseMenuSectionListBuilder
	{
		public CodeDescriptionPairList Build(ZString product, ZString productArea, bool excludeExternal = false, bool excludeDisabled = false)
		{
			return EDIDataRegistry.Instance.SystemProductMappings.Value.GetModuleList(product, productArea, excludeExternal, excludeDisabled);
		}
	}
}

