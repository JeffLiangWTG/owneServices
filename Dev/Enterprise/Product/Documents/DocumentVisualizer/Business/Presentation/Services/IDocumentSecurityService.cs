
namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface IDocumentSecurityService
	{
		bool CanView { get; }
		bool CanModify { get; }
		bool CanDeliver { get; }
		bool CanSendMessage { get; }
		bool AllowToolsAccess { get; }

		void ShowViewError();
		void ShowModifyError();
		void ShowDeliveryError();
		void ShowSendMessageError();
		void ShowAllowToolsAccessError();
	}
}
