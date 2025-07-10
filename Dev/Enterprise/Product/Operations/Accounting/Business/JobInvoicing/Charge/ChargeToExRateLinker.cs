using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	internal enum ExchangeRateKind
	{
		CostRate,
		SellRate,
		SellInvoiceRate
	}

	internal class ChargeToExRateLinker : IService
	{
		public ChargeToExRateLinker()
		{
			ChargeLinks = new Dictionary<ZString, List<ZGuid>>();
			ChargeKeys = new Dictionary<(ZGuid, ExchangeRateKind), List<ZString>>();
			ChargeInitialisingKeys = new Dictionary<(ZGuid, ExchangeRateKind), int>();
		}

		readonly Dictionary<ZString, List<ZGuid>> ChargeLinks;
		readonly Dictionary<(ZGuid, ExchangeRateKind), List<ZString>> ChargeKeys;
		readonly Dictionary<(ZGuid, ExchangeRateKind), int> ChargeInitialisingKeys;

		public static ChargeToExRateLinker Get(BusinessObjectFactory factory) => factory.ServiceContainer.GetService<ChargeToExRateLinker>();

		public static ChargeToExRateLinker GetOrCreate(BusinessObjectFactory factory) => factory.ServiceContainer.GetService<ChargeToExRateLinker>() ?? factory.ServiceContainer.AddService(new ChargeToExRateLinker());

		public void AddLinks(BaseCharge charge)
		{
			foreach (ExchangeRateKind rateKind in Enum.GetValues(typeof(ExchangeRateKind)))
			{
				var keys = GetKeys(charge, rateKind);
				if (keys.Any())
				{
					foreach (var key in keys)
					{
						if (ChargeLinks.TryGetValue(key, out var links))
						{
							links.Add(charge.PK);
						}
						else
						{
							var newLinks = new List<ZGuid>();
							newLinks.Add(charge.PK);
							ChargeLinks[key] = newLinks;
						}
					}

					var chargeRateKey = (charge.PK, rateKind);
					if (ChargeKeys.TryGetValue(chargeRateKey, out var linkKeys))
					{
						linkKeys.AddRange(keys);
					}
					else
					{
						ChargeKeys[chargeRateKey] = new List<ZString>(keys);
					}
				}
			}
		}

		public IDisposable BeginInitialising(BaseCharge charge, ExchangeRateKind rateKind)
		{
			return new DisposableKey(this, charge, rateKind);
		}

		public bool IsInitialising(BaseCharge charge, ExchangeRateKind rateKind)
		{
			var key = (charge.PK, rateKind);
			return ChargeInitialisingKeys.TryGetValue(key, out var count) && count > 0;
		}

		class DisposableKey : IDisposable
		{
			public DisposableKey(ChargeToExRateLinker linker, BaseCharge charge, ExchangeRateKind rateKind)
			{
				Linker = linker;
				Key = (charge.PK, rateKind);
				if (Linker.ChargeInitialisingKeys.ContainsKey(Key))
				{
					Linker.ChargeInitialisingKeys[Key]++;
				}
				else
				{
					Linker.ChargeInitialisingKeys[Key] = 1;
				}
			}

			readonly (ZGuid, ExchangeRateKind) Key;
			readonly ChargeToExRateLinker Linker;

			void IDisposable.Dispose()
			{
				Linker.ChargeInitialisingKeys[Key]--;
				if (Linker.ChargeInitialisingKeys[Key] <= 0)
				{
					Linker.ChargeInitialisingKeys.Remove(Key);
				}
			}
		}

		public bool IsInitialised(BaseCharge charge, ExchangeRateKind rateKind)
		{
			var key = (charge.PK, rateKind);
			return !ChargeKeys.ContainsKey(key);
		}

		public void RemoveLinks(BaseCharge charge, ExchangeRateKind rateKind)
		{
			var key = (charge.PK, rateKind);
			if (ChargeKeys.TryGetValue(key, out var linkKeys))
			{
				foreach (var linkKey in linkKeys)
				{
					ChargeLinks[linkKey].Remove(charge.PK);
					if (!ChargeLinks[linkKey].Any())
					{
						ChargeLinks.Remove(linkKey);
					}
				}
				ChargeKeys.Remove(key);
			}
		}

		ZString[] GetKeys(BaseCharge charge, ExchangeRateKind rateKind)
		{
			var result = new HashSet<ZString>();
			var localCurrency = charge.Company?.GC_RX_NKLocalCurrency ?? GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			switch (rateKind)
			{
				case ExchangeRateKind.CostRate:
					if (!charge.JR_RX_NKCostCurrency.IsEmpty && charge.JR_RX_NKCostCurrency != localCurrency && !charge.JR_IsApportioned)
					{
						result.Add(charge.JR_JH.ToStringKey() + charge.JR_RX_NKCostCurrency + ExchangeRateOrgTypeEnum.Creditor.ToCode() + charge.JR_OH_CostAccount.ToStringKey());
						result.Add(charge.JR_JH.ToStringKey() + charge.JR_RX_NKCostCurrency + ExchangeRateOrgTypeEnum.Creditor.ToCode() + ZGuid.Empty.ToStringKey());
						result.Add(charge.JR_JH.ToStringKey() + charge.JR_RX_NKCostCurrency + ZGuid.Empty.ToStringKey());
					}
					break;
				case ExchangeRateKind.SellRate:
					if (!charge.JR_RX_NKSellCurrency.IsEmpty && charge.JR_RX_NKSellCurrency != localCurrency)
					{
						result.Add(charge.JR_JH.ToStringKey() + charge.JR_RX_NKSellCurrency + ExchangeRateOrgTypeEnum.Debtor.ToCode() + charge.JR_OH_SellAccount.ToStringKey());
						result.Add(charge.JR_JH.ToStringKey() + charge.JR_RX_NKSellCurrency + ExchangeRateOrgTypeEnum.Debtor.ToCode() + ZGuid.Empty.ToStringKey());
						result.Add(charge.JR_JH.ToStringKey() + charge.JR_RX_NKSellCurrency + ZGuid.Empty.ToStringKey());
					}
					break;
				case ExchangeRateKind.SellInvoiceRate:
					if (!charge.JR_RX_NKSellInvoiceCurrency.IsEmpty && charge.JR_RX_NKSellInvoiceCurrency != localCurrency
						&& charge.JR_RX_NKSellInvoiceCurrency !=  charge.JR_RX_NKSellCurrency)
					{
						result.Add(charge.JR_JH.ToStringKey() + charge.JR_RX_NKSellInvoiceCurrency + ExchangeRateOrgTypeEnum.Debtor.ToCode() + charge.JR_OH_SellAccount.ToStringKey());
						result.Add(charge.JR_JH.ToStringKey() + charge.JR_RX_NKSellInvoiceCurrency + ExchangeRateOrgTypeEnum.Debtor.ToCode() + ZGuid.Empty.ToStringKey());
						result.Add(charge.JR_JH.ToStringKey() + charge.JR_RX_NKSellInvoiceCurrency + ZGuid.Empty.ToStringKey());
					}
					break;
			}

			return result.ToArray();
		}

		public ZGuid[] GetRelatedChargesPKs(ExchangeRate exRate)
		{
			var key = GetKey(exRate);
			return ChargeLinks.ContainsKey(key) ? ChargeLinks[key].Distinct().ToArray() : Array.Empty<ZGuid>();
		}

		ZString GetKey(ExchangeRate exRate)
		{
			return exRate.JF_JH.ToStringKey() + exRate.JF_RX_NKRateCurrency + exRate.JF_OrgType + exRate.JF_OH_Org.ToStringKey();
		}
	}
}