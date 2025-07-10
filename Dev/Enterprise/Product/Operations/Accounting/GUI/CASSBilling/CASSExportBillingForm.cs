using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.DataTransfer;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI
{
	public partial class CASSExportBillingForm : ZForm, IButtonApplyTextOverride, IButtonPostTextOverride
	{
		public CASSExportBillingForm()
		{
		}

		public CASSExportBillingForm(CASSBilling cASSBillingBizo)
			: base(cASSBillingBizo)
		{
			CASSBusinessEntity = cASSBillingBizo;
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtons);
		}

		readonly CASSBilling CASSBusinessEntity;
		bool promptForAdjustmentFilePrint;

		public void ShowError(string message, string caption)
		{
			Globals.Message.ShowError(message, caption);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			MenuItem menuItemRefresh = ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("CASSExportBillingForm|ActionsMenu|Refresh", "&Refresh"), Refresh_Click); // Existing source code, could already be translated
			menuItemRefresh.Shortcut = Shortcut.F5;

			HookCollections();

			APTransactionsGrid.ContextMenu.MenuItems.Add("-");
			APTransactionsGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("APTransactionsGrid|ContextMenu|View", "&View"), new EventHandler(ViewMenuItem_Click))); // Existing source code, could already be translated

			DiscrepancyReportGrid.SetAvailability(CASSBilling.IsRejectedClaimLinesExpected, CASSBillingLine.Schema.CASSRejectedClaimValueInLocalCurrencyForDisplay);
			RejectedClaimsPanel.Visible = CASSBilling.IsRejectedClaimLinesExpected;

			if (!CASSBusinessEntity.AllowCASSCostAdjustment)
			{
				reloadCostsButton.Visible = exportButton.Visible = ExportLinesTabPage.TabVisible = ImportLinesTabPage.TabVisible = false;
			}
			else
			{
				SecurityCheckpointMessageForExportTabLabel.Text = SecurityCheckpointMessageForImportTabLabel.Text = Env.Security.APCASSCostFileModification.ErrorMessageForNotAllowed;
				SetupVisibility();
			}

			int totalsGroupBoxBottomBorderHeight = TotalsGroupBox.Size.Height - TotalsGroupBox.DisplayRectangle.Bottom;
			TotalsGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(TotalsGroupBox.Size.Width),
				ControlDpiScalingHelper.UnscaleFromCurrentDpiY(totalsGroupBoxBottomBorderHeight + SystemCostAccrualPanel.Bottom));

			Lines_CountChanged(null, new CollectionCountChangedEventArgs(true, null));

			if (AccountingConfigurationRegistry.Instance.CASSCostImportAllowedDiscrepancy.Value != AccountingConfigurationRegistry.Instance.CASSCostImportAllowedDiscrepancy.DefaultValue)
			{
				DiscrepancyReportGrid.IsWholeRowSelectedOnClick = true;
				DiscrepancyReportGrid.SetAvailability(false, CASSBillingLine.Schema.InvoiceNumber);
			}
			else
			{
				DiscrepancyReportGrid.IsWholeRowSelectedOnClick = false;
				DiscrepancyReportGrid.SetAvailability(true, CASSBillingLine.Schema.InvoiceNumber);
			}

			CASSBusinessEntity.OnStartLongTimeProcess += CASSBusinessEntity_OnStartLongTimeProcess;
			CASSBusinessEntity.OnProgressLongTimeProcess += CASSBusinessEntity_OnProgressLongTimeProcess;
			CASSBusinessEntity.OnEndLongTimeProcess += CASSBusinessEntity_OnEndLongTimeProcess;
			CASSBusinessEntity.JobCreationError += CASSBusinessEntity_JobCreationError;
			CASSBusinessEntity.OnCASSCostHeaderChanged += CASSBusinessEntity_CASSCostHeaderChanged;
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			UnhookCollections();
			CASSBusinessEntity.OnStartLongTimeProcess -= CASSBusinessEntity_OnStartLongTimeProcess;
			CASSBusinessEntity.OnProgressLongTimeProcess -= CASSBusinessEntity_OnProgressLongTimeProcess;
			CASSBusinessEntity.OnEndLongTimeProcess -= CASSBusinessEntity_OnEndLongTimeProcess;
			CASSBusinessEntity.JobCreationError -= CASSBusinessEntity_JobCreationError;
			CASSBusinessEntity.OnCASSCostHeaderChanged -= CASSBusinessEntity_CASSCostHeaderChanged;

			UnhookAPTransactions();
		}

		void HookAPTransactions()
		{
			foreach (InvoicingBase invoice in CASSBusinessEntity.APTransactions)
			{
				if (invoice.ShowError == null)
				{
					invoice.ShowError = ShowError;
				}
			}
		}

		void UnhookAPTransactions()
		{
			foreach (InvoicingBase invoice in CASSBusinessEntity.APTransactions)
			{
				invoice.ShowError = null;
			}
		}

		void HookCollections()
		{
			var exportCollectionHooked = CASSBusinessEntity.ExportLines;
			var importCollectionHooked = CASSBusinessEntity.ImportLines;
			exportCollectionHooked.CountChanged += Lines_CountChanged;
			importCollectionHooked.CountChanged += Lines_CountChanged;
			exportCollectionHooked.HasChangesChanged += CASSBusinessEntity_FileLineChanged;
			importCollectionHooked.HasChangesChanged += CASSBusinessEntity_FileLineChanged;
		}

		void UnhookCollections()
		{
			var exportCollectionHooked = CASSBusinessEntity.ExportLines;
			var importCollectionHooked = CASSBusinessEntity.ImportLines;
			exportCollectionHooked.CountChanged -= Lines_CountChanged;
			importCollectionHooked.CountChanged -= Lines_CountChanged;
			exportCollectionHooked.HasChangesChanged -= CASSBusinessEntity_FileLineChanged;
			importCollectionHooked.HasChangesChanged -= CASSBusinessEntity_FileLineChanged;
			exportCollectionHooked = null;
			importCollectionHooked = null;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return Res.GetString("3cc6da78-cbee-435b-8ced-eb399595cafb", "CASS Cost File Import"); }
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			if (CASSBusinessEntity.HasCriticalErrorsForInvoicePosting)
			{
				Globals.Message.ShowError(Res.GetString("77949A6B-83BB-4018-9FFD-B6E9E19B127D", "There are errors that need correcting before the AP invoices can be calculated. Please hover over the red stop errors for specific details."));
				return ContinueWithSave.No;
			}

			ContinueWithSave result = base.ValidateAndSave();
			if (!IsSavedSuccessfully)
			{
				return ContinueWithSave.No;
			}

			return result;
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			base.Save(factories);
			if (CASSBusinessEntity.AreAnyTransactionsPosted)
			{
				Globals.Message.ShowWarning(Res.GetString("b0e6d37d-a5d5-4297-aa6a-677719b9d85d", "Some invoices have already been posted in a previous save attempt. All unposted invoices will now be saved"));
			}

			if (!(IsSavedSuccessfully = CASSBusinessEntity.PostAPTransactions()))
			{
				Globals.Message.ShowWarning(Res.GetString("1d197c6e-b5bb-4e84-9c76-b90eea74e52a", "Some invoices have validation errors and can't be posted. See errors on the AP Invoices tab. All posted invoices have \"Is Posted\" ticked"));
			}

			ImportButton.Enabled = !CASSBusinessEntity.AreAnyTransactionsPosted;
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = ContinueWithSave.Yes;
			string question = string.Empty;

			if (CASSBusinessEntity.NeedToBeRefreshed)
			{
				question = Res.GetString("8dfa8220-074d-40e9-9018-d33a38342023", "You have not applied your change to CASS Cost Line yet. Do you want to continue?");
			}
			else if (promptForAdjustmentFilePrint)
			{
				question = Res.GetString("f0bba015-1d49-452f-af05-3dafc1fbb82f", "You have not exported your change to an Adjustment File yet. Do you want to continue?");
			}
			else if (CASSBusinessEntity.NeedToRecalculateInvoices)
			{
				question = Res.GetString("6aec92a0-1f46-4c10-88d8-9c23e5be4084", "You have changed the AP Invoice Number on one or more lines but haven't recalculated AP invoices to update the invoice number/s.\r\nDo you wish to continue without recalculating?");
			}

			if (!string.IsNullOrEmpty(question) && Globals.Message.Show(question, Res.GetString("94c3d114-3900-43ad-8a16-23284f6b812a", "Apply Changes"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.No)
			{
				result = ContinueWithSave.No;
			}

			return result;
		}

		bool IsSavedSuccessfully;

		protected override void PopulateDevTools(System.Collections.Generic.List<IDevTool> tools)
		{
			base.PopulateDevTools(tools);
			tools.Add(new CASSBillingDocumentCustomisationDevTool());
		}

		void SetAutoCloseCheckBoxVisibility()
		{
			AutoCloseClaimCheckBox.Visible = CASSBusinessEntity.ClaimExistsAndNeedsToBeClosed;
		}

		#region Event Handlers

		void ImportButton_Click(object sender, EventArgs e)
		{
			openFileDialog.Filter = CASSHOTFileDataImporter.GetFileFormat();
			openFileDialog.FilterIndex = 0;
			openFileDialog.RestoreDirectory = true;

			if (ZFormModaliser.ShowCommonDialogWithoutDispose(openFileDialog) == DialogResult.OK)
			{
				try
				{
					Cursor prevCursor = Cursor.Current;
					Cursor.Current = Cursors.WaitCursor;
					try
					{
						var importer = new CASSHOTFileDataImporter(CASSBusinessEntity, openFileDialog.UnmappedFileName);
						var notifications = new NotificationBuffer();
						importer.ImportData(openFileDialog.ForceLocalFile(), notifications);
						if (notifications.HasErrors)
						{
							throw new InvalidOperationException(notifications.AsString);
						}
						if (notifications.HasWarnings)
						{
							Globals.Message.ShowWarning(notifications.AsString);
						}
					}
					finally
					{
						Cursor.Current = prevCursor;
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.ShowError(ex.Message);
				}
				this.DiscrepancyReportGrid.SetColumnVisible(CASSBusinessEntity.IsExportBilling, CASSBillingLine.Schema.IssueDate);
				this.DiscrepancyReportGrid.SetColumnVisible(CASSBusinessEntity.IsImportBilling, [CASSBillingLine.Schema.DateOfArrival, CASSBillingLine.Schema.DateOfDelivery]);

				reloadCostsButton_Click(null, null);
			}
			this.DiscrepancyReportGrid.SetColumnVisible(CASSBusinessEntity.IsExportBilling, CASSBillingLine.Schema.IssueDate);
			this.DiscrepancyReportGrid.SetColumnVisible(CASSBusinessEntity.IsImportBilling, [CASSBillingLine.Schema.DateOfArrival, CASSBillingLine.Schema.DateOfDelivery]);
		}

		void reloadCostsButton_Click(object sender, EventArgs e)
		{
			Cursor prevCursor = Cursor.Current;
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				NotificationBuffer notifications = new NotificationBuffer();

				var adapter = new CASSHOTFileDataAdapter(notifications);
				adapter.Fill(CASSBusinessEntity);
				SetAutoCloseCheckBoxVisibility();
				UnhookAPTransactions();
				CASSBusinessEntity.ClearAPTransactions();

				if (notifications.HasErrors)
				{
					Globals.Message.ShowError(notifications.AsString);
					CASSBusinessEntity_CASSCostHeaderChanged(null, null);
				}
				else
				{
					if (CASSBusinessEntity.CostHeader.Lines != null)
					{
						CASSBusinessEntity.CostHeader.Lines.HasChanges = false;
					}
					CASSBusinessEntity.HasChanges = true;
					TabControl.SelectedIndex = TabControl.TabPages.IndexOf(DiscrepancyReportTabPage);

					if (notifications.HasWarnings)
					{
						Globals.Message.ShowWarning(notifications.AsString);
					}
				}
				CASSBusinessEntity.RunPreSaveValidation();
			}
			finally
			{
				Cursor.Current = prevCursor;
				SetEnableReloadCostButton(CASSBusinessEntity.CostHeader.Lines);
			}
		}

		void Refresh_Click(object sender, EventArgs e)
		{
			Cursor prevCursor = Cursor.Current;
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				UnhookAPTransactions();
				CASSBusinessEntity.ForceRecalculateData();
			}
			finally
			{
				Cursor.Current = prevCursor;
			}
		}

		void PrintButton_Click(object sender, EventArgs e)
		{
			CASSBusinessEntity.PrintDiscrepanciesDocument();
		}

		void ViewMenuItem_Click(object sender, EventArgs e)
		{
			if (APTransactionsGrid.SelectedElements.Length == 1)
			{
				Cursor prevCursor = Cursor.Current;
				Cursor.Current = Cursors.WaitCursor;
				try
				{
					InvoicingBase selectedInvoice = APTransactionsGrid.SelectedElements[0] as InvoicingBase;
					try
					{
						selectedInvoice = CASSBusinessEntity.GenerateCompleteInvoice(selectedInvoice);
						ZController controller = AccountingControllerCreator.GetNewController(selectedInvoice);
						controller.SetFormsModalTo(this);
						if (selectedInvoice.IsInDatabase)
						{
							controller.ShowViewForm(selectedInvoice);
						}
						else
						{
							selectedInvoice.IsImportedFromFile = true;
							IZForm form = controller.ShowFormForNewEntity(selectedInvoice);
							if (form != null)
							{
								form.DisplayMode = ODisplayMode.ReadOnly;
							}
						}
					}
					catch (TransactionNotFoundException)
					{
						Globals.Message.ShowError(Res.GetString("28da1f38-6b72-46b0-b909-5994a5ed6f97", "The posted invoice can't be found. Is Posted state was reset."));
					}
					finally
					{
						selectedInvoice.ReleaseAllMutexOnInvoice();
						GCWrapper.ReclaimMemory(ref selectedInvoice);
					}
				}
				finally
				{
					Cursor.Current = prevCursor;
				}
			}
		}

		void PreviewButton_Click(object sender, EventArgs e)
		{
			Cursor prevCursor = Cursor.Current;
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				if (!CASSBusinessEntity.HasCriticalErrorsForInvoiceCreation)
				{
					CASSBusinessEntity.CreateInvoices();
					CASSBusinessEntity.RunPreSaveValidation();
					TabControl.SelectedIndex = TabControl.TabPages.IndexOf(APTransactionsTabPage);
					HookAPTransactions();
					//We set HasChanges to true to enable the save buttons in the GUI
					CASSBusinessEntity.HasChanges = true;
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("77949A6B-83BB-4018-9FFD-B6E9E19B127D", "There are errors that need correcting before the AP invoices can be calculated. Please hover over the red stop errors for specific details."));
				}
			}
			catch (CASSGstRegistryNotSetException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
			finally
			{
				Cursor.Current = prevCursor;
			}
		}

		void exportButton_Click(object sender, EventArgs e)
		{
			if (!Env.Security.APCASSCostFileModification.IsAllowed)
			{
				Env.Security.APCASSCostFileModification.ShowError();
			}
			else
			{
				if (CASSBusinessEntity.HasAnyCASSCostAdjustmentBeenMade)
				{
					var exporter = new CASSAdjustmentFileDataExporter();
					ShowExportDialog(exporter, CASSBusinessEntity, (x) => { if (x) { promptForAdjustmentFilePrint = false; } });
				}
				else
				{
					Globals.Message.ShowInformation(Res.GetString("4ed0f606-43cf-4d83-bdd1-d1c72d3f3b93", "No Adjustment Made"));
				}
			}
		}

		void ShowExportDialog(FlatFileDataExporter exporter, CASSBilling cASSBusinessEntity, Action<bool> action)
		{
			using (DataExportForm exportForm = new DataExportForm(exporter, new ArrayBusinessObjectReader(new CASSBilling[] { cASSBusinessEntity }, typeof(CASSBilling))))
			{
				exportForm.ShowDialog();
				action(exporter.IsExportOK);
			}
		}

		void APTransactionsGrid_DoubleClick(object sender, EventArgs e)
		{
			MouseEventArgs mouseArgs = e as MouseEventArgs;
			if (mouseArgs != null && mouseArgs.Button == MouseButtons.Left)
			{
				if (APTransactionsGrid.HitTest(mouseArgs.X, mouseArgs.Y).Row > -1)
				{
					ViewMenuItem_Click(sender, EventArgs.Empty);
				}
			}
		}

		void Lines_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			int lineCount = CASSBusinessEntity.CostHeader.Lines != null ? CASSBusinessEntity.CostHeader.Lines.Count : 0;
			InfoLabel.Text = Res.GetString("CASSExportBillingForm|RecordsWereImported", "{0} records were imported.", lineCount);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void CASSBusinessEntity_OnStartLongTimeProcess(object sender, CASSBilling.LongTimeEventArgs e)
		{
			try
			{
				progressForm = new ProgressForm
				{
					ShowCancelButton = false,
				};
				progressForm.ShowModalTo(this);

				Application.DoEvents();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				progressForm?.Dispose();
				progressForm = null;
				throw;
			}
		}

		void CASSBusinessEntity_OnEndLongTimeProcess(object sender, EventArgs e)
		{
			progressForm.Dispose();
			progressForm = null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void CASSBusinessEntity_OnProgressLongTimeProcess(object sender, CASSBilling.LongTimeEventArgs e)
		{
			progressForm.SetStatusAndPercentComplete(e.Status, e.PercentComplete);
			Application.DoEvents();
		}

		void CASSBusinessEntity_JobCreationError(object sender, JobConsolCost.APInvoiceCostingJobCreationErrorEventArgs e)
		{
			Globals.Message.ShowError(e.ErrorMessage);
		}

		void CASSBusinessEntity_CASSCostHeaderChanged(object sender, EventArgs e)
		{
			SetupVisibility();
		}

		void CASSBusinessEntity_FileLineChanged(object sender, HasChangesChangedEventArgs e)
		{
			SetEnableReloadCostButton(sender as IBusinessObjectState);
		}

		ProgressForm progressForm;

		void SetEnableReloadCostButton(IBusinessObjectState bizO)
		{
			if (bizO != null)
			{
				promptForAdjustmentFilePrint = CASSBusinessEntity.HasAnyCASSCostAdjustmentBeenMade;
				reloadCostsButton.Enabled = bizO.HasChanges;
			}
		}

		void SetupVisibility()
		{
			if (CASSBusinessEntity.AllowCASSCostAdjustment)
			{
				ExportLinesTabPage.TabVisible = false;
				ImportLinesTabPage.TabVisible = false;

				if (CASSBusinessEntity.IsExportBilling)
				{
					ExportLinesTabPage.TabVisible = true;
					TabControl.SelectTab(ExportLinesTabPage.Name);
				}

				if (CASSBusinessEntity.IsImportBilling)
				{
					ImportLinesTabPage.TabVisible = true;
					TabControl.SelectTab(ImportLinesTabPage.Name);
				}

				ImportLinesGrid.Visible = ExportLinesGrid.Visible = Env.Security.APCASSCostFileModification.IsAllowed;
				SecurityCheckpointMessageForExportTabLabel.Visible = SecurityCheckpointMessageForImportTabLabel.Visible = !Env.Security.APCASSCostFileModification.IsAllowed;
			}
		}

		#endregion

		#region IButtonApplyTextOverride Members

		string IButtonApplyTextOverride.ApplyButtonText
		{
			get { return Res.GetString("3bfb5af8-b299-4a83-b885-04dcbbb9a646", "&Post"); }
		}

		#endregion

		#region IButtonPostTextOverride Members

		string IButtonPostTextOverride.PostButtonText
		{
			get { return Res.GetString("48d0b938-5013-4ccc-969f-73552888e3bd", "P&ost && Close"); }
		}

		#endregion
	}
}
