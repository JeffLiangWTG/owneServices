using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class QuoteCancellationReasonCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return RatingDataRegistry.Instance.QuoteCancellationReasonCodes.Value.GetCodeDescriptionPairList();
		}
	}
}
