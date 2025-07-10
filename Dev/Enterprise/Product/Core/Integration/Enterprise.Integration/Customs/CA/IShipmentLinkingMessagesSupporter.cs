using Enterprise.Integration.Freight;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class CA
		{
			public interface IShipmentLinkingMessagesSupporterProvider
			{
				IShipmentLinkingMessagesSupporter Create(ICommonShipment shipment);
			}

			public interface IShipmentLinkingMessagesSupporter
			{
				void HookShipment();
				void UnHookShipment();
			}
		}
	}
}
