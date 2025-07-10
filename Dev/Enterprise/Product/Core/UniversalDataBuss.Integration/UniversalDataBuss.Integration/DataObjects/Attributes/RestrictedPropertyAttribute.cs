using System;
using System.Reflection;

namespace Enterprise.UniversalDataBuss.Integration
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class RestrictedPropertyAttribute : Attribute
	{
		public RestrictedPropertyAttribute(string restrictionPropertyName)
		{
			RestrictionPropertyName = restrictionPropertyName;
		}

		public string RestrictionPropertyName { get; }

		public static bool IsRestricted(PropertyInfo propertyInfo)
		{
			var attribute = propertyInfo.GetCustomAttribute<RestrictedPropertyAttribute>();
			if (attribute != null && !string.IsNullOrEmpty(attribute.RestrictionPropertyName))
			{
				var componentType = propertyInfo.DeclaringType;
				var restrictingProperty = componentType?.GetProperty(attribute.RestrictionPropertyName, BindingFlags.Static | BindingFlags.Public);
				if (restrictingProperty != null)
				{
					if (restrictingProperty.PropertyType != typeof(bool))
					{
						throw new InvalidCastException(FormattableString.Invariant($"Boolean type property expected, but property {restrictingProperty.Name} of type {restrictingProperty.PropertyType.Name} was found."));
					}

					return (bool)restrictingProperty.GetValue(null);
				}
			}

			return false;
		}
	}
}
