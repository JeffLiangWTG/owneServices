using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class ConsolTypePairListProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.AgentType);
			list.AddPair(Core.Constants.AgentType.AWBCoload, Core.Constants.AgentTypeDescriptions.AWBCoload);
			list.AddPair(Core.Constants.AgentType.AWBMaster, Core.Constants.AgentTypeDescriptions.AWBMaster);
			return list;
		}
	}
}
