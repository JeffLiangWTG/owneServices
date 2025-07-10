using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EnterpriseMenuSectionListBuilder : IEnterpriseMenuSectionListBuilder
	{
		public CodeDescriptionPairList Build(ZString product, ZString productArea, bool excludeExternal = false, bool excludeDisabled = false)
		{
			var list = new CodeDescriptionPairList();

			list.AddRange(new EnterpriseModuleList());
			list.AddRangeOverwriteIfExists(EDIDataRegistry.Instance.LegacyMenuSectionMappings.Value.GetLegacyModules());

			return list;
		}
	}
}

