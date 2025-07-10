using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class HBLPackLinesDisplayOrderRegistryDataType : CodePairRegistryDataType
	{
		public HBLPackLinesDisplayOrderRegistryDataType(ICodeDescriptionPairListProvider lookUpListProvider) : base(lookUpListProvider, false, true)
		{
		}

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return new HBLPackLinesDisplayOrderRegistryEditorInfo(LookUpListProvider);
		}
	}
}
