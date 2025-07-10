using System;
using Enterprise.Billing.Integration;

namespace Enterprise.eHubMessaging.ServiceTasks.Outbound.Scavenging.Billing
{
	class LightweightOutboundBillingItem : LightweightOutboundItem<StmUsageData>
	{
		public LightweightOutboundBillingItem(StmUsageData stmUsageData)
			: base()
		{
			this.stmUsageData = stmUsageData;
		}

		public byte[] BillingData
		{
			get { return billingData ?? (billingData = BillingManager.GetTransactionData(stmUsageData)); }
		}

		protected override Guid GetPK() => stmUsageData.PK.ToGuid();
		protected override StmUsageData GetFullItem() => stmUsageData;

		readonly StmUsageData stmUsageData;
		byte[] billingData;
	}
}
