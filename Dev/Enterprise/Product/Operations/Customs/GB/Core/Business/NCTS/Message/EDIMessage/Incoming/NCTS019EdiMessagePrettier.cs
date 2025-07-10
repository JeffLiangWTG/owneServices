using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class NCTS019EdiMessagePrettier : NCTS019ResponsePrettyFormatter
	{
		public NCTS019EdiMessagePrettier(EDIMessage message) : base(message)
		{
		}
	}
}
