using System.IO;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine
{
	public interface IPrintTaskUIProvider
	{
		bool ShowRuntimeOptionsUI(PrintTask printTask, AllowedDeliveryOptions deliveryOptions, DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint);
		bool ShowPrintTaskDeliveryUI(PrintTaskSettings taskSettings);
		IProgressNotificationUI GetNewProgressNotificationUI(PrintTaskSettings taskSettings);
		IProgressNotificationUI GetNewProgressNotificationUI(DeliveryInstructions instructions, int totalPacks);
		bool ShowDocDeliveryUI(PrintTask printTask, DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint);
		void ShowPrinterSelectionUI(DeliveryInstructions deliveryInstructions);
		void ShowPreview(Stream xlsStream, DeliveryInfo[] deliveryInfos, IDeliverCapableForm parentForm);
		bool ShowErrors(Report report);
		void ShowWarning(string caption, string message);
	}
}
