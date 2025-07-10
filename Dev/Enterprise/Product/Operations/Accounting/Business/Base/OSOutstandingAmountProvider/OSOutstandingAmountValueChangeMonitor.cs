using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class OSOutstandingAmountValueChangeMonitor
	{
		public OSOutstandingAmountValueChangeMonitor(TransactionHeader sourceHeader)
		{
			Header = sourceHeader;
			IsValueFixed = false;
			IsValueUnchanged = true;
			var isNowFeatureEnabled = IsPreviouslyFeatureEnabled = IsFeatureEnabled;

			Reset();
			BindLedgerChangedEvents();

			if (isNowFeatureEnabled)
			{
				BindValueChangedEvents();

				if (!Header.IsInDatabase)
				{
					ConfigureTransactionHeader();
				}
			}
		}

		public bool IsValueUpToDate => IsValueUnchanged || IsValueFixed;

		public void Reset()
		{
			IsValueUnchanged = true;
		}

		public void Fixed()
		{
			IsValueFixed = true;
		}

		#region Implementation

		bool IsMonitorApplicable
		{
			get
			{
				var list = Header.Factory.GetCachedValue("OSOutstandingAmountTransactionList", () => OSOutstandingAmountTransactionList);
				return list.Any(x =>
					(x.Ledger == null || x.Ledger == Header.AH_Ledger) &&
					(x.TransactionType == null || x.TransactionType == Header.AH_TransactionType));
			}
		}

		void ConfigureTransactionHeader()
		{
			var isNowFeatureEnabled = IsFeatureEnabled;

			if (Header.AH_IsOSOutstandingAmountApplicable != isNowFeatureEnabled)
			{
				Header.AH_IsOSOutstandingAmountApplicable = isNowFeatureEnabled;

				if (Header.AH_IsOSOutstandingAmountApplicable)
				{
					IsValueUnchanged = false;
				}
				else
				{
					Header.AH_OSOutstandingAmount = 0;
					Reset();
				}
			}
		}

		void ValueChangedHandler(object sender, EventArgs e)
		{
			IsValueUnchanged = false;
		}

		void LedgerChangedHandler(object sender, EventArgs e)
		{
			RebindEvents();
			ConfigureTransactionHeader();
		}

		bool IsFeatureEnabled => AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.GetFallBackValueAtAllLevels(Header.Company.PK.ToGuid(), Guid.Empty, Guid.Empty) && IsMonitorApplicable;

		IEnumerable<(string Ledger, string TransactionType)> OSOutstandingAmountTransactionList => new (string Ledger, string TransactionType)[]
			{
				(LedgerTypes.AccountsReceivable, null),
				(LedgerTypes.AccountsPayable, null),
				(LedgerTypes.CashBook, TransactionTypes.OpeningReceipt),
				(LedgerTypes.CashBook, TransactionTypes.OpeningPayment),
			};

		#region Event Binding

		void BindLedgerChangedEvents()
		{
			Header.AH_LedgerInfo.ValueChanged += LedgerChangedHandler;
			Header.AH_TransactionTypeInfo.ValueChanged += LedgerChangedHandler;
		}

		void BindValueChangedEvents()
		{
			Header.AH_InvoiceAmountInfo.ValueChanged += ValueChangedHandler;
			Header.AH_GSTAmountInfo.ValueChanged += ValueChangedHandler;
			Header.AH_LocalTaxAmountOtherTaxesInfo.ValueChanged += ValueChangedHandler;
			Header.AH_OSTotalInfo.ValueChanged += ValueChangedHandler;
			Header.AH_OutstandingAmountInfo.ValueChanged += ValueChangedHandler;
			Header.AH_RX_NKTransactionCurrencyInfo.ValueChanged += ValueChangedHandler;
		}

		void UnbindValueChangedEvents()
		{
			Header.AH_InvoiceAmountInfo.ValueChanged -= ValueChangedHandler;
			Header.AH_GSTAmountInfo.ValueChanged -= ValueChangedHandler;
			Header.AH_LocalTaxAmountOtherTaxesInfo.ValueChanged -= ValueChangedHandler;
			Header.AH_OSTotalInfo.ValueChanged -= ValueChangedHandler;
			Header.AH_OutstandingAmountInfo.ValueChanged -= ValueChangedHandler;
			Header.AH_RX_NKTransactionCurrencyInfo.ValueChanged -= ValueChangedHandler;
		}

		void RebindEvents()
		{
			var isNowFeatureEnabled = IsFeatureEnabled;

			if (IsPreviouslyFeatureEnabled && !isNowFeatureEnabled)
			{
				UnbindValueChangedEvents();
				Reset();
			}

			if (!IsPreviouslyFeatureEnabled && isNowFeatureEnabled)
			{
				BindValueChangedEvents();
				IsValueUnchanged = false;
			}

			IsPreviouslyFeatureEnabled = isNowFeatureEnabled;
		}

		#endregion

		readonly TransactionHeader Header;
		bool IsPreviouslyFeatureEnabled;
		bool IsValueFixed;
		bool IsValueUnchanged;

#if DEBUG
		public bool IsPreviouslyFeatureEnabled_ForTestOnly => IsPreviouslyFeatureEnabled;

		public bool IsFeatureEnabled_ForTestOnly => IsFeatureEnabled;

		public bool IsMonitorApplicable_ForTestOnly => IsMonitorApplicable;
#endif

		#endregion
	}
}
