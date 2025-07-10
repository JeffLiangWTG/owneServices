using System;
using System.IO;
using System.Windows.Forms;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.DocumentEngine.GUI.DocumentDelivery;
using Enterprise.DocumentEngine.GUI.RuntimeOptions;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.GUI
{
	public class PrintTaskFormsProvider : IPrintTaskUIProvider
	{
		bool IPrintTaskUIProvider.ShowRuntimeOptionsUI(PrintTask printTask, AllowedDeliveryOptions deliveryOptions, DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint)
		{
			bool result = false;

			foreach (DocumentPack documentPack in printTask.GetDocumentPacks())
			{
				Report report = documentPack.GetFirstReport();
				if (report != null)
				{
					if (report.Parent.StmMenuCommand != null)
					{
						RuntimeOptionsForm form = new RuntimeOptionsForm(printTask, report, deliveryOptions, instructions, modifyDocumentCheckPoint);
						try
						{
							if (form.NeedsToShow || printTask.IsReportPrintSet)
							{
								form.Show();
								result = true;
							}
							else
							{
								form.Dispose();
							}
						}
						catch
						{
							form.Dispose();
							throw;
						}
					}
				}
			}

			return result;
		}

		bool IPrintTaskUIProvider.ShowPrintTaskDeliveryUI(PrintTaskSettings taskSettings)
		{
			return ZFormModaliser.ShowDialogAndDispose(new PrintTaskDeliveryForm(taskSettings)) == DialogResult.OK;
		}

		IProgressNotificationUI IPrintTaskUIProvider.GetNewProgressNotificationUI(PrintTaskSettings taskSettings)
		{
#if DEBUG
			if (Globals.IsTest && !Enterprise.DocumentEngine.Testing.PrintTaskUIProviderForTestingSuppressor.Enabled)
			{
				return new Enterprise.DocumentEngine.GUI.Testing.DocUserNotificationForTesting(taskSettings);
			}
			else
#endif
			{
				return new DocUserNotification(taskSettings);
			}
		}

		IProgressNotificationUI IPrintTaskUIProvider.GetNewProgressNotificationUI(DeliveryInstructions instructions, int totalPacks)
		{
#if DEBUG
			if (Globals.IsTest && !Enterprise.DocumentEngine.Testing.PrintTaskUIProviderForTestingSuppressor.Enabled)
			{
				return new Enterprise.DocumentEngine.GUI.Testing.DocUserNotificationForTesting(instructions);
			}
			else
#endif
			{
				return new DocUserNotification(instructions, totalPacks);
			}
		}

		bool IPrintTaskUIProvider.ShowDocDeliveryUI(PrintTask printTask, DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint)
		{
			return ZFormModaliser.ShowDialogAndDispose(new DocDeliveryForm(printTask, instructions, modifyDocumentCheckPoint)) == DialogResult.OK;
		}

		void IPrintTaskUIProvider.ShowPrinterSelectionUI(DeliveryInstructions deliveryInstructions)
		{
			ZFormModaliser.ShowDialogAndDispose(new PrinterSelectionForm(deliveryInstructions));
		}

		void IPrintTaskUIProvider.ShowPreview(Stream xlsStream, DeliveryInfo[] deliveryInfos, IDeliverCapableForm parentForm)
		{
			var previewForm = new XLSPreviewForm(xlsStream, deliveryInfos, parentForm);
			try
			{
				ZController.ShowModelessFormCore(previewForm);
			}
			catch (ObjectDisposedException)
			{
				//It is possible for the following to happen:
				//1) EnterpriseFormLookStrategy restores form layout/size/position
				//2) In the process, a Splitter's SplitPosition setter is called
				//3) Splitter.set_SplitPosition calls Application.DoEvents(), allowing for re-entrant windows message processing
				//4) If WM_QUIT was posed to the application, the form will be disposed in response to it
				//5) When Show() logic resumes, attempts to focus on XLSPreviewForm fail with ObjectDisposedException().
				//The warning below will only have a chance to show if the application is not shutting down - it will only appear in the case of an actual problem in our logic.
				//See Issue 00883381
				Globals.Message.ShowWarning(Res.GetString("c52a1cf1-e8ac-4451-9bdf-a2781f6e096b", "The preview form was unexpectedly closed, please try this action again."));
			}
		}

		bool IPrintTaskUIProvider.ShowErrors(Report report)
		{
			bool result = false;
			using (RuntimeOptionsForm form = new RuntimeOptionsForm(report))
			{
				result = ZFormModaliser.ShowDialogAndDispose(form) != DialogResult.Cancel;
			}
			return result;
		}

		public void ShowWarning(string caption, string message)
		{
			Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}
	}
}
