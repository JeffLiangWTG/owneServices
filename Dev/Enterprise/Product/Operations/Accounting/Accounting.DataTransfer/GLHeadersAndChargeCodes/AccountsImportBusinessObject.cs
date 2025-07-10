using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes
{
	public class AccountsImportBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AccountsImportBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		readonly Dictionary<string, Guid> cachedRegistryItems = new Dictionary<string, Guid>();

		public void DeleteAllChargeCodes()
		{
			var globalChargeCodes = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, null));
			for (int index = 0; index < globalChargeCodes.Length; index++)
			{
				globalChargeCodes[index].Delete();
			}
			var remainingChargeCodes = Factory.Load<AccChargeCode>(new ZQuery());
			for (int index = 0; index < remainingChargeCodes.Length; index++)
			{
				remainingChargeCodes[index].Delete();
			}
		}

		public void DeleteAllGLHeaders()
		{
			ClearAllControlAccounts();

			AccGLHeader[] gLHeaders = Factory.Load<AccGLHeader>(new ZQuery());
			int gLHeaderLength = gLHeaders.Length;
			for (int index = 0; index < gLHeaderLength; index++)
			{
				gLHeaders[index].Delete();
			}
		}

		public void ClearAllControlAccounts()
		{
			cachedRegistryItems.Clear();
			CacheRegistryItem(AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount);
			CacheRegistryItem(AccountingConfigurationRegistry.Instance.ARControlAccount);
			CacheRegistryItem(AccountingConfigurationRegistry.Instance.APControlAccount);
			CacheRegistryItem(AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount);
			CacheRegistryItem(AccountingConfigurationRegistry.Instance.APSuspenseControlAccount);
			CacheRegistryItem(AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount);
			CacheRegistryItem(AccountingConfigurationRegistry.Instance.RealizedExchangeLossAccount);
			CacheRegistryItem(AccountingConfigurationRegistry.Instance.ARDiscountAccount);
			CacheRegistryItem(AccountingConfigurationRegistry.Instance.APDiscountAccount);
			CacheRegistryItem(AccountingConfigurationRegistry.Instance.OverpaymentsAccount);
			CacheRegistryItem(AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount);
			CacheRegistryItem(AccountingConfigurationRegistry.Instance.AccruedCostControlAccount);
			CacheRegistryItem(AccountingConfigurationRegistry.Instance.GSTInputControlAccount);
			CacheRegistryItem(AccountingConfigurationRegistry.Instance.GSTOutputControlAccount);
			CacheRegistryItem(AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount);
			CacheRegistryItem(AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount);
			CacheRegistryItem(AccountingConfigurationRegistry.Instance.WHTInputControlAccount);
			CacheRegistryItem(AccountingConfigurationRegistry.Instance.WHTOutputControlAccount);
			CacheRegistryItem(AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount);
			CacheRegistryItem(AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount);
			CacheRegistryItem(AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount);

			AccountingConfigurationRegistry.Instance.ClearAllControlAccountRegistryItems();
		}

		void CacheRegistryItem(GuidRegistryItem item)
		{
			cachedRegistryItems.Add(item.Name, item.Value);
		}

		public void RevertAllControlAccounts()
		{
			IRegistryItem[] items = AccountingConfigurationRegistry.Instance.GetAllItems();
			foreach (string name in cachedRegistryItems.Keys)
			{
				Guid guid = cachedRegistryItems[name];
				try
				{
					foreach (IRegistryItem item in items)
					{
						if (item.Name.Equals(name))
						{
							item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, guid);
							break;
						}
					}
				}
				catch (RegistryValidationException)
				{
				}
			}
		}
	}
}
