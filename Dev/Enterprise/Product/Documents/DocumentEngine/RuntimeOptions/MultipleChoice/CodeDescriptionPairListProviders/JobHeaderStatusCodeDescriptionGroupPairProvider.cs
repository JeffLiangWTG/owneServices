using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class JobHeadreStatusCodeDescriptionGroupPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return new JobHeaderStatusList();
		}
	}
}
