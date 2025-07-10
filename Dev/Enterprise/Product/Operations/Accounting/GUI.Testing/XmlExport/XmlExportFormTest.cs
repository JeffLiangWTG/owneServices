using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.XmlExport.Testing
{
	[TestedType(typeof(XmlExportForm))]
	public class XmlExportFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new XmlExportForm(new XmlExportGUIWrapper(Factory));
		}

		public void TestFormCaption()
		{
			XmlExportGUIWrapper gUIWrapper = new XmlExportGUIWrapper(Factory);
			using (XmlExportForm form = new XmlExportForm(gUIWrapper))
			{
				AssertEquals("Export Accounting Transactions", form.FormCaption);
			}
		}

		public void TestExistingBatchNumberChanged()
		{
			XmlExportGUIWrapper gUIWrapper = new XmlExportGUIWrapper(Factory);

			using (XmlExportForm form = new XmlExportForm(gUIWrapper))
			{
				AssertEquals("Should be enabled by default", true, form.OrganisationModuleButtonGrid.Enabled);
				AssertEquals("Should be enabled by default", true, form.JobModuleButtonGrid.Enabled);
				AssertEquals("Should be enabled by default", true, form.DepartmentModuleButtonGrid.Enabled);
				AssertEquals("Should be enabled by default", true, form.BranchModuleButtonGrid.Enabled);

				gUIWrapper.ExistingBatchNumberToExport = 1;

				AssertEquals("Should not be enabled due to batch number", false, form.OrganisationModuleButtonGrid.Enabled);
				AssertEquals("Should not be enabled due to batch number", false, form.JobModuleButtonGrid.Enabled);
				AssertEquals("Should not be enabled due to batch number", false, form.DepartmentModuleButtonGrid.Enabled);
				AssertEquals("Should not be enabled due to batch number", false, form.BranchModuleButtonGrid.Enabled);

				gUIWrapper.ExistingBatchNumberToExport = 0;

				AssertEquals("Should be enabled again", true, form.OrganisationModuleButtonGrid.Enabled);
				AssertEquals("Should be enabled again", true, form.JobModuleButtonGrid.Enabled);
				AssertEquals("Should be enabled again", true, form.DepartmentModuleButtonGrid.Enabled);
				AssertEquals("Should be enabled again", true, form.BranchModuleButtonGrid.Enabled);
			}
		}

		public void TestHighWaterMarkMessageRefreshBinding()
		{
			CodeDescriptionBoolCollection defaultCollection = new CodeDescriptionBoolCollection {
					{ ExportTransactionTypes.ARInvoice, (NoResString)"AR Invoice", true },
					{ ExportTransactionTypes.APInvoice, (NoResString)"AP Invoice", true },
					{ ExportTransactionTypes.ARCreditNote, (NoResString)"AR Credit Note", false },
					{ ExportTransactionTypes.APCreditNote, (NoResString)"AP Credit Note", false },
					{ ExportTransactionTypes.ARAdjustmentNote, (NoResString)"AR Adjustment Note", false },
					{ ExportTransactionTypes.APAdjustmentNote, (NoResString)"AP Adjustment Note", false },
					{ ExportTransactionTypes.WIPPosting, (NoResString)"WIP Posting", false },
					{ ExportTransactionTypes.AccrualPosting, (NoResString)"Accrual Posting", false },
					{ ExportTransactionTypes.WIPReversal, (NoResString)"WIP Reversal", false },
					{ ExportTransactionTypes.AccrualReversal, (NoResString)"Accrual Reversal", false },
					{ ExportTransactionTypes.UnallocatedAPInvoices, (NoResString)"Unallocated AP Invoices", false },
					{ ExportTransactionTypes.UnallocatedAPCreditNotes, (NoResString)"Unallocated AP Credit Notes", false } };
			SystemDataRegistry.Instance.AccountingTransactionTypes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, defaultCollection);

			using (SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.DataType.SuspendValidation())
			{
				SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcNow.ToDateTime());
			}

			using (XmlExportForm form = (XmlExportForm)GetFormToBash())
			{
				var highWaterMarkMessage = ZString.Empty;
				var gUIWrapper = (XmlExportGUIWrapper)form.BusinessEntity;
				var exporterWithHighWaterMark = gUIWrapper.DataExporter as DataTransfer.ISupportHighWaterMark;

				if (exporterWithHighWaterMark != null && exporterWithHighWaterMark.IsHighWaterMarkEnabled)
				{
					highWaterMarkMessage = ZString.Format(@"You have selected the same transaction types as nominated in the registry ('System/Data Export Settings/Accounting Transaction Types').
To aid performance of the export, the system will only search for un-batched transactions that were created or edited since {0}.
If you need to export transactions from before this date, please use the date filtering and this will search for all un-batched transactions.", SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.Value);
				}

				form.Show();
				gUIWrapper.IncludeARInvoices = true;
				gUIWrapper.IncludeAPInvoices = true;
				gUIWrapper.ExistingBatchNumberToExport = 0;
				AssertEquals("HighWaterMarkMessage", highWaterMarkMessage, form.HighWaterMarkLabel.Text);

				var creator = new TestObjectCreator(Factory);
				var shipment = creator.CreateShipment("S0001", false);
				var job = creator.CreateJob(shipment);
				Factory.Save();

				gUIWrapper.SelectedJobs.Load();
				AssertEquals("HighWaterMarkMessage", ZString.Empty, form.HighWaterMarkLabel.Text);
				gUIWrapper.SelectedJobs.RemoveAll();
				AssertEquals("HighWaterMarkMessage", highWaterMarkMessage, form.HighWaterMarkLabel.Text);

				gUIWrapper.SelectedBranches.AddNew();
				AssertEquals("HighWaterMarkMessage", ZString.Empty, form.HighWaterMarkLabel.Text);
				gUIWrapper.SelectedBranches.RemoveAll();
				AssertEquals("HighWaterMarkMessage", highWaterMarkMessage, form.HighWaterMarkLabel.Text);

				gUIWrapper.SelectedDepartments.AddNew();
				AssertEquals("HighWaterMarkMessage", ZString.Empty, form.HighWaterMarkLabel.Text);
				gUIWrapper.SelectedDepartments[0].Delete();
				AssertEquals("HighWaterMarkMessage", highWaterMarkMessage, form.HighWaterMarkLabel.Text);

				gUIWrapper.SelectedOrganisations.AddNew();
				AssertEquals("HighWaterMarkMessage", ZString.Empty, form.HighWaterMarkLabel.Text);
				gUIWrapper.SelectedOrganisations.RemoveAll();
				AssertEquals("HighWaterMarkMessage", highWaterMarkMessage, form.HighWaterMarkLabel.Text);
			}
		}
	}
}
