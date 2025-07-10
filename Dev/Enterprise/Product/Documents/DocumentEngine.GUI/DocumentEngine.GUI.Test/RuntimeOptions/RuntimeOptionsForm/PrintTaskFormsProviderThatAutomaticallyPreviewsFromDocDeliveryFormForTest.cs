using System;
using System.Windows.Forms;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	class PrintTaskFormsProviderThatAutomaticallyPreviewsFromDocDeliveryFormForTest : IPrintTaskUIProvider
	{
		readonly PrintTaskFormsProvider defaultUiProvider = new PrintTaskFormsProvider();

		#region IPrintTaskUIProvider Members

		public bool ShowRuntimeOptionsUI(PrintTask printTask, AllowedDeliveryOptions deliveryOptions, DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint)
		{
			throw new NotImplementedException();
		}

		public bool ShowPrintTaskDeliveryUI(PrintTaskSettings taskSettings)
		{
			throw new NotImplementedException();
		}

		public IProgressNotificationUI GetNewProgressNotificationUI(PrintTaskSettings taskSettings)
		{
			throw new NotImplementedException();
		}

		public IProgressNotificationUI GetNewProgressNotificationUI(DeliveryInstructions instructions, int totalPacks)
		{
			return ((IPrintTaskUIProvider)defaultUiProvider).GetNewProgressNotificationUI(instructions, totalPacks);
		}

		public bool ShowDocDeliveryUI(DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint)
		{
			throw new NotImplementedException();
		}

		public bool ShowDocDeliveryUI(PrintTask printTask, DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint)
		{
			using (DocDeliveryForm deliveryForm = new DocDeliveryForm(printTask, instructions, modifyDocumentCheckPoint))
			{
				DocDeliveryContact contact = null;

				if (instructions.Recipients != null || instructions.Recipients.Count > 0)
				{
					contact = instructions.Recipients[0];
				}
				else
				{
					contact = instructions.Recipients.AddNew();
				}

				contact.Name = "Unit Test";
				contact.Email = "unit.test@cargowise.com";
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact.AttachmentType = OrgConstants.AttachmentType.PDF;

				deliveryForm.Show();
				deliveryForm.PreviewButton.PerformClick();

				Assertion.AssertEquals("clonedInstructions.DocPack.IsDisposed", false, instructions.DocPack.IsDisposed);
				Assertion.AssertEquals("Application.OpenForms.FindAll<XLSPreviewForm>().Count", 1, Application.OpenForms.FindAll<XLSPreviewForm>().Count);
				XLSPreviewForm previewForm = Application.OpenForms.FindAll<XLSPreviewForm>()[0];
				previewForm.Close();
				Assertion.AssertEquals("clonedInstructions.DocPack.IsDisposed", false, instructions.DocPack.IsDisposed);

				deliveryForm.Close();
			}

			LastDeliveryInstructions = instructions;

			return true;
		}

		internal DeliveryInstructions LastDeliveryInstructions { get; private set; }

		public void ShowPrinterSelectionUI(DeliveryInstructions deliveryInstructions)
		{
			throw new NotImplementedException();
		}

		public void ShowPreview(System.IO.Stream xlsStream, DeliveryMethods.DeliveryInfo[] deliveryInfos, IDeliverCapableForm parentForm)
		{
			((IPrintTaskUIProvider)defaultUiProvider).ShowPreview(xlsStream, deliveryInfos, parentForm);
		}

		public bool ShowErrors(Report report)
		{
			throw new NotImplementedException();
		}

		public void ShowWarning(string caption, string message)
		{
		}

		#endregion
	}
}
