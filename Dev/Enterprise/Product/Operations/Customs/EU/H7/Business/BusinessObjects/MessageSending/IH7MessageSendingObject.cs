using CargoWise.Types;

namespace Enterprise.Customs.EU.H7.Business
{
	public interface IH7MessageSendingObject
	{
		AsycudaBill Bill { get; }
		ZString Action { get; }

		MessageSender CreateSender();
		string MessageCreated(string messageText);
	}
}
