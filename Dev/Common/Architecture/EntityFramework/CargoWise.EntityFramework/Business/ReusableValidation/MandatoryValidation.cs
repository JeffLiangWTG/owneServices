using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	public interface IMandatoryValidationInternals
	{
		void AddNotificationIfNotEntered(ZPropertyInfo propertyInfo, ZString messagePrefix, MandatoryValidationMessageType messageType, ZString propertyDescription, Action<string> addNotification);
		void AddNotificationIfIsEntered(ZPropertyInfo propertyInfo, ZString messagePrefix, ZString propertyDescription, Action<string> addNotification);
		string GetErrorFieldFromProperyInfo(ZPropertyInfo propertyInfo);
	}

	public enum MandatoryValidationMessageType
	{
		YouHaveNotEntered,
		MustBeEntered
	}

	public class MandatoryValidation : ValidationProvider, IMandatoryValidationInternals
	{
		delegate void AddNotification(string notification);
		delegate void AddNotificationToInfo(ZPropertyInfo propertyInfo, ZString propertyDescription);

		static void AddNotificationIfNotEntered(ZPropertyInfo propertyInfo, ZString messagePrefix, MandatoryValidationMessageType messageType, ZString propertyDescription, AddNotification addNotification)
		{
			AddNotificationIfNotEntered(propertyInfo, messagePrefix, messageType, (IMultilingualString)(NoResString)propertyDescription, addNotification);
		}

		static void AddNotificationIfNotEntered(ZPropertyInfo propertyInfo, ZString messagePrefix, MandatoryValidationMessageType messageType, IMultilingualString propertyDescription, AddNotification addNotification)
		{
			if (propertyInfo.Value.IsEmpty)
			{
				ZString properyDescriptionString = propertyDescription.ToString();
				if (properyDescriptionString.IsEmpty)
				{
					properyDescriptionString = GetErrorFieldFromProperyInfo(propertyInfo);
				}
				string notification;
				switch (messageType)
				{
					case MandatoryValidationMessageType.YouHaveNotEntered:
						notification = YouHaveNotEnteredMessage(properyDescriptionString);
						break;

					case MandatoryValidationMessageType.MustBeEntered:
						notification = MustBeEnteredMessage(properyDescriptionString);
						break;

					default:
						throw new InvalidOperationException("Invalid messageType: " + messageType);
				}
				notification = messagePrefix + notification;
				addNotification(notification);
			}
		}

		static void AddNotificationIfIsEntered(ZPropertyInfo propertyInfo, ZString messagePrefix, ZString propertyDescription, AddNotification addNotification)
		{
			if (!propertyInfo.Value.IsEmpty)
			{
				if (propertyDescription.IsEmpty)
				{
					propertyDescription = GetErrorFieldFromProperyInfo(propertyInfo);
				}
				string notification = messagePrefix + DoNotEnterMessage(propertyDescription);
				addNotification(notification);
			}
		}

		/// <summary>
		/// Sets an error if the field is empty.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		public static void CheckEntered(ZPropertyInfo propertyInfo)
		{
			CheckEntered(propertyInfo, ZString.Empty);
		}

		/// <summary>
		/// Sets an error if the field is empty, using the given description.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		/// <param name="propertyDescription">The description of the property.</param>
		public static void CheckEntered(ZPropertyInfo propertyInfo, ZString propertyDescription)
		{
			AddNotificationIfNotEntered(propertyInfo, "", MandatoryValidationMessageType.MustBeEntered, propertyDescription, propertyInfo.AddError);
		}

		/// <summary>
		/// Sets an error if the field is empty, using the given description.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		/// <param name="propertyDescription">The description of the property.</param>
		public static void CheckEntered(ZPropertyInfo propertyInfo, IMultilingualString propertyDescription)
		{
			AddNotificationIfNotEntered(propertyInfo, "", MandatoryValidationMessageType.MustBeEntered, propertyDescription, propertyInfo.AddError);
		}

		/// <summary>
		/// Sets an error if the field is empty, using the given description and/or prefix.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		/// <param name="propertyDescription">The description of the property.</param>
		/// <param name="errorNotificationPrefix">Prefix to be added to the error notification.</param>
		public static void CheckEntered(ZPropertyInfo propertyInfo, string propertyDescription = "", string errorNotificationPrefix = "")
		{
			AddNotificationIfNotEntered(propertyInfo, errorNotificationPrefix, MandatoryValidationMessageType.MustBeEntered, propertyDescription, propertyInfo.AddError);
		}

		/// <summary>
		/// Sets an error if the field has a value in it.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		public static void CheckNotEntered(ZPropertyInfo propertyInfo)
		{
			CheckNotEntered(propertyInfo, ZString.Empty);
		}

		/// <summary>
		/// Sets an error if the field has a value in it, using the given description.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		/// <param name="propertyDescription">The description of the property.</param>
		public static void CheckNotEntered(ZPropertyInfo propertyInfo, ZString propertyDescription)
		{
			AddNotificationIfIsEntered(propertyInfo, "", propertyDescription, propertyInfo.AddError);
		}

		/// <summary>
		/// Sets a warning if the field is empty.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		public static void WarnIfNotEntered(ZPropertyInfo propertyInfo)
		{
			WarnIfNotEntered(propertyInfo, ZString.Empty);
		}

		/// <summary>
		/// Sets a warning if the field is empty, using the given description.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		/// <param name="propertyDescription">The description of the property.</param>
		public static void WarnIfNotEntered(ZPropertyInfo propertyInfo, ZString propertyDescription)
		{
			WarnIfNotEntered(propertyInfo, propertyDescription, ZString.Empty);
		}

		/// <param name="warningMessagePrefix">This can be used to define notification sub-types until we are able to use subclasses of the notification types.</param>
		protected static void WarnIfNotEntered(ZPropertyInfo propertyInfo, ZString propertyDescription, ZString warningMessagePrefix)
		{
			AddNotificationIfNotEntered(propertyInfo, warningMessagePrefix, MandatoryValidationMessageType.YouHaveNotEntered, propertyDescription, propertyInfo.AddWarning);
		}

		/// <summary>
		/// Sets a warning if the field has a value in it.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		public static void WarnIfIsEntered(ZPropertyInfo propertyInfo)
		{
			WarnIfIsEntered(propertyInfo, ZString.Empty);
		}

		/// <summary>
		/// Sets a warning if the field has a value in it, using the given description.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		/// <param name="propertyDescription">The description of the property.</param>
		public static void WarnIfIsEntered(ZPropertyInfo propertyInfo, ZString propertyDescription)
		{
			WarnIfIsEntered(propertyInfo, propertyDescription, ZString.Empty);
		}

		/// <param name="warningMessagePrefix">This can be used to define notification sub-types until we are able to use subclasses of the notification types.</param>
		protected static void WarnIfIsEntered(ZPropertyInfo propertyInfo, ZString propertyDescription, ZString warningMessagePrefix)
		{
			AddNotificationIfIsEntered(propertyInfo, warningMessagePrefix, propertyDescription, propertyInfo.AddWarning);
		}

		/// <summary>
		/// Sets a message error if the field is empty, using the given description.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		/// <param name="propertyDescription">The description of the property.</param>
		public static void MessageErrorIfNotEntered(ZPropertyInfo propertyInfo, ZString propertyDescription)
		{
			MessageErrorIfNotEntered(propertyInfo, propertyDescription, "");
		}

		/// <summary>
		/// Sets a message error if the field is empty, using the given description and/or prefix.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		/// <param name="propertyDescription">The description of the property.</param>
		/// <param name="messagePrefix">Prefix to be added to the message.</param>
		public static void MessageErrorIfNotEntered(ZPropertyInfo propertyInfo, string propertyDescription = "", string messagePrefix = "")
		{
			AddNotificationIfNotEntered(propertyInfo, messagePrefix, MandatoryValidationMessageType.YouHaveNotEntered, propertyDescription, propertyInfo.AddMessageError);
		}

		/// <summary>
		/// Sets a message error if the field has a value in it.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		public static void MessageErrorIfIsEntered(ZPropertyInfo propertyInfo)
		{
			MessageErrorIfIsEntered(propertyInfo, ZString.Empty);
		}

		/// <summary>
		/// Sets a message error if the field has a value in it, using the given description.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		/// <param name="propertyDescription">The description of the property.</param>
		public static void MessageErrorIfIsEntered(ZPropertyInfo propertyInfo, ZString propertyDescription)
		{
			AddNotificationIfIsEntered(propertyInfo, "", propertyDescription, propertyInfo.AddMessageError);
		}

		public static string ValueCannotBeNegativeMessage(string propertyDescription)
		{
			return Res.GetString("dd74ec05-80ca-46f2-a13e-b9ef8bbd8cc5", "{0} cannot be negative.", propertyDescription);
		}

		public static string ValueCannotBeZeroMessage(string propertyDescription)
		{
			return Res.GetString("93d3d609-2625-43cf-92d0-d06d3e7fbe8b", "{0} cannot be zero.", propertyDescription);
		}
		public static string YouHaveNotEnteredMessage(string propertyDescriptor)
		{
			return Res.GetString("ab84bedd-177d-4da8-a36a-3328cfa90599", "You have not entered {0}{1}.", Grammar.Instance.IndefiniteArticlePrefix(propertyDescriptor), propertyDescriptor);
		}
		public static string DoNotEnterMessage(string propertyDescriptor)
		{
			return Res.GetString("d27c0d4e-0f75-48a7-9b6f-ccfd37875c4b", "Please do not enter {0}{1}.", Grammar.Instance.IndefiniteArticlePrefix(propertyDescriptor), propertyDescriptor);
		}
		public static string MustBeEnteredMessage(string propertyDescriptor)
		{
			return Res.GetString("79e90f78-648b-4cb2-83ea-03d49697a80a", "Please enter {0}{1}.", Grammar.Instance.IndefiniteArticlePrefix(propertyDescriptor), propertyDescriptor);
		}

#if DEBUG
		// message prefixes for unit tests
		public const string ValueCannotBeNegative = " cannot be negative";
		public const string ValueCannotBeZero = " cannot be zero";
		public const string MustBeEntered = "Please enter";
		public const string YouHaveNotEntered = "You have not entered";
		public const string DoNotEntered = "Please do not enter";
#endif

		static void AddNotificationIfUnitNotEntered(ZPropertyInfo unit, ZPropertyInfo amount, ZString propertyDescription, AddNotificationToInfo addNotificationToInfo)
		{
			if (amount.Value.DataType.IsNumeric)
			{
				var amountVal = ZDecimal.Parse(amount.Value.ToString());
				if (amountVal != 0m)
				{
					addNotificationToInfo(unit, propertyDescription);
				}
			}
			else
			{
				throw new Exception("The property '" + amount.Name + "' <" + amount.Value.GetType() + "> is not a valid numeric type!");
			}
		}

		/// <summary>
		/// Sets an error on the unit if it is empty and there is a non-zero amount entered.
		/// </summary>
		/// <param name="unit">The unit of the field.</param>
		/// <param name="amount">The amount of the field.</param>
		public static void CheckUnitEntered(ZPropertyInfo unit, ZPropertyInfo amount)
		{
			CheckUnitEntered(unit, amount, ZString.Empty);
		}

		/// <summary>
		/// Sets an error on the unit if it is empty and there is a non-zero amount entered.
		/// </summary>
		/// <param name="unit">The unit of the field.</param>
		/// <param name="amount">The amount of the field.</param>
		/// <param name="propertyDescription">The description of the property.</param>
		public static void CheckUnitEntered(ZPropertyInfo unit, ZPropertyInfo amount, ZString propertyDescription)
		{
			AddNotificationIfUnitNotEntered(unit, amount, propertyDescription, CheckEntered);
		}

		/// <summary>
		/// Sets a message error on the unit if it is empty and there is a non-zero amount entered.
		/// </summary>
		/// <param name="unit">The unit of the field.</param>
		/// <param name="amount">The amount of the field.</param>
		/// <param name="propertyDescription">The description of the property.</param>
		public static void MessageErrorIfUnitNotEntered(ZPropertyInfo unit, ZPropertyInfo amount, ZString propertyDescription)
		{
			AddNotificationIfUnitNotEntered(unit, amount, propertyDescription, MessageErrorIfNotEntered);
		}

		/// <summary>
		/// Sets a message error on the unit if it is empty and there is a non-zero amount entered.
		/// </summary>
		/// <param name="unit">The unit of the field.</param>
		/// <param name="amount">The amount of the field.</param>
		public static void MessageErrorIfUnitNotEntered(ZPropertyInfo unit, ZPropertyInfo amount)
		{
			MessageErrorIfUnitNotEntered(unit, amount, ZString.Empty);
		}

		/// <summary>
		/// Sets a warning on the unit if it is empty and there is a non-zero amount entered.
		/// </summary>
		/// <param name="unit">The unit of the field.</param>
		/// <param name="amount">The amount of the field.</param>
		/// <param name="propertyDescription">The description of the property.</param>
		public static void WarnIfUnitNotEntered(ZPropertyInfo unit, ZPropertyInfo amount, ZString propertyDescription)
		{
			AddNotificationIfUnitNotEntered(unit, amount, propertyDescription, WarnIfNotEntered);
		}

		/// <summary>
		/// Sets a warning on the unit if it is empty and there is a non-zero amount entered.
		/// </summary>
		/// <param name="unit">The unit of the field.</param>
		/// <param name="amount">The amount of the field.</param>
		public static void WarnIfUnitNotEntered(ZPropertyInfo unit, ZPropertyInfo amount)
		{
			WarnIfUnitNotEntered(unit, amount, ZString.Empty);
		}

		static void AddNotificationIfIsNegative(ZPropertyInfo propertyInfo, ZString propertyDescription, AddNotification addNotification)
		{
			var value = propertyInfo.Value;
			if (value.DataType.IsNumeric)
			{
				if (propertyInfo is ZPropertyInfoLong)
				{
					if (new ZLong(value) < 0L)
					{
						AddNegativeNotification();
					}
				}
				else if (new ZDecimal(value) < 0m)
				{
					AddNegativeNotification();
				}
			}
			else
			{
				throw new ArgumentOutOfRangeException("A number was expected but this was entered: " + value);
			}

			void AddNegativeNotification()
			{
				if (propertyDescription.IsEmpty)
				{
					propertyDescription = GetErrorFieldFromProperyInfo(propertyInfo);
				}
				var notification = ValueCannotBeNegativeMessage(propertyDescription);
				addNotification(notification);
			}
		}

		public static void CheckNotNegative(ZPropertyInfo propertyInfo)
		{
			CheckNotNegative(propertyInfo, ZString.Empty);
		}

		public static void CheckValidShortGreaterOrEqualToZero(ZPropertyInfo info)
		{
			var value = new ZDecimal(info.Value);
			if (value < 0 || value > short.MaxValue)
			{
				info.AddError(ResString.GetMultilingualString("2DF42186-10ED-4A09-86BC-2A2CB75D1877", "Value must be between {0} and {1}", 0, short.MaxValue));
			}
		}

		public static void CheckNotNegative(ZPropertyInfo propertyInfo, ZString propertyDescription)
		{
			AddNotificationIfIsNegative(propertyInfo, propertyDescription, propertyInfo.AddError);
		}

		public static void MessageErrorIfIsNegative(ZPropertyInfo propertyInfo)
		{
			MessageErrorIfIsNegative(propertyInfo, ZString.Empty);
		}

		public static void MessageErrorIfIsNegative(ZPropertyInfo propertyInfo, ZString propertyDescription)
		{
			AddNotificationIfIsNegative(propertyInfo, propertyDescription, propertyInfo.AddMessageError);
		}

		public static void WarnIfIsNegative(ZPropertyInfo propertyInfo)
		{
			WarnIfIsNegative(propertyInfo, ZString.Empty);
		}

		public static void WarnIfIsNegative(ZPropertyInfo propertyInfo, ZString propertyDescription)
		{
			AddNotificationIfIsNegative(propertyInfo, propertyDescription, propertyInfo.AddWarning);
		}

		static void AddNotificationIfIsZero(ZPropertyInfo propertyInfo, ZString propertyDescription, AddNotification addNotification)
		{
			var value = propertyInfo.Value;
			if (value.DataType.IsNumeric)
			{
				if (value.IsEmpty)
				{
					if (propertyDescription.IsEmpty)
					{
						propertyDescription = GetErrorFieldFromProperyInfo(propertyInfo);
					}
					var notification = ValueCannotBeZeroMessage(propertyDescription);
					addNotification(notification);
				}
			}
			else
			{
				throw new ArgumentOutOfRangeException("A number was expected but this was entered: " + value);
			}
		}

		public static void CheckNotZero(ZPropertyInfo propertyInfo)
		{
			CheckNotZero(propertyInfo, ZString.Empty);
		}

		public static void CheckNotZero(ZPropertyInfo propertyInfo, ZString propertyDescription)
		{
			AddNotificationIfIsZero(propertyInfo, propertyDescription, propertyInfo.AddError);
		}

		public static void MessageErrorIfIsZero(ZPropertyInfo propertyInfo)
		{
			MessageErrorIfIsZero(propertyInfo, ZString.Empty);
		}

		public static void MessageErrorIfIsZero(ZPropertyInfo propertyInfo, ZString propertyDescription)
		{
			AddNotificationIfIsZero(propertyInfo, propertyDescription, propertyInfo.AddMessageError);
		}

		public static void WarnIfIsZero(ZPropertyInfo propertyInfo)
		{
			WarnIfIsZero(propertyInfo, ZString.Empty);
		}

		public static void WarnIfIsZero(ZPropertyInfo propertyInfo, ZString propertyDescription)
		{
			AddNotificationIfIsZero(propertyInfo, propertyDescription, propertyInfo.AddWarning);
		}

		public static string GetErrorFieldFromProperyInfo(ZPropertyInfo propertyInfo)
		{
			return propertyInfo.HasHumanReadableName ? propertyInfo.HumanReadableName.ToString() : Res.GetString("03c6543b-78ab-4af1-b352-7001e43828fa", "value");
		}

		public static void AddYouHaveNotEnteredMessage(ZPropertyInfo propertyInfo, string propertyDescription = null)
		{
			propertyInfo.AddMessageError(YouHaveNotEnteredMessage(string.IsNullOrEmpty(propertyDescription) ? propertyInfo.Description : propertyDescription));
		}

		public static void AddErrorIfNotEnteredAndOtherPropertyIsEntered(ZPropertyInfo propertyInfoToValidate, ZPropertyInfo otherPropertyInfo)
		{
			if (propertyInfoToValidate.Value.IsEmpty && !otherPropertyInfo.Value.IsEmpty)
			{
				propertyInfoToValidate.AddError(GetErrorWhenPropertyNotEnteredAndOtherPropertyIsEntered(propertyInfoToValidate, otherPropertyInfo));
			}
		}

		public static void AddErrorIfNotEnteredAndOtherPropertyHasValue(ZPropertyInfo propertyInfoToValidate, ZPropertyInfo otherPropertyInfo, IZType valueThatShouldTriggerValidation)
		{
			AddErrorIfNotEnteredAndOtherPropertyHasValues(propertyInfoToValidate, otherPropertyInfo, new[] { valueThatShouldTriggerValidation });
		}

		public static void AddErrorIfNotEnteredAndOtherPropertyHasValues(ZPropertyInfo propertyInfoToValidate, ZPropertyInfo otherPropertyInfo, IEnumerable<IZType> valuesThatShouldTriggerValidation)
		{
			var otherPropertyValue = otherPropertyInfo.Value;
			if (propertyInfoToValidate.Value.IsEmpty && valuesThatShouldTriggerValidation.Any(v => otherPropertyValue.Equals(v) || v.Equals(otherPropertyValue)))
			{
				propertyInfoToValidate.AddError(GetErrorWhenPropertyNotEnteredAndOtherPropertyHasValue(propertyInfoToValidate, otherPropertyInfo, otherPropertyValue));
			}
		}

		/// <summary>
		/// Add message error to <paramref name="propertyInfoToValidate"/> if
		/// <br /> - Value of <paramref name="propertyInfoToValidate"/> is empty.
		/// <br /> - Code list of <paramref name="propertyInfoToValidate"/> is not empty. (provided by <see cref="ListAttribute"/>)
		/// </summary>
		/// <param name="propertyInfoToValidate">The property to validate.</param>
		public static void AddMessageErrorIfNotEnteredAndCodeListIsNotEmpty(ZPropertyInfo propertyInfoToValidate)
		{
			AddMessageErrorIfNotEnteredAndCodeListIsNotEmpty(propertyInfoToValidate, propertyInfoToValidate);
		}

		/// <summary>
		/// Add message error to <paramref name="propertyInfoToValidate"/> if
		/// <br /> - Value of <paramref name="propertyInfoToValidate"/> is empty.
		/// <br /> - Code list of <paramref name="propertyInfoWithList"/> is not empty. (provided by <see cref="ListAttribute"/>)
		/// </summary>
		/// <param name="propertyInfoToValidate">The property to validate.</param>
		/// <param name="propertyInfoWithList">The property containing List.</param>
		public static void AddMessageErrorIfNotEnteredAndCodeListIsNotEmpty(ZPropertyInfo propertyInfoToValidate, ZPropertyInfo propertyInfoWithList)
		{
			var list = MetaData.GetListDataSource(propertyInfoWithList.BizObj, propertyInfoWithList.PropertyDescriptor);
			AddMessageErrorIfNotEnteredAndCodeListIsNotEmpty(propertyInfoToValidate, list);
		}

		/// <summary>
		/// Add message error to <paramref name="propertyInfoToValidate"/> if
		/// <br /> - Value of <paramref name="propertyInfoToValidate"/> is empty.
		/// <br /> - Code list <paramref name="list"/> is not empty.
		/// </summary>
		/// <param name="propertyInfoToValidate">The property to validate.</param>
		/// <param name="list">The code list to check.</param>
		public static void AddMessageErrorIfNotEnteredAndCodeListIsNotEmpty(ZPropertyInfo propertyInfoToValidate, IEnumerable list)
		{
			if (list is not null && propertyInfoToValidate.Value.IsEmpty && list.OfType<object>().Any())
			{
				propertyInfoToValidate.AddMessageError(YouHaveNotEnteredMessage(propertyInfoToValidate.Description));
			}
		}

		public static void AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(ZPropertyInfo propertyInfoToValidate, ZPropertyInfo otherPropertyInfo, string message = "")
		{
			if (propertyInfoToValidate.Value.IsDefault && !otherPropertyInfo.Value.IsDefault)
			{
				propertyInfoToValidate.AddMessageError(string.IsNullOrEmpty(message) ? GetMessageErrorWhenPropertyNotEnteredAndOtherPropertyIsEntered(propertyInfoToValidate, otherPropertyInfo) : message);
			}
		}

		public static void AddMessageErrorIfNotEnteredAndOtherPropertyHasValue(ZPropertyInfo propertyInfoToValidate, ZPropertyInfo otherPropertyInfo, IZType valueThatShouldTriggerValidation, string message = "")
		{
			AddMessageErrorIfNotEnteredAndOtherPropertyHasValues(propertyInfoToValidate, otherPropertyInfo, new[] { valueThatShouldTriggerValidation }, message);
		}

		public static void AddMessageErrorIfNotEnteredAndOtherPropertyHasValues(ZPropertyInfo propertyInfoToValidate, ZPropertyInfo otherPropertyInfo, IEnumerable<IZType> valuesThatShouldTriggerValidation, string message = "")
		{
			var otherPropertyValue = otherPropertyInfo.Value;
			if (propertyInfoToValidate.Value.IsEmpty && valuesThatShouldTriggerValidation.Any(v => otherPropertyValue.Equals(v) || v.Equals(otherPropertyValue)))
			{
				propertyInfoToValidate.AddMessageError(string.IsNullOrEmpty(message) ? GetMessageErrorWhenPropertyNotEnteredAndOtherPropertyHasValue(propertyInfoToValidate, otherPropertyInfo, otherPropertyValue) : message);
			}
		}

		public static string GetMessageErrorWhenPropertyNotEnteredAndOtherPropertyIsEntered(ZPropertyInfo propertyInfoThatShouldHaveBeenEntered, ZPropertyInfo otherPropertyInfo)
		{
			return YouHaveNotEnteredMessage(Res.GetString("53752487-21F2-4BBC-A7F8-AA7246AF08B4", "{0} when {1} is entered", propertyInfoThatShouldHaveBeenEntered.HumanReadableName, otherPropertyInfo.HumanReadableName));
		}

		public static string GetMessageErrorWhenPropertyNotEnteredAndOtherPropertyHasValue(ZPropertyInfo propertyInfoThatShouldHaveBeenEntered, ZPropertyInfo otherPropertyInfo, IZType value)
		{
			return YouHaveNotEnteredMessage(Res.GetString("516E9001-1026-4A3B-8F93-CABA87BF84EB", "{0} when {1} is {2}", propertyInfoThatShouldHaveBeenEntered.HumanReadableName, otherPropertyInfo.HumanReadableName, value));
		}

		public static string GetErrorWhenPropertyNotEnteredAndOtherPropertyIsEntered(ZPropertyInfo propertyInfoThatShouldHaveBeenEntered, ZPropertyInfo otherPropertyInfo)
		{
			return MustBeEnteredMessage(Res.GetString("f2445345-2f40-4d68-9fff-862cceb475e5", "{0} when {1} is entered", propertyInfoThatShouldHaveBeenEntered.HumanReadableName, otherPropertyInfo.HumanReadableName));
		}

		public static string GetErrorWhenPropertyNotEnteredAndOtherPropertyHasValue(ZPropertyInfo propertyInfoThatShouldHaveBeenEntered, ZPropertyInfo otherPropertyInfo, IZType value)
		{
			return MustBeEnteredMessage(Res.GetString("cc838eea-a4fe-451d-8036-af7f95e10cfd", "{0} when {1} is {2}", propertyInfoThatShouldHaveBeenEntered.HumanReadableName, otherPropertyInfo.HumanReadableName, value));
		}

		#region Implementation of IMandatoryValidationInernals

		void IMandatoryValidationInternals.AddNotificationIfNotEntered(ZPropertyInfo propertyInfo, ZString messagePrefix, MandatoryValidationMessageType messageType, ZString propertyDescription, Action<string> addNotification)
		{
			AddNotificationIfNotEntered(propertyInfo, messagePrefix, messageType, propertyDescription, new AddNotification(addNotification));
		}

		void IMandatoryValidationInternals.AddNotificationIfIsEntered(ZPropertyInfo propertyInfo, ZString messagePrefix, ZString propertyDescription, Action<string> addNotification)
		{
			AddNotificationIfIsEntered(propertyInfo, messagePrefix, propertyDescription, new AddNotification(addNotification));
		}

		string IMandatoryValidationInternals.GetErrorFieldFromProperyInfo(ZPropertyInfo propertyInfo)
		{
			return GetErrorFieldFromProperyInfo(propertyInfo);
		}

		#endregion
	}
}
