using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class HouseBillTypePairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList(Registry.Business.FreightDataRegistry.Instance.HouseBillOfLadingTypesForSea.Value.GetCodeDescriptionPairList());
			result.AddRangeOverwriteIfExists(new CodeDescriptionPairList(Registry.Business.FreightDataRegistry.Instance.HouseBillOfLadingTypesForRail.Value.GetCodeDescriptionPairList()));
			result.AddRangeOverwriteIfExists(new CodeDescriptionPairList(Registry.Business.FreightDataRegistry.Instance.HouseBillOfLadingTypesForRoad.Value.GetCodeDescriptionPairList()));
			return result;
		}
	}
}
