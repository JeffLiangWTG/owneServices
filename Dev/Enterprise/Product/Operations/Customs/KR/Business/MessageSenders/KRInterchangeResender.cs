using Enterprise.Messaging.Business;

namespace Enterprise.Customs.KR.Business
{
	public class KRInterchangeResender : InterchangeResender
	{
		public KRInterchangeResender(Enterprise.Messaging.Business.EDIInterchange interchange) : base(interchange)
		{
		}

		protected override void SetToQueued(Enterprise.Messaging.Business.EDIInterchange interchange)
		{
			interchange.EI_Status = EDIInterchange.Status.eHubQueued;
		}
	}
}
