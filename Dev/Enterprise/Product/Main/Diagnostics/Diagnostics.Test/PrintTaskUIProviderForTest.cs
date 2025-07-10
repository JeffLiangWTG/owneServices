using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Testing;

namespace Enterprise.Diagnostics.Testing
{
	/// <summary>
	/// Provider that generates an exception if the DocWrapperContextManager doesn't have a documentPackContext while showing UI.
	/// 
	/// It calls DeliveryInstructions.DeliverablesToBePrinted.HasIncludedPreviewableDocuments
	/// during ShowDocDeliveryUI to simulate the behaviour of DocDeliveryForm.PreviewButton_Click.
	/// This will cause an InvalidOperationException if the documentPackContext is null.
	/// </summary>
	class PrintTaskUIProviderForTest : IPrintTaskUIProvider
	{
		internal PrintTaskUIProviderForTest()
		{
			testProvider = new PrintTaskForcePreviewTestingUIProvider();
			provider = testProvider;
		}

		internal PrintTaskForcePreviewTestingUIProvider testProvider;
		readonly IPrintTaskUIProvider provider;

		IProgressNotificationUI IPrintTaskUIProvider.GetNewProgressNotificationUI(DeliveryInstructions instructions, int totalPacks)
		{
			return provider.GetNewProgressNotificationUI(instructions, totalPacks);
		}

		IProgressNotificationUI IPrintTaskUIProvider.GetNewProgressNotificationUI(PrintTaskSettings taskSettings)
		{
			return provider.GetNewProgressNotificationUI(taskSettings);
		}

		bool IPrintTaskUIProvider.ShowDocDeliveryUI(PrintTask printTask, DeliveryInstructions instructions, ZArchitecture.Modules.ISecurityCheckpoint modifyDocumentCheckPoint)
		{
			return instructions.DeliverablesToBePrinted.HasIncludedPreviewableDocuments
				&& provider.ShowDocDeliveryUI(printTask, instructions, modifyDocumentCheckPoint);
		}

		bool IPrintTaskUIProvider.ShowErrors(Report report)
		{
			return provider.ShowErrors(report);
		}

		void IPrintTaskUIProvider.ShowPreview(System.IO.Stream xlsStream, DocumentEngine.DeliveryMethods.DeliveryInfo[] deliveryInfos, DocumentEngine.DocumentDelivery.IDeliverCapableForm parentForm)
		{
			provider.ShowPreview(xlsStream, deliveryInfos, parentForm);
		}

		bool IPrintTaskUIProvider.ShowPrintTaskDeliveryUI(PrintTaskSettings taskSettings)
		{
			return provider.ShowPrintTaskDeliveryUI(taskSettings);
		}

		void IPrintTaskUIProvider.ShowPrinterSelectionUI(DeliveryInstructions deliveryInstructions)
		{
			provider.ShowPrinterSelectionUI(deliveryInstructions);
		}

		bool IPrintTaskUIProvider.ShowRuntimeOptionsUI(PrintTask printTask, AllowedDeliveryOptions deliveryOptions, DeliveryInstructions instructions, ZArchitecture.Modules.ISecurityCheckpoint modifyDocumentCheckPoint)
		{
			return provider.ShowRuntimeOptionsUI(printTask, deliveryOptions, instructions, modifyDocumentCheckPoint);
		}

		void IPrintTaskUIProvider.ShowWarning(string caption, string message)
		{
			provider.ShowWarning(caption, message);
		}
	}
}
