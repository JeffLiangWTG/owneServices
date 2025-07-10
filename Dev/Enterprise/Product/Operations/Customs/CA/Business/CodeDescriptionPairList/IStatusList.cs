namespace Enterprise.Customs.CA.Business
{
	public interface IStatusList
	{
		string GetDescriptionFromCode(string code);
		string GetFirstClearStatusFor(MessageType messageType);
	}
}
