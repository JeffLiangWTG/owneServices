
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class WhsPickMethodCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return WarehouseDataRegistry.Instance.PickMethod.Value.GetCodeDescriptionPairList();
		}
	}
}
