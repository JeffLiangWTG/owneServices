using System;
using CargoWise.Application;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Environment
{
	public class Currency : ICurrency
	{
		public Currency(string code)
		{
			Code = code;

			Currency loadedCurrency = CurrencyCache[code];
			if (loadedCurrency != null)
			{
				PK = loadedCurrency.PK;
				SubUnitRatio = loadedCurrency.SubUnitRatio;
			}
			else
			{
				var factory = new BusinessObjectFactory();
				var bizO = factory.LoadFromNaturalKey(ObjectFactory.GetType("IRefCurrency"), RefCurrencySchema.RX_Code, code);
				if (bizO != null)
				{
					PK = bizO.PK.ToGuid();
					SubUnitRatio = (ZInt)bizO[RefCurrencySchema.RX_SubUnitRatio];
				}
				CurrencyCache.Add(code, this);
			}
		}

#if DEBUG
		internal
#endif
 static LRUCache<string, Currency> CurrencyCache
		{
			get
			{
				if (currencyCache == null)
				{
					currencyCache = new LRUCache<string, Currency>();
				}
				return currencyCache;
			}
		}

		[ThreadStatic]
		static LRUCache<string, Currency> currencyCache;

		public Guid PK { get; private set; }
		public string Code { get; private set; }
		public int SubUnitRatio { get; private set; }

		public int Decimals
		{
			get
			{
				if (SubUnitRatio <= 1)
				{
					return 0;
				}

				return (int)Math.Log10(SubUnitRatio);
			}
		}
	}
}
