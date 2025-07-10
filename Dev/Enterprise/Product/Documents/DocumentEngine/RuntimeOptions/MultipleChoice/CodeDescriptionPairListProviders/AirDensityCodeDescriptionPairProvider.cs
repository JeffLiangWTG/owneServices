using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class AirDensityCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return new CodeDescriptionPairList(OLookUpEditType.AirDensity);
		}
	}
}
