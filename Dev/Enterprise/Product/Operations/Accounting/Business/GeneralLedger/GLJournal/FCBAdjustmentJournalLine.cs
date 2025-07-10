using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals
{
	public class FCBAdjustmentJournalLine : GLJournalLine
	{
		public FCBAdjustmentJournalLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overriden Members

		public override ZDecimal AL_LocalExTaxAmount
		{
			get
			{
				return base.AL_LocalExTaxAmount;
			}
			set
			{
				if (IsForeignCurrencyBalanceAdjustmentLine)
				{
					((AccountingSuspenders.IRunMethodSuspending)this).RunMethodSuspended = true;
				}

				try
				{
					base.AL_LocalExTaxAmount = value;
				}
				finally
				{
					if (IsForeignCurrencyBalanceAdjustmentLine)
					{
						((AccountingSuspenders.IRunMethodSuspending)this).RunMethodSuspended = false;
					}
				}
			}
		}

		public override ZDecimal AL_ExchangeRate
		{
			get { return base.AL_ExchangeRate; }
			set
			{
				using (GetLocalAmountCalculationSuspender())
				using (GetOSAmountCalculationSuspender())
				{
					base.AL_ExchangeRate = value;
				}
			}
		}

		public override ZString AL_RX_NKTransactionCurrency
		{
			get { return base.AL_RX_NKTransactionCurrency; }
			set
			{
				((AccountingSuspenders.IRunMethodSuspending)this).RunMethodSuspended = true;

				try
				{
					base.AL_RX_NKTransactionCurrency = value;
				}
				finally
				{
					((AccountingSuspenders.IRunMethodSuspending)this).RunMethodSuspended = false;
				}
			}
		}

		public override ZGuid AL_AG
		{
			get
			{
				return base.AL_AG;
			}
			set
			{
				base.AL_AG = value;

				if (IsForeignCurrencyBalanceAdjustmentLine)
				{
					((AccountingSuspenders.IRunMethodSuspending)this).RunMethodSuspended = true;
					try
					{
						AL_OSExTaxAmount = 0M;
					}
					finally
					{
						((AccountingSuspenders.IRunMethodSuspending)this).RunMethodSuspended = false;
					}
				}
				else
				{
					AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				}

				if (LineValidation != null)
				{
					LineValidation.ValidateAL_RX_NKTransactionCurrency();
					LineValidation.ValidateAL_OSExTaxAmount();
				}
			}
		}

		public override void CopyValuesFrom(GLJournalLine line)
		{
			base.CopyValuesFrom(line);
			UnsignedLocalLineAmount = line.UnsignedLocalLineAmount;
		}

		protected override AccTransactionLinesValidation GetNewValidationCore()
		{
			return new FCBAdjustmentJournalLineValidation(this);
		}

		FCBAdjustmentJournalLineValidation LineValidation
		{
			get { return Validation as FCBAdjustmentJournalLineValidation; }
		}

		public FCBAdjustmentJournal ForeignCurrencyJournalHeader
		{
			get { return (FCBAdjustmentJournal)MasterTransactionHeader; }
		}

		#endregion

		public bool IsUnrealizedLocalAmountLine
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.Value != Guid.Empty && AL_AG == AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.Value;
			}
		}

		public bool IsForeignCurrencyBalanceAdjustmentLine
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.Value != Guid.Empty && AL_AG != AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.Value;
			}
		}

		protected bool UnsignedOSLineAmount_ReadOnly
		{
			get { return IsForeignCurrencyBalanceAdjustmentLine; }
		}
	}
}
