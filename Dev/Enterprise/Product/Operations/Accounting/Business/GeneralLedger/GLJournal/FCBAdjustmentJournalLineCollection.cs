using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals
{
	public class FCBAdjustmentJournalLineCollection : GLJournalLineCollection
	{
		public FCBAdjustmentJournalLineCollection(FCBAdjustmentJournal @object, ZQuery query)
			: base(@object, query)
		{
		}

		public FCBAdjustmentJournalLineCollection(FCBAdjustmentJournal @object)
			: this(@object, new ZQuery())
		{
		}

		protected override void SetDefaultsForNewChildCore(BusinessObject child)
		{
			base.SetDefaultsForNewChildCore(child);

			FCBAdjustmentJournalLine newLine = child as FCBAdjustmentJournalLine;
			FCBAdjustmentJournalLine previousItem = (FCBAdjustmentJournalLine)GetSecondLastItemInCollection();
			if (previousItem == null)
			{
				newLine.AL_AG = AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.Value;
				newLine.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}
		}
	}
}
