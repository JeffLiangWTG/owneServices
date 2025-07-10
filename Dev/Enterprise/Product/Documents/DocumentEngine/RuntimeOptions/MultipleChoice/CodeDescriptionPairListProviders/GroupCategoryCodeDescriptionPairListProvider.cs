using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class GroupCategoryCodeDescriptionPairListProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
			=> SystemDataRegistry.Instance.GroupCategoryList.Value;
	}
}
