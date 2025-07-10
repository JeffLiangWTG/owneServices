using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals
{
	public class FCBAdjustmentJournalLineValidation : GLJournalLineValidation
	{
		public FCBAdjustmentJournalLineValidation(FCBAdjustmentJournalLine parent)
			: base(parent)
		{
		}

		new FCBAdjustmentJournalLine Parent
		{
			get { return (FCBAdjustmentJournalLine)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			var errorMessage = Res.GetString("d8eb188c-9c04-4de2-a30a-41ad685d31dc", "At least one line should be posted against '{0}'.", AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.Caption);

			Parent.RemoveRowError(errorMessage);
			if (Parent.ForeignCurrencyJournalHeader != null && Parent.ForeignCurrencyJournalHeader.GLJournalLines.FirstOrDefault(x => ((FCBAdjustmentJournalLine)x).IsUnrealizedLocalAmountLine) == null)
			{
				Parent.AddRowError(errorMessage);
			}
		}

		protected override void CheckUnsignedOSLineAmount()
		{
			if (Parent.IsUnrealizedLocalAmountLine)
			{
				base.CheckUnsignedOSLineAmount();
			}
		}

		protected override void CheckAL_OSExTaxAmount()
		{
			TypeValidation.CheckValidDecimal(Parent.AL_OSExTaxAmountInfo, 19, 4);
			if (Parent.IsUnrealizedLocalAmountLine)
			{
				base.CheckAL_OSExTaxAmount();
			}
		}

		protected override void CheckAL_RX_NKTransactionCurrency()
		{
			base.CheckAL_RX_NKTransactionCurrency();

			if (!Parent.AL_RX_NKTransactionCurrencyInfo.HasErrors())
			{
				if (Parent.IsUnrealizedLocalAmountLine && Parent.AL_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					Parent.AL_RX_NKTransactionCurrencyInfo.AddError(Res.GetString("3988af73-4c70-4fbe-af09-3892fcc5126a", "Please select '{0}' as currency."
						, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency));
				}

				if (Parent.IsForeignCurrencyBalanceAdjustmentLine && Parent.AL_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					Parent.AL_RX_NKTransactionCurrencyInfo.AddError(Res.GetString("37c0e8a4-564a-4339-86e3-870c5e5eee2d", "Please select a Foreign currency."));
				}
			}
		}

		protected override void CheckAL_AG()
		{
			base.CheckAL_AG();

			if (AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.Value == Guid.Empty)
			{
				Parent.AL_AGInfo.AddError(Res.GetString("760e9bca-7330-45df-bb42-8587d977c711", @"Please set up a correct value for the Registry Item: {0}/{1}."
					, AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.Category
					, AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.Caption));
			}
		}

		protected override void CheckUnsignedLocalLineAmount()
		{
			if (Parent.IsUnrealizedLocalAmountLine)
			{
				base.CheckUnsignedLocalLineAmount();
			}
		}
	}
}
