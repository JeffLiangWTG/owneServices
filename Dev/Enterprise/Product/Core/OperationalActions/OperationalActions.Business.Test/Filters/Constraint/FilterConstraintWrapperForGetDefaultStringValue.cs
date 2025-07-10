using System;

namespace Enterprise.Services.OperationalActions.Business.Testing;

 class FilterConstraintWrapperForGetDefaultStringValue : IFilterConstraint
{
	public FilterConstraintWrapperForGetDefaultStringValue(IFilterConstraint inner)
	{
		this.inner = inner;
	}

	readonly IFilterConstraint inner;

	string IFilterConstraint.Name => inner.Name;

	string IFilterConstraint.SingularValueName => inner.SingularValueName;

	string IFilterConstraint.PluralValueName => inner.PluralValueName;

	string IFilterConstraint.Description => inner.Description;

	object IFilterConstraint.GetValue()
	{
		var value = inner.GetValue();
		var defaultStringValue = inner.GetDefaultStringValue();

		if (!Equals(value, defaultStringValue))
		{
			throw new InvalidOperationException("Expected GetValue() and GetDefaultStringValue() to return same value.");
		}

		return value;
	}

	public string GetDefaultStringValue()
	{
		return inner.GetDefaultStringValue();
	}
}
