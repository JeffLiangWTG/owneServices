using System;
using Enterprise.Messaging.Business;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	class LightweightOutboundInterchangeCandidate : LightweightOutboundItem<EDIInterchange>
	{
		public LightweightOutboundInterchangeCandidate(EDIInterchange interchange)
			: base()
		{
			this.interchange = interchange;
		}

		public Guid SessionGuid => interchange.EI_SessionGUID.ToGuid();

		protected override Guid GetPK() => interchange.PK.ToGuid();
		protected override EDIInterchange GetFullItem() => interchange;

		readonly EDIInterchange interchange;
	}
}
