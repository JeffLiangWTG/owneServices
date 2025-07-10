
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class WorkflowTypePairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			foreach (WorkflowDescriptor module in WorkflowDescriptors.Instance.Values)
			{
				if (module.SupportsEventTracking)
				{
					result.AddPair(module.Code, module.Description.ToString());
				}
			}
			return result;
		}
	}
}
