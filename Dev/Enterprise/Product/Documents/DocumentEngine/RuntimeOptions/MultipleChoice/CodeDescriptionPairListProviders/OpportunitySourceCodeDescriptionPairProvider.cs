using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class OpportunitySourceCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var list = new CodeDescriptionPairList();
			foreach (ICodeDescriptionBool item in OrganisationsDataRegistry.Instance.OpportunitySource.Value)
			{
				list.AddPairIfNotExist(item.Code, item.Description);
			}

			return list;
		}
	}
}
