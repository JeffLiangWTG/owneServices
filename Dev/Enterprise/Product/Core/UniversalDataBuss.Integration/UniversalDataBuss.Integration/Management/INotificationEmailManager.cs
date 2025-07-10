using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface INotificationEmailManager
	{
		void Register(INotificationEmailProcessor processor);
		HtmlEmailDef Process(IEDIMessage message, HtmlEmailDef email);
		void Clear();
	}
}
