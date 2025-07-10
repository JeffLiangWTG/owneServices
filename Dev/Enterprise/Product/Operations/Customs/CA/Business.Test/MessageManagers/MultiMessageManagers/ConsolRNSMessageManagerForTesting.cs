using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	sealed class ConsolRNSMessageManagerForTesting : ConsolRNSMessageManager
	{
		public ConsolRNSMessageManagerForTesting(IRNSRequestParent parent, IEnumerable<RNSRequestBO> requestBOs)
			: base(parent, requestBOs)
		{
		}

		public SingleMessageManager[] GetAllMessageManagers_Exposed()
		{
			return GetAllMessageManagers();
		}
	}
}
