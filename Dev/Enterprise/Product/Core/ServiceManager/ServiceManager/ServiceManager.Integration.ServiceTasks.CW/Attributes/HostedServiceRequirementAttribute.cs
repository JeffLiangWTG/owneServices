using System;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using Enterprise.Integration;

namespace ServiceManager.Integration.ServiceTasks.CW
{
	[AttributeUsage(AttributeTargets.Method)]
	[Serializable]
	public sealed class HostedServiceRequirementAttribute : Attribute
	{
		public HostedServiceRequirementAttribute()
		{ }

		public static string CheckValueIsNotNullOrEmptyString(IRegistryItem registryItem)
		{
			if (!string.IsNullOrEmpty((string)registryItem.Value))
			{
				return string.Empty;
			}

			return string.Format(CultureInfo.InvariantCulture, "The registry setting '{0}' has not been configured.", registryItem.GetLocationInEnglish());
		}

		public static string CheckValueIsNotEqualTo(IRegistryItem registryItem, IConvertible notEqualToValue)
		{
			if (!registryItem.Value.Equals(notEqualToValue))
			{
				return string.Empty;
			}

			return string.Format(CultureInfo.InvariantCulture, "The registry setting '{0}' requires a value other than '{1}'.", registryItem.GetLocationInEnglish(), notEqualToValue.ToString(CultureInfo.InvariantCulture));
		}

		public static string CheckValueIsEqualTo(IRegistryItem registryItem, IConvertible equalToValue)
		{
			_ = Argument.NotNull(registryItem, nameof(registryItem));
			_ = Argument.NotNull(registryItem.Value, $"{nameof(registryItem)}.{nameof(registryItem.Value)}");

			if (registryItem!.Value!.Equals(equalToValue))
			{
				return string.Empty;
			}

			return string.Format(CultureInfo.InvariantCulture, "The registry setting '{0}' requires a value equal to '{1}'.", registryItem.GetLocationInEnglish(), equalToValue.ToString(CultureInfo.InvariantCulture));
		}

		public static string CheckValueIsAmong(IRegistryItem registryItem, params IConvertible[] equalToValues)
		{
			_ = Argument.NotNull(registryItem, nameof(registryItem));
			_ = Argument.NotNull(registryItem.Value, $"{nameof(registryItem)}.{nameof(registryItem.Value)}");

			if (equalToValues.Any(registryItem!.Value!.Equals))
			{
				return string.Empty;
			}

			var valuesInQuotes = equalToValues.Select(x => FormattableString.Invariant($"'{x.ToString(CultureInfo.InvariantCulture)}'"));
			var valuesString = string.Join(", ", valuesInQuotes);

			return FormattableString.Invariant($"The registry setting '{registryItem.GetLocationInEnglish()}' requires one of the following values: {valuesString}.");
		}

		public static string CheckValueStringLengthIsGreaterThan(IRegistryItem registryItem, int length)
		{
			_ = Argument.NotNull(registryItem, nameof(registryItem));

			var stringVal = (string)registryItem!.Value;
			if (!string.IsNullOrEmpty(stringVal) && stringVal.Length > length)
			{
				return string.Empty;
			}

			return string.Format(CultureInfo.InvariantCulture, "The registry setting '{0}' must be set to a value longer than '{1}' characters.", registryItem.GetLocationInEnglish(), length);
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	[Serializable]
	public sealed class HostedServiceRequirementsAttribute : Attribute
	{
		public HostedServiceRequirementsAttribute()
		{ }
	}
}
