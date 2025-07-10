using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.Base
{
	public abstract class GenericWrapperCollection<T> : GenericWrapperCollection where T : GenericWrapper
	{
		protected GenericWrapperCollection(IBusinessObjectCollection collectionToWrap, BusinessObjectFactory factory) : base(collectionToWrap, factory) { }
		protected GenericWrapperCollection(BusinessObjectFactory factory) : base(factory) { }

		public new T this[int index]
		{
			get { return (T)base[index]; }
		}

		public new T this[string index]
		{
			get { return (T)base[index]; }
		}

		public new T AddNew()
		{
			return (T)base.AddNew();
		}
	}

	public abstract class GenericWrapperCollection : DocBaseWrapperCollection<GenericWrapper>
	{
		protected GenericWrapperCollection(IBusinessObjectCollection collectionToWrap, BusinessObjectFactory factory) : base(collectionToWrap, factory) { }
		protected GenericWrapperCollection(BusinessObjectFactory factory) : base(factory) { }

		protected override object GetTotal(ZString fieldName, ZString decimalPlaces, ZString filter)
		{
			var property = Helper.GetPropertyInfo(fieldName);

			if (property != null && property.PropertyType == typeof(MoneyWrapper))
			{
				return GetTotalMoney(property, Helper.GetFilteredBusinessObjects(filter));
			}
			return base.GetTotal(fieldName, decimalPlaces, filter);
		}

		ZString GetTotalMoney(PropertyInfo property, IEnumerable<BusinessObject> filteredBusinessObjects)
		{
			var result = Money.Empty;
			foreach (var wrapper in filteredBusinessObjects)
			{
				var moneyWrapper = (MoneyWrapper)property.GetValue(wrapper, null);
				if (moneyWrapper != null)
				{
					result = CurrencyConverter.Add(result, moneyWrapper.AmountAsMoney);
				}
			}
			return result.IsEmpty ? "" : result.ToString();
		}

		protected CurrencyConverter CurrencyConverter
		{
			get
			{
				if (fCurrencyConverter == null)
				{
					if (Count > 0)
					{
						ICurrencyConverterProvider converterProvider = this[0].WrappedObject as ICurrencyConverterProvider;
						if (converterProvider != null)
						{
							fCurrencyConverter = converterProvider.CurrencyConverter;
						}
					}
					if (fCurrencyConverter == null)
					{
						fCurrencyConverter = new RefCurrencyCurrencyConverter(Factory, ZDateTime.Now, Enterprise.ZArchitecture.Core.ExchangeRateType.All, 7);
					}
				}
				return fCurrencyConverter;
			}
		}
		CurrencyConverter fCurrencyConverter;
	}
}
