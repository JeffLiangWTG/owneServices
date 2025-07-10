namespace Enterprise.Customs.FR.Messaging.Interfaces.CIN
{
	public interface ICIN745ExportMessage : ICINExportMessage
	{
		ICINNested745Message MessageCIN745 { get; }
	}
}
