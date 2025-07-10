using System;
using System.Windows.Forms;
using Enterprise.DocumentEngine.GUI.Visualisation;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.DocumentDelivery
{
	public partial class PrintTaskDeliveryButtonsControl : ZUserControl
	{
		public PrintTaskDeliveryButtonsControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			UpdateControlsFromInstructions();
		}

		#region Related Objects

		public PrintTaskSettings TaskSettings
		{
			get { return (PrintTaskSettings)CurrentDataItem; }
		}

		#endregion

		#region Button Clicks

		#region Preview

		public void PreviewButton_Click(object sender, EventArgs e)
		{
			DialogResult result = DialogResult.OK;

			if (!TaskSettings.AllowPreview)
			{
				Globals.Message.ShowError(Res.GetString("9bbffabd-0015-44df-8420-0e06d49c55a5", "Cannot preview this many documents. Please print to view them."), Res.GetString("8a223c90-be16-4298-973e-9088e702073b", "Preview not allowed"));
				result = DialogResult.Cancel;
			}
			else if (TaskSettings.MultipleDocumentPacks)
			{
				result = Globals.Message.Show(Res.GetString("43d21aee-9c9f-448e-b16d-f08f53d7701f", "There is more than one document pack to preview. Would you like to continue?"), Res.GetString("a5203cc7-83c4-4f8a-bfc0-6221d67c7c9e", "Multiple Documents to Preview"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
			}

			if (result != DialogResult.Cancel)
			{
				TaskSettings.PrintTask.Preview(TaskSettings);
				CloseButton.Text = Res.GetString("8b357fa6-9f86-49c0-8945-b3100c8cb5d3", "Close");
			}
		}

		#endregion

		#region Deliver

		void DeliverButton_Click(object sender, EventArgs e)
		{
			Deliver();
		}

		public void Deliver()
		{
			bool success = ValidateForDelivery();
			if (success)
			{
				ParentForm.DialogResult = DialogResult.OK;
			}
		}

		#endregion

		#region Visualisation

		public void VisualiseButton_Click(object sender, EventArgs e)
		{
			string errorMessage = GetErrorMessageForNonVisualisableMenuItem();
			if (string.IsNullOrEmpty(errorMessage))
			{
				VisualiseDocument();
			}
			else
			{
				Globals.Message.Show(errorMessage);
			}
		}

		void VisualiseDocument()
		{
			using (var form = new VisualiserForm())
			{
				var visualizerManager = new DocPackVisualiserManager(TaskSettings.SingleDocPackInstructions.DocPack, TaskSettings.SingleDocPackInstructions.DeliverablesToBePrinted);
				var controller = new VisualizerViewController(visualizerManager, form);

				ZFormModaliser.ShowDialogAndDispose(form);
			}
		}

		string GetErrorMessageForNonVisualisableMenuItem()
		{
			string result = "";
			if (TaskSettings.MultipleDocumentPacks || TaskSettings.SingleDocPackInstructions == null)
			{
				result = Res.GetString("699686f2-96ac-4ca8-bafa-381419f0b66f", "Only available for single document pack.");
			}
			else if (!(TaskSettings.SingleDocPackInstructions.DocPack.StmMenuCommand is DocumentCommand))
			{
				result = Res.GetString("9bbefc11-6008-4257-84a2-58f7eb3a72ea", "Only documents can have overriding data.");
			}
			else if (!DocumentsDataRegistry.Instance.AllowDocumentsToBeModified.Value)
			{
				result = Res.GetString("d221aafb-d8cb-4c75-931c-2700e642ed6c", "This feature allows users to modify the values on this document. You do not currently have access to use this feature because this feature is disabled in your system. To enable it, please ask your system administrator to turn on the Registry item 'Documents -> Allow Documents To Be Modified'. You will need to log out and log back into {0} once this registry value has been changed.", Core.Constants.ProductName);
			}
			else if (!TaskSettings.SingleDocPackInstructions.DocPack.StmMenuCommand.SU_SupportsVisualisation)
			{
				result = Res.GetString("8befb372-21dd-4e82-8f65-e9f3c0709e98", "This feature allows users to modify the values on the select document. However, you cannot modify this particular document, because this document does not, by design, allow any user to modify it for auditing and data integrity purposes.");
			}
			else if (!TaskSettings.SingleDocPackInstructions.DocPack.StmMenuCommand.SU_IsModifiable)
			{
				result = Res.GetString("aaecb460-bdd0-4360-972a-4b6644ad4301", "This feature allows users to modify the values on the select document. However, you cannot modify this particular document, because your system admin has restricted modifying this document.");
			}
			else
			{
				if (TaskSettings.ModifyDocumentCheckPoint != null)
				{
					if (!TaskSettings.ModifyDocumentCheckPoint.IsAllowed)
					{
						result = TaskSettings.ModifyDocumentCheckPoint.ErrorMessageForNotAllowed;
					}
				}
			}

			return result;
		}

		#endregion

		#endregion

		#region Validations

		bool ValidateForDelivery()
		{
			bool success = true;

			foreach (DeliveryInstructions instructions in TaskSettings.DocPacksDeliveryInstructions)
			{
				instructions.PrinterDelivery.PrintQueuePK = TaskSettings.PrinterDelivery.PrintQueuePK;
				instructions.RunPreSaveValidation();
				if (instructions.HasErrors)
				{
					success = false;
				}
			}
			if (!success)
			{
				ShowErrorsEvent(this);
			}
			else
			{
				bool hasDocuments = false;
				foreach (DeliveryInstructions instructions in TaskSettings.DocPacksDeliveryInstructions)
				{
					if (instructions.Recipients != null && instructions.Recipients.Count == 0)
					{
						Globals.Message.ShowError(Res.GetString("3d0f4561-1485-4832-9d16-68eff0bd9441", "You have not specified any recipients for {0}.", instructions.DocumentPackTitle));
						success = false;
						break;
					}
					if (instructions.DeliverablesToBePrinted != null && instructions.DeliverablesToBePrinted.HasIncludedDocuments)
					{
						hasDocuments = true;
					}
				}
				if (!hasDocuments)
				{
					Globals.Message.ShowError(Res.GetString("82ee4256-2e62-44ac-9306-aadc7a1740c2", "There are no documents to deliver for."));
					success = false;
				}
			}

			return success;
		}

		#endregion

		public event ShowErrorsEventHandler ShowErrorsEvent;

		public delegate void ShowErrorsEventHandler(object sender);

		#region Implementation

		void UpdateControlsFromInstructions()
		{
			if (TaskSettings != null && TaskSettings.DeliveryOptions == AllowedDeliveryOptions.AllExceptPreview)
			{
				PreviewButton.Visible = false;
			}
		}

		#endregion
	}
}
