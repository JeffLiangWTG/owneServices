using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.Progress;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public partial class MonthlyUsageBillingForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public MonthlyUsageBillingForm()
		{
			InitializeComponent();
		}

		public MonthlyUsageBillingForm(MonthlyUsageBilling bo)
			: base(bo)
		{
			InitializeComponent();
			InitializeColorLabels();
			CreateViewSummaryPopupMenu();
		}

		MonthlyUsageBilling MonthlyBilling
		{
			get { return (MonthlyUsageBilling)BusinessEntity; }
		}

		#region View Summary Menu

		void CreateViewSummaryPopupMenu()
		{
			MenuItem viewSummaryMenu = new ZMenuItem("View Summary");
			viewSummaryMenu.MenuItems.Add(new ZMenuItem("-"));  // Need submenu to fire Popup Event
			viewSummaryMenu.Popup += delegate
			{
				if (OrganisationBillsGrid.ListManager != null)
				{
					OrganisationBill organisationBill = OrganisationBillsGrid.ListManager.GetCurrent() as OrganisationBill;
					if (organisationBill != null)
					{
						RebuildViewSummaryMenuItems(viewSummaryMenu, organisationBill);
					}
				}
			};

			OrganisationBillsGrid.ContextMenu.MenuItems.Add(new ZMenuItem("-"));
			OrganisationBillsGrid.ContextMenu.MenuItems.Add(viewSummaryMenu);
			OrganisationBillsGrid.ContextMenu.MenuItems.Add(new ZMenuItem("-"));
		}

		void RebuildViewSummaryMenuItems(MenuItem viewSummaryMenu, OrganisationBill organisationBill)
		{
			viewSummaryMenu.MenuItems.Clear();

			MenuItem viewSummaryMainMenuItem = CreateViewSummaryMenuItem(organisationBill, organisationBill.OrganisationPK);
			viewSummaryMenu.MenuItems.Add(viewSummaryMainMenuItem);

			IEnumerable<ZGuid> childOrganisationPKs = organisationBill.SystemUsages.Cast<SystemUsage>().Where(u => u.ShowOnBillingSummary).Select(x => x.OrganisationPK).Where(x => x != organisationBill.OrganisationPK).Distinct();
			if (childOrganisationPKs.Any())
			{
				viewSummaryMenu.MenuItems.Add(new ZMenuItem("-"));
				foreach (ZGuid childOrganisationPK in childOrganisationPKs)
				{
					MenuItem viewSummaryMenuItem = CreateViewSummaryMenuItem(organisationBill, childOrganisationPK);
					viewSummaryMenu.MenuItems.Add(viewSummaryMenuItem);
				}
			}
		}

		MenuItem CreateViewSummaryMenuItem(OrganisationBill organisationBill, ZGuid summaryOrganisationPK)
		{
			EDIOrgHeader summaryOrganisation = organisationBill.Factory.Load<EDIOrgHeader>(summaryOrganisationPK);

			MenuItem viewSummaryMenuItem = new ZMenuItem(summaryOrganisation.OH_Code);
			viewSummaryMenuItem.Click += delegate
			{
				try
				{
					using (new ZWaitCursorChanger())
					{
						DocOrganisationBill docWrapper = DocOrganisationBill.New(organisationBill, organisationBill.Factory, summaryOrganisation.PK);
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
			};

			return viewSummaryMenuItem;
		}

		#endregion

		#region Grid Coloring

		void InitializeColorLabels()
		{
			NewUserPictureBox.BackColor = NewUserColor;
			CannotInvoicePictureBox.BackColor = UsageHasErrorsColor;
			AlreadyInvoicedPictureBox.BackColor = AlreadyInvoicedColor;
		}

		void InvoiceGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			OrganisationBill bizo = (OrganisationBill)e.ObjectAtRow;
			FieldInfo rowNumberField = typeof(ColourDecidingEventArgs).GetField("RowNumber", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			if (!bizo.InvoicePkForThisMonth.IsEmpty)
			{
				e.Colour = AlreadyInvoicedColor;
			}
			else if (!bizo.CanInvoice)
			{
				e.Colour = UsageHasErrorsColor;
			}
			else if (bizo.IsNewUser)
			{
				e.Colour = NewUserColor;
			}
			else if (rowNumberField != null)
			{
				int rowNumber = (int)rowNumberField.GetValue(e);
				e.Colour = (rowNumber % 2 == 1) ? lightColor : Color.White;
			}
		}

		readonly Color NewUserColor = Color.LightGreen;
		readonly Color UsageHasErrorsColor = Color.Orange;
		readonly Color AlreadyInvoicedColor = Color.LightBlue;
		readonly Color lightColor = TmpGrid.GetLightColor();

		#endregion

		#region Button Click Handlers

		#region Generate Report

		void GenerateButton_Click(object sender, EventArgs e)
		{
			GenerateReport();
		}

		void GenerateReport()
		{
			if (MonthlyBilling.HasErrors)
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

					progressForm.Status = "Generating...";
					MonthlyBilling.GenerateReport(progress);

					// Reapply current sort since it doesn't happen automatically when elements are added one at a time
					System.ComponentModel.IBindingListView view = OrganisationBillsGrid.ListManager.List as System.ComponentModel.IBindingListView;
					if (view != null && view.SortDescriptions.Count > 0)
					{
						view.ApplySort(view.SortDescriptions);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError("Error generating report:\r\n"
					+ "DateTo: " + MonthlyBilling.DateTo.ToISO8601ShortDateString()
					+ ", Systems: " + MonthlyBilling.BillingSystemsText
					+ ", Org: " + (MonthlyBilling.Organisation?.OH_Code ?? string.Empty)
					+ ", Licence: " + MonthlyBilling.LicenceMode
					+ "\r\n"
					+ ex.ToString());
			}
		}

		#endregion

		#region Create Invoices

		void CreateInvoicesButton_Click(object sender, EventArgs e)
		{
			CreateInvoices();
		}

		void CreateInvoices()
		{
			OrganisationBill[] selection = GetValidSelectedElements();
			if (ConfirmCreateInvoices(selection.Length))
			{
				using (ProgressForm progressForm = new ProgressForm())
				{
					ProgressFormAdapter progress = new ProgressFormAdapter(progressForm);
					//progressForm.ShowCancelButton = true;
					progressForm.ShowProgressBar = true;
					ZFormModaliser.Show(progressForm, this);

					progressForm.Status = "Creating...";
					progress.SetExpectedCount(selection.Length);

					try
					{
						MonthlyBilling.CreateInvoices(selection, progress);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						Globals.Message.ShowError("Error " + progressForm.Status + ":\r\n" + ex.ToString());
					}

					OrganisationBillsGrid.Refresh();
				}
			}
		}

		OrganisationBill[] GetValidSelectedElements()
		{
			return OrganisationBillsGrid.SelectedElements.Cast<OrganisationBill>().Where(s => s.CanInvoice).ToArray();
		}

		bool ConfirmCreateInvoices(int count)
		{
			bool ok;
			var security = Enterprise.Environment.Env.Security.NewReceivablesTransaction;
			if (!security.IsAllowed)
			{
				security.ShowError();
				ok = false;
			}
			else if (count == 0)
			{
				Globals.Message.Show("Please select one or more rows without errors.", CreateInvoicesButton.Text, MessageBoxButtons.OK, DialogResult.OK);
				ok = false;
			}
			else
			{
				string msg = string.Format(CultureInfo.CurrentCulture, "Create invoices for {0} client(s)?\r\n\r\nRows with errors or amounts below minimum will be ignored.", count);
				ok = Globals.Message.Show(msg, CreateInvoicesButton.Text, MessageBoxButtons.OKCancel, DialogResult.OK) == DialogResult.OK;
			}

			return ok;
		}

		#endregion

		#region Instructions

		void InfoLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowInstructions();
		}

		void ShowInstructions()
		{
			StringBuilder instructions = new StringBuilder();
			instructions.AppendLine("1. Enter report date range.");
			instructions.AppendLine("2. Select billing systems to bill.");
			instructions.AppendLine("3. Optionally enter an organization. Result will include all organizations from the billing group.");
			instructions.AppendLine("4. Press 'Generate Report' button to generate the report.");
			instructions.AppendLine("5. Double check everything against errors/mismatches.");
			instructions.AppendLine("6. Press 'Create Invoices...' button to create invoices.");

			Globals.Message.ShowInformation(instructions.ToString());
		}

		#endregion

		void OrganisationUsageGrid_DoubleClick(object sender, EventArgs e)
		{
			DataGrid.HitTestInfo hit = OrganisationBillsGrid.HitTest(OrganisationBillsGrid.PointToClient(Cursor.Position));
			if (OrganisationBillsGrid.SelectedElements.Length > 0
				&& OrganisationBillsGrid.List[hit.Row] != null)
			{
				OrganisationBill organisationUsage = (OrganisationBill)OrganisationBillsGrid.SelectedElements[0];
				if (ModifierKeys == Keys.Control)
				{
					ShowOrgForm(organisationUsage.Organisation);
				}
				else if (!organisationUsage.InvoicePkForThisMonth.IsEmpty && !organisationUsage.BranchPK.IsEmpty)
				{
					using (BillingInvoicingHelper.BranchContext(organisationUsage.BranchPK.ToGuid()))
					{
						var invoice = new BusinessObjectFactory().Load<ARInvoice>(organisationUsage.InvoicePkForThisMonth);
						ShowInvoice(invoice);
					}
				}
			}
		}

		void ShowInvoice(BusinessObject invoice)
		{
			if (invoice != null)
			{
				ZController controller = ZControllerFactory.Create(ControllerIDs.ARInvoice);
				controller.ShowViewForm(invoice);
			}
		}

		void ShowOrgForm(BusinessObject organisation)
		{
			if (organisation != null)
			{
				ZController controller = ZControllerFactory.Create(ControllerIDs.Organisation);
				controller.ShowEditForm(organisation);
			}
		}

		void systemsButton_Click(object sender, EventArgs e)
		{
			var form = new BillingSystemChooserForm(new BillingSystemWrapperCollection(MonthlyBilling.BillingSystems));
			form.ShowDialog(this);
			MonthlyBilling.BillingSystemsTextInfo.RefreshBinding();
		}

		#endregion

		void CloseMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		void NewEditFaxPricesMenuItem_Click(object sender, EventArgs e)
		{
			var form = new FaxPricesForm(new BulkClientFaxPriceUpdater(new BusinessObjectFactory()));
			form.Show(this);
		}

		void NewEditExcludeOrgsMenuItem_Click(object sender, EventArgs e)
		{
			var form = new BillingExcludeOrgForm(new ClientLicenceBillingExcludeOrgUpdater(new BusinessObjectFactory()));
			form.Show(this);
		}

		#region Implementation

		protected override void ZForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// Prevent base class method from asking the user if they are sure they want to close
		}

		class TmpGrid : ZDisplayGrid
		{
			internal static Color GetLightColor()
			{
				using (TmpGrid grid = new TmpGrid())
				{
					return grid.LightColor;
				}
			}
		}

		#endregion

		#region ABM Customs

		void ImportABMCustomsMenuItem_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new ImportABMForm());
		}

		#endregion

		void MarkNonInvoicedUsageMenuItem_Click(object sender, EventArgs e)
		{
			var security = Enterprise.Environment.Env.Security.ViewReceivablesTransaction;
			if (security.IsAllowed)
			{
				ZFormModaliser.ShowDialogAndDispose(new MarkManuallyProcessedUsageForm());
			}
			else
			{
				security.ShowError();
			}
		}
	}
}

