using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public interface ICodeDescriptionPairListProvider
	{
		ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList();
	}

	public interface IDependenceCodeDescriptionPairListProvider : ICodeDescriptionPairListProvider
	{
		ReadOnlyCodeDescriptionPairList GetDependenceCodeDescriptionPairList(string value);
	}
}

