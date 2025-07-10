namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class MessageSubTypes
	{
		MessageSubTypes()
		{
		}

		public static MessageSubType Original { get; } = new MessageSubType(AirCargoMessage.MessageSubType.Original);
		public static MessageSubType Amendment { get; } = new MessageSubType(AirCargoMessage.MessageSubType.Amendment);
		public static MessageSubType Withdraw { get; } = new MessageSubType(AirCargoMessage.MessageSubType.Withdraw);
		public static MessageSubType PartShipment { get; } = new MessageSubType(AirCargoMessage.MessageSubType.PartShipment);
		public static MessageSubType UnderbondRequest { get; } = new MessageSubType(AirCargoMessage.MessageSubType.UnderbondRequest);
		public static MessageSubType UnderbondAcquittal { get; } = new MessageSubType(AirCargoMessage.MessageSubType.UnderbondAcquittal);
		public static MessageSubType UnderbondCancel { get; } = new MessageSubType(AirCargoMessage.MessageSubType.UnderbondCancel);
	}
}
