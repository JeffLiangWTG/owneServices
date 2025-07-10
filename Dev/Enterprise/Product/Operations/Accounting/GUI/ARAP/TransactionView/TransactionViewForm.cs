using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using BusinessContext = Enterprise.Integration.Accounting.BusinessContext;

namespace Enterprise.Accounting.GUI.ARAP.TransactionView
{
	public partial class TransactionViewForm : ZForm, IDoDisplayModeBrowseOverride, IDoDisplayModeNewOverride, IDoDisplayModeEditOverride
	{
		public TransactionViewForm(TransactionHeader bO) : base(bO)
		{
			Header = bO;
			ZFormPostingButtonsStrategy.SetupPosting(this, oPostingButtonsUserControl1);
			fCancelButton.Click += new EventHandler(fCancelButton_Click);
			HasBeenSaved = false;
			AH_NumberOfSupportingDocumentsCalcEdit.Visible = bO.AH_NumberOfSupportingDocumentsVisible_ReadOnly;
			ZFormMenuStrategy.SetMenuItemText(this, ZFormMenuStrategy.FileSaveAndCloseMenuItemName, Res.GetString("TransactionViewForm|C794B7F0-FB91-41a6-8D97-2FA67651AD4F", "&Post"));

			var dataExportBatchSource = BusinessEntity as IDataExportBatchSource;
			if (dataExportBatchSource != null && dataExportBatchSource.IsDataExportBatchSupported)
			{
				PlugIns.Add(ControllerIDs.DataExportBatchPlugin);
			}
			if (ShowAuditTab)
			{
				PlugIns.Add(ControllerIDs.Audit);
			}
		}

		public override string FormCaption
		{
			get
			{
				string result = "";
				if (Header != null)
				{
					string ledger = Header.AH_Ledger;
					switch (Header.AH_TransactionType)
					{
						case TransactionTypes.Overpayment:
							result = Res.GetString("TransactionViewForm|5648BB14-A97B-4929-A477-8BB33E277C68", "Overpayment");
							break;

						case TransactionTypes.Discount:
							result = Res.GetString("TransactionViewForm|62427353-E449-4482-B37B-D6EC2481BE87", "Discount");
							break;

						case TransactionTypes.ExchangeDifference:
							result = Res.GetString("TransactionViewForm|1070EF4C-ADFF-4c6b-810A-6DBE0AE9FB42", "Exchange difference");
							break;
					}
					result = new LedgerTypesList().GetMultilingualDescriptionFromCode(ledger) + " " + result;
				}

				return result;
			}
		}

		public override string FormVerb
		{
			get
			{
				return Header.IsInDatabase ? FormVerbs.View : base.FormVerb;
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		// Deletes the unsaved TransactionHeader
		void DeleteUnsavedRow()
		{
			Header.DeleteFromDB();
		}

		// 'Saves' the unsaved TransactionHeader (i.e. pass the Header to MatchingBase)
		void RetainUnsavedRow(TransactionHeader header)
		{
			Header.SetOSPartialPaymentReadOnly();
			((IMatching)Header).LocalPartialPaymentAmountInfo.RefreshBinding();
			if (IsCurrentContextMatching)
			{
				((IMiscellaneousTransaction)header).MatchingBizO.AddMiscellaneousTransaction(header);
			}
			HasBeenSaved = true;
		}

		// true for new transactions not referenced by MatchingBase
		ZBool IsNewUnsavedObject
		{
			get
			{
				if (IsCurrentContextMatching)
				{
					return !((IMiscellaneousTransaction)Header).MatchingBizO.ContainsMiscellaneousTransaction(Header);
				}
				else
				{
					return false;
				}
			}
		}

		ZBool IsCurrentContextMatching
		{
			get { return (Header is IMiscellaneousTransaction) && ((IMiscellaneousTransaction)Header).MatchingBizO != null; }
		}

		// set to true if a reference to this transaction is given to MatchingBase
		ZBool HasBeenSaved
		{
			get { return fHasBeenSaved; }
			set { fHasBeenSaved = value; }
		}
		ZBool fHasBeenSaved;

		void fCancelButton_Click(object sender, EventArgs e)
		{
			if (!Header.IsInDatabase && DisplayMode == ODisplayMode.New)
			{
				DeleteUnsavedRow();
			}
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			if (Header.AH_LocalExTaxAmount == 0)
			{
				if (IsCurrentContextMatching)
				{
					((IMiscellaneousTransaction)Header).MatchingBizO.DeleteMiscTransaction(Header);
				}
				DeleteUnsavedRow();
			}
			else
			{
				if (DisplayMode == ODisplayMode.Edit && IsNewUnsavedObject)
				{
					if (WindowState == FormWindowState.Minimized)
					{
						WindowState = FormWindowState.Normal;
					}

					BringToFront();

					DialogResult cancelOptions = Globals.Message.Show(FormClosingQuestion, Res.GetString("fa951e63-b6ca-4900-812c-5bc2f165cac6", "Warning"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

					switch (cancelOptions)
					{
						case DialogResult.Yes:
							HandleSaveWhileClosing(e);
							break;
						case DialogResult.Cancel:
							e.Cancel = true;
							break;
						case DialogResult.No:
							if (IsNewUnsavedObject)
							{
								DeleteUnsavedRow();
							}
							break;
					}
				}
				else if (IsNewUnsavedObject && !HasBeenSaved)
				{
					DeleteUnsavedRow();
				}
			}
		}

		void IDoDisplayModeNewOverride.DoDisplayModeNew()
		{
			ZFormStrategy.DoDisplayModeNew(this); // Call base
			fPostButton.Enabled = true;
			fPostButton.Text = Res.GetString("TransactionViewForm|1C01C1E8-6A75-4580-B080-4F22CD9E9A0D", "Add");
			fCancelButton.Visible = true;
			fCancelButton.Enabled = true;
		}

		void IDoDisplayModeBrowseOverride.DoDisplayModeBrowse()
		{
			fPostButton.Visible = false;
			fPostButton.Enabled = false;

			if (Header.Factory.HasContext(BusinessContext.EditingPaymentApprovalBatch))
			{
				fApplyButton.Visible = false;
				fApplyButton.Enabled = false;

				fCancelButton.Text = EditText;
				fCancelButton.Visible = true;
				fCancelButton.Enabled = true;
			}
			else if (IsNewUnsavedObject)
			{
				ZFormStrategy.DoDisplayModeBrowse(this); // Call base
				fCancelButton.Text = Res.GetString("TransactionViewForm|C72442AC-5B9C-4c0c-B2AB-03B998518A55", "Save");
			}
			else if (!IsCurrentContextMatching)
			{
				ZFormStrategy.DoDisplayModeBrowse(this); // Call base
			}
		}

		void IDoDisplayModeEditOverride.DoDisplayModeEdit()
		{
			if (!IsNewUnsavedObject)
			{
				DisplayMode = ODisplayMode.Browse;
			}

			if (IsCurrentContextMatching)
			{
				if (Header.Factory.HasContext(BusinessContext.EditingPaymentApprovalBatch))
				{
					fPostButton.Visible = false;
					fPostButton.Enabled = false;

					fApplyButton.Visible = false;
					fApplyButton.Enabled = false;

					fCancelButton.Text = EditText;
					fCancelButton.Visible = true;
					fCancelButton.Enabled = true;
				}
				else
				{
					fPostButton.Text = Res.GetString("TransactionViewForm|0DAAF93E-15D6-4867-887A-142BAB465378", "Add");
				}
			}
		}

		// same as the ValidateAndSave in ZWinForm but does not call SaveInternal
		protected override ContinueWithSave ValidateAndSave()
		{
			bool oldEnabled = Enabled;
			ContinueWithSave result = ContinueWithSave.No;

			try
			{
				if (Enabled)
				{
					Enabled = false;
				}

				BusinessEntityForValidation.RunPreSaveValidation();
				TabPageNotificationsExposer.ExposeTabPageNotifications(this, BusinessEntity);
				if (BusinessEntityForValidation.HasErrors())
				{
					ShowErrorsDialog();
				}
				else if (ShowPreSaveDialogs() == ContinueWithSave.Yes)
				{
					if (!(Header.Notes.HasRelatedNotes && !Header.Logs.HasUserReadNotes()))
					{
						result = ContinueWithSave.Yes;
					}
				}
				if (result == ContinueWithSave.Yes)
				{
					if (Header is IMatching && Header.AH_LocalExTaxAmount != 0)
					{
						RetainUnsavedRow(Header);
					}
					DisplayMode = ODisplayMode.New;
				}
			}
			finally
			{
				Enabled = oldEnabled;
			}
			return result;
		}

		string EditText => Res.GetString("TransactionViewForm|C1B2EDF1-5C2C-4D6C-85D0-276EAECA84BC", "Edit");

		bool ShowAuditTab => true;
	}
}
