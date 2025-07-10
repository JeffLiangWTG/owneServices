using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public abstract class AutomaticProcessRegistryItemBase<T> : StronglyTypedRegistryItem<T> where T : AutomaticProcessRegistryBusinessObject
	{
		protected AutomaticProcessRegistryItemBase(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, NonPersistentBusinessObjectRegistryDataType<T> dataType, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, dataType, storage, RegistryOptions.NotCached))
		{
		}

		protected AutomaticProcessRegistryItemBase(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, NonPersistentBusinessObjectRegistryDataType<T> dataType, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, dataType, storage, options))
		{
		}

		public void UpdateLastRun(ZDateTime lastRun)
		{
			RegistryItemFallBackValueAccessor accessor = new RegistryItemFallBackValueAccessor(this, RegistryStorageFlags.Company, Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			FallbackValue fallbackValue = accessor.GetFallBackValue();
			T currentValue = (T)fallbackValue.Value;
			currentValue.UpdateRuns(lastRun);
			Guid companyPK = (fallbackValue.Level == RegistryStorageFlags.System) ? Guid.Empty : Env.CurrentCompany.PK;
			SetValue(companyPK, Guid.Empty, Guid.Empty, currentValue);
		}
	}

	public class AutomaticProcessRegistryItem : AutomaticProcessRegistryItemBase<AutomaticProcessRegistryBusinessObject>
	{
		public AutomaticProcessRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(name, category, caption, hint, new AutomaticProcessRegistryDataType(), storage, options)
		{
		}

		public AutomaticProcessRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, bool shouldCheckIntervalNotLessThanMinimumValue = false)
			: base(name, category, caption, hint, new AutomaticProcessRegistryDataType(shouldCheckIntervalNotLessThanMinimumValue), storage)
		{
		}

		public AutomaticProcessRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, string minimumIntervalType, int minimumInterval)
			: base(name, category, caption, hint, new AutomaticProcessRegistryDataType(minimumIntervalType, minimumInterval), storage, options)
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.AutomaticProcessRegistryItemEditor, Enterprise.Registry.GUI")]
	public class AutomaticProcessRegistryDataType : AutomaticProcessRegistryDataType<AutomaticProcessRegistryBusinessObject>
	{
		public AutomaticProcessRegistryDataType() : base()
		{
		}

		public AutomaticProcessRegistryDataType(bool shouldValidate) : base(shouldValidate)
		{
		}

		public AutomaticProcessRegistryDataType(string minimumIntervalType, int minimumInterval) : base(minimumIntervalType, minimumInterval)
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.AutomaticProcessRegistryItemEditor, Enterprise.Registry.GUI")]
	public class AutomaticProcessRegistryDataType<T> : NonPersistentBusinessObjectRegistryDataType<T> where T : AutomaticProcessRegistryBusinessObject
	{
		public AutomaticProcessRegistryDataType()
		{
		}

		public AutomaticProcessRegistryDataType(bool shouldValidate)
		{
			ShouldCheckIntervalNotLessThanMinimumValue = shouldValidate;
		}

		public AutomaticProcessRegistryDataType(string minimumIntervalType, int minimumInterval)
		{
			ShouldCheckIntervalNotLessThanMinimumValue = true;
			MinimumIntervalType = minimumIntervalType;
			MinimumInterval = minimumInterval;
		}

		public string MinimumIntervalType { get; internal set; }
		public int MinimumInterval { get; internal set; }
		public bool ShouldCheckIntervalNotLessThanMinimumValue { get; internal set; }

		protected override T CloneValue(T value)
		{
			value.ShouldCheckIntervalNotLessThanMinimumValue = ShouldCheckIntervalNotLessThanMinimumValue;
			value.MinimumIntervalType = MinimumIntervalType;
			value.MinimumInterval = MinimumInterval;
			return base.CloneValue(value);
		}

		protected override T DeserialiseCore(byte[] value)
		{
			var deserialisedValue = base.DeserialiseCore(value);
			deserialisedValue.ShouldCheckIntervalNotLessThanMinimumValue = ShouldCheckIntervalNotLessThanMinimumValue;
			deserialisedValue.MinimumIntervalType = MinimumIntervalType;
			deserialisedValue.MinimumInterval = MinimumInterval;
			return deserialisedValue;
		}
	}
}
