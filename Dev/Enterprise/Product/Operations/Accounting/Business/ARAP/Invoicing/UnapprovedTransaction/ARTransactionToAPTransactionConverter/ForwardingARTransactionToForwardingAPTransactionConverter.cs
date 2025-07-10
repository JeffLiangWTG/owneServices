using Enterprise.Accounting.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	internal class ForwardingARTransactionToForwardingAPTransactionConverter : ARTransactionToAPTransactionConverterBase
	{
		public ForwardingARTransactionToForwardingAPTransactionConverter(NotificationBuffer notificationBuffer) : base(notificationBuffer)
		{
		}

		protected override bool ShouldCreateConsolCostsForConsol(IJobCostingPlugIn consol) => consol != null;
	}
}
