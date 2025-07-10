namespace Enterprise.Customs.FR.Messaging.Interfaces.CIN
{
	public interface ICIN755ExportMessage : ICINExportMessage
	{
		///<summary>
		/// Xml Tag: EnveloppeCIN
		///</summary>
		ICINNested755Envelope NestedCINMessageEnvelope { get; }
	}
}
