using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngine.Testing
{
	public sealed class PrintTaskForcePreviewTestingUIProvider : IPrintTaskUIProvider
	{
		public PrintTask LastPrintTask;
		public string LastErrors;
		public string LastSheetRendered;

		IProgressNotificationUI IPrintTaskUIProvider.GetNewProgressNotificationUI(DeliveryInstructions instructions, int totalPacks)
		{
			return new DummyProgressNotificationUI();
		}

		IProgressNotificationUI IPrintTaskUIProvider.GetNewProgressNotificationUI(PrintTaskSettings taskSettings)
		{
			throw new System.NotImplementedException();
		}

		bool IPrintTaskUIProvider.ShowDocDeliveryUI(PrintTask printTask, DeliveryInstructions instructions, Enterprise.ZArchitecture.Modules.ISecurityCheckpoint modifyDocumentCheckPoint)
		{
			instructions.Destination = DeliveryInstructionDestination.Preview;
			LastPrintTask = printTask;
			return true;
		}

		bool IPrintTaskUIProvider.ShowErrors(Report report)
		{
			LastErrors = report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false);
			return false;
		}

		void IPrintTaskUIProvider.ShowPreview(System.IO.Stream xlsStream, Enterprise.DocumentEngine.DeliveryMethods.DeliveryInfo[] deliveryInfos, Enterprise.DocumentEngine.DocumentDelivery.IDeliverCapableForm parentForm)
		{
			if (xlsStream != null)
			{
				using (ExcelInterface excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(xlsStream);
					ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
					LastSheetRendered = excelInterface.WorkSheets[0].ToString();
				}
				xlsStream.Seek(0, System.IO.SeekOrigin.Begin);
			}
		}

		bool IPrintTaskUIProvider.ShowPrintTaskDeliveryUI(PrintTaskSettings taskSettings)
		{
			throw new System.NotImplementedException();
		}

		void IPrintTaskUIProvider.ShowPrinterSelectionUI(DeliveryInstructions deliveryInstructions)
		{
			throw new System.NotImplementedException();
		}

		bool IPrintTaskUIProvider.ShowRuntimeOptionsUI(PrintTask printTask, AllowedDeliveryOptions deliveryOptions, DeliveryInstructions instructions, Enterprise.ZArchitecture.Modules.ISecurityCheckpoint modifyDocumentCheckPoint)
		{
			throw new System.NotImplementedException();
		}

		void IPrintTaskUIProvider.ShowWarning(string caption, string message)
		{
			throw new System.NotImplementedException();
		}
	}
}
