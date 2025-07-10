using System;
using System.Collections.Concurrent;

namespace CargoWise.Common
{
	public static class ConcurrentDictionaryExtensions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static TValue GetOrAdd<TKey, TValue>(this ConcurrentDictionary<TKey, Lazy<TValue>> @this, TKey key, Func<TKey, TValue> valueFactory)
		{
			Argument.NotNull(@this, nameof(@this));
			Argument.NotNull(key, nameof(key));
			var result = @this.GetOrAdd(key, (k) => new Lazy<TValue>(() => valueFactory(k)));
			if (result != null)
			{
				return result.Value;
			}
			return default(TValue);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static TValue GetOrAdd<TKey, TValue>(this ConcurrentDictionary<TKey, Lazy<TValue>> @this, TKey key, TValue value)
		{
			Argument.NotNull(@this, nameof(@this));
			Argument.NotNull(key, nameof(key));
			return @this.GetOrAdd(key, new Lazy<TValue>(() => value)).Value;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static bool TryGetValue<TKey, TValue>(this ConcurrentDictionary<TKey, Lazy<TValue>> @this, TKey key, out TValue value)
		{
			Argument.NotNull(@this, nameof(@this));
			Argument.NotNull(key, nameof(key));
			value = default(TValue);

			var result = @this.TryGetValue(key, out Lazy<TValue> v);

			if (result & v != null)
			{
				value = v.Value;
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static TValue AddOrUpdate<TKey, TValue>(this ConcurrentDictionary<TKey, Lazy<TValue>> @this, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
		{
			Argument.NotNull(@this, nameof(@this));
			Argument.NotNull(key, nameof(key));
			var result = @this.AddOrUpdate(key, (k) => new Lazy<TValue>(() => addValueFactory(k)), (k, currentValue) => new Lazy<TValue>(() => updateValueFactory(k, currentValue.Value)));
			if (result != null)
			{
				return result.Value;
			}
			return default(TValue);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static TValue AddOrUpdate<TKey, TValue>(this ConcurrentDictionary<TKey, Lazy<TValue>> @this, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
		{
			Argument.NotNull(@this, nameof(@this));
			Argument.NotNull(key, nameof(key));
			return @this.AddOrUpdate(key, new Lazy<TValue>(() => addValue), (k, currentValue) => new Lazy<TValue>(() => updateValueFactory(k, currentValue.Value))).Value;
		}
	}
}
