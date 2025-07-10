using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class StorageClassCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return Env.Registry.ContainerStorageClass;
		}
	}
}
