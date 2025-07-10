using System.Globalization;

namespace CargoWise.Data.SqlProxy.Interface.Converters;

[WTG.StaticAnalysis.Annotation.CodeAlive("SQL Over Http Connection")]
public static class ObjectConverter
{
	public static T? PrimitiveTypeCast<T>(object? value)
	{
		if (value == null || value == DBNull.Value)
		{
			return default;
		}

		if (value is T t)
		{
			return t;
		}

		try
		{
			return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
		}
		catch (InvalidCastException ex)
		{
			throw new InvalidCastException($"Cannot convert value: {value} of type {value.GetType().Name} to type {typeof(T).Name}.", ex);
		}
	}
}
