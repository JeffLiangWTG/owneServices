using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class DummyDependenceCodeDescriptionPairListProvider : IDependenceCodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return new CodeDescriptionPairList();
		}

		public ReadOnlyCodeDescriptionPairList GetDependenceCodeDescriptionPairList(string value)
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(value, value);
			return result;
		}
	}
}
