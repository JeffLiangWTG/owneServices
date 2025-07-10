using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CA.Business
{
	public partial class BondTypeList : Integration.Customs.CA.ICABondTypeCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return this;
		}
	}
}
