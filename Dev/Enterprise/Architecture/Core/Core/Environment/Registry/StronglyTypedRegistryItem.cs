using System;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public abstract class StronglyTypedRegistryItem<T> : StronglyTypedRegistryItem<T, T>
	{
		public StronglyTypedRegistryItem(IRegistryItem inner)
			: base(inner)
		{
		}

		protected override T Convert(T value)
		{
			return value;
		}

		public virtual bool HasBeenSetOrChanged(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var savedValue = Value as string;
			var changedValue = ((IRegistryItemInternals)this).GetProposedValue(companyPK, branchPK, departmentPK) as string;
			var result = !string.IsNullOrWhiteSpace(savedValue) || !string.IsNullOrWhiteSpace(changedValue);
			return result;
		}

		public virtual bool IsEmpty(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var savedValue = Value as string;
			var result = string.IsNullOrWhiteSpace(savedValue);
			return result;
		}
	}

	public abstract class ConvertableRegistryItem<TGet, TSet> : RegistryItemWrapper
	{
		public ConvertableRegistryItem(IRegistryItem inner)
			: base(inner)
		{
		}

		protected override object ValueCore
		{
			get { return Convert((TSet)base.ValueCore); }
		}

		protected override object GetValueWithoutFallbackCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return Convert((TSet)base.GetValueWithoutFallbackCore(companyPK, branchPK, departmentPK));
		}

		public override object GetFallBackValueAtAllLevels(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return Convert((TSet)base.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK));
		}

		public override object DefaultValue
		{
			get { return Convert((TSet)base.DefaultValue); }
		}

		protected virtual TGet Convert(TSet value)
		{
			return (TGet)((object)value);
		}
	}

	public abstract class StronglyTypedRegistryItem<TGet, TSet> : ConvertableRegistryItem<TGet, TSet>
	{
		public StronglyTypedRegistryItem(IRegistryItem inner)
			: base(inner)
		{
		}

		public new TGet GetValueWithoutFallback(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return (TGet)base.GetValueWithoutFallback(companyPK, branchPK, departmentPK);
		}

		public new TGet GetFallBackValueAtAllLevels(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return (TGet)base.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK);
		}

		public void SetValue(Guid companyPK, Guid branchPK, Guid departmentPK, TSet value)
		{
			((IRegistryItem)this).SetValue(companyPK, branchPK, departmentPK, value);
		}

		public new TGet DefaultValue
		{
			get { return (TGet)base.DefaultValue; }
		}

		public TGet Value
		{
			get { return (TGet)((IRegistryItem)this).Value; }
		}
	}
}
