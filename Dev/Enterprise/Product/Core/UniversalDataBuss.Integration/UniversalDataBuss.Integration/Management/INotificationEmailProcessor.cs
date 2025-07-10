using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface INotificationEmailProcessor
	{
		HtmlEmailDef Process(IEDIMessage message, HtmlEmailDef htmlEmailDef);
	}
}
