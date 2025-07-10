using CargoWise.EntityFramework;

namespace Enterprise.Customs.BE.Business;

public abstract class MessageSender
{
	public abstract void Send();

	public string MessageSubType;

	public BusinessObject MessageObject { get; set; }

	public abstract object DataProvider { get; }

	public abstract bool IsTestMessage { get; }
}
