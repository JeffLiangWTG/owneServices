using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class MetaDataLibrary : MacroLibraryBase
	{
		public override IEnumerator<IMacroMetaData> GetEnumerator() => lazyMacrosRegister.Value.GetEnumerator();

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static readonly Lazy<ICollection<IMacroMetaData>> lazyMacrosRegister = new Lazy<ICollection<IMacroMetaData>>(() => Load(MacroHandlers));

		#region SuppressResourceStringsCheckRegion

		static IEnumerable<IHandler> MacroHandlers
		{
			get
			{
				yield return new Handler<Action<IMacroScope, IDynamicData, MacroClosure, string>>(
					"ErrorIf",
					errorIfMacroDescription,
					(scope, data, rule, message) => SetValidationRule(scope, data, rule, NotificationType.Error, message));

				yield return new Handler<Action<IMacroScope, IDynamicData, MacroClosure, string>>(
					"MessageErrorIf",
					messageErrorIfMacroDescription,
					(scope, data, rule, message) => SetValidationRule(scope, data, rule, NotificationType.MessageError, message));

				yield return new Handler<Action<IMacroScope, IDynamicData, MacroClosure, string>>(
					"WarningIf",
					warningIfMacroDescription,
					(scope, data, rule, message) => SetValidationRule(scope, data, rule, NotificationType.Warning, message));

				yield return new Handler<Action<IMacroScope, IDynamicData, MacroClosure, string>>(
					"DeliveryErrorIf",
					deliveryErrorIfMacroDescription,
					(scope, data, rule, message) => SetValidationRule(scope, data, rule, NotificationType.DeliveryError, message));

				// meta data

				yield return new Handler<Action<IDynamicData, int>>(
					"SetDecimalPlaces",
					setDecimalPlacesMacroDescription,
					(data, numberOfDecimals) => SetMetaData(data, MetaDataType.DecimalPlaces, numberOfDecimals));

				yield return new Handler<Action<IDynamicData, int>>(
					"SetMaxLength",
					setMaxLengthMacroDescription,
					(data, maxLength) => SetMetaData(data, MetaDataType.MaxLength, maxLength));

				yield return new Handler<Action<IDynamicData, string>>(
					"SetDateTimeFormat",
					setDateTimeFormatMacroDescription,
					(data, format) => SetMetaData(data, MetaDataType.DateTimeFormat, format));

				yield return new Handler<Action<IDynamicData, bool>>(
					"SetReadOnly",
					setReadOnlyMacroDescription,
					(data, isReadOnly) => SetMetaData(data, MetaDataType.IsReadOnly, isReadOnly));

				yield return new Handler<Action<IDynamicDataCollection, string>>(
					"SetNaturalKey",
					setNaturalKeyMacroDescription,
					(collection, propertyName) => SetNaturalKey(collection, propertyName));

				yield return new Handler<Action<IDynamicData, object[]>>(
					"SetDropDown",
					setDropDownMacroDescription,
					(data, values) => SetDropDown(data, values));

				yield return new Handler<Action<IDynamicData, object>>(
					"SetLookup",
					setLookupMacroDescription,
					(data, values) => SetLookup(data, values));

				yield return new Handler<Action<IDynamicData>>(
					"ToUpperCase",
					toUpperCaseMacroDescription,
					(data) => SetMetaData(data, MetaDataType.IsUpperCase, true));

				// predefined validation rules

				yield return new Handler<Func<MacroClosure>>(
					"IsValueNoneOrEmpty",
					"Returns true if data is none or empty.",
					() => new MacroClosure(IsValueNoneOrEmpty));

				yield return new Handler<Func<int, MacroClosure>>(
					"IsNotLongerThan",
					"Returns true if text is within max number of characters.",
					(length) => CreateIsNotLongerThanRule(null, length));

				yield return new Handler<Func<MacroClosure, int, MacroClosure>>(
					"IsNotLongerThan",
					"Returns true if text is within max number of characters.",
					(closure, length) => CreateIsNotLongerThanRule(closure, length));

				yield return new Handler<Func<string, MacroClosure>>(
					"MatchesRegex",
					"Returns true if data matches regular expression pattern.",
					(regexPattern) => CreateRegexMatchRule(regexPattern));

				yield return new Handler<Func<MacroClosure>>(
					"HasASCIICharacters",
					"Returns true if data contains valid ASCII characters.",
					() => new MacroClosure(HasASCIICharacters));

				// reflect out property given the name

				yield return new Handler<Func<IDynamicData, string, IDynamicData>>(
					"GetProperty",
					"Returns the property with the provided name.",
					(data, propertyName) => data.Properties.GetOrCreate(propertyName));
			}
		}

		#region SetValidationRule

		const string errorIfMacroDescription = "Adds specified error rule to the data.";
		const string messageErrorIfMacroDescription = "Adds specified message error rule to the data.";
		const string warningIfMacroDescription = "Adds specified warning rule to the data.";
		const string deliveryErrorIfMacroDescription = "Adds specified delivery error rule to the data.";

		static void SetValidationRule(IMacroScope scope, IValidatable validatable, MacroClosure rule, NotificationType notificationType, string message)
		{
			if (validatable == null)
			{
				return;
			}

			if (rule == null)
			{
				throw new MacroRuntimeException("You must specify validation rule.");
			}

			if (string.IsNullOrWhiteSpace(message))
			{
				throw new MacroRuntimeException("You must specify validation message.");
			}

			var validationRule = CreateValidationRule(scope, rule, notificationType, message);

			validatable.ValidationRules.Add(validationRule);
		}

		static ValidationRule CreateValidationRule(IMacroScope scope, MacroClosure rule, NotificationType notificationType, string message)
		{
			return data =>
			{
				using (var dataScope = new MacroScope(scope, data))
				{
					try
					{
						var result = rule.Invoke(dataScope);

						if (Convert.ToBoolean(result, CultureInfo.InvariantCulture))
						{
							return new Notification(
								data.CreateNotificationSource(),
								notificationType,
								message);
						}
					}
					catch (InvalidCastException)
					{
						return new Notification(
							data.CreateNotificationSource(),
							NotificationType.Warning,
							"Validation rule is not a valid logical expression.");
					}
					catch (MacroException exc)
					{
						return new Notification(
							data.CreateNotificationSource(),
							NotificationType.Warning,
							string.Format(CultureInfo.InvariantCulture, "An error has occurred while evaluating validation rule: '{0}'", exc.Message));
					}
				}

				return null;
			};
		}

		#endregion

		#region SetMetaData

		const string setDecimalPlacesMacroDescription = "Sets number of decimals for decimal numbers.";
		const string setMaxLengthMacroDescription = "Sets maximum allowed length for a data field.";
		const string setDateTimeFormatMacroDescription = "Sets format for a date data field.";
		const string setReadOnlyMacroDescription = "Sets whether field is read only or not.";
		const string toUpperCaseMacroDescription = "Converts string representation of a data field to upper case.";

		const string setNaturalKeyMacroDescription = "Sets natural key property name. This statement sould run before the collection is initialized.";

		static void SetMetaData(IDynamicData dynamicData, MetaDataType metaDataType, object value)
		{
			if (dynamicData != null)
			{
				MetaDataHelper.SetMetaData(dynamicData, metaDataType, value, false);
			}
		}

		static void SetNaturalKey(IDynamicDataCollection collection, string propertyName)
		{
			if (collection == null)
			{
				return;
			}

			if (collection.IsInitialized)
			{
				throw new MacroRuntimeException("Cannot set natural key as the collection has already been initialized.");
			}

			collection.SetMetaData(MetaDataType.NaturalKey, propertyName);
		}

		#endregion

		#region SetLookup/SetDropDown

		const string setDropDownMacroDescription = "Sets drop down to a data field.";
		const string setLookupMacroDescription = "Sets lookup to a data field.";

		static void SetLookup(IDynamicData data, object values)
		{
			if (data == null || values == null)
			{
				return;
			}

			switch (values)
			{
				case MacroMap macroMap:
					{
						var codeDescriptionPairList = GetCodeDescription(macroMap);
						SetMetaData(data, MetaDataType.ListDataSource, codeDescriptionPairList);
						SetOnValueChangedHandler(data, codeDescriptionPairList);
					}
					break;

				case object[] macroList:
					{
						var codeDescriptionPairList = GetCodeDescription(macroList);
						SetMetaData(data, MetaDataType.ListDataSource, codeDescriptionPairList);
						SetOnValueChangedHandler(data, codeDescriptionPairList);
					}
					break;

				case IList list:
					SetMetaData(data, MetaDataType.ListDataSource, list);
					break;

				default:
					throw new MacroRuntimeException(string.Format(CultureInfo.InvariantCulture, "Lookup cannot be set from '{0}'.", values.GetFormattedName()));
			}
		}

		static void SetOnValueChangedHandler(IDynamicData codeProperty, CodeDescriptionPairList list)
		{
			var parent = codeProperty.Parent;

			if (typeof(DocDataObjects.ICodeDescription).IsAssignableFrom(parent?.Type))
			{
				var descProperty = parent.GetDynamicProperty(nameof(DocDataObjects.ICodeDescription.Description));
				SetOnValueChangedHandler(codeProperty, descProperty, list);
			}
		}

		static void SetOnValueChangedHandler(IDynamicData codeProperty, IDynamicData descProperty, CodeDescriptionPairList list)
		{
			if (codeProperty == null
				|| descProperty == null
				|| list == null)
			{
				return;
			}

			void OnValueChangedAction()
			{
				var code = Convert.ToString(codeProperty.Value, CultureInfo.InvariantCulture);
				var desc = list.GetDescriptionFromCode(code);
				descProperty.SetValue(desc);
			}
			codeProperty.SubscribeOnValueChanged("UpdateDescriptionOnValueChanged", OnValueChangedAction);
		}

		static void SetDropDown(IDynamicData data, object[] values)
		{
			if (data == null || values == null)
			{
				return;
			}

			var dropDown = new CodeDescriptionPairList();

			foreach (var value in values)
			{
				dropDown.AddPairIfNotExist(Convert.ToString(value, CultureInfo.InvariantCulture), string.Empty);
			}

			SetMetaData(data, MetaDataType.ListDataSource, dropDown);
		}

		static CodeDescriptionPairList GetCodeDescription(MacroMap map)
		{
			var result = new CodeDescriptionPairList();

			foreach (var key in map.Keys)
			{
				result.AddPairIfNotExist(key, Convert.ToString(map[key], CultureInfo.InvariantCulture));
			}

			return result;
		}

		static CodeDescriptionPairList GetCodeDescription(object[] list)
		{
			var result = new CodeDescriptionPairList();

			foreach (var elem in list)
			{
				var elemList = elem as object[];

				if (elemList?.Length == 2)
				{
					result.AddPairIfNotExist(
						Convert.ToString(elemList.ElementAt(0), CultureInfo.InvariantCulture),
						Convert.ToString(elemList.ElementAt(1), CultureInfo.InvariantCulture));
				}
			}

			return result;
		}

		#endregion

		#region Predefined Validation Rules

		static object IsValueNoneOrEmpty(IMacroScope scope)
		{
			return scope.Data == null
				|| string.IsNullOrWhiteSpace(scope.Data.ToString());
		}

		static MacroClosure CreateIsNotLongerThanRule(MacroClosure closure, int length)
		{
			return new MacroClosure(scope =>
			{
				var value = string.Empty;

				if (closure != null)
				{
					value = Convert.ToString(closure.Invoke(scope), CultureInfo.InvariantCulture);

					return value.Length <= length;
				}

				value = Convert.ToString(scope.Data, CultureInfo.InvariantCulture);

				return value.Length <= length;
			});
		}

		static MacroClosure CreateRegexMatchRule(string regexPattern)
		{
			return new MacroClosure(scope =>
			{
				if (scope == null)
				{
					return false;
				}

				var regex = new Regex(regexPattern);

				return regex.IsMatch(Convert.ToString(scope.Data, CultureInfo.InvariantCulture));
			});
		}

		static object HasASCIICharacters(IMacroScope scope)
		{
			var value = new ZString(Convert.ToString(scope.Data, CultureInfo.InvariantCulture));

			return value.IsWesternEuropeanOrEmpty;
		}

		#endregion

		#endregion
	}
}
