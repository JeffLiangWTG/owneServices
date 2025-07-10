using Enterprise.Integration.Freight;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class CH
		{
			public interface IForwardingConsolIntegrationHelper
			{
				bool CanPrintGroupDeliveryNoteForConsol(Forwarding.IForwardingConsol forwardingConsol, IDocumentSupporterQueryProvider queryProvider);
			}
		}
	}
}
