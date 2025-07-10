using System.IO;

namespace Enterprise.Messaging.Integration
{
	public interface IMessageDataProvider
	{
		BinaryReader GetMessageData();
	}
}
