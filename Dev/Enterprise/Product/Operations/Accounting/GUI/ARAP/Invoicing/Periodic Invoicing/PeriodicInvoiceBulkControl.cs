using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class PeriodicInvoiceBulkControl : PeriodicInvoiceControl
	{
		public PeriodicInvoiceBulkControl()
		{
			JobsGridPanel.Controls.Remove(JobsGrid);
			MiscInvoicesGridPanel.Controls.Remove(MiscInvoicesGrid);
			InitializeComponent();
			SetupPeriodicInvoicesGridContextMenu();
			InitializeAdditionalCaptions();
			JobsSplitter.AllowOverlap(JobsGroupBox);
		}

		PeriodicInvoiceBulk PeriodicInvoice
		{
			get { return (PeriodicInvoiceBulk)BindingSource.Current; }
		}

		void SetupPeriodicInvoicesGridContextMenu()
		{
			PeriodicInvoicesOnJobsGrid.ContextMenu.MenuItems.Add(0, new ZMenuItem(ResString.GetMultilingualString("BulkPeriodicInvoiceForm|fe165d47-2a46-4faf-976c-a188333941ab", "Preview Invoice"), new EventHandler(PreviewInvoice)));
		}

		void InitializeAdditionalCaptions()
		{
			if (!DesignModeFinder.IsDesigning && GlbCompany.CurrentCompany.IsExtraTaxApplicable())
			{
				PeriodicInvoicesOnJobsGrid.GetColumnStyle("LocalExtraTaxAmount").CaptionResourceString = JobsGrid.GetColumnStyle(Job.Schema.JH_LocalExtraTaxAmount).CaptionResourceString;
				PeriodicInvoicesOnJobsGrid.GetColumnStyle("OSExtraTaxAmount").CaptionResourceString = JobsGrid.GetColumnStyle(Job.Schema.JH_OSExtraTaxAmount).CaptionResourceString;
			}
		}

		#region Event Handlers

		void PreviewInvoice(object sender, EventArgs e)
		{
			if (PeriodicInvoicesOnJobsGrid.SelectedElements.Length > 0)
			{
				var periodicInvoice = (PeriodicInvoice)PeriodicInvoicesOnJobsGrid.SelectedElements[0];
				if (!periodicInvoice.IsInDatabase)
				{
					if (!periodicInvoice.HasErrors)
					{
						var previewErrors = periodicInvoice.PreviewPeriodicInvoiceAndReturnErrors(eventHandler: PeriodicInvoiceErrorHandleHelper.OnCriticalPostError);

						if (previewErrors != null && previewErrors.Length > 0)
						{
							Globals.Message.ShowError(Res.GetString("193090fa-8053-4b6c-922d-6aace8848d9a",
								"This invoice can't be previewed due to the following:\r\nSince this invoice screen was opened, one of the billing lines included in this invoice has been changed.\r\n{0}\r\nYou will need to cancel this screen and begin the periodic invoice process again.",
								string.Join("\r\n", previewErrors)));
						}
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("ea614496-5093-44fb-9223-a446f7d76e28", "Selected periodic invoice has errors."));
					}
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("d9b8cd5e-e888-4bcc-bf4a-6f578a5af213", "Selected periodic invoice has already been posted."));
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("4edca6c2-671a-49ba-9250-1157a54cd5cc", "Please select a Periodic Invoice."));
			}
		}

		void JobsGridPanel_Resize(object sender, EventArgs e)
		{
			ControlDpiScalingHelper.SetHeight(ref PeriodicInvoicesOnJobsGroupBox, (int)(SplittedJobsHeight * sprittedJobsControlsSizeRatio), false);
		}

		void JobsSplitter_SplitterMoved(object sender, SplitterEventArgs e)
		{
			sprittedJobsControlsSizeRatio = (double)PeriodicInvoicesOnJobsGroupBox.Height / SplittedJobsHeight;
		}

		void JobsSplitter_DoubleClick(object sender, EventArgs e)
		{
			sprittedJobsControlsSizeRatio = 0.5;
			JobsGridPanel_Resize(this, EventArgs.Empty);
		}

		void MiscInvoicesGridPanel_Resize(object sender, EventArgs e)
		{
			ControlDpiScalingHelper.SetHeight(ref PeriodicInvoicesOnMiscInvoicesGroupBox, (int)(SplittedMiscInvoicesHeight * sprittedMiscInvoicesControlsSizeRatio), false);
		}

		void MiscInvoicesSplitter_SplitterMoved(object sender, SplitterEventArgs e)
		{
			sprittedMiscInvoicesControlsSizeRatio = (double)PeriodicInvoicesOnMiscInvoicesGroupBox.Height / SplittedMiscInvoicesHeight;
		}

		void MiscInvoicesSplitter_DoubleClick(object sender, EventArgs e)
		{
			sprittedMiscInvoicesControlsSizeRatio = 0.5;
			MiscInvoicesGridPanel_Resize(this, EventArgs.Empty);
		}

		#endregion

		#region Overrides

		protected override string GetJobsInfoLableText()
		{
			return GetInfoLableText(PeriodicInvoiceJobsCount, PeriodicInvoice.PeriodicInvoices.Count);
		}

		protected override string GetMiscInvoicesInfoLableText()
		{
			return GetInfoLableText(PeriodicInvoice.MiscInvoices.Count, PeriodicInvoice.PeriodicInvoices.Count);
		}

		int PeriodicInvoiceJobsCount
		{
			get
			{
				int count = 0;
				foreach (PeriodicInvoice invoice in PeriodicInvoice.PeriodicInvoices)
				{
					count += invoice.Jobs.Count;
				}
				return count;
			}
		}

		public override void PreparePeriodicInvoiceControl()
		{
			base.PreparePeriodicInvoiceControl();
			if (!GlbCompany.CurrentCompany.IsExtraTaxApplicable())
			{
				this.PeriodicInvoicesOnJobsGrid.RemoveFromAvailableColumns("LocalExtraTaxAmount", "OSExtraTaxAmount");
			}

			if (!AccountingMasterFilesUtils.IsTaxBranchApplicable)
			{
				PeriodicInvoicesOnJobsGrid.RemoveFromAvailableColumns("TaxBranch");
			}
		}

		#endregion

		#region Implementation

		string GetInfoLableText(int foundRecordCount, int createdPeriodicInvoiceCount)
		{
			return Res.GetString("55356ED5-CACD-4591-BEEE-D10B991E1847", "Found {0} record(s) that match your criteria. Created {1} Periodic Invoice(s).", foundRecordCount, createdPeriodicInvoiceCount);
		}

		protected int SplittedJobsHeight
		{
			get
			{
				int result = JobsGridPanel.Height - JobsNotificationPanel.Height - JobsSplitter.Height;
				return result == 0 ? 1 : result;
			}
		}

		double sprittedJobsControlsSizeRatio = 0.5;

		protected int SplittedMiscInvoicesHeight
		{
			get
			{
				int result = MiscInvoicesGridPanel.Height - MiscInvoicesNotificationPanel.Height - MiscInvoicesSplitter.Height;
				return result == 0 ? 1 : result;
			}
		}

		double sprittedMiscInvoicesControlsSizeRatio = 0.5;

		#endregion
	}
}
