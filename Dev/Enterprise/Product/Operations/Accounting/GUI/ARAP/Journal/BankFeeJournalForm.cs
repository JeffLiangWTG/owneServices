using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using FormWindowState = System.Windows.Forms.FormWindowState;
using JournalBase = Enterprise.Accounting.Business.ARAP.Journal.Journal;

namespace Enterprise.Accounting.GUI.ARAP.Journal
{
	public partial class BankFeeJournalForm : JournalBaseForm, IDoDisplayModeBrowseOverride, IDoDisplayModeNewOverride, IDoDisplayModeEditOverride
	{
		public BankFeeJournalForm(JournalBase bO)
			: base(bO)
		{
			HasBeenSaved = false;

			ZFormMenuStrategy.SetMenuItemText(this, ZFormMenuStrategy.FileSaveAndCloseMenuItemName, Res.GetString("TransactionViewForm|1C34F4CB-487B-4715-8735-603D369F776F", "&Post"));
		}

		// Deletes the unsaved TransactionHeader 
		void DeleteUnsavedRow()
		{
			Header.DeleteFromDB();
		}

		// 'Saves' the unsaved Journal (i.e. pass the Header to MatchingBase)
		void RetainUnsavedRow(JournalBase header)
		{
			Header.SetOSPartialPaymentReadOnly();
			((IMatching)Header).LocalPartialPaymentAmountInfo.RefreshBinding();
			if (IsCurrentContextMatching)
			{
				((IMiscellaneousTransaction)header).MatchingBizO.AddMiscellaneousTransaction(header);
			}
			HasBeenSaved = true;
		}

		JournalBase Header
		{
			get { return BusinessEntity as JournalBase; }
		}

		// true for new transactions not referenced by MatchingBase
		bool IsNewUnsavedObject
		{
			get
			{
				if (IsCurrentContextMatching)
				{
					return !((IMiscellaneousTransaction)Header).MatchingBizO.ContainsMiscellaneousTransaction(Header);
				}
				return false;
			}
		}

		bool IsCurrentContextMatching
		{
			get { return ((IMiscellaneousTransaction)Header).MatchingBizO != null; }
		}

		// set to true if a reference to this transaction is given to MatchingBase
		bool HasBeenSaved { get; set; }

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			bool shouldSaveCurrent = false;
			bool shouldDeleteHeader = false;
			bool shouldDeleteMisc = false;

			if (DisplayMode == ODisplayMode.New)
			{
				shouldDeleteHeader = !HasBeenSaved;
			}
			else if (DisplayMode == ODisplayMode.Edit)
			{
				if (Header.AH_LocalExTaxAmount == 0)
				{
					shouldDeleteMisc = true;
					shouldDeleteHeader = true;
				}
				else if (IsNewUnsavedObject)
				{
					if (WindowState == FormWindowState.Minimized)
					{
						WindowState = FormWindowState.Normal;
					}

					BringToFront();
					DialogResult cancelOptions = Globals.Message.Show(FormClosingQuestion, Res.GetString("1cafd328-f03f-4fba-b9b1-f98b6db61283", "Warning"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

					switch (cancelOptions)
					{
						case DialogResult.Yes:
							shouldSaveCurrent = true;
							break;
						case DialogResult.Cancel:
							e.Cancel = true;
							break;
						case DialogResult.No:
							shouldDeleteHeader = true;
							break;
					}
				}
			}

			if (shouldDeleteMisc && IsCurrentContextMatching)
			{
				((IMiscellaneousTransaction)Header).MatchingBizO.DeleteMiscTransaction(Header);
			}

			if (shouldDeleteHeader)
			{
				DeleteUnsavedRow();
			}

			if (shouldSaveCurrent)
			{
				HandleSaveWhileClosing(e);
			}
		}

		void IDoDisplayModeNewOverride.DoDisplayModeNew()
		{
			ZFormStrategy.DoDisplayModeNew(this); // Call base
			fPostButton.Enabled = true;
			fPostButton.Text = Res.GetString("TransactionViewForm|4730B496-7651-4d5f-80E5-AD3B9401DE61", "Add");
			fCancelButton.Visible = true;
			fCancelButton.Enabled = true;
		}

		void IDoDisplayModeBrowseOverride.DoDisplayModeBrowse()
		{
			ZFormStrategy.DoDisplayModeBrowse(this); // Call base
			fPostButton.Visible = false;
			fPostButton.Enabled = false;
			fCancelButton.Text = Res.GetString("TransactionViewForm|0EB4857F-70C6-4372-AA51-E31750657708", "Save");
		}

		void IDoDisplayModeEditOverride.DoDisplayModeEdit()
		{
			if (!IsNewUnsavedObject)
			{
				DisplayMode = ODisplayMode.Browse;
			}
			fPostButton.Text = Res.GetString("TransactionViewForm|1B278710-745F-4087-8F4B-FD67405D7CC7", "Add");
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
					if (Header.AH_LocalExTaxAmount != 0)
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

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}

