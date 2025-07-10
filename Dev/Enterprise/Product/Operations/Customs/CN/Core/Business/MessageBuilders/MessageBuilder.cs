namespace Enterprise.Customs.CN.Business
{
	public abstract class MessageBuilder<T>
	{
		protected abstract T GenerateMessage();

		public string GetXMLMessage() => CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.SerializeDefaultSettingsWithNamespaces(GenerateMessage());
	}
}
