using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Unmatching;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class ViewMatchGroupForm : ZForm, IButtonDeleteTextOverride
	{
		public ViewMatchGroupForm(UnmatchingRow matchGroupBizO) : base(matchGroupBizO)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, oPostingButtonsUserControl1);
		}

		ZPanel TopPanel;
		ZDateEdit MatchDateDateEdit;
		ZArchitecture.ZTextBox MatchGroupTextBox;
		ZPanel BottomPanel;
		ZPostingButtonsUserControl oPostingButtonsUserControl1;
		ZDateEdit UnmatchDateEdit;

		UnmatchingRow MatchGroup
		{
			get { return BusinessEntity as UnmatchingRow; }
		}

		string IButtonDeleteTextOverride.DeleteButtonText
		{
			get { return Res.GetString("ViewMatchGroupForm|830C91EC-8156-4bf5-81D1-87F91AE146E1", "Unmatch"); }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormVerb
		{
			get { return DisplayMode == ODisplayMode.Delete ? ((IButtonDeleteTextOverride)this).DeleteButtonText : base.FormVerb; }
		}

		protected override DialogResult ShowConfirmationForDelete()
		{
			string question = Res.GetString("44741150-63b9-4f84-8d09-c570dffd44bf", "You are about to unmatch this match group. Do you want to proceed?");
			return Globals.Message.Show(question, Res.GetString("8a91904c-1b98-4116-a0de-ee44963cbce8", "Unmatch Confirmation"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No);
		}

		protected override void Delete()
		{
			base.Delete();

			var transactions = MatchGroup.MatchedTransactions.Cast<AccTransactionHeader>().Where(x =>
				(x.AH_Ledger == LedgerTypes.AccountsReceivable || x.AH_Ledger == LedgerTypes.AccountsPayable) &&
				(x.AH_TransactionType == TransactionTypes.Discount || x.AH_TransactionType == TransactionTypes.Journal ||
				 x.AH_TransactionType == TransactionTypes.ExchangeDifference || x.AH_TransactionType == TransactionTypes.Overpayment));

			if (AccountingConfigurationRegistry.Instance.PopupUnMatchTransactionDescriptionOverrideOnUnMatching.Value && transactions.Any())
			{
				OverrideTransactionDescription(transactions.Cast<IReversing>().Where(x => x.ReverseTransaction != null).Select(x => x.ReverseTransaction.PK).ToArray());
			}
		}

		protected override ContinueWithDelete ShowPreDeleteDialogs()
		{
			ContinueWithDelete result = ContinueWithDelete.No;

			foreach (var checkpoint in MatchGroup.CheckpointsToUnmatch)
			{
				if (!checkpoint.IsAllowed)
				{
					checkpoint.ShowError();
					return ContinueWithDelete.No;
				}
			}

			UnmatchingResult unmatchResult = MatchGroup.CanUnmatchThisMatchGroup;
			if (unmatchResult > UnmatchingResult.DataError)
			{
				switch (unmatchResult)
				{
					case UnmatchingResult.DataErrorAddingMatchAmountDecreaseAbsValueOfOutstandingAmount:
						Globals.Message.ShowError(Res.GetString("479c27d1-46d6-4275-8760-dcef1d8ed473", "Data Error occurred while trying to unmatch match group {0}. Adding match amount decreases absolute value of outstanding amount.", MatchGroup.MatchGroupNum), Res.GetString("e7970244-c4a9-438a-b8fb-2b30cbbdb49a", "Unmatch Transactions"));
						break;
					case UnmatchingResult.DataErrorAddingMatchAmountExceedOriginalInvoiceAmount:
						Globals.Message.ShowError(Res.GetString("8dd28f12-3a46-438b-89c4-1440b3da47e0", "Data Error occurred while trying to unmatch match group {0}. Adding match amount exceeds original invoice amount.", MatchGroup.MatchGroupNum), Res.GetString("e7970244-c4a9-438a-b8fb-2b30cbbdb49a", "Unmatch Transactions"));
						break;
					case UnmatchingResult.DataErrorInvoiceAmountAndMatchLinkAmountHasOppositeSigns:
						Globals.Message.ShowError(Res.GetString("ecfdd1b3-5615-4643-a219-734844187254", "Data Error occurred while trying to unmatch match group {0}. Both invoice amount and match amount should either be positive or negative.", MatchGroup.MatchGroupNum), Res.GetString("e7970244-c4a9-438a-b8fb-2b30cbbdb49a", "Unmatch Transactions"));
						break;
					case UnmatchingResult.DataErrorTransactionOrganisationIsInactive:
						Globals.Message.ShowError(Res.GetString("ed6fbc7d-fb6d-4ba4-ba9e-3c9e634e8e30", "Data Error occurred while trying to unmatch match group {0}. The associated Organization is inactive.", MatchGroup.MatchGroupNum), Res.GetString("e7970244-c4a9-438a-b8fb-2b30cbbdb49a", "Unmatch Transactions"));
						break;
				}
			}
			else
			{
				string caption = Res.GetString("e7970244-c4a9-438a-b8fb-2b30cbbdb49a", "Unmatch Transactions");
				string secondCashAdvanceMessageLine = Res.GetString("cb8a93b3-995c-40c1-a7b6-b3badbb17d8f", "The transaction with the paid Advance Payment Request must be reversed to unmatch a journal with one of these categories.");
				var stringBuilder = new ZStringBuilder();
				switch (unmatchResult)
				{
					case UnmatchingResult.Success:
						{
							MatchGroup.RunPreSaveValidation();
							if (MatchGroup.HasErrors)
							{
								ShowErrorsDialog();
							}
							else
							{
								result = base.ShowPreDeleteDialogs();
							}
						}
						break;
					case UnmatchingResult.ContainsPayment:
						{
							string message = Res.GetString("abc3be4f-fed5-4ac8-ab20-b0efdbbd0ba4", "{0} Unmatch Payment Matching.", SecurityCore.SecurityErrorMessage);
							Globals.Message.ShowInformation(message, caption);
							result = ContinueWithDelete.No;
						}
						break;
					case UnmatchingResult.ContainsCashAdvanceARJournal:
						{
							stringBuilder.AppendLine(Res.GetString("18c2de3e-9fee-4e01-9aa6-e56056b58425", "This group cannot be unmatched because it contains an AR journal with a category of CAR or CAI, which is matched with a transaction with a paid Advance Payment Request."));
							stringBuilder.Append(secondCashAdvanceMessageLine);
							Globals.Message.ShowInformation(stringBuilder.ToString(), caption);
							result = ContinueWithDelete.No;
						}
						break;
					case UnmatchingResult.ContainsCashAdvanceAPJournal:
						{
							stringBuilder.AppendLine(Res.GetString("45eeeee6-c27c-4e6a-924c-40a4d48307b8", "This group cannot be unmatched because it contains an AP journal with a category of CAP or CAI, which is matched with a transaction with a paid Advance Payment Request."));
							stringBuilder.Append(secondCashAdvanceMessageLine);
							Globals.Message.ShowInformation(stringBuilder.ToString(), caption);
							result = ContinueWithDelete.No;
						}
						break;
				}
			}
			return result;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UnmatchDateEdit.Visible = DisplayMode == ODisplayMode.Delete;
		}

		protected override void SetReadOnlyIncludingChildren()
		{
			if (DisplayMode == ODisplayMode.Delete)
			{
				MatchGroup.ReadOnly = false;
				MatchGroup.AddWritableProperties(new string[] { nameof(MatchGroup.UnmatchDate) });
			}
			else
			{
				base.SetReadOnlyIncludingChildren();
			}
		}

		void OverrideTransactionDescription(ZGuid[] transactions)
		{
			var caption = Res.GetData("OverrideTransactionDescriptionForm|16F1FC6D-547B-4630-BBB7-7EDB4AF4C054", "Override Un-matched Transaction Description");
#if DEBUG
			var form = new OverrideTransactionDescriptionForm(new OverrideTransactionDescriptionHelper(new BusinessObjectFactory(), transactions), caption);
			ZFormModaliser.ShowDialogWithoutDispose(form);
#else
			using (var form = new OverrideTransactionDescriptionForm(new OverrideTransactionDescriptionHelper(new BusinessObjectFactory(), transactions), caption))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
#endif
		}

		#region IDisposable Members

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}

