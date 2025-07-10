using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals
{
	public class FCBAdjustmentJournal : GLJournal
	{
		public FCBAdjustmentJournal(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overridden Members

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			AH_ReceiptType = ReceiptTypes.ForeignCurrencyBalance;
			AH_Desc = AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(AH_Ledger + AH_ReceiptType, "");
		}

		public override IRegistryItem BalancingAccount
		{
			get { return AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount; }
		}

		public override GLJournalLineCollection GLJournalLines
		{
			get { return (FCBAdjustmentJournalLineCollection)Lines; }
		}

		protected override GLJournalLineCollection GetDependentLinesCollectionCore(ZQuery orderByQuery)
		{
			return new FCBAdjustmentJournalLineCollection(this, orderByQuery);
		}

		public override Type DependentTransactionLineType
		{
			get { return typeof(FCBAdjustmentJournalLine); }
		}

		protected override bool AH_TransactionType_ReadOnly
		{
			get { return true; }
		}

		public override bool AH_TransactionCategory_ReadOnly
		{
			get { return true; }
		}

		protected override bool ShouldCreateEDocOnSaving
		{
			get { return false; }
		}

		#endregion
	}
}
