using System.Collections.Generic;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class FRSendNCTSArrivalNotificationMessageProcessor : FRSendNCTSMessageProcessor
	{
		public FRSendNCTSArrivalNotificationMessageProcessor(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		protected override IEnumerable<IAutoSendNCTSMessageRule> AutoSendNctsMessages
		{
			get
			{
				yield return new FromSANTriggerToIE007Rule();
			}
		}
	}
}
