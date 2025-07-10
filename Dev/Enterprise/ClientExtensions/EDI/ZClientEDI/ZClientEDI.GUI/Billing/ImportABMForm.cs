using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public partial class ImportABMForm : ZChildForm
	{
		public ImportABMForm()
		{
			InitializeComponent();
			var today = ZDateTime.Today;
			PeriodStartDateEdit.DateTimeValue = new ZDateTime(today.Year, today.Month, 1).AddMonths(-1);
		}

		public string FileToImport
		{
			get { return FilePathTextBox.Text; }
			set { FilePathTextBox.Text = value; }
		}

		void FileSelectButton_Click(object sender, EventArgs e)
		{
			var dialog = new ZOpenFileDialog();
			dialog.FileName = FileToImport;
			if (dialog.ShowDialog(this) == DialogResult.OK)
			{
				FileToImport = dialog.UnmappedFileName;
			}
		}

		void ImportButton_Click(object sender, EventArgs e)
		{
			bool isImportSuccessful = false;

			if (!PeriodStartDateEdit.DateTimeValue.IsValid)
			{
				MessageTextBox.Text = "Enter valid date from.";
			}
			else if (string.IsNullOrWhiteSpace(FileToImport))
			{
				MessageTextBox.Text = "Select file to import data.";
			}
			else
			{
				MessageTextBox.Text = "Importing data...";
				ToggleContolsStatus(false);
				var notifications = new NotificationBuffer();

				using (new ZWaitCursorChanger())
				{
					try
					{
						var periodStart = new ZDateTime(PeriodStartDateEdit.DateTimeValue.Year, PeriodStartDateEdit.DateTimeValue.Month, 1);
						var localFilePath = FileToImport;
						using (ZOpenFileDialog.ForceLocalFile(ref localFilePath))
						{
							var importer = new ABMCustomsDataImporter(new BusinessObjectFactory(), periodStart, localFilePath, notifications);
							var result = DialogResult.Yes;
							if (importer.HasExistingChargeableUsage())
							{
								result = Globals.Message.Show(
	@"Usage for the selected period already exists.
If data is imported twice it may fail or create duplicate charges.

Is this a new import file never before imported?",
									"Usage Exists", MessageBoxButtons.YesNo, DialogResult.No);
							}

							if (result == DialogResult.Yes)
							{
								importer.Import();
								if (!notifications.ContainsNotificationType(CargoWise.EntityFramework.NotificationType.Error))
								{
									importer.Save();
									isImportSuccessful = true;
								}
							}
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						notifications.AddError(ex.Message);
					}
				}

				if (isImportSuccessful)
				{
					if (DialogResult.OK == Globals.Message.Show("Data is successfully imported.", "Import Successful", MessageBoxButtons.OK, MessageBoxIcon.Information))
					{
						Close();
					}
				}
				else
				{
					ToggleContolsStatus(true);
					MessageTextBox.Text = "Import failed. Error(s) occured:" + System.Environment.NewLine + System.Environment.NewLine + notifications.AsString;
				}
			}
		}

		void ToggleContolsStatus(bool isEnabled)
		{
			this.PeriodStartDateEdit.Enabled = isEnabled;
			this.FilePathTextBox.Enabled = isEnabled;
			this.SelectFileButton.Enabled = isEnabled;
			this.ImportButton.Enabled = isEnabled;
		}

		void DeleteButton_Click(object sender, EventArgs e)
		{
			try
			{
				CheckAndDeleteUsage();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		void CheckAndDeleteUsage()
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var periodStart = new ZDateTime(PeriodStartDateEdit.DateTimeValue.Year, PeriodStartDateEdit.DateTimeValue.Month, 1);
			if (ConfirmCanDeleteWithWaitCursor(factory, periodStart))
			{
				DeleteUsage(factory, periodStart);
			}
		}

		bool ConfirmCanDeleteWithWaitCursor(BusinessObjectFactory factory, ZDateTime periodStart)
		{
			using (new ZWaitCursorChanger())
			{
				return ConfirmCanDelete(factory, periodStart);
			}
		}

		bool ConfirmCanDelete(BusinessObjectFactory factory, ZDateTime periodStart)
		{
			if (!PeriodStartDateEdit.DateTimeValue.IsValid)
			{
				MessageTextBox.Text = "Enter valid date from.";
				return false;
			}

			if (!ABMCustomsDataImporter.HasExistingChargeableUsage(factory, periodStart))
			{
				Globals.Message.Show(@"Nothing to delete. No usage for the selected period was found.");
				return false;
			}

			if (ABMCustomsDataImporter.HasExistingInvoicedChargeableUsage(factory, periodStart))
			{
				Globals.Message.Show(@"Usage for the selected period has been invoiced. It cannot be deleted unless the invoices are reversed.");
				return false;
			}

			if (ABMCustomsDataImporter.HasExistingBillingTransactions(periodStart))
			{
				Globals.Message.Show(
@"Billing records for the selected period exist in the billing database.
Please contact the billing development team to delete them first.");
				return false;
			}

			var result = Globals.Message.Show(
@"This will delete ABM usage from ediProd for the selected period.

Are you sure?",
				"Delete", MessageBoxButtons.YesNo, DialogResult.No);
			return result == DialogResult.Yes;
		}

		void DeleteUsage(BusinessObjectFactory factory, ZDateTime periodStart)
		{
			var notifications = new NotificationBuffer();
			var importer = new ABMCustomsDataImporter(factory, periodStart, string.Empty, notifications);
			using (new ZWaitCursorChanger())
			{
				importer.DeleteChargeableUsages();
				Globals.Message.Show("Usage was deleted from ediProd successfully.");
			}
		}
	}
}

