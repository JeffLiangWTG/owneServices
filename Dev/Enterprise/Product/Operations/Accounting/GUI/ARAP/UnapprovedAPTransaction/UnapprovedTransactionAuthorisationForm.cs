using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.ARAP.UnapprovedAPTransaction
{
	public partial class UnapprovedTransactionAuthorisationForm : ZChildForm
	{
		public UnapprovedTransactionAuthorisationForm(UnapprovedTransactionConverter unapprovedTransactionConverter)
			: base(unapprovedTransactionConverter)
		{
			Argument.NotNull(unapprovedTransactionConverter, "UnapprovedTransactionConverter");
			if (!DesignModeFinder.IsDesigning)
			{
				DataSourceChanged += new EventHandler(Form_DataSourceChanged);
			}
		}

		void Form_DataSourceChanged(object sender, EventArgs e)
		{
			if (DataSource == null)
			{
				var message = string.Format(CultureInfo.InvariantCulture, (NoResString)"Data Source is set to null, call stack: {0}", System.Environment.StackTrace);  // set debugging message for error reporter
				ErrorReporter.ReportOnce("UnapprovedTransactionAuthorisationForm.ReportSetNullDataSource", message);
			}
		}

		new UnapprovedTransactionConverter BusinessEntity
		{
			get { return (UnapprovedTransactionConverter)base.BusinessEntity; }
		}

		#region Overrides

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			int menuIndex = 0;
			CandidatesGrid.ContextMenu.MenuItems.Add(menuIndex, new ZMenuItem(ResString.GetMultilingualString("UnapprovedTransactionAuthorisationForm|D359BB9E-DDEF-4de2-88E7-BAB54F1D0BCF", "Edit then Approve Invoice"), new EventHandler(CadidatesGrid_EditApproveTransactions_Click)));
			menuIndex++;
			CandidatesGrid.ContextMenu.MenuItems.Add(menuIndex, new ZMenuItem(ResString.GetMultilingualString("UnapprovedTransactionAuthorisationForm|3E122139-3CD3-4ccc-BF79-D4EE63D1FD7E", "Approve Invoice"), new EventHandler(CandidatesGrid_ApproveInvoices_Click)));
			menuIndex++;
			CandidatesGrid.ContextMenu.MenuItems.Add(menuIndex, new ZMenuItem("-"));
			menuIndex++;
			CandidatesGrid.ContextMenu.MenuItems.Add(menuIndex, new ZMenuItem(ResString.GetMultilingualString("UnapprovedTransactionAuthorisationForm|EditAndApproveWithClaim", "Edit and Approve with Claim"), new EventHandler(CadidatesGrid_EditApproveTransactionsWithClaim_Click)));
			menuIndex++;
			CandidatesGrid.ContextMenu.MenuItems.Add(menuIndex, new ZMenuItem(ResString.GetMultilingualString("UnapprovedTransactionAuthorisationForm|ApproveWithClaimForVarianceAmount", "Approve with Claim for Variance Amount"), new EventHandler(CandidatesGrid_ApproveInvoicesWithClaim_Click)));
			menuIndex++;
			CandidatesGrid.ContextMenu.MenuItems.Add(menuIndex, new ZMenuItem("-"));
			CandidatesGrid.ContextMenu.Popup += CandidatesGridContextMenu_Popup;
		}

		protected override void Dispose(bool disposing)
		{
			if (CandidatesGrid.ContextMenu != null)
			{
				CandidatesGrid.ContextMenu.Popup -= CandidatesGridContextMenu_Popup;
			}
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			DataSourceChanged -= new EventHandler(Form_DataSourceChanged);
			base.Dispose(disposing);
		}
		#endregion

		#region Event Handlers

		void zButtonApproveAll_Click(object sender, EventArgs e)
		{
			ApproveInvoice(false, false, false);
		}

		void zButtonCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		void CadidatesGrid_EditApproveTransactions_Click(object sender, EventArgs e)
		{
			ApproveInvoice(true, true, false);
		}

		void CadidatesGrid_EditApproveTransactionsWithClaim_Click(object sender, EventArgs e)
		{
			if (Env.Security.PayablesClaimsAndQueriesNew.IsAllowed)
			{
				ApproveInvoice(true, true, true);
			}
			else
			{
				Globals.Message.ShowError(Env.Security.PayablesClaimsAndQueriesNew.ErrorMessageForNotAllowed);
			}
		}

		void CandidatesGrid_ApproveInvoices_Click(object sender, EventArgs e)
		{
			ApproveInvoice(false, true, false);
		}

		void CandidatesGrid_ApproveInvoicesWithClaim_Click(object sender, EventArgs e)
		{
			if (Env.Security.PayablesClaimsAndQueriesNew.IsAllowed)
			{
				ApproveInvoice(false, true, true);
			}
			else
			{
				Globals.Message.ShowError(Env.Security.PayablesClaimsAndQueriesNew.ErrorMessageForNotAllowed);
			}
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (BusinessEntity != null)
			{
				BusinessEntity.Candidates.Load();
			}
			else
			{
				if (base.DataSource != null)
				{
					ErrorReporter.ReportOnce("UnapprovedTransactionAuthorisationForm.Factory_Saved", Res.GetString("3D9A907F-F7C0-4002-B93F-2145A1AFC1EE", "The type of Data Source is {0} .", base.DataSource.GetType().ToString()));
				}
				else
				{
					ErrorReporter.ReportOnce("UnapprovedTransactionAuthorisationForm.Factory_Saved", "DataSource is null.");
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		void CandidatesGridContextMenu_Popup(object sender, EventArgs e)
		{
			foreach (MenuItem menuItem in CandidatesGrid.ContextMenu.MenuItems)
			{
				if (menuItem.Text == "Documents")
				{
					CandidatesGrid.ContextMenu.MenuItems.Remove(menuItem);
					break;
				}
			}
		}
		#endregion

		#region Implementation

		List<InvoicingBase> GetSelectedObjectsAsInvoicingBaseWithSecurityOverrideProvider()
		{
			List<InvoicingBase> selectedInvoices = new List<InvoicingBase>();
			var commonSecurityProvider = new InvoicingSecurityOverrideProvider();
			foreach (BusinessObject bizO in CandidatesGrid.SelectedElements)
			{
				InvoicingBase transaction = bizO as InvoicingBase;
				SecurityOverrideProviderSource.Get(transaction).Provider = commonSecurityProvider;
				selectedInvoices.Add(transaction);
			}
			return selectedInvoices;
		}

		List<InvoicingBase> GetFirstObjectAsInvoicingBaseWithSecurityOverrideProvider()
		{
			List<InvoicingBase> selectedInvoices = new List<InvoicingBase>();
			var commonSecurityProvider = new InvoicingSecurityOverrideProvider();
			if (CandidatesGrid.List[0] != null)
			{
				InvoicingBase transaction = CandidatesGrid.List[0] as InvoicingBase;
				if (transaction == null)
				{
					ErrorReporter.ReportOnce("UnapprovedTransactionAuthorisationForm.GetFirstObjectAsInvoicingBaseWithSecurityOverrideProvider", string.Format("Expected Type is 'InvoicingBase'. But provided type is {0}", CandidatesGrid.List[0].GetType().ToString()));

					Globals.Message.ShowWarning(Res.GetString("9b1d3e4b-8deb-4e81-b484-fe5a0c66d7b7", "Unexpected error occurred - could not approve transaction. Please contact support."));
				}
				else
				{
					SecurityOverrideProviderSource.Get(transaction).Provider = commonSecurityProvider;
					selectedInvoices.Add(transaction);
				}
			}
			else
			{
				Globals.Message.ShowWarning(Res.GetString("0a7645ee-963e-4162-9dcd-ea97a3e90594", "Nothing to approve."));
			}
			return selectedInvoices;
		}

		void ApproveInvoice(bool withGui, bool onlySelected, bool withClaim)
		{
			if (!withGui)
			{
				Cursor = Cursors.WaitCursor;
			}
			try
			{
				bool areThereAnyInvalidTransactions = false;
				string additionalErrorInfo = "";
				List<InvoicingBase> approvedSelfBillingInvoices = new List<InvoicingBase>();

				IEnumerable transactions = GetTransactionsApproveInvoice(onlySelected);
				foreach (InvoicingBase invoice in transactions)
				{
					invoice.ShowError = Globals.Message.ShowError;
					ApproveInvoice(invoice, approvedSelfBillingInvoices, withGui, withClaim, ref areThereAnyInvalidTransactions, ref additionalErrorInfo);
				}

				additionalErrorInfo = "\r\n\r\n" + Res.GetString("dbb7f8a3-ab20-46f2-ab0f-b45c315085f8", "Reason: {0}", additionalErrorInfo);

				HandleInvalidTransactions(approvedSelfBillingInvoices, withGui, withClaim, areThereAnyInvalidTransactions, additionalErrorInfo);
			}
			finally
			{
				if (!withGui)
				{
					Cursor = Cursors.Default;
				}
			}
		}

		IEnumerable GetTransactionsApproveInvoice(bool onlySelected)
		{
			if (onlySelected)
			{
				List<InvoicingBase> selectedInvoices = GetSelectedObjectsAsInvoicingBaseWithSecurityOverrideProvider();
				if (selectedInvoices.Count == 0)
				{
					selectedInvoices = GetFirstObjectAsInvoicingBaseWithSecurityOverrideProvider();
				}
				return selectedInvoices;
			}
			else
			{
				return BusinessEntity.Candidates;
			}
		}

		bool IsHandledInvalidInvoice(InvoicingBase convertedInvoice, InvoicingBase invoice, bool withGui, bool withClaim, ref bool areThereAnyInvalidTransactions, ref string additionalErrorInfo)
		{
			Argument.NotNull(invoice, "Invoice");
			Argument.NotNull(convertedInvoice, "convertedInvoice");

			if (convertedInvoice == null)
			{
				areThereAnyInvalidTransactions = true;
				additionalErrorInfo = Res.GetString("dce9b4d2-3aa6-4291-964a-19f417ea38ff", "AP Invoice can't be created.");
				return true;
			}
			else if (!convertedInvoice.AH_GB.IsValid)
			{
				if (withGui)
				{
					StringBuilder message = new StringBuilder();

					message.AppendLine(Res.GetString("3ae3a9dc-df09-4728-81cd-1d3e39b5f22d", "An attempt to import an invoice with transaction number {0} organization {1} in company [{2}] branch [{3}] department [{4}] has failed because of the following error:",
											invoice.AH_TransactionNum,
											invoice.Header.OH_Code,
											invoice.Company.GC_Code,
											invoice.Branch.GB_Code,
											invoice.Department.GE_Code));

					message.AppendLine();
					message.AppendLine(Res.GetString("bbc30540-362f-4b0e-9b8e-6e49c2e06946", "Intercompany Invoice cannot be imported as Transaction Branch or Company cannot be set with reference to the invoice debtor organization proxy."));
					message.AppendLine();
					message.AppendLine(Res.GetString("1eb31f97-3e2a-4919-a392-399b00fb9717", "Do you want to set current branch instead?"));

					DialogResult result = Globals.Message.Show(message.ToString(),
						Res.GetString("5ee8e013-3093-44f0-b985-e0cd1b9e43b0", "Cannot calculate branch for invoice"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

					if (result == DialogResult.Yes)
					{
						convertedInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
						return false;
					}
					else
					{
						return true;
					}
				}
				else
				{
					areThereAnyInvalidTransactions = true;
					convertedInvoice.ReleaseAllMutexOnInvoice();
					additionalErrorInfo = Res.GetString("65f6a981-0f22-4570-b176-cfd208dd4b83", "Intercompany Invoice cannot be imported as Transaction Branch or Company cannot be set with reference to the invoice debtor organization proxy.");
					return true;
				}
			}
			else if (!withGui && convertedInvoice.HasErrors)
			{
				areThereAnyInvalidTransactions = true;
				convertedInvoice.ReleaseAllMutexOnInvoice();
				additionalErrorInfo = Res.GetString("329aa639-0497-4035-b119-d4574b140e48", "AP Invoice has validation errors.");
				return true;
			}
			else if (withClaim && !convertedInvoice.InitialiseApprovingWithClaim(invoice))
			{
				areThereAnyInvalidTransactions = true;
				additionalErrorInfo = Res.GetString("ba01d43e-2000-4228-bb78-bb6587b81a07", "This invoice can't be approved with claim.");
				return true;
			}
			return false;
		}

		void ApproveInvoice_WithGui(InvoicingBase convertedInvoice)
		{
			Argument.NotNull(convertedInvoice, "convertedInvoice");

			ZController aPController = null;
			if (convertedInvoice.AH_TransactionType == ZArchitecture.Core.TransactionTypes.UACreditNote ||
					convertedInvoice.AH_TransactionType == ZArchitecture.Core.TransactionTypes.CreditNote)
			{
				aPController = ZControllerFactory.Create(ControllerIDs.APCreditNote);
			}
			else
			{
				aPController = ZControllerFactory.Create(ControllerIDs.APInvoice);
			}
			aPController.SetFormsModalTo(this);

			if (convertedInvoice is UAInvoice || convertedInvoice is UACreditNote)
			{
				List<string> writableInvoicePropertiesNames;
				convertedInvoice.Lines.SetReadOnlyIncludingChildren(false);
				if (convertedInvoice.IsJobRelated)
				{
					List<string> writablePropertiesNames = new List<string>
										{ InvoicingLineBase.Schema.AL_OSExTaxAmount,
											InvoicingLineBase.Schema.AL_OSTaxAmount,
											InvoicingLineBase.Schema.AL_LocalExTaxAmount,
											InvoicingLineBase.Schema.AL_LocalTaxAmount };

					foreach (InvoicingLineBase line in convertedInvoice.Lines)
					{
						JobCharge jobCharge = convertedInvoice.Factory.LoadTop1<JobCharge>(new ZQuery(JobChargeSchema.JR_AL_APLine, line.PK));
						if (jobCharge != null)
						{
							line.ClearWritableProperties();
							line.AddWritableProperties(writablePropertiesNames.ToArray());
						}
					}

					writableInvoicePropertiesNames = new List<string>()
										{
											AccTransactionHeaderSchema.AH_PostDate.Name,
											AccTransactionHeaderSchema.AH_InvoiceDate.Name,
											AccTransactionHeaderSchema.AH_TransactionNum.Name,
											AccTransactionHeaderSchema.AH_DueDate.Name
										};
				}
				else
				{
					writableInvoicePropertiesNames = new List<string>();
					foreach (ZPropertyInfo property in convertedInvoice.ZPropertyInfoHash)
					{
						if (property.HasSetter)
						{
							writableInvoicePropertiesNames.Add(property.Name);
						}
					}
				}
				convertedInvoice.AddWritableProperties(writableInvoicePropertiesNames.ToArray());
				convertedInvoice.NeedToResetWritableProperties = true;
			}

			if (ShouldMakeFormReadOnly(convertedInvoice))
			{
				convertedInvoice.SetReadOnlyIncludingChildren(true);
			}
			IZForm aPForm = aPController.ShowFormForNewEntity(convertedInvoice);
			if (aPForm != null)
			{
				if (ShouldMakeFormReadOnly(convertedInvoice))
				{
					DisableFormControls((BaseInvoicingForm)aPForm);
				}

				aPForm.DisplayMode = ODisplayMode.Edit;
				convertedInvoice.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
			}
		}

		void ApproveInvoice_WithoutGui(InvoicingBase convertedInvoice, List<InvoicingBase> approvedSelfBillingInvoices, bool withClaim, ref bool areThereAnyInvalidTransactions, ref string additionalErrorInfo)
		{
			Argument.NotNull(convertedInvoice, "convertedInvoice");
			Argument.NotNull(approvedSelfBillingInvoices, "approvedSelfBillingInvoices");

			APAccQueryClaim claim = null;
			if (withClaim)
			{
				claim = convertedInvoice.CreateClaim(true) as APAccQueryClaim;
				if (claim == null || claim.HasErrors ||
					claim.RelatedUnapprovedCreditNote == null || claim.RelatedUnapprovedCreditNote.HasErrors)
				{
					areThereAnyInvalidTransactions = true;
					if (claim == null)
					{
						additionalErrorInfo = Res.GetString("281e410c-a496-461c-9161-0350260b35e4", "A claim can't be created. Please check appropriate security settings.");
					}
					else if (claim.HasErrors)
					{
						additionalErrorInfo = Res.GetString("7222d990-7608-4a32-9b89-b982074db9b6", "A claim has validation errors.");
					}
					else if (claim.RelatedUnapprovedCreditNote == null)
					{
						additionalErrorInfo = Res.GetString("d0997759-916b-4925-94a1-42958d81688b", "Claim related unapproved credit note can't be created.");
					}
					else if (claim.RelatedUnapprovedCreditNote.HasErrors)
					{
						additionalErrorInfo = Res.GetString("7c51f52d-312a-414b-91ce-938950d7bbbb", "Claim related unapproved credit note has validation errors.");
					}
					claim = null;
				}
			}
			if (!withClaim || claim != null)
			{
				if (convertedInvoice.CheckLevelSecurityRights())
				{
					convertedInvoice.RunPreSaveValidation();
					if (!convertedInvoice.HasErrors)
					{
						bool canContinue = DealWithClosedJobs(ref areThereAnyInvalidTransactions, ref additionalErrorInfo, convertedInvoice);

						if (canContinue)
						{
							List<BusinessObjectFactory> factoriesToSave = new List<BusinessObjectFactory>();
							if (claim != null && claim.RelatedUnapprovedCreditNote != null)
							{
								factoriesToSave.Add(claim.RelatedUnapprovedCreditNote.Factory);
							}
							if (claim != null && !factoriesToSave.Contains(claim.Factory))
							{
								factoriesToSave.Add(claim.Factory);
							}
							if (!factoriesToSave.Contains(convertedInvoice.Factory))
							{
								factoriesToSave.Add(convertedInvoice.Factory);
							}

							BusinessObjectFactory.SaveTogether(factoriesToSave.ToArray());

							if (convertedInvoice.IsSelfBillingInvoice && !approvedSelfBillingInvoices.Contains(convertedInvoice))
							{
								approvedSelfBillingInvoices.Add(convertedInvoice);
							}
						}
					}
					else
					{
						areThereAnyInvalidTransactions = true;
						additionalErrorInfo = Res.GetString("159b816e-b6bf-474f-bbef-98f984a7006c", "AP Invoice has validation errors.");
					}
				}
				else
				{
					areThereAnyInvalidTransactions = true;
					additionalErrorInfo = Res.GetString("a2da611e-5673-40af-960f-b4cde9dc623b", "There are no security right to approve the invoice.");
				}
			}
		}

		void ApproveInvoice(InvoicingBase invoice, List<InvoicingBase> approvedSelfBillingInvoices, bool withGui, bool withClaim, ref bool areThereAnyInvalidTransactions, ref string additionalErrorInfo)
		{
			Argument.NotNull(invoice, "Invoice");
			Argument.NotNull(approvedSelfBillingInvoices, "approvedSelfBillingInvoices");

			try
			{
				var convertedInvoice = BusinessEntity.ConvertToAP(invoice, false);
				if (!IsHandledInvalidInvoice(convertedInvoice, invoice, withGui, withClaim, ref areThereAnyInvalidTransactions, ref additionalErrorInfo))
				{
					convertedInvoice.SetComplianceSubTypeIfIsNecessary();
					if (withGui)
					{
						ApproveInvoice_WithGui(convertedInvoice);
					}
					else
					{
						ApproveInvoice_WithoutGui(convertedInvoice, approvedSelfBillingInvoices, withClaim, ref areThereAnyInvalidTransactions, ref additionalErrorInfo);
					}
				}
			}
			catch (JobCreationException ex)
			{
				Globals.Message.ShowError(ex.Message, Res.GetString("e6cab7b8-3637-4121-b4e9-bd547ab2defc", "Unable to convert invoice"));
			}
			catch (ZCannotSaveException ex)
			{
				if (withGui)
				{
					Globals.Message.ShowError(ex.Message, Res.GetString("44147671-7f67-418e-b57f-0d91976261a7", "Transaction saving error"));
				}
				else
				{
					areThereAnyInvalidTransactions = true;
					additionalErrorInfo = ex.Message;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		void HandleInvalidTransactions(List<InvoicingBase> approvedSelfBillingInvoices, bool withGui, bool withClaim, bool areThereAnyInvalidTransactions, string additionalErrorInfo)
		{
			Argument.NotNull(approvedSelfBillingInvoices, "approvedSelfBillingInvoices");

			if (withGui)
			{
				if (areThereAnyInvalidTransactions)
				{
					if (withClaim)
					{
						Globals.Message.ShowWarning(Res.GetString("37bb8704-5d9e-41df-8d67-54b329135e46", "Some invoices have not been approved with claim. \r\nThis operation is only for intercompany AR Invoices with Variance Approval Level greater then Maximum Variance Approval Level for the company issued the invoice.{0}",
							additionalErrorInfo));
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("0c7384b1-f47b-402c-9ecf-9f1bf9184853", "Transaction for approving can not be created.{0}",
							additionalErrorInfo));
					}
				}
			}
			else
			{
				if (areThereAnyInvalidTransactions)
				{
					if (withClaim)
					{
						Globals.Message.ShowWarning(Res.GetString("26638aa5-506a-4aa3-89ef-c235fb93547b", "Some invoices have not been approved with claim. \r\nThis option should only be used when approving sister company AR invoices with a required Variance Approval Level greater than that sister company's defined Maximum Variance Approval Level. \r\nPlease use 'Edit and Approve' to approve those invoices.{0}",
							additionalErrorInfo));
					}
					else
					{
						Globals.Message.ShowWarning(Res.GetString("6ea7dffc-a604-4886-a808-0d2f7156d2e3", "There are some transactions with incorrect data. Please use 'Edit and Approve those invoices' to finalize the operation for them.{0}",
							additionalErrorInfo));
					}
				}

				if (approvedSelfBillingInvoices.Count > 0)
				{
					DialogResult result = Globals.Message.Show(Res.GetString("58f071dd-1ea2-43d0-8e40-fb1958f8ba08", "You have approved Self Billing Invoices or Credit Notes.\r\nDo you want to print these Self Billing Invoices or Credit Notes?"),
					Res.GetString("9cf2f9f5-7c4c-45ce-aa7f-333beb1730a7", "Printing Self Billing Invoices and Credit Notes"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (result == DialogResult.Yes)
					{
						new InvoicePrintTask(new InvoicePrintTask.Configuration(approvedSelfBillingInvoices.ToArray())).Run();
					}
				}

				if (BusinessEntity != null)
				{
					BusinessEntity.Candidates.Load();
				}
				else
				{
					if (base.DataSource != null)
					{
						ErrorReporter.ReportOnce("UnapprovedTransactionAuthorisationForm.HandleInvalidTransactions", Res.GetString("3D9A907F-F7C0-4002-B93F-2145A1AFC1EE", "The type of Data Source is {0} .", base.DataSource.GetType().ToString()));
					}
					else
					{
						ErrorReporter.ReportOnce("UnapprovedTransactionAuthorisationForm.HandleInvalidTransactions", "DataSource is null.");
					}
				}
			}
		}

		static bool DealWithClosedJobs(ref bool areThereAnyInvalidTransactions, ref string additionalErrorInfo, InvoicingBase convertedInvoice)
		{
			if (convertedInvoice.HasClosedJob)
			{
				var originalProvider = convertedInvoice.SecurityOverrideProvider;
				var unapprovedTransactionSecurityProvider = (ISecurityOverrideProvider)new InvoicingSecurityOverrideProvider();
				convertedInvoice.SecurityOverrideProvider = unapprovedTransactionSecurityProvider;
				try
				{
					if (JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(convertedInvoice, convertedInvoice.RelatedJobsForReversing))
					{
						convertedInvoice.ReOpenClosedJob();
						return true;
					}
					else
					{
						areThereAnyInvalidTransactions = true;
						additionalErrorInfo = Env.Security.ReopenJob.ErrorMessageForNotAllowed;
						return false;
					}
				}
				finally
				{
					convertedInvoice.SecurityOverrideProvider = originalProvider;
				}
			}
			else
			{
				return true;
			}
		}

		void DisableFormControls(BaseInvoicingForm apForm)
		{
			if (apForm != null)
			{
				apForm.InvoiceDetails.ApportionChargesButton.Enabled = false;
				apForm.InvoiceDetails.BulkChargeImportButton.Enabled = false;
				apForm.LineChargesGrid.Enabled = false;

				if (apForm.AutoAllocateDiscrepancyMenuItem != null)
				{
					apForm.AutoAllocateDiscrepancyMenuItem.Enabled = false;
				}

				if (apForm.SaveAsIncompleteMenuItem != null)
				{
					apForm.SaveAsIncompleteMenuItem.Enabled = false;
				}

				var invoiceForm = apForm as InvoiceForm;
				if (invoiceForm != null)
				{
					invoiceForm.CashInvoiceOnCheckbox.Enabled = false;
				}
			}
		}

		bool ShouldMakeFormReadOnly(InvoicingBase invoice)
		{
			return !Env.Security.APUnapprovedInvoicesAllowEditWhenImportSisterCoInv.IsAllowed && invoice.IsConvertedFromARInvoice;
		}

		#endregion
	}
}
