using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class OrgFreightDirectionCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return new MasterFiles.Business.OrgHeaderLookups(null).FilterFreightDirectionList;
		}
	}
}
