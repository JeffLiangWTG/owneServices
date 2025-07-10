using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.Client.EDI.MasterFiles.Progress;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using ZClientEDI.Business.Billing;
using ZClientEDI.Business.Billing.TestingEnvironment;

namespace Enterprise.Client.EDI.Billing.GUI
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class StlBillingForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public StlBillingForm()
		{
			InitializeComponent();
		}

		public StlBillingForm(StlBilling bo)
			: base(bo)
		{
			InitializeComponent();
			InitializeColorLabels();

			invoiceGrid.ContextMenu.MenuItems.Add(new ZMenuItem("-"));
			CreateViewOrgMenu();
			CreateViewThisInvoiceMenu();
			CreateViewSummaryPopupMenu();
			invoiceGrid.ContextMenu.MenuItems.Add(new ZMenuItem("-"));
			SetToolTips();
			SetUpAdminMenuItem();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SplitterState.Persist(invoiceSplitContainer);
			SplitterState.Persist(usageSplitContainer);
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (keyData == (Keys.Control | Keys.Enter))
			{
				GenerateButton.Focus(); // commit current changes
				GenerateReport();
				return true;
			}

			return base.ProcessDialogKey(keyData);
		}

		StlBilling Billing
		{
			get { return (StlBilling)BusinessEntity; }
		}

		#region Menu

		void CreateViewOrgMenu()
		{
			MenuItem menu = new ZMenuItem("View Organization...");
			menu.Click += ViewOrgButton_Click;
			invoiceGrid.ContextMenu.MenuItems.Add(menu);
		}

		void CreateViewThisInvoiceMenu()
		{
			MenuItem menu = new ZMenuItem("View Invoice...");
			menu.Click += ViewInvoiceButton_Click;
			invoiceGrid.ContextMenu.MenuItems.Add(menu);
		}

		void CreateViewSummaryPopupMenu()
		{
			MenuItem viewSummaryMenu = new ZMenuItem("View Summary");
			viewSummaryMenu.MenuItems.Add(new ZMenuItem("-"));  // Need submenu to fire Popup Event
			viewSummaryMenu.Popup += delegate
			{
				if (invoiceGrid.ListManager != null)
				{
					var bill = invoiceGrid.ListManager.GetCurrent() as StlBill;
					if (bill != null)
					{
						RebuildViewSummaryMenuItems(viewSummaryMenu, bill);
					}
				}
			};

			invoiceGrid.ContextMenu.MenuItems.Add(viewSummaryMenu);
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
			var bizo = (StlBill)e.ObjectAtRow;
			FieldInfo rowNumberField = typeof(ColourDecidingEventArgs).GetField("RowNumber", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			if (bizo.InvoicePksForThisMonth.Any())
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

		public IBillingBackgroundTaskChecker BackgroundTaskChecker
		{
			get => backgroundTaskChecker ?? (new BillingBackgroundTaskChecker("CHU"));
			set => backgroundTaskChecker = value;
		}
		IBillingBackgroundTaskChecker backgroundTaskChecker;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void GenerateReport()
		{
			Billing.ValidateFilterStrips();
			if (Billing.HasErrors)
			{
				Globals.Message.Show("Please correct the invalid report parameters and try again.", GenerateButton.Text, MessageBoxButtons.OK, DialogResult.OK);
				return;
			}

			if (!BackgroundTaskChecker.CanProceed())
			{
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
					Billing.GenerateReport(progress);

					// Reapply current sort since it doesn't happen automatically when elements are added one at a time
					System.ComponentModel.IBindingListView view = invoiceGrid.ListManager.List as System.ComponentModel.IBindingListView;
					if (view != null && view.SortDescriptions.Count > 0)
					{
						view.ApplySort(view.SortDescriptions);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError("Error generating report:\r\n"
					+ "DateTo: " + Billing.DateTo.ToISO8601ShortDateString()
					+ ", Org: " + (Billing.Organisation?.OH_Code ?? string.Empty)
					+ ", Ent: " + Billing.EnterpriseCode
					+ ", Accumulate Months: " + Billing.AccumulateMonths
					+ ", Site Live: " + (Billing.IncludeOnlySiteLive ? "Y" : "N")
					+ ", Allow Back Post: " + (Billing.IsBackPostAllowed ? "Y" : "N")
					+ "\r\n"
					+ ex.ToString());
			}
		}

		#endregion

		#region Create Invoices

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void CreateForEditButton_Click(object sender, EventArgs e)
		{
			var selection = GetValidSelectedElements();
			if (CheckPriceItemFilter()
				&& ConfirmCreateInvoices(selection.Length, true)
				&& CheckLoginBranch(selection[0]))
			{
				try
				{
					CreateAndEditInvoice(selection[0]);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.ShowError("Error creating invoice:\r\n" + ex.Message);
				}
			}
		}

		bool CheckLoginBranch(StlBill bill)
		{
			bool ok = true;
			if (bill.InvoicingBranch == null || bill.InvoicingBranch.PK != Env.CurrentBranch.PK)
			{
				Globals.Message.ShowWarning("Must be logged into the invoice branch: " + (bill.InvoicingBranch != null ? bill.InvoicingBranch.GB_Code : ZString.Empty));
				ok = false;
			}

			return ok;
		}

		void CreateAndEditInvoice(StlBill bill)
		{
			if (!BackgroundTaskChecker.CanProceed())
			{
				return;
			}

			foreach (var invoice in Billing.CreateInvoices(bill))
			{
				var controller = ZControllerFactory.Create(invoice is ARInvoice ? ControllerIDs.ARInvoice : ControllerIDs.ARCreditNote);
				controller.SetFormsModalTo(this);
				var form = (ZForm)controller.ShowFormForNewEntity(invoice);
				foreach (var plugin in form.PlugIns.Instances)
				{
					var edocs = plugin as IEDocsPlugIn;
					if (edocs != null)
					{
						edocs.ForceSetup();
						break;
					}
				}
			}
		}

		void CreateInvoicesButton_Click(object sender, EventArgs e)
		{
			CreateInvoices();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void CreateInvoices()
		{
			if (!BackgroundTaskChecker.CanProceed() || !CheckPriceItemFilter())
			{
				return;
			}

			var selection = GetValidSelectedElements();
			if (ConfirmCreateInvoices(selection.Length, false))
			{
				try
				{
					using (ProgressForm progressForm = new ProgressForm())
					{
						ProgressFormAdapter progress = new ProgressFormAdapter(progressForm);
						//progressForm.ShowCancelButton = true;
						progressForm.ShowProgressBar = true;
						ZFormModaliser.Show(progressForm, this);

						progressForm.Status = "Creating...";
						progress.SetExpectedCount(selection.Length);

						Billing.CreateInvoices(selection, progress);

						invoiceGrid.Refresh();
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.ShowError("Error creating invoice:\r\n" + ex.Message);
				}
			}
		}

		StlBill[] GetValidSelectedElements()
		{
			return invoiceGrid.SelectedElements.Cast<StlBill>().Where(s => s.CanInvoice).ToArray();
		}

		bool ConfirmCreateInvoices(int count, bool selectOneOnly)
		{
			bool ok;
			var security = Enterprise.Environment.Env.Security.NewReceivablesTransaction;
			if (!security.IsAllowed)
			{
				security.ShowError();
				ok = false;
			}
			else if (count == 0 || (count != 1 && selectOneOnly))
			{
				string msg = selectOneOnly
					? "Please select one and only one row without errors and not already invoiced."
					: "Please select one or more rows without errors and not already invoiced.";
				Globals.Message.Show(msg, CreateInvoicesButton.Text, MessageBoxButtons.OK, DialogResult.OK);
				ok = false;
			}
			else if (!selectOneOnly)
			{
				string msg = string.Format(CultureInfo.InvariantCulture, "Create {0} invoice(s)?\r\n\r\nRows with errors or amounts below minimum will be ignored.", count);
				ok = Globals.Message.Show(msg, CreateInvoicesButton.Text, MessageBoxButtons.OKCancel, DialogResult.OK) == DialogResult.OK;
			}
			else
			{
				ok = true;
			}

			return ok;
		}

		#endregion

		void ShowInvoice(BusinessObject invoice)
		{
			if (invoice != null)
			{
				ZController controller = ZControllerFactory.Create(invoice is ARInvoice ? ControllerIDs.ARInvoice : ControllerIDs.ARCreditNote);
				controller.ShowViewForm(invoice);
			}
		}

		void ShowOrgForm(EDIOrgHeader org)
		{
			if (org != null)
			{
				var controller = (IOrganisationController)ZControllerFactory.Create(ControllerIDs.Organisation);
				controller.ShowForm(org, EDIOrganisationTabPageType.LicenceSTL, FormAction.Edit);
			}
		}

		#endregion

		void CloseMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#region Biling Exclude Orgs

		void NewEditExcludeOrgsMenuItem_Click(object sender, EventArgs e)
		{
			var form = new BillingExcludeOrgForm(new ClientLicenceBillingExcludeOrgUpdater(new BusinessObjectFactory()));
			form.Show(this);
		}

		#endregion

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

		#region Imports

		void ImportABMCustomsMenuItem_Click(object sender, EventArgs e)
		{
			using (var form = new ImportABMForm())
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		void ImportNonCW1UsagesMenuItem_Click(object sender, EventArgs e)
		{
			new EdiBillingTransactionImporter().Import();
		}

		#endregion

		void ViewCurrentOrg()
		{
			if (invoiceGrid.ListManager != null)
			{
				var bill = invoiceGrid.ListManager.GetCurrent() as StlBill;
				if (bill != null)
				{
					ShowOrgForm(bill.Organisation);
				}
			}
		}

		void ViewCurrentInvoice()
		{
			if (invoiceGrid.ListManager != null)
			{
				var bill = invoiceGrid.ListManager.GetCurrent() as StlBill;
				if (bill != null && bill.InvoicePksForThisMonth.Any() && bill.InvoicingBranch != null)
				{
					using (BillingInvoicingHelper.BranchContext(bill.InvoicingBranch.PK.ToGuid()))
					{
						foreach (var pk in bill.InvoicePksForThisMonth)
						{
							var invoice = new BusinessObjectFactory().Load<InvoicingBase>(pk);
							ShowInvoice(invoice);
						}
					}
				}
			}
		}

		void ViewOrgButton_Click(object sender, EventArgs e)
		{
			ViewCurrentOrg();
		}

		void ViewInvoiceButton_Click(object sender, EventArgs e)
		{
			ViewCurrentInvoice();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void ShowSummary(StlMonthlyUsage monthlyUsage)
		{
			try
			{
				using (new ZWaitCursorChanger())
				{
					DocStlSummary docWrapper;
					using (monthlyUsage.Bill?.TemporarilySwitchToInvoiceLanguage())
					{
						docWrapper = DocStlSummary.New(monthlyUsage, monthlyUsage.Factory);
					}
					byte[] rawDocument = BillingInvoicingHelper.GetRawDocumentInPdf(StlBill.LoadSummaryTemplate(monthlyUsage.Factory), docWrapper, monthlyUsage.Bill?.InvoiceLanguage);

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

		void ViewSummaryButton_Click(object sender, EventArgs e)
		{
			if (invoiceGrid.ListManager != null)
			{
				var bill = invoiceGrid.ListManager.GetCurrent() as StlBill;
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

						summaryMenu.Show(viewSummaryButton, ControlDpiScalingHelper.NewScaledPoint(0, viewSummaryButton.Height, false));
					}
				}
			}
		}

		void StlPreviewMenuItem_Click(object sender, EventArgs e)
		{
			var preview = new StlPreview(new BusinessObjectFactory() { RefreshEnabled = false });
			preview.SetDefaults();
			var form = new StlPreviewForm(preview);
			form.Show(this);
		}

		void FreeTrialMenuItem_Click(object sender, EventArgs e)
		{
			var dateTo = Billing.DateTo;

			var msg = "Create free trials from any new usage in period " + dateTo.Year + "-" + dateTo.Month + ".";
			if (DialogResult.OK == Globals.Message.Show(msg, "Free Trials", MessageBoxButtons.OKCancel, MessageBoxIcon.Question))
			{
				var logger = new SimpleLogger();
				new FreeTrials().CreateFreeTrialsFromNewUsage(new DateTime(dateTo.Year, dateTo.Month, 1), logger);
				var logText = logger.ToString();
				if (logText.Length == 0)
				{
					logText = "No new trials found.";
				}
				Globals.Message.Show(logText, "Free Trials", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		void UsageUpdateMenuItem_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new UsageUpdateForm());
		}

		void BillingTestingEnvironmentSyncMenuItem_Click(object sender, EventArgs e)
		{
			if (DialogResult.OK == Globals.Message.ShowConfirmation(
@"This operation will synchronize the Billing Testing Environment with the latest billing data, replacing any existing information.
Please note that this action cannot be undone.
Are you sure you want to proceed?",
				"Warning",
				"I CONFIRM THAT I WANT TO SYNC.",
				MessageBoxIcon.Question))
			{
				using (var progressForm = new ProgressForm())
				{
					var progressLogger = new ProgressFormAdapterLogger(progressForm);
					progressForm.ShowCancelButton = false;
					progressForm.ShowProgressBar = false;
					ZFormModaliser.Show(progressForm, this);

					var synchronizer = new TestingEnvironmentSynchronizer(progressLogger);
					synchronizer.Synchronise();
					Globals.Message.ShowInformation(new[] { progressLogger.ToString(), "Operation has been successful" }.First(x => !string.IsNullOrWhiteSpace(x)));
				}
			}
		}

		void BillingExternalServerCheckerMenuItem_Click(object sender, EventArgs e)
		{
			var userResponseArgument = new UserResponseArgument();
			userResponseArgument.Message = "Please enter the connection string for the new external server, or leave it blank to test the current ones.";
			userResponseArgument.UserResponseTextBoxCharactersCasing = ZCharacterCasing.Normal;
			userResponseArgument.UserResponseDropEditCharactersCasing = ZCharacterCasing.Normal;
			userResponseArgument.Buttons = ZMessageBoxButtons.OKCancel;
			userResponseArgument.MinimumResponseLength = 0;
			userResponseArgument.DefaultAnswer = " ";

			var connectionString = Globals.Message.QueryUserResponse(userResponseArgument);
			if (!string.IsNullOrEmpty(connectionString))
			{
				using (var progressForm = new ProgressForm())
				{
					var progressLogger = new ProgressFormAdapterLogger(progressForm);
					progressForm.ShowCancelButton = false;
					progressForm.ShowProgressBar = false;
					ZFormModaliser.Show(progressForm, this);

					var checker = new ExternalServerChecker(progressLogger);
					if (string.IsNullOrWhiteSpace(connectionString))
					{
						checker.Check();
					}
					else
					{
						checker.Check(connectionString);
					}

					Globals.Message.ShowInformation(progressLogger.Logs);
				}
			}
		}

		void BillingMilestoneSetterMenuItem_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new StlBillingMilestoneSetterForm(new StlBillingMilestoneSetter()));
		}

		void ValidationResultsGrid_DoubleClick(object sender, EventArgs e)
		{
			if (invoiceGrid.ListManager.Count > 0 && validationResultsGrid.GetCurrent() is StlBillValidationResult validationResult)
			{
				for (var i = 0; i < invoiceGrid.ListManager.Count; i++)
				{
					var bill = (StlBill)invoiceGrid.List[i];
					if (bill.PK == validationResult.StlBillPK)
					{
						zTabControl1.SelectedTab = invoiceTabPage;
						invoiceGrid.ListManager.Position = i;
						break;
					}
				}
			}
		}

		bool CheckPriceItemFilter()
		{
			var result = !Billing.IsPriceItemFilterApplicableOnReport;
			if (!result)
			{
				Globals.Message.Show("Invoice(s) cannot be generated or posted while a Price Item Filter is active.", GenerateButton.Text, MessageBoxButtons.OK, DialogResult.OK);
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1046", Justification = "Controls are not buttons")]
		void SetToolTips()
		{
			viewOrgButton.ToolTipCaption = (NoResString)"Open Organisation for active row";
			viewInvoiceButton.ToolTipCaption = (NoResString)"Display posted invoice for active row";
			viewSummaryButton.ToolTipCaption = (NoResString)"Display billing summary for active row";
			CreateInvoicesButton.ToolTipCaption = (NoResString)"Post all invoices";
			createForEditButton.ToolTipCaption = (NoResString)"Preview system generated invoice before posting";

			ToolTipService.SetToolTip(siteLiveCheckBox, (NoResString)"Only generate billings for customers with a valid Agreed Go Live Date");
			ToolTipService.SetToolTip(IsBackPostAvailableCheckBox, (NoResString)"Check to confirm subledger for specified period is still open for posting");
			ToolTipService.SetToolTip(IsBackPostAllowedCheckBox, (NoResString)"Allow posting to prior period if subledger is still open");
		}

		void SetUpAdminMenuItem()
		{
			var adminFunctionAllowed = EDISecurityCheckpoints.STLBillingAdminFunction.IsAllowed;
			foreach (var item in adminMenuItem.MenuItems)
			{
				if (item is MenuItem menuItem)
				{
					menuItem.Enabled = adminFunctionAllowed;
				}
			}
		}

		#region For Testing
#if DEBUG

		public ZMenuItem AdminMenuItem_Exposed => adminMenuItem;

#endif
		#endregion
	}
}

