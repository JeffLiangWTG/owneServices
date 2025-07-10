using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.AccountingIServices
{
	internal abstract class BizoDataRowRelatedValues<T> : BizoDataRowRelatedValue<Dictionary<string, T>>
	{
		void SetDataRowRelatedValue(BusinessObject bizoForDataRow, string valueName, T value)
		{
			Dictionary<string, T> values;
			if (!TryGetDataRowRelatedValue(bizoForDataRow, out values))
			{
				values = new Dictionary<string, T>();
				SetDataRowRelatedValue(bizoForDataRow, values);
			}

			values[valueName] = value;
		}

		bool TryGetDataRowRelatedValue(BusinessObject bizoForDataRow, string valueName, out T value)
		{
			value = default(T);

			Dictionary<string, T> values;
			if (TryGetDataRowRelatedValue(bizoForDataRow, out values))
			{
				return values.TryGetValue(valueName, out value);
			}

			return false;
		}

		protected static class BizoDataRowRelatedValuesAccessor<ServiceType> where ServiceType : BizoDataRowRelatedValues<T>, new()
		{
			public static void SetDataRowRelatedValue(BusinessObject bizoForDataRow, string flagName, T flagValue)
			{
				var service = bizoForDataRow.Factory.ServiceContainer.GetService<ServiceType>();
				if (service == null)
				{
					service = new ServiceType();
					bizoForDataRow.Factory.ServiceContainer.AddService(service);
				}

				service.SetDataRowRelatedValue(bizoForDataRow, flagName, flagValue);
			}

			public static T GetDataRowRelatedValue(BusinessObject bizoForDataRow, string flagName)
			{
				var service = bizoForDataRow.Factory.ServiceContainer.GetService<ServiceType>();
				T value;
				if (service != null && service.TryGetDataRowRelatedValue(bizoForDataRow, flagName, out value))
				{
					return value;
				}

				return default(T);
			}
		}
	}

	internal abstract class BizoDataRowRelatedValue<T> : IService
	{
		protected void SetDataRowRelatedValue(BusinessObject bizoForDataRow, T value)
		{
			dataRowValues[bizoForDataRow.PK] = value;
		}

		protected bool TryGetDataRowRelatedValue(BusinessObject bizoForDataRow, out T value)
		{
			return dataRowValues.TryGetValue(bizoForDataRow.PK, out value);
		}

		protected bool RemoveDataRowRelatedValue(BusinessObject bizoForDataRow)
		{
			return dataRowValues.Remove(bizoForDataRow.PK);
		}

		readonly Dictionary<ZGuid, T> dataRowValues = new Dictionary<ZGuid, T>();
	}
}
