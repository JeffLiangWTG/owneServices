using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Testing;

public class FieldPreq<T> : Preq<T>
	where T : BusinessObject
{
	public FieldPreq(Func<T, ZPropertyInfo> getConcernedProperty) : this(getConcernedProperty, null, null) { }

	public FieldPreq(Func<T, ZPropertyInfo> getConcernedProperty, FieldPreqValue validFieldValue, FieldPreqValue invalidFieldValue)
	{
		this.getConcernedProperty = getConcernedProperty;
		this.validFieldValue = validFieldValue;
		this.invalidFieldValue = invalidFieldValue;
	}

	protected override IEnumerable<Action<T>> GetSatisfactoryActionsCore() => validFieldValue.Values.Select(value => (Action<T>)(x => {
		var propertyInfo = getConcernedProperty(x);
		if (propertyInfo is ZWrappedPropertyInfo wrapperPropertyInfo)
		{
			propertyInfo = wrapperPropertyInfo.InnerInfo;
		}
		propertyInfo.SetValueFromString(value.ToString());
	}));

	protected override IEnumerable<Action<T>> GetFailingActionsCore() => invalidFieldValue.Values.Select(value => (Action<T>)(x => {
		var propertyInfo = getConcernedProperty(x);
		if (propertyInfo is ZWrappedPropertyInfo wrapperPropertyInfo)
		{
			propertyInfo = wrapperPropertyInfo.InnerInfo;
		}
		propertyInfo.SetValueFromString(value.ToString());
	}));

	readonly Func<T, ZPropertyInfo> getConcernedProperty;
	FieldPreqValue validFieldValue;
	FieldPreqValue invalidFieldValue;

	#region Convertion

	public static implicit operator FieldPreq<T>((Func<T, ZPropertyInfo> getPropertyInfo, FieldPreqValue valid, FieldPreqValue invalid) invocation) => new FieldPreq<T>(invocation.getPropertyInfo, invocation.valid, invocation.invalid);

	public static implicit operator FieldPreq<T>((Func<T, ZPropertyInfo> getPropertyInfo, FieldPreqValue valid) invocation) => new FieldPreq<T>(invocation.getPropertyInfo, invocation.valid, GetDefaultInvalidValue(invocation.valid));

	static FieldPreqValue GetDefaultInvalidValue(FieldPreqValue valid)
	{
		FieldPreqValue result = null;
		if (!valid.Values.Any())
		{
			throw new InvalidOperationException("Cannot retrieve invalid value for empty FieldPreqValue");
		}
		else if (valid.Values.All(v => v is ZString))
		{
			result = string.Empty;
		}
		else if (valid.Values.All(v => v is ZBool))
		{
			result = !((ZBool)valid.Values.First());
		}
		else if (valid.Values.All(v => v is ZInt))
		{
			result = 0;
		}
		else if (valid.Values.All(v => v is ZDecimal))
		{
			result = decimal.Zero;
		}
		else if (valid.Values.All(v => v is ZDateTime))
		{
			result = DateTime.MaxValue;
		}
		return result;
	}

	#endregion

	public FieldPreq<T> Values(params FieldPreqValue[] values)
	{
		validFieldValue = values.Aggregate((a, b) => a || b);
		invalidFieldValue = GetDefaultInvalidValue(validFieldValue);
		return this;
	}

	public FieldPreq<T> NotValues(FieldPreqValue invalidValue)
	{
		invalidFieldValue = invalidValue;
		return this;
	}
}
