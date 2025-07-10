using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.DocumentEngine.GUI.DocumentDelivery;
using Enterprise.DocumentEngine.GUI.Scheduler;
using Enterprise.DocumentEngine.GUI.Visualisation;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.GUI
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class DocDeliveryForm : ZChildForm, IDeliverCapableForm, IDocumentDeliveryView
	{
		public DocDeliveryForm(DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint)
			: this(null, instructions, modifyDocumentCheckPoint)
		{
		}

		public DocDeliveryForm(PrintTask printTask, DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint)
			: this(printTask, instructions)
		{
			this.ModifyDocumentCheckPoint = modifyDocumentCheckPoint;

			if (instructions.DeliveryOptions == AllowedDeliveryOptions.OverridePrintDetails)
			{
				HideControlsExceptDocumentToSendTab();
			}
		}

		DocDeliveryForm(PrintTask printTask, DeliveryInstructions deliveryInstructions)
			: base(deliveryInstructions)
		{
			Argument.NotNull(deliveryInstructions, "deliveryInstructions");
			InitializeComponent();

			ChangeFormBorderStyleForRendering();

			this.deliveryInstructions = deliveryInstructions;
			this.printTask = printTask;

			deliveryInstructions.IsProceedingToDelivery = true;
			UpdateControlsFromInstructions();

			VisualiseButton.Enabled = deliveryInstructions.AllowModify;

			ShowInTaskbar = true;

			var shouldShowEDocsTab = !deliveryInstructions.MultipleDocumentPacks && deliveryInstructions.EDocsToBeDelivered.Any();
			if (printTask != null)
			{
				PreviewRequested += new PrintTask.PreviewRequestedEventHandler(printTask.Form_PreviewRequested);
				shouldShowEDocsTab = shouldShowEDocsTab && !printTask.IsReportPrintSet;
			}
			IncludedEDocsTabPage.TabVisible = shouldShowEDocsTab;

			_ = new DocumentDeliveryPresenter(this, printTask, deliveryInstructions);

			var addressOverrideColumnStyleInfo = new NonPersistentAddressOverrideColumnStyleInfo<DocDeliveryContact>
			{
				CaptionResourceString = Res.GetData("DocDeliveryForm|{9B9B9AF8-1F46-4683-9B5F-2B4F03E97952}", "E-Mail Address / Fax"),
				GetCopyRecipients = docDeliveryContact => docDeliveryContact.EmailToRecipients,
				ColumnName = "DeliveryAddress",
				EmailAddressPropertyName = "EmailAddress",
				FieldTypeColumnName = "DeliveryMethodFieldType",
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(230)
			};
			RecipientsGrid.ColumnStyles.Add(addressOverrideColumnStyleInfo);

			var emailCarbonCopyRecipientsColumnStyleInfo = new NonPersistentCopyRecipientsColumnStyleInfo<DocDeliveryContact>()
			{
				CaptionResourceString = Res.GetData("DocDeliveryForm|47678178-ED20-4F15-9A9D-BF3244AEDC1A", "Email CC"),
				ColumnName = "EmailCarbonCopyRecipientsAsString",
				EmailAddressPropertyName = "EmailAddress",
				GetCopyRecipients = docDeliveryContact => docDeliveryContact.EmailCarbonCopyRecipients,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
			};
			RecipientsGrid.ColumnStyles.Add(emailCarbonCopyRecipientsColumnStyleInfo);

			var emailBlindCarbonCopyRecipientsColumnStyleInfo = new NonPersistentCopyRecipientsColumnStyleInfo<DocDeliveryContact>()
			{
				CaptionResourceString = Res.GetData("DocDeliveryForm|275195F5-9654-450F-8837-5E99235C3C72", "Email BCC"),
				ColumnName = "EmailBlindCarbonCopyRecipientsAsString",
				EmailAddressPropertyName = "EmailAddress",
				GetCopyRecipients = docDeliveryContact => docDeliveryContact.EmailBlindCarbonCopyRecipients,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			};
			RecipientsGrid.ColumnStyles.Add(emailBlindCarbonCopyRecipientsColumnStyleInfo);

			AddEmailSubjectMacroColumn();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
		}

		void AddEmailSubjectMacroColumn()
		{
			var basicRootTypes = new[]
			{
				typeof(OrgHeader),
				typeof(OrgContact),
				typeof(OrgDocument),
				typeof(StmMenuItem),
				typeof(DocDeliveryContact),
				ObjectFactory.GetType<Forwarding.IForwardingConsol>(),
				ObjectFactory.GetType<Forwarding.IForwardingShipment>(),
				ObjectFactory.GetType<IARInvoice>(),
				ObjectFactory.GetType<IWorkItem>(),
				ObjectFactory.GetType<IDtbBooking>(),
				GenericWrapperLoader.GetFromDataContext(Core.Constants.DataContext.GenericFreightJob).GetWrapperType()
			};

			var emailSubjectColumnInfo = new ZMacrosFindBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("{56A83885-C0D7-4525-9920-BD76F796E24F}", "Email Subject"),
				ColumnName = nameof(DocDeliveryContact.EmailSubjectMacro),
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
				IsUsedForExpressions = true,
				AllowMultipleMacroses = true,
				UsePredefinedRoots = true,
				RootTypes = basicRootTypes,
				Roots = new BusinessObject[] { deliveryInstructions }
			};
			RecipientsGrid.ColumnStyles.Add(emailSubjectColumnInfo);

			RecipientsGrid.AfterBind += (s, a) =>
			{
				SetRootsForMacroColumn();
				RecipientsGrid.ListManager.CurrentChanged += (sender, args) => { SetRootsForMacroColumn(); };
			};

			void SetRootsForMacroColumn()
			{
				if (RecipientsGrid.ListManager.GetCurrent() is DocDeliveryContact contact)
				{
					emailSubjectColumnInfo.Roots = deliveryInstructions.GetRelatedBusinessObjectsForEmailSubject(contact);
					var typeList = new List<Type>(basicRootTypes);
					foreach (var rootType in emailSubjectColumnInfo.Roots.Where(r => r != null).Select(r => r.GetType()))
					{
						if (!typeList.Exists(t => rootType == t || rootType.IsSubclassOf(t)))
						{
							typeList.Add(rootType);
						}
					}
					emailSubjectColumnInfo.RootTypes = typeList.ToArray();
				}
			}
		}

		void HideControlsExceptDocumentToSendTab()
		{
			this.MainPage.TabVisible = false;
			this.IncludedEDocsTabPage.TabVisible = false;
			this.CoverNotePage.TabVisible = false;
			this.LanguageZDropEdit.Visible = false;
			this.PrintAsDraftCheckbox.Visible = false;
			this.VisualiseButton.Visible = false;
			this.PreviewButton.Visible = false;
			this.saveAsButton.Visible = false;
		}

		readonly PrintTask printTask;
		readonly DeliveryInstructions deliveryInstructions;
		internal ReportScheduleTask scheduleTask;

#if DEBUG
		internal bool changeCell;
#endif

		#region Form Layout

		public override string FormHeading
		{
			get { return FormCaption; }
		}

		bool IsDocument => deliveryInstructions.IsDocument;

		bool IsReport => deliveryInstructions.DocPack.StmMenuCommand is ReportCommand;

		bool IsForm => deliveryInstructions.IsDeliveringFormDocument;

		bool AllowPreviewIfReport => Env.Security.PreviewReportButton.IsAllowed && IsReport;

		bool AllowPreviewIfDocument => deliveryInstructions.AllowPreviewIfDocument;

		bool AllowSaveAsIfReport => IsReport && Env.Security.SaveAsReportButton.IsAllowed;

		bool AllowSaveAsIfDocument => IsDocument && Env.Security.SaveAsDocumentButton.IsAllowed;

		bool AllowSaveAsIfForms => IsForm && Env.Security.SaveAsDocumentButton.IsAllowed;

		void UpdateControlsFromInstructions()
		{
			MultiDocPackGroupbox.Visible = deliveryInstructions.MultipleDocumentPacks;
			IndividualDocPackGroupBox.Visible = !deliveryInstructions.MultipleDocumentPacks;

			if (IsForm)
			{
				CoverNotePage.TabVisible = false;
			}

			if (IsForm || AllowPreviewIfReport || AllowPreviewIfDocument)
			{
				PreviewButton.Visible = true;
			}

			if (deliveryInstructions.DeliveryOptions == AllowedDeliveryOptions.AllExceptPreview)
			{
				PreviewButton.Visible = false;
			}

			saveAsButton.Visible = AllowSaveAsIfForms || (!deliveryInstructions.MultipleDocumentPacks && (AllowSaveAsIfReport || AllowSaveAsIfDocument));

			if (!IsReport)
			{
				BackgroundDeliveryCheckBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|9132786f-e0c9-4202-835d-0810e1020c26", "Background Delivery");
			}

			if (deliveryInstructions.MultipleDocumentPacks)
			{
				MainTabControl.TabPages.Remove(DocumentsTabPage);
				DocumentsTabPage.Dispose();
			}
		}

		void NumMultiDocPacksLabel_TextChanged(object sender, EventArgs e)
		{
			MultiDocPacksLabel.Text = Res.GetString("539dd571-6e30-4a4c-a398-2edc0c84c7f5", "There are {0} document packs to deliver.", NumMultiDocPacksLabel.Text);
		}

		#endregion

		#region Delivery

		void DeliverButton_Click(object sender, EventArgs e)
		{
			Deliver(deliveryInstructions);
		}

		public void Deliver(DeliveryInstructions instructions1)
		{
			if (!DeliverButton.Focused)
			{
				DeliverButton.Focus();
			}
			var instructions = instructions1 ?? deliveryInstructions;
			bool success = ValidateForDelivery(instructions);
			if (success)
			{
				if (IsReport)
				{
					if (instructions.BackgroundDelivery)
					{
						if (Env.Security.ScheduledTaskNew.IsAllowed)
						{
							CreateSchedule(false);

							if (printTask != null)
							{
								printTask.SavePrinterDeliveryDefaults(instructions);
							}
						}
						else
						{
							Env.Security.ScheduledTaskNew.ShowError();
						}
					}
					else
					{
						if (Env.Registry.DeliverReportsInBackground)
						{
							CreateSchedule(true);

							if (printTask != null)
							{
								printTask.SavePrinterDeliveryDefaults(instructions);
							}
						}
						else
						{
							DialogResult = DialogResult.OK;
						}
					}
				}
				else
				{
					DialogResult = DialogResult.OK;
				}
			}
		}

		bool ValidateForDelivery(DeliveryInstructions instructions)
		{
			bool success = true;

			try
			{
				instructions.IgnoreValidationSuspended = true;
				instructions.RunPreSaveValidation();
				if (instructions.HasErrors)
				{
					ShowErrorsDialog();
					success = false;
				}
				else
				{
					if (!instructions.MultipleDocumentPacks)
					{
						if ((instructions.Recipients != null) && (instructions.Recipients.Count == 0))
						{
							Globals.Message.ShowError(Res.GetString("5e3847d1-2f08-46b4-bce4-cacb5ae2cf43", "You have not specified any recipients."));
							success = false;
						}
						else if (instructions.DeliverablesToBePrinted == null || instructions.DeliverablesToBePrinted.Count == 0 || !instructions.DeliverablesToBePrinted.HasIncludedDocuments)
						{
							Globals.Message.ShowError(Res.GetString("125b8026-0f31-40ff-8f74-e193886fbe50", "There are no documents to deliver."));
							success = false;
						}
					}

					if (success && instructions.HasPrintedDocuments && instructions.PrinterDelivery != null && instructions.PrinterDelivery.NumberOfCopies > 10)
					{
						success = Globals.Message.Show(
							Res.GetString("3b1c6f56-471c-418d-8653-8e9bdc87399f", "You are going to print {0} copies of each document. Are you sure this is correct?", instructions.PrinterDelivery.NumberOfCopies),
							Res.GetString("9aaa3e61-85d5-4ddd-b29d-cdc3983724ed", "Printing large amount of copies"),
							MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK;
					}

					if (success && instructions.Recipients != null)
					{
						var contactEmails = instructions.Recipients.Cast<DocDeliveryContact>().SelectMany(GetAllEmails);
						var presendChecker = ObjectFactory.Get<IEmailPreSendChecker>();
						success = presendChecker.PromptUserIfSendingToNdrRecipients(contactEmails);
					}
				}
			}
			finally
			{
				instructions.IgnoreValidationSuspended = false;
			}
			return success;
		}

		static IEnumerable<ZString> GetAllEmails(DocDeliveryContact deliveryContact)
		{
			foreach (NonPersistentCopyRecipient recipient in deliveryContact.EmailToRecipients)
			{
				yield return recipient.EmailAddress;
			}

			foreach (NonPersistentCopyRecipient recipient in deliveryContact.EmailCarbonCopyRecipients)
			{
				yield return recipient.EmailAddress;
			}

			foreach (NonPersistentCopyRecipient recipient in deliveryContact.EmailBlindCarbonCopyRecipients)
			{
				yield return recipient.EmailAddress;
			}
		}

		#endregion

		#region Preview

		void PreviewButton_Click(object sender, EventArgs e)
		{
			var result = DialogResult.OK;

			try
			{
				if (!deliveryInstructions.AllowPreview)
				{
					Globals.Message.ShowError(Res.GetString("9bbffabd-0015-44df-8420-0e06d49c55a5", "Cannot preview this many documents. Please print to view them."), Res.GetString("b4ffa4a2-4671-4dac-a48c-878ad0232f17", "Preview not allowed"));
					result = DialogResult.Cancel;
				}
				else if (deliveryInstructions.MultipleDocumentPacks)
				{
					result = Globals.Message.Show(Res.GetString("6f36d76c-a915-436e-a3ea-27f95e85e605", "There is more than one document to preview. Would you like to continue?"), Res.GetString("a005ecd3-e11f-4598-a4a5-812a6019c7c7", "Multiple Documents to Preview"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
				}
				else if (deliveryInstructions.DeliverablesToBePrinted == null || deliveryInstructions.DeliverablesToBePrinted.Count == 0 || !deliveryInstructions.DeliverablesToBePrinted.HasIncludedDocuments)
				{
					Globals.Message.ShowError(Res.GetString("d64a596d-68b2-4c2f-81b0-72d1c543791b", "There are no documents to preview."));
					result = DialogResult.Cancel;
				}
				else if (!deliveryInstructions.DeliverablesToBePrinted.HasIncludedPreviewableDocuments)
				{
					Globals.Message.ShowInformation(Res.GetString("1efe99af-c7ab-4548-b8bf-fb2ce4b38bc8", "As eDocs are unable to be printed, no preview is available."));
					result = DialogResult.Cancel;
				}
				else if (deliveryInstructions.SpecifiedPageRangesTextInfo.HasErrors())
				{
					Globals.Message.ShowError(deliveryInstructions.SpecifiedPageRangesError);
					result = DialogResult.Cancel;
				}

				if (result != DialogResult.Cancel)
				{
					deliveryInstructions.ParentForm = this;
					OnPreviewRequested(deliveryInstructions);
					CloseButton.Text = Res.GetString("e577a4dc-0720-4418-8a9c-946b457f3df5", "Close");
				}
			}
			catch (ExternalStorageException ex)
			{
				ex.ReportExceptionForDeveloper();

				var caption = Res.GetString("3B92AF6D-BFA7-47E1-8118-76A7ED14E23E", "Failed to access eDocs");
				Globals.Message.ShowError(ex.UnableToAccessStorageFriendlyMessage, caption);
			}
		}

		#region Print Preview Event

		public event PrintTask.PreviewRequestedEventHandler PreviewRequested;

		public void OnPreviewRequested(DeliveryInstructions instructions)
		{
			if (PreviewRequested != null)
			{
				using (new MenuClickPendingTracker())
				{
					PreviewRequested(this, instructions);
				}
			}
		}

		#endregion

		#endregion

		#region Close

		void CloseButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		#endregion

		#region Visualisation

		void VisualiseButton_Click(object sender, EventArgs e)
		{
			string errorMessage = GetErrorMessageForNonVisualisableMenuItem();
			if (string.IsNullOrEmpty(errorMessage))
			{
				VisualiseDocument();
			}
			else
			{
				Globals.Message.ShowError(errorMessage, Res.GetString("A2A29573-35D0-4BD8-B44B-1DAE059466FD", "Access Denied: Modify"));
			}
		}

#if DEBUG
		internal
#endif
		void VisualiseDocument()
		{
			DocPackVisualiserManager visualiserManager = new DocPackVisualiserManager(deliveryInstructions.DocPack, deliveryInstructions.DeliverablesToBePrinted);
			visualiserManager.RebuildDocPackIfLanguageChanged(deliveryInstructions);
			ShowVisualiserFormCore(visualiserManager);
		}

		protected virtual void ShowVisualiserFormCore(DocPackVisualiserManager visualizerManager)
		{
			using (var form = new VisualiserForm() { Owner = this })
			{
				var controller = new VisualizerViewController(visualizerManager, form);
				if (controller.ShouldShowVisualizerView())
				{
					ZFormModaliser.ShowDialogWithoutDispose(form);
				}
			}
		}

		internal string GetErrorMessageForNonVisualisableMenuItem()
		{
			string result = "";
			if (!(deliveryInstructions.DocPack.StmMenuCommand is DocumentCommand))
			{
				result = Res.GetString("c503cd1d-f5fb-4b50-a159-7ac287ee4710", "Only documents can have overriding data.");
			}
			else if (!DocumentsDataRegistry.Instance.AllowDocumentsToBeModified.Value)
			{
				result = Res.GetString("5552ca3a-e49c-484a-a03c-2d4cd0f2aa83", "This feature allows users to modify the values on this document. You do not currently have access to use this feature because this feature is disabled in your system. To enable it, please ask your system administrator to turn on the Registry item 'Documents -> Allow Documents To Be Modified'. You will need to log out and log back into {0} once this registry value has been changed.", Core.Constants.ProductName);
			}
			else if (!deliveryInstructions.DocPack.StmMenuCommand.SU_SupportsVisualisation)
			{
				result = Res.GetString("7d02e716-586a-44c7-b1b6-2033f387a44b", "This feature allows users to modify the values on the select document. However, you cannot modify this particular document, because this document does not, by design, allow any user to modify it for auditing and data integrity purposes.");
			}
			else if (!deliveryInstructions.DocPack.StmMenuCommand.SU_IsModifiable)
			{
				result = Res.GetString("a154cd26-631a-4ac9-8122-6f12f0bba50f", "This feature allows users to modify the values on the select document. However, you cannot modify this particular document, because your system admin has restricted modifying this document.");
			}
			else
			{
				if (!ModifyDocumentCheckPoint.IsAllowed)
				{
					result = ModifyDocumentCheckPoint.ErrorMessageForNotAllowed;
				}
			}

			return result;
		}

		readonly ISecurityCheckpoint ModifyDocumentCheckPoint;

		#endregion

		#region Scheduling

		void CreateSchedule(bool instantDelivery)
		{
			RecipientsGrid.ListManager.EndCurrentEdit();

			if (deliveryInstructions.Recipients.Count == 0)
			{
				Globals.Message.ShowError(Res.GetString("5e3847d1-2f08-46b4-bce4-cacb5ae2cf43", "You have not specified any recipients."));
			}
			else
			{
				deliveryInstructions.RunPreSaveValidation();
				if (deliveryInstructions.HasErrors)
				{
					ShowErrorsDialog();
				}
				else
				{
					DialogResult = DialogResult.Cancel;

					if (scheduleTask == null)
					{
						scheduleTask = deliveryInstructions.Factory.New<ReportScheduleTask>();
					}
					scheduleTask.SetScheduleFromDeliveryInstructions(deliveryInstructions, instantDelivery);

					if (instantDelivery)
					{
						scheduleTask.Factory.Save();
					}
					else
					{
						if (ZFormModaliser.ShowDialogAndDispose(new ScheduleDatePickerForm(scheduleTask)) == DialogResult.Cancel)
						{
							DialogResult = DialogResult.None;
						}
					}
				}
			}
		}

		#endregion

		#region Dispose

		public bool IsFormClosed
		{
			get { return fIsFormClosed; }
			set { fIsFormClosed = value; }
		}

		bool fIsFormClosed;

		protected override void Dispose(bool disposing)
		{
			IsFormClosed = true;

			if (printTask != null)
			{
				PreviewRequested -= new PrintTask.PreviewRequestedEventHandler(printTask.Form_PreviewRequested);
			}

			base.Dispose(disposing);
		}

		#endregion

		void IDocumentDeliveryView.HideLanguageSelectionDropDown()
		{
			LanguageZDropEdit.Hide();
			hideLanguageZDropEdit = true;
			ResizeBottomPanel();
		}
		bool hideLanguageZDropEdit;

		void IDocumentDeliveryView.HidePageRangesPanel()
		{
			pageRangesPanel.Hide();
			hidePageRangesPanel = true;
			ResizeBottomPanel();
		}
		bool hidePageRangesPanel;

		void IDocumentDeliveryView.HideBackgroundDeliveryCheckBox()
		{
			BackgroundDeliveryCheckBox.Hide();
		}

		void ResizeBottomPanel()
		{
			if (hideLanguageZDropEdit && hidePageRangesPanel)
			{
				var height = Math.Max(LanguageZDropEdit.Size.Height, pageRangesPanel.Height);
				bottomPanel.Size = ControlDpiScalingHelper.NewScaledSize(bottomPanel.Size.Width, bottomPanel.Size.Height - height);
			}
		}

		void PageRangesSpecifiedCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			PageRangesTextBox.Enabled = PageRangesSpecifiedCheckBox.Checked;
		}

		void ShowOnlyPrintersUserCanPrintToCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			var showOnlyPrintersUserCanPrintTo = deliveryInstructions.PrinterDelivery.ShowOnlyPrintersUserCanPrintTo;
			deliveryInstructions.DocumentsToBeDelivered.OfType<IDeliverable>().ForEach(deliverable => deliverable.PrinterDetails.ShowOnlyPrintersUserCanPrintTo = showOnlyPrintersUserCanPrintTo);
		}

		event EventHandler IDocumentDeliveryView.SaveAsButtonClicked
		{
			add { saveAsButton.Click += value; }
			remove { saveAsButton.Click -= value; }
		}

		SaveAsFileInfo IDocumentDeliveryView.GetSaveAsFileName()
		{
			return new DocumentSaveHelper().GetSaveAsFileInfo(printTask, deliveryInstructions.DocumentPackTitle);
		}

		event FormClosedEventHandler IDocumentDeliveryView.ViewClosed
		{
			add { this.FormClosed += value; }
			remove { this.FormClosed -= value; }
		}
	}

	#region ZGridThatNotNeedNotifyHasChanges class

	internal class ZGridThatNotNeedNotifyHasChanges : ZGrid
	{
		protected override void NotifyColumnEditStart() { }
	}

	#endregion
}
