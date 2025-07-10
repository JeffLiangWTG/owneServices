using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business.Maintenance;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.Client.EDI.MasterFiles.Progress;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public partial class MaintenanceBillingForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public MaintenanceBillingForm()
		{
			InitializeComponent();
		}

		public MaintenanceBillingForm(MaintenanceBilling bo)
			: base(bo)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, null, closeButton, saveButton);
			DisplayModeChanged += new DisplayModeChangedEventHandler(OnDisplayModeChanged);
			DisableNewAction();
			saveButton.Enabled = false;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SplitterState.Persist(invoiceSplitter);
			SplitterState.Persist(billSplitter);
			SplitterState.Persist(invoiceLineSplitter);
		}

		void UpdateBillSelection()
		{
			increasePercentButton.Enabled = billGrid.CurrentRowIndex >= 0;
			editLicenceButton.Enabled = billGrid.CurrentRowIndex >= 0;
		}

		void OnDisplayModeChanged(object sender, DisplayModeChangedEventArgs e)
		{
			saveButton.Enabled = e.ToMode == ZArchitecture.Core.ODisplayMode.Edit;
		}

		MaintenanceBilling MaintenanceBilling
		{
			get { return (MaintenanceBilling)BusinessEntity; }
		}

		bool ConfirmSaveChanges(object sender)
		{
			System.ComponentModel.CancelEventArgs e = new System.ComponentModel.CancelEventArgs();
			ZForm_Closing(sender, e);
			return !e.Cancel;
		}

		void GenerateButton_Click(object sender, EventArgs e)
		{
			if (ConfirmSaveChanges(GenerateButton))
			{
				Generate();
				DisplayMode = ZArchitecture.Core.ODisplayMode.NewSaved;
			}
		}

		void Generate()
		{
			if (MaintenanceBilling.Filter.HasErrors)
			{
				Globals.Message.Show("Please correct the invalid report parameters and try again.", GenerateButton.Text, MessageBoxButtons.OK, DialogResult.OK);
				return;
			}

			try
			{
				using (ProgressForm progressForm = new ProgressForm())
				{
					ProgressFormAdapter progress = new ProgressFormAdapter(progressForm);
					progressForm.ShowCancelButton = true;
					ZFormModaliser.Show(progressForm, this);

					MaintenanceBilling.Generate(progress);

					// Reapply current sort since it doesn't happen automatically when elements are added one at a time
					System.ComponentModel.IBindingListView view = billGrid.ListManager.List as System.ComponentModel.IBindingListView;
					if (view != null && view.SortDescriptions.Count > 0)
					{
						view.ApplySort(view.SortDescriptions);
					}

					UpdateBillSelection();
					SetupPerModuleCheckBox();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError("Error generating report:\r\n" + ex.Message);
			}
		}

		#region Per Module Check Box

		void SetupPerModuleCheckBox()
		{
			foreach (MaintenanceBillRecipient billRecipient in MaintenanceBilling.Recipients)
			{
				billRecipient.ShowPerModuleAmountsInfo.ValueChanged += ShowPerModuleAmountsInfo_ValueChanged;
			}

			UpdatePerModuleCheckState();
		}

		void ShowPerModuleAmountsInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdatePerModuleCheckState();
		}

		void UpdatePerModuleCheckState()
		{
			if (!inPerModuleCheckBox_Click)
			{
				var showPerModuleAmounts = MaintenanceBilling.ShowPerModuleAmounts;
				perModuleCheckBox.CheckState = showPerModuleAmounts.HasValue
					? (showPerModuleAmounts.Value ? CheckState.Checked : CheckState.Unchecked)
					: CheckState.Indeterminate;
			}
		}

		void perModuleCheckBox_Click(object sender, EventArgs e)
		{
			inPerModuleCheckBox_Click = true;
			MaintenanceBilling.ShowPerModuleAmounts = perModuleCheckBox.Checked;
			inPerModuleCheckBox_Click = false;
		}

		bool inPerModuleCheckBox_Click;

		#endregion

		#region Create Invoices

		void CreateInvoicesButton_Click(object sender, EventArgs e)
		{
			if (ConfirmSaveChanges(sender))
			{
				if (MaintenanceBilling.HasChanges)
				{
					Globals.Message.ShowInformation("Cannot create invoices if there are unsaved changes.");
				}
				else if (MaintenanceBilling.HasErrors)
				{
					Globals.Message.ShowError("Please correct the errors and try again.", CreateInvoicesButton.Text);
				}
				else
				{
					CreateInvoices(sender == ForceCreateInvoicesButton);
				}
			}
		}

		void CreateInvoices(bool force)
		{
			var selection = GetValidSelectedElements(force);
			if (ConfirmCreateInvoices(selection.Length))
			{
				try
				{
					using (ProgressForm progressForm = new ProgressForm())
					{
						ProgressFormAdapter progress = new ProgressFormAdapter(progressForm);
						progressForm.ShowCancelButton = true;
						progressForm.ShowProgressBar = true;
						ZFormModaliser.Show(progressForm, this);

						progressForm.Status = "Creating...";
						progress.SetExpectedCount(selection.Length);

						MaintenanceBilling.CreateInvoices(selection, progress);

						billGrid.Refresh();
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.ShowError("Error creating invoice:\r\n" + ex.Message);
				}
			}
		}

		MaintenanceBillRecipient[] GetValidSelectedElements(bool force)
		{
			return invoiceGrid.SelectedElements.Cast<MaintenanceBillRecipient>().Where(s => force ? s.CanForceInvoice : s.CanInvoice).ToArray();
		}

		bool ConfirmCreateInvoices(int count)
		{
			bool ok;
			if (count == 0)
			{
				Globals.Message.Show("Please select one or more rows without errors.", CreateInvoicesButton.Text, MessageBoxButtons.OK, DialogResult.OK);
				ok = false;
			}
			else if (Enterprise.Environment.Env.Security.NewReceivablesTransaction.IsAllowed)
			{
				string msg = string.Format(CultureInfo.CurrentCulture, "Create invoices for {0} client(s)?\r\n\r\nRows with errors will be ignored.", count);
				ok = Globals.Message.Show(msg, CreateInvoicesButton.Text, MessageBoxButtons.OKCancel, DialogResult.OK) == DialogResult.OK;
			}
			else
			{
				Enterprise.Environment.Env.Security.NewReceivablesTransaction.ShowError();
				ok = false;
			}

			return ok;
		}

		#endregion

		void viewInvoiceButton_Click(object sender, EventArgs e)
		{
			MaintenanceBillRecipient recipient = invoiceGrid.ListManager.GetCurrent() as MaintenanceBillRecipient;

			if (recipient != null)
			{
				ShowInvoices(recipient.Invoices);
			}
		}

		void ShowInvoices(IEnumerable<BusinessObject> invoices)
		{
			foreach (var invoice in invoices)
			{
				ZController controller = ZControllerFactory.Create(ControllerIDs.ARInvoice);
				controller.ShowViewForm(invoice);
			}
		}

		#region Implementation

		protected override string FormClosingQuestion
		{
			get { return "Do you want to save the changes you made?"; }
		}

		protected override void Save(CargoWise.Integration.ITransactionParticipant[] factories)
		{
			// Only the BillsFactory is saved. A new factory is used for every Generate() to allow
			// unwanted changes to be discarded.
			base.Save(new CargoWise.Integration.ITransactionParticipant[] { MaintenanceBilling.BillsFactory });
		}

		#endregion

		void InfoLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowInstructions();
		}

		void ShowInstructions()
		{
			StringBuilder instructions = new StringBuilder();
			instructions.AppendLine("Enter report parameters.");
			instructions.AppendLine("Press 'Generate Report' button to generate the report.");
			instructions.AppendLine("Update/check maintenance percentages.");
			instructions.AppendLine("Go to Invoices tab.");
			instructions.AppendLine("  Set the folder containing files to attach to invoices.");
			instructions.AppendLine("  Press '" + CreateInvoicesButton.Text + "' to create invoices.");
			instructions.AppendLine("  This will not create an invoice if an invoice already exists.");
			instructions.AppendLine("  Use '" + ForceCreateInvoicesButton.Text + "' to create an invoice if an invoice already exists.");
			instructions.AppendLine("  Use this only if an invoice has been manually credited.");

			Globals.Message.ShowInformation(instructions.ToString());
		}

		void attachmentsLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			StringBuilder instructions = new StringBuilder();
			instructions.AppendLine("All files in this folder will be attached to each invoice.");
			instructions.AppendLine("For country specific attachments, place them in a <CountryCode> subfolder.");
			instructions.AppendLine("E.g. place attachments for invoices out of the CargoWise Chicago branch in a subfolder called: US");
			instructions.AppendLine("Note, country is not the client country. It is the CargoWise invoicing country.");
			instructions.AppendLine("If there is no folder for a particular country then the files in the root folder will be used.");
			instructions.AppendLine();
			instructions.AppendLine("For client specific attachments, place them in a subfolder called: " + MaintenanceBilling.ClientAttachmentFolderName);
			instructions.AppendLine("The subfolder can be in a country folder or this folder.");
			instructions.AppendLine("The file(s) must start with the organization code.");
			instructions.AppendLine("For files in a country folder, the organization must be billed from that country.");
			instructions.AppendLine("If there are files for an org in both a country subfolder and the root folder they will all be attached.");
			instructions.AppendLine("E.g. AU/" + MaintenanceBilling.ClientAttachmentFolderName + "/QUASHIBNE Doc1.pdf");
			instructions.AppendLine("     AU/" + MaintenanceBilling.ClientAttachmentFolderName + "/QUASHIBNE Doc2.docx");
			instructions.AppendLine("or " + MaintenanceBilling.ClientAttachmentFolderName + "/QUASHIBNE Doc1.pdf");

			Globals.Message.ShowInformation(instructions.ToString());
		}

		void attachmentFolderButton_Click(object sender, EventArgs e)
		{
			using (ZFolderBrowserDialog dlg = new ZFolderBrowserDialog())
			{
				dlg.RequireMappablePath = true;
				dlg.SelectedPath = MaintenanceBilling.AttachmentFolder;
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					MaintenanceBilling.AttachmentFolder = dlg.UnmappedSelectedPath;
				}
			}
		}

		void editLicenceButton_Click(object sender, EventArgs e)
		{
			MaintenanceBill item = billGrid.ListManager.GetCurrent() as MaintenanceBill;

			if (item != null)
			{
				ZController controller = ZControllerFactory.Create(ControllerIDs.Organisation);
				var factory2 = new BusinessObjectFactory();
				var org = factory2.Load<EDIOrgHeader>(item.LicHeader.Company.LC_OH);
				controller.ShowEditForm(org);
			}
		}

		void increasePercentButton_Click(object sender, EventArgs e)
		{
			using (IncreaseMaintenanceForm form = new IncreaseMaintenanceForm())
			{
				if (form.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
				{
					decimal percentage = form.Percentage;
					bool increaseNew = form.IncreaseNew;
					bool increaseOld = form.IncreaseOld;

					foreach (MaintenanceBill bill in billGrid.SelectedRowCount > 0 ? billGrid.SelectedElements : new BusinessObject[] { (BusinessObject)billGrid.ListManager.GetCurrent() })
					{
						if (increaseNew)
						{
							bill.NewSeatPercentIncrease = percentage;
						}
						if (increaseOld)
						{
							bill.OldSeatPercentIncrease = percentage;
						}

						bill.RefreshBinding();
					}
				}
			}
		}
	}
}

