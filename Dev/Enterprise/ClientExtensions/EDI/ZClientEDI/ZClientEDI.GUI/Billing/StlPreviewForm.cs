using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.MasterFiles.Progress;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public partial class StlPreviewForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public StlPreviewForm()
		{
			InitializeComponent();
		}

		public StlPreviewForm(StlPreview bo)
			: base(bo)
		{
			InitializeComponent();

			reportTabPage.Enabled = false;
		}

		void mainTabControl_Selecting(object sender, TabControlCancelEventArgs e)
		{
			e.Cancel = !e.TabPage.Enabled;
		}

		void generateButton_Click(object sender, EventArgs e)
		{
			using (new ZWaitCursorChanger())
			{
				var preview = ((StlPreview)BusinessEntity);
				preview.RunPreSaveValidation();
				if (preview.HasErrors)
				{
					reportTabPage.Enabled = false;
				}
				else
				{
					reportTabPage.Enabled = true;

					using (ProgressForm progressForm = new ProgressForm())
					{
						ProgressFormAdapter progress = new ProgressFormAdapter(progressForm);
						progressForm.ShowCancelButton = true;
						ZFormModaliser.Show(progressForm, this);

						progressForm.Status = "Generating...";
						preview.GenerateReport(progress);
					}

					mainTabControl.SelectedTab = reportTabPage;
				}
			}
		}

		#region View Summary

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void ShowSummary(StlMonthlyUsage monthlyUsage)
		{
			try
			{
				using (new ZWaitCursorChanger())
				{
					DocStlSummary docWrapper = DocStlSummary.New(monthlyUsage, monthlyUsage.Factory);
					byte[] rawDocument = BillingInvoicingHelper.GetRawDocumentInPdf(StlBill.LoadSummaryTemplate(monthlyUsage.Factory), docWrapper);

					string fileName = Temp.GetTempFileNameWithExtension(BillingConstants.FileExtensions.Pdf);
					System.IO.File.WriteAllBytes(fileName, rawDocument);
					FileOpener.Open(fileName);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError("There was a problem showing summary document:\r\n" + ex.ToString());
			}
		}

		ContextMenu summaryMenu;

		void viewStlSummaryButton_Click(object sender, EventArgs e)
		{
			if (stlPreviewLinesGrid.ListManager != null)
			{
				var bill = GetObjectWithWaitCursor(() => (stlPreviewLinesGrid.ListManager.GetCurrent() as StlPreviewLine)?.GetStlBillForSummary(BusinessEntity.Factory));
				if (bill != null)
				{
					if (bill.MonthlyUsages.Count() == 1)
					{
						ShowSummary(bill.MonthlyUsages.First());
					}
					else
					{
						if (summaryMenu == null)
						{
							summaryMenu = new ContextMenu();
							components.Add(summaryMenu);
						}
						RebuildViewSummaryMenuItems(summaryMenu, bill);

						summaryMenu.Show(viewStlSummaryButton, ControlDpiScalingHelper.NewScaledPoint(0, viewStlSummaryButton.Height, false));
					}
				}
			}
		}

		void RebuildViewSummaryMenuItems(Menu viewSummaryMenu, StlBill bill)
		{
			viewSummaryMenu.MenuItems.Clear();

			foreach (var monthlyUsage in bill.MonthlyUsages)
			{
				MenuItem viewSummaryMenuItem = CreateViewSummaryMenuItem(monthlyUsage);
				viewSummaryMenu.MenuItems.Add(viewSummaryMenuItem);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		MenuItem CreateViewSummaryMenuItem(StlMonthlyUsage monthlyUsage)
		{
			var bill = monthlyUsage.Bill;
			MenuItem viewSummaryMenuItem = new ZMenuItem(monthlyUsage.EnterpriseCode + "-" + monthlyUsage.ServerCode + " (" + bill.PeriodStart.ToString("MMMM yyyy", CultureInfo.InvariantCulture) + ")");
			viewSummaryMenuItem.Click += delegate
			{
				ShowSummary(monthlyUsage);
			};

			return viewSummaryMenuItem;
		}

		void viewOdplSummaryButton_Click(object sender, EventArgs e)
		{
			var organisationBill = GetObjectWithWaitCursor(() => (stlPreviewLinesGrid.ListManager.GetCurrent() as StlPreviewLine)?.GetOrganisationBillForSummary(BusinessEntity.Factory));
			if (organisationBill != null)
			{
				ShowOdplSummary(organisationBill);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void ShowOdplSummary(OrganisationBill organisationBill)
		{
			try
			{
				using (new ZWaitCursorChanger())
				{
					DocOrganisationBill docWrapper = DocOrganisationBill.New(organisationBill, organisationBill.Factory, organisationBill.OrganisationPK);
					byte[] rawDocument = BillingInvoicingHelper.GetRawDocumentInPdf(organisationBill.SummaryDocTemplate, docWrapper);

					string fileName = Temp.GetTempFileNameWithExtension(BillingConstants.FileExtensions.Pdf);
					System.IO.File.WriteAllBytes(fileName, rawDocument);
					FileOpener.Open(fileName);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError("There was a problem showing summary document:\r\n" + ex.ToString());
			}
		}

		#endregion

		void closeButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		T GetObjectWithWaitCursor<T>(Func<T> func)
		{
			var currentCursor = Cursor.Current;

			try
			{
				Cursor = Cursors.WaitCursor;
				return func();
			}
			finally
			{
				Cursor = currentCursor;
			}
		}
	}
}

