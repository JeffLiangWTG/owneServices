using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Guarantees
{
	public partial class GuaranteeTransactionFilterControl : Customs.GUI.Guarantees.GuaranteeTransactionFilterControl
	{
		public GuaranteeTransactionFilterControl(CusGuaranteeLineTransactionCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject) : base(gridCollection, filterStripBusinessObject)
		{
			SetUpTransactionGridContextMenu();
		}
		public new CusGuaranteeLineTransactionCollection gridCollection => (CusGuaranteeLineTransactionCollection)base.gridCollection;

		void SetUpTransactionGridContextMenu()
		{
			var writeOffTransactionMenu = new ZMenuItem(ResString.GetMultilingualString("4A81D234-270B-46D2-8B3C-02AE3FCAC129", "Write-off transaction"));
			writeOffTransactionMenu.Click += WriteOffTransactionClick;
			Grid.ContextMenu.MenuItems.Add(writeOffTransactionMenu);
		}

		void WriteOffTransactionClick(object sender, EventArgs ev)
		{
			if (Grid.SelectedElements.Length == 0)
			{
				Globals.Message.Show(SelectRowFirstMessage);
			}
			else
			{
				var gridSelectedElementsGroupByReference = Grid.SelectedElements.Cast<CusGuaranteeLineTransaction>().Where(x => x.CPL_TransactionType != Customs.Business.PermitTransactionTypeList.Codes.OBL && x.CPL_TranValue < ZDecimal.Zero).
					GroupBy(x => x.CPL_Reference).Select(x => new { References = x.Key, WriteOffTotalAmount = Math.Abs(x.Sum(t => t.CPL_TranValue)) });
				if (gridSelectedElementsGroupByReference.Any())
				{
					var header = this.gridCollection.GuaranteeHeader;
					var totalPendingData = header.ConfirmedCusGuaranteeLineTransactions.GroupBy(x => x.CPL_Reference).ToDictionary(x => x.Key, x => x.Sum(t => t.CPL_TranValue));
					foreach (var cusGuaranteeTransactionLineGrouped in gridSelectedElementsGroupByReference)
					{
						var reference = cusGuaranteeTransactionLineGrouped.References;
						var totalPendingAmountFromRelatedConfirmedLineTransactions = decimal.Zero;
						totalPendingData.TryGetValue(reference, out totalPendingAmountFromRelatedConfirmedLineTransactions);
						if (totalPendingAmountFromRelatedConfirmedLineTransactions > 0)
						{
							Globals.Message.ShowError(PendingAmountLessThanZeroMessage(reference, totalPendingAmountFromRelatedConfirmedLineTransactions));
						}
						else if (totalPendingAmountFromRelatedConfirmedLineTransactions < 0)
						{
							var writeOffAmount = cusGuaranteeTransactionLineGrouped.WriteOffTotalAmount <= Math.Abs(totalPendingAmountFromRelatedConfirmedLineTransactions) ? cusGuaranteeTransactionLineGrouped.WriteOffTotalAmount : Math.Abs(totalPendingAmountFromRelatedConfirmedLineTransactions);
							WriteOffTransaction(header, reference, writeOffAmount);
						}
					}
				}
			}
		}

		void WriteOffTransaction(CusGuaranteeHeader guaranteeHeader, ZString reference, ZDecimal writeOffAmount)
		{
			var writeOffDate = GetRequestDateForm(WriteOffTextMessage(reference, writeOffAmount), ZDateTime.Now);
			if (!writeOffDate.IsEmpty)
			{
				guaranteeHeader.AddWriteOffTransaction(reference, writeOffAmount, writeOffDate);
			}
		}

		protected virtual ZDateTime GetRequestDateForm(ZString message, ZDateTime defaultDate) => RequestDateFormHelper.GetDateFromRequestDateForm(WriteOffFormTittle, message, defaultDate);

		ZString SelectRowFirstMessage => ResString.GetMultilingualString("149F2742-EC28-498A-8A7D-BC6C6304185A", "Please select a row first");

		ZString PendingAmountLessThanZeroMessage(ZString reference, ZDecimal pendingAmount) => Res.GetString("649993DB-4145-444C-A4CB-0474E3E27C20",
			@"Reference {0} has a positive balance of {1} EUR. Please, check the existing transactions for this reference and create a manual adjustment if needed.", reference, pendingAmount.ToString("N2"));

		ResourceString WriteOffFormTittle => ResString.GetMultilingualString("D6C8AE15-A5DC-44F0-BB61-96C04E29FEFE", "Write Off Transaction");

		ZString WriteOffTextMessage(ZString reference, ZDecimal writeOffAmount) => Res.GetString("D547C6DA-B5C5-42A0-9A80-643340C93A42",
			@"You have selected to write-off debt for reference {0}. An automatic
transaction will be created to add {1} EUR to the guarantee’s balance.

To continue with this action, please enter the write-off date:", reference, writeOffAmount.ToString("N2"));
	}
}
