using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class OpportunityProductTypeCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			foreach (CodeDescriptionBool element in OrganisationsDataRegistry.Instance.ProductTypeList.Value)
			{
				result.AddPair(element.Code, element.Description);
			}
			return result;
		}
	}
}
