using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class CompareValidation : ValidationProvider
	{
		/// <summary>
		/// Sets an error if two properties are not equal.
		/// </summary>
		/// <param name="propertyInfoToSetErrorOn">The property to set the error on.</param>
		/// <param name="propertyInfoToCompareTo">The property to compare to.</param>
		public static void CheckEqual(ZPropertyInfo propertyInfoToSetErrorOn, ZPropertyInfo propertyInfoToCompareTo)
		{
			if (!IsEqual(propertyInfoToSetErrorOn.Value, propertyInfoToCompareTo.Value))
			{
				string error = Res.GetString("f0921d92-3847-41e8-9308-3d8438e2cd0f", "The {0} and {1} must be the same.", propertyInfoToSetErrorOn.HumanReadableName, propertyInfoToCompareTo.HumanReadableName);
				propertyInfoToSetErrorOn.AddError(error);
			}
		}

		/// <summary>
		/// Sets an error if two properties are the same.
		/// </summary>
		/// <param name="propertyInfoToSetErrorOn">The property to set the error on.</param>
		/// <param name="propertyInfoToCompareTo">The property to compare to.</param>
		public static void CheckNotEqual(ZPropertyInfo propertyInfoToSetErrorOn, ZPropertyInfo propertyInfoToCompareTo)
		{
			if (IsEqual(propertyInfoToSetErrorOn.Value, propertyInfoToCompareTo.Value))
			{
				string error = Res.GetString("fc9e54e0-8ee3-40b2-8bbc-1c61ff39da14", "The {0} and {1} cannot be the same.", propertyInfoToSetErrorOn.HumanReadableName, propertyInfoToCompareTo.HumanReadableName);
				propertyInfoToSetErrorOn.AddError(error);
			}
		}

		/// <summary>
		/// Sets an error if first property is greater than or equal to the second property.
		/// </summary>
		/// <param name="propertyInfoToSetErrorOn">The property to set the error on.</param>
		/// <param name="propertyInfoToCompareTo">The property to compare to.</param>
		public static void CheckNumberLessThanOtherNumber(ZPropertyInfo propertyInfoToSetErrorOn, ZPropertyInfo propertyInfoToCompareTo)
		{
			object valueToSetErrorOn = ((IZTypeInternals)propertyInfoToSetErrorOn.Value).GetValueForLogicalDataLayer(false);
			object valueToCompareTo = ((IZTypeInternals)propertyInfoToCompareTo.Value).GetValueForLogicalDataLayer(false);
			if (new ZDecimal(valueToSetErrorOn) >= new ZDecimal(valueToCompareTo))
			{
				string error = Res.GetString("5cd515a1-7b15-4b05-bc2e-383834e74d86", "The {0} must be less than the {1}.", propertyInfoToSetErrorOn.HumanReadableName, propertyInfoToCompareTo.HumanReadableName);
				propertyInfoToSetErrorOn.AddError(error);
			}
		}

		/// <summary>
		/// Sets an error if first property is greater than the second property.
		/// </summary>
		/// <param name="propertyInfoToSetErrorOn">The property to set the error on.</param>
		/// <param name="propertyInfoToCompareTo">The property to compare to.</param>
		public static void CheckNumberLessThanOrEqualToOtherNumber(ZPropertyInfo propertyInfoToSetErrorOn, ZPropertyInfo propertyInfoToCompareTo)
		{
			object valueToSetErrorOn = ((IZTypeInternals)propertyInfoToSetErrorOn.Value).GetValueForLogicalDataLayer(false);
			object valueToCompareTo = ((IZTypeInternals)propertyInfoToCompareTo.Value).GetValueForLogicalDataLayer(false);
			if (new ZDecimal(valueToSetErrorOn) > new ZDecimal(valueToCompareTo))
			{
				string error = Res.GetString("59750d6b-48a3-43d8-b087-45e4950cc51c", "The {0} must be less than or equal to the {1}.", propertyInfoToSetErrorOn.HumanReadableName, propertyInfoToCompareTo.HumanReadableName);
				propertyInfoToSetErrorOn.AddError(error);
			}
		}

		/// <summary>
		/// Sets an error if first property is less than or equal to the second property.
		/// </summary>
		/// <param name="propertyInfoToSetErrorOn">The property to set the error on.</param>
		/// <param name="propertyInfoToCompareTo">The property to compare to.</param>
		public static void CheckNumberGreaterThanOtherNumber(ZPropertyInfo propertyInfoToSetErrorOn, ZPropertyInfo propertyInfoToCompareTo)
		{
			object valueToSetErrorOn = ((IZTypeInternals)propertyInfoToSetErrorOn.Value).GetValueForLogicalDataLayer(false);
			object valueToCompareTo = ((IZTypeInternals)propertyInfoToCompareTo.Value).GetValueForLogicalDataLayer(false);
			if (new ZDecimal(valueToSetErrorOn) <= new ZDecimal(valueToCompareTo))
			{
				string error = Res.GetString("c09aa062-d27f-4c2e-bdd1-bb54636046ca", "The {0} must be greater than the {1}.", propertyInfoToSetErrorOn.HumanReadableName, propertyInfoToCompareTo.HumanReadableName);
				propertyInfoToSetErrorOn.AddError(error);
			}
		}

		/// <summary>
		/// Sets an error if first property is less than the second property.
		/// </summary>
		/// <param name="propertyInfoToSetErrorOn">The property to set the error on.</param>
		/// <param name="propertyInfoToCompareTo">The property to compare to.</param>
		public static void CheckNumberGreaterThanOrEqualToOtherNumber(ZPropertyInfo propertyInfoToSetErrorOn, ZPropertyInfo propertyInfoToCompareTo)
		{
			object valueToSetErrorOn = ((IZTypeInternals)propertyInfoToSetErrorOn.Value).GetValueForLogicalDataLayer(false);
			object valueToCompareTo = ((IZTypeInternals)propertyInfoToCompareTo.Value).GetValueForLogicalDataLayer(false);
			if (new ZDecimal(valueToSetErrorOn) < new ZDecimal(valueToCompareTo))
			{
				string error = Res.GetString("5da8db67-90bd-430b-a488-14e1c76f6ad6", "The {0} must be greater than or equal to the {1}.", propertyInfoToSetErrorOn.HumanReadableName, propertyInfoToCompareTo.HumanReadableName);
				propertyInfoToSetErrorOn.AddError(error);
			}
		}

		/// <summary>
		/// Set an error if numeric property is not greater than 0.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		public static void CheckNumberGreaterThanZero(ZPropertyInfo propertyInfo)
		{
			AddErrorForLessThanOrEqualToZero(propertyInfo, NotificationType.Error);
		}

		static void AddErrorForLessThanOrEqualToZero(ZPropertyInfo propertyInfo, ComponentModel.INotificationType errorType)
		{
			object value = ((IZTypeInternals)propertyInfo.Value).GetValueForLogicalDataLayer(false);
			if (new ZDecimal(value) <= 0M)
			{
				string humanReadableName = propertyInfo.HumanReadableName;
				string prefix = Grammar.Instance.IndefiniteArticlePrefix(humanReadableName);
				string error = Res.GetString("1b922814-2b0d-41c3-b193-1c6635e6a13e", "Please enter {0}'{1}' greater than 0.", prefix, humanReadableName);

				propertyInfo.AddNotification(errorType, error);
			}
		}

		/// <summary>
		/// Set an message error if numeric property is not greater than 0.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		public static void MessageErrorIfLessThanOrEqualToZero(ZPropertyInfo propertyInfo)
		{
			AddErrorForLessThanOrEqualToZero(propertyInfo, NotificationType.MessageError);
		}

		/// <summary>
		/// Set an error if numeric property is not greater than or equal to 0.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		public static void CheckNumberNotNegative(ZPropertyInfo propertyInfo)
		{
			CheckGreaterThanOrEqualTo(propertyInfo, 0M);
		}

		/// <summary>
		/// Set an error if numeric property is not greater than or equal to a passed value.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		/// <param name="minimumAcceptableValue">The minimum acceptable value.</param>
		public static void CheckGreaterThanOrEqualTo(ZPropertyInfo propertyInfo, decimal minimumAcceptableValue)
		{
			AddErrorForLessThan(propertyInfo, minimumAcceptableValue, NotificationType.Error);
		}

		static void AddErrorForLessThan(ZPropertyInfo propertyInfo, decimal minimumAcceptableValue, ComponentModel.INotificationType errorType)
		{
			object value = ((IZTypeInternals)propertyInfo.Value).GetValueForLogicalDataLayer(false);
			if (new ZDecimal(value) < minimumAcceptableValue)
			{
				string humanReadableName = propertyInfo.HumanReadableName;
				string prefix = Grammar.Instance.IndefiniteArticlePrefix(humanReadableName);
				string error = Res.GetString("73b55bab-46f4-4421-a2ff-c605b706a731", "Please enter {0}'{1}' greater than or equal to {2}.", prefix, humanReadableName, minimumAcceptableValue.ToString(System.Globalization.CultureInfo.CurrentCulture));

				propertyInfo.AddNotification(errorType, error);
			}
		}

		/// <summary>
		/// Set an message error if numeric property is not greater than or equal to a passed value.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		/// <param name="minimumAcceptableValue">The minimum acceptable value.</param>
		public static void MessageErrorIfLessThan(ZPropertyInfo propertyInfo, decimal minimumAcceptableValue)
		{
			AddErrorForLessThan(propertyInfo, minimumAcceptableValue, NotificationType.MessageError);
		}

		/// <summary>
		/// Set an message error if numeric property is not greater than or equal to 0.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		public static void MessageErrorIfNumberNegative(ZPropertyInfo propertyInfo)
		{
			MessageErrorIfLessThan(propertyInfo, 0M);
		}

		/// <summary>
		/// Set an error if numeric property is not less than or equal to a passed value.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		/// <param name="maximumAcceptableValue">The maximum acceptable value.</param>
		public static void CheckLessThanOrEqualTo(ZPropertyInfo propertyInfo, decimal maximumAcceptableValue)
		{
			object value = ((IZTypeInternals)propertyInfo.Value).GetValueForLogicalDataLayer(false);
			if (new ZDecimal(value) > maximumAcceptableValue)
			{
				string humanReadableName = propertyInfo.HumanReadableName;
				string prefix = Grammar.Instance.IndefiniteArticlePrefix(humanReadableName);
				string error = Res.GetString("86f34042-f685-4367-96e2-3391eac98f03", "Please enter {0}'{1}' less than or equal to {2}.", prefix, humanReadableName, maximumAcceptableValue.ToString());

				propertyInfo.AddError(error);
			}
		}

		/// <summary>
		/// Set an error if numeric property is not within range of passed values.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		/// <param name="minimumAcceptableValue">The minimum acceptable value.</param>
		/// <param name="maximumAcceptableValue">The maximum acceptable value.</param>
		public static void CheckWithinRange(ZPropertyInfo propertyInfo, decimal minimumAcceptableValue, decimal maximumAcceptableValue)
		{
			object value = ((IZTypeInternals)propertyInfo.Value).GetValueForLogicalDataLayer(false);
			ZDecimal zValue = new ZDecimal(value);
			if (zValue < minimumAcceptableValue || zValue > maximumAcceptableValue)
			{
				string humanReadableName = propertyInfo.HumanReadableName;
				string prefix = Grammar.Instance.IndefiniteArticlePrefix(humanReadableName);
				string error = Res.GetString("e77c961f-2801-48d4-b40c-54033be31bfc", "Please enter {0}'{1}' within the range {2} to {3}.", prefix, humanReadableName, minimumAcceptableValue.ToString(), maximumAcceptableValue.ToString());

				propertyInfo.AddError(error);
			}
		}

		/// <summary>
		/// Set an error if numeric property is not within set of passed values.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		/// <param name="acceptableValues">The set of acceptable values (MUST have value(s)).</param>
		public static void CheckWithinSet(ZPropertyInfo propertyInfo, params decimal[] acceptableValues)
		{
			object value = ((IZTypeInternals)propertyInfo.Value).GetValueForLogicalDataLayer(false);
			ZDecimal zValue = new ZDecimal(value);
			if (acceptableValues == null || acceptableValues.Length == 0)
			{
				throw new ArgumentOutOfRangeException(nameof(acceptableValues), "Parameter must have value(s).");
			}
			else if (!acceptableValues.Contains(zValue))
			{
				string humanReadableName = propertyInfo.HumanReadableName;
				string prefix = Grammar.Instance.IndefiniteArticlePrefix(humanReadableName);
				string error = Res.GetString("b5563389-1a05-4070-8e99-fdc206e74e61", "Please enter {0}'{1}' within the set [{2}].", prefix, humanReadableName, string.Join(", ", acceptableValues));

				propertyInfo.AddError(error);
			}
		}

		/// <summary>
		/// Set a warning if numeric property is greater than a passed value.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		/// <param name="maximumAcceptableValue">The maximum acceptable value.</param>
		public static void WarnIfGreaterThanValue(ZPropertyInfo propertyInfo, decimal maximumAcceptableValue)
		{
			object value = ((IZTypeInternals)propertyInfo.Value).GetValueForLogicalDataLayer(false);
			if (new ZDecimal(value) > maximumAcceptableValue)
			{
				string warning = Res.GetString("27f7ce14-810d-46be-ae07-d6efce12e91c", "The '{0}' is greater than {1}.", propertyInfo.HumanReadableName, maximumAcceptableValue.ToString());

				propertyInfo.AddWarning(warning);
			}
		}

		/// <summary>
		/// Set an error if the value of the property has been duplicated (ie. it is not unique).
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		/// <param name="propertyInfos">The properties to validate against.</param>
		public static void CheckValueIsNotDuplicated(ZPropertyInfo propertyInfo, ZPropertyInfo[] propertyInfos)
		{
			foreach (ZPropertyInfo otherPropertyInfo in propertyInfos)
			{
				if (propertyInfo != otherPropertyInfo && propertyInfo.Value.Equals(otherPropertyInfo.Value))
				{
					propertyInfo.AddError(Res.GetString("27804ff2-f8e9-40e6-ae73-82b98c9734a3", "The '{0}' you have entered has the same value as the '{1}'. Please enter a unique value.",
						propertyInfo.HumanReadableName, otherPropertyInfo.HumanReadableName));
					break;
				}
			}
		}

		/// <summary>
		/// Sets an error on a date if it IS NOT after a second date.
		/// </summary>
		public static void CheckDateIsAfterAnotherDate(ZPropertyInfo dateToSetErrorOn, ZPropertyInfo otherDate)
		{
			if (TryGetDateConverter(dateToSetErrorOn, otherDate, out TypeConverter convertor))
			{
				if (!IsBefore((IZType)convertor.ConvertFrom(otherDate.Value), (IZType)convertor.ConvertFrom(dateToSetErrorOn.Value)))
				{
					string d1 = "'" + dateToSetErrorOn.HumanReadableName + "'";
					string d2 = "'" + otherDate.HumanReadableName + "'";
					string error = Res.GetString("035a0ef5-dc6a-494f-8f2b-777aa1a004e1", "The {0} must be after the {1}.", d1, d2);

					dateToSetErrorOn.AddError(error);
				}
			}
		}

		static bool TryGetDateConverter(ZPropertyInfo info, ZPropertyInfo otherInfo, out TypeConverter converter)
		{
			if (TryGetDateConverter(info, out converter) && converter.CanConvertFrom(otherInfo.Value.GetType()))
			{
				return true;
			}
			else if (TryGetDateConverter(otherInfo, out converter) && converter.CanConvertFrom(info.Value.GetType()))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		static bool TryGetDateConverter(ZPropertyInfo info, out TypeConverter converter)
		{
			converter = ZDateTimeTypeConverter.Instance;
			var infoType = info.Value.GetType();
			if (converter.CanConvertFrom(infoType))
			{
				return true;
			}
			else
			{
				converter = ZDateTimeOffsetTypeConverter.Instance;
				return converter.CanConvertFrom(infoType);
			}
		}

		/// <summary>
		/// Sets an error on a date if it IS NOT after a second date.
		/// </summary>
		public static void CheckDateIsAfterAnotherDate(ZPropertyInfo dateToSetErrorOn, ZDateTime otherDateValue)
		{
			if (TryGetDateConverter(dateToSetErrorOn, out TypeConverter convertor))
			{
				if (!IsBefore((IZType)convertor.ConvertFrom(otherDateValue), (IZType)convertor.ConvertFrom(dateToSetErrorOn.Value)))
				{
					string d1 = "'" + dateToSetErrorOn.HumanReadableName + "'";
					string d2 = "'" + otherDateValue.FormatDateTime() + "'";
					string error = Res.GetString("7b37fba6-f0ca-4168-84e4-deb03abe3558", "The {0} must be after {1}.", d1, d2);

					dateToSetErrorOn.AddError(error);
				}
			}
		}

		/// <summary>
		/// Sets an error on a date if it IS NOT after a second date.
		/// </summary>
		public static void CheckDateIsNotBeforeAnotherDate(ZPropertyInfo dateToSetErrorOn, ZDateTime otherDateValue)
		{
			if (TryGetDateConverter(dateToSetErrorOn, out TypeConverter convertor))
			{
				if (IsBefore((IZType)convertor.ConvertFrom(dateToSetErrorOn.Value), (IZType)convertor.ConvertFrom(otherDateValue)))
				{
					string d1 = "'" + dateToSetErrorOn.HumanReadableName + "'";
					string d2 = "'" + otherDateValue.FormatDateTime() + "'";
					string error = Res.GetString("7b37fba6-f0ca-4168-84e4-deb03abe3558", "The {0} must be after {1}.", d1, d2);

					dateToSetErrorOn.AddError(error);
				}
			}
		}

		/// <summary>
		/// Sets a warning on a date if it IS NOT after a second date.
		/// </summary>
		public static void WarnIfDateIsNotAfterAnotherDate(ZPropertyInfo dateToSetWarningOn, ZPropertyInfo otherDate)
		{
			if (TryGetDateConverter(dateToSetWarningOn, otherDate, out TypeConverter convertor))
			{
				if (!IsBefore((IZType)convertor.ConvertFrom(otherDate.Value), (IZType)convertor.ConvertFrom(dateToSetWarningOn.Value)))
				{
					string d1 = "'" + dateToSetWarningOn.HumanReadableName + "'";
					string d2 = "'" + otherDate.HumanReadableName + "'";
					string warning = Res.GetString("9bc23540-b10f-486d-a702-2f01a667351c", "The {0} should be after the {1}.", d1, d2);

					dateToSetWarningOn.AddWarning(warning);
				}
			}
		}

		/// <summary>
		/// Sets a warning on a date if it IS NOT after a second date.
		/// </summary>
		public static void WarnIfDateIsNotBeforeAnotherDate(ZPropertyInfo dateToSetWarningOn, ZPropertyInfo otherDate)
		{
			if (TryGetDateConverter(dateToSetWarningOn, otherDate, out TypeConverter convertor))
			{
				if (!IsBefore((IZType)convertor.ConvertFrom(dateToSetWarningOn.Value), (IZType)convertor.ConvertFrom(otherDate.Value)))
				{
					string d1 = "'" + dateToSetWarningOn.HumanReadableName + "'";
					string d2 = "'" + otherDate.HumanReadableName + "'";
					string warning = Res.GetString("25a9b58b-e2c0-48f9-bd16-1253ba1de23b", "The {0} should be before the {1}.", d1, d2);

					dateToSetWarningOn.AddWarning(warning);
				}
			}
		}

		/// <summary>
		/// Sets an error on a date if it IS after a second date.
		/// </summary>
		public static void CheckDateIsNotAfterAnotherDate(ZPropertyInfo dateToSetErrorOn, ZPropertyInfo otherDate)
		{
			if (TryGetDateConverter(dateToSetErrorOn, otherDate, out TypeConverter convertor))
			{
				if (!IsBeforeOrSameAs((IZType)convertor.ConvertFrom(dateToSetErrorOn.Value), (IZType)convertor.ConvertFrom(otherDate.Value)))
				{
					string d1 = "'" + dateToSetErrorOn.HumanReadableName + "'";
					string d2 = "'" + otherDate.HumanReadableName + "'";
					string error = Res.GetString("770b8806-49da-4df4-8df0-336b7c34941e", "The {0} must be before or the same as the {1}.", d1, d2);

					dateToSetErrorOn.AddError(error);
				}
			}
		}

		/// <summary>
		/// Sets an error on a date if it is not before a second date.
		/// </summary>
		public static void CheckDateIsBeforeAnotherDate(ZPropertyInfo dateToSetErrorOn, ZPropertyInfo otherDate)
		{
			if (TryGetDateConverter(dateToSetErrorOn, otherDate, out TypeConverter convertor))
			{
				if (!IsBefore((IZType)convertor.ConvertFrom(dateToSetErrorOn.Value), (IZType)convertor.ConvertFrom(otherDate.Value)))
				{
					string d1 = "'" + dateToSetErrorOn.HumanReadableName + "'";
					string d2 = "'" + otherDate.HumanReadableName + "'";
					string error = Res.GetString("d8661e27-5167-45ac-98f0-a9bac78f1d93", "The {0} must be before the {1}.", d1, d2);

					dateToSetErrorOn.AddError(error);
				}
			}
		}

		/// <summary>
		/// Sets an error on a date if it is not before a second date.
		/// </summary>
		public static void CheckDateIsBeforeAnotherDate(ZPropertyInfo dateToSetErrorOn, ZDateTime otherDateValue)
		{
			if (TryGetDateConverter(dateToSetErrorOn, out TypeConverter convertor))
			{
				if (!IsBefore((IZType)convertor.ConvertFrom(dateToSetErrorOn.Value), (IZType)convertor.ConvertFrom(otherDateValue)))
				{
					string d1 = dateToSetErrorOn.HumanReadableName;
					string d2 = otherDateValue.FormatDateTime();
					string error = Res.GetString("9f24dec5-084e-476e-95b2-31f39996b11b", "The '{0}' must be before '{1}'.", d1, d2);

					dateToSetErrorOn.AddError(error);
				}
			}
		}

		#region Implementation

		protected static bool IsEqual(IZType property1, IZType property2)
		{
			return property1.Equals(property2);
		}

		protected static bool IsBefore<T>(T x, T y)
			where T : IZType, IComparable
		{
			if (!x.IsEmpty && !y.IsEmpty && x.CompareTo(y) >= 0)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		protected static bool IsBeforeOrSameAs<T>(T x, T y)
			where T : IZType, IComparable
		{
			if (!x.IsEmpty && !y.IsEmpty && x.CompareTo(y) > 0)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		#endregion
	}
}
