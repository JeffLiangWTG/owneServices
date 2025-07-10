using Enterprise.Registry.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class CategoryCodeDescriptionPairListProvider : ICodeDescriptionPairListProvider
	{
		public ZArchitecture.Core.ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return OrganisationsDataRegistry.Instance.CategoryList.Value.GetCodeDescriptionPairList();
		}
	}
}
