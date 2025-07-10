using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	sealed class RNSMultiMessageManagerForTesting : RNSMultiMessageManager
	{
		public RNSMultiMessageManagerForTesting(IRNSRequestParent parent)
			: base(parent)
		{
		}

		public SingleMessageManager[] GetAllMessageManagers_Exposed()
		{
			return GetAllMessageManagers();
		}
	}
}
