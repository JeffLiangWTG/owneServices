using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.US
{
	public partial class MessageStatusListEI : CodeDescriptionPairList,
		Integration.Customs.US.IEIStatusCodeDescriptionPairProvider,
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
