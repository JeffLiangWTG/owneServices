using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.US
{
	public partial class MessageStatusListENS : CodeDescriptionPairList,
		Integration.Customs.US.IENSStatusCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		#region ICodeDescriptionPairListProvider Members

		ReadOnlyCodeDescriptionPairList DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider.GetCodeDescriptionPairList()
		{
			return this;
		}

		#endregion
	}
}
