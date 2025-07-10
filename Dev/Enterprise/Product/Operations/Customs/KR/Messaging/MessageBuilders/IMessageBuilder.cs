using System.IO;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IMessageBuilder
	{
		Stream MessageContent { get; }
	}
}
