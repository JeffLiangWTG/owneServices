using System;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	public interface ICacheVersionProvider
	{
		int CacheVersion { get; }
	}

	public delegate T GetValueDelegate<T>();

	public class CachedValue<T>
	{
		public CachedValue(GetValueDelegate<T> getValueDelegate)
		{
			if (getValueDelegate == null)
			{
				throw new ArgumentNullException(nameof(getValueDelegate));
			}

			this.getValueDelegate = getValueDelegate;
		}

		public virtual T Value
		{
			get
			{
				if (NeedRecalc())
				{
					value = CalculateValue();
				}
				return value;
			}
		}

		protected virtual bool NeedRecalc()
		{
			return !cached;
		}

		protected virtual T CalculateValue()
		{
			cached = true;
			try
			{
				return getValueDelegate();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				cached = false;
				throw;
			}
		}

		protected readonly GetValueDelegate<T> getValueDelegate;
		protected bool cached;
		protected T value;
	}

	public class CachedProperty<T> : CachedValue<T>
	{
		public CachedProperty(ICacheVersionProvider factory, GetValueDelegate<T> getValueDelegate)
			: base(getValueDelegate)
		{
			cacheVersionProvider = Argument.NotNull(factory, nameof(factory));
		}

		public CachedProperty(ICacheVersionProvider factory, GetValueDelegate<T> getValueDelegate, BusinessObject parentBusinessObject, T valueWhenParentIsDeleted)
			: this(factory, getValueDelegate)
		{
			this.parentBusinessObject = Argument.NotNull(parentBusinessObject, nameof(parentBusinessObject));
			this.valueWhenParentIsDeleted = valueWhenParentIsDeleted;
		}

		protected override bool NeedRecalc()
		{
			return lastFactoryVersion == null || cacheVersionProvider.CacheVersion != lastFactoryVersion.Value;
		}

		public override T Value => parentBusinessObject != null && parentBusinessObject.IsDeleted ? valueWhenParentIsDeleted : base.Value;

		protected override T CalculateValue()
		{
			// Cannot remove this assignment as it prevents Stack Overflow on some Cached Properties
			lastFactoryVersion = cacheVersionProvider.CacheVersion;
			try
			{
				var result = base.CalculateValue();
				lastFactoryVersion = cacheVersionProvider.CacheVersion;
				return result;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				lastFactoryVersion = null;
				throw;
			}
		}

		int? lastFactoryVersion;
		readonly ICacheVersionProvider cacheVersionProvider;
		readonly BusinessObject parentBusinessObject;
		readonly T valueWhenParentIsDeleted;
	}

	public static class CachedValueHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		public static TData GetValue<TData>(this ICacheVersionProvider factory, ref CachedProperty<TData> cachedProperty, GetValueDelegate<TData> getValueDelegate)
		{
			if (cachedProperty == null)
			{
				cachedProperty = new CachedProperty<TData>(factory, getValueDelegate);
			}
			return cachedProperty.Value;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		public static TData GetValue<TData>(ref CachedValue<TData> cachedValue, GetValueDelegate<TData> getValueDelegate)
		{
			if (cachedValue == null)
			{
				cachedValue = new CachedValue<TData>(getValueDelegate);
			}
			return cachedValue.Value;
		}
	}
}
